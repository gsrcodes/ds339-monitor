using System.Drawing;
using DS339Monitor.App;
using DS339Monitor.Themes;

namespace DS339Monitor.World;

/// <summary>
/// Onde as coisas estão no gabinete, em milímetros, vistas pelo vidro (x para a direita, y para baixo). Padrão
/// ("Torre"): a Thermalright deitada no topo, a Pro Flow logo abaixo (centrada) e a luz da placa de vídeo, montada na
/// vertical, embaixo. O eixo y conta a história: em cima o céu e a superfície do mar, no meio debaixo d'água, embaixo
/// a luz das profundezas.
/// </summary>
internal sealed class WorldLayout
{
    /// <summary>Quanto os vãos entre as telas "contam" no tempo de travessia (o que cai não some por 1 s no vão).</summary>
    public const float GapFactor = 0.35f;

    public WorldLayout(float gapTopMm = 63, float gapBottomMm = 90, float proFlowOffsetMm = 0)
    {
        Top = new RectangleF(0, 0, 222, 53);
        Middle = new RectangleF(111 - 60.5f + proFlowOffsetMm, Top.Bottom + gapTopMm, 121, 60);
        Led = new PointF(Middle.X + Middle.Width / 2, Middle.Bottom + gapBottomMm);
    }

    /// <summary>Thermalright 9,16" (1920x462).</summary>
    public RectangleF Top { get; }

    /// <summary>Pro Flow 360 (960x480).</summary>
    public RectangleF Middle { get; }

    /// <summary>A luz da placa de vídeo.</summary>
    public PointF Led { get; }

    /// <summary>Até onde a luz da placa "sente" um ator (mm).</summary>
    public float Reach => 100;

    /// <summary>Altura da superfície do mar na Thermalright (onde o que cai do céu entra na água).</summary>
    public float SurfaceY => Top.Y + Top.Height * 0.82f;

    /// <summary>Faixa de x que aparece nas duas telas (o que cai aqui chega à Pro Flow).</summary>
    public (float From, float To) Shared => (Math.Max(Top.X, Middle.X) + 6, Math.Min(Top.Right, Middle.Right) - 6);

    public RectangleF Window(ScreenKind kind) => kind == ScreenKind.Thermalright ? Top : Middle;

    public SizeF PxPerMm(ScreenKind kind)
    {
        var (w, h) = ThemeCatalog.Size(kind);
        var r = Window(kind);
        return new SizeF(w / r.Width, h / r.Height);
    }

    public PointF ToPx(ScreenKind kind, PointF mm)
    {
        var r = Window(kind);
        var k = PxPerMm(kind);
        return new PointF((mm.X - r.X) * k.Width, (mm.Y - r.Y) * k.Height);
    }

    public float ToMmX(ScreenKind kind, float px) => Window(kind).X + px / PxPerMm(kind).Width;

    /// <summary>Quanto da luz de um ator nesta posição chega à placa (1 em cima dela, 0 a partir do alcance).</summary>
    public double Falloff(PointF mm)
    {
        double dx = mm.X - Led.X, dy = mm.Y - Led.Y;
        double d = Math.Sqrt(dx * dx + dy * dy) / Reach;
        return d >= 1 ? 0 : (1 - d * d) * (1 - d * d);
    }

    // ---------------------------------------------------------------- rota vertical (com os vãos encurtados)

    /// <summary>Comprimento "de viagem" de y0 até y1 (descendo), com os vãos contando <see cref="GapFactor"/>.</summary>
    public float TravelLength(float y0, float y1) => Travel(y1) - Travel(y0);

    /// <summary>Posição de viagem de um y do mundo (0 = borda de cima da Thermalright; acima dela, negativo).</summary>
    public float Travel(float y)
    {
        if (y <= Top.Y) return y - Top.Y;
        float s = 0;
        foreach (var (from, to, factor) in Segments())
        {
            if (y <= to) return s + (y - from) * factor;
            s += (to - from) * factor;
        }
        return s + (y - Led.Y); // abaixo da placa: anda normal
    }

    /// <summary>O y do mundo para uma posição de viagem.</summary>
    public float FromTravel(float s)
    {
        if (s <= 0) return Top.Y + s;
        float acc = 0;
        foreach (var (from, to, factor) in Segments())
        {
            float length = (to - from) * factor;
            if (s <= acc + length) return from + (s - acc) / factor;
            acc += length;
        }
        return Led.Y + (s - acc);
    }

    private IEnumerable<(float From, float To, float Factor)> Segments()
    {
        yield return (Top.Y, Top.Bottom, 1);
        yield return (Top.Bottom, Middle.Y, GapFactor);
        yield return (Middle.Y, Middle.Bottom, 1);
        yield return (Middle.Bottom, Led.Y, GapFactor);
    }
}

/// <summary>Como uma tela vê um ator agora.</summary>
internal readonly record struct ActorView(PointF Px, SizeF PxPerMm, float Angle, float Opacity, double Age, double Seconds);

/// <summary>
/// Algo que existe no mundo por um tempo: nasce num instante, anda por uma trajetória em milímetros (função do tempo —
/// todas as telas e a luz perguntam a mesma conta), pode emitir luz ou fazer sombra na placa, e sabe se desenhar.
/// </summary>
internal sealed class WorldActor
{
    public required string Name { get; init; }
    public required long Start { get; init; }
    public required double Seconds { get; init; }

    /// <summary>Posição em mm, s segundos depois de nascer.</summary>
    public required Func<double, PointF> Position { get; init; }

    public Func<double, float> Angle { get; init; } = _ => 0;
    public Func<double, float> Opacity { get; init; } = _ => 1;

