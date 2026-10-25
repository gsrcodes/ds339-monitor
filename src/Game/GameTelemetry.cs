namespace DS339Monitor.Game;

/// <summary>Estatísticas da partida do CS2 (Game State Integration oficial da Valve).</summary>
internal sealed record Cs2State(
    string Map, string Mode, string Phase, int Round, int ScoreCt, int ScoreT, string Team,
    int Health, int Armor, bool Helmet, int Money, int RoundKills, int Kills, int Assists, int Deaths, int Mvps,
    string? Weapon, int? Ammo, int? AmmoReserve, string? Bomb, long At)
{
    /// <summary>Flash, smoke e fogo em você (0–255, como o jogo manda).</summary>
    public int Flashed { get; init; }
    public int Smoked { get; init; }
    public int Burning { get; init; }

    /// <summary>Abates de capacete na rodada.</summary>
    public int RoundHeadshots { get; init; }

    /// <summary>Fase da rodada (freezetime, live, over) e quem venceu (CT ou T, quando acaba).</summary>
    public string RoundPhase { get; init; } = "";
    public string? WinTeam { get; init; }

    /// <summary>A mensagem é de você jogando (não de outro jogador que você está assistindo depois de morrer).</summary>
    public bool Self { get; init; } = true;
}

/// <summary>Tudo o que o painel de jogo mostra, num instante.</summary>
internal sealed record GameSnapshot(
    string Game,
    double? Fps, double? FrameTimeMs, double? Low1Fps, double? GpuBusyPct, double? WaitPct, double? Variation, double? LatencyMs, string? PresentMode,
    float[] FrameTimes,
    double? PingMs, double? JitterMs, double? LossPct, string? Server, double? KbpsIn, double? KbpsOut,
    double? CpuTemp, double? CpuLoad, double? GpuTemp, double? GpuLoad, double? GpuPower,
    Cs2State? Cs2, bool Demo);

/// <summary>
/// O ponto de encontro dos dados do Modo jogo: o leitor de quadros (PresentMon), a rede (ping), a integração do CS2 e os
/// sensores escrevem aqui; o painel de jogo da Thermalright e o painel do programa leem um retrato de cada vez.
/// </summary>
internal static class GameTelemetry
{
    private const int HistoryFrames = 720;

    private static readonly object Gate = new();
    private static readonly float[] Frames = new float[HistoryFrames];
    private static readonly float[] GpuBusy = new float[HistoryFrames];
    private static readonly float[] CpuBusy = new float[HistoryFrames];
    private static readonly double[] FrameAt = new double[HistoryFrames]; // ms (relógio do PresentMon)
    private static int _head, _count;
    private static double? _latency;
    private static string? _presentMode;
    private static string _game = "";
    private static double? _ping, _jitter, _loss, _kbpsIn, _kbpsOut;
    private static string? _server;
    private static double? _cpuTemp, _cpuLoad, _gpuTemp, _gpuLoad, _gpuPower;
    private static Cs2State? _cs2;
    private static long _demoUntil;

    public static void Reset(string game)
    {
        lock (Gate)
        {
            _game = game;
            _head = _count = 0;
            _latency = null;
            _presentMode = null;
            _ping = _jitter = _loss = _kbpsIn = _kbpsOut = null;
            _server = null;
        }
    }

    /// <summary>Um quadro apresentado pelo jogo (do PresentMon).</summary>
    public static void AddFrame(double atMs, double frameTimeMs, double gpuBusyMs, double cpuBusyMs, double? latencyMs, string presentMode)
    {
        if (frameTimeMs <= 0 || frameTimeMs > 1000) return;
        lock (Gate)
        {
            Frames[_head] = (float)frameTimeMs;
            GpuBusy[_head] = (float)Math.Max(0, gpuBusyMs);
            CpuBusy[_head] = (float)Math.Max(0, cpuBusyMs);
            FrameAt[_head] = atMs;
            _head = (_head + 1) % HistoryFrames;
            _count = Math.Min(_count + 1, HistoryFrames);
            if (latencyMs is > 0 and < 1000) _latency = _latency is double l ? l + (latencyMs.Value - l) * 0.05 : latencyMs;
            _presentMode = presentMode;
        }
    }

