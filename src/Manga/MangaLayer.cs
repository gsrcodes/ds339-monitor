using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using DS339Monitor.Lighting;
using DS339Monitor.Themes;
using static DS339Monitor.Manga.MangaStage;

namespace DS339Monitor.Manga;

/// <summary>
/// Encena o Modo Mangá numa tela, por cima do quadro que o tema acabou de desenhar (o vídeo, o GIF ou o painel):
/// primeiro o que mexe na imagem (onda, soco de câmera, tremida, quadro de impacto), depois tinta e luz (linhas de
/// foco, rachaduras, raios do Haki, fachos, flash), por fim o letreiramento (onomatopeias, balões, manchetes).
/// A Thermalright é a "trilha sonora" (as onomatopeias e as manchetes); a Pro Flow é o painel que sente o golpe e onde
/// os personagens "falam" (os balões das ações do PC, colocados no pedaço mais vazio do painel).
/// Só roda na thread de animação.
/// </summary>
/// <summary>Ritmo de quadros que os efeitos pedem.</summary>
internal enum MangaPace
{
    None,
    /// <summary>Redesenha cada quadro da animação (sem o cache de JPEG), no ritmo dela.</summary>
    Slow,
    /// <summary>~30 quadros por segundo.</summary>
    Fast,
}

internal sealed class MangaLayer : IDisposable
{
    private const int FlashFrameMs = 90; // quadro de impacto: ~3 quadros a 30 por segundo

    private static readonly object GifGate = new();
    private static string? _gifPath;
    private static Task<FrameAnimation?>? _gif;

    private readonly ScreenKind _kind;
    private readonly Dictionary<MangaCue, CueArt> _art = new();
    private Bitmap? _scratch;
    private Bitmap? _onAirTag;
    private Bitmap? _sleepBalloon;

    public MangaLayer(ScreenKind kind) => _kind = kind;

    private bool Wide => _kind == ScreenKind.Thermalright;

    /// <summary>Peças criadas para um acontecimento (texto renderizado, onde o balão ficou).</summary>
    private sealed class CueArt : IDisposable
    {
        public Bitmap? Sprite;
        public PointF Place;
        public bool Placed;
        public MangaFx.Crack? Crack;

        public void Dispose()
        {
            Sprite?.Dispose();
        }
    }

    /// <summary>Carrega (em segundo plano) o GIF do despertar do Gear 5, já no tamanho em que ele aparece.</summary>
    public static void Preload(string? path)
    {
        lock (GifGate)
        {
            if (string.Equals(path, _gifPath, StringComparison.OrdinalIgnoreCase)) return;
            _gifPath = path;
            _gif = string.IsNullOrEmpty(path) || !File.Exists(path) ? null : Task.Run(() =>
            {
                try
                {
                    using var probe = Image.FromFile(path);
                    int height = 462 - 2 * GifMargin;
                    int width = (int)Math.Round(height * probe.Width / (double)probe.Height);
                    return (FrameAnimation?)FrameAnimation.LoadGif(path, width, height);
                }
                catch (Exception ex) when (ex is IOException or OutOfMemoryException or ArgumentException)
                {
                    Log.Warn($"Modo Mangá: não consegui abrir o GIF do Gear 5 ({ex.Message}).");
                    return null;
                }
            });
        }
    }

    private const int GifMargin = 30;

    /// <summary>Espera o GIF do Gear 5 terminar de carregar (para a prévia).</summary>
    public static void WaitForGif(int milliseconds) => _gif?.Wait(milliseconds);

    private static FrameAnimation? Gif()
    {
        var task = _gif;
        return task is { IsCompletedSuccessfully: true } ? task.Result : null;
    }

    /// <summary>Quais acontecimentos aparecem em cada tela.</summary>
    public static bool Affects(ScreenKind kind, MangaCueKind cue) => kind switch
    {
        ScreenKind.Thermalright => cue is not MangaCueKind.Copy,
        ScreenKind.ProFlow => cue is not (MangaCueKind.Tide or MangaCueKind.Farewell or MangaCueKind.TurnCut or MangaCueKind.TurnHalftone
            or MangaCueKind.Track or MangaCueKind.Chapter),
        _ => false,
    };

    private static bool IsPanelCue(MangaCueKind kind) => kind is MangaCueKind.Impact or MangaCueKind.Haki or MangaCueKind.Drums
        or MangaCueKind.Quake or MangaCueKind.Tide or MangaCueKind.Farewell or MangaCueKind.Dawn or MangaCueKind.TurnCut or MangaCueKind.TurnHalftone;

    /// <summary>Há algo para encenar nesta tela agora (então cada quadro passa por <see cref="Apply"/>, a ~30 por segundo).</summary>
    public bool Wants(MangaState state, long now) => Need(state, now) != MangaPace.None;

