using System.Drawing;
using DS339Monitor.App;
using DS339Monitor.Lighting;
using DS339Monitor.Themes;

namespace DS339Monitor.Manga;

/// <summary>O que o Modo Mangá encena.</summary>
internal enum MangaCueKind
{
    // Momentos dos painéis da Pro Flow (campo "luz" do quadros.json), nas duas telas ao mesmo tempo.
    Impact,
    Haki,
    Drums,
    Quake,
    Tide,
    Farewell,
    Dawn,
    // Viradas de painel da Pro Flow, que atravessam também a Thermalright.
    TurnCut,
    TurnHalftone,
    // Ações do PC.
    Print,
    Download,
    Copy,
    Drive,
    Ring,
    Track,
    Mute,
    Welcome,
    Chapter,
    Menace,
    Gear5,
}

/// <summary>Um acontecimento datado: começa em <see cref="At"/> (<see cref="Clock.Ms"/>) e dura <see cref="Seconds"/>.</summary>
internal sealed class MangaCue
{
    public required MangaCueKind Kind { get; init; }
    public required long At { get; init; }
    public required double Seconds { get; init; }
    public string? Text { get; init; }
    public string? Detail { get; init; }
    public int Seed { get; init; }
    /// <summary>Disparado pelo menu de teste (sem o painel correspondente na Pro Flow): o LED encena sozinho.</summary>
    public bool Manual { get; init; }
    /// <summary>Quadro pronto do print (convertido em retícula em segundo plano); null enquanto carrega ou se não houver.</summary>
    public volatile Bitmap? Picture;

    public long End => At + (long)(Seconds * 1000);
    public double Age(long now) => (now - At) / 1000.0;
    public bool Alive(long now) => now >= At && now <= End;
}

/// <summary>Retrato do Modo Mangá num instante (imutável: as threads só trocam a referência).</summary>
internal sealed record MangaState(IReadOnlyList<MangaCue> Cues, bool OnAir, long OnAirSince, bool Sleeping, long SleepingSince, float GpuTemp)
{
    public static readonly MangaState Empty = new(Array.Empty<MangaCue>(), false, 0, false, 0, float.NaN);

    public MangaCue? Find(MangaCueKind kind, long now) => Cues.FirstOrDefault(c => c.Kind == kind && c.Alive(now));
}

/// <summary>
/// Diretor do Modo Mangá: transforma o que acontece no PC e no mangá da Pro Flow em acontecimentos datados que as duas
/// telas (<see cref="MangaLayer"/>) e o LED da placa (<see cref="ShapeLed"/>) encenam juntos, por cima da arte que já
/// está nelas (o vídeo, o GIF, os painéis). Roda na thread de animação, depois do <see cref="SceneDirector"/>.
/// </summary>
internal sealed class MangaStage
{
    /// <summary>Tempos do despertar do Gear 5 (jogo abrindo), em segundos desde o começo.</summary>
    public static class Gear5Times
    {
        public static readonly double[] Heartbeats = { 0.0, 0.8 };
        public const double Awaken = 1.6;       // quadro de impacto: a Thermalright vira o GIF e a Pro Flow pula para o Gear 5
        public const double WashOut = 11.4;     // véu branco de volta ao vídeo...
        public const double Back = 11.9;        // ...troca no meio do branco
        public const double Seconds = 12.5;
    }

    private const long EventMemoryMs = 60_000;
    private const float HotGpu = 80;            // °C por 10 s: a ameaça ゴゴゴゴ
    private const long HotHoldMs = 10_000;

    private static volatile MangaState _current = MangaState.Empty;

    /// <summary>Ligado pelo <see cref="MonitorService"/> enquanto o Modo Mangá está ativo.</summary>
    public static volatile bool InUse;

    public static MangaState Current => _current;

    private readonly List<MangaCue> _cues = new();
    private readonly HashSet<(StageEventKind, long)> _seenEvents = new();
    private readonly HashSet<SystemSignal> _seenSignals = new();
    private readonly Dictionary<string, long> _cooldowns = new();
    private readonly Random _random = new();
    private int _panel = -1;
    private bool _turning;
    private bool _wasInGame, _wasOnAir, _wasSleeping;
    private long _onAirSince, _sleepingSince;
    private long _hotSince;
    private int _lastHour = -1;
    private long _headlineFree;                 // a Thermalright mostra uma manchete por vez
    private long _seekAt = -1;

    public MangaSettings Settings { get; set; } = new();

    /// <summary>Hora de pular a Pro Flow para o painel do Gear 5, se chegou (consumido pelo <see cref="MonitorService"/>).</summary>
    public bool TakeSeek(long now)
    {
        if (_seekAt < 0 || now < _seekAt) return false;
        _seekAt = -1;
        return true;
    }

