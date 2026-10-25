using System.Drawing;
using DS339Monitor.Lighting;

namespace DS339Monitor.Argb;

/// <summary>Quadros prontos de uma animação: <c>Frames[quadro][parte]</c> = R, G, B de cada LED (fração de luz, 0–1).</summary>
internal sealed record BootFrames(string Name, int FrameMs, List<float[][]> Frames);

/// <summary>Quadros entregues um de cada vez, em ordem (<see cref="At"/>(0), At(1)…): <c>[parte]</c> = R, G, B de cada LED.</summary>
internal sealed record FrameSource(string Name, int FrameMs, int Count, Func<int, float[][]> At);

/// <summary>
/// Animações de boot desenhadas pelo programa, LED por LED, pela posição de cada LED no gabinete
/// (<see cref="CaseLayout"/>). Saem como quadros prontos: o programa pode tocá-las nos fans ("ver no gabinete") e
/// gravá-las na controladora, que as toca sozinha ao ligar o PC. Já saem com o brilho geral, o limitador de energia e a
/// trava contra pisca-pisca na luz média aplicados — a controladora toca os quadros como estão.
/// </summary>
internal static class BootAnimation
{
    public const string ShortCircuit = "Curto-circuito";
    public const string KnockUp = "Knock Up Stream";
    public const string Haki = "Haki do Rei";
    public const string Awakening = "Despertar";

    public static readonly string[] All = { ShortCircuit, KnockUp, Haki, Awakening };

    public static readonly Dictionary<string, string> Descriptions = new()
    {
        [ShortCircuit] = "Faíscas nos cabos de energia, arcos entre as peças, um estouro e a luz firmando de baixo para cima",
        [KnockUp] = "A corrente sobe dos fans de baixo até o topo e cai de volta em gotas",
        [Haki] = "A bomba junta força e solta ondas de choque pelo gabinete",
        [Awakening] = "Um coração bate duas vezes na bomba e o branco toma o gabinete",
    };

    public const int FrameMs = 33; // ~30 quadros por segundo

    /// <summary>Passo do arquivo longo (animação + o loop dos fans): 25 quadros por segundo, para caber e gravar mais rápido.</summary>
    public const int LongFrameMs = 40;

    /// <summary>Onde a animação entrega a luz ao loop dos fans, antes do fim dela (que apaga), em segundos.</summary>
    public static double HandOff(string name) => name switch { ShortCircuit => 6.1, KnockUp => 6.0, Haki => 4.2, _ => 4.8 };
    private const double MilliampsPerChannel = 50.0 / 3;

    public static BootFrames Render(string name, IReadOnlyList<ArgbZone> zones, ArgbSettings settings, int frameMs = FrameMs)
    {
        var points = zones.Select(z => CaseLayout.Parse(z.Forma, z.Leds, z.Inverter)
            ?? Enumerable.Range(0, z.Leds).Select(i => new LedPoint(0, 0, i / (double)z.Leds, 0.5, double.NaN)).ToArray()).ToArray();
        double seconds = name switch { ShortCircuit => 7.2, KnockUp => 7.0, Haki => 5.2, _ => 6.0 };
        int count = (int)Math.Round(seconds * 1000 / frameMs);
        var frames = new List<float[][]>(count);
        var white = GpuLighting.ParseColor(settings.Cor);
        // Faíscas e arcos saem no máximo mesmo com o brilho geral baixo (são poucos LEDs de cada vez): compensa o brilho
        // que o acabamento aplica depois.
        double boost = 1 / Math.Max(0.05, Math.Pow(settings.Brilho / 100.0, 2));
        var sparks = name == ShortCircuit ? new ShortCircuitScript(points, 1904, boost, frameMs) : null;

        for (int f = 0; f < count; f++)
        {
            double t = f * frameMs / 1000.0;
            var frame = new float[zones.Count][];
            for (int z = 0; z < zones.Count; z++)
            {
                var buffer = new float[zones[z].Leds * 3];
                for (int i = 0; i < zones[z].Leds; i++)
                {
                    var (r, g, b) = name switch
                    {
                        ShortCircuit => sparks!.Shade(z, i, points[z][i], t, white),
                        _ => SceneShade(name, points[z][i], t, seconds, white),
                    };
                    buffer[i * 3] = (float)r;
                    buffer[i * 3 + 1] = (float)g;
                    buffer[i * 3 + 2] = (float)b;
                }
                frame[z] = buffer;
            }
            frames.Add(frame);
        }
        Finish(frames, zones, settings, frameMs);
        return new BootFrames(name, frameMs, frames);
    }