    /// <summary>
    /// De quantos quadros a tela precisa agora. Os momentos longos e lentos (maré, despedida, amanhecer, o Haki e os
    /// tambores, que duram o painel inteiro) seguem o ritmo da própria animação; só os golpes rápidos (impacto,
    /// carimbos, Gear 5, viradas, balões...) pedem 30 quadros por segundo. Assim o vídeo da Thermalright não é
    /// redesenhado a 30 por segundo durante metade do tempo.
    /// </summary>
    public MangaPace Need(MangaState state, long now)
    {
        var pace = MangaPace.None;
        foreach (var cue in state.Cues)
        {
            if (!cue.Alive(now) || !Affects(_kind, cue.Kind)) continue;
            if (cue.Kind is MangaCueKind.Tide or MangaCueKind.Farewell or MangaCueKind.Dawn or MangaCueKind.Haki or MangaCueKind.Drums)
            {
                pace = MangaPace.Slow;
                continue;
            }
            return MangaPace.Fast;
        }
        if (_kind == ScreenKind.ProFlow && (state.OnAir || state.Sleeping)) pace = MangaPace.Slow;
        return pace;
    }

    public void Apply(Bitmap canvas, long now, MangaState state)
    {
        int w = canvas.Width, h = canvas.Height;
        if (_scratch is null || _scratch.Width != w || _scratch.Height != h)
        {
            _scratch?.Dispose();
            _scratch = new Bitmap(w, h, PixelFormat.Format32bppPArgb);
        }
        Forget(state);

        var gear5 = state.Find(MangaCueKind.Gear5, now);
        double g5 = gear5?.Age(now) ?? -1;
        bool gifStage = Wide && gear5 is not null && g5 >= Gear5Times.Awaken && g5 < Gear5Times.Back;
        var live = state.Cues.Where(c => c.Alive(now) && Affects(_kind, c.Kind) && !(gifStage && IsPanelCue(c.Kind))).ToList();

        // 1. A imagem de base: no despertar do Gear 5, a Thermalright mostra o GIF.
        if (gifStage) Gear5Stage(canvas, g5);

        // 2. O que mexe na imagem.
        float scale = 1, dx = 0, dy = 0;
        bool invert = false;
        foreach (var cue in live) Camera(cue, cue.Age(now), ref scale, ref dx, ref dy, ref invert);
        foreach (var cue in live)
        {
            if (cue.Kind == MangaCueKind.Tide) MangaFx.Wave(canvas, _scratch, cue.Age(now), 5 * Env(cue, now, 1.2), 150, 3.4f);
            if (cue.Kind == MangaCueKind.TurnHalftone) MangaFx.HalftoneRipple(canvas, cue.Age(now) / cue.Seconds);
        }
        MangaFx.Punch(canvas, _scratch, scale, dx, dy);
        if (invert) MangaFx.ImpactFrame(canvas);

        using var g = Graphics.FromImage(canvas);
        g.CompositingMode = CompositingMode.SourceOver;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.InterpolationMode = InterpolationMode.Bilinear;
        g.PixelOffsetMode = PixelOffsetMode.Half;
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

        // 3. Tinta e luz.
        foreach (var cue in live) Ink(g, cue, cue.Age(now), w, h, invert);

        // 4. Letreiramento.
        foreach (var cue in live)
        {
            if (Wide) Sound(g, cue, cue.Age(now), w, h);
            else Speech(g, canvas, cue, cue.Age(now), w, h);
        }
        if (!Wide) Tags(g, state, now, w, h);
    }

    public void Dispose()
    {
        foreach (var art in _art.Values) art.Dispose();
        _art.Clear();
        _scratch?.Dispose();
        _onAirTag?.Dispose();
        _sleepBalloon?.Dispose();
    }

    private void Forget(MangaState state)
    {
        if (_art.Count == 0) return;
        foreach (var cue in _art.Keys.Where(c => !state.Cues.Contains(c)).ToList())
        {
            _art[cue].Dispose();
            _art.Remove(cue);
            if (Wide) cue.Picture?.Dispose();
        }
    }

    private CueArt Art(MangaCue cue)
    {
        if (!_art.TryGetValue(cue, out var art)) _art[cue] = art = new CueArt();
        return art;
    }

    /// <summary>Entra nos primeiros <paramref name="fade"/> s e sai nos últimos.</summary>
    private static float Env(MangaCue cue, long now, double fade)
    {
        double x = cue.Age(now);
        return (float)(Choreo.Smooth(0, fade, x) * Choreo.Smooth(0, fade, cue.Seconds - x));
    }

    private static double Decay(double x, double at, double tau) => x < at ? 0 : Math.Exp(-(x - at) / tau);

    // ---------------------------------------------------------------- câmera

