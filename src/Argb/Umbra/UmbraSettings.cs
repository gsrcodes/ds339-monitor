using System.Text.Json;
using DS339Monitor.Lighting;

namespace DS339Monitor.Argb;

/// <summary>
/// O "Modo Umbra": o software do hub Asiahorse UMBRA (Sync_light 3.5.5, feito pela Robobloq) copiado para a Nollie, a
/// pedido do usuário (uso pessoal, 2026-10-25). Os mesmos modos — sincronizado, independente (por porta, com as zonas de
/// cada fan: centro, laterais em paralelo, em série, esquerda, direita, tudo), combinado (todas as portas como uma fita
/// só) e o modo tela —, os mesmos 15 efeitos com os mesmos controles (6 níveis de velocidade e de brilho, direção, até 4
/// cores) e as predefinições do modo independente. Fica à parte dos efeitos de sempre: ligado, toma o lugar deles;
/// desligado, nada muda.
/// <para>
/// No hub, os 15 efeitos são desenhados pelo chip dele — o software só manda número, velocidade, direção, brilho e cores —,
/// então aqui eles foram refeitos pelo nome e pelos parâmetros (ver <see cref="UmbraEffects"/>). As regras (que controle
/// cada efeito mostra, os valores, os padrões, as zonas) são as do código do software.
/// </para>
/// </summary>
internal sealed class UmbraSettings
{
    /// <summary>O Modo Umbra no lugar dos efeitos de sempre dos fans e fitas.</summary>
    public bool Ligado { get; set; }

    /// <summary>Aba de efeito ativa: Sincronizado, Independente, Combinado ou Tela.</summary>
    public string Modo { get; set; } = UmbraModes.Sync;

    /// <summary>O "开灯/关灯" do software: false = tudo apagado (com o Modo Umbra ligado).</summary>
    public bool LuzesAcesas { get; set; } = true;

    /// <summary>Efeito sincronizado: o mesmo efeito em todas as peças, cada uma com a sua cópia.</summary>
    public UmbraSlot Sincronizado { get; set; } = new() { Efeito = 1 };

    /// <summary>Efeito combinado: todas as portas como uma fita só. <see cref="UmbraSlot.Efeito"/> = índice 0–9 da lista do combinado.</summary>
    public UmbraSlot Combinado { get; set; } = new() { Efeito = 0, Velocidade = 2 };

    /// <summary>Efeito independente: uma "porta" por peça (os grupos da página Fans e fitas).</summary>
    public List<UmbraPort> Portas { get; set; } = new();

    /// <summary>A porta escolhida na aba Independente.</summary>
    public string? Porta { get; set; }

    /// <summary>O interruptor "同步" da aba Independente: mexer num fan muda todos os fans juntos.</summary>
    public bool FansJuntos { get; set; }

    /// <summary>As predefinições salvas da aba Independente (até 9, como no software).</summary>
    public List<UmbraPreset> Predefinicoes { get; set; } = new();

    public UmbraCanvasSettings Tela { get; set; } = new();

    public const int MaxPresets = 9;

    public void Normalize(IEnumerable<string> groups)
    {
        if (!UmbraModes.All.Contains(Modo)) Modo = UmbraModes.Sync;
        Sincronizado ??= new UmbraSlot { Efeito = 1 };
        Sincronizado.Normalize(UmbraEffects.Fan.Length);
        Combinado ??= new UmbraSlot { Efeito = 0, Velocidade = 2 };
        Combinado.Normalize(UmbraEffects.MergeIds.Length);
        Portas ??= new List<UmbraPort>();
        Portas.RemoveAll(p => p is null || string.IsNullOrWhiteSpace(p.Grupo));
        foreach (string group in groups)
            if (Portas.All(p => p.Grupo != group)) Portas.Add(new UmbraPort { Grupo = group });
        foreach (var port in Portas) port.Normalize();
        if (Porta is null || Portas.All(p => p.Grupo != Porta)) Porta = Portas.FirstOrDefault()?.Grupo;
        Predefinicoes ??= new List<UmbraPreset>();
        Predefinicoes.RemoveAll(p => p is null);
        if (Predefinicoes.Count > MaxPresets) Predefinicoes.RemoveRange(MaxPresets, Predefinicoes.Count - MaxPresets);
        foreach (var preset in Predefinicoes)
        {
            preset.Portas ??= new List<UmbraPort>();
            foreach (var port in preset.Portas) port.Normalize();
        }
        Tela ??= new UmbraCanvasSettings();
        Tela.Normalize();
    }

    public UmbraPort? SelectedPort => Portas.FirstOrDefault(p => p.Grupo == Porta);

    /// <summary>O que a aba ativa está editando (null = modo tela).</summary>
    public UmbraSlot? CurrentSlot => Modo switch
    {
        UmbraModes.Sync => Sincronizado,
        UmbraModes.Merge => Combinado,
        UmbraModes.Async => SelectedPort?.Current,
        _ => null,
    };
}

