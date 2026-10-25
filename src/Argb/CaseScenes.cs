using System.Drawing;
using DS339Monitor.Manga;

namespace DS339Monitor.Argb;

/// <summary>O que as cenas do gabinete sabem do mundo neste quadro.</summary>
internal sealed class SceneInput
{
    /// <summary>Segundos (relógio do programa).</summary>
    public double T;

    public long NowMs;

    /// <summary>0,6 (lenta), 1 ou 1,6 (rápida).</summary>
    public double Speed = 1;

    public double GpuTemp = double.NaN;
    public double CpuTemp = double.NaN;
    public double GpuLoad = double.NaN;

    /// <summary>Brilho médio da Pro Flow, suavizado (0–1).</summary>
    public double Screen = 0.5;

    /// <summary>O que o Modo Mangá está encenando (null = modo desligado).</summary>
    public MangaState? Manga;
}

/// <summary>
/// Cenas que atravessam o gabinete inteiro, calculadas pela posição de cada LED (ver <see cref="CaseLayout"/>): a luz
/// sobe dos fans do piso, explode da bomba, enche o gabinete como um aquário, segue o caminho do ar até o fan de trás.
/// Tudo é função do tempo e da posição — sem estado —, então qualquer LED (e o LED da placa, e as prévias) calcula a
/// mesma cena. Os níveis são na escala do olho (0–1); a cor sai em fração de luz por canal.
/// </summary>
internal static class CaseScenes
{
    private readonly record struct Light(double Level, Tone Tint, double Mix);

    private readonly record struct Tone(double R, double G, double B);

    private static readonly Tone Cool = new(0.76, 0.87, 1.0);
    private static readonly Tone Warm = new(1.0, 0.74, 0.46);
    private static readonly Tone Gold = new(1.0, 0.78, 0.36);
    private static readonly Tone Neutral = new(1, 1, 1);

    public static (double R, double G, double B) Shade(string effect, Color color, in LedPoint p, SceneInput s)
    {
        var light = effect switch
        {
            ArgbEffects.KnockUp => KnockUp(p, s),
            ArgbEffects.Haki => Haki(p, s),
            ArgbEffects.Aquarium => Aquarium(p, s),
            ArgbEffects.Airflow => Airflow(p, s),
            ArgbEffects.Rain => Rain(p, s),
            ArgbEffects.Beacon => Beacon(p, s),
            ArgbEffects.Drums => new Light(0.16 + Drums(p, Frac(s.T * s.Speed / 2.4) * 2.4, 1), Warm, 0.15),
            ArgbEffects.HeatMap => HeatMap(p, s),
            ArgbEffects.MangaScene => MangaScene(p, s),
            _ => new Light(1, Neutral, 0),
        };
        double k = Math.Pow(Math.Clamp(light.Level, 0, 1), 2.2);
        double r = color.R / 255.0, g = color.G / 255.0, b = color.B / 255.0;
        double m = Math.Clamp(light.Mix, 0, 1);
        return ((r + (light.Tint.R - r) * m) * k, (g + (light.Tint.G - g) * m) * k, (b + (light.Tint.B - b) * m) * k);
    }

    // ---------------------------------------------------------------- cenas

