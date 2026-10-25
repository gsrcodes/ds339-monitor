using System.Globalization;

namespace DS339Monitor.Argb;

/// <summary>Um LED no gabinete: posição na foto (pixels) e no gabinete (u, v de 0 a 1), e o ângulo se for de um anel.</summary>
internal readonly record struct LedPoint(double X, double Y, double U, double V, double Angle)
{
    public bool OnRing => !double.IsNaN(Angle);
}

/// <summary>
/// Onde cada coisa está no gabinete, medido na foto que o usuário mandou (ui/gabinete.jpg, 1200×1600, vista pelo vidro
/// lateral): a Thermalright no topo, a Pro Flow sobre a CPU (a bomba), o fan de 120 na parede de trás (à esquerda), o
/// radiador com três fans na parede da direita, a placa de vídeo na vertical no meio e três fans no piso. As cenas dos
/// fans usam a posição de cada LED no gabinete: u = 0 na parede de trás (esquerda) … 1 no radiador (direita); v = 0 no
/// topo … 1 no piso.
/// <para>
/// A posição de cada parte fica no config.json (campo "Forma"), num formato curto em pixels da foto:
/// <c>linha x0,y0 x1,y1</c> · <c>anel rx,ry x,y</c> · <c>aneis rx,ry x1,y1 x2,y2 …</c> (os LEDs divididos pelos anéis,
/// cada um começando no alto e girando no sentido horário) · <c>matriz x0,y0 x1,y1 FxL h|v</c> (F fileiras de L LEDs,
/// fileiras na horizontal ou na vertical). "Inverter" na parte vira a ordem.
/// </para>
/// </summary>
internal static class CaseLayout
{
    public const string Photo = "gabinete.jpg";
    public const int PhotoWidth = 1200;
    public const int PhotoHeight = 1600;

    /// <summary>O recorte da foto que o painel mostra (o gabinete).</summary>
    public static readonly int[] View = { 90, 330, 990, 1230 };

    /// <summary>Telas e placa de vídeo na foto (x0, y0, x1, y1).</summary>
    public static readonly int[] Thermalright = { 268, 371, 722, 490 };
    public static readonly int[] ProFlow = { 340, 594, 586, 736 };
    public static readonly int[] Gpu = { 176, 766, 816, 1060 };

    // A caixa de dentro do gabinete na foto.
    private const double Left = 100, Top = 360, Width = 860, Height = 830;

    public static (double U, double V) ToCase(double x, double y) => ((x - Left) / Width, (y - Top) / Height);

    /// <summary>A bomba (CPU), atrás da Pro Flow: o centro do gabinete para o Haki e o farol.</summary>
    public static readonly (double U, double V) Pump = ToCase(463, 665);

    /// <summary>O meio da placa de vídeo (onde a luz dela entra nas cenas).</summary>
    public static readonly (double U, double V) GpuCenter = ToCase(496, 912);

    /// <summary>O fan de 120 da parede de trás, por onde o ar sai.</summary>
    public static readonly (double U, double V) Exhaust = ToCase(192, 598);

    /// <summary>Memórias (o "copiar" do Modo Mangá acende as duas).</summary>
    public static readonly (double U, double V) Memory = ToCase(611, 610);

    /// <summary>
    /// As partes deste gabinete, pelo canal e pelo trecho de LEDs que vêm do SignalRGB: o nome da peça (dito pelo
    /// usuário em 2026-10-21) e a forma na foto.
    /// </summary>
    private static readonly (int Canal, int Inicio, int Leds, string Grupo, string Forma)[] Known =
    {
        (1, 0, 25, "Fans de baixo", "aneis 40,9 235,1108 470,1132 735,1152"),
        (1, 25, 31, "Fans de baixo", "linha 140,1097 880,1117"),
        (1, 56, 31, "Fans de baixo", "linha 115,1170 895,1188"),
        (2, 0, 25, "Fans do radiador", "aneis 42,42 822,545 815,772 812,992"),
        (2, 25, 31, "Fans do radiador", "linha 742,440 742,1100"),
        (2, 56, 31, "Fans do radiador", "linha 935,440 935,1100"),
        (3, 0, 9, "Memória da direita", "linha 622,515 622,705"),
        (4, 0, 110, "Cabo da placa de vídeo", "matriz 682,684 790,758 5x22 v"),
        (5, 0, 8, "Fan traseiro", "anel 28,38 192,598"),
        (5, 8, 9, "Fan traseiro", "linha 132,472 132,724"),
        (5, 17, 9, "Fan traseiro", "linha 268,470 268,728"),
        (6, 0, 5, "Bomba do water cooler", "anel 140,88 463,665"),
        (7, 0, 9, "Memória da esquerda", "linha 600,515 600,705"),
        (8, 0, 50, "Placa sob a placa-mãe", "linha 160,1082 700,1082"),
        (9, 0, 110, "Cabo da placa-mãe", "matriz 622,515 748,618 5x22 v"),
    };

    /// <summary>Nomes que a importação do SignalRGB dá (trocados pelos nomes de verdade enquanto não foram mudados).</summary>
    private static readonly string[] ImportNames = { "Fans 360", "Fan 120", "Memória RAM", "Capa de cabo" };

