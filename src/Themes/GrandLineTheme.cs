using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using DS339Monitor.App;
using DS339Monitor.Lighting;
using DS339Monitor.World;
using static DS339Monitor.Themes.ThemeKit;

namespace DS339Monitor.Themes;

/// <summary>
/// "Grand Line": a Thermalright vira um painel de mangá vivo, de ponta a ponta — o Thousand Sunny navegando, em preto e
/// branco com retícula, como as páginas que passam na Pro Flow. Tudo vem do <see cref="SceneDirector"/>:
/// <list type="bullet">
/// <item>o céu segue o sol de verdade (dia claro de papel, hora dourada, noite com lua e estrelas) e às vezes fecha
/// em tempestade;</item>
/// <item>o mar sobe com o som do PC e espirra nas batidas;</item>
/// <item>o painel da Pro Flow conta a história: a virada vira um "ドン!!", o Gear 5 transforma o sol no Nika com os
/// Tambores da Libertação, o Haki racha o céu com raios negros, o Barba Branca entorta o horizonte...;</item>
/// <item>o PC entra na história: download pronto é o Den Den Mushi tocando, jogo abrindo é o Coup de Burst, GPU quente é
/// Buster Call, PC parado é o Sunny ancorado de noite, e voltar é um "Bem-vindo de volta, Capitão!".</item>
/// </list>
/// Os tempos são os de <see cref="Choreo"/>, os mesmos do LED da placa. O pesado (céu, mar, navio, nuvens, onomatopeias)
/// é desenhado uma vez; cada quadro só posiciona.
/// </summary>
internal static class GrandLineTheme
{
    public const string Name = "Grand Line";

    public static Theme Create(int width, int height)
    {
        var f = new FontResolver(null);
        var e = new List<ThemeElement>();
        var left = new RectangleF(18, 14, 372, 62);
        var right = new RectangleF(width - 18 - 214, 14, 214, 62);
        const string Bold = "Bahnschrift SemiBold";
        Literal(e, f, "CPU", new RectangleF(left.X + 18, left.Y + 15, 70, 36), Bold, 26, Color.Black);
        Text(e, f, "CPUTemp", new RectangleF(left.X + 80, left.Y + 6, 110, 52), Bold, 38, Color.Black, TextAlign.Left, "°", 38);
        Literal(e, f, "GPU", new RectangleF(left.X + 196, left.Y + 15, 70, 36), Bold, 26, Color.Black);
        Text(e, f, "GPUTemp", new RectangleF(left.X + 258, left.Y + 6, 110, 52), Bold, 38, Color.Black, TextAlign.Left, "°", 38);
        Text(e, f, "CurrentTimeShut", new RectangleF(right.X, right.Y + 6, right.Width, 52), Bold, 40, Color.Black, TextAlign.Center);

        return new Theme
        {
            Name = Name,
            Width = width,
            Height = height,
            Animation = new GrandLineAnimation(width, height),
            BackgroundColor = Color.White,
            DrawStatic = g =>
            {
                Caption(g, left);
                Caption(g, right);
            },
            Elements = e.OrderBy(x => x.Z).ToList(),
        };
    }

    /// <summary>Caixa de narração de mangá: papel branco, traço preto e sombra dura.</summary>
    public static void Caption(Graphics g, RectangleF box)
    {
        using var shadow = new SolidBrush(Color.Black);
        using var paper = new SolidBrush(Color.White);
        using var ink = new Pen(Color.Black, 3);
        g.FillRectangle(shadow, box.X + 6, box.Y + 6, box.Width, box.Height);
        g.FillRectangle(paper, box);
        g.DrawRectangle(ink, box.X, box.Y, box.Width, box.Height);
    }
}

internal sealed class GrandLineAnimation : IAnimation
{
    private const int FrameMs = 50;           // 20 quadros por segundo: mar e navio andam liso
    private const int CalmFrameMs = 100;      // jogando ou dormindo: 10 por segundo (sobra CPU para o jogo)
    private const int SeaPeriod = 480;        // as faixas de mar se repetem a cada 480 px (rolagem sem emenda)
    private const float ShipScale = 0.82f;
    /// <summary>Período de cada camada de mar (múltiplos de 6, que a arte emenda sem costura): diferentes entre si, para
    /// a mesma onda não aparecer repetida lado a lado.</summary>
    private static readonly int[] SeaPeriods = { 480, 570, 690 };

    private readonly int _horizon;
    private readonly int _waterline; // linha d'água do Sunny: as ondas da frente cobrem o pé do casco
    private readonly Bitmap _shipDay, _shipNight, _glow, _gull, _gullUp, _denDen, _rain;
    private readonly Bitmap[] _cloudDay, _cloudDark;
    private readonly Bitmap[] _seaDay, _seaNight;
    private readonly int[] _seaTop = { 0, 22, 54 };
    private readonly PointF _shipAnchor;
    private readonly Bitmap _sunSprite;
    private readonly TextureBrush _halftone;
    private readonly Dictionary<string, Bitmap> _sfx = new();
    private readonly (float X, float Y, float R)[] _stars;
    private readonly PointF[][] _cracks;
    private readonly FontFamily _japanese;
    private Bitmap? _sky;
    private long _skyKey = -1;

    public GrandLineAnimation(int width, int height)
    {
        Width = width;
        Height = height;
        _horizon = (int)(height * 0.62);
        _waterline = _horizon + 92;
        _japanese = JapaneseFamily();
        _shipDay = MangaArt.Sunny(ShipScale, false, out _shipAnchor);
        _shipNight = MangaArt.Sunny(ShipScale, true, out _);
        _sunSprite = MangaArt.Sun(40);
        _glow = Glow(140);
        _gull = Gull(up: false);
        _gullUp = Gull(up: true);
        _denDen = DenDenMushi();
        _rain = Rain();
        _cloudDay = Enumerable.Range(0, 6).Select(i => MangaArt.Cloud(i, false)).ToArray();
        _cloudDark = Enumerable.Range(0, 6).Select(i => MangaArt.Cloud(i, true)).ToArray();
        _seaDay = Enumerable.Range(0, 3).Select(i => MangaArt.SeaStrip(i, SeaPeriods[i], width + SeaPeriods[i], false)).ToArray();
        _seaNight = Enumerable.Range(0, 3).Select(i => MangaArt.SeaStrip(i, SeaPeriods[i], width + SeaPeriods[i], true)).ToArray();
        _halftone = Halftone();
        _stars = Enumerable.Range(0, 110)
            .Select(i => ((float)(Choreo.Hash(i * 3 + 1) * width), (float)(Choreo.Hash(i * 3 + 2) * (_horizon - 70)), (float)(0.8 + 1.8 * Choreo.Hash(i * 3 + 3))))
            .ToArray();
        _cracks = Cracks(new PointF(width * 0.66f, height * 0.28f));
    }