    private void Camera(MangaCue cue, double x, ref float scale, ref float dx, ref float dy, ref bool invert)
    {
        float k = Wide ? 1 : 0.8f;
        double shake = 0;
        switch (cue.Kind)
        {
            case MangaCueKind.Impact:
                foreach (var (at, peak) in Choreo.Impact)
                {
                    scale += (float)(0.09 * k * peak / 0.62 * Decay(x, at, 0.16));
                    shake += 13 * k * peak / 0.62 * Decay(x, at, 0.22);
                }
                invert |= x >= Choreo.Impact[0].At && x < Choreo.Impact[0].At + FlashFrameMs / 1000.0;
                break;
            case MangaCueKind.Drums:
            {
                double beat = x % Choreo.DrumCycle;
                double strength = x < 5 ? 1 : 0.45;
                scale += (float)(0.045 * k * strength * Math.Exp(-beat / 0.12) * Choreo.Smooth(0, 0.3, x) * Choreo.Smooth(0, 0.6, cue.Seconds - x));
                break;
            }
            case MangaCueKind.Quake:
            {
                double amp = 10 * k * Choreo.Quake(x);
                dx += (float)(amp * (Choreo.SlowNoise(x, 5, cue.Seed) - 0.5) * 2);
                dy += (float)(amp * 0.6 * (Choreo.SlowNoise(x, 5, cue.Seed + 1) - 0.5) * 2);
                scale += (float)(0.02 * Choreo.Quake(x));
                break;
            }
            case MangaCueKind.Haki:
                foreach (var (at, peak, index) in Choreo.HakiCrackles(x))
                {
                    shake += 7 * k * peak * Decay(x, at, 0.1);
                    if (index == 0) invert |= x >= at && x < at + FlashFrameMs / 1000.0;
                }
                break;
            case MangaCueKind.Gear5:
                foreach (double beat in Gear5Times.Heartbeats) scale += (float)(0.06 * k * Decay(x, beat, 0.14));
                scale += (float)(0.1 * k * Decay(x, Gear5Times.Awaken, 0.2));
                shake += 15 * k * Decay(x, Gear5Times.Awaken, 0.3);
                invert |= x >= Gear5Times.Awaken && x < Gear5Times.Awaken + FlashFrameMs / 1000.0;
                if (x > Gear5Times.Awaken + 0.3 && x < Gear5Times.Back)
                    scale += (float)(0.035 * k * Math.Exp(-((x - Gear5Times.Awaken) % Choreo.DrumCycle) / 0.12));
                break;
            case MangaCueKind.Download:
            case MangaCueKind.Drive:
                scale += (float)(0.03 * k * Decay(x, 0.15, 0.15));
                break;
        }
        if (shake > 0.3)
        {
            long step = (long)(x * 25);
            dx += (float)(shake * (MangaFx.Hash(cue.Seed, step) - 0.5) * 2);
            dy += (float)(shake * 0.6 * (MangaFx.Hash(cue.Seed + 3, step) - 0.5) * 2);
        }
    }

    // ---------------------------------------------------------------- tinta e luz