    /// <summary>
    /// Knock Up Stream: a corrente sobe dos fans do piso até o topo (passando pelo radiador, pelos cabos, pela bomba),
    /// fica um instante lá em cima, cai de volta em gotas e o piso junta força de novo. Uma vez a cada 7 s.
    /// </summary>
    private static Light KnockUp(in LedPoint p, SceneInput s)
    {
        double cycle = s.T * s.Speed / 7.0;
        double x = Frac(cycle) * 7.0;
        double column = 0.72 + 0.28 * Band(p.U - 0.47, 0.42); // mais forte no meio, onde a corrente passa
        double level;
        if (x < 1.6)
        {
            double front = 1.08 - 1.2 * EaseOut(x / 1.6) + 0.035 * Math.Sin(p.U * 17 + s.T * 9);
            level = p.V > front ? (0.5 + 0.5 * Band(p.V - front, 0.14)) * column : 0.15 + 0.85 * Band(p.V - front, 0.05);
        }
        else if (x < 2.6)
        {
            level = (0.62 + 0.1 * Math.Sin(2 * Math.PI * (x * 1.2 + p.U * 1.7 + p.V * 2.3))) * column;
        }
        else if (x < 5.2)
        {
            double y = (x - 2.6) / 2.6;
            double front = -0.08 + 1.2 * y * y; // a água volta caindo
            if (p.V < front)
            {
                level = 0.15;
                int n = (int)Math.Floor(cycle);
                for (int k = 0; k < 8; k++)
                {
                    double du = 0.06 + 0.88 * Hash(k, n, 11);
                    double dv = front - 0.05 - 0.3 * Hash(k, n, 12);
                    level += 0.55 * Band(p.U - du, 0.03) * Band(p.V - dv, 0.035);
                }
            }
            else
            {
                level = (0.62 * (1 - 0.55 * y) + 0.3 * Band(p.V - front, 0.05)) * column;
            }
        }
        else
        {
            level = 0.15 + 0.4 * Smooth(5.2, 7.0, x) * Smooth(0.72, 1.0, p.V);
        }
        return new Light(level, Cool, 0.25);
    }

    /// <summary>
    /// Haki do Rei: a bomba (a CPU, atrás da Pro Flow) junta força e solta uma onda de choque que atravessa o gabinete,
    /// com um eco logo atrás. A cada 4,5 s.
    /// </summary>
    private static Light Haki(in LedPoint p, SceneInput s)
    {
        double x = Frac(s.T * s.Speed / 4.5) * 4.5;
        double d = Dist(p, CaseLayout.Pump);
        double charge = x < 1.2 ? Smooth(0, 1.2, x) : 1 - Smooth(1.2, 2.6, x);
        double level = 0.15 + (0.3 + 0.7 * charge) * Band(d, 0.22);
        level += Shock(d, x - 1.2, 0.8, 1.0) + 0.6 * Shock(d, x - 1.45, 0.75, 1.0) + 0.35 * Shock(d, x - 1.7, 0.7, 1.0);
        return new Light(level, Cool, 0.15);
    }

    /// <summary>
    /// Aquário: o gabinete de vidro enche e esvazia devagar. Debaixo d'água, luz de piscina dançando (cáusticas) e
    /// bolhas subindo dos fans do piso; a superfície é uma linha clara com ondinhas; acima dela, quase escuro.
    /// </summary>
    private static Light Aquarium(in LedPoint p, SceneInput s)
    {
        double t = s.T * s.Speed;
        double surface = 0.52 + 0.3 * Math.Sin(2 * Math.PI * t / 20) + 0.018 * Math.Sin(p.U * 16 + t * 2.1) + 0.01 * Math.Sin(p.U * 31 - t * 3.3);
        double level;
        if (p.V > surface)
        {
            double caustic = (0.5 + 0.5 * Math.Sin(p.U * 11 + t * 1.2 + 1.4 * Math.Sin(p.V * 7 - t * 0.8)))
                * (0.5 + 0.5 * Math.Sin(p.V * 9 - t * 1.1 + p.U * 5));
            level = 0.36 + 0.32 * caustic + 0.85 * Band(p.V - surface, 0.025);
            for (int k = 0; k < 7; k++)
            {
                double period = 5 + 3 * Hash(k, 0, 9);
                double phase = t / period + Hash(k, 0, 7);
                int n = (int)Math.Floor(phase);
                double bu = 0.08 + 0.85 * Hash(k, n, 3) + 0.02 * Math.Sin(t * 3 + k);
                double bv = 1.0 - Frac(phase) * 1.1;
                if (bv > surface) level += 0.7 * Band(p.U - bu, 0.025) * Band(p.V - bv, 0.03);
            }
        }
        else
        {
            level = 0.12 + 0.6 * Band(p.V - surface, 0.03);
        }
        return new Light(level, Cool, 0.35);
    }

