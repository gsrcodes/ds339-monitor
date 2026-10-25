using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace DS339Monitor.Display;

/// <summary>
/// Tela Thermalright de 9,16" (1920x462, USB 0416:5408 "USBDISPLAY"), a que o TRCC chama de "LY". Ela se anuncia como
/// WinUSB (descritor MS OS 2.0, interface {3876E417-…}): o Windows já carrega o driver da Microsoft, e o programa fala
/// direto com o winusb.dll — nada de libusb nem driver extra. Protocolo levantado do TRCC 2 (USBLCDNEW.dll,
/// ThreadSendDeviceDataLY, e TRCC.exe, FormCZTV.ImageToJpg):
/// <list type="bullet">
/// <item>endpoints bulk 0x09 (saída) e 0x81 (entrada);</item>
/// <item>apresentação: 2048 bytes [02 FF 00 00 00 00 00 00 01 …]; a tela responde [03 FF … 01 …], com o número de série
/// nos bytes 16–19 e o modelo no 20 (0–3 = 1920x462; 4 = 1280x480 e 5 = 1920x440, que este programa não conhece);</item>
/// <item>quadro: um JPEG (até 450 KB — o TRCC descarta os maiores) partido em blocos de 512 bytes: 16 de cabeçalho
/// [01 FF | tamanho do JPEG (4) | bytes neste bloco (2) | 01 | nº de blocos (2) | índice (2) | 0 0 0] e 496 de JPEG. O
/// número de blocos é completado com blocos zerados até múltiplo de 4, tudo vai em escritas de 4096 bytes (a última
/// pode ter 2048) e a tela confirma com 512 bytes na entrada;</item>
/// <item>não há comando de brilho nem de rotação neste modelo: o TRCC escurece e gira a própria imagem — aqui é igual
/// (<see cref="App.JpegEncoder"/>).</item>
/// </list>
/// A thread de monitoramento conecta e entrega os quadros (<see cref="SendFrame"/> só copia e volta); quem fala com a
/// USB é uma thread própria (<see cref="Sender"/>). Se o TRCC estiver com a tela aberta, o Windows não deixa abrir de
/// novo (WinUSB é exclusivo): o programa só tenta outra vez mais tarde, sem disputar.
/// </summary>
internal sealed class ThermalrightDisplay : IDisposable
{
    public const int Width = 1920;
    public const int Height = 462;
    /// <summary>Limite de JPEG do TRCC para esta tela (quadro maior ele nem manda).</summary>
    public const int MaxJpegBytes = 450_000;

    private const string DevicePathTag = "vid_0416&pid_5408";
    private static readonly Guid InterfaceGuid = new("3876E417-E089-4BCC-BBF8-47ABA55E46FB");
    private const byte PipeOut = 0x09;
    private const byte PipeIn = 0x81;
    private const int HelloSize = 2048;
    private const int ReplySize = 512;
    private const int BlockSize = 512;
    private const int BlockHeader = 16;
    private const int BlockData = BlockSize - BlockHeader; // 496
    private const int WriteSize = 4096;
    private const uint TimeoutMs = 1000;
    private const long ScanIntervalMs = 2000;
    private const long RepeatMs = 2000; // reenvia o último quadro se nada novo chegar (por garantia)

    private SafeFileHandle? _file;
    private IntPtr _usb;
    private Sender? _sender;
    private long _nextScan;
    private string? _lastProblem;
    private string? _instanceId;
    private static int _markerChecked;

    public bool IsReady => (_usb != IntPtr.Zero && _sender is { Failed: null }) || Asleep;
    public string? Serial { get; private set; }

    /// <summary>A tela está desativada no Windows (<see cref="Sleep"/>) até <see cref="Wake"/>.</summary>
    public bool Asleep { get; private set; }

    /// <summary>Guarda o dispositivo desativado, para reativar mesmo se o programa cair no meio.</summary>
    private static string MarkerPath => Path.Combine(AppContext.BaseDirectory, "thermalright-desativada.flag");

