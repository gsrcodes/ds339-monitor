using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using DS339Monitor.App;

namespace DS339Monitor.Lighting;

/// <summary>Estado de um canal como o controlador devolve: modo, cor e parâmetros.</summary>
internal readonly record struct LedChannelState(byte Mode, byte R, byte G, byte B, byte Speed, byte Brightness, byte Sync, byte Extra)
{
    public Color Color => Color.FromArgb(R, G, B);

    /// <summary>Modos em que o firmware não redesenha sozinho (0x11, 0xE1, 0xE2).</summary>
    public bool IsDirect => Mode is 0x11 or 0xE1 or 0xE2;

    public override string ToString() => $"modo {Mode}, RGB {R},{G},{B}, velocidade {Speed}, brilho {Brightness}, sync {Sync}, extra {Extra}";
}

/// <summary>
/// LED da ASRock RX 6900 XT Phantom Gaming (PCI 1002:73BF, subsistema 1849:5212): um microcontrolador Nuvoton NUC121 no
/// barramento I2C da placa, alcançado pelo próprio driver de vídeo (ADL2_Display_WriteAndReadI2C) — sem driver de
/// kernel extra. Protocolo levantado do ASRock Polychrome RGB e conferido no firmware do controlador:
/// <list type="bullet">
/// <item>endereço 0x36 (0x6C no formato de 8 bits do ADL), linha 1 nas placas Overdrive 8, 100 kHz;</item>
/// <item>escrita: registrador = comando, dados [0x00, canal/subcomando, 8 bytes de dados, 0x00, 0x00]; o controlador
/// executa o comando ao receber o 12º byte (registrador + 11);</item>
/// <item>resposta do comando anterior: 12 bytes no registrador 0x9F — [eco do canal/subcomando, 0, contador, 0, dados].</item>
/// </list>
/// Os efeitos usam o modo direto do firmware (0x11). No modo normal o laço principal do controlador redesenha os LEDs sem
/// parar, bit a bit (temporização por instruções vazias) e sem desligar a interrupção do I2C: cada comando que chega
/// estica um bit no meio do desenho e, às vezes, os LEDs pegam um quadro desalinhado — piscam numa cor errada. No modo
/// direto o laço para e o desenho acontece dentro da interrupção, uma vez por comando, sem nada que o interrompa. Por isso:
/// <list type="bullet">
/// <item>a escrita direta termina no 12º byte. Com o byte extra do formato do Polychrome o controlador segura o barramento
/// enquanto desenha (~6 ms) e o driver acusa erro em toda escrita;</item>
/// <item>nada é enviado nos <see cref="DirectSettleMs"/> ms seguintes a uma escrita direta;</item>
/// <item>o modo direto grava os 16 registros de canal: os 16 são lidos ao abrir e devolvidos por <see cref="RestoreAll"/>.</item>
/// </list>
/// Segurança (a placa de vídeo é sensível):
/// <list type="bullet">
/// <item>só abre se a placa, a versão do Overdrive, o chip e a versão do firmware forem exatamente os analisados;</item>
/// <item>só existem métodos para os comandos de leitura (0xB1, 0xA6, 0x14, 0x11) e para 0x10 (cor do canal, gravada só
/// na RAM do controlador — o firmware mostra que só 0x12 grava na flash). Comandos que regravam configuração, firmware ou
/// o modo de boot do chip (0x12, 0x15, 0x20, 0xA1, 0xAB–0xAD, 0xD0) não têm como ser enviados por esta classe;</item>
/// <item>toda resposta é conferida (eco do canal/subcomando) antes de ser usada.</item>
/// </list>
/// Não é thread-safe: uma thread só.
/// </summary>
internal sealed unsafe class AsrockGpuLed : IDisposable
{
    private const string Dll = "atiadlxx.dll";
    private const string Device = "VEN_1002&DEV_73BF&SUBSYS_52121849";
    private const uint SupportedFirmware = 0x01000101; // APROM_VGA_201201 — a versão analisada e testada
    private const int AmdVendor = 1002;                 // o ADL informa o fabricante em decimal
    private const int RequiredOverdrive = 8;
    private const int Line = 1;
    private const int Address = 0x6C;
    private const int SpeedKhz = 100;
    private const int ResponseRegister = 0x9F;
    private const int PacketSize = 12;                  // formato do Polychrome (consultas e devolução)
    private const int DirectPacketSize = 11;            // termina no byte que dispara o comando
    private const int DirectSettleMs = 20;              // o controlador desenha os LEDs dentro da interrupção (~6 ms)
    private const int ActionRead = 1;
    private const int ActionWrite = 2;

