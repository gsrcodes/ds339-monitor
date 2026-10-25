using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace DS339Monitor.Themes;

/// <summary>
/// Arte de mangá desenhada por código (só System.Drawing / GDI+): o Thousand Sunny, o mar, as nuvens, o sol e um
/// elenco de pequenos atores. Preto e branco, traço de tinta de 2 a 5 px, retícula de pontos e hachura; a versão
/// noturna é silhueta escura com contorno claro. Cada método devolve um Bitmap novo (32bppPArgb, fundo
/// transparente) — quem chama é dono dele e dá Dispose. Gere uma vez e reaproveite: nada aqui é para cada quadro.
/// </summary>
internal static class MangaArt
{
    #region Paleta, pincéis e utilitários

    private sealed class Palette
    {
        public Color Ink;     // contorno forte
        public Color Line;    // traço interno (mais fino)
        public Color Paper;   // preenchimento
        public Color Tone;    // pontos da retícula e hachura
        public Color Solid;   // massas pretas (bandeira, listras, pupilas)
        public Color Window;  // janelas e escotilhas (à noite, acesas)
        public bool Night;
    }

    private static readonly Palette Day = new()
    {
        Ink = Color.FromArgb(20, 20, 24), Line = Color.FromArgb(20, 20, 24), Paper = Color.White,
        Tone = Color.FromArgb(20, 20, 24), Solid = Color.FromArgb(20, 20, 24), Window = Color.FromArgb(64, 64, 72),
    };

    private static readonly Palette Night = new()
    {
        Ink = Color.FromArgb(214, 222, 238), Line = Color.FromArgb(138, 148, 172), Paper = Color.FromArgb(38, 42, 56),
        Tone = Color.FromArgb(14, 16, 24), Solid = Color.FromArgb(18, 20, 29), Window = Color.FromArgb(255, 212, 138), Night = true,
    };

    private static readonly Palette StormCloud = new()
    {
        Ink = Color.FromArgb(178, 186, 204), Line = Color.FromArgb(150, 158, 178), Paper = Color.FromArgb(62, 66, 82),
        Tone = Color.FromArgb(24, 26, 36), Solid = Color.FromArgb(24, 26, 36), Window = Color.FromArgb(24, 26, 36), Night = true,
    };

    /// <summary>Pincel de um desenho: guarda a paleta, a escala (para a retícula ter passo fixo em pixels) e descarta o que criou.</summary>
    private sealed class Painter : IDisposable
    {
        public readonly Graphics G;
        public readonly Palette P;
        public readonly float S;
        private readonly List<IDisposable> _keep = new();
        private readonly Dictionary<int, Brush> _dots = new();

        public Painter(Graphics g, Palette p, float s) { G = g; P = p; S = s; }

        public T Keep<T>(T d) where T : IDisposable { _keep.Add(d); return d; }
        public Pen Pen(Color c, float w) => Keep(new Pen(c, w) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round });
        public Pen Ink(float w) => Pen(P.Ink, w);
        public Pen Line(float w) => Pen(P.Line, w);
        public Brush Fill(Color c) => Keep(new SolidBrush(c));
        public Brush Paper => Fill(P.Paper);
        public Brush Solid => Fill(P.Solid);
        public Brush Window => Fill(P.Window);

        /// <summary>Retícula: 1 = clara, 2 = média, 3 = escura, 4 = quase chapada.</summary>
        public Brush Dots(int level)
        {
            if (_dots.TryGetValue(level, out var b)) return b;
            float r = level switch { 1 => 0.95f, 2 => 1.4f, 3 => 1.95f, _ => 2.45f };
            b = Keep(DotBrush(P.Tone, 6, r, S));
            _dots[level] = b;
            return b;
        }

        public void Shape(GraphicsPath path, Brush fill, float w) { G.FillPath(fill, path); G.DrawPath(Ink(w), path); }

        /// <summary>Contorno só por fora: traço grosso embaixo e o preenchimento por cima (as sobreposições internas somem).</summary>
        public void Blob(GraphicsPath path, Brush fill, float w) { G.DrawPath(Ink(2 * w), path); G.FillPath(fill, path); }

        public void Tone(GraphicsPath path, int level) => G.FillPath(Dots(level), path);

        public void Tone(Region region, int level) => G.FillRegion(Dots(level), region);

        /// <summary>Faixa branca com borda de tinta dos dois lados (frisos, cordas, mastros).</summary>
        public void Band(GraphicsPath path, float width, float edge, Color? fill = null)
        {
            G.DrawPath(Ink(width + 2 * edge), path);
            G.DrawPath(Pen(fill ?? P.Paper, width), path);
        }

        public void Band(PointF a, PointF b, float width, float edge, Color? fill = null)
        {
            using var path = new GraphicsPath();
            path.AddLine(a, b);
            Band(path, width, edge, fill);
        }

        /// <summary>Hachura: linhas paralelas recortadas por uma área.</summary>
        public void Hatch(GraphicsPath clip, float angleDeg, float spacing, float w, Color? color = null)
        {
            using var region = new Region(clip);
            Hatch(region, clip.GetBounds(), angleDeg, spacing, w, color);
        }

        public void Hatch(Region clip, RectangleF bounds, float angleDeg, float spacing, float w, Color? color = null)
        {
            var state = G.Save();
            G.SetClip(clip, CombineMode.Intersect);
            var pen = Pen(color ?? P.Tone, w);
            double a = angleDeg * Math.PI / 180;
            float dx = (float)Math.Cos(a), dy = (float)Math.Sin(a);
            float cx = bounds.X + bounds.Width / 2, cy = bounds.Y + bounds.Height / 2;
            float half = (float)Math.Sqrt(bounds.Width * bounds.Width + bounds.Height * bounds.Height) / 2 + 2;
            for (float t = -half; t <= half; t += spacing)
            {
                float ox = cx - dy * t, oy = cy + dx * t;
                G.DrawLine(pen, ox - dx * half, oy - dy * half, ox + dx * half, oy + dy * half);
            }
            G.Restore(state);
        }

