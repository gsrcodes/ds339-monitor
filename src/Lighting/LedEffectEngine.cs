using System.Drawing;
using DS339Monitor.App;
using DS339Monitor.Themes;

namespace DS339Monitor.Lighting;

/// <summary>
/// A cor do LED quadro a quadro, sem tocar no hardware: a thread do LED (<see cref="GpuLighting"/>) e a simulação
/// (<c>--led-gpu-simular</c>) fazem a mesma conta.
/// </summary>
internal sealed class LedEffectEngine
{
    private const double FadeSeconds = 0.8;

    private static readonly (double At, Color Color)[] TemperatureStops =
    {
        (35, Rgb(0x1E6BFF)), (50, Rgb(0x00C2D1)), (62, Rgb(0x2EE06B)), (72, Rgb(0xFFC400)), (82, Rgb(0xFF3B00)),
    };
    private static readonly (double At, Color Color)[] LoadStops =
    {
        (0, Rgb(0x1E6BFF)), (40, Rgb(0x7C5CFF)), (75, Rgb(0xFF2D95)), (100, Rgb(0xFF3B00)),
    };
    private static readonly Color[] AuroraPalette = { Rgb(0x00F5D4), Rgb(0x00BBF9), Rgb(0x9B5DE5), Rgb(0xF15BB5) };

    private readonly GpuLedSettings _settings;
    private readonly MangaLight _manga = new();
    private readonly WhiteMotion _motion = new();
    private string? _lastEffect;
    private readonly FlashGuard _guard = new();
    private Color _cardWhite = Color.White;
    private double _r, _g, _b; // cor suavizada (temperatura / uso)
    private long _last = -1;

    public LedEffectEngine(GpuLedSettings settings) => _settings = settings;

    /// <summary>Nível da luz branca (0–1, na escala que o olho percebe) no último quadro dos efeitos brancos.</summary>
    public double LastLevel { get; private set; } = double.NaN;

    /// <summary>
    /// Recomeço (placa aberta de novo): os efeitos de sensor partem de <paramref name="current"/> e o tom "Igual ao da
    /// placa" usa <paramref name="cardWhite"/>, a cor que a placa tinha guardada.
    /// </summary>
    public void Reset(Color current, Color cardWhite)
    {
        (_r, _g, _b) = (current.R, current.G, current.B);
        _cardWhite = cardWhite;
        _last = -1;
        _manga.Reset();
        _motion.Reset();
        _lastEffect = null;
        _guard.Reset();
    }

    /// <summary>Cor agora, já com o brilho. <paramref name="animating"/> = muda sozinha com o tempo (precisa de quadros).</summary>
    public Color Next(long nowMs, float gpuTemp, float gpuLoad, ScreenLight? screen, out bool animating)
    {
        double dt = _last < 0 ? 0 : Math.Clamp((nowMs - _last) / 1000.0, 0, 1);
        _last = nowMs;
        var color = Compute(_settings.Efeito, nowMs, dt, gpuTemp, gpuLoad, screen, out animating);
        if (GameReactions?.Invoke() is { } moment)
        {
            // A partida do CS2 por cima do efeito, no ponto do gabinete onde a placa está.
            var (u, v) = Argb.CaseLayout.GpuCenter;
            var shade = Game.GameLights.At(moment, new Argb.LedPoint(0, 0, u, v, double.NaN));
            var white = White();
            color = Color.FromArgb(
                ToByte(color.R * shade.Dim + shade.R * white.R),
                ToByte(color.G * shade.Dim + shade.G * white.G),
                ToByte(color.B * shade.Dim + shade.B * white.B));
            animating = true;
        }
        // A trava contra pisca-pisca vem por último: vale para qualquer efeito e qualquer soma de acontecimentos.
        return _guard.Apply(Scale(color, Math.Clamp(_settings.Brilho, 0, 100) / 100.0), nowMs);
    }

    /// <summary>A partida do CS2 agora (null = sem reações).</summary>
    public Func<Game.Cs2Moment?>? GameReactions { get; set; }

    /// <summary>A cena dos fans no lugar da placa (efeito "Igual aos fans"); null = os fans não estão numa cena.</summary>
    public Func<Color?>? FansSample { get; set; }

