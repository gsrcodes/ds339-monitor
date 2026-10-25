using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace DS339Monitor.Themes;

/// <summary>Para onde a "câmera" anda sobre o painel.</summary>
internal enum PanelMotion
{
    /// <summary>Ordem de leitura do mangá: desce no painel mais alto que a tela, vai da direita para a esquerda no mais largo.</summary>
    Auto,
    Down,
    Up,
    Left,
    Right,
    Still,
}

/// <summary>Como a apresentação passa de um painel para o outro.</summary>
internal enum PanelTurn
{
    /// <summary>Corte diagonal (painéis de índice par).</summary>
    Cut,
    /// <summary>Retícula (painéis de índice ímpar).</summary>
    Halftone,
}

/// <summary>Momento que o LED da placa encena enquanto o painel está na tela (campo "luz" do quadros.json).</summary>
internal enum PanelCue
{
    None,
    /// <summary>"impacto": um DON! quando o painel entra, com eco.</summary>
    Impact,
    /// <summary>"tambores": os Tambores da Libertação do Gear 5.</summary>
    Drums,
    /// <summary>"tremor": a luz treme (a Gura Gura do Barba Branca).</summary>
    Quake,
    /// <summary>"mare": ondas lentas.</summary>
    Tide,
    /// <summary>"haki": a luz cai e estala em clarões curtos (Haki do Rei).</summary>
    Haki,
    /// <summary>"despedida": a luz baixa devagar e fica fraca.</summary>
    Farewell,
    /// <summary>"amanhecer": a luz cai e nasce de novo ao longo do painel.</summary>
    Dawn,
}

/// <summary>
/// O que a apresentação mostra num quadro: o painel, se está no movimento dele ou virando para o próximo, há quanto
/// tempo (<see cref="Seconds"/>) e quanto essa fase dura (<see cref="Duration"/>).
/// </summary>
internal readonly record struct PanelScene(int Panel, bool Turning, PanelTurn Turn, double Seconds, double Duration, PanelCue Cue);

/// <summary>
/// Apresentação de painéis em tela cheia, montada quadro a quadro na hora (nada fica pré-codificado). Cada painel cobre
/// a tela inteira e a "câmera" o percorre devagar — acelera suave, anda em velocidade constante e freia suave —, então
/// nada dele fica de fora. A posição da câmera é fracionária: cada quadro é reamostrado (bicúbico, só no eixo em que a
/// câmera anda) na posição exata, a no máximo 1 px por quadro. Andar em pixels inteiros dava passos de 1–3 px em
/// intervalos irregulares, e como a tela troca a imagem no meio da varredura (sem sincronismo), parte dela mostrava o
/// quadro novo e parte o anterior: com passos grandes a imagem "se desencaixava".
/// Entre um painel e outro, uma transição de mangá: um corte diagonal (a divisa branca entre dois quadros da página)
/// que passa da esquerda para a direita, como ao virar a página; ou a retícula — pontos que crescem até revelar o
/// próximo painel.
/// Os painéis ficam em memória em tons de cinza (1 byte por pixel), já no tamanho final: montar um quadro custa ~2 ms.
/// </summary>
internal sealed class PanelSlideshow : IAnimation
{
    /// <summary>30 quadros por segundo, o mesmo ritmo do app original.</summary>
    public const int FrameMs = 33;

    private const int TransitionFrames = 18;  // ~0,6 s
    private const int MinTravel = 40;          // painel com a proporção exata da tela ganha um leve zoom para andar
    private const double MaxSpeed = 30;        // px/s no trecho de velocidade constante (1 px por quadro)
    private const double RampSeconds = 1.6;    // acelerando no começo e freando no fim
    private const int CaptionDelayFrames = 10;
    private const int CaptionSlideFrames = 11;
    private const int CaptionMargin = 16;
    private const int Gutter = 18;             // corte diagonal: faixa branca entre os dois painéis...
    private const int Ink = 3;                 // ...com o traço preto dos dois lados
    private const double CutSlope = 0.27;      // inclinação do corte (~15°)
    private const int DotSpacing = 12;         // retícula: distância entre os pontos, em diagonal