    private void Ink(Graphics g, MangaCue cue, double x, int w, int h, bool inverted)
    {
        switch (cue.Kind)
        {
            case MangaCueKind.Impact:
            {
                double at = Choreo.Impact[0].At;
                if (x < at) break;
                float alpha = (float)(x < at + 0.75 ? 1 : 1 - (x - at - 0.75) / 0.4);
                if (alpha > 0) DrawLines(g, w, h, (int)(x * 12) % 3, alpha, inverted);
                break;
            }
            case MangaCueKind.Drums:
            {
                double beat = x % Choreo.DrumCycle;
                float alpha = (float)(0.85 * Math.Exp(-beat / 0.2) * Choreo.Smooth(0, 0.6, cue.Seconds - x) * (x < 5 ? 1 : 0.5));
                if (alpha > 0.03) DrawLines(g, w, h, (int)(x / Choreo.DrumCycle) % 3, alpha * (Wide ? 1 : 0.7f), false);
                break;
            }
            case MangaCueKind.Quake when x >= Choreo.QuakeStart && x < Choreo.QuakeEnd + 0.35:
            {
                var art = Art(cue);
                // Na Thermalright a rachadura nasce de um lado (o rosto do vídeo fica livre); na Pro Flow, perto do meio.
                float originX = Wide ? (MangaFx.Hash(cue.Seed, 1) < 0.5 ? 0.2f : 0.8f) : (float)(0.35 + 0.3 * MangaFx.Hash(cue.Seed, (int)_kind));
                art.Crack ??= MangaFx.MakeCrack(cue.Seed + (int)_kind, new PointF(w * originX, h * (float)(0.4 + 0.2 * MangaFx.Hash(cue.Seed, 9 + (int)_kind))),
                    Wide ? w * 0.3f : w * 0.55f);
                MangaFx.DrawCrack(g, art.Crack, art.Crack.Length * (float)Choreo.Smooth(Choreo.QuakeStart, Choreo.QuakeStart + 0.8, x), Wide ? 1.1f : 0.9f);
                break;
            }
            case MangaCueKind.Haki:
            {
                float env = (float)(Choreo.Smooth(0, 0.3, x) * Choreo.Smooth(0, 0.8, cue.Seconds - x));
                MangaFx.Wash(g, w, h, 0.3f * env, vignette: true, color: Color.Black);
                foreach (var (at, peak, index) in Choreo.HakiCrackles(x))
                {
                    if (x - at > 0.13) continue;
                    for (int b = 0; b < 3; b++)
                    {
                        int seed = cue.Seed + index * 17 + b * 101 + (int)_kind;
                        var from = EdgePoint(seed, w, h);
                        var to = new PointF(from.X + (w / 2f - from.X) * (float)(0.45 + 0.35 * MangaFx.Hash(seed, 3)), from.Y + (h / 2f - from.Y) * (float)(0.5 + 0.4 * MangaFx.Hash(seed, 4)));
                        MangaFx.DrawBolt(g, MangaFx.MakeBolt(seed, from, to), (Wide ? 11 : 8) * (float)(0.7 + peak));
                    }
                }
                break;
            }
            case MangaCueKind.Farewell:
            {
                // A despedida desbota para o branco, devagar.
                float env = (float)(Choreo.Smooth(0, 2.5, x) * Choreo.Smooth(0, 1, cue.Seconds - x));
                MangaFx.Wash(g, w, h, 0.28f * env);
                MangaFx.Wash(g, w, h, 0.6f * env, vignette: true);
                break;
            }
            case MangaCueKind.Dawn:
            {
                float env = (float)(Choreo.Smooth(0.3, 3, x) * Choreo.Smooth(0, 1, cue.Seconds - x));
                var rays = MangaFx.Rays(w, h);
                var state = g.Save();
                g.TranslateTransform(w / 2f, -h * 0.9f);
                g.RotateTransform((float)(3 * Math.Sin(x * 0.5)));
                g.TranslateTransform(-w / 2f, h * 0.9f);
                MangaFx.DrawSprite(g, rays, new PointF(w / 2f, h / 2f), 1, 0, 0.75f * env);
                g.Restore(state);
                MangaFx.Wash(g, w, h, 0.15f * env);
                break;
            }
            case MangaCueKind.TurnCut:
                MangaFx.Gutter(g, w, h, x / cue.Seconds, Wide ? 1.2f : 1);
                break;
            case MangaCueKind.Print:
                MangaFx.Wash(g, w, h, (float)Math.Exp(-x / 0.13));
                break;
            case MangaCueKind.Welcome when Wide:
                MangaFx.Gutter(g, w, h, x / 0.7, 1.2f);
                break;
            case MangaCueKind.Mute when Wide:
                MangaFx.Wash(g, w, h, 0.3f * (float)(Choreo.Smooth(0, 0.3, x) * Choreo.Smooth(0, 0.5, cue.Seconds - x)));
                break;
            case MangaCueKind.Gear5:
            {
                foreach (double beat in Gear5Times.Heartbeats)
                    MangaFx.Wash(g, w, h, 0.55f * (float)Decay(x, beat, 0.2), vignette: true);
                if (!Wide && x >= Gear5Times.Awaken && x < Gear5Times.Awaken + 1.0)
                    DrawLines(g, w, h, (int)(x * 12) % 3, (float)(1 - (x - Gear5Times.Awaken)), inverted);
                // Volta ao vídeo no meio de um véu branco.
                double wash = x < Gear5Times.Back ? Choreo.Smooth(Gear5Times.WashOut, Gear5Times.Back, x) : 1 - Choreo.Smooth(Gear5Times.Back, Gear5Times.Seconds, x);
                if (Wide) MangaFx.Wash(g, w, h, (float)wash);
                break;
            }
        }
    }

    private static void DrawLines(Graphics g, int w, int h, int variant, float alpha, bool white)
    {
        var lines = MangaFx.SpeedLines(w, h, variant, white);
        MangaFx.DrawSprite(g, lines, new PointF(w / 2f, h / 2f), 1, 0, alpha);
    }

    private static PointF EdgePoint(int seed, int w, int h)
    {
        double side = MangaFx.Hash(seed, 1), along = MangaFx.Hash(seed, 2);
        return side switch
        {
            < 0.3 => new PointF((float)(along * w), -4),
            < 0.6 => new PointF((float)(along * w), h + 4),
            < 0.8 => new PointF(-4, (float)(along * h)),
            _ => new PointF(w + 4, (float)(along * h)),
        };
    }

    // ---------------------------------------------------------------- Thermalright: o som e as manchetes