    public int Count => 72_000; // uma hora de quadros; a cena em si não se repete (vem do relógio)
    public int Width { get; }
    public int Height { get; }
    public bool CacheFrames => false;
    public int DelayMs(int index) => SceneDirector.Current is { InGame: true } or { Sleeping: true } ? CalmFrameMs : FrameMs;
    public long FrameKey(int index) => index;

    /// <summary>JPEG + envio USB: o quadro aparece uns 20 ms depois de desenhado.</summary>
    private const long LatencyMs = 20;

    public void DrawFrame(Bitmap canvas, Graphics g, int index) => Draw(g, Clock.Ms + LatencyMs, SceneDirector.Current, WorldDirector.Current);

    public void Dispose()
    {
        _rainBrush?.Dispose();
        foreach (var bitmap in new[] { _shipDay, _shipNight, _glow, _gull, _gullUp, _denDen, _rain, _sky, _sunSprite })
            bitmap?.Dispose();
        foreach (var bitmap in _cloudDay.Concat(_cloudDark).Concat(_seaDay).Concat(_seaNight).Concat(_sfx.Values)) bitmap.Dispose();
        _halftone.Dispose();
        _japanese.Dispose();
    }

    /// <summary>Desenha a cena no instante <paramref name="now"/> (<see cref="Clock.Ms"/>) com o retrato <paramref name="stage"/>.</summary>
    public void Draw(Graphics g, long now, StageState stage, WorldState? world = null)
    {
        world ??= WorldDirector.Current;
        var saved = g.Save();
        g.CompositingMode = CompositingMode.SourceOver;
        g.InterpolationMode = InterpolationMode.Bilinear;
        g.PixelOffsetMode = PixelOffsetMode.Half;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        try
        {
            DrawScene(g, now, stage, world);
        }
        finally
        {
            g.Restore(saved);
        }
    }

    // ---------------------------------------------------------------- a cena

    private double _stormLevel, _sleepLevel;
    private long _lastScene;

    private void DrawScene(Graphics g, long now, StageState st, WorldState world)
    {
        double t = now / 1000.0;
        // Tempestade e sono chegam devagar (a tempestade em ~1 min; o sono em alguns segundos), nunca num corte.
        double dt = _lastScene == 0 ? 10 : Math.Clamp((now - _lastScene) / 1000.0, 0, 10);
        _lastScene = now;
        _stormLevel += ((st.Storm && !st.Sleeping ? 1 : 0) - _stormLevel) * (1 - Math.Exp(-dt / 20));
        _sleepLevel += ((st.Sleeping ? 1 : 0) - _sleepLevel) * (1 - Math.Exp(-dt / 2.5));
        var cue = st.Panel is { Turning: false } p ? p : (PanelScene?)null;
        double s = cue?.Seconds ?? 0, d = cue?.Duration ?? 1;
        double env = cue is null ? 0 : Choreo.Smooth(0, 0.8, s) * Choreo.Smooth(0, 0.8, d - s);
        PanelCue kind = cue?.Cue ?? PanelCue.None;

        // Quão noite está: o sol de verdade, a cena dormindo e os momentos do painel.
        double night = 1 - st.Daylight;
        night = Math.Max(night, 0.9 * _sleepLevel);
        if (kind == PanelCue.Farewell) night = Math.Max(night, 0.8 * Choreo.Smooth(0, 2.5, s) * Choreo.Smooth(0, 1, d - s));
        if (kind == PanelCue.Dawn) night = Math.Max(night, 0.85 * (1 - Choreo.Smooth(0.6, d * 0.85, s)) * env);
        if (kind == PanelCue.Drums) night *= 1 - 0.85 * env; // o Deus do Sol acende a noite
        double storm = _stormLevel < 0.02 ? 0 : _stormLevel;

        // Tremida de câmera: DON!, impacto, Coup de Burst.
        var shake = Shake(now, st, cue, s);
        g.TranslateTransform(shake.X, shake.Y);

        DrawSky(g, night, storm, st.Warmth * (1 - night));
        if (kind == PanelCue.Drums && env > 0) NikaSun(g, s, env);
        else if (kind == PanelCue.Dawn && env > 0) Sun(g, new PointF(Width * 0.72f, (float)(_horizon + 30 - 230 * Choreo.Smooth(0.6, d * 0.85, s))), 1, t, 1.3f);
        else if (night < 0.85 && storm < 0.5) Sun(g, SunPosition(st.SunArc), (float)(1 - night), t, (float)(1 + 0.45 * (1 - Math.Sin(Math.PI * st.SunArc))));
        if (night > 0.3) Moon(g, (float)Math.Min(1, (night - 0.3) / 0.4), t);
        Clouds(g, t, night, storm);
        if (night < 0.5 && storm == 0 && !st.Sleeping) Gulls(g, t, (float)(1 - night * 2));

        // Mar e navio (o tremor do Barba Branca inclina tudo junto).
        double audio = st.Sleeping ? 0 : st.Audio;
        double tide = kind == PanelCue.Tide ? env : 0;
        double drumBounce = kind == PanelCue.Drums ? env * DrumEnvelope(s) : 0;
        double amplitude = st.Muted ? 1.5 : 3 + 12 * audio + 16 * tide + 10 * drumBounce + (st.Sleeping ? -1 : 0) + 6 * storm;
        float tilt = kind == PanelCue.Quake ? (float)(2.6 * Choreo.Quake(s) * env * (Choreo.SlowNoise(s, 1.5) * 2 - 1)) : 0;
        if (tilt != 0)
        {
            g.TranslateTransform(Width / 2f, _horizon + 60);
            g.RotateTransform(tilt);
            g.TranslateTransform(-Width / 2f, -(_horizon + 60));
        }

        SeaLayer(g, 0, t, 14, 0, night);
        float midBob = (float)(amplitude * 0.5 * Math.Sin(t * 1.1));
        SeaLayer(g, 1, t, 30, midBob, night);
        if (night > 0.45 && kind != PanelCue.Drums) MoonPath(g, (float)Math.Min(1, (night - 0.45) / 0.3), t);
        WorldLayer.Draw(g, ScreenKind.Thermalright, world, now, -1);
        ShipAndStory(g, now, t, world, amplitude, midBob, night);
        WorldLayer.Draw(g, ScreenKind.Thermalright, world, now, 0); // o que entra na água fica atrás das ondas da frente
        float nearBob = (float)(amplitude * Math.Sin(t * 1.3 + 1.2));
        SeaLayer(g, 2, t, 62, nearBob, night);
        if (st.LastBeat > 0 && !st.Sleeping) Splash(g, (now - st.LastBeat) / 1000.0, st.LastBeat, nearBob);

        if (tilt != 0) g.ResetTransform();
        g.ResetTransform();
        g.TranslateTransform(shake.X, shake.Y);

        if (storm > 0) RainOverlay(g, t, (float)storm);
        PanelMoments(g, now, t, st, kind, s, d, env);
        Events(g, now, st);
        WorldLayer.Draw(g, ScreenKind.Thermalright, world, now, 1);
        if (world.Ship == ShipMode.Anchored) Sleeping(g, t, world.Layout);
    }

