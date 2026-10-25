using DS339Monitor.Lighting;

namespace DS339Monitor.Argb;

/// <summary>
/// Fans e fitas ARGB ligados à controladora Nollie 16CH. Desligado (padrão) = o programa não toca na controladora e o
/// SignalRGB continua como sempre.
/// </summary>
internal sealed class ArgbSettings
{
    /// <summary>O programa controla a Nollie (no lugar do SignalRGB). Com o SignalRGB aberto, espera ele fechar.</summary>
    public bool Ligado { get; set; }

    /// <summary>Efeito geral (o das peças que estão em "Igual ao geral").</summary>
    public string Efeito { get; set; } = ArgbEffects.Wave;

    /// <summary>Cor do efeito geral (#RRGGBB); branco por padrão.</summary>
    public string Cor { get; set; } = "#FFFFFF";

    /// <summary>Brilho geral, 5–100%.</summary>
    public int Brilho { get; set; } = 60;

    /// <summary>Lenta, Normal ou Rápida.</summary>
    public string Velocidade { get; set; } = GpuEffects.Normal;

    /// <summary>
    /// Teto estimado de corrente de todos os LEDs juntos, em ampères (4–12). A Nollie tem fusível de 15 A e divide a
    /// linha de 5 V da fonte com SSDs e USB; acima do teto o brilho de tudo baixa por igual, como no WLED.
    /// </summary>
    public int LimiteAmperes { get; set; } = 8;

    /// <summary>As peças: trechos de LEDs de um canal (importados do SignalRGB na primeira vez).</summary>
    public List<ArgbZone> Zonas { get; set; } = new();

    /// <summary>
    /// O que o usuário quer guardado na própria controladora (efeito de boot e efeito sem programa). É um rascunho: só vai
    /// para a Nollie quando ele manda gravar. Null = ainda não lido da controladora.
    /// </summary>
    public NollieStoredSettings? NaControladora { get; set; }

    /// <summary>O Modo Umbra (a cópia do software do hub Asiahorse UMBRA): ligado, toma o lugar dos efeitos acima.</summary>
    public UmbraSettings Umbra { get; set; } = new();

    public void Normalize()
    {
        Efeito = ArgbEffects.All.FirstOrDefault(e => e.Equals(Efeito, StringComparison.OrdinalIgnoreCase)) ?? ArgbEffects.Wave;
        Cor = GpuLighting.ToHex(GpuLighting.ParseColor(Cor));
        Brilho = Math.Clamp(Brilho, 5, 100);
        Velocidade = GpuEffects.Speeds.FirstOrDefault(v => v.Equals(Velocidade, StringComparison.OrdinalIgnoreCase)) ?? GpuEffects.Normal;
        LimiteAmperes = Math.Clamp(LimiteAmperes, 4, 12);
        Zonas ??= new List<ArgbZone>();
        Zonas.RemoveAll(z => z is null || z.Canal is < 1 or > NollieDevice.Channels || z.Leds <= 0 || z.Inicio < 0 ||
            z.Inicio + z.Leds > NollieDevice.MaxLedsPerChannel);
        foreach (var zone in Zonas)
        {
            zone.Efeito = ArgbEffects.ForZone.FirstOrDefault(e => e.Equals(zone.Efeito, StringComparison.OrdinalIgnoreCase)) ?? ArgbEffects.Global;
            zone.Cor = GpuLighting.ToHex(GpuLighting.ParseColor(zone.Cor));
            zone.Brilho = Math.Clamp(zone.Brilho, 0, 200);
            zone.Balanco = WhiteBalance.Clean(zone.Balanco);
            if (string.IsNullOrWhiteSpace(zone.Nome)) zone.Nome = $"LEDs {zone.Inicio + 1}–{zone.Inicio + zone.Leds}";
            if (string.IsNullOrWhiteSpace(zone.Grupo)) zone.Grupo = $"Canal {zone.Canal}";
        }
        NaControladora?.Normalize();
        Umbra ??= new UmbraSettings();
        Umbra.Normalize(Zonas.Select(z => z.Grupo).Distinct());
    }
}

