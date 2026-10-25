using System.Drawing;
using DS339Monitor.Argb;

namespace DS339Monitor.Game;

/// <summary>
/// O que acontece na partida, tirado das mensagens da integração oficial do CS2 (só o que o jogo manda para o próprio
/// jogador: a bomba, a rodada, os seus abates, o seu flash/smoke/fogo, a sua vida). Nada de cronômetro da bomba: a Valve
/// atrasa de propósito o aviso de "plantada" para que ninguém faça um, e o gabinete só pulsa num ritmo fixo enquanto ela
/// estiver plantada.
/// </summary>
internal static class Cs2Events
{
    public enum Kind { Planted, Exploded, Defused, RoundWon, RoundLost, Kill, Headshot, Died, Ace }

    /// <summary>Um acontecimento; <see cref="Value"/> = quantos abates na rodada (nos abates).</summary>
    public readonly record struct Event(Kind Kind, long At, int Value = 0);

    private static readonly object Gate = new();
    private static readonly List<Event> Recent = new();
    private static Cs2State? _last;
    private static long _plantedAt, _diedAt, _lastMessage;
    private static int _flashed, _smoked, _burning, _health = -1, _kills;

    /// <summary>Um acontecimento novo (para as telas: o Modo Mangá encena alguns).</summary>
    public static event Action<Kind>? Happened;

    public static void Update(Cs2State s)
    {
        var raised = new List<(Kind Kind, int Value)>();
        lock (Gate)
        {
            long now = Environment.TickCount64;
            _lastMessage = now;
            var last = _last;
            _last = s;

            // A bomba (da rodada; vale para quem joga e para quem assiste).
            if (s.Bomb != last?.Bomb)
            {
                switch (s.Bomb)
                {
                    case "planted":
                        _plantedAt = now;
                        raised.Add((Kind.Planted, 0));
                        break;
                    case "exploded":
                        _plantedAt = 0;
                        raised.Add((Kind.Exploded, 0));
                        break;
                    case "defused":
                        _plantedAt = 0;
                        raised.Add((Kind.Defused, 0));
                        break;
                    default:
                        _plantedAt = 0;
                        break;
                }
            }

            // Fim da rodada: venceu ou perdeu o seu time.
            if (s.RoundPhase == "over" && last?.RoundPhase != "over" && s.WinTeam is { } win && s.Team.Length > 0)
                raised.Add((win == s.Team ? Kind.RoundWon : Kind.RoundLost, 0));
            if (s.RoundPhase != "live" && s.RoundPhase != "over") _plantedAt = 0; // nova rodada

            // Você: abates, capacete, ace, morte, flash, smoke e fogo — só das mensagens que são suas.
            if (s.Self && last is { Self: true })
            {
                if (s.RoundKills > last.RoundKills) raised.Add((s.RoundHeadshots > last.RoundHeadshots ? Kind.Headshot : Kind.Kill, s.RoundKills));
                if (s.RoundKills >= 5 && last.RoundKills < 5) raised.Add((Kind.Ace, s.RoundKills));
                if (s.Health == 0 && last.Health > 0 && s.RoundPhase == "live")
                {
                    _diedAt = now;
                    raised.Add((Kind.Died, 0));
                }
            }
            if (!s.Self && last is { Self: true } && s.RoundPhase == "live" && _diedAt == 0)
            {
                _diedAt = now; // morreu e o jogo já passou a mostrar outro jogador
                raised.Add((Kind.Died, 0));
            }
            if (s.RoundPhase is "freezetime" || (s.Self && s.Health > 0)) _diedAt = 0;
            _flashed = s.Self ? s.Flashed : 0;
            _smoked = s.Self ? s.Smoked : 0;
            _burning = s.Self ? s.Burning : 0;
            _health = s.Self ? s.Health : -1;
            _kills = s.Self ? s.RoundKills : 0;

            foreach (var (kind, value) in raised) Recent.Add(new Event(kind, now, value));
            Recent.RemoveAll(e => now - e.At > 8000);
        }
        foreach (var (kind, value) in raised)
        {
            Log.Info($"CS2: {kind}{(value > 0 ? $" ({value})" : "")}.");
            Happened?.Invoke(kind);
        }
    }

    private static long _simulatingUntil;

    /// <summary>"Testar as reações" em andamento (vale mesmo com o modo jogo desligado).</summary>
    public static bool Simulating => Environment.TickCount64 < Volatile.Read(ref _simulatingUntil);