    private PointF SunPosition(double arc) =>
        new((float)(150 + arc * (Width - 300)), (float)(_horizon - 28 - Math.Sin(Math.PI * arc) * (_horizon - 90)));

    // ---------------------------------------------------------------- céu

    private void DrawSky(Graphics g, double night, double storm, double warmth)
    {
        long key = (long)Math.Round(night * 40) * 1000 + (long)Math.Round(storm * 4) * 100 + (long)Math.Round(warmth * 10);
        if (key != _skyKey || _sky is null)
        {
            _skyKey = key;
            _sky?.Dispose();
            _sky = Sky(Math.Round(night * 40) / 40, Math.Round(storm * 4) / 4, Math.Round(warmth * 10) / 10);
        }
        g.DrawImageUnscaled(_sky, 0, 0);
    }

    private Bitmap Sky(double night, double storm, double warmth)
    {
        int h = _horizon + 70;
        var sky = new Bitmap(Width, h, PixelFormat.Format32bppPArgb);
        using var g = Graphics.FromImage(sky);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        double top = Choreo.Lerp(Choreo.Lerp(232, 14, night), 100, storm * (1 - night * 0.6));
        double bottom = Choreo.Lerp(Choreo.Lerp(252, 48, night), 150, storm * (1 - night * 0.6));
        using (var gradient = new LinearGradientBrush(new Rectangle(0, 0, Width, h), Tint(top, warmth), Tint(bottom, warmth), LinearGradientMode.Vertical))
            g.FillRectangle(gradient, 0, 0, Width, h);

        // Retícula no alto do céu de dia, como o céu de um mangá.
        if (night < 0.6)
        {
            var region = new Region(new Rectangle(0, 0, Width, (int)(_horizon * 0.42)));
            g.SetClip(region, CombineMode.Replace);
            // Retícula forte no alto, sumindo para baixo (a faixa por cima vai de quase transparente à cor do céu).
            var skyTop = Tint(top, warmth);
            using var fade = new LinearGradientBrush(new Rectangle(0, 0, Width, (int)(_horizon * 0.42) + 1),
                Color.FromArgb((int)(255 * Math.Min(1, 0.2 + night / 0.6)), skyTop), Color.FromArgb(255, Tint(Choreo.Lerp(top, bottom, 0.42), warmth)), LinearGradientMode.Vertical);
            using var dots = (TextureBrush)_halftone.Clone();
            g.FillRectangle(dots, 0, 0, Width, (int)(_horizon * 0.42));
            g.FillRectangle(fade, 0, 0, Width, (int)(_horizon * 0.42));
            g.ResetClip();
            region.Dispose();
        }
        if (night > 0.35)
        {
            int alpha = (int)(255 * Math.Min(1, (night - 0.35) / 0.4));
            using var star = new SolidBrush(Color.FromArgb(alpha, 255, 255, 255));
            foreach (var (x, y, r) in _stars) g.FillEllipse(star, x - r / 2, y - r / 2, r, r);
        }
        return sky;
    }

    private static Color Tint(double gray, double warmth)
    {
        int v = (int)Math.Clamp(gray, 0, 255);
        return Color.FromArgb(v, (int)(v * (1 - 0.05 * warmth)), (int)(v * (1 - 0.15 * warmth)));
    }

    /// <summary>Sol de mangá: disco branco com contorno e raios girando devagar.</summary>
    private void Sun(Graphics g, PointF c, float alpha, double t, float scale)
    {
        if (alpha <= 0.02) return;
        var state = g.Save();
        g.TranslateTransform(c.X, c.Y);
        g.RotateTransform((float)(t * 4 % 360));
        g.ScaleTransform(scale, scale);
        using var attributes = Alpha(alpha);
        g.DrawImage(_sunSprite, new Rectangle(-_sunSprite.Width / 2, -_sunSprite.Height / 2, _sunSprite.Width, _sunSprite.Height), 0, 0, _sunSprite.Width, _sunSprite.Height, GraphicsUnit.Pixel, attributes);
        g.Restore(state);
    }

    /// <summary>Lua crescente com contorno claro e algumas estrelas cintilando.</summary>
    private void Moon(Graphics g, float alpha, double t)
    {
        var c = new PointF(Width * 0.8f, _horizon * 0.3f);
        // A fase de verdade (do hemisfério sul, como se vê de São Paulo: a parte acesa da crescente fica à esquerda).
        // Parte acesa = meio disco + meia elipse do terminador; o resto do disco aparece de leve (luz cinérea).
        const float R = 30;
        double phase = MoonPhase(DateTime.UtcNow);
        using var faint = new SolidBrush(Color.FromArgb((int)(40 * alpha), 240, 240, 232));
        g.FillEllipse(faint, c.X - R, c.Y - R, 2 * R, 2 * R);
        using var light = new SolidBrush(Color.FromArgb((int)(255 * alpha), 240, 240, 232));
        using var lit = new GraphicsPath();
        bool waxing = phase < 0.5;
        float w = (float)Math.Abs(Math.Cos(2 * Math.PI * phase)) * R;
        bool crescentShape = phase < 0.25 || phase > 0.75;
        var disk = new RectangleF(c.X - R, c.Y - R, 2 * R, 2 * R);
        var terminator = new RectangleF(c.X - Math.Max(w, 0.5f), c.Y - R, 2 * Math.Max(w, 0.5f), 2 * R);
        // No sul o lado aceso é o oposto do norte: crescente = esquerda.
        if (waxing)
        {
            lit.AddArc(disk, 90, 180);                                       // metade esquerda
            lit.AddArc(terminator, 270, crescentShape ? -180 : 180);
        }
        else
        {
            lit.AddArc(disk, 270, 180);                                      // metade direita
            lit.AddArc(terminator, 90, crescentShape ? -180 : 180);
        }
        lit.CloseFigure();
        g.FillPath(light, lit);
        using var sparkle = new Pen(Color.FromArgb((int)(220 * alpha), 255, 255, 255), 1.4f);
        for (int i = 0; i < 9; i++)
        {
            var (x, y, _) = _stars[i * 11];
            float k = (float)(Choreo.SlowNoise(t, 0.8, i * 31) * 5 * alpha);
            if (k < 1) continue;
            g.DrawLine(sparkle, x - k, y, x + k, y);
            g.DrawLine(sparkle, x, y - k, x, y + k);
        }
    }

    /// <summary>Fase da lua (0 = nova, 0,5 = cheia), a partir da lua nova de 6/jan/2000 18:14 UTC e do mês sinódico.</summary>
    private static double MoonPhase(DateTime utc)
    {
        double days = (utc - new DateTime(2000, 1, 6, 18, 14, 0, DateTimeKind.Utc)).TotalDays;
        double p = days / 29.530588853 % 1;
        return p < 0 ? p + 1 : p;
    }

