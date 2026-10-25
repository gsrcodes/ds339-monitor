using System.Drawing;
using System.Drawing.Drawing2D;

namespace DS339Monitor.Themes;

/// <summary>
/// Temas próprios: CPU e GPU lado a lado, relógio e RAM embaixo. Só usam Segoe UI.
/// Mesmo layout em duas paletas — escura ("Padrão") e clara ("Padrão Claro", para setups brancos).
/// </summary>
internal static class BuiltInTheme
{
    public const string Name = "Padrão";
    public const string LightName = "Padrão Claro";

    private sealed record Palette(
        Color GradientTop, Color GradientBottom,
        Color Card, Color CardEdge, Color Shadow,
        Color Ink, Color Soft, Color Muted, Color Track,
        Color Cpu, Color Gpu, Color Ram);

    private static readonly Palette Dark = new(
        GradientTop: Rgb(0x0E131D), GradientBottom: Rgb(0x080B12),
        Card: Rgb(0x141B29), CardEdge: Rgb(0x222B3B), Shadow: Color.Transparent,
        Ink: Rgb(0xF1F5F9), Soft: Rgb(0xCBD5E1), Muted: Rgb(0x8B95A7), Track: Rgb(0x263042),
        Cpu: Rgb(0x38BDF8), Gpu: Rgb(0xFB923C), Ram: Rgb(0xA78BFA));

    // Acentos mais escuros que os da versão escura: tons claros somem sobre branco.
    private static readonly Palette Light = new(
        GradientTop: Rgb(0xF8FAFC), GradientBottom: Rgb(0xE9EEF4),
        Card: Rgb(0xFFFFFF), CardEdge: Rgb(0xDCE3EC), Shadow: Color.FromArgb(18, 15, 23, 42),
        Ink: Rgb(0x0F172A), Soft: Rgb(0x334155), Muted: Rgb(0x64748B), Track: Rgb(0xE2E8F0),
        Cpu: Rgb(0x0284C7), Gpu: Rgb(0xEA580C), Ram: Rgb(0x7C3AED));

    private static readonly RectangleF CpuCard = new(16, 16, 456, 232);
    private static readonly RectangleF GpuCard = new(488, 16, 456, 232);
    private static readonly RectangleF BottomCard = new(16, 264, 928, 96);

    public static Theme Create(string name = Name)
    {
        bool light = name.Equals(LightName, StringComparison.OrdinalIgnoreCase);
        var p = light ? Light : Dark;
        var fonts = new FontResolver(null);
        var elements = new List<ThemeElement>();

        AddDevice(elements, fonts, p, CpuCard, p.Cpu, "CPU", "CPUName", "CPUTemp", "CpuUsage",
            ("CpuFrequency", " MHz"), ("CpuTEC", " W"), ("CpuVoltage", " V"));
        AddDevice(elements, fonts, p, GpuCard, p.Gpu, "GPU", "GPUName", "GPUTemp", "GpuUsage",
            ("GpuFrequency", " MHz"), ("GpuTEC", " W"), ("GPUMemoryUse", ""));

        // Faixa de baixo: relógio | data | RAM
        Text(elements, fonts, p, "CurrentTimeShut", new RectangleF(36, 265, 200, 96), "Segoe UI Semibold", 64, p.Ink);
        Text(elements, fonts, p, "WeekDays", new RectangleF(232, 282, 250, 30), "Segoe UI Semibold", 20, p.Ink);
        Text(elements, fonts, p, "CurrentDate", new RectangleF(232, 308, 250, 26), "Segoe UI", 17, p.Muted);

        Literal(elements, fonts, "RAM", new RectangleF(520, 280, 100, 28), "Segoe UI", 18, p.Ram, FontStyle.Bold);
        Text(elements, fonts, p, "MemoryUsage", new RectangleF(720, 268, 204, 44), "Segoe UI Semibold", 30, p.Ink, TextAlign.Right, "%", 18);
        Bar(elements, p, "MemoryUsage", new RectangleF(520, 312, 404, 10), p.Ram);
        Text(elements, fonts, p, "MemoryUse", new RectangleF(520, 328, 404, 24), "Segoe UI", 15, p.Muted);

        return new Theme
        {
            Name = light ? LightName : Name,
            BackgroundColor = p.GradientBottom,
            DrawStatic = g => DrawPanels(g, p),
            Elements = elements.OrderBy(e => e.Z).ToList(), // literais (Z=0) vão para o fundo pré-renderizado
        };
    }