    public void Update(StageState stage, long now)
    {
        Panels(stage, now);

        // Estados do PC.
        if (stage.InGame && !_wasInGame && Cooled("gear5", 300, now)) Gear5(now, manual: false);
        if (stage.OnAir && !_wasOnAir)
        {
            _onAirSince = now;
            if (Settings.AcoesDoPc && Cooled("ring", 120, now)) Headline(MangaCueKind.Ring, now, 3.0, "DEN DEN MUSHI", "Microfone ou câmera ligados");
        }
        if (stage.Sleeping && !_wasSleeping) _sleepingSince = now;
        _wasInGame = stage.InGame;
        _wasOnAir = stage.OnAir;
        _wasSleeping = stage.Sleeping;

        // Acontecimentos do diretor de cena.
        foreach (var e in stage.Events)
        {
            if (e.At > now || !_seenEvents.Add((e.Kind, e.At))) continue;
            switch (e.Kind)
            {
                case StageEventKind.Welcome when Cooled("welcome", 30, now):
                    Add(MangaCueKind.Welcome, now, 3.2);
                    break;
                case StageEventKind.DenDenMushi when Settings.AcoesDoPc && Cooled("download", 60, now):
                    Headline(MangaCueKind.Download, now, 4.8, "EDIÇÃO EXTRA!", e.Text ?? "Download concluído");
                    break;
                case StageEventKind.BusterCall when Cooled("menace", 900, now):
                    Menace(now, stage.GpuTemp);
                    break;
            }
        }
        _seenEvents.RemoveWhere(e => now - e.Item2 > EventMemoryMs);

        // Ações do sistema.
        foreach (var signal in stage.Signals)
        {
            if (!_seenSignals.Add(signal) || !Settings.AcoesDoPc) continue;
            switch (signal.Kind)
            {
                case SignalKind.Screenshot when Cooled("print", 8, now):
                    Print(now, manual: false);
                    break;
                case SignalKind.Download when Cooled("download", 60, now):
                    string size = signal.Value >= 1024 ? $"{signal.Value / 1024:0.0} GB" : $"{Math.Max(1, signal.Value):0} MB";
                    Headline(MangaCueKind.Download, now, 4.8, "EDIÇÃO EXTRA!", $"{Shorten(signal.Text, 40)} baixado ({size})");
                    break;
                case SignalKind.Copy when Cooled("copy", 12, now):
                    Add(MangaCueKind.Copy, now, 1.9);
                    break;
                case SignalKind.DriveArrived when Cooled("drive", 20, now):
                    Headline(MangaCueKind.Drive, now, 3.4, "PENDRIVE CONECTADO!", signal.Text);
                    break;
                case SignalKind.Track when Cooled("track", 30, now):
                    Headline(MangaCueKind.Track, now, 5.0, "TOCANDO AGORA", Shorten(signal.Text, 48));
                    break;
                case SignalKind.Muted when Cooled("mute", 10, now):
                    Add(MangaCueKind.Mute, now, 2.8);
                    break;
            }
        }
        _seenSignals.RemoveWhere(s => now - s.At > EventMemoryMs);

        // GPU quente por um tempo: a ameaça.
        if (stage.GpuTemp >= HotGpu)
        {
            if (_hotSince == 0) _hotSince = now;
            if (now - _hotSince >= HotHoldMs && Cooled("menace", 900, now)) Menace(now, stage.GpuTemp);
        }
        else _hotSince = 0;

        // Hora cheia: página de título do capítulo (de dia, sem jogo, acordado).
        var clock = DateTime.Now;
        if (clock.Minute == 0 && clock.Hour != _lastHour)
        {
            _lastHour = clock.Hour;
            if (Settings.CapitulosNaHora && clock.Hour is >= 8 and < 23 && !stage.InGame && !stage.Sleeping) Chapter(now, clock);
        }
        else if (clock.Minute != 0) _lastHour = -1;

        _cues.RemoveAll(c => now > c.End + 500);
        _current = new MangaState(_cues.ToArray(), stage.OnAir, _onAirSince, stage.Sleeping, _sleepingSince, stage.GpuTemp);
    }

