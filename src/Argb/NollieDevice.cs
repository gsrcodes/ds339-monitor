using HidSharp;

namespace DS339Monitor.Argb;

/// <summary>
/// Controladora Nollie 16CH RGB (Nollie16v3, firmware "Nos2.0", USB 16D5:2A16), a que liga os fans, as fitas e os
/// acessórios ARGB do gabinete. Protocolo copiado do plugin do SignalRGB (Plugins\Nollie\Nollie16v3_Nos2.0.js), sem
/// nada a mais:
/// <list type="bullet">
/// <item>relatório HID de saída de 1024 bytes na interface 0: [0 (id do relatório), canal físico, fim de quadro, 0, 0,
/// G, R, B, G, R, B...], até 256 LEDs por canal;</item>
/// <item>os 16 canais da placa (Channel 01..16 no SignalRGB) correspondem aos canais físicos 3, 2, 1, 0, 8, 9, 10, 11,
/// 4, 5, 6, 7, 15, 14, 13, 12; os pacotes saem na ordem dos canais e o de maior canal físico leva o "fim de quadro" = 1;</item>
/// <item>canal sem LEDs não recebe pacote.</item>
/// </list>
/// Os comandos de configuração (efeitos guardados na controladora, ver <see cref="NollieConfig"/>) são os do app oficial
/// NollieRGB, e só saem por <see cref="Query"/> / <see cref="Command"/>.
/// </summary>
internal sealed class NollieDevice : IDisposable
{
    public const int VendorId = 0x16D5;
    private static readonly int[] ProductIds = { 0x2A16, 0x4716 };
    public const int Channels = 16;
    public const int MaxLedsPerChannel = 256;
    private const int ReportLength = 1024;
    private static readonly int[] PhysicalChannel = { 3, 2, 1, 0, 8, 9, 10, 11, 4, 5, 6, 7, 15, 14, 13, 12 };

    /// <summary>Canal físico de cada canal 1–16 (índice 0 = Channel 01).</summary>
    public static int[] HardwareChannels => (int[])PhysicalChannel.Clone();

    private readonly HidStream _stream;
    private readonly byte[] _packet;
    private readonly byte[] _input;

    private NollieDevice(HidDevice device, HidStream stream, int reportLength, int inputLength)
    {
        Device = device;
        _stream = stream;
        _packet = new byte[reportLength];
        _input = new byte[Math.Max(inputLength, 65)];
    }

    public HidDevice Device { get; }

    /// <summary>A interface 0 da controladora (a que o plugin usa), se estiver conectada.</summary>
    public static HidDevice? Find()
    {
        foreach (int product in ProductIds)
        {
            foreach (var device in DeviceList.Local.GetHidDevices(VendorId, product))
            {
                string path = device.DevicePath.ToLowerInvariant();
                if (path.Contains("mi_") && !path.Contains("mi_00")) continue;
                return device;
            }
        }
        return null;
    }

    /// <summary>Descrição para o diagnóstico (só lê o que o Windows já sabe do aparelho).</summary>
    public static string Describe()
    {
        var lines = new List<string>();
        foreach (int product in ProductIds)
        {
            foreach (var device in DeviceList.Local.GetHidDevices(VendorId, product))
            {
                string name = Safe(device.GetProductName), serial = Safe(device.GetSerialNumber);
                lines.Add($"{device.VendorID:X4}:{device.ProductID:X4} \"{name}\" série {serial} — saída {device.GetMaxOutputReportLength()} B, " +
                    $"entrada {device.GetMaxInputReportLength()} B, recurso {device.GetMaxFeatureReportLength()} B — {device.DevicePath}");
            }
        }
        return lines.Count == 0 ? "nenhuma Nollie conectada" : string.Join(Environment.NewLine, lines);
    }

    private static string Safe(Func<string> read)
    {
        try
        {
            return read();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            return "?";
        }
    }

    public static NollieDevice? TryOpen(out string reason)
    {
        var device = Find();
        if (device is null)
        {
            reason = "Nollie 16CH não encontrada";
            return null;
        }
        int length = device.GetMaxOutputReportLength();
        if (length < ReportLength)
        {
            reason = $"a Nollie anuncia relatório de {length} bytes (esperado {ReportLength}); não vou arriscar";
            return null;
        }
        if (!device.TryOpen(out HidStream stream))
        {
            reason = "não consegui abrir a Nollie (outro programa usando?)";
            return null;
        }
        stream.WriteTimeout = 500;
        reason = "";
        return new NollieDevice(device, stream, length, device.GetMaxInputReportLength());
    }

    /// <summary>
    /// Manda um quadro: <paramref name="grb"/>[i] são as cores do canal i (Channel i+1), 3 bytes por LED na ordem G, R, B
    /// (null ou vazio = canal sem LEDs). False se a escrita falhou (a controladora foi desconectada).
    /// </summary>
    public bool SendFrame(IReadOnlyList<byte[]?> grb)
    {
        int lastPhysical = -1;
        for (int i = 0; i < Channels && i < grb.Count; i++)
            if (grb[i] is { Length: > 0 }) lastPhysical = Math.Max(lastPhysical, PhysicalChannel[i]);
        if (lastPhysical < 0) return true;

        try
        {
            for (int i = 0; i < Channels && i < grb.Count; i++)
            {
                var colors = grb[i];
                if (colors is not { Length: > 0 }) continue;
                int length = Math.Min(colors.Length, MaxLedsPerChannel * 3);
                Array.Clear(_packet);
                _packet[1] = (byte)PhysicalChannel[i];
                _packet[2] = (byte)(PhysicalChannel[i] == lastPhysical ? 1 : 0);
                Buffer.BlockCopy(colors, 0, _packet, 5, length);
                _stream.Write(_packet);
            }
            return true;
        }
        catch (Exception ex) when (ex is IOException or TimeoutException or ObjectDisposedException)
        {
            return false;
        }
    }

