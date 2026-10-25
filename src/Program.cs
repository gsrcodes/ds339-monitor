using System.Globalization;
using DS339Monitor.App;
using DS339Monitor.Render;
using DS339Monitor.Sensors;

namespace DS339Monitor;

internal static class Program
{
    private const string InstanceName = @"Local\DS339Monitor";
    public const string WaitFlag = "--aguardar";

    /// <summary>Arquivo-sinal de <c>--sair</c>: a instância aberta vê, apaga e fecha.</summary>
    public static string ExitFlagPath => Path.Combine(AppContext.BaseDirectory, "sair.flag");

    /// <summary>Arquivo-sinal de <c>--painel</c> (e de abrir o programa de novo): a instância aberta mostra o painel.</summary>
    public static string PanelFlagPath => Path.Combine(AppContext.BaseDirectory, "painel.flag");

    private static Mutex? _instance;

    [STAThread]
    private static int Main(string[] args)
    {
        Log.Init(AppContext.BaseDirectory);
        string command = args.Length > 0 ? args[0].ToLowerInvariant() : "";

        // Comandos que não tocam na tela (podem rodar com o programa aberto).
        switch (command)
        {
            case "--autostart":
                return Autostart.Set(args.Length > 1 && args[1].Equals("on", StringComparison.OrdinalIgnoreCase)) ? 0 : 1;
            case "--diagnostico":
                return Tools.Diagnose();
            case "--renderizar":
                return Tools.RenderToFile(args);
            case "--capturar":
                return Tools.RequestCapture();
            case "--sair":
                return RequestExit();
            case "--rgb-info":
                Log.Info($"Nollie: {Argb.NollieDevice.Describe()}");
                return 0;
            case "--placa-mae":
                if (Environment.IsPrivilegedProcess)
                {
                    Sensors.BoardFans.LogAll();
                    return 0;
                }
                File.WriteAllText(App.MonitorService.BoardFlagPath, "");
                return 0;
            case "--jogo-previa":
            {
                // O painel de jogo com a partida de mentira (ou sem dados, com "vazio"), sem tocar em nada.
                string output = args.Length > 1 ? args[1] : Path.Combine(AppContext.BaseDirectory, "jogo-previa.png");
                if (!args.Contains("vazio")) Game.GameTelemetry.StartDemo(10);
                if (args.Length > 2) Game.Cs2Events.Stage(args[2]); // vitoria, derrota ou plantada (a faixa)
                using var hud = new Game.GameHud(1920, 462);
                using var canvas = new System.Drawing.Bitmap(1920, 462);
                using (var g = System.Drawing.Graphics.FromImage(canvas)) hud.DrawFrame(canvas, g, 0);
                canvas.Save(output, System.Drawing.Imaging.ImageFormat.Png);
                Log.Info($"Modo jogo: prévia do painel salva em {output}");
                return 0;
            }
            case "--nollie-fseq-teste":
                return Argb.NollieTools.FseqSelfTest(args);
            case "--nollie-conferir-gravacao":
                return Argb.NollieTools.CheckWrite();
            case "--nollie-ler":
                return Argb.NollieTools.ReadConfig();
            case "--fitas-previa":
                return Argb.ArgbPreview.Run(args);
            case "--fitas":
                File.WriteAllText(App.MonitorService.ArgbFlagPath, string.Join(' ', args.Skip(1)));
                return 0;
            case "--led-gpu":
                return Tools.GpuLedDiagnose();
            case "--led-gpu-simular":
                return Tools.GpuLedSimulate(args);
            case "--sinais":
                return Tools.WatchSignals(args);
            case "--historia":
                File.WriteAllText(App.MonitorService.StoryFlagPath, args.Length > 1 ? args[1] : "news");
                return 0;
            case "--desligar-tudo":
                File.WriteAllText(App.MonitorService.AllOffFlagPath, "desligar");
                return 0;
            case "--ligar-tudo":
                File.WriteAllText(App.MonitorService.AllOffFlagPath, "ligar");
                return 0;
            case "--alternar-tudo":
                File.WriteAllText(App.MonitorService.AllOffFlagPath, "alternar");
                return 0;
            case "--painel":
                File.WriteAllText(PanelFlagPath, "abrir");
                return 0;
            case "--abrir":
                return OpenApp();
            case "--criar-atalhos":
                return CreateShortcuts();
            case "--painel-imagem":
                File.WriteAllText(PanelFlagPath, "imagem:" + (args.Length > 1 ? args[1] : "overview"));
                return 0;
            case "--mundo-previa":
                return Tools.WorldPreview(args);
            case "--manga-previa":
                return Tools.MangaPreview(args);
            case "--grand-line-previa":
                return Tools.GrandLinePreview(args);
            case "--converter-video":
                return Tools.ConvertVideo(args);
            case "--thermalright":
                Log.Info($"Thermalright: {Display.ThermalrightDisplay.Identify()}");
                return 0;
            case "--baixar-one-piece":
                TrayApp.OpenFile(Themes.OnePieceTheme.WriteDownloadPage());
                return 0;
        }

        if (!AcquireInstance(wait: args.Contains(WaitFlag)))
        {
            // Abrir o programa de novo (pelo atalho, por exemplo) mostra o painel da instância que já está rodando.
            if (command.Length == 0)
            {
                try
                {
                    File.WriteAllText(PanelFlagPath, "abrir");
                }
                catch (IOException)
                {
                }
            }
            Log.Info("Outra instância já está usando a tela; saindo.");
            return 0;
        }

        try
        {
            if (command == "--led-gpu-teste") return Tools.GpuLedTest(args);
            if (command == "--thermalright-teste") return Tools.ThermalrightTest(args);
            if (command == "--teste")
            {
                int seconds = args.Length > 1 && int.TryParse(args[1], out var s) ? s : 20;
                int rotation = args.Length > 2 && int.TryParse(args[2], out var r) ? r : 90;
                return TestPattern.Run(seconds, rotation);
            }

            var config = AppConfig.Load(Path.Combine(AppContext.BaseDirectory, "config.json"));
            ApplyCulture(config.Idioma);
            Log.Info($"DS339 Monitor iniciado (admin: {Environment.IsPrivilegedProcess}).");

            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var service = new MonitorService(config);
            service.Start();
            using (var tray = new TrayApp(config, service))
                Application.Run(tray);
            service.Stop(); // idempotente: garante a tela apagada e o SDK parado em qualquer saída
            Log.Info("Encerrado.");
            return 0;
        }
        finally
        {
            _instance?.ReleaseMutex();
            _instance?.Dispose();
        }
    }

