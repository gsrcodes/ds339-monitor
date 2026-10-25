using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace DS339Monitor.Themes;

internal enum TextAlign
{
    Left,
    Center,
    Right,
}

internal abstract class ThemeElement
{
    public int Z { get; init; }

    /// <summary>Elementos estáticos no início da pilha são pré-renderizados no fundo uma única vez.</summary>
    public abstract bool IsStatic { get; }
}

internal sealed class ImageElement : ThemeElement
{
    public required Bitmap Image { get; init; } // já redimensionada para Bounds, em PArgb
    public required Point Location { get; init; }
    public override bool IsStatic => true;
}

internal sealed class TextElement : ThemeElement
{
    public required RectangleF Bounds { get; init; }
    public required Font Font { get; init; }
    public required Color Color { get; init; }
    /// <summary>Chave do sensor — ou o próprio texto, quando <see cref="IsLiteral"/>.</summary>
    public required string Data { get; init; }
    public bool IsLiteral { get; init; }
    public string Title { get; init; } = "";
    public string Unit { get; init; } = "";
    /// <summary>Se definida, a unidade é desenhada menor, alinhada pela linha de base do valor.</summary>
    public Font? UnitFont { get; init; }
    public Color? UnitColor { get; init; }
    public TextAlign Align { get; init; }
    public override bool IsStatic => IsLiteral;
}

internal sealed class BarElement : ThemeElement
{
    public required RectangleF Bounds { get; init; }
    public required string Data { get; init; }
    public float MaxValue { get; init; } = 100;
    public float[] CornerRadius { get; init; } = new float[4]; // sup-esq, sup-dir, inf-dir, inf-esq
    public Color Fill { get; init; }
    /// <summary>Se definida, o preenchimento é um degradê de <see cref="Fill"/> até esta cor ao longo da barra inteira.</summary>
    public Color? Fill2 { get; init; }
    public Color Back { get; init; }
    public Color Border { get; init; }
    public float BorderThickness { get; init; }
    public bool Vertical => Bounds.Height > Bounds.Width;
    public override bool IsStatic => false;
}

/// <summary>
/// Anel de progresso. Ângulos em graus, 0 = 3 horas, sentido horário; o padrão é 270° com a abertura embaixo.
/// Com várias cores, o degradê acompanha o ângulo (a cor de cada ponto do anel é fixa; o valor só "revela" o anel).
/// </summary>
internal sealed class ArcElement : ThemeElement
{
    public required PointF Center { get; init; }
    /// <summary>Raio da linha central do traço.</summary>
    public required float Radius { get; init; }
    public required float Thickness { get; init; }
    public required string Data { get; init; }
    public float Min { get; init; }
    public float Max { get; init; } = 100;
    public float StartAngle { get; init; } = 135;
    public float SweepAngle { get; init; } = 270;
    public Color Track { get; init; }
    public required Color[] Colors { get; init; }
    /// <summary>Bolinha branca na ponta do arco.</summary>
    public bool Knob { get; init; }
    public override bool IsStatic => false;
}

/// <summary>Gráfico de linhas com o histórico recente (uma amostra por ciclo) de uma ou mais chaves.</summary>
internal sealed class ChartElement : ThemeElement
{
    public required RectangleF Bounds { get; init; }
    public required string[] Series { get; init; }
    public required Color[] Colors { get; init; }
    public int Capacity { get; init; } = 60;
    public float Max { get; init; } = 100;
    public Color Grid { get; init; } = Color.Transparent;
    public override bool IsStatic => false;
}

/// <summary>
/// Reservatório: o cartão "enche" de baixo para cima conforme <see cref="LevelKey"/>, com a superfície em onda,
/// e a cor do líquido vem de <see cref="ColorKey"/> (ex.: nível = uso, cor = temperatura).
/// </summary>
internal sealed class LiquidElement : ThemeElement
{
    public required RectangleF Bounds { get; init; }
    public float CornerRadius { get; init; }
    public required string LevelKey { get; init; }
    public float LevelMax { get; init; } = 100;
    /// <summary>Fração mínima visível, para nunca ficar totalmente vazio.</summary>
    public float MinLevel { get; init; } = 0.06f;
    public string? ColorKey { get; init; }
    /// <summary>Pares (valor, cor) em ordem crescente; entre eles a cor é interpolada.</summary>
    public required (float Value, Color Color)[] ColorStops { get; init; }
    public float WaveHeight { get; init; } = 7;
    public float WaveLength { get; init; } = 170;
    /// <summary>Contorno redesenhado por cima (esconde o serrilhado do recorte nos cantos).</summary>
    public Color Border { get; init; } = Color.Transparent;
    public override bool IsStatic => false;
}

internal sealed class Theme : IDisposable
{
    public required string Name { get; init; }
    public int Width { get; init; } = 960;
    public int Height { get; init; } = 376;
    public Color BackgroundColor { get; init; } = Color.Black;
    public Bitmap? Background { get; init; }
    /// <summary>Fundo animado (GIF, apresentação de painéis). Quando existe, substitui <see cref="Background"/>.</summary>
    public IAnimation? Animation { get; init; }
    /// <summary>Desenho fixo extra (temas embutidos), feito por cima do fundo.</summary>
    public Action<Graphics>? DrawStatic { get; init; }
    /// <summary>Ordenados por Z (o maior fica por cima).</summary>
    public required List<ThemeElement> Elements { get; init; }
    public PrivateFontCollection? Fonts { get; init; }

