using System.Drawing;
using System.Drawing.Drawing2D;
using DS339Monitor.App;
using DS339Monitor.Lighting;
using DS339Monitor.Themes;
using static DS339Monitor.Lighting.Choreo;

namespace DS339Monitor.World;

/// <summary>
/// Diretor do mundo "Coluna do Mar": transforma o que acontece no PC em histórias que atravessam o gabinete de cima
/// para baixo — o pesado cai da Thermalright (céu e superfície), passa pela Pro Flow (debaixo d'água) e chega à luz da
/// placa de vídeo (as profundezas); o leve sobe da placa até a superfície. Cada história é um punhado de atores com a
/// trajetória em milímetros em função do tempo: as duas telas e a luz perguntam a mesma conta. Roda na thread de
/// animação, logo depois do <see cref="SceneDirector"/>.
/// </summary>
internal sealed class WorldDirector
{
    private const double BigGapSeconds = 4;      // folga entre duas histórias grandes
    private const long QueueMaxAgeMs = 90_000;   // história que esperou demais é esquecida
    private const float BowMm = 14;              // da linha d'água do meio do Sunny até a proa (onde fica a âncora)

    private static volatile WorldState _current = WorldState.Empty;
    public static WorldState Current => _current;

    private readonly List<WorldActor> _actors = new();
    private readonly Queue<(long Since, string Key, Func<long, double> Run)> _queue = new();
    private readonly Dictionary<string, long> _lastRun = new();
    private readonly HashSet<(StageEventKind, long)> _seenEvents = new();
    private readonly HashSet<SystemSignal> _seenSignals = new();
    private readonly List<(string Label, long Until)> _chests = new();
    private readonly Random _random = new();
    private WorldLayout _layout = new();
    private ShipMode _ship = ShipMode.Sailing;
    private long _shipSince;
    private long _bigUntil;
    private bool _wasSleeping, _wasInGame, _wasOnAir;
    private long _onAirSince, _lastWeigh, _lastNews;
    private long _nextGull, _nextStar;
    private int _lastBellHour = -1;
    private DateOnly _laboonNight;
    private WorldActor? _anchor, _gauge;

    public void Configure(WorldLayout layout) => _layout = layout;

    /// <summary>Disparo manual (menu "Testar cena").</summary>
    public void Trigger(string story, long now)
    {
        var stage = SceneDirector.Current;
        switch (story)
        {
            case "knockup": KnockUp(now); break;
            case "return": Return(now); break;
            case "anchor": AnchorDrop(now); break;
            case "weigh": Weigh(now); break;
            case "news": NewsCoo(now, "Download concluído — teste"); break;
            case "poster": Poster(now, null); break;
            case "eruption": Eruption(now, "GPU 88°C (teste)"); break;
            case "bubble": Bubble(now, full: true); break;
            case "brook": Brook(now, "Binks no Sake — Brook"); break;
            case "chest": Chest(now, "E: PENDRIVE"); break;
            case "denden": DenDenRing(now); break;
            case "laboon": Laboon(now); break;
            case "gull": GullDive(now); break;
            case "star": ShootingStar(now); break;
            case "lightning": LightningSink(now); break;
            case "bell": Bell(now); break;
            case "mute": Shh(now); break;
        }
        Publish(now, stage);
    }

    public void Update(StageState stage)
    {
        long now = Clock.Ms;
        var clock = DateTime.Now;
        bool quiet = clock.Hour >= 23 || clock.Hour < 7;

        // Estados do PC que mudam o navio.
        if (stage.InGame && !_wasInGame)
        {
            if (Cooled("knockup", 600, now)) KnockUp(now);
            else SetShip(ShipMode.Away, now);
        }
        else if (!stage.InGame && _wasInGame && _ship is ShipMode.Launch or ShipMode.Away)
        {
            Return(now);
        }
        if (_ship == ShipMode.Launch && now - _shipSince > 3400) SetShip(ShipMode.Away, now);
        if (_ship == ShipMode.Return && now - _shipSince > 6500) SetShip(ShipMode.Sailing, now);
        // Dormindo e navegando (inclusive depois de voltar do céu): lança a âncora. Acordou: recolhe.
        if (stage.Sleeping && _ship == ShipMode.Sailing && now - _shipSince > 3000) AnchorDrop(now);
        else if (!stage.Sleeping && _ship == ShipMode.Anchored) Weigh(now);
        if (stage.OnAir && !_wasOnAir)
        {
            _onAirSince = now;
            if (Cooled("denden", 600, now)) DenDenRing(now);
        }
        _wasInGame = stage.InGame;
        _wasSleeping = stage.Sleeping;
        _wasOnAir = stage.OnAir;

        // Acontecimentos do diretor de cena.
        foreach (var e in stage.Events)
        {
            if (e.At > now || !_seenEvents.Add((e.Kind, e.At))) continue;
            switch (e.Kind)
            {
                case StageEventKind.Lightning:
                    if (!quiet) LightningSink(e.At);
                    break;
                case StageEventKind.DenDenMushi:
                    Big("news", 300, now, at => NewsCoo(at, e.Text ?? "Download concluído"));
                    break;
                case StageEventKind.BusterCall:
                    Big("eruption", 600, now, at => Eruption(at, e.Text ?? "GPU quente"), urgent: true);
                    break;
                case StageEventKind.Welcome when now - _lastWeigh > 5000 && _ship != ShipMode.Anchored:
                    Sweep(now);
                    break;
            }
        }
        if (_seenEvents.Count > 200) _seenEvents.RemoveWhere(e => now - e.Item2 > 60_000);

        // Ações do sistema.
        foreach (var signal in stage.Signals)
        {
            if (!_seenSignals.Add(signal)) continue;
            switch (signal.Kind)
            {
                case SignalKind.Copy:
                    Bubble(now, full: Cooled("bubble", 120, now), image: signal.Value > 0);
                    break;
                case SignalKind.Screenshot:
                    Big("poster", 60, now, at => Poster(at, RecentScreenshot()));
                    break;
                case SignalKind.DriveArrived:
                    Big("chest", 60, now, at => Chest(at, signal.Text ?? "Pendrive"));
                    break;
                case SignalKind.Track:
                    Big("brook", 180, now, at => Brook(at, signal.Text ?? "♪"));
                    break;
                case SignalKind.Download when now - _lastNews > 90_000:
                    string size = signal.Value >= 1024 ? $"{signal.Value / 1024:0.0} GB" : $"{Math.Max(1, signal.Value):0} MB";
                    Big("news", 120, now, at => NewsCoo(at, $"{Shorten(signal.Text, 34)} ({size})"));
                    break;
                case SignalKind.Muted:
                    Shh(now);
                    break;
                case SignalKind.Volume:
                    Gauge(now, signal.Value);
                    break;
            }
        }
        if (_seenSignals.Count > 200) _seenSignals.RemoveWhere(s => now - s.At > 60_000);

        // Vida ambiente.
        bool calm = !stage.Sleeping && !stage.InGame && _ship == ShipMode.Sailing;
        if (_nextGull == 0) _nextGull = now + 60_000;
        if (_nextStar == 0) _nextStar = now + 120_000;
        if (now >= _nextGull)
        {
            _nextGull = now + 180_000 + _random.Next(180_000);
            if (calm && !quiet && stage.Daylight > 0.4 && now >= _bigUntil) GullDive(now);
        }
        if (now >= _nextStar)
        {
            _nextStar = now + 900_000 + _random.Next(900_000);
            if (stage.Daylight < 0.2 && !stage.InGame) ShootingStar(now);
        }
        if (clock.Minute == 0 && clock.Hour != _lastBellHour)
        {
            _lastBellHour = clock.Hour;
            if (!quiet && calm) Bell(now);
        }
        var night = DateOnly.FromDateTime(clock.AddHours(-12)); // "a noite" vai do meio-dia ao meio-dia
        if (_ship == ShipMode.Anchored && stage.Sleeping && now - _shipSince > 600_000 && stage.Daylight < 0.2 && _laboonNight != night)
        {
            _laboonNight = night;
            Laboon(now);
        }

        // Fila das histórias grandes.
        while (_queue.Count > 0 && now - _queue.Peek().Since > QueueMaxAgeMs) _queue.Dequeue();
        if (_queue.Count > 0 && now >= _bigUntil && !stage.InGame)
        {
            var (_, key, run) = _queue.Dequeue();
            RunBig(key, now, run);
        }

        _chests.RemoveAll(c => now > c.Until);
        _actors.RemoveAll(a => now > a.Start + (long)(a.Seconds * 1000));
        Publish(now, stage);
    }

