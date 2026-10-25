using System.Drawing;
using System.Text;
using System.Text.Json;
using DS339Monitor.Lighting;

namespace DS339Monitor.Argb;

/// <summary>
/// Um efeito guardado na controladora (um "canvas" do NollieRGB): um grupo de canais que roda um efeito com um conjunto
/// de parâmetros. Slots 0–13 = efeitos normais (rodam quando nenhum programa manda cores); 14–15 = boot (tocam uma vez
/// ao ligar o PC, antes dos normais).
/// </summary>
internal sealed class NollieCanvas
{
    public int Slot { get; set; }

    /// <summary>Índice do efeito: na lista dos normais (slots 0–13) ou na dos de boot (14–15).</summary>
    public int Effect { get; set; }

    /// <summary>0–100.</summary>
    public int Brightness { get; set; }

    /// <summary>Velocidade, 1–10.</summary>
    public int Step { get; set; }

    /// <summary>Tamanho da parte que anda (barra, rastro), 0–50.</summary>
    public int Size { get; set; }

    public int ChannelCount { get; set; }

    public int VmapProcess { get; set; }

    /// <summary>Atraso, 0–8.</summary>
    public int Delay { get; set; }

    public Color Color1 { get; set; }
    public Color Color2 { get; set; }
    public Color Color3 { get; set; }

    /// <summary>Canais físicos da controladora neste canvas, na ordem.</summary>
    public int[] Channels { get; set; } = Array.Empty<int>();

    public bool IsBoot => Slot >= NollieConfig.BootBase;

    public string EffectName => NollieConfig.EffectName(IsBoot, Effect);

    /// <summary>Os 16 bytes do "general config" (depois de FA 02 slot), na ordem do firmware.</summary>
    public byte[] ToBytes() => new[]
    {
        (byte)Effect, (byte)Brightness, (byte)Step, (byte)Size, (byte)ChannelCount, (byte)VmapProcess, (byte)Delay,
        Color1.R, Color1.G, Color1.B, Color2.R, Color2.G, Color2.B, Color3.R, Color3.G, Color3.B,
    };

    public static NollieCanvas FromBytes(int slot, ReadOnlySpan<byte> b) => new()
    {
        Slot = slot,
        Effect = b[0],
        Brightness = b[1],
        Step = b[2],
        Size = b[3],
        ChannelCount = b[4],
        VmapProcess = b[5],
        Delay = b[6],
        Color1 = Color.FromArgb(b[7], b[8], b[9]),
        Color2 = Color.FromArgb(b[10], b[11], b[12]),
        Color3 = Color.FromArgb(b[13], b[14], b[15]),
    };
}

/// <summary>
/// A configuração guardada na controladora Nollie 16CH (firmware 19), lida e gravada pelos mesmos comandos do app
/// oficial NollieRGB (relatório de saída de 1025 bytes; byte 1 = comando: F9 = ler efeito, FA = gravar efeito, FC = ler
/// versão, F5/F6 = ler/gravar núcleo). A controladora grava sozinha cada ajuste na memória flash dela — não existe
/// "salvar"; por isso o programa só grava o que mudou, quando o usuário pede.
/// <para>
/// Ao ligar: os canvases de boot tocam uma vez ("intro"), depois os normais ficam em loop ("steady"). Quando um
/// programa manda cores, ele assume na hora; parou de mandar, depois de <see cref="IdleSeconds"/> a controladora volta
/// sozinha aos efeitos normais.
/// </para>
/// </summary>
internal sealed class NollieConfig
{
    public const int Version19 = 19;
    public const int BootBase = 14;
    public const int LoopMax = 14;
    public const int BootMax = 2;

    /// <summary>Efeitos normais (índice = valor no firmware).</summary>
    public static readonly string[] LoopEffects =
    {
        "Apagado", "Cor sólida", "Respiração", "Varredura", "Arco-íris", "Arco-íris correndo", "Meteoro", "Balanço",
        "Varredura lenta", "Varredura de cor", "Faixa arco-íris", "Faixa arco-íris longa", "Onda", "Céu limpo", "Laço",
        "Empilhar 1", "Empilhar 2", "Empilhar 3", "Fusão 1", "Fusão 2", "Encontro", "Balas", "Sequência", "Paleta",
    };