    private const byte CmdSetChannel = 0x10;
    private const byte CmdGetChannel = 0x11;
    private const byte CmdConfig = 0x14;
    private const byte CmdVersion = 0xA6;
    private const byte CmdChipId = 0xB1;
    private const byte ConfigActiveChannels = 1;
    private const byte ConfigLedCounts = 2;
    private const byte ModeStatic = 1;
    private const byte ModeDirect = 0x11;
    private const int MaxChannels = 8;                  // canais que o 0x14 descreve
    private const int SlotCount = 16;                   // registros de canal do firmware

    private IntPtr _context;
    private readonly int _adapter;
    private readonly Dictionary<int, LedChannelState> _original = new();
    private readonly byte[] _packet = new byte[PacketSize];
    private readonly byte[] _directPacket = new byte[DirectPacketSize];
    private readonly byte[] _response = new byte[PacketSize];
    private long _quietUntil;
    private bool _changed;

    private AsrockGpuLed(IntPtr context, int adapter)
    {
        _context = context;
        _adapter = adapter;
    }

    /// <summary>Canais que existem nesta placa (ativos e com LEDs), na ordem.</summary>
    public IReadOnlyList<int> Channels { get; private set; } = Array.Empty<int>();

    /// <summary>Estado dos 16 registros de canal no momento em que a placa foi aberta.</summary>
    public IReadOnlyDictionary<int, LedChannelState> Original => _original;

    public string Description { get; private set; } = "";

    /// <summary>Arquivo com os registros lidos na última abertura em que a placa estava no estado normal.</summary>
    private static string SavedPath => Path.Combine(AppContext.BaseDirectory, "led-gpu-original.txt");

    /// <summary>
    /// Identifica a placa só com leituras. Devolve null (e o motivo) se qualquer detalhe não bater com o que foi
    /// analisado — nesse caso nada além das consultas de identificação foi enviado.
    /// </summary>
    public static AsrockGpuLed? TryOpen(out string reason)
    {
        reason = "";
        if (!NativeLibrary.TryLoad(Dll, out _))
        {
            reason = "driver de vídeo AMD não encontrado";
            return null;
        }

        IntPtr context = IntPtr.Zero;
        AsrockGpuLed? led = null;
        try
        {
            if (ADL2_Main_Control_Create(&Alloc, 1, out context) != 0)
            {
                reason = "o driver AMD não iniciou";
                return null;
            }

            int adapter = FindAdapter(context);
            if (adapter < 0)
            {
                reason = "placa ASRock RX 6900 XT Phantom Gaming (1849:5212) não encontrada";
                return Fail(context);
            }

            int supported, enabled, version;
            if (ADL2_Overdrive_Caps(context, adapter, &supported, &enabled, &version) != 0 || version != RequiredOverdrive)
            {
                reason = $"versão do Overdrive inesperada ({version}); a linha I2C do LED não é a conhecida";
                return Fail(context);
            }

            led = new AsrockGpuLed(context, adapter);
            context = IntPtr.Zero; // agora é do objeto
            if (!led.Identify(out reason))
            {
                led.Dispose();
                return null;
            }
            return led;
        }
        catch (Exception ex) when (ex is EntryPointNotFoundException or DllNotFoundException or BadImageFormatException)
        {
            reason = $"driver AMD incompatível ({ex.Message})";
            led?.Dispose();
            return Fail(context);
        }
    }