    private static readonly uint[] GrayToPixel = Enumerable.Range(0, 256).Select(v => 0xFF000000u | (uint)(v << 16 | v << 8 | v)).ToArray();

    private readonly Slide[] _slides;
    private readonly int[] _starts; // primeiro quadro de cada painel (movimento + transição para o próximo)
    private byte[]? _dots;          // distância de cada pixel ao centro do ponto da retícula (0–255)
    private byte[]? _sweep;         // ordem em que a retícula avança pela tela (0–255)

    public PanelSlideshow(IReadOnlyList<Slide> slides, int width, int height)
    {
        if (slides.Count == 0) throw new ArgumentException("Nenhum painel.", nameof(slides));
        _slides = slides.ToArray();
        Width = width;
        Height = height;
        _starts = new int[_slides.Length + 1];
        for (int i = 0; i < _slides.Length; i++)
            _starts[i + 1] = _starts[i] + _slides[i].Frames + (_slides.Length > 1 ? TransitionFrames : 0);
    }

    public int Count => _starts[^1];
    public int Width { get; }
    public int Height { get; }
    public int SlideCount => _slides.Length;
    public bool CacheFrames => false;
    public long MemoryBytes => _slides.Sum(s => (long)s.Pixels.Length) + (_dots?.Length ?? 0) * 2L;

    public int DelayMs(int index) => FrameMs;

    /// <summary>Quadro do movimento do painel <paramref name="slide"/>, na fração <paramref name="t"/> dele (para prévias).</summary>
    public int PanOf(int slide, double t) =>
        _starts[slide % _slides.Length] + (int)Math.Clamp(t * (_slides[slide % _slides.Length].Frames - 1), 0, _slides[slide % _slides.Length].Frames - 1);

    /// <summary>Quadro da transição que sai do painel <paramref name="slide"/>, na fração <paramref name="t"/> dela (para prévias).</summary>
    public int TransitionOf(int slide, double t) =>
        _starts[slide % _slides.Length] + _slides[slide % _slides.Length].Frames + (int)Math.Clamp(t * TransitionFrames, 0, TransitionFrames - 1);

    /// <summary>Primeiro quadro do primeiro painel com o momento <paramref name="cue"/> (null se nenhum tem).</summary>
    public int? FirstFrameOf(PanelCue cue)
    {
        for (int i = 0; i < _slides.Length; i++)
            if (_slides[i].Cue == cue) return _starts[i];
        return null;
    }

    /// <summary>O que o quadro <paramref name="index"/> mostra (para o LED da placa acompanhar).</summary>
    public PanelScene Describe(int index)
    {
        var (i, k) = Locate(index);
        var slide = _slides[i];
        bool turning = k >= slide.Frames;
        int frame = turning ? k - slide.Frames : k;
        int length = turning ? TransitionFrames : slide.Frames;
        return new PanelScene(i, turning, i % 2 == 0 ? PanelTurn.Cut : PanelTurn.Halftone,
            frame * FrameMs / 1000.0, length * FrameMs / 1000.0, slide.Cue);
    }

    public long FrameKey(int index)
    {
        var (i, k) = Locate(index);
        var slide = _slides[i];
        if (k >= slide.Frames) return HashCode.Combine(i, -1, k);
        var at = slide.PositionAt(k);
        return HashCode.Combine(i, (int)Math.Round(at.X * 32), (int)Math.Round(at.Y * 32), CaptionX(slide, k));
    }

    public unsafe void DrawFrame(Bitmap canvas, Graphics g, int index)
    {
        var (i, k) = Locate(index);
        var slide = _slides[i];
        g.Flush(FlushIntention.Sync);
        var data = canvas.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
        try
        {
            var target = (uint*)data.Scan0;
            int stride = data.Stride / 4;
            if (k < slide.Frames)
            {
                Sample(slide, slide.PositionAt(k), target, stride);
            }
            else
            {
                var next = _slides[(i + 1) % _slides.Length];
                double t = (k - slide.Frames + 1) / (double)(TransitionFrames + 1);
                t = t * t * (3 - 2 * t);
                var from = slide.To;
                var to = next.From;
                if (i % 2 == 0) Cut(slide, from, next, to, t, target, stride);
                else Halftone(slide, from, next, to, t, target, stride);
            }
        }
        finally
        {
            canvas.UnlockBits(data);
        }

        if (k < slide.Frames && slide.Caption is not null)
        {
            int x = CaptionX(slide, k);
            if (x > -slide.Caption.Width)
            {
                var mode = g.CompositingMode;
                g.CompositingMode = CompositingMode.SourceOver;
                g.DrawImage(slide.Caption, new Rectangle(x, CaptionMargin, slide.Caption.Width, slide.Caption.Height));
                g.CompositingMode = mode;
            }
        }
    }

