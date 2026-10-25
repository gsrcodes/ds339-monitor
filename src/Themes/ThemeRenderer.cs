using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using DS339Monitor.Sensors;

namespace DS339Monitor.Themes;

/// <summary>
/// Desenha um tema num bitmap do tamanho dele. Fundo + elementos estáticos iniciais são compostos uma vez;
/// a cada ciclo só o resto é redesenhado — e só se algum valor mudou (ou se o quadro da animação mudou).
/// Com fundo animado, os elementos estáticos viram uma camada transparente pré-desenhada, aplicada sobre cada
/// quadro só na área que ela ocupa.
/// </summary>
internal sealed class ThemeRenderer : IDisposable
{
    private static readonly StringFormat Typographic = CreateFormat(StringAlignment.Near);
    private static readonly StringFormat[] Aligned =
    {
        CreateFormat(StringAlignment.Near),
        CreateFormat(StringAlignment.Center),
        CreateFormat(StringAlignment.Far),
    };

    private readonly Bitmap? _base;          // fundo + estáticos (tema sem animação)
    private readonly IAnimation? _animation;
    private readonly Bitmap? _overlay;       // estáticos sobre o GIF (transparente)
    private readonly Rectangle _overlayBounds;
    private int _frame;
    private long _renderedKey = -1;
    private readonly Bitmap _canvas;
    private readonly Graphics _graphics;
    private readonly ThemeElement[] _dynamic;
    private readonly string?[] _texts;
    private readonly double[] _values;
    private readonly double[] _colorValues; // segundo valor do líquido (o que escolhe a cor)
    private readonly Pen?[] _arcPens;
    private readonly History[]?[] _histories;
    private readonly List<IDisposable> _resources = new();
    private readonly Dictionary<int, SolidBrush> _brushes = new();

    public ThemeRenderer(Theme theme)
    {
        _animation = theme.Animation;
        var prebaked = new Bitmap(theme.Width, theme.Height, PixelFormat.Format32bppPArgb);
        int firstDynamic = 0;
        using (var g = Graphics.FromImage(prebaked))
        {
            Configure(g);
            g.Clear(_animation is null ? theme.BackgroundColor : Color.Transparent);
            if (_animation is null && theme.Background is not null) DrawImageExact(g, theme.Background, 0, 0);
            theme.DrawStatic?.Invoke(g);
            while (firstDynamic < theme.Elements.Count && theme.Elements[firstDynamic].IsStatic)
                Draw(g, theme.Elements[firstDynamic++], null, 0);
        }

        if (_animation is null)
        {
            _base = prebaked;
        }
        else
        {
            _overlayBounds = OpaqueBounds(prebaked);
            if (_overlayBounds.IsEmpty) prebaked.Dispose();
            else _overlay = prebaked;
        }

        _dynamic = theme.Elements.Skip(firstDynamic).ToArray();
        _texts = new string?[_dynamic.Length];
        _values = new double[_dynamic.Length];
        Array.Fill(_values, double.NaN);
        _colorValues = new double[_dynamic.Length];
        _arcPens = new Pen?[_dynamic.Length];
        _histories = new History[]?[_dynamic.Length];
        for (int i = 0; i < _dynamic.Length; i++)
        {
            if (_dynamic[i] is ArcElement arc) _arcPens[i] = CreateArcPen(arc);
            if (_dynamic[i] is ChartElement chart)
                _histories[i] = chart.Series.Select(_ => new History(Math.Max(2, chart.Capacity))).ToArray();
        }

        _canvas = new Bitmap(theme.Width, theme.Height, PixelFormat.Format32bppPArgb);
        _graphics = Graphics.FromImage(_canvas);
        Configure(_graphics);
    }

    public Bitmap Canvas => _canvas;

    public bool IsAnimated => _animation is not null;

    /// <summary>
    /// Só a animação, sem nada por cima, e com poucos quadros (GIF): cada quadro sai sempre igual e pode ir para um
    /// cache já codificado.
    /// </summary>
    public bool CanCacheFrames => _animation is { CacheFrames: true } && _overlay is null && _dynamic.Length == 0;

    public int FrameIndex => _frame;