    /// <summary>
    /// Duas rodadas de mentira (~26 s), para ver as reações sem jogar: três abates (um de capacete), a bomba plantada com
    /// pouca vida, a explosão com vitória; na seguinte, um flash, fogo, a morte e a derrota.
    /// </summary>
    public static void Simulate()
    {
        Volatile.Write(ref _simulatingUntil, Environment.TickCount64 + 26_000);
        Task.Run(async () =>
        {
            var steps = new (int Ms, Cs2State State)[]
            {
                (0, Fake()),
                (1000, Fake(1)),
                (2200, Fake(2, 1)),
                (3200, Fake(3, 1)),
                (4400, Fake(3, 1, "planted")),
                (8400, Fake(3, 1, "planted", health: 18)),
                (10400, Fake(3, 1, "exploded", "over", "T", health: 18)),
                (14600, Fake(phase: "freezetime")),
                (15400, Fake(phase: "live", flashed: 255)),
                (16200, Fake(phase: "live", flashed: 160)),
                (16700, Fake(phase: "live", flashed: 80)),
                (17200, Fake(phase: "live", flashed: 20)),
                (17600, Fake(phase: "live", burning: 200, health: 60)),
                (19400, Fake(phase: "live", health: 0)),
                (21000, Fake(phase: "over", win: "CT", health: 0)),
                (25000, Fake(phase: "freezetime")),
            };
            long start = Environment.TickCount64;
            foreach (var (ms, state) in steps)
            {
                int wait = (int)(start + ms - Environment.TickCount64);
                if (wait > 0) await Task.Delay(wait);
                var stamped = state with { At = Environment.TickCount64 };
                GameTelemetry.SetCs2(stamped);
                Update(stamped);
            }
        });
    }

    /// <summary>Uma mensagem de mentira do CS2 (você joga de TR, na Mirage).</summary>
    private static Cs2State Fake(int kills = 0, int hs = 0, string? bomb = null, string phase = "live", string? win = null, int flashed = 0,
        int health = 100, int burning = 0) =>
        new("de_mirage", "competitive", "live", 13, 7, 6, "T", health, 100, true, 3400, kills, 14 + kills, 3, 8, 2, "AK-47", 24, 90, bomb,
            Environment.TickCount64)
        { RoundHeadshots = hs, RoundPhase = phase, WinTeam = win, Flashed = flashed, Burning = burning };

    /// <summary>Para as prévias: leva a partida de mentira até o fim de uma rodada ganha ou perdida, ou até a bomba plantada.</summary>
    public static void Stage(string moment)
    {
        var live = Fake(2, 1);
        GameTelemetry.SetCs2(live);
        Update(live);
        var next = moment switch
        {
            "vitoria" => Fake(2, 1, phase: "over", win: "T"),
            "derrota" => Fake(2, 1, phase: "over", win: "CT"),
            "plantada" => Fake(2, 1, "planted"),
            _ => live,
        };
        GameTelemetry.SetCs2(next);
        Update(next);
    }

    /// <summary>O estado de agora, para desenhar (null = o CS2 não mandou nada nos últimos 15 s).</summary>
    public static Cs2Moment? Moment()
    {
        lock (Gate)
        {
            long now = Environment.TickCount64;
            if (_lastMessage == 0 || now - _lastMessage > 15_000) return null;
            return new Cs2Moment(now, Recent.ToArray(), _plantedAt, _diedAt, _flashed / 255.0, _smoked / 255.0, _burning / 255.0, _health, _kills);
        }
    }
}

/// <summary>A faixa da Thermalright: o texto e a cor do fundo (vitória em verde, derrota em vermelho, o resto preto).</summary>
internal readonly record struct Cs2Banner(string Text, Color Back);

/// <summary>A partida num instante: acontecimentos recentes e estados que duram (bomba plantada, morto, flash…).</summary>
internal sealed record Cs2Moment(long Now, Cs2Events.Event[] Events, long PlantedAt, long DiedAt, double Flash, double Smoke, double Fire,
    int Health = -1, int Kills = 0)
{
    public static readonly Color Ink = Color.Black;
    public static readonly Color Win = Color.FromArgb(22, 150, 64);
    public static readonly Color Loss = Color.FromArgb(200, 26, 36);

    /// <summary>A faixa da Thermalright (null = nenhuma).</summary>
    public Cs2Banner? Banner
    {
        get
        {
            foreach (var e in Events.Reverse())
            {
                long age = Now - e.At - (long)(Delay(e) * 1000);
                if (age < 0) continue;
                Cs2Banner? banner = e.Kind switch
                {
                    Cs2Events.Kind.Exploded when age < 4000 => new Cs2Banner("EXPLODIU!", Ink),
                    Cs2Events.Kind.Defused when age < 4000 => new Cs2Banner("DESARMADA!", Ink),
                    Cs2Events.Kind.RoundWon when age < 4000 => new Cs2Banner("VITÓRIA!", Win),
                    Cs2Events.Kind.RoundLost when age < 4000 => new Cs2Banner("DERROTA", Loss),
                    Cs2Events.Kind.Ace when age < 5000 => new Cs2Banner("ACE!!", Ink),
                    _ => null,
                };
                if (banner is not null) return banner;
            }
            return PlantedAt != 0 ? new Cs2Banner("BOMBA PLANTADA", Ink) : null;
        }
    }

    /// <summary>
    /// Quanto um acontecimento espera para entrar (s): o fim da rodada que chega junto da explosão ou do desarme deixa a
    /// cena da bomba acontecer primeiro; o ace entra quando o traçante do 5º abate chega.
    /// </summary>
    public double Delay(in Cs2Events.Event e)
    {
        if (e.Kind == Cs2Events.Kind.Ace) return 0.45;
        if (e.Kind is not (Cs2Events.Kind.RoundWon or Cs2Events.Kind.RoundLost)) return 0;
        foreach (var other in Events)
        {
            if (Math.Abs(other.At - e.At) > 1500) continue;
            if (other.Kind == Cs2Events.Kind.Exploded) return 2.2;
            if (other.Kind == Cs2Events.Kind.Defused) return 1.6;
        }
        return 0;
    }
}