    /// <summary>Reflexo da lua no mar: tracinhos claros tremendo.</summary>
    private void MoonPath(Graphics g, float alpha, double t)
    {
        using var pen = new Pen(Color.FromArgb((int)(170 * alpha), 230, 230, 220), 2);
        float x = Width * 0.8f;
        for (int i = 0; i < 9; i++)
        {
            float y = _horizon + 12 + i * 11;
            float w = 18 + i * 9 + (float)(8 * Math.Sin(t * 1.7 + i));
            g.DrawLine(pen, x - w / 2 + (float)(6 * Math.Sin(t + i * 1.3)), y, x + w / 2, y);
        }
    }

    private void Clouds(Graphics g, double t, double night, double storm)
    {
        int count = 4 + (int)(4 * storm);
        using var attributes = Alpha(night > 0.5 ? 0.55f : 1f);
        for (int i = 0; i < count; i++)
        {
            var sprite = (storm > 0 || night > 0.6 ? _cloudDark : _cloudDay)[i % _cloudDay.Length];
            double speed = 6 + 10 * Choreo.Hash(i * 5 + 1) + 20 * storm;
            float span = Width + sprite.Width;
            float x = (float)((Choreo.Hash(i * 5 + 2) * span + t * speed) % span) - sprite.Width;
            float y = (float)(18 + Choreo.Hash(i * 5 + 3) * (_horizon * 0.45) - 20 * storm);
            g.DrawImage(sprite, new Rectangle((int)x, (int)y, sprite.Width, sprite.Height), 0, 0, sprite.Width, sprite.Height, GraphicsUnit.Pixel, attributes);
        }
    }

    private void Gulls(Graphics g, double t, float alpha)
    {
        if (alpha < 0.05) return;
        for (int i = 0; i < 4; i++)
        {
            double speed = 28 + 18 * Choreo.Hash(i + 40);
            float span = Width + 120;
            float x = (float)((Choreo.Hash(i + 50) * span + t * speed) % span) - 60;
            float y = (float)(70 + 110 * Choreo.Hash(i + 60) + 8 * Math.Sin(t * 1.5 + i));
            g.DrawImageUnscaled(Math.Sin(t * 7 + i * 2) > 0 ? _gull : _gullUp, (int)x, (int)y);
        }
    }

    // ---------------------------------------------------------------- mar

    private void SeaLayer(Graphics g, int layer, double t, double speed, float bob, double night)
    {
        int offset = (int)(t * speed % SeaPeriods[layer]);
        int y = _horizon + _seaTop[layer] + (int)bob;
        if (night < 0.95) g.DrawImageUnscaled(_seaDay[layer], -offset, y);
        if (night > 0.05)
        {
            if (night >= 0.95)
            {
                g.DrawImageUnscaled(_seaNight[layer], -offset, y);
            }
            else
            {
                using var attributes = Alpha((float)night);
                var sprite = _seaNight[layer];
                g.DrawImage(sprite, new Rectangle(-offset, y, sprite.Width, sprite.Height), 0, 0, sprite.Width, sprite.Height, GraphicsUnit.Pixel, attributes);
            }
        }
    }

    /// <summary>Respingos na batida da música: gotas saltando da onda perto da proa.</summary>
    private void Splash(Graphics g, double x, long seed, float nearBob)
    {
        if (x < 0 || x > 0.7) return;
        using var ink = new Pen(Color.Black, 2);
        using var paper = new SolidBrush(Color.White);
        // Um respingo por batida, num lugar diferente da onda: gotas abrindo em leque e caindo.
        double origin = Width * (0.3 + 0.55 * Choreo.Hash(seed / 7));
        for (int i = 0; i < 8; i++)
        {
            double vx = 220 * (Choreo.Hash(seed + i) - 0.5), vy = -220 - 220 * Choreo.Hash(seed + i + 20);
            float px = (float)(origin + 14 * (i - 4) + vx * x);
            float py = (float)(_waterline + 4 + nearBob + vy * x + 900 * x * x);
            float r = 3 + 3 * (float)Choreo.Hash(seed + i + 40);
            g.FillEllipse(paper, px - r, py - r, 2 * r, 2 * r);
            g.DrawEllipse(ink, px - r, py - r, 2 * r, 2 * r);
        }
    }

    // ---------------------------------------------------------------- o Sunny

    private void ShipAndStory(Graphics g, long now, double t, WorldState world, double amplitude, float midBob, double night)
    {
        float x = world.Layout.ToPx(ScreenKind.Thermalright, new PointF(WorldState.ShipX(now), 0)).X;
        float waterline = _waterline + midBob;
        float angle = (float)((1.5 + amplitude * 0.15) * Math.Sin(t * 1.1 + 0.6));
        double since = (now - world.ShipSince) / 1000.0;
        float balloon = 0;
        switch (world.Ship)
        {
            case ShipMode.Away:
                return;
            case ShipMode.Launch:
                if (since < 2.3)
                {
                    x += (float)(3 * Math.Sin(since * 75) * Choreo.Smooth(0.8, 2.3, since)); // o mar treme embaixo
                }
                else
                {
                    double e = Math.Pow((since - 2.3) / 1.1, 2);
                    if (e > 1) return;
                    BurstCloud(g, new PointF(x, _waterline - 40), since - 2.3);
                    waterline -= (float)(720 * e);
                    angle -= (float)(14 * Math.Min(1, e * 3));
                }
                break;
            case ShipMode.Return:
                if (since < 5.9)
                {
                    double e = 1 - Math.Pow(1 - since / 5.9, 3);
                    waterline = (float)(-140 + (_waterline + midBob + 140) * e);
                    angle = (float)(6 * Math.Sin(since * 1.8));
                    balloon = (float)Math.Min(1, (5.9 - since) / 0.6 + 0.2);
                }
                else
                {
                    waterline += (float)(26 * Math.Exp(-(since - 5.9) / 0.35) * Math.Cos((since - 5.9) * 9));
                }
                break;
        }

        bool dark = night > 0.6;
        var sprite = dark ? _shipNight : _shipDay;
        var state = g.Save();
        g.TranslateTransform(x, waterline);
        g.RotateTransform(angle);
        g.DrawImage(sprite, -_shipAnchor.X, -_shipAnchor.Y, sprite.Width, sprite.Height);
        Chests(g, world.Chests);
        if (balloon > 0) WorldArt.OctopusBalloon(g, new PointF(0, -_shipAnchor.Y - 230), 8.65f, t, balloon);
        g.Restore(state);
    }

    /// <summary>Baús no convés (pendrives conectados), com o rótulo.</summary>
    private static void Chests(Graphics g, IReadOnlyList<string> chests)
    {
        for (int i = 0; i < chests.Count && i < 3; i++)
            WorldArt.Chest(g, new PointF(-40 + i * 64, -86), 6.2f, chests[i]);
    }

    private void Glow(Graphics g, PointF c, float alpha, float scale)
    {
        if (alpha <= 0.02) return;
        using var attributes = Alpha(Math.Min(1, alpha));
        float w = _glow.Width * scale, h = _glow.Height * scale;
        g.DrawImage(_glow, new Rectangle((int)(c.X - w / 2), (int)(c.Y - h / 2), (int)w, (int)h), 0, 0, _glow.Width, _glow.Height, GraphicsUnit.Pixel, attributes);
    }