    public void Dispose()
    {
        foreach (var slide in _slides) slide.Caption?.Dispose();
    }

    private (int Slide, int Frame) Locate(int index)
    {
        int i = Array.BinarySearch(_starts, index);
        if (i < 0) i = ~i - 1;
        i = Math.Clamp(i, 0, _slides.Length - 1);
        return (i, index - _starts[i]);
    }

    /// <summary>A caixa de narração entra deslizando pela esquerda logo depois da transição e sai antes da próxima.</summary>
    private static int CaptionX(Slide slide, int k)
    {
        if (slide.Caption is null) return int.MinValue;
        int hidden = -slide.Caption.Width - 4;
        int enter = k - CaptionDelayFrames;
        int leave = slide.Frames - 4 - k; // quadros até sumir de vez
        double t = Math.Min(enter, leave) / (double)CaptionSlideFrames;
        if (t <= 0) return hidden;
        if (t >= 1) return CaptionMargin;
        t = 1 - Math.Pow(1 - t, 3); // freia ao chegar
        return (int)Math.Round(hidden + (CaptionMargin - hidden) * t);
    }

    /// <summary>
    /// Janela da tela sobre o painel na posição <paramref name="at"/> (fracionária). A câmera anda num eixo só: nele os
    /// pixels são interpolados com Catmull-Rom (4 vizinhos, nitidez quase igual à do original); posição inteira é cópia.
    /// </summary>
    private unsafe void Sample(Slide slide, PointF at, uint* target, int stride)
    {
        int x0 = (int)Math.Floor(at.X), y0 = (int)Math.Floor(at.Y);
        double fx = at.X - x0, fy = at.Y - y0;
        const double Epsilon = 1 / 64.0;
        if (fx >= 1 - Epsilon) { x0++; fx = 0; }
        if (fy >= 1 - Epsilon) { y0++; fy = 0; }
        bool vertical = fy > Epsilon, horizontal = fx > Epsilon && !vertical;
        if (!vertical && !horizontal)
        {
            var whole = new Point(x0, y0);
            for (int y = 0; y < Height; y++) CopyRow(slide, whole, y, 0, Width, target + y * stride);
            return;
        }

        var (w0, w1, w2, w3) = CatmullRom(vertical ? fy : fx);
        fixed (byte* pixels = slide.Pixels)
        {
            for (int y = 0; y < Height; y++)
            {
                uint* row = target + y * stride;
                if (vertical)
                {
                    int sy = y0 + y;
                    byte* r0 = pixels + (long)Math.Max(sy - 1, 0) * slide.Width + x0;
                    byte* r1 = pixels + (long)sy * slide.Width + x0;
                    byte* r2 = pixels + (long)Math.Min(sy + 1, slide.Height - 1) * slide.Width + x0;
                    byte* r3 = pixels + (long)Math.Min(sy + 2, slide.Height - 1) * slide.Width + x0;
                    for (int x = 0; x < Width; x++)
                        row[x] = GrayToPixel[Clamp255((w0 * r0[x] + w1 * r1[x] + w2 * r2[x] + w3 * r3[x] + 2048) >> 12)];
                }
                else
                {
                    // Nas bordas do painel faltam vizinhos: repete o pixel da borda.
                    byte* line = pixels + (long)(y0 + y) * slide.Width;
                    int last = slide.Width - 1;
                    for (int x = 0; x < Width; x++)
                    {
                        int c = x0 + x;
                        int v = c >= 1 && c + 2 <= last
                            ? w0 * line[c - 1] + w1 * line[c] + w2 * line[c + 1] + w3 * line[c + 2]
                            : w0 * line[Math.Max(c - 1, 0)] + w1 * line[c] + w2 * line[Math.Min(c + 1, last)] + w3 * line[Math.Min(c + 2, last)];
                        row[x] = GrayToPixel[Clamp255((v + 2048) >> 12)];
                    }
                }
            }
        }
    }