    /// <summary>Efeitos de boot (índice = valor no firmware; outra tabela).</summary>
    public static readonly string[] BootEffects =
    {
        "Acender devagar", "Varredura de cor", "Varredura lenta", "Laço", "Empilhar 1", "Empilhar 2", "Empilhar 3", "Balas",
    };

    public static string EffectName(bool boot, int index)
    {
        var table = boot ? BootEffects : LoopEffects;
        return index >= 0 && index < table.Length ? table[index] : $"efeito {index}";
    }

    public int Version { get; set; }

    /// <summary>0 = recebendo cores de um programa, 1 = efeitos da controladora, 3 = FSEQ.</summary>
    public int Mode { get; set; }

    public int LoopCount { get; set; }
    public int BootCount { get; set; }

    /// <summary>Milissegundos por quadro dos efeitos normais e dos de boot.</summary>
    public int LoopFrameMs { get; set; }
    public int BootFrameMs { get; set; }

    /// <summary>Ao ligar: 0 = canvases de boot, 1 = nada, 2 = FSEQ uma vez.</summary>
    public int BootIntro { get; set; }

    /// <summary>Depois: 0 = efeitos normais, 1 = FSEQ em loop.</summary>
    public int BootSteady { get; set; }

    /// <summary>Segundos sem cores de um programa até voltar aos efeitos normais (0 = padrão do firmware, 65535 = nunca).</summary>
    public int IdleSeconds { get; set; }

    /// <summary>Quantas gravações na flash a controladora já fez.</summary>
    public long WriteCount { get; set; }

    public int[] LoopMap { get; set; } = new int[16];
    public int[] BootMap { get; set; } = new int[16];
    public int[] LedCount { get; set; } = new int[16];
    public int[] HiddenLedCount { get; set; } = new int[16];
    public int[] FrameDelay { get; set; } = new int[16];

    /// <summary>Os 16 slots (só os primeiros <see cref="LoopCount"/> normais e <see cref="BootCount"/> de boot valem).</summary>
    public NollieCanvas[] Slots { get; set; } = Array.Empty<NollieCanvas>();

    public IEnumerable<NollieCanvas> Loop => Slots.Where(s => s.Slot < LoopCount);

    public IEnumerable<NollieCanvas> Boot => Slots.Where(s => s.Slot >= BootBase && s.Slot < BootBase + BootCount);

    // ---------------------------------------------------------------- leitura (só perguntas, nada é gravado)

    public static NollieConfig? Read(NollieDevice device, out string error)
    {
        byte[]? Ask(params byte[] request) => device.Query(request);
        var config = new NollieConfig();
        error = "";

        var version = Ask(0xFC, 0x01);
        if (version is null)
        {
            error = "a controladora não respondeu";
            return null;
        }
        config.Version = version[1];
        if (config.Version != Version19)
        {
            error = $"firmware com protocolo {config.Version} (o programa conhece o 19)";
            return null;
        }

        int Byte(byte sub) => Ask(0xF9, sub) is { } r ? r[0] : -1;
        config.Mode = Byte(0x01);
        config.LoopCount = Byte(0x04);
        config.BootCount = Byte(0x05);
        config.LoopFrameMs = Byte(0x06);
        config.BootFrameMs = Byte(0x07);
        if (Ask(0xF9, 0x13) is { } playlist)
        {
            config.BootIntro = playlist[0];
            config.BootSteady = playlist[1];
        }
        if (Ask(0xF5, 0x16) is { } idle) config.IdleSeconds = idle[0] | idle[1] << 8;
        if (Ask(0xF5, 0x03) is { } count) config.WriteCount = BitConverter.ToUInt32(count, 0);
        if (Ask(0xF9, 0x03, 0x00) is { } loopMap) config.LoopMap = loopMap.Take(16).Select(b => (int)b).ToArray();
        if (Ask(0xF9, 0x0B, 0x00) is { } bootMap) config.BootMap = bootMap.Take(16).Select(b => (int)b).ToArray();
        if (Ask(0xF9, 0x08) is { } leds) config.LedCount = Words(leds);
        if (Ask(0xF9, 0x09) is { } hidden) config.HiddenLedCount = Words(hidden);
        if (Ask(0xF9, 0x12) is { } delays) config.FrameDelay = delays.Take(16).Select(b => (int)b).ToArray();

        var slots = new List<NollieCanvas>();
        for (int slot = 0; slot < 16; slot++)
        {
            if (Ask(0xF9, 0x02, (byte)slot) is not { } reply) continue;
            slots.Add(NollieCanvas.FromBytes(slot, reply.AsSpan(3, 16)));
        }
        config.Slots = slots.ToArray();
        config.AssignChannels();
        return config;
    }