    /// <summary>Nuvem do Coup de Burst: bolas de fumaça branca com traço, crescendo e sumindo.</summary>
    private static void BurstCloud(Graphics g, PointF c, double x)
    {
        if (x < 0 || x > 2.2) return;
        float a = (float)Math.Clamp(1 - (x - 1.2) / 1.0, 0, 1);
        using var ink = new Pen(Color.FromArgb((int)(255 * a), 20, 20, 20), 3);
        using var paper = new SolidBrush(Color.FromArgb((int)(255 * a), 255, 255, 255));
        for (int i = 0; i < 9; i++)
        {
            double ang = Math.PI * (0.55 + 0.9 * Choreo.Hash(i + 300));
            float dist = (float)(30 + 160 * (1 - Math.Exp(-x * 3)) * Choreo.Hash(i + 310));
            float r = (float)(22 + 30 * Choreo.Hash(i + 320)) * (float)(0.6 + 0.6 * Math.Min(1, x * 2));
            float px = c.X + (float)(Math.Cos(ang) * dist), py = c.Y + (float)(Math.Sin(ang) * dist * 0.5);
            g.FillEllipse(paper, px - r, py - r, 2 * r, 2 * r);
            g.DrawEllipse(ink, px - r, py - r, 2 * r, 2 * r);
        }
    }

    // ---------------------------------------------------------------- momentos dos painéis da Pro Flow

    private void PanelMoments(Graphics g, long now, double t, StageState st, PanelCue kind, double s, double d, double env)
    {
        // Viradas: DON!! no corte diagonal, onda de retícula na retícula.
        if (st.Last(StageEventKind.Turn, now) is { } turn)
        {
            double x = (now - turn.At) / 1000.0;
            if (turn.Variant == (int)PanelTurn.Cut) Stamp(g, "ドン!!", x - Choreo.TurnDon, (turn.At / 1000) % 2 == 0 ? new PointF(Width * 0.18f, 150) : new PointF(Width * 0.62f, 160), -8, 1.0f);
            else HalftoneWipe(g, x);
        }

        if (kind == PanelCue.None || env <= 0.01) return;
        switch (kind)
        {
            case PanelCue.Impact:
            {
                double x = s - Choreo.Impact[0].At;
                if (x >= 0 && x < 0.9) SpeedLines(g, new PointF(Width * 0.6f, Height * 0.42f), (float)(1 - x / 0.9), (long)(s * 7));
                Stamp(g, "ドォン!!", x, new PointF(Width * 0.62f, 150), 6, 1.25f);
                break;
            }
            case PanelCue.Drums:
                Stamp(g, "ドンドットット", s - 0.3, new PointF(Width * 0.55f, 95), -4, 0.8f, hold: 2.6);
                break;
            case PanelCue.Haki:
            {
                using (var shade = new SolidBrush(Color.FromArgb((int)(130 * env), 0, 0, 0)))
                    g.FillRectangle(shade, 0, 0, Width, Height);
                SkySplit(g, s, env);
                foreach (var (at, _, index) in Choreo.HakiCrackles(s))
                {
                    double age = s - at;
                    double envAt = Choreo.Smooth(0, 0.8, at) * Choreo.Smooth(0, 0.8, d - at);
                    if (age >= 0 && age < 0.28 && envAt > 0.3) Bolt(g, index * 97 + 13, (float)(1 - age / 0.28), black: true);
                }
                Stamp(g, "ゴゴゴゴ", s - 0.4, new PointF(Width * 0.86f, 170), 10, 0.85f, hold: d - 1.2);
                break;
            }
            case PanelCue.Quake:
            {
                double q = Choreo.Quake(s) * env;
                if (q > 0.02) AirCracks(g, (float)q);
                Stamp(g, "グラグラ", s - Choreo.QuakeStart, new PointF(Width * 0.8f, 120), -6, 0.9f, hold: 3);
                break;
            }
            case PanelCue.Farewell:
                Snow(g, t, (float)(env * Choreo.Smooth(0, 2, s)));
                break;
        }
    }

    /// <summary>Sol do Nika (Gear 5): disco grande com chamas brancas ondulando, sorriso e anéis a cada batida do tambor.</summary>
    private void NikaSun(Graphics g, double s, double env)
    {
        var c = new PointF(Width * 0.72f, _horizon * 0.42f);
        float a = (float)env;
        float r = 64;
        using var ink = new Pen(Color.FromArgb((int)(255 * a), 15, 15, 15), 4) { LineJoin = LineJoin.Round };
        using var paper = new SolidBrush(Color.FromArgb((int)(255 * a), 255, 255, 255));

        // Anéis: um forte a cada ciclo, três finos no "do-tto-to".
        double cycleStart = Math.Floor(s / Choreo.DrumCycle) * Choreo.DrumCycle;
        for (int back = 0; back < 2; back++)
        {
            double start = cycleStart - back * Choreo.DrumCycle;
            Ring(g, c, r, s - start, 6, a);
            foreach (double light in Choreo.DrumLight) Ring(g, c, r, s - start - light * Choreo.DrumCycle, 2.5f, a * 0.75f);
        }

        // Chamas: 22 línguas onduladas em volta do disco.
        var flames = new List<PointF>();
        for (int i = 0; i <= 88; i++)
        {
            double ang = i * Math.PI * 2 / 88;
            double wave = Math.Sin(ang * 11 + s * 5) * 0.5 + 0.5;
            float rr = r + 10 + (float)(28 * wave + 6 * Math.Sin(ang * 3 - s * 2));
            flames.Add(new PointF(c.X + (float)(Math.Cos(ang) * rr), c.Y + (float)(Math.Sin(ang) * rr)));
        }
        g.FillPolygon(paper, flames.ToArray());
        g.DrawPolygon(ink, flames.ToArray());
        g.FillEllipse(paper, c.X - r, c.Y - r, 2 * r, 2 * r);
        g.DrawEllipse(ink, c.X - r, c.Y - r, 2 * r, 2 * r);
        // Rosto: olhos fechados de riso e o sorrisão.
        g.DrawArc(ink, c.X - 30, c.Y - 18, 20, 14, 200, 140);
        g.DrawArc(ink, c.X + 10, c.Y - 18, 20, 14, 200, 140);
        g.DrawArc(ink, c.X - 32, c.Y - 20, 64, 52, 20, 140);
    }

    private static void Ring(Graphics g, PointF c, float r, double age, float width, float alpha)
    {
        if (age < 0 || age > 1.25) return;
        float rr = r + 30 + (float)(age * 300);
        float fade = (float)(1 - age / 1.25);
        using var pen = new Pen(Color.FromArgb((int)(255 * alpha * fade), 255, 255, 255), width + 6);
        using var ink = new Pen(Color.FromArgb((int)(255 * alpha * fade), 15, 15, 15), width);
        g.DrawEllipse(pen, c.X - rr, c.Y - rr, 2 * rr, 2 * rr);
        g.DrawEllipse(ink, c.X - rr, c.Y - rr, 2 * rr, 2 * rr);
    }

