using System.Drawing;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace DS339Monitor.Manga;

/// <summary>
/// Fontes do Modo Mangá, na pasta fonts\ (licença OFL, do Google Fonts): Dela Gothic One para as onomatopeias em
/// japonês e Bangers para o letreiramento dos balões e caixas. Sem os arquivos, cai para Yu Gothic UI e Bahnschrift.
/// </summary>
internal static class MangaFonts
{
    private static readonly Lazy<(FontFamily Sfx, FontFamily Comic)> Families = new(Load);
    private static PrivateFontCollection? _collection; // precisa viver enquanto as famílias forem usadas

    /// <summary>Onomatopeias (katakana e kanji, traço grosso).</summary>
    public static FontFamily Sfx => Families.Value.Sfx;

    /// <summary>Letreiramento de quadrinho (caixa alta), para balões e caixas de narração.</summary>
    public static FontFamily Comic => Families.Value.Comic;

    /// <summary>Texto corrido menor (nomes de arquivo e de música), legível em qualquer tamanho.</summary>
    public static readonly FontFamily Text = new("Bahnschrift");

    /// <summary>Símbolos que as fontes acima não têm (♪).</summary>
    public static readonly FontFamily Symbols = new("Segoe UI Symbol");

    private static (FontFamily, FontFamily) Load()
    {
        var collection = new PrivateFontCollection();
        string folder = Path.Combine(AppContext.BaseDirectory, "fonts");
        foreach (string file in new[] { "DelaGothicOne-Regular.ttf", "Bangers-Regular.ttf" })
        {
            string path = Path.Combine(folder, file);
            try
            {
                if (File.Exists(path)) collection.AddFontFile(path);
                else Log.Warn($"Modo Mangá: fonte {file} não encontrada em {folder}; usando uma do Windows.");
            }
            catch (Exception ex) when (ex is ArgumentException or ExternalException or IOException)
            {
                Log.Warn($"Modo Mangá: não consegui carregar {file} ({ex.Message}).");
            }
        }
        _collection = collection;
        FontFamily Find(string name, string fallback) =>
            collection.Families.FirstOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) ?? new FontFamily(fallback);
        return (Find("Dela Gothic One", "Yu Gothic UI"), Find("Bangers", "Bahnschrift"));
    }
}