    public IEnumerable<string> SensorKeys => Elements
        .SelectMany(e => e switch
        {
            TextElement { IsLiteral: false } t => new[] { t.Data },
            BarElement b => new[] { b.Data },
            ArcElement a => new[] { a.Data },
            ChartElement c => c.Series,
            LiquidElement l => l.ColorKey is null ? new[] { l.LevelKey } : new[] { l.LevelKey, l.ColorKey },
            _ => Array.Empty<string>(),
        })
        .Distinct(StringComparer.OrdinalIgnoreCase);

    public void Dispose()
    {
        foreach (var element in Elements)
        {
            switch (element)
            {
                case ImageElement image:
                    image.Image.Dispose();
                    break;
                case TextElement text:
                    text.Font.Dispose();
                    text.UnitFont?.Dispose();
                    break;
            }
        }
        Background?.Dispose();
        Animation?.Dispose();
        Fonts?.Dispose();
    }
}

/// <summary>Encontra fontes pelo nome: primeiro as do tema (pasta font/), depois as instaladas.</summary>
internal sealed class FontResolver
{
    private const string Fallback = "Segoe UI";
    private const int ChineseSimplified = 0x0804;
    private const int ChineseTraditional = 0x0404;

    private readonly FontFamily[] _private;
    private readonly HashSet<string> _warned = new(StringComparer.OrdinalIgnoreCase);

    public FontResolver(PrivateFontCollection? fonts)
    {
        _private = fonts?.Families ?? Array.Empty<FontFamily>();
    }

    public Font Create(string family, float sizePx, FontStyle style = FontStyle.Regular)
    {
        string name = family.Trim().TrimStart('#').Trim();
        var privateFamily = FindPrivate(name);
        if (privateFamily is not null)
            return new Font(privateFamily, sizePx, PickStyle(privateFamily, style), GraphicsUnit.Pixel);

        bool found = IsInstalled(name);
        string installed = found ? name : Fallback;
        if (!found && name.Length > 0 && _warned.Add(name))
            Log.Warn($"Fonte '{name}' não encontrada; usando {Fallback}.");

        using var probe = new FontFamily(installed);
        return new Font(installed, sizePx, PickStyle(probe, style), GraphicsUnit.Pixel);
    }

    private FontFamily? FindPrivate(string name)
    {
        foreach (var family in _private)
        {
            if (family.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
                || family.GetName(ChineseSimplified).Equals(name, StringComparison.OrdinalIgnoreCase)
                || family.GetName(ChineseTraditional).Equals(name, StringComparison.OrdinalIgnoreCase))
                return family;
        }
        // "HarmonyOS Sans SC Black" pedido, mas o arquivo registra "HarmonyOS Sans SC" (ou vice-versa).
        foreach (var family in _private)
        {
            if (name.StartsWith(family.Name, StringComparison.OrdinalIgnoreCase)
                || family.Name.StartsWith(name, StringComparison.OrdinalIgnoreCase))
                return family;
        }
        // Tema com uma única fonte própria: quase certamente é ela.
        return _private.Length == 1 ? _private[0] : null;
    }

    private static bool IsInstalled(string name)
    {
        if (name.Length == 0) return false;
        try
        {
            using var family = new FontFamily(name);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private static FontStyle PickStyle(FontFamily family, FontStyle preferred)
    {
        foreach (var style in new[] { preferred, FontStyle.Regular, FontStyle.Bold, FontStyle.Italic, FontStyle.Bold | FontStyle.Italic })
        {
            if (family.IsStyleAvailable(style)) return style;
        }
        return preferred;
    }
}

internal static class Shapes
{
    /// <summary>Retângulo com cantos arredondados independentes (sup-esq, sup-dir, inf-dir, inf-esq).</summary>
    public static GraphicsPath RoundedRect(RectangleF r, float[] radii)
    {
        var path = new GraphicsPath();
        float max = Math.Min(r.Width, r.Height) / 2;
        float tl = Math.Min(radii[0], max), tr = Math.Min(radii[1], max), br = Math.Min(radii[2], max), bl = Math.Min(radii[3], max);

        if (tl > 0) path.AddArc(r.Left, r.Top, tl * 2, tl * 2, 180, 90); else path.AddLine(r.Left, r.Top, r.Left, r.Top);
        if (tr > 0) path.AddArc(r.Right - tr * 2, r.Top, tr * 2, tr * 2, 270, 90); else path.AddLine(r.Right, r.Top, r.Right, r.Top);
        if (br > 0) path.AddArc(r.Right - br * 2, r.Bottom - br * 2, br * 2, br * 2, 0, 90); else path.AddLine(r.Right, r.Bottom, r.Right, r.Bottom);
        if (bl > 0) path.AddArc(r.Left, r.Bottom - bl * 2, bl * 2, bl * 2, 90, 90); else path.AddLine(r.Left, r.Bottom, r.Left, r.Bottom);
        path.CloseFigure();
        return path;
    }

    public static GraphicsPath RoundedRect(RectangleF r, float radius) => RoundedRect(r, new[] { radius, radius, radius, radius });
}
