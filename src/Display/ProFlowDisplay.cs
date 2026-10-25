using System.IO.Ports;
using System.Text;
using System.Text.Json;
using Microsoft.Win32;

namespace DS339Monitor.Display;

/// <summary>
/// Tela do watercooler Jungle Leopard Pro Flow 360 (chip TXW818 + painel ST7701S de 5,5", 960x480), via serial USB.
/// Protocolo reconstruído do Jungle Leopard Display (main/_baseClass/device.js):
/// <list type="bullet">
/// <item>comando: 55 AA | tamanho total (2 bytes, LE) | código | dados | soma de todos os bytes anteriores (2 bytes, LE);</item>
/// <item>modo ao vivo (comando 17): cada quadro vai cru como [tamanho do JPEG (4, LE)][JPEG][soma (2, LE)],
/// e o 17 é repetido a cada 1,5 s para a tela não sair do modo;</item>
/// <item>FF D9 FF D9 cru encerra o que estiver em andamento (o app original manda antes de comandos e ao desligar).</item>
/// </list>
/// A tela espera um fluxo contínuo: o app original manda quadros sem parar (30 por segundo, repetindo a mesma imagem
/// quando nada muda). Com pausas longas entre quadros ela pisca e às vezes volta a mostrar por instantes um quadro
/// anterior — por isso o último quadro é reenviado sozinho a cada <see cref="RepeatMs"/> e logo depois de cada 17.
/// Brilho: o comando 3 é ignorado no modo ao vivo. O app original manda 3 = 100 uma vez antes de entrar no modo ao
/// vivo e aplica o brilho escolhido escurecendo a própria imagem — aqui é igual (ver <see cref="App.JpegEncoder"/>).
/// A thread de monitoramento conecta, desconecta e entrega os quadros (<see cref="SendFrame"/> só copia e volta); quem
/// escreve na porta durante o modo ao vivo é uma thread própria (<see cref="Sender"/>): mandar um quadro leva ~18 ms
/// de USB, e isso não pode atrasar o quadro seguinte da animação.
/// </summary>
internal sealed class ProFlowDisplay : IDisposable
{
    public const int Width = 960;
    public const int Height = 480;
    /// <summary>Limite de JPEG por quadro que o app original usa para firmware ≥ 2.8.</summary>
    public const int MaxJpegBytes = 80 * 1024;

    private const int BaudRate = 2_000_000;
    private const long KeepAliveMs = 1500;
    private const long RepeatMs = 100;
    private const long ScanIntervalMs = 2000;
    private const long RestartAfterMs = 36L * 60 * 60 * 1000; // o app original reinicia a tela a cada 36 h
    private const string VendorId = "33C3";
    private static readonly string[] ProductIds = { "7788", "7791", "7792" }; // genérica, Lovingcool, Jungle Leopard

    private const byte CmdRestart = 1;
    private const byte CmdBrightness = 3;
    private const byte CmdInfo = 6;
    private const byte CmdLive = 17;
    private const byte CmdMotion = 20;
    private static readonly byte[] StopMarker = { 0xFF, 0xD9, 0xFF, 0xD9 };

    private SerialPort? _port;
    private Sender? _sender;
    private long _nextScan;
    private long _connectedAt;
    private string? _lastProblem;

    public bool IsReady => _port is { IsOpen: true } && (Asleep || _sender is { Failed: null });
    public string? PortName { get; private set; }
    public string? Model { get; private set; }

    /// <summary>Conecta quando a tela aparece; percebe quando ela some.</summary>
    public void Pump()
    {
        long now = Environment.TickCount64;
        if (_port is null)
        {
            if (now >= _nextScan)
            {
                _nextScan = now + ScanIntervalMs;
                TryConnect();
            }
            return;
        }

        if (_sender?.Failed is { } error) Lost(error);
        else if (!Asleep && now - _connectedAt >= RestartAfterMs) Restart();
    }

    /// <summary>Entrega um quadro JPEG para envio. Maior que <see cref="MaxJpegBytes"/> a tela não mostra direito: nem vai.</summary>
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

    /// <summary>Envios (novos + repetidos), tempo médio de cada um e quantos foram repetição, desde a última chamada.</summary>
    public (int Writes, double AverageMs, int Repeats) TakeStats() => _sender?.TakeStats() ?? (0, 0, 0);

    /// <summary>A tela está apagada (<see cref="Sleep"/>): conectada, mas fora do modo ao vivo.</summary>
    public bool Asleep { get; private set; }

