using System.Drawing;
using System.Drawing.Drawing2D;
using static DS339Monitor.Themes.ThemeKit;

namespace DS339Monitor.Themes;

/// <summary>
/// "Aurora": tema claro para setups brancos. Fundo com manchas de cor pastel, cartões de vidro com sombra difusa,
/// anéis de temperatura (degradê) e de uso, gráfico de atividade dos últimos 60 s e relógio em fonte fina.
/// Todo o enfeite (fundo, sombras, cartões, rótulos) é desenhado uma vez só; por frame vão só os valores.
/// </summary>
internal static class AuroraTheme
{
    public const string Name = "Aurora";

    // Segoe UI Variable é do Windows 11; no Windows 10 o FontResolver cai em Segoe UI.
    // Os nomes truncados ("Semil", "Semib") são os que o GDI registra.
    private const string Display = "Segoe UI Variable Display Semil";
    private const string DisplayLight = "Segoe UI Variable Display Light";
    private const string DisplaySemibold = "Segoe UI Variable Display Semib";
    private const string Body = "Segoe UI Variable Text";
    private const string BodySemibold = "Segoe UI Variable Text Semibold";

    private static readonly Color Ink = Rgb(0x0F172A);
    private static readonly Color Soft = Rgb(0x334155);
    private static readonly Color Muted = Rgb(0x64748B);
    private static readonly Color Faint = Rgb(0x94A3B8);
    private static readonly Color Track = Rgb(0xE8ECF4);

    private static readonly Color[] CpuGradient = { Rgb(0x38BDF8), Rgb(0x6366F1) }; // céu → índigo
    private static readonly Color[] GpuGradient = { Rgb(0xFBBF24), Rgb(0xF43F5E) }; // âmbar → rosa
    private static readonly Color CpuLabel = Rgb(0x4F46E5);
    private static readonly Color GpuLabel = Rgb(0xE11D48);
    private static readonly Color CpuUsage = Rgb(0xA5B4FC);
    private static readonly Color GpuUsage = Rgb(0xFDA4AF);
    private static readonly Color RamFrom = Rgb(0xA78BFA);
    private static readonly Color RamTo = Rgb(0xEC4899);

    private static readonly RectangleF CpuCard = new(16, 16, 296, 344);
    private static readonly RectangleF GpuCard = new(324, 16, 296, 344);
    private static readonly RectangleF SideCard = new(632, 16, 312, 344);

    // Conteúdo do cartão lateral (margem interna de 24 px).
    private static readonly float SideX = SideCard.X + 24;
    private static readonly float SideW = SideCard.Width - 48;
    private static readonly PointF CpuLegend = new(SideX + SideW - 104, SideCard.Y + 138);
    private static readonly PointF GpuLegend = new(SideX + SideW - 50, SideCard.Y + 138);

    public static Theme Create()
    {
        var f = new FontResolver(null);
        var e = new List<ThemeElement>();

        Gauge(e, f, CpuCard, "CPU", CpuLabel, CpuGradient, CpuUsage, "CPUName", "CPUTemp", "CpuUsage",
            ("CLOCK", "CpuFrequencyGhz", " GHz"), ("POTÊNCIA", "CpuTEC", " W"), ("TENSÃO", "CpuVoltage", " V"));
        Gauge(e, f, GpuCard, "GPU", GpuLabel, GpuGradient, GpuUsage, "GPUName", "GPUTemp", "GpuUsage",
            ("CLOCK", "GpuFrequencyGhz", " GHz"), ("POTÊNCIA", "GpuTEC", " W"), ("VRAM", "GPUMemoryUse", ""));
        Side(e, f);

        return new Theme
        {
            Name = Name,
            BackgroundColor = Rgb(0xF5F7FB),
            DrawStatic = DrawBackdrop,
            Elements = e.OrderBy(x => x.Z).ToList(), // literais (Z=0) vão para o fundo pré-renderizado
        };
    }

    /// <summary>Cartão com anel externo = temperatura (degradê), anel interno = uso, e três números embaixo.</summary>
    private static void Gauge(List<ThemeElement> e, FontResolver f, RectangleF card, string label, Color labelColor,
        Color[] gradient, Color usageColor, string nameKey, string tempKey, string loadKey,
        params (string Caption, string Key, string Unit)[] stats)
    {
        float x = card.X, y = card.Y, cx = x + card.Width / 2;
        var center = new PointF(cx, y + 192);

        Literal(e, f, label, new RectangleF(x + 22, y + 18, 120, 20), BodySemibold, 12, labelColor);
        Text(e, f, nameKey, new RectangleF(x + 22, y + 35, card.Width - 44, 24), Body, 15, Soft);

        e.Add(new ArcElement
        {
            Z = 1, Center = center, Radius = 96, Thickness = 16, Data = tempKey, Min = 25, Max = 95,
            Track = Track, Colors = gradient, Knob = true,
        });
        e.Add(new ArcElement
        {
            Z = 1, Center = center, Radius = 74, Thickness = 7, Data = loadKey,
            Track = Track, Colors = new[] { usageColor },
        });

        Text(e, f, tempKey, new RectangleF(cx - 100, y + 128, 200, 80), Display, 64, Ink, TextAlign.Center, "°C", 22);
        Text(e, f, loadKey, new RectangleF(cx - 100, y + 208, 200, 24), BodySemibold, 15, Soft, TextAlign.Center, "% de uso", 13);

        for (int i = 0; i < stats.Length && i < 3; i++)
        {
            float column = x + 4 + i * 96;
            Literal(e, f, stats[i].Caption, new RectangleF(column, y + 290, 96, 16), BodySemibold, 10.5f, Faint, TextAlign.Center);
            Text(e, f, stats[i].Key, new RectangleF(column, y + 306, 96, 26), DisplaySemibold, 17, Ink, TextAlign.Center, stats[i].Unit, 12);
        }
    }