/// <summary>Um efeito da própria controladora: índice na lista dela, duas cores, brilho, velocidade e tamanho.</summary>
internal sealed class NollieEffectSettings
{
    public int Efeito { get; set; }
    public string Cor1 { get; set; } = "#FFFFFF";
    public string Cor2 { get; set; } = "#000000";

    /// <summary>0–100.</summary>
    public int Brilho { get; set; } = 100;

    /// <summary>1–10.</summary>
    public int Velocidade { get; set; } = 5;

    /// <summary>0–50.</summary>
    public int Tamanho { get; set; } = 1;

    public NollieEffectSettings Copy() => (NollieEffectSettings)MemberwiseClone();

    public void Normalize(bool boot)
    {
        int count = boot ? NollieConfig.BootEffects.Length : NollieConfig.LoopEffects.Length;
        Efeito = Math.Clamp(Efeito, 0, count - 1);
        Cor1 = GpuLighting.ToHex(GpuLighting.ParseColor(Cor1));
        Cor2 = GpuLighting.ToHex(GpuLighting.ParseColor(Cor2));
        Brilho = Math.Clamp(Brilho, 0, 100);
        Velocidade = Math.Clamp(Velocidade, 1, 10);
        Tamanho = Math.Clamp(Tamanho, 0, 50);
    }

    public bool SameAs(NollieEffectSettings other) =>
        Efeito == other.Efeito && Cor1 == other.Cor1 && Cor2 == other.Cor2 && Brilho == other.Brilho && Velocidade == other.Velocidade && Tamanho == other.Tamanho;
}

/// <summary>Rascunho do que fica guardado na controladora (ver <see cref="NollieConfig"/>).</summary>
internal sealed class NollieStoredSettings
{
    /// <summary>Toca uma vez quando o PC liga (antes de qualquer programa).</summary>
    public NollieEffectSettings Boot { get; set; } = new() { Efeito = 1 };

    /// <summary>Roda depois do boot até o programa abrir, e depois que ele fecha.</summary>
    public NollieEffectSettings SemPrograma { get; set; } = new() { Efeito = 2, Brilho = 30 };

    /// <summary>Segundos sem cores do programa até a controladora voltar ao efeito dela (0 = padrão dela, 65535 = nunca).</summary>
    public int VoltaSegundos { get; set; }

    /// <summary>
    /// Animação do programa tocada ao ligar (gravada na memória da controladora, ver <see cref="NollieFseq"/>) no lugar
    /// do efeito de boot dela. Null = o efeito de boot da controladora.
    /// </summary>
    public string? BootAnimacao { get; set; }

    /// <summary>
    /// Minutos do loop dos fans (o efeito configurado) emendados depois da animação de boot, tocados pela própria
    /// controladora até o programa abrir (0 = depois da animação vem o efeito sem programa). 1–5.
    /// </summary>
    public int ContinuarMinutos { get; set; } = 3;

    /// <summary>Cores da animação gravadas na ordem G, R, B (a dos LEDs). Trocar se as cores saírem trocadas.</summary>
    public bool CoresGrb { get; set; } = true;

    /// <summary>A animação que está gravada na controladora ("nome|CRC"), conferida depois de gravar.</summary>
    public string? AnimacaoGravada { get; set; }

    public void Normalize()
    {
        Boot ??= new NollieEffectSettings();
        SemPrograma ??= new NollieEffectSettings();
        Boot.Normalize(boot: true);
        SemPrograma.Normalize(boot: false);
        if (BootAnimacao is not null && !BootAnimation.All.Contains(BootAnimacao)) BootAnimacao = null;
        ContinuarMinutos = ContinuarMinutos is 0 or 1 or 2 or 3 or 5 ? ContinuarMinutos : 3;
        VoltaSegundos = Math.Clamp(VoltaSegundos, 0, 65535);
    }
}