    /// <summary>
    /// Apaga a tela de verdade. Ela não tem comando de luz de fundo (preto ainda deixa a luz acesa), então o caminho é a
    /// USB: depois do quadro preto, o programa solta a tela e desativa o dispositivo no Windows — a porta entra em
    /// suspensão e a tela corta o consumo. A desativação não é permanente (um reinício do PC já reativa), e
    /// <see cref="Wake"/>, o fechar do programa e o próximo início reativam.
    /// </summary>
    public void Sleep()
    {
        if (Asleep || _usb == IntPtr.Zero) return;
        Thread.Sleep(400); // o quadro preto (já entregue) sai e é confirmado
        string? id = _instanceId;
        Close();
        Asleep = true;
        if (id is null) return;
        try
        {
            File.WriteAllText(MarkerPath, id);
        }
        catch (IOException)
        {
        }
        int result = CM_Locate_DevNodeW(out uint node, id, 0);
        if (result == 0) result = CM_Disable_DevNode(node, 0);
        if (result == 0) Log.Info("Thermalright: tela desativada (a porta USB entra em suspensão).");
        else Log.Warn($"Thermalright: não consegui desativar o dispositivo (CONFIGRET {result}); fica só preta.");
    }

    /// <summary>Reativa o dispositivo; a varredura reconecta logo depois.</summary>
    public void Wake()
    {
        if (!Asleep) return;
        Asleep = false;
        EnableFromMarker();
        _nextScan = Environment.TickCount64 + 1500; // o Windows recarrega o driver
    }

    private static void EnableFromMarker()
    {
        if (!File.Exists(MarkerPath)) return;
        try
        {
            string id = File.ReadAllText(MarkerPath).Trim();
            int result = CM_Locate_DevNodeW(out uint node, id, 0);
            if (result == 0) result = CM_Enable_DevNode(node, 0);
            if (result == 0) Log.Info("Thermalright: tela reativada.");
            else Log.Warn($"Thermalright: não consegui reativar o dispositivo (CONFIGRET {result}).");
            File.Delete(MarkerPath);
        }
        catch (IOException)
        {
        }
    }

    /// <summary>Conecta quando a tela aparece; percebe quando ela some.</summary>
    public void Pump()
    {
        if (Interlocked.Exchange(ref _markerChecked, 1) == 0) EnableFromMarker(); // ficou desativada da última vez
        if (Asleep) return;
        if (_usb == IntPtr.Zero)
        {
            long now = Environment.TickCount64;
            if (now < _nextScan) return;
            _nextScan = now + ScanIntervalMs;
            TryConnect();
            return;
        }
        if (_sender?.Failed is { } error) Lost(error);
    }

    /// <summary>Entrega um quadro JPEG para envio (copiado; volta na hora).</summary>
    public bool SendFrame(byte[] jpeg, int length)
    {
        if (_sender is null) return false;
        if (length > MaxJpegBytes)
        {
            Report($"quadro de {length / 1024} KB descartado (limite da tela: {MaxJpegBytes / 1024} KB)");
            return false;
        }
        _sender.Post(jpeg, length);
        return true;
    }

    /// <summary>Quadros enviados, tempo médio de envio + confirmação e quantos foram repetição, desde a última chamada.</summary>
    public (int Writes, double AverageMs, int Repeats) TakeStats() => _sender?.TakeStats() ?? (0, 0, 0);

    public void Dispose()
    {
        Close();
        if (Asleep)
        {
            Asleep = false;
            EnableFromMarker(); // nunca deixa a tela desativada ao sair
        }
    }

    /// <summary>Só para o diagnóstico: conecta, se apresenta e devolve a descrição (ou o motivo de não conseguir).</summary>
    public static string Identify()
    {
        using var display = new ThermalrightDisplay();
        display.TryConnect();
        return display._usb != IntPtr.Zero ? $"conectada, série {display.Serial}" : display._lastProblem ?? "não encontrada";
    }

