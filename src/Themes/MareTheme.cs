using System.Drawing;
using static DS339Monitor.Themes.ThemeKit;

namespace DS339Monitor.Themes;

/// <summary>
/// "Maré": feito para ler de longe numa tela pequena. Cada cartão de CPU/GPU é um reservatório — o nível do
/// líquido é o uso e a cor é a temperatura (azul frio → verde → âmbar → vermelho quente) — com a temperatura
/// em letra gigante por cima. Nenhum texto abaixo de 22 px.
/// </summary>
internal static class MareTheme
{
    public const string Name = "Maré";

    // Bahnschrift é a DIN do Windows, desenhada para sinalização: a mais legível em tela pequena.
    private const string Bold = "Bahnschrift SemiBold";
    private const string Regular = "Bahnschrift";

    private static readonly Color Ink = Rgb(0x0B1220);
    private static readonly Color Soft = Rgb(0x3B4A5E);
    private static readonly Color Muted = Rgb(0x64748B);
    private static readonly Color Track = Rgb(0xE6EBF2);
    private static readonly Color Edge = Rgb(0xDCE2EB);

    /// <summary>Cor do líquido por temperatura (°C). Tons pastel próximos, para a mistura entre eles sair limpa.</summary>
    private static readonly (float, Color)[] Heat =
    {
        (45, Rgb(0x7DD3FC)), // céu    — ocioso
        (55, Rgb(0x5EEAD4)), // água
        (63, Rgb(0x86EFAC)), // menta  — tranquilo
        (70, Rgb(0xFDE68A)), // âmbar  — jogando
        (78, Rgb(0xFDBA74)), // pêssego
        (86, Rgb(0xFDA4AF)), // coral  — esquentando
        (94, Rgb(0xF87171)), // vermelho — quente
    };

    private const float Radius = 26;

    /// <summary>
    /// Layout para 960 de largura e qualquer altura (376 na DS339, 480 no Pro Flow 360); a partir de 1600 de largura
    /// (Thermalright, 1920x462), o layout largo.
    /// </summary>
    public static Theme Create(int width = 960, int height = 376)
    {
        if (width >= 1600) return CreateWide(width, height);

        float cardHeight = height - 24;
        var cpuCard = new RectangleF(12, 12, 318, cardHeight);
        var gpuCard = new RectangleF(342, 12, 318, cardHeight);
        var sideCard = new RectangleF(672, 12, 276, cardHeight);

        var f = new FontResolver(null);
        var e = new List<ThemeElement>();

        Tank(e, f, cpuCard, "CPU", "CPUTemp", "CpuUsage", ("CpuFrequencyGhz", " GHz"), ("CpuTEC", " W"));
        Tank(e, f, gpuCard, "GPU", "GPUTemp", "GpuUsage", ("GpuFrequencyGhz", " GHz"), ("GpuTEC", " W"));
        Side(e, f, sideCard);

        return new Theme
        {
            Name = Name,
            Width = width,
            Height = height,
            BackgroundColor = Rgb(0xEEF2F7),
            DrawStatic = g => DrawBackdrop(g, width, height, new[] { cpuCard, gpuCard, sideCard }, sideCard),
            Elements = e.OrderBy(x => x.Z).ToList(),
        };
    }