/// <summary>Uma peça (ou parte de uma peça): um trecho de LEDs seguidos num canal da Nollie.</summary>
internal sealed class ArgbZone
{
    /// <summary>Nome da parte (ex.: "Centro", "Lateral 1").</summary>
    public string Nome { get; set; } = "";

    /// <summary>A peça a que a parte pertence (ex.: "Fans 360 · 1"); as partes de um mesmo grupo aparecem juntas.</summary>
    public string Grupo { get; set; } = "";

    /// <summary>Canal da Nollie, 1–16 (o "Channel 01..16" do SignalRGB).</summary>
    public int Canal { get; set; }

    /// <summary>Primeiro LED da parte dentro do canal (0 = o primeiro).</summary>
    public int Inicio { get; set; }

    public int Leds { get; set; }

    /// <summary>"Igual ao geral", "Apagado" ou um efeito próprio.</summary>
    public string Efeito { get; set; } = ArgbEffects.Global;

    /// <summary>Cor do efeito próprio (#RRGGBB).</summary>
    public string Cor { get; set; } = "#FFFFFF";

    /// <summary>Brilho da parte em relação ao geral, 0–200% (a bomba, que é fraca, pode passar de 100%).</summary>
    public int Brilho { get; set; } = 100;

    /// <summary>Animações correm no sentido contrário (e, nas cenas do gabinete, o primeiro LED vira o último).</summary>
    public bool Inverter { get; set; }

    /// <summary>Balanço de branco da parte: quanto de vermelho, verde e azul (0–100%) sai nos LEDs dela. Null = 100% os três.</summary>
    public int[]? Balanco { get; set; }

    /// <summary>
    /// Onde a parte está no gabinete, em pixels da foto (ver <see cref="CaseLayout"/>): "linha x0,y0 x1,y1",
    /// "anel rx,ry x,y", "aneis rx,ry x1,y1 x2,y2 …" ou "matriz x0,y0 x1,y1 FxL h|v". Vazio = fora das cenas do gabinete.
    /// </summary>
    public string? Forma { get; set; }
}

internal static class ArgbEffects
{
    public const string Global = "Igual ao geral";
    public const string Off = "Apagado";
    public const string Fixed = "Cor fixa";
    public const string GpuSync = "Igual à placa de vídeo";
    public const string Breathing = "Respiração";
    public const string Wave = "Onda";
    public const string Comet = "Cometa";
    public const string Twinkle = "Cintilar";
    public const string Rainbow = "Arco-íris";
    public const string Aurora = "Aurora";

    // Cenas do gabinete: usam a posição de cada LED (ver CaseScenes).
    public const string KnockUp = "Knock Up Stream";
    public const string Haki = "Haki do Rei";
    public const string Aquarium = "Aquário";
    public const string Airflow = "Fluxo de ar";
    public const string Rain = "Chuva";
    public const string Beacon = "Farol";
    public const string Drums = "Tambores da Libertação";
    public const string HeatMap = "Mapa de calor";
    public const string MangaScene = "Mangá no gabinete";

    // Capas de cabo, no estilo dos Strimer da Lian Li: usam as fileiras da grade (ver CableEffects).
    public const string Runway = "Pista";
    public const string Meteors = "Meteoros";
    public const string Current = "Corrente elétrica";
    public const string Stack = "Empilhar";
    public const string PingPong = "Pingue-pongue";
    public const string Staggered = "Contramão";
    public const string Snake = "Serpente";
    public const string Cover = "Cobrir";
    public const string Kaleidoscope = "Caleidoscópio";
    public const string Diagonal = "Arco-íris diagonal";

    /// <summary>Cenas que atravessam o gabinete inteiro.</summary>
    public static readonly string[] Scenes = { MangaScene, KnockUp, Haki, Aquarium, Airflow, Drums, Rain, Beacon, HeatMap };

    /// <summary>Efeitos por parte (cada parte anima sozinha, ao longo dos próprios LEDs).</summary>
    public static readonly string[] Main = { Fixed, Wave, Breathing, Comet, Twinkle, GpuSync, Aurora, Rainbow, Off };

