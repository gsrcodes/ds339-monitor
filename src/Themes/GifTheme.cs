using System.Drawing;
using System.Drawing.Drawing2D;
using static DS339Monitor.Themes.ThemeKit;

namespace DS339Monitor.Themes;

/// <summary>
/// GIF escolhido pelo usuário em tela cheia, com os dados por cima em painéis escuros translúcidos
/// (legíveis sobre qualquer imagem). Os painéis e rótulos são pré-desenhados; por quadro só entram os números.
/// </summary>
internal static class GifTheme
{
    public const string Name = "GIF";
    public const string StyleNone = "Nenhum";
    public const string StyleCompact = "Compacto";
    public const string StyleFull = "Completo";
    public static readonly string[] Styles = { StyleNone, StyleCompact, StyleFull };

    private const string Bold = "Bahnschrift SemiBold";
    private const string Regular = "Bahnschrift";

    private static readonly Color Panel = Color.FromArgb(150, 8, 12, 20);
    private static readonly Color PanelEdge = Color.FromArgb(55, 255, 255, 255);
    private static readonly Color White = Color.White;
    private static readonly Color Dim = Color.FromArgb(200, 255, 255, 255);
    private static readonly Color CpuColor = Rgb(0x7DD3FC);
    private static readonly Color GpuColor = Rgb(0xF9A8D4);

    public static Theme Create(string? gifPath, string style, int width, int height)
    {
        FrameAnimation? animation = null;
        if (!string.IsNullOrEmpty(gifPath) && File.Exists(gifPath))
        {
            try
            {
                animation = FrameAnimation.LoadGif(gifPath, width, height);
            }
            catch (Exception ex)
            {
                Log.Error($"Não consegui abrir o GIF {gifPath}", ex);
            }
        }
        if (animation is null) return Placeholder(width, height, gifPath);
        return WithOverlay(Name, animation, style, width, height);
    }

    /// <summary>
    /// Animação em tela cheia com os dados por cima no estilo pedido (Nenhum / Compacto / Completo).
    /// Também usado pelo tema One Piece. Os cantos de cima à esquerda ficam sempre livres.
    /// </summary>
    public static Theme WithOverlay(string name, IAnimation animation, string style, int width, int height)
    {
        var f = new FontResolver(null);
        var e = new List<ThemeElement>();
        var panels = new List<(RectangleF Rect, float Radius)>();
        if (style.Equals(StyleCompact, StringComparison.OrdinalIgnoreCase)) Compact(e, f, panels, width, height);
        else if (!style.Equals(StyleNone, StringComparison.OrdinalIgnoreCase)) Full(e, f, panels, width, height);

        return new Theme
        {
            Name = name,
            Width = width,
            Height = height,
            Animation = animation,
            BackgroundColor = Color.Black,
            DrawStatic = panels.Count == 0 ? null : g => DrawPanels(g, panels),
            Elements = e.OrderBy(x => x.Z).ToList(),
        };
    }

    /// <summary>Painéis de CPU e GPU nos cantos de baixo, relógio no canto de cima.</summary>
    private static void Full(List<ThemeElement> e, FontResolver f, List<(RectangleF, float)> panels, int w, int h)
    {
        var cpu = new RectangleF(20, h - 180, 300, 160);
        var gpu = new RectangleF(w - 320, h - 180, 300, 160);
        var clock = new RectangleF(w - 200, 20, 180, 66);
        panels.Add((cpu, 24));
        panels.Add((gpu, 24));
        panels.Add((clock, 24));

        Device(e, f, cpu, "CPU", CpuColor, "CPUTemp", "CpuUsage");
        Device(e, f, gpu, "GPU", GpuColor, "GPUTemp", "GpuUsage");
        Text(e, f, "CurrentTimeShut", new RectangleF(clock.X, clock.Y + 7, clock.Width, 60), Bold, 46, White, TextAlign.Center);
    }