    private void TryConnect()
    {
        string? path = FindDevicePath();
        if (path is null)
        {
            Report(null);
            return;
        }

        var file = CreateFileW(path, GenericRead | GenericWrite, FileShareRead | FileShareWrite, IntPtr.Zero, OpenExisting,
            FileAttributeNormal | FileFlagOverlapped, IntPtr.Zero);
        if (file.IsInvalid)
        {
            int error = Marshal.GetLastWin32Error();
            file.Dispose();
            Report(error is 5 or 32
                ? "em uso por outro programa (o TRCC está aberto?)"
                : $"não consegui abrir a tela (erro {error})");
            return;
        }
        if (!WinUsb_Initialize(file, out IntPtr usb))
        {
            int error = Marshal.GetLastWin32Error();
            file.Dispose();
            Report($"o WinUSB não iniciou (erro {error})");
            return;
        }

        try
        {
            if (!HasPipes(usb)) throw new IOException("a tela não tem os endpoints esperados (0x09 e 0x81)");
            uint timeout = TimeoutMs;
            WinUsb_SetPipePolicy(usb, PipeOut, PipeTransferTimeout, 4, ref timeout);
            WinUsb_SetPipePolicy(usb, PipeIn, PipeTransferTimeout, 4, ref timeout);

            var reply = Hello(usb) ?? throw new IOException("a tela não respondeu à apresentação");
            Serial = Convert.ToHexString(reply, 16, 4);
            int model = reply[20];
            if (model > 3) throw new IOException($"modelo {model} (não é a de 1920x462 — este programa só conhece essa)");

            _file = file;
            _usb = usb;
            _sender = new Sender(usb);
            _instanceId = InstanceIdFromPath(path);
            Report(null);
            Log.Info($"Thermalright conectada: 0416:5408, {Width}x{Height}, série {Serial} (modelo {model}, sub {reply[22]})");
        }
        catch (IOException ex)
        {
            WinUsb_Free(usb);
            file.Dispose();
            Report(ex.Message);
        }
    }

    /// <summary>
    /// Apresentação. Se sobrou uma confirmação de quadro de uma sessão anterior (o TRCC fechado no meio de um envio),
    /// ela vem antes da resposta: lê mais um pouco até achar a resposta certa.
    /// </summary>
    private static unsafe byte[]? Hello(IntPtr usb)
    {
        var hello = new byte[HelloSize];
        hello[0] = 0x02;
        hello[1] = 0xFF;
        hello[8] = 0x01;
        if (!WritePipe(usb, hello, 0, hello.Length)) return null;
        var reply = new byte[ReplySize];
        for (int attempt = 0; attempt < 3; attempt++)
        {
            int read = ReadPipe(usb, reply);
            if (read < 0) return null;
            if (read >= 24 && reply[0] == 0x03 && reply[1] == 0xFF && reply[8] == 0x01) return reply;
        }
        return null;
    }

    private static bool HasPipes(IntPtr usb)
    {
        if (!WinUsb_QueryInterfaceSettings(usb, 0, out var settings)) return false;
        bool output = false, input = false;
        for (byte i = 0; i < settings.NumEndpoints; i++)
        {
            if (!WinUsb_QueryPipe(usb, 0, i, out var pipe) || pipe.PipeType != PipeTypeBulk) continue;
            output |= pipe.PipeId == PipeOut;
            input |= pipe.PipeId == PipeIn;
        }
        return output && input;
    }

    private void Lost(Exception ex)
    {
        Log.Info($"Thermalright desconectada ({ex.Message})");
        Close();
        _nextScan = Environment.TickCount64 + ScanIntervalMs;
    }

    private void Close()
    {
        _sender?.Stop();
        _sender = null;
        if (_usb != IntPtr.Zero)
        {
            WinUsb_Free(_usb);
            _usb = IntPtr.Zero;
        }
        _file?.Dispose();
        _file = null;
    }

    /// <summary>Só registra quando o problema muda, para não encher o log a cada varredura.</summary>
    private void Report(string? problem)
    {
        if (problem == _lastProblem) return;
        _lastProblem = problem;
        if (problem is not null) Log.Warn($"Thermalright: {problem}");
    }