    /// <summary>Relógio de entrada falso para a "Brasa" deste motor (as prévias do painel).</summary>
    public Func<long, uint>? SimulatedInput
    {
        set => _motion.Input = value;
    }

    /// <summary>Quantas vezes a trava segurou a luz desde a última consulta.</summary>
    public int TakeFlashHolds() => _guard.TakeHeld();

    private Color Compute(string effect, long nowMs, double dt, float gpuTemp, float gpuLoad, ScreenLight? screen, out bool animating)
    {
        double seconds = nowMs / 1000.0;
        var chosen = GpuLighting.ParseColor(_settings.Cor);
        double speed = _settings.Velocidade switch
        {
            GpuEffects.Slow => 0.5,
            GpuEffects.Fast => 2,
            _ => 1,
        };
        animating = true;
        LastLevel = double.NaN;
        if (effect != _lastEffect)
        {
            _lastEffect = effect;
            _motion.Reset(); // o Pomodoro começa agora; coração, brasa e turbina partem do repouso
        }

        switch (effect)
        {
            case GpuEffects.Breathing:
            {
                double period = 3.5 / speed;
                double wave = 0.5 - 0.5 * Math.Cos(2 * Math.PI * seconds / period);
                return Scale(chosen, 0.03 + 0.97 * Math.Pow(wave, 2.2)); // curva com gama: o olho vê a respiração por igual
            }
            case GpuEffects.Rainbow:
                return FromHue(seconds / (15 / speed) * 360);
            case GpuEffects.Aurora:
            {
                double position = seconds / (20 / speed) * AuroraPalette.Length;
                int i = (int)Math.Floor(position) % AuroraPalette.Length;
                double t = position - Math.Floor(position);
                t = t * t * (3 - 2 * t);
                return Lerp(AuroraPalette[i], AuroraPalette[(i + 1) % AuroraPalette.Length], t);
            }
            case GpuEffects.Temperature:
            case GpuEffects.Load:
            {
                float value = effect == GpuEffects.Temperature ? gpuTemp : gpuLoad;
                var stops = effect == GpuEffects.Temperature ? TemperatureStops : LoadStops;
                var target = float.IsNaN(value) ? stops[0].Color : Gradient(stops, value);
                // Desliza até a cor nova em vez de pular (a leitura dos sensores é 1x por segundo).
                double k = 1 - Math.Exp(-dt / FadeSeconds);
                _r += (target.R - _r) * k;
                _g += (target.G - _g) * k;
                _b += (target.B - _b) * k;
                animating = Math.Abs(target.R - _r) + Math.Abs(target.G - _g) + Math.Abs(target.B - _b) > 1.5;
                return Color.FromArgb(ToByte(_r), ToByte(_g), ToByte(_b));
            }
            case GpuEffects.Page:
            case GpuEffects.Manga:
                // No Modo Mangá, as ações do PC entram por cima da luz do painel (Manga.MangaStage.ShapeLed).
                return WhiteAt(Manga.MangaStage.ShapeLed(_manga.Next(seconds, dt, nowMs, screen, narrated: effect == GpuEffects.Manga), nowMs));
            case GpuEffects.GrandLine:
                return GrandLine(nowMs, seconds, dt, screen);
            case GpuEffects.Candle:
                return WhiteAt(Candle(seconds), 0.9);
            case GpuEffects.LivingTide:
                return WhiteAt(0.6 + 0.05 * Math.Sin(2 * Math.PI * seconds / 7) + 0.04 * Math.Sin(2 * Math.PI * seconds / 11.3)
                    + 0.03 * Math.Sin(2 * Math.PI * seconds / 17.9), -0.1 + 0.1 * Math.Sin(2 * Math.PI * seconds / 40));
            case GpuEffects.DistantStorm:
                return WhiteAt(DistantStorm(seconds), -0.4);
            case GpuEffects.SunCycle:
            {
                var stage = SceneDirector.Current;
                return WhiteAt(Choreo.Lerp(0.25, 0.85, stage.Daylight), stage.Warmth);
            }
            case GpuEffects.Lighthouse:
                return WhiteAt(Lighthouse(DateTime.Now, seconds), 0);
            case GpuEffects.SoundPulse:
                return WhiteAt(SoundPulse(nowMs, seconds, SceneDirector.Current), 0);
            case GpuEffects.Drums:
            {
                double cycle = _settings.Velocidade switch
                {
                    GpuEffects.Slow => 1.7,
                    GpuEffects.Fast => 1.0,
                    _ => 1.3,
                };
                return WhiteAt(MangaLight.Drums(seconds, cycle));
            }
            case GpuEffects.Fireflies:
                return WhiteAt(WhiteMotion.Fireflies(seconds, speed));
            case GpuEffects.StarrySky:
                return WhiteAt(WhiteMotion.StarrySky(seconds, speed));
            case GpuEffects.Clouds:
                return WhiteAt(WhiteMotion.Clouds(seconds));
            case GpuEffects.Drizzle:
                return WhiteAt(WhiteMotion.Drizzle(seconds, speed));
            case GpuEffects.Fireworks:
                return WhiteAt(WhiteMotion.Fireworks(seconds, speed));
            case GpuEffects.Fluorescent:
                return WhiteAt(WhiteMotion.Fluorescent(seconds));
            case GpuEffects.Heartbeat:
                return WhiteAt(_motion.Heartbeat(seconds, dt, gpuLoad));
            case GpuEffects.SleepBreath:
                return WhiteAt(WhiteMotion.SleepBreath(seconds));
            case GpuEffects.GuidedBreath:
                return WhiteAt(WhiteMotion.GuidedBreath(seconds));
            case GpuEffects.Pomodoro:
                return WhiteAt(_motion.Pomodoro(nowMs));
            case GpuEffects.Ember:
                return WhiteAt(_motion.Ember(nowMs, seconds, dt));
            case GpuEffects.Turbine:
                return WhiteAt(_motion.Turbine(seconds, dt, gpuLoad, gpuTemp));
            case GpuEffects.FansScene:
            {
                // A luz que a cena dos fans tem no meio da placa; o branco neutro dos fans vira o branco da placa.
                if (FansSample?.Invoke() is not Color fans) return WhiteAt(0.6);
                var white = White();
                return Color.FromArgb(fans.R * white.R / 255, fans.G * white.G / 255, fans.B * white.B / 255);
            }
            default: // cor fixa
                animating = false;
                return chosen;
        }
    }

