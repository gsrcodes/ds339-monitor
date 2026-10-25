using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using DS339Monitor.App;
using DS339Monitor.Lighting;
using DS339Monitor.Manga;

namespace DS339Monitor.Argb;

/// <summary>
/// <c>--fitas-previa "Cena" [saida.png] [quadros] [segundos] [momento do Modo Mangá]</c>: desenha uma cena dos fans sobre
/// a foto do gabinete em vários instantes, sem tocar na controladora (para conferir as animações). Um efeito de parte
/// (ex.: "Meteoros") sai do motor de verdade, em todas as partes; os instantes vão em <c>quadros</c> e
/// <c>"0.1,0.4,…"</c>.
/// </summary>
internal static class ArgbPreview
{
    public static int Run(string[] args)
    {
        string effect = args.Length > 1 ? args[1] : ArgbEffects.KnockUp;
        string output = args.Length > 2 ? args[2] : Path.Combine(AppContext.BaseDirectory, "fitas-previa.png");
        int frames = args.Length > 3 && int.TryParse(args[3], out int f) ? Math.Clamp(f, 1, 24) : 8;
        double seconds = args.Length > 4 && double.TryParse(args[4], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double s) ? s : 7;
        string? cue = args.Length > 5 ? args[5] : null;

        var config = AppConfig.Load(Path.Combine(AppContext.BaseDirectory, "config.json"));
        var zones = config.FansEFitas.Zonas;
        if (effect.StartsWith("cs2:", StringComparison.OrdinalIgnoreCase)) return RunCs2(effect[4..], output, frames, seconds, config);
        if (BootAnimation.All.Contains(effect)) return RunBoot(effect, output, args, config);
        int[]? crop = null;
        if (effect.EndsWith("@cabos", StringComparison.OrdinalIgnoreCase))
        {
            effect = effect[..^6];
            crop = new[] { 575, 495, 855, 775 }; // as duas capas de cabo de perto
        }
        if (effect.StartsWith("umbra:", StringComparison.OrdinalIgnoreCase))
        {
            // Modo Umbra: "umbra:Sincronizado:5", "umbra:Combinado:2", "umbra:Tela:storm" ou
            // "umbra:Independente:<arranjo>:<efeito do centro>:<efeito das laterais>" (em todos os fans).
            string[] spec = effect.Split(':');
            var u = config.FansEFitas.Umbra;
            u.Ligado = true;
            u.LuzesAcesas = true;
            u.Modo = UmbraModes.All.FirstOrDefault(m => m.Equals(spec.ElementAtOrDefault(1), StringComparison.OrdinalIgnoreCase)) ?? UmbraModes.Sync;
            int Id(int at) => int.TryParse(spec.ElementAtOrDefault(at), out int v) ? v : 1;
            switch (u.Modo)
            {
                case UmbraModes.Sync: u.Sincronizado.Efeito = Id(2); break;
                case UmbraModes.Merge: u.Combinado.Efeito = Id(2); break;
                case UmbraModes.Canvas: u.Tela.Efeito = spec.ElementAtOrDefault(2) ?? "rainbow"; break;
                default:
                    foreach (var port in u.Portas)
                    {
                        port.Arranjo = spec.ElementAtOrDefault(2) ?? UmbraZones.LayoutAll;
                        port.Slot(UmbraZones.All).Efeito = Id(3);
                        port.Slot(UmbraZones.Center).Efeito = Id(3);
                        foreach (string side in new[] { UmbraZones.Parallel, UmbraZones.Series, UmbraZones.Left }) port.Slot(side).Efeito = Id(4);
                        port.Slot(UmbraZones.Right).Efeito = spec.Length > 5 ? Id(5) : Id(4);
                    }
                    break;
            }
            config.FansEFitas.Normalize();
            var umbraEngine = ArgbLighting.Offline(config.FansEFitas);
            const int UmbraMs = 33;
            return RunBoot(effect, output, args, config, new FrameSource(effect, UmbraMs, int.MaxValue / 2, f => umbraEngine.Frame(f * (long)UmbraMs)), crop);
        }
        if (ArgbEffects.All.Contains(effect) && !ArgbEffects.IsScene(effect))
        {
            // Efeitos de parte: o motor de verdade, com o efeito em todas as partes.
            config.FansEFitas.Efeito = effect;
            foreach (var zone in zones) zone.Efeito = ArgbEffects.Global;
            var engine = ArgbLighting.Offline(config.FansEFitas);
            const int Ms = 33;
            return RunBoot(effect, output, args, config, new FrameSource(effect, Ms, int.MaxValue / 2, f => engine.Frame(f * (long)Ms)), crop);
        }
        if (effect.Split('+') is [var boot, var extra] && BootAnimation.All.Contains(boot) && int.TryParse(extra, out int loopMinutes))
            return RunBoot(effect, output, args, config, BootAnimation.Combined(boot, config.FansEFitas, loopMinutes * 60));
        string photoPath = Path.Combine(AppContext.BaseDirectory, "ui", CaseLayout.Photo);
        if (!File.Exists(photoPath))
        {
            Log.Warn($"Fans e fitas: sem a foto do gabinete em {photoPath}.");
            return 1;
        }

        using var photo = new Bitmap(photoPath);
        int[] view = CaseLayout.View;
        const int Tile = 420;
        int cols = Math.Min(frames, 4), rows = (frames + cols - 1) / cols;
        using var sheet = new Bitmap(cols * Tile, rows * (Tile + 22), PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(sheet);
        g.Clear(Color.Black);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        using var font = new Font("Segoe UI", 10);
        using var dark = new ImageAttributes();
        dark.SetColorMatrix(new ColorMatrix(new[]
        {
            new[] { 0.28f, 0, 0, 0, 0 }, new[] { 0, 0.28f, 0, 0, 0 }, new[] { 0, 0, 0.28f, 0, 0 }, new[] { 0, 0, 0, 1f, 0 }, new[] { 0, 0, 0, 0, 1f },
        }));

        var color = GpuLighting.ParseColor(config.FansEFitas.Cor);
        var scene = new SceneInput { Speed = 1, CpuTemp = 68, GpuTemp = 72, GpuLoad = 80, Screen = 0.6 };
        var kind = cue is not null && Enum.TryParse<MangaCueKind>(cue, true, out var k) ? k : (MangaCueKind?)null;
        double scale = Tile / (double)(view[2] - view[0]);
        for (int i = 0; i < frames; i++)
        {
            double t = frames == 1 ? 0 : seconds * i / frames;
            scene.T = t;
            scene.NowMs = (long)(t * 1000);
            scene.Manga = kind is { } kk
                ? new MangaState(new[] { new MangaCue { Kind = kk, At = 0, Seconds = kk == MangaCueKind.Gear5 ? MangaStage.Gear5Times.Seconds : Math.Max(seconds, 2) } }, true, 0, false, 0, 70)
                : null;
            int ox = i % cols * Tile, oy = i / cols * (Tile + 22);
            g.DrawImage(photo, new Rectangle(ox, oy, Tile, Tile), view[0], view[1], view[2] - view[0], view[3] - view[1], GraphicsUnit.Pixel, dark);
            foreach (var zone in zones)
            {
                var points = CaseLayout.Parse(zone.Forma, zone.Leds, zone.Inverter);
                if (points is null) continue;
                string zoneEffect = zone.Efeito == ArgbEffects.Global ? effect : zone.Efeito;
                if (!ArgbEffects.IsScene(zoneEffect)) zoneEffect = effect;
                foreach (var p in points)
                {
                    var (r, gg, b) = CaseScenes.Shade(zoneEffect, color, p, scene);
                    int R = Display(r), G = Display(gg), B = Display(b);
                    double level = Math.Max(R, Math.Max(G, B)) / 255.0;
                    if (level < 0.02) continue;
                    float x = (float)(ox + (p.X - view[0]) * scale), y = (float)(oy + (p.Y - view[1]) * scale);
                    float rad = (float)((5 + 14 * level) * scale);
                    using var path = new GraphicsPath();
                    path.AddEllipse(x - rad, y - rad, rad * 2, rad * 2);
                    using var brush = new PathGradientBrush(path)
                    {
                        CenterColor = Color.FromArgb(240, R, G, B),
                        SurroundColors = new[] { Color.FromArgb(0, R, G, B) },
                    };
                    g.FillPath(brush, path);
                }
            }
            g.DrawString($"{t:0.0} s", font, Brushes.White, ox + 6, oy + Tile + 2);
        }
        g.DrawString(effect + (cue is null ? "" : $" · {cue}"), font, Brushes.Gold, 6, 4);
        sheet.Save(output, ImageFormat.Png);
        Log.Info($"Fans e fitas: prévia de {effect} salva em {output}");
        return 0;
    }

    private static int Display(double v) => (int)Math.Round(255 * Math.Pow(Math.Clamp(v, 0, 1), 1 / 2.2));

    /// <summary>
    /// Reações ao CS2: <c>cs2:Evento[+Evento…]</c> com os acontecimentos (Planted, Exploded, Defused, RoundWon,
    /// RoundLost, Kill1…Kill5, Headshot1…, Ace, Died) e os estados (Flash — que vai passando —, Smoke, Fire, LowHp),
    /// todos no instante 0, sobre o branco fraco de base.
    /// </summary>
    private static int RunCs2(string spec, string output, int frames, double seconds, AppConfig config)
    {
        const long Start = 100_000;
        var events = new List<Game.Cs2Events.Event>();
        long planted = 0, died = 0;
        bool flash = false, smoke = false, fire = false;
        int health = 100, kills = 0;
        foreach (string part in spec.Split('+', StringSplitOptions.RemoveEmptyEntries))
        {
            string name = part.TrimEnd("0123456789".ToCharArray());
            int value = int.TryParse(part[name.Length..], out int n) ? n : 1;
            switch (name.ToLowerInvariant())
            {
                case "planted": planted = Start; break;
                case "died": died = Start; health = 0; break;
                case "flash": flash = true; break;
                case "smoke": smoke = true; break;
                case "fire": fire = true; break;
                case "lowhp": health = 12; break;
                default:
                    if (!Enum.TryParse<Game.Cs2Events.Kind>(name, true, out var kind))
                    {
                        Log.Warn($"Fans e fitas: acontecimento do CS2 desconhecido: {part}.");
                        return 1;
                    }
                    if (kind is Game.Cs2Events.Kind.Kill or Game.Cs2Events.Kind.Headshot) kills = Math.Max(kills, value);
                    events.Add(new Game.Cs2Events.Event(kind, Start, kind is Game.Cs2Events.Kind.Kill or Game.Cs2Events.Kind.Headshot ? value : 0));
                    break;
            }
        }

        using var photo = new Bitmap(Path.Combine(AppContext.BaseDirectory, "ui", CaseLayout.Photo));
        int[] view = CaseLayout.View;
        const int Tile = 360;
        int cols = Math.Min(frames, 4), rows = (frames + cols - 1) / cols;
        using var sheet = new Bitmap(cols * Tile, rows * (Tile + 22), PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(sheet);
        g.Clear(Color.Black);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var font = new Font("Segoe UI", 10);
        using var dark = new ImageAttributes();
        dark.SetColorMatrix(new ColorMatrix(new[]
        {
            new[] { 0.22f, 0, 0, 0, 0 }, new[] { 0, 0.22f, 0, 0, 0 }, new[] { 0, 0, 0.22f, 0, 0 }, new[] { 0, 0, 0, 1f, 0 }, new[] { 0, 0, 0, 0, 1f },
        }));
        double scale = Tile / (double)(view[2] - view[0]);
        const double Base = 0.12; // o efeito de sempre por baixo (branco fraco)
        for (int i = 0; i < frames; i++)
        {
            double t = frames == 1 ? 0 : seconds * i / (frames - 1);
            long now = Start + (long)(t * 1000);
            var moment = new Game.Cs2Moment(now, events.ToArray(), planted, died, flash ? Math.Max(0, 1 - t / Math.Max(seconds, 0.1)) : 0,
                smoke ? 1 : 0, fire ? 0.8 : 0, health, kills);
            int ox = i % cols * Tile, oy = i / cols * (Tile + 22);
            g.DrawImage(photo, new Rectangle(ox, oy, Tile, Tile), view[0], view[1], view[2] - view[0], view[3] - view[1], GraphicsUnit.Pixel, dark);
            foreach (var zone in config.FansEFitas.Zonas)
            {
                var points = CaseLayout.Parse(zone.Forma, zone.Leds, zone.Inverter);
                if (points is null) continue;
                foreach (var p in points)
                {
                    var shade = Game.GameLights.At(moment, p);
                    int R = Display(Base * shade.Dim + shade.R), G = Display(Base * shade.Dim + shade.G), B = Display(Base * shade.Dim + shade.B);
                    double level = Math.Max(R, Math.Max(G, B)) / 255.0;
                    if (level < 0.02) continue;
                    float x = (float)(ox + (p.X - view[0]) * scale), y = (float)(oy + (p.Y - view[1]) * scale);
                    float rad = (float)((5 + 16 * level) * scale);
                    using var path = new GraphicsPath();
                    path.AddEllipse(x - rad, y - rad, rad * 2, rad * 2);
                    using var brush = new PathGradientBrush(path)
                    {
                        CenterColor = Color.FromArgb(245, R, G, B),
                        SurroundColors = new[] { Color.FromArgb(0, R, G, B) },
                    };
                    g.FillPath(brush, path);
                }
            }
            string label = $"{t:0.00} s" + (moment.Banner is { } banner ? $" · {banner.Text}" : "");
            g.DrawString(label, font, Brushes.White, ox + 6, oy + Tile + 2);
        }
        g.DrawString($"CS2 · {spec}", font, Brushes.Gold, 6, 4);
        sheet.Save(output, ImageFormat.Png);
        Log.Info($"Fans e fitas: prévia das reações {spec} salva em {output}");
        return 0;
    }

    /// <summary>Os quadros de uma fonte longa até o último instante pedido (só para a prévia).</summary>
    private static BootFrames Collect(FrameSource source, string[] args)
    {
        double last = args.Length > 4 ? args[4].Split(',').Max(x => double.Parse(x, System.Globalization.CultureInfo.InvariantCulture)) : 30;
        int count = Math.Min(source.Count, (int)(last * 1000 / source.FrameMs) + 2);
        var frames = new List<float[][]>(count);
        for (int f = 0; f < count; f++) frames.Add(source.At(f));
        return new BootFrames(source.Name, source.FrameMs, frames);
    }

    /// <summary>
    /// Animação de boot: quadros escolhidos (args[3] = quantos, espalhados; ou args[4] = instantes "1.0,1.2,…" em
    /// segundos) desenhados sobre a foto, exatamente como vão para os LEDs (com brilho e limitador aplicados).
    /// </summary>
    private static int RunBoot(string name, string output, string[] args, AppConfig config, FrameSource? source = null, int[]? crop = null)
    {
        var zones = config.FansEFitas.Zonas;
        var animation = source is null ? BootAnimation.Render(name, zones, config.FansEFitas) : Collect(source, args);
        double total = animation.Frames.Count * animation.FrameMs / 1000.0;
        var times = args.Length > 4
            ? args[4].Split(',').Select(x => double.Parse(x, System.Globalization.CultureInfo.InvariantCulture)).ToList()
            : Enumerable.Range(0, args.Length > 3 && int.TryParse(args[3], out int n) ? Math.Clamp(n, 1, 24) : 12)
                .Select(i => total * i / (args.Length > 3 && int.TryParse(args[3], out int m) ? Math.Clamp(m, 1, 24) : 12)).ToList();

        using var photo = new Bitmap(Path.Combine(AppContext.BaseDirectory, "ui", CaseLayout.Photo));
        int[] view = crop ?? CaseLayout.View;
        const int Tile = 360;
        int cols = Math.Min(times.Count, 4), rows = (times.Count + cols - 1) / cols;
        using var sheet = new Bitmap(cols * Tile, rows * (Tile + 22), PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(sheet);
        g.Clear(Color.Black);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var font = new Font("Segoe UI", 10);
        using var dark = new ImageAttributes();
        dark.SetColorMatrix(new ColorMatrix(new[]
        {
            new[] { 0.22f, 0, 0, 0, 0 }, new[] { 0, 0.22f, 0, 0, 0 }, new[] { 0, 0, 0.22f, 0, 0 }, new[] { 0, 0, 0, 1f, 0 }, new[] { 0, 0, 0, 0, 1f },
        }));
        double scale = Tile / (double)(view[2] - view[0]);
        for (int i = 0; i < times.Count; i++)
        {
            int frame = Math.Clamp((int)Math.Round(times[i] * 1000 / animation.FrameMs), 0, animation.Frames.Count - 1);
            int ox = i % cols * Tile, oy = i / cols * (Tile + 22);
            g.DrawImage(photo, new Rectangle(ox, oy, Tile, Tile), view[0], view[1], view[2] - view[0], view[3] - view[1], GraphicsUnit.Pixel, dark);
            for (int z = 0; z < zones.Count; z++)
            {
                var points = CaseLayout.Parse(zones[z].Forma, zones[z].Leds, zones[z].Inverter);
                if (points is null) continue;
                var buffer = animation.Frames[frame][z];
                for (int k = 0; k < points.Length; k++)
                {
                    int R = Display(buffer[k * 3]), G = Display(buffer[k * 3 + 1]), B = Display(buffer[k * 3 + 2]);
                    double level = Math.Max(R, Math.Max(G, B)) / 255.0;
                    if (level < 0.02) continue;
                    float x = (float)(ox + (points[k].X - view[0]) * scale), y = (float)(oy + (points[k].Y - view[1]) * scale);
                    float rad = (float)((crop is null ? 5 + 16 * level : 2 + 4 * level) * scale);
                    using var path = new GraphicsPath();
                    path.AddEllipse(x - rad, y - rad, rad * 2, rad * 2);
                    using var brush = new PathGradientBrush(path)
                    {
                        CenterColor = Color.FromArgb(245, R, G, B),
                        SurroundColors = new[] { Color.FromArgb(0, R, G, B) },
                    };
                    g.FillPath(brush, path);
                }
            }
            g.DrawString($"{frame * animation.FrameMs / 1000.0:0.00} s", font, Brushes.White, ox + 6, oy + Tile + 2);
        }
        g.DrawString($"{name} · {animation.Frames.Count} quadros de {animation.FrameMs} ms", font, Brushes.Gold, 6, 4);
        sheet.Save(output, ImageFormat.Png);
        Log.Info($"Animação de boot {name}: prévia salva em {output}");
        return 0;
    }
}