    /// <summary>Caminho da interface ("\\?\usb#vid_0416&amp;pid_5408#série#{guid}") → id do dispositivo ("USB\VID_0416&amp;PID_5408\SÉRIE").</summary>
    private static string? InstanceIdFromPath(string path)
    {
        var parts = path.TrimStart('\\', '?').Split('#');
        return parts.Length >= 3 ? $@"{parts[0]}\{parts[1]}\{parts[2]}".ToUpperInvariant() : null;
    }

    private static string? FindDevicePath()
    {
        var guid = InterfaceGuid;
        IntPtr set = SetupDiGetClassDevsW(ref guid, IntPtr.Zero, IntPtr.Zero, DigcfPresent | DigcfDeviceInterface);
        if (set == new IntPtr(-1)) return null;
        try
        {
            var data = new SpDeviceInterfaceData { Size = (uint)Marshal.SizeOf<SpDeviceInterfaceData>() };
            for (uint index = 0; SetupDiEnumDeviceInterfaces(set, IntPtr.Zero, ref guid, index, ref data); index++)
            {
                SetupDiGetDeviceInterfaceDetailW(set, ref data, IntPtr.Zero, 0, out uint required, IntPtr.Zero);
                if (required == 0) continue;
                IntPtr detail = Marshal.AllocHGlobal((int)required);
                try
                {
                    Marshal.WriteInt32(detail, IntPtr.Size == 8 ? 8 : 6); // cbSize de SP_DEVICE_INTERFACE_DETAIL_DATA_W
                    if (!SetupDiGetDeviceInterfaceDetailW(set, ref data, detail, required, out _, IntPtr.Zero)) continue;
                    string? path = Marshal.PtrToStringUni(detail + 4);
                    if (path is not null && path.Contains(DevicePathTag, StringComparison.OrdinalIgnoreCase)) return path;
                }
                finally
                {
                    Marshal.FreeHGlobal(detail);
                }
            }
            return null;
        }
        finally
        {
            SetupDiDestroyDeviceInfoList(set);
        }
    }

    private static unsafe bool WritePipe(IntPtr usb, byte[] data, int offset, int count)
    {
        fixed (byte* p = data)
            return WinUsb_WritePipe(usb, PipeOut, p + offset, (uint)count, out uint written, IntPtr.Zero) && written == count;
    }

    /// <summary>Bytes lidos, ou -1 em erro (inclusive tempo esgotado).</summary>
    private static unsafe int ReadPipe(IntPtr usb, byte[] buffer)
    {
        fixed (byte* p = buffer)
            return WinUsb_ReadPipe(usb, PipeIn, p, (uint)buffer.Length, out uint read, IntPtr.Zero) ? (int)read : -1;
    }

    /// <summary>
    /// Monta os blocos de 512 bytes do JPEG em <paramref name="blocks"/> (redimensionado se preciso) e devolve quantos
    /// bytes enviar. Exatamente como o TRCC: com tamanho múltiplo de 496 sai um bloco vazio no fim, e o total é completado
    /// com blocos zerados até múltiplo de 4.
    /// </summary>
    public static int Pack(ReadOnlySpan<byte> jpeg, ref byte[] blocks)
    {
        int length = jpeg.Length;
        int count = length / BlockData + 1;
        int padded = (count + 3) / 4 * 4;
        int total = padded * BlockSize;
        if (blocks.Length < total) blocks = new byte[total];
        Array.Clear(blocks, 0, total);
        for (int i = 0; i < count; i++)
        {
            int size = i == count - 1 ? length % BlockData : BlockData;
            var block = blocks.AsSpan(i * BlockSize, BlockSize);
            block[0] = 0x01;
            block[1] = 0xFF;
            BitConverter.TryWriteBytes(block.Slice(2, 4), length);
            BitConverter.TryWriteBytes(block.Slice(6, 2), (ushort)size);
            block[8] = 0x01;
            BitConverter.TryWriteBytes(block.Slice(9, 2), (ushort)count);
            BitConverter.TryWriteBytes(block.Slice(11, 2), (ushort)i);
            jpeg.Slice(i * BlockData, size).CopyTo(block.Slice(BlockHeader));
        }
        return total;
    }

