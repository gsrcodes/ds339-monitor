using System.Drawing;
using DS339Monitor.Lighting;

namespace DS339Monitor.Argb;

/// <summary>
/// Os 15 efeitos de fan do UMBRA (a grade 5×3 do software, na mesma ordem e com os mesmos números) e os 10 do modo
/// combinado. As regras de cada um — se tem velocidade, direção, quantas cores — são as da tabela
/// <c>ARGB_effect_check_list</c> do software. O desenho é nosso: no hub, quem desenha é o chip.
/// <para>
/// Cada efeito anda ao longo de uma "fita": um anel, uma lateral, as duas laterais em série, uma peça inteira ou (no
/// combinado) todas as peças em fila. Tudo é função do tempo e da posição (sem estado); o que é sorteado sai de um hash.
/// </para>
/// </summary>
internal static class UmbraEffects
{
    /// <summary>Um efeito: o nome em português, o do software (chinês · inglês) e os controles que ele mostra.</summary>
    internal sealed record Effect(int Id, string Nome, string Original, bool Speed, bool Direction, int Colors, string Descricao);

    public static readonly Effect[] Fan =
    {
        new(0, "Cor estática", "静态色 · Static", false, false, 1, "Uma cor parada (no centro, uma cor por fan)"),
        new(1, "Arco-íris", "彩虹 · Rainbow", true, true, 0, "O arco-íris correndo pela peça"),
        new(2, "Arco-íris gradiente", "渐变彩虹 · Gradient Rainbow", true, false, 0, "A peça inteira passando por todas as cores"),
        new(3, "Respiração", "呼吸 · Breath", true, false, 4, "Acende e apaga devagar, uma cor de cada vez"),
        new(4, "Respiração arco-íris", "呼吸彩虹 · Breath Rainbow", true, false, 0, "Respira, cada vez numa cor do arco-íris"),
        new(5, "Pista", "跑道 · Raceway", true, true, 1, "Dois carros de luz correndo em volta"),
        new(6, "Ladrilho", "平铺 · Tiling", true, true, 4, "Cada cor cobre a peça por cima da anterior"),
        new(7, "Esvanecer", "渐灭 · Fade Out", true, true, 1, "Acende inteira e vai apagando de uma ponta à outra"),
        new(8, "Rastro", "拖尾 · Trail", true, true, 4, "Um cometa com rastro; cada volta numa cor"),
        new(9, "Varredura", "扫描 · Scan", true, false, 2, "Um bloco vai e volta sobre a segunda cor"),
        new(10, "Blocos", "积木 · Building Blocks", true, true, 1, "Blocos caem e empilham até encher; aí somem"),
        new(11, "Discoteca", "迪斯科 · Disco", true, true, 4, "Trechos coloridos sorteados andando no ritmo"),
        new(12, "Expandir e contrair", "收张 · Expand and Contract", true, false, 4, "A luz abre do meio para as pontas e fecha"),
        new(13, "Relâmpago", "闪电 · Lightning", true, false, 2, "Relâmpagos da primeira cor sobre a segunda"),
        new(14, "Endereçamento", "寻址 · Addressing", true, true, 1, "Um LED de cada vez, de ponta a ponta"),
    };

    /// <summary>O modo combinado: só os efeitos que correm pela fila inteira (índice 0–9 → número do efeito).</summary>
    public static readonly int[] MergeIds = { 1, 5, 6, 7, 8, 9, 10, 12, 13, 14 };

    /// <summary>Velocidade (1 = a mais rápida … 6 = a mais lenta) → quanto o tempo anda.</summary>
    private static double Rate(int speed) => Math.Clamp(speed, 1, 6) switch
    {
        1 => 2.2,
        2 => 1.7,
        3 => 1.3,
        4 => 1.0,
        5 => 0.75,
        _ => 0.55,
    };

