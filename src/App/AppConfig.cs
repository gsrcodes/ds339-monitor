using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using DS339Monitor.Argb;
using DS339Monitor.Lighting;
using DS339Monitor.Sensors;
using DS339Monitor.Themes;

namespace DS339Monitor.App;

/// <summary>config.json ao lado do executável. Pode ser editado à mão com o programa fechado.</summary>
internal sealed class AppConfig
{
    private static readonly ConfigJson Json = new(new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // mantém "Maré" legível
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    });

    /// <summary>De quanto em quanto tempo os sensores são lidos.</summary>
    public int IntervaloMs { get; set; } = 1000;

    /// <summary>Idioma do dia da semana.</summary>
    public string Idioma { get; set; } = "pt-BR";

    /// <summary>Trecho do nome da GPU a monitorar (vazio = escolhe a dedicada automaticamente).</summary>
    public string? Gpu { get; set; }

    public Ds339Settings DS339 { get; set; } = new();

    public ProFlowSettings ProFlow { get; set; } = new();

    public ThermalrightSettings Thermalright { get; set; } = new();

    public GpuLedSettings LedGpu { get; set; } = new();

    public SceneSettings Cena { get; set; } = new();

    public MangaSettings Manga { get; set; } = new();

    /// <summary>Fans e fitas ARGB da controladora Nollie (ver <see cref="ArgbLighting"/>).</summary>
    public ArgbSettings FansEFitas { get; set; } = new();

    /// <summary>Curva dos fans do gabinete pela placa-mãe (ver <see cref="FanCurve"/>).</summary>
    public FanSettings Ventoinhas { get; set; } = new();

    /// <summary>Modo jogo: painel de FPS, frametime e ping na Thermalright enquanto um jogo está aberto.</summary>
    public Game.GameSettings ModoJogo { get; set; } = new();

    /// <summary>
    /// Modo Grand Line ("Coluna do Mar"): a Thermalright no tema Grand Line (céu e superfície), a Pro Flow no Fundo do Mar
    /// (debaixo d'água) e o LED no efeito Grand Line (a luz das profundezas) — um mundo só, de cima a baixo do gabinete.
    /// </summary>
    [JsonIgnore]
    public bool GrandLine =>
        Thermalright.Tema == GrandLineTheme.Name && ProFlow.Tema == DeepSeaTheme.Name && LedGpu.Efeito == GpuEffects.GrandLine;

    /// <summary>A cena (sol, som, presença, acontecimentos) tem quem a use agora.</summary>
    [JsonIgnore]
    public bool UsesScene =>
        Manga.Ligado || Thermalright.Tema == GrandLineTheme.Name || ProFlow.Tema == DeepSeaTheme.Name || GpuEffects.UsesScene(LedGpu.Efeito);

    // Formato antigo (só havia a DS339): lido para migrar, nunca mais gravado.
    [JsonPropertyName("Tema")]
    public string? LegacyTema { get; set; }

    [JsonPropertyName("Rotacao")]
    public int? LegacyRotacao { get; set; }

    [JsonIgnore]
    public string FilePath { get; private set; } = "";

    public static AppConfig Load(string path)
    {
        AppConfig config;
        bool hadProFlow = false;
        try
        {
            if (File.Exists(path))
            {
                string text = File.ReadAllText(path);
                hadProFlow = text.Contains("\"ProFlow\"", StringComparison.Ordinal);
                config = JsonSerializer.Deserialize(text, Json.AppConfig) ?? new AppConfig();
            }
            else
            {
                config = new AppConfig();
            }
        }
        catch (Exception ex)
        {
            Log.Error("config.json inválido; usando padrões", ex);
            config = new AppConfig();
        }

        config.FilePath = path;
        bool changed = config.Migrate();
        if (!hadProFlow)
        {
            JungleLeopardImport.Apply(config.ProFlow);
            changed = true;
        }
        bool newCs2Code = string.IsNullOrWhiteSpace(config.ModoJogo?.CodigoCs2);
        config.Normalize();
        changed |= newCs2Code; // o código combinado com o CS2 tem que ficar salvo
        if (config.FansEFitas.Zonas.Count == 0)
        {
            // Primeira vez: as partes vêm de como a Nollie está montada no SignalRGB (só leitura do registro).
            try
            {
                config.FansEFitas.Zonas = SignalRgbImport.Read();
                config.FansEFitas.Normalize();
                changed |= config.FansEFitas.Zonas.Count > 0;
            }
            catch (Exception ex) when (ex is System.Security.SecurityException or UnauthorizedAccessException or IOException)
            {
                Log.Warn($"Fans e fitas: não consegui ler a configuração do SignalRGB ({ex.Message}).");
            }
        }
        changed |= CaseLayout.Apply(config.FansEFitas.Zonas); // nomes e lugares das peças deste gabinete
        if (changed || !File.Exists(path)) config.Save();
        return config;
    }

    public void Save()
    {
        try
        {
            File.WriteAllText(FilePath, JsonSerializer.Serialize(this, Json.AppConfig));
        }
        catch (Exception ex)
        {
            Log.Error("Não consegui salvar o config.json", ex);
        }
    }

    private bool Migrate()
    {
        if (LegacyTema is null && LegacyRotacao is null) return false;
        if (LegacyTema is not null) DS339.Tema = LegacyTema;
        if (LegacyRotacao is int rotation) DS339.Rotacao = rotation;
        LegacyTema = null;
        LegacyRotacao = null;
        Log.Info("config.json migrado para o formato com as duas telas.");
        return true;
    }

    private void Normalize()
    {
        IntervaloMs = Math.Clamp(IntervaloMs, 250, 10_000);
        if (string.IsNullOrWhiteSpace(Idioma)) Idioma = "pt-BR";
        DS339 ??= new Ds339Settings();
        ProFlow ??= new ProFlowSettings();
        Thermalright ??= new ThermalrightSettings();
        LedGpu ??= new GpuLedSettings();
        Cena ??= new SceneSettings();
        Manga ??= new MangaSettings();
        FansEFitas ??= new ArgbSettings();
        FansEFitas.Normalize();
        Ventoinhas ??= new FanSettings();
        Ventoinhas.Normalize();
        ModoJogo ??= new Game.GameSettings();
        ModoJogo.Normalize();
        if (string.IsNullOrWhiteSpace(Manga.GifGear5))
        {
            string nika = Path.Combine(AppContext.BaseDirectory, "media", "nika-luffy.gif");
            if (File.Exists(nika)) Manga.GifGear5 = nika;
        }
        Cena.Latitude = Math.Clamp(Cena.Latitude, -66, 66);
        Cena.Longitude = Math.Clamp(Cena.Longitude, -180, 180);

        if (DS339.Rotacao is not (90 or 270)) DS339.Rotacao = 90; // temas paisagem num painel retrato
        if (string.IsNullOrWhiteSpace(DS339.Tema)) DS339.Tema = MareTheme.Name;
        DS339.Brilho = Math.Clamp(DS339.Brilho, 5, 100);

        if (ProFlow.Rotacao is not (0 or 180)) ProFlow.Rotacao = 180;
        if (string.IsNullOrWhiteSpace(ProFlow.Tema)) ProFlow.Tema = MareTheme.Name;
        ProFlow.Brilho = Math.Clamp(ProFlow.Brilho, 5, 100);
        ProFlow.FpsMax = Math.Clamp(ProFlow.FpsMax, 1, 30);
        if (!GifTheme.Styles.Contains(ProFlow.Dados, StringComparer.OrdinalIgnoreCase)) ProFlow.Dados = GifTheme.StyleFull;

        if (Thermalright.Rotacao is not (0 or 180)) Thermalright.Rotacao = 180;
        if (string.IsNullOrWhiteSpace(Thermalright.Tema)) Thermalright.Tema = MareTheme.Name;
        Thermalright.Brilho = Math.Clamp(Thermalright.Brilho, 5, 100);
        Thermalright.FpsMax = Math.Clamp(Thermalright.FpsMax, 1, 30);
        if (!GifTheme.Styles.Contains(Thermalright.Dados, StringComparer.OrdinalIgnoreCase)) Thermalright.Dados = GifTheme.StyleNone;

        LedGpu.Efeito = GpuEffects.All.FirstOrDefault(e => e.Equals(LedGpu.Efeito, StringComparison.OrdinalIgnoreCase)) ?? GpuEffects.Off;
        LedGpu.Velocidade = GpuEffects.Speeds.FirstOrDefault(v => v.Equals(LedGpu.Velocidade, StringComparison.OrdinalIgnoreCase)) ?? GpuEffects.Normal;
        LedGpu.Cor = GpuLighting.ToHex(GpuLighting.ParseColor(LedGpu.Cor));
        LedGpu.Brilho = Math.Clamp(LedGpu.Brilho, 5, 100);
        LedGpu.Branco = GpuEffects.WhiteTones.FirstOrDefault(t => t.Equals(LedGpu.Branco, StringComparison.OrdinalIgnoreCase)) ?? GpuEffects.WhiteCard;
        LedGpu.Balanco = WhiteBalance.Clean(LedGpu.Balanco);
    }
}