    /// <summary>
    /// Apaga a tela como o app original faz quando o PC bloqueia: um último quadro preto, sai do modo ao vivo
    /// (FF D9 FF D9) e o comando 20 = [0, 0] ("setMotionBeforeOffScreen": nada de animação própria enquanto não houver
    /// quadros). A porta continua aberta; <see cref="Wake"/> volta ao modo ao vivo.
    /// </summary>
    public void Sleep(byte[] blackJpeg, int length)
    {
        if (_port is null || Asleep) return;
        StopSender();
        try
        {
            var frame = new byte[length + 6];
            frame[0] = (byte)length;
            frame[1] = (byte)(length >> 8);
            frame[2] = (byte)(length >> 16);
            frame[3] = (byte)(length >> 24);
            Buffer.BlockCopy(blackJpeg, 0, frame, 4, length);
            int sum = 0;
            for (int i = 0; i < length + 4; i++) sum += frame[i];
            frame[length + 4] = (byte)sum;
            frame[length + 5] = (byte)(sum >> 8);
            _port.Write(frame, 0, frame.Length);
            Thread.Sleep(150);
            _port.Write(StopMarker, 0, StopMarker.Length);
            _port.Write(Packet(CmdMotion, new byte[] { 0, 0 }), 0, 9);
            Asleep = true;
            Log.Info("Pro Flow 360: tela apagada.");
        }
        catch (Exception ex) when (IsPortError(ex))
        {
            Lost(ex);
        }
    }

    /// <summary>Acende de novo: luz de fundo no máximo e modo ao vivo, como na conexão.</summary>
    public void Wake()
    {
        if (_port is null || !Asleep) return;
        Asleep = false;
        try
        {
            _port.Write(Packet(CmdBrightness, new byte[] { 100 }), 0, 8);
            _port.Write(Packet(CmdLive), 0, 7);
            _sender = new Sender(_port);
            _connectedAt = Environment.TickCount64;
            Log.Info("Pro Flow 360: tela acesa.");
        }
        catch (Exception ex) when (IsPortError(ex))
        {
            Lost(ex);
        }
    }

    public void Dispose()
    {
        if (_port is null) return;
        StopSender();
        try
        {
            _port.DiscardOutBuffer();
            _port.Write(StopMarker, 0, StopMarker.Length); // igual ao app original ao desligar
        }
        catch (Exception ex) when (IsPortError(ex))
        {
        }
        Close();
    }

    private void TryConnect()
    {
        string? portName = FindPort();
        if (portName is null)
        {
            Report(null);
            return;
        }

        var port = new SerialPort(portName, BaudRate)
        {
            DtrEnable = true, // sem DTR a tela não responde
            RtsEnable = true,
            ReadTimeout = 500,
            WriteTimeout = 2000,
            WriteBufferSize = 256 * 1024,
        };
        try
        {
            port.Open();
            // Mesma sequência do app original: espera, limpa o que houver pela metade e pede as informações.
            Thread.Sleep(500);
            port.Write(StopMarker, 0, StopMarker.Length);
            port.Write(new byte[4], 0, 4);
            Thread.Sleep(200);
            port.DiscardInBuffer();

            var info = QueryInfo(port) ?? throw new IOException("a tela não respondeu ao pedido de informações");
            Model = info.Model;
            if (info.Width != Width || info.Height != Height)
                Log.Warn($"Pro Flow 360 informou {info.Width}x{info.Height}; o programa desenha em {Width}x{Height}.");

            // Igual ao app original: luz de fundo no máximo antes do modo ao vivo (também desfaz qualquer valor
            // antigo); o brilho escolhido é aplicado na imagem.
            port.Write(Packet(CmdBrightness, new byte[] { 100 }), 0, 8);
            port.Write(Packet(CmdLive), 0, 7);
            _port = port;
            _sender = new Sender(port);
            PortName = portName;
            _connectedAt = Environment.TickCount64;
            Report(null);
            Log.Info($"Pro Flow 360 conectada em {portName}: {info.Model}, firmware {info.Version}, {info.Width}x{info.Height}");
        }
        catch (Exception ex) when (IsPortError(ex))
        {
            port.Dispose();
            Report(ex is UnauthorizedAccessException
                ? $"{portName} está em uso por outro programa (o Jungle Leopard Display está aberto?)"
                : $"falha ao conectar em {portName}: {ex.Message}");
        }
    }