    /// <summary>
    /// Fluxo de ar: o caminho de verdade do ar no gabinete. Entra pelos fans do piso (subindo) e pelo radiador (vindo da
    /// direita), atravessa tudo e sai pelo fan de trás, que brilha mais morno (ar quente saindo). Os centros dos fans
    /// giram. Mais uso da placa de vídeo = mais ar, mais rápido.
    /// </summary>
    private static Light Airflow(in LedPoint p, SceneInput s)
    {
        double load = double.IsNaN(s.GpuLoad) ? 20 : s.GpuLoad;
        double flow = 0.7 + 0.9 * Math.Clamp(load / 100, 0, 1);
        double t = s.T * s.Speed * flow;
        var (eu, ev) = CaseLayout.Exhaust;
        double level = 0.18;
        for (int k = 0; k < 16; k++)
        {
            double life = 2.8 + 1.4 * Hash(k, 0, 1);
            double phase = t / life + Hash(k, 0, 2);
            int n = (int)Math.Floor(phase);
            double prog = Frac(phase);
            double u, v;
            if (k < 9)
            {
                double u0 = 0.1 + 0.8 * Hash(k, n, 3);
                u = u0 + (eu - u0) * Smooth(0.3, 1, prog);
                v = 0.97 - (0.97 - ev) * EaseOut(prog);
            }
            else
            {
                double v0 = 0.12 + 0.75 * Hash(k, n, 4);
                u = 0.97 - (0.97 - eu) * prog;
                v = v0 + (ev - v0) * Smooth(0.2, 1, prog);
            }
            double fade = Smooth(0, 0.1, prog) * (1 - Smooth(0.85, 1, prog));
            double du = p.U - u, dv = p.V - v;
            level += 0.75 * fade * Math.Exp(-(du * du + dv * dv) / (2 * 0.045 * 0.045));
        }
        if (p.OnRing) level += 0.45 * Math.Pow(0.5 + 0.5 * Math.Cos(p.Angle - 2 * Math.PI * (0.6 + 0.6 * flow) * s.T), 6);
        double exhaust = Band(Dist(p, CaseLayout.Exhaust), 0.13);
        level += 0.25 * exhaust * flow;
        return exhaust > 0.3 ? new Light(level, Warm, 0.5 * exhaust) : new Light(level, Cool, 0.2);
    }

    // Onde há linhas verticais de LEDs: as laterais do fan de trás, as memórias e as laterais do radiador.
    private static readonly double[] RainLanes = { 0.037, 0.195, 0.58, 0.605, 0.746, 0.97 };

    /// <summary>Chuva: gotas caindo do topo (escorrendo pelas laterais e pelas memórias) e respingando nos fans do piso.</summary>
    private static Light Rain(in LedPoint p, SceneInput s)
    {
        double level = 0.16;
        double t = s.T * s.Speed;
        for (int k = 0; k < 20; k++)
        {
            double period = 1.3 + 0.8 * Hash(k, 0, 1);
            double phase = t / period + Hash(k, 0, 2);
            int n = (int)Math.Floor(phase);
            double fall = Frac(phase);
            double u = k % 5 < 3 ? RainLanes[(int)(Hash(k, n, 5) * RainLanes.Length) % RainLanes.Length] : 0.05 + 0.9 * Hash(k, n, 6);
            double drop = -0.1 + 1.12 * Math.Pow(fall, 1.5);
            double behind = drop - p.V;
            if (behind >= 0 && behind < 0.2) level += 0.85 * Band(p.U - u, 0.025) * Math.Pow(1 - behind / 0.2, 2);
            if (fall > 0.88 && p.V > 0.84)
            {
                double age = (fall - 0.88) * period;
                level += 0.6 * Math.Exp(-age * 6) * Band(p.U - u, 0.04 + age * 0.3);
            }
        }
        return new Light(level, Cool, 0.4);
    }

    /// <summary>Farol: a bomba é a lâmpada e um facho gira em volta dela, varrendo o gabinete a cada 6 s.</summary>
    private static Light Beacon(in LedPoint p, SceneInput s)
    {
        double angle = 2 * Math.PI * s.T * s.Speed / 6;
        double phi = Math.Atan2(p.V - CaseLayout.Pump.V, p.U - CaseLayout.Pump.U);
        double diff = Math.IEEERemainder(phi - angle, 2 * Math.PI);
        double beam = Math.Exp(-diff * diff / (0.28 * 0.28));
        double after = diff < 0 ? Math.Exp(diff * 1.6) : 0; // o rastro de onde o facho acabou de passar
        double d = Dist(p, CaseLayout.Pump);
        double level = 0.14 + (0.9 * beam + 0.3 * after) * (0.55 + 0.45 * Math.Exp(-d * 1.5)) + 0.7 * Band(d, 0.22);
        return new Light(level, Warm, 0.12);
    }