    /// <summary>Luz que o ator emite (na escala linear; o LED soma com a queda pela distância).</summary>
    public Func<double, double>? Emission { get; init; }

    /// <summary>Sombra que o ator faz na luz da placa (0–0,8).</summary>
    public Func<double, double>? Shadow { get; init; }

    /// <summary>Tom da luz do ator: −1 frio, +1 morno.</summary>
    public double Warmth { get; init; }

    /// <summary>A emissão não cai com a distância (luz "da cena", não de um objeto).</summary>
    public bool Ambient { get; init; }

    /// <summary>Desenho próprio. Recebe a tela e a vista; null = não aparece nas telas (só luz).</summary>
    public Action<Graphics, ScreenKind, ActorView>? Draw { get; init; }

    /// <summary>Na Thermalright: −1 atrás do navio, 0 atrás das ondas da frente, 1 na frente de tudo.</summary>
    public int Layer { get; init; }

    /// <summary>Raio de desenho em mm (para saber se aparece numa tela).</summary>
    public float RadiusMm { get; init; } = 12;

    public bool Alive(long now) => now >= Start && now <= Start + (long)(Seconds * 1000);
}

/// <summary>O que o Sunny está fazendo (a Thermalright desenha; o mundo decide).</summary>
internal enum ShipMode
{
    Sailing,
    /// <summary>Knock Up Stream: a coluna d'água lança o navio para o céu (jogo abrindo).</summary>
    Launch,
    /// <summary>Lá em cima, fora da tela, enquanto o jogo roda.</summary>
    Away,
    /// <summary>Voltando do céu pendurado no balão-polvo (jogo fechou).</summary>
    Return,
    /// <summary>Âncora lançada (PC dormindo).</summary>
    Anchored,
}

/// <summary>Retrato do mundo num instante (imutável: as threads só trocam a referência).</summary>
internal sealed record WorldState
{
    public static WorldState Empty { get; } = new();

    public WorldLayout Layout { get; init; } = new();
    public IReadOnlyList<WorldActor> Actors { get; init; } = Array.Empty<WorldActor>();
    public ShipMode Ship { get; init; } = ShipMode.Sailing;
    public long ShipSince { get; init; }
    /// <summary>Das 23h às 7h: a luz quase não reage e não há onomatopeias.</summary>
    public bool Quiet { get; init; }
    /// <summary>Baús no convés (pendrives), com o rótulo.</summary>
    public IReadOnlyList<string> Chests { get; init; } = Array.Empty<string>();
    /// <summary>Microfone/webcam em uso: o Den Den Mushi fica grudado no vidro da Pro Flow.</summary>
    public bool OnAir { get; init; }
    public long OnAirSince { get; init; }

    /// <summary>x do Sunny em mm (balança devagar no mar).</summary>
    public static float ShipX(long now) => 64.4f + 4.2f * (float)Math.Sin(now / 1000.0 / 9);
}

/// <summary>
/// A luz das profundezas — o que a placa de vídeo mostra e o que a borda de baixo da Pro Flow reflete: branco frio da
/// Ilha dos Tritões seguindo o sol de verdade, esquentando com o calor da placa (o vulcão), mais a luz e a sombra dos
/// atores perto dela. Uma conta só para a placa e para a tela.
/// </summary>
internal static class WorldLight
{
    /// <summary>Nível (0–1, escala do olho) e tom (−1 frio, +1 morno) da luz da placa no instante <paramref name="now"/>.</summary>
    public static (double Level, double Warmth) At(long now, StageState stage, WorldState world)
    {
        double heat = float.IsNaN(stage.GpuTemp) ? 0 : Lighting.Choreo.Smooth(45, 82, stage.GpuTemp);
        double level = Lighting.Choreo.Lerp(0.25, 0.55, stage.Daylight) + 0.12 * heat + 0.06 * stage.Audio;
        double warmth = Lighting.Choreo.Lerp(-0.35, 0.7, heat);
        if (world.Ship == ShipMode.Anchored && stage.Sleeping)
        {
            double t = now / 1000.0, period = stage.Locked ? 10 : 5.2;
            double p = t % period / period;
            double w = p < 0.35 ? Lighting.Choreo.Smooth(0, 0.35, p) : p < 0.85 ? 1 - Lighting.Choreo.Smooth(0.35, 0.85, p) : 0;
            level = 0.15 + (stage.Locked ? 0.2 : 0.3) * w;
            warmth = 0.5;
        }

        // Atores: emissão (soma, com queda pela distância) e sombra, na luz linear.
        double y = Math.Pow(Math.Clamp(level, 0, 1), 2.2), emission = 0, shadow = 0, tone = 0, toneWeight = 0;
        foreach (var actor in world.Actors)
        {
            if (!actor.Alive(now)) continue;
            double s = (now - actor.Start) / 1000.0;
            double k = actor.Ambient ? 1 : world.Layout.Falloff(actor.Position(s));
            if (k <= 0) continue;
            double e = (actor.Emission?.Invoke(s) ?? 0) * k;
            emission += e;
            shadow += (actor.Shadow?.Invoke(s) ?? 0) * k;
            tone += actor.Warmth * e;
            toneWeight += e;
        }
        if (world.Quiet) emission *= 0.35;
        y *= 1 - Math.Min(0.8, shadow);
        y += (1 - y) * Math.Min(1, emission);
        if (toneWeight > 0.001) warmth = Lighting.Choreo.Lerp(warmth, tone / toneWeight, Math.Min(1, toneWeight * 2));
        return (Math.Pow(y, 1 / 2.2), Math.Clamp(warmth, -1, 1));
    }
}
