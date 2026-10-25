using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;

namespace DS339Monitor.Game;

/// <summary>
/// A rede do jogo. O jogo não diz o próprio ping, então: o rastreamento de rede do Windows (ETW, provedor
/// Microsoft-Windows-Kernel-Network — só endereços e tamanhos, nunca o conteúdo) mostra com quem o processo do jogo troca
/// pacotes; o endereço com mais tráfego é o servidor (ou o relé da Valve, no CS2), e um ping (ICMP) até ele a cada
/// segundo dá a latência, a variação (jitter) e a perda. Também soma o tráfego de entrada e saída do jogo.
/// </summary>
internal sealed class GameNetwork : IDisposable
{
    private const string SessionName = "DS339MonitorRede";
    private static readonly Guid KernelNetwork = new("7DD42A49-5329-4832-8DFD-43D979153A88");
    private const ulong KeywordIpv4 = 0x10, KeywordIpv6 = 0x20;

    private readonly int _pid;
    private readonly TraceEventSession? _session;
    private readonly Thread? _events;
    private readonly Thread _pinger;
    private readonly object _gate = new();
    private readonly Dictionary<IPAddress, (long Packets, long Bytes)> _peers = new();
    private long _bytesIn, _bytesOut;
    private long _seen, _mine; // eventos de rede vistos (de todos / do jogo), para o log
    private volatile bool _stop;

    public GameNetwork(int pid)
    {
        _pid = pid;
        try
        {
            TraceEventSession.GetActiveSession(SessionName)?.Stop(); // sobra de uma execução que caiu
            _session = new TraceEventSession(SessionName) { StopOnDispose = true };
            _session.EnableProvider(KernelNetwork, TraceEventLevel.Informational, KeywordIpv4 | KeywordIpv6);
            var parser = new RegisteredTraceEventParser(_session.Source);
            parser.All += OnEvent;
            _events = new Thread(() =>
            {
                try
                {
                    _session.Source.Process();
                }
                catch (Exception ex) when (ex is InvalidOperationException or ObjectDisposedException)
                {
                }
            }) { IsBackground = true, Name = "Rede do jogo" };
            _events.Start();
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or InvalidOperationException or System.Runtime.InteropServices.COMException)
        {
            Log.Warn($"Modo jogo: sem o rastreamento de rede ({ex.Message}); sem ping.");
            _session?.Dispose();
            _session = null;
        }
        _pinger = new Thread(PingLoop) { IsBackground = true, Name = "Ping do jogo" };
        _pinger.Start();
    }

    private void OnEvent(TraceEvent e)
    {
        // 10/11 TCP v4, 26/27 TCP v6, 42/43 UDP v4, 58/59 UDP v6 (enviado / recebido).
        int id = (int)e.ID;
        bool send = id is 10 or 26 or 42 or 58;
        bool receive = id is 11 or 27 or 43 or 59;
        if (!send && !receive) return;
        Interlocked.Increment(ref _seen);
        try
        {
            if (Convert.ToInt32(e.PayloadByName("PID")) != _pid) return;
            Interlocked.Increment(ref _mine);
            long size = Convert.ToInt64(e.PayloadByName("size"));
            // daddr é sempre o outro lado da conexão (saddr é este PC), no envio e no recebimento.
            var address = Address(e.PayloadByName("daddr"));
            if (address is null || IsLocal(address)) return;
            lock (_gate)
            {
                var peer = _peers.GetValueOrDefault(address);
                _peers[address] = (peer.Packets + 1, peer.Bytes + size);
                if (send) _bytesOut += size;
                else _bytesIn += size;
            }
        }
        catch (Exception ex) when (ex is InvalidCastException or FormatException or ArgumentException or OverflowException)
        {
        }
    }