    private void Publish(long now, StageState stage)
    {
        var clock = DateTime.Now;
        _current = new WorldState
        {
            Layout = _layout,
            Actors = _actors.ToArray(),
            Ship = _ship,
            ShipSince = _shipSince,
            Quiet = clock.Hour >= 23 || clock.Hour < 7,
            Chests = _chests.Select(c => c.Label).ToArray(),
            OnAir = stage.OnAir,
            OnAirSince = _onAirSince,
        };
    }

    // ---------------------------------------------------------------- regras de ritmo

    private bool Cooled(string key, double seconds, long now)
    {
        if (_lastRun.TryGetValue(key, out long last) && now - last < seconds * 1000) return false;
        _lastRun[key] = now;
        return true;
    }

    /// <summary>História grande: uma por vez (as outras esperam na fila), nunca durante o jogo (salvo urgente).</summary>
    private void Big(string key, double cooldownSeconds, long now, Func<long, double> run, bool urgent = false)
    {
        if (_lastRun.TryGetValue(key, out long last) && now - last < cooldownSeconds * 1000) return;
        _lastRun[key] = now;
        if (urgent || (now >= _bigUntil && !SceneDirector.Current.InGame)) RunBig(key, now, run);
        else if (_queue.Count < 4) _queue.Enqueue((now, key, run));
    }

    private void RunBig(string key, long now, Func<long, double> run)
    {
        double seconds = run(now);
        _bigUntil = now + (long)((seconds + BigGapSeconds) * 1000);
    }

    private void SetShip(ShipMode mode, long now)
    {
        // Saindo do ancoradouro por outro motivo (um jogo abriu): a âncora sobe junto, sem cerimônia.
        if (mode != ShipMode.Anchored && _anchor is not null)
        {
            _actors.Remove(_anchor);
            _anchor = null;
        }
        _ship = mode;
        _shipSince = now;
    }

    private WorldActor Add(WorldActor actor)
    {
        _actors.Add(actor);
        return actor;
    }

    // ---------------------------------------------------------------- trajetórias

    /// <summary>Do y0 ao y1 (mm) pela rota vertical, entre t0 e t1 segundos, com a curva pedida; parado fora disso.</summary>
    private Func<double, float> Vertical(float y0, float y1, double t0, double t1, Func<double, double>? ease = null)
    {
        var layout = _layout;
        float s0 = layout.Travel(y0), s1 = layout.Travel(y1);
        ease ??= u => u;
        return s =>
        {
            double u = Math.Clamp((s - t0) / (t1 - t0), 0, 1);
            return layout.FromTravel((float)(s0 + (s1 - s0) * ease(u)));
        };
    }

    private static double EaseOut(double u) => 1 - (1 - u) * (1 - u);
    private static double EaseIn(double u) => u * u;
    private static double EaseInOut(double u) => u * u * (3 - 2 * u);

    private PointF LedPoint(double _) => _layout.Led;

    private float SharedX(double fraction)
    {
        var (from, to) = _layout.Shared;
        return (float)(from + (to - from) * Math.Clamp(fraction, 0, 1));
    }

    private static RectangleF Screen(ScreenKind kind)
    {
        var (w, h) = ThemeCatalog.Size(kind);
        return new RectangleF(0, 0, w, h);
    }

    /// <summary>Pulso de luz na placa (para as coreografias): sobe em <paramref name="attack"/> s e apaga em <paramref name="decay"/>.</summary>
    private WorldActor LedCue(long at, double seconds, Func<double, double> emission, double warmth, double shadow = 0, Func<double, double>? shadowCurve = null) =>
        Add(new WorldActor
        {
            Name = "luz",
            Start = at,
            Seconds = seconds,
            Position = LedPoint,
            Emission = emission,
            Shadow = shadowCurve ?? (shadow > 0 ? _ => shadow : null),
            Warmth = warmth,
        });

    private WorldActor StampActor(long at, ScreenKind kind, string text, PointF mm, double seconds, float heightMm, float angle = -6) =>
        Add(new WorldActor
        {
            Name = "sfx:" + text,
            Start = at,
            Seconds = seconds + 0.35,
            Position = _ => mm,
            RadiusMm = -1,
            Layer = 1,
            Draw = (g, screen, view) =>
            {
                if (screen != kind || _current.Quiet) return;
                WorldArt.Stamp(g, text, view.Age, view.Px, angle, heightMm * view.PxPerMm.Height, seconds);
            },
        });

    private WorldActor BannerActor(long at, string text, double from, double to, float yFraction = 0.11f) =>
        Add(new WorldActor
        {
            Name = "faixa",
            Start = at,
            Seconds = to,
            Position = _ => new PointF(_layout.Top.X + _layout.Top.Width / 2, _layout.Top.Y + _layout.Top.Height * yFraction),
            RadiusMm = -1,
            Layer = 1,
            Draw = (g, screen, view) =>
            {
                if (screen != ScreenKind.Thermalright || view.Age < from) return;
                double x = view.Age - from, length = to - from;
                float alpha = (float)Math.Min(1, Math.Min(x / 0.35, (length - x) / 0.5));
                WorldArt.Caption(g, text, view.Px, 34, alpha);
            },
        });

