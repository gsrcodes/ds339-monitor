namespace DS339Monitor.Argb;

/// <summary>
/// Desenha o Modo Umbra nos buffers dos canais da Nollie. As "portas" do hub são as peças da página Fans e fitas (os
/// grupos, na ordem dos canais); um fan é uma peça com anéis (o centro) e laterais (as partes em linha: a primeira é a
/// esquerda, a segunda a direita).
/// <list type="bullet">
/// <item>Sincronizado: o mesmo efeito em cada parte, cada uma com a sua cópia (num conjunto de fans, cada anel a sua).</item>
/// <item>Independente: cada porta com o seu efeito; num fan, conforme o arranjo — tudo junto; centro + laterais iguais
/// (paralelo); centro + as duas laterais como uma fita só, subindo por uma e descendo pela outra (série); ou centro +
/// esquerda + direita, cada uma com o seu.</item>
/// <item>Combinado: todas as portas em fila, na ordem dos canais, como uma fita só.</item>
/// <item>Tela: os efeitos do "modo tela" desenhados no gabinete pela posição de cada LED (ver <see cref="UmbraCanvas"/>).</item>
/// </list>
/// </summary>
internal sealed class UmbraLighting
{
    private readonly UmbraCanvas _canvas = new();

    private readonly record struct Led(ArgbZone Zone, int Index);

    private sealed record Port(string Name, List<ArgbZone> Zones, List<ArgbZone> Centers, List<ArgbZone> Sides)
    {
        public bool IsFan => Centers.Count > 0 && Sides.Count > 0;
    }

    /// <summary>Desenha um quadro. <paramref name="master"/> = o brilho geral (já linear).</summary>
    public void Render(UmbraSettings u, List<ArgbZone> zones, float[][] target, double t, double master, Func<ArgbZone, LedPoint[]?> points)
    {
        if (!u.LuzesAcesas) return;
        var ports = Ports(zones);
        switch (u.Modo)
        {
            case UmbraModes.Sync:
                foreach (var port in ports)
                foreach (var zone in port.Zones)
                {
                    if (CaseLayout.Rings(zone.Forma, zone.Leds) is { } rings)
                        for (int r = 0; r < rings.Count; r++) Draw(target, u.Sincronizado, u.Sincronizado.Efeito, Ring(zone, rings[r]), t, master, Seed(zone, r));
                    else
                        Draw(target, u.Sincronizado, u.Sincronizado.Efeito, Strip(zone), t, master, Seed(zone, 0));
                }
                break;
            case UmbraModes.Async:
                foreach (var port in ports)
                {
                    var settings = u.Portas.FirstOrDefault(p => p.Grupo == port.Name);
                    if (settings is null) continue;
                    if (port.IsFan) DrawFan(target, port, settings, t, master);
                    else
                    {
                        var all = settings.Slot(UmbraZones.All);
                        Draw(target, all, all.Efeito, port.Zones.SelectMany(Strip).ToList(), t, master, Seed(port.Zones[0], 7));
                    }
                }
                break;
            case UmbraModes.Merge:
            {
                var slot = u.Combinado;
                var chain = ports.SelectMany(p => p.Zones.OrderBy(z => z.Canal).ThenBy(z => z.Inicio)).SelectMany(Strip).ToList();
                Draw(target, slot, UmbraEffects.MergeIds[Math.Clamp(slot.Efeito, 0, UmbraEffects.MergeIds.Length - 1)], chain, t, master, 99);
                break;
            }
            case UmbraModes.Canvas:
                _canvas.Render(u.Tela, zones, target, t, master, points);
                break;
        }
    }

    private static void DrawFan(float[][] target, Port port, UmbraPort settings, double t, double master)
    {
        var left = Strip(port.Sides[0]);
        var right = port.Sides.Count > 1 ? Strip(port.Sides[1]) : new List<Led>();
        if (settings.Arranjo == UmbraZones.LayoutAll)
        {
            // Tudo: o mesmo efeito em cada anel e em cada lateral.
            var all = settings.Slot(UmbraZones.All);
            Centers(target, port, all, t, master, perFan: false);
            Draw(target, all, all.Efeito, left, t, master, Seed(port.Sides[0], 1));
            if (right.Count > 0) Draw(target, all, all.Efeito, right, t, master, Seed(port.Sides[^1], 2));
            return;
        }

        Centers(target, port, settings.Slot(UmbraZones.Center), t, master, perFan: true);
        switch (settings.Arranjo)
        {
            case UmbraZones.LayoutParallel:
            {
                // As duas laterais com o mesmo efeito, LED a LED (espelhadas).
                var slot = settings.Slot(UmbraZones.Parallel);
                Draw(target, slot, slot.Efeito, left, t, master, Seed(port.Sides[0], 3));
                if (right.Count > 0) Draw(target, slot, slot.Efeito, right, t, master, Seed(port.Sides[0], 3));
                break;
            }
            case UmbraZones.LayoutSeries:
            {
                // Uma fita só: sobe por uma lateral e desce pela outra.
                var slot = settings.Slot(UmbraZones.Series);
                var chain = new List<Led>(left);
                chain.AddRange(Enumerable.Reverse(right));
                Draw(target, slot, slot.Efeito, chain, t, master, Seed(port.Sides[0], 4));
                break;
            }
            default:
            {
                var l = settings.Slot(UmbraZones.Left);
                var r = settings.Slot(UmbraZones.Right);
                Draw(target, l, l.Efeito, left, t, master, Seed(port.Sides[0], 5));
                if (right.Count > 0) Draw(target, r, r.Efeito, right, t, master, Seed(port.Sides[^1], 6));
                break;
            }
        }
    }

