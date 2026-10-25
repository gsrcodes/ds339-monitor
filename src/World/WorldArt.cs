using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using DS339Monitor.Lighting;

namespace DS339Monitor.World;

/// <summary>
/// Desenho dos atores do mundo em traço de mangá (tinta preta, papel branco), em qualquer escala (px por mm). Tudo é
/// vetorial e pequeno — desenhar na hora custa décimos de milissegundo — e as onomatopeias ficam em cache.
/// </summary>
internal static class WorldArt
{
    private static readonly Dictionary<string, Bitmap> SfxCache = new();
    private static readonly object SfxLock = new();
    private static FontFamily? _japanese;

    private static readonly Dictionary<string, Bitmap> Sprites = new();

    /// <summary>Sprite da arte nova (MangaArt), gerado uma vez e guardado.</summary>
    private static Bitmap Sprite(string key, Func<Bitmap> make)
    {
        lock (Sprites)
        {
            if (!Sprites.TryGetValue(key, out var sprite)) Sprites[key] = sprite = make();
            return sprite;
        }
    }

    public static Bitmap NewsCooFrame(bool wingsUp) => Sprite(wingsUp ? "coo-up" : "coo-down", () => Themes.MangaArt.NewsCoo(wingsUp));
    public static Bitmap BubbleSprite() => Sprite("bubble", () => Themes.MangaArt.Bubble(30));
    public static Bitmap NoteSprite() => Sprite("note", Themes.MangaArt.Note);
    public static Bitmap DenDenSprite() => Sprite("denden", Themes.MangaArt.DenDenMushi);

    /// <summary>Desenha um sprite centrado em <paramref name="c"/>, na escala, girado e com transparência.</summary>
    public static void DrawSprite(Graphics g, Bitmap sprite, PointF c, float scale, float angle, float alpha, bool flip = false)
    {
        if (alpha <= 0.01f) return;
        var state = g.Save();
        g.TranslateTransform(c.X, c.Y);
        if (angle != 0) g.RotateTransform(angle);
        g.ScaleTransform(flip ? -scale : scale, scale);
        var target = new Rectangle(-sprite.Width / 2, -sprite.Height / 2, sprite.Width, sprite.Height);
        if (alpha >= 0.99f)
        {
            g.DrawImage(sprite, target);
        }
        else
        {
            using var attributes = new ImageAttributes();
            attributes.SetColorMatrix(new ColorMatrix { Matrix33 = alpha });
            g.DrawImage(sprite, target, 0, 0, sprite.Width, sprite.Height, GraphicsUnit.Pixel, attributes);
        }
        g.Restore(state);
    }