    // ---------------------------------------------------------------- histórias

    /// <summary>
    /// Knock Up Stream (um jogo abriu): o fundo esquenta e borbulha, uma coluna branca sobe pela Pro Flow, rompe o mar
    /// embaixo do Sunny e o lança para o céu. A água volta como chuva.
    /// </summary>
    private void KnockUp(long at)
    {
        SetShip(ShipMode.Launch, at);
        float x = WorldState.ShipX(at);
        LedCue(at, 5, s => s < 2 ? 0.22 * Smooth(0, 2, s) * (1 + 0.15 * (SlowNoise(s, 2) * 2 - 1)) : s < 2.2 ? 1.0 : 0.9 * Math.Exp(-(s - 2.2) / 0.6), 0.6);
        Add(new WorldActor
        {
            Name = "fervura",
            Start = at,
            Seconds = 3,
            Position = _ => new PointF(x, _layout.Middle.Y + _layout.Middle.Height / 2),
            RadiusMm = -1,
            Draw = (g, screen, view) =>
            {
                if (screen != ScreenKind.ProFlow) return;
                var r = Screen(screen);
                double s = view.Age;
                float cx = view.Px.X;
                if (s >= 0.6 && s < 2.4)
                {
                    float grow = (float)Smooth(0.6, 2.0, s);
                    for (int i = 0; i < 18; i++)
                    {
                        double life = (s * 1.6 + Hash(i + 3)) % 1;
                        float bx = cx + (float)((Hash(i + 10) - 0.5) * 220 * grow);
                        float by = r.Bottom - (float)(life * r.Height * (0.4 + 0.6 * grow));
                        WorldArt.Bubble(g, new PointF(bx, by), 6 + 14 * grow * (float)Hash(i + 20));
                    }
                }
                if (s >= 2.0 && s < 2.9)
                {
                    float rise = (float)Math.Min(1, (s - 2.0) / 0.3), fade = (float)Math.Clamp((2.9 - s) / 0.5, 0, 1);
                    float w = 150;
                    var column = new RectangleF(cx - w / 2, r.Bottom - r.Height * rise, w, r.Height * rise);
                    using var white = new SolidBrush(Color.FromArgb((int)(240 * fade), 255, 255, 255));
                    using var ink = WorldArt.Ink(5, fade);
                    g.FillRectangle(white, column);
                    g.DrawLine(ink, column.X, column.Y, column.X, column.Bottom);
                    g.DrawLine(ink, column.Right, column.Y, column.Right, column.Bottom);
                    for (int i = 0; i < 9; i++)
                    {
                        float ly = column.Y + (float)(((s * 3 + i / 9.0) % 1) * column.Height);
                        g.DrawLine(ink, column.X + 20, ly, column.X + 20, ly + 40);
                        g.DrawLine(ink, column.Right - 24, ly + 20, column.Right - 24, ly + 60);
                    }
                }
            },
        });
        Add(new WorldActor
        {
            Name = "coluna",
            Start = at,
            Seconds = 7,
            Position = _ => new PointF(x, _layout.SurfaceY),
            RadiusMm = -1,
            Layer = -1, // atrás do navio: a coluna o empurra para cima
            Draw = (g, screen, view) =>
            {
                if (screen != ScreenKind.Thermalright) return;
                double s = view.Age;
                if (s >= 2.3 && s < 4.6)
                {
                    float rise = (float)Math.Min(1, (s - 2.3) / 0.35), fade = (float)Math.Clamp((4.6 - s) / 0.8, 0, 1);
                    float w = 190, bottom = view.Px.Y + 30, top = bottom - (bottom + 40) * rise;
                    using var white = new SolidBrush(Color.FromArgb((int)(245 * fade), 255, 255, 255));
                    using var ink = WorldArt.Ink(6, fade);
                    using var path = new GraphicsPath();
                    path.AddBezier(view.Px.X - w / 2, bottom, view.Px.X - w / 2 - 20, (top + bottom) / 2, view.Px.X - w / 2 + 10, top + 30, view.Px.X - w / 3, top);
                    path.AddLine(view.Px.X - w / 3, top, view.Px.X + w / 3, top);
                    path.AddBezier(view.Px.X + w / 3, top, view.Px.X + w / 2 - 10, top + 30, view.Px.X + w / 2 + 20, (top + bottom) / 2, view.Px.X + w / 2, bottom);
                    path.CloseFigure();
                    g.FillPath(white, path);
                    g.DrawPath(ink, path);
                    for (int i = 0; i < 12; i++)
                    {
                        float ly = top + (float)(((s * 4 + i / 12.0) % 1) * (bottom - top));
                        float lx = view.Px.X + (float)((Hash(i + 40) - 0.5) * w * 0.7);
                        g.DrawLine(ink, lx, ly, lx, ly + 34);
                    }
                }
                if (s >= 4.0 && s < 6.8)
                {
                    float a = (float)Math.Min(1, Math.Min((s - 4.0) / 0.4, (6.8 - s) / 0.8));
                    using var drop = new Pen(Color.FromArgb((int)(200 * a), 40, 40, 50), 2);
                    for (int i = 0; i < 70; i++)
                    {
                        float dx = view.Px.X + (float)((Hash(i + 60) - 0.5) * 900);
                        float dy = (float)(((s - 4.0) * 520 + Hash(i + 61) * 462) % 462);
                        g.DrawLine(drop, dx, dy, dx - 4, dy + 22);
                    }
                }
            },
        });
        StampActor(at + 2300, ScreenKind.Thermalright, "ドドドド", new PointF(x + 40, _layout.Top.Y + 22), 1.6, 15, -8);
        _bigUntil = Math.Max(_bigUntil, at + 7000);
    }

    /// <summary>O jogo fechou: o Sunny volta do céu pendurado no balão-polvo e cai no mar ("ザブン").</summary>
    private void Return(long at)
    {
        SetShip(ShipMode.Return, at);
        float x = WorldState.ShipX(at + 6000);
        StampActor(at + 5900, ScreenKind.Thermalright, "ザブン", new PointF(x + 26, _layout.Top.Y + 30), 1.2, 12, 6);
        LedCue(at + 5900, 2, s => 0.07 * Math.Exp(-s / 0.5), -0.2);
        _bigUntil = Math.Max(_bigUntil, at + 7000);
    }

