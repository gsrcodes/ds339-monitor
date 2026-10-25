using System.Runtime.InteropServices;

namespace DS339Monitor.Lighting;

/// <summary>
/// Efeitos brancos "no tempo" para o LED da placa — uma cor só para a placa inteira, então tudo é intensidade, ritmo e
/// tom do branco. Cada efeito devolve o nível (0–1, na escala do olho) e o tom (&gt; 0 mais quente, &lt; 0 mais frio,
/// no máximo um tom de lâmpada). Inspirados no catálogo do WLED (Twinklefox, Fireworks, Heartbeat, Candle), nas cenas
/// do Philips Hue (Glisten, Fireplace) e na luz de repouso dos MacBooks (respiração ~12 por minuto). Todos nascem
/// dentro do limite de clarões: os picos grandes são espaçados e o que é rápido é raso (abaixo de 10% da luz).
/// </summary>
internal sealed class WhiteMotion
{
    private double _heartPhase;
    private double _bpm = HeartRest;
    private double _load = -1;
    private double _ember;
    private uint _lastInput;
    private long _pomodoroStart = -1;

    private const double HeartRest = 54, HeartMax = 72; // "tum-tum" = 2 clarões: até 72 bpm fica dentro de 3 por segundo

    /// <summary>Para a simulação: relógio de entrada falso (no lugar do GetLastInputInfo).</summary>
    public static Func<long, uint>? SimulatedInput;

    /// <summary>O mesmo, só para esta instância (as prévias do painel, sem mexer no LED de verdade).</summary>
    public Func<long, uint>? Input { get; set; }

    public void Reset()
    {
        _heartPhase = 0;
        _bpm = HeartRest;
        _load = -1;
        _ember = 0;
        _lastInput = 0;
        _pomodoroStart = -1;
    }

    // ---------------------------------------------------------------- ritmo

    /// <summary>
    /// Coração da máquina: "tum-tum" (lub forte, dub mais fraco 0,26 s depois) sobre um fundo morno. Em repouso, 54 por
    /// minuto; com a GPU trabalhando, acelera até 72 — a frequência anda devagar, como um coração de verdade.
    /// </summary>
    public (double Level, double Warmth) Heartbeat(double t, double dt, float gpuLoad)
    {
        double load = float.IsNaN(gpuLoad) ? 0 : Math.Clamp(gpuLoad / 100.0, 0, 1);
        double target = HeartRest + (HeartMax - HeartRest) * load;
        _bpm += (target - _bpm) * (1 - Math.Exp(-dt / 4));
        _heartPhase = (_heartPhase + dt * _bpm / 60) % 1;
        double x = _heartPhase * 60 / _bpm; // segundos desde a batida
        double level = 0.28 + Choreo.Pulse(x, 0.62, 0.06, 0.13) + Choreo.Pulse(x - 0.26, 0.34, 0.05, 0.13);
        return (level, 0.25);
    }

    /// <summary>
    /// Respiro (sono): a luz de repouso dos MacBooks — 12 respirações por minuto, inspira mais rápido do que expira,
    /// segura um instante em cima e descansa quase apagada embaixo.
    /// </summary>
    public static (double Level, double Warmth) SleepBreath(double t)
    {
        const double Period = 5.0;
        double p = t % Period;
        double w = p < 1.8 ? Ease(p / 1.8) : p < 2.1 ? 1 : p < 4.5 ? 1 - Ease((p - 2.1) / 2.4) : 0;
        return (0.06 + 0.74 * w, 0);
    }

    /// <summary>
    /// Respiração guiada 4-7-8: inspira 4 s (a luz sobe e esfria), segura 7 s (quase parada), expira 8 s (desce e
    /// esquenta). Dá para respirar junto.
    /// </summary>
    public static (double Level, double Warmth) GuidedBreath(double t)
    {
        double p = t % 19;
        if (p < 4) return (Choreo.Lerp(0.12, 0.85, Ease(p / 4)), Choreo.Lerp(0.3, -0.25, Ease(p / 4)));
        if (p < 11) return (0.85 - 0.04 * (p - 4) / 7 + 0.015 * Math.Sin(2 * Math.PI * (p - 4) / 3.5), -0.25);
        double e = Ease((p - 11) / 8);
        return (Choreo.Lerp(0.81, 0.12, e), Choreo.Lerp(-0.25, 0.3, e));
    }