    private static DeviceInfo? QueryInfo(SerialPort port)
    {
        port.Write(Packet(CmdInfo), 0, 7);
        var received = new List<byte>(512);
        var buffer = new byte[512];
        long deadline = Environment.TickCount64 + 2000;
        while (Environment.TickCount64 < deadline)
        {
            if (port.BytesToRead == 0)
            {
                Thread.Sleep(20);
                continue;
            }
            int read = port.Read(buffer, 0, Math.Min(buffer.Length, port.BytesToRead));
            received.AddRange(buffer.AsSpan(0, read).ToArray());

            int start = received.IndexOf(0x55);
            if (start < 0 || received.Count - start < 7 || received[start + 1] != 0xAA) continue;
            int length = received[start + 2] | received[start + 3] << 8;
            if (received.Count - start < length) continue;

            // Resposta: {"cmd":"info","data":{"uid":...,"width":960,"height":480,"model":...,"version":"3.1",...}}
            string json = Encoding.UTF8.GetString(received.GetRange(start + 5, length - 7).ToArray());
            using var doc = JsonDocument.Parse(json);
            var data = doc.RootElement.GetProperty("data");
            return new DeviceInfo(
                data.TryGetProperty("width", out var w) ? w.GetInt32() : Width,
                data.TryGetProperty("height", out var h) ? h.GetInt32() : Height,
                data.TryGetProperty("model", out var m) ? m.GetString() ?? "?" : "?",
                data.TryGetProperty("version", out var v) ? v.ToString() : "?");
        }
        return null;
    }

    /// <summary>Acha a COM da tela pelo registro (a mesma lista que o Gerenciador de Dispositivos usa), sem WMI.</summary>
    private static string? FindPort()
    {
        var present = SerialPort.GetPortNames();
        using var usb = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\USB");
        if (usb is null) return null;
        foreach (var pid in ProductIds)
        {
            using var device = usb.OpenSubKey($"VID_{VendorId}&PID_{pid}");
            if (device is null) continue;
            foreach (var instance in device.GetSubKeyNames())
            {
                using var parameters = device.OpenSubKey($@"{instance}\Device Parameters");
                if (parameters?.GetValue("PortName") is string name && present.Contains(name, StringComparer.OrdinalIgnoreCase))
                    return name;
            }
        }
        return null;
    }

    /// <summary>
    /// O app original reinicia a tela a cada 36 h (provavelmente contornando algum problema do firmware).
    /// Ela se desconecta da USB e volta sozinha; a varredura reconecta.
    /// </summary>
    private void Restart()
    {
        Log.Info("Pro Flow 360: reinício preventivo (a cada 36 h, como o app original).");
        StopSender();
        try
        {
            _port!.Write(StopMarker, 0, StopMarker.Length);
            var restart = Packet(CmdRestart);
            _port.Write(restart, 0, restart.Length);
        }
        catch (Exception ex) when (IsPortError(ex))
        {
        }
        Close();
        _nextScan = Environment.TickCount64 + 5000;
    }

    private void Lost(Exception ex)
    {
        Log.Info($"Pro Flow 360 desconectada ({ex.GetType().Name}: {ex.Message})");
        Close();
        _nextScan = Environment.TickCount64 + ScanIntervalMs;
    }

    private void Close()
    {
        StopSender();
        Asleep = false;
        var port = _port;
        _port = null;
        PortName = null;
        if (port is null) return;
        try
        {
            port.Close();
        }
        catch (Exception ex) when (IsPortError(ex))
        {
        }
        port.Dispose();
    }

    private void StopSender()
    {
        _sender?.Stop();
        _sender = null;
    }

    /// <summary>Só registra quando o problema muda, para não encher o log a cada varredura.</summary>
    private void Report(string? problem)
    {
        if (problem == _lastProblem) return;
        _lastProblem = problem;
        if (problem is not null) Log.Warn($"Pro Flow 360: {problem}");
    }

    private static byte[] Packet(byte command, byte[]? payload = null)
    {
        payload ??= Array.Empty<byte>();
        int length = payload.Length + 7;
        var packet = new byte[length];
        packet[0] = 0x55;
        packet[1] = 0xAA;
        packet[2] = (byte)length;
        packet[3] = (byte)(length >> 8);
        packet[4] = command;
        payload.CopyTo(packet, 5);
        int sum = 0;
        for (int i = 0; i < length - 2; i++) sum += packet[i];
        packet[length - 2] = (byte)sum;
        packet[length - 1] = (byte)(sum >> 8);
        return packet;
    }

    private static bool IsPortError(Exception ex) =>
        ex is IOException or UnauthorizedAccessException or InvalidOperationException or TimeoutException or JsonException or KeyNotFoundException;

    private sealed record DeviceInfo(int Width, int Height, string Model, string Version);