    /// <summary>PC parado: "ガラガラ", a âncora cai do Sunny, atravessa a Pro Flow e toca a luz do fundo.</summary>
    private void AnchorDrop(long at)
    {
        SetShip(ShipMode.Anchored, at);
        float rest = _layout.Middle.Bottom - 18;
        var y = Vertical(_layout.SurfaceY - 3, rest, 0.2, 3.8, EaseOut);
        float x = Math.Clamp(WorldState.ShipX(at) + BowMm, _layout.Shared.From, _layout.Shared.To);
        _anchor = Add(AnchorActor(at, 1e7, s => new PointF(x, y(s))));
        StampActor(at, ScreenKind.Thermalright, "ガラガラ", new PointF(x + 30, _layout.Top.Y + 20), 1.3, 10, 6);
        LedCue(at, 6, s => s < 3.0 ? 0 : s < 3.8 ? 0.05 * Smooth(3.0, 3.8, s) : 0.05 + 0.14 * Math.Exp(-(s - 3.8) / 0.6), -0.6);
    }

    /// <summary>Voltou: a âncora sobe e uma faixa de luz varre o gabinete de baixo para cima.</summary>
    private void Weigh(long at)
    {
        _lastWeigh = at;
        if (_anchor is not null)
        {
            _actors.Remove(_anchor);
            _anchor = null;
        }
        SetShip(ShipMode.Sailing, at);
        float rest = _layout.Middle.Bottom - 18;
        var y = Vertical(rest, _layout.SurfaceY - 3, 0, 3, EaseInOut);
        float x = Math.Clamp(WorldState.ShipX(at) + BowMm, _layout.Shared.From, _layout.Shared.To);
        Add(AnchorActor(at, 3.2, s => new PointF(x, y(s))));
        Sweep(at + 600);
    }

    private WorldActor AnchorActor(long at, double seconds, Func<double, PointF> position) => new()
    {
        Name = "âncora",
        Start = at,
        Seconds = seconds,
        Position = position,
        RadiusMm = -1, // a corrente aparece mesmo com a âncora fora da tela
        Draw = (g, screen, view) =>
        {
            var pos = view.Px;
            float k = view.PxPerMm.Width;
            var (_, height) = ThemeCatalog.Size(screen);
            if (screen == ScreenKind.Thermalright)
            {
                var bow = _layout.ToPx(screen, new PointF(WorldState.ShipX(Clock.Ms) + BowMm, _layout.SurfaceY - 5));
                WorldArt.Chain(g, bow, new PointF(pos.X, Math.Min(pos.Y, height + 8)), k);
                if (pos.Y < height + 120) WorldArt.Anchor(g, pos, k * 1.4f, (float)(4 * Math.Sin(view.Age * 1.7)));
            }
            else if (pos.Y > -120)
            {
                if (pos.Y > 0) WorldArt.Chain(g, new PointF(pos.X, -4), pos, k);
                WorldArt.Anchor(g, pos, k * 1.4f, (float)(4 * Math.Sin(view.Age * 1.7)));
            }
        },
    };

    /// <summary>Faixa de luz subindo: a placa acende, depois a Pro Flow, depois a Thermalright (e o "bem-vindo").</summary>
    private void Sweep(long at)
    {
        LedCue(at, 2, s => 0.3 * Math.Exp(-s / 0.4), -0.3);
        Add(new WorldActor
        {
            Name = "varredura",
            Start = at,
            Seconds = 2,
            Position = LedPoint,
            RadiusMm = -1,
            Layer = 1,
            Draw = (g, screen, view) =>
            {
                var r = Screen(screen);
                double from = screen == ScreenKind.ProFlow ? 0.25 : 0.75, length = screen == ScreenKind.ProFlow ? 0.55 : 0.8;
                double u = (view.Age - from) / length;
                if (u < 0 || u > 1) return;
                float center = r.Bottom + 140 - (r.Height + 280) * (float)u;
                using var band = new LinearGradientBrush(new RectangleF(0, center - 140, r.Width, 280), Color.Transparent, Color.Transparent, LinearGradientMode.Vertical)
                {
                    InterpolationColors = new ColorBlend
                    {
                        Colors = new[] { Color.FromArgb(0, 255, 255, 255), Color.FromArgb(210, 255, 255, 255), Color.FromArgb(0, 255, 255, 255) },
                        Positions = new[] { 0f, 0.5f, 1f },
                    },
                };
                g.FillRectangle(band, 0, center - 140, r.Width, 280);
            },
        });
        BannerActor(at, "Bem-vindo de volta, Capitão!", 0.9, 5);
    }

    /// <summary>O raio da tempestade afunda: a luz entra pela superfície da Pro Flow e chega ao fundo.</summary>
    private void LightningSink(long at)
    {
        LedCue(at, 3, s => s < 0.16 ? 0 : Pulse(s - 0.16, 0.45, 0.06, 0.5), -0.8);
        Add(new WorldActor
        {
            Name = "raio-fundo",
            Start = at,
            Seconds = 1.2,
            Position = LedPoint,
            RadiusMm = -1,
            Draw = (g, screen, view) =>
            {
                if (screen != ScreenKind.ProFlow) return;
                double x = view.Age - 0.08;
                if (x < 0) return;
                float a = (float)Math.Exp(-x / 0.25);
                WorldArt.LightShafts(g, Screen(screen), 0, 40, a * 3);
                using var white = new SolidBrush(Color.FromArgb((int)(110 * a), 255, 255, 255));
                g.FillRectangle(white, Screen(screen));
            },
        });
    }

