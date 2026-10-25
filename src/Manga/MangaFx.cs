using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace DS339Monitor.Manga;

/// <summary>Formato do balão de fala.</summary>
internal enum BalloonShape
{
    /// <summary>Fala normal: oval com rabicho.</summary>
    Round,
    /// <summary>Grito: contorno de espinhos.</summary>
    Shout,
    /// <summary>Pensamento / sono: nuvem com bolinhas.</summary>
    Thought,
}

/// <summary>
/// Peças de desenho do Modo Mangá, todas por cima da arte que já está na tela (vídeo, GIF, painel do mangá): quadro de
/// impacto (negativo de alto contraste), linhas de foco, "soco" de zoom com tremida, onda, lavagem branca, raios de
/// luz, rachaduras no ar, raios negros do Haki, a divisa de página que atravessa a tela, onda de retícula, onomatopeias
/// com contorno, balões de fala, caixas de narração, a página de título do capítulo e o print convertido em retícula.
/// Sprites que dependem de texto são criados aqui e pertencem a quem pediu; os fixos ficam guardados.
/// </summary>
internal static class MangaFx
{
    private static readonly Dictionary<string, Bitmap> Fixed = new();

    /// <summary>Sprite fixo (onomatopeia, linhas de foco, raios...), criado uma vez.</summary>
    public static Bitmap Cached(string key, Func<Bitmap> make)
    {
        lock (Fixed)
        {
            if (!Fixed.TryGetValue(key, out var sprite)) Fixed[key] = sprite = make();
            return sprite;
        }
    }

    // ---------------------------------------------------------------- curvas

    public static double EaseOutBack(double x)
    {
        x = Math.Clamp(x, 0, 1);
        const double c1 = 1.70158, c3 = c1 + 1;
        return 1 + c3 * Math.Pow(x - 1, 3) + c1 * Math.Pow(x - 1, 2);
    }

    public static double EaseOutCubic(double x) => 1 - Math.Pow(1 - Math.Clamp(x, 0, 1), 3);

    public static double EaseInOut(double x)
    {
        x = Math.Clamp(x, 0, 1);
        return x * x * (3 - 2 * x);
    }

    /// <summary>Número pseudoaleatório estável (0–1) para uma semente e um índice.</summary>
    public static double Hash(long seed, long i)
    {
        ulong z = unchecked((ulong)seed * 0x9E3779B97F4A7C15UL + (ulong)i * 0xBF58476D1CE4E5B9UL);
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
        return ((z ^ (z >> 31)) >> 11) / (double)(1UL << 53);
    }

    // ---------------------------------------------------------------- sprites com transparência

    /// <summary>Desenha um sprite centrado em <paramref name="c"/>, na escala, girado e com transparência.</summary>
    public static void DrawSprite(Graphics g, Bitmap sprite, PointF c, float scale, float angle = 0, float alpha = 1)
    {
        if (alpha <= 0.01f || scale <= 0.01f) return;
        var state = g.Save();
        g.TranslateTransform(c.X, c.Y);
        if (angle != 0) g.RotateTransform(angle);
        g.ScaleTransform(scale, scale);
        var target = new RectangleF(-sprite.Width / 2f, -sprite.Height / 2f, sprite.Width, sprite.Height);
        if (alpha >= 0.99f)
        {
            g.DrawImage(sprite, target);
        }
        else
        {
            using var attributes = new ImageAttributes();
            attributes.SetColorMatrix(new ColorMatrix { Matrix33 = alpha });
            g.DrawImage(sprite, Rectangle.Round(target), 0, 0, sprite.Width, sprite.Height, GraphicsUnit.Pixel, attributes);
        }
        g.Restore(state);
    }

    /// <summary>
    /// Carimbo de onomatopeia: entra grande (1,6× → 1× com sobra, 0,16 s) tremendo, fica <paramref name="hold"/> s e sai
    /// crescendo e sumindo em 0,25 s. <paramref name="t"/> = segundos desde que entrou.
    /// </summary>
    public static void Stamp(Graphics g, Bitmap sprite, PointF c, double t, double hold, float scale = 1, float angle = 0, int seed = 0, float alpha = 1)
    {
        if (t < 0 || t > hold + 0.25) return;
        double pop = t < 0.16 ? 1.6 - 0.6 * EaseOutBack(t / 0.16) : 1;
        double fade = t > hold ? (t - hold) / 0.25 : 0;
        float s = (float)(scale * pop * (1 + 0.12 * fade));
        float a = alpha * (float)(1 - fade);
        // Treme nos primeiros 0,35 s (troca de posição a cada 40 ms, como desenho animado na mão).
        float jitter = t < 0.35 ? (float)(1 - t / 0.35) * 4 * s : 0;
        long step = (long)(t * 25);
        var at = new PointF(c.X + jitter * (float)(Hash(seed, step) - 0.5) * 2, c.Y + jitter * (float)(Hash(seed + 7, step) - 0.5) * 2);
        DrawSprite(g, sprite, at, s, angle, a);
    }

    // ---------------------------------------------------------------- onomatopeias