    private static void Device(List<ThemeElement> e, FontResolver f, RectangleF p, string label, Color color, string tempKey, string loadKey)
    {
        Literal(e, f, label, new RectangleF(p.X + 22, p.Y + 14, 100, 36), Bold, 28, color);
        Text(e, f, loadKey, new RectangleF(p.Right - 152, p.Y + 14, 130, 36), Bold, 28, White, TextAlign.Right, "%", 20, unitColor: Dim);
        Text(e, f, tempKey, new RectangleF(p.X + 18, p.Y + 44, p.Width - 36, 112), Bold, 96, White, TextAlign.Left, "°", 96, unitColor: Dim);
    }

    /// <summary>Uma faixa embaixo: CPU °/% · GPU °/% · relógio.</summary>
    private static void Compact(List<ThemeElement> e, FontResolver f, List<(RectangleF, float)> panels, int w, int h)
    {
        var bar = new RectangleF(20, h - 80, w - 40, 62);
        panels.Add((bar, 31));
        float x = bar.X, y = bar.Y;

        Literal(e, f, "CPU", new RectangleF(x + 30, y + 13, 60, 36), Bold, 28, CpuColor);
        Text(e, f, "CPUTemp", new RectangleF(x + 90, y + 6, 100, 50), Bold, 40, White, TextAlign.Left, "°", 40, unitColor: Dim);
        Text(e, f, "CpuUsage", new RectangleF(x + 176, y + 13, 100, 36), Bold, 28, Dim, TextAlign.Left, "%", 20, unitColor: Dim);

        Literal(e, f, "GPU", new RectangleF(x + 330, y + 13, 60, 36), Bold, 28, GpuColor);
        Text(e, f, "GPUTemp", new RectangleF(x + 390, y + 6, 100, 50), Bold, 40, White, TextAlign.Left, "°", 40, unitColor: Dim);
        Text(e, f, "GpuUsage", new RectangleF(x + 476, y + 13, 100, 36), Bold, 28, Dim, TextAlign.Left, "%", 20, unitColor: Dim);

        Text(e, f, "CurrentTimeShut", new RectangleF(bar.Right - 190, y + 6, 160, 50), Bold, 40, White, TextAlign.Right);
    }

    /// <summary>Sem GIF escolhido (ou arquivo sumiu): instrução na própria tela.</summary>
    private static Theme Placeholder(int width, int height, string? gifPath)
    {
        var f = new FontResolver(null);
        var e = new List<ThemeElement>();
        Literal(e, f, "Escolha um GIF", new RectangleF(0, height / 2f - 80, width, 76), Bold, 60, White, TextAlign.Center);
        Literal(e, f, "Ícone do DS339 Monitor  ›  Pro Flow 360  ›  Escolher GIF…", new RectangleF(0, height / 2f + 10, width, 40), Regular, 26, Dim, TextAlign.Center);
        if (!string.IsNullOrEmpty(gifPath))
            Literal(e, f, $"(não consegui abrir {Path.GetFileName(gifPath)})", new RectangleF(0, height / 2f + 56, width, 36), Regular, 22, Rgb(0xFCA5A5), TextAlign.Center);

        return new Theme
        {
            Name = Name,
            Width = width,
            Height = height,
            BackgroundColor = Rgb(0x0B1220),
            DrawStatic = g =>
            {
                using var gradient = new LinearGradientBrush(new Rectangle(0, 0, width, height), Rgb(0x1E1B4B), Rgb(0x0B1220), LinearGradientMode.ForwardDiagonal);
                g.FillRectangle(gradient, 0, 0, width, height);
            },
            Elements = e,
        };
    }

    private static void DrawPanels(Graphics g, List<(RectangleF Rect, float Radius)> panels)
    {
        using var fill = new SolidBrush(Panel);
        using var edge = new Pen(PanelEdge, 1.2f);
        foreach (var (rect, radius) in panels)
        {
            using var path = Shapes.RoundedRect(rect, radius);
            g.FillPath(fill, path);
            g.DrawPath(edge, path);
        }
    }
}