    /// <summary>
    /// Cor fixa na placa inteira pelo modo direto (um comando vale para todos os canais), mantendo velocidade, brilho,
    /// sync e extra do primeiro canal.
    /// </summary>
    public bool SetColor(byte r, byte g, byte b)
    {
        var state = DirectState(r, g, b);
        Array.Clear(_directPacket);
        _directPacket[1] = (byte)Channels[0];
        Fill(_directPacket, state);
        _changed = true;
        bool ok = Transfer(ActionWrite, CmdSetChannel, _directPacket) == 0;
        _quietUntil = Clock.Ms + DirectSettleMs;
        return ok;
    }

    /// <summary>O que qualquer canal deve devolver na leitura depois de <see cref="SetColor"/>.</summary>
    public LedChannelState DirectState(byte r, byte g, byte b) =>
        _original[Channels[0]] with { Mode = ModeDirect, R = r, G = g, B = b };

    /// <summary>
    /// Devolve os 16 registros ao estado da abertura (o primeiro já tira o controlador do modo direto) e confere cada um.
    /// Sem nada alterado nesta sessão, não envia nada.
    /// </summary>
    public bool RestoreAll()
    {
        if (!_changed) return true;
        bool ok = true;
        for (int slot = 0; slot < SlotCount; slot++) ok &= Write(slot, _original[slot]);
        for (int slot = 0; slot < SlotCount; slot++) ok &= Read(slot) == _original[slot];
        if (ok) _changed = false;
        return ok;
    }

    /// <summary>Lê o estado atual de um registro de canal (null se a resposta não conferir).</summary>
    public LedChannelState? Read(int slot)
    {
        if (!Query(CmdGetChannel, (byte)slot)) return null;
        var r = _response;
        return new LedChannelState(r[4], r[5], r[6], r[7], r[8], r[9], r[10], r[11]);
    }

    public void Dispose()
    {
        if (_context == IntPtr.Zero) return;
        ADL2_Main_Control_Destroy(_context);
        _context = IntPtr.Zero;
    }

    private bool Identify(out string reason)
    {
        // 1) Presença: só lê (o ADL envia apenas o número do registrador de resposta).
        if (Transfer(ActionRead, ResponseRegister, _response) != 0)
        {
            reason = "o controlador de LED não respondeu no endereço 0x36";
            return false;
        }

        // 2) Mesma identificação que o Polychrome faz: 0 = não suportado.
        uint chip = Query(CmdChipId, 0) ? BitConverter.ToUInt32(_response, 4) : 0;
        if (chip == 0)
        {
            reason = "o controlador de LED não se identificou";
            return false;
        }

        // 3) Só o firmware analisado: outro firmware pode ter outro protocolo.
        uint firmware = Query(CmdVersion, 0) ? BitConverter.ToUInt32(_response, 4) : 0;
        if (firmware != SupportedFirmware)
        {
            reason = $"firmware do controlador de LED 0x{firmware:X8} diferente do analisado (0x{SupportedFirmware:X8}); por segurança, não controla";
            return false;
        }

        // 4) Canais: ativos (máscara) e com LEDs.
        if (!Query(CmdConfig, ConfigActiveChannels))
        {
            reason = "não consegui ler os canais ativos";
            return false;
        }
        int mask = _response[4] | _response[5] << 8;
        if (!Query(CmdConfig, ConfigLedCounts))
        {
            reason = "não consegui ler o número de LEDs";
            return false;
        }
        var counts = _response.AsSpan(4, MaxChannels).ToArray();
        var channels = Enumerable.Range(0, MaxChannels).Where(ch => (mask & (1 << ch)) != 0 && counts[ch] > 0).ToList();
        if (channels.Count == 0)
        {
            reason = "nenhum canal de LED ativo";
            return false;
        }

        // 5) Os 16 registros (o modo direto grava todos).
        var slots = new LedChannelState[SlotCount];
        for (int slot = 0; slot < SlotCount; slot++)
        {
            if (Read(slot) is not LedChannelState state)
            {
                reason = $"não consegui ler o registro do canal {slot}";
                return false;
            }
            slots[slot] = state;
        }

        string note = "";
        if (slots.Any(s => s.IsDirect))
        {
            // A sessão anterior terminou sem devolver as cores (programa fechado à força): volta ao que foi lido na
            // última abertura normal; sem esse arquivo, à mesma cor em modo estático.
            var saved = LoadSaved();
            slots = saved ?? slots.Select(s => s.IsDirect ? s with { Mode = ModeStatic } : s).ToArray();
            note = saved is not null
                ? " (estava no modo direto; as cores originais vêm da última abertura)"
                : " (estava no modo direto, sem registro das cores originais; ao sair fica a cor atual)";
            _changed = true;
        }
        else
        {
            Save(slots);
        }

        // O modo sincronizado do Polychrome muda o caminho do firmware e não foi testado.
        if (channels.Any(ch => slots[ch].Sync != 0))
        {
            reason = "o LED está no modo sincronizado do Polychrome (não testado); por segurança, não controla";
            return false;
        }

        for (int slot = 0; slot < SlotCount; slot++) _original[slot] = slots[slot];
        Channels = channels;
        Description = $"chip 0x{chip:X8}, firmware 0x{firmware:X8}, canais " +
            string.Join(", ", channels.Select(ch => $"{ch} ({counts[ch]} LEDs)")) + note;
        reason = "";
        return true;
    }

