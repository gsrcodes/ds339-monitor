using DS339Monitor.App;

namespace DS339Monitor.Sensors;

/// <summary>Curva dos fans: pontos (temperatura °C, velocidade %) para a CPU e para a placa de vídeo.</summary>
internal sealed class FanSettings
{
    /// <summary>A curva manda na saída escolhida. Desligado = a BIOS manda (o programa não toca nos fans).</summary>
    public bool Ligado { get; set; }

    /// <summary>A saída de PWM do hub (0 = PWM 1 do chip da placa); null = ainda não escolhida.</summary>
    public int? Saida { get; set; }

    public string Perfil { get; set; } = FanCurve.Balanced;

    public List<int[]> CurvaCpu { get; set; } = FanCurve.Presets[FanCurve.Balanced].Cpu.Select(p => (int[])p.Clone()).ToList();

    public List<int[]> CurvaGpu { get; set; } = FanCurve.Presets[FanCurve.Balanced].Gpu.Select(p => (int[])p.Clone()).ToList();

    /// <summary>Velocidade mínima, 15–60%.</summary>
    public int Piso { get; set; } = 25;

    public void Normalize()
    {
        if (Saida is < 0 or > 15) Saida = null;
        if (Saida is null) Ligado = false;
        Perfil = FanCurve.Profiles.Contains(Perfil) ? Perfil : FanCurve.Balanced;
        CurvaCpu = FanCurve.Clean(CurvaCpu, FanCurve.Presets[FanCurve.Balanced].Cpu);
        CurvaGpu = FanCurve.Clean(CurvaGpu, FanCurve.Presets[FanCurve.Balanced].Gpu);
        Piso = Math.Clamp(Piso, 15, 60);
    }
}

/// <summary>Uma saída de PWM da placa como o painel mostra: valor null = a BIOS está mandando (modo automático).</summary>
internal sealed record FanOutput(int Index, string Name, double? Value);

internal sealed record FanSnapshot(
    string? Board, IReadOnlyList<FanOutput> Outputs, IReadOnlyList<(string Name, double? Rpm)> Fans,
    double? CpuTemp, double? GpuTemp, double? Target, int? Applied, string Status);

/// <summary>
/// A curva dos fans do gabinete (o hub ligado no SYS_FAN), numa thread própria que acorda uma vez por segundo. Escreve
/// na saída de PWM escolhida pelo LibreHardwareMonitor (chip ITE IT8613E da placa), sempre sob a trava dele — a mesma da
/// leitura dos sensores. Cuidados:
/// <list type="bullet">
/// <item>desligada, sem saída escolhida ou sem acesso à placa: não escreve nada (a BIOS manda);</item>
/// <item>sem temperatura nova por 15 s (o driver da AMD às vezes trava a leitura): devolve o controle à BIOS;</item>
/// <item>CPU ≥ 88 °C ou placa de vídeo ≥ 85 °C: 100%;</item>
/// <item>sobe depressa (10%/s) e desce devagar (2%/s), com mínimo configurável;</item>
/// <item>ao desligar a curva ou fechar o programa, devolve o controle à BIOS (o LibreHardwareMonitor também devolve ao
/// fechar). Só um programa travado sem fechar deixaria o fan na última velocidade.</item>
/// </list>
/// "Testar" põe uma saída em 100% por 6 s, para achar qual é a do hub.
/// </summary>
internal sealed class FanCurve : IDisposable
{
    public const string Quiet = "Silenciosa";
    public const string Balanced = "Equilibrada";
    public const string Performance = "Desempenho";
    public const string Custom = "Personalizada";
    public static readonly string[] Profiles = { Quiet, Balanced, Performance, Custom };

    /// <summary>Curvas prontas. A 7800X3D esquenta rápido e roda quente; a placa de vídeo parada fica perto de 30 °C.</summary>
    public static readonly Dictionary<string, (int[][] Cpu, int[][] Gpu)> Presets = new()
    {
        [Quiet] = (new[] { new[] { 50, 22 }, new[] { 65, 32 }, new[] { 76, 55 }, new[] { 86, 100 } },
                   new[] { new[] { 50, 22 }, new[] { 63, 35 }, new[] { 74, 60 }, new[] { 83, 100 } }),
        [Balanced] = (new[] { new[] { 45, 28 }, new[] { 60, 38 }, new[] { 72, 62 }, new[] { 82, 100 } },
                      new[] { new[] { 45, 28 }, new[] { 58, 42 }, new[] { 70, 68 }, new[] { 80, 100 } }),
        [Performance] = (new[] { new[] { 40, 38 }, new[] { 55, 52 }, new[] { 67, 78 }, new[] { 77, 100 } },
                         new[] { new[] { 40, 38 }, new[] { 53, 56 }, new[] { 65, 82 }, new[] { 75, 100 } }),
    };

    private const int TickMs = 1000;
    private const long StaleMs = 15_000;
    private const int TestMs = 6000;
    private const double CpuPanic = 88, GpuPanic = 85;
    private const double RiseRate = 10, FallRate = 2; // % por segundo