    /// <summary>
    /// Onomatopeia de mangá: cada letra levemente girada e de tamanho próprio, preenchida de preto com contorno branco e
    /// um fio preto por fora — legível sobre papel branco e sobre tinta preta. <paramref name="vertical"/> empilha as
    /// letras (escrita japonesa vertical). <paramref name="white"/> = letras brancas com contorno preto.
    /// </summary>
    public static Bitmap RenderSfx(string text, float em, int seed, bool vertical = false, bool white = false, float outline = 1)
    {
        using var path = new GraphicsPath();
        using var format = new StringFormat(StringFormat.GenericTypographic);
        float advance = 0;
        int index = 0;
        foreach (var rune in text.EnumerateRunes())
        {
            string s = rune.ToString();
            var family = rune.Value is 0x266A or 0x266B ? MangaFonts.Symbols : MangaFonts.Sfx;
            float size = em * (float)(0.88 + 0.26 * Hash(seed, index));
            using var glyph = new GraphicsPath();
            glyph.AddString(s, family, 0, size, new PointF(0, (em - size) * 0.5f), format);
            var b = glyph.GetBounds();
            if (b.Width <= 0 || b.Height <= 0)
            {
                advance += em * 0.3f;
                index++;
                continue;
            }
            float angle = (float)((Hash(seed + 1, index) - 0.5) * 16);
            float wobble = (float)((Hash(seed + 2, index) - 0.5) * em * 0.12);
            using var m = new Matrix();
            if (vertical) m.Translate(-b.X - b.Width / 2 + wobble, advance - b.Y, MatrixOrder.Append);
            else m.Translate(advance - b.X, wobble, MatrixOrder.Append);
            var center = vertical ? new PointF(wobble, advance + b.Height / 2) : new PointF(advance + b.Width / 2, b.Y + b.Height / 2 + wobble);
            m.RotateAt(angle, center, MatrixOrder.Append);
            glyph.Transform(m);
            path.AddPath(glyph, false);
            advance += (vertical ? b.Height : b.Width) * 0.94f + em * 0.03f;
            index++;
        }

        var bounds = path.GetBounds();
        float pad = em * 0.2f;
        var bitmap = new Bitmap(Math.Max(1, (int)Math.Ceiling(bounds.Width + 2 * pad)), Math.Max(1, (int)Math.Ceiling(bounds.Height + 2 * pad)), PixelFormat.Format32bppPArgb);
        using var g = Graphics.FromImage(bitmap);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TranslateTransform(pad - bounds.X, pad - bounds.Y);
        using var outer = new Pen(Color.Black, em * 0.27f * outline) { LineJoin = LineJoin.Round };
        using var halo = new Pen(white ? Color.Black : Color.White, em * 0.17f * outline) { LineJoin = LineJoin.Round };
        g.DrawPath(outer, path);
        g.DrawPath(halo, path);
        g.FillPath(white ? Brushes.White : Brushes.Black, path);
        return bitmap;
    }

    /// <summary>Onomatopeia fixa, guardada. Kanji cheio de traços pede <paramref name="outline"/> menor (o contorno engole o desenho).</summary>
    public static Bitmap Sfx(string text, float em, int seed = 1, bool vertical = false, bool white = false, float outline = 1) =>
        Cached($"sfx|{text}|{em}|{seed}|{vertical}|{white}|{outline}", () => RenderSfx(text, em, seed, vertical, white, outline));

    // ---------------------------------------------------------------- linhas de foco

    /// <summary>
    /// Linhas de foco (集中線): cunhas finas vindas das bordas em direção ao centro, deixando um miolo oval livre.
    /// </summary>
    public static Bitmap SpeedLines(int width, int height, int variant, bool white = false) =>
        Cached($"speed|{width}|{height}|{variant}|{white}", () =>
        {
            var bitmap = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
            using var g = Graphics.FromImage(bitmap);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float cx = width / 2f, cy = height / 2f;
            float far = (float)Math.Sqrt(width * width + height * height);
            int count = Math.Clamp(width * height / 4500, 90, 260);
            using var ink = new SolidBrush(white ? Color.White : Color.Black);
            for (int i = 0; i < count; i++)
            {
                double a = Hash(variant * 7919 + 11, i) * Math.PI * 2;
                double spread = 0.0016 + Hash(variant * 7919 + 12, i) * 0.0075;
                if (Hash(variant * 7919 + 13, i) < 0.1) spread *= 2.6; // algumas grossas, como feitas com pincel
                double inner = 1 + Hash(variant * 7919 + 14, i) * 0.8;
                var tip = new PointF(cx + (float)(Math.Cos(a) * width * 0.30 * inner), cy + (float)(Math.Sin(a) * height * 0.36 * inner));
                var p1 = new PointF(cx + (float)Math.Cos(a - spread) * far, cy + (float)Math.Sin(a - spread) * far);
                var p2 = new PointF(cx + (float)Math.Cos(a + spread) * far, cy + (float)Math.Sin(a + spread) * far);
                g.FillPolygon(ink, new[] { tip, p1, p2 });
            }
            return bitmap;
        });

    // ---------------------------------------------------------------- operações na imagem toda

    private static readonly byte[] ImpactLut = Enumerable.Range(0, 256).Select(v =>
    {
        double x = 1 - v / 255.0;                  // negativo...
        x = Math.Clamp((x - 0.22) / 0.56, 0, 1);  // ...de alto contraste: quase só preto e branco
        return (byte)Math.Round(255 * x * x * (3 - 2 * x));
    }).ToArray();