    /// <summary>Escrita no formato do Polychrome (modo normal): só o registro do canal indicado.</summary>
    private bool Write(int slot, LedChannelState state)
    {
        if (state.IsDirect) throw new InvalidOperationException("o modo direto só é enviado por SetColor");
        Array.Clear(_packet);
        _packet[1] = (byte)slot;
        Fill(_packet, state);
        _changed = true;
        return Transfer(ActionWrite, CmdSetChannel, _packet) == 0;
    }

    private static void Fill(byte[] packet, LedChannelState state)
    {
        packet[2] = state.Mode;
        packet[3] = state.R;
        packet[4] = state.G;
        packet[5] = state.B;
        packet[6] = state.Speed;
        packet[7] = state.Brightness;
        packet[8] = state.Sync;
        packet[9] = state.Extra;
    }

    /// <summary>Consulta: escreve o comando e lê a resposta, conferindo o eco do canal/subcomando.</summary>
    private bool Query(byte command, byte sub)
    {
        // Lista fechada: só comandos que apenas preparam a resposta no controlador.
        if (command is not (CmdChipId or CmdVersion or CmdConfig or CmdGetChannel))
            throw new InvalidOperationException($"comando 0x{command:X2} não permitido");
        if (command == CmdConfig && sub is not (ConfigActiveChannels or ConfigLedCounts))
            throw new InvalidOperationException($"subcomando 0x14/{sub} não permitido");

        Array.Clear(_packet);
        _packet[1] = sub;
        if (Transfer(ActionWrite, command, _packet) != 0) return false;
        Array.Clear(_response);
        if (Transfer(ActionRead, ResponseRegister, _response) != 0) return false;
        return _response[0] == sub && _response[1] == 0;
    }

    private int Transfer(int action, int register, byte[] data)
    {
        if (_context == IntPtr.Zero) return -1;
        long wait = _quietUntil - Clock.Ms;
        if (wait > 0) Thread.Sleep((int)wait); // o controlador ainda está desenhando o último comando direto

        fixed (byte* p = data)
        {
            var i2c = new AdlI2C
            {
                Size = sizeof(AdlI2C),
                Line = Line,
                Address = Address,
                Offset = register,
                Action = action,
                Speed = SpeedKhz,
                DataSize = data.Length,
                Data = (IntPtr)p,
            };
            return ADL2_Display_WriteAndReadI2C(_context, _adapter, &i2c);
        }
    }

    private static void Save(LedChannelState[] slots)
    {
        try
        {
            File.WriteAllLines(SavedPath, slots.Select(s => $"{s.Mode:X2} {s.R:X2} {s.G:X2} {s.B:X2} {s.Speed:X2} {s.Brightness:X2} {s.Sync:X2} {s.Extra:X2}"));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Log.Warn($"LED da GPU: não consegui guardar as cores originais ({ex.Message}).");
        }
    }