internal sealed class Ds339Settings
{
    public string Tema { get; set; } = MareTheme.Name;

    /// <summary>90 = como o app da Jonsbo; 270 = tela montada de cabeça para baixo.</summary>
    public int Rotacao { get; set; } = 90;

    /// <summary>Brilho da tela, 5–100% (aplicado escurecendo a imagem).</summary>
    public int Brilho { get; set; } = 100;
}

/// <summary>Onde o sol do Modo Grand Line nasce e se põe (padrão: São Paulo).</summary>
internal sealed class SceneSettings
{
    public double Latitude { get; set; } = -23.55;

    public double Longitude { get; set; } = -46.63;

    /// <summary>Distância (mm) da borda de baixo da Thermalright até a borda de cima da Pro Flow.</summary>
    public float VaoTelasMm { get; set; } = 63;

    /// <summary>Distância (mm) da borda de baixo da Pro Flow até a luz da placa de vídeo.</summary>
    public float VaoPlacaMm { get; set; } = 90;

    /// <summary>Quanto o centro da Pro Flow fica à direita (+) ou à esquerda (−) do centro da Thermalright (mm).</summary>
    public float DeslocamentoProFlowMm { get; set; }

    /// <summary>O que estava escolhido antes de ligar o Modo Grand Line (volta ao desligar).</summary>
    public string? AntesThermalright { get; set; }