    /// <summary>
    /// O que o atalho faz: pede o painel e, se o programa não estiver aberto, abre pela tarefa agendada (como
    /// administrador, sem a pergunta do UAC) — ou, sem a tarefa, por esta mesma instância.
    /// </summary>
    private static int OpenApp()
    {
        File.WriteAllText(PanelFlagPath, "abrir");
        if (System.Diagnostics.Process.GetProcessesByName("DS339Monitor").Length > 1) return 0;
        if (Autostart.IsEnabled() && Autostart.RunNow()) return 0;
        return Main(Array.Empty<string>());
    }

    /// <summary>Atalhos "DS339 Monitor" na área de trabalho e no menu Iniciar, que abrem o painel.</summary>
    private static int CreateShortcuts()
    {
        var shellType = Type.GetTypeFromProgID("WScript.Shell");
        if (shellType is null) return 1;
        dynamic shell = Activator.CreateInstance(shellType)!;
        string[] folders =
        {
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
            Environment.GetFolderPath(Environment.SpecialFolder.Programs),
        };
        foreach (string folder in folders)
        {
            dynamic link = shell.CreateShortcut(Path.Combine(folder, "DS339 Monitor.lnk"));
            link.TargetPath = Environment.ProcessPath;
            link.Arguments = "--abrir";
            link.WorkingDirectory = AppContext.BaseDirectory;
            link.IconLocation = (File.Exists(AppIcon.FilePath) ? AppIcon.FilePath : Environment.ProcessPath) + ",0";
            link.Description = "Painel das telas e do LED do gabinete";
            link.Save();
            Log.Info($"Atalho criado em {folder}");
        }
        return 0;
    }

    /// <summary>Só uma instância pode falar com a tela.</summary>
    private static bool AcquireInstance(bool wait)
    {
        try
        {
            var mutex = new Mutex(false, InstanceName);
            bool acquired;
            try
            {
                // Ao "reiniciar como administrador" a instância antiga ainda está fechando: espera ela.
                acquired = mutex.WaitOne(wait ? TimeSpan.FromSeconds(15) : TimeSpan.Zero);
            }
            catch (AbandonedMutexException)
            {
                acquired = true; // a anterior morreu sem liberar
            }

            if (acquired) _instance = mutex;
            else mutex.Dispose();
            return acquired;
        }
        catch (UnauthorizedAccessException)
        {
            // Mutex criado por uma instância elevada: uma não elevada não consegue nem abri-lo.
            return false;
        }
    }

    /// <summary>Pede à instância aberta que feche e espera até 10 s. 0 = fechou (ou não havia nenhuma).</summary>
    private static int RequestExit()
    {
        File.WriteAllText(ExitFlagPath, "");
        for (int attempt = 0; attempt < 50; attempt++)
        {
            Thread.Sleep(200);
            if (File.Exists(ExitFlagPath)) continue;
            // Sinal consumido; espera a outra instância soltar as telas.
            for (int wait = 0; wait < 25 && System.Diagnostics.Process.GetProcessesByName("DS339Monitor").Length > 1; wait++)
                Thread.Sleep(200);
            return 0;
        }
        try { File.Delete(ExitFlagPath); } catch (IOException) { }
        return 1;
    }

    private static void ApplyCulture(string name)
    {
        try
        {
            SensorKeys.Culture = CultureInfo.GetCultureInfo(name);
        }
        catch (CultureNotFoundException)
        {
            Log.Warn($"Idioma '{name}' desconhecido; usando pt-BR.");
        }
    }
}