    private void Sound(Graphics g, MangaCue cue, double x, int w, int h)
    {
        float side = cue.Seed % 2 == 0 ? 0.2f : 0.8f;
        float tilt = side < 0.5f ? -8 : 8;
        switch (cue.Kind)
        {
            case MangaCueKind.Impact:
                MangaFx.Stamp(g, MangaFx.Sfx("ドン!!", 150), new PointF(w * side, h * 0.52f), x - Choreo.Impact[0].At, 0.95, 1, tilt, cue.Seed);
                break;
            case MangaCueKind.Drums:
            {
                double beat = x % Choreo.DrumCycle;
                float bump = 1 + 0.08f * (float)Math.Exp(-beat / 0.12);
                MangaFx.Stamp(g, MangaFx.Sfx("ドンドットット", 84), new PointF(w * 0.5f, h * 0.2f), x - 0.2, Math.Min(4, cue.Seconds - 0.5), bump, -3, cue.Seed);
                break;
            }
            case MangaCueKind.Quake:
                MangaFx.Stamp(g, MangaFx.Sfx("グラグラ", 120), new PointF(w * (MangaFx.Hash(cue.Seed, 1) < 0.5 ? 0.62f : 0.38f), h * 0.24f), x - Choreo.QuakeStart, 3.0, 1, 6, cue.Seed);
                break;
            case MangaCueKind.Haki:
                MangaFx.Stamp(g, MangaFx.Sfx("覇", 270, 1, outline: 0.32f), new PointF(w * (side < 0.5f ? 0.13f : 0.87f), h * 0.52f), x - 0.1, 1.5, 1, tilt * 0.5f, cue.Seed);
                break;
            case MangaCueKind.Tide:
                MangaFx.Stamp(g, MangaFx.Sfx("ザザーン", 76), new PointF(w * 0.16f, h * 0.76f), x - 0.6, 2.6, 1, -5, cue.Seed);
                break;
            case MangaCueKind.Download:
                Headline(g, cue, x, w, h);
                MangaFx.Stamp(g, MangaFx.Sfx("ドン!", 116), new PointF(w * 0.82f, h * 0.5f), x - 0.15, 1.2, 1, 8, cue.Seed);
                break;
            case MangaCueKind.Drive:
                Headline(g, cue, x, w, h);
                MangaFx.Stamp(g, MangaFx.Sfx("ガチャ", 104), new PointF(w * 0.82f, h * 0.5f), x - 0.15, 1.3, 1, 7, cue.Seed);
                break;
            case MangaCueKind.Ring:
            {
                Headline(g, cue, x, w, h);
                float rock = (float)(7 * Math.Sin(2 * Math.PI * x * 3));
                if (x < 2.3) MangaFx.Stamp(g, MangaFx.Sfx("プルプルプル", 80), new PointF(w * 0.78f, h * 0.48f), x, 2.05, 1, rock, cue.Seed);
                MangaFx.Stamp(g, MangaFx.Sfx("ガチャ", 96), new PointF(w * 0.8f, h * 0.5f), x - 2.35, 0.5, 1, 8, cue.Seed);
                break;
            }
            case MangaCueKind.Track:
            {
                Headline(g, cue, x, w, h);
                var note = MangaFx.Sfx("♪", 70, 3);
                for (int i = 0; i < 4; i++)
                {
                    double p = (x * 0.45 + i / 4.0) % 1;
                    float alpha = (float)(Math.Sin(Math.PI * p) * Choreo.Smooth(0, 0.6, cue.Seconds - x) * Choreo.Smooth(0, 0.4, x));
                    var at = new PointF(w * (0.7f + 0.06f * i) + (float)(14 * Math.Sin(x * 2 + i)), h * (1.05f - 1.05f * (float)p));
                    MangaFx.DrawSprite(g, note, at, 0.8f + 0.25f * (i % 2), (float)(12 * Math.Sin(x * 3 + i)), alpha);
                }
                break;
            }
            case MangaCueKind.Mute:
                MangaFx.Stamp(g, MangaFx.Sfx("シーン…", 128), new PointF(w * 0.5f, h * 0.5f), x - 0.1, cue.Seconds - 0.6, 1, -2, cue.Seed);
                break;
            case MangaCueKind.Welcome:
                MangaFx.Stamp(g, MangaFx.Sfx("おかえり!", 118), new PointF(w * 0.24f, h * 0.5f), x - 0.35, 2.2, 1, -7, cue.Seed);
                break;
            case MangaCueKind.Menace:
                Menace(g, x, cue.Seconds, w, h, cue.Seed, 6, 92);
                Headline(g, cue, x, w, h);
                break;
            case MangaCueKind.Chapter:
                Chapter(g, cue, x, w, h);
                break;
            case MangaCueKind.Print:
                PrintPanel(g, cue, x, w, h);
                break;
            case MangaCueKind.Gear5:
                foreach (double beat in Gear5Times.Heartbeats)
                    MangaFx.Stamp(g, MangaFx.Sfx("ドクン", 112), new PointF(w * 0.5f, h * 0.5f), x - beat, 0.42, 1, beat == 0 ? -6 : 6, cue.Seed + (int)(beat * 10));
                MangaFx.Stamp(g, MangaFx.Sfx("ドン!!", 160), new PointF(w * 0.5f, h * 0.5f), x - Gear5Times.Awaken, 0.6, 1, -4, cue.Seed);
                break;
        }
    }