    private readonly FanSettings _settings;
    private readonly Func<HardwareMonitor?> _hardware;
    private readonly Thread _thread;
    private readonly AutoResetEvent _wake = new(false);
    private volatile bool _stop;
    private double _cpuTemp = double.NaN, _gpuTemp = double.NaN;
    private long _tempsAt;
    private int _testIndex = -1;
    private long _testUntil;
    private HardwareMonitor? _boundTo;
    private int? _controlling;
    private int _written = -1;
    private double _applied = double.NaN;
    private long _lastTick;
    private bool _staleLogged;
    private volatile FanSnapshot? _snapshot;

    public FanCurve(FanSettings settings, Func<HardwareMonitor?> hardware)
    {
        _settings = settings;
        _hardware = hardware;
        _thread = new Thread(Run) { IsBackground = true, Name = "Curva dos fans", Priority = ThreadPriority.AboveNormal };
    }

    /// <summary>O que o painel mostra (atualizado a cada segundo).</summary>
    public FanSnapshot? Snapshot => _snapshot;

    /// <summary>A curva precisa da temperatura da CPU e da placa de vídeo.</summary>
    public IEnumerable<string> SensorKeys => _settings.Ligado ? new[] { "CPUTemp", "GPUTemp" } : Array.Empty<string>();

    private int _keysChanged = 1;

    public bool TakeSensorKeysChanged() => Interlocked.Exchange(ref _keysChanged, 0) == 1;

    public void Start() => _thread.Start();

    /// <summary>Configuração mudou pelo painel.</summary>
    public void Reload()
    {
        Interlocked.Exchange(ref _keysChanged, 1);
        _wake.Set();
    }

    /// <summary>Leitura nova dos sensores (a thread de monitoramento chama depois de cada leitura).</summary>
    public void UpdateTemps(double? cpu, double? gpu)
    {
        lock (_wake)
        {
            _cpuTemp = cpu ?? double.NaN;
            _gpuTemp = gpu ?? double.NaN;
            if (cpu is not null || gpu is not null) _tempsAt = Environment.TickCount64;
        }
    }

    /// <summary>Põe a saída <paramref name="index"/> em 100% por 6 s.</summary>
    public void Test(int index)
    {
        Volatile.Write(ref _testUntil, Environment.TickCount64 + TestMs);
        Volatile.Write(ref _testIndex, index);
        _wake.Set();
    }