    /// <summary>Teste pelo menu (ou <c>--historia</c>): encena já.</summary>
    public void Trigger(string name, long now)
    {
        switch (name.Trim().ToLowerInvariant())
        {
            case "impacto" or "impact": Add(MangaCueKind.Impact, now, 1.8, manual: true); break;
            case "haki": Add(MangaCueKind.Haki, now, 6, manual: true); break;
            case "tambores" or "drums": Add(MangaCueKind.Drums, now, 7, manual: true); break;
            case "tremor" or "quake": Add(MangaCueKind.Quake, now, Choreo.QuakeEnd + 0.6, manual: true); break;
            case "mare" or "maré" or "tide": Add(MangaCueKind.Tide, now, 8, manual: true); break;
            case "despedida" or "farewell": Add(MangaCueKind.Farewell, now, 8, manual: true); break;
            case "amanhecer" or "dawn": Add(MangaCueKind.Dawn, now, 8, manual: true); break;
            case "virada" or "turn": Add(MangaCueKind.TurnCut, now, 0.6, manual: true); break;
            case "reticula" or "retícula" or "halftone": Add(MangaCueKind.TurnHalftone, now, 0.6, manual: true); break;
            case "print": Print(now, manual: true); break;
            case "download" or "news": Headline(MangaCueKind.Download, now, 4.8, "EDIÇÃO EXTRA!", "one-piece-1130.zip baixado (230 MB)"); break;
            case "copiar" or "copy" or "bubble": Add(MangaCueKind.Copy, now, 1.9); break;
            case "pendrive" or "drive" or "chest": Headline(MangaCueKind.Drive, now, 3.4, "PENDRIVE CONECTADO!", "E: SUNNY"); break;
            case "microfone" or "ring" or "denden": _onAirSince = now; Headline(MangaCueKind.Ring, now, 3.0, "DEN DEN MUSHI", "Microfone ou câmera ligados"); break;
            case "musica" or "música" or "track" or "brook": Headline(MangaCueKind.Track, now, 5.0, "TOCANDO AGORA", "Brook — Binks no Sake"); break;
            case "mudo" or "mute": Add(MangaCueKind.Mute, now, 2.8); break;
            case "bemvindo" or "welcome" or "weigh": Add(MangaCueKind.Welcome, now, 3.2); break;
            case "capitulo" or "capítulo" or "chapter" or "bell": Chapter(now, DateTime.Now); break;
            case "ameaca" or "ameaça" or "menace" or "eruption": Menace(now, float.IsNaN(SceneDirector.Current.GpuTemp) ? 84 : SceneDirector.Current.GpuTemp); break;
            case "gear5" or "knockup": Gear5(now, manual: true); break;
            default: Log.Warn($"Modo Mangá: não conheço '{name}'."); return;
        }
        _current = _current with { Cues = _cues.ToArray() };
    }

    // ---------------------------------------------------------------- painéis da Pro Flow

    /// <summary>
    /// O começo de cada fase da apresentação (virada ou movimento de um painel) vira um acontecimento datado do instante
    /// exato em que começou — se o modo ligou no meio de uma fase, não há acontecimento (seria um efeito fora de hora).
    /// </summary>
    private void Panels(StageState stage, long now)
    {
        if (stage.Panel is not PanelScene scene) return;
        if (scene.Panel == _panel && scene.Turning == _turning) return;
        bool joinedMidway = _panel < 0 || scene.Seconds > 0.5;
        _panel = scene.Panel;
        _turning = scene.Turning;
        if (joinedMidway) return;

        long start = now - (long)(scene.Seconds * 1000);
        if (scene.Turning)
        {
            Add(scene.Turn == PanelTurn.Cut ? MangaCueKind.TurnCut : MangaCueKind.TurnHalftone, start, scene.Duration);
            return;
        }
        switch (scene.Cue)
        {
            case PanelCue.Impact: Add(MangaCueKind.Impact, start, 1.8); break;
            case PanelCue.Haki: Add(MangaCueKind.Haki, start, scene.Duration); break;
            case PanelCue.Drums: Add(MangaCueKind.Drums, start, scene.Duration); break;
            case PanelCue.Quake: Add(MangaCueKind.Quake, start, Math.Min(scene.Duration, Choreo.QuakeEnd + 0.6)); break;
            case PanelCue.Tide: Add(MangaCueKind.Tide, start, scene.Duration); break;
            case PanelCue.Farewell: Add(MangaCueKind.Farewell, start, scene.Duration); break;
            case PanelCue.Dawn: Add(MangaCueKind.Dawn, start, scene.Duration); break;
        }
    }

    // ---------------------------------------------------------------- histórias

    private void Gear5(long now, bool manual)
    {
        _cues.RemoveAll(c => c.Kind is MangaCueKind.Gear5);
        Add(MangaCueKind.Gear5, now, Gear5Times.Seconds, manual: manual);
        _seekAt = now + (long)(Gear5Times.Awaken * 1000);
        _headlineFree = Math.Max(_headlineFree, now + (long)(Gear5Times.Seconds * 1000));
    }