    public static Pen Ink(float width, float alpha = 1) =>
        new(Color.FromArgb((int)(255 * Math.Clamp(alpha, 0, 1)), 18, 18, 18), width) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round };

    public static SolidBrush Paper(float alpha = 1) => new(Color.FromArgb((int)(255 * Math.Clamp(alpha, 0, 1)), 255, 255, 255));

    public static SolidBrush Solid(float alpha = 1) => new(Color.FromArgb((int)(255 * Math.Clamp(alpha, 0, 1)), 18, 18, 18));

    /// <summary>Âncora (~11 mm de altura), com a argola em cima em <paramref name="c"/>.</summary>
    public static void Anchor(Graphics g, PointF c, float k, float angle, float alpha = 1)
    {
        var state = g.Save();
        g.TranslateTransform(c.X, c.Y);
        g.RotateTransform(angle);
        g.ScaleTransform(k / 8, k / 8); // desenhado em unidades de 1/8 mm
        using var ink = Ink(5, alpha);
        using var paper = Paper(alpha);
        g.FillEllipse(paper, -8, 0, 16, 16);
        g.DrawEllipse(ink, -8, 0, 16, 16);
        g.FillRectangle(paper, -5, 16, 10, 64);
        g.DrawRectangle(ink, -5, 16, 10, 64);
        g.FillRectangle(paper, -26, 26, 52, 8);
        g.DrawRectangle(ink, -26, 26, 52, 8);
        using (var arms = new GraphicsPath())
        {
            arms.AddArc(-40, 40, 80, 56, 20, 140);
            using var thick = Ink(11, alpha);
            g.DrawPath(thick, arms);
            using var thin = new Pen(Color.FromArgb((int)(255 * alpha), 255, 255, 255), 4) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            g.DrawPath(thin, arms);
        }
        foreach (int side in new[] { -1, 1 })
        {
            var fluke = new[] { new PointF(side * 38, 72), new PointF(side * 50, 58), new PointF(side * 30, 64) };
            g.FillPolygon(paper, fluke);
            g.DrawPolygon(ink, fluke);
        }
        g.Restore(state);
    }

    /// <summary>Corrente: elos alternados de <paramref name="from"/> até <paramref name="to"/>.</summary>
    public static void Chain(Graphics g, PointF from, PointF to, float k, float alpha = 1)
    {
        float dx = to.X - from.X, dy = to.Y - from.Y, length = (float)Math.Sqrt(dx * dx + dy * dy);
        if (length < 1) return;
        float link = 1.6f * k, angle = (float)(Math.Atan2(dy, dx) * 180 / Math.PI);
        float width = Math.Max(1.5f, 0.35f * k);
        using var ink = Ink(width, alpha);
        using var halo = new Pen(Color.FromArgb((int)(170 * alpha), 235, 235, 235), width * 2.6f); // aparece no mar escuro também
        var state = g.Save();
        g.TranslateTransform(from.X, from.Y);
        g.RotateTransform(angle);
        g.DrawLine(halo, 0, 0, length, 0);
        for (float s = 0, i = 0; s < length; s += link * 0.8f, i++)
        {
            if (i % 2 == 0) g.DrawEllipse(ink, s, -link * 0.3f, link, link * 0.6f);
            else g.DrawLine(ink, s, 0, s + link, 0);
        }
        g.Restore(state);
    }

    /// <summary>Moeda (berry) girando: a largura encolhe com o giro.</summary>
    public static void Coin(Graphics g, PointF c, float k, double spin, float alpha = 1)
    {
        float r = 2.6f * k, w = Math.Max(0.12f, (float)Math.Abs(Math.Cos(spin))) * r;
        using var ink = Ink(Math.Max(1.5f, 0.3f * k), alpha);
        using var paper = Paper(alpha);
        g.FillEllipse(paper, c.X - w, c.Y - r, 2 * w, 2 * r);
        g.DrawEllipse(ink, c.X - w, c.Y - r, 2 * w, 2 * r);
        if (w > r * 0.5f)
        {
            using var font = new Font("Segoe UI", r * 1.1f, FontStyle.Bold, GraphicsUnit.Pixel);
            using var solid = Solid(alpha);
            var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString("฿", font, solid, c, format);
        }
    }

    /// <summary>Nota musical (colcheia), ~6 mm.</summary>
    public static void Note(Graphics g, PointF c, float k, float angle, float alpha = 1)
    {
        var state = g.Save();
        g.TranslateTransform(c.X, c.Y);
        g.RotateTransform(angle - 15);
        using var solid = Solid(alpha);
        using var ink = Ink(Math.Max(2, 0.45f * k), alpha);
        g.FillEllipse(solid, -1.8f * k, 1.2f * k, 2.4f * k, 1.7f * k);
        g.DrawLine(ink, 0.5f * k, 2f * k, 0.5f * k, -3.2f * k);
        g.DrawBezier(ink, 0.5f * k, -3.2f * k, 1.8f * k, -2.4f * k, 2.4f * k, -1.4f * k, 1.6f * k, -0.2f * k);
        g.Restore(state);
    }

    /// <summary>Bolha de sabão/ar: contorno, brilho e, se pedir, um símbolo dentro.</summary>
    public static void Bubble(Graphics g, PointF c, float r, string? symbol = null, float alpha = 1, bool symbolOnly = false)
    {
        if (symbolOnly)
        {
            if (symbol is null) return;
            using var symbolFont = new Font("Bahnschrift SemiBold", r * 0.8f, GraphicsUnit.Pixel);
            using var ink = Solid(0.85f);
            g.DrawString(symbol, symbolFont, ink, c, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            return;
        }
        using var rim = new Pen(Color.FromArgb((int)(230 * alpha), 30, 30, 30), Math.Max(1.5f, r * 0.07f));
        using var fill = new SolidBrush(Color.FromArgb((int)(70 * alpha), 255, 255, 255));
        using var shine = new Pen(Color.FromArgb((int)(230 * alpha), 255, 255, 255), Math.Max(1.5f, r * 0.12f)) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        g.FillEllipse(fill, c.X - r, c.Y - r, 2 * r, 2 * r);
        g.DrawEllipse(rim, c.X - r, c.Y - r, 2 * r, 2 * r);
        g.DrawArc(shine, c.X - r * 0.7f, c.Y - r * 0.7f, r * 1.4f, r * 1.4f, 200, 60);
        if (symbol is null) return;
        using var font = new Font("Bahnschrift SemiBold", r * 0.8f, GraphicsUnit.Pixel);
        using var solid = Solid(alpha);
        var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        g.DrawString(symbol, font, solid, c, format);
    }

    /// <summary>Baú do tesouro (~9 mm de largura) com uma etiqueta.</summary>
    public static void Chest(Graphics g, PointF c, float k, string? label, float alpha = 1)
    {
        float w = 9 * k, h = 6 * k;
        var body = new RectangleF(c.X - w / 2, c.Y - h / 2, w, h);
        using var ink = Ink(Math.Max(2, 0.4f * k), alpha);
        using var paper = Paper(alpha);
        using var solid = Solid(alpha);
        using (var lid = new GraphicsPath())
        {
            lid.AddArc(body.X, body.Y - h * 0.45f, w, h * 0.9f, 180, 180);
            lid.CloseFigure();
            g.FillPath(paper, lid);
            g.DrawPath(ink, lid);
        }
        g.FillRectangle(paper, body);
        g.DrawRectangle(ink, body.X, body.Y, body.Width, body.Height);
        g.FillRectangle(solid, body.X + w * 0.12f, body.Y - h * 0.4f, w * 0.1f, h * 1.4f);
        g.FillRectangle(solid, body.Right - w * 0.22f, body.Y - h * 0.4f, w * 0.1f, h * 1.4f);
        g.FillRectangle(paper, c.X - w * 0.08f, c.Y - h * 0.15f, w * 0.16f, h * 0.3f);
        g.DrawRectangle(ink, c.X - w * 0.08f, c.Y - h * 0.15f, w * 0.16f, h * 0.3f);
        if (label is null) return;
        Caption(g, label, new PointF(c.X, body.Bottom + 2.2f * k), 2.2f * k, alpha);
    }

    /// <summary>
    /// Cartaz de procurado (~15x20 mm): "WANTED", a foto (ou a silhueta do chapéu de palha), "DEAD OR ALIVE" e a
    /// recompensa.
    /// </summary>
    public static void Poster(Graphics g, PointF c, float k, float angle, Image? photo, string bounty, float alpha = 1)
    {
        var state = g.Save();
        g.TranslateTransform(c.X, c.Y);
        g.RotateTransform(angle);
        float w = 15 * k, h = 20 * k;
        var paper = new RectangleF(-w / 2, -h / 2, w, h);
        using (var cream = new SolidBrush(Color.FromArgb((int)(255 * alpha), 246, 240, 226)))
            g.FillRectangle(cream, paper);
        using var ink = Ink(Math.Max(1.5f, 0.3f * k), alpha);
        using var solid = Solid(alpha);
        g.DrawRectangle(ink, paper.X, paper.Y, paper.Width, paper.Height);
        var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        using (var title = new Font("Bahnschrift SemiBold", 3.2f * k, GraphicsUnit.Pixel))
            g.DrawString("WANTED", title, solid, new RectangleF(paper.X, paper.Y + 0.4f * k, w, 3.6f * k), format);
        var photoBox = new RectangleF(paper.X + 1.4f * k, paper.Y + 4.2f * k, w - 2.8f * k, 9.6f * k);
        if (photo is not null)
        {
            using var attributes = new ImageAttributes();
            var gray = new ColorMatrix(new[]
            {
                new[] { 0.3f, 0.3f, 0.3f, 0, 0 }, new[] { 0.59f, 0.59f, 0.59f, 0, 0 }, new[] { 0.11f, 0.11f, 0.11f, 0, 0 },
                new[] { 0f, 0, 0, alpha, 0 }, new[] { 0f, 0, 0, 0, 1 },
            });
            attributes.SetColorMatrix(gray);
            float scale = Math.Max(photoBox.Width / photo.Width, photoBox.Height / photo.Height);
            float sw = photoBox.Width / scale, sh = photoBox.Height / scale;
            g.DrawImage(photo, Rectangle.Round(photoBox), (photo.Width - sw) / 2, (photo.Height - sh) / 2, sw, sh, GraphicsUnit.Pixel, attributes);
        }
        else
        {
            // Silhueta: chapéu de palha.
            using var shade = new SolidBrush(Color.FromArgb((int)(200 * alpha), 60, 60, 60));
            var cx = photoBox.X + photoBox.Width / 2;
            g.FillEllipse(shade, cx - 3.6f * k, photoBox.Y + 3.4f * k, 7.2f * k, 6.4f * k);
            g.FillEllipse(paper2(alpha), cx - 4.6f * k, photoBox.Y + 1.6f * k, 9.2f * k, 2.4f * k);
            g.DrawEllipse(ink, cx - 4.6f * k, photoBox.Y + 1.6f * k, 9.2f * k, 2.4f * k);
            g.FillEllipse(paper2(alpha), cx - 2.4f * k, photoBox.Y + 0.4f * k, 4.8f * k, 2.4f * k);
            g.DrawEllipse(ink, cx - 2.4f * k, photoBox.Y + 0.4f * k, 4.8f * k, 2.4f * k);
        }
        g.DrawRectangle(ink, photoBox.X, photoBox.Y, photoBox.Width, photoBox.Height);
        using (var small = new Font("Bahnschrift", 1.6f * k, GraphicsUnit.Pixel))
            g.DrawString("DEAD OR ALIVE", small, solid, new RectangleF(paper.X, photoBox.Bottom + 0.2f * k, w, 2.2f * k), format);
        using (var big = new Font("Bahnschrift SemiBold", 1.75f * k, GraphicsUnit.Pixel))
            g.DrawString(bounty, big, solid, new RectangleF(paper.X, photoBox.Bottom + 2.2f * k, w, 3f * k), format);
        g.Restore(state);

        static SolidBrush paper2(float a) => new(Color.FromArgb((int)(255 * a), 236, 222, 180));
    }

    /// <summary>Peixinho de mangá (~5 mm), virado para a direita (<paramref name="facing"/> = −1 vira).</summary>
    public static void Fish(Graphics g, PointF c, float k, int facing, double tail, float alpha = 1)
    {
        var state = g.Save();
        g.TranslateTransform(c.X, c.Y);
        g.ScaleTransform(facing * k / 8, k / 8);
        using var ink = Ink(4, alpha);
        using var paper = Paper(alpha);
        float wag = (float)(6 * Math.Sin(tail));
        var tailFin = new[] { new PointF(-16, 0), new PointF(-28, -9 + wag), new PointF(-28, 9 + wag) };
        g.FillPolygon(paper, tailFin);
        g.DrawPolygon(ink, tailFin);
        g.FillEllipse(paper, -20, -9, 40, 18);
        g.DrawEllipse(ink, -20, -9, 40, 18);
        using var solid = Solid(alpha);
        g.FillEllipse(solid, 9, -4, 5, 5);
        g.DrawArc(ink, -4, -7, 10, 14, 300, 120);
        g.Restore(state);
    }

    /// <summary>
    /// News Coo: gaivota-carteiro com boné e bolsa de jornal (~16 mm de envergadura). <paramref name="flap"/> anima as
    /// asas (0–1).
    /// </summary>
    public static void NewsCoo(Graphics g, PointF c, float k, double flap, int facing = -1, float alpha = 1, bool plain = false)
    {
        var state = g.Save();
        g.TranslateTransform(c.X, c.Y);
        g.ScaleTransform(facing * k / 8, k / 8);
        using var ink = Ink(4, alpha);
        using var paper = Paper(alpha);
        using var solid = Solid(alpha);
        float up = (float)(Math.Sin(flap * Math.PI * 2) * 22);
        foreach (int side in new[] { -1, 1 })
        {
            var wing = new[] { new PointF(-6, -2), new PointF(-24, -10 - up), new PointF(-62, -18 - up * 1.4f), new PointF(-30, 6), new PointF(-6, 6) };
            if (side == 1) wing = wing.Select(p => new PointF(-p.X * 0.95f, p.Y)).ToArray();
            g.FillPolygon(paper, wing);
            g.DrawPolygon(ink, wing);
        }
        g.FillEllipse(paper, -18, -10, 40, 22);
        g.DrawEllipse(ink, -18, -10, 40, 22);
        g.FillEllipse(paper, 14, -22, 20, 18);
        g.DrawEllipse(ink, 14, -22, 20, 18);
        g.FillPolygon(solid, new[] { new PointF(32, -14), new PointF(44, -10), new PointF(32, -8) });
        g.FillEllipse(solid, 25, -17, 4, 4);
        if (plain)
        {
            g.Restore(state);
            return;
        }
        // Boné.
        g.FillPie(solid, 13, -30, 22, 16, 180, 180);
        g.FillRectangle(solid, 26, -23, 14, 3);
        // Bolsa com jornais.
        g.FillRectangle(paper, -8, 6, 18, 14);
        g.DrawRectangle(ink, -8, 6, 18, 14);
        g.DrawLine(ink, -5, 11, 7, 11);
        g.DrawLine(ink, 0, -6, -6, 8);
        g.Restore(state);
    }

    /// <summary>Balão-polvo (o que trouxe o Merry de Skypiea): cabeça redonda, rosto e tentáculos segurando as cordas.</summary>
    public static void OctopusBalloon(Graphics g, PointF top, float k, double t, float alpha = 1)
    {
        float r = 13 * k;
        var c = new PointF(top.X, top.Y + r);
        using var ink = Ink(Math.Max(2, 0.5f * k), alpha);
        using var paper = Paper(alpha);
        using var solid = Solid(alpha);
        for (int i = 0; i < 6; i++)
        {
            float x = c.X - r * 0.8f + i * r * 0.32f;
            float sway = (float)(Math.Sin(t * 2 + i) * 1.2 * k);
            g.DrawBezier(ink, x, c.Y + r * 0.7f, x + sway, c.Y + r * 1.3f, x - sway, c.Y + r * 1.7f, c.X + (x - c.X) * 0.3f, c.Y + r * 2.2f);
        }
        g.FillEllipse(paper, c.X - r, c.Y - r, 2 * r, 2 * r * 0.9f);
        g.DrawEllipse(ink, c.X - r, c.Y - r, 2 * r, 2 * r * 0.9f);
        g.FillEllipse(solid, c.X - r * 0.35f, c.Y - r * 0.05f, r * 0.16f, r * 0.2f);
        g.FillEllipse(solid, c.X + r * 0.19f, c.Y - r * 0.05f, r * 0.16f, r * 0.2f);
        g.DrawArc(ink, c.X - r * 0.25f, c.Y + r * 0.15f, r * 0.5f, r * 0.3f, 20, 140);
        using var dots = new HatchBrush(HatchStyle.Percent20, Color.FromArgb((int)(150 * alpha), 0, 0, 0), Color.Transparent);
        g.FillEllipse(dots, c.X - r * 0.9f, c.Y + r * 0.1f, r * 1.8f, r * 0.7f);
    }

    /// <summary>Faixa de narração (papel, traço e sombra dura) centrada em <paramref name="center"/>.</summary>
    public static void Caption(Graphics g, string text, PointF center, float sizePx, float alpha = 1, bool bold = true)
    {
        if (alpha <= 0.01f) return;
        using var font = new Font(bold ? "Bahnschrift SemiBold" : "Bahnschrift", sizePx, GraphicsUnit.Pixel);
        var size = g.MeasureString(text, font);
        var box = new RectangleF(center.X - size.Width / 2 - sizePx * 0.6f, center.Y - size.Height / 2 - sizePx * 0.25f, size.Width + sizePx * 1.2f, size.Height + sizePx * 0.5f);
        using var shadow = Solid(alpha);
        using var paper = Paper(alpha);
        using var ink = Ink(Math.Max(2, sizePx * 0.09f), alpha);
        g.FillRectangle(shadow, box.X + sizePx * 0.18f, box.Y + sizePx * 0.18f, box.Width, box.Height);
        g.FillRectangle(paper, box);
        g.DrawRectangle(ink, box.X, box.Y, box.Width, box.Height);
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
        g.DrawString(text, font, shadow, box.X + sizePx * 0.6f, box.Y + sizePx * 0.25f);
    }

    /// <summary>
    /// Onomatopeia carimbada (contorno branco grosso, traço preto, miolo branco), entrando com um "pop" e tremendo de
    /// leve. <paramref name="x"/> = segundos desde que entrou.
    /// </summary>
    public static void Stamp(Graphics g, string text, double x, PointF center, float angle, float heightPx, double hold = 0.8, float alpha = 1)
    {
        if (x < 0 || x > hold + 0.3) return;
        var sprite = Sfx(text);
        float pop = x < 0.12 ? (float)(1.3 - 0.3 * x / 0.12) : 1;
        float a = alpha * (float)Math.Clamp((hold + 0.3 - x) / 0.3, 0, 1);
        float wobble = (float)(2 * Math.Sin(x * 2 * Math.PI * 1.6)); // ±2°, devagar
        float scale = heightPx / sprite.Height * pop;
        var state = g.Save();
        g.TranslateTransform(center.X, center.Y);
        g.RotateTransform(angle + wobble);
        g.ScaleTransform(scale, scale);
        using var attributes = new ImageAttributes();
        attributes.SetColorMatrix(new ColorMatrix { Matrix33 = Math.Clamp(a, 0, 1) });
        g.InterpolationMode = InterpolationMode.Bilinear;
        g.DrawImage(sprite, new Rectangle(-sprite.Width / 2, -sprite.Height / 2, sprite.Width, sprite.Height), 0, 0, sprite.Width, sprite.Height, GraphicsUnit.Pixel, attributes);
        g.Restore(state);
    }

    private static Bitmap Sfx(string text)
    {
        lock (SfxLock)
        {
            if (SfxCache.TryGetValue(text, out var cached)) return cached;
            _japanese ??= JapaneseFamily();
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
            SfxCache[text] = bitmap;
            return bitmap;
        }
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

    /// <summary>Olho do Laboon ocupando a tela (com pálpebra, cicatrizes e a pele riscada), subindo devagar.</summary>
    public static void LaboonEye(Graphics g, RectangleF screen, float rise, float alpha = 1)
    {
        float cx = screen.X + screen.Width * 0.5f, cy = screen.Y + screen.Height * (1.25f - 1.6f * rise);
        float r = screen.Height * 0.62f;
        using var skin = new SolidBrush(Color.FromArgb((int)(235 * alpha), 34, 36, 42));
        using var light = new Pen(Color.FromArgb((int)(140 * alpha), 200, 205, 215), 3);
        g.FillEllipse(skin, cx - r * 2.4f, cy - r * 1.6f, r * 4.8f, r * 3.6f);
        for (int i = 0; i < 9; i++)
        {
            float y = cy - r * 1.2f + i * r * 0.32f;
            g.DrawArc(light, cx - r * 2.2f, y, r * 4.4f, r * 0.5f, 200, 140);
        }
        using var white = new SolidBrush(Color.FromArgb((int)(255 * alpha), 236, 236, 230));
        using var ink = Ink(6, alpha);
        g.FillEllipse(white, cx - r * 0.55f, cy - r * 0.32f, r * 1.1f, r * 0.64f);
        g.DrawEllipse(ink, cx - r * 0.55f, cy - r * 0.32f, r * 1.1f, r * 0.64f);
        using var pupil = Solid(alpha);
        g.FillEllipse(pupil, cx - r * 0.18f, cy - r * 0.2f, r * 0.36f, r * 0.4f);
        using var glint = Paper(alpha);
        g.FillEllipse(glint, cx - r * 0.06f, cy - r * 0.15f, r * 0.1f, r * 0.1f);
        g.DrawArc(ink, cx - r * 0.7f, cy - r * 0.55f, r * 1.4f, r * 0.7f, 200, 140); // pálpebra
        using var scar = Ink(5, alpha);
        g.DrawLine(scar, cx + r * 0.75f, cy - r * 0.9f, cx + r * 1.2f, cy + r * 0.2f);
        g.DrawLine(scar, cx + r * 0.95f, cy - r * 0.8f, cx + r * 1.35f, cy + r * 0.05f);
    }

    /// <summary>Raios de luz entrando pela superfície (feixes suaves, inclinados conforme o sol).</summary>
    public static void LightShafts(Graphics g, RectangleF screen, double t, float tilt, float alpha)
    {
        if (alpha <= 0.01f) return;
        for (int i = 0; i < 6; i++)
        {
            float x = screen.X + screen.Width * (0.08f + 0.17f * i) + (float)(14 * Math.Sin(t * 0.3 + i * 1.7));
            float w = screen.Width * (0.05f + 0.03f * (float)Choreo.Hash(i + 77));
            var shaft = new[]
            {
                new PointF(x - w / 2, screen.Y), new PointF(x + w / 2, screen.Y),
                new PointF(x + w + tilt, screen.Bottom), new PointF(x - w + tilt * 1.1f, screen.Bottom),
            };
            using var brush = new LinearGradientBrush(new RectangleF(screen.X, screen.Y, screen.Width, screen.Height + 1),
                Color.FromArgb((int)(70 * alpha * (0.6 + 0.4 * Choreo.SlowNoise(t, 0.3, i * 13))), 255, 255, 255), Color.FromArgb(0, 255, 255, 255), LinearGradientMode.Vertical);
            g.FillPolygon(brush, shaft);
        }
    }
}
