using System.Diagnostics;
using System.Drawing;
using DS339Monitor.Argb;
using DS339Monitor.Game;
using DS339Monitor.Lighting;
using DS339Monitor.Manga;
using DS339Monitor.Sensors;
using DS339Monitor.Themes;
using DS339Monitor.World;

namespace DS339Monitor.App;

/// <summary>
/// Thread única de prioridade baixa que cuida das duas telas: lê os sensores uma vez por segundo (uma leitura
/// serve as duas), desenha e envia. Dorme até o próximo evento — virada do segundo ou próximo quadro da animação
/// (com o timer de alta resolução: ver <see cref="PreciseWait"/>).
/// Sem nenhuma tela conectada, só verifica a cada meio segundo se alguma apareceu.
/// O LED da placa de vídeo tem thread própria (<see cref="GpuLighting"/>); daqui ele só recebe as leituras de que precisa
/// e, nos efeitos que acompanham a tela, a medida de cada quadro da Pro Flow.
/// </summary>
internal sealed class MonitorService
{
    private const int DisconnectedPollMs = 500;

    private readonly AppConfig _config;
    private readonly ScreenPipeline[] _screens;
    private readonly Thread _thread;
    private readonly AutoResetEvent _wake = new(false);
    private readonly LoopStats _stats = new();
    private readonly SceneDirector _director = new();
    private readonly WorldDirector _world = new();
    private readonly MangaStage _manga = new();
    private volatile string? _story;
    private volatile bool _stop;
    private volatile bool _captureRequested;
    private volatile string _status = "Iniciando…";

    public MonitorService(AppConfig config)
    {
        _config = config;
        Ds339 = new Ds339Screen(config);
        ProFlow = new ProFlowScreen(config);
        Thermalright = new ThermalrightScreen(config);
        _screens = new ScreenPipeline[] { Ds339, ProFlow, Thermalright };
        GpuLed = new GpuLighting(config.LedGpu);
        Argb = new ArgbLighting(config.FansEFitas, GpuColorForArgb, () => ProFlow.IsReady ? ProFlow.Light : null);
        GpuLed.FansSample = Argb.GpuSample;
        Fans = new FanCurve(config.Ventoinhas, () => _hub?.Hardware);
        Game = new GameMode(config.ModoJogo, Thermalright);
        // A partida do CS2 no gabinete: só com o modo jogo e as reações ligados.
        Func<Cs2Moment?> reactions = () => config.ModoJogo is { Ligado: true, Reacoes: true } || Cs2Events.Simulating ? Cs2Events.Moment() : null;
        Argb.GameReactions = reactions;
        GpuLed.GameReactions = reactions;
        // No Modo Mangá, a explosão vira o DON! das telas e o ace, o Haki do Rei.
        Cs2Events.Happened += kind =>
        {
            if (!config.ModoJogo.Reacoes && !Cs2Events.Simulating) return;
            if (kind == Cs2Events.Kind.Exploded) TriggerStory("impacto");
            else if (kind == Cs2Events.Kind.Ace) TriggerStory("haki");
            Argb.Nudge();
            GpuLed.Nudge();
        };
        _thread = new Thread(Run) { IsBackground = true, Name = "Monitor", Priority = ThreadPriority.BelowNormal };
    }

    public Ds339Screen Ds339 { get; }
    public ProFlowScreen ProFlow { get; }
    public ThermalrightScreen Thermalright { get; }
    public GpuLighting GpuLed { get; }

    /// <summary>Fans e fitas na controladora Nollie.</summary>
    public ArgbLighting Argb { get; }

    /// <summary>Curva dos fans do gabinete (PWM da placa-mãe).</summary>
    public FanCurve Fans { get; }

    /// <summary>Modo jogo (painel de jogo na Thermalright).</summary>
    public GameMode Game { get; }

    private volatile SensorHub? _hub;

    /// <summary>Diretor de cena do Modo Grand Line.</summary>
    public SceneDirector Director => _director;