    /// <summary>
    /// O branco escolhido no nível <paramref name="level"/> (escala do olho: passa pela curva de gama do LED), puxado
    /// para o quente (<paramref name="warmth"/> &gt; 0) ou para o frio (&lt; 0) — no máximo um tom de lâmpada, nunca cor.
    /// </summary>
    private Color WhiteAt(double level, double warmth = 0)
    {
        LastLevel = Math.Clamp(level, 0, 1);
        var white = White();
        if (warmth > 0) white = Lerp(white, WarmWhite, Math.Min(1, warmth));
        else if (warmth < 0) white = Lerp(white, CoolWhite, Math.Min(1, -warmth));
        return Scale(white, Math.Pow(LastLevel, 2.2));
    }

    private Color WhiteAt((double Level, double Warmth) light) => WhiteAt(light.Level, light.Warmth);

    private static readonly Color WarmWhite = Rgb(0xFFB06E);
    private static readonly Color CoolWhite = Rgb(0xBED7FF);

    /// <summary>
    /// Grand Line: a luz da cena inteira. Lê o mangá da Pro Flow (os mesmos momentos do "Mangá narrado"), acompanha o
    /// sol de verdade (mais baixa e morna à noite, dourada no pôr do sol), respira com o som do PC e encena os
    /// acontecimentos do diretor de cena no mesmo instante em que a Thermalright os desenha. Com o PC parado ou a tela
    /// bloqueada, dorme respirando devagar.
    /// </summary>
    private Color GrandLine(long nowMs, double seconds, double dt, ScreenLight? screen)
    {
        // Coluna do Mar: a luz das profundezas, a mesma conta que a borda de baixo da Pro Flow mostra.
        if (World.WorldDirector.Current.Actors.Count > 0 || SceneDirector.InUse)
        {
            var (depth, tone) = World.WorldLight.At(nowMs + 8, SceneDirector.Current, World.WorldDirector.Current);
            return WhiteAt(depth, tone);
        }
        var stage = SceneDirector.Current;
        double level = _manga.Next(seconds, dt, nowMs, screen, narrated: true) * Choreo.Lerp(0.6, 1, stage.Daylight);
        double warmth = stage.Warmth * 0.8;

        // O som: a luz sobe com o volume e ganha um soco a cada batida (o diretor garante no máximo ~3 por segundo).
        level += 0.15 * stage.Audio;
        if (stage.LastBeat > 0) level += 0.18 * Choreo.Pulse((nowMs - stage.LastBeat) / 1000.0, 1, 0.04, 0.18);

        foreach (var e in stage.Events)
        {
            double x = (nowMs - e.At) / 1000.0;
            if (x < 0) continue;
            switch (e.Kind)
            {
                case StageEventKind.Lightning:
                {
                    double flash = Choreo.Lightning(x, e.Variant);
                    level = Math.Max(level, flash);
                    warmth -= 0.6 * flash; // o raio é frio
                    break;
                }
                case StageEventKind.Welcome when x < Choreo.WelcomeSweep:
                    level = Choreo.Lerp(level, 1, Math.Sin(Math.PI * x / Choreo.WelcomeSweep));
                    break;
                case StageEventKind.DenDenMushi when x < Choreo.DenDenRing + 4:
                    level += 0.2 * Choreo.DenDenRings(x);
                    if (x >= Choreo.DenDenRing) level = Math.Max(level, 0.85 * Math.Exp(-(x - Choreo.DenDenRing) / 1.2)); // "gacha"
                    break;
                case StageEventKind.CoupDeBurst when x < Choreo.BurstCharge + 5:
                    if (x < Choreo.BurstCharge)
                    {
                        double charge = 0.12 + 0.3 * Math.Pow(x / Choreo.BurstCharge, 2); // a cola enchendo
                        level = Choreo.Lerp(level, charge, Choreo.Smooth(0, 0.3, x));
                    }
                    else
                    {
                        double after = x - Choreo.BurstCharge;
                        level = after < 0.2 ? 1 : Math.Max(level, Choreo.Lerp(1, level, Choreo.Smooth(0.2, 4, after)));
                    }
                    break;
                case StageEventKind.BusterCall when x < Choreo.BusterCallSeconds:
                {
                    double env = Choreo.Smooth(0, 0.5, x) * Choreo.Smooth(0, 0.5, Choreo.BusterCallSeconds - x);
                    double alarm = 0.3 + 0.3 * (0.5 - 0.5 * Math.Cos(Math.PI * x)); // pulso grave a 0,5 Hz
                    foreach (double shot in Choreo.BusterShots()) alarm += 0.5 * Choreo.Pulse(x - shot, 1, 0.04, 0.6); // canhões ao longe
                    level = Choreo.Lerp(level, Math.Min(1, alarm), env);
                    warmth = Choreo.Lerp(warmth, 0.9, env);
                    break;
                }
            }
        }

        if (stage.Sleeping)
        {
            level = Sleep(seconds, stage.Locked);
            warmth = 0.7;
        }
        return WhiteAt(Math.Clamp(level, 0, 1), Math.Clamp(warmth, -1, 1));
    }