    /// <summary>
    /// Layout largo: os dois reservatórios do mesmo tamanho do layout normal (só mais largos), e o que no normal se aperta
    /// num cartão lateral ganha dois cartões — relógio com dia da semana e data; RAM e VRAM, cada uma com sua barra.
    /// </summary>
    private static Theme CreateWide(int width, int height)
    {
        const float Gap = 12, TankWidth = 540, ClockWidth = 400;
        float cardHeight = height - 2 * Gap;
        var cpuCard = new RectangleF(Gap, Gap, TankWidth, cardHeight);
        var gpuCard = new RectangleF(cpuCard.Right + Gap, Gap, TankWidth, cardHeight);
        var clockCard = new RectangleF(gpuCard.Right + Gap, Gap, ClockWidth, cardHeight);
        var memoryCard = new RectangleF(clockCard.Right + Gap, Gap, width - Gap - (clockCard.Right + Gap), cardHeight);

        var f = new FontResolver(null);
        var e = new List<ThemeElement>();
        Tank(e, f, cpuCard, "CPU", "CPUTemp", "CpuUsage", ("CpuFrequencyGhz", " GHz"), ("CpuTEC", " W"));
        Tank(e, f, gpuCard, "GPU", "GPUTemp", "GpuUsage", ("GpuFrequencyGhz", " GHz"), ("GpuTEC", " W"));

        // Relógio: hora gigante, dia da semana e data, o bloco todo centrado na altura.
        float x = clockCard.X, w = clockCard.Width, top = clockCard.Y + (cardHeight - 236) / 2;
        Text(e, f, "CurrentTimeShut", new RectangleF(x, top, w, 140), Bold, 124, Ink, TextAlign.Center);
        Text(e, f, "CurrentWeek", new RectangleF(x, top + 146, w, 46), Bold, 34, Soft, TextAlign.Center);
        Text(e, f, "CurrentDateShort", new RectangleF(x, top + 192, w, 44), Regular, 32, Muted, TextAlign.Center);

        // Memória: RAM em cima, VRAM embaixo, separadas por uma linha no meio do cartão.
        float half = cardHeight / 2;
        Usage(e, f, new RectangleF(memoryCard.X, memoryCard.Y, memoryCard.Width, half), "RAM", "MemoryUsage", "MemoryUse",
            Rgb(0xA78BFA), Rgb(0xEC4899));
        Usage(e, f, new RectangleF(memoryCard.X, memoryCard.Y + half, memoryCard.Width, half), "VRAM", "GPUMemoryLoad", "GPUMemoryUse",
            Rgb(0x7DD3FC), Rgb(0x6366F1));

        return new Theme
        {
            Name = Name,
            Width = width,
            Height = height,
            BackgroundColor = Rgb(0xEEF2F7),
            DrawStatic = g =>
            {
                DrawCards(g, width, height, new[] { cpuCard, gpuCard, clockCard, memoryCard });
                using var divider = new Pen(Track, 2);
                g.DrawLine(divider, memoryCard.X + 26, memoryCard.Y + half, memoryCard.Right - 26, memoryCard.Y + half);
            },
            Elements = e.OrderBy(x => x.Z).ToList(),
        };
    }

    /// <summary>Bloco de uso de memória: rótulo, porcentagem grande, barra e quanto está em uso.</summary>
    private static void Usage(List<ThemeElement> e, FontResolver f, RectangleF area, string label, string percentKey, string usedKey,
        Color from, Color to)
    {
        float inner = area.X + 26, innerW = area.Width - 52, y = area.Y + (area.Height - 168) / 2;
        Literal(e, f, label, new RectangleF(inner, y + 22, 140, 40), Bold, 30, Soft);
        Text(e, f, percentKey, new RectangleF(inner, y, innerW, 80), Bold, 64, Ink, TextAlign.Right, "%", 32, unitColor: Soft);
        e.Add(new BarElement
        {
            Z = 1,
            Bounds = new RectangleF(inner, y + 92, innerW, 22),
            Data = percentKey,
            CornerRadius = new[] { 11f, 11f, 11f, 11f },
            Fill = from,
            Fill2 = to,
            Back = Track,
        });
        Text(e, f, usedKey, new RectangleF(inner, y + 126, innerW, 40), Regular, 28, Soft);
    }