    /// <summary>
    /// Download concluído: a News Coo desce do céu, entrega a "EDIÇÃO EXTRA" no convés, a moeda cai pela borda, afunda
    /// girando pela Pro Flow e tilinta na luz do fundo ("チャリン").
    /// </summary>
    private double NewsCoo(long at, string text)
    {
        _lastNews = at;
        float shipX = WorldState.ShipX(at);
        var start = new PointF(_layout.Top.Right + 18, _layout.Top.Y - 8);
        var hover = new PointF(shipX + 4, _layout.Top.Y + 18);
        var away = new PointF(_layout.Top.X - 24, _layout.Top.Y - 14);
        Add(new WorldActor
        {
            Name = "news-coo",
            Start = at,
            Seconds = 7.2,
            RadiusMm = 12,
            Layer = 1,
            Position = s => s < 3 ? LerpPoint(start, hover, EaseOut(s / 3)) : s < 4.5 ? new PointF(hover.X, hover.Y + (float)(1.2 * Math.Sin(s * 4))) : LerpPoint(hover, away, EaseIn((s - 4.5) / 2.7)),
            Draw = (g, screen, view) =>
            {
                if (screen != ScreenKind.Thermalright) return;
                WorldArt.DrawSprite(g, WorldArt.NewsCooFrame(view.Age * 2.2 % 1 < 0.5), view.Px, 0.8f, 0, 1, flip: true);
            },
        });
        BannerActor(at, "EDIÇÃO EXTRA! " + text, 3.4, 8.8);
        float coinX = Math.Clamp(shipX + 9, _layout.Shared.From, _layout.Shared.To);
        float rest = _layout.Middle.Bottom - 7;
        var y = Vertical(_layout.Top.Y + 22, rest, 4.4, 6.6, u => 0.5 * u + 0.5 * u * u);
        Add(new WorldActor
        {
            Name = "moeda",
            Start = at,
            Seconds = 9.6,
            RadiusMm = 4,
            Position = s => new PointF(coinX + (float)(1.5 * Math.Sin(s * 3)), y(s)),
            Opacity = s => s < 4.4 ? 0 : (float)Math.Clamp((9.6 - s) / 0.5, 0, 1),
            Draw = (g, screen, view) =>
            {
                if (view.Opacity <= 0) return;
                double spin = view.Age < 6.6 ? view.Age * 9 : 0;
                WorldArt.Coin(g, view.Px, view.PxPerMm.Width, spin, view.Opacity);
            },
        });
        StampActor(at + 6600, ScreenKind.ProFlow, "チャリン", new PointF(coinX + 18, _layout.Middle.Y + 30), 1.1, 9, 8);
        LedCue(at, 9, s => s < 6.1 ? 0 : s < 6.6 ? 0.05 * Smooth(6.1, 6.6, s) : 0.05 * Math.Exp(-(s - 6.6) / 1.2) + Pulse(s - 6.6, 0.18, 0.05, 0.3), 0.35);
        return 9.6;
    }

    /// <summary>Print de tela: "カシャ!", o cartaz de procurado cai no mar, afunda pela Pro Flow e pousa na luz do fundo.</summary>
    private double Poster(long at, string? photoPath)
    {
        Image? photo = null;
        if (photoPath is not null)
        {
            Task.Run(() =>
            {
                try
                {
                    using var full = Image.FromFile(photoPath);
                    float scale = Math.Min(1, 360f / full.Width);
                    var thumb = new Bitmap(full, Math.Max(1, (int)(full.Width * scale)), Math.Max(1, (int)(full.Height * scale)));
                    Volatile.Write(ref photo, thumb);
                }
                catch (Exception ex) when (ex is IOException or OutOfMemoryException or ArgumentException)
                {
                }
            });
        }
        Add(new WorldActor
        {
            Name = "flash",
            Start = at,
            Seconds = 0.5,
            Position = _ => PointF.Empty,
            RadiusMm = -1,
            Layer = 1,
            Draw = (g, screen, view) =>
            {
                if (screen != ScreenKind.Thermalright) return;
                using var white = new SolidBrush(Color.FromArgb((int)(200 * Math.Exp(-view.Age / 0.12)), 255, 255, 255));
                g.FillRectangle(white, Screen(screen));
            },
        });
        StampActor(at, ScreenKind.Thermalright, "カシャ", new PointF(_layout.Top.Right - 40, _layout.Top.Y + 18), 1.0, 11, -8);
        float x = SharedX(0.62);
        float rest = _layout.Middle.Y + 34;
        var y = Vertical(_layout.Top.Y - 14, rest, 0.3, 4.6, u => u < 0.4 ? u * 1.4 : 0.56 + (u - 0.4) * 0.733);
        Add(new WorldActor
        {
            Name = "cartaz",
            Start = at,
            Seconds = 10,
            RadiusMm = 14,
            Position = s => new PointF(x + (s < 4.6 ? (float)(5 * Math.Sin(s * 2.4)) : 0), y(s)),
            Angle = s => s < 4.6 ? (float)(15 * Math.Sin(s * 2.4 + 0.6)) : (float)Lerp(15 * Math.Sin(4.6 * 2.4 + 0.6), 4, Smooth(4.6, 5.2, s)),
            Opacity = s => (float)Math.Clamp((10 - s) / 0.6, 0, 1),
            Draw = (g, screen, view) =>
            {
                if (view.Age < 0.3) return;
                WorldArt.Poster(g, view.Px, view.PxPerMm.Width * (screen == ScreenKind.ProFlow ? 1.25f : 1), view.Angle, Volatile.Read(ref photo), "฿ 1.920.000.000", view.Opacity);
            },
        });
        LedCue(at, 10, s => s < 4.2 ? 0 : s < 4.6 ? 0.18 * Smooth(4.2, 4.6, s) : s < 6.6 ? 0.18 : 0.18 * (1 - Smooth(6.6, 7.6, s)), -0.3);
        return 10;
    }