    /// <summary>
    /// Respiração de PC dormindo (a do LED do MacBook): inspira em 35% do ciclo, expira em 50% e descansa embaixo; mais
    /// lenta e mais baixa com a tela bloqueada. O piso de 0,15 evita os degraus do LED no escuro.
    /// </summary>
    private static double Sleep(double t, bool locked)
    {
        double period = locked ? 10 : 5.2;
        double p = t % period / period;
        double w = p < 0.35 ? Choreo.Smooth(0, 0.35, p) : p < 0.85 ? 1 - Choreo.Smooth(0.35, 0.85, p) : 0;
        return 0.15 + (locked ? 0.2 : 0.35) * w;
    }

    /// <summary>Vela: chama viva (ruído lento, ±0,03) e, a cada 10–25 s, um sopro que faz a chama tombar e tremer.</summary>
    private static double Candle(double t)
    {
        double level = 0.6 + 0.03 * (Choreo.SlowNoise(t, 2) * 2 - 1);
        long window = (long)Math.Floor(t / 17.5);
        double x = t - (window * 17.5 + 7.5 * Choreo.Hash(window));
        if (x >= 0 && x < 2)
        {
            double gust = Math.Sin(Math.PI * x / 2);
            level += -0.12 * gust + 0.03 * gust * Math.Sin(2 * Math.PI * 5 * t); // tremor raso: não conta como clarão
        }
        return level;
    }