/// <summary>
/// A luz do gabinete reagindo à partida, pela posição de cada LED (ver <see cref="CaseLayout"/>: u = 0 na parede de
/// trás … 1 no radiador; v = 0 no topo … 1 no piso). Cada acontecimento é uma pequena cena:
/// <list type="bullet">
/// <item>bomba plantada — o gabinete baixa, um "sonar" vermelho sai do piso uma vez por segundo (sempre no mesmo ritmo:
/// não é cronômetro), a capa de cabo da placa-mãe vira o teclado da C4 (o código 7355608 sendo digitado) e a da placa de
/// vídeo, o visor;</item>
/// <item>explosão — núcleo branco-quente no piso, bola de fogo que enche o gabinete e esfria de amarelo a vermelho,
/// turbinas (anéis dos fans) pegando fogo, fumaça e brasas caindo;</item>
/// <item>desarme — arco elétrico azul do piso até a bomba do water cooler (o fio cortado) e uma geada ciano subindo;</item>
/// <item>vitória — maré VERDE subindo, volta olímpica nos anéis e confete; derrota — cortina VERMELHA que cai e escorre
/// pelo piso, com os fans parando;</item>
/// <item>abate — clarão no fan de trás e um traçante até o radiador, cada abate da rodada numa altura, com respingo; no
/// capacete, o tiro vai no alto e acerta uma estrela branca; o ace é a coroa dourada;</item>
/// <item>as memórias contam os abates da rodada; pouca vida faz as bordas baterem como coração; morto, a linha do
/// monitor cardíaco corre pelo piso e fica reta, e uma névoa azul sobe;</item>
/// <item>flash deixa tudo branco e a visão volta de fora para dentro; smoke rola uma névoa cinza; fogo lambe de baixo.</item>
/// </list>
/// A trava de pisca-pisca continua valendo na saída, por cima de qualquer soma destas cenas.
/// </summary>
internal static class GameLights
{
    public readonly record struct Shade(double R, double G, double B, double Dim);

    private readonly record struct Tone(double R, double G, double B)
    {
        public static Tone operator +(Tone a, Tone b) => new(a.R + b.R, a.G + b.G, a.B + b.B);
        public static Tone operator *(Tone a, double k) => new(a.R * k, a.G * k, a.B * k);
    }

    private static readonly Tone None = new(0, 0, 0);
    private static readonly Tone White = new(1, 1, 1);
    private static readonly Tone Hot = new(1, 0.9, 0.72);
    private static readonly Tone Yellow = new(1, 0.72, 0.18);
    private static readonly Tone Orange = new(1, 0.38, 0.04);
    private static readonly Tone Red = new(1, 0.04, 0.03);
    private static readonly Tone Blood = new(0.5, 0.01, 0.01);
    private static readonly Tone Green = new(0.08, 1, 0.22);
    private static readonly Tone Mint = new(0.5, 1, 0.68);
    private static readonly Tone Lime = new(0.55, 1, 0.08);
    private static readonly Tone Cyan = new(0.15, 0.8, 1);
    private static readonly Tone Ice = new(0.72, 0.9, 1);
    private static readonly Tone Gold = new(1, 0.7, 0.14);
    private static readonly Tone Ivory = new(1, 0.82, 0.6);
    private static readonly Tone Haze = new(0.55, 0.58, 0.64);
    private static readonly Tone Spirit = new(0.42, 0.58, 1);
    private static readonly Tone Afterimage = new(0.62, 0.3, 1);

    /// <summary>Onde a bomba fica: o meio do piso, nos fans de baixo.</summary>
    private static readonly (double U, double V) Floor = (0.45, 1.0);

    /// <summary>O fan de cima do radiador (o "tink" do capacete).</summary>
    private static readonly (double U, double V) RadiatorTop = CaseLayout.ToCase(822, 545);

    /// <summary>Até onde o tiro de cada abate da rodada sobe no radiador (1º embaixo, 2º no meio, 3º no alto…).</summary>
    private static readonly double[] Targets = { 0.76, 0.5, 0.24, 0.62, 0.36 };

