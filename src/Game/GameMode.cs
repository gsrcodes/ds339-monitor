using System.Diagnostics;
using System.Runtime.InteropServices;
using DS339Monitor.App;

namespace DS339Monitor.Game;

/// <summary>Modo jogo: liga/desliga e quais executáveis contam como jogo.</summary>
internal sealed class GameSettings
{
    /// <summary>Ligado: abrir um dos jogos da lista põe o painel de jogo na Thermalright até o jogo fechar.</summary>
    public bool Ligado { get; set; }

    /// <summary>Executáveis (sem .exe) que contam como jogo.</summary>
    public List<string> Jogos { get; set; } = new() { "cs2" };

    /// <summary>O gabinete (fans, fitas e LED da placa) reage à partida do CS2: bomba, rodada, abates, flash…</summary>
    public bool Reacoes { get; set; } = true;

    /// <summary>Código combinado com o CS2 no arquivo da integração (as mensagens sem ele são ignoradas).</summary>
    public string? CodigoCs2 { get; set; }

    public void Normalize()
    {
        Jogos = (Jogos ?? new List<string>())
            .Select(j => j.Trim().ToLowerInvariant().Replace(".exe", ""))
            .Where(j => j.Length > 0 && j.Length < 64)
            .Distinct()
            .ToList();
        if (string.IsNullOrWhiteSpace(CodigoCs2)) CodigoCs2 = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(12));
    }
}

/// <summary>
/// Detecta o jogo (um executável da lista em primeiro plano) e, enquanto ele estiver aberto — mesmo minimizado —, mostra
/// o painel de jogo na Thermalright no lugar do tema escolhido, lendo os quadros (PresentMon) e a rede (ping). Ao fechar o
/// jogo, o tema volta. A integração do CS2 escuta enquanto o modo estiver ligado.
/// </summary>
internal sealed class GameMode : IDisposable
{
    private readonly GameSettings _settings;
    private readonly ScreenPipeline _screen;
    private readonly Thread _thread;
    private readonly AutoResetEvent _wake = new(false);
    private volatile bool _stop;
    private Process? _game;
    private FrameReader? _frames;
    private GameNetwork? _network;
    private Cs2Gsi? _gsi;
    private bool _overridden;
    private int _keysChanged = 1;
    private volatile string _status = "desligado";

    public GameMode(GameSettings settings, ScreenPipeline screen)
    {
        _settings = settings;
        _screen = screen;
        _thread = new Thread(Run) { IsBackground = true, Name = "Modo jogo" };
    }

    public string Status => _status;

    /// <summary>O painel de jogo está na tela (jogo aberto ou prévia).</summary>
    public bool Showing => _overridden;

    /// <summary>Com o painel na tela: temperaturas, uso e consumo de CPU e placa.</summary>
    public IEnumerable<string> SensorKeys =>
        _overridden ? new[] { "CPUTemp", "CpuUsage", "GPUTemp", "GpuUsage", "GpuTEC" } : Array.Empty<string>();

    public bool TakeSensorKeysChanged() => Interlocked.Exchange(ref _keysChanged, 0) == 1;

    public void Start() => _thread.Start();

    public void Reload() => _wake.Set();

    /// <summary>Mostra o painel com uma partida de mentira por alguns segundos.</summary>
    public void Preview(int seconds)
    {
        GameTelemetry.StartDemo(seconds);
        _wake.Set();
    }

    private volatile string? _forced;

    /// <summary>Teste: trata o processo <paramref name="name"/> como o jogo, mesmo sem estar na lista nem em primeiro plano.</summary>
    public void Force(string? name)
    {
        _forced = name;
        _wake.Set();
    }

    public void Stop()
    {
        if (_stop) return;
        _stop = true;
        _wake.Set();
        _thread.Join(5000);
    }

    public void Dispose()
    {
        Stop();
        _wake.Dispose();
    }

    private void Run()
    {
        try
        {
            while (!_stop)
            {
                try
                {
                    Tick();
                }
                catch (Exception ex)
                {
                    Log.Error("Modo jogo", ex);
                }
                _wake.WaitOne(1000);
            }
        }
        finally
        {
            EndGame("programa fechando");
            SetOverride(false);
            _gsi?.Dispose();
        }
    }

    private void Tick()
    {
        bool on = _settings.Ligado || _forced is not null; // o teste funciona com o modo desligado
        bool demo = GameTelemetry.DemoActive;

        // Integração do CS2: escuta enquanto o modo estiver ligado (o jogo manda a partir do momento em que abre).
        if (on && _gsi is null && _settings.CodigoCs2 is { } token)
        {
            try
            {
                _gsi = new Cs2Gsi(token);
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                Log.Warn($"Modo jogo: a porta {Cs2Gsi.Port} da integração do CS2 está ocupada ({ex.Message}).");
            }
        }
        else if (!on && _gsi is not null)
        {
            _gsi.Dispose();
            _gsi = null;
        }

        if (!on)
        {
            EndGame("modo desligado");
        }
        else if (_game is not null)
        {
            if (HasExited(_game)) EndGame("o jogo fechou");
        }
        else if (Foreground() is { } candidate)
        {
            StartGame(candidate);
        }
        else if (_forced is { } forced && Process.GetProcessesByName(forced).OrderByDescending(p => p.WorkingSet64).FirstOrDefault() is { } test)
        {
            StartGame(test);
        }
        if (_forced is null && _game is not null && !_settings.Jogos.Contains(_game.ProcessName.ToLowerInvariant())) EndGame("teste encerrado");

        SetOverride(_game is not null || demo);
        _status = !on ? (demo ? "prévia" : "desligado")
            : _game is not null ? $"{_game.ProcessName} aberto"
            : demo ? "prévia"
            : "esperando um jogo";
    }

    private void StartGame(Process game)
    {
        _game = game;
        GameTelemetry.Reset(game.ProcessName);
        _frames = new FrameReader(game.Id);
        _network = new GameNetwork(game.Id);
        Log.Info($"Modo jogo: {game.ProcessName} (processo {game.Id}) aberto — painel de jogo na Thermalright.");
    }

    private void EndGame(string why)
    {
        if (_game is null) return;
        Log.Info($"Modo jogo: {why}; a Thermalright volta ao tema.");
        _frames?.Dispose();
        _network?.Dispose();
        _frames = null;
        _network = null;
        _game.Dispose();
        _game = null;
    }

    private void SetOverride(bool on)
    {
        if (on == _overridden) return;
        _overridden = on;
        _screen.ThemeOverride = on ? GameTheme.Name : null;
        Interlocked.Exchange(ref _keysChanged, 1);
    }

    /// <summary>O processo da janela em primeiro plano, se for um dos jogos da lista.</summary>
    private Process? Foreground()
    {
        var window = GetForegroundWindow();
        if (window == IntPtr.Zero) return null;
        GetWindowThreadProcessId(window, out uint pid);
        if (pid == 0) return null;
        try
        {
            var process = Process.GetProcessById((int)pid);
            if (_settings.Jogos.Contains(process.ProcessName.ToLowerInvariant())) return process;
            process.Dispose();
        }
        catch (ArgumentException)
        {
            // Saiu nesse meio-tempo.
        }
        return null;
    }

    private static bool HasExited(Process process)
    {
        try
        {
            process.Refresh();
            return process.HasExited;
        }
        catch (InvalidOperationException)
        {
            return true;
        }
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);
}