    public void Stop()
    {
        if (_stop) return;
        _stop = true;
        _wake.Set();
        if (_thread.IsAlive && !_thread.Join(3000)) Log.Warn("Curva dos fans: a thread não terminou a tempo.");
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
                    Log.Error("Curva dos fans", ex);
                    _controlling = null;
                    _written = -1;
                }
                _wake.WaitOne(TickMs);
            }
        }
        finally
        {
            ReleaseAll("programa fechando");
        }
    }

    private void Tick()
    {
        var hardware = _hardware();
        if (hardware is null || !hardware.HasBoard || hardware.IsDisposed)
        {
            _snapshot = new FanSnapshot(null, Array.Empty<FanOutput>(), Array.Empty<(string, double?)>(), null, null, null, null,
                Environment.IsPrivilegedProcess ? "lendo a placa-mãe…" : "precisa de administrador");
            return;
        }

        long now = Environment.TickCount64;
        double dt = _lastTick == 0 ? 1 : Math.Clamp((now - _lastTick) / 1000.0, 0.1, 5);
        _lastTick = now;
        double cpu, gpu;
        long tempsAt;
        lock (_wake)
        {
            cpu = _cpuTemp;
            gpu = _gpuTemp;
            tempsAt = _tempsAt;
        }

        lock (hardware.Gate)
        {
            if (hardware.IsDisposed) return;
            if (!ReferenceEquals(hardware, _boundTo))
            {
                // Leitor novo (os sensores foram reabertos): o controle anterior foi devolvido à BIOS ao fechar.
                _boundTo = hardware;
                _controlling = null;
                _written = -1;
            }
            var controls = hardware.BoardControls;
            string status;
            double? target = null;

            int test = Volatile.Read(ref _testIndex);
            if (test >= 0 && now < Volatile.Read(ref _testUntil) && test < controls.Count)
            {
                if (controls[test].Value is not >= 99.5f) Log.Info($"Curva dos fans: testando a PWM {test + 1} em 100% (estava {(controls[test].Value is float v ? $"{v:0}%" : "no automático da BIOS")}).");
                controls[test].Control!.SetSoftware(100);
                if (_controlling == test) _written = 100;
                status = $"testando a PWM {test + 1} (100%)";
            }
            else
            {
                if (test >= 0)
                {
                    Volatile.Write(ref _testIndex, -1);
                    if (test < controls.Count && test != _controlling) controls[test].Control!.SetDefault();
                    _written = -1; // reaplica a curva
                    Log.Info($"Curva dos fans: teste da PWM {test + 1} terminado.");
                }

                int? wanted = _settings.Ligado ? _settings.Saida : null;
                if (_controlling is int previous && previous != wanted)
                {
                    if (previous < controls.Count) controls[previous].Control!.SetDefault();
                    Log.Info($"Curva dos fans: a BIOS voltou a mandar na PWM {previous + 1}.");
                    _controlling = null;
                    _written = -1;
                    _applied = double.NaN;
                }

                if (wanted is not int output || output >= controls.Count)
                {
                    status = _settings.Saida is null ? "escolha a saída do hub" : _settings.Ligado ? "saída não encontrada" : "a BIOS manda";
                }
                else if (now - tempsAt > StaleMs || (double.IsNaN(cpu) && double.IsNaN(gpu)))
                {
                    // Sem temperatura confiável: quem sabe a temperatura agora é a BIOS.
                    if (_controlling is not null)
                    {
                        controls[output].Control!.SetDefault();
                        _controlling = null;
                        _written = -1;
                    }
                    // Antes da primeira leitura (programa abrindo) é normal: só avisa se as leituras pararam.
                    if (!_staleLogged && tempsAt != 0) Log.Warn("Curva dos fans: sem leitura de temperatura há 15 s; a BIOS manda até voltar.");
                    _staleLogged = tempsAt != 0;
                    status = tempsAt == 0 ? "esperando a primeira leitura" : "sem temperatura: a BIOS manda";
                }
                else
                {
                    _staleLogged = false;
                    double want = Compute(cpu, gpu);
                    target = want;
                    if (double.IsNaN(_applied)) _applied = want;
                    else if (want > _applied) _applied = Math.Min(want, _applied + RiseRate * dt);
                    else _applied = Math.Max(want, _applied - FallRate * dt);
                    int duty = (int)Math.Round(_applied);
                    if (_controlling != output || duty != _written)
                    {
                        controls[output].Control!.SetSoftware(duty);
                        if (_controlling != output) Log.Info($"Curva dos fans: a curva assumiu a PWM {output + 1} ({_settings.Perfil}, {duty}%).");
                        _controlling = output;
                        _written = duty;
                    }
                    status = $"curva {_settings.Perfil.ToLowerInvariant()}";
                }
            }

            _snapshot = new FanSnapshot(
                hardware.BoardName,
                controls.Select((c, i) => new FanOutput(i, $"PWM {i + 1}", c.Value)).ToArray(),
                hardware.BoardFans.Select(f => (f.Name, (double?)f.Value)).ToArray(),
                double.IsNaN(cpu) ? null : cpu,
                double.IsNaN(gpu) ? null : gpu,
                target,
                _controlling is null ? null : _written,
                status);
        }
    }

    /// <summary>A velocidade que as duas curvas pedem (a maior), com o mínimo e a trava de temperatura alta.</summary>
    public double Compute(double cpu, double gpu)
    {
        if (cpu >= CpuPanic || gpu >= GpuPanic) return 100;
        double want = 0;
        if (!double.IsNaN(cpu)) want = Math.Max(want, Interpolate(_settings.CurvaCpu, cpu));
        if (!double.IsNaN(gpu)) want = Math.Max(want, Interpolate(_settings.CurvaGpu, gpu));
        return Math.Clamp(want, _settings.Piso, 100);
    }

    public static double Interpolate(IReadOnlyList<int[]> points, double temperature)
    {
        if (points.Count == 0) return 100;
        if (temperature <= points[0][0]) return points[0][1];
        for (int i = 1; i < points.Count; i++)
        {
            if (temperature > points[i][0]) continue;
            double k = (temperature - points[i - 1][0]) / Math.Max(1, points[i][0] - points[i - 1][0]);
            return points[i - 1][1] + (points[i][1] - points[i - 1][1]) * k;
        }
        return points[^1][1];
    }

    /// <summary>Pontos válidos: 2 a 8, temperaturas crescentes entre 20 e 100 °C, velocidades 0–100%.</summary>
    public static List<int[]> Clean(List<int[]>? points, int[][] fallback)
    {
        var clean = (points ?? new List<int[]>())
            .Where(p => p is { Length: 2 })
            .Select(p => new[] { Math.Clamp(p[0], 20, 100), Math.Clamp(p[1], 0, 100) })
            .OrderBy(p => p[0])
            .ToList();
        for (int i = clean.Count - 1; i > 0; i--)
            if (clean[i][0] <= clean[i - 1][0]) clean.RemoveAt(i);
        return clean.Count is >= 2 and <= 8 ? clean : fallback.Select(p => (int[])p.Clone()).ToList();
    }

    private void ReleaseAll(string why)
    {
        var hardware = _boundTo;
        if (hardware is null) return;
        try
        {
            lock (hardware.Gate)
            {
                if (hardware.IsDisposed) return;
                var controls = hardware.BoardControls;
                foreach (int index in new[] { _controlling ?? -1, Volatile.Read(ref _testIndex) }.Where(i => i >= 0 && i < controls.Count).Distinct())
                {
                    controls[index].Control!.SetDefault();
                    Log.Info($"Curva dos fans: a BIOS voltou a mandar na PWM {index + 1} ({why}).");
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("Curva dos fans: devolvendo à BIOS", ex);
        }
        _controlling = null;
    }
}