        public void Dispose()
        {
            foreach (var d in _keep) d.Dispose();
        }
    }

    private static TextureBrush DotBrush(Color c, int pitch, float radius, float worldScale)
    {
        using var tile = new Bitmap(pitch, pitch, PixelFormat.Format32bppPArgb);
        using (var g = Graphics.FromImage(tile))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            using var b = new SolidBrush(c);
            float h = pitch / 2f;
            foreach (var (x, y) in new[] { (h, h), (0f, 0f), ((float)pitch, 0f), (0f, (float)pitch), ((float)pitch, (float)pitch) })
                g.FillEllipse(b, x - radius, y - radius, 2 * radius, 2 * radius);
        }
        var brush = new TextureBrush(tile, WrapMode.Tile);
        brush.ScaleTransform(1 / worldScale, 1 / worldScale);
        return brush;
    }

    private static Bitmap NewCanvas(int w, int h, out Graphics g)
    {
        var bmp = new Bitmap(Math.Max(1, w), Math.Max(1, h), PixelFormat.Format32bppPArgb);
        g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.CompositingQuality = CompositingQuality.HighQuality;
        return bmp;
    }

    private static PointF Pt(float x, float y) => new(x, y);

    private static PointF Polar(PointF c, float r, double deg, float kx = 1)
    {
        double a = deg * Math.PI / 180;
        return new PointF(c.X + (float)(Math.Cos(a) * r) * kx, c.Y + (float)(Math.Sin(a) * r));
    }

    private static PointF Lerp(PointF a, PointF b, float t) => new(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);

    private static double Hash(int n)
    {
        unchecked
        {
            uint x = (uint)n * 747796405u + 2891336453u;
            x = ((x >> (int)((x >> 28) + 4)) ^ x) * 277803737u;
            x = (x >> 22) ^ x;
            return x / 4294967296.0;
        }
    }

    private static GraphicsPath Ellipse(float cx, float cy, float rx, float ry)
    {
        var p = new GraphicsPath();
        p.AddEllipse(cx - rx, cy - ry, 2 * rx, 2 * ry);
        return p;
    }

    private static GraphicsPath Shifted(GraphicsPath path, float dx, float dy)
    {
        var p = (GraphicsPath)path.Clone();
        using var m = new Matrix();
        m.Translate(dx, dy);
        p.Transform(m);
        return p;
    }

    /// <summary>Área de sombra: a forma menos ela mesma deslocada na direção da luz (sobra uma lua crescente do lado oposto).</summary>
    private static Region Crescent(GraphicsPath path, float towardLightX, float towardLightY)
    {
        var r = new Region(path);
        using var lit = Shifted(path, towardLightX, towardLightY);
        r.Exclude(lit);
        return r;
    }

    /// <summary>
    /// Acabamento comum: contorno externo (silhueta) de raio <paramref name="radius"/> na cor dada, por baixo da
    /// arte. É o que dá o "traço grosso por fora" do mangá — e, à noite, o contorno claro em volta da silhueta.
    /// </summary>
    private static Bitmap Finish(Bitmap art, Color rim, float radius)
    {
        int w = art.Width, h = art.Height;
        var px = new int[w * h];
        var rect = new Rectangle(0, 0, w, h);
        var data = art.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        Marshal.Copy(data.Scan0, px, 0, px.Length);
        art.UnlockBits(data);
        art.Dispose();

        var alpha = new byte[px.Length];
        for (int i = 0; i < px.Length; i++) alpha[i] = (byte)((uint)px[i] >> 24);
        int R = (int)Math.Ceiling(radius + 0.5f);
        var offs = new List<(int dx, int dy, float cov)>();
        for (int dy = -R; dy <= R; dy++)
            for (int dx = -R; dx <= R; dx++)
            {
                float cov = radius + 0.5f - (float)Math.Sqrt(dx * dx + dy * dy);
                if (cov > 0) offs.Add((dx, dy, Math.Min(1, cov)));
            }
        offs.Sort((a, b) => b.cov.CompareTo(a.cov));
        var o = offs.ToArray();

        var result = new int[px.Length];
        float rr = rim.R, rg = rim.G, rb = rim.B;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int i = y * w + x;
                int sa = alpha[i];
                float ro = 0;
                if (sa < 255)
                {
                    float best = 0;
                    for (int k = 0; k < o.Length; k++)
                    {
                        int nx = x + o[k].dx, ny = y + o[k].dy;
                        if ((uint)nx >= (uint)w || (uint)ny >= (uint)h) continue;
                        float v = alpha[ny * w + nx] * o[k].cov;
                        if (v > best) { best = v; if (best >= 255) break; }
                    }
                    ro = best / 255f;
                }
                float fa = sa / 255f;
                float oa = fa + ro * (1 - fa);
                if (oa <= 0.001f) { result[i] = 0; continue; }
                int c = px[i];
                float cr = ((c >> 16) & 255) * fa + rr * ro * (1 - fa);
                float cg = ((c >> 8) & 255) * fa + rg * ro * (1 - fa);
                float cb = (c & 255) * fa + rb * ro * (1 - fa);
                int A = (int)(oa * 255 + 0.5f);
                result[i] = (A << 24) | ((int)(cr / oa + 0.5f) << 16) | ((int)(cg / oa + 0.5f) << 8) | (int)(cb / oa + 0.5f);
            }
        }

        using var tmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
        var d2 = tmp.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        Marshal.Copy(result, 0, d2.Scan0, result.Length);
        tmp.UnlockBits(d2);
        var final = new Bitmap(w, h, PixelFormat.Format32bppPArgb);
        using (var g = Graphics.FromImage(final))
        {
            g.CompositingMode = CompositingMode.SourceCopy;
            g.DrawImage(tmp, rect);
        }
        return final;
    }

    /// <summary>Menor/maior Y de um contorno fechado na coluna x (para desenhar tábuas que acompanham o casco).</summary>
    private static float YAt(PointF[] poly, float x, bool lowest)
    {
        float best = float.NaN;
        for (int i = 0; i < poly.Length; i++)
        {
            var a = poly[i];
            var b = poly[(i + 1) % poly.Length];
            if ((a.X <= x && b.X > x) || (b.X <= x && a.X > x))
            {
                float y = a.Y + (b.Y - a.Y) * (x - a.X) / (b.X - a.X);
                if (float.IsNaN(best) || (lowest ? y > best : y < best)) best = y;
            }
        }
        return best;
    }

    private static PointF[] Flat(GraphicsPath path)
    {
        using var p = (GraphicsPath)path.Clone();
        p.Flatten(null, 0.4f);
        return p.PathPoints;
    }

    #endregion

    #region Thousand Sunny

    // Coordenadas de desenho (escala 1): convés do gramado em D, quilha em K, linha d'água em WL.
    private const float SunnyW = 488, SunnyH = 396, SunnyPad = 8;
    private const float D = 290, K = D + 86, WL = D + 64, HullMidX = 224;

    /// <summary>
    /// O Thousand Sunny de lado (proa para a direita), em traço de mangá: casco redondo de madeira com os painéis
    /// de retícula e escotilhas, a escotilha "1" do Soldier Dock, o gramado, o mastro de proa com a vela do Jolly
    /// Roger e o cesto da gávea em cúpula, o castelo de popa com as mexeriqueiras, a torre da popa com lampiões, o
    /// mastro de ré com a vela listrada, e a figura de proa de leão com juba de pétalas e ossos cruzados.
    /// </summary>
    /// <param name="scale">1 = cerca de 500 x 410 px.</param>
    /// <param name="night">Silhueta escura com contorno claro e janelas acesas.</param>
    /// <param name="waterlineAnchor">Ponto da linha d'água sob o meio do casco, em pixels do bitmap.</param>
    public static Bitmap Sunny(float scale, bool night, out PointF waterlineAnchor)
    {
        var pal = night ? Night : Day;
        var bmp = NewCanvas((int)Math.Ceiling((SunnyW + 2 * SunnyPad) * scale), (int)Math.Ceiling((SunnyH + 2 * SunnyPad) * scale), out var g);
        using (g)
        {
            g.ScaleTransform(scale, scale);
            g.TranslateTransform(SunnyPad, SunnyPad);
            using var p = new Painter(g, pal, scale);
            DrawSunny(p);
        }
        waterlineAnchor = new PointF((HullMidX + SunnyPad) * scale, (WL + SunnyPad) * scale);
        return Finish(bmp, pal.Ink, Math.Max(1.6f, 2.2f * scale));
    }

    private static void DrawSunny(Painter p)
    {
        RearMast(p);
        SternTower(p);
        AftCastle(p);
        Lawn(p);
        ForeMast(p);
        MainSail(p);
        Helm(p, Pt(326, D - 82), 12);
        Hull(p);
        Paddle(p);
        CrowsNest(p);
        Figurehead(p, Pt(404, D - 66));
    }

    private static void Pole(Painter p, PointF a, PointF b, float w)
    {
        p.Band(a, b, w, 1.8f);
        // Veio da madeira de um lado.
        float dx = b.X - a.X, dy = b.Y - a.Y, len = (float)Math.Sqrt(dx * dx + dy * dy);
        float nx = -dy / len * w * 0.22f, ny = dx / len * w * 0.22f;
        p.G.DrawLine(p.Line(1.1f), a.X + nx, a.Y + ny, b.X + nx, b.Y + ny);
    }

    private static void RearMast(Painter p)
    {
        const float mx = 128;
        Pole(p, Pt(mx, D - 70), Pt(mx, D - 206), 8);

        // Vela listrada de ré (a "gávea" de carangueja): leque entre o mastro e a carangueja.
        var peak = Pt(mx, D - 188); var gaffEnd = Pt(50, D - 216); var clew = Pt(60, D - 160); var tack = Pt(mx, D - 136);
        using var sail = new GraphicsPath();
        sail.AddLine(peak, gaffEnd);
        sail.AddBezier(gaffEnd, Pt(38, D - 196), Pt(44, D - 174), clew);
        sail.AddBezier(clew, Pt(84, D - 150), Pt(106, D - 140), tack);
        sail.CloseFigure();
        p.G.FillPath(p.Paper, sail);
        var state = p.G.Save();
        p.G.SetClip(sail, CombineMode.Intersect);
        for (int i = 0; i < 7; i++)
        {
            if (i % 2 == 0) continue;
            float t0 = i / 7f, t1 = (i + 1) / 7f;
            using var stripe = new GraphicsPath();
            stripe.AddPolygon(new[]
            {
                Lerp(peak, gaffEnd, t0) with { Y = Lerp(peak, gaffEnd, t0).Y - 6 }, Lerp(peak, gaffEnd, t1) with { Y = Lerp(peak, gaffEnd, t1).Y - 6 },
                Lerp(tack, clew, t1) with { Y = Lerp(tack, clew, t1).Y + 10 }, Lerp(tack, clew, t0) with { Y = Lerp(tack, clew, t0).Y + 10 },
            });
            p.G.FillPath(p.Solid, stripe);
        }
        p.G.Restore(state);
        p.G.DrawPath(p.Ink(2.6f), sail);
        Pole(p, Pt(mx + 2, D - 190), Pt(42, D - 220), 4.5f);
        Pole(p, Pt(mx + 2, D - 136), Pt(54, D - 162), 4f);

        // Pináculo e uma flâmula fininha.
        p.G.DrawLine(p.Ink(2.2f), mx, D - 206, mx, D - 222);
        using var pennant = new GraphicsPath();
        pennant.AddBezier(Pt(mx, D - 222), Pt(mx + 8, D - 226), Pt(mx + 14, D - 218), Pt(mx + 24, D - 221));
        pennant.AddBezier(Pt(mx + 24, D - 221), Pt(mx + 14, D - 214), Pt(mx + 8, D - 220), Pt(mx, D - 216));
        pennant.CloseFigure();
        p.Shape(pennant, p.Solid, 1.6f);
        using var knob = Ellipse(mx, D - 207, 3.5f, 3.5f);
        p.Shape(knob, p.Paper, 1.8f);
    }

    private static void SternTower(Painter p)
    {
        // Pedestal em taça.
        using (var ped = new GraphicsPath())
        {
            ped.AddBezier(Pt(50, D - 30), Pt(50, D - 54), Pt(42, D - 76), Pt(30, D - 90));
            ped.AddLine(Pt(30, D - 90), Pt(102, D - 90));
            ped.AddBezier(Pt(102, D - 90), Pt(90, D - 76), Pt(82, D - 54), Pt(82, D - 30));
            ped.CloseFigure();
            p.Shape(ped, p.Paper, 3);
            using var shade = Crescent(ped, 12, 0);
            p.Tone(shade, 2);
            p.G.DrawBezier(p.Line(1.4f), Pt(62, D - 32), Pt(62, D - 56), Pt(56, D - 76), Pt(50, D - 89));
            p.G.DrawBezier(p.Line(1.4f), Pt(70, D - 32), Pt(70, D - 56), Pt(76, D - 76), Pt(82, D - 89));
        }

        // Sala (biblioteca / observatório): cilindro com janelas em arco.
        using (var room = new GraphicsPath())
        {
            room.AddRectangle(new RectangleF(34, D - 118, 64, 28));
            p.Shape(room, p.Paper, 3);
            for (int i = 0; i < 4; i++) ArchWindow(p, 42 + i * 14.5f, D - 113, 8, 16);
            using var shade = new GraphicsPath();
            shade.AddRectangle(new RectangleF(34, D - 118, 7, 28));
            p.Tone(shade, 2);
        }
        Dome(p, 66, D - 118, 34, 30, 6, 1);

        // Sacada com balaústres e os dois lampiões.
        p.G.DrawLine(p.Ink(2.4f), 26, D - 101, 106, D - 101);
        for (float x = 28; x <= 104; x += 6) p.G.DrawLine(p.Line(1.4f), x, D - 101, x, D - 90);
        p.Band(Pt(24, D - 90), Pt(108, D - 90), 4, 1.6f);
        Lantern(p, Pt(24, D - 90), -1);
        Lantern(p, Pt(108, D - 90), 1);
    }

    private static void Lantern(Painter p, PointF foot, int side)
    {
        using var post = new GraphicsPath();
        post.AddBezier(foot, Pt(foot.X, foot.Y - 14), Pt(foot.X + side * 8, foot.Y - 18), Pt(foot.X + side * 6, foot.Y - 30));
        p.G.DrawPath(p.Ink(2.4f), post);
        var top = Pt(foot.X + side * 6, foot.Y - 30);
        using var body = new GraphicsPath();
        body.AddPolygon(new[] { Pt(top.X - 4, top.Y - 2), Pt(top.X + 4, top.Y - 2), Pt(top.X + 5, top.Y - 12), Pt(top.X - 5, top.Y - 12) });
        p.Shape(body, p.Window, 1.8f);
        using var cap = new GraphicsPath();
        cap.AddPolygon(new[] { Pt(top.X - 7, top.Y - 12), Pt(top.X + 7, top.Y - 12), Pt(top.X, top.Y - 19) });
        p.Shape(cap, p.Solid, 1.6f);
        if (p.P.Night) Glow(p, Pt(top.X, top.Y - 7), 16);
    }

    private static void Glow(Painter p, PointF c, float r)
    {
        using var path = Ellipse(c.X, c.Y, r, r);
        using var brush = new PathGradientBrush(path)
        {
            CenterColor = Color.FromArgb(120, p.P.Window),
            SurroundColors = new[] { Color.FromArgb(0, p.P.Window) },
        };
        p.G.FillPath(brush, path);
    }

    private static void ArchWindow(Painter p, float x, float y, float w, float h)
    {
        using var win = new GraphicsPath();
        win.AddArc(x, y, w, w, 180, 180);
        win.AddLine(x + w, y + w / 2, x + w, y + h);
        win.AddLine(x + w, y + h, x, y + h);
        win.CloseFigure();
        p.Shape(win, p.Window, 1.8f);
        p.G.DrawLine(p.Line(1.1f), x + w / 2, y + 1, x + w / 2, y + h);
        p.G.DrawLine(p.Line(1.1f), x, y + h * 0.55f, x + w, y + h * 0.55f);
    }

    /// <summary>Cúpula listrada (gomos alternando retícula e branco) com uma ou duas trapeiras que parecem olhos.</summary>
    private static void Dome(Painter p, float cx, float baseY, float rx, float ry, int gores, int eyes)
    {
        using var dome = new GraphicsPath();
        dome.AddArc(cx - rx, baseY - ry, 2 * rx, 2 * ry, 180, 180);
        dome.CloseFigure();
        p.G.FillPath(p.Paper, dome);
        // Meridianos: x = cx + rx·sen(φ)·cos(lat), y = base − ry·sen(lat).
        PointF[] Meridian(double phi)
        {
            var pts = new PointF[13];
            for (int i = 0; i <= 12; i++)
            {
                double lat = i / 12.0 * Math.PI / 2;
                pts[i] = Pt(cx + (float)(rx * Math.Sin(phi) * Math.Cos(lat)), baseY - (float)(ry * Math.Sin(lat)));
            }
            return pts;
        }
        var state = p.G.Save();
        p.G.SetClip(dome, CombineMode.Intersect);
        for (int i = 0; i < gores; i++)
        {
            double a0 = -Math.PI / 2 + Math.PI * i / gores, a1 = -Math.PI / 2 + Math.PI * (i + 1) / gores;
            if (i % 2 == 0)
            {
                var m0 = Meridian(a0);
                var m1 = Meridian(a1);
                Array.Reverse(m1);
                using var gore = new GraphicsPath();
                gore.AddLines(m0);
                gore.AddLines(m1);
                gore.CloseFigure();
                p.Tone(gore, 3);
            }
            if (i > 0) p.G.DrawLines(p.Line(1.3f), Meridian(a0));
        }
        p.G.Restore(state);
        p.G.DrawPath(p.Ink(3), dome);
        p.Band(Pt(cx - rx - 2, baseY), Pt(cx + rx + 2, baseY), 3.5f, 1.4f);
        // Trapeiras.
        for (int e = 0; e < eyes; e++)
        {
            float ex = eyes == 1 ? cx - rx * 0.1f : cx + (e == 0 ? -rx * 0.42f : rx * 0.42f);
            float ew = rx * 0.36f, eh = ry * 0.42f;
            using var dormer = new GraphicsPath();
            dormer.AddArc(ex - ew / 2, baseY - eh - 2, ew, ew * 0.9f, 180, 180);
            dormer.AddLine(ex + ew / 2, baseY - eh - 2 + ew * 0.45f, ex + ew / 2, baseY - 2);
            dormer.AddLine(ex + ew / 2, baseY - 2, ex - ew / 2, baseY - 2);
            dormer.CloseFigure();
            p.Shape(dormer, p.Paper, 2);
            using var hole = new GraphicsPath();
            float iw = ew * 0.55f;
            hole.AddArc(ex - iw / 2, baseY - eh + 1, iw, iw, 180, 180);
            hole.AddLine(ex + iw / 2, baseY - eh + 1 + iw / 2, ex + iw / 2, baseY - 4);
            hole.AddLine(ex + iw / 2, baseY - 4, ex - iw / 2, baseY - 4);
            hole.CloseFigure();
            p.G.FillPath(p.Window, hole);
        }
        // Pináculo.
        using var knob = Ellipse(cx, baseY - ry - 2, 3.5f, 3.5f);
        p.Shape(knob, p.Paper, 2);
    }

    private static void AftCastle(Painter p)
    {
        // Mexeriqueiras da Nami no terraço (atrás da grade).
        MikanTree(p, Pt(108, D - 100), 16, 1);
        MikanTree(p, Pt(154, D - 97), 13, 2);

        using (var block = new GraphicsPath())
        {
            block.AddRectangle(new RectangleF(92, D - 76, 82, 46));
            p.Shape(block, p.Paper, 3);
            for (int i = 0; i < 3; i++) ArchWindow(p, 104 + i * 22, D - 68, 12, 24);
            using var shade = new GraphicsPath();
            shade.AddRectangle(new RectangleF(92, D - 76, 82, 8));
            p.Tone(shade, 2);
        }
        // Grade do terraço.
        p.G.DrawLine(p.Ink(2.4f), 88, D - 90, 178, D - 90);
        for (float x = 91; x <= 176; x += 6) p.G.DrawLine(p.Line(1.5f), x, D - 90, x, D - 77);
        p.Band(Pt(86, D - 76), Pt(180, D - 76), 4, 1.6f);
    }

    private static void MikanTree(Painter p, PointF c, float r, int seed)
    {
        using var crown = new GraphicsPath(FillMode.Winding);
        crown.AddEllipse(c.X - r * 0.8f, c.Y - r * 0.8f, r * 1.6f, r * 1.6f);
        for (int i = 0; i < 8; i++)
        {
            var q = Polar(c, r * 0.72f, i * 45 + seed * 13);
            float lr = r * (0.38f + 0.08f * (float)Hash(seed * 31 + i));
            crown.AddEllipse(q.X - lr, q.Y - lr, 2 * lr, 2 * lr);
        }
        p.Blob(crown, p.Paper, 1.6f);
        using var shade = Crescent(crown, 5, -6);
        p.Tone(shade, 2);
        for (int i = 0; i < 6; i++)
        {
            var q = Polar(c, r * (0.2f + 0.6f * (float)Hash(seed * 7 + i)), Hash(seed * 11 + i) * 360);
            using var fruit = Ellipse(q.X, q.Y, 2.4f, 2.4f);
            p.Shape(fruit, p.Dots(3), 1.2f);
        }
    }

    private static void Lawn(Painter p)
    {
        // Grade do outro lado do gramado.
        p.G.DrawLine(p.Ink(2.2f), 166, D - 28, 300, D - 28);
        for (float x = 169; x <= 298; x += 7) p.G.DrawLine(p.Line(1.4f), x, D - 28, x, D - 6);
        // Grama: faixa com borda de tufinhos.
        using var grass = new GraphicsPath();
        var top = new List<PointF>();
        for (float x = 166; x <= 304; x += 4)
        {
            bool tip = ((int)(x / 4)) % 2 == 0;
            top.Add(Pt(x, D - 8 - (tip ? 5 + 3 * (float)Hash((int)x) : 0)));
        }
        grass.AddLines(top.ToArray());
        grass.AddLine(304, D - 8, 304, D + 30);
        grass.AddLine(304, D + 30, 166, D + 30);
        grass.CloseFigure();
        p.Shape(grass, p.Paper, 1.8f);
        using var shade = new GraphicsPath();
        shade.AddRectangle(new RectangleF(166, D - 2, 138, 32));
        p.Tone(shade, 1);
    }

    private static void ForeMast(Painter p)
    {
        Pole(p, Pt(270, D - 4), Pt(270, D - 214), 10);
        p.Band(Pt(270, D - 34), Pt(270, D - 26), 12, 1.4f, p.P.Solid);
        // Escadas de corda (enfrechates).
        RopeLadder(p, Pt(266, D - 146), Pt(212, D - 4));
        RopeLadder(p, Pt(276, D - 146), Pt(316, D - 64));
    }

    private static void RopeLadder(Painter p, PointF top, PointF bottom)
    {
        var pen = p.Ink(1.6f);
        var a0 = Pt(top.X - 2, top.Y); var a1 = Pt(bottom.X - 6, bottom.Y);
        var b0 = Pt(top.X + 2, top.Y); var b1 = Pt(bottom.X + 6, bottom.Y);
        p.G.DrawLine(pen, a0, a1);
        p.G.DrawLine(pen, b0, b1);
        var rung = p.Line(1.1f);
        for (float t = 0.06f; t < 1; t += 0.07f) p.G.DrawLine(rung, Lerp(a0, a1, t), Lerp(b0, b1, t));
    }

    private static void MainSail(Painter p)
    {
        using var sail = new GraphicsPath();
        sail.AddBezier(Pt(198, D - 146), Pt(246, D - 150), Pt(296, D - 152), Pt(342, D - 150));
        sail.AddBezier(Pt(342, D - 150), Pt(352, D - 118), Pt(350, D - 76), Pt(336, D - 44));
        sail.AddBezier(Pt(336, D - 44), Pt(296, D - 60), Pt(244, D - 58), Pt(204, D - 40));
        sail.AddBezier(Pt(204, D - 40), Pt(190, D - 74), Pt(190, D - 116), Pt(198, D - 146));
        sail.CloseFigure();
        p.G.FillPath(p.Paper, sail);
        // Sombra da barriga do pano (lado esquerdo e embaixo) e costuras.
        using (var shade = Crescent(sail, 22, -10)) p.Tone(shade, 1);
        using (var shade2 = Crescent(sail, 7, -4)) p.Tone(shade2, 2);
        var seam = p.Line(1.2f);
        for (int i = 1; i < 6; i++)
        {
            float t = i / 6f;
            float xTop = 198 + 144 * t, xBot = 204 + 132 * t;
            float bulge = (float)Math.Sin(Math.PI * t) * 6;
            p.G.DrawBezier(seam, Pt(xTop, D - 148 - 2 * (float)Math.Sin(Math.PI * t)), Pt(xTop + bulge, D - 114), Pt(xBot + bulge, D - 80), Pt(xBot, D - 46 - 12 * (float)Math.Sin(Math.PI * t)));
        }
        JollyRoger(p, Pt(270, D - 100), 15.5f, false);
        p.G.DrawPath(p.Ink(3), sail);
        // Verga com as amarrações.
        Pole(p, Pt(186, D - 146), Pt(354, D - 151), 6);
        for (int i = 0; i < 6; i++)
        {
            float x = 206 + i * 26;
            p.G.DrawLine(p.Line(1.4f), x - 2, D - 152, x + 2, D - 143);
        }
        // Escotas.
        p.G.DrawLine(p.Ink(1.4f), 204, D - 40, 196, D - 12);
        p.G.DrawLine(p.Ink(1.4f), 336, D - 44, 322, D - 70);
    }

    /// <summary>Jolly Roger dos Chapéus de Palha: caveira, ossos cruzados atrás e o chapéu de palha com a fita.</summary>
    private static void JollyRoger(Painter p, PointF c, float s, bool onFlag)
    {
        // Na bandeira (preta) a caveira é clara; à noite usa o traço claro para continuar legível.
        var bone = onFlag && p.P.Night ? p.Fill(p.P.Ink) : p.Paper;
        float lw = onFlag ? 0 : Math.Max(1.4f, s * 0.13f);
        void Bone(PointF a, PointF b)
        {
            using var path = new GraphicsPath(FillMode.Winding);
            float dx = b.X - a.X, dy = b.Y - a.Y, len = (float)Math.Sqrt(dx * dx + dy * dy);
            float nx = -dy / len, ny = dx / len, t = s * 0.17f;
            path.AddPolygon(new[] { Pt(a.X + nx * t, a.Y + ny * t), Pt(b.X + nx * t, b.Y + ny * t), Pt(b.X - nx * t, b.Y - ny * t), Pt(a.X - nx * t, a.Y - ny * t) });
            float kr = s * 0.2f;
            foreach (var e in new[] { a, b })
            {
                float sx = e == a ? 1 : -1;
                var k1 = Pt(e.X + nx * kr * 0.9f + dx / len * kr * 0.3f * sx, e.Y + ny * kr * 0.9f + dy / len * kr * 0.3f * sx);
                var k2 = Pt(e.X - nx * kr * 0.9f + dx / len * kr * 0.3f * sx, e.Y - ny * kr * 0.9f + dy / len * kr * 0.3f * sx);
                path.AddEllipse(k1.X - kr, k1.Y - kr, 2 * kr, 2 * kr);
                path.AddEllipse(k2.X - kr, k2.Y - kr, 2 * kr, 2 * kr);
            }
            if (lw > 0) p.Blob(path, bone, lw); else p.G.FillPath(bone, path);
        }
        Bone(Pt(c.X - s * 2.0f, c.Y - s * 0.75f), Pt(c.X + s * 2.0f, c.Y + s * 1.85f));
        Bone(Pt(c.X + s * 2.0f, c.Y - s * 0.75f), Pt(c.X - s * 2.0f, c.Y + s * 1.85f));

        using (var skull = new GraphicsPath(FillMode.Winding))
        {
            skull.AddEllipse(c.X - s, c.Y - s * 0.95f, 2 * s, s * 1.85f);
            skull.AddRectangle(new RectangleF(c.X - s * 0.55f, c.Y + s * 0.5f, s * 1.1f, s * 0.55f));
            if (lw > 0) p.Blob(skull, p.Paper, lw); else p.G.FillPath(bone, skull);
        }
        var holes = p.Solid;
        p.G.FillEllipse(holes, c.X - s * 0.68f, c.Y - s * 0.12f, s * 0.58f, s * 0.62f);
        p.G.FillEllipse(holes, c.X + s * 0.10f, c.Y - s * 0.12f, s * 0.58f, s * 0.62f);
        p.G.FillPolygon(holes, new[] { Pt(c.X - s * 0.13f, c.Y + s * 0.62f), Pt(c.X + s * 0.13f, c.Y + s * 0.62f), Pt(c.X, c.Y + s * 0.4f) });
        if (s > 8)
        {
            var teeth = onFlag ? p.Pen(p.P.Solid, s * 0.08f) : p.Line(Math.Max(1, s * 0.08f));
            p.G.DrawLine(teeth, c.X - s * 0.45f, c.Y + s * 0.82f, c.X + s * 0.45f, c.Y + s * 0.82f);
            for (int i = -1; i <= 1; i++) p.G.DrawLine(teeth, c.X + i * s * 0.22f, c.Y + s * 0.68f, c.X + i * s * 0.22f, c.Y + s * 1.02f);
        }

        // Chapéu de palha: copa, fita e aba.
        using (var crown = new GraphicsPath())
        {
            crown.AddArc(c.X - s * 0.86f, c.Y - s * 1.55f, s * 1.72f, s * 1.5f, 180, 180);
            crown.CloseFigure();
            if (lw > 0) { p.Shape(crown, p.Paper, lw); p.Tone(crown, 1); p.G.DrawPath(p.Ink(lw), crown); }
            else p.G.FillPath(bone, crown);
        }
        using (var ribbon = new GraphicsPath())
        {
            ribbon.AddRectangle(new RectangleF(c.X - s * 0.86f, c.Y - s * 1.02f, s * 1.72f, s * 0.24f));
            p.G.FillPath(p.Solid, ribbon);
        }
        using (var brim = Ellipse(c.X, c.Y - s * 0.78f, s * 1.45f, s * 0.24f))
        {
            if (lw > 0) { p.Shape(brim, p.Paper, lw); p.Tone(brim, 1); p.G.DrawPath(p.Ink(lw), brim); }
            else p.G.FillPath(bone, brim);
        }
    }

    private static void Helm(Painter p, PointF c, float r)
    {
        var spoke = p.Ink(2.4f);
        for (int i = 0; i < 8; i++)
        {
            var a = Polar(c, r * 0.3f, i * 45);
            var b = Polar(c, r * 1.45f, i * 45);
            p.G.DrawLine(spoke, a, b);
        }
        using var rim = Ellipse(c.X, c.Y, r, r);
        p.G.DrawPath(p.Ink(5), rim);
        p.G.DrawPath(p.Pen(p.P.Paper, 2), rim);
        using var hub = Ellipse(c.X, c.Y, r * 0.32f, r * 0.32f);
        p.Shape(hub, p.Paper, 2);
    }

    private static GraphicsPath HullPath()
    {
        var hull = new GraphicsPath();
        var A = Pt(364, D - 104);
        var B = Pt(296, D - 58);
        var C = Pt(249, D + 13.5f);
        var E = Pt(195, D + 13.5f);
        var F = Pt(164, D - 32);
        var G = Pt(82, D - 34);
        var H = Pt(122, K - 4);
        var I = Pt(330, K - 20);
        var J = Pt(374, D - 48);
        hull.AddBezier(A, Pt(344, D - 84), Pt(318, D - 62), B);
        hull.AddBezier(B, Pt(290, D - 30), Pt(262, D + 0), C);
        hull.AddArc(222 - 35, D + 36 - 35, 70, 70, -40, -100);
        hull.AddBezier(E, Pt(182, D + 0), Pt(170, D - 24), F);
        hull.AddLine(F, G);
        hull.AddBezier(G, Pt(70, D + 4), Pt(76, D + 56), H);
        hull.AddBezier(H, Pt(180, K + 6), Pt(284, K + 2), I);
        hull.AddBezier(I, Pt(358, K - 38), Pt(378, D + 0), J);
        hull.AddBezier(J, Pt(372, D - 70), Pt(368, D - 90), A);
        hull.CloseFigure();
        return hull;
    }

    private static GraphicsPath TopEdge()
    {
        var top = new GraphicsPath();
        top.AddBezier(Pt(364, D - 104), Pt(344, D - 84), Pt(318, D - 62), Pt(296, D - 58));
        top.AddBezier(Pt(296, D - 58), Pt(290, D - 30), Pt(262, D + 0), Pt(249, D + 13.5f));
        top.AddArc(222 - 35, D + 36 - 35, 70, 70, -40, -100);
        top.AddBezier(Pt(195, D + 13.5f), Pt(182, D + 0), Pt(170, D - 24), Pt(164, D - 32));
        top.AddLine(Pt(164, D - 32), Pt(82, D - 34));
        top.AddBezier(Pt(82, D - 34), Pt(74, D - 16), Pt(72, D + 4), Pt(74, D + 26));
        return top;
    }

    private static void Hull(Painter p)
    {
        using var hull = HullPath();
        var outline = Flat(hull);
        p.G.FillPath(p.Paper, hull);

        // Faixa escura (verdugo) que separa a madeira dos painéis.
        using var band = new GraphicsPath();
        band.AddBezier(Pt(70, D + 26), Pt(150, D + 34), Pt(300, D + 34), Pt(378, D + 6));
        var bandPts = Flat(band);
        using var below = new GraphicsPath();
        below.AddLines(bandPts);
        below.AddLine(378, D + 6, 400, K + 20);
        below.AddLine(400, K + 20, 50, K + 20);
        below.CloseFigure();

        using var wood = new Region(hull);
        wood.Intersect(below);
        using var panels = new Region(hull);
        panels.Exclude(below);

        // Painéis (vermelhos no anime) em retícula média, mais escura logo abaixo do friso.
        p.Tone(panels, 2);

        // Tábuas que acompanham a curva do casco, com emendas desencontradas.
        var state = p.G.Save();
        p.G.SetClip(wood, CombineMode.Intersect);
        var plank = p.Line(1.25f);
        const int rows = 8;
        var rowsPts = new List<PointF[]>();
        for (int k = 1; k <= rows; k++)
        {
            var pts = new List<PointF>();
            for (float x = 60; x <= 386; x += 3)
            {
                float yb = YAt(bandPts, x, true), yk = YAt(outline, x, true);
                if (float.IsNaN(yb)) yb = x < 200 ? D + 26 : D + 6;
                if (float.IsNaN(yk) || yk < yb) continue;
                pts.Add(Pt(x, yb + (yk - yb) * k / (rows + 1f)));
            }
            if (pts.Count > 1) { p.G.DrawLines(plank, pts.ToArray()); rowsPts.Add(pts.ToArray()); }
        }
        for (int k = 0; k + 1 < rowsPts.Count; k++)
        {
            for (int j = 0; j < 4; j++)
            {
                float x = 96 + j * 72 + (float)Hash(k * 13 + j) * 44;
                float y0 = YAt(rowsPts[k], x, true), y1 = YAt(rowsPts[k + 1], x, true);
                if (!float.IsNaN(y0) && !float.IsNaN(y1)) p.G.DrawLine(plank, x, y0, x, y1);
            }
        }
        // Volume: retícula clara e hachura na barriga do casco.
        using (var belly = Crescent(hull, 0, -34)) p.Tone(belly, 1);
        using (var keel = Crescent(hull, 6, -14)) p.Hatch(keel, hull.GetBounds(), 62, 4.2f, 1.3f);
        p.G.Restore(state);

        p.Band(band, 6, 1.8f, p.P.Solid);

        // Escotilhas: três descendo na proa, três no painel de ré.
        foreach (var c in new[] { Pt(338, D - 44), Pt(322, D - 18), Pt(305, D + 8), Pt(160, D + 2), Pt(131, D - 6), Pt(102, D - 9) })
            Porthole(p, c, 8);

        // Friso branco do costado, descendo em U até a escotilha do Soldier Dock.
        using (var top = TopEdge()) p.Band(top, 6.5f, 2.2f);
        p.G.DrawPath(p.Ink(3.4f), hull);

        SoldierDock(p, Pt(222, D + 36), 31);
        PawAnchor(p, Pt(350, D + 4));
    }

    private static void Porthole(Painter p, PointF c, float r)
    {
        using var ring = Ellipse(c.X, c.Y, r, r);
        p.Shape(ring, p.Paper, 2.2f);
        using var glass = Ellipse(c.X, c.Y, r * 0.6f, r * 0.6f);
        p.Shape(glass, p.Window, 1.4f);
        if (p.P.Night) Glow(p, c, r * 2.2f);
        else p.G.DrawArc(p.Pen(Color.White, 1.2f), c.X - r * 0.38f, c.Y - r * 0.38f, r * 0.76f, r * 0.76f, 200, 60);
    }

    private static void SoldierDock(Painter p, PointF c, float r)
    {
        using var outer = Ellipse(c.X, c.Y, r, r);
        p.Shape(outer, p.Paper, 3.2f);
        using var inner = Ellipse(c.X, c.Y, r * 0.72f, r * 0.72f);
        p.Shape(inner, p.Paper, 2.4f);
        using (var shade = Crescent(inner, 4, -5)) p.Tone(shade, 2);
        var state = p.G.Save();
        p.G.SetClip(inner, CombineMode.Intersect);
        for (int i = -3; i <= 3; i++) p.G.DrawLine(p.Line(1.1f), c.X - r, c.Y + i * 6, c.X + r, c.Y + i * 6);
        p.G.Restore(state);
        using var one = new GraphicsPath();
        FontFamily fam;
        try { fam = new FontFamily("Georgia"); } catch { fam = new FontFamily(System.Drawing.Text.GenericFontFamilies.Serif); }
        using (fam)
        {
            using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            one.AddString("1", fam, (int)FontStyle.Bold, r * 1.05f, new RectangleF(c.X - r, c.Y - r + r * 0.06f, 2 * r, 2 * r), sf);
        }
        p.G.DrawPath(p.Pen(p.P.Paper, 3), one);
        p.G.FillPath(p.P.Night ? p.Fill(p.P.Line) : p.Solid, one);
    }

    private static void PawAnchor(Painter p, PointF c)
    {
        p.G.DrawLine(p.Ink(2), c.X - 2, c.Y - 30, c.X - 1, c.Y - 10);
        using var ring = Ellipse(c.X - 2, c.Y - 32, 4, 4);
        p.Shape(ring, p.Paper, 1.8f);
        using var paw = new GraphicsPath();
        paw.AddEllipse(c.X - 11, c.Y - 11, 22, 21);
        p.Shape(paw, p.Paper, 2.6f);
        using (var shade = Crescent(paw, 4, -5)) p.Tone(shade, 2);
        for (int i = -1; i <= 1; i++)
        {
            using var claw = new GraphicsPath();
            float x = c.X + i * 6;
            claw.AddBezier(Pt(x - 2.4f, c.Y + 4), Pt(x - 2, c.Y + 12), Pt(x + 1, c.Y + 16), Pt(x + 2, c.Y + 18));
            claw.AddBezier(Pt(x + 2, c.Y + 18), Pt(x + 2.6f, c.Y + 12), Pt(x + 2.4f, c.Y + 8), Pt(x + 2.4f, c.Y + 4));
            claw.CloseFigure();
            p.Shape(claw, p.Solid, 1.2f);
        }
    }

    private static void Paddle(Painter p)
    {
        // Roda de madeira na popa (vista quase de quina): face elíptica com anéis e o cubo.
        var c = Pt(54, D + 6);
        using var side = new GraphicsPath();
        side.AddArc(c.X - 11, c.Y - 32, 22, 64, 90, 180);
        side.AddLine(c.X, c.Y - 32, c.X + 9, c.Y - 32);
        side.AddArc(c.X - 2, c.Y - 32, 22, 64, 270, 180);
        side.CloseFigure();
        p.Shape(side, p.Paper, 3);
        p.Hatch(side, 90, 3.5f, 1.1f, p.P.Line);
        using var face = Ellipse(c.X, c.Y, 11, 32);
        p.Shape(face, p.Paper, 3);
        p.G.DrawEllipse(p.Line(1.3f), c.X - 7, c.Y - 22, 14, 44);
        p.G.DrawEllipse(p.Line(1.2f), c.X - 3.5f, c.Y - 11, 7, 22);
        using (var shade = Crescent(face, 3, -8)) p.Tone(shade, 2);
    }

    private static void CrowsNest(Painter p)
    {
        const float cx = 270;
        // Base em cogumelo.
        using (var ped = new GraphicsPath())
        {
            ped.AddBezier(Pt(264, D - 140), Pt(262, D - 150), Pt(248, D - 156), Pt(236, D - 162));
            ped.AddLine(Pt(236, D - 162), Pt(304, D - 162));
            ped.AddBezier(Pt(304, D - 162), Pt(292, D - 156), Pt(278, D - 150), Pt(276, D - 140));
            ped.CloseFigure();
            p.Shape(ped, p.Paper, 2.8f);
            using var shade = Crescent(ped, 10, 0);
            p.Tone(shade, 2);
        }
        using (var cabin = new GraphicsPath())
        {
            cabin.AddRectangle(new RectangleF(238, D - 180, 64, 18));
            p.Shape(cabin, p.Paper, 2.8f);
            for (int i = 0; i < 5; i++)
            {
                using var w = new GraphicsPath();
                w.AddRectangle(new RectangleF(243 + i * 12, D - 177, 7, 10));
                p.Shape(w, p.Window, 1.4f);
            }
        }
        p.Band(Pt(234, D - 162), Pt(306, D - 162), 3.5f, 1.4f);
        Dome(p, cx, D - 180, 36, 30, 6, 2);
        // Mastaréu e o Jolly Roger.
        p.G.DrawLine(p.Ink(2.6f), cx, D - 212, cx, D - 266);
        Flag(p, Pt(cx, D - 264), 44, 28);
    }

    private static void Flag(Painter p, PointF pole, float w, float h)
    {
        using var flag = new GraphicsPath();
        float x = pole.X, y = pole.Y;
        flag.AddBezier(Pt(x, y), Pt(x - w * 0.3f, y - 7), Pt(x - w * 0.6f, y + 7), Pt(x - w, y + 1));
        flag.AddBezier(Pt(x - w, y + 1), Pt(x - w - 3, y + h * 0.4f), Pt(x - w + 2, y + h * 0.7f), Pt(x - w + 1, y + h + 2));
        flag.AddBezier(Pt(x - w + 1, y + h + 2), Pt(x - w * 0.6f, y + h + 8), Pt(x - w * 0.3f, y + h - 6), Pt(x, y + h));
        flag.CloseFigure();
        p.Shape(flag, p.Solid, 2.2f);
        JollyRoger(p, Pt(x - w * 0.5f, y + h * 0.52f), h * 0.2f, true);
    }

    private static void Figurehead(Painter p, PointF o)
    {
        // Disco dourado da proa (o "colar" atrás da cabeça), inclinado para trás, com rebites.
        var state = p.G.Save();
        p.G.TranslateTransform(368, D - 54);
        p.G.RotateTransform(-10);
        using (var ring = Ellipse(0, 0, 22, 62))
        {
            p.Shape(ring, p.Paper, 3.2f);
            p.Tone(ring, 1);
            p.G.DrawPath(p.Ink(3.2f), ring);
            using var hole = Ellipse(3, 0, 11, 40);
            p.Shape(hole, p.Solid, 2);
            for (int i = 0; i < 14; i++)
            {
                var q = Polar(Pt(0, 0), 52, i * 360 / 14.0 + 8, 17 / 52f);
                using var rivet = Ellipse(q.X, q.Y, 2.4f, 2.4f);
                p.Shape(rivet, p.Paper, 1.3f);
            }
        }
        p.G.Restore(state);

        const float kx = 0.86f;
        // Ossos cruzados atrás da juba, com as pontas aparecendo além das pétalas.
        void Bone(PointF a, PointF b)
        {
            using var path = new GraphicsPath(FillMode.Winding);
            float dx = b.X - a.X, dy = b.Y - a.Y, len = (float)Math.Sqrt(dx * dx + dy * dy);
            float ux = dx / len, uy = dy / len, nx = -uy, ny = ux, t = 4.4f;
            path.AddPolygon(new[] { Pt(a.X + nx * t, a.Y + ny * t), Pt(b.X + nx * t, b.Y + ny * t), Pt(b.X - nx * t, b.Y - ny * t), Pt(a.X - nx * t, a.Y - ny * t) });
            foreach (var (e, sgn) in new[] { (a, 1f), (b, -1f) })
                for (int s = -1; s <= 1; s += 2)
                {
                    var k = Pt(e.X + nx * 5.2f * s + ux * 2.5f * sgn, e.Y + ny * 5.2f * s + uy * 2.5f * sgn);
                    path.AddEllipse(k.X - 6, k.Y - 6, 12, 12);
                }
            p.Blob(path, p.Paper, 2.6f);
            using var shade = Crescent(path, 2.5f, -3.5f);
            p.Tone(shade, 2);
        }
        Bone(Polar(o, 90, -132, kx), Polar(o, 90, 48, kx));
        Bone(Polar(o, 90, -48, kx), Polar(o, 90, 132, kx));

        // Juba: duas camadas de pétalas pontudas (5 na frente, 5 atrás), cada uma com uma dobra no meio e meia face
        // em retícula, e um leve giro nas pontas — é o "girassol" que confundiram com o sol.
        void Petal(double deg, float rOut, float halfW, bool back)
        {
            const float rb = 26, swirl = 10;
            var b1 = Polar(o, rb, deg - halfW, kx);
            var b2 = Polar(o, rb, deg + halfW, kx);
            var mb = Polar(o, rb, deg, kx);
            var tip = Polar(o, rOut, deg + swirl, kx);
            var c1 = Polar(o, rb + (rOut - rb) * 0.42f, deg - halfW * 0.86f, kx);
            var c2 = Polar(o, rOut * 0.84f, deg + swirl - 7, kx);
            var c3 = Polar(o, rOut * 0.8f, deg + swirl + 5, kx);
            var c4 = Polar(o, rb + (rOut - rb) * 0.36f, deg + halfW * 0.92f, kx);
            using var petal = new GraphicsPath();
            petal.AddBezier(b1, c1, c2, tip);
            petal.AddBezier(tip, c3, c4, b2);
            petal.CloseFigure();
            p.G.FillPath(p.Paper, petal);
            if (back) p.Tone(petal, 2);
            using (var half = new GraphicsPath())
            {
                half.AddBezier(tip, c3, c4, b2);
                half.AddLine(b2, mb);
                half.CloseFigure();
                p.Tone(half, back ? 3 : 1);
            }
            p.G.DrawLine(p.Line(1.4f), Lerp(mb, tip, 0.12f), Lerp(mb, tip, 0.9f));
            p.G.DrawPath(p.Ink(3), petal);
        }
        for (int i = 0; i < 5; i++) Petal(-90 + 36 + i * 72, 66, 27, true);
        for (int i = 0; i < 5; i++) Petal(-90 + i * 72, 78, 30, false);

        // Rosto: redondo, focinho largo e branco, narizinho escuro, boca de gato e olhos de bolinha.
        var f = Pt(o.X + 5, o.Y + 3);
        using (var face = Ellipse(f.X, f.Y, 33, 33))
        {
            p.Shape(face, p.Paper, 3.6f);
            using var shade = Crescent(face, 4, -6);
            p.Tone(shade, 1);
        }
        using (var muzzle = new GraphicsPath())
        {
            muzzle.AddClosedCurve(new[] { Pt(f.X - 4, f.Y + 8), Pt(f.X + 8, f.Y + 0), Pt(f.X + 24, f.Y + 2), Pt(f.X + 30, f.Y + 12), Pt(f.X + 20, f.Y + 22), Pt(f.X + 4, f.Y + 21) }, 0.55f);
            p.Shape(muzzle, p.Paper, 2.4f);
        }
        using (var nose = new GraphicsPath())
        {
            nose.AddClosedCurve(new[] { Pt(f.X + 7, f.Y + 3), Pt(f.X + 13, f.Y + 1.8f), Pt(f.X + 20, f.Y + 3.2f), Pt(f.X + 14, f.Y + 8.5f) }, 0.6f);
            p.G.FillPath(p.Solid, nose);
            p.G.FillEllipse(p.Paper, f.X + 10.5f, f.Y + 3, 3, 1.8f);
        }
        var mouth = p.Ink(2.3f);
        p.G.DrawLine(mouth, f.X + 14, f.Y + 8, f.X + 14, f.Y + 13);
        p.G.DrawBezier(mouth, Pt(f.X + 14, f.Y + 13), Pt(f.X + 12, f.Y + 18), Pt(f.X + 4, f.Y + 18), Pt(f.X + 2, f.Y + 13));
        p.G.DrawBezier(mouth, Pt(f.X + 14, f.Y + 13), Pt(f.X + 16, f.Y + 18), Pt(f.X + 24, f.Y + 18), Pt(f.X + 26, f.Y + 12));
        void Eye(float x, float y, float rx, float ry)
        {
            using var e = Ellipse(x, y, rx, ry);
            p.Shape(e, p.Paper, 2.4f);
            p.G.FillEllipse(p.Solid, x - rx * 0.3f, y - ry * 0.55f, rx * 1.05f, ry * 1.0f);
            p.G.FillEllipse(p.Paper, x + rx * 0.15f, y - ry * 0.45f, 2.4f, 2.4f);
        }
        Eye(f.X - 9, f.Y - 10, 7, 8.5f);
        Eye(f.X + 17, f.Y - 11, 5.4f, 8);
    }

    #endregion

    #region Nuvens

    /// <summary>
    /// Nuvem de mangá: cúmulo de bolas com contorno só por fora, curvinhas internas onde uma bola encosta na outra,
    /// sombra em retícula em meia-lua do lado oposto à luz (que vem de cima, pela direita) e base mais achatada.
    /// <paramref name="dark"/> = nuvem de tempestade / de noite: silhueta escura com contorno claro.
    /// </summary>
    public static Bitmap Cloud(int seed, bool dark)
    {
        var rnd = new Random(seed * 7919 + 17);
        float Rf() => (float)rnd.NextDouble();
        // Três famílias pelo seed: 0 = cúmulo com torre, 1 = nuvem comprida e baixa, 2 = nuvenzinha redonda.
        int kind = ((seed % 3) + 3) % 3;
        float w = kind == 1 ? 300 + Rf() * 140 : kind == 2 ? 130 + Rf() * 60 : 220 + Rf() * 150;
        int n = kind == 1 ? 7 + rnd.Next(3) : kind == 2 ? 3 + rnd.Next(2) : 5 + rnd.Next(3);
        float rMin = kind == 1 ? 17 : 16, rAdd = kind == 1 ? 16 : kind == 2 ? 20 : 26, lift = kind == 1 ? 6 : 18;
        var balls = new List<(float x, float y, float r)>();
        for (int i = 0; i < n; i++)
        {
            float t = (i + 0.5f) / n;
            float env = (float)Math.Sin(Math.PI * t);
            float r = (rMin + rAdd * env) * (0.85f + 0.3f * Rf());
            float x = w * t + (Rf() - 0.5f) * w / n * 0.35f;
            float y = -r * 0.9f - env * lift * (0.6f + 0.8f * Rf());
            balls.Add((x, y, r));
        }
        int towers = kind == 1 ? 2 + rnd.Next(2) : kind == 2 ? 0 : 1 + rnd.Next(2);
        for (int j = 0; j < towers; j++)
        {
            float x = kind == 1 ? w * (0.15f + 0.7f * (j + Rf()) / towers) : w * (0.32f + 0.36f * Rf());
            float r = kind == 1 ? 13 + 9 * Rf() : 22 + 12 * Rf();
            float under = 0;
            foreach (var b in balls) if (Math.Abs(b.x - x) < b.r) under = Math.Min(under, b.y - b.r * 0.4f);
            balls.Add((x, under - r * 0.55f, r));
        }
        var bases = new List<RectangleF>();
        for (int j = 0; j < n - 1; j++)
        {
            float cx = w * (j + 1f) / n;
            float rx = w / n * 0.85f, ry = 9 + 4 * Rf();
            bases.Add(new RectangleF(cx - rx, -6 - ry, 2 * rx, 2 * ry));
        }

        var bounds = RectangleF.Empty;
        foreach (var b in balls) bounds = bounds.IsEmpty ? new RectangleF(b.x - b.r, b.y - b.r, 2 * b.r, 2 * b.r) : RectangleF.Union(bounds, new RectangleF(b.x - b.r, b.y - b.r, 2 * b.r, 2 * b.r));
        foreach (var r in bases) bounds = RectangleF.Union(bounds, r);
        bounds.Inflate(8, 8);

        var pal = dark ? StormCloud : Day;
        var bmp = NewCanvas((int)Math.Ceiling(bounds.Width), (int)Math.Ceiling(bounds.Height), out var g);
        using (g)
        {
            g.TranslateTransform(-bounds.X, -bounds.Y);
            using var p = new Painter(g, pal, 1);
            using var cloud = new GraphicsPath(FillMode.Winding);
            foreach (var b in balls) cloud.AddEllipse(b.x - b.r, b.y - b.r, 2 * b.r, 2 * b.r);
            foreach (var r in bases) cloud.AddEllipse(r);
            p.Blob(cloud, p.Paper, 2.4f);

            using (var soft = Crescent(cloud, 14, -20)) p.Tone(soft, 1);
            using (var core = Crescent(cloud, 5, -8)) p.Tone(core, 2);

            // Curvinhas internas nos "vales" entre bolas vizinhas.
            var sorted = new List<(float x, float y, float r)>(balls);
            sorted.Sort((a, b) => a.x.CompareTo(b.x));
            var curl = p.Line(dark ? 1.8f : 2.1f);
            for (int i = 0; i + 1 < sorted.Count; i++)
            {
                var a = sorted[i];
                var b = sorted[i + 1];
                if (!Intersect(a, b, out var hit)) continue;
                var front = a.r >= b.r ? a : b;
                var other = a.r >= b.r ? b : a;
                double ang = Math.Atan2(hit.Y - front.y, hit.X - front.x) * 180 / Math.PI;
                var probe = Polar(Pt(front.x, front.y), front.r, ang + 8);
                float sweep = Inside(other, probe) ? 48 : -48;
                g.DrawArc(curl, front.x - front.r, front.y - front.r, 2 * front.r, 2 * front.r, (float)ang, sweep);
            }
            // Um ou dois caracóis no meio, típicos do mangá.
            var big = balls[0];
            foreach (var b in balls) if (b.r > big.r) big = b;
            g.DrawArc(curl, big.x - big.r * 0.45f, big.y - big.r * 0.1f, big.r * 0.7f, big.r * 0.6f, 160, 120);
        }
        return Finish(bmp, pal.Ink, 1.6f);
    }

    private static bool Intersect((float x, float y, float r) a, (float x, float y, float r) b, out PointF upper)
    {
        upper = PointF.Empty;
        float dx = b.x - a.x, dy = b.y - a.y, d = (float)Math.Sqrt(dx * dx + dy * dy);
        if (d < 1e-3f || d >= a.r + b.r || d <= Math.Abs(a.r - b.r)) return false;
        float l = (a.r * a.r - b.r * b.r + d * d) / (2 * d);
        float h = (float)Math.Sqrt(Math.Max(0, a.r * a.r - l * l));
        float mx = a.x + l * dx / d, my = a.y + l * dy / d;
        var p1 = Pt(mx - h * dy / d, my + h * dx / d);
        var p2 = Pt(mx + h * dy / d, my - h * dx / d);
        upper = p1.Y < p2.Y ? p1 : p2;
        return true;
    }

    private static bool Inside((float x, float y, float r) c, PointF q) => (q.X - c.x) * (q.X - c.x) + (q.Y - c.y) * (q.Y - c.y) < c.r * c.r;

    #endregion

    #region Mar

    /// <summary>
    /// Faixa de mar periódica na horizontal (repete a cada <paramref name="period"/> px; desenhe com deslocamento
    /// x mod period para rolar sem emenda). Camada 0 = horizonte (64 px de altura, opaca), 1 = ondulação do meio
    /// (110 px, transparente acima da crista), 2 = ondas da frente com crista de espuma (190 px, idem).
    /// <paramref name="width"/> normalmente é a largura da tela + period.
    /// </summary>
    public static Bitmap SeaStrip(int layer, int period, int width, bool night)
    {
        layer = Math.Clamp(layer, 0, 2);
        period = Math.Max(60, period);
        int h = layer == 0 ? 64 : layer == 1 ? 110 : 190;
        var basePal = night ? Night : Day;
        var pal = new Palette
        {
            Ink = basePal.Ink, Line = basePal.Line, Tone = basePal.Tone, Solid = basePal.Solid, Window = basePal.Window, Night = night,
            Paper = night ? (layer == 0 ? Color.FromArgb(40, 45, 60) : layer == 1 ? Color.FromArgb(32, 36, 50) : Color.FromArgb(25, 28, 40)) : Color.White,
        };
        if (night) pal.Tone = Color.FromArgb(70, 78, 100);
        // Passo da retícula ajustado para caber um número inteiro de vezes no período: a faixa emenda sem costura.
        float pitch = period / (float)Math.Max(1, Math.Round(period / 6.0));
        var bmp = NewCanvas(width, h, out var g);
        using (g)
        using (var p = new Painter(g, pal, 6f / pitch))
        {
            if (layer == 0) Horizon(p, period, width, h);
            else if (layer == 1) MiddleSea(p, period, width, h);
            else FrontSea(p, period, width, h);
        }
        return bmp;
    }

    private static void Horizon(Painter p, int period, int width, int h)
    {
        var g = p.G;
        g.FillRectangle(p.Paper, 0, 0, width, h);
        // Neblina de retícula colada no horizonte.
        GradientDots(g, p.P.Tone, new RectangleF(0, 3, width, 26), y => 1.25f * (1 - (y - 3) / 26f), 6 / p.S);
        g.DrawLine(p.Ink(2.2f), 0, 1.2f, width, 1.2f);
        // Tracinhos de perspectiva, cada vez maiores e mais espaçados.
        for (int r = 0; r < 7; r++)
        {
            float y = 7 + r * r * 1.05f + r * 3.2f;
            if (y > h - 3) break;
            float len = 9 + r * 6.5f;
            int count = Math.Max(1, (int)Math.Round(period / (len + 34 + r * 22f)));
            float step = period / (float)count;
            var pen = p.Line(1.1f + r * 0.2f);
            for (int j = 0; j < count; j++)
            {
                float x0 = (float)(Hash(r * 97 + j) * step * 0.8 + j * step);
                for (float x = x0 - period; x < width + period; x += period)
                {
                    g.DrawLine(pen, x, y, x + len, y);
                    if (r >= 3) g.DrawLine(pen, x + len * 0.25f, y + 2.5f + r * 0.4f, x + len * 0.7f, y + 2.5f + r * 0.4f);
                }
            }
        }
    }

    private static float Peak(double u, double sharp)
    {
        double c = (1 + Math.Cos(2 * Math.PI * u)) / 2;
        return (float)Math.Pow(c, sharp);
    }

    private static void MiddleSea(Painter p, int period, int width, int h)
    {
        var g = p.G;
        int waves = Math.Max(1, (int)Math.Round(period / 120.0));
        float L = period / (float)waves;
        float Crest(float x)
        {
            double u = x / L;
            double amp = 8 + 1.5 * Math.Sin(2 * Math.PI * x / period);
            return (float)(20 - amp * Peak(u, 1.6) - 1.4 * Math.Sin(4 * Math.PI * u + 1));
        }
        var top = new List<PointF>();
        float step = period / (float)Math.Max(1, Math.Round(period / 2.0));
        for (int i = 0; i * step <= width + step; i++) top.Add(Pt(i * step, Crest(i * step)));
        using (var body = new GraphicsPath())
        {
            body.AddLines(top.ToArray());
            body.AddLine(width, top[^1].Y, width, h);
            body.AddLine(width, h, 0, h);
            body.CloseFigure();
            g.FillPath(p.Paper, body);
            GradientDots(g, p.P.Tone, new RectangleF(0, 34, width, h - 34), y => Math.Min(1.3f, (y - 34) / 70f * 1.3f), 6 / p.S, body);
        }
        g.DrawLines(p.Ink(2.6f), top.ToArray());
        // Hachura nas costas de cada ondulação e risquinhos de reflexo.
        var hatch = p.Line(1.5f);
        for (int i = -1; i <= width / L + 1; i++)
        {
            float xc = i * L;
            for (int k = 1; k <= 3; k++)
            {
                var pts = new List<PointF>();
                float a = 0.04f + k * 0.02f, b = 0.30f - k * 0.05f;
                for (float u = a; u <= b; u += 0.02f) pts.Add(Pt(xc + u * L, Crest(xc + u * L) + 4 + k * 4.5f));
                if (pts.Count > 1) g.DrawLines(hatch, pts.ToArray());
            }
            int hv = ((i % waves) + waves) % waves;
            float rx = xc + L * (0.55f + 0.2f * (float)Hash(hv + 40));
            float ry = 44 + 30 * (float)Hash(hv + 41);
            g.DrawLine(hatch, rx, ry, rx + 26, ry);
            g.DrawLine(hatch, rx + 8, ry + 5, rx + 22, ry + 5);
        }
    }

    private static void FrontSea(Painter p, int period, int width, int h)
    {
        var g = p.G;
        int waves = Math.Max(1, (int)Math.Round(period / 240.0));
        float L = period / (float)waves;
        const float c0 = 34;
        float Amp(float x) => (float)(28 + 7 * Math.Sin(2 * Math.PI * x / period + 0.7));
        float Crest(float x)
        {
            double u = x / L - 0.3;
            u += 0.08 * Math.Sin(2 * Math.PI * u);
            return c0 + Amp(x) * (1 - Peak(u, 1.3));
        }
        var top = new List<PointF>();
        float step = period / (float)Math.Max(1, Math.Round(period / 2.0));
        for (int i = 0; i * step <= width + step; i++) top.Add(Pt(i * step, Crest(i * step)));
        using var body = new GraphicsPath();
        body.AddLines(top.ToArray());
        body.AddLine(width, top[^1].Y, width, h);
        body.AddLine(width, h, 0, h);
        body.CloseFigure();
        g.FillPath(p.Paper, body);
        GradientDots(g, p.P.Tone, new RectangleF(0, c0 + 40, width, h - c0 - 40), y => Math.Min(1.8f, (y - c0 - 40) / 80f * 1.8f), 6 / p.S, body);

        var hatch = p.Line(1.6f);
        for (int i = -1; i <= width / L + 1; i++)
        {
            float xc = i * L + L * 0.3f;
            int hv = ((i % waves) + waves) % waves;
            // Face da onda à frente da crista, na sombra do lábio: retícula escura em lente.
            var upper = new List<PointF>();
            var lower = new List<PointF>();
            for (float u = -0.30f; u <= -0.015f; u += 0.01f)
            {
                float x = xc + u * L;
                float t = (u + 0.30f) / 0.285f;
                float depth = 26 * (float)Math.Sin(Math.PI * Math.Pow(t, 0.8));
                upper.Add(Pt(x, Crest(x) + 7));
                lower.Add(Pt(x, Crest(x) + 7 + depth));
            }
            lower.Reverse();
            using (var face = new GraphicsPath())
            {
                face.AddCurve(upper.ToArray(), 0.3f);
                face.AddCurve(lower.ToArray(), 0.3f);
                face.CloseFigure();
                GradientDots(g, p.P.Tone, face.GetBounds(), _ => 1.95f, 6 / p.S, face);
            }
            // Hachura nas costas da onda (atrás da crista), acompanhando o perfil.
            for (int k = 1; k <= 4; k++)
            {
                var pts = new List<PointF>();
                for (float u = 0.12f + k * 0.015f; u <= 0.42f - k * 0.03f; u += 0.015f) pts.Add(Pt(xc + u * L, Crest(xc + u * L) + 6 + k * 6));
                if (pts.Count > 1) g.DrawLines(hatch, pts.ToArray());
            }
            // Riscos de correnteza mais embaixo.
            float sx = xc + L * (0.25f + 0.3f * (float)Hash(hv * 3 + 7));
            float sy = c0 + 74 + 40 * (float)Hash(hv * 3 + 8);
            g.DrawLine(hatch, sx, sy, sx + 44, sy);
            g.DrawLine(hatch, sx + 12, sy + 6, sx + 36, sy + 6);
        }
        g.DrawLines(p.Ink(3.4f), top.ToArray());

        // Crista de espuma: capa branca de bolhas com borda recortada, caracol, garras e respingos.
        for (int i = -1; i <= width / L + 1; i++)
        {
            float xc = i * L + L * 0.3f;
            int hv = ((i % waves) + waves) % waves;
            Foam(p, xc, L, Crest, hv);
        }
    }

    private static void Foam(Painter p, float xc, float L, Func<float, float> crest, int variant)
    {
        var g = p.G;
        const float u0 = -0.22f, u1 = 0.17f;
        var upper = new List<PointF>();
        for (float u = u0; u <= u1 + 1e-4f; u += 0.01f) upper.Add(Pt(xc + u * L, crest(xc + u * L) - 1.5f));
        using var foam = new GraphicsPath();
        foam.AddLines(upper.ToArray());
        // Borda de baixo: festões (bolhas) voltando da direita para a esquerda.
        float Thick(float u) => 21 * (float)Math.Pow(Math.Sin(Math.PI * (u - u0) / (u1 - u0)), 0.75);
        const int scallops = 10;
        PointF prev = upper[^1];
        for (int s = scallops - 1; s >= 0; s--)
        {
            float ua = u0 + (u1 - u0) * s / scallops;
            var q = Pt(xc + ua * L, crest(xc + ua * L) + Thick(ua));
            float bulge = 5 + 3 * (float)Hash(variant * 17 + s);
            foam.AddBezier(prev, Pt(prev.X - 1, prev.Y + bulge), Pt(q.X + 1, q.Y + bulge), q);
            prev = q;
        }
        foam.CloseFigure();
        p.Shape(foam, p.Paper, 2.6f);
        // Bolinhas dentro da espuma.
        for (int k = 0; k < 6; k++)
        {
            float u = u0 + 0.05f + (u1 - u0 - 0.1f) * (float)Hash(variant * 23 + k);
            float x = xc + u * L, y = crest(x) + 5 + 6 * (float)Hash(variant * 29 + k);
            float r = 1.6f + 1.8f * (float)Hash(variant * 31 + k);
            g.DrawEllipse(p.Line(1.2f), x - r, y - r, 2 * r, 2 * r);
        }
        // Caracol na frente da crista.
        var spiral = new List<PointF>();
        var sc = Pt(xc - 0.07f * L, crest(xc - 0.07f * L) + 9);
        for (double t = 0; t < Math.PI * 3.2; t += 0.12)
        {
            double rr = 12 * (1 - t / (Math.PI * 3.8));
            spiral.Add(Pt(sc.X + (float)(Math.Cos(t + Math.PI * 1.1) * rr), sc.Y + (float)(Math.Sin(t + Math.PI * 1.1) * rr)));
        }
        g.DrawLines(p.Ink(2.6f), spiral.ToArray());
        // Garras de espuma caindo à frente (à esquerda) da crista.
        for (int k = 0; k < 4; k++)
        {
            float x = xc + L * (u0 + 0.005f + k * 0.022f), y = crest(x) + 1;
            float len = 10 + 4 * (float)Hash(variant * 37 + k);
            using var claw = new GraphicsPath();
            claw.AddBezier(Pt(x + 6, y - 2), Pt(x - 2, y - 6), Pt(x - len, y - 2), Pt(x - len - 2, y + 7));
            claw.AddBezier(Pt(x - len - 2, y + 7), Pt(x - len + 4, y + 2), Pt(x - 2, y + 1), Pt(x + 4, y + 5));
            claw.CloseFigure();
            p.Shape(claw, p.Paper, 2.2f);
        }
        // Respingos.
        for (int k = 0; k < 7; k++)
        {
            float x = xc - L * (0.02f + 0.24f * (float)Hash(variant * 41 + k)), y = crest(xc) - 8 - 20 * (float)Hash(variant * 43 + k);
            float r = 1.5f + 2.6f * (float)Hash(variant * 47 + k);
            using var drop = Ellipse(x, y, r, r);
            p.Shape(drop, p.Paper, 1.5f);
        }
    }

    /// <summary>Retícula em degradê: pontos numa grade a 45° com raio que varia com a altura.</summary>
    private static void GradientDots(Graphics g, Color c, RectangleF area, Func<float, float> radiusAtY, float pitch, GraphicsPath? clip = null)
    {
        var state = g.Save();
        if (clip != null) g.SetClip(clip, CombineMode.Intersect);
        using var b = new SolidBrush(c);
        int row = 0;
        // Grade ancorada na origem (não no canto da área): pontos de áreas diferentes caem na mesma retícula e a
        // faixa continua periódica quando o período é múltiplo do passo.
        int r0 = (int)Math.Floor(area.Top / (pitch / 2)), r1 = (int)Math.Ceiling(area.Bottom / (pitch / 2));
        int c0 = (int)Math.Floor(area.Left / pitch) - 1, c1 = (int)Math.Ceiling(area.Right / pitch) + 1;
        for (row = r0; row <= r1; row++)
        {
            float y = row * pitch / 2;
            float r = radiusAtY(y);
            if (r < 0.3f) continue;
            float shift = (row & 1) == 0 ? 0 : pitch / 2;
            for (int col = c0; col <= c1; col++)
            {
                float x = col * pitch + shift;
                g.FillEllipse(b, x - r, y - r, 2 * r, 2 * r);
            }
        }
        g.Restore(state);
    }

    #endregion

    #region Sol

    /// <summary>Sol de mangá: disco com contorno grosso, anel interno, 12 raios longos e 12 curtos com meia face em retícula.</summary>
    public static Bitmap Sun(float radius)
    {
        radius = Math.Max(8, radius);
        float ext = radius * 1.95f + 6;
        int size = (int)Math.Ceiling(ext * 2);
        var bmp = NewCanvas(size, size, out var g);
        using (g)
        using (var p = new Painter(g, Day, 1))
        {
            var c = Pt(size / 2f, size / 2f);
            float lw = Math.Max(1.6f, radius * 0.06f);
            void Ray(double deg, float len, float half)
            {
                var b1 = Polar(c, radius * 1.02f, deg - half);
                var b2 = Polar(c, radius * 1.02f, deg + half);
                var tip = Polar(c, len, deg);
                using var ray = new GraphicsPath();
                ray.AddPolygon(new[] { b1, tip, b2 });
                p.Shape(ray, p.Paper, lw);
                using var half2 = new GraphicsPath();
                half2.AddPolygon(new[] { Polar(c, radius * 1.02f, deg), tip, b2 });
                p.Tone(half2, 2);
                p.G.DrawPath(p.Ink(lw), ray);
            }
            for (int i = 0; i < 12; i++) Ray(i * 30 + 15, radius * 1.42f, 6);
            for (int i = 0; i < 12; i++) Ray(i * 30, radius * 1.9f, 8.5f);
            using var disc = Ellipse(c.X, c.Y, radius, radius);
            p.Shape(disc, p.Paper, Math.Max(2.4f, radius * 0.09f));
            using (var shade = Crescent(disc, radius * 0.16f, -radius * 0.2f)) p.Tone(shade, 1);
            g.DrawEllipse(p.Line(Math.Max(1.2f, radius * 0.035f)), c.X - radius * 0.8f, c.Y - radius * 0.8f, radius * 1.6f, radius * 1.6f);
            g.DrawArc(p.Line(Math.Max(1.4f, radius * 0.05f)), c.X - radius * 0.55f, c.Y - radius * 0.55f, radius * 1.1f, radius * 1.1f, 200, 70);
        }
        return Finish(bmp, Day.Ink, 1.2f);
    }

    #endregion

    #region Elenco

    /// <summary>News Coo: a gaivota-carteiro, de lado (voando para a direita), com quepe, bolsa de jornal e o jornal enrolado.</summary>
    public static Bitmap NewsCoo(bool wingsUp)
    {
        var bmp = NewCanvas(172, 158, out var g);
        using (g)
        using (var p = new Painter(g, Day, 1))
        {
            g.TranslateTransform(6, 20);
            // Asa de trás: mesma pose, deslocada para trás e em retícula clara.
            Wing(p, wingsUp, Pt(96, 52), true);
            // Cauda em leque com a ponta preta.
            using (var tail = new GraphicsPath())
            {
                tail.AddClosedCurve(new[] { Pt(50, 52), Pt(30, 47), Pt(12, 46), Pt(16, 52), Pt(10, 58), Pt(30, 63), Pt(50, 64) }, 0.25f);
                p.Shape(tail, p.Paper, 2.6f);
                var s = g.Save();
                g.SetClip(tail, CombineMode.Intersect);
                g.FillRectangle(p.Solid, 0, 38, 23, 32);
                g.Restore(s);
                g.DrawPath(p.Ink(2.6f), tail);
            }
            // Jornal enrolado saindo da bolsa (atrás do corpo).
            using (var roll = new GraphicsPath())
            {
                roll.AddRectangle(new RectangleF(42, 72, 34, 12));
                p.Shape(roll, p.Paper, 2.2f);
                for (int i = 1; i < 4; i++) g.DrawLine(p.Line(1.1f), 46 + i * 7, 74, 46 + i * 7, 82);
                using var end = Ellipse(42, 78, 4, 6);
                p.Shape(end, p.Paper, 2);
                g.DrawArc(p.Line(1.1f), 40, 75.5f, 4, 5, 0, 300);
            }
            // Corpo: gota alongada, peito redondo.
            using (var body = new GraphicsPath())
            {
                body.AddClosedCurve(new[] { Pt(44, 57), Pt(58, 47), Pt(82, 43), Pt(104, 44), Pt(118, 54), Pt(112, 66), Pt(90, 73), Pt(62, 71) }, 0.55f);
                p.Shape(body, p.Paper, 3);
                using var belly = Crescent(body, 0, -8);
                p.Tone(belly, 1);
            }
            // Bolsa (vermelha no anime) com alça cruzando o peito.
            g.DrawLine(p.Pen(p.P.Solid, 3), 76, 69, 110, 47);
            using (var bag = new GraphicsPath())
            {
                bag.AddClosedCurve(new[] { Pt(62, 68), Pt(88, 66), Pt(90, 83), Pt(75, 89), Pt(60, 83) }, 0.3f);
                p.Shape(bag, p.Paper, 2.4f);
                p.Tone(bag, 3);
                p.G.DrawPath(p.Ink(2.4f), bag);
                using var flap = new GraphicsPath();
                flap.AddClosedCurve(new[] { Pt(62, 68), Pt(88, 66), Pt(84, 76), Pt(66, 77) }, 0.3f);
                p.Shape(flap, p.Paper, 2);
                p.Tone(flap, 2);
                p.G.DrawPath(p.Ink(2), flap);
                using var buckle = Ellipse(75, 76, 2.4f, 2.4f);
                p.Shape(buckle, p.Paper, 1.4f);
            }
            // Cabeça, bico grande e curvo de ponta preta, olho decidido e quepe de marinheiro.
            using (var head = Ellipse(116, 40, 15, 14)) p.Shape(head, p.Paper, 3);
            using (var beak = new GraphicsPath())
            {
                beak.AddBezier(Pt(124, 34), Pt(136, 31), Pt(150, 34), Pt(156, 42));
                beak.AddBezier(Pt(156, 42), Pt(157, 47), Pt(153, 50), Pt(150, 47));
                beak.AddBezier(Pt(150, 47), Pt(142, 48), Pt(132, 49), Pt(123, 49));
                beak.CloseFigure();
                p.Shape(beak, p.Paper, 2.6f);
                using var tip = new GraphicsPath();
                tip.AddBezier(Pt(146, 34.5f), Pt(152, 36), Pt(155, 39), Pt(156, 42));
                tip.AddBezier(Pt(156, 42), Pt(157, 47), Pt(153, 50), Pt(150, 47));
                tip.AddBezier(Pt(150, 47), Pt(148, 48), Pt(146, 48.5f), Pt(144, 48.5f));
                tip.CloseFigure();
                p.G.FillPath(p.Solid, tip);
                p.G.DrawBezier(p.Line(1.4f), Pt(126, 43), Pt(132, 44.5f), Pt(140, 44.5f), Pt(146, 43));
                p.G.DrawPath(p.Ink(2.6f), beak);
            }
            g.FillEllipse(p.Solid, 117.5f, 34, 5, 6);
            g.FillEllipse(p.Paper, 120, 35, 1.7f, 1.7f);
            g.DrawLine(p.Ink(2), 114, 31, 124, 33);
            using (var flapCap = new GraphicsPath())
            {
                flapCap.AddClosedCurve(new[] { Pt(104, 28), Pt(92, 25), Pt(82, 33), Pt(96, 36) }, 0.5f);
                p.Shape(flapCap, p.Paper, 2);
                p.Tone(flapCap, 1);
                p.G.DrawPath(p.Ink(2), flapCap);
            }
            using (var cap = new GraphicsPath())
            {
                cap.AddClosedCurve(new[] { Pt(100, 28), Pt(106, 17), Pt(121, 15), Pt(132, 21), Pt(128, 29), Pt(110, 31) }, 0.5f);
                p.Shape(cap, p.Paper, 2.6f);
                using var band = new GraphicsPath();
                band.AddPolygon(new[] { Pt(102, 27.5f), Pt(129, 25.5f), Pt(129, 30.5f), Pt(103, 32.5f) });
                p.Shape(band, p.Solid, 1.6f);
                using var visor = new GraphicsPath();
                visor.AddClosedCurve(new[] { Pt(125, 28.5f), Pt(137, 28.5f), Pt(140, 32), Pt(128, 33) }, 0.4f);
                p.Shape(visor, p.Solid, 1.4f);
            }
            // Asa da frente.
            Wing(p, wingsUp, Pt(84, 54), false);
        }
        return Finish(bmp, Day.Ink, 1.3f);
    }

    /// <summary>
    /// Asa de desenho animado: bordo de ataque convexo, ponta preta com três "dedos" (penas primárias abertas) e
    /// bordo de fuga suave até o ombro. A de trás é a mesma forma deslocada, em retícula clara.
    /// </summary>
    private static void Wing(Painter p, bool up, PointF shoulder, bool far)
    {
        float ox = far ? 16 : 0, oy = far ? (up ? 3 : -3) : 0;
        PointF P(float x, float y) => Pt(x + ox, y + oy);
        using var wing = new GraphicsPath();
        PointF tip;
        if (up)
        {
            tip = P(26, -14);
            wing.AddBezier(P(94, 50), P(98, 24), P(72, -2), tip);
            wing.AddBezier(tip, P(38, -8), P(46, 0), P(52, 6));
            wing.AddBezier(P(52, 6), P(48, 7), P(44, 9), P(41, 12));
            wing.AddBezier(P(41, 12), P(48, 11), P(53, 13), P(57, 16));
            wing.AddBezier(P(57, 16), P(53, 18), P(49, 21), P(47, 24));
            wing.AddBezier(P(47, 24), P(54, 22), P(59, 24), P(62, 27));
            wing.AddBezier(P(62, 27), P(58, 38), P(60, 50), P(66, 57));
        }
        else
        {
            tip = P(42, 128);
            wing.AddBezier(P(94, 54), P(102, 80), P(84, 112), tip);
            wing.AddBezier(tip, P(52, 118), P(56, 112), P(61, 107));
            wing.AddBezier(P(61, 107), P(57, 106), P(53, 105), P(50, 103));
            wing.AddBezier(P(50, 103), P(56, 101), P(61, 99), P(64, 96));
            wing.AddBezier(P(64, 96), P(60, 94), P(56, 92), P(54, 89));
            wing.AddBezier(P(54, 89), P(60, 89), P(64, 87), P(67, 84));
            wing.AddBezier(P(67, 84), P(62, 74), P(62, 64), P(66, 58));
        }
        wing.CloseFigure();
        p.G.FillPath(p.Paper, wing);
        if (far) p.Tone(wing, 1);
        var state = p.G.Save();
        p.G.SetClip(wing, CombineMode.Intersect);
        using (var tipZone = Ellipse(tip.X + (up ? 8 : 6), tip.Y + (up ? 12 : -12), 22, 22)) p.G.FillPath(p.Solid, tipZone);
        p.G.Restore(state);
        // Linhas das penas cobertoras, paralelas ao bordo de ataque.
        var pen = p.Line(1.2f);
        if (up)
        {
            p.G.DrawBezier(pen, P(84, 46), P(86, 30), P(76, 18), P(66, 14));
            p.G.DrawBezier(pen, P(74, 50), P(74, 38), P(70, 28), P(64, 22));
        }
        else
        {
            p.G.DrawBezier(pen, P(84, 60), P(88, 74), P(82, 88), P(74, 96));
            p.G.DrawBezier(pen, P(74, 60), P(76, 70), P(74, 80), P(70, 86));
        }
        p.G.DrawPath(p.Ink(2.6f), wing);
        _ = shoulder;
    }

    /// <summary>Den Den Mushi: o caracol-telefone de lado, com o fone em cima da concha, o disco de discar e o fio enrolado.</summary>
    public static Bitmap DenDenMushi()
    {
        var bmp = NewCanvas(190, 160, out var g);
        using (g)
        using (var p = new Painter(g, Day, 1))
        {
            g.TranslateTransform(4, 6);
            // Corpo (pé) com a cabeça erguida à direita.
            using (var body = new GraphicsPath())
            {
                body.AddClosedCurve(new[]
                {
                    Pt(8, 132), Pt(40, 122), Pt(96, 120), Pt(124, 112), Pt(130, 88), Pt(138, 70), Pt(154, 64), Pt(170, 74),
                    Pt(174, 100), Pt(170, 126), Pt(160, 138), Pt(100, 140), Pt(36, 140),
                }, 0.45f);
                p.Shape(body, p.Paper, 3.4f);
                using var shade = Crescent(body, 6, -9);
                p.Tone(shade, 2);
                g.DrawBezier(p.Line(1.4f), Pt(40, 132), Pt(80, 128), Pt(130, 130), Pt(160, 128));
            }
            // Pedúnculos com olhos de pálpebra caída.
            void Eye(PointF baseP, PointF top, float r, bool far)
            {
                using var stalk = new GraphicsPath();
                stalk.AddBezier(baseP, Lerp(baseP, top, 0.4f) with { X = Lerp(baseP, top, 0.4f).X - 3 }, Lerp(baseP, top, 0.75f), top);
                p.Band(stalk, 7, 2.2f);
                using var ball = Ellipse(top.X, top.Y, r, r);
                p.Shape(ball, p.Paper, 2.6f);
                g.FillEllipse(p.Solid, top.X - r * 0.15f, top.Y + r * 0.02f, r * 0.55f, r * 0.6f);
                using var lid = new GraphicsPath();
                lid.AddArc(top.X - r, top.Y - r, 2 * r, 2 * r, 180, 180);
                lid.AddBezier(Pt(top.X + r, top.Y), Pt(top.X + r * 0.4f, top.Y + r * 0.12f), Pt(top.X - r * 0.4f, top.Y + r * 0.12f), Pt(top.X - r, top.Y));
                lid.CloseFigure();
                p.Shape(lid, p.Paper, 2.2f);
                if (far) p.Tone(lid, 1);
                p.G.DrawPath(p.Ink(2.2f), lid);
            }
            Eye(Pt(150, 70), Pt(146, 32), 9, true);
            Eye(Pt(162, 72), Pt(170, 36), 10, false);
            // Boca de lábios grossos.
            using (var lips = new GraphicsPath())
            {
                lips.AddClosedCurve(new[] { Pt(160, 104), Pt(170, 98), Pt(180, 102), Pt(178, 110), Pt(168, 112) }, 0.5f);
                p.Shape(lips, p.Paper, 2.4f);
                g.DrawBezier(p.Line(1.6f), Pt(161, 105), Pt(168, 104), Pt(172, 107), Pt(179, 105));
            }
            // Concha em espiral.
            var sc = Pt(76, 82);
            using (var shell = Ellipse(sc.X, sc.Y, 46, 44))
            {
                p.Shape(shell, p.Paper, 3.6f);
                using var shade = Crescent(shell, 10, -12);
                p.Tone(shade, 1);
                var spiral = new List<PointF>();
                for (double t = 0; t < Math.PI * 4.2; t += 0.12)
                {
                    double r = 42 * (1 - t / (Math.PI * 4.9));
                    spiral.Add(Pt(sc.X + (float)(Math.Cos(t + 0.6) * r * 1.03), sc.Y + (float)(Math.Sin(t + 0.6) * r)));
                }
                g.DrawLines(p.Line(2.2f), spiral.ToArray());
            }
            // Disco de discar.
            var dc = Pt(80, 88);
            using (var dial = Ellipse(dc.X, dc.Y, 19, 19))
            {
                p.Shape(dial, p.Paper, 2.6f);
                for (int i = 0; i < 9; i++)
                {
                    var q = Polar(dc, 12.5f, -60 + i * 32);
                    using var hole = Ellipse(q.X, q.Y, 3, 3);
                    p.Shape(hole, p.Solid, 1);
                }
                using var hub = Ellipse(dc.X, dc.Y, 5, 5);
                p.Shape(hub, p.Paper, 1.8f);
            }
            // Fio enrolado do disco até o fone.
            var coil = new List<PointF>();
            for (double t = 0; t <= 1; t += 0.01)
            {
                var a = Pt(60 + (float)(t * -30), 96 - (float)(t * 50));
                coil.Add(Pt(a.X + (float)Math.Cos(t * 40) * 4, a.Y + (float)Math.Sin(t * 40) * 3));
            }
            g.DrawLines(p.Ink(1.6f), coil.ToArray());
            // Fone (gancho) em cima da concha.
            using (var handle = new GraphicsPath())
            {
                handle.AddBezier(Pt(38, 40), Pt(56, 22), Pt(96, 22), Pt(114, 40));
                p.Band(handle, 7, 2.4f);
                p.G.DrawBezier(p.Line(1.2f), Pt(44, 37), Pt(60, 25), Pt(92, 25), Pt(108, 37));
            }
            foreach (var x in new[] { 34f, 118f })
            {
                using var cup = new GraphicsPath();
                cup.AddClosedCurve(new[] { Pt(x - 14, 50), Pt(x - 10, 39), Pt(x, 35), Pt(x + 10, 39), Pt(x + 14, 50), Pt(x, 52) }, 0.4f);
                p.Shape(cup, p.Solid, 2.4f);
                g.DrawArc(p.Pen(Color.FromArgb(170, 255, 255, 255), 1.6f), x - 8, 39, 12, 8, 190, 80);
            }
        }
        return Finish(bmp, Day.Ink, 1.3f);
    }

    /// <summary>Garrafa com mensagem: vidro inclinado, rolha, o bilhete enrolado com fita e os brilhos do vidro.</summary>
    public static Bitmap Bottle()
    {
        var bmp = NewCanvas(140, 90, out var g);
        using (g)
        using (var p = new Painter(g, Day, 1))
        {
            g.TranslateTransform(70, 46);
            g.RotateTransform(-18);
            g.TranslateTransform(-66, -20);
            using var glass = new GraphicsPath();
            glass.AddArc(4, 0, 18, 40, 90, 180);
            glass.AddLine(13, 0, 80, 0);
            glass.AddBezier(Pt(80, 0), Pt(90, 0), Pt(94, 13), Pt(104, 13));
            glass.AddLine(104, 13, 116, 13);
            glass.AddLine(116, 27, 104, 27);
            glass.AddBezier(Pt(104, 27), Pt(94, 27), Pt(90, 40), Pt(80, 40));
            glass.AddLine(80, 40, 13, 40);
            glass.CloseFigure();
            g.FillPath(p.Fill(Color.FromArgb(205, 255, 255, 255)), glass);
            // Bilhete enrolado lá dentro.
            using (var scroll = new GraphicsPath())
            {
                scroll.AddRectangle(new RectangleF(22, 11, 52, 18));
                p.Shape(scroll, p.Paper, 2);
                for (int i = 1; i < 4; i++) g.DrawLine(p.Line(1), 26 + i * 12, 14, 26 + i * 12, 26);
                using var end = Ellipse(74, 20, 4, 9);
                p.Shape(end, p.Paper, 1.8f);
                g.DrawArc(p.Line(1.1f), 72, 15, 4, 8, 90, 260);
                g.FillRectangle(p.Solid, 46, 10, 5, 20);
            }
            using (var shade = Crescent(glass, 0, -9)) p.Tone(shade, 1);
            p.G.DrawPath(p.Ink(3), glass);
            // Bocal e rolha.
            p.Band(Pt(115, 11), Pt(115, 29), 3.5f, 1.4f);
            using (var cork = new GraphicsPath())
            {
                cork.AddPolygon(new[] { Pt(117, 14), Pt(130, 15), Pt(130, 25), Pt(117, 26) });
                p.Shape(cork, p.Paper, 2.2f);
                p.Tone(cork, 2);
                p.G.DrawPath(p.Ink(2.2f), cork);
            }
            // Brilhos do vidro.
            var shine = p.Pen(Color.White, 3);
            g.DrawLine(p.Ink(5.4f), 22, 5.5f, 64, 5.5f);
            g.DrawLine(shine, 22, 5.5f, 64, 5.5f);
            g.DrawLine(p.Line(1.3f), 70, 6, 76, 6);
            g.DrawArc(p.Line(1.4f), 7, 4, 10, 32, 120, 120);
        }
        return Finish(bmp, Day.Ink, 1.2f);
    }

    /// <summary>Notas musicais (colcheias ligadas), pretas com brilho e um halo branco para aparecer em fundo escuro.</summary>
    public static Bitmap Note()
    {
        var bmp = NewCanvas(76, 86, out var g);
        using (g)
        using (var p = new Painter(g, Day, 1))
        {
            g.TranslateTransform(4, 4);
            void Head(float x, float y)
            {
                var s = g.Save();
                g.TranslateTransform(x, y);
                g.RotateTransform(-24);
                g.FillEllipse(p.Solid, -11, -7.5f, 22, 15);
                g.DrawArc(p.Pen(Color.White, 2), -6, -4.5f, 9, 6, 190, 90);
                g.Restore(s);
            }
            Head(16, 66);
            Head(52, 56);
            g.FillRectangle(p.Solid, 23.5f, 14, 5, 52);
            g.FillRectangle(p.Solid, 59.5f, 4, 5, 52);
            g.FillPolygon(p.Solid, new[] { Pt(23.5f, 12), Pt(64.5f, 2), Pt(64.5f, 15), Pt(23.5f, 25) });
            g.FillPolygon(p.Solid, new[] { Pt(23.5f, 30), Pt(64.5f, 20), Pt(64.5f, 27), Pt(23.5f, 37) });
        }
        var inked = Finish(bmp, Color.White, 2.4f);
        return inked;
    }

    /// <summary>Bolha de Sabaody: aro fino, interior quase transparente, brilho em janela e um reflexo do lado oposto.</summary>
    public static Bitmap Bubble(float radius)
    {
        radius = Math.Max(5, radius);
        int size = (int)Math.Ceiling(radius * 2 + 12);
        var bmp = NewCanvas(size, size, out var g);
        using (g)
        using (var p = new Painter(g, Day, 1))
        {
            var c = Pt(size / 2f, size / 2f);
            using var ball = Ellipse(c.X, c.Y, radius, radius);
            g.FillPath(p.Fill(Color.FromArgb(60, 255, 255, 255)), ball);
            using (var rim = Crescent(ball, -radius * 0.12f, -radius * 0.12f))
            using (var dots = DotBrush(Color.FromArgb(150, 20, 20, 24), 6, radius < 18 ? 0.9f : 1.25f, 1))
                g.FillRegion(dots, rim);
            float lw = radius < 18 ? 1.8f : 2.4f;
            g.DrawPath(p.Ink(lw), ball);
            // Janela de brilho (curva) no alto à esquerda e um pontinho.
            using (var shine = new GraphicsPath())
            {
                shine.AddArc(c.X - radius * 0.78f, c.Y - radius * 0.78f, radius * 1.56f, radius * 1.56f, 196, 62);
                shine.AddArc(c.X - radius * 0.56f, c.Y - radius * 0.56f, radius * 1.12f, radius * 1.12f, 258, -62);
                shine.CloseFigure();
                p.Shape(shine, p.Fill(Color.White), Math.Max(1, lw * 0.55f));
            }
            using (var dot = Ellipse(c.X - radius * 0.62f, c.Y - radius * 0.05f, radius * 0.09f, radius * 0.09f))
                p.Shape(dot, p.Fill(Color.White), Math.Max(1, lw * 0.5f));
            g.DrawArc(p.Line(Math.Max(1, lw * 0.6f)), c.X - radius * 0.8f, c.Y - radius * 0.8f, radius * 1.6f, radius * 1.6f, 15, 55);
        }
        return bmp;
    }

    /// <summary>Barril de madeira em pé: aduelas, arcos de ferro, tampa com tábuas, batoque e sombra em retícula.</summary>
    public static Bitmap Barrel()
    {
        var bmp = NewCanvas(110, 130, out var g);
        using (g)
        using (var p = new Painter(g, Day, 1))
        {
            g.TranslateTransform(55, 66);
            g.RotateTransform(-6);
            const float top = -46, bot = 48, rxEnd = 31, ryEnd = 9, rxMid = 38;
            float Rx(float y)
            {
                float t = (y - top) / (bot - top);
                return rxEnd + (rxMid - rxEnd) * (float)Math.Sin(Math.PI * t);
            }
            using var body = new GraphicsPath();
            body.AddBezier(Pt(-rxEnd, top), Pt(-rxMid - 4, top + 30), Pt(-rxMid - 4, bot - 30), Pt(-rxEnd, bot));
            body.AddArc(-rxEnd, bot - ryEnd, 2 * rxEnd, 2 * ryEnd, 180, -180);
            body.AddBezier(Pt(rxEnd, bot), Pt(rxMid + 4, bot - 30), Pt(rxMid + 4, top + 30), Pt(rxEnd, top));
            body.AddArc(-rxEnd, top - ryEnd, 2 * rxEnd, 2 * ryEnd, 0, 180);
            body.CloseFigure();
            p.Shape(body, p.Paper, 3.2f);
            using (var shade = Crescent(body, 14, 0)) p.Tone(shade, 2);
            using (var edge = Crescent(body, 5, 0)) p.Hatch(edge, body.GetBounds(), 80, 3.2f, 1.2f);
            // Aduelas.
            var state = g.Save();
            g.SetClip(body, CombineMode.Intersect);
            for (int i = -3; i <= 3; i++)
            {
                double phi = i * 0.38;
                var pts = new List<PointF>();
                for (float y = top; y <= bot + 0.1f; y += 4) pts.Add(Pt((float)Math.Sin(phi) * Rx(y), y + ryEnd * (float)Math.Cos(phi)));
                g.DrawLines(p.Line(1.3f), pts.ToArray());
            }
            g.Restore(state);
            // Arcos de ferro.
            foreach (float y in new[] { top + 9, top + 17, bot - 17, bot - 9 })
            {
                float rx = Rx(y);
                using var hoop = new GraphicsPath();
                hoop.AddArc(-rx, y - ryEnd, 2 * rx, 2 * ryEnd, 180, -180);
                p.Band(hoop, 4.2f, 1.4f, p.P.Solid);
            }
            // Tampa.
            using (var lid = Ellipse(0, top, rxEnd, ryEnd))
            {
                p.Shape(lid, p.Paper, 3);
                var s2 = g.Save();
                g.SetClip(lid, CombineMode.Intersect);
                for (int i = -2; i <= 2; i++) g.DrawLine(p.Line(1.2f), i * 11 - 6, top - ryEnd, i * 11 + 6, top + ryEnd);
                g.Restore(s2);
                using var rim = Ellipse(0, top, rxEnd - 4, ryEnd - 2.4f);
                g.DrawPath(p.Line(1.3f), rim);
            }
            // Batoque.
            using (var bung = Ellipse(10, 2, 4.5f, 6))
            {
                p.Shape(bung, p.Solid, 1.6f);
            }
        }
        return Finish(bmp, Day.Ink, 1.3f);
    }

    #endregion
}