    public string? AntesProFlow { get; set; }

    public string? AntesLed { get; set; }
}

/// <summary>
/// Modo Mangá: efeitos de mangá por cima do vídeo, do GIF e dos painéis do One Piece, nas duas telas e no LED ao mesmo
/// tempo, ligados ao que acontece no mangá da Pro Flow e às ações do PC (ver <see cref="Manga.MangaStage"/>).
/// </summary>
internal sealed class MangaSettings
{
    public bool Ligado { get; set; }

    /// <summary>GIF do despertar do Gear 5, que a Thermalright mostra quando um jogo abre.</summary>
    public string? GifGear5 { get; set; }

    /// <summary>Página de título do capítulo na hora cheia (das 8h às 23h).</summary>
    public bool CapitulosNaHora { get; set; } = true;

    /// <summary>Encenar as ações do PC (print, download, copiar, pendrive, música, microfone, mudo).</summary>
    public bool AcoesDoPc { get; set; } = true;

    /// <summary>O que estava escolhido antes de ligar o Modo Mangá (volta ao desligar).</summary>
    public string? AntesThermalright { get; set; }

    public string? AntesProFlow { get; set; }

    public string? AntesLed { get; set; }
}

internal sealed class ThermalrightSettings
{
    public string Tema { get; set; } = MareTheme.Name;

    /// <summary>
    /// 0 ou 180. 180 é o normal desta tela: no submodelo dela o TRCC gira a imagem 180° na direção padrão (FormCZTV,
    /// ImageToJpg, 1920x462 com sub ≠ 2 e ≠ 4).
    /// </summary>
    public int Rotacao { get; set; } = 180;

    /// <summary>Brilho da tela, 5–100% (aplicado escurecendo a imagem, como no TRCC).</summary>
    public int Brilho { get; set; } = 100;

    /// <summary>Caminho do GIF do tema "GIF".</summary>
    public string? Gif { get; set; }

    /// <summary>Caminho do clipe (.zt) do tema "Vídeo" (convertido de MP4 ou vindo do TRCC, na pasta media/).</summary>
    public string? Video { get; set; }

    /// <summary>Dados sobre o vídeo, o GIF e o One Piece: Nenhum, Compacto ou Completo.</summary>
    public string Dados { get; set; } = GifTheme.StyleNone;