    /// <summary>
    /// Relâmpago distante: penumbra fria e, a cada janela de 24 s, ou um raio (com re-descargas) ou só um clarão difuso
    /// dentro da nuvem.
    /// </summary>
    private static double DistantStorm(double t)
    {
        const double Window = 24;
        long k = (long)Math.Floor(t / Window);
        double x = t - (k * Window + 16 * Choreo.Hash(k));
        double level = 0.2;
        if (Choreo.Hash(k + 999) < 0.4)
            level += x < 0 ? 0 : x < 0.3 ? 0.2 * x / 0.3 : x < 1.2 ? 0.2 * (1 - (x - 0.3) / 0.9) : 0; // clarão na nuvem
        else
            level = Math.Max(level, Choreo.Lightning(x, (int)(k % 3)));
        return level;
    }

    /// <summary>
    /// Farol das horas: luz firme e, na virada da hora, um lampejo por hora (1 a 12, um a cada 1,4 s, como a lente de
    /// um farol); nos quartos de hora, um lampejo longo.
    /// </summary>
    private static double Lighthouse(DateTime now, double t)
    {
        const double Base = 0.45;
        double second = now.Second + now.Millisecond / 1000.0;
        if (now.Minute == 0 && second < 20)
        {
            int hours = now.Hour % 12 == 0 ? 12 : now.Hour % 12;
            double level = Base;
            for (int i = 0; i < hours; i++) level += 0.55 * Math.Exp(-Math.Pow((second - (0.6 + 1.4 * i)) / 0.2, 2));
            return Math.Min(1, level);
        }
        if (now.Minute % 15 == 0 && second < 4) return Base + 0.5 * Math.Exp(-Math.Pow((second - 1.6) / 0.6, 2));
        return Base + 0.03 * Math.Sin(2 * Math.PI * t / 6);
    }

    /// <summary>Pulso do som: a luz sobe com o volume e bate junto com a música; em silêncio, respira calma.</summary>
    private static double SoundPulse(long nowMs, double t, StageState stage)
    {
        double level = 0.18 + 0.32 * stage.Audio;
        if (stage.LastBeat > 0) level += 0.5 * Choreo.Pulse((nowMs - stage.LastBeat) / 1000.0, 1, 0.04, 0.18);
        if (stage.Audio < 0.02) level = Math.Max(level, 0.3 + 0.05 * Math.Sin(2 * Math.PI * t / 6));
        return Math.Clamp(level, 0, 1);
    }

    /// <summary>
    /// Tom do branco. "Igual ao da placa" é a cor que estava guardada nela (o branco já acertado no Polychrome para
    /// combinar com o resto do PC) — se aquela cor não for um branco, fica o neutro.
    /// </summary>
    private Color White() => _settings.Branco switch
    {
        GpuEffects.WhiteNeutral => Rgb(0xFFFFFF),
        GpuEffects.WhiteWarm => Rgb(0xFFC48C),
        GpuEffects.WhiteCool => Rgb(0xC8E1FF),
        _ => Math.Min(_cardWhite.R, Math.Min(_cardWhite.G, _cardWhite.B)) >= 150 ? _cardWhite : Rgb(0xFFFFFF),
    };

    private static Color Gradient((double At, Color Color)[] stops, double value)
    {
        if (value <= stops[0].At) return stops[0].Color;
        for (int i = 1; i < stops.Length; i++)
        {
            if (value > stops[i].At) continue;
            double t = (value - stops[i - 1].At) / (stops[i].At - stops[i - 1].At);
            return Lerp(stops[i - 1].Color, stops[i].Color, t);
        }
        return stops[^1].Color;
    }