    /// <summary>
    /// Cor do LED <paramref name="i"/> de uma fita de <paramref name="n"/> LEDs (fração de luz por canal, sem o brilho).
    /// <paramref name="fan"/> = qual fan da porta (a cor estática do centro é uma por fan); <paramref name="seed"/> varia
    /// o que é sorteado de uma fita para outra.
    /// </summary>
    public static (double R, double G, double B) Shade(int effect, UmbraSlot s, Color[] colors, int i, int n, double t, int seed, int fan = -1)
    {
        n = Math.Max(1, n);
        if (Fan[Math.Clamp(effect, 0, Fan.Length - 1)].Direction && s.Direcao == 0) i = n - 1 - i;
        double x = i / (double)n;
        double tau = t * Rate(s.Velocidade);
        switch (effect)
        {
            case 0:
                return Of(colors[fan >= 0 ? fan % colors.Length : 0], 1);
            case 1:
                return Hue(Frac(x - tau / 3.5));
            case 2:
                return Hue(Frac(tau / 7));
            case 3:
            {
                const double Period = 3.6;
                double phase = Frac(tau / Period);
                return Of(colors[(int)Math.Floor(tau / Period) & 3], Breath(phase));
            }
            case 4:
            {
                const double Period = 3.6;
                double phase = Frac(tau / Period);
                return Scale(Hue(Frac(Math.Floor(tau / Period) * 0.618)), Linear(Breath(phase)));
            }
            case 5:
            {
                double head = Frac(tau / 2.6), level = 0;
                for (int car = 0; car < 2; car++)
                {
                    double behind = Frac(head + car * 0.5 - x);
                    if (behind < 0.18) level = Math.Max(level, Math.Pow(1 - behind / 0.18, 1.5));
                }
                return Of(colors[0], level);
            }
            case 6:
            {
                const double Wipe = 1.6;
                long k = (long)Math.Floor(tau / Wipe);
                double front = Frac(tau / Wipe) * (1 + 1.0 / n);
                double cover = Smooth(front, front - 1.0 / n, x);
                return Blend(Of(colors[(int)((k + 3) & 3)], 1), Of(colors[(int)(k & 3)], 1), cover);
            }
            case 7:
            {
                double phase = Frac(tau / 3.0);
                double start = 0.15 + 0.55 * x;
                return Of(colors[0], Smooth(0, 0.08, phase) * (1 - Smooth(start, start + 0.2, phase)));
            }
            case 8:
            {
                const double Lap = 2.2;
                double behind = Frac(Frac(tau / Lap) - x);
                double level = behind < 0.45 ? Math.Pow(1 - behind / 0.45, 2) : 0;
                return Of(colors[(int)((long)Math.Floor(tau / Lap) & 3)], level);
            }
            case 9:
            {
                double width = Math.Max(2.0 / n, 0.2);
                double p = Frac(tau / 2.4);
                double at = (p < 0.5 ? p * 2 : 2 - p * 2) * (1 - width);
                double center = at + width / 2;
                double bar = Smooth(width / 2 + 0.5 / n, width / 2 - 0.5 / n, Math.Abs(x + 0.5 / n - center));
                return Blend(Of(colors[1], 1), Of(colors[0], 1), bar);
            }
            case 10:
                return Of(colors[0], Bricks(i, n, tau));
            case 11:
            {
                int size = Math.Max(2, n / 8);
                long beat = (long)Math.Floor(tau * 2.5);
                long segment = (long)Math.Floor((double)i / size) - beat;
                return Of(colors[(int)(Hash(seed, (int)segment, 11) * 4) & 3], 1);
            }
            case 12:
            {
                const double Period = 2.4;
                double phase = Frac(tau / Period);
                double radius = phase < 0.5 ? phase * 2 : 2 - phase * 2;
                double d = Math.Abs(x + 0.5 / n - 0.5) * 2;
                double lit = Smooth(radius + 1.0 / n, radius - 1.0 / n, d);
                return Of(colors[(int)((long)Math.Floor(tau / Period) & 3)], lit);
            }
            case 13:
            {
                // Relâmpagos de verdade em segundos (no máximo 2 clarões a cada 0,9 s, abaixo do limite de 3 por segundo).
                double slot = Math.Max(0.9, 1.6 / Rate(s.Velocidade));
                long k = (long)Math.Floor(t / slot);
                double local = t - k * slot - Hash(seed, (int)k, 13) * (slot - 0.4);
                bool strike = Hash(seed, (int)k, 17) < 0.7 && ((local >= 0 && local < 0.08) || (local >= 0.17 && local < 0.29));
                return strike ? Of(colors[0], 1) : Of(colors[1], 0.35);
            }
            case 14:
            {
                long step = (long)Math.Floor(tau * 10) % (n + 4);
                double level = i == step ? 1 : i == step - 1 ? 0.4 : i == step - 2 ? 0.15 : 0;
                return Of(colors[0], level);
            }
            default:
                return Of(colors[0], 1);
        }
    }