    /// <summary>Dá a forma (e o nome de verdade, se ainda estiver o da importação) às partes conhecidas. True = mudou algo.</summary>
    public static bool Apply(List<ArgbZone> zones)
    {
        bool changed = false;
        foreach (var zone in zones)
        {
            foreach (var known in Known)
            {
                if (known.Canal != zone.Canal || known.Inicio != zone.Inicio || known.Leds != zone.Leds) continue;
                if (string.IsNullOrWhiteSpace(zone.Forma))
                {
                    zone.Forma = known.Forma;
                    changed = true;
                }
                if (ImportNames.Any(n => zone.Grupo.StartsWith(n, StringComparison.Ordinal)))
                {
                    zone.Grupo = known.Grupo;
                    changed = true;
                }
                break;
            }
        }
        return changed;
    }

    /// <summary>Posições dos <paramref name="leds"/> LEDs de uma forma (null = forma vazia ou inválida).</summary>
    public static LedPoint[]? Parse(string? shape, int leds, bool reverse)
    {
        if (string.IsNullOrWhiteSpace(shape) || leds <= 0) return null;
        try
        {
            string[] parts = shape.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var points = new LedPoint[leds];
            switch (parts[0].ToLowerInvariant())
            {
                case "linha":
                {
                    var (x0, y0) = Pair(parts[1]);
                    var (x1, y1) = Pair(parts[2]);
                    for (int i = 0; i < leds; i++)
                    {
                        double t = leds == 1 ? 0.5 : i / (double)(leds - 1);
                        points[i] = Point(x0 + (x1 - x0) * t, y0 + (y1 - y0) * t, double.NaN);
                    }
                    break;
                }
                case "anel":
                case "aneis":
                {
                    var (rx, ry) = Pair(parts[1]);
                    var centers = parts.Skip(2).Select(Pair).ToArray();
                    if (centers.Length == 0) return null;
                    int at = 0;
                    for (int ring = 0; ring < centers.Length; ring++)
                    {
                        // LEDs divididos por igual; a sobra vai para os últimos anéis (25 em 3 = 8, 8, 9).
                        int count = leds / centers.Length + (ring >= centers.Length - leds % centers.Length ? 1 : 0);
                        for (int j = 0; j < count && at < leds; j++, at++)
                        {
                            double angle = -Math.PI / 2 + 2 * Math.PI * j / count;
                            points[at] = Point(centers[ring].A + rx * Math.Cos(angle), centers[ring].B + ry * Math.Sin(angle), angle);
                        }
                    }
                    break;
                }
                case "matriz":
                {
                    var (x0, y0) = Pair(parts[1]);
                    var (x1, y1) = Pair(parts[2]);
                    string[] size = parts[3].ToLowerInvariant().Split('x');
                    int rows = int.Parse(size[0], CultureInfo.InvariantCulture), cols = int.Parse(size[1], CultureInfo.InvariantCulture);
                    bool vertical = parts.Length > 4 && parts[4].Equals("v", StringComparison.OrdinalIgnoreCase);
                    for (int i = 0; i < leds; i++)
                    {
                        int row = Math.Min(rows - 1, i / cols), k = i % cols;
                        double across = (row + 0.5) / rows, along = (k + 0.5) / cols;
                        points[i] = vertical
                            ? Point(x0 + (x1 - x0) * across, y0 + (y1 - y0) * along, double.NaN)
                            : Point(x0 + (x1 - x0) * along, y0 + (y1 - y0) * across, double.NaN);
                    }
                    break;
                }
                default:
                    return null;
            }
            if (reverse) Array.Reverse(points);
            return points;
        }
        catch (Exception ex) when (ex is FormatException or IndexOutOfRangeException or OverflowException)
        {
            return null;
        }
    }

    /// <summary>
    /// Os anéis de uma parte <c>anel</c>/<c>aneis</c> (um por fan): onde cada um começa e quantos LEDs tem, divididos como
    /// em <see cref="Parse"/>. Null = a parte não é de anéis.
    /// </summary>
    public static List<(int Start, int Count)>? Rings(string? shape, int leds)
    {
        string[] parts = shape?.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
        if (parts.Length < 3 || leds <= 0 || !(parts[0].Equals("anel", StringComparison.OrdinalIgnoreCase) || parts[0].Equals("aneis", StringComparison.OrdinalIgnoreCase)))
            return null;
        int centers = parts.Length - 2;
        var rings = new List<(int, int)>();
        int at = 0;
        for (int ring = 0; ring < centers && at < leds; ring++)
        {
            int count = leds / centers + (ring >= centers - leds % centers ? 1 : 0);
            count = Math.Min(count, leds - at);
            rings.Add((at, count));
            at += count;
        }
        return rings;
    }

    /// <summary>Fileiras × LEDs por fileira de uma parte em grade (<c>matriz</c>); qualquer outra forma = uma fileira só.</summary>
    public static (int Rows, int Cols) Grid(string? shape, int leds)
    {
        string[] parts = shape?.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
        if (parts.Length > 3 && parts[0].Equals("matriz", StringComparison.OrdinalIgnoreCase))
        {
            string[] size = parts[3].ToLowerInvariant().Split('x');
            if (size.Length == 2 && int.TryParse(size[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int rows)
                && int.TryParse(size[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int cols) && rows > 0 && cols > 0)
                return (rows, cols);
        }
        return (1, Math.Max(1, leds));
    }

    private static LedPoint Point(double x, double y, double angle)
    {
        var (u, v) = ToCase(x, y);
        return new LedPoint(x, y, u, v, angle);
    }

    private static (double A, double B) Pair(string text)
    {
        string[] xy = text.Split(',');
        return (double.Parse(xy[0], CultureInfo.InvariantCulture), double.Parse(xy[1], CultureInfo.InvariantCulture));
    }
}