    public int FrameCount => _animation?.Count ?? 1;

    /// <summary>Há valores de sensor por cima (precisam ser redesenhados quando mudam).</summary>
    public bool HasDynamicContent => _dynamic.Length > 0;

    public int FrameDelayMs => _animation?.DelayMs(_frame) ?? 1000;

    /// <summary>Pula para o quadro <paramref name="frame"/> da animação (o próximo AdvanceFrame segue dele).</summary>
    public void Seek(int frame)
    {
        if (_animation is not null) _frame = Math.Clamp(frame, 0, _animation.Count - 1);
    }

    public void AdvanceFrame()
    {
        if (_animation is not null) _frame = (_frame + 1) % _animation.Count;
    }

    /// <summary>
    /// Atualiza o <see cref="Canvas"/>. Devolve false (sem desenhar nada) se nada mudou.
    /// <paramref name="refreshValues"/> = false redesenha com os valores já lidos (quadros do GIF entre leituras).
    /// </summary>
    public bool Render(SensorHub? sensors, bool force, bool refreshValues = true)
    {
        bool changed = force || (_animation is not null && _animation.FrameKey(_frame) != _renderedKey);
        for (int i = 0; refreshValues && sensors is not null && i < _dynamic.Length; i++)
        {
            switch (_dynamic[i])
            {
                case TextElement { IsLiteral: false } text:
                    string value = sensors.Text(text.Data);
                    if (!string.Equals(value, _texts[i], StringComparison.Ordinal))
                    {
                        _texts[i] = value;
                        changed = true;
                    }
                    break;

                case BarElement bar:
                    double number = Math.Round(sensors.Number(bar.Data) ?? 0, 1);
                    if (!number.Equals(_values[i]))
                    {
                        _values[i] = number;
                        changed = true;
                    }
                    break;

                case ArcElement arc:
                    double angle = Math.Round(sensors.Number(arc.Data) ?? double.NaN, 1); // NaN = sem dado: só o trilho
                    if (!angle.Equals(_values[i]))
                    {
                        _values[i] = angle;
                        changed = true;
                    }
                    break;

                case ChartElement chart:
                    var series = _histories[i]!;
                    for (int s = 0; s < series.Length; s++)
                        series[s].Push((float)(sensors.Number(chart.Series[s]) ?? double.NaN));
                    changed = true; // o gráfico anda a cada ciclo
                    break;

                case LiquidElement liquid:
                    _values[i] = sensors.Number(liquid.LevelKey) ?? double.NaN;
                    _colorValues[i] = liquid.ColorKey is null ? double.NaN : sensors.Number(liquid.ColorKey) ?? double.NaN;
                    changed = true; // a onda anda a cada ciclo
                    break;
            }
        }
        if (!changed) return false;

        var g = _graphics;
        g.CompositingMode = CompositingMode.SourceCopy;
        if (_animation is not null)
        {
            _animation.DrawFrame(_canvas, g, _frame);
            _renderedKey = _animation.FrameKey(_frame);
            g.CompositingMode = CompositingMode.SourceOver;
            if (_overlay is not null)
            {
                var previous = g.PixelOffsetMode;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.DrawImage(_overlay, _overlayBounds, _overlayBounds, GraphicsUnit.Pixel);
                g.PixelOffsetMode = previous;
            }
        }
        else
        {
            DrawImageExact(g, _base!, 0, 0);
            g.CompositingMode = CompositingMode.SourceOver;
        }
        for (int i = 0; i < _dynamic.Length; i++)
        {
            switch (_dynamic[i])
            {
                case ArcElement arc:
                    DrawArc(g, arc, _values[i], _arcPens[i]!);
                    break;
                case ChartElement chart:
                    DrawChart(g, chart, _histories[i]!);
                    break;
                case LiquidElement liquid:
                    DrawLiquid(g, liquid, _values[i], _colorValues[i]);
                    break;
                default:
                    Draw(g, _dynamic[i], _texts[i], _values[i]);
                    break;
            }
        }
        g.Flush(FlushIntention.Sync);
        return true;
    }

