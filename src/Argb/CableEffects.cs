using System.Drawing;

namespace DS339Monitor.Argb;

/// <summary>
/// Efeitos no estilo dos Strimer da Lian Li, feitos para as capas de cabo: a capa é uma grade de fileiras ao longo do
/// cabo (ver <see cref="CaseLayout.Grid"/> — aqui, 5 fileiras de 22 LEDs), e cada efeito usa as duas direções: ao longo
/// do cabo (0 numa ponta … 1 na outra) e de uma fileira para a outra. Numa parte que não é grade (fans, fitas), o efeito
/// vira uma fileira só ao longo da parte. "Inverter" na parte vira o sentido.
/// <para>
/// Como os outros efeitos de parte, tudo é função do tempo e da posição (sem estado), e a cor sai em fração de luz por
/// canal. A trava contra pisca-pisca continua valendo na saída.
/// </para>
/// </summary>
internal static class CableEffects
{
    private readonly record struct Rgb(double R, double G, double B)
    {
        public static Rgb operator +(Rgb a, Rgb b) => new(a.R + b.R, a.G + b.G, a.B + b.B);
        public static Rgb operator *(Rgb a, double k) => new(a.R * k, a.G * k, a.B * k);
    }

    /// <summary>Onde o LED fica na grade.</summary>
    private readonly record struct Cell(int Row, int Rows, int Col, int Cols, int Index, int Zone)
    {
        /// <summary>Ao longo do cabo: 0 numa ponta, 1 na outra.</summary>
        public double Along => Cols > 1 ? Col / (double)(Cols - 1) : 0.5;

        /// <summary>De uma fileira à outra: 0 na primeira, 1 na última.</summary>
        public double Across => Rows > 1 ? Row / (double)(Rows - 1) : 0.5;

        /// <summary>Distância da fileira do meio: 0 no meio, 1 nas de fora.</summary>
        public double FromMiddle => Rows > 1 ? Math.Abs(Across - 0.5) * 2 : 0;

        /// <summary>Um número fixo por fileira (para sortear coisas por fileira).</summary>
        public int Lane => Zone * 31 + Row;
    }

    private static readonly Rgb White = new(1, 1, 1);

    /// <summary>Cor do LED <paramref name="index"/> (já com o "Inverter" aplicado) numa grade de fileiras × LEDs.</summary>
    public static (double R, double G, double B) Shade(string effect, Color color, (int Rows, int Cols) grid, int index, double t, double speed, int zone)
    {
        int rows = Math.Max(1, grid.Rows), cols = Math.Max(1, grid.Cols);
        var p = new Cell(Math.Min(rows - 1, index / cols), rows, index % cols, cols, index, zone);
        var c = new Rgb(color.R / 255.0, color.G / 255.0, color.B / 255.0);
        double s = t * speed;
        var light = effect switch
        {
            ArgbEffects.Runway => Runway(c, p, s),
            ArgbEffects.Meteors => Meteors(c, p, s),
            ArgbEffects.Current => Current(c, p, s),
            ArgbEffects.Stack => Stack(c, p, s),
            ArgbEffects.PingPong => PingPong(c, p, s),
            ArgbEffects.Staggered => Staggered(c, p, s),
            ArgbEffects.Snake => Snake(c, p, s),
            ArgbEffects.Cover => Cover(c, p, s),
            ArgbEffects.Kaleidoscope => Kaleidoscope(p, s),
            ArgbEffects.Diagonal => Hue(Frac(p.Along * 0.9 + p.Across * 0.35 - s / 5)),
            _ => c,
        };
        return (Math.Clamp(light.R, 0, 1), Math.Clamp(light.G, 0, 1), Math.Clamp(light.B, 0, 1));
    }

    /// <summary>
    /// Pista: luzes de aproximação correndo das duas pontas para o meio do cabo, a fileira do meio na frente (uma seta);
    /// onde as duas se encontram, o meio acende.
    /// </summary>
    private static Rgb Runway(Rgb c, Cell p, double s)
    {
        double d = Math.Abs(p.Along - 0.5) * 2;
        double phase = Frac(d * 1.6 + s * 1.1 + p.FromMiddle * 0.22);
        double head = Math.Exp(-phase / 0.07) + Math.Exp(-(1 - phase) / 0.012);
        double landing = Math.Exp(-Frac(s * 1.1) / 0.15) * Band(d, 0.12);
        double level = Math.Min(1, 0.08 + 0.92 * head + 0.6 * landing);
        return Mix(c, White, Math.Pow(Math.Min(1, head), 4)) * Linear(level);
    }