    private static IPAddress? Address(object? value) => value switch
    {
        IPAddress address => address,
        string text when IPAddress.TryParse(text, out var parsed) => parsed,
        int v4 => new IPAddress(BitConverter.GetBytes(v4)),
        uint v4 => new IPAddress(BitConverter.GetBytes(v4)),
        byte[] bytes when bytes.Length is 4 or 16 => new IPAddress(bytes),
        _ => null,
    };

    private static bool IsLocal(IPAddress address)
    {
        if (IPAddress.IsLoopback(address) || address.Equals(IPAddress.Any) || address.Equals(IPAddress.IPv6Any)) return true;
        var b = address.GetAddressBytes();
        if (address.AddressFamily == AddressFamily.InterNetwork)
            return b[0] == 10 || (b[0] == 172 && b[1] >= 16 && b[1] < 32) || (b[0] == 192 && b[1] == 168) || b[0] >= 224 || (b[0] == 169 && b[1] == 254);
        return address.IsIPv6LinkLocal || address.IsIPv6Multicast || address.IsIPv6SiteLocal;
    }

    private void PingLoop()
    {
        using var ping = new Ping();
        var samples = new Queue<double?>();
        IPAddress? target = null;
        long lastIn = 0, lastOut = 0;
        long lastTick = Environment.TickCount64;
        int ticks = 0;
        while (!_stop)
        {
            Thread.Sleep(1000);
            if (_stop) break;
            if (++ticks == 5) Log.Info($"Modo jogo: rede — {Interlocked.Read(ref _seen)} eventos em 5 s, {Interlocked.Read(ref _mine)} do jogo.");
            long now = Environment.TickCount64;
            double seconds = Math.Max(0.5, (now - lastTick) / 1000.0);
            lastTick = now;

            // O servidor = quem mais trocou pacotes com o jogo nos últimos segundos.
            IPAddress? top;
            long bytesIn, bytesOut;
            lock (_gate)
            {
                top = _peers.OrderByDescending(p => p.Value.Packets).Select(p => p.Key).FirstOrDefault();
                foreach (var key in _peers.Keys.ToList()) _peers[key] = (_peers[key].Packets / 2, _peers[key].Bytes / 2); // esquece devagar
                bytesIn = _bytesIn;
                bytesOut = _bytesOut;
            }
            double kbpsIn = (bytesIn - lastIn) * 8 / 1000.0 / seconds, kbpsOut = (bytesOut - lastOut) * 8 / 1000.0 / seconds;
            lastIn = bytesIn;
            lastOut = bytesOut;
            if (top is not null && !top.Equals(target))
            {
                target = top;
                samples.Clear();
                Log.Info($"Modo jogo: o jogo está conversando com {target}.");
            }
            if (target is null)
            {
                GameTelemetry.SetNetwork(null, null, null, null, _session is null ? null : kbpsIn, _session is null ? null : kbpsOut);
                continue;
            }

            double? rtt = null;
            try
            {
                var reply = ping.Send(target, 900);
                if (reply.Status == IPStatus.Success) rtt = reply.RoundtripTime;
            }
            catch (PingException)
            {
            }
            samples.Enqueue(rtt);
            while (samples.Count > 20) samples.Dequeue();
            var ok = samples.Where(s => s is not null).Select(s => s!.Value).ToList();
            double? average = ok.Count > 0 ? ok.TakeLast(5).Average() : null;
            double? jitter = ok.Count > 1 ? ok.Zip(ok.Skip(1), (a, b) => Math.Abs(a - b)).TakeLast(10).Average() : null;
            double loss = samples.Count == 0 ? 0 : samples.Count(s => s is null) * 100.0 / samples.Count;
            GameTelemetry.SetNetwork(average, jitter, ok.Count == 0 && samples.Count >= 3 ? 100 : loss, target.ToString(), kbpsIn, kbpsOut);
        }
    }

    public void Dispose()
    {
        _stop = true;
        _session?.Dispose();
        _events?.Join(2000);
        _pinger.Join(2000);
    }
}