    /// <summary>Caixa de narração no canto de cima à esquerda: entra deslizando, fica e sai subindo.</summary>
    private void Headline(Graphics g, MangaCue cue, double x, int w, int h)
    {
        if (cue.Text is null) return;
        var art = Art(cue);
        art.Sprite ??= MangaFx.RenderNarration(cue.Text, cue.Detail, 1.05f, (int)(w * 0.42));
        var sprite = art.Sprite;
        double enter = MangaFx.EaseOutCubic(x / 0.35);
        double leave = Choreo.Smooth(cue.Seconds - 0.35, cue.Seconds, x);
        float left = 34 - (float)((1 - enter) * (sprite.Width + 40));
        float top = 30 - (float)(leave * (sprite.Height + 40));
        g.DrawImage(sprite, left, top, sprite.Width, sprite.Height);
    }

    /// <summary>A ameaça (ゴゴゴゴ): colunas de "ゴ" subindo e balançando pelos lados.</summary>
    private static void Menace(Graphics g, double x, double seconds, int w, int h, int seed, int count, float em)
    {
        var glyph = MangaFx.Sfx("ゴ", em, 4);
        float env = (float)(Choreo.Smooth(0, 0.5, x) * Choreo.Smooth(0, 0.8, seconds - x));
        for (int i = 0; i < count; i++)
        {
            bool right = i % 2 == 1;
            double p = (x * 0.32 + i / (double)count + MangaFx.Hash(seed, i) * 0.1) % 1;
            float alpha = env * (float)Math.Sin(Math.PI * p);
            float cx = w * (right ? 0.93f : 0.07f) + (float)((MangaFx.Hash(seed, i + 20) - 0.5) * w * 0.06);
            var at = new PointF(cx + (float)(6 * Math.Sin(x * 4 + i)), h * (1.1f - 1.2f * (float)p));
            MangaFx.DrawSprite(g, glyph, at, 0.85f + 0.3f * (float)MangaFx.Hash(seed, i + 40), (float)((right ? 10 : -10) + 8 * Math.Sin(x * 5 + i * 1.7)), alpha);
        }
    }

    /// <summary>Página de título do capítulo, entrando pela direita por cima do vídeo.</summary>
    private void Chapter(Graphics g, MangaCue cue, double x, int w, int h)
    {
        var art = Art(cue);
        int cardWidth = (int)(w * 0.64);
        art.Sprite ??= MangaFx.RenderChapterCard(cardWidth, h, cue.Seed > 0 ? cue.Seed : DateTime.Now.Hour, cue.Text ?? "", cue.Detail ?? "");
        double enter = MangaFx.EaseOutCubic(x / 0.5);
        double leave = Choreo.Smooth(cue.Seconds - 0.45, cue.Seconds, x);
        float left = w - (float)(cardWidth * enter) + (float)(leave * (cardWidth + 40));
        g.DrawImage(art.Sprite, left, 0, cardWidth, h);
    }

    /// <summary>O print, virado quadro de mangá, entra pela direita meio torto; sem o arquivo, só a manchete.</summary>
    private void PrintPanel(Graphics g, MangaCue cue, double x, int w, int h)
    {
        MangaFx.Stamp(g, MangaFx.Sfx("カシャ!", 104), new PointF(w * 0.5f, h * 0.3f), x - 0.05, 1.1, 1, -9, cue.Seed);
        var picture = cue.Picture;
        if (picture is null)
        {
            if (x > 0.9) Headline(g, cue, x, w, h);
            return;
        }
        double enter = MangaFx.EaseOutBack(Math.Clamp((x - 0.2) / 0.45, 0, 1));
        double leave = Choreo.Smooth(cue.Seconds - 0.4, cue.Seconds, x);
        float cx = w * 0.79f + (float)((1 - enter) * w * 0.4) + (float)(leave * w * 0.4);
        MangaFx.DrawSprite(g, picture, new PointF(cx, h * 0.5f), Math.Min(1, (h - 40f) / picture.Height), -3);
    }

    // ---------------------------------------------------------------- Thermalright: o despertar do Gear 5