    /// <summary>Meteoros: cada fileira solta os seus, em horas e velocidades sorteadas; cabeça branca, rastro na cor.</summary>
    private static Rgb Meteors(Rgb c, Cell p, double s)
    {
        var light = c * 0.003;
        double clock = s / 1.5 + Hash(p.Lane, 0, 3);
        long n = (long)Math.Floor(clock);
        for (long k = n - 1; k <= n; k++)
        {
            if (Hash(p.Lane, (int)k, 5) < 0.25) continue; // de vez em quando uma fileira descansa
            double pace = 0.8 + 0.6 * Hash(p.Lane, (int)k, 7);
            double head = -0.15 + (clock - k) * 1.5 * pace;
            double behind = head - p.Along;
            if (behind < -0.05 || behind > 0.7) continue;
            double tail = behind < 0 ? Math.Exp(behind / 0.01) : Math.Exp(-behind / 0.16);
            light += c * Linear(0.9 * tail) + White * (0.8 * Linear(Band(behind, 0.035)));
        }
        return light;
    }

    /// <summary>
    /// Corrente elétrica: pacotes de energia disparando pelo cabo, tremendo de fileira em fileira, com faíscas em volta;
    /// de vez em quando, um arco atravessa todas as fileiras de uma vez.
    /// </summary>
    private static Rgb Current(Rgb c, Cell p, double s)
    {
        var light = c * 0.004;
        double jitter = (Hash(p.Lane, (int)Math.Floor(s * 10), 11) - 0.5) * 0.08;
        double head = -0.2 + Frac(s / 0.9 + p.Row * 0.07) * 1.5 + jitter;
        double behind = head - p.Along;
        double glow = behind < 0 ? Math.Exp(behind / 0.015) : Math.Exp(-behind / 0.12);
        double spark = glow > 0.15 && Hash(p.Zone, p.Index, (int)Math.Floor(s * 14)) > 0.85 ? 0.7 : 0;
        light += Mix(c, White, glow * glow) * Linear(glow) + White * (Linear(spark) * glow);

        double clock = s / 1.3;
        long n = (long)Math.Floor(clock);
        double age = (clock - n) * 1.3;
        double arc = Math.Exp(-age / 0.12) * Band(p.Along - (0.1 + 0.8 * Hash(p.Zone, (int)n, 13)), 0.05);
        return light + (White * 0.7 + c * 0.3) * Linear(arc);
    }

    /// <summary>
    /// Empilhar: blocos caem da ponta de lá e empilham na de cá, cada fileira no seu tempo; cheia, a pilha brilha e
    /// escoa, e começa de novo.
    /// </summary>
    private static Rgb Stack(Rgb c, Cell p, double s)
    {
        const double Fall = 0.45, Hold = 0.7, Drain = 0.6;
        int slots = Math.Max(2, p.Cols / 2);
        double total = 0;
        for (int k = 0; k < slots; k++) total += Fall * (slots - k) / slots;
        double cycle = total + Hold + Drain;
        double u = Frac((s + p.Row * 0.09) / cycle) * cycle;
        int slot = Math.Min(slots - 1, p.Col * slots / p.Cols);

        int stacked = slots;
        double falling = -1;
        double elapsed = 0;
        for (int k = 0; k < slots; k++)
        {
            double duration = Fall * (slots - k) / slots;
            if (u < elapsed + duration)
            {
                stacked = k;
                falling = slots - (slots - k) * EaseIn((u - elapsed) / duration);
                break;
            }
            elapsed += duration;
        }

        double shade = slot % 2 == 0 ? 1 : 0.72;
        if (stacked == slots)
        {
            // Cheia: um brilho que assenta e a pilha escoando pela ponta de cá.
            double after = u - total;
            double glow = Math.Exp(-after / 0.25);
            double left = after < Hold ? slots : slots * (1 - (after - Hold) / Drain);
            if (slot >= left) return c * 0.003;
            return Mix(c, White, 0.5 * glow) * Linear(0.6 * shade + 0.4 * glow);
        }
        if (slot < stacked) return c * Linear(0.65 * shade);
        if (slot + 0.5 >= falling && slot + 0.5 <= falling + 1) return Mix(c, White, 0.6) * Linear(1);
        return c * 0.003;
    }

    /// <summary>
    /// Pingue-pongue: uma barra atravessando todas as fileiras vai e volta; em cada batida na ponta, um respingo, e a
    /// barra troca entre a cor e o branco.
    /// </summary>
    private static Rgb PingPong(Rgb c, Cell p, double s)
    {
        const double Leg = 0.8;
        double clock = s / Leg;
        long n = (long)Math.Floor(clock);
        double f = clock - n;
        bool forward = (n & 1) == 0;
        double x = forward ? f : 1 - f;
        double d = p.Along - x;
        double behind = forward ? -d : d;
        double bar = Band(d, 1.3 / p.Cols) + (behind > 0 ? 0.35 * Math.Exp(-behind / 0.12) : 0);
        var tone = forward ? c : Mix(c, White, 0.85);
        double splash = Math.Exp(-f * Leg / 0.18) * Math.Exp(-Math.Abs(p.Along - (forward ? 0 : 1)) / 0.08);
        return c * 0.004 + tone * Linear(Math.Min(1, bar)) + White * (0.8 * Linear(splash));
    }