    public void Dispose()
    {
        _graphics.Dispose();
        _canvas.Dispose();
        _base?.Dispose();
        _overlay?.Dispose();
        foreach (var pen in _arcPens) pen?.Dispose();
        foreach (var resource in _resources) resource.Dispose();
        foreach (var brush in _brushes.Values) brush.Dispose();
    }

    private void DrawArc(Graphics g, ArcElement a, double value, Pen pen)
    {
        var rect = new RectangleF(a.Center.X - a.Radius, a.Center.Y - a.Radius, a.Radius * 2, a.Radius * 2);
        if (a.Track.A > 0)
        {
            using var track = new Pen(a.Track, a.Thickness) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            g.DrawArc(track, rect, a.StartAngle, a.SweepAngle);
        }
        if (!double.IsFinite(value)) return;

        float fraction = (float)Math.Clamp((value - a.Min) / (a.Max - a.Min), 0, 1);
        float sweep = Math.Max(a.SweepAngle * fraction, 0.5f); // no mínimo vira um ponto (as pontas são redondas)
        g.DrawArc(pen, rect, a.StartAngle, sweep);

        if (a.Knob)
        {
            double end = (a.StartAngle + sweep) * Math.PI / 180;
            float r = a.Thickness * 0.2f;
            float x = a.Center.X + a.Radius * (float)Math.Cos(end);
            float y = a.Center.Y + a.Radius * (float)Math.Sin(end);
            g.FillEllipse(Brush(Color.White), x - r, y - r, r * 2, r * 2);
        }
    }