    /// <summary>
    /// Envia na USB: o quadro mais novo assim que chega (se chegarem dois antes de dar tempo, vai só o último) e o último
    /// de novo se passar <see cref="RepeatMs"/> sem nada novo. Cada quadro espera a confirmação da tela antes do próximo.
    /// Um erro encerra a thread e fica em <see cref="Failed"/>.
    /// </summary>
    private sealed class Sender
    {
        private readonly IntPtr _usb;
        private readonly Thread _thread;
        private readonly AutoResetEvent _signal = new(false);
        private readonly object _gate = new();
        private readonly byte[] _ack = new byte[ReplySize];
        private byte[] _next = new byte[256 * 1024]; // JPEG entregue e ainda não enviado
        private int _nextLength;
        private byte[] _blocks = new byte[256 * 1024]; // o que está na tela, já em blocos
        private int _blocksLength;
        private volatile bool _stop;
        private long _sendTicks;
        private int _writes;
        private int _repeats;
        private bool _ackLogged;

        public Sender(IntPtr usb)
        {
            _usb = usb;
            _thread = new Thread(Run) { IsBackground = true, Name = "Thermalright" };
            _thread.Start();
        }

        public volatile Exception? Failed;

        public void Post(byte[] jpeg, int length)
        {
            lock (_gate)
            {
                if (_next.Length < length) _next = new byte[length];
                Buffer.BlockCopy(jpeg, 0, _next, 0, length);
                _nextLength = length;
            }
            _signal.Set();
        }

        public (int Writes, double AverageMs, int Repeats) TakeStats()
        {
            lock (_gate)
            {
                var result = (_writes, _writes == 0 ? 0 : _sendTicks * 1000.0 / Stopwatch.Frequency / _writes, _repeats);
                _writes = 0;
                _sendTicks = 0;
                _repeats = 0;
                return result;
            }
        }

        public void Stop()
        {
            _stop = true;
            _signal.Set();
            _thread.Join(3000); // uma transferência travada desiste sozinha pelo tempo limite da pipe (1 s)
        }

        private void Run()
        {
            long lastSent = 0;
            try
            {
                while (!_stop)
                {
                    long now = Environment.TickCount64;
                    if (_blocksLength == 0 || now - lastSent < RepeatMs)
                        _signal.WaitOne(_blocksLength == 0 ? -1 : (int)(RepeatMs - (now - lastSent)));
                    if (_stop) break;

                    bool fresh;
                    lock (_gate)
                    {
                        fresh = _nextLength > 0;
                        if (fresh)
                        {
                            _blocksLength = Pack(_next.AsSpan(0, _nextLength), ref _blocks);
                            _nextLength = 0;
                        }
                    }
                    if (_blocksLength == 0 || !(fresh || Environment.TickCount64 - lastSent >= RepeatMs)) continue;

                    long start = Stopwatch.GetTimestamp();
                    Send();
                    long ticks = Stopwatch.GetTimestamp() - start;
                    lastSent = Environment.TickCount64;
                    lock (_gate)
                    {
                        _sendTicks += ticks;
                        _writes++;
                        if (!fresh) _repeats++;
                    }
                }
            }
            catch (IOException ex)
            {
                Failed = ex;
            }
        }

        private void Send()
        {
            for (int offset = 0; offset < _blocksLength; offset += WriteSize)
            {
                int count = Math.Min(WriteSize, _blocksLength - offset); // 4096, ou 2048 no fim
                if (!WritePipe(_usb, _blocks, offset, count))
                    throw new IOException($"escrita na USB falhou (erro {Marshal.GetLastWin32Error()})");
            }
            int read = ReadPipe(_usb, _ack);
            if (read < 0) throw new IOException($"a tela não confirmou o quadro (erro {Marshal.GetLastWin32Error()})");
            if (!_ackLogged)
            {
                _ackLogged = true;
                Log.Info($"Thermalright: primeira confirmação ({read} bytes): {Convert.ToHexString(_ack, 0, Math.Min(read, 24))}");
            }
        }
    }