    /// <summary>
    /// Escreve na porta durante o modo ao vivo: o quadro mais novo assim que chega (se chegarem dois antes de dar tempo,
    /// vai só o último), o 17 a cada 1,5 s com o quadro logo atrás, e o último quadro de novo se passar
    /// <see cref="RepeatMs"/> sem nada novo. Um erro de porta encerra a thread e fica em <see cref="Failed"/>.
    /// </summary>
    private sealed class Sender
    {
        private static readonly byte[] KeepAlive = Packet(CmdLive);

        private readonly SerialPort _port;
        private readonly Thread _thread;
        private readonly AutoResetEvent _signal = new(false);
        private readonly object _gate = new();
        private byte[] _next = new byte[MaxJpegBytes + 6];    // entregue e ainda não enviado
        private int _nextLength;
        private byte[] _current = new byte[MaxJpegBytes + 6]; // o que está na tela
        private int _currentLength;
        private volatile bool _stop;
        private long _writeTicks;
        private int _writes;
        private int _repeats;

        public Sender(SerialPort port)
        {
            _port = port;
            _thread = new Thread(Run) { IsBackground = true, Name = "Pro Flow 360" };
            _thread.Start();
        }

        public volatile Exception? Failed;

        /// <summary>Monta o pacote [tamanho][JPEG][soma] no buffer de espera e avisa a thread.</summary>
        public void Post(byte[] jpeg, int length)
        {
            lock (_gate)
            {
                if (_next.Length < length + 6) _next = new byte[length + 6];
                var frame = _next;
                frame[0] = (byte)length;
                frame[1] = (byte)(length >> 8);
                frame[2] = (byte)(length >> 16);
                frame[3] = (byte)(length >> 24);
                Buffer.BlockCopy(jpeg, 0, frame, 4, length);
                int sum = 0;
                for (int i = 0; i < length + 4; i++) sum += frame[i];
                frame[length + 4] = (byte)sum;
                frame[length + 5] = (byte)(sum >> 8);
                _nextLength = length + 6;
            }
            _signal.Set();
        }

        public (int Writes, double AverageMs, int Repeats) TakeStats()
        {
            lock (_gate)
            {
                var result = (_writes, _writes == 0 ? 0 : _writeTicks * 1000.0 / System.Diagnostics.Stopwatch.Frequency / _writes, _repeats);
                _writes = 0;
                _writeTicks = 0;
                _repeats = 0;
                return result;
            }
        }

        public void Stop()
        {
            _stop = true;
            _signal.Set();
            _thread.Join(3000); // uma escrita travada desiste sozinha pelo WriteTimeout (2 s)
        }

        private void Run()
        {
            long lastFrame = 0, lastKeepAlive = Environment.TickCount64; // o 17 acabou de ir na conexão
            try
            {
                while (!_stop)
                {
                    long now = Environment.TickCount64;
                    long due = Math.Min(lastKeepAlive + KeepAliveMs, _currentLength > 0 ? lastFrame + RepeatMs : long.MaxValue);
                    if (due > now) _signal.WaitOne((int)Math.Min(due - now, KeepAliveMs));
                    if (_stop) break;

                    now = Environment.TickCount64;
                    bool afterKeepAlive = false;
                    if (now - lastKeepAlive >= KeepAliveMs)
                    {
                        _port.Write(KeepAlive, 0, KeepAlive.Length);
                        if (_port.BytesToRead > 0) _port.DiscardInBuffer(); // confirmações dos keepalives: ninguém precisa delas
                        lastKeepAlive = now;
                        afterKeepAlive = true; // o quadro vai logo atrás do 17, como no fluxo do app original
                    }

                    bool fresh;
                    lock (_gate)
                    {
                        fresh = _nextLength > 0;
                        if (fresh)
                        {
                            (_current, _next) = (_next, _current);
                            _currentLength = _nextLength;
                            _nextLength = 0;
                        }
                    }
                    if (_currentLength == 0 || !(fresh || afterKeepAlive || now - lastFrame >= RepeatMs)) continue;

                    long start = System.Diagnostics.Stopwatch.GetTimestamp();
                    _port.Write(_current, 0, _currentLength);
                    long ticks = System.Diagnostics.Stopwatch.GetTimestamp() - start;
                    lastFrame = Environment.TickCount64;
                    lock (_gate)
                    {
                        _writeTicks += ticks;
                        _writes++;
                        if (!fresh) _repeats++;
                    }
                }
            }
            catch (Exception ex) when (IsPortError(ex))
            {
                Failed = ex;
            }
        }
    }
}