    /// <summary>Céu Partido: uma rachadura negra atravessa o céu no choque do Haki do Rei.</summary>
    private void SkySplit(Graphics g, double s, double env)
    {
        double grow = Choreo.Smooth(0.3, 0.9, s);
        if (grow <= 0) return;
        var points = new List<PointF>();
        float y = _horizon * 0.36f;
        int n = 46;
        for (int i = 0; i <= n * grow; i++)
        {
            float x = Width * i / (float)n + (float)((Choreo.Hash(i + 690) - 0.5) * 18);
            points.Add(new PointF(x, y + (float)((Choreo.Hash(i + 700) - 0.5) * 64)));
        }
        if (points.Count < 2) return;
        using var glow = new Pen(Color.FromArgb((int)(230 * env), 255, 255, 255), 22) { LineJoin = LineJoin.Miter, MiterLimit = 3 };
        using var core = new Pen(Color.FromArgb((int)(255 * env), 0, 0, 0), 11) { LineJoin = LineJoin.Miter, MiterLimit = 3 };
        using var chip = new Pen(Color.FromArgb((int)(255 * env), 0, 0, 0), 4) { LineJoin = LineJoin.Miter };
        g.DrawLines(glow, points.ToArray());
        g.DrawLines(core, points.ToArray());
        // Lascas saindo da rachadura, para cima e para baixo.
        for (int i = 2; i < points.Count - 1; i += 3)
        {
            var p = points[i];
            float dir = Choreo.Hash(i + 710) > 0.5 ? 1 : -1;
            var tip = new PointF(p.X + (float)((Choreo.Hash(i + 720) - 0.5) * 50), p.Y + dir * (24 + 40 * (float)Choreo.Hash(i + 730)));
            g.DrawLine(chip, p, tip);
        }
    }

    /// <summary>Raio em zigue-zague, com galhos. Negro (Haki) tem miolo preto e borda branca; o da tempestade, o contrário.</summary>
    private void Bolt(Graphics g, long seed, float alpha, bool black)
    {
        var points = new List<PointF>();
        float x = (float)(Width * (0.12 + 0.76 * Choreo.Hash(seed)));
        float y = 0;
        while (y < _horizon - 20)
        {
            points.Add(new PointF(x, y));
            y += 26 + (float)(30 * Choreo.Hash(seed + points.Count * 3 + 1));
            x += (float)((Choreo.Hash(seed + points.Count * 3 + 2) - 0.5) * 70);
        }
        points.Add(new PointF(x, _horizon - 20));
        var main = points.ToArray();
        int a = (int)(255 * Math.Clamp(alpha, 0, 1));
        using var outer = new Pen(black ? Color.FromArgb(a, 255, 255, 255) : Color.FromArgb(a, 20, 20, 20), 12) { LineJoin = LineJoin.Round };
        using var inner = new Pen(black ? Color.FromArgb(a, 0, 0, 0) : Color.FromArgb(a, 255, 255, 255), 6) { LineJoin = LineJoin.Round };
        g.DrawLines(outer, main);
        g.DrawLines(inner, main);
        // Um galho saindo do meio.
        int from = main.Length / 2;
        var branch = new[] { main[from], new PointF(main[from].X + 60, main[from].Y + 40), new PointF(main[from].X + 90, main[from].Y + 95) };
        outer.Width = 8;
        inner.Width = 3;
        g.DrawLines(outer, branch);
        g.DrawLines(inner, branch);
    }

    /// <summary>Gura Gura: o ar racha em linhas brancas saindo de um ponto do céu.</summary>
    private void AirCracks(Graphics g, float alpha)
    {
        using var outer = new Pen(Color.FromArgb((int)(230 * alpha), 20, 20, 20), 7) { LineJoin = LineJoin.Miter };
        using var inner = new Pen(Color.FromArgb((int)(255 * alpha), 255, 255, 255), 3) { LineJoin = LineJoin.Miter };
        foreach (var crack in _cracks)
        {
            g.DrawLines(outer, crack);
            g.DrawLines(inner, crack);
        }
    }

    private void Snow(Graphics g, double t, float alpha)
    {
        if (alpha <= 0.02) return;
        using var flake = new SolidBrush(Color.FromArgb((int)(230 * alpha), 255, 255, 255));
        for (int i = 0; i < 70; i++)
        {
            double speed = 18 + 26 * Choreo.Hash(i + 900);
            float x = (float)(Choreo.Hash(i + 910) * Width + 20 * Math.Sin(t * 0.6 + i));
            float y = (float)((Choreo.Hash(i + 920) * Height + t * speed) % Height);
            float r = 1.5f + 2.5f * (float)Choreo.Hash(i + 930);
            g.FillEllipse(flake, x - r, y - r, 2 * r, 2 * r);
        }
    }

    /// <summary>Linhas de velocidade de mangá saindo de um ponto (impacto, Coup de Burst).</summary>
    private void SpeedLines(Graphics g, PointF c, float alpha, long seed)
    {
        if (alpha <= 0.02) return;
        using var ink = new SolidBrush(Color.FromArgb((int)(200 * alpha), 10, 10, 10));
        for (int i = 0; i < 72; i++)
        {
            double ang = i * Math.PI * 2 / 72 + (Choreo.Hash(seed * 100 + i) - 0.5) * 0.05;
            float r0 = 170 + 90 * (float)Choreo.Hash(seed * 100 + i + 50), r1 = 1300;
            double half = 0.004 + 0.008 * Choreo.Hash(seed * 100 + i + 90);
            var p = new[]
            {
                new PointF(c.X + (float)(Math.Cos(ang) * r0), c.Y + (float)(Math.Sin(ang) * r0)),
                new PointF(c.X + (float)(Math.Cos(ang - half) * r1), c.Y + (float)(Math.Sin(ang - half) * r1)),
                new PointF(c.X + (float)(Math.Cos(ang + half) * r1), c.Y + (float)(Math.Sin(ang + half) * r1)),
            };
            g.FillPolygon(ink, p);
        }
    }

    /// <summary>Onda de retícula varrendo a tela (a transição de retícula da Pro Flow).</summary>
    private void HalftoneWipe(Graphics g, double x)
    {
        if (x < 0 || x > 0.8) return;
        float center = (float)(-300 + (Width + 600) * (x / 0.8));
        var state = g.Save();
        g.SetClip(new RectangleF(center - 220, 0, 440, Height));
        g.FillRectangle(_halftone, center - 220, 0, 440, Height);
        g.Restore(state);
    }

    // ---------------------------------------------------------------- acontecimentos do PC

