using DS339Monitor.App;

namespace DS339Monitor.Themes;

internal enum ScreenKind
{
    Ds339,
    ProFlow,
    Thermalright,
}

internal sealed record ThemeInfo(string Name, string? Directory)
{
    public bool IsBuiltIn => Directory is null;
}

/// <summary>
/// Temas disponíveis para cada tela. DS339 (960x376): embutidos + pasta themes/ + temas que o JONSBO-AIO tiver
/// baixado (só os de fundo estático). Pro Flow 360 (960x480): GIF, One Piece e Maré. Thermalright (1920x462): Grand Line,
/// Maré (layout largo), Vídeo, One Piece e GIF.
/// </summary>
internal static class ThemeCatalog
{
    public static string LocalThemesDirectory => Path.Combine(AppContext.BaseDirectory, "themes");

    public static (int Width, int Height) Size(ScreenKind kind) => kind switch
    {
        ScreenKind.ProFlow => (960, 480),
        ScreenKind.Thermalright => (1920, 462),
        _ => (960, 376),
    };

    public static List<ThemeInfo> Discover(ScreenKind kind)
    {
        var themes = BuiltIns(kind).Select(b => new ThemeInfo(b.Name, null)).ToList();
        if (kind != ScreenKind.Ds339) return themes;

        var names = new HashSet<string>(themes.Select(t => t.Name), StringComparer.OrdinalIgnoreCase);
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var (width, height) = Size(kind);
        foreach (var root in JonsboRoots())
        {
            if (!System.IO.Directory.Exists(root)) continue;
            foreach (var dir in System.IO.Directory.EnumerateDirectories(root).OrderBy(d => d, StringComparer.OrdinalIgnoreCase))
            {
                string setting = Path.Combine(dir, "Setting.txt");
                if (!File.Exists(setting)) continue;
                var header = JonsboThemeLoader.ReadHeader(setting);
                if (header is null || header.Width != width || header.Height != height) continue;
                if (!JonsboThemeLoader.HasStaticBackground(dir)) continue;
                if (!ids.Add(header.Id)) continue; // mesmo tema já veio de outra pasta

                string name = Path.GetFileName(dir);
                if (names.Add(name)) themes.Add(new ThemeInfo(name, dir));
            }
        }
        return themes;
    }

    /// <summary>Carrega o tema pedido; se não existir ou falhar, volta para o padrão da tela.</summary>
    public static Theme Load(ScreenKind kind, string name, AppConfig config)
    {
        var builtIn = BuiltIns(kind).FirstOrDefault(b => b.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (builtIn.Create is not null) return builtIn.Create(config);

        var info = Discover(kind).FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (info?.Directory is null)
        {
            Log.Warn($"Tema '{name}' não encontrado para {kind}; usando o padrão.");
            return Fallback(kind);
        }

        try
        {
            return JonsboThemeLoader.Load(info.Directory, info.Name);
        }
        catch (Exception ex)
        {
            Log.Error($"Falha ao carregar o tema '{name}'; usando o padrão", ex);
            return Fallback(kind);
        }
    }

    public static Theme Fallback(ScreenKind kind) => kind switch
    {
        ScreenKind.ProFlow => MareTheme.Create(960, 480),
        ScreenKind.Thermalright => MareTheme.Create(1920, 462),
        _ => BuiltInTheme.Create(),
    };

    /// <summary>Temas desenhados em código, na ordem em que aparecem no menu.</summary>
    private static (string Name, Func<AppConfig, Theme> Create)[] BuiltIns(ScreenKind kind) => kind switch
    {
        ScreenKind.ProFlow => new (string, Func<AppConfig, Theme>)[]
        {
            (GifTheme.Name, c => GifTheme.Create(c.ProFlow.Gif, c.ProFlow.Dados, 960, 480)),
            (OnePieceTheme.Name, c => OnePieceTheme.Create(c.ProFlow.Dados, 960, 480)),
            (DeepSeaTheme.Name, c => DeepSeaTheme.Create(c.ProFlow.Dados, 960, 480)),
            (MareTheme.Name, _ => MareTheme.Create(960, 480)),
        },
        ScreenKind.Thermalright => new (string, Func<AppConfig, Theme>)[]
        {
            (GrandLineTheme.Name, _ => GrandLineTheme.Create(1920, 462)),
            (MareTheme.Name, _ => MareTheme.Create(1920, 462)),
            (VideoTheme.Name, c => VideoTheme.Create(c.Thermalright.Video, c.Thermalright.Dados, 1920, 462)),
            (OnePieceTheme.Name, c => OnePieceTheme.Create(c.Thermalright.Dados, 1920, 462)),
            (GifTheme.Name, c => GifTheme.Create(c.Thermalright.Gif, c.Thermalright.Dados, 1920, 462)),
        },
        _ => new (string, Func<AppConfig, Theme>)[]
        {
            (MareTheme.Name, _ => MareTheme.Create(960, 376)),
            (AuroraTheme.Name, _ => AuroraTheme.Create()),
            (BuiltInTheme.Name, _ => BuiltInTheme.Create(BuiltInTheme.Name)),
            (BuiltInTheme.LightName, _ => BuiltInTheme.Create(BuiltInTheme.LightName)),
        },
    };

    private static IEnumerable<string> JonsboRoots()
    {
        yield return LocalThemesDirectory;
        yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JONSBO-AIO", "Programme");
    }
}