    // A trilha do tiro: as fitas do piso, da esquerda até o radiador, e o radiador para cima (unidades do gabinete/s).
    private const double TrackStart = 0.06, TrackFloor = 0.935, TrackWall = 0.86, ShotSpeed = 3.4;

    private const string Code = "7355608";
    private const int LowHealth = 30;

    public static Shade At(Cs2Moment m, in LedPoint p)
    {
        double t = m.Now / 1000.0;
        double dim = 1;
        var add = None;

        if (m.Smoke > 0) add += Smoke(p, m.Smoke, t, ref dim);
        if (m.PlantedAt != 0) add += Planted(p, t, (m.Now - m.PlantedAt) / 1000.0, ref dim);
        if (m.DiedAt != 0) add += Died(p, (m.Now - m.DiedAt) / 1000.0, ref dim);
        else if (m.Health is > 0 and <= LowHealth) add += Heartbeat(p, m.Health, t, ref dim);
        if (m.Kills > 0) add += Tally(m, p);

        foreach (var e in m.Events)
        {
            double age = (m.Now - e.At) / 1000.0 - m.Delay(e);
            int seed = (int)(e.At & 0x7FFFFFFF);
            add += e.Kind switch
            {
                Cs2Events.Kind.Exploded => Explosion(p, age, seed, t, ref dim),
                Cs2Events.Kind.Defused => Defuse(p, age, seed),
                Cs2Events.Kind.RoundWon => Victory(p, age, seed),
                Cs2Events.Kind.RoundLost => Defeat(p, age, seed, ref dim),
                Cs2Events.Kind.Kill => Shot(p, age, e.Value, false, seed),
                Cs2Events.Kind.Headshot => Shot(p, age, e.Value, true, seed),
                Cs2Events.Kind.Ace => Crown(p, age, seed),
                _ => None,
            };
        }

        if (m.Fire > 0) add += Flames(p, m.Fire, t, ref dim);
        if (m.Flash > 0) add += Flashbang(p, m.Flash, ref dim);
        return new Shade(add.R, add.G, add.B, dim);
    }

    // ---------------------------------------------------------------- a bomba

    private static Tone Planted(in LedPoint p, double t, double since, ref double dim)
    {
        dim *= 1 - 0.6 * Smooth(0, 0.5, since);
        // Uma vez por segundo, sempre igual: o anel do sonar e o piso batendo junto.
        double phase = Frac(t);
        double ping = Band(Dist(p, Floor) - phase * 0.95, 0.07) * Math.Pow(1 - phase, 1.5);
        double floor = Math.Pow(0.5 - 0.5 * Math.Cos(2 * Math.PI * t), 4) * Smooth(0.6, 1.0, p.V);
        var light = Red * (0.75 * ping + 0.55 * floor);

        if (InBoardCable(p))
        {
            // O teclado: as teclas 7-3-5-5-6-0-8 acendendo uma depois da outra.
            light += Blood * 0.12;
            for (int k = 0; k < Code.Length && since < 2.2; k++)
            {
                double press = since - 0.2 * k;
                if (press < 0 || press > 0.6) continue;
                var (x, y) = Key(Code[k]);
                light += Orange * (Band(Math.Sqrt(Sq(p.X - x) + Sq(p.Y - y)), 16) * (1 - press / 0.6));
            }
        }
        else if (InGpuCable(p))
        {
            // O visor: um ponto em cada seis aceso, trocando duas vezes por segundo.
            light += Red * (Hash(LedId(p), (int)Math.Floor(t * 2)) > 0.83 ? 0.4 : 0.05);
        }
        return light;
    }

    /// <summary>Onde fica cada tecla do teclado da C4 na capa de cabo da placa-mãe (3 colunas, 4 fileiras; pixels da foto).</summary>
    private static (double X, double Y) Key(char digit)
    {
        int n = digit == '0' ? 10 : digit - '1';
        return (640 + n % 3 * 45, 527 + n / 3 * 28);
    }

