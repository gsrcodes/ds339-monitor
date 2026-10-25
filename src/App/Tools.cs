using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using DS339Monitor.Lighting;
using DS339Monitor.Sensors;
using DS339Monitor.Themes;

namespace DS339Monitor.App;

/// <summary>Comandos de linha de comando que não usam as telas.</summary>
internal static class Tools
{
    /// <summary>
    /// <c>--renderizar "Tema" [saida.png] [ds339|proflow] [arquivo.gif] [estilo] [quadro]</c>: desenha o tema com valores
    /// reais num PNG (sem girar). Para o tema GIF, o arquivo e o estilo de dados vêm dos argumentos; em temas animados,
    /// [quadro] escolhe qual quadro desenhar (padrão: um terço da animação).
    /// </summary>
    public static int RenderToFile(string[] args)
    {
        string themeName = args.Length > 1 ? args[1] : MareTheme.Name;
        string output = Path.GetFullPath(args.Length > 2 ? args[2] : Path.Combine(AppContext.BaseDirectory, "preview.png"));
        var kind = (args.Length > 3 ? args[3].ToLowerInvariant() : "") switch
        {
            "proflow" => ScreenKind.ProFlow,
            "thermalright" => ScreenKind.Thermalright,
            _ => ScreenKind.Ds339,
        };
        var config = new AppConfig(); // só em memória
        if (args.Length > 4)
        {
            string media = Path.GetFullPath(args[4]);
            config.ProFlow.Gif = config.Thermalright.Gif = media;
            config.Thermalright.Video = media;
        }
        if (args.Length > 5) config.ProFlow.Dados = config.Thermalright.Dados = args[5];

        using var theme = ThemeCatalog.Load(kind, themeName, config);
        using var sensors = new SensorHub();
        sensors.Configure(theme.SensorKeys, null);
        Thread.Sleep(1000); // uso de CPU e rede precisam de duas amostras

        using var renderer = new ThemeRenderer(theme);
        // Gráficos precisam de histórico; GIFs mostram um quadro do meio da animação.
        int samples = theme.Elements.Any(e => e is ChartElement) ? 60 : 1;
        for (int i = 0; i < samples; i++)
        {
            if (i > 0) Thread.Sleep(150);
            sensors.Update();
            renderer.Render(sensors, force: true);
        }
        int frame = args.Length > 6 ? PreviewFrame(args[6], theme, renderer) : renderer.FrameCount / 3;
        for (int i = 0; i < frame % renderer.FrameCount; i++) renderer.AdvanceFrame();
        renderer.Render(sensors, force: true);
        renderer.Canvas.Save(output, ImageFormat.Png);
        Log.Info($"Prévia de '{theme.Name}' ({kind}) salva em {output}");
        return 0;
    }