    /// <summary>Pesos Catmull-Rom para a fração <paramref name="t"/> (somam 4096).</summary>
    private static (int, int, int, int) CatmullRom(double t)
    {
        double t2 = t * t, t3 = t2 * t;
        int w0 = (int)Math.Round((-t3 + 2 * t2 - t) / 2 * 4096);
        int w1 = (int)Math.Round((3 * t3 - 5 * t2 + 2) / 2 * 4096);
        int w3 = (int)Math.Round((t3 - t2) / 2 * 4096);
        return (w0, w1, 4096 - w0 - w1 - w3, w3);
    }

    private static int Clamp255(int v) => v < 0 ? 0 : v > 255 ? 255 : v;

    private static unsafe void CopyRow(Slide slide, Point at, int y, int x0, int x1, uint* row)
    {
        if (x1 <= x0) return;
        fixed (byte* pixels = slide.Pixels)
        {
            byte* source = pixels + (long)(at.Y + y) * slide.Width + at.X;
            for (int x = x0; x < x1; x++) row[x] = GrayToPixel[source[x]];
        }
    }

    /// <summary>
    /// Corte diagonal: a divisa entre dois quadros (faixa branca com traço preto dos dois lados) atravessa a tela da
    /// esquerda para a direita; o próximo painel fica à esquerda dela, como numa página que vira.
    /// </summary>
    private unsafe void Cut(Slide current, Point from, Slide next, Point to, double t, uint* target, int stride)
    {
        double lean = CutSlope * Height;
        double reach = lean / 2 + Gutter / 2.0 + Ink + 2;
        double center = -reach + (Width + 2 * reach) * t;
        uint black = GrayToPixel[0], white = GrayToPixel[255];
        for (int y = 0; y < Height; y++)
        {
            uint* row = target + y * stride;
            int b = (int)Math.Round(center + (Height / 2.0 - y) * CutSlope); // "/": mais à direita em cima
            int newEnd = Math.Clamp(b - Gutter / 2 - Ink, 0, Width);
            int gutterStart = Math.Clamp(b - Gutter / 2, 0, Width);
            int gutterEnd = Math.Clamp(b + Gutter / 2, 0, Width);
            int oldStart = Math.Clamp(b + Gutter / 2 + Ink, 0, Width);
            CopyRow(next, to, y, 0, newEnd, row);
            for (int x = newEnd; x < gutterStart; x++) row[x] = black;
            for (int x = gutterStart; x < gutterEnd; x++) row[x] = white;
            for (int x = gutterEnd; x < oldStart; x++) row[x] = black;
            CopyRow(current, from, y, oldStart, Width, row);
        }
    }

    /// <summary>
    /// Retícula: pontos de retícula (grade a 45°, como no mangá impresso) crescem até cobrir a tela, mostrando o próximo
    /// painel; a onda começa no canto de cima à direita, onde a leitura de uma página começa.
    /// </summary>
    private unsafe void Halftone(Slide current, Point from, Slide next, Point to, double t, uint* target, int stride)
    {
        EnsureHalftoneMaps();
        const double Spread = 1.3; // quanto a onda se espalha: vários tamanhos de ponto na tela ao mesmo tempo
        Span<int> threshold = stackalloc int[256];
        for (int g = 0; g < 256; g++)
            threshold[g] = (int)Math.Round(Math.Clamp(t * (1 + Spread) - g / 255.0 * Spread, 0, 1) * 256);

        fixed (byte* dots = _dots, sweep = _sweep, oldPixels = current.Pixels, newPixels = next.Pixels)
        {
            for (int y = 0; y < Height; y++)
            {
                uint* row = target + y * stride;
                byte* dotRow = dots + y * Width;
                byte* sweepRow = sweep + y * Width;
                byte* oldRow = oldPixels + (long)(from.Y + y) * current.Width + from.X;
                byte* newRow = newPixels + (long)(to.Y + y) * next.Width + to.X;
                for (int x = 0; x < Width; x++)
                    row[x] = GrayToPixel[dotRow[x] < threshold[sweepRow[x]] ? newRow[x] : oldRow[x]];
            }
        }
    }