    /// <summary>O efeito em cada anel da porta (um por fan; na cor estática do centro, cada fan com a sua cor).</summary>
    private static void Centers(float[][] target, Port port, UmbraSlot slot, double t, double master, bool perFan)
    {
        int fan = 0;
        foreach (var zone in port.Centers)
        {
            var rings = CaseLayout.Rings(zone.Forma, zone.Leds) ?? new List<(int, int)> { (0, zone.Leds) };
            foreach (var ring in rings)
            {
                Draw(target, slot, slot.Efeito, Ring(zone, ring), t, master, Seed(zone, fan), perFan ? fan : -1);
                fan++;
            }
        }
    }

    private static void Draw(float[][] target, UmbraSlot slot, int effect, List<Led> strip, double t, double master, int seed, int fan = -1)
    {
        if (strip.Count == 0) return;
        var colors = UmbraEffects.Colors(slot);
        double gain = master * slot.Brilho / 255.0;
        for (int i = 0; i < strip.Count; i++)
        {
            var (r, g, b) = UmbraEffects.Shade(effect, slot, colors, i, strip.Count, t, seed, fan);
            var led = strip[i];
            double k = gain * led.Zone.Brilho / 100.0;
            var buffer = target[led.Zone.Canal - 1];
            int at = (led.Zone.Inicio + led.Index) * 3;
            buffer[at] = (float)(r * k);
            buffer[at + 1] = (float)(g * k);
            buffer[at + 2] = (float)(b * k);
        }
    }

    /// <summary>Os LEDs de uma parte em ordem ("Inverter" vira o sentido).</summary>
    private static List<Led> Strip(ArgbZone zone)
    {
        var leds = new List<Led>(zone.Leds);
        for (int i = 0; i < zone.Leds; i++) leds.Add(new Led(zone, zone.Inverter ? zone.Leds - 1 - i : i));
        return leds;
    }

    private static List<Led> Ring(ArgbZone zone, (int Start, int Count) ring)
    {
        var leds = new List<Led>(ring.Count);
        for (int i = 0; i < ring.Count; i++) leds.Add(new Led(zone, ring.Start + (zone.Inverter ? ring.Count - 1 - i : i)));
        return leds;
    }

    private static int Seed(ArgbZone zone, int salt) => zone.Canal * 1009 + zone.Inicio * 31 + salt;

    /// <summary>As portas: as peças (grupos), na ordem dos canais.</summary>
    private static List<Port> Ports(List<ArgbZone> zones) => zones
        .GroupBy(z => z.Grupo)
        .OrderBy(g => g.Min(z => z.Canal * 1000 + z.Inicio))
        .Select(g =>
        {
            var list = g.OrderBy(z => z.Canal).ThenBy(z => z.Inicio).ToList();
            var centers = list.Where(z => CaseLayout.Rings(z.Forma, z.Leds) is not null).ToList();
            var sides = list.Where(z => z.Forma?.TrimStart().StartsWith("linha", StringComparison.OrdinalIgnoreCase) == true).ToList();
            return new Port(g.Key, list, centers, sides);
        })
        .ToList();

    /// <summary>
    /// Prévia de um efeito para o painel: um fan de mentira — anel de 12 LEDs e duas laterais de 6 —, cada quadro como
    /// "RRGGBB" por LED (anel, lateral esquerda, lateral direita), como o olho vê. <paramref name="chain"/> = as três
    /// partes como uma fita só (o modo combinado).
    /// </summary>
    public static string[] Preview(int effect, UmbraSlot slot, bool chain, int steps, int stepMs)
    {
        var colors = UmbraEffects.Colors(slot);
        var frames = new string[steps];
        var text = new System.Text.StringBuilder(24 * 6);
        (double R, double G, double B)[] leds = new (double, double, double)[24];
        for (int s = 0; s < steps; s++)
        {
            double t = s * stepMs / 1000.0;
            if (chain)
            {
                for (int i = 0; i < 24; i++) leds[i] = UmbraEffects.Shade(effect, slot, colors, i, 24, t, 5);
            }
            else
            {
                for (int i = 0; i < 12; i++) leds[i] = UmbraEffects.Shade(effect, slot, colors, i, 12, t, 1, 0);
                for (int i = 0; i < 6; i++) leds[12 + i] = UmbraEffects.Shade(effect, slot, colors, i, 6, t, 2);
                for (int i = 0; i < 6; i++) leds[18 + i] = UmbraEffects.Shade(effect, slot, colors, i, 6, t, 3);
            }
            text.Clear();
            foreach (var (r, g, b) in leds) text.Append(Hex(r)).Append(Hex(g)).Append(Hex(b));
            frames[s] = text.ToString();
        }
        return frames;
    }

    private static string Hex(double v) => ((int)Math.Round(255 * Math.Pow(Math.Clamp(v, 0, 1), 1 / 2.2))).ToString("X2");

    /// <summary>Para o painel: as portas na ordem, quantos fans cada uma tem (0 = não é fan) e quantos LEDs.</summary>
    public static IEnumerable<(string Name, int Fans, int Leds)> Describe(List<ArgbZone> zones) =>
        Ports(zones).Select(p => (p.Name, p.IsFan ? p.Centers.Sum(z => CaseLayout.Rings(z.Forma, z.Leds)?.Count ?? 1) : 0, p.Zones.Sum(z => z.Leds)));
}
