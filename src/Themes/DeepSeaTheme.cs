using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using DS339Monitor.App;
using DS339Monitor.Lighting;
using DS339Monitor.World;
using static DS339Monitor.Lighting.Choreo;

namespace DS339Monitor.Themes;

/// <summary>
/// "Fundo do Mar" (Pro Flow, no meio do gabinete): a água logo abaixo do Sunny da Thermalright. O casco do navio
/// visto de baixo corta a superfície, a luz do sol entra em fachos (inclinados pela posição real do sol), um cardume
/// passa (mais rápido com som), bolhas sobem do fundo conforme o calor da placa de vídeo e, embaixo, a luz das
/// profundezas — a mesma conta da luz da placa — continua no LED. O que cai da Thermalright atravessa esta tela; o que
/// sobe da placa também (<see cref="WorldDirector"/>).
/// </summary>
internal static class DeepSeaTheme
{
    public const string Name = "Fundo do Mar";

    public static Theme Create(string style, int width, int height) =>
        GifTheme.WithOverlay(Name, new DeepSeaAnimation(width, height), style, width, height);
}

internal sealed class DeepSeaAnimation : IAnimation
{
    private const int FrameMs = 33;
    private const int CalmFrameMs = 100;
    private const long LatencyMs = 35; // desenho + JPEG + envio serial: o quadro aparece um pouco depois

    private Bitmap? _water;
    private int _waterKey = -1;