    private static Tone Explosion(in LedPoint p, double age, int seed, double t, ref double dim)
    {
        if (age < 0 || age > 4.8) return None;
        double d = Dist(p, Floor);
        // O núcleo: um clarão só, branco-quente, mais forte perto do piso.
        var light = Hot * (Math.Exp(-age / 0.18) * (0.45 + 0.55 * Math.Exp(-d * 2.5)));

        // A bola de fogo: enche o gabinete em ~1 s e esfria de dentro para fora.
        double radius = 1.3 * EaseOut(age / 1.1);
        double heat = Math.Clamp(1 - age / 2.3, 0, 1);
        double inside = Smooth(radius + 0.05, radius - 0.15, d);
        if (inside > 0 && heat > 0)
        {
            double temperature = heat * (0.5 + 0.5 * (1 - d / Math.Max(radius, 0.05)));
            double flicker = 0.82 + 0.18 * Math.Sin(t * 19 + Hash(LedId(p), 7) * 6.3);
            light += FireTone(temperature) * (inside * (0.2 + 0.8 * heat) * flicker);
        }
        light += Yellow * (Band(d - radius, 0.06) * (1 - Smooth(0.5, 1.3, age)));

        // As turbinas pegando fogo.
        if (p.OnRing)
            light += Orange * (0.7 * Comet(p.Angle, seed + age * 11, 0.9) * Smooth(0.2, 0.5, age) * (1 - Smooth(2.4, 3.8, age)));

        // Fumaça: a base baixa depois do fogo e volta devagar.
        dim *= 1 - 0.6 * Smooth(0.8, 1.6, age) * (1 - Smooth(3.4, 4.8, age));

        // Brasas: 40 faíscas que caem balançando e esfriam.
        for (int i = 0; i < 40; i++)
        {
            double life = age - (0.6 + 0.9 * Hash(seed, i));
            if (life < 0 || life > 2.8) continue;
            double u = 0.04 + 0.92 * Hash(seed, i + 100) + 0.04 * Math.Sin(life * 2.3 + i);
            double v = 0.05 + 0.6 * Hash(seed, i + 200) + 0.1 * life + 0.04 * life * life;
            double glow = Band(Dist(p, (u, v)), 0.045);
            if (glow < 0.01) continue;
            double cool = life / 2.8;
            light += Mix(Yellow, Blood, cool) * (glow * (1 - cool) * (0.75 + 0.25 * Math.Sin(t * 13 + i)));
        }
        return light;
    }

    private static Tone Defuse(in LedPoint p, double age, int seed)
    {
        if (age < 0 || age > 3.6) return None;
        // O fio cortado: um estalo branco no meio do piso e faíscas azuis correndo pelas fitas de lá para os lados,
        // trocando a cada 60 ms.
        var light = White * (Band(Dist(p, Floor), 0.12) * Math.Exp(-age / 0.12));
        if (age < 0.8 && p.V > 0.8)
        {
            double reach = 0.6 * EaseOut(age / 0.3);
            double near = Smooth(reach + 0.05, reach - 0.05, Math.Abs(p.U - Floor.U));
            double spark = Hash(LedId(p), seed + (int)(age / 0.06)) > 0.55 ? 1 : 0.15;
            light += (Ice * 0.7 + Cyan * 0.5) * (Smooth(0.8, 0.86, p.V) * near * spark * (1 - Smooth(0.5, 0.8, age)));
        }
        // A geada subindo do piso, cintilando, e indo embora.
        double front = 1.05 - 1.25 * EaseOut((age - 0.3) / 1.5);
        double frost = Smooth(front - 0.05, front + 0.05, p.V) * (1 - Smooth(1.8, 3.6, age));
        double crest = Band(p.V - front, 0.05) * Smooth(0.3, 0.45, age) * (1 - Smooth(1.6, 2.3, age));
        double glitter = Hash(LedId(p), seed + (int)(age * 6)) > 0.9 ? 0.5 : 0;
        light += Cyan * (0.3 * frost) + Ice * (0.85 * crest + glitter * frost);
        if (p.OnRing)
            light += Ice * (0.6 * Comet(p.Angle, seed + 2 * Math.PI * 1.6 * (1 - Math.Exp(-age / 0.7)), 0.8) * (1 - Smooth(1.4, 2.8, age)));
        return light;
    }

    // ---------------------------------------------------------------- a rodada

    private static Tone Victory(in LedPoint p, double age, int seed)
    {
        if (age < 0 || age > 4.6) return None;
        // A maré verde subindo do piso até o topo, com a crista clara.
        double front = 1.05 - 1.15 * EaseOut(age / 1.3);
        double fill = Smooth(front - 0.04, front + 0.06, p.V) * (1 - Smooth(2.4, 4.4, age));
        double crest = Band(p.V - front, 0.05) * (1 - Smooth(1.0, 1.6, age));
        var light = Green * (0.45 * fill) + Mint * crest;
        // A volta olímpica: um cometa verde com a cabeça branca girando em cada anel.
        if (p.OnRing)
        {
            double lap = Comet(p.Angle, seed + age * 2 * Math.PI * 1.3, 0.7) * Smooth(0.2, 0.6, age) * (1 - Smooth(3.0, 4.4, age));
            light += Mix(Lime, White, lap * lap) * lap;
        }
        return light + Confetti(p, age, seed, Green, Mint) * (Smooth(0.8, 1.2, age) * (1 - Smooth(3.4, 4.6, age)));
    }