    /// <summary>Quadro de impacto: a imagem vira negativo em alto contraste (dura 2–3 quadros).</summary>
    public static unsafe void ImpactFrame(Bitmap canvas)
    {
        var data = canvas.LockBits(new Rectangle(0, 0, canvas.Width, canvas.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppPArgb);
        try
        {
            fixed (byte* lut = ImpactLut)
            {
                for (int y = 0; y < canvas.Height; y++)
                {
                    uint* row = (uint*)((byte*)data.Scan0 + (long)y * data.Stride);
                    for (int x = 0; x < canvas.Width; x++)
                    {
                        uint p = row[x];
                        int luma = ((int)((p >> 16) & 0xFF) * 77 + (int)((p >> 8) & 0xFF) * 150 + (int)(p & 0xFF) * 29) >> 8;
                        uint v = lut[luma];
                        row[x] = 0xFF000000u | v << 16 | v << 8 | v;
                    }
                }
            }
        }
        finally
        {
            canvas.UnlockBits(data);
        }
    }

    /// <summary>Copia os pixels de <paramref name="from"/> para <paramref name="to"/> (mesmo tamanho).</summary>
    public static unsafe void Copy(Bitmap from, Bitmap to)
    {
        var rect = new Rectangle(0, 0, from.Width, from.Height);
        var a = from.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);
        var b = to.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
        try
        {
            for (int y = 0; y < rect.Height; y++)
                Buffer.MemoryCopy((byte*)a.Scan0 + (long)y * a.Stride, (byte*)b.Scan0 + (long)y * b.Stride, b.Stride, rect.Width * 4);
        }
        finally
        {
            from.UnlockBits(a);
            to.UnlockBits(b);
        }
    }

    /// <summary>"Soco" de câmera: amplia a imagem (≥ 1×) a partir do centro e desloca (tremida), sem deixar borda vazia.</summary>
    public static void Punch(Bitmap canvas, Bitmap scratch, float scale, float dx, float dy)
    {
        if (scale <= 1.001f && Math.Abs(dx) < 0.3f && Math.Abs(dy) < 0.3f) return;
        Copy(canvas, scratch);
        scale = Math.Max(scale, 1 + 2.2f * Math.Max(Math.Abs(dx) / canvas.Width, Math.Abs(dy) / canvas.Height));
        float w = canvas.Width * scale, h = canvas.Height * scale;
        using var g = Graphics.FromImage(canvas);
        g.CompositingMode = CompositingMode.SourceCopy;
        g.InterpolationMode = InterpolationMode.Bilinear;
        g.PixelOffsetMode = PixelOffsetMode.Half;
        using var attributes = new ImageAttributes();
        attributes.SetWrapMode(WrapMode.TileFlipXY);
        var target = new RectangleF((canvas.Width - w) / 2 + dx, (canvas.Height - h) / 2 + dy, w, h);
        g.DrawImage(scratch, new[] { target.Location, new PointF(target.Right, target.Top), new PointF(target.Left, target.Bottom) },
            new RectangleF(0, 0, scratch.Width, scratch.Height), GraphicsUnit.Pixel, attributes);
    }