    /// <summary>
    /// GPU quente demais: erupção submarina. O fundo acende em brasa e arrota lava, bolhas de lava sobem pela Pro Flow e
    /// o mar ferve em volta do Sunny ("ボコボコ").
    /// </summary>
    private double Eruption(long at, string text)
    {
        const double Seconds = 10;
        var shots = BusterShots().ToArray();
        LedCue(at, Seconds, s =>
        {
            double env = Smooth(0, 0.6, s) * Smooth(0, 0.8, Seconds - s);
            double e = 0.18 + 0.15 * (0.5 - 0.5 * Math.Cos(Math.PI * s));
            foreach (double shot in shots) e += Pulse(s - shot, 0.25, 0.05, 0.6);
            return env * e;
        }, 0.9);
        Add(new WorldActor
        {
            Name = "lava",
            Start = at,
            Seconds = Seconds,
            Position = LedPoint,
            RadiusMm = -1,
            Draw = (g, screen, view) =>
            {
                double s = view.Age, env = Smooth(0, 0.6, s) * Smooth(0, 0.8, Seconds - s);
                var r = Screen(screen);
                if (screen == ScreenKind.ProFlow)
                {
                    using (var glow = new LinearGradientBrush(new RectangleF(0, r.Height * 0.45f, r.Width, r.Height * 0.55f + 1),
                        Color.FromArgb(0, 255, 236, 200), Color.FromArgb((int)(200 * env), 255, 236, 200), LinearGradientMode.Vertical))
                        g.FillRectangle(glow, 0, r.Height * 0.45f, r.Width, r.Height * 0.55f);
                    using var ink = WorldArt.Ink(4, (float)env);
                    using var molten = new SolidBrush(Color.FromArgb((int)(255 * env), 40, 40, 44));
                    for (int i = 0; i < 9; i++)
                    {
                        double life = (s * 0.45 + Hash(i + 200)) % 1;
                        float bx = (float)(r.Width * (0.1 + 0.8 * Hash(i + 210)) + 18 * Math.Sin(s * 2 + i));
                        float by = r.Bottom + 30 - (float)(life * (r.Height + 60));
                        float br = 10 + 16 * (float)Hash(i + 220);
                        g.FillEllipse(molten, bx - br, by - br, 2 * br, 2 * br);
                        g.DrawEllipse(ink, bx - br, by - br, 2 * br, 2 * br);
                        using var hot = new SolidBrush(Color.FromArgb((int)(220 * env), 255, 255, 255));
                        g.FillEllipse(hot, bx - br * 0.45f, by - br * 0.5f, br * 0.6f, br * 0.5f);
                    }
                }
                else
                {
                    var shipPx = _layout.ToPx(screen, new PointF(WorldState.ShipX(Clock.Ms), _layout.SurfaceY));
                    for (int i = 0; i < 26; i++)
                    {
                        double life = (s * 1.4 + Hash(i + 300)) % 1;
                        float bx = shipPx.X + (float)((Hash(i + 310) - 0.5) * 760);
                        float by = shipPx.Y + 20 - (float)(life * 40);
                        WorldArt.Bubble(g, new PointF(bx, by), (float)(5 + 8 * Hash(i + 320) * (1 - life)), null, (float)env);
                    }
                    using var steam = new SolidBrush(Color.FromArgb((int)(120 * env), 255, 255, 255));
                    for (int i = 0; i < 7; i++)
                    {
                        double life = (s * 0.35 + i / 7.0) % 1;
                        float sx = shipPx.X + (float)((Hash(i + 330) - 0.5) * 600), sy = shipPx.Y - (float)(life * 260);
                        float sr = 26 + (float)(life * 50);
                        g.FillEllipse(steam, sx - sr, sy - sr, 2 * sr, 2 * sr);
                    }
                }
            },
        });
        StampActor(at + 600, ScreenKind.Thermalright, "ボコボコ", new PointF(WorldState.ShipX(at) + 52, _layout.Top.Y + 24), 3, 11, 6);
        BannerActor(at, "O MAR ESTÁ FERVENDO — " + text, 0.6, Seconds - 0.4);
        return Seconds;
    }

    /// <summary>Copiou algo: uma bolha nasce na luz do fundo, sobe pela Pro Flow e estoura na superfície ("パチン").</summary>
    private void Bubble(long at, bool full, bool image = false)
    {
        string symbol = image ? "▣" : "Aa";
        if (!full)
        {
            float bx = _layout.Middle.X + 12 + (float)(_random.NextDouble() * (_layout.Middle.Width - 24));
            var my = Vertical(_layout.Middle.Bottom + 2, _layout.Middle.Y - 3, 0, 2.2, EaseInOut);
            Add(new WorldActor
            {
                Name = "bolhinha",
                Start = at,
                Seconds = 2.3,
                RadiusMm = 4,
                Position = s => new PointF(bx + (float)(2 * Math.Sin(s * 5)), my(s)),
                Draw = (g, screen, view) => WorldArt.Bubble(g, view.Px, 2.2f * view.PxPerMm.Width),
            });
            return;
        }
        float x0 = _layout.Led.X, x1 = SharedX(0.45);
        var y = Vertical(_layout.Led.Y, _layout.SurfaceY + 1, 0, 3.4, EaseInOut);
        Add(new WorldActor
        {
            Name = "bolha",
            Start = at,
            Seconds = 3.45,
            RadiusMm = 6,
            Emission = s => Pulse(s, 0.07, 0.05, 0.5),
            Warmth = -0.6,
            Position = s => new PointF((float)Lerp(x0, x1, Smooth(0, 3.4, s)) + (float)(2.5 * Math.Sin(s * 4)), y(s)),
            Draw = (g, screen, view) =>
            {
                WorldArt.DrawSprite(g, WorldArt.BubbleSprite(), view.Px, 3.6f * view.PxPerMm.Width / 30f, 0, 1);
                WorldArt.Bubble(g, view.Px, 3.6f * view.PxPerMm.Width * 0.85f, symbol, 0.0f, symbolOnly: true);
            },
        });
        StampActor(at + 3400, ScreenKind.Thermalright, "パチン", new PointF(x1 + 10, _layout.SurfaceY - 9), 0.7, 7, 8);
    }

    /// <summary>Música nova: "♪ título" no convés, e as notas afundam pela Pro Flow até a luz do fundo.</summary>
    private double Brook(long at, string title)
    {
        BannerActor(at, "♪ " + Shorten(title, 48), 0, 5.5);
        float rest = _layout.Middle.Bottom - 6;
        var arrivals = new List<double>();
        for (int i = 0; i < 4; i++)
        {
            double t0 = 0.6 + 0.6 * i, t1 = t0 + 2.6;
            arrivals.Add(t1);
            float x = SharedX(0.2 + 0.2 * i);
            var y = Vertical(_layout.SurfaceY - 6, rest, t0, t1, u => u);
            Add(new WorldActor
            {
                Name = "nota",
                Start = at,
                Seconds = t1 + 0.6,
                RadiusMm = 4,
                Position = s => new PointF(x + (float)(3 * Math.Sin(s * 3 + i)), y(s)),
                Opacity = s => s < t0 ? 0 : (float)Math.Clamp((t1 + 0.6 - s) / 0.5, 0, 1),
                Angle = s => (float)(18 * Math.Sin(s * 2.5 + i)),
                Draw = (g, screen, view) =>
                {
                    if (view.Opacity > 0) WorldArt.DrawSprite(g, WorldArt.NoteSprite(), view.Px, view.PxPerMm.Width / 7f, view.Angle, view.Opacity);
                },
            });
        }
        LedCue(at, 7, s => arrivals.Sum(a => Pulse(s - a, 0.08, 0.05, 0.3)), 0.2);
        return 6;
    }

    /// <summary>Pendrive conectado: um baú sobe das profundezas e fica no convés enquanto isso.</summary>
    private double Chest(long at, string label)
    {
        LedCue(at, 3, s => 0.18 * Math.Sin(Math.PI * Math.Clamp(s / 1.5, 0, 1)) + 0.05 * Smooth(0, 1.5, s) * (1 - Smooth(1.5, 3, s)), 0.5);
        var y = Vertical(_layout.Led.Y, _layout.SurfaceY - 2, 0, 4, EaseInOut);
        float x0 = _layout.Led.X, x1 = WorldState.ShipX(at) - 10;
        Add(new WorldActor
        {
            Name = "baú",
            Start = at,
            Seconds = 4.1,
            RadiusMm = 8,
            Position = s => new PointF((float)Lerp(x0, x1, Smooth(0, 4, s)), y(s)),
            Draw = (g, screen, view) => WorldArt.Chest(g, view.Px, view.PxPerMm.Width, null),
        });
        _chests.Add((label, at + 60_000));
        BannerActor(at, "Tesouro à vista: " + label, 4, 8.5);
        return 8.5;
    }