    // ---------------------------------------------------------------- Win32

    private const uint GenericRead = 0x80000000;
    private const uint GenericWrite = 0x40000000;
    private const uint FileShareRead = 1;
    private const uint FileShareWrite = 2;
    private const uint OpenExisting = 3;
    private const uint FileAttributeNormal = 0x80;
    private const uint FileFlagOverlapped = 0x40000000;
    private const uint DigcfPresent = 0x02;
    private const uint DigcfDeviceInterface = 0x10;
    private const uint PipeTransferTimeout = 0x03;
    private const int PipeTypeBulk = 2;

    [StructLayout(LayoutKind.Sequential)]
    private struct SpDeviceInterfaceData
    {
        public uint Size;
        public Guid InterfaceClassGuid;
        public uint Flags;
        public IntPtr Reserved;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct UsbInterfaceDescriptor
    {
        public byte Length;
        public byte DescriptorType;
        public byte InterfaceNumber;
        public byte AlternateSetting;
        public byte NumEndpoints;
        public byte InterfaceClass;
        public byte InterfaceSubClass;
        public byte InterfaceProtocol;
        public byte Interface;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WinUsbPipeInformation
    {
        public int PipeType;
        public byte PipeId;
        public ushort MaximumPacketSize;
        public byte Interval;
    }

    [DllImport("setupapi.dll", SetLastError = true)]
    private static extern IntPtr SetupDiGetClassDevsW(ref Guid classGuid, IntPtr enumerator, IntPtr parent, uint flags);

    [DllImport("setupapi.dll", SetLastError = true)]
    private static extern bool SetupDiEnumDeviceInterfaces(IntPtr set, IntPtr info, ref Guid guid, uint index, ref SpDeviceInterfaceData data);

    [DllImport("setupapi.dll", SetLastError = true)]
    private static extern bool SetupDiGetDeviceInterfaceDetailW(IntPtr set, ref SpDeviceInterfaceData data, IntPtr detail, uint size, out uint required, IntPtr info);

    [DllImport("setupapi.dll")]
    private static extern bool SetupDiDestroyDeviceInfoList(IntPtr set);

    [DllImport("cfgmgr32.dll", CharSet = CharSet.Unicode)]
    private static extern int CM_Locate_DevNodeW(out uint node, string deviceId, uint flags);

    [DllImport("cfgmgr32.dll")]
    private static extern int CM_Disable_DevNode(uint node, uint flags);

    [DllImport("cfgmgr32.dll")]
    private static extern int CM_Enable_DevNode(uint node, uint flags);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern SafeFileHandle CreateFileW(string path, uint access, uint share, IntPtr security, uint creation, uint flags, IntPtr template);

    [DllImport("winusb.dll", SetLastError = true)]
    private static extern bool WinUsb_Initialize(SafeFileHandle device, out IntPtr usb);

    [DllImport("winusb.dll", SetLastError = true)]
    private static extern bool WinUsb_Free(IntPtr usb);

    [DllImport("winusb.dll", SetLastError = true)]
    private static extern bool WinUsb_QueryInterfaceSettings(IntPtr usb, byte alternate, out UsbInterfaceDescriptor descriptor);

    [DllImport("winusb.dll", SetLastError = true)]
    private static extern bool WinUsb_QueryPipe(IntPtr usb, byte alternate, byte index, out WinUsbPipeInformation pipe);

    [DllImport("winusb.dll", SetLastError = true)]
    private static extern bool WinUsb_SetPipePolicy(IntPtr usb, byte pipe, uint policy, uint length, ref uint value);

    [DllImport("winusb.dll", SetLastError = true)]
    private static extern unsafe bool WinUsb_WritePipe(IntPtr usb, byte pipe, byte* buffer, uint length, out uint written, IntPtr overlapped);

    [DllImport("winusb.dll", SetLastError = true)]
    private static extern unsafe bool WinUsb_ReadPipe(IntPtr usb, byte pipe, byte* buffer, uint length, out uint read, IntPtr overlapped);
}