    /// <summary>
    /// O GIF do Nika no meio de uma página branca com linhas de foco que pulsam nos Tambores da Libertação, "GEAR 5" de
    /// um lado e ドンドットット do outro.
    /// </summary>
    private void Gear5Stage(Bitmap canvas, double x)
    {
        int w = canvas.Width, h = canvas.Height;
        double s = x - Gear5Times.Awaken;
        double beat = s % Choreo.DrumCycle;
        float drum = (float)Math.Exp(-beat / 0.16);
        using var g = Graphics.FromImage(canvas);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.InterpolationMode = InterpolationMode.Bilinear;
        g.PixelOffsetMode = PixelOffsetMode.Half;
        g.Clear(Color.White);
        DrawLines(g, w, h, (int)(s / Choreo.DrumCycle) % 3, 0.45f + 0.55f * drum, false);

        var gif = Gif();
        float gifWidth = gif?.Width ?? (h - 2 * GifMargin) * 1.77f, gifHeight = gif?.Height ?? h - 2 * GifMargin;
        var frame = new RectangleF((w - gifWidth) / 2, (h - gifHeight) / 2, gifWidth, gifHeight);
        g.FillRectangle(Brushes.Black, frame.X + 9, frame.Y + 9, frame.Width, frame.Height);
        if (gif is not null)
        {
            var state = g.Save();
            g.TranslateTransform(frame.X, frame.Y);
            gif.DrawFrame(canvas, g, FrameAt(gif, s));
            g.Restore(state);
        }
        else
        {
            g.FillRectangle(Brushes.White, frame);
        }
        using (var border = new Pen(Color.Black, 7))
            g.DrawRectangle(border, frame.X, frame.Y, frame.Width, frame.Height);

        float side = (w - gifWidth) / 2;
        var title = MangaFx.Sfx("GEAR 5", 118, 21);
        float fit = Math.Min(1, (side - 50) / title.Width);
        MangaFx.DrawSprite(g, title, new PointF(side / 2, h * 0.42f), fit * (1 + 0.07f * drum), -7);
        var nika = MangaFx.Sfx("太陽の神ニカ", 40, 22);
        MangaFx.DrawSprite(g, nika, new PointF(side / 2, h * 0.72f), Math.Min(1, (side - 60) / nika.Width), -4);
        var drums = MangaFx.Sfx("ドンドットット", 52, 23, vertical: true);
        MangaFx.DrawSprite(g, drums, new PointF(w - side / 2, h / 2f), Math.Min(1, (h - 30f) / drums.Height) * (1 + 0.06f * drum), 4);
    }

    private static int FrameAt(FrameAnimation gif, double seconds)
    {
        int total = gif.Delays.Sum();
        if (total <= 0) return 0;
        long ms = (long)(seconds * 1000) % total;
        for (int i = 0; i < gif.Delays.Length; i++)
        {
            ms -= gif.Delays[i];
            if (ms < 0) return i;
        }
        return gif.Delays.Length - 1;
    }

    // ---------------------------------------------------------------- Pro Flow: os balões

    private void Speech(Graphics g, Bitmap canvas, MangaCue cue, double x, int w, int h)
    {
        double remaining = cue.Seconds - x;
        switch (cue.Kind)
        {
            case MangaCueKind.Copy:
                Balloon(g, canvas, cue, x, remaining, "COPIADO!", BalloonShape.Round, 46);
                break;
            case MangaCueKind.Download:
                Balloon(g, canvas, cue, x - 0.2, remaining, "DOWNLOAD\nCONCLUÍDO!", BalloonShape.Shout, 44);
                break;
            case MangaCueKind.Drive:
                Balloon(g, canvas, cue, x - 0.2, remaining, "TESOURO\nENCONTRADO!", BalloonShape.Shout, 44);
                break;
            case MangaCueKind.Ring:
                Balloon(g, canvas, cue, x - 0.4, remaining, "ALÔ, ALÔ?", BalloonShape.Round, 46);
                break;
            case MangaCueKind.Welcome:
                Balloon(g, canvas, cue, x - 0.3, remaining, "BEM-VINDO\nDE VOLTA!", BalloonShape.Shout, 46);
                break;
            case MangaCueKind.Mute:
                Balloon(g, canvas, cue, x - 0.2, remaining, "…", BalloonShape.Thought, 70);
                break;
            case MangaCueKind.Menace:
                Menace(g, x, cue.Seconds, w, h, cue.Seed + 5, 4, 70);
                break;
        }
    }