    public static void SetNetwork(double? ping, double? jitter, double? loss, string? server, double? kbpsIn, double? kbpsOut)
    {
        lock (Gate) (_ping, _jitter, _loss, _server, _kbpsIn, _kbpsOut) = (ping, jitter, loss, server, kbpsIn, kbpsOut);
    }

    public static void SetSensors(double? cpuTemp, double? cpuLoad, double? gpuTemp, double? gpuLoad, double? gpuPower)
    {
        lock (Gate) (_cpuTemp, _cpuLoad, _gpuTemp, _gpuLoad, _gpuPower) = (cpuTemp, cpuLoad, gpuTemp, gpuLoad, gpuPower);
    }

    public static void SetCs2(Cs2State state)
    {
        lock (Gate) _cs2 = state;
    }

    /// <summary>"Ver o painel de jogo": dados de mentira por alguns segundos.</summary>
    public static void StartDemo(int seconds) => Volatile.Write(ref _demoUntil, Environment.TickCount64 + seconds * 1000L);

    public static bool DemoActive => Environment.TickCount64 < Volatile.Read(ref _demoUntil);

    public static GameSnapshot Snapshot()
    {
        if (DemoActive) return Demo();
        lock (Gate)
        {
            var frames = new float[_count];
            for (int i = 0; i < _count; i++) frames[i] = Frames[(_head - _count + i + HistoryFrames) % HistoryFrames];
            double? fps = null, frameTime = null, low = null, gpuBusy = null, cpuBusy = null, variation = null;
            if (_count > 0)
            {
                // FPS e frametime do último segundo; 1% low dos últimos ~10 s (o quadro de 99% mais lento).
                double last = FrameAt[(_head - 1 + HistoryFrames) % HistoryFrames];
                double sum = 0, busy = 0, cpu = 0, squares = 0;
                int n = 0;
                for (int i = _count - 1; i >= 0; i--)
                {
                    int at = (_head - _count + i + HistoryFrames) % HistoryFrames;
                    if (last - FrameAt[at] > 1000 && n > 0) break;
                    sum += Frames[at];
                    busy += GpuBusy[at];
                    cpu += CpuBusy[at];
                    squares += Frames[at] * (double)Frames[at];
                    n++;
                }
                frameTime = sum / n;
                fps = 1000 / frameTime;
                gpuBusy = Math.Clamp(busy / sum * 100, 0, 100);
                cpuBusy = Math.Clamp(cpu / sum * 100, 0, 100);
                double mean = sum / n;
                variation = Math.Sqrt(Math.Max(0, squares / n - mean * mean)) / mean; // quadros regulares = teto de FPS
                var sorted = frames.OrderByDescending(f => f).ToArray();
                low = 1000 / sorted[Math.Min(sorted.Length - 1, sorted.Length / 100)];
            }
            bool cs2Fresh = _cs2 is { } c && Environment.TickCount64 - c.At < 15_000;
            return new GameSnapshot(_game, fps, frameTime, low, gpuBusy, cpuBusy, variation, _latency, _presentMode, frames,
                _ping, _jitter, _loss, _server, _kbpsIn, _kbpsOut, _cpuTemp, _cpuLoad, _gpuTemp, _gpuLoad, _gpuPower,
                cs2Fresh ? _cs2 : null, false);
        }
    }

    /// <summary>Uma partida de mentira, para ver o painel sem jogo aberto.</summary>
    private static GameSnapshot Demo()
    {
        double t = Environment.TickCount64 / 1000.0;
        var random = new Random(7);
        var frames = new float[600];
        for (int i = 0; i < frames.Length; i++)
        {
            double phase = t * 240 - (frames.Length - i);
            frames[i] = (float)(4.1 + 0.35 * Math.Sin(phase / 37) + 0.25 * random.NextDouble() + (((int)phase) % 173 == 0 ? 9 : 0));
        }
        double ping = 17 + 2 * Math.Sin(t / 3);
        return new GameSnapshot("cs2", 241 + 6 * Math.Sin(t), 4.15, 187, 93, 2, 0.08, 14.2, "Hardware: Independent Flip", frames,
            ping, 1.4, 0, "Valve · São Paulo", 186, 42, 61, 34, 68, 97, 281,
            new Cs2State("de_mirage", "competitive", "live", 13, 7, 5, "CT", 76, 100, true, 3400, 2, 14, 3, 8, 2,
                "AK-47", 24, 90, null, Environment.TickCount64), true);
    }
}