    private void Print(long now, bool manual)
    {
        var cue = Headline(MangaCueKind.Print, now, 4.6, "PRINT SALVO!", null);
        string? path = manual ? AnyScreenshot() : null;
        Task.Run(async () =>
        {
            try
            {
                // O arquivo do print aparece logo depois do aviso; espera ele ser escrito.
                for (int attempt = 0; attempt < 6 && path is null; attempt++)
                {
                    await Task.Delay(250);
                    path = RecentScreenshot();
                }
                if (path is null) return;
                await Task.Delay(150);
                using var photo = Image.FromFile(path);
                cue.Picture = MangaFx.RenderPrintPanel(photo, 620, 360);
            }
            catch (Exception ex) when (ex is IOException or OutOfMemoryException or ArgumentException or UnauthorizedAccessException)
            {
                Log.Warn($"Modo Mangá: não consegui abrir o print ({ex.Message}).");
            }
        });
    }

    private void Menace(long now, float gpuTemp) =>
        Headline(MangaCueKind.Menace, now, 7.0, "A GPU ESTÁ ESQUENTANDO…", float.IsNaN(gpuTemp) ? null : $"{gpuTemp:0} °C");

    private static readonly string[] ChapterTitles =
    {
        "O homem que será o Rei dos Piratas", "Romance Dawn", "Rumo à Grand Line", "Os sonhos nunca acabam",
        "Zarpar!", "Um novo amanhecer", "Os Tambores da Libertação", "A vontade herdada", "Ao Novo Mundo",
        "A promessa do chapéu de palha", "Uma aventura de verdade", "O mar está chamando",
    };

    private void Chapter(long now, DateTime clock)
    {
        int day = clock.DayOfYear;
        string title = ChapterTitles[(day * 24 + clock.Hour) % ChapterTitles.Length];
        Headline(MangaCueKind.Chapter, now, 6.0, title, clock.ToString("HH:mm"), chapter: clock.Hour == 0 ? 24 : clock.Hour);
    }

    /// <summary>Manchete da Thermalright: se outra está na tela, espera ela acabar.</summary>
    private MangaCue Headline(MangaCueKind kind, long now, double seconds, string? text, string? detail, int chapter = 0)
    {
        long at = Math.Max(now, _headlineFree);
        if (at - now > 20_000) at = now; // fila longa demais: entra por cima
        var cue = Add(kind, at, seconds, text, detail, seed: chapter);
        _headlineFree = cue.End + 300;
        return cue;
    }

    private MangaCue Add(MangaCueKind kind, long at, double seconds, string? text = null, string? detail = null, bool manual = false, int seed = 0)
    {
        var cue = new MangaCue
        {
            Kind = kind,
            At = at,
            Seconds = seconds,
            Text = text,
            Detail = detail,
            Manual = manual,
            Seed = seed != 0 ? seed : _random.Next(1, int.MaxValue),
        };
        _cues.Add(cue);
        return cue;
    }

    private bool Cooled(string key, int seconds, long now)
    {
        if (_cooldowns.TryGetValue(key, out long last) && now - last < seconds * 1000L) return false;
        _cooldowns[key] = now;
        return true;
    }

    private static string? Shorten(string? text, int max) =>
        string.IsNullOrEmpty(text) ? text : text.Length <= max ? text : text[..(max - 1)] + "…";

    private static string ScreenshotsFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Screenshots");

    /// <summary>O print mais novo da pasta de capturas, se foi salvo nos últimos 20 s.</summary>
    private static string? RecentScreenshot()
    {
        try
        {
            if (!Directory.Exists(ScreenshotsFolder)) return null;
            var newest = new DirectoryInfo(ScreenshotsFolder).EnumerateFiles("*.png").OrderByDescending(f => f.LastWriteTimeUtc).FirstOrDefault();
            return newest is not null && DateTime.UtcNow - newest.LastWriteTimeUtc < TimeSpan.FromSeconds(20) ? newest.FullName : null;
        }
        catch (IOException)
        {
            return null;
        }
    }

    /// <summary>Para o teste: o print mais novo, de quando for.</summary>
    private static string? AnyScreenshot()
    {
        try
        {
            if (!Directory.Exists(ScreenshotsFolder)) return null;
            return new DirectoryInfo(ScreenshotsFolder).EnumerateFiles("*.png").OrderByDescending(f => f.LastWriteTimeUtc).FirstOrDefault()?.FullName;
        }
        catch (IOException)
        {
            return null;
        }
    }