    /// <summary>Feitos para as capas de cabo (fileiras × LEDs); em outras partes, uma fileira só.</summary>
    public static readonly string[] Cable = { Runway, Meteors, Current, Stack, PingPong, Staggered, Snake, Cover, Kaleidoscope, Diagonal };

    public static readonly string[] All = Scenes.Concat(Main).Concat(Cable).ToArray();

    /// <summary>O que uma parte pode escolher.</summary>
    public static readonly string[] ForZone = new[] { Global }.Concat(All).ToArray();

    public static bool IsScene(string effect) => Scenes.Contains(effect);

    public static bool IsCable(string effect) => Cable.Contains(effect);

    public static bool UsesColor(string effect) => effect is Fixed or Breathing or Wave or Comet or Twinkle
        or KnockUp or Haki or Aquarium or Airflow or Rain or Beacon or Drums or MangaScene
        or Runway or Meteors or Current or Stack or PingPong or Staggered or Snake or Cover;

    public static bool UsesSpeed(string effect) => effect is Breathing or Wave or Comet or Twinkle or Rainbow or Aurora
        or KnockUp or Haki or Aquarium or Airflow or Rain or Beacon or Drums || IsCable(effect);

    public static bool Animated(string effect) => effect is not (Fixed or Off);

    /// <summary>Sensores de que o efeito precisa.</summary>
    public static IEnumerable<string> SensorKeys(string effect) => effect switch
    {
        HeatMap => new[] { "CPUTemp", "GPUTemp" },
        Airflow => new[] { "GpuUsage" },
        _ => Array.Empty<string>(),
    };

    public static readonly Dictionary<string, string> Descriptions = new()
    {
        [Global] = "Segue o efeito geral",
        [Fixed] = "A cor escolhida, parada",
        [Wave] = "Uma faixa de luz correndo sobre a cor",
        [Breathing] = "Acende e apaga devagar",
        [Comet] = "Um ponto de luz com rastro dando a volta",
        [Twinkle] = "Cada LED cintila sozinho, como estrelas",
        [GpuSync] = "Acompanha o LED da placa de vídeo, efeito e tudo",
        [Aurora] = "Branco, verde-água e violeta fluindo",
        [Rainbow] = "As cores do arco-íris correndo",
        [Off] = "Apagado",
        [KnockUp] = "A corrente sobe do piso até o topo e cai de volta em gotas",
        [Haki] = "A bomba junta força e solta ondas de choque pelo gabinete",
        [Aquarium] = "O gabinete enche como um aquário: luz de piscina e bolhas",
        [Airflow] = "O caminho do ar: entra pelo piso e pelo radiador, sai pelo fan de trás",
        [Rain] = "Gotas escorrendo pelas laterais e respingando no piso",
        [Beacon] = "Um facho girando em volta da bomba",
        [Drums] = "DON do-tto-to: cada batida sobe do piso",
        [HeatMap] = "CPU e placa esquentam o ar em volta; o calor sobe",
        [MangaScene] = "A página da Pro Flow e os momentos do Modo Mangá no gabinete todo",
        [Runway] = "Luzes de pista correndo das duas pontas para o meio, em seta",
        [Meteors] = "Cada fileira solta meteoros na sua hora: cabeça branca, rastro na cor",
        [Current] = "Pacotes de energia disparando pelo cabo, com faíscas e arcos",
        [Stack] = "Blocos caem e empilham até encher o cabo; aí escoam",
        [PingPong] = "Uma barra vai e volta batendo nas pontas, trocando de cor",
        [Staggered] = "Fileiras alternadas correndo em sentidos opostos",
        [Snake] = "Duas cobras percorrendo a grade em zigue-zague",
        [Cover] = "A cor cobre o cabo fileira atrás de fileira; o escuro cobre de volta",
        [Kaleidoscope] = "Arco-íris espelhado a partir do meio, pulsando em anéis",
        [Diagonal] = "O arco-íris correndo em diagonal pela grade",
    };
}