    /// <summary>Relógio, data, gráfico de atividade e memória.</summary>
    private static void Side(List<ThemeElement> e, FontResolver f)
    {
        float x = SideX, y = SideCard.Y, w = SideW;
        Text(e, f, "CurrentTimeShut", new RectangleF(x - 4, y + 4, w, 92), DisplayLight, 76, Ink);
        Text(e, f, "CurrentDateLong", new RectangleF(x, y + 98, w, 24), Body, 15, Soft);

        Literal(e, f, "ATIVIDADE · 60 s", new RectangleF(x, y + 134, 150, 16), BodySemibold, 10.5f, Faint);
        Literal(e, f, "CPU", new RectangleF(CpuLegend.X + 13, y + 134, 40, 16), BodySemibold, 10.5f, Muted);
        Literal(e, f, "GPU", new RectangleF(GpuLegend.X + 13, y + 134, 40, 16), BodySemibold, 10.5f, Muted);
        e.Add(new ChartElement
        {
            Z = 1,
            Bounds = new RectangleF(x, y + 160, w, 74),
            Series = new[] { "CpuUsage", "GpuUsage" },
            Colors = new[] { CpuGradient[1], GpuGradient[1] },
            Grid = Rgb(0xE2E8F0),
        });

        Literal(e, f, "MEMÓRIA", new RectangleF(x, y + 266, 120, 16), BodySemibold, 10.5f, Faint);
        Text(e, f, "MemoryUsage", new RectangleF(x, y + 254, w, 32), DisplaySemibold, 24, Ink, TextAlign.Right, "%", 14);
        e.Add(new BarElement
        {
            Z = 1,
            Bounds = new RectangleF(x, y + 290, w, 10),
            Data = "MemoryUsage",
            CornerRadius = new[] { 5f, 5f, 5f, 5f },
            Fill = RamFrom,
            Fill2 = RamTo,
            Back = Track,
        });
        Text(e, f, "MemoryUse", new RectangleF(x, y + 308, w, 22), Body, 13, Muted);
    }

    private static void DrawBackdrop(Graphics g)
    {
        g.Clear(Rgb(0xF5F7FB));
        Blob(g, new PointF(90, 10), 360, Color.FromArgb(170, 0x9C, 0xD4, 0xFF));  // céu
        Blob(g, new PointF(930, 20), 320, Color.FromArgb(160, 0xFB, 0xC2, 0xD9)); // rosa
        Blob(g, new PointF(470, 430), 400, Color.FromArgb(160, 0xD4, 0xC6, 0xFF)); // lavanda
        Blob(g, new PointF(760, 390), 240, Color.FromArgb(120, 0xFF, 0xDB, 0xAE)); // pêssego

        foreach (var card in new[] { CpuCard, GpuCard, SideCard })
        {
            SoftShadow(g, card, 20);
            using var path = Shapes.RoundedRect(card, 20);
            // Vidro fosco: branco quase opaco em cima, deixando a cor do fundo transparecer embaixo.
            using (var glass = new LinearGradientBrush(card, Color.FromArgb(236, 255, 255, 255), Color.FromArgb(200, 255, 255, 255), LinearGradientMode.Vertical))
                g.FillPath(glass, path);
            using var rim = new Pen(Color.White, 1.5f);
            g.DrawPath(rim, path);
        }

        using (var cpu = new SolidBrush(CpuGradient[1]))
            g.FillEllipse(cpu, CpuLegend.X, CpuLegend.Y, 8, 8);
        using (var gpu = new SolidBrush(GpuGradient[1]))
            g.FillEllipse(gpu, GpuLegend.X, GpuLegend.Y, 8, 8);
    }

    private static void Literal(List<ThemeElement> e, FontResolver f, string text, RectangleF bounds, string family, float size,
        Color color, TextAlign align = TextAlign.Left) =>
        ThemeKit.Literal(e, f, text, bounds, family, size, color, align);

    private static void Text(List<ThemeElement> e, FontResolver f, string key, RectangleF bounds, string family, float size,
        Color color, TextAlign align = TextAlign.Left, string unit = "", float unitSize = 0) =>
        ThemeKit.Text(e, f, key, bounds, family, size, color, align, unit, unitSize, Body, Muted);
}