    /// <summary>
    /// Pomodoro: 25 min de foco (branco firme e frio), avisando no último minuto (pulsa devagar); uma "badalada" de luz
    /// e 5 min de pausa (morna, respirando); a cada 4 focos, pausa de 15 min. Começa quando o efeito é escolhido.
    /// </summary>
    public (double Level, double Warmth) Pomodoro(long nowMs)
    {
        if (_pomodoroStart < 0) _pomodoroStart = nowMs;
        const double Focus = 25 * 60, Short = 5 * 60, Long = 15 * 60;
        const double Round = 4 * Focus + 3 * Short + Long;
        double s = (nowMs - _pomodoroStart) / 1000.0 % Round;
        for (int i = 0; i < 4; i++)
        {
            if (s < Focus)
            {
                double left = Focus - s;
                double level = 0.75;
                if (left < 60) level += 0.1 * Math.Sin(2 * Math.PI * s / 3) * Choreo.Smooth(60, 50, left);
                return (level + 0.25 * Choreo.Pulse(s, 1, 0.6, 0.5), -0.15);
            }
            s -= Focus;
            double pause = i == 3 ? Long : Short;
            if (s < pause) return Break(s, pause);
            s -= pause;
        }
        return (0.75, -0.15);
    }

    private static (double, double) Break(double s, double length)
    {
        double chime = 0.3 * Choreo.Pulse(s, 1, 0.5, 0.8);
        double breathe = 0.47 + 0.12 * Math.Sin(2 * Math.PI * s / 8);
        double toFocus = Choreo.Smooth(length - 30, length, s); // os últimos 30 s acordam para o foco
        return (Choreo.Lerp(breathe, 0.75, toFocus) + chime, Choreo.Lerp(0.5, -0.15, toFocus));
    }

    // ---------------------------------------------------------------- natureza

    /// <summary>
    /// Vaga-lumes: quase escuro e morno, e piscadas suaves soltas (sobe em 0,25 s, apaga em 0,7 s), às vezes em dupla —
    /// como um vaga-lume respondendo a outro.
    /// </summary>
    public static (double Level, double Warmth) Fireflies(double t, double speed)
    {
        double window = 1.7 / speed;
        long k = (long)Math.Floor(t / window);
        double level = 0.06;
        for (long j = k - 2; j <= k; j++)
        {
            if (Choreo.Hash(j * 3 + 1) > 0.55) continue;
            double at = j * window + window * 0.8 * Choreo.Hash(j * 3 + 2);
            double peak = 0.35 + 0.45 * Choreo.Hash(j * 3 + 3);
            level += peak * Blink(t - at);
            if (Choreo.Hash(j * 7 + 5) < 0.25) level += 0.7 * peak * Blink(t - at - 0.55);
        }
        return (Math.Min(level, 0.95), 0.6);
    }

    private static double Blink(double x) => x < 0 || x > 1.05 ? 0 : x < 0.25 ? Ease(x / 0.25) : x < 0.33 ? 1 : 1 - Ease((x - 0.33) / 0.72);

    /// <summary>
    /// Céu estrelado (o "Twinklefox" do WLED numa luz só): um branco frio com sete "estrelas" que cintilam cada uma no
    /// seu ritmo — um brilho vivo, raso o bastante para nunca piscar.
    /// </summary>
    public static (double Level, double Warmth) StarrySky(double t, double speed)
    {
        double level = 0.36;
        for (int i = 0; i < 7; i++)
        {
            double period = (1.8 + 2.7 * Choreo.Hash(i + 40)) / speed;
            double phase = Choreo.Hash(i + 50);
            double s = Math.Max(0, Math.Sin(2 * Math.PI * (t / period + phase)));
            level += (0.035 + 0.035 * Choreo.Hash(i + 60)) * s * s * s;
        }
        return (level, -0.35);
    }