    /// <summary>Dispara uma história do mundo (menu "Testar cena"); roda na thread de animação, no próximo ciclo.</summary>
    public void TriggerStory(string story)
    {
        _story = story;
        _wake.Set();
    }
    public IReadOnlyList<ScreenPipeline> Screens => _screens;
    public string Status => _status;

    /// <summary>Onde "Salvar imagem das telas" grava o quadro atual de cada tela (sem girar).</summary>
    public static string CapturePath(ScreenKind kind) =>
        Path.Combine(AppContext.BaseDirectory, kind switch
        {
            ScreenKind.ProFlow => "tela-proflow.png",
            ScreenKind.Thermalright => "tela-thermalright.png",
            _ => "tela-ds339.png",
        });

    /// <summary>Criado por <c>--historia nome</c> (outro processo) para disparar uma história do mundo.</summary>
    public static string StoryFlagPath => Path.Combine(AppContext.BaseDirectory, "historia.flag");

    /// <summary>Criado por <c>--capturar</c> (outro processo, possivelmente sem admin) para pedir uma captura.</summary>
    public static string CaptureFlagPath => Path.Combine(AppContext.BaseDirectory, "capturar.flag");

    /// <summary>Criado por <c>--placa-mae</c>: a instância aberta (com administrador) registra os fans da placa no log.</summary>
    public static string BoardFlagPath => Path.Combine(AppContext.BaseDirectory, "placa.flag");

    /// <summary>Criado por <c>--desligar-tudo</c> / <c>--ligar-tudo</c> / <c>--alternar-tudo</c> (outro processo).</summary>
    public static string AllOffFlagPath => Path.Combine(AppContext.BaseDirectory, "tudo.flag");

    /// <summary>Criado por <c>--fitas assumir|devolver|identificar N</c> (outro processo).</summary>
    public static string ArgbFlagPath => Path.Combine(AppContext.BaseDirectory, "fitas.flag");

    private readonly object _allOffGate = new();
    private Task _signalRgbWork = Task.CompletedTask;

    /// <summary>
    /// Existe enquanto o SignalRGB está fechado por causa do "desligar tudo" — num arquivo, para valer mesmo se este
    /// programa for fechado ou reiniciado no meio.
    /// </summary>
    private static string SignalRgbClosedPath => Path.Combine(AppContext.BaseDirectory, "signalrgb-fechado.flag");

    /// <summary>Sensores que o painel mostra (lidos só enquanto ele está aberto).</summary>
    public static readonly string[] DashboardKeys = { "CPUTemp", "CpuUsage", "CpuTEC", "GPUTemp", "GpuUsage", "GpuTEC", "GpuFan", "MemoryUsage" };

    private volatile bool _dashboard;
    private bool _dashboardKeys;
    private volatile IReadOnlyDictionary<string, double> _readings = new Dictionary<string, double>();

    /// <summary>O painel está aberto: miniaturas das telas e os sensores dele são lidos; fechado, nada disso custa.</summary>
    public bool DashboardOpen
    {
        get => _dashboard;
        set
        {
            _dashboard = value;
            foreach (var screen in _screens) screen.PreviewWanted = value;
            _wake.Set();
        }
    }

    /// <summary>Última leitura dos sensores do painel (NaN = sem leitura).</summary>
    public IReadOnlyDictionary<string, double> Readings => _readings;

    /// <summary>"Desligar tudo" ligado: telas pretas (a Pro Flow apagada), LED preto e o SignalRGB fechado.</summary>
    public bool AllOff { get; private set; }