    /// <summary>
    /// Tambores da Libertação (Gear 5): DON do-tto-to — cada batida é uma onda que sobe dos fans do piso (o tambor); o
    /// DON chega até o topo, as batidas pequenas só até o meio. <paramref name="x"/> = segundos dentro do compasso de 2,4 s.
    /// </summary>
    private static double Drums(in LedPoint p, double x, double strength)
    {
        ReadOnlySpan<double> at = stackalloc double[] { 0, 0.8, 1.1, 1.4 };
        ReadOnlySpan<double> power = stackalloc double[] { 1.0, 0.5, 0.55, 0.8 };
        double level = 0;
        for (int i = 0; i < at.Length; i++)
        {
            double age = x - at[i];
            if (age < 0) age += 2.4;
            if (age > 1.2) continue;
            double a = power[i] * strength;
            double front = 1.0 - age * 1.5 * (0.6 + 0.4 * power[i]);
            double reach = 1 - power[i];
            if (front > reach - 0.1) level += 0.9 * a * Band(p.V - front, 0.08) * Math.Exp(-age * 1.6);
            if (p.V > 0.84) level += 0.8 * a * Math.Exp(-age * 7);
        }
        return level;
    }

    /// <summary>
    /// Mapa de calor: a CPU (na bomba) e a placa de vídeo esquentam o ar em volta, e o calor sobe; os fans que puxam ar
    /// de fora (piso e radiador) ficam num branco frio. Do branco frio ao morno, e laranja só quando esquenta de verdade.
    /// </summary>
    private static Light HeatMap(in LedPoint p, SceneInput s)
    {
        double cpu = Math.Clamp(((double.IsNaN(s.CpuTemp) ? 45 : s.CpuTemp) - 35) / 50, 0, 1);
        double gpu = Math.Clamp(((double.IsNaN(s.GpuTemp) ? 40 : s.GpuTemp) - 30) / 55, 0, 1);
        double heat = cpu * Plume(p, CaseLayout.Pump, 0.26) + gpu * Plume(p, CaseLayout.GpuCenter, 0.34);
        heat += 0.08 * Math.Sin(p.U * 9 + s.T * 1.3 + p.V * 7 - s.T * 0.9); // o ar quente tremendo
        double intake = Math.Max(Smooth(0.82, 0.95, p.V), Smooth(0.72, 0.76, p.U));
        heat = Math.Clamp(heat * (1 - 0.6 * intake), 0, 1);
        Tone tone = heat < 0.4 ? Mix(new Tone(0.8, 0.88, 1.0), new Tone(1.0, 0.96, 0.92), heat / 0.4)
            : heat < 0.7 ? Mix(new Tone(1.0, 0.96, 0.92), Warm, (heat - 0.4) / 0.3)
            : Mix(Warm, new Tone(1.0, 0.45, 0.12), (heat - 0.7) / 0.3);
        return new Light(0.5 + 0.45 * heat, tone, 1);
    }

    /// <summary>Calor em volta de uma fonte, esticado para cima (o ar quente sobe).</summary>
    private static double Plume(in LedPoint p, (double U, double V) source, double radius)
    {
        double du = p.U - source.U, dv = p.V - source.V;
        if (dv < 0) dv *= 0.55;
        return Math.Exp(-(du * du + dv * dv) / (radius * radius));
    }