    /// <summary>As cenas do gabinete, uma volta só, entrando e saindo do escuro.</summary>
    private static (double, double, double) SceneShade(string name, in LedPoint p, double t, double seconds, Color white)
    {
        var input = new SceneInput { T = t, NowMs = (long)(t * 1000), Speed = 1, CpuTemp = 50, GpuTemp = 40, GpuLoad = 30, Screen = 0.6 };
        double envelope = Smooth(0, 0.5, t) * (1 - Smooth(seconds - 1.0, seconds, t));
        if (name == Awakening)
        {
            input.Manga = new Manga.MangaState(new[]
            {
                new Manga.MangaCue { Kind = Manga.MangaCueKind.Gear5, At = 0, Seconds = Manga.MangaStage.Gear5Times.Seconds },
            }, true, 0, false, 0, 50);
            var (r, g, b) = CaseScenes.Shade(ArgbEffects.MangaScene, white, p, input);
            double fade = 1 - Smooth(seconds - 1.2, seconds, t);
            double baseCut = Smooth(0, 0.3, t); // a página do mangá (base) entra por baixo do branco
            return (r * fade * baseCut, g * fade * baseCut, b * fade * baseCut);
        }
        string effect = name == Haki ? ArgbEffects.Haki : ArgbEffects.KnockUp;
        var (sr, sg, sb) = CaseScenes.Shade(effect, white, p, input);
        return (sr * envelope, sg * envelope, sb * envelope);
    }

    /// <summary>Brilho geral, limitador de energia e trava de pisca-pisca (na luz média), quadro a quadro.</summary>
    private static void Finish(List<float[][]> frames, IReadOnlyList<ArgbZone> zones, ArgbSettings settings, int frameMs)
    {
        double master = Math.Pow(settings.Brilho / 100.0, 2);
        int leds = zones.Sum(z => z.Leds);
        double budget = settings.LimiteAmperes * 1000.0 - leds;
        var guard = new FlashGuard();
        for (int f = 0; f < frames.Count; f++)
        {
            double sum = 0, luminance = 0;
            for (int z = 0; z < zones.Count; z++)
            {
                double gain = master * zones[z].Brilho / 100.0;
                var buffer = frames[f][z];
                for (int i = 0; i < buffer.Length; i += 3)
                {
                    float r = (float)Math.Clamp(buffer[i] * gain, 0, 1), g = (float)Math.Clamp(buffer[i + 1] * gain, 0, 1), b = (float)Math.Clamp(buffer[i + 2] * gain, 0, 1);
                    buffer[i] = r;
                    buffer[i + 1] = g;
                    buffer[i + 2] = b;
                    sum += r + g + b;
                    luminance += 0.2126 * r + 0.7152 * g + 0.0722 * b;
                }
            }
            double scale = sum * MilliampsPerChannel > budget ? budget / (sum * MilliampsPerChannel) : 1;
            int gray = (int)Math.Round(Math.Clamp(luminance * scale / Math.Max(1, leds), 0, 1) * 255);
            var guarded = guard.Apply(Color.FromArgb(gray, gray, gray), f * (long)frameMs);
            if (gray > 0 && guarded.R < gray) scale *= guarded.R / (double)gray;
            if (scale >= 1) continue;
            foreach (var buffer in frames[f])
                for (int i = 0; i < buffer.Length; i++) buffer[i] = (float)(buffer[i] * scale);
        }
    }

    private static double Smooth(double e0, double e1, double x)
    {
        double t = Math.Clamp((x - e0) / (e1 - e0), 0, 1);
        return t * t * (3 - 2 * t);
    }

    /// <summary>
    /// O arquivo longo: a animação até o ponto de entrega, uma transição de 1 s e o loop dos fans (o efeito configurado,
    /// calculado pelo mesmo motor de quando o programa está aberto) por <paramref name="seconds"/> segundos. Quadros
    /// entregues em ordem, um de cada vez (o loop é calculado na hora, sem guardar minutos de quadros na memória).
    /// </summary>
    public static FrameSource Combined(string name, ArgbSettings settings, int seconds)
    {
        var zones = settings.Zonas;
        var intro = Render(name, zones, settings, LongFrameMs);
        int cut = Math.Min(intro.Frames.Count, (int)Math.Round(HandOff(name) * 1000 / LongFrameMs));
        int cross = 1000 / LongFrameMs;
        int loopFrames = seconds * 1000 / LongFrameMs;
        var loop = ArgbLighting.Offline(settings);
        var hold = intro.Frames[cut - 1];
        return new FrameSource($"{name} + {seconds / 60} min do loop", LongFrameMs, cut + loopFrames, f =>
        {
            if (f < cut) return intro.Frames[f];
            var live = loop.Frame((f - cut) * (long)LongFrameMs);
            int k = f - cut;
            if (k >= cross) return live;
            double a = k / (double)cross;
            a = a * a * (3 - 2 * a);
            var mixed = new float[live.Length][];
            for (int z = 0; z < live.Length; z++)
            {
                mixed[z] = new float[live[z].Length];
                for (int i = 0; i < live[z].Length; i++) mixed[z][i] = (float)(hold[z][i] + (live[z][i] - hold[z][i]) * a);
            }
            return mixed;
        });
    }