    /// <summary>
    /// Desliga (ou religa) tudo: as telas e o LED na hora; o SignalRGB em segundo plano, numa fila (fechar leva ~2 s —
    /// ele apaga os próprios dispositivos ao sair). Ao religar, o SignalRGB só é reaberto se foi este programa que o
    /// fechou.
    /// </summary>
    public void SetAllOff(bool off)
    {
        lock (_allOffGate)
        {
            if (off == AllOff) return;
            AllOff = off;
            foreach (var screen in _screens) screen.Blackout = off;
            GpuLed.Blackout = off;
            Argb.Blackout = off;
            _wake.Set();
            Log.Info(off ? "Desligar tudo: telas, LED da placa, fans e fitas e SignalRGB." : "Religando tudo.");
            _signalRgbWork = _signalRgbWork.ContinueWith(_ =>
            {
                try
                {
                    if (off)
                    {
                        if (AllOff && SignalRgb.IsRunning() && SignalRgb.Close(TimeSpan.FromSeconds(20)))
                            File.WriteAllText(SignalRgbClosedPath, DateTime.Now.ToString("O"));
                    }
                    else
                    {
                        ReopenSignalRgb();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("SignalRGB", ex);
                }
            }, TaskScheduler.Default);
        }
    }

    public void Start()
    {
        // O programa começa sempre com tudo aceso: se o SignalRGB ficou fechado por um "desligar tudo" de antes (o
        // programa foi fechado ou reiniciado no meio), reabre. Logo depois de ligar o PC, o próprio início com o Windows
        // já abre o SignalRGB: aí só esquece.
        if (File.Exists(SignalRgbClosedPath))
        {
            if (Environment.TickCount64 < 3 * 60_000) TryDelete(SignalRgbClosedPath);
            else _signalRgbWork = Task.Run(ReopenSignalRgb);
        }
        _thread.Start();
        GpuLed.Start();
        Argb.Start();
        Fans.Start();
        Game.Start();
    }

    // ---------------------------------------------------------------- fans e fitas

    private Color? _cardWhite;

    /// <summary>
    /// A cor do LED da placa para o efeito "Igual à placa de vídeo": o branco guardado na placa (255, 210, 255, que nela
    /// é branco) vira branco neutro nos fans. Null = o LED da placa não está com o programa.
    /// </summary>
    private Color? GpuColorForArgb()
    {
        var shown = GpuLed.Shown;
        if (shown.A == 0) return null;
        if (_config.LedGpu.Branco != GpuEffects.WhiteCard) return shown;
        var white = _cardWhite ??= AsrockGpuLed.SavedColor() ?? Color.White;
        if (Math.Min(white.R, Math.Min(white.G, white.B)) < 150) return shown;
        static int Fix(int v, int w) => Math.Clamp(v * 255 / w, 0, 255);
        return Color.FromArgb(Fix(shown.R, white.R), Fix(shown.G, white.G), Fix(shown.B, white.B));
    }

    /// <summary>
    /// Os canais usados (Channel 1–16) com a quantidade de LEDs de cada um, do piso para o topo do gabinete (pela posição
    /// das partes na foto): a ordem em que os efeitos da própria controladora atravessam as peças.
    /// </summary>
    public IReadOnlyList<(int Logical, int Leds)> ArgbChannelsBottomUp() =>
        _config.FansEFitas.Zonas.GroupBy(z => z.Canal)
            .Select(g => (
                Logical: g.Key,
                Leds: g.Max(z => z.Inicio + z.Leds),
                Height: g.SelectMany(z => CaseLayout.Parse(z.Forma, z.Leds, false) ?? Array.Empty<LedPoint>()).Select(p => p.V).DefaultIfEmpty(-1).Average()))
            .OrderByDescending(c => c.Height)
            .ThenBy(c => c.Logical)
            .Select(c => (c.Logical, c.Leds))
            .ToList();

    /// <summary>
    /// Passa os fans e fitas para este programa: liga o controle e, se o SignalRGB estiver aberto, fecha ele pelo caminho
    /// normal (nunca à força). True = o programa ficou com a controladora (ou vai ficar assim que o SignalRGB sair).
    /// </summary>
    public Task<bool> TakeArgbControl()
    {
        _config.FansEFitas.Ligado = true;
        _config.Save();
        Argb.Reload();
        TryDelete(SignalRgbClosedPath); // escolheu o programa: o "religar tudo" não reabre o SignalRGB por cima
        Log.Info("Fans e fitas: controle passado para o programa.");
        return Task.Run(() =>
        {
            try
            {
                return !SignalRgb.IsRunning() || SignalRgb.Close(TimeSpan.FromSeconds(20));
            }
            catch (Exception ex)
            {
                Log.Error("SignalRGB", ex);
                return false;
            }
        });
    }

    /// <summary>Devolve os fans e fitas ao SignalRGB: o programa solta a controladora e reabre o SignalRGB.</summary>
    public void ReturnArgbToSignalRgb()
    {
        _config.FansEFitas.Ligado = false;
        _config.Save();
        Argb.Reload();
        Log.Info("Fans e fitas: controle devolvido ao SignalRGB.");
        Task.Run(() =>
        {
            try
            {
                Thread.Sleep(500); // a thread dos fans solta a controladora antes
                if (!SignalRgb.IsRunning()) SignalRgb.Start();
            }
            catch (Exception ex)
            {
                Log.Error("SignalRGB", ex);
            }
        });
    }

    private void HandleArgbFlag()
    {
        try
        {
            string[] command = File.ReadAllText(ArgbFlagPath).Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            File.Delete(ArgbFlagPath);
            switch (command.FirstOrDefault())
            {
                case "assumir":
                    _ = TakeArgbControl();
                    break;
                case "devolver":
                    ReturnArgbToSignalRgb();
                    break;
                case "boot":
                    _ = Argb.PreviewBoot(12);
                    break;
                case "gravar-animacao" when command.Length > 1:
                {
                    // Só grava a animação na memória da controladora e confere; o que toca ao ligar não muda.
                    // "gravar-animacao Curto-circuito 3" = emendada em 3 min do loop dos fans.
                    int minutes = command.Length > 2 && int.TryParse(command[^1], out int m) ? m : 0;
                    string name = string.Join(' ', command.Skip(1).Take(command.Length - 1 - (minutes > 0 ? 1 : 0)));
                    if (!BootAnimation.All.Contains(name)) break;
                    var argb = _config.FansEFitas;
                    bool grb = argb.NaControladora?.CoresGrb ?? true;
                    var (file, map, source) = NollieBootFile.Build(argb, name, minutes, grb);
                    string key = NollieBootFile.Key(argb, name, minutes, grb);
                    var started = Stopwatch.StartNew();
                    Argb.RunOnDevice(device =>
                    {
                        try
                        {
                            return NollieFseq.Upload(device, file, map, Argb.SetJobStatus);
                        }
                        finally
                        {
                            Argb.SetJobStatus(null);
                        }
                    }).ContinueWith(t =>
                    {
                        Log.Info($"Nollie: gravar \"{source.Name}\" ({source.Count} quadros, {file.Length} bytes, CRC {NollieFseq.Crc(file):X8}) — {t.Result} em {started.ElapsedMilliseconds} ms.");
                        if (t.Result == "ok" && argb.NaControladora is { } draft)
                        {
                            draft.AnimacaoGravada = key; // escolher essa mesma no painel não grava de novo
                            _config.Save();
                        }
                    });
                    break;
                }
                case "escolher-boot" when command.Length > 1 && _config.FansEFitas.NaControladora is { } choice:
                {
                    // Só o rascunho do painel (nada vai para a controladora até "Gravar").
                    string name = string.Join(' ', command.Skip(1));
                    choice.BootAnimacao = BootAnimation.All.Contains(name) ? name : null;
                    _config.Save();
                    break;
                }
                case "comparar" when command.Length > 1:
                {
                    // "comparar 50" / "comparar off": a placa e todas as partes no mesmo branco (o balanço de cada uma aplicado).
                    int level = int.TryParse(command[1], out int l) ? Math.Clamp(l, 10, 100) : 0;
                    double linear = Math.Pow(level / 100.0, 2);
                    Argb.SetCalibration((float)linear);
                    int v = (int)Math.Round(255 * linear);
                    GpuLed.CalibrationWhite = level > 0 ? Color.FromArgb(v, v, v) : null;
                    Log.Info(level > 0 ? $"Comparar brancos: ligado ({level}%)." : "Comparar brancos: desligado.");
                    break;
                }
                case "presentmon" when command.Length > 1:
                {
                    // Teste: 3 s de captura do PresentMon num processo (ex.: dwm.exe), em presentmon-teste.csv.
                    string exe = Path.Combine(AppContext.BaseDirectory, "tools", "PresentMon.exe");
                    string csv = Path.Combine(AppContext.BaseDirectory, "presentmon-teste.csv");
                    var info = new ProcessStartInfo(exe, $"--process_name {command[1]} --timed 3 --terminate_after_timed --output_file \"{csv}\" --stop_existing_session --session_name DS339Teste --no_console_stats")
                    { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
                    Task.Run(() =>
                    {
                        using var process = Process.Start(info)!;
                        string output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
                        process.WaitForExit(15000);
                        Log.Info($"PresentMon (teste): saída {process.ExitCode}; {output.Trim().Replace(Environment.NewLine, " ")}");
                    });
                    break;
                }
                case "jogo-previa":
                    Game.Preview(20);
                    break;
                case "cs2-teste":
                    Game.Preview(17);
                    Cs2Events.Simulate();
                    break;
                case "jogo-testar":
                    Game.Force(command.Length > 1 ? command[1] : null);
                    break;
                case "tocar-animacao":
                    _ = Argb.PreviewStoredAnimation(8000);
                    break;
                case "animacao" when command.Length > 1:
                {
                    string name = string.Join(' ', command.Skip(1));
                    if (BootAnimation.All.Contains(name))
                        Argb.Play(BootAnimation.Render(name, _config.FansEFitas.Zonas, _config.FansEFitas));
                    break;
                }
                case "ventoinha" when command.Length > 1 && int.TryParse(command[1], out int output) && output >= 1:
                    Fans.Test(output - 1);
                    break;
                case "identificar" when command.Length > 1 && int.TryParse(command[1], out int zone):
                    Argb.Identify(zone);
                    break;
            }
        }
        catch (IOException)
        {
        }
    }

    /// <summary>Reabre o SignalRGB se foi o "desligar tudo" que o fechou (e ele não foi aberto por outro caminho).</summary>
    private static void ReopenSignalRgb()
    {
        if (!File.Exists(SignalRgbClosedPath)) return;
        TryDelete(SignalRgbClosedPath);
        if (!SignalRgb.IsRunning()) SignalRgb.Start();
    }

    private static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (IOException)
        {
        }
    }

    /// <summary>Acorda a thread para aplicar logo uma mudança feita pelo menu.</summary>
    public void Wake() => _wake.Set();

    public void RequestCapture()
    {
        _captureRequested = true;
        _wake.Set();
    }

    public void Stop()
    {
        Fans.Stop(); // devolve os fans à BIOS antes de os sensores fecharem
        Game.Stop();
        _stop = true;
        _wake.Set();
        if (_thread.IsAlive && !_thread.Join(5000))
            Log.Warn("A thread de monitoramento não terminou a tempo.");
        GpuLed.Stop(); // devolve as cores originais do LED da placa
        Argb.Stop();
        _director.Dispose();
    }

    private void Run()
    {
        try
        {
            foreach (var screen in _screens) screen.Start();
            var sensors = new SensorHub { Board = Environment.IsPrivilegedProcess };
            _hub = sensors;
            using var wait = new PreciseWait(_wake);
            long nextSensors = 0;
            int errors = 0;
            // A leitura dos sensores roda fora desta thread: o driver da placa de vídeo às vezes segura a consulta por
            // dezenas de segundos (ex.: enquanto outro programa inicializa o Direct3D nela), e as telas não podem
            // congelar junto. Esta thread usa a última leitura pronta; a configuração só muda entre leituras.
            Task? sensorRead = null;
            long sensorStart = 0;
            string[]? pendingKeys = null;

            try
            {
                while (!_stop)
                {
                    bool anyReady = false;
                    try
                    {
                        bool keysChanged = GpuLed.TakeSensorKeysChanged() | Argb.TakeSensorKeysChanged() | Fans.TakeSensorKeysChanged() | Game.TakeSensorKeysChanged();
                        foreach (var screen in _screens)
                        {
                            screen.Pump();
                            keysChanged |= screen.EnsureTheme();
                        }

                        // Modo Grand Line / Modo Mangá: a cena liga quando um tema ou efeito precisa dela (e traz os sensores dela junto).
                        bool scene = _config.UsesScene;
                        bool manga = scene && _config.Manga.Ligado;
                        if (manga != MangaStage.InUse)
                        {
                            MangaStage.InUse = manga;
                            if (manga) MangaLayer.Preload(_config.Manga.GifGear5);
                        }
                        if (scene != SceneDirector.InUse)
                        {
                            SceneDirector.InUse = scene;
                            keysChanged = true;
                        }
                        bool dashboard = _dashboard;
                        if (dashboard != _dashboardKeys)
                        {
                            _dashboardKeys = dashboard;
                            keysChanged = true;
                        }
                        if (keysChanged)
                        {
                            // Um único SensorHub para tudo: lê a união do que os temas, o efeito do LED e a cena usam.
                            var keys = _screens.SelectMany(s => s.SensorKeys).Concat(GpuLed.SensorKeys).Concat(Argb.SensorKeys).Concat(Fans.SensorKeys).Concat(Game.SensorKeys);
                            if (dashboard) keys = keys.Concat(DashboardKeys);
                            if (scene)
                            {
                                keys = keys.Concat(SceneDirector.SensorKeys);
                                _director.Configure(_config.Cena.Latitude, _config.Cena.Longitude);
                                _world.Configure(new WorldLayout(_config.Cena.VaoTelasMm, _config.Cena.VaoPlacaMm, _config.Cena.DeslocamentoProFlowMm));
                            }
                            pendingKeys = keys.ToArray();
                        }
                        if (pendingKeys is not null && sensorRead is null)
                        {
                            sensors.Configure(pendingKeys, _config.Gpu);
                            pendingKeys = null;
                            nextSensors = 0;
                            GC.Collect(); // devolve de uma vez a memória do tema anterior
                        }

                        anyReady = _screens.Any(s => s.IsReady);
                        ProFlow.MeasureLight = GpuLed.WantsScreen || scene || Argb.WantsScreen;

                        // Quadros de animação que já venceram saem antes da leitura dos sensores (que pode levar alguns ms):
                        // assim o ritmo da animação não oscila. Os valores novos entram no quadro seguinte.
                        // O diretor de cena anda antes das telas: o quadro que sai agora já mostra o que ele decidiu.
                        if (SceneDirector.InUse)
                        {
                            _director.Update(ProFlow.IsReady ? ProFlow.Light : null);
                            if (Interlocked.Exchange(ref _story, null) is { } story)
                            {
                                if (manga) _manga.Trigger(story, Clock.Ms);
                                else if (story == "lightning") _director.Trigger(StageEventKind.Lightning);
                                else if (story == "mute") SystemSignals.Post(SignalKind.Muted);
                                else _world.Trigger(story, Clock.Ms);
                            }
                            if (manga)
                            {
                                _manga.Settings = _config.Manga;
                                _manga.Update(SceneDirector.Current, Clock.Ms);
                                if (_manga.TakeSeek(Clock.Ms)) ProFlow.SeekToCue(PanelCue.Drums);
                            }
                            else
                            {
                                _world.Update(SceneDirector.Current);
                            }
                        }
                        foreach (var screen in _screens) screen.Tick(sensors, freshValues: false);
                        if (GpuLed.WantsScreen) GpuLed.UpdateScreen(ProFlow.IsReady ? ProFlow.Light : null);

                        bool fresh = false;
                        if (sensorRead is null && pendingKeys is null && (anyReady || GpuLed.SensorKeys.Any() || Argb.SensorKeys.Any() || Fans.SensorKeys.Any() || Game.SensorKeys.Any()) && Clock.Ms >= nextSensors)
                        {
                            sensorStart = Stopwatch.GetTimestamp();
                            var hub = sensors;
                            sensorRead = Task.Run(() =>
                            {
                                try
                                {
                                    hub.Update();
                                }
                                finally
                                {
                                    _wake.Set();
                                }
                            });
                        }
                        if (sensorRead is { IsCompleted: true })
                        {
                            if (sensorRead.Exception is { } failure) Log.Error("Leitura dos sensores", failure.GetBaseException());
                            sensorRead = null;
                            _stats.AddSensors(Stopwatch.GetTimestamp() - sensorStart);
                            nextSensors = Clock.Ms + NextSensorDelay();
                            fresh = true;
                            GpuLed.UpdateSensors(sensors.Number("GPUTemp"), sensors.Number("GpuUsage"));
                            Argb.UpdateSensors(sensors.Number("CPUTemp"), sensors.Number("GPUTemp"), sensors.Number("GpuUsage"));
                            Fans.UpdateTemps(sensors.Number("CPUTemp"), sensors.Number("GPUTemp"));
                            if (Game.Showing)
                                GameTelemetry.SetSensors(sensors.Number("CPUTemp"), sensors.Number("CpuUsage"), sensors.Number("GPUTemp"),
                                    sensors.Number("GpuUsage"), sensors.Number("GpuTEC"));
                            if (_dashboardKeys) _readings = DashboardKeys.ToDictionary(k => k, k => sensors.Number(k) ?? double.NaN);
                            if (SceneDirector.InUse)
                                _director.UpdateSensors(sensors.Number("CPUTemp"), sensors.Number("GPUTemp"), sensors.Number("GpuUsage"), sensors.Number("NetDownBytes"));
                            foreach (var screen in _screens) screen.Tick(sensors, freshValues: true);
                        }

                        if (_captureRequested || File.Exists(CaptureFlagPath)) SaveCaptures();
                        if (File.Exists(BoardFlagPath))
                        {
                            TryDelete(BoardFlagPath);
                            // Num processo à parte (herda o administrador): um segundo leitor do LibreHardwareMonitor no mesmo
                            // processo fecha o driver que o leitor das telas usa.
                            Process.Start(new ProcessStartInfo(Environment.ProcessPath!, "--placa-mae") { UseShellExecute = false, CreateNoWindow = true })?.Dispose();
                        }
                        if (fresh && File.Exists(AllOffFlagPath))
                        {
                            try
                            {
                                string wanted = File.ReadAllText(AllOffFlagPath).Trim();
                                File.Delete(AllOffFlagPath);
                                SetAllOff(wanted switch { "desligar" => true, "ligar" => false, _ => !AllOff });
                            }
                            catch (IOException)
                            {
                            }
                        }
                        if (fresh && File.Exists(ArgbFlagPath)) HandleArgbFlag();
                        if (fresh && File.Exists(StoryFlagPath))
                        {
                            try
                            {
                                TriggerStory(File.ReadAllText(StoryFlagPath).Trim());
                                File.Delete(StoryFlagPath);
                            }
                            catch (IOException)
                            {
                            }
                        }

                        if (fresh)
                        {
                            string summary = sensors.Summary();
                            _status = summary.Length > 0 ? summary : "Telas conectadas";
                            if (_stats.IsDue) _stats.Log(summary, _screens);
                        }
                        else if (!anyReady)
                        {
                            _status = "Aguardando as telas…";
                        }
                    }
                    catch (Exception ex)
                    {
                        // Um ciclo com problema não pode congelar as telas: registra e tenta de novo no próximo.
                        errors++;
                        if (errors <= 3 || errors % 300 == 0) Log.Error($"Ciclo de atualização (erro nº {errors})", ex);
                        _status = "Erro: " + ex.Message;
                    }

                    wait.Wait(NextWait(anyReady, sensorRead is null ? nextSensors : long.MaxValue));
                }
            }
            finally
            {
                // Leitura presa no driver: não desmonta os sensores por baixo dela (o processo está saindo de qualquer jeito).
                if (sensorRead is null || sensorRead.Wait(2000)) sensors.Dispose();
            }
        }
        catch (Exception ex)
        {
            Log.Error("Loop principal", ex);
            _status = "Erro: " + ex.Message;
        }
        finally
        {
            foreach (var screen in _screens)
            {
                var timer = Stopwatch.StartNew();
                try
                {
                    screen.Dispose();
                }
                catch (Exception ex)
                {
                    Log.Error($"Fechando {screen.Name}", ex);
                }
                if (timer.ElapsedMilliseconds > 500) Log.Info($"Fechando {screen.Name}: {timer.ElapsedMilliseconds} ms.");
            }
        }
    }

    private void SaveCaptures()
    {
        _captureRequested = false;
        foreach (var screen in _screens)
        {
            if (!screen.IsReady) continue;
            try
            {
                screen.SaveCapture(CapturePath(screen.Kind));
                Log.Info($"Imagem da tela {screen.Name} salva em {CapturePath(screen.Kind)}");
            }
            catch (Exception ex)
            {
                Log.Error($"Salvando imagem da tela {screen.Name}", ex);
            }
        }
        try
        {
            File.Delete(CaptureFlagPath);
        }
        catch (IOException)
        {
        }
    }

    private int NextSensorDelay()
    {
        int interval = _config.IntervaloMs;
        // Com intervalo de 1 s, acorda logo depois da virada do segundo: o relógio da tela anda junto com o do Windows.
        return interval == 1000 ? 1000 - DateTime.Now.Millisecond + 20 : interval;
    }

    private int NextWait(bool anyReady, long nextSensors)
    {
        if (!anyReady) return DisconnectedPollMs;
        long due = Math.Min(nextSensors, _screens.Min(s => s.NextWakeDue));
        return (int)Math.Clamp(due - Clock.Ms, 1, 1000);
    }

    /// <summary>Custo médio de cada etapa e memória do processo, no log a cada tanto (1 min depois de abrir, depois a cada 30).</summary>
    private sealed class LoopStats
    {
        private readonly long _startedAt = Environment.TickCount64;
        private long _sensorTicks;
        private int _sensorReads;
        private long _nextLog = Environment.TickCount64 + 75_000;

        public bool IsDue => Environment.TickCount64 >= _nextLog;

        public void AddSensors(long ticks)
        {
            if (Environment.TickCount64 - _startedAt < 15_000) return; // ignora inicialização e JIT
            _sensorTicks += ticks;
            _sensorReads++;
        }

        public void Log(string readings, IEnumerable<ScreenPipeline> screens)
        {
            _nextLog = Environment.TickCount64 + 30 * 60_000;
            double sensorsMs = _sensorReads == 0 ? 0 : _sensorTicks * 1000.0 / Stopwatch.Frequency / _sensorReads;
            var parts = screens.Select(s => s.TakeStats());
            using var process = Process.GetCurrentProcess();
            var gc = GC.GetGCMemoryInfo();
            DS339Monitor.Log.Info($"Leituras: {readings}");
            DS339Monitor.Log.Info($"Desempenho: sensores {sensorsMs:0.00} ms por leitura | {string.Join(" | ", parts)} | " +
                $"memória privada {process.PrivateMemorySize64 / 1048576.0:0.0} MB, heap .NET {gc.HeapSizeBytes / 1048576.0:0.0} MB, " +
                $"GCs {GC.CollectionCount(0)}/{GC.CollectionCount(1)}/{GC.CollectionCount(2)}");
            _sensorTicks = 0;
            _sensorReads = 0;
        }
    }
}