    private static Tone Defeat(in LedPoint p, double age, int seed, ref double dim)
    {
        if (age < 0 || age > 4.8) return None;
        dim *= 1 - 0.75 * Smooth(0, 0.5, age) * (1 - Smooth(3.6, 4.8, age));
        // A cortina vermelha caindo do topo…
        double curtain = -0.05 + 1.15 * EaseOut(age / 0.6);
        double above = Smooth(curtain + 0.05, curtain - 0.05, p.V);
        // …e escorrendo para o piso, cada coluna num ritmo, com a gota da frente mais clara.
        double column = Hash((int)Math.Round(p.U * 24), seed);
        double line = Smooth(0.8, 3.2, age) * (1.15 + 0.3 * column) - 0.1;
        double remaining = Smooth(line - 0.04, line + 0.04, p.V);
        double drop = Band(p.V - line, 0.035) * Smooth(0.8, 1.0, age) * (1 - Smooth(2.9, 3.3, age));
        var light = Red * (0.7 * above * remaining) + Mix(Red, White, 0.25) * (0.6 * drop);
        // Sobra uma brasa no piso, e os fans param de girar.
        light += Blood * (0.9 * Smooth(0.82, 1.0, p.V) * Smooth(2.2, 3.0, age) * (1 - Smooth(3.4, 4.8, age)));
        if (p.OnRing)
            light += Red * (0.6 * Comet(p.Angle, seed + 2 * Math.PI * 1.2 * (1 - Math.Exp(-age / 0.8)), 0.7) * (1 - Smooth(1.5, 3.0, age)));
        return light;
    }

    // ---------------------------------------------------------------- você

    private static Tone Shot(in LedPoint p, double age, int kill, bool headshot, int seed)
    {
        if (age < 0 || age > 2) return None;
        double target = Target(kill, headshot);
        (double U, double V) to = (TrackWall, target);
        double travel = Travel(kill, headshot);
        // O clarão do disparo no canto do piso.
        var light = Hot * (Math.Exp(-age / 0.08) * Band(Dist(p, (TrackStart, TrackFloor)), 0.1));

        // O traçante: a cabeça clara corre pelas fitas do piso e sobe o radiador; o rastro laranja some logo.
        var (along, across) = Track(p, target);
        double behind = age * ShotSpeed - along;
        if (across < 1.4 && behind > -0.04)
        {
            double tip = age < travel + 0.03 ? Band(behind, 0.05) : 0;
            double trail = Math.Exp(-Math.Max(behind, 0) / 0.25) * (1 - Smooth(travel, travel + 0.35, age));
            light += (Hot * tip + Orange * (0.6 * trail)) * Smooth(1.4, 0.9, across);
        }

        double hit = age - travel;
        if (hit <= 0) return light;
        double d = Dist(p, to);
        if (!headshot)
        {
            // O respingo se abre e escorre pelo radiador.
            double splash = Band(d - hit * 0.6, 0.05) * (1 - Smooth(0, 0.6, hit)) + Band(d, 0.08) * Math.Exp(-hit / 0.3);
            double drip = p.U > 0.72 && p.V > to.V && p.V < to.V + hit * 0.35 ? 0.5 * (1 - Smooth(0.6, 1.3, hit)) : 0;
            return light + Red * (splash + drip);
        }
        // Capacete: uma estrela de seis pontas e o anel do fan de cima dando uma volta.
        double rays = Math.Pow(Math.Abs(Math.Cos(3 * Math.Atan2(p.V - to.V, p.U - to.U) + seed % 7)), 16);
        double star = rays * Band(d - hit * 0.9, 0.12) * (1 - Smooth(0, 0.7, hit)) + Band(d, 0.06) * Math.Exp(-hit / 0.15);
        light += (White * 0.8 + Yellow * 0.2) * star;
        if (p.OnRing && Dist(p, RadiatorTop) < 0.15)
            light += White * (Comet(p.Angle, seed + hit * 2 * Math.PI * 2, 0.6) * (1 - Smooth(0.3, 0.9, hit)));
        return light;
    }

    private static Tone Crown(in LedPoint p, double age, int seed)
    {
        if (age < 0 || age > 5) return None;
        double fade = 1 - Smooth(3.8, 5.0, age);
        double front = 1.05 - 1.15 * EaseOut(age / 0.9);
        var light = Gold * (0.35 * Smooth(front - 0.04, front + 0.06, p.V) * fade)
            + Mix(Gold, White, 0.6) * (Band(p.V - front, 0.05) * (1 - Smooth(0.8, 1.3, age)));
        if (p.OnRing)
        {
            double head = seed + age * 2 * Math.PI * 2;
            light += Mix(Gold, White, 0.3) * ((Comet(p.Angle, head, 0.6) + Comet(p.Angle, head + Math.PI, 0.6)) * 0.8 * Smooth(0.1, 0.4, age) * fade);
        }
        // O brilho de troféu: um reflexo em diagonal atravessando o gabinete a cada 1,3 s.
        if (age > 0.9)
        {
            double sweep = Frac((age - 0.9) / 1.3) * 2.2 - 0.6;
            light += Mix(Gold, White, 0.7) * (0.7 * Band(p.U - 0.5 * p.V - sweep, 0.07) * fade);
        }
        // Chuva de faíscas douradas caindo do topo.
        for (int i = 0; i < 30; i++)
        {
            double life = age - (0.5 + 3.0 * Hash(seed, i + 300));
            if (life < 0 || life > 1.6) continue;
            double glow = Band(Dist(p, (0.03 + 0.94 * Hash(seed, i + 400), -0.05 + 0.75 * life)), 0.04);
            if (glow > 0.01) light += Mix(Gold, White, 0.5) * (glow * (1 - life / 1.6));
        }
        return light;
    }