    /// <summary>
    /// Mangá no gabinete: a luz acompanha a tinta da página da Pro Flow e, com o Modo Mangá ligado, cada momento vira
    /// uma cena no gabinete inteiro — o DON! sai da Pro Flow como onda de choque, o Haki escurece tudo e dispara anéis, os
    /// tambores sobem do piso, a página vira da direita para a esquerda (como no mangá), o "copiar" acende as memórias, o
    /// mudo apaga, o tesouro do pendrive brilha dourado e o Gear 5 explode em branco.
    /// </summary>
    private static Light MangaScene(in LedPoint p, SceneInput s)
    {
        double level = 0.16 + 0.42 * s.Screen;
        var state = s.Manga;
        if (state is null) return new Light(level, Cool, 0.1);

        long now = s.NowMs;
        double dPump = Dist(p, CaseLayout.Pump);
        double dim = 1, add = 0, warm = 0, cool = 0, gold = 0;
        foreach (var cue in state.Cues)
        {
            if (!cue.Alive(now)) continue;
            double x = cue.Age(now), dur = cue.Seconds;
            double env = Smooth(0, 0.3, x) * Smooth(0, 0.5, dur - x);
            switch (cue.Kind)
            {
                case MangaCueKind.Impact:
                    add += Shock(dPump, x, 1.5, 1.0) + 0.5 * Math.Exp(-x * 5) * Band(dPump, 0.15);
                    break;
                case MangaCueKind.Download:
                    add += Shock(dPump, x - 0.15, 1.2, 0.6);
                    break;
                case MangaCueKind.Drive:
                    add += Shock(dPump, x - 0.15, 1.1, 0.5) + 0.25 * env;
                    gold = Math.Max(gold, env);
                    break;
                case MangaCueKind.Haki:
                    dim *= 1 - 0.55 * env;
                    for (int k = 0; k < 4; k++) add += 0.9 * Shock(dPump, x - 0.3 - k * 0.9, 1.0, 1.0);
                    break;
                case MangaCueKind.Drums:
                    add += Drums(p, x % 2.4, 1) * env;
                    break;
                case MangaCueKind.Track:
                    add += 0.35 * Drums(p, x % 2.4, 1) * env;
                    break;
                case MangaCueKind.Quake:
                    add += 0.35 * env * (0.5 + 0.5 * Math.Sin(2 * Math.PI * 1.4 * x + (p.U < 0.5 ? 0 : Math.PI)));
                    break;
                case MangaCueKind.Tide:
                {
                    double surface = 1.05 - 1.1 * Math.Sin(Math.PI * Math.Clamp(x / dur, 0, 1));
                    add += env * ((p.V > surface ? 0.35 : 0) + 0.7 * Band(p.V - surface, 0.04));
                    cool = Math.Max(cool, env);
                    break;
                }
                case MangaCueKind.Farewell:
                {
                    double front = -0.1 + 1.3 * Math.Clamp(x / Math.Min(2.5, dur * 0.4), 0, 1);
                    add += env * ((p.V < front ? 0.6 : 0) + 0.5 * Band(p.V - front, 0.06));
                    break;
                }
                case MangaCueKind.Dawn:
                {
                    double rise = Smooth(0, dur * 0.6, x);
                    double rays = Math.Pow(0.5 + 0.5 * Math.Cos(7 * Math.Atan2(p.U - 0.5, 1.15 - p.V)), 4);
                    add += env * (0.45 * Smooth(1.05 - rise * 1.2, 1.2 - rise * 1.2, p.V) + 0.25 * rays * rise);
                    warm = Math.Max(warm, 0.7 * env);
                    break;
                }
                case MangaCueKind.TurnCut:
                case MangaCueKind.TurnHalftone:
                {
                    double front = 1.1 - 1.25 * Math.Clamp(x / 0.7, 0, 1); // da direita para a esquerda, como se lê mangá
                    add += 0.7 * Band(p.U - front, 0.06) * (1 - Smooth(0.6, 0.9, x));
                    break;
                }
                case MangaCueKind.Print:
                    add += 0.55 * Math.Exp(-x * 4) * Smooth(0, 0.03, x) * (1 - 0.5 * p.V);
                    break;
                case MangaCueKind.Copy:
                    add += 0.9 * Math.Sin(Math.PI * Math.Clamp(x / 0.6, 0, 1)) * Band(Dist(p, CaseLayout.Memory), 0.1);
                    break;
                case MangaCueKind.Ring:
                    for (int k = 0; k < 4; k++) add += 0.5 * env * Shock(dPump, x - k * 0.8, 0.9, 1.0);
                    break;
                case MangaCueKind.Mute:
                    dim *= 1 - 0.7 * env;
                    break;
                case MangaCueKind.Welcome:
                {
                    double k = Math.Clamp(x / 1.6, 0, 1);
                    add += 0.6 * Band(p.U - (-0.1 + 1.2 * k), 0.12) * Math.Sin(Math.PI * k);
                    break;
                }
                case MangaCueKind.Chapter:
                    add += 0.6 * env * Band(p.V - (-0.1 + 1.2 * Math.Clamp(x / 1.2, 0, 1)), 0.08);
                    break;
                case MangaCueKind.Menace:
                {
                    double near = Band(Dist(p, CaseLayout.GpuCenter), 0.45);
                    add += 0.4 * env * (0.5 + 0.5 * Math.Sin(2 * Math.PI * x / 1.3)) * near;
                    warm = Math.Max(warm, 0.8 * env * near);
                    break;
                }
                case MangaCueKind.Gear5:
                    add += Gear5(dPump, x, ref dim);
                    break;
            }
        }
        level = level * dim + add;
        if (gold > 0) return new Light(level, Gold, 0.7 * gold);
        if (warm >= cool && warm > 0) return new Light(level, Warm, warm);
        return new Light(level, Cool, 0.1 + 0.4 * cool);
    }