    /// <summary>Contramão: fileiras alternadas correm em sentidos opostos — listras na cor e listras brancas se cruzando.</summary>
    private static Rgb Staggered(Rgb c, Cell p, double s)
    {
        bool even = p.Row % 2 == 0;
        double stripe = Math.Pow(0.5 + 0.5 * Math.Sin(2 * Math.PI * (p.Along * 2.2 + (even ? -1 : 1) * s * 0.6)), 3);
        return (even ? c : Mix(c, White, 0.8)) * Linear(0.08 + 0.92 * stripe);
    }

    /// <summary>
    /// Serpente: duas cobras percorrendo a grade em zigue-zague (uma fileira para lá, a seguinte para cá), uma na cor e
    /// uma branca, uma correndo atrás da outra.
    /// </summary>
    private static Rgb Snake(Rgb c, Cell p, double s)
    {
        int total = p.Rows * p.Cols;
        int along = p.Row % 2 == 0 ? p.Col : p.Cols - 1 - p.Col;
        double q = p.Row * p.Cols + along;
        double head = Frac(s / 3.2) * total;
        double tail = Math.Max(6, total * 0.22);
        var light = c * 0.003;
        for (int k = 0; k < 2; k++)
        {
            double behind = head - k * total / 2.0 - q;
            behind = (behind % total + total) % total;
            if (behind > tail) continue;
            double level = Math.Pow(1 - behind / tail, 1.6);
            var tone = k == 0 ? c : White * 0.9;
            light += Mix(tone, White, behind < 1 ? 0.7 : 0) * Linear(level);
        }
        return light;
    }

    /// <summary>Cobrir: a cor cobre o cabo de uma ponta à outra, fileira atrás de fileira, com a borda clara; depois o escuro cobre de volta.</summary>
    private static Rgb Cover(Rgb c, Cell p, double s)
    {
        double clock = s / 1.8 - p.Row * 0.06;
        long n = (long)Math.Floor(clock);
        double f = clock - n;
        double front = EaseInOut(Math.Clamp(f / 0.7, 0, 1)) * 1.12 - 0.06;
        double covered = Smooth(front + 0.04, front - 0.04, p.Along);
        double level = (n & 1) == 0 ? 0.12 + 0.88 * covered : 1 - 0.88 * covered;
        double edge = f < 0.75 ? Band(p.Along - front, 0.05) : 0;
        return c * Linear(level) + White * (0.7 * Linear(edge));
    }

    /// <summary>Caleidoscópio: o arco-íris espelhado a partir do meio do cabo e da fileira do meio, pulsando em anéis.</summary>
    private static Rgb Kaleidoscope(Cell p, double s)
    {
        double d = Math.Abs(p.Along - 0.5) * 2, e = p.FromMiddle;
        double rings = 0.5 + 0.5 * Math.Cos(2 * Math.PI * (d * 1.8 + e * 0.6 - s * 0.45));
        return Hue(Frac(d * 0.55 + e * 0.25 - s / 7)) * Linear(0.3 + 0.7 * rings);
    }

    // ---------------------------------------------------------------- utilidades

    private static Rgb Hue(double hue)
    {
        double h = hue * 6;
        double x = 1 - Math.Abs(h % 2 - 1);
        return (int)h switch
        {
            0 => new Rgb(1, x, 0),
            1 => new Rgb(x, 1, 0),
            2 => new Rgb(0, 1, x),
            3 => new Rgb(0, x, 1),
            4 => new Rgb(x, 0, 1),
            _ => new Rgb(1, 0, x),
        };
    }

    private static Rgb Mix(Rgb a, Rgb b, double k)
    {
        k = Math.Clamp(k, 0, 1);
        return new Rgb(a.R + (b.R - a.R) * k, a.G + (b.G - a.G) * k, a.B + (b.B - a.B) * k);
    }

    /// <summary>Nível como o olho vê (0–1) → fração de luz do LED.</summary>
    private static double Linear(double level) => Math.Pow(Math.Clamp(level, 0, 1), 2.2);

    private static double Band(double x, double width) => Math.Exp(-x * x / (width * width));

    private static double Smooth(double e0, double e1, double x)
    {
        double k = Math.Clamp((x - e0) / (e1 - e0), 0, 1);
        return k * k * (3 - 2 * k);
    }

    private static double EaseIn(double x)
    {
        x = Math.Clamp(x, 0, 1);
        return x * x;
    }

    private static double EaseInOut(double x) => x * x * (3 - 2 * x);

    private static double Frac(double x) => x - Math.Floor(x);

    private static double Hash(int a, int b, int salt)
    {
        unchecked
        {
            uint h = (uint)(a * 73856093) ^ (uint)(b * 19349663) ^ (uint)(salt * 83492791);
            h ^= h >> 13;
            h *= 0x5BD1E995;
            h ^= h >> 15;
            return (h & 0xFFFFFF) / (double)0x1000000;
        }
    }
}