    private void DrawChart(Graphics g, ChartElement c, History[] series)
    {
        var b = c.Bounds;
        if (c.Grid.A > 0)
        {
            using var grid = new Pen(c.Grid, 1) { DashPattern = new[] { 3f, 4f } };
            for (int k = 0; k <= 2; k++)
            {
                float y = b.Top + b.Height * k / 2f;
                g.DrawLine(grid, b.Left, y, b.Right, y);
            }
        }

        float step = b.Width / (c.Capacity - 1);
        var state = g.Save();
        g.SetClip(RectangleF.Inflate(b, 4, 4)); // a curva suavizada pode passar um pouco do limite
        for (int s = 0; s < series.Length; s++)
        {
            var history = series[s];
            if (history.Count < 2) continue;

            var points = new PointF[history.Count];
            for (int i = 0; i < points.Length; i++)
            {
                float v = history[i];
                float level = float.IsFinite(v) ? Math.Clamp(v / c.Max, 0, 1) : 0;
                points[i] = new PointF(b.Right - (points.Length - 1 - i) * step, b.Bottom - level * b.Height);
            }

            var color = c.Colors[s % c.Colors.Length];
            using (var area = new GraphicsPath())
            {
                area.AddLine(points[0].X, b.Bottom, points[0].X, points[0].Y);
                area.AddCurve(points, 0.4f);
                area.AddLine(points[^1].X, points[^1].Y, points[^1].X, b.Bottom);
                area.CloseFigure();
                using var fill = new LinearGradientBrush(RectangleF.Inflate(b, 0, 1), Color.FromArgb(64, color), Color.FromArgb(0, color), LinearGradientMode.Vertical);
                g.FillPath(fill, area);
            }
            using (var line = new Pen(color, 2.2f) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawCurve(line, points, 0.4f);

            // Ponto no valor mais recente.
            var last = points[^1];
            using var ring = new Pen(color, 2.2f);
            g.FillEllipse(Brush(Color.White), last.X - 4, last.Y - 4, 8, 8);
            g.DrawEllipse(ring, last.X - 4, last.Y - 4, 8, 8);
        }
        g.Restore(state);
    }

    private void DrawLiquid(Graphics g, LiquidElement l, double level, double colorValue)
    {
        var b = l.Bounds;
        float fraction = double.IsFinite(level) ? (float)Math.Clamp(level / l.LevelMax, l.MinLevel, 1) : l.MinLevel;
        float surface = b.Bottom - fraction * b.Height;
        Color front = HeatColor(l.ColorStops, colorValue);
        Color back = Mix(front, Color.White, 0.5f);

        // A onda anda devagar: ~0,5 rad por segundo, independente do intervalo de atualização.
        double phase = Environment.TickCount64 / 2000.0;

        using var shape = Shapes.RoundedRect(b, l.CornerRadius);
        var state = g.Save();
        g.SetClip(shape);
        FillWave(g, b, surface - l.WaveHeight * 0.7f, l.WaveHeight * 0.8f, l.WaveLength * 1.15f, phase + 2.2, back);
        var crest = FillWave(g, b, surface, l.WaveHeight, l.WaveLength, phase, front);
        using (var gloss = new Pen(Color.FromArgb(150, Color.White), 2f) { LineJoin = LineJoin.Round })
        {
            for (int i = 0; i < crest.Length; i++) crest[i].Y += 2.5f; // brilho logo abaixo da crista
            g.DrawLines(gloss, crest);
        }
        g.Restore(state);

        if (l.Border.A > 0)
        {
            using var edge = new Pen(l.Border, 1.5f);
            g.DrawPath(edge, shape);
        }
    }

    /// <summary>Preenche abaixo de uma senoide e devolve os pontos da crista.</summary>
    private static PointF[] FillWave(Graphics g, RectangleF b, float surface, float amplitude, float wavelength, double phase, Color color)
    {
        const float Step = 8;
        int count = (int)Math.Ceiling(b.Width / Step) + 1;
        var crest = new PointF[count];
        for (int i = 0; i < count; i++)
        {
            float x = Math.Min(b.Left + i * Step, b.Right);
            crest[i] = new PointF(x, surface + amplitude * (float)Math.Sin(2 * Math.PI * (x - b.Left) / wavelength + phase));
        }

        var polygon = new PointF[count + 2];
        crest.CopyTo(polygon, 0);
        polygon[count] = new PointF(b.Right, b.Bottom + 1);
        polygon[count + 1] = new PointF(b.Left, b.Bottom + 1);
        using var brush = new SolidBrush(color);
        g.FillPolygon(brush, polygon);
        return crest;
    }

    /// <summary>
    /// Cor para um valor entre paradas (mistura em RGB). Use paradas próximas: entre cores distantes, como
    /// verde e âmbar, a mistura em RGB passa por um oliva sujo.
    /// </summary>
    private static Color HeatColor((float Value, Color Color)[] stops, double value)
    {
        if (!double.IsFinite(value) || value <= stops[0].Value) return stops[0].Color;
        for (int i = 1; i < stops.Length; i++)
        {
            if (value > stops[i].Value) continue;
            float t = (float)((value - stops[i - 1].Value) / (stops[i].Value - stops[i - 1].Value));
            return Mix(stops[i - 1].Color, stops[i].Color, t);
        }
        return stops[^1].Color;
    }

    private static Color Mix(Color a, Color b, float t) => Color.FromArgb(
        (int)Math.Round(a.R + (b.R - a.R) * t),
        (int)Math.Round(a.G + (b.G - a.G) * t),
        (int)Math.Round(a.B + (b.B - a.B) * t));

    /// <summary>
    /// Caneta do anel. Para degradê, gera uma textura "cônica" (cor por ângulo) uma única vez; desenhar o arco com
    /// essa textura dá o degradê ao longo do anel com as bordas e pontas suavizadas do próprio GDI+.
    /// </summary>
    private Pen CreateArcPen(ArcElement a)
    {
        if (a.Colors.Length == 1)
            return new Pen(a.Colors[0], a.Thickness) { StartCap = LineCap.Round, EndCap = LineCap.Round };

        int half = (int)Math.Ceiling(a.Radius + a.Thickness) + 2;
        int size = half * 2;
        var texture = new Bitmap(size, size, PixelFormat.Format32bppPArgb);
        var data = texture.LockBits(new Rectangle(0, 0, size, size), ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
        try
        {
            unsafe
            {
                uint* pixels = (uint*)data.Scan0;
                int stride = data.Stride / 4;
                float gap = 360 - a.SweepAngle;
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        double angle = Math.Atan2(y + 0.5 - half, x + 0.5 - half) * 180 / Math.PI;
                        double relative = ((angle - a.StartAngle) % 360 + 360) % 360;
                        // Na abertura, usa a cor da ponta mais próxima (as pontas redondas invadem um pouco essa área).
                        double t = relative <= a.SweepAngle ? relative / a.SweepAngle : relative - a.SweepAngle < gap / 2 ? 1 : 0;
                        pixels[y * stride + x] = (uint)Lerp(a.Colors, t).ToArgb();
                    }
                }
            }
        }
        finally
        {
            texture.UnlockBits(data);
        }

        var brush = new TextureBrush(texture, WrapMode.Clamp);
        brush.TranslateTransform(a.Center.X - half, a.Center.Y - half);
        _resources.Add(texture);
        _resources.Add(brush);
        return new Pen(brush, a.Thickness) { StartCap = LineCap.Round, EndCap = LineCap.Round };
    }