    private static void AddDevice(List<ThemeElement> e, FontResolver f, Palette p, RectangleF card, Color accent, string label,
        string nameKey, string tempKey, string loadKey, params (string Key, string Unit)[] stats)
    {
        float x = card.X, y = card.Y;
        Literal(e, f, label, new RectangleF(x + 24, y + 14, 80, 34), "Segoe UI", 24, accent, FontStyle.Bold);
        Literal(e, f, "USO", new RectangleF(x + 232, y + 62, 200, 22), "Segoe UI Semibold", 14, p.Muted, align: TextAlign.Right);

        Text(e, f, p, nameKey, new RectangleF(x + 88, y + 24, 344, 24), "Segoe UI", 15, p.Muted);
        Text(e, f, p, tempKey, new RectangleF(x + 18, y + 44, 230, 120), "Segoe UI Semibold", 96, p.Ink, unit: "°C", unitSize: 34);
        Text(e, f, p, loadKey, new RectangleF(x + 232, y + 80, 200, 64), "Segoe UI Semibold", 50, p.Ink, TextAlign.Right, "%", 26);
        Bar(e, p, loadKey, new RectangleF(x + 24, y + 164, 408, 10), accent);

        var columns = new[] { (X: x + 24, Align: TextAlign.Left), (X: x + 160, Align: TextAlign.Center), (X: x + 296, Align: TextAlign.Right) };
        for (int i = 0; i < stats.Length && i < columns.Length; i++)
            Text(e, f, p, stats[i].Key, new RectangleF(columns[i].X, y + 186, 136, 30), "Segoe UI Semibold", 20, p.Soft, columns[i].Align, stats[i].Unit, 14);
    }

    private static void Literal(List<ThemeElement> e, FontResolver f, string text, RectangleF bounds, string family, float size, Color color,
        FontStyle style = FontStyle.Regular, TextAlign align = TextAlign.Left) =>
        e.Add(new TextElement
        {
            Z = 0,
            Bounds = bounds,
            Font = f.Create(family, size, style),
            Color = color,
            Data = text,
            IsLiteral = true,
            Align = align,
        });

    private static void Text(List<ThemeElement> e, FontResolver f, Palette p, string key, RectangleF bounds, string family, float size, Color color,
        TextAlign align = TextAlign.Left, string unit = "", float unitSize = 0) =>
        e.Add(new TextElement
        {
            Z = 1,
            Bounds = bounds,
            Font = f.Create(family, size),
            Color = color,
            Data = key,
            Align = align,
            Unit = unit,
            UnitFont = unit.Length > 0 && unitSize > 0 ? f.Create("Segoe UI", unitSize) : null,
            UnitColor = p.Muted,
        });

    private static void Bar(List<ThemeElement> e, Palette p, string key, RectangleF bounds, Color accent) =>
        e.Add(new BarElement
        {
            Z = 1,
            Bounds = bounds,
            Data = key,
            MaxValue = 100,
            CornerRadius = new[] { 5f, 5f, 5f, 5f },
            Fill = accent,
            Back = p.Track,
        });

    private static void DrawPanels(Graphics g, Palette p)
    {
        using (var background = new LinearGradientBrush(new Rectangle(0, 0, 960, 376), p.GradientTop, p.GradientBottom, LinearGradientMode.Vertical))
            g.FillRectangle(background, 0, 0, 960, 376);

        using var fill = new SolidBrush(p.Card);
        using var edge = new Pen(p.CardEdge, 1.5f);
        using var shadow = new SolidBrush(p.Shadow);
        foreach (var card in new[] { CpuCard, GpuCard, BottomCard })
        {
            if (p.Shadow.A > 0)
            {
                // Sombra curta e suave: dá profundidade aos cartões brancos sobre o fundo claro.
                using var shadowPath = Shapes.RoundedRect(RectangleF.Inflate(new RectangleF(card.X, card.Y + 3, card.Width, card.Height), 1, 1), 19);
                g.FillPath(shadow, shadowPath);
            }
            using var path = Shapes.RoundedRect(card, 18);
            g.FillPath(fill, path);
            g.DrawPath(edge, path);
        }
        g.DrawLine(edge, 496, 284, 496, 340); // divisória relógio | RAM
    }

    private static Color Rgb(int rgb) => Color.FromArgb(unchecked((int)0xFF000000) | rgb);
}