    private void Events(Graphics g, long now, StageState st)
    {
        foreach (var e in st.Events)
        {
            double x = (now - e.At) / 1000.0;
            if (x < 0) continue;
            switch (e.Kind)
            {
                case StageEventKind.Lightning when x < 1.0:
                {
                    double flash = Choreo.Lightning(x, e.Variant);
                    if (x < 0.35) Bolt(g, e.At, (float)(1 - x / 0.35), black: false);
                    using var white = new SolidBrush(Color.FromArgb((int)(150 * flash), 255, 255, 255));
                    g.FillRectangle(white, 0, 0, Width, Height);
                    break;
                }
            }
        }
    }

    private TextureBrush? _rainBrush;

    /// <summary>Chuva em duas camadas (velocidades e tamanhos diferentes): a segunda entra quando a tempestade engrossa.</summary>
    private void RainOverlay(Graphics g, double t, float strength)
    {
        if (strength < 0.25f) return;
        _rainBrush ??= new TextureBrush(_rain, WrapMode.Tile);
        for (int layer = 0; layer < (strength > 0.6f ? 2 : 1); layer++)
        {
            float speed = layer == 0 ? 620 : 900, drift = layer == 0 ? 160 : 260, scale = layer == 0 ? 1 : 1.6f;
            _rainBrush.ResetTransform();
            _rainBrush.TranslateTransform((float)(-t * drift % (256 * scale)), (float)(t * speed % (256 * scale)));
            _rainBrush.ScaleTransform(scale, scale);
            g.FillRectangle(_rainBrush, 0, 0, Width, Height);
        }
    }

    private void Sleeping(Graphics g, double t, WorldLayout layout)
    {
        float shipX = layout.ToPx(ScreenKind.Thermalright, new PointF(WorldState.ShipX((long)(t * 1000)), 0)).X;
        // Zzz subindo da cabeça de leão (o Sunny cochilando), no céu livre à frente das velas e abaixo das legendas.
        float bowX = shipX - _shipAnchor.X + _shipDay.Width - 40;
        for (int i = 0; i < 3; i++)
        {
            double phase = (t * 0.45 + i / 3.0) % 1;
            float a = (float)Math.Sin(Math.PI * phase);
            float x = bowX + (float)(phase * 50) + i * 8;
            float y = _waterline - _shipAnchor.Y * 0.62f - (float)(phase * 90);
            Stamp(g, "Z", 0.5, new PointF(x, y), -10, 0.35f + 0.12f * i, hold: 99, alpha: a * 0.9f);
        }
    }

    /// <summary>Faixa de narração no rodapé (texto do acontecimento), entrando e saindo suave.</summary>
    private void Banner(Graphics g, string text, double x, double length, bool big = false)
    {
        if (x < 0 || x > length) return;
        float alpha = (float)Math.Min(1, Math.Min(x / 0.35, (length - x) / 0.5));
        using var font = new Font("Bahnschrift SemiBold", big ? 40 : 32, GraphicsUnit.Pixel);
        var size = g.MeasureString(text, font);
        var box = new RectangleF((Width - size.Width - 60) / 2, big ? 100 : Height - 92, size.Width + 60, size.Height + 22);
        using var shadow = new SolidBrush(Color.FromArgb((int)(255 * alpha), 0, 0, 0));
        using var paper = new SolidBrush(Color.FromArgb((int)(255 * alpha), 255, 255, 255));
        using var ink = new Pen(Color.FromArgb((int)(255 * alpha), 0, 0, 0), big ? 5 : 3);
        using var textBrush = new SolidBrush(Color.FromArgb((int)(255 * alpha), 0, 0, 0));
        g.FillRectangle(shadow, box.X + 7, box.Y + 7, box.Width, box.Height);
        g.FillRectangle(paper, box);
        g.DrawRectangle(ink, box.X, box.Y, box.Width, box.Height);
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
        g.DrawString(text, font, textBrush, box.X + 30, box.Y + 11);
    }

    /// <summary>
    /// Onomatopeia de mangá carimbada: entra crescendo (1,35× → 1× em 120 ms), fica <paramref name="hold"/> s e some em
    /// 0,3 s. <paramref name="x"/> = segundos desde que entrou.
    /// </summary>
    private void Stamp(Graphics g, string text, double x, PointF center, float angle, float scale, double hold = 0.7, float alpha = 1)
    {
        if (x < 0 || x > hold + 0.3) return;
        var sprite = Sfx(text);
        float pop = x < 0.12 ? (float)(1.35 - 0.35 * x / 0.12) : 1;
        float a = alpha * (float)Math.Clamp((hold + 0.3 - x) / 0.3, 0, 1);
        var state = g.Save();
        g.TranslateTransform(center.X, center.Y);
        g.RotateTransform(angle);
        g.ScaleTransform(scale * pop, scale * pop);
        using var attributes = Alpha(a);
        g.DrawImage(sprite, new Rectangle(-sprite.Width / 2, -sprite.Height / 2, sprite.Width, sprite.Height), 0, 0, sprite.Width, sprite.Height, GraphicsUnit.Pixel, attributes);
        g.Restore(state);
    }

    private PointF Shake(long now, StageState st, PanelScene? cue, double s)
    {
        double power = 0;
        if (st.Last(StageEventKind.Turn, now) is { Variant: (int)PanelTurn.Cut } turn)
        {
            double x = (now - turn.At) / 1000.0 - Choreo.TurnDon;
            if (x >= 0 && x < 0.18) power = 4 * (1 - x / 0.18);
        }
        if (cue is { Cue: PanelCue.Impact })
        {
            double x = s - Choreo.Impact[0].At;
            if (x >= 0 && x < 0.3) power = Math.Max(power, 8 * (1 - x / 0.3));
        }
        if (power <= 0) return PointF.Empty;
        return new PointF((float)(power * Math.Sin(now * 0.09)), (float)(power * Math.Cos(now * 0.13)));
    }

    private static double DrumEnvelope(double s)
    {
        double t = s % Choreo.DrumCycle;
        return Math.Min(1, Choreo.Hit(t, 0, 1));
    }

    // ---------------------------------------------------------------- pré-desenho (uma vez)

