using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace DS339Monitor.Themes;

/// <summary>
/// Animação pré-processada: cada quadro já no tamanho da tela, guardado como JPEG em memória, com sua duração.
/// Um GIF de 100 quadros ocupa ~8 MB assim, contra ~180 MB em pixels crus; ler um quadro de volta custa ~2 ms.
/// </summary>
internal sealed class FrameAnimation : IAnimation
{
    private const int MaxGifFrames = 600;

    private readonly byte[][] _frames;

    private FrameAnimation(byte[][] frames, int[] delays, int width, int height)
    {
        _frames = frames;
        Delays = delays;
        Width = width;
        Height = height;
    }

    public int Count => _frames.Length;
    /// <summary>Duração de cada quadro, em ms.</summary>
    public int[] Delays { get; }
    public int Width { get; }
    public int Height { get; }
    public long CacheBytes => _frames.Sum(f => (long)f.Length);
    public bool CacheFrames => true;

    public int DelayMs(int index) => Delays[index];

    public long FrameKey(int index) => index;

    public void Dispose()
    {
    }

    public static FrameAnimation LoadGif(string path, int width, int height)
    {
        var timer = Stopwatch.StartNew();
        using var gif = Image.FromFile(path);
        var dimension = new FrameDimension(gif.FrameDimensionsList[0]);
        int count = Math.Min(gif.GetFrameCount(dimension), MaxGifFrames);
        int[] delays = ReadDelays(gif, count);

        // Preenche a tela mantendo a proporção; o que sobra nas bordas é cortado.
        float scale = Math.Max((float)width / gif.Width, (float)height / gif.Height);
        var dest = new RectangleF((width - gif.Width * scale) / 2, (height - gif.Height * scale) / 2, gif.Width * scale, gif.Height * scale);

        using var builder = new Builder(width, height);
        using var frame = new Bitmap(width, height, PixelFormat.Format24bppRgb); // JPEG não tem alfa
        using var g = Graphics.FromImage(frame);
        g.InterpolationMode = InterpolationMode.HighQualityBilinear;
        g.PixelOffsetMode = PixelOffsetMode.Half;
        g.CompositingQuality = CompositingQuality.HighSpeed;
        for (int i = 0; i < count; i++)
        {
            gif.SelectActiveFrame(dimension, i); // o GDI+ já entrega o quadro composto sobre os anteriores
            g.Clear(Color.Black);                // fundo para áreas transparentes
            g.DrawImage(gif, dest);
            builder.Add(frame, delays[i]);
        }

        var animation = builder.Build();
        Log.Info($"GIF {Path.GetFileName(path)}: {gif.Width}x{gif.Height}, {count} quadros, {delays.Sum()} ms por volta, " +
            $"{animation.CacheBytes / 1048576.0:0.0} MB em cache, preparado em {timer.ElapsedMilliseconds} ms");
        return animation;
    }

    /// <summary>Desenha o quadro ocupando a área toda (use CompositingMode.SourceCopy: o quadro é opaco).</summary>
    public void DrawFrame(Bitmap canvas, Graphics g, int index)
    {
        using var stream = new MemoryStream(_frames[index], writable: false);
        using var bitmap = new Bitmap(stream);
        g.DrawImage(bitmap, new Rectangle(0, 0, Width, Height));
    }

    /// <summary>Duração por quadro (propriedade 0x5100, em centésimos de segundo). ≤ 1 vira 100 ms, como nos navegadores.</summary>
    private static int[] ReadDelays(Image gif, int count)
    {
        var delays = new int[count];
        byte[]? raw = gif.PropertyIdList.Contains(0x5100) ? gif.GetPropertyItem(0x5100)?.Value : null;
        for (int i = 0; i < count; i++)
        {
            int centiseconds = raw is not null && raw.Length >= (i + 1) * 4 ? BitConverter.ToInt32(raw, i * 4) : 10;
            delays[i] = centiseconds <= 1 ? 100 : centiseconds * 10;
        }
        return delays;
    }

    /// <summary>Monta uma animação quadro a quadro (cada quadro é codificado em JPEG na hora de adicionar).</summary>
    private sealed class Builder : IDisposable
    {
        private static readonly ImageCodecInfo Codec = ImageCodecInfo.GetImageEncoders().First(e => e.FormatID == ImageFormat.Jpeg.Guid);
        private readonly int _width;
        private readonly int _height;
        private readonly EncoderParameters _parameters = new(1);
        private readonly MemoryStream _stream = new(128 * 1024);
        private readonly List<byte[]> _frames = new();
        private readonly List<int> _delays = new();

        public Builder(int width, int height, long quality = 92)
        {
            _width = width;
            _height = height;
            _parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
        }

        public void Add(Bitmap frame, int delayMs)
        {
            if (frame.Width != _width || frame.Height != _height)
                throw new ArgumentException($"Quadro {frame.Width}x{frame.Height} ≠ {_width}x{_height}.");
            _stream.SetLength(0);
            frame.Save(_stream, Codec, _parameters);
            _frames.Add(_stream.ToArray());
            _delays.Add(Math.Max(20, delayMs));
        }

        public FrameAnimation Build() => new(_frames.ToArray(), _delays.ToArray(), _width, _height);

        public void Dispose()
        {
            _parameters.Dispose();
            _stream.Dispose();
        }
    }
}