    private static int[] Words(byte[] data)
    {
        var words = new int[16];
        for (int i = 0; i < 16; i++) words[i] = data[i * 2] | data[i * 2 + 1] << 8;
        return words;
    }

    /// <summary>Os canais de cada canvas: fatias seguidas do mapa, na ordem dos canvases.</summary>
    private void AssignChannels()
    {
        int at = 0;
        foreach (var canvas in Slots.Where(s => s.Slot < LoopCount).OrderBy(s => s.Slot))
        {
            canvas.Channels = LoopMap.Skip(at).Take(canvas.ChannelCount).ToArray();
            at += canvas.ChannelCount;
        }
        at = 0;
        foreach (var canvas in Slots.Where(s => s.Slot >= BootBase && s.Slot < BootBase + BootCount).OrderBy(s => s.Slot))
        {
            canvas.Channels = BootMap.Skip(at).Take(canvas.ChannelCount).ToArray();
            at += canvas.ChannelCount;
        }
    }

    /// <summary>Canal físico (0–15) → nome do canal como o SignalRGB e o programa contam ("Channel 1".."16").</summary>
    public static int LogicalChannel(int hardware)
    {
        int index = Array.IndexOf(NollieDevice.HardwareChannels, hardware);
        return index < 0 ? -1 : index + 1;
    }

    public string Describe()
    {
        var text = new StringBuilder();
        text.Append($"protocolo {Version}, modo {Mode}, {WriteCount} gravações na flash; boot: intro {BootIntro}, depois {BootSteady}; ");
        text.Append($"volta aos efeitos {IdleSeconds} s depois de o programa parar; quadro {LoopFrameMs} ms (boot {BootFrameMs} ms)\n");
        text.Append("  LEDs por canal (canal do programa = LEDs+ocultos): ");
        text.Append(string.Join(", ", Enumerable.Range(0, 16).Select(hw => (hw, n: LedCount[hw], z: HiddenLedCount[hw]))
            .Where(c => c.n + c.z > 0).Select(c => $"{LogicalChannel(c.hw)}={c.n}" + (c.z > 0 ? $"+{c.z}" : ""))));
        text.Append('\n');
        foreach (var canvas in Loop.Concat(Boot))
        {
            text.Append($"  {(canvas.IsBoot ? "boot" : "normal")} {canvas.Slot}: {canvas.EffectName} (#{canvas.Effect}), brilho {canvas.Brightness}, " +
                $"velocidade {canvas.Step}, tamanho {canvas.Size}, atraso {canvas.Delay}, vmap {canvas.VmapProcess}, " +
                $"cores {Hex(canvas.Color1)} {Hex(canvas.Color2)} {Hex(canvas.Color3)}, pausa {FrameDelay[canvas.Slot]}, " +
                $"canais {string.Join(" ", canvas.Channels.Select(LogicalChannel))}\n");
        }
        return text.ToString().TrimEnd();
    }

