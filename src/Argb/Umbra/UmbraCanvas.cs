using DS339Monitor.Argb.Umbra;
using DS339Monitor.Lighting;

namespace DS339Monitor.Argb;

/// <summary>
/// O "modo tela" (幕布模式) do UMBRA no gabinete: o software desenha um efeito numa tela de 600×600 e cada LED pega o
/// pixel que está embaixo dele. Lá, as peças são ícones arrastados na tela; aqui, a tela é a foto do gabinete (o recorte
/// <see cref="CaseLayout.View"/>) e cada LED fica na posição dele na foto. Como no original, sem correção de gama: o
/// valor do pixel vai direto para o LED, vezes o brilho do modo tela (1–10).
/// </summary>
internal sealed class UmbraCanvas
{
    private const int Size = UmbraCanvasEffect.OriginalCanvasSize;
    private readonly float[,,] _canvas = new float[Size, Size, 3];
    private UmbraCanvasEffect? _effect;
    private string? _key;
    private string _applied = "";
    private double _lastT = -1;

    public void Render(UmbraCanvasSettings s, List<ArgbZone> zones, float[][] target, double t, double master, Func<ArgbZone, LedPoint[]?> points)
    {
        if (_effect is null || _key != s.Efeito)
        {
            _effect = UmbraEffectCatalog.Create(s.Efeito, 7);
            _key = s.Efeito;
            _applied = "";
            _lastT = -1;
        }
        if (_effect is null) return;
        Apply(s);
        double dt = _lastT < 0 ? 0 : Math.Clamp(t - _lastT, 0, 0.5);
        _lastT = t;
        _effect.Render(_canvas, t, dt);

        int[] view = CaseLayout.View;
        double sx = Size / (double)(view[2] - view[0]), sy = Size / (double)(view[3] - view[1]);
        double gain = master * s.Brilho / 10.0;
        foreach (var zone in zones)
        {
            var leds = points(zone);
            if (leds is null) continue;
            var buffer = target[zone.Canal - 1];
            double k = gain * zone.Brilho / 100.0;
            for (int i = 0; i < leds.Length && i < zone.Leds; i++)
            {
                int x = (int)((leds[i].X - view[0]) * sx), y = (int)((leds[i].Y - view[1]) * sy);
                if (x < 0 || y < 0 || x >= Size || y >= Size) continue;
                int at = (zone.Inicio + i) * 3;
                buffer[at] = (float)(_canvas[y, x, 0] * k);
                buffer[at + 1] = (float)(_canvas[y, x, 1] * k);
                buffer[at + 2] = (float)(_canvas[y, x, 2] * k);
            }
        }
    }

    /// <summary>Velocidade e cores escolhidas no painel para o efeito (só quando mudam).</summary>
    private void Apply(UmbraCanvasSettings s)
    {
        string signature = $"{s.SpeedOf(s.Efeito)}|{string.Join(',', s.ColorsOf(s.Efeito) ?? Array.Empty<string>())}";
        if (signature == _applied || _effect is null) return;
        _applied = signature;
        _effect.Speed = s.SpeedOf(s.Efeito);
        if (s.ColorsOf(s.Efeito) is not { } hex) return;
        var colors = hex.Select(h => GpuLighting.ParseColor(h)).Select(c => new Rgb(c.R, c.G, c.B)).ToArray();
        var property = _effect.GetType().GetProperty("Colors");
        if (property?.PropertyType == typeof(Rgb[]) && property.GetValue(_effect) is Rgb[] current && current.Length == colors.Length)
            property.SetValue(_effect, colors);
        else if (_effect.GetType().GetProperty("Color") is { } single && single.PropertyType == typeof(Rgb) && colors.Length == 1)
            single.SetValue(_effect, colors[0]);
    }

    private static readonly Dictionary<string, (int Speed, string[]? Colors)> Defaults = new();

    /// <summary>A velocidade e as cores com que o efeito começa no original (calculadas uma vez por efeito).</summary>
    private static (int Speed, string[]? Colors) DefaultsOf(string key)
    {
        lock (Defaults)
        {
            if (Defaults.TryGetValue(key, out var known)) return known;
            var effect = UmbraEffectCatalog.Create(key, 1);
            string[]? colors = effect?.GetType().GetProperty("Colors")?.GetValue(effect) is Rgb[] many ? many.Select(Hex).ToArray()
                : effect?.GetType().GetProperty("Color")?.GetValue(effect) is Rgb one ? new[] { Hex(one) }
                : null;
            return Defaults[key] = (effect?.Speed ?? 5, colors);
        }
    }

    /// <summary>As cores que o efeito tem (as do original), para o painel; null = o efeito não tem cores ajustáveis.</summary>
    public static string[]? DefaultColors(string key) => DefaultsOf(key).Colors;

    public static int DefaultSpeed(string key) => DefaultsOf(key).Speed;

    private static string Hex(Rgb c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";
}

/// <summary>O modo tela: o efeito, o brilho (1–10, como no software) e, por efeito, a velocidade e as cores escolhidas.</summary>
internal sealed class UmbraCanvasSettings
{
    public string Efeito { get; set; } = "rainbow";
    public int Brilho { get; set; } = 10;
    public Dictionary<string, int> Velocidades { get; set; } = new();
    public Dictionary<string, string[]> Cores { get; set; } = new();

    /// <summary>Os efeitos do modo tela que existem aqui (na ordem do software), com o nome em português.</summary>
    public static readonly (string Key, string Nome)[] Effects =
    {
        ("rainbow", "Arco-íris em fluxo"),
        ("fourth_dimension", "Quarta dimensão"),
        ("radar", "Radar"),
        ("matrix_rain", "Matrix"),
        ("fire", "Chamas"),
        ("aurora", "Aurora"),
        ("riptide_wave", "Correnteza"),
        ("meteor", "Chuva de meteoros"),
        ("storm", "Tempestade"),
        ("hyperspace", "Hiperespaço"),
        ("jellyfish", "Águas-vivas"),
        ("lava_lamp", "Lâmpada de lava"),
    };

    public void Normalize()
    {
        if (Effects.All(e => e.Key != Efeito)) Efeito = Effects[0].Key;
        Brilho = Math.Clamp(Brilho, 1, 10);
        Velocidades ??= new Dictionary<string, int>();
        Cores ??= new Dictionary<string, string[]>();
        foreach (string key in Velocidades.Keys.ToList())
            Velocidades[key] = Math.Clamp(Velocidades[key], 1, MaxSpeed(key));
        foreach (string key in Cores.Keys.ToList())
            Cores[key] = Cores[key].Select(c => GpuLighting.ToHex(GpuLighting.ParseColor(c))).ToArray();
    }

    public int SpeedOf(string key) => Velocidades.TryGetValue(key, out int v) ? v : UmbraCanvas.DefaultSpeed(key);

    public string[]? ColorsOf(string key) => Cores.TryGetValue(key, out var c) ? c : null;

    /// <summary>O deslizante do software vai de 1 a 10; a correnteza já começa em 30.</summary>
    public static int MaxSpeed(string key) => Math.Max(10, UmbraCanvas.DefaultSpeed(key));
}