    public DeepSeaAnimation(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public int Count => 108_000;
    public int Width { get; }
    public int Height { get; }
    public bool CacheFrames => false;
    public int DelayMs(int index) => SceneDirector.Current is { InGame: true } or { Sleeping: true } ? CalmFrameMs : FrameMs;
    public long FrameKey(int index) => index;

    public void DrawFrame(Bitmap canvas, Graphics g, int index) => Draw(g, Clock.Ms + LatencyMs, SceneDirector.Current, WorldDirector.Current);

    public void Dispose() => _water?.Dispose();

    public void Draw(Graphics g, long now, StageState stage, WorldState world)
    {
        var saved = g.Save();
        g.CompositingMode = CompositingMode.SourceOver;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.InterpolationMode = InterpolationMode.Bilinear;
        g.PixelOffsetMode = PixelOffsetMode.Half;
        try
        {
            double t = now / 1000.0;
            double night = stage.Sleeping ? Math.Max(1 - stage.Daylight, 0.85) : 1 - stage.Daylight;
            var screen = new RectangleF(0, 0, Width, Height);
            var k = world.Layout.PxPerMm(ScreenKind.ProFlow);

            DrawWater(g, night);
            WorldArt.LightShafts(g, screen, t, (float)((0.5 - stage.SunArc) * 220), (float)(0.9 * (1 - night)));
            Plankton(g, t, (float)Math.Clamp((night - 0.4) / 0.4, 0, 1));
            Fish(g, t, stage, k.Width, night);
            Bubbles(g, t, stage, k.Width);
            BottomGlow(g, now, stage, world);
            WorldLayer.Draw(g, ScreenKind.ProFlow, world, now, -1);
            WorldLayer.Draw(g, ScreenKind.ProFlow, world, now, 0);
            Ceiling(g, t, stage, night);
            Keel(g, now, world, k);
            if (world.OnAir) DenDen(g, now - world.OnAirSince, k.Width);
            WorldLayer.Draw(g, ScreenKind.ProFlow, world, now, 1);
        }
        finally
        {
            g.Restore(saved);
        }
    }

    /// <summary>Água: clara junto à superfície, escura embaixo (cache por nível de luz).</summary>
    private void DrawWater(Graphics g, double night)
    {
        int key = (int)Math.Round(night * 40);
        if (key != _waterKey || _water is null)
        {
            _waterKey = key;
            _water?.Dispose();
            _water = new Bitmap(Width, Height, PixelFormat.Format32bppPArgb);
            using var wg = Graphics.FromImage(_water);
            double n = key / 40.0;
            int top = (int)Lerp(176, 34, n), mid = (int)Lerp(104, 18, n), bottom = (int)Lerp(46, 8, n);
            using var brush = new LinearGradientBrush(new Rectangle(0, 0, Width, Height), Color.Black, Color.Black, LinearGradientMode.Vertical)
            {
                InterpolationColors = new ColorBlend
                {
                    Colors = new[] { Gray(top, 1.04), Gray(mid, 1.06), Gray(bottom, 1.1) },
                    Positions = new[] { 0f, 0.45f, 1f },
                },
            };
            wg.FillRectangle(brush, 0, 0, Width, Height);
        }
        g.DrawImageUnscaled(_water, 0, 0);
    }

    /// <summary>Cinza levemente azulado (o mar de mangá impresso em papel).</summary>
    private static Color Gray(int v, double blue) => Color.FromArgb(Math.Clamp(v - 4, 0, 255), Math.Clamp(v, 0, 255), Math.Clamp((int)(v * blue) + 2, 0, 255));

    /// <summary>A face de baixo das ondas: a linha da superfície ondulando e o brilho das cáusticas logo abaixo.</summary>
    private void Ceiling(Graphics g, double t, StageState stage, double night)
    {
        double amplitude = stage.Muted ? 2 : 5 + 10 * stage.Audio;
        float offset = (float)(t * 57);
        var line = new PointF[Width / 8 + 2];
        for (int i = 0; i < line.Length; i++)
        {
            float x = i * 8;
            double phase = (x + offset) / 140.0;
            line[i] = new PointF(x, 22 + (float)(amplitude * (Math.Sin(phase) + 0.4 * Math.Sin(2.3 * phase + 1))));
        }
        int light = (int)Lerp(235, 90, night);
        using (var path = new GraphicsPath())
        {
            path.AddLine(0, -2, Width, -2);
            path.AddLine(Width, -2, Width, line[^1].Y);
            for (int i = line.Length - 1; i >= 0; i--) path.AddLine(line[Math.Min(i + 1, line.Length - 1)], line[i]);
            path.CloseFigure();
            using var sky = new SolidBrush(Color.FromArgb(200, light, light, light));
            g.FillPath(sky, path);
        }
        using var ink = new Pen(Color.FromArgb(220, 20, 20, 24), 3);
        g.DrawLines(ink, line);
        using var caustic = new Pen(Color.FromArgb((int)(110 * (1 - night)), 255, 255, 255), 2);
        for (int row = 0; row < 3; row++)
        {
            var wave = new PointF[Width / 16 + 2];
            for (int i = 0; i < wave.Length; i++)
            {
                float x = i * 16;
                wave[i] = new PointF(x, 46 + row * 22 + (float)(5 * Math.Sin((x + offset * (0.6 + row * 0.2)) / (60 + row * 14) + row * 2 + t * 0.8)));
            }
            g.DrawCurve(caustic, wave, 0.5f);
        }
    }

    /// <summary>O casco do Sunny visto de baixo, cortando a superfície no mesmo x em que ele navega na Thermalright.</summary>
    private void Keel(Graphics g, long now, WorldState world, SizeF k)
    {
        float lift = 0;
        double since = (now - world.ShipSince) / 1000.0;
        switch (world.Ship)
        {
            case ShipMode.Launch when since > 2.3:
            case ShipMode.Away:
                return;
            case ShipMode.Return:
                if (since < 5.9) return;
                lift = (float)(-30 * Math.Exp(-(since - 5.9) / 0.5)); // acabou de cair: afunda e volta
                break;
        }
        var center = world.Layout.ToPx(ScreenKind.ProFlow, new PointF(WorldState.ShipX(now), world.Layout.Middle.Y));
        float w = 44 * k.Width, h = 46 - lift;
        float bob = (float)(4 * Math.Sin(now / 1000.0 * 1.1));
        using var hull = new SolidBrush(Color.FromArgb(240, 26, 26, 30));
        using var rim = new Pen(Color.FromArgb(190, 225, 228, 232), 3);
        using var plank = new Pen(Color.FromArgb(90, 210, 214, 220), 2);
        var body = new RectangleF(center.X - w / 2, -h + bob, w, h * 2);
        g.FillEllipse(hull, body);
        // Tábuas do casco vistas de baixo (arcos concêntricos) e o brilho da água na borda.
        for (int i = 1; i <= 4; i++)
        {
            float ix = w * 0.09f * i, iy = h * 0.16f * i;
            if (body.Width - 2 * ix < 4 || body.Height - 2 * iy < 4) break;
            g.DrawArc(plank, body.X + ix, body.Y + iy, body.Width - 2 * ix, body.Height - 2 * iy, 25, 130);
        }
        g.DrawArc(rim, body, 18, 144);
        // Quilha (a linha do meio, de proa a popa) e o leme na popa (à esquerda: o Sunny navega para a direita).
        using var keel = new Pen(Color.FromArgb(250, 16, 16, 18), 7) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        g.DrawLine(keel, center.X - w * 0.42f, h * 0.55f + bob, center.X + w * 0.38f, h * 0.62f + bob);
        g.FillPolygon(hull, new[] { new PointF(center.X - w * 0.46f, h * 0.35f + bob), new PointF(center.X - w * 0.36f, h * 0.45f + bob), new PointF(center.X - w * 0.43f, h + 16 + bob) });
    }

    private static void Plankton(Graphics g, double t, float alpha)
    {
        if (alpha <= 0.02f) return;
        using var dot = new SolidBrush(Color.FromArgb((int)(200 * alpha), 230, 240, 235));
        for (int i = 0; i < 60; i++)
        {
            float x = (float)((Hash(i + 900) * 960 + t * (4 + 6 * Hash(i + 910))) % 960);
            float y = (float)(80 + Hash(i + 920) * 390 + 10 * Math.Sin(t * 0.4 + i));
            float r = (float)(1 + 1.6 * Hash(i + 930)) * (float)(0.6 + 0.4 * SlowNoise(t, 0.7, i));
            g.FillEllipse(dot, x - r, y - r, 2 * r, 2 * r);
        }
    }

    /// <summary>Cardume: 9 peixes seguindo um centro que passeia; mais rápido com som.</summary>
    private static void Fish(Graphics g, double t, StageState stage, float k, double night)
    {
        if (stage.Sleeping) return;
        double speed = 1 + 1.5 * stage.Audio;
        double lap = 28 / speed; // segundos para atravessar
        double u = t % (lap * 2) / lap;
        int facing = u < 1 ? 1 : -1;
        double p = u < 1 ? u : 2 - u;
        float cx = (float)(-120 + p * (960 + 240)), cy = (float)(250 + 70 * Math.Sin(t * 0.35));
        float alpha = (float)(0.95 - 0.5 * night);
        for (int i = 0; i < 9; i++)
        {
            float dx = (float)((Hash(i + 500) - 0.5) * 170 + 10 * Math.Sin(t * 1.3 + i));
            float dy = (float)((Hash(i + 510) - 0.5) * 110 + 8 * Math.Cos(t * 1.1 + i * 2));
            WorldArt.Fish(g, new PointF(cx + dx * facing, cy + dy), k * (0.7f + 0.3f * (float)Hash(i + 520)), facing, t * 9 * speed + i, alpha);
        }
    }

    /// <summary>Bolhas subindo do fundo: quantas e quão rápido dependem do calor e do uso da placa de vídeo.</summary>
    private static void Bubbles(Graphics g, double t, StageState stage, float k)
    {
        double heat = float.IsNaN(stage.GpuTemp) ? 0.2 : Smooth(38, 80, stage.GpuTemp);
        int count = 5 + (int)(20 * heat);
        for (int i = 0; i < count; i++)
        {
            double rise = 0.08 + 0.1 * Hash(i + 600) + 0.12 * heat;
            double life = (t * rise + Hash(i + 610)) % 1;
            float x = (float)(960 * (0.15 + 0.7 * Hash(i + 620)) + 12 * Math.Sin(t * 2 + i));
            float y = (float)(500 - life * 520);
            float r = (float)(0.35 + 0.5 * Hash(i + 630)) * k * (float)(0.5 + 0.5 * life);
            WorldArt.Bubble(g, new PointF(x, y), r, null, (float)(0.9 * Math.Min(1, (1 - life) * 4)));
        }
    }

    /// <summary>A luz das profundezas subindo da borda de baixo — o mesmo nível e tom que a placa de vídeo mostra.</summary>
    private void BottomGlow(Graphics g, long now, StageState stage, WorldState world)
    {
        var (level, warmth) = WorldLight.At(now, stage, world);
        int alpha = (int)(230 * Math.Clamp(level, 0, 1));
        var color = warmth > 0 ? Color.FromArgb(alpha, 255, (int)(255 - 40 * warmth), (int)(255 - 90 * warmth)) : Color.FromArgb(alpha, (int)(255 + 40 * warmth), (int)(255 + 15 * warmth), 255);
        float h = 170;
        using var glow = new LinearGradientBrush(new RectangleF(0, Height - h, Width, h + 1), Color.FromArgb(0, color), color, LinearGradientMode.Vertical);
        g.FillRectangle(glow, 0, Height - h, Width, h);
    }

    /// <summary>Den Den Mushi grudado no vidro (microfone/webcam em uso): entra deslizando e fica no canto, "NO AR".</summary>
    private void DenDen(Graphics g, long sinceMs, float k)
    {
        float enter = (float)Smooth(0, 0.8, sinceMs / 1000.0);
        var c = new PointF(Width - 120 + (1 - enter) * 200, Height - 96);
        WorldArt.DrawSprite(g, WorldArt.DenDenSprite(), c, 0.9f, -4, 1);
        WorldArt.Caption(g, "NO AR", new PointF(c.X - 10, c.Y + 74), 22, enter);
    }
}