    /// <summary>Balão que estoura para dentro (0,22 s), boia de leve e some encolhendo nos últimos 0,2 s.</summary>
    private void Balloon(Graphics g, Bitmap canvas, MangaCue cue, double x, double remaining, string text, BalloonShape shape, float em)
    {
        if (x < 0) return;
        var art = Art(cue);
        art.Sprite ??= MangaFx.RenderBalloon(text, em, shape, cue.Seed, tailLeft: cue.Seed % 2 == 0);
        if (!art.Placed)
        {
            art.Place = FindCalm(canvas, art.Sprite.Size);
            art.Placed = true;
        }
        double pop = MangaFx.EaseOutBack(x / 0.22);
        double leave = 1 - Choreo.Smooth(0.05, 0.25, remaining);
        float scale = (float)((0.4 + 0.6 * pop) * (1 - 0.15 * leave));
        var at = new PointF(art.Place.X, art.Place.Y + (float)(3 * Math.Sin(2 * Math.PI * x / 2.2)));
        MangaFx.DrawSprite(g, art.Sprite, at, scale, 0, (float)(1 - leave));
    }

    /// <summary>
    /// Onde o balão incomoda menos: a janela do tamanho dele mais clara e mais lisa do painel (papel em branco), longe da
    /// caixa de narração do canto de cima à esquerda, com uma leve preferência pelo alto (onde os balões ficam no mangá).
    /// </summary>
    private static unsafe PointF FindCalm(Bitmap canvas, Size size)
    {
        const int Cell = 24;
        int w = canvas.Width, h = canvas.Height, cols = w / Cell, rows = h / Cell;
        var mean = new double[rows, cols];
        var rough = new double[rows, cols];
        var data = canvas.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);
        try
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    double sum = 0, squares = 0;
                    int n = 0;
                    for (int y = r * Cell; y < (r + 1) * Cell; y += 3)
                    {
                        uint* row = (uint*)((byte*)data.Scan0 + (long)y * data.Stride);
                        for (int x = c * Cell; x < (c + 1) * Cell; x += 3)
                        {
                            uint p = row[x];
                            double v = (((p >> 16) & 0xFF) * 77 + ((p >> 8) & 0xFF) * 150 + (p & 0xFF) * 29) >> 8;
                            sum += v;
                            squares += v * v;
                            n++;
                        }
                    }
                    mean[r, c] = sum / n;
                    rough[r, c] = Math.Sqrt(Math.Max(0, squares / n - mean[r, c] * mean[r, c]));
                }
            }
        }
        finally
        {
            canvas.UnlockBits(data);
        }

        int bw = Math.Min(cols, (int)Math.Ceiling(size.Width / (double)Cell)), bh = Math.Min(rows, (int)Math.Ceiling(size.Height / (double)Cell));
        var avoid = new Rectangle(0, 0, 440, 140); // caixa de narração do One Piece
        double best = double.MinValue;
        var place = new PointF(w * 0.7f, h * 0.3f);
        for (int r = 0; r + bh <= rows; r++)
        {
            for (int c = 0; c + bw <= cols; c++)
            {
                var window = new Rectangle(c * Cell, r * Cell, bw * Cell, bh * Cell);
                if (window.IntersectsWith(avoid)) continue;
                double light = 0, flat = 0;
                for (int i = r; i < r + bh; i++)
                {
                    for (int j = c; j < c + bw; j++)
                    {
                        light += mean[i, j];
                        flat += rough[i, j];
                    }
                }
                int n = bw * bh;
                double score = light / n - 1.3 * flat / n - 30.0 * r / rows;
                if (score > best)
                {
                    best = score;
                    place = new PointF(window.X + window.Width / 2f, window.Y + window.Height / 2f);
                }
            }
        }
        float margin = 10;
        return new PointF(Math.Clamp(place.X, size.Width / 2f + margin, w - size.Width / 2f - margin), Math.Clamp(place.Y, size.Height / 2f + margin, h - size.Height / 2f - margin));
    }

    /// <summary>Etiquetas que ficam enquanto dura o estado: "● NO AR" (microfone/câmera) e o "Zzz…" do PC dormindo.</summary>
    private void Tags(Graphics g, MangaState state, long now, int w, int h)
    {
        if (state.OnAir)
        {
            _onAirTag ??= MangaFx.RenderTag("● NO AR", 0.9f);
            double x = (now - state.OnAirSince) / 1000.0;
            float slide = (float)MangaFx.EaseOutCubic(x / 0.35);
            g.DrawImage(_onAirTag, w - 14 - _onAirTag.Width * slide, 14, _onAirTag.Width, _onAirTag.Height);
        }
        if (state.Sleeping)
        {
            _sleepBalloon ??= MangaFx.RenderBalloon("Zzz…", 44, BalloonShape.Thought, 7, tailLeft: false);
            double x = (now - state.SleepingSince) / 1000.0;
            float alpha = (float)Choreo.Smooth(1, 2.5, x);
            var at = new PointF(w - _sleepBalloon.Width / 2f - 24, h - _sleepBalloon.Height / 2f - 20 + (float)(5 * Math.Sin(2 * Math.PI * x / 3)));
            MangaFx.DrawSprite(g, _sleepBalloon, at, 1 + 0.03f * (float)Math.Sin(2 * Math.PI * x / 3), 0, alpha);
        }
    }
}