    /// <summary>As memórias contam os abates da rodada: cada um acende um quinto delas, de baixo para cima (cinco = dourado).</summary>
    private static Tone Tally(Cs2Moment m, in LedPoint p)
    {
        if (p.X < 590 || p.X > 630 || p.Y < 505 || p.Y > 715) return None;
        int segment = (int)(Math.Clamp((705 - p.Y) / 190, 0, 0.999) * 5);
        if (segment >= m.Kills) return None;
        double level = 0.22;
        foreach (var e in m.Events)
        {
            // O quinto novo acende forte quando o tiro chega e assenta.
            if (e.Kind is not (Cs2Events.Kind.Kill or Cs2Events.Kind.Headshot) || e.Value != segment + 1) continue;
            double age = (m.Now - e.At) / 1000.0 - Travel(e.Value, e.Kind == Cs2Events.Kind.Headshot);
            level = age < 0 ? 0 : 0.22 + 0.7 * Math.Exp(-age / 0.4);
        }
        return (m.Kills >= 5 ? Gold : Ivory) * level;
    }

    private static Tone Heartbeat(in LedPoint p, int health, double t, ref double dim)
    {
        // Tum-tum num ritmo fixo de coração, nas bordas do gabinete, mais forte quanto menos vida.
        double k = (LowHealth + 1 - health) / (double)LowHealth;
        double phase = Frac(t * 1.2);
        double beat = Band(phase - 0.1, 0.04) + 0.6 * Band(phase - 0.28, 0.04);
        double edge = Math.Max(Math.Abs(p.U - 0.5), Math.Abs(p.V - 0.5)) * 2;
        dim *= 1 - 0.25 * k;
        return Red * (Smooth(0.45, 1.0, edge) * (0.06 + 0.45 * beat) * k);
    }

    private static Tone Died(in LedPoint p, double since, ref double dim)
    {
        dim *= 0.18 + 0.82 * Math.Exp(-since / 0.5);
        var light = None;
        // A linha do monitor cardíaco correndo pelas fitas do piso; depois de passar, fica reta e fraca até a próxima rodada.
        if (p.V > 0.8)
        {
            double behind = since / 1.3 - p.U;
            if (behind >= 0) light += Red * (0.9 * Math.Exp(-behind / 0.1) * (1 - Smooth(1.3, 2.0, since)) + 0.06);
        }
        // A última batida, quando a linha passa do primeiro terço.
        light += Red * (Band(since - 0.45, 0.07) * (p.V > 0.8 ? 0.6 : 0.2));
        // A névoa azul subindo e saindo pelo topo.
        double rise = 1.15 - (since - 0.6) * 0.5;
        return light + Spirit * (0.22 * Band(p.V - rise, 0.12) * Smooth(0.6, 1.0, since) * (1 - Smooth(2.4, 3.4, since)));
    }

    // ---------------------------------------------------------------- em você

    private static Tone Smoke(in LedPoint p, double amount, double t, ref double dim)
    {
        double roll = 0.5 + 0.25 * Math.Sin(p.U * 7 + t * 0.7) * Math.Sin(p.V * 5 - t * 0.5) + 0.25 * Math.Sin((p.U + p.V) * 4 + t * 0.9);
        double density = amount * (0.55 + 0.45 * roll);
        dim *= 1 - 0.75 * density;
        return Haze * (0.1 * density);
    }

    private static Tone Flames(in LedPoint p, double amount, double t, ref double dim)
    {
        dim *= 1 - 0.3 * amount;
        // Cada coluna tem a sua altura de chama, que sobe e desce.
        double flick = 0.5 + 0.25 * Math.Sin(p.U * 17 + t * 7.3) + 0.15 * Math.Sin(p.U * 31 - t * 11.1) + 0.1 * Math.Sin(t * 4.1 + p.U * 5);
        double reach = amount * (0.2 + 0.35 * flick);
        double y = 1 - p.V;
        if (y > reach + 0.05) return None;
        double x = Math.Max(0, y) / Math.Max(reach, 0.02);
        return FireTone(1 - x * 0.85) * (amount * Smooth(1.15, 0.7, x));
    }

    private static Tone Flashbang(in LedPoint p, double amount, ref double dim)
    {
        // A visão volta de fora para dentro: o meio (a placa, a "tela") é o último a sair do branco…
        double white = Math.Clamp(amount * 1.25 - Dist(p, CaseLayout.GpuCenter) * (1 - amount) * 1.6, 0, 1);
        dim *= 1 - 0.7 * white;
        // …com a sombra roxa da imagem queimada na borda do branco.
        double afterimage = white > 0 ? Band(white - 0.15, 0.12) * (1 - amount) * Smooth(0.02, 0.15, amount) : 0;
        return new Tone(1, 0.97, 0.9) * white + Afterimage * (0.3 * afterimage);
    }

