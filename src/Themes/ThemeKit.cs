using System.Drawing;
using System.Drawing.Drawing2D;

namespace DS339Monitor.Themes;

/// <summary>Atalhos compartilhados pelos temas desenhados em código.</summary>
internal static class ThemeKit
{
    public static Color Rgb(int rgb) => Color.FromArgb(unchecked((int)0xFF000000) | rgb);

    /// <summary>Texto fixo. Z=0 vai para o fundo pré-renderizado; use Z maior se precisar ficar por cima de algo dinâmico.</summary>
    public static void Literal(List<ThemeElement> e, FontResolver f, string text, RectangleF bounds, string family, float size,
        Color color, TextAlign align = TextAlign.Left, int z = 0) =>
        e.Add(new TextElement
        {
            Z = z,
            Bounds = bounds,
            Font = f.Create(family, size),
            Color = color,
            Data = text,
            IsLiteral = true,
            Align = align,
        });

    /// <summary>Valor de sensor, com unidade opcional em outra fonte/tamanho/cor (alinhada pela linha de base).</summary>
    public static void Text(List<ThemeElement> e, FontResolver f, string key, RectangleF bounds, string family, float size,
        Color color, TextAlign align = TextAlign.Left, string unit = "", float unitSize = 0, string? unitFamily = null,
        Color? unitColor = null, int z = 1) =>
        e.Add(new TextElement
        {
            Z = z,
            Bounds = bounds,
            Font = f.Create(family, size),
            Color = color,
            Data = key,
            Align = align,
            Unit = unit,
            UnitFont = unit.Length > 0 && unitSize > 0 ? f.Create(unitFamily ?? family, unitSize) : null,
            UnitColor = unitColor,
        });

    /// <summary>Mancha de cor com queda suave (sino) do centro para a borda.</summary>
    public static void Blob(Graphics g, PointF center, float radius, Color color)
    {
        var rect = new RectangleF(center.X - radius, center.Y - radius, radius * 2, radius * 2);
        using var path = new GraphicsPath();
        path.AddEllipse(rect);
        using var brush = new PathGradientBrush(path)
        {
            CenterColor = color,
            SurroundColors = new[] { Color.FromArgb(0, color) },
        };
        brush.SetSigmaBellShape(1f, 1f);
        g.FillEllipse(brush, rect);
    }

    /// <summary>Sombra difusa: o GDI+ não tem desfoque, então empilha contornos cada vez maiores e mais fracos.</summary>
    public static void SoftShadow(Graphics g, RectangleF card, float radius, int offsetY = 7)
    {
        using var brush = new SolidBrush(Color.FromArgb(4, 30, 41, 82));
        for (int i = 14; i >= 1; i--)
        {
            var layer = RectangleF.Inflate(new RectangleF(card.X, card.Y + offsetY, card.Width, card.Height), i, i);
            using var path = Shapes.RoundedRect(layer, radius + i);
            g.FillPath(brush, path);
        }
    }
}
