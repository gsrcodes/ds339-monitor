namespace DS339Monitor.Lighting;

/// <summary>
/// Coreografia do Modo Grand Line: os tempos de cada momento, numa conta só, sem nada sorteado na hora. O LED da placa e
/// a tela Thermalright perguntam aqui "o que acontece no segundo s deste momento?" e chegam à mesma resposta — é isso
/// que faz o estalo do Haki na luz cair junto com o raio negro na tela, e o anel do Nika junto com a batida do tambor.
/// Regra de segurança de todos os clarões: no máximo 3 por segundo (WCAG 2.3.1).
/// </summary>
internal static class Choreo
{
    /// <summary>Atraso do DON! depois que a virada de painel começa (a divisa branca cruzando a tela).</summary>
    public const double TurnDon = 0.14;

    /// <summary>Clarões do "impacto": (instante no painel, força).</summary>
    public static readonly (double At, double Peak)[] Impact = { (0.15, 0.62), (0.70, 0.30) };

    /// <summary>Ciclo dos Tambores da Libertação dentro do painel do Gear 5, em s.</summary>
    public const double DrumCycle = 1.3;

    /// <summary>Batidas leves do "do-tto-to", em fração do ciclo.</summary>
    public static readonly double[] DrumLight = { 0.30, 0.42, 0.54 };

    /// <summary>
    /// Estalos do Haki do Rei até o instante <paramref name="until"/> do painel: uma rajada nos primeiros 2,5 s (no
    /// mínimo 0,34 s entre um e outro) e depois um raro e mais fraco, a cada 1,6–4,2 s.
    /// </summary>
    public static IEnumerable<(double At, double Peak, int Index)> HakiCrackles(double until)
    {
        double t = 0.1;
        for (int k = 0; t <= until && k < 400; k++)
        {
            bool burst = t < 2.5;
            yield return (t, (burst ? 0.5 : 0.3) + 0.3 * Hash(k * 7 + 1), k);
            t += burst ? 0.34 + 0.2 * Hash(k * 7 + 2) : 1.6 + 2.6 * Hash(k * 7 + 3);
        }
    }

    /// <summary>Instantes (no ciclo) em que o terremoto começa e termina dentro do painel "tremor".</summary>
    public const double QuakeStart = 0.9, QuakeEnd = 4.5;

    /// <summary>0 fora do tremor, 1 no meio dele.</summary>
    public static double Quake(double s) => s < QuakeStart || s > QuakeEnd ? 0 : Math.Sin(Math.PI * (s - QuakeStart) / (QuakeEnd - QuakeStart));

    // ---------------------------------------------------------------- acontecimentos do diretor de cena

    /// <summary>
    /// Raio, x segundos depois de cair (nível 0–1 na escala do olho): sobe num quadro, tem 1–2 "re-descargas" que só
    /// baixam até 0,92 (acima do limite que conta como clarão novo) e apaga em ~1 s.
    /// </summary>
    public static double Lightning(double x, int variant)
    {
        if (x < 0 || x > 1.6) return 0;
        if (x < 0.04) return x / 0.04;
        int strokes = 1 + variant % 3;
        for (int i = 1; i < strokes; i++)
        {
            double at = 0.05 + 0.08 * (i - 1);
            if (x >= at && x < at + 0.04) return 0.92;
        }
        double end = 0.05 + 0.08 * (strokes - 1);
        return x < end ? 1 : Math.Exp(-(x - end) / 0.35);
    }

    /// <summary>Duração do toque do Den Den Mushi ("purupuru") antes do "gacha", em s.</summary>
    public const double DenDenRing = 2.6;

    /// <summary>Toque do Den Den Mushi: dois ciclos de três toques a 3 por segundo, separados por uma pausa.</summary>
    public static double DenDenRings(double x)
    {
        if (x < 0 || x >= DenDenRing) return 0;
        double cycle = x % 1.3;
        if (cycle >= 1.0) return 0;
        double phase = cycle * 3 % 1; // 3 toques por segundo, no limite seguro
        return Math.Sin(Math.PI * phase);
    }

    /// <summary>Coup de Burst: carga, disparo e quando o Sunny volta navegando.</summary>
    public const double BurstCharge = 1.6, BurstLaunch = 1.2, BurstReturn = 9, BurstReturnSail = 4;

    /// <summary>Buster Call: quanto dura o alerta.</summary>
    public const double BusterCallSeconds = 10;

    /// <summary>Tiros de canhão ao longe no Buster Call: um a cada 1,5–3 s.</summary>
    public static IEnumerable<double> BusterShots()
    {
        double t = 0.8;
        for (int k = 0; t < BusterCallSeconds; k++)
        {
            yield return t;
            t += 1.5 + 1.5 * Hash(k * 13 + 5);
        }
    }

    /// <summary>"Bem-vindo de volta": varredura de luz que dura isto, em s.</summary>
    public const double WelcomeSweep = 1.2;

    // ---------------------------------------------------------------- matemática comum

    /// <summary>Pseudoaleatório fixo para cada número (0–1).</summary>
    public static double Hash(long i)
    {
        ulong h = (ulong)i * 0x9E3779B97F4A7C15UL;
        h ^= h >> 31;
        h *= 0xBF58476D1CE4E5B9UL;
        h ^= h >> 29;
        return (h & 0xFFFFFF) / (double)0xFFFFFF;
    }

    /// <summary>
    /// Ruído suave entre 0 e 1 que muda no máximo <paramref name="rate"/> vezes por segundo (valores sorteados em pontos
    /// fixos e interpolados): tremor sem virar pisca-pisca.
    /// </summary>
    public static double SlowNoise(double seconds, double rate, long seed = 0)
    {
        double x = seconds * rate;
        long i = (long)Math.Floor(x);
        double f = x - i;
        f = f * f * (3 - 2 * f);
        return Hash(i + seed) + (Hash(i + 1 + seed) - Hash(i + seed)) * f;
    }

    /// <summary>Batida de tambor: sobe em 40 ms e apaga com constante de 160 ms.</summary>
    public static double Hit(double t, double at, double peak)
    {
        double x = t - at;
        if (x < 0) return 0;
        return x < 0.04 ? peak * x / 0.04 : peak * Math.Exp(-(x - 0.04) / 0.16);
    }

    /// <summary>Clarão genérico: sobe em <paramref name="attack"/> s e apaga com constante <paramref name="decay"/> s.</summary>
    public static double Pulse(double x, double peak, double attack, double decay)
    {
        if (x < 0) return 0;
        return x < attack ? peak * x / attack : peak * Math.Exp(-(x - attack) / decay);
    }

    public static double Smooth(double from, double to, double x)
    {
        double t = Math.Clamp((x - from) / (to - from), 0, 1);
        return t * t * (3 - 2 * t);
    }

    public static double Lerp(double a, double b, double t) => a + (b - a) * t;
}