    private static Color Lerp(Color[] stops, double t)
    {
        if (stops.Length == 1) return stops[0];
        double position = Math.Clamp(t, 0, 1) * (stops.Length - 1);
        int index = Math.Min((int)position, stops.Length - 2);
        double f = position - index;
        Color a = stops[index], b = stops[index + 1];
        return Color.FromArgb(
            (int)Math.Round(a.R + (b.R - a.R) * f),
            (int)Math.Round(a.G + (b.G - a.G) * f),
            (int)Math.Round(a.B + (b.B - a.B) * f));
    }

    /// <summary>Fila circular de amostras; índice 0 = mais antiga.</summary>
    private sealed class History
    {
        private readonly float[] _values;
        private int _next;

        public History(int capacity) => _values = new float[capacity];

        public int Count { get; private set; }

        public float this[int index] => _values[(_next - Count + index + _values.Length * 2) % _values.Length];

        public void Push(float value)
        {
            _values[_next] = value;
            _next = (_next + 1) % _values.Length;
            if (Count < _values.Length) Count++;
        }
    }

    private void Draw(Graphics g, ThemeElement element, string? text, double value)
    {
        switch (element)
        {
            case ImageElement image:
                DrawImageExact(g, image.Image, image.Location.X, image.Location.Y);
                break;
            case TextElement t:
                DrawText(g, t, t.IsLiteral ? t.Data : text ?? SensorKeys.Missing);
                break;
            case BarElement bar:
                DrawBar(g, bar, value);
                break;
        }
    }

    private void DrawText(Graphics g, TextElement t, string value)
    {
        // O JONSBO-AIO monta "Título Valor Unidade" separados por espaço.
        string main = t.Title.Length > 0 ? t.Title + " " + value : value;
        bool showUnit = t.Unit.Length > 0 && value != SensorKeys.Missing; // "--", não "-- °C"
        var brush = Brush(t.Color);

        if (!showUnit || t.UnitFont is null)
        {
            string full = showUnit ? main + " " + t.Unit : main;
            g.DrawString(full, t.Font, brush, t.Bounds, Aligned[(int)t.Align]);
            return;
        }

        // Unidade menor, apoiada na mesma linha de base do valor.
        float mainWidth = g.MeasureString(main, t.Font, PointF.Empty, Typographic).Width;
        float unitWidth = g.MeasureString(t.Unit, t.UnitFont, PointF.Empty, Typographic).Width;
        float total = mainWidth + unitWidth;
        float x = t.Align switch
        {
            TextAlign.Center => t.Bounds.X + (t.Bounds.Width - total) / 2,
            TextAlign.Right => t.Bounds.Right - total,
            _ => t.Bounds.X,
        };
        g.DrawString(main, t.Font, brush, x, t.Bounds.Y, Typographic);
        float unitY = t.Bounds.Y + Ascent(t.Font) - Ascent(t.UnitFont);
        g.DrawString(t.Unit, t.UnitFont, Brush(t.UnitColor ?? t.Color), x + mainWidth, unitY, Typographic);
    }