    /// <summary>Só a animação (como quadros prontos).</summary>
    public static FrameSource Alone(BootFrames frames) => new(frames.Name, frames.FrameMs, frames.Frames.Count, f => frames.Frames[f]);

    /// <summary>
    /// O roteiro do curto-circuito (sorteado uma vez, com semente fixa: sai sempre igual). As faíscas nascem nos cabos
    /// de energia (as capas de LED), na placa de baixo e na bomba; cada uma é um clarão branco-azulado de 1–2 quadros
    /// num punhado de LEDs, que vira brasa laranja e se apaga. São só pontos — a luz do gabinete inteiro pisca no máximo
    /// 3 vezes por segundo (trava na luz média).
    /// </summary>
    private sealed class ShortCircuitScript
    {
        private readonly record struct Spark(double At, double X, double Y, double Radius, double Power);

        private readonly record struct Arc(double At, double Duration, (double X, double Y)[] Path);

        private readonly List<Spark> _sparks = new();
        private readonly List<Arc> _arcs = new();

        // Tempos do roteiro (s).
        private const double SparksFrom = 0.5, Bang = 3.25, PowerFrom = 3.9, PowerFull = 5.3, FadeFrom = 6.1, End = 7.2;

        private readonly double _boost;
        private readonly int _frameMs;

        public ShortCircuitScript(LedPoint[][] points, int seed, double boost, int frameMs)
        {
            _boost = boost;
            _frameMs = frameMs;
            var random = new Random(seed);
            // Onde há "fio": as capas de cabo pesam mais; depois a placa sob a placa-mãe e a bomba; o resto às vezes.
            var hot = new List<(double X, double Y, double Weight)>();
            foreach (var zone in points)
                foreach (var p in zone)
                {
                    double w = 1;
                    if (InBox(p, 600, 500, 800, 770)) w = 6;          // as duas capas de cabo (e memórias)
                    else if (p.Y > 1060 && p.Y < 1100 && p.X < 720) w = 3; // placa sob a placa-mãe
                    else if (Math.Abs(p.X - 463) < 160 && Math.Abs(p.Y - 665) < 100) w = 3; // bomba
                    hot.Add((p.X, p.Y, w));
                }
            double total = hot.Sum(h => h.Weight);
            (double X, double Y) Pick()
            {
                double r = random.NextDouble() * total;
                foreach (var h in hot)
                {
                    r -= h.Weight;
                    if (r <= 0) return (h.X, h.Y);
                }
                return (hot[^1].X, hot[^1].Y);
            }

            // Faíscas: começam raras e vão ficando seguidas até o estouro; poucas depois, enquanto a energia firma.
            double at = SparksFrom;
            while (at < PowerFull)
            {
                double rate = at < Bang ? 2.5 + 16 * Math.Pow((at - SparksFrom) / (Bang - SparksFrom), 1.6) : 3;
                at += -Math.Log(1 - random.NextDouble()) / rate;
                var (x, y) = Pick();
                _sparks.Add(new Spark(at, x, y, 8 + 18 * random.NextDouble(), 0.75 + 0.25 * random.NextDouble()));
                if (random.NextDouble() < 0.35) // faísca dupla, logo em seguida
                    _sparks.Add(new Spark(at + 0.05 + 0.08 * random.NextDouble(), x + random.Next(-25, 26), y + random.Next(-25, 26), 5 + 8 * random.NextDouble(), 0.6));
            }

            // Arcos: um fio de luz em zigue-zague pulando entre dois pontos de fio próximos.
            foreach (double when in new[] { 1.35, 2.05, 2.55, 2.9, Bang })
            {
                var a = Pick();
                (double X, double Y) b;
                int tries = 0;
                do b = Pick(); while (++tries < 50 && Distance(a, b) is < 70 or > 260);
                if (when == Bang) (a, b) = ((736, 720), (463, 665)); // o estouro: do cabo da placa de vídeo até a bomba
                _arcs.Add(new Arc(when, when == Bang ? 0.16 : 0.1, Jagged(a, b, random)));
            }
        }

        private static bool InBox(in LedPoint p, double x0, double y0, double x1, double y1) => p.X >= x0 && p.X <= x1 && p.Y >= y0 && p.Y <= y1;