    /// <summary>
    /// Blocos: um bloco (1/24 da fita, pelo menos 1 LED) cai da ponta de lá e para em cima da pilha na de cá; cheia, a
    /// pilha fica um instante e apaga, e começa de novo.
    /// </summary>
    private static double Bricks(int i, int n, double tau)
    {
        int size = Math.Max(1, (int)Math.Ceiling(n / 24.0));
        int slots = (n + size - 1) / size;
        double Fall(int k) => 0.05 + 0.35 * (slots - k) / slots;
        double total = 0;
        for (int k = 0; k < slots; k++) total += Fall(k);
        const double Hold = 0.6, Clear = 0.4;
        double u = Frac(tau / (total + Hold + Clear)) * (total + Hold + Clear);
        int slot = i / size;
        double elapsed = 0;
        for (int k = 0; k < slots; k++)
        {
            double duration = Fall(k);
            if (u < elapsed + duration)
            {
                if (slot < k) return 1;
                double f = (u - elapsed) / duration;
                double at = slots - (slots - k) * f * f;
                return slot + 0.5 >= at && slot + 0.5 <= at + 1 ? 1 : 0;
            }
            elapsed += duration;
        }
        double after = u - total;
        return after < Hold ? 1 : 1 - Math.Clamp((after - Hold) / Clear, 0, 1);
    }

    // ---------------------------------------------------------------- utilidades

    public static Color[] Colors(UmbraSlot s) => s.Cores.Select(GpuLighting.ParseColor).ToArray();

    /// <summary>Cor escolhida num nível como o olho vê (0–1).</summary>
    private static (double R, double G, double B) Of(Color c, double level)
    {
        double k = Linear(level);
        return (c.R / 255.0 * k, c.G / 255.0 * k, c.B / 255.0 * k);
    }

    private static (double R, double G, double B) Scale((double R, double G, double B) c, double k) => (c.R * k, c.G * k, c.B * k);

    private static (double R, double G, double B) Blend((double R, double G, double B) a, (double R, double G, double B) b, double k) =>
        (a.R + (b.R - a.R) * k, a.G + (b.G - a.G) * k, a.B + (b.B - a.B) * k);

    private static double Breath(double phase) => Math.Pow(Math.Sin(Math.PI * phase), 2);

    private static (double R, double G, double B) Hue(double hue)
    {
        double h = hue * 6;
        double x = 1 - Math.Abs(h % 2 - 1);
        return (int)h switch
        {
            0 => (1, x, 0),
            1 => (x, 1, 0),
            2 => (0, 1, x),
            3 => (0, x, 1),
            4 => (x, 0, 1),
            _ => (1, 0, x),
        };
    }

    private static double Linear(double level) => Math.Pow(Math.Clamp(level, 0, 1), 2.2);

    private static double Smooth(double e0, double e1, double x)
    {
        double k = Math.Clamp((x - e0) / (e1 - e0), 0, 1);
        return k * k * (3 - 2 * k);
    }

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