    /// <summary>
    /// O despertar do Gear 5 nos fans: o coração bate duas vezes na bomba, no despertar o branco explode dela e enche o
    /// gabinete, e o Nika fica pulando em ondas brancas até o véu branco devolver a cena.
    /// </summary>
    private static double Gear5(double dPump, double x, ref double dim)
    {
        double add = 0;
        foreach (double beat in MangaStage.Gear5Times.Heartbeats)
            if (x >= beat) add += 0.8 * Math.Exp(-(x - beat) * 6) * Band(dPump, 0.18);
        double awaken = x - MangaStage.Gear5Times.Awaken;
        if (awaken >= 0)
        {
            double r = awaken * 1.6;
            double fill = (dPump < r ? 0.8 : 0) + Band(dPump - r, 0.08);
            double bounce = 0.12 * Math.Sin(2 * Math.PI * awaken / 0.8 - dPump * 6);
            double fade = 1 - Smooth(MangaStage.Gear5Times.WashOut, MangaStage.Gear5Times.Seconds, x);
            add += (fill + (awaken > 0.8 ? bounce : 0)) * fade;
            dim *= 1 - 0.5 * fade; // o branco do Nika manda: a página fica por baixo
        }
        return add;
    }

    // ---------------------------------------------------------------- utilidades

    /// <summary>Onda de choque saindo de um ponto: um anel claro com rastro, que some ao chegar nas bordas.</summary>
    private static double Shock(double d, double age, double speed, double amplitude)
    {
        if (age < 0) return 0;
        double r = age * speed;
        double ring = Band(d - r, 0.07) * (1 - 0.6 * Smooth(0.5, 1.3, r));
        double trail = d < r ? 0.35 * Math.Exp(-(r - d) * 4) * (1 - Smooth(0.6, 1.5, r)) : 0;
        return amplitude * (ring + trail);
    }

    private static double Dist(in LedPoint p, (double U, double V) c)
    {
        double du = p.U - c.U, dv = p.V - c.V;
        return Math.Sqrt(du * du + dv * dv);
    }

    private static Tone Mix(Tone a, Tone b, double t)
    {
        t = Math.Clamp(t, 0, 1);
        return new Tone(a.R + (b.R - a.R) * t, a.G + (b.G - a.G) * t, a.B + (b.B - a.B) * t);
    }

    private static double Band(double x, double width) => Math.Exp(-x * x / (width * width));

    private static double Smooth(double e0, double e1, double x)
    {
        double t = Math.Clamp((x - e0) / (e1 - e0), 0, 1);
        return t * t * (3 - 2 * t);
    }

    private static double EaseOut(double t)
    {
        t = Math.Clamp(t, 0, 1);
        return 1 - (1 - t) * (1 - t);
    }

    private static double Frac(double v) => v - Math.Floor(v);

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