internal static class UmbraModes
{
    public const string Sync = "Sincronizado";
    public const string Async = "Independente";
    public const string Merge = "Combinado";
    public const string Canvas = "Tela";

    public static readonly string[] All = { Sync, Async, Merge, Canvas };
}

/// <summary>As zonas de um fan no modo independente (中心, 并联, 串联, 左侧, 右侧, 全部).</summary>
internal static class UmbraZones
{
    public const string Center = "Centro";
    public const string Parallel = "Paralelo";
    public const string Series = "Série";
    public const string Left = "Esquerda";
    public const string Right = "Direita";
    public const string All = "Tudo";

    public static readonly string[] Names = { Center, Parallel, Series, Left, Right, All };

    /// <summary>Arranjos: centro + laterais iguais, centro + laterais em série, centro + cada lateral, ou tudo junto.</summary>
    public const string LayoutParallel = "Paralelo";
    public const string LayoutSeries = "Série";
    public const string LayoutSides = "Laterais";
    public const string LayoutAll = "Tudo";

    /// <summary>O arranjo que escolher a zona deixa (como no software: o centro não muda o arranjo).</summary>
    public static string? LayoutFor(string zone) => zone switch
    {
        Parallel => LayoutParallel,
        Series => LayoutSeries,
        Left or Right => LayoutSides,
        All => LayoutAll,
        _ => null,
    };
}

/// <summary>
/// Um efeito com os parâmetros do software: número, velocidade 1 (mais rápida) … 6 (mais lenta), direção 1 = "正序"
/// (normal) / 0 = "倒序" (inverso), brilho em 6 níveis (50, 51, 102, 153, 204, 255) e 4 cores.
/// </summary>
internal sealed class UmbraSlot
{
    public int Efeito { get; set; } = 1;
    public int Velocidade { get; set; } = 3;
    public int Direcao { get; set; } = 1;
    public int Brilho { get; set; } = 153;
    public string[] Cores { get; set; } = DefaultColors();

    public static string[] DefaultColors() => new[] { "#FF0000", "#00FF00", "#0000FF", "#FFFF00" };

    public void Normalize(int effects)
    {
        Efeito = Math.Clamp(Efeito, 0, effects - 1);
        Velocidade = Math.Clamp(Velocidade, 1, 6);
        Direcao = Direcao == 0 ? 0 : 1;
        Brilho = Brilho <= 50 ? 50 : Math.Clamp((int)Math.Round(Brilho / 51.0) * 51, 51, 255);
        var colors = Cores is { Length: > 0 } ? Cores : DefaultColors();
        var defaults = DefaultColors();
        Cores = Enumerable.Range(0, 4).Select(i => GpuLighting.ToHex(GpuLighting.ParseColor(i < colors.Length ? colors[i] : defaults[i]))).ToArray();
    }

    public UmbraSlot Clone() => new() { Efeito = Efeito, Velocidade = Velocidade, Direcao = Direcao, Brilho = Brilho, Cores = (string[])Cores.Clone() };
}

/// <summary>Uma porta do modo independente: uma peça, o arranjo das zonas e o efeito de cada zona.</summary>
internal sealed class UmbraPort
{
    public string Grupo { get; set; } = "";

    /// <summary>Paralelo, Série, Laterais ou Tudo (ver <see cref="UmbraZones"/>).</summary>
    public string Arranjo { get; set; } = UmbraZones.LayoutAll;

    /// <summary>A zona que está sendo editada.</summary>
    public string Zona { get; set; } = UmbraZones.All;

    /// <summary>Um efeito por zona (no software, cada zona é um "slot" inteiro: efeito, velocidade, direção, brilho e cores).</summary>
    public Dictionary<string, UmbraSlot> Zonas { get; set; } = new();

    public void Normalize()
    {
        if (Arranjo is not (UmbraZones.LayoutParallel or UmbraZones.LayoutSeries or UmbraZones.LayoutSides or UmbraZones.LayoutAll))
            Arranjo = UmbraZones.LayoutAll;
        if (!UmbraZones.Names.Contains(Zona)) Zona = UmbraZones.All;
        Zonas ??= new Dictionary<string, UmbraSlot>();
        foreach (string zone in UmbraZones.Names)
        {
            if (!Zonas.TryGetValue(zone, out var slot) || slot is null) Zonas[zone] = slot = new UmbraSlot();
            slot.Normalize(UmbraEffects.Fan.Length);
        }
        foreach (string key in Zonas.Keys.Where(k => !UmbraZones.Names.Contains(k)).ToList()) Zonas.Remove(key);
    }

    public UmbraSlot Current => Zonas[Zona];

    public UmbraSlot Slot(string zone) => Zonas[zone];

    public UmbraPort Clone() => JsonSerializer.Deserialize<UmbraPort>(JsonSerializer.Serialize(this))!;
}

/// <summary>Uma predefinição salva da aba Independente (o "保存灯效" do software): nome, descrição e as portas.</summary>
internal sealed class UmbraPreset
{
    public string Nome { get; set; } = "";
    public string Descricao { get; set; } = "";
    public List<UmbraPort> Portas { get; set; } = new();
}