    // ---------------------------------------------------------------- LED da placa

    /// <summary>
    /// A luz da placa no Modo Mangá: parte do nível do "Mangá narrado" (que já acompanha a Pro Flow e os momentos dos
    /// painéis) e soma o que as ações do PC encenam — o flash do print, o DON da edição extra, o coração e os tambores do
    /// Gear 5, o silêncio do mudo, a ameaça. Dormindo, respira devagar. A trava de clarões vem depois, no motor.
    /// </summary>
    public static double ShapeLed(double level, long now)
    {
        if (!InUse) return level;
        var state = _current;
        double add = 0;
        foreach (var cue in state.Cues)
        {
            if (!cue.Alive(now)) continue;
            double x = cue.Age(now), d = cue.Seconds;
            double env = Choreo.Smooth(0, 0.4, x) * Choreo.Smooth(0, 0.5, d - x);
            switch (cue.Kind)
            {
                case MangaCueKind.Print:
                    add += Choreo.Pulse(x, 0.7, 0.02, 0.14);
                    break;
                case MangaCueKind.Download:
                case MangaCueKind.Drive:
                    add += Choreo.Pulse(x - 0.15, cue.Kind == MangaCueKind.Download ? 0.5 : 0.35, 0.05, 0.3);
                    break;
                case MangaCueKind.Copy:
                    add += Choreo.Pulse(x - 0.05, 0.15, 0.04, 0.15);
                    break;
                case MangaCueKind.Ring:
                    add += 0.3 * Choreo.DenDenRings(x) + Choreo.Pulse(x - 2.5, 0.3, 0.04, 0.2);
                    break;
                case MangaCueKind.Track:
                    add += 0.12 * env * (0.5 + 0.5 * Math.Sin(2 * Math.PI * x / 1.6));
                    break;
                case MangaCueKind.Welcome:
                    add += 0.35 * Math.Sin(Math.PI * Math.Clamp(x / Choreo.WelcomeSweep, 0, 1));
                    break;
                case MangaCueKind.Chapter:
                    add += Choreo.Pulse(x - 0.3, 0.4, 0.05, 0.35);
                    break;
                case MangaCueKind.Mute:
                    level = Choreo.Lerp(level, 0.18, Choreo.Smooth(0, 0.3, x) * Choreo.Smooth(0, 0.6, d - x));
                    break;
                case MangaCueKind.Menace:
                    level = Choreo.Lerp(level, 0.22 + 0.16 * (0.5 - 0.5 * Math.Cos(2 * Math.PI * x / 1.8)), env);
                    break;
                case MangaCueKind.Gear5:
                    foreach (double beat in Gear5Times.Heartbeats) add += Choreo.Pulse(x - beat, 0.45, 0.04, 0.16);
                    add += Choreo.Pulse(x - Gear5Times.Awaken, 0.7, 0.03, 0.3);
                    if (x > Gear5Times.Awaken && x < Gear5Times.Back)
                        level = Choreo.Lerp(level, MangaLight.Drums(x - Gear5Times.Awaken, Choreo.DrumCycle),
                            Choreo.Smooth(Gear5Times.Awaken, Gear5Times.Awaken + 0.4, x) * Choreo.Smooth(0, 0.5, Gear5Times.Back - x));
                    break;
                // Momentos dos painéis disparados à mão (sem o painel na Pro Flow, o "Mangá narrado" não os vê).
                case MangaCueKind.Impact when cue.Manual:
                    foreach (var (at, peak) in Choreo.Impact) add += Choreo.Pulse(x - at, peak, 0.05, 0.4);
                    break;
                case MangaCueKind.Drums when cue.Manual:
                    level = Choreo.Lerp(level, MangaLight.Drums(x, Choreo.DrumCycle), env);
                    break;
                case MangaCueKind.Haki when cue.Manual:
                    level = Choreo.Lerp(level, 0.28, env);
                    foreach (var (at, peak, _) in Choreo.HakiCrackles(x)) add += Choreo.Pulse(x - at, peak, 0.02, 0.06);
                    break;
                case MangaCueKind.Quake when cue.Manual:
                    level *= 1 - 0.32 * Choreo.Quake(x) * Choreo.SlowNoise(x, 3);
                    break;
            }
        }
        if (state.Sleeping)
        {
            double asleep = Choreo.Smooth(0, 3, (now - state.SleepingSince) / 1000.0);
            level = Choreo.Lerp(level, 0.22 + 0.1 * (0.5 - 0.5 * Math.Cos(2 * Math.PI * now / 1000.0 / 6)), asleep);
        }
        return Math.Clamp(level + add, 0, 1);
    }
}