    /// <summary>Microfone/webcam ligou: o Den Den Mushi toca (purupuru… gacha) e fica no vidro da Pro Flow.</summary>
    private void DenDenRing(long at)
    {
        var rings = new List<double>();
        for (int c = 0; c < 2; c++)
            for (int j = 0; j < 3; j++) rings.Add(c * 1.6 + j * 0.4);
        LedCue(at, 4.5, s => rings.Sum(r => Pulse(s - r, 0.06, 0.04, 0.12)) + Pulse(s - 3.4, 0.1, 0.05, 0.35), -0.2);
        StampActor(at, ScreenKind.ProFlow, "プルプル", new PointF(_layout.Middle.Right - 30, _layout.Middle.Y + 12), 3.0, 8, -6);
        StampActor(at + 3400, ScreenKind.ProFlow, "ガチャ", new PointF(_layout.Middle.Right - 30, _layout.Middle.Y + 12), 0.9, 8, 6);
    }

    /// <summary>Laboon sobe para respirar (noite, PC dormindo): o olho enche a Pro Flow e a baleia esguicha atrás do Sunny.</summary>
    private void Laboon(long at)
    {
        LedCue(at, 10, _ => 0, -0.4, 0, s => 0.6 * Smooth(0, 2, s) * (1 - Smooth(7, 9, s)));
        Add(new WorldActor
        {
            Name = "laboon",
            Start = at,
            Seconds = 16,
            Position = LedPoint,
            RadiusMm = -1,
            Draw = (g, screen, view) =>
            {
                double s = view.Age;
                if (screen == ScreenKind.ProFlow)
                {
                    if (s < 1 || s > 9) return;
                    float a = (float)Math.Min(1, Math.Min((s - 1) / 0.8, (9 - s) / 0.8));
                    WorldArt.LaboonEye(g, Screen(screen), (float)Smooth(1, 9, s), a);
                    return;
                }
                if (s < 8) return;
                var r = Screen(screen);
                float rise = (float)(Smooth(8, 9.5, s) * (1 - Smooth(13.5, 15.5, s)));
                float cx = r.Width * 0.72f, top = r.Height * 0.66f - 70 * rise;
                using var body = new SolidBrush(Color.FromArgb(255, 34, 36, 42));
                using var shine = new Pen(Color.FromArgb(160, 210, 215, 225), 3);
                g.FillEllipse(body, cx - 420, top, 840, 220);
                g.DrawArc(shine, cx - 380, top + 14, 760, 120, 200, 140);
                if (s > 9.6 && s < 13.5)
                {
                    // Esguicho: um jato que sobe e abre em leque, com gotas caindo dos dois lados.
                    float sp = (float)Smooth(9.6, 10.3, s), fall = (float)Smooth(11.2, 13.5, s);
                    float a = 1 - fall;
                    using var water = new SolidBrush(Color.FromArgb((int)(235 * a), 255, 255, 255));
                    using var ink = WorldArt.Ink(3, a);
                    float jetTop = top - 170 * sp;
                    var jet = new[] { new PointF(cx - 12, top + 6), new PointF(cx - 30, jetTop + 30), new PointF(cx, jetTop), new PointF(cx + 30, jetTop + 30), new PointF(cx + 12, top + 6) };
                    g.FillClosedCurve(water, jet, FillMode.Winding, 0.4f);
                    g.DrawClosedCurve(ink, jet, 0.4f, FillMode.Winding);
                    for (int i = 0; i < 14; i++)
                    {
                        double ang = Math.PI * (0.15 + 0.7 * i / 13.0);
                        float dist = (float)(60 + 120 * sp + 160 * fall);
                        float dx = cx + (float)(Math.Cos(ang) * dist * (i % 2 == 0 ? 1 : 0.7));
                        float dy = jetTop + 20 + (float)(Math.Sin(ang - Math.PI / 2) * -10) + 260 * fall * fall;
                        float dr = 5 + 4 * (float)Hash(i + 1700);
                        g.FillEllipse(water, dx - dr, dy - dr * 1.3f, 2 * dr, 2.6f * dr);
                        g.DrawEllipse(ink, dx - dr, dy - dr * 1.3f, 2 * dr, 2.6f * dr);
                    }
                }
            },
        });
        StampActor(at + 9800, ScreenKind.Thermalright, "ブシュー", new PointF(_layout.Top.X + _layout.Top.Width * 0.82f, _layout.Top.Y + 10), 1.8, 10, 8);
    }

    /// <summary>Gaivota mergulha da Thermalright, pega um peixe na Pro Flow e volta.</summary>
    private void GullDive(long at)
    {
        float diveX = SharedX(0.35 + 0.3 * _random.NextDouble());
        var enter = new PointF(_layout.Top.X - 12, _layout.Top.Y + 10);
        var above = new PointF(diveX - 10, _layout.Top.Y + 14);
        var exit = new PointF(_layout.Top.Right + 14, _layout.Top.Y + 4);
        var dive = Vertical(_layout.Top.Y + 14, _layout.Middle.Y + 22, 0, 1, EaseIn);
        var up = Vertical(_layout.Middle.Y + 22, _layout.Top.Y + 12, 0, 1, EaseOut);
        Add(new WorldActor
        {
            Name = "gaivota",
            Start = at,
            Seconds = 7,
            RadiusMm = 10,
            Layer = 1,
            Position = s => s < 2.5 ? LerpPoint(enter, above, EaseOut(s / 2.5))
                : s < 3.7 ? new PointF(diveX, dive((s - 2.5) / 1.2))
                : s < 4.9 ? new PointF(diveX + (float)(s - 3.7) * 6, up((s - 3.7) / 1.2))
                : LerpPoint(new PointF(diveX + 7, _layout.Top.Y + 12), exit, EaseIn((s - 4.9) / 2.1)),
            Angle = s => s is >= 2.5 and < 3.7 ? 70 : s is >= 3.7 and < 4.9 ? -60 : 0,
            Draw = (g, screen, view) =>
            {
                var state = g.Save();
                g.TranslateTransform(view.Px.X, view.Px.Y);
                g.RotateTransform(view.Angle);
                WorldArt.NewsCoo(g, PointF.Empty, view.PxPerMm.Width * 0.95f, view.Age is >= 2.5 and < 3.7 ? 0.25 : view.Age * 2.4, 1, plain: true);
                if (view.Age >= 3.6) WorldArt.Fish(g, new PointF(10 * view.PxPerMm.Width * 0.95f, 6), view.PxPerMm.Width * 0.8f, 1, view.Age * 12);
                g.Restore(state);
            },
        });
    }