        private static double Distance((double X, double Y) a, (double X, double Y) b) => Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));

        private static (double X, double Y)[] Jagged((double X, double Y) a, (double X, double Y) b, Random random)
        {
            const int Segments = 6;
            var path = new (double X, double Y)[Segments + 1];
            double nx = -(b.Y - a.Y), ny = b.X - a.X, length = Math.Max(1, Math.Sqrt(nx * nx + ny * ny));
            for (int i = 0; i <= Segments; i++)
            {
                double k = i / (double)Segments, off = i == 0 || i == Segments ? 0 : (random.NextDouble() - 0.5) * 50;
                path[i] = (a.X + (b.X - a.X) * k + nx / length * off, a.Y + (b.Y - a.Y) * k + ny / length * off);
            }
            return path;
        }

        public (double R, double G, double B) Shade(int zone, int led, in LedPoint p, double t, Color white)
        {
            double wr = white.R / 255.0, wg = white.G / 255.0, wb = white.B / 255.0;
            // A energia do gabinete: escuro, a queda no estouro, depois sobe do piso ao topo, firma e apaga.
            double mains = 0;
            if (t >= Bang && t < PowerFrom)
            {
                // Depois do estouro, um zumbido fraco que falha duas vezes (queda de energia).
                double x = t - Bang;
                mains = 0.18 * Smooth(0.05, 0.15, x) * (x < 0.32 || x > 0.42 ? 1 : 0.2) * (x < 0.5 ? 1 : 0.6);
            }
            else if (t >= PowerFrom)
            {
                double front = 1.08 - 1.2 * Smooth(PowerFrom, PowerFull, t);
                double lit = p.V >= front ? 1 : Math.Exp(-(front - p.V) * (front - p.V) / 0.004);
                double level = 0.12 + 0.68 * Smooth(PowerFrom, PowerFull + 0.3, t);
                double hum = 1 + 0.03 * Math.Sin(2 * Math.PI * 1.7 * t + p.U * 4);
                double fade = 1 - Smooth(FadeFrom, End, t);
                mains = level * lit * hum * fade;
            }
            double k = Math.Pow(Math.Clamp(mains, 0, 1), 2.2);
            double r = wr * k, g = wg * k, b = wb * k;

            // Faíscas: clarão branco-azulado de 1–2 quadros e a brasa laranja apagando.
            foreach (var s in _sparks)
            {
                double age = t - s.At;
                if (age < 0 || age > 0.6) continue;
                double dx = p.X - s.X, dy = p.Y - s.Y, d2 = dx * dx + dy * dy;
                if (d2 > s.Radius * s.Radius * 9) continue;
                double near = Math.Exp(-d2 / (s.Radius * s.Radius));
                double flash = age < 0.07 ? 1 : 0;
                double ember = Math.Exp(-age / 0.2) * 0.8;
                double fl = flash * near * s.Power * _boost;
                double em = Math.Pow(ember * near * s.Power, 2.2) * _boost;
                r += fl * 0.9 + em * 1.0;
                g += fl * 0.95 + em * 0.38;
                b += fl * 1.0 + em * 0.08;
            }

            // Arcos: tudo perto do zigue-zague acende forte, piscando dentro dos poucos quadros dele.
            foreach (var arc in _arcs)
            {
                double age = t - arc.At;
                if (age < 0 || age > arc.Duration + 0.25) continue;
                double d = DistanceToPath(p.X, p.Y, arc.Path);
                if (d > 40) continue;
                double near = Math.Exp(-d * d / (14.0 * 14.0));
                double on = age < arc.Duration ? (((int)(age * 1000 / _frameMs)) % 2 == 0 ? 1 : 0.55) : 0;
                double glow = age >= arc.Duration ? 0.35 * Math.Exp(-(age - arc.Duration) / 0.08) : 0;
                double v = (on + glow) * near * _boost;
                r += v * 0.85;
                g += v * 0.92;
                b += v;
            }
            return (r, g, b); // o acabamento aplica o brilho e corta em 1
        }

        private static double DistanceToPath(double x, double y, (double X, double Y)[] path)
        {
            double best = double.MaxValue;
            for (int i = 1; i < path.Length; i++)
            {
                var (ax, ay) = path[i - 1];
                var (bx, by) = path[i];
                double vx = bx - ax, vy = by - ay, len2 = vx * vx + vy * vy;
                double k = len2 == 0 ? 0 : Math.Clamp(((x - ax) * vx + (y - ay) * vy) / len2, 0, 1);
                double dx = x - (ax + vx * k), dy = y - (ay + vy * k);
                best = Math.Min(best, Math.Sqrt(dx * dx + dy * dy));
            }
            return best;
        }
    }
}