    private static Color FromHue(double hue)
    {
        hue = ((hue % 360) + 360) % 360 / 60;
        int sector = (int)hue;
        double f = hue - sector;
        byte up = ToByte(255 * f), down = ToByte(255 * (1 - f));
        return sector switch
        {
            0 => Color.FromArgb(255, up, 0),
            1 => Color.FromArgb(down, 255, 0),
            2 => Color.FromArgb(0, 255, up),
            3 => Color.FromArgb(0, down, 255),
            4 => Color.FromArgb(up, 0, 255),
            _ => Color.FromArgb(255, 0, down),
        };
    }

    private static Color Lerp(Color a, Color b, double t) =>
        Color.FromArgb(ToByte(a.R + (b.R - a.R) * t), ToByte(a.G + (b.G - a.G) * t), ToByte(a.B + (b.B - a.B) * t));

    private static Color Scale(Color c, double k) => Color.FromArgb(ToByte(c.R * k), ToByte(c.G * k), ToByte(c.B * k));

    private static byte ToByte(double v) => (byte)Math.Clamp(Math.Round(v), 0, 255);

    public static Color Rgb(int rgb) => Color.FromArgb((rgb >> 16) & 0xFF, (rgb >> 8) & 0xFF, rgb & 0xFF);
}

/// <summary>
/// Luz branca que lê o mangá da Pro Flow (níveis de 0 a 1 na escala do olho).
/// <list type="bullet">
/// <item>Base: acompanha a tinta da imagem na tela — painel claro, luz cheia; painel carregado de preto, luz baixa —
/// deslizando, nunca pulando. Sem tela (desconectada, outro tema parado), fica num branco firme.</item>
/// <item>No "Mangá narrado", a virada de painel ganha um DON! (corte diagonal) ou um pontilhado (retícula), e os painéis
/// marcados no quadros.json ganham um momento próprio (<see cref="PanelCue"/>), que entra e sai suave com o painel.
/// Os tempos vêm de <see cref="Choreo"/>: a tela Thermalright encena os mesmos instantes.</item>
/// </list>
/// </summary>
internal sealed class MangaLight
{
    private const long StaleMs = 1500;
    private const double BaseSmoothing = 0.45; // s
    private const double NoScreenLevel = 0.85;

    private readonly List<Pulse> _pulses = new();
    private double _base = -1;
    private int _panel = -1;
    private bool _turning;

    /// <summary>Clarão: sobe em <see cref="Attack"/> s até <see cref="Peak"/> e apaga com constante <see cref="Decay"/> s.</summary>
    private readonly record struct Pulse(double At, double Peak, double Attack, double Decay);

    public void Reset()
    {
        _pulses.Clear();
        _base = -1;
        _panel = -1;
        _turning = false;
    }

    public double Next(double now, double dt, long nowMs, ScreenLight? screen, bool narrated)
    {
        bool live = screen is not null && nowMs - screen.At <= StaleMs;
        double ink = live ? Choreo.Smooth(0.30, 0.85, screen!.Luminance) : 0;
        double target = !live ? NoScreenLevel : narrated ? 0.38 + 0.47 * ink : 0.42 + 0.58 * ink; // o narrado guarda folga para os clarões
        _base = _base < 0 ? target : _base + (target - _base) * (1 - Math.Exp(-dt / BaseSmoothing));
        if (!narrated || !live || screen!.Panel is not PanelScene scene) return Math.Clamp(_base + PulsesAt(now), 0, 1);

        // O começo de cada fase (virada ou movimento do painel) é um evento — se o efeito acabou de ligar no meio de uma
        // fase, não há evento (seria um clarão fora de hora).
        if (scene.Panel != _panel || scene.Turning != _turning)
        {
            bool joinedMidway = _panel < 0 || scene.Seconds > 0.5;
            _panel = scene.Panel;
            _turning = scene.Turning;
            if (!joinedMidway) Begin(scene, now - scene.Seconds);
        }

        double level = scene.Turning ? _base : Cue(scene, _base);
        return Math.Clamp(level + PulsesAt(now), 0, 1);
    }

    /// <summary>
    /// Tambores da Libertação (Gear 5), ドンドットット — "DON do-tto-to": uma batida forte e três leves logo depois, em
    /// ciclos de <paramref name="cycle"/> s, sobre um fundo baixo. As leves são rasas de propósito (+0,08 no nível, menos de
    /// 10% da luz): só a forte conta como clarão, então o ciclo fica abaixo de 3 clarões por segundo.
    /// </summary>
    public static double Drums(double seconds, double cycle)
    {
        double t = seconds % cycle;
        double light = 0;
        foreach (double at in Choreo.DrumLight) light += Choreo.Hit(t, cycle * at, 1);
        return 0.32 + 0.68 * Math.Min(1, Choreo.Hit(t, 0, 1)) + 0.08 * Math.Min(1, light);
    }