    private static string Hex(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

    // ---------------------------------------------------------------- gravação

    /// <summary>Um comando de gravação e o que ele muda (para o log e o aviso no painel).</summary>
    public sealed record Change(string What, byte[] Payload);

    /// <summary>
    /// O que mandar para a controladora ficar como <paramref name="wanted"/>: um canvas de boot (slot 14) e um normal
    /// (slot 0), cada um com os canais usados na ordem de <paramref name="channels"/> (do piso para o topo), e a
    /// quantidade real de LEDs desses canais. Só entra o que difere do que está gravado — cada comando é uma gravação
    /// na flash dela. Canais sem nada ligado não mudam.
    /// </summary>
    public List<Change> Plan(NollieStoredSettings wanted, IReadOnlyList<(int Logical, int Leds)> channels)
    {
        var changes = new List<Change>();
        int[] hardware = NollieDevice.HardwareChannels;
        int[] order = channels.Select(c => hardware[c.Logical - 1]).ToArray();

        // LEDs de cada canal: os efeitos da controladora andam por esse comprimento (256 = padrão de fábrica).
        int[] leds = (int[])LedCount.Clone();
        foreach (var (logical, count) in channels) leds[hardware[logical - 1]] = Math.Clamp(count, 1, NollieDevice.MaxLedsPerChannel);
        if (!leds.SequenceEqual(LedCount))
        {
            var payload = new byte[34];
            payload[0] = 0xFA;
            payload[1] = 0x08;
            for (int i = 0; i < 16; i++)
            {
                payload[2 + i * 2] = (byte)(leds[i] & 0xFF);
                payload[3 + i * 2] = (byte)(leds[i] >> 8);
            }
            changes.Add(new Change("LEDs de cada canal, do número de fábrica (256) para o real: " + string.Join(", ", channels.Select(c => $"canal {c.Logical} = {c.Leds}")), payload));
        }

        // O que toca ao ligar: os canvases de boot e depois os normais.
        int intro = wanted.BootAnimacao is null ? 0 : 2; // 0 = efeito de boot dela, 2 = a animação gravada (uma vez)
        if (BootIntro != intro || BootSteady != 0)
            changes.Add(new Change(intro == 2 ? $"ao ligar: a animação \"{wanted.BootAnimacao}\" e depois o efeito sem programa" : "ao ligar: o efeito de boot dela e depois o efeito sem programa",
                new byte[] { 0xFA, 0x13, (byte)intro, 0x00 }));
        if (BootCount != 1) changes.Add(new Change("um efeito de boot", new byte[] { 0xFA, 0x05, 0x01 }));
        if (LoopCount != 1) changes.Add(new Change("um efeito sem programa", new byte[] { 0xFA, 0x04, 0x01 }));

        // Mapas: os canais usados primeiro (na ordem do gabinete), o resto depois, em ordem — uma permutação de 0–15.
        int[] map = order.Concat(Enumerable.Range(0, 16).Except(order)).ToArray();
        if (!map.SequenceEqual(BootMap) || Slot(BootBase)?.ChannelCount != order.Length)
            changes.Add(new Change($"boot só nos {order.Length} canais com LEDs, do piso para o topo", Map(0x0B, map)));
        if (!map.SequenceEqual(LoopMap) || Slot(0)?.ChannelCount != order.Length)
            changes.Add(new Change($"efeito sem programa só nos {order.Length} canais com LEDs, do piso para o topo", Map(0x03, map)));

        AddCanvas(changes, BootBase, wanted.Boot, order.Length, "boot");
        AddCanvas(changes, 0, wanted.SemPrograma, order.Length, "efeito sem programa");

        if (wanted.VoltaSegundos != IdleSeconds)
            changes.Add(new Change($"voltar ao efeito {IdleText(wanted.VoltaSegundos)}",
                new byte[] { 0xF6, 0x16, (byte)(wanted.VoltaSegundos & 0xFF), (byte)(wanted.VoltaSegundos >> 8) }));
        return changes;
    }

    private void AddCanvas(List<Change> changes, int slot, NollieEffectSettings wanted, int channelCount, string label)
    {
        var current = Slot(slot);
        var color1 = GpuLighting.ParseColor(wanted.Cor1);
        var color2 = GpuLighting.ParseColor(wanted.Cor2);
        var canvas = new NollieCanvas
        {
            Slot = slot,
            Effect = wanted.Efeito,
            Brightness = wanted.Brilho,
            Step = wanted.Velocidade,
            Size = wanted.Tamanho,
            ChannelCount = channelCount,
            VmapProcess = current?.VmapProcess ?? 0,
            Delay = current?.Delay ?? 0,
            Color1 = color1,
            Color2 = color2,
            Color3 = current?.Color3 ?? Color.FromArgb(0, 255, 255),
        };
        byte[] bytes = canvas.ToBytes();
        if (current is not null && bytes.SequenceEqual(current.ToBytes())) return;
        changes.Add(new Change($"{label}: {canvas.EffectName}, brilho {canvas.Brightness}, velocidade {canvas.Step}",
            new byte[] { 0xFA, 0x02, (byte)slot }.Concat(bytes).ToArray()));
    }

    private static byte[] Map(byte sub, int[] map) => new byte[] { 0xFA, sub, 0x00 }.Concat(map.Select(m => (byte)m)).ToArray();

    public NollieCanvas? Slot(int slot) => Slots.FirstOrDefault(s => s.Slot == slot);

    public static string IdleText(int seconds) => seconds switch
    {
        0 => "no tempo padrão da controladora",
        65535 => "nunca",
        < 60 => $"depois de {seconds} s",
        _ => $"depois de {seconds / 60} min",
    };

    /// <summary>O rascunho a partir do que está gravado (para começar do que já existe).</summary>
    public NollieStoredSettings ToSettings()
    {
        static NollieEffectSettings From(NollieCanvas? c, NollieEffectSettings fallback) => c is null ? fallback : new NollieEffectSettings
        {
            Efeito = c.Effect,
            Cor1 = Hex(c.Color1),
            Cor2 = Hex(c.Color2),
            Brilho = c.Brightness,
            Velocidade = Math.Max(1, c.Step),
            Tamanho = c.Size,
        };
        var settings = new NollieStoredSettings
        {
            Boot = From(BootCount > 0 ? Slot(BootBase) : null, new NollieEffectSettings { Efeito = 1 }),
            SemPrograma = From(LoopCount > 0 ? Slot(0) : null, new NollieEffectSettings { Efeito = 2, Brilho = 30 }),
            VoltaSegundos = IdleSeconds,
        };
        settings.Normalize();
        return settings;
    }

    /// <summary>Cópia de tudo o que foi lido, para guardar antes de qualquer gravação.</summary>
    public string ToJson()
    {
        using var stream = new MemoryStream();
        using (var json = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
        {
            json.WriteStartObject();
            json.WriteString("lido", DateTime.Now.ToString("O"));
            json.WriteNumber("versao", Version);
            json.WriteNumber("modo", Mode);
            json.WriteNumber("canvasNormais", LoopCount);
            json.WriteNumber("canvasBoot", BootCount);
            json.WriteNumber("quadroMs", LoopFrameMs);
            json.WriteNumber("quadroBootMs", BootFrameMs);
            json.WriteNumber("bootIntro", BootIntro);
            json.WriteNumber("bootDepois", BootSteady);
            json.WriteNumber("voltaSegundos", IdleSeconds);
            json.WriteNumber("gravacoesFlash", WriteCount);
            Array("mapaNormais", LoopMap);
            Array("mapaBoot", BootMap);
            Array("leds", LedCount);
            Array("ledsOcultos", HiddenLedCount);
            Array("pausa", FrameDelay);
            json.WriteStartArray("slots");
            foreach (var slot in Slots)
            {
                json.WriteStartObject();
                json.WriteNumber("slot", slot.Slot);
                json.WriteString("efeito", slot.EffectName);
                json.WriteStartArray("bytes");
                foreach (byte b in slot.ToBytes()) json.WriteNumberValue(b);
                json.WriteEndArray();
                json.WriteEndObject();
            }
            json.WriteEndArray();
            json.WriteEndObject();

            void Array(string name, int[] values)
            {
                json.WriteStartArray(name);
                foreach (int v in values) json.WriteNumberValue(v);
                json.WriteEndArray();
            }
        }
        return Encoding.UTF8.GetString(stream.ToArray());
    }
}