    private Bitmap Sfx(string text)
    {
        if (_sfx.TryGetValue(text, out var cached)) return cached;
        using var path = new GraphicsPath();
        var style = _japanese.IsStyleAvailable(FontStyle.Bold) ? FontStyle.Bold : FontStyle.Regular;
        path.AddString(text, _japanese, (int)style, 120, PointF.Empty, StringFormat.GenericTypographic);
        var bounds = path.GetBounds();
        const int Pad = 24;
        var bitmap = new Bitmap((int)bounds.Width + 2 * Pad, (int)bounds.Height + 2 * Pad, PixelFormat.Format32bppPArgb);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TranslateTransform(Pad - bounds.X, Pad - bounds.Y);
            using var halo = new Pen(Color.White, 24) { LineJoin = LineJoin.Round };
            using var ink = new Pen(Color.Black, 12) { LineJoin = LineJoin.Round };
            g.DrawPath(halo, path);
            g.DrawPath(ink, path);
            g.FillPath(Brushes.White, path);
        }
        _sfx[text] = bitmap;
        return bitmap;
    }

    private static FontFamily JapaneseFamily()
    {
        using var fonts = new InstalledFontCollection();
        foreach (string name in new[] { "Yu Gothic UI", "Yu Gothic", "Meiryo UI", "Meiryo", "MS Gothic" })
        {
            var family = fonts.Families.FirstOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (family is not null) return new FontFamily(family.Name);
        }
        return new FontFamily(GenericFontFamilies.SansSerif);
    }

    private static Bitmap Glow(int size)
    {
        var bitmap = new Bitmap(size, size, PixelFormat.Format32bppPArgb);
        using var g = Graphics.FromImage(bitmap);
        using var path = new GraphicsPath();
        path.AddEllipse(0, 0, size, size);
        using var brush = new PathGradientBrush(path)
        {
            CenterColor = Color.FromArgb(230, 255, 236, 200),
            SurroundColors = new[] { Color.FromArgb(0, 255, 236, 200) },
        };
        g.FillEllipse(brush, 0, 0, size, size);
        return bitmap;
    }

    private static Bitmap Gull(bool up)
    {
        var bitmap = new Bitmap(46, 22, PixelFormat.Format32bppPArgb);
        using var g = Graphics.FromImage(bitmap);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var ink = new Pen(Color.FromArgb(30, 30, 30), 3) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        if (up)
        {
            g.DrawBezier(ink, 3, 16, 10, 2, 18, 4, 23, 14);
            g.DrawBezier(ink, 23, 14, 28, 4, 36, 2, 43, 16);
        }
        else
        {
            g.DrawBezier(ink, 3, 8, 10, 14, 18, 16, 23, 12);
            g.DrawBezier(ink, 23, 12, 28, 16, 36, 14, 43, 8);
        }
        return bitmap;
    }

    /// <summary>Den Den Mushi: o caracol-telefone, com o fone na concha.</summary>
    private static Bitmap DenDenMushi()
    {
        var bitmap = new Bitmap(220, 170, PixelFormat.Format32bppPArgb);
        using var g = Graphics.FromImage(bitmap);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var ink = new Pen(Color.FromArgb(20, 20, 20), 4) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round };
        using var thin = new Pen(Color.FromArgb(20, 20, 20), 2.5f);
        using var paper = new SolidBrush(Color.White);
        using var solid = new SolidBrush(Color.FromArgb(20, 20, 20));
        // Corpo.
        using (var body = new GraphicsPath())
        {
            body.AddBezier(18, 150, 40, 128, 120, 128, 196, 140);
            body.AddBezier(196, 140, 208, 120, 206, 92, 188, 80);
            body.AddBezier(188, 80, 172, 70, 160, 100, 164, 120);
            body.AddBezier(164, 120, 120, 128, 60, 132, 18, 150);
            body.CloseFigure();
            g.FillPath(paper, body);
            g.DrawPath(ink, body);
        }
        // Olhos nos pedúnculos e boca.
        g.DrawLine(ink, 178, 82, 172, 52);
        g.DrawLine(ink, 192, 84, 198, 54);
        g.FillEllipse(paper, 163, 40, 18, 18);
        g.DrawEllipse(ink, 163, 40, 18, 18);
        g.FillEllipse(paper, 190, 42, 18, 18);
        g.DrawEllipse(ink, 190, 42, 18, 18);
        g.FillEllipse(solid, 169, 46, 7, 8);
        g.FillEllipse(solid, 196, 48, 7, 8);
        g.DrawArc(thin, 176, 100, 22, 16, 10, 160);
        // Concha em espiral.
        g.FillEllipse(paper, 40, 56, 100, 92);
        g.DrawEllipse(ink, 40, 56, 100, 92);
        g.DrawArc(thin, 58, 72, 64, 60, 0, 300);
        g.DrawArc(thin, 72, 86, 38, 34, 300, 300);
        g.DrawArc(thin, 84, 96, 16, 14, 240, 300);
        // Fone e botão no alto da concha.
        using (var phone = new GraphicsPath())
        {
            phone.AddBezier(46, 46, 70, 30, 110, 30, 134, 46);
            phone.AddLine(134, 46, 128, 58);
            phone.AddBezier(128, 58, 106, 46, 74, 46, 52, 58);
            phone.CloseFigure();
            g.FillPath(solid, phone);
        }
        g.FillEllipse(solid, 36, 40, 22, 22);
        g.FillEllipse(solid, 122, 40, 22, 22);
        return bitmap;
    }

    private static Bitmap Rain()
    {
        var bitmap = new Bitmap(256, 256, PixelFormat.Format32bppPArgb);
        using var g = Graphics.FromImage(bitmap);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var pen = new Pen(Color.FromArgb(90, 230, 232, 240), 1.6f);
        for (int i = 0; i < 46; i++)
        {
            float x = (float)(Choreo.Hash(i + 1200) * 256), y = (float)(Choreo.Hash(i + 1300) * 256);
            float len = 18 + 22 * (float)Choreo.Hash(i + 1400);
            for (int dx = -256; dx <= 256; dx += 256)
                for (int dy = -256; dy <= 256; dy += 256)
                    g.DrawLine(pen, x + dx, y + dy, x + dx - len * 0.28f, y + dy + len);
        }
        return bitmap;
    }

    private static TextureBrush Halftone(Color? color = null)
    {
        var tile = new Bitmap(10, 10, PixelFormat.Format32bppPArgb);
        using (var g = Graphics.FromImage(tile))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var dot = new SolidBrush(color ?? Color.FromArgb(120, 0, 0, 0));
            g.FillEllipse(dot, 2.5f, 2.5f, 5, 5);
        }
        var brush = new TextureBrush(tile, WrapMode.Tile);
        brush.RotateTransform(45);
        return brush;
    }

    private PointF[][] Cracks(PointF origin)
    {
        var cracks = new List<PointF[]>();
        for (int i = 0; i < 8; i++)
        {
            double ang = i * Math.PI * 2 / 8 + Choreo.Hash(i + 1500) * 0.5;
            var points = new List<PointF> { origin };
            float x = origin.X, y = origin.Y;
            for (int k = 0; k < 6; k++)
            {
                double a = ang + (Choreo.Hash(i * 10 + k + 1600) - 0.5) * 0.9;
                float step = 26 + 30 * (float)Choreo.Hash(i * 10 + k + 1700);
                x += (float)(Math.Cos(a) * step);
                y += (float)(Math.Sin(a) * step * 0.7);
                points.Add(new PointF(x, Math.Min(y, _horizon - 10)));
            }
            cracks.Add(points.ToArray());
        }
        return cracks.ToArray();
    }

    private static ImageAttributes Alpha(float alpha)
    {
        var attributes = new ImageAttributes();
        attributes.SetColorMatrix(new ColorMatrix { Matrix33 = Math.Clamp(alpha, 0, 1) });
        return attributes;
    }
}