    private void DrawBar(Graphics g, BarElement b, double value)
    {
        var bounds = b.Bounds;
        if (b.Back.A > 0)
        {
            using var back = Shapes.RoundedRect(bounds, b.CornerRadius);
            g.FillPath(Brush(b.Back), back);
        }

        double fraction = double.IsFinite(value) ? Math.Clamp(value / b.MaxValue, 0, 1) : 0;
        var inner = RectangleF.Inflate(bounds, -b.BorderThickness, -b.BorderThickness);
        if (fraction > 0 && inner.Width > 0 && inner.Height > 0)
        {
            float f = (float)fraction;
            var fill = b.Vertical
                ? new RectangleF(inner.X, inner.Bottom - inner.Height * f, inner.Width, inner.Height * f) // enche de baixo pra cima
                : new RectangleF(inner.X, inner.Y, inner.Width * f, inner.Height);
            using var path = Shapes.RoundedRect(fill, Shrink(b.CornerRadius, b.BorderThickness));
            if (b.Fill2 is Color end)
            {
                // Degradê calculado sobre a barra inteira: o valor só "revela" até onde vai.
                using var gradient = new LinearGradientBrush(RectangleF.Inflate(inner, 1, 1), b.Fill, end,
                    b.Vertical ? LinearGradientMode.Vertical : LinearGradientMode.Horizontal);
                g.FillPath(gradient, path);
            }
            else
            {
                g.FillPath(Brush(b.Fill), path);
            }
        }

        if (b.BorderThickness > 0 && b.Border.A > 0)
        {
            float half = b.BorderThickness / 2;
            using var path = Shapes.RoundedRect(RectangleF.Inflate(bounds, -half, -half), Shrink(b.CornerRadius, half));
            using var pen = new Pen(b.Border, b.BorderThickness);
            g.DrawPath(pen, path);
        }
    }

    private SolidBrush Brush(Color color)
    {
        int key = color.ToArgb();
        if (!_brushes.TryGetValue(key, out var brush))
        {
            brush = new SolidBrush(color);
            _brushes[key] = brush;
        }
        return brush;
    }

    private static float[] Shrink(float[] radii, float by) =>
        new[] { Math.Max(0, radii[0] - by), Math.Max(0, radii[1] - by), Math.Max(0, radii[2] - by), Math.Max(0, radii[3] - by) };

    private static float Ascent(Font font)
    {
        var family = font.FontFamily;
        return font.Size * family.GetCellAscent(font.Style) / family.GetEmHeight(font.Style);
    }

    /// <summary>Menor retângulo com algum pixel não transparente (vazio se a imagem toda for transparente).</summary>
    private static unsafe Rectangle OpaqueBounds(Bitmap bitmap)
    {
        var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);
        try
        {
            int left = int.MaxValue, top = int.MaxValue, right = -1, bottom = -1;
            for (int y = 0; y < data.Height; y++)
            {
                uint* row = (uint*)((byte*)data.Scan0 + y * data.Stride);
                for (int x = 0; x < data.Width; x++)
                {
                    if (row[x] >> 24 == 0) continue;
                    if (x < left) left = x;
                    if (x > right) right = x;
                    if (y < top) top = y;
                    bottom = y;
                }
            }
            return right < 0 ? Rectangle.Empty : Rectangle.FromLTRB(left, top, right + 1, bottom + 1);
        }
        finally
        {
            bitmap.UnlockBits(data);
        }
    }

    /// <summary>Copia 1:1, sem reamostrar nem deslocar meio pixel.</summary>
    private static void DrawImageExact(Graphics g, Image image, int x, int y)
    {
        var previous = g.PixelOffsetMode;
        g.PixelOffsetMode = PixelOffsetMode.Half;
        g.DrawImage(image, new Rectangle(x, y, image.Width, image.Height));
        g.PixelOffsetMode = previous;
    }

    private static void Configure(Graphics g)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit; // ClearType não serve: o painel é girado
        g.InterpolationMode = InterpolationMode.NearestNeighbor;  // tudo já está no tamanho final
        g.CompositingQuality = CompositingQuality.HighSpeed;
    }

    private static StringFormat CreateFormat(StringAlignment alignment) =>
        new(StringFormat.GenericTypographic)
        {
            Alignment = alignment,
            LineAlignment = StringAlignment.Near,
            FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.NoClip,
            Trimming = StringTrimming.None,
        };
}