    // ---------------------------------------------------------------- utilidades

    /// <summary>Cor do fogo pela temperatura (0 = brasa, 1 = branco).</summary>
    private static Tone FireTone(double x)
    {
        x = Math.Clamp(x, 0, 1);
        return x < 0.35 ? Mix(Blood, Red, x / 0.35)
            : x < 0.6 ? Mix(Red, Orange, (x - 0.35) / 0.25)
            : x < 0.85 ? Mix(Orange, Yellow, (x - 0.6) / 0.25)
            : Mix(Yellow, Hot, (x - 0.85) / 0.15);
    }

    /// <summary>Confete: cada LED acende um instante numa cor (a, b ou branco), cada um na sua hora.</summary>
    private static Tone Confetti(in LedPoint p, double age, int seed, Tone a, Tone b)
    {
        int id = LedId(p);
        double pick = Hash(id, seed);
        if (pick < 0.35) return None;
        double pulse = Math.Pow(Math.Max(0, Math.Sin(2 * Math.PI * (age / 0.9 + Hash(id, seed + 1)))), 12);
        return (pick < 0.65 ? a : pick < 0.88 ? b : White) * (0.9 * pulse);
    }

    /// <summary>Cometa num anel: claro na cabeça (ângulo <paramref name="head"/>), com rastro para trás.</summary>
    private static double Comet(double angle, double head, double tail)
    {
        double behind = (head - angle) % (2 * Math.PI);
        if (behind < 0) behind += 2 * Math.PI;
        return Math.Exp(-behind / tail);
    }

    private static double Target(int kill, bool headshot) => headshot ? 0.13 : Targets[Math.Clamp(kill, 1, 5) - 1];

    /// <summary>Quanto o tiro leva do canto do piso até acertar (s).</summary>
    private static double Travel(int kill, bool headshot) => (TrackWall - TrackStart + TrackFloor - Target(kill, headshot)) / ShotSpeed;

    /// <summary>
    /// Onde <paramref name="p"/> fica na trilha do tiro (as fitas do piso e depois o radiador até <paramref name="target"/>):
    /// quanto já se andou até ele e a distância do lado (1 = na borda da trilha).
    /// </summary>
    private static (double Along, double Across) Track(in LedPoint p, double target)
    {
        double floor = Math.Abs(p.V - TrackFloor) / 0.075;
        double wall = p.V >= target - 0.06 ? Math.Abs(p.U - TrackWall) / 0.12 : double.MaxValue;
        return floor <= wall ? (p.U - TrackStart, floor) : (TrackWall - TrackStart + TrackFloor - p.V, wall);
    }

    // As capas de cabo, em pixels da foto (as memórias ficam à esquerda de x = 628).
    private static bool InBoardCable(in LedPoint p) => p.X >= 628 && p.X <= 745 && p.Y >= 510 && p.Y <= 625;
    private static bool InGpuCable(in LedPoint p) => p.X >= 676 && p.X <= 796 && p.Y >= 678 && p.Y <= 765;

    /// <summary>Um número fixo por LED (pela posição), para os efeitos que sorteiam LEDs.</summary>
    private static int LedId(in LedPoint p) => (int)Math.Round(p.U * 4000) * 8191 + (int)Math.Round(p.V * 4000);

    private static double Hash(int a, int b)
    {
        unchecked
        {
            uint h = (uint)a * 0x9E3779B1u ^ (uint)b * 0x85EBCA77u;
            h ^= h >> 15;
            h *= 0x2C1B3C6Du;
            h ^= h >> 12;
            h *= 0x297A2D39u;
            h ^= h >> 15;
            return h / 4294967296.0;
        }
    }

    private static Tone Mix(Tone a, Tone b, double k)
    {
        k = Math.Clamp(k, 0, 1);
        return new Tone(a.R + (b.R - a.R) * k, a.G + (b.G - a.G) * k, a.B + (b.B - a.B) * k);
    }

    private static double Dist(in LedPoint p, (double U, double V) c) => Dist((p.U, p.V), c);

    private static double Dist((double U, double V) a, (double U, double V) b) => Math.Sqrt(Sq(a.U - b.U) + Sq(a.V - b.V));

    private static double Band(double x, double width) => Math.Exp(-x * x / (width * width));

    private static double Smooth(double e0, double e1, double x)
    {
        double k = Math.Clamp((x - e0) / (e1 - e0), 0, 1);
        return k * k * (3 - 2 * k);
    }

    private static double EaseOut(double x)
    {
        x = Math.Clamp(x, 0, 1);
        return 1 - (1 - x) * (1 - x);
    }

    private static double Frac(double x) => x - Math.Floor(x);

    private static double Sq(double x) => x * x;
}