    /// <summary>
    /// Manda um comando de configuração (<paramref name="payload"/> = os bytes depois do id do relatório; o resto do
    /// relatório vai zerado). A controladora não responde aos comandos de gravação.
    /// </summary>
    public void Command(params byte[] payload)
    {
        if (payload.Length == 0 || payload.Length >= _packet.Length) throw new ArgumentException("comando inválido", nameof(payload));
        if (!Allowed(payload)) throw new InvalidOperationException($"comando {payload[0]:X2} {(payload.Length > 1 ? payload[1].ToString("X2") : "")} fora da lista permitida");
        Array.Clear(_packet);
        Buffer.BlockCopy(payload, 0, _packet, 1, payload.Length);
        _stream.Write(_packet);
    }

    /// <summary>
    /// Pergunta e espera a resposta (até 3 × 80 ms). Devolve os bytes da resposta sem o id do relatório, ou null sem
    /// resposta. Antes, joga fora respostas velhas que tenham ficado na fila.
    /// </summary>
    public byte[]? Query(params byte[] request) => Query(request, 80, 3);

    /// <summary>Como <see cref="Query(byte[])"/>, com uma espera só de <paramref name="timeoutMs"/>.</summary>
    public byte[]? Query(byte[] request, int timeoutMs) => Query(request, timeoutMs, 1);

    private byte[]? Query(byte[] request, int timeoutMs, int attempts)
    {
        Drain();
        Command(request);
        _stream.ReadTimeout = timeoutMs;
        for (int attempt = 0; attempt < attempts; attempt++)
        {
            try
            {
                int read = _stream.Read(_input, 0, _input.Length);
                if (read > 1) return _input.AsSpan(1, read - 1).ToArray();
            }
            catch (TimeoutException)
            {
            }
        }
        return null;
    }

    /// <summary>
    /// A lista fechada do que o programa manda para a controladora além dos quadros de cor: as perguntas (FC 01 versão,
    /// F9 xx efeitos, F5 03/16 contador e tempo de volta), os ajustes de efeito que o NollieRGB grava pela tela dele, e a
    /// animação de boot (FSEQ) — só dentro da área dela na flash externa (de 0x10000 até 16 MiB), como o NollieRGB.
    /// Nunca: FD (bootloader), F4 abaixo de 0x10000 ou fora dos comandos de animação, FA 21/22/23 (restaurar/reiniciar),
    /// FA 14 (detecção de LEDs), 80–84 (atualização de firmware).
    /// </summary>
    private static bool Allowed(byte[] p) => p[0] switch
    {
        0xF4 => AllowedFlash(p),
        0xF3 => (p.Length == 5 && p[1] == 0x02 && p[2] == 0 && p[3] == 0 && p[4] <= 1) || (p.Length == 2 && p[1] is 0x03 or 0x04),
        0xFC => p.Length >= 2 && p[1] == 0x01,
        0xF9 => p.Length >= 2 && p[1] is >= 0x01 and <= 0x13,
        0xF5 => p.Length >= 2 && p[1] is 0x03 or 0x16,
        0xF6 => p.Length == 4 && p[1] == 0x16,
        0xFA => p.Length >= 2 && p[1] switch
        {
            0x01 => p.Length == 3 && p[2] == 0x01,                            // modo: efeitos da controladora
            0x02 => p.Length == 19 && p[2] < 16,                             // efeito de um canvas
            0x03 or 0x0B => p.Length == 19 && p[2] == 0x00,                  // mapa de canais (normais / boot)
            0x04 => p.Length == 3 && p[2] <= 14,                             // quantos canvases normais
            0x05 => p.Length == 3 && p[2] <= 2,                              // quantos canvases de boot
            0x08 => p.Length == 34,                                          // LEDs por canal
            0x13 => p.Length == 4 && p[2] <= 2 && p[3] <= 1,                 // o que toca ao ligar
            0x20 => p.Length == 2,                                           // tocar o boot agora
            0x10 => p.Length == 34,                                          // LEDs por canal da animação (FSEQ)
            _ => false,
        },
        _ => false,
    };

    private const int FlashBase = 0x10000, FlashEnd = 0x1000000;

    /// <summary>Flash externa: só a área das animações, em blocos inteiros, pedaços de até 1016 bytes.</summary>
    private static bool AllowedFlash(byte[] p)
    {
        if (p.Length < 8 || p[7] != 0 && p[1] != 0x07) return false;
        int address = p[2] | p[3] << 8 | p[4] << 16, length = p[5] | p[6] << 8;
        bool zeros = p.Length == 8 && p.Skip(2).All(b => b == 0);
        return p[1] switch
        {
            0x03 or 0x04 => zeros,                                                                   // situação / informação
            0x05 => p.Length == 8 && address >= FlashBase && address % FlashBase == 0 && address < FlashEnd && length == 1, // apagar 1 bloco
            0x06 => p.Length == 8 + length && length is > 0 and <= 1016 && address >= FlashBase && address + length <= FlashEnd, // gravar
            0x01 => p.Length == 8 && length is > 0 and <= 1016 && address >= FlashBase && address + length <= FlashEnd, // ler
            0x07 => zeros || (p.Length == 8 && address == FlashBase),                                // CRC: pedir / resultado
            _ => false,
        };
    }

    private void Drain()
    {
        _stream.ReadTimeout = 1;
        for (int i = 0; i < 16; i++)
        {
            try
            {
                if (_stream.Read(_input, 0, _input.Length) <= 0) return;
            }
            catch (TimeoutException)
            {
                return;
            }
        }
    }

    public void Dispose() => _stream.Dispose();
}