    /// <summary>
    /// Sombra de nuvens: um dia claro e, de vez em quando, uma nuvem passa devagar — a luz cai pela metade e fica um
    /// pouco mais fria na sombra, com bordas macias.
    /// </summary>
    public static (double Level, double Warmth) Clouds(double t)
    {
        double cover = Choreo.Smooth(0.5, 0.85, Choreo.SlowNoise(t, 0.06, 7));
        double thin = 0.15 * Choreo.Smooth(0.4, 0.9, Choreo.SlowNoise(t, 0.2, 9)); // véus finos dentro da nuvem
        double shade = Math.Min(1, cover + thin * cover);
        return (0.84 * (1 - 0.48 * shade), Choreo.Lerp(0.2, -0.25, shade));
    }

    /// <summary>
    /// Garoa: penumbra fria e gotas — cada uma acende rápido e apaga devagar, de tamanhos diferentes —, com pancadas
    /// mais fortes de vez em quando.
    /// </summary>
    public static (double Level, double Warmth) Drizzle(double t, double speed)
    {
        double shower = Choreo.Smooth(0.55, 0.8, Choreo.SlowNoise(t, 0.05, 3)); // pancada: o dobro de gotas
        double window = 0.5 / speed;
        long k = (long)Math.Floor(t / window);
        double level = 0.34;
        for (long j = k - 4; j <= k; j++)
        {
            if (Choreo.Hash(j * 5 + 11) > 0.35 + 0.35 * shower) continue;
            double at = j * window + window * Choreo.Hash(j * 5 + 12);
            level += (0.06 + 0.13 * Choreo.Hash(j * 5 + 13)) * Choreo.Pulse(t - at, 1, 0.03, 0.22);
        }
        return (Math.Min(level, 0.8), -0.3);
    }

    /// <summary>
    /// Fogos de artifício: noite e, a cada 7–16 s, um foguete subindo (um brilho fraco que cresce), o estouro e a queda
    /// devagar com faíscas miúdas — às vezes um segundo estouro logo depois. Dourado num, prateado no outro.
    /// </summary>
    public static (double Level, double Warmth) Fireworks(double t, double speed)
    {
        double level = 0.08, warmth = -0.2;
        double window = 11.5 / speed;
        long k = (long)Math.Floor(t / window);
        for (long j = k - 1; j <= k; j++)
        {
            double launch = j * window + (window - 4) * Choreo.Hash(j + 300);
            double x = t - launch;
            if (x < 0 || x > 6) continue;
            bool gold = Choreo.Hash(j + 301) < 0.5;
            if (x < 1.1) level += 0.12 * Ease(x / 1.1) + 0.015 * Math.Sin(2 * Math.PI * 7 * x); // o rastro subindo
            double burst = Choreo.Pulse(x - 1.1, 0.85, 0.05, 0.7);
            double echo = Choreo.Hash(j + 302) < 0.4 ? Choreo.Pulse(x - 1.8, 0.5, 0.05, 0.5) : 0;
            level += burst + echo;
            if (burst + echo is > 0.03 and < 0.35) level += 0.03 * (Choreo.Hash((long)(t * 9) + j * 100) - 0.5); // faíscas
            if (burst + echo > 0.05) warmth = Choreo.Lerp(warmth, gold ? 0.6 : -0.45, Math.Min(1, (burst + echo) * 2));
        }
        return (Math.Min(level, 1), warmth);
    }