    /// <summary>A cor mais comum entre os registros guardados na última abertura normal (para a simulação), ou null.</summary>
    public static Color? SavedColor() =>
        LoadSaved() is { } slots ? slots.GroupBy(s => s.Color.ToArgb()).OrderByDescending(g => g.Count()).First().First().Color : null;

    /// <summary>Registros guardados, se o arquivo existir, estiver inteiro e não tiver nenhum no modo direto.</summary>
    private static LedChannelState[]? LoadSaved()
    {
        try
        {
            var lines = File.ReadAllLines(SavedPath);
            if (lines.Length != SlotCount) return null;
            var slots = new LedChannelState[SlotCount];
            for (int i = 0; i < SlotCount; i++)
            {
                var b = lines[i].Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => byte.TryParse(x, System.Globalization.NumberStyles.HexNumber, null, out byte v) ? (int)v : -1).ToArray();
                if (b.Length != 8 || b.Any(v => v < 0)) return null;
                slots[i] = new LedChannelState((byte)b[0], (byte)b[1], (byte)b[2], (byte)b[3], (byte)b[4], (byte)b[5], (byte)b[6], (byte)b[7]);
                if (slots[i].IsDirect) return null;
            }
            return slots;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }

    private static int FindAdapter(IntPtr context)
    {
        if (ADL2_Adapter_NumberOfAdapters_Get(context, out int count) != 0 || count <= 0) return -1;
        var infos = new AdapterInfo[count];
        fixed (AdapterInfo* p = infos)
        {
            for (int i = 0; i < count; i++) p[i].Size = sizeof(AdapterInfo);
            if (ADL2_Adapter_AdapterInfo_Get(context, p, sizeof(AdapterInfo) * count) != 0) return -1;
        }
        foreach (var info in infos)
        {
            var copy = info;
            string pnp = Encoding.ASCII.GetString(copy.PnpString, 256).Split('\0')[0];
            if (info.Present != 0 && info.Vendor == AmdVendor && pnp.Contains(Device, StringComparison.OrdinalIgnoreCase))
                return info.Index; // o ADL lista uma entrada por saída de vídeo; a primeira serve
        }
        return -1;
    }

    private static AsrockGpuLed? Fail(IntPtr context)
    {
        if (context != IntPtr.Zero) ADL2_Main_Control_Destroy(context);
        return null;
    }

    /// <summary>O ADL pede memória por callback.</summary>
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static IntPtr Alloc(int size) => Marshal.AllocHGlobal(size);

    [StructLayout(LayoutKind.Sequential)]
    private struct AdlI2C
    {
        public int Size;
        public int Line;
        public int Address;
        public int Offset;
        public int Action;
        public int Speed;
        public int DataSize;
        public IntPtr Data;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct AdapterInfo
    {
        public int Size;
        public int Index;
        public fixed byte Udid[256];
        public int Bus;
        public int Device;
        public int Function;
        public int Vendor;
        public fixed byte Name[256];
        public fixed byte DisplayName[256];
        public int Present;
        public int Exist;
        public fixed byte DriverPath[256];
        public fixed byte DriverPathExt[256];
        public fixed byte PnpString[256];
        public int OsDisplayIndex;
    }

    [DllImport(Dll)]
    private static extern int ADL2_Main_Control_Create(delegate* unmanaged[Stdcall]<int, IntPtr> callback, int connectedOnly, out IntPtr context);

    [DllImport(Dll)]
    private static extern int ADL2_Main_Control_Destroy(IntPtr context);

    [DllImport(Dll)]
    private static extern int ADL2_Adapter_NumberOfAdapters_Get(IntPtr context, out int count);

    [DllImport(Dll)]
    private static extern int ADL2_Adapter_AdapterInfo_Get(IntPtr context, AdapterInfo* info, int size);

    [DllImport(Dll)]
    private static extern int ADL2_Overdrive_Caps(IntPtr context, int adapter, int* supported, int* enabled, int* version);

    [DllImport(Dll)]
    private static extern int ADL2_Display_WriteAndReadI2C(IntPtr context, int adapter, AdlI2C* i2c);
}