    private void EnsureHalftoneMaps()
    {
        if (_dots is not null) return;
        var dots = new byte[Width * Height];
        var sweep = new byte[Width * Height];
        double diagonal = Width + Height;
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                // Grade girada 45°: distância ao centro do ponto mais próximo, 0 no centro e 1 no canto da célula.
                double u = (x + y) / Math.Sqrt(2) / DotSpacing, v = (x - y) / Math.Sqrt(2) / DotSpacing;
                double fu = u - Math.Floor(u) - 0.5, fv = v - Math.Floor(v) - 0.5;
                double d = Math.Sqrt(fu * fu + fv * fv) / Math.Sqrt(0.5);
                dots[y * Width + x] = (byte)Math.Min(255, d * 255);
                sweep[y * Width + x] = (byte)Math.Min(255, (Width - x + y) / diagonal * 255);
            }
        }
        _dots = dots;
        _sweep = sweep;
    }

    // ---------------------------------------------------------------- preparo dos painéis

    /// <summary>Um painel pronto: pixels em tons de cinza no tamanho final e o caminho da câmera.</summary>
    public sealed class Slide
    {
        public required byte[] Pixels { get; init; }
        public required int Width { get; init; }
        public required int Height { get; init; }
        /// <summary>Canto de cima à esquerda da janela da tela sobre o painel, no começo e no fim do movimento.</summary>
        public required Point From { get; init; }
        public required Point To { get; init; }
        public required int Frames { get; init; }
        /// <summary>Fração do tempo gasta acelerando (e a mesma freando).</summary>
        public double Ramp { get; init; }
        /// <summary>Caixa de narração já desenhada (com transparência), ou null.</summary>
        public Bitmap? Caption { get; set; }
        /// <summary>Momento que o LED da placa encena neste painel.</summary>
        public PanelCue Cue { get; set; }

        public PointF PositionAt(int k)
        {
            if (Frames <= 1 || From == To) return From;
            double p = Travelled(Math.Clamp(k / (double)(Frames - 1), 0, 1), Ramp);
            return new PointF((float)(From.X + (To.X - From.X) * p), (float)(From.Y + (To.Y - From.Y) * p));
        }

        /// <summary>
        /// Fração do caminho feita no instante <paramref name="t"/> (0–1): a velocidade sobe em curva suave durante
        /// <paramref name="ramp"/>, fica constante e desce do mesmo jeito no fim.
        /// </summary>
        public static double Travelled(double t, double ramp)
        {
            if (ramp <= 0) return t;
            if (t > 0.5) return 1 - Travelled(1 - t, ramp);
            double peak = 1 / (1 - ramp); // velocidade constante do meio, para o total dar 1
            return t < ramp
                ? peak * (t / 2 - ramp / (2 * Math.PI) * Math.Sin(Math.PI * t / ramp))
                : peak * (ramp / 2 + (t - ramp));
        }
    }

    /// <summary>Opções de enquadramento de um painel (vêm do quadros.json).</summary>
    public sealed record Framing(PanelMotion Motion = PanelMotion.Auto, double Start = 0, double End = 1, RectangleF Trim = default);

    /// <summary>
    /// Prepara um painel para cobrir <paramref name="width"/>x<paramref name="height"/>: corta o que foi pedido e as
    /// margens lisas (papel em branco, bordas pretas do scan), passa para tons de cinza, ajusta o preto e o branco e
    /// redimensiona com a sobra no eixo em que a câmera anda.
    /// </summary>
    public static Slide Prepare(Bitmap source, Framing framing, int width, int height, double seconds)
    {
        var area = TrimMargins(source, Requested(source, framing.Trim));

        var motion = framing.Motion;
        float cover = Math.Max((float)width / area.Width, (float)height / area.Height);
        float excessX = area.Width * cover - width, excessY = area.Height * cover - height;
        if (motion == PanelMotion.Auto) motion = excessX > excessY ? PanelMotion.Left : PanelMotion.Down;
        bool horizontal = motion is PanelMotion.Left or PanelMotion.Right;
        float scale = motion == PanelMotion.Still ? cover
            : horizontal ? Math.Max(cover, (float)(width + MinTravel) / area.Width)
            : Math.Max(cover, (float)(height + MinTravel) / area.Height);
        int w = Math.Max(width, (int)Math.Round(area.Width * scale));
        int h = Math.Max(height, (int)Math.Round(area.Height * scale));
        var pixels = ToGray(source, area, w, h);
        Levels(pixels);
        if (w < area.Width * 0.95) Soften(pixels, w, h); // ampliado já sai macio; só o reduzido guarda o hachurado fino

        int spanX = w - width, spanY = h - height;
        double start = Math.Clamp(framing.Start, 0, 1), end = Math.Clamp(framing.End, 0, 1);
        Point Along(double f) => motion switch
        {
            PanelMotion.Down => new Point(spanX / 2, (int)Math.Round(spanY * f)),
            PanelMotion.Up => new Point(spanX / 2, (int)Math.Round(spanY * (1 - f))),
            PanelMotion.Right => new Point((int)Math.Round(spanX * f), spanY / 2),
            PanelMotion.Left => new Point((int)Math.Round(spanX * (1 - f)), spanY / 2),
            _ => new Point((int)Math.Round(spanX * f), (int)Math.Round(spanY * f)),
        };
        var from = motion == PanelMotion.Still ? Along(framing.Start is 0 ? 0.5 : start) : Along(start);
        var to = motion == PanelMotion.Still ? from : Along(end);
        double travel = Math.Abs(to.X - from.X) + Math.Abs(to.Y - from.Y);
        double ramp = Math.Min(0.25, RampSeconds / seconds);
        double duration = Math.Max(seconds, travel / (1 - ramp) / MaxSpeed); // no trecho constante, até MaxSpeed
        ramp = Math.Min(0.25, RampSeconds / duration);
        return new Slide
        {
            Pixels = pixels, Width = w, Height = h, From = from, To = to, Ramp = ramp,
            Frames = Math.Max(2, (int)(duration * 1000 / FrameMs)),
        };
    }

    private static Rectangle Requested(Bitmap source, RectangleF trim)
    {
        // trim = frações a cortar de cada lado (esquerda, cima, direita, baixo), guardadas como X, Y, Width, Height.
        int left = (int)(source.Width * Math.Clamp(trim.X, 0, 0.45f));
        int top = (int)(source.Height * Math.Clamp(trim.Y, 0, 0.45f));
        int right = (int)(source.Width * Math.Clamp(trim.Width, 0, 0.45f));
        int bottom = (int)(source.Height * Math.Clamp(trim.Height, 0, 0.45f));
        return Rectangle.FromLTRB(left, top, source.Width - right, source.Height - bottom);
    }

    /// <summary>Tira linhas e colunas lisas das bordas (≥ 97% do papel branco ou preto do scan), até 12% de cada lado.</summary>
    private static unsafe Rectangle TrimMargins(Bitmap source, Rectangle area)
    {
        var data = source.LockBits(area, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        try
        {
            byte Luma(int x, int y)
            {
                byte* p = (byte*)data.Scan0 + y * data.Stride + x * 4;
                return (byte)((p[2] * 77 + p[1] * 150 + p[0] * 29) >> 8);
            }
            bool Plain(int count, Func<int, byte> sample)
            {
                int white = 0, black = 0;
                for (int i = 0; i < count; i++)
                {
                    byte v = sample(i);
                    if (v > 232) white++;
                    else if (v < 22) black++;
                }
                return white >= count * 0.97 || black >= count * 0.97;
            }

            int w = area.Width, h = area.Height;
            int maxX = w * 12 / 100, maxY = h * 12 / 100;
            int top = 0, bottom = 0, left = 0, right = 0;
            while (top < maxY && Plain(w, x => Luma(x, top))) top++;
            while (bottom < maxY && Plain(w, x => Luma(x, h - 1 - bottom))) bottom++;
            while (left < maxX && Plain(h - top - bottom, y => Luma(left, top + y))) left++;
            while (right < maxX && Plain(h - top - bottom, y => Luma(w - 1 - right, top + y))) right++;
            return Rectangle.FromLTRB(area.Left + left, area.Top + top, area.Right - right, area.Bottom - bottom);
        }
        finally
        {
            source.UnlockBits(data);
        }
    }

    /// <summary>Redimensiona (bicúbico) já em tons de cinza e devolve 1 byte por pixel.</summary>
    private static unsafe byte[] ToGray(Bitmap source, Rectangle area, int width, int height)
    {
        using var scaled = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(scaled))
        using (var attributes = new ImageAttributes())
        {
            attributes.SetColorMatrix(new ColorMatrix(new[]
            {
                new[] { 0.299f, 0.299f, 0.299f, 0, 0 },
                new[] { 0.587f, 0.587f, 0.587f, 0, 0 },
                new[] { 0.114f, 0.114f, 0.114f, 0, 0 },
                new[] { 0f, 0, 0, 1, 0 },
                new[] { 0f, 0, 0, 0, 1 },
            }));
            attributes.SetWrapMode(WrapMode.TileFlipXY); // sem borda escura no contorno ao reduzir
            g.Clear(Color.White);                         // fundo para PNG com transparência
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.DrawImage(source, new Rectangle(0, 0, width, height), area.X, area.Y, area.Width, area.Height, GraphicsUnit.Pixel, attributes);
        }

        var pixels = new byte[width * height];
        var data = scaled.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        try
        {
            for (int y = 0; y < height; y++)
            {
                byte* row = (byte*)data.Scan0 + y * data.Stride;
                for (int x = 0; x < width; x++) pixels[y * width + x] = row[x * 4 + 1];
            }
        }
        finally
        {
            scaled.UnlockBits(data);
        }
        return pixels;
    }

    /// <summary>
    /// Desfoque gaussiano bem leve (σ ≈ 0,65 px). A tela só aceita JPEG de até 80 KB, e o hachurado e as retículas do
    /// mangá custam caro: sem isso um painel cheio só cabe com qualidade ~35 (linhas borradas em blocos); com isso cabe
    /// com ~60 e as linhas ficam limpas — a 195 pixels por polegada o desfoque em si não aparece.
    /// </summary>
    private static void Soften(byte[] pixels, int width, int height)
    {
        ReadOnlySpan<int> kernel = stackalloc int[] { 1, 48, 158, 48, 1 }; // soma 256
        var temp = new byte[pixels.Length];
        for (int y = 0; y < height; y++)
        {
            int row = y * width;
            for (int x = 0; x < width; x++)
            {
                int sum = 0;
                for (int k = -2; k <= 2; k++) sum += kernel[k + 2] * pixels[row + Math.Clamp(x + k, 0, width - 1)];
                temp[row + x] = (byte)((sum + 128) >> 8);
            }
        }
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int sum = 0;
                for (int k = -2; k <= 2; k++) sum += kernel[k + 2] * temp[Math.Clamp(y + k, 0, height - 1) * width + x];
                pixels[y * width + x] = (byte)((sum + 128) >> 8);
            }
        }
    }

    /// <summary>Estica o histograma para o preto e o branco de verdade (scans lavados ou amarelados ficam iguais aos outros).</summary>
    private static void Levels(byte[] pixels)
    {
        var histogram = new int[256];
        foreach (byte v in pixels) histogram[v]++;
        int Percentile(double fraction)
        {
            long target = (long)(pixels.Length * fraction), seen = 0;
            for (int v = 0; v < 256; v++)
                if ((seen += histogram[v]) > target) return v;
            return 255;
        }
        int low = Math.Min(Percentile(0.003), 70), high = Math.Max(Percentile(0.997), 170);
        if (low <= 2 && high >= 253) return;
        Span<byte> map = stackalloc byte[256];
        for (int v = 0; v < 256; v++) map[v] = (byte)Math.Clamp((v - low) * 255 / (high - low), 0, 255);
        for (int i = 0; i < pixels.Length; i++) pixels[i] = map[pixels[i]];
    }
}