    /// <summary>
    /// Lâmpada fluorescente: o branco frio de escritório com um zumbido imperceptível e, a cada minuto mais ou menos, o
    /// reator falhando — duas a quatro piscadas espaçadas e, às vezes, a lâmpada apagando e reacendendo como na partida.
    /// </summary>
    public static (double Level, double Warmth) Fluorescent(double t)
    {
        double level = 0.78 + 0.008 * Math.Sin(2 * Math.PI * 9 * t);
        const double Window = 60;
        long k = (long)Math.Floor(t / Window);
        double x = t - (k * Window + 40 * Choreo.Hash(k + 500));
        if (x < 0 || x > 5) return (level, -0.45);
        if (Choreo.Hash(k + 501) < 0.7)
        {
            int dips = 2 + (int)(Choreo.Hash(k + 502) * 3);
            for (int i = 0; i < dips; i++)
            {
                double at = i * (0.36 + 0.14 * Choreo.Hash(k * 10 + i));
                if (x >= at && x < at + 0.09) level = 0.32 + 0.1 * Choreo.Hash(k * 10 + i + 5);
            }
        }
        else
        {
            // Reacende: apaga, duas tentativas e acende subindo, como a partida de um reator.
            if (x < 0.6) level = 0.05;
            else if (x < 0.7 || (x >= 1.1 && x < 1.2)) level = 0.6;
            else if (x < 1.6) level = 0.08;
            else level = Choreo.Lerp(0.3, level, Choreo.Smooth(1.6, 3.2, x));
        }
        return (level, -0.45);
    }

    // ---------------------------------------------------------------- reagem ao PC

    /// <summary>
    /// Brasa: um carvão morno quase apagado que aviva enquanto você digita ou mexe no mouse — cada toque é um sopro — e
    /// volta a cochilar devagar. Só percebe que houve uma entrada (GetLastInputInfo), nunca o quê.
    /// </summary>
    public (double Level, double Warmth) Ember(long nowMs, double t, double dt)
    {
        uint input = Input?.Invoke(nowMs) ?? SimulatedInput?.Invoke(nowMs) ?? LastInput();
        if (_lastInput != 0 && input != _lastInput) _ember = Math.Min(3, _ember + 0.35);
        _lastInput = input;
        _ember *= Math.Exp(-dt / 2.5);
        double glow = 1 - Math.Exp(-_ember);
        double flicker = 0.03 * (Choreo.SlowNoise(t, 1.5, 21) * 2 - 1);
        return (0.12 + 0.68 * glow + flicker * (0.4 + glow), Choreo.Lerp(0.95, 0.15, glow));
    }

    /// <summary>
    /// Turbina: a GPU em branco. Parada, gira devagar (respira baixo); trabalhando, a luz sobe com o uso e ganha um
    /// zumbido de rotação (raso); o tom acompanha a temperatura — frio a 40 °C, morno a 80 °C. Sobe depressa e desacelera
    /// devagar, como uma turbina.
    /// </summary>
    public (double Level, double Warmth) Turbine(double t, double dt, float gpuLoad, float gpuTemp)
    {
        double load = float.IsNaN(gpuLoad) ? 0 : Math.Clamp(gpuLoad / 100.0, 0, 1);
        if (_load < 0) _load = load;
        _load += (load - _load) * (1 - Math.Exp(-dt / (load > _load ? 1.5 : 4)));
        double idle = 1 - Choreo.Smooth(0.03, 0.15, _load);
        double level = 0.25 + 0.65 * _load + idle * 0.05 * Math.Sin(2 * Math.PI * t / 10)
            + 0.02 * _load * Math.Sin(2 * Math.PI * (2 + 6 * _load) * t);
        double warmth = float.IsNaN(gpuTemp) ? 0 : Choreo.Lerp(-0.4, 0.6, Choreo.Smooth(40, 80, gpuTemp));
        return (level, warmth);
    }

    private static double Ease(double x)
    {
        x = Math.Clamp(x, 0, 1);
        return 0.5 - 0.5 * Math.Cos(Math.PI * x);
    }

    private static uint LastInput()
    {
        var info = new LastInputInfo { Size = (uint)Marshal.SizeOf<LastInputInfo>() };
        return GetLastInputInfo(ref info) ? info.Time : 0;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct LastInputInfo
    {
        public uint Size;
        public uint Time;
    }

    [DllImport("user32.dll")]
    private static extern bool GetLastInputInfo(ref LastInputInfo info);
}