    /// <summary>Onda: cada linha da imagem desliza de lado numa senoide que desce (o mar balançando a imagem).</summary>
    public static unsafe void Wave(Bitmap canvas, Bitmap scratch, double t, float amplitude, float wavelength, float period)
    {
        if (amplitude < 0.2f) return;
        Copy(canvas, scratch);
        int w = canvas.Width, h = canvas.Height;
        var rect = new Rectangle(0, 0, w, h);
        var src = scratch.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);
        var dst = canvas.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
        try
        {
            for (int y = 0; y < h; y++)
            {
                double shift = amplitude * Math.Sin(2 * Math.PI * (y / wavelength - t / period));
                int s0 = (int)Math.Floor(shift);
                int f = (int)Math.Round((shift - s0) * 256);
                uint* from = (uint*)((byte*)src.Scan0 + (long)y * src.Stride);
                uint* to = (uint*)((byte*)dst.Scan0 + (long)y * dst.Stride);
                for (int x = 0; x < w; x++)
                {
                    uint a = from[Math.Clamp(x - s0, 0, w - 1)], b = from[Math.Clamp(x - s0 - 1, 0, w - 1)];
                    to[x] = Mix(a, b, f);
                }
            }
        }
        finally
        {
            scratch.UnlockBits(src);
            canvas.UnlockBits(dst);
        }
    }

    private static uint Mix(uint a, uint b, int f)
    {
        uint rb = ((a & 0xFF00FF) * (uint)(256 - f) + (b & 0xFF00FF) * (uint)f) >> 8 & 0xFF00FF;
        uint g = ((a & 0x00FF00) * (uint)(256 - f) + (b & 0x00FF00) * (uint)f) >> 8 & 0x00FF00;
        return 0xFF000000u | rb | g;
    }

    /// <summary>Véu (0–1, branco se não disser a cor) sobre a tela inteira; com <paramref name="vignette"/>, só nas bordas.</summary>
    public static void Wash(Graphics g, int width, int height, float alpha, bool vignette = false, Color? color = null)
    {
        if (alpha <= 0.005f) return;
        int a = (int)(255 * Math.Clamp(alpha, 0, 1));
        var tint = color ?? Color.White;
        if (!vignette)
        {
            using var fill = new SolidBrush(Color.FromArgb(a, tint));
            g.FillRectangle(fill, 0, 0, width, height);
            return;
        }
        // A vinheta é desenhada uma vez (em 1/4 do tamanho) e depois só esticada com a transparência pedida.
        var sprite = Cached($"vignette|{width}|{height}|{tint.ToArgb()}", () =>
        {
            int w = Math.Max(8, width / 4), h = Math.Max(8, height / 4);
            var bitmap = new Bitmap(w, h, PixelFormat.Format32bppPArgb);
            using var vg = Graphics.FromImage(bitmap);
            using var path = new GraphicsPath();
            path.AddEllipse(-w * 0.25f, -h * 0.6f, w * 1.5f, h * 2.2f);
            using var brush = new PathGradientBrush(path)
            {
                CenterColor = Color.FromArgb(0, tint),
                SurroundColors = new[] { Color.FromArgb(255, tint) },
                FocusScales = new PointF(0.45f, 0.35f),
            };
            vg.FillRectangle(brush, 0, 0, w, h);
            return bitmap;
        });
        using var attributes = new ImageAttributes();
        attributes.SetColorMatrix(new ColorMatrix { Matrix33 = a / 255f });
        attributes.SetWrapMode(WrapMode.TileFlipXY);
        var state = g.Save();
        g.InterpolationMode = InterpolationMode.Bilinear;
        g.DrawImage(sprite, new Rectangle(0, 0, width, height), 0, 0, sprite.Width, sprite.Height, GraphicsUnit.Pixel, attributes);
        g.Restore(state);
    }

    // ---------------------------------------------------------------- luz, rachaduras e raios

    /// <summary>Fachos de luz branca descendo de cima (amanhecer), um sprite do tamanho da tela.</summary>
    public static Bitmap Rays(int width, int height) =>
        Cached($"rays|{width}|{height}", () =>
        {
            var bitmap = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
            using var g = Graphics.FromImage(bitmap);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var origin = new PointF(width * 0.5f, -height * 0.9f);
            float length = height * 2.6f;
            for (int i = 0; i < 16; i++)
            {
                double a = Math.PI / 2 + (Hash(31, i) - 0.5) * 1.9;
                double half = 0.012 + Hash(32, i) * 0.03;
                var end = new PointF(origin.X + (float)Math.Cos(a) * length, origin.Y + (float)Math.Sin(a) * length);
                var p1 = new PointF(origin.X + (float)Math.Cos(a - half) * length, origin.Y + (float)Math.Sin(a - half) * length);
                var p2 = new PointF(origin.X + (float)Math.Cos(a + half) * length, origin.Y + (float)Math.Sin(a + half) * length);
                using var brush = new LinearGradientBrush(origin, end, Color.FromArgb((int)(150 + 80 * Hash(33, i)), 255, 255, 255), Color.FromArgb(0, 255, 255, 255));
                g.FillPolygon(brush, new[] { origin, p1, p2 });
            }
            return bitmap;
        });

    /// <summary>Rachadura no ar (a Gura Gura do Barba Branca): galhos a partir de um ponto, cada trecho com a distância do centro.</summary>
    public sealed record Crack(IReadOnlyList<(PointF A, PointF B, float Start)> Segments, float Length);

    public static Crack MakeCrack(int seed, PointF origin, float size)
    {
        var segments = new List<(PointF, PointF, float)>();
        float longest = 0;
        void Branch(PointF from, double angle, float length, float start, int depth, int id)
        {
            int steps = depth == 0 ? 10 : 5;
            float step = length / steps;
            var p = from;
            float d = start;
            for (int i = 0; i < steps; i++)
            {
                angle += (Hash(seed + id, i) - 0.5) * 1.3;
                var q = new PointF(p.X + (float)Math.Cos(angle) * step * (float)(0.7 + 0.6 * Hash(seed + id + 1, i)), p.Y + (float)Math.Sin(angle) * step * (float)(0.7 + 0.6 * Hash(seed + id + 2, i)));
                segments.Add((p, q, d));
                d += step;
                longest = Math.Max(longest, d);
                if (depth == 0 && i is 2 or 5 && Hash(seed + id + 3, i) < 0.8)
                    Branch(q, angle + (Hash(seed + id + 4, i) < 0.5 ? -1 : 1) * (0.5 + 0.5 * Hash(seed + id + 5, i)), length * 0.4f, d, 1, id * 31 + i);
                p = q;
            }
        }
        int arms = 7;
        for (int k = 0; k < arms; k++)
            Branch(origin, k * 2 * Math.PI / arms + (Hash(seed, 100 + k) - 0.5) * 0.6, size * (float)(0.6 + 0.5 * Hash(seed, 200 + k)), 0, 0, 1000 + k * 97);
        return new Crack(segments, longest);
    }

    /// <summary>Rachadura até <paramref name="reveal"/> px de distância do centro: traço preto com o vão branco no meio.</summary>
    public static void DrawCrack(Graphics g, Crack crack, float reveal, float width = 1)
    {
        if (reveal <= 0) return;
        using var black = new Pen(Color.Black, 10f * width) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Miter };
        using var white = new Pen(Color.White, 3.6f * width) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Miter };
        foreach (var pen in new[] { black, white })
        {
            foreach (var (a, b, start) in crack.Segments)
            {
                if (start >= reveal) continue;
                float length = Distance(a, b);
                float k = Math.Min(1, (reveal - start) / Math.Max(1, length));
                g.DrawLine(pen, a, new PointF(a.X + (b.X - a.X) * k, a.Y + (b.Y - a.Y) * k));
            }
        }
    }

    private static float Distance(PointF a, PointF b) => (float)Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));

    /// <summary>Raio em zigue-zague de <paramref name="from"/> a <paramref name="to"/>, com um galho.</summary>
    public static PointF[][] MakeBolt(int seed, PointF from, PointF to)
    {
        PointF[] Line(PointF a, PointF b, int segments, float jitter, int s)
        {
            var points = new PointF[segments + 1];
            float nx = -(b.Y - a.Y), ny = b.X - a.X;
            float n = (float)Math.Sqrt(nx * nx + ny * ny);
            nx /= n;
            ny /= n;
            for (int i = 0; i <= segments; i++)
            {
                float k = i / (float)segments;
                float off = i == 0 || i == segments ? 0 : (float)((Hash(s, i) - 0.5) * 2 * jitter);
                points[i] = new PointF(a.X + (b.X - a.X) * k + nx * off, a.Y + (b.Y - a.Y) * k + ny * off);
            }
            return points;
        }
        float length = Distance(from, to);
        var main = Line(from, to, 11, length * 0.11f, seed);
        var fork = main[4];
        var forkEnd = new PointF(fork.X + (to.X - from.X) * 0.35f + (float)((Hash(seed, 50) - 0.5) * length * 0.4), fork.Y + (to.Y - from.Y) * 0.35f + (float)((Hash(seed, 51) - 0.5) * length * 0.4));
        return new[] { main, Line(fork, forkEnd, 5, length * 0.05f, seed + 9) };
    }

    /// <summary>Raio negro do Haki do Rei: núcleo preto com brilho branco em volta.</summary>
    public static void DrawBolt(Graphics g, PointF[][] bolt, float width, float alpha = 1)
    {
        using var glow = new Pen(Color.FromArgb((int)(220 * alpha), 255, 255, 255), width * 2.6f) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round };
        using var core = new Pen(Color.FromArgb((int)(255 * alpha), 0, 0, 0), width) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Triangle };
        for (int i = 0; i < bolt.Length; i++)
        {
            float scale = i == 0 ? 1 : 0.55f;
            glow.Width = width * 2.6f * scale;
            core.Width = width * scale;
            g.DrawLines(glow, bolt[i]);
            g.DrawLines(core, bolt[i]);
        }
    }

    // ---------------------------------------------------------------- passagens de página

    /// <summary>
    /// A divisa entre dois quadros (faixa branca com traço preto dos dois lados, inclinada como a da Pro Flow)
    /// atravessando a tela da esquerda para a direita em <paramref name="t"/> (0–1).
    /// </summary>
    public static void Gutter(Graphics g, int width, int height, double t, float scale = 1)
    {
        const float Slope = 0.27f;
        float gutter = 18 * scale, ink = 3 * scale;
        float reach = Slope * height / 2 + gutter / 2 + ink + 2;
        float center = -reach + (width + 2 * reach) * (float)EaseInOut(t);
        PointF At(float offset, float y) => new(center + offset + (height / 2f - y) * Slope, y);
        float outer = gutter / 2 + ink, inner = gutter / 2;
        g.FillPolygon(Brushes.Black, new[] { At(-outer, -2), At(outer, -2), At(outer, height + 2), At(-outer, height + 2) });
        g.FillPolygon(Brushes.White, new[] { At(-inner, -2), At(inner, -2), At(inner, height + 2), At(-inner, height + 2) });
    }

    private static readonly Dictionary<(int, int), (byte[] Dots, byte[] Sweep)> HalftoneMaps = new();

    /// <summary>Onda de retícula: pontos pretos crescem e somem passando pela tela (do canto de cima à direita).</summary>
    public static unsafe void HalftoneRipple(Bitmap canvas, double t)
    {
        int w = canvas.Width, h = canvas.Height;
        (byte[] dots, byte[] sweep) maps;
        lock (HalftoneMaps)
        {
            if (!HalftoneMaps.TryGetValue((w, h), out maps))
            {
                var dots = new byte[w * h];
                var sweep = new byte[w * h];
                const double Spacing = 13;
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        double u = (x + y) / Math.Sqrt(2) / Spacing, v = (x - y) / Math.Sqrt(2) / Spacing;
                        double fu = u - Math.Floor(u) - 0.5, fv = v - Math.Floor(v) - 0.5;
                        dots[y * w + x] = (byte)Math.Min(255, Math.Sqrt(fu * fu + fv * fv) / Math.Sqrt(0.5) * 255);
                        sweep[y * w + x] = (byte)Math.Min(255, (w - x + y * (w / (double)h) * 0.5) / (w + w * 0.5) * 255);
                    }
                }
                HalftoneMaps[(w, h)] = maps = (dots, sweep);
            }
        }

        const double Spread = 2.2; // uma faixa de pontos passando, não a tela toda de uma vez
        Span<int> threshold = stackalloc int[256];
        for (int s = 0; s < 256; s++)
        {
            double p = Math.Clamp(t * (1 + Spread) - s / 255.0 * Spread, 0, 1);
            threshold[s] = (int)(125 * Math.Sin(Math.PI * p));
        }
        var data = canvas.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, PixelFormat.Format32bppPArgb);
        try
        {
            fixed (byte* dots = maps.dots, sweep = maps.sweep)
            {
                for (int y = 0; y < h; y++)
                {
                    uint* row = (uint*)((byte*)data.Scan0 + (long)y * data.Stride);
                    int i = y * w;
                    for (int x = 0; x < w; x++, i++)
                        if (dots[i] < threshold[sweep[i]]) row[x] = 0xFF101010u;
                }
            }
        }
        finally
        {
            canvas.UnlockBits(data);
        }
    }

    // ---------------------------------------------------------------- letreiramento

    /// <summary>
    /// Balão de fala com o texto em caixa alta (Bangers). O rabicho aponta para baixo, para o lado
    /// <paramref name="tailLeft"/>. O contorno é um traço só por fora (balão e rabicho fundidos).
    /// </summary>
    public static Bitmap RenderBalloon(string text, float em, BalloonShape shape, int seed, bool tailLeft = true)
    {
        using var textPath = new GraphicsPath();
        using var format = new StringFormat(StringFormat.GenericTypographic) { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        textPath.AddString(text.ToUpperInvariant(), MangaFonts.Comic, 0, em, new RectangleF(-2000, -1000, 4000, 2000), format);
        var tb = textPath.GetBounds();
        float cx = tb.X + tb.Width / 2, cy = tb.Y + tb.Height / 2;
        float rx = tb.Width / 2 * 1.32f + em * 0.65f, ry = tb.Height / 2 * 1.45f + em * 0.6f;
        float ink = Math.Max(2.5f, em * 0.085f);

        using var body = new GraphicsPath { FillMode = FillMode.Winding };
        float side = tailLeft ? -1 : 1;
        switch (shape)
        {
            case BalloonShape.Shout:
            {
                int spikes = 18 + (int)(Hash(seed, 1) * 6);
                var points = new List<PointF>();
                for (int i = 0; i < spikes * 2; i++)
                {
                    double a = Math.PI * i / spikes + Hash(seed, i + 10) * 0.12;
                    double r = i % 2 == 0 ? 1.18 + 0.22 * Hash(seed, i + 40) : 0.9;
                    points.Add(new PointF(cx + (float)(Math.Cos(a) * rx * r), cy + (float)(Math.Sin(a) * ry * r)));
                }
                body.AddPolygon(points.ToArray());
                rx *= 1.2f;
                ry *= 1.2f;
                break;
            }
            case BalloonShape.Thought:
            {
                int puffs = 11;
                for (int i = 0; i < puffs; i++)
                {
                    double a = 2 * Math.PI * i / puffs;
                    float pr = Math.Min(rx, ry) * (float)(0.36 + 0.1 * Hash(seed, i));
                    body.AddEllipse(cx + (float)Math.Cos(a) * rx * 0.86f - pr, cy + (float)Math.Sin(a) * ry * 0.82f - pr, pr * 2, pr * 2);
                }
                body.AddEllipse(cx - rx * 0.9f, cy - ry * 0.85f, rx * 1.8f, ry * 1.7f);
                for (int i = 0; i < 2; i++)
                {
                    float pr = em * (0.28f - 0.1f * i);
                    body.AddEllipse(cx + side * rx * (0.9f + 0.3f * i) - pr, cy + ry * (1.05f + 0.35f * i) - pr, pr * 2, pr * 2);
                }
                break;
            }
            default:
                body.AddEllipse(cx - rx, cy - ry, rx * 2, ry * 2);
                break;
        }
        if (shape != BalloonShape.Thought)
        {
            var root = new PointF(cx + side * rx * 0.35f, cy + ry * 0.75f);
            body.AddPolygon(new[]
            {
                new PointF(root.X - em * 0.45f, root.Y), new PointF(root.X + em * 0.45f, root.Y),
                new PointF(cx + side * rx * 0.85f, cy + ry * 1.55f),
            });
        }

        var bounds = body.GetBounds();
        float pad = ink * 2 + 2;
        var bitmap = new Bitmap((int)Math.Ceiling(bounds.Width + 2 * pad), (int)Math.Ceiling(bounds.Height + 2 * pad), PixelFormat.Format32bppPArgb);
        using var g = Graphics.FromImage(bitmap);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TranslateTransform(pad - bounds.X, pad - bounds.Y);
        using (var outline = new Pen(Color.Black, ink * 2) { LineJoin = LineJoin.Round })
            g.DrawPath(outline, body);
        g.FillPath(Brushes.White, body);
        g.FillPath(Brushes.Black, textPath);
        return bitmap;
    }

    /// <summary>
    /// Caixa de narração de mangá: papel branco, traço preto, sombra dura. Título em caixa alta (Bangers) e, embaixo,
    /// uma linha menor (nome do arquivo, da música...).
    /// </summary>
    public static Bitmap RenderNarration(string title, string? body, float scale, int maxWidth)
    {
        using var titleFont = new Font(MangaFonts.Comic, 40 * scale, GraphicsUnit.Pixel);
        using var bodyFont = new Font(MangaFonts.Text, 25 * scale, FontStyle.Bold, GraphicsUnit.Pixel);
        using var format = new StringFormat(StringFormat.GenericTypographic) { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap };
        float pad = 16 * scale, shadow = 8 * scale, border = 3.5f * scale;
        SizeF titleSize, bodySize;
        using (var probe = new Bitmap(1, 1))
        using (var measure = Graphics.FromImage(probe))
        {
            measure.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            titleSize = measure.MeasureString(title.ToUpperInvariant(), titleFont, maxWidth, format);
            bodySize = string.IsNullOrEmpty(body) ? SizeF.Empty : measure.MeasureString(body, bodyFont, maxWidth, format);
        }
        float width = Math.Min(maxWidth, Math.Max(titleSize.Width, bodySize.Width)) + 2 * pad;
        float height = titleSize.Height + (bodySize.Height > 0 ? bodySize.Height + 4 * scale : 0) + 2 * pad - 4 * scale;
        var bitmap = new Bitmap((int)Math.Ceiling(width + shadow + border * 2), (int)Math.Ceiling(height + shadow + border * 2), PixelFormat.Format32bppPArgb);
        using var g = Graphics.FromImage(bitmap);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
        var box = new RectangleF(border, border, width, height);
        using (var dark = new SolidBrush(Color.FromArgb(235, 0, 0, 0)))
            g.FillRectangle(dark, box.X + shadow, box.Y + shadow, box.Width, box.Height);
        g.FillRectangle(Brushes.White, box);
        using (var pen = new Pen(Color.Black, border))
            g.DrawRectangle(pen, box.X, box.Y, box.Width, box.Height);
        g.DrawString(title.ToUpperInvariant(), titleFont, Brushes.Black, new RectangleF(box.X + pad, box.Y + pad - 4 * scale, maxWidth, titleSize.Height + 4), format);
        if (bodySize.Height > 0)
        {
            using var muted = new SolidBrush(Color.FromArgb(40, 40, 40));
            g.DrawString(body, bodyFont, muted, new RectangleF(box.X + pad, box.Y + pad - 4 * scale + titleSize.Height + 4 * scale, maxWidth, bodySize.Height + 4), format);
        }
        return bitmap;
    }

    /// <summary>Etiqueta pequena (ex.: "● NO AR"): caixa branca de cantos redondos com traço preto.</summary>
    public static Bitmap RenderTag(string text, float scale)
    {
        using var font = new Font(MangaFonts.Comic, 30 * scale, GraphicsUnit.Pixel);
        using var format = new StringFormat(StringFormat.GenericTypographic);
        SizeF size;
        using (var probe = new Bitmap(1, 1))
        using (var measure = Graphics.FromImage(probe))
            size = measure.MeasureString(text, font, int.MaxValue, format);
        float pad = 12 * scale;
        var bitmap = new Bitmap((int)Math.Ceiling(size.Width + 2 * pad + 8), (int)Math.Ceiling(size.Height + 2 * pad - 4 * scale + 8), PixelFormat.Format32bppPArgb);
        using var g = Graphics.FromImage(bitmap);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
        var box = new RectangleF(2, 2, size.Width + 2 * pad, size.Height + 2 * pad - 4 * scale);
        using var path = DS339Monitor.Themes.Shapes.RoundedRect(box, box.Height / 2);
        g.FillPath(Brushes.White, path);
        using (var pen = new Pen(Color.Black, 3 * scale))
            g.DrawPath(pen, path);
        g.DrawString(text, font, Brushes.Black, box.X + pad, box.Y + pad - 3 * scale, format);
        return bitmap;
    }

    // ---------------------------------------------------------------- página de título e print

    /// <summary>
    /// Página de título do capítulo (na hora cheia): papel branco com linhas de foco, "第N話" enorme em pincel e, ao
    /// lado, "CAPÍTULO N", o título e a hora. Ocupa <paramref name="width"/> × <paramref name="height"/>, com a borda da
    /// esquerda inclinada (entra deslizando por cima do vídeo).
    /// </summary>
    public static Bitmap RenderChapterCard(int width, int height, int chapter, string title, string clock)
    {
        var bitmap = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
        using var g = Graphics.FromImage(bitmap);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
        float lean = height * 0.22f;
        var page = new[] { new PointF(lean, 0), new PointF(width, 0), new PointF(width, height), new PointF(0, height) };
        using (var clip = new GraphicsPath())
        {
            clip.AddPolygon(page);
            g.SetClip(clip);
            g.Clear(Color.White);
            // Linhas de foco puxando para o número.
            var lines = SpeedLines(width, height, 5);
            using (var attributes = new ImageAttributes())
            {
                attributes.SetColorMatrix(new ColorMatrix { Matrix33 = 0.35f });
                g.DrawImage(lines, new Rectangle(0, 0, width, height), 0, 0, width, height, GraphicsUnit.Pixel, attributes);
            }
            g.ResetClip();
        }
        using (var edge = new Pen(Color.Black, 7))
            g.DrawLine(edge, page[0], page[3]);

        var kanji = RenderSfx($"第{chapter}話", height * 0.36f, chapter * 13 + 5);
        float kanjiScale = Math.Min(1, width * 0.48f / kanji.Width);
        DrawSprite(g, kanji, new PointF(lean + 40 + kanji.Width * kanjiScale / 2, height * 0.47f), kanjiScale, -4);
        kanji.Dispose();

        float x = lean + 70 + width * 0.48f;
        float columnWidth = width - x - 36;
        using var chapterFont = new Font(MangaFonts.Comic, height * 0.15f, GraphicsUnit.Pixel);
        using var titleFont = new Font(MangaFonts.Comic, height * 0.105f, GraphicsUnit.Pixel);
        using var clockFont = new Font(MangaFonts.Text, height * 0.075f, FontStyle.Bold, GraphicsUnit.Pixel);
        using var format = new StringFormat(StringFormat.GenericTypographic);
        float y = height * 0.16f;
        g.DrawString($"CAPÍTULO {chapter}", chapterFont, Brushes.Black, new RectangleF(x, y, columnWidth, height * 0.2f), format);
        y += height * 0.2f;
        using (var rule = new Pen(Color.Black, 4))
            g.DrawLine(rule, x, y, x + columnWidth * 0.8f, y);
        y += height * 0.05f;
        var titleSize = g.MeasureString(title.ToUpperInvariant(), titleFont, (int)columnWidth, format);
        g.DrawString(title.ToUpperInvariant(), titleFont, Brushes.Black, new RectangleF(x, y, columnWidth, titleSize.Height + 4), format);
        y += titleSize.Height + height * 0.05f;
        using var muted = new SolidBrush(Color.FromArgb(70, 70, 70));
        g.DrawString(clock, clockFont, muted, x, y, format);
        return bitmap;
    }

    /// <summary>
    /// O print vira um quadro de mangá: tons de cinza, contraste de mangá e retícula de pontos a 45° (como papel
    /// impresso), com margem branca e traço preto grosso. Print de tela escura (modo escuro) é invertido antes: vira
    /// papel branco com tinta preta, como um painel de verdade.
    /// </summary>
    public static unsafe Bitmap RenderPrintPanel(Image photo, int width, int height)
    {
        const int Border = 6, Margin = 10, Shadow = 10;
        int innerW = width - 2 * (Border + Margin) - Shadow, innerH = height - 2 * (Border + Margin) - Shadow;
        using var gray = new Bitmap(innerW, innerH, PixelFormat.Format32bppPArgb);
        using (var g = Graphics.FromImage(gray))
        {
            float scale = Math.Max((float)innerW / photo.Width, (float)innerH / photo.Height);
            float w = photo.Width * scale, h = photo.Height * scale;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(photo, (innerW - w) / 2, (innerH - h) / 2, w, h);
        }
        var data = gray.LockBits(new Rectangle(0, 0, innerW, innerH), ImageLockMode.ReadWrite, PixelFormat.Format32bppPArgb);
        try
        {
            double average = 0;
            for (int y = 0; y < innerH; y += 4)
            {
                uint* row = (uint*)((byte*)data.Scan0 + (long)y * data.Stride);
                for (int x = 0; x < innerW; x += 4)
                    average += (((row[x] >> 16) & 0xFF) * 0.299 + ((row[x] >> 8) & 0xFF) * 0.587 + (row[x] & 0xFF) * 0.114) / 255;
            }
            bool darkScreen = average / ((innerH + 3) / 4 * ((innerW + 3) / 4)) < 0.42;
            const double Period = 5.5;
            for (int y = 0; y < innerH; y++)
            {
                uint* row = (uint*)((byte*)data.Scan0 + (long)y * data.Stride);
                for (int x = 0; x < innerW; x++)
                {
                    uint p = row[x];
                    double luma = (((p >> 16) & 0xFF) * 0.299 + ((p >> 8) & 0xFF) * 0.587 + (p & 0xFF) * 0.114) / 255;
                    if (darkScreen) luma = 1 - luma;
                    luma = Math.Clamp((luma - 0.08) / 0.84, 0, 1);
                    double dark = 1 - luma;
                    double u = (x + y) / Math.Sqrt(2) / Period, v = (x - y) / Math.Sqrt(2) / Period;
                    double fu = u - Math.Floor(u) - 0.5, fv = v - Math.Floor(v) - 0.5;
                    double d = (fu * fu + fv * fv) / 0.5; // 0 no centro do ponto, 1 no canto da célula
                    bool black = dark > 0.9 || (dark > 0.06 && d < dark * 0.95);
                    row[x] = black ? 0xFF141414u : 0xFFFFFFFFu;
                }
            }
        }
        finally
        {
            gray.UnlockBits(data);
        }

        var bitmap = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
        using (var g = Graphics.FromImage(bitmap))
        {
            var outer = new Rectangle(0, 0, width - Shadow, height - Shadow);
            using (var shadow = new SolidBrush(Color.FromArgb(230, 0, 0, 0)))
                g.FillRectangle(shadow, outer.X + Shadow, outer.Y + Shadow, outer.Width, outer.Height);
            g.FillRectangle(Brushes.White, outer);
            g.DrawImageUnscaled(gray, Border + Margin, Border + Margin);
            using var pen = new Pen(Color.Black, Border);
            g.DrawRectangle(pen, Border / 2f, Border / 2f, outer.Width - Border, outer.Height - Border);
            using var thin = new Pen(Color.Black, 3);
            g.DrawRectangle(thin, Border + Margin - 1.5f, Border + Margin - 1.5f, innerW + 3, innerH + 3);
        }
        return bitmap;
    }
}