    private static void Tank(List<ThemeElement> e, FontResolver f, RectangleF card, string label, string tempKey, string loadKey,
        (string Key, string Unit) left, (string Key, string Unit) right)
    {
        float x = card.X, y = card.Y, w = card.Width, h = card.Height;
        e.Add(new LiquidElement
        {
            Z = 1,
            Bounds = card,
            CornerRadius = Radius,
            LevelKey = loadKey,
            ColorKey = tempKey,
            ColorStops = Heat,
            Border = Edge,
        });

        // Os textos ficam por cima do líquido (Z=2), inclusive o rótulo fixo.
        Literal(e, f, label, new RectangleF(x + 24, y + 16, 140, 48), Bold, 36, Ink, z: 2);
        Text(e, f, loadKey, new RectangleF(x + w - 184, y + 16, 160, 48), Bold, 36, Ink, TextAlign.Right, "%", 24, unitColor: Soft, z: 2);
        // A temperatura fica centralizada na altura do cartão; os números de baixo grudam no rodapé.
        Text(e, f, tempKey, new RectangleF(x, y + (h - 190) / 2 - 5, w, 190), Bold, 172, Ink, TextAlign.Center, "°", 172, unitColor: Soft, z: 2);
        Text(e, f, left.Key, new RectangleF(x + 24, y + h - 56, 150, 48), Bold, 36, Ink, TextAlign.Left, left.Unit, 22, unitColor: Soft, z: 2);
        Text(e, f, right.Key, new RectangleF(x + w - 174, y + h - 56, 150, 48), Bold, 36, Ink, TextAlign.Right, right.Unit, 22, unitColor: Soft, z: 2);
    }

    /// <summary>Relógio, data, RAM e VRAM. O bloco da RAM gruda no rodapé; relógio e data se centralizam no espaço de cima.</summary>
    private static void Side(List<ThemeElement> e, FontResolver f, RectangleF card)
    {
        float x = card.X, y = card.Y, w = card.Width, h = card.Height;
        float inner = x + 22, innerW = w - 44;
        float top = y + (h - 352) / 2; // na DS339 (cartão de 352) fica igual ao layout original
        float bottom = y + h;

        Text(e, f, "CurrentTimeShut", new RectangleF(x, top + 10, w, 110), Bold, 96, Ink, TextAlign.Center);
        Text(e, f, "CurrentDateShort", new RectangleF(x, top + 120, w, 40), Regular, 28, Soft, TextAlign.Center);

        Literal(e, f, "RAM", new RectangleF(inner, bottom - 156, 100, 40), Bold, 28, Soft);
        Text(e, f, "MemoryUsage", new RectangleF(inner, bottom - 170, innerW, 60), Bold, 50, Ink, TextAlign.Right, "%", 28, unitColor: Soft);
        e.Add(new BarElement
        {
            Z = 1,
            Bounds = new RectangleF(inner, bottom - 106, innerW, 22),
            Data = "MemoryUsage",
            CornerRadius = new[] { 11f, 11f, 11f, 11f },
            Fill = Rgb(0xA78BFA),
            Fill2 = Rgb(0xEC4899),
            Back = Track,
        });
        Text(e, f, "MemoryUse", new RectangleF(inner, bottom - 74, innerW, 32), Regular, 24, Soft);

        Literal(e, f, "VRAM", new RectangleF(inner, bottom - 38, 100, 32), Bold, 24, Muted);
        Text(e, f, "GPUMemoryUse", new RectangleF(inner, bottom - 38, innerW, 32), Bold, 24, Ink, TextAlign.Right);
    }

    private static void DrawBackdrop(Graphics g, int width, int height, RectangleF[] cards, RectangleF side)
    {
        DrawCards(g, width, height, cards);
        float dividerY = side.Y + (side.Height - 352) / 2 + 172;
        using var divider = new Pen(Track, 2);
        g.DrawLine(divider, side.X + 22, dividerY, side.Right - 22, dividerY);
    }

    /// <summary>Fundo claro com duas manchas de cor e os cartões brancos com sombra suave.</summary>
    private static void DrawCards(Graphics g, int width, int height, RectangleF[] cards)
    {
        g.Clear(Rgb(0xEEF2F7));
        Blob(g, new PointF(80, 0), 320, Color.FromArgb(90, 0x9C, 0xD4, 0xFF));
        Blob(g, new PointF(width - 20, height + 4), 300, Color.FromArgb(80, 0xD4, 0xC6, 0xFF));

        using var fill = new SolidBrush(Color.White);
        using var edge = new Pen(Edge, 1.5f);
        foreach (var card in cards)
        {
            SoftShadow(g, card, Radius);
            using var path = Shapes.RoundedRect(card, Radius);
            g.FillPath(fill, path);
            g.DrawPath(edge, path);
        }
    }
}