    /// <summary>Estrela cadente: risca o céu, cai no mar e afunda brilhando até o fundo.</summary>
    private void ShootingStar(long at)
    {
        float x = SharedX(0.3 + 0.4 * _random.NextDouble());
        var from = new PointF(x - 70, _layout.Top.Y + 3);
        var sink = Vertical(_layout.SurfaceY, _layout.Middle.Bottom - 4, 1.0, 3.8, EaseOut);
        Add(new WorldActor
        {
            Name = "estrela",
            Start = at,
            Seconds = 5,
            RadiusMm = 6,
            Position = s => s < 0.8 ? LerpPoint(from, new PointF(x, _layout.SurfaceY), EaseIn(s / 0.8)) : new PointF(x, sink(s)),
            Emission = s => s < 3.8 ? 0 : Pulse(s - 3.8, 0.05, 0.1, 0.6),
            Warmth = -0.5,
            Draw = (g, screen, view) =>
            {
                float a = (float)Math.Clamp((5 - view.Age) / 1, 0, 1);
                using var glow = new SolidBrush(Color.FromArgb((int)(230 * a), 255, 255, 240));
                float r = 0.8f * view.PxPerMm.Width;
                if (screen == ScreenKind.Thermalright && view.Age < 0.8)
                {
                    using var trail = new Pen(Color.FromArgb(200, 255, 255, 255), 3);
                    var tail = _layout.ToPx(screen, LerpPoint(from, new PointF(x, _layout.SurfaceY), EaseIn(Math.Max(0, view.Age - 0.15) / 0.8)));
                    g.DrawLine(trail, tail, view.Px);
                }
                g.FillEllipse(glow, view.Px.X - r, view.Px.Y - r, 2 * r, 2 * r);
            },
        });
    }

    /// <summary>Hora cheia: o sino do Sunny ("カーン") e um anel que desce pela água.</summary>
    private void Bell(long at)
    {
        StampActor(at, ScreenKind.Thermalright, "カーン", new PointF(WorldState.ShipX(at) + 2, _layout.Top.Y + 8), 1.4, 8, -4);
        Add(new WorldActor
        {
            Name = "sino",
            Start = at,
            Seconds = 2.6,
            Position = LedPoint,
            RadiusMm = -1,
            Draw = (g, screen, view) =>
            {
                if (screen != ScreenKind.ProFlow || view.Age < 0.2) return;
                var r = Screen(screen);
                double u = (view.Age - 0.2) / 2.4;
                float rr = (float)(40 + u * 520), a = (float)(1 - u);
                using var ring = new Pen(Color.FromArgb((int)(170 * a), 255, 255, 255), 4);
                g.DrawEllipse(ring, r.Width * 0.3f - rr, -rr * 0.5f, 2 * rr, rr);
            },
        });
    }

    /// <summary>Mudo: "シーン" (o silêncio do mangá).</summary>
    private void Shh(long at) =>
        StampActor(at, ScreenKind.Thermalright, "シーン", new PointF(_layout.Top.X + _layout.Top.Width * 0.62f, _layout.Top.Y + 20), 2.2, 11, 0);

    /// <summary>Volume mudou: um medidor rápido no céu (substitui o anterior).</summary>
    private void Gauge(long at, float level)
    {
        if (_gauge is not null) _actors.Remove(_gauge);
        _gauge = Add(new WorldActor
        {
            Name = "volume",
            Start = at,
            Seconds = 2.2,
            Position = _ => new PointF(_layout.Top.X + _layout.Top.Width / 2, _layout.Top.Y + 8),
            RadiusMm = -1,
            Layer = 1,
            Draw = (g, screen, view) =>
            {
                if (screen != ScreenKind.Thermalright) return;
                float a = (float)Math.Clamp((2.2 - view.Age) / 0.4, 0, 1);
                var box = new RectangleF(view.Px.X - 190, view.Px.Y - 24, 380, 50);
                using var shadow = WorldArt.Solid(a);
                using var paper = WorldArt.Paper(a);
                using var ink = WorldArt.Ink(3, a);
                g.FillRectangle(shadow, box.X + 6, box.Y + 6, box.Width, box.Height);
                g.FillRectangle(paper, box);
                g.DrawRectangle(ink, box.X, box.Y, box.Width, box.Height);
                g.FillRectangle(shadow, box.X + 14, box.Y + 18, (box.Width - 120) * level, 14);
                g.DrawRectangle(ink, box.X + 14, box.Y + 18, box.Width - 120, 14);
                using var font = new Font("Bahnschrift SemiBold", 26, GraphicsUnit.Pixel);
                g.DrawString($"{level * 100:0}%", font, shadow, box.Right - 98, box.Y + 9);
            },
        });
    }

    private static PointF LerpPoint(PointF a, PointF b, double t) => new((float)(a.X + (b.X - a.X) * t), (float)(a.Y + (b.Y - a.Y) * t));

    private static string Shorten(string? text, int max) =>
        string.IsNullOrEmpty(text) ? "" : text.Length <= max ? text : text[..(max - 1)] + "…";

    /// <summary>O print mais novo da pasta de capturas (dos últimos 20 s), para a foto do cartaz.</summary>
    private static string? RecentScreenshot()
    {
        try
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Screenshots");
            if (!Directory.Exists(folder)) return null;
            var newest = new DirectoryInfo(folder).EnumerateFiles("*.png").OrderByDescending(f => f.LastWriteTimeUtc).FirstOrDefault();
            return newest is not null && DateTime.UtcNow - newest.LastWriteTimeUtc < TimeSpan.FromSeconds(20) ? newest.FullName : null;
        }
        catch (IOException)
        {
            return null;
        }
    }
}

/// <summary>Desenha os atores do mundo que aparecem numa tela, na camada pedida.</summary>
internal static class WorldLayer
{
    public static void Draw(Graphics g, ScreenKind kind, WorldState world, long showAt, int layer)
    {
        var window = world.Layout.Window(kind);
        var k = world.Layout.PxPerMm(kind);
        foreach (var actor in world.Actors)
        {
            if (actor.Draw is null || actor.Layer != layer || !actor.Alive(showAt)) continue;
            double s = (showAt - actor.Start) / 1000.0;
            var mm = actor.Position(s);
            if (actor.RadiusMm >= 0)
            {
                var bounds = RectangleF.Inflate(window, actor.RadiusMm, actor.RadiusMm);
                if (!bounds.Contains(mm)) continue;
            }
            var state = g.Save();
            try
            {
                actor.Draw(g, kind, new ActorView(world.Layout.ToPx(kind, mm), k, actor.Angle(s), actor.Opacity(s), s, actor.Seconds));
            }
            finally
            {
                g.Restore(state);
            }
        }
    }
}
