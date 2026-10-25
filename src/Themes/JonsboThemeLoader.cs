using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.Text;

namespace DS339Monitor.Themes;

/// <summary>
/// Lê temas no formato do JONSBO-AIO (Programme/339Hxx/Setting.txt):
/// <code>
/// name:339H01
/// width:960
/// height:376
/// decoration.png:x@0,y@0,z@12,height@376,width@960
/// Text:x@443,y@262,z@11,maxheight@80,maxwidth@500,FontSize@17,FontFamily@#Fonte,Foreground@#FFFFFFFF,Title@,data@GPUTemp,unit@℃
/// BorderLine:x@277,y@222,z@7,maxheight@35,maxwidth@207,MaxNum@200,CornerRadius@0 0 0 0,Fill@#FF66BB17,BorderThicknes@2,BorderFill@#FFFFFF44,BackColor@#00000000,data@GPUTemp
/// </code>
/// O fundo é back.png na raiz do tema; imagens extras ficam em source/; fontes em font/.
/// </summary>
internal static class JonsboThemeLoader
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
    private static readonly string[] BackgroundNames = { "back.png", "back.jpg", "back.jpeg", "source/back.png" };

    public sealed record Header(string Id, int Width, int Height);

    public static Header? ReadHeader(string settingPath)
    {
        try
        {
            string id = Path.GetFileName(Path.GetDirectoryName(settingPath)) ?? "";
            int width = 0, height = 0;
            foreach (var line in File.ReadLines(settingPath, Encoding.UTF8).Take(10))
            {
                int colon = line.IndexOf(':');
                if (colon <= 0) continue;
                string key = line[..colon].Trim();
                string value = line[(colon + 1)..].Trim();
                if (key.Equals("name", StringComparison.OrdinalIgnoreCase)) id = value;
                else if (key.Equals("width", StringComparison.OrdinalIgnoreCase)) int.TryParse(value, out width);
                else if (key.Equals("height", StringComparison.OrdinalIgnoreCase)) int.TryParse(value, out height);
            }
            return new Header(id, width, height);
        }
        catch (IOException)
        {
            return null;
        }
    }

    /// <summary>Temas com fundo em vídeo (back.mp4) não são suportados — tocar vídeo é justamente o que pesa.</summary>
    public static bool HasStaticBackground(string directory) =>
        BackgroundNames.Any(n => File.Exists(Path.Combine(directory, n)));

    public static Theme Load(string directory, string displayName)
    {
        int width = 960, height = 376;
        var fonts = LoadFonts(Path.Combine(directory, "font"));
        var resolver = new FontResolver(fonts);
        var elements = new List<ThemeElement>();

        foreach (var raw in File.ReadAllLines(Path.Combine(directory, "Setting.txt"), Encoding.UTF8))
        {
            string line = raw.Trim();
            int colon = line.IndexOf(':');
            if (colon <= 0) continue;
            string kind = line[..colon].Trim();
            string rest = line[(colon + 1)..];

            switch (kind.ToLowerInvariant())
            {
                case "name":
                    continue;
                case "width":
                    width = ParseInt(rest, width);
                    continue;
                case "height":
                    height = ParseInt(rest, height);
                    continue;
            }

            var p = ParseProperties(rest);
            try
            {
                ThemeElement? element = kind.ToLowerInvariant() switch
                {
                    "text" => CreateText(p, resolver),
                    "borderline" => CreateBar(p),
                    _ when IsImageFile(kind) => CreateImage(directory, kind, p),
                    _ => null,
                };
                if (element is not null) elements.Add(element);
                else Log.Warn($"Tema {displayName}: elemento '{kind}' ignorado.");
            }
            catch (Exception ex)
            {
                Log.Error($"Tema {displayName}: linha inválida '{line}'", ex);
            }
        }

        return new Theme
        {
            Name = displayName,
            Width = width,
            Height = height,
            Background = LoadBackground(directory, width, height),
            Elements = elements.OrderBy(e => e.Z).ToList(), // OrderBy é estável: empate mantém a ordem do arquivo
            Fonts = fonts,
        };
    }

    private static TextElement CreateText(Dictionary<string, string> p, FontResolver fonts)
    {
        bool literal = Get(p, "IsDefaultText").Equals("true", StringComparison.OrdinalIgnoreCase);
        return new TextElement
        {
            Z = ParseInt(Get(p, "z"), 0),
            Bounds = new RectangleF(ParseFloat(Get(p, "x"), 0), ParseFloat(Get(p, "y"), 0), ParseFloat(Get(p, "maxwidth"), 500), ParseFloat(Get(p, "maxheight"), 80)),
            Font = fonts.Create(Get(p, "FontFamily"), ParseFloat(Get(p, "FontSize"), 16)),
            Color = ParseColor(Get(p, "Foreground"), Color.White),
            Title = Get(p, "Title"),
            Data = Get(p, "data"),
            IsLiteral = literal,
            Unit = Get(p, "unit"),
        };
    }

    private static BarElement CreateBar(Dictionary<string, string> p)
    {
        var radii = Get(p, "CornerRadius")
            .Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(v => ParseFloat(v, 0))
            .ToArray();
        if (radii.Length == 1) radii = new[] { radii[0], radii[0], radii[0], radii[0] };
        if (radii.Length != 4) radii = new float[4];

        return new BarElement
        {
            Z = ParseInt(Get(p, "z"), 0),
            Bounds = new RectangleF(ParseFloat(Get(p, "x"), 0), ParseFloat(Get(p, "y"), 0), ParseFloat(Get(p, "maxwidth"), 100), ParseFloat(Get(p, "maxheight"), 10)),
            Data = Get(p, "data"),
            MaxValue = Math.Max(1, ParseFloat(Get(p, "MaxNum"), 100)),
            CornerRadius = radii,
            Fill = ParseColor(Get(p, "Fill"), Color.White),
            Back = ParseColor(Get(p, "BackColor"), Color.Transparent),
            Border = ParseColor(Get(p, "BorderFill"), Color.Transparent),
            BorderThickness = ParseFloat(Get(p, "BorderThicknes"), 0),
        };
    }

    private static ImageElement? CreateImage(string directory, string file, Dictionary<string, string> p)
    {
        string? path = new[] { Path.Combine(directory, "source", file), Path.Combine(directory, file) }.FirstOrDefault(File.Exists);
        if (path is null) return null;

        int w = ParseInt(Get(p, "width"), 0);
        int h = ParseInt(Get(p, "height"), 0);
        using var source = new Bitmap(path);
        return new ImageElement
        {
            Z = ParseInt(Get(p, "z"), 0),
            Location = new Point(ParseInt(Get(p, "x"), 0), ParseInt(Get(p, "y"), 0)),
            Image = ToPArgb(source, w > 0 ? w : source.Width, h > 0 ? h : source.Height),
        };
    }

    private static Bitmap? LoadBackground(string directory, int width, int height)
    {
        string? path = BackgroundNames.Select(n => Path.Combine(directory, n)).FirstOrDefault(File.Exists);
        if (path is null) return null;
        using var source = new Bitmap(path);
        return ToPArgb(source, width, height);
    }

    /// <summary>Converte para 32bpp pré-multiplicado já no tamanho final: é o formato mais rápido para o GDI+ compor.</summary>
    private static Bitmap ToPArgb(Image source, int width, int height)
    {
        var result = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
        using var g = Graphics.FromImage(result);
        g.CompositingMode = CompositingMode.SourceCopy;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.DrawImage(source, new Rectangle(0, 0, width, height));
        return result;
    }

    private static PrivateFontCollection? LoadFonts(string fontDirectory)
    {
        if (!Directory.Exists(fontDirectory)) return null;
        var collection = new PrivateFontCollection();
        foreach (var file in Directory.EnumerateFiles(fontDirectory).Where(f => f.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".otf", StringComparison.OrdinalIgnoreCase)))
        {
            try
            {
                collection.AddFontFile(file);
            }
            catch (Exception ex)
            {
                Log.Warn($"Não consegui carregar a fonte {file}: {ex.Message}");
            }
        }
        if (collection.Families.Length == 0)
        {
            collection.Dispose();
            return null;
        }
        return collection;
    }

    /// <summary>"x@1,y@2,Title@A,B" → {x:1, y:2, Title:"A,B"} (vírgula sem @ depois dela pertence ao valor anterior).</summary>
    private static Dictionary<string, string> ParseProperties(string text)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string? lastKey = null;
        foreach (var part in text.Split(','))
        {
            int at = part.IndexOf('@');
            if (at > 0)
            {
                lastKey = part[..at].Trim();
                result[lastKey] = part[(at + 1)..];
            }
            else if (lastKey is not null)
            {
                result[lastKey] += "," + part;
            }
        }
        return result;
    }

    private static bool IsImageFile(string name) =>
        name.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase);

    private static string Get(Dictionary<string, string> p, string key) => p.TryGetValue(key, out var value) ? value : "";

    private static int ParseInt(string value, int fallback) =>
        double.TryParse(value.Trim(), NumberStyles.Float, Inv, out var d) ? (int)Math.Round(d) : fallback;

    private static float ParseFloat(string value, float fallback) =>
        float.TryParse(value.Trim(), NumberStyles.Float, Inv, out var f) ? f : fallback;

    /// <summary>"#AARRGGBB" ou "#RRGGBB".</summary>
    private static Color ParseColor(string value, Color fallback)
    {
        string hex = value.Trim().TrimStart('#');
        if (!uint.TryParse(hex, NumberStyles.HexNumber, Inv, out uint argb)) return fallback;
        return hex.Length switch
        {
            8 => Color.FromArgb(unchecked((int)argb)),
            6 => Color.FromArgb(unchecked((int)(0xFF000000 | argb))),
            _ => fallback,
        };
    }
}