    /// <summary>Teto de quadros por segundo das animações.</summary>
    public int FpsMax { get; set; } = 30;
}

internal sealed class ProFlowSettings
{
    public string Tema { get; set; } = MareTheme.Name;

    /// <summary>0 ou 180. O padrão 180 é o que o Jungle Leopard usava nesta máquina.</summary>
    public int Rotacao { get; set; } = 180;

    /// <summary>Brilho da tela, 5–100%.</summary>
    public int Brilho { get; set; } = 80;

    /// <summary>Caminho do GIF do tema "GIF" (copiado para a pasta media/ do programa).</summary>
    public string? Gif { get; set; }

    /// <summary>Dados sobre o GIF: Nenhum, Compacto ou Completo.</summary>
    public string Dados { get; set; } = GifTheme.StyleFull;

    /// <summary>Teto de quadros por segundo das animações (o GIF pode pedir menos). 30 é o que o app original usa.</summary>
    public int FpsMax { get; set; } = 30;
}

/// <summary>LED da placa de vídeo (ASRock RX 6900 XT). Desligado = o programa não mexe no LED.</summary>
internal sealed class GpuLedSettings
{
    /// <summary>
    /// Desligado, Cor fixa, Respiração, Arco-íris, Aurora, Temperatura da GPU, Uso da GPU, Luz da página, Mangá narrado
    /// ou Tambores da Libertação.
    /// </summary>
    public string Efeito { get; set; } = GpuEffects.Off;

    /// <summary>Cor dos efeitos "Cor fixa" e "Respiração" (#RRGGBB).</summary>
    public string Cor { get; set; } = "#00A2FF";

    /// <summary>5–100%.</summary>
    public int Brilho { get; set; } = 100;

    /// <summary>Lenta, Normal ou Rápida (respiração, arco-íris, aurora, tambores).</summary>
    public string Velocidade { get; set; } = GpuEffects.Normal;

    /// <summary>Tom dos efeitos brancos: Igual ao da placa (o branco guardado nela pelo Polychrome), Neutro, Quente ou Frio.</summary>
    public string Branco { get; set; } = GpuEffects.WhiteCard;

    /// <summary>
    /// Balanço de branco do LED da placa: quanto de vermelho, verde e azul (0–100%) sai em tudo o que ele mostra. Null =
    /// sem balanço (o tom "Igual ao da placa" já faz esse papel até o usuário calibrar).
    /// </summary>
    public int[]? Balanco { get; set; }

    /// <summary>O tom de branco que valia antes de o balanço ser ajustado (volta a ele no "Voltar").</summary>
    public string? BrancoAntesDoBalanco { get; set; }
}

/// <summary>Na primeira vez, herda brilho e rotação que o usuário tinha no Jungle Leopard Display.</summary>
internal static class JungleLeopardImport
{
    public static void Apply(ProFlowSettings settings)
    {
        try
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Jungle_Leopard_Display", "config.json");
            if (!File.Exists(path)) return;
            using var root = JsonDocument.Parse(File.ReadAllText(path));
            // "deviceConfig" é um JSON dentro de uma string: { "<uid>": { "rotate": -540, "brightness": 33, ... } }
            if (!root.RootElement.TryGetProperty("deviceConfig", out var raw) || raw.GetString() is not string inner) return;
            using var devices = JsonDocument.Parse(inner);
            foreach (var device in devices.RootElement.EnumerateObject())
            {
                var d = device.Value;
                if (d.TryGetProperty("brightness", out var b) && b.TryGetInt32(out int brightness))
                    settings.Brilho = brightness;
                if (d.TryGetProperty("rotate", out var r) && r.TryGetInt32(out int rotate))
                {
                    int normalized = ((rotate % 360) + 360) % 360; // -540 → 180
                    if (normalized is 0 or 180) settings.Rotacao = normalized;
                }
                Log.Info($"Configuração do Jungle Leopard importada: brilho {settings.Brilho}%, rotação {settings.Rotacao}°.");
                return;
            }
        }
        catch (Exception ex)
        {
            Log.Warn($"Não consegui ler a configuração do Jungle Leopard: {ex.Message}");
        }
    }
}

[JsonSerializable(typeof(AppConfig))]
internal sealed partial class ConfigJson : JsonSerializerContext
{
}