    private void Begin(PanelScene scene, double start)
    {
        if (scene.Turning)
        {
            if (scene.Turn == PanelTurn.Cut)
            {
                Add(start, -0.25, 0.06, 0.10);                // puxa o ar...
                Add(start + Choreo.TurnDon, 0.55, 0.06, 0.32); // ...DON! quando a divisa branca cruza a tela
            }
            else
            {
                for (int i = 0; i < 3; i++) Add(start + 0.34 * i, 0.15, 0.04, 0.08); // pontilhado da retícula (< 3 por s)
            }
            return;
        }

        switch (scene.Cue)
        {
            case PanelCue.Impact:
                foreach (var (at, peak) in Choreo.Impact) Add(start + at, peak, 0.05, at < 0.5 ? 0.45 : 0.35);
                break;
            case PanelCue.Quake:
                Add(start + Choreo.QuakeStart, 0.45, 0.03, 0.20); // o estalo que abre o tremor
                break;
        }
    }

    /// <summary>O momento do painel sobre a base. Entra nos primeiros 0,8 s do painel e sai nos últimos 0,8 s.</summary>
    private static double Cue(PanelScene scene, double baseLevel)
    {
        double s = scene.Seconds, d = scene.Duration;
        double env = Choreo.Smooth(0, 0.8, s) * Choreo.Smooth(0, 0.8, d - s);
        switch (scene.Cue)
        {
            case PanelCue.Drums:
                return Choreo.Lerp(baseLevel, Drums(s, Choreo.DrumCycle), env);

            case PanelCue.Tide:
                return baseLevel * Choreo.Lerp(1, 0.72 + 0.28 * (0.5 - 0.5 * Math.Cos(2 * Math.PI * s / 4.2)), env);

            case PanelCue.Quake:
                // Gura Gura: a luz chacoalha, mas devagar o bastante para não piscar (≤ 3 mudanças por segundo).
                return baseLevel * (1 - 0.32 * Choreo.Quake(s) * env * Choreo.SlowNoise(s, 3));

            case PanelCue.Haki:
            {
                // A luz cai e estala: uma rajada no choque do Haki do Rei, depois um estalo raro e mais fraco. Os instantes
                // são fixos (Choreo.HakiCrackles): o raio negro na Thermalright cai junto.
                double crackle = 0;
                foreach (var (at, peak, _) in Choreo.HakiCrackles(s))
                {
                    if (s - at > 0.5) continue;
                    double envAt = Choreo.Smooth(0, 0.8, at) * Choreo.Smooth(0, 0.8, d - at);
                    if (envAt > 0.3) crackle += Choreo.Pulse(s - at, peak, 0.02, 0.06);
                }
                return Choreo.Lerp(baseLevel, 0.28, env) + crackle;
            }

            case PanelCue.Farewell:
                return Choreo.Lerp(baseLevel, 0.26 + 0.04 * Math.Sin(2 * Math.PI * s / 5), Choreo.Smooth(0, 2.5, s) * Choreo.Smooth(0, 1.0, d - s));

            case PanelCue.Dawn:
                return Choreo.Lerp(baseLevel, Math.Min(1, baseLevel * (0.45 + 0.73 * Choreo.Smooth(0.6, d * 0.85, s))), env);

            default:
                return baseLevel;
        }
    }

    private void Add(double at, double peak, double attack, double decay) => _pulses.Add(new Pulse(at, peak, attack, decay));

    private double PulsesAt(double now)
    {
        double sum = 0;
        for (int i = _pulses.Count - 1; i >= 0; i--)
        {
            var p = _pulses[i];
            double x = now - p.At;
            if (x > p.Attack + 6 * p.Decay)
            {
                _pulses.RemoveAt(i);
                continue;
            }
            sum += Choreo.Pulse(x, p.Peak, p.Attack, p.Decay);
        }
        return sum;
    }
}