    /// <summary>
    /// Quadro pedido: um número, ou — na apresentação de painéis — "pN:fração" (o painel N, contando de 1, na fração
    /// indicada do movimento da câmera) ou "tN:fração" (a transição que sai do painel N). Sem fração, o meio.
    /// </summary>
    private static int PreviewFrame(string spec, Theme theme, ThemeRenderer renderer)
    {
        if (theme.Animation is PanelSlideshow slideshow && spec.Length > 1 && spec[0] is 'p' or 't')
        {
            var parts = spec[1..].Split(':');
            if (int.TryParse(parts[0], out int panel) && panel >= 1)
            {
                double t = parts.Length > 1 && double.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double v) ? v : 0.5;
                return spec[0] == 'p' ? slideshow.PanOf(panel - 1, t) : slideshow.TransitionOf(panel - 1, t);
            }
        }
        return int.TryParse(spec, out int frame) ? frame : renderer.FrameCount / 3;
    }

    /// <summary>
    /// <c>--capturar</c>: pede à instância em execução (mesmo que elevada) que salve o quadro atual de cada tela.
    /// A conversa é por um arquivo-sinal na pasta do programa, que qualquer processo do usuário pode criar.
    /// </summary>
    public static int RequestCapture()
    {
        var paths = new[] { MonitorService.CapturePath(ScreenKind.Ds339), MonitorService.CapturePath(ScreenKind.ProFlow), MonitorService.CapturePath(ScreenKind.Thermalright) };
        var before = paths.Select(p => File.Exists(p) ? File.GetLastWriteTimeUtc(p) : DateTime.MinValue).ToArray();
        File.WriteAllText(MonitorService.CaptureFlagPath, "");
        for (int attempt = 0; attempt < 25; attempt++)
        {
            Thread.Sleep(200);
            if (!File.Exists(MonitorService.CaptureFlagPath)) // a instância já atendeu
                return paths.Where((p, i) => File.Exists(p) && File.GetLastWriteTimeUtc(p) > before[i]).Any() ? 0 : 1;
        }
        try { File.Delete(MonitorService.CaptureFlagPath); } catch (IOException) { }
        Log.Warn("Nenhuma instância salvou as imagens (programa fechado ou telas desconectadas).");
        return 1;
    }

    /// <summary>
    /// <c>--led-gpu</c>: identifica o LED da placa de vídeo e registra o estado de cada canal no log — só leitura, nada é
    /// gravado na placa.
    /// </summary>
    public static int GpuLedDiagnose()
    {
        using var led = AsrockGpuLed.TryOpen(out string reason);
        if (led is null)
        {
            Log.Info($"LED da GPU: não controlável — {reason}.");
            return 1;
        }
        Log.Info($"LED da GPU: {led.Description}");
        foreach (var (channel, state) in led.Original) Log.Info($"    canal {channel}: {state}");
        return 0;
    }

    /// <summary>
    /// <c>--led-gpu-teste "Efeito" [segundos] [#cor] [início]</c>: roda um efeito do LED da placa por alguns segundos e
    /// devolve as cores originais — com todas as travas do uso normal. Nos efeitos de temperatura e uso, simula uma subida
    /// de 30 a 90; nos que acompanham a Pro Flow, uma Pro Flow virtual com o tema One Piece (a partir de [início] s).
    /// Só roda com o programa principal fechado (os dois não podem mexer no LED ao mesmo tempo).
    /// </summary>
    public static int GpuLedTest(string[] args)
    {
        var settings = new GpuLedSettings
        {
            Efeito = args.Length > 1 ? args[1] : GpuEffects.Rainbow,
            Cor = args.Length > 3 ? args[3] : "#00A2FF",
        };
        int seconds = args.Length > 2 && int.TryParse(args[2], out int s) ? Math.Clamp(s, 1, 600) : 8;
        if (!GpuEffects.All.Contains(settings.Efeito))
        {
            Log.Warn($"Efeito desconhecido: {settings.Efeito}. Opções: {string.Join(", ", GpuEffects.All)}");
            return 1;
        }

        using var screen = GpuEffects.UsesScreen(settings.Efeito) ? VirtualProFlow.Open() : null;
        screen?.Skip(args.Length > 4 && double.TryParse(args[4], NumberStyles.Float, CultureInfo.InvariantCulture, out double from) ? from : 0);
        using var lighting = new GpuLighting(settings);
        lighting.Start();
        long end = Clock.Ms + seconds * 1000L;
        long nextFrame = Clock.Ms;
        string? lastStatus = null;
        while (Clock.Ms < end)
        {
            double progress = 1 - (end - Clock.Ms) / (seconds * 1000.0);
            lighting.UpdateSensors(30 + 60 * progress, 100 * progress); // temperatura 30→90 °C, uso 0→100%
            if (screen is not null)
            {
                for (; nextFrame <= Clock.Ms; nextFrame += PanelSlideshow.FrameMs) lighting.UpdateScreen(screen.Next(Clock.Ms));
            }
            if (lighting.Status != lastStatus) Log.Info($"LED da GPU (teste): {lastStatus = lighting.Status}");
            Thread.Sleep(screen is null ? 200 : 10);
        }
        lighting.Stop();
        return 0;
    }

    /// <summary>
    /// <c>--led-gpu-simular "Efeito" [saida.png] [segundos] [início]</c>: roda o efeito sobre uma Pro Flow virtual com o
    /// tema One Piece — nada vai para a placa nem para a tela — e desenha num PNG a luz ao longo do tempo: a cor que o LED
    /// teria, o nível dela (na escala do olho), o brilho da imagem e os painéis, com o momento de cada um.
    /// </summary>
    public static int GpuLedSimulate(string[] args)
    {
        var settings = new GpuLedSettings { Efeito = args.Length > 1 ? args[1] : GpuEffects.Manga };
        if (!GpuEffects.All.Contains(settings.Efeito))
        {
            Log.Warn($"Efeito desconhecido: {settings.Efeito}. Opções: {string.Join(", ", GpuEffects.All)}");
            return 1;
        }
        string output = Path.GetFullPath(args.Length > 2 ? args[2] : Path.Combine(AppContext.BaseDirectory, "led-simulacao.png"));
        int seconds = args.Length > 3 && int.TryParse(args[3], out int s) ? Math.Clamp(s, 5, 900) : 120;
        double from = args.Length > 4 && double.TryParse(args[4], NumberStyles.Float, CultureInfo.InvariantCulture, out double f) ? f : 0;

        using var screen = VirtualProFlow.Open();
        if (screen is null) return 1;
        screen.Skip(from);

        var white = AsrockGpuLed.SavedColor() ?? Color.White;
        var engine = new LedEffectEngine(settings);
        engine.Reset(white, white);
        // GPU de mentira (parada, trabalhando de 30% a 75% do tempo, parada de novo) e alguém digitando em rajadas.
        long total = seconds * 1000L;
        double Busy(long t) => Choreo.Smooth(0.3, 0.42, t / (double)total) * (1 - Choreo.Smooth(0.75, 0.8, t / (double)total));
        WhiteMotion.SimulatedInput = t =>
        {
            double p = t / (double)total;
            bool typing = p is > 0.2 and < 0.35 || p is > 0.55 and < 0.6;
            return typing ? (uint)(t / 180 * 180) : (uint)(total * (p < 0.35 ? 0.2 : p < 0.6 ? 0.35 : 0.6));
        };
        const int LedStepMs = 40; // o mesmo ritmo da thread do LED
        var samples = new List<LedSample>();
        var marks = new List<PanelMark>();
        ScreenLight? light = null;
        PanelScene? previous = null;
        long frameAt = 0;
        for (long t = 0; t <= seconds * 1000L; t += LedStepMs)
        {
            for (; frameAt <= t; frameAt += PanelSlideshow.FrameMs)
            {
                light = screen.Next(frameAt);
                if (light.Panel is PanelScene scene && (previous is not PanelScene last || last.Panel != scene.Panel || last.Turning != scene.Turning))
                    marks.Add(new PanelMark(frameAt, scene, scene.Turning ? null : screen.Thumbnail(96, 48)));
                previous = light.Panel;
            }
            double busy = Busy(t);
            var color = engine.Next(t, (float)(42 + 38 * busy), (float)(97 * busy), light, out _);
            samples.Add(new LedSample(t, color, engine.LastLevel, light?.Luminance ?? float.NaN));
        }

        try
        {
            DrawLedChart(output, $"{settings.Efeito} — {seconds} s a partir de {from:0} s do tema One Piece", seconds, samples, marks);
        }
        finally
        {
            foreach (var mark in marks) mark.Thumbnail?.Dispose();
        }
        WhiteMotion.SimulatedInput = null;
        Log.Info($"Simulação do LED ({settings.Efeito}) salva em {output}; a trava contra clarões segurou a luz {engine.TakeFlashHolds()} vezes");
        return 0;
    }

    private readonly record struct LedSample(long At, Color Color, double Level, float Luminance);

    private sealed record PanelMark(long At, PanelScene Scene, Bitmap? Thumbnail);

    private static void DrawLedChart(string output, string title, int seconds, List<LedSample> samples, List<PanelMark> marks)
    {
        const int PxPerSecond = 20, Left = 36, Right = 110, Top = 28, ThumbHeight = 48, StripHeight = 34, PlotHeight = 160, Gap = 8;
        int plotWidth = seconds * PxPerSecond;
        int yThumb = Top, yStrip = yThumb + ThumbHeight + Gap, yPlot = yStrip + StripHeight + Gap, yLabels = yPlot + PlotHeight + 4;
        using var chart = new Bitmap(Left + plotWidth + Right, yLabels + 44, PixelFormat.Format24bppRgb);
        using var g = Graphics.FromImage(chart);
        g.Clear(Color.FromArgb(24, 24, 28));
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        using var font = new Font("Segoe UI", 11, GraphicsUnit.Pixel);
        using var bold = new Font("Segoe UI", 13, FontStyle.Bold, GraphicsUnit.Pixel);
        using var grid = new Pen(Color.FromArgb(60, 60, 68));
        using var panelPen = new Pen(Color.FromArgb(110, 110, 120)) { DashStyle = DashStyle.Dash };
        using var levelPen = new Pen(Color.White, 2);
        using var inkPen = new Pen(Color.FromArgb(120, 170, 255), 1.5f);
        using var dim = new SolidBrush(Color.FromArgb(170, 170, 180));
        float X(long at) => Left + at / 1000f * PxPerSecond;
        float Y(double v) => yPlot + PlotHeight - (float)(Math.Clamp(v, 0, 1) * PlotHeight);

        g.DrawString(title, bold, Brushes.White, Left, 6);
        foreach (var mark in marks)
            if (mark.Thumbnail is not null) g.DrawImage(mark.Thumbnail, X(mark.At), yThumb, 96, 48);

        // Cor que o LED teria, quadro a quadro.
        for (int i = 0; i < samples.Count; i++)
        {
            float x0 = X(samples[i].At), x1 = i + 1 < samples.Count ? X(samples[i + 1].At) : x0 + 1;
            using var brush = new SolidBrush(samples[i].Color);
            g.FillRectangle(brush, x0, yStrip, Math.Max(1, x1 - x0 + 0.6f), StripHeight);
        }

        g.DrawRectangle(grid, Left, yPlot, plotWidth, PlotHeight);
        foreach (double v in new[] { 0.25, 0.5, 0.75 }) g.DrawLine(grid, Left, Y(v), Left + plotWidth, Y(v));
        for (int sec = 0; sec <= seconds; sec += 5)
        {
            g.DrawLine(grid, X(sec * 1000L), yPlot + PlotHeight, X(sec * 1000L), yPlot + PlotHeight + 4);
            g.DrawString($"{sec}s", font, dim, X(sec * 1000L) - 8, yLabels + 26);
        }
        foreach (var mark in marks)
        {
            float x = X(mark.At);
            if (mark.Scene.Turning)
            {
                g.DrawString(mark.Scene.Turn == PanelTurn.Cut ? "corte" : "retícula", font, dim, x, yPlot + 2);
                continue;
            }
            g.DrawLine(panelPen, x, yThumb, x, yPlot + PlotHeight);
            string cue = mark.Scene.Cue == PanelCue.None ? "" : " " + mark.Scene.Cue.ToString().ToLowerInvariant();
            g.DrawString($"painel {mark.Scene.Panel + 1}{cue}", font, Brushes.White, x + 2, yLabels + 6);
        }

        void Line(Pen pen, Func<LedSample, double> value)
        {
            var points = new List<PointF>();
            foreach (var sample in samples)
            {
                double v = value(sample);
                if (double.IsNaN(v))
                {
                    if (points.Count > 1) g.DrawLines(pen, points.ToArray());
                    points.Clear();
                    continue;
                }
                points.Add(new PointF(X(sample.At), Y(v)));
            }
            if (points.Count > 1) g.DrawLines(pen, points.ToArray());
        }
        Line(inkPen, sample => sample.Luminance);
        Line(levelPen, sample => sample.Level);
        g.DrawString("— luz (olho)", font, Brushes.White, Left + plotWidth + 8, yPlot + 4);
        using var inkBrush = new SolidBrush(inkPen.Color);
        g.DrawString("— brilho da tela", font, inkBrush, Left + plotWidth + 8, yPlot + 20);
        chart.Save(output, ImageFormat.Png);
    }

    /// <summary>Pro Flow de mentira: o tema One Piece desenhado quadro a quadro na memória (simulação e teste do LED).</summary>
    private sealed class VirtualProFlow : IDisposable
    {
        private readonly Theme _theme;
        private readonly ThemeRenderer _renderer;
        private readonly PanelSlideshow _slideshow;

        private VirtualProFlow(Theme theme, PanelSlideshow slideshow)
        {
            _theme = theme;
            _slideshow = slideshow;
            _renderer = new ThemeRenderer(theme);
        }

        public static VirtualProFlow? Open()
        {
            var theme = ThemeCatalog.Load(ScreenKind.ProFlow, OnePieceTheme.Name, new AppConfig());
            if (theme.Animation is PanelSlideshow slideshow) return new VirtualProFlow(theme, slideshow);
            Log.Warn("Simulação do LED: o tema One Piece não tem painéis.");
            theme.Dispose();
            return null;
        }

        public void Skip(double seconds)
        {
            for (int i = (int)(seconds * 1000 / PanelSlideshow.FrameMs); i > 0; i--) _renderer.AdvanceFrame();
        }

        /// <summary>Desenha o quadro atual, mede e passa para o próximo.</summary>
        public ScreenLight Next(long at)
        {
            _renderer.Render(null, force: true, refreshValues: false);
            var light = new ScreenLight(at, ScreenLight.Measure(_renderer.Canvas), _slideshow.Describe(_renderer.FrameIndex));
            _renderer.AdvanceFrame();
            return light;
        }

        /// <summary>Miniatura do último quadro desenhado.</summary>
        public Bitmap Thumbnail(int width, int height)
        {
            var thumbnail = new Bitmap(width, height);
            using var g = Graphics.FromImage(thumbnail);
            g.InterpolationMode = InterpolationMode.HighQualityBilinear;
            g.DrawImage(_renderer.Canvas, 0, 0, width, height);
            return thumbnail;
        }

        public void Dispose()
        {
            _renderer.Dispose();
            _theme.Dispose();
        }
    }

    /// <summary>
    /// <c>--thermalright-teste [segundos] [tema]</c>: liga só a tela Thermalright, com o mesmo caminho do programa (tema,
    /// sensores, JPEG, envio), por alguns segundos, e registra no log quantos quadros foram e quanto cada envio levou.
    /// O último quadro fica na tela.
    /// </summary>
    public static int ThermalrightTest(string[] args)
    {
        int seconds = args.Length > 1 && int.TryParse(args[1], out int s) ? Math.Clamp(s, 3, 600) : 15;
        var config = new AppConfig(); // só em memória
        config.Thermalright.Tema = args.Length > 2 ? args[2] : MareTheme.Name;

        using var screen = new ThermalrightScreen(config);
        using var sensors = new SensorHub();
        long end = Clock.Ms + seconds * 1000L, nextSensors = 0;
        bool connected = false;
        while (Clock.Ms < end)
        {
            screen.Pump();
            if (!screen.IsReady)
            {
                if (connected) break; // caiu no meio
                Thread.Sleep(100);
                continue;
            }
            if (!connected)
            {
                connected = true;
                end = Clock.Ms + seconds * 1000L; // conta a partir da conexão
            }
            if (screen.EnsureTheme()) sensors.Configure(screen.SensorKeys, config.Gpu);
            bool fresh = Clock.Ms >= nextSensors;
            if (fresh)
            {
                sensors.Update();
                nextSensors = Clock.Ms + 1000;
            }
            screen.Tick(sensors, fresh);
            Thread.Sleep(Math.Max(1, (int)Math.Min(screen.NextWakeDue, nextSensors) - (int)Clock.Ms));
        }
        if (!connected)
        {
            Log.Warn("Thermalright (teste): a tela não conectou (veja acima).");
            return 1;
        }
        Log.Info($"Thermalright (teste): {screen.TakeStats()}");
        return 0;
    }

    /// <summary><c>--converter-video entrada [saida.zt]</c>: converte um vídeo para o tema Vídeo da Thermalright (sem a tela).</summary>
    public static int ConvertVideo(string[] args)
    {
        if (args.Length < 2)
        {
            Log.Warn("Uso: --converter-video entrada.mp4 [saida.zt]");
            return 1;
        }
        string output = args.Length > 2 ? args[2] : Path.ChangeExtension(Path.GetFullPath(args[1]), VideoTheme.Extension);
        var timer = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            int frames = VideoImport.Convert(args[1], output, Display.ThermalrightDisplay.Width, Display.ThermalrightDisplay.Height);
            Log.Info($"Vídeo convertido: {output}, {frames} quadros em {timer.Elapsed.TotalSeconds:0.0} s ({new FileInfo(output).Length / 1048576.0:0.0} MB)");
            return 0;
        }
        catch (IOException ex)
        {
            Log.Warn($"Não consegui converter {args[1]}: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// <c>--grand-line-previa [saida.png] [cena]</c>: desenha o tema Grand Line da Thermalright em várias situações (dia,
    /// pôr do sol, noite, tempestade e cada momento) numa folha só — ou só a cena pedida, em tamanho real.
    /// </summary>
    public static int GrandLinePreview(string[] args)
    {
        string output = Path.GetFullPath(args.Length > 1 ? args[1] : Path.Combine(AppContext.BaseDirectory, "grand-line.png"));
        string? only = args.Length > 2 ? args[2] : null;
        const long Now = 600_000;
        StageState Base(double daylight = 1, double warmth = 0, double arc = 0.45) =>
            new() { At = Now, Daylight = daylight, Warmth = warmth, SunArc = arc, Audio = 0.1 };
        PanelScene Cue(PanelCue cue, double s, double d = 9) => new(3, false, PanelTurn.Cut, s, d, cue);
        StageEvent Ev(StageEventKind kind, double ago, int variant = 0, string? text = null) => new(kind, Now - (long)(ago * 1000), variant, text);

        var scenes = new (string Name, StageState State)[]
        {
            ("Dia", Base()),
            ("Pôr do sol", Base(0.75, 1, 0.96)),
            ("Noite", Base(0, 0.5)),
            ("Tempestade + raio", Base() with { Storm = true, Events = new[] { Ev(StageEventKind.Lightning, 0.02, 2) } }),
            ("Virada: DON!!", Base() with { Events = new[] { Ev(StageEventKind.Turn, 0.3, (int)PanelTurn.Cut) } }),
            ("Gear 5: Tambores", Base(0.2) with { Panel = Cue(PanelCue.Drums, 1.35) }),
            ("Haki: céu partido", Base() with { Panel = Cue(PanelCue.Haki, 1.15) }),
            ("Gura Gura", Base() with { Panel = Cue(PanelCue.Quake, 2.4) }),
            ("Impacto", Base() with { Panel = Cue(PanelCue.Impact, 0.3) }),
            ("Den Den Mushi", Base() with { Events = new[] { Ev(StageEventKind.DenDenMushi, 3.2, 0, "Download concluído — 4.2 GB") } }),
            ("Coup de Burst", Base() with { Events = new[] { Ev(StageEventKind.CoupDeBurst, Choreo.BurstCharge + 0.35) } }),
            ("Buster Call", Base(0.6, 0.6, 0.85) with { Events = new[] { Ev(StageEventKind.BusterCall, 0.85, 0, "GPU 88°C") } }),
            ("Dormindo", Base(1) with { Sleeping = true, IdleSeconds = 900 }),
            ("Bem-vindo", Base() with { Events = new[] { Ev(StageEventKind.Welcome, 0.55) } }),
            ("Som alto + batida", Base() with { Audio = 0.9, LastBeat = Now - 150 }),
        };
        if (only == "nuvens")
        {
            using var check = new Bitmap(1600, 200);
            using (var cg = Graphics.FromImage(check))
            {
                for (int y = 0; y < 200; y += 20) for (int x = 0; x < 1600; x += 20) cg.FillRectangle((x + y) / 20 % 2 == 0 ? Brushes.HotPink : Brushes.LightGreen, x, y, 20, 20);
                for (int i = 0; i < 4; i++) using (var cloud = MangaArt.Cloud(i, false)) cg.DrawImage(cloud, i * 400, 10);
            }
            check.Save(output, ImageFormat.Png);
            return 0;
        }
        if (only is not null) scenes = scenes.Where(s => s.Name.StartsWith(only, StringComparison.OrdinalIgnoreCase)).ToArray();
        if (scenes.Length == 0)
        {
            Log.Warn("Cena desconhecida.");
            return 1;
        }

        const int W = 1920, H = 462;
        using var animation = new GrandLineAnimation(W, H);
        using var frame = new Bitmap(W, H, PixelFormat.Format32bppPArgb);
        using var fg = Graphics.FromImage(frame);
        var timer = System.Diagnostics.Stopwatch.StartNew();
        int draws = 0;
        void Render(StageState state)
        {
            fg.Clear(Color.White);
            animation.Draw(fg, Now, state);
            GrandLineTheme.Caption(fg, new RectangleF(18, 14, 372, 62));
            GrandLineTheme.Caption(fg, new RectangleF(W - 18 - 214, 14, 214, 62));
            using var font = new Font("Bahnschrift SemiBold", 34, GraphicsUnit.Pixel);
            fg.DrawString("CPU 52°   GPU 41°", font, Brushes.Black, 36, 24);
            fg.DrawString("18:12", font, Brushes.Black, W - 160, 24);
            draws++;
        }

        if (scenes.Length == 1)
        {
            Render(scenes[0].State);
            frame.Save(output, ImageFormat.Png);
        }
        else
        {
            const int Cols = 2, CellW = W / 2, CellH = H / 2, Label = 26;
            int rows = (scenes.Length + Cols - 1) / Cols;
            using var sheet = new Bitmap(Cols * CellW + 10, rows * (CellH + Label) + 6, PixelFormat.Format24bppRgb);
            using var sg = Graphics.FromImage(sheet);
            sg.Clear(Color.FromArgb(30, 30, 36));
            sg.InterpolationMode = InterpolationMode.HighQualityBilinear;
            using var labelFont = new Font("Segoe UI", 15, FontStyle.Bold, GraphicsUnit.Pixel);
            for (int i = 0; i < scenes.Length; i++)
            {
                Render(scenes[i].State);
                int x = 4 + i % Cols * (CellW + 2), y = 4 + i / Cols * (CellH + Label);
                sg.DrawString(scenes[i].Name, labelFont, Brushes.White, x, y + 2);
                sg.DrawImage(frame, x, y + Label - 2, CellW, CellH);
            }
            sheet.Save(output, ImageFormat.Png);
        }

        // Custo por quadro (o pré-desenho do céu fica fora: ele só muda quando a luz muda).
        animation.Draw(fg, Now, scenes[0].State);
        var cost = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < 40; i++) animation.Draw(fg, Now + i * 50, scenes[0].State);
        Log.Info($"Grand Line: prévia em {output} ({draws} cenas, {timer.ElapsedMilliseconds} ms); {cost.Elapsed.TotalMilliseconds / 40:0.00} ms por quadro na cena '{scenes[0].Name}'");
        return 0;
    }

    /// <summary>
    /// <c>--sinais [segundos]</c>: liga os ouvintes de ações do sistema (copiar, print, pendrive, volume, microfone,
    /// webcam, app em foco, monitor, downloads, Spotify) e registra no log tudo o que perceber. Pode rodar com o
    /// programa aberto.
    /// </summary>
    public static int WatchSignals(string[] args)
    {
        int seconds = args.Length > 1 && int.TryParse(args[1], out int s) ? Math.Clamp(s, 5, 3600) : 60;
        SystemSignals.Logging = true;
        using var listener = new SignalListener();
        using var director = new SceneDirector();
        using var timer = new System.Windows.Forms.Timer { Interval = 50 };
        long end = Clock.Ms + seconds * 1000L;
        var context = new ApplicationContext();
        timer.Tick += (_, _) =>
        {
            director.Update(null);
            if (Clock.Ms >= end) context.ExitThread();
        };
        Log.Info($"Sinais: ouvindo por {seconds} s.");
        timer.Start();
        Application.Run(context);
        var stage = SceneDirector.Current;
        Log.Info($"Sinais: fim. Volume {stage.Volume:0.00}{(stage.Muted ? " (mudo)" : "")}, no ar: {stage.OnAir}.");
        return 0;
    }

    /// <summary>
    /// <c>--mundo-previa história [saida.png] [quadros] [segundos]</c>: roda uma história do Modo Grand Line sem tocar no
    /// hardware e desenha uma folha com vários instantes dela — em cada um, a Thermalright em cima, a Pro Flow embaixo
    /// (na posição do gabinete) e a luz da placa de vídeo, como se vê pelo vidro.
    /// </summary>
    public static int WorldPreview(string[] args)
    {
        string story = args.Length > 1 ? args[1] : "anchor";
        string output = Path.GetFullPath(args.Length > 2 ? args[2] : Path.Combine(AppContext.BaseDirectory, $"mundo-{story}.png"));
        int frames = args.Length > 3 && int.TryParse(args[3], out int f) ? Math.Clamp(f, 1, 16) : 8;
        double seconds = args.Length > 4 && double.TryParse(args[4], NumberStyles.Float, CultureInfo.InvariantCulture, out double s) ? s : 8;
        bool night = args.Contains("noite");

        var layout = new World.WorldLayout();
        var director = new World.WorldDirector();
        director.Configure(layout);
        long start = Clock.Ms;
        var stage = new StageState { At = start, Daylight = night ? 0 : 1, SunArc = 0.4, Audio = 0.25, GpuTemp = 55, Warmth = night ? 0.5 : 0 };
        director.Trigger(story, start);
        var world = World.WorldDirector.Current;
        if (story == "anchor") world = world with { Ship = World.ShipMode.Anchored };

        using var top = new GrandLineAnimation(1920, 462);
        using var middle = new DeepSeaAnimation(960, 480);
        using var topFrame = new Bitmap(1920, 462, PixelFormat.Format32bppPArgb);
        using var middleFrame = new Bitmap(960, 480, PixelFormat.Format32bppPArgb);
        using var tg = Graphics.FromImage(topFrame);
        using var mg = Graphics.FromImage(middleFrame);

        const int CellW = 980, TopH = 231, Gap = 46, Label = 24;
        float pxPerMm = 960f / layout.Top.Width;
        int midW = (int)(layout.Middle.Width * pxPerMm), midH = (int)(layout.Middle.Height * pxPerMm);
        int midX = 10 + (int)((layout.Middle.X - layout.Top.X) * pxPerMm);
        int cellH = Label + TopH + Gap + midH + Gap + 70;
        const int Cols = 2;
        int rows = (frames + Cols - 1) / Cols;
        using var sheet = new Bitmap(Cols * CellW, rows * cellH, PixelFormat.Format24bppRgb);
        using var sg = Graphics.FromImage(sheet);
        sg.Clear(Color.FromArgb(232, 232, 236)); // o gabinete branco
        sg.InterpolationMode = InterpolationMode.HighQualityBilinear;
        sg.SmoothingMode = SmoothingMode.AntiAlias;
        using var font = new Font("Segoe UI", 15, FontStyle.Bold, GraphicsUnit.Pixel);
        var cost = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < frames; i++)
        {
            long at = start + (long)(seconds * 1000 * i / Math.Max(1, frames - 1));
            var st = stage with { At = at };
            tg.Clear(Color.White);
            top.Draw(tg, at, st, world);
            mg.Clear(Color.Black);
            middle.Draw(mg, at, st, world);
            var (level, warmth) = World.WorldLight.At(at, st, world);

            int x = (i % Cols) * CellW, y = (i / Cols) * cellH;
            sg.DrawString($"{(at - start) / 1000.0:0.0} s", font, Brushes.Black, x + 10, y + 3);
            sg.DrawImage(topFrame, x + 10, y + Label, 960, TopH);
            sg.DrawRectangle(Pens.Black, x + 10, y + Label, 960, TopH);
            int my = y + Label + TopH + Gap;
            sg.DrawImage(middleFrame, x + midX, my, midW, midH);
            sg.DrawRectangle(Pens.Black, x + midX, my, midW, midH);
            // A luz da placa: um retângulo com a cor que o LED teria (o branco 255,210,255 do setup).
            double lin = Math.Pow(level, 2.2);
            var white = warmth > 0 ? Color.FromArgb(255, (int)(210 - 50 * warmth), (int)(255 - 130 * warmth)) : Color.FromArgb((int)(255 + 50 * warmth), (int)(210 + 15 * -warmth), 255);
            var led = Color.FromArgb((int)(white.R * lin), (int)(white.G * lin), (int)(white.B * lin));
            int ly = my + midH + Gap;
            using (var glow = new SolidBrush(led)) sg.FillRectangle(glow, x + 360, ly, 260, 26);
            sg.DrawRectangle(Pens.Black, x + 360, ly, 260, 26);
            sg.DrawString($"LED {level:0.00}  tom {warmth:+0.00;-0.00}", font, Brushes.Black, x + 630, ly + 3);
        }
        sheet.Save(output, ImageFormat.Png);
        Log.Info($"Mundo: '{story}' em {output} ({frames} quadros em {cost.ElapsedMilliseconds} ms)");
        return 0;
    }

    /// <summary>
    /// <c>--manga-previa efeito [saida.png] [quadros] [segundos] [início] [config.json]</c>: encena um efeito do Modo Mangá
    /// sem tocar no hardware, por cima do vídeo da Thermalright e dos painéis do One Piece de verdade, e desenha vários
    /// instantes (de [início] a [início]+[segundos]) como se vê pelo vidro: a Thermalright, a Pro Flow e a luz da placa.
    /// </summary>
    public static int MangaPreview(string[] args)
    {
        string name = args.Length > 1 ? args[1] : "impacto";
        string output = Path.GetFullPath(args.Length > 2 ? args[2] : Path.Combine(AppContext.BaseDirectory, $"manga-{name}.png"));
        int frames = args.Length > 3 && int.TryParse(args[3], out int f) ? Math.Clamp(f, 1, 16) : 8;
        double seconds = args.Length > 4 && double.TryParse(args[4], NumberStyles.Float, CultureInfo.InvariantCulture, out double s) ? s : 2;
        double from = args.Length > 5 && double.TryParse(args[5], NumberStyles.Float, CultureInfo.InvariantCulture, out double b) ? b : 0;
        string configPath = args.Length > 6 ? args[6] : Path.Combine(AppContext.BaseDirectory, "config.json");
        var config = File.Exists(configPath) ? AppConfig.Load(configPath) : new AppConfig();
        if (string.IsNullOrEmpty(config.Manga.GifGear5))
            config.Manga.GifGear5 = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(configPath))!, "media", "nika-luffy.gif");

        string topName = config.Thermalright.Tema is VideoTheme.Name or GifTheme.Name or OnePieceTheme.Name ? config.Thermalright.Tema : VideoTheme.Name;
        using var topTheme = ThemeCatalog.Load(ScreenKind.Thermalright, topName, config);
        using var middleTheme = ThemeCatalog.Load(ScreenKind.ProFlow, OnePieceTheme.Name, config);
        using var top = new ThemeRenderer(topTheme);
        using var middle = new ThemeRenderer(middleTheme);
        Manga.MangaLayer.Preload(config.Manga.GifGear5);
        Manga.MangaLayer.WaitForGif(15000);

        // A Pro Flow mostra um painel que combina com o efeito (o do impacto, o do Haki...).
        var slideshow = middleTheme.Animation as PanelSlideshow;
        var cue = name switch
        {
            "impacto" or "impact" => PanelCue.Impact,
            "haki" => PanelCue.Haki,
            "tambores" or "drums" => PanelCue.Drums,
            "tremor" or "quake" => PanelCue.Quake,
            "mare" or "tide" => PanelCue.Tide,
            "despedida" or "farewell" => PanelCue.Farewell,
            "amanhecer" or "dawn" => PanelCue.Dawn,
            _ => PanelCue.None,
        };
        int panelFrame = (cue != PanelCue.None ? slideshow?.FirstFrameOf(cue) : null) ?? slideshow?.PanOf(4, 0.3) ?? 0;

        var director = new Manga.MangaStage { Settings = config.Manga };
        Manga.MangaStage.InUse = true;
        long start = Clock.Ms;
        director.Trigger(name, start);
        if (name == "print")
        {
            for (int i = 0; i < 40 && Manga.MangaStage.Current.Find(Manga.MangaCueKind.Print, start)?.Picture is null; i++) Thread.Sleep(100);
        }
        using var topLayer = new Manga.MangaLayer(ScreenKind.Thermalright);
        using var middleLayer = new Manga.MangaLayer(ScreenKind.ProFlow);

        const int CellW = 980, TopH = 231, Label = 24, Gap = 34, MidW = 480, MidH = 240;
        int cellH = Label + TopH + Gap + MidH + Gap + 40;
        const int Cols = 2;
        int rows = (frames + Cols - 1) / Cols;
        using var sheet = new Bitmap(Cols * CellW, rows * cellH, PixelFormat.Format24bppRgb);
        using var sg = Graphics.FromImage(sheet);
        sg.Clear(Color.FromArgb(232, 232, 236)); // o gabinete branco
        sg.InterpolationMode = InterpolationMode.HighQualityBilinear;
        using var font = new Font("Segoe UI", 15, FontStyle.Bold, GraphicsUnit.Pixel);
        var cost = System.Diagnostics.Stopwatch.StartNew();
        int midOffset = 0;
        for (int i = 0; i < frames; i++)
        {
            long at = start + (long)((from + seconds * i / Math.Max(1, frames - 1)) * 1000);
            if (director.TakeSeek(at) && slideshow?.FirstFrameOf(PanelCue.Drums) is int drums)
            {
                panelFrame = drums;
                midOffset = (int)((at - start) / PanelSlideshow.FrameMs);
            }
            var state = Manga.MangaStage.Current;
            top.Seek((int)((at - start) / 40));
            top.Render(null, force: true, refreshValues: false);
            topLayer.Apply(top.Canvas, at, state);
            middle.Seek(panelFrame + (int)((at - start) / PanelSlideshow.FrameMs) - midOffset);
            middle.Render(null, force: true, refreshValues: false);
            float ink = Lighting.ScreenLight.Measure(middle.Canvas);
            middleLayer.Apply(middle.Canvas, at, state);
            double level = Manga.MangaStage.ShapeLed(0.38 + 0.47 * Choreo.Smooth(0.30, 0.85, ink), at);

            int x = (i % Cols) * CellW, y = (i / Cols) * cellH;
            sg.DrawString($"{(at - start) / 1000.0:0.00} s", font, Brushes.Black, x + 10, y + 3);
            sg.DrawImage(top.Canvas, x + 10, y + Label, 960, TopH);
            sg.DrawRectangle(Pens.Black, x + 10, y + Label, 960, TopH);
            int my = y + Label + TopH + Gap;
            sg.DrawImage(middle.Canvas, x + 10 + (960 - MidW) / 2, my, MidW, MidH);
            sg.DrawRectangle(Pens.Black, x + 10 + (960 - MidW) / 2, my, MidW, MidH);
            double lin = Math.Pow(level, 2.2);
            using (var glow = new SolidBrush(Color.FromArgb((int)(255 * lin), (int)(210 * lin), (int)(255 * lin))))
                sg.FillRectangle(glow, x + 360, my + MidH + Gap, 260, 22);
            sg.DrawRectangle(Pens.Black, x + 360, my + MidH + Gap, 260, 22);
            sg.DrawString($"LED {level:0.00}", font, Brushes.Black, x + 630, my + MidH + Gap + 2);
        }
        Manga.MangaStage.InUse = false;
        sheet.Save(output, ImageFormat.Png);
        Log.Info($"Modo Mangá: '{name}' em {output} ({frames} quadros em {cost.ElapsedMilliseconds} ms)");
        return 0;
    }

    /// <summary><c>--diagnostico</c>: lista temas, fontes e todos os sensores de CPU/GPU no log.</summary>
    public static int Diagnose()
    {
        Log.Info($"=== Diagnóstico — admin: {Environment.IsPrivilegedProcess}, CPU: {SystemInfo.CpuName} ===");

        foreach (var kind in new[] { ScreenKind.Ds339, ScreenKind.ProFlow })
        {
            foreach (var info in ThemeCatalog.Discover(kind))
            {
                Log.Info($"Tema {kind}: {info.Name} ({info.Directory ?? "embutido"})");
                if (info.IsBuiltIn) continue;
                using var theme = ThemeCatalog.Load(kind, info.Name, new AppConfig());
                foreach (var family in theme.Fonts?.Families ?? Array.Empty<System.Drawing.FontFamily>())
                    Log.Info($"    fonte: '{family.Name}' (zh-CN: '{family.GetName(0x0804)}')");
                Log.Info($"    dados: {string.Join(", ", theme.SensorKeys)}");
            }
        }

        using var hardware = HardwareMonitor.TryCreate(cpu: true, gpu: true, board: false, gpuHint: null);
        if (hardware is not null)
        {
            hardware.Update(new Readings());
            Thread.Sleep(1000);
            hardware.LogAllSensors();
            hardware.LogUpdateCost();
        }
        return 0;
    }
}
