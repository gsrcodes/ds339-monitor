using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.Json;
using DS339Monitor.Argb;
using DS339Monitor.Lighting;
using DS339Monitor.Sensors;
using DS339Monitor.Themes;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace DS339Monitor.App;

/// <summary>
/// Painel de controle: uma janela com a interface em HTML (pasta ui\), desenhada pelo WebView2 (o motor do Edge que
/// vem com o Windows). Mostra o gabinete ao vivo (miniaturas das duas telas e a cor do LED), os sensores, e configura
/// tudo o que o menu da bandeja configura — pelas mesmas funções do <see cref="TrayApp"/>, então os dois nunca
/// divergem. A página só fala com o programa por mensagens; nada de servidor nem porta aberta. Enquanto a janela não
/// existe, nada disso custa: as miniaturas e os sensores extras só são feitos com ela aberta (e não minimizada).
/// </summary>
internal sealed partial class ControlWindow : Form
{
    private const string Host = "painel.ds339";
    private const int PreviewSteps = 300; // 12 s de prévia de cada efeito, no ritmo da thread do LED (40 ms)

    private static readonly Color Paper = Color.FromArgb(0xF4, 0xF4, 0xF2);

    private readonly TrayApp _app;
    private readonly AppConfig _config;
    private readonly MonitorService _service;
    private readonly WebView2 _web = new() { Dock = DockStyle.Fill, DefaultBackgroundColor = Paper };
    private readonly System.Windows.Forms.Timer _live = new() { Interval = 100 };
    private readonly byte[]?[] _sentPreviews = new byte[3][];
    private readonly Dictionary<ScreenKind, object[]> _themes = new();
    private bool _ready;
    private bool _autostart;
    private string? _lastState;
    private string? _lastLed;
    private string[]? _lastArgb;
    private FanSnapshot? _lastFans;
    private string? _lastArgbStatus;
    private int _tick;
    private Color? _cardWhite;
    private bool _closing;
    private string? _captureRequest;

    public ControlWindow(TrayApp app, AppConfig config, MonitorService service)
    {
        _app = app;
        _config = config;
        _service = service;
        Text = "DS339 Monitor";
        Icon = AppIcon.Load();
        BackColor = Paper;
        AutoScaleMode = AutoScaleMode.Dpi;
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(LogicalToDeviceUnits(1320), LogicalToDeviceUnits(860));
        MinimumSize = new Size(LogicalToDeviceUnits(1040), LogicalToDeviceUnits(680));
        Controls.Add(_web);

        _live.Tick += (_, _) => PushLive();
        Shown += async (_, _) => await StartAsync();
        Resize += (_, _) => _service.DashboardOpen = _ready && WindowState != FormWindowState.Minimized;
        FormClosing += (_, _) => _closing = true;
        FormClosed += (_, _) =>
        {
            if (_calibrating) SetCalibration(false);
            _live.Stop();
            _service.DashboardOpen = false;
        };
        Task.Run(() => _autostart = Autostart.IsEnabled()); // consulta o agendador de tarefas: fora da interface
    }

    /// <summary>Aviso rápido dentro do painel.</summary>
    public void Toast(string text, bool warning) => Post(new { type = "toast", text, warning });

    /// <summary>Abre numa página.</summary>
    public void Navigate(string page) => Post(new { type = "navigate", page });

    /// <summary>
    /// Para conferir o visual (<c>--painel-imagem página</c>): vai para a página e, depois que ela desenha, salva uma
    /// imagem do painel em painel.png, ao lado do programa.
    /// </summary>
    public void CaptureImage(string page)
    {
        if (!_ready)
        {
            _captureRequest = page;
            return;
        }
        if (!page.EndsWith('!'))
        {
            // Recarrega antes (a página pode ter mudado no disco); o "pronto" da página dispara a captura.
            _captureRequest = page + "!";
            _ready = false;
            _web.CoreWebView2?.Reload();
            return;
        }
        page = page.TrimEnd('!');
        // "pagina@900" = rolada 900 px para baixo.
        int scroll = 0;
        if (page.IndexOf('@') is int at and > 0)
        {
            int.TryParse(page[(at + 1)..], out scroll);
            page = page[..at];
        }
        Navigate(page);
        var timer = new System.Windows.Forms.Timer { Interval = 2500 };
        timer.Tick += async (_, _) =>
        {
            timer.Dispose();
            if (_web.CoreWebView2 is null) return;
            if (scroll > 0)
            {
                await _web.CoreWebView2.ExecuteScriptAsync($"document.getElementById('page').scrollTop = {scroll}");
                await Task.Delay(400);
            }
            string path = Path.Combine(AppContext.BaseDirectory, "painel.png");
            using (var file = File.Create(path))
                await _web.CoreWebView2.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png, file);
            Log.Info($"Painel: imagem da página {page} salva em {path}");
        };
        timer.Start();
    }

    /// <summary>O programa vai sair: para as atualizações e não pede mais nada à página.</summary>
    public void PrepareForExit()
    {
        _closing = true;
        _ready = false;
        _live.Stop();
        _service.DashboardOpen = false;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _live.Dispose();
            var timer = Stopwatch.StartNew();
            _web.Dispose(); // fecha o processo do navegador junto com a janela
            if (timer.ElapsedMilliseconds > 500) Log.Info($"Painel: navegador fechado em {timer.ElapsedMilliseconds} ms.");
        }
        base.Dispose(disposing);
    }

    private async Task StartAsync()
    {
        try
        {
            // Sem aceleração de GPU: a página é simples, e o Direct3D do navegador na placa de vídeo prendia a leitura dos
            // sensores do driver da AMD por dezenas de segundos enquanto inicializava.
            var options = new CoreWebView2EnvironmentOptions("--disable-gpu --disable-gpu-compositing");
            var environment = await CoreWebView2Environment.CreateAsync(null, Path.Combine(AppContext.BaseDirectory, "painel-dados"), options);
            await _web.EnsureCoreWebView2Async(environment);
        }
        catch (Exception ex) when (ex is WebView2RuntimeNotFoundException or COMException or InvalidOperationException or IOException
            or ObjectDisposedException)
        {
            // Fechado (ou o programa saindo) enquanto o navegador abria: o WebView2 aborta, e está tudo certo.
            // Nunca uma caixa de mensagem aqui — no meio da saída ela prenderia o programa aberto.
            if (IsDisposed || Disposing || _closing || ex is ObjectDisposedException || (ex is COMException com && com.HResult == unchecked((int)0x80004004)))
            {
                Log.Info("Painel: fechado antes de terminar de abrir.");
                return;
            }
            Log.Error("Painel: o WebView2 não abriu", ex);
            _app.Notify("O painel precisa do Microsoft Edge WebView2 (vem com o Windows 11). Detalhes no log.", warning: true);
            Close();
            return;
        }
        if (_closing) return;

        var core = _web.CoreWebView2;
        core.Settings.AreDefaultContextMenusEnabled = false;
        core.Settings.AreDevToolsEnabled = Debugger.IsAttached;
        core.Settings.IsStatusBarEnabled = false;
        core.Settings.IsZoomControlEnabled = false;
        core.Settings.AreBrowserAcceleratorKeysEnabled = false;
        // A pasta do programa vira um "site" local: a página (ui\), as fontes (fonts\) e os GIFs da biblioteca (media\).
        core.SetVirtualHostNameToFolderMapping(Host, AppContext.BaseDirectory, CoreWebView2HostResourceAccessKind.Deny);
        core.NavigationStarting += (_, e) =>
        {
            if (!e.Uri.StartsWith($"https://{Host}/", StringComparison.OrdinalIgnoreCase)) e.Cancel = true;
        };
        core.NewWindowRequested += (_, e) => e.Handled = true;
        core.WebMessageReceived += OnMessage;
        core.Navigate($"https://{Host}/ui/index.html");
    }

    // ---------------------------------------------------------------- mensagens da página

    private void OnMessage(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        try
        {
            using var document = JsonDocument.Parse(e.WebMessageAsJson);
            HandleMessage(document.RootElement);
        }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException or FormatException)
        {
            Log.Warn($"Painel: mensagem inválida ({ex.Message}).");
        }
    }

    private void HandleMessage(JsonElement m)
    {
        string S(string name) => m.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() ?? "" : "";
        // Número, ou texto com número (os atributos data-* da página chegam como texto).
        int I(string name) => !m.TryGetProperty(name, out var v) ? 0 : v.ValueKind switch
        {
            JsonValueKind.Number => v.TryGetInt32(out int i) ? i : 0,
            JsonValueKind.String => int.TryParse(v.GetString(), out int j) ? j : 0,
            _ => 0,
        };
        bool B(string name) => m.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.True;
        ArgbZone? Zone() => int.TryParse(S("zone"), out int z) && z >= 0 && z < _config.FansEFitas.Zonas.Count ? _config.FansEFitas.Zonas[z] : null;
        ScreenKind K() => S("screen") switch
        {
            "thermalright" => ScreenKind.Thermalright,
            "proflow" => ScreenKind.ProFlow,
            _ => ScreenKind.Ds339,
        };

        switch (S("cmd"))
        {
            case "ready":
                _ready = true;
                _lastState = null;
                Array.Clear(_sentPreviews);
                _service.DashboardOpen = WindowState != FormWindowState.Minimized;
                _live.Start();
                if (Interlocked.Exchange(ref _captureRequest, null) is { } pending)
                {
                    PushState(force: true);
                    CaptureImage(pending);
                }
                break;
            case "theme": _app.SetTheme(K(), S("name")); break;
            case "brightness": _app.SetBrightness(K(), I("value")); break;
            case "rotate": _app.SetRotated(K(), B("on")); break;
            case "data": _app.SetDataStyle(K(), S("style")); break;
            case "fps": _app.SetFps(K(), I("value")); break;
            case "media": _app.UseMedia(K(), Path.Combine(MediaFolder, Path.GetFileName(S("file")))); break;
            case "chooseVideo": _app.ChooseVideo(); break;
            case "chooseGif": _app.ChooseGifFor(K()); break;
            case "onePiece": _app.OpenOnePieceDownloads(); break;
            case "ledEffect": _app.SetLedEffect(S("name")); break;
            case "ledTone": _app.SetLedTone(S("name")); break;
            case "ledSpeed": _app.SetLedSpeed(S("name")); break;
            case "ledBrightness": _app.SetLedBrightness(I("value")); break;
            case "ledColor": _app.SetLedColor(S("hex")); break;
            case "manga": _app.ToggleManga(B("on")); break;
            case "mangaOption": _app.SetMangaOption(S("option"), B("on")); break;
            case "gear5": _app.ChooseGear5Gif(); break;
            case "grandLine": _app.ToggleGrandLine(B("on")); break;
            case "allOff": _service.SetAllOff(B("on")); break;
            case "story": _service.TriggerStory(S("name")); break;
            case "autostart":
                _app.ToggleAutostart(B("on"));
                Task.Run(() => _autostart = Autostart.IsEnabled()).ContinueWith(_ => BeginInvoke(new Action(() => PushState(force: true))));
                break;
            case "capture": _app.SaveScreenImages(); break;
            case "log": TrayApp.OpenFile(Log.FilePath); break;
            case "folder":
                Process.Start(new ProcessStartInfo("explorer.exe", $"\"{AppContext.BaseDirectory}\"") { UseShellExecute = true })?.Dispose();
                break;
            case "admin": _app.RestartElevated(); break;
            case "exit": _app.Exit(); break;
            case "previews":
                SendEffectPreviews();
                return;
            default:
                if (!HandleArgb(S("cmd"), S, I, B, Zone) && !HandleUmbra(S("cmd"), S, I, B) && !HandleNollie(S("cmd"), S, I, B) && !HandleFans(S("cmd"), S, I, B) && !HandleWhite(S("cmd"), S, I, B) && !HandleGame(S("cmd"), S, B)) return;
                break;
        }
        PushState(force: true);
    }

    // ---------------------------------------------------------------- estado

    private void PushState(bool force)
    {
        if (!_ready) return;
        string json = JsonSerializer.Serialize(BuildState());
        if (!force && json == _lastState) return;
        _lastState = json;
        _web.CoreWebView2?.PostWebMessageAsJson(json);
    }

    private object BuildState()
    {
        var led = _config.LedGpu;
        string[] groupTitles = { "Cores", "Cena", "Com a Pro Flow", "Natureza", "Ritmo", "Reagem ao PC" };
        return new
        {
            type = "state",
            version = Application.ProductVersion,
            admin = Environment.IsPrivilegedProcess,
            autostart = _autostart,
            allOff = _service.AllOff,
            screens = new[]
            {
                ScreenState(ScreenKind.Thermalright, "thermalright", "Thermalright 9,16\"", "1920 × 462", _config.Thermalright.Tema,
                    _config.Thermalright.Brilho, _config.Thermalright.Rotacao == 0, _config.Thermalright.Dados, _config.Thermalright.FpsMax,
                    _config.Thermalright.Video, _config.Thermalright.Gif),
                ScreenState(ScreenKind.ProFlow, "proflow", "Pro Flow 360", "960 × 480", _config.ProFlow.Tema,
                    _config.ProFlow.Brilho, _config.ProFlow.Rotacao == 0, _config.ProFlow.Dados, _config.ProFlow.FpsMax, null, _config.ProFlow.Gif),
                ScreenState(ScreenKind.Ds339, "ds339", "Jonsbo DS339", "960 × 376", _config.DS339.Tema,
                    _config.DS339.Brilho, _config.DS339.Rotacao == 270, null, 0, null, null),
            },
            led = new
            {
                effect = led.Efeito,
                tone = led.Branco,
                tones = GpuEffects.WhiteTones,
                speed = led.Velocidade,
                speeds = GpuEffects.Speeds,
                brightness = led.Brilho,
                color = led.Cor,
                groups = GpuEffects.Groups.Select((group, i) => new
                {
                    title = i < groupTitles.Length ? groupTitles[i] : "",
                    effects = group.Select(name => new
                    {
                        name,
                        desc = EffectDescriptions.TryGetValue(name, out string? d) ? d : "",
                        color = GpuEffects.UsesColor(name),
                        speed = GpuEffects.UsesSpeed(name),
                        white = GpuEffects.UsesWhite(name),
                    }),
                }),
            },
            manga = new
            {
                on = _config.Manga.Ligado,
                chapters = _config.Manga.CapitulosNaHora,
                actions = _config.Manga.AcoesDoPc,
                gear5 = FileName(_config.Manga.GifGear5),
                stories = MangaStories,
            },
            grandLine = new { on = _config.GrandLine, stories = SceneStories },
            argb = ArgbState(),
            umbra = UmbraState(),
            nollie = NollieState(),
            fans = FanState(),
            white = WhiteState(),
            game = GameState(),
        };
    }

    // ---------------------------------------------------------------- modo jogo

    private string? _cs2Folder;
    private bool _cs2Searched;

    private object GameState()
    {
        var game = _config.ModoJogo;
        if (!_cs2Searched)
        {
            _cs2Searched = true;
            _cs2Folder = Game.Cs2Gsi.FindCfgFolder();
        }
        long last = Game.Cs2Gsi.LastMessage;
        return new
        {
            on = game.Ligado,
            games = string.Join(", ", game.Jogos),
            status = _service.Game.Status,
            cs2Found = _cs2Folder is not null,
            cs2Installed = Game.Cs2Gsi.IsInstalled(_cs2Folder),
            cs2Seen = last == 0 ? null : FormatAgo(Environment.TickCount64 - last),
            reactions = game.Reacoes,
        };
    }

    private static string FormatAgo(long ms) => ms switch
    {
        < 15_000 => "agora",
        < 3_600_000 => $"há {ms / 60_000 + 1} min",
        _ => $"há {ms / 3_600_000} h",
    };

    private bool HandleGame(string cmd, Func<string, string> S, Func<string, bool> B)
    {
        var game = _config.ModoJogo;
        switch (cmd)
        {
            case "gameEnabled":
                game.Ligado = B("on");
                Toast(game.Ligado ? "Modo jogo ligado: ao abrir um jogo da lista, a Thermalright mostra o painel de jogo." : "Modo jogo desligado.", false);
                break;
            case "gameList":
                game.Jogos = S("text").Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                game.Normalize();
                break;
            case "gamePreview":
                _service.Game.Preview(20);
                Toast("Olhe a Thermalright: o painel de jogo com uma partida de mentira (20 s).", false);
                return true;
            case "gameReactions":
                game.Reacoes = B("on");
                break;
            case "cs2Test":
                _service.Game.Preview(17);
                Game.Cs2Events.Simulate();
                Toast("Rodada de mentira (16 s): abate, capacete, bomba plantada, explosão, vitória e um flash — olhe o gabinete.", false);
                return true;
            case "cs2Install":
                _cs2Folder ??= Game.Cs2Gsi.FindCfgFolder();
                if (_cs2Folder is null)
                {
                    Toast("Não achei o CS2 nas bibliotecas da Steam.", true);
                    return true;
                }
                Game.Cs2Gsi.Install(_cs2Folder, game.CodigoCs2!);
                Toast("Integração gravada no CS2 — vale a partir da próxima vez que ele abrir.", false);
                break;
            case "cs2Uninstall" when _cs2Folder is not null:
                Game.Cs2Gsi.Uninstall(_cs2Folder);
                Toast("Integração removida do CS2.", false);
                break;
            default:
                return false;
        }
        _config.Save();
        _service.Game.Reload();
        return true;
    }

    // ---------------------------------------------------------------- balanço de branco

    private bool _calibrating;
    private int _calibrationLevel = 50;
    private System.Windows.Forms.Timer? _calibrationTimer;

    /// <summary>Peças que o usuário quer acertar primeiro (as que mais puxam o branco para um lado).</summary>
    private static readonly string[] MainWhiteGroups = { "Cabo da placa de vídeo", "Cabo da placa-mãe" };

    /// <summary>O branco que o tom escolhido no LED da placa produz, como balanço (ponto de partida do ajuste).</summary>
    private int[] ToneGains(string tone)
    {
        var white = tone switch
        {
            GpuEffects.WhiteCard => CardWhite,
            GpuEffects.WhiteWarm => Color.FromArgb(0xFF, 0xC4, 0x8C),
            GpuEffects.WhiteCool => Color.FromArgb(0xC8, 0xE1, 0xFF),
            _ => Color.White,
        };
        return new[] { white.R * 100 / 255, white.G * 100 / 255, white.B * 100 / 255 };
    }

    private object WhiteState()
    {
        var zones = _config.FansEFitas.Zonas;
        var channels = _service.ArgbChannelsBottomUp().Select(c => c.Logical).ToList();
        var groups = zones.GroupBy(z => z.Grupo)
            .OrderBy(g => Array.IndexOf(MainWhiteGroups, g.Key) is int i && i >= 0 ? i : 10 + channels.IndexOf(g.First().Canal))
            .Select(g => new
            {
                name = g.Key,
                main = MainWhiteGroups.Contains(g.Key),
                gains = g.First().Balanco ?? new[] { 100, 100, 100 },
                leds = g.Sum(z => z.Leds),
            });
        var gpu = _config.LedGpu;
        return new
        {
            calibrating = _calibrating,
            level = _calibrationLevel,
            gpu = new
            {
                gains = gpu.Balanco ?? ToneGains(gpu.Branco),
                note = gpu.Balanco is null && gpu.Branco != GpuEffects.WhiteNeutral
                    ? gpu.Branco == GpuEffects.WhiteCard
                        ? "Hoje ela usa o branco guardado nela (255, 210, 255). Ao mexer, ele vira o ponto de partida."
                        : $"Hoje ela usa o tom \"{gpu.Branco}\" da página do LED. Ao mexer, ele vira o ponto de partida (\"Voltar\" devolve o tom)."
                    : null,
            },
            groups,
        };
    }

    /// <summary>Liga/desliga o "comparar brancos": a placa e todas as partes no mesmo branco (volta sozinho em 10 min).</summary>
    private void SetCalibration(bool on)
    {
        _calibrating = on;
        double linear = Math.Pow(_calibrationLevel / 100.0, 2);
        _service.Argb.SetCalibration(on ? (float)linear : 0);
        // Na placa, sem balanço ainda, o branco dos efeitos é o do tom escolhido: compara com ele.
        int[] tone = _config.LedGpu.Balanco is null ? ToneGains(_config.LedGpu.Branco) : new[] { 100, 100, 100 };
        int V(int i) => (int)Math.Round(255 * linear * tone[i] / 100);
        _service.GpuLed.CalibrationWhite = on ? Color.FromArgb(V(0), V(1), V(2)) : null;
        _calibrationTimer?.Dispose();
        _calibrationTimer = null;
        if (!on) return;
        _calibrationTimer = new System.Windows.Forms.Timer { Interval = 10 * 60 * 1000 };
        _calibrationTimer.Tick += (_, _) =>
        {
            SetCalibration(false);
            PushState(force: true);
        };
        _calibrationTimer.Start();
    }

    private bool HandleWhite(string cmd, Func<string, string> S, Func<string, int> I, Func<string, bool> B)
    {
        bool live = B("live");
        switch (cmd)
        {
            case "whiteCalibrate":
                SetCalibration(B("on"));
                break;
            case "whiteLevel":
                _calibrationLevel = Math.Clamp(I("value"), 10, 100);
                if (_calibrating) SetCalibration(true);
                if (live) return true;
                break;
            case "whiteGain" when int.TryParse(S("channel"), out int channel) && channel is >= 0 and <= 2:
            {
                int value = Math.Clamp(I("value"), 0, 100);
                if (S("target") == "gpu")
                {
                    var gpu = _config.LedGpu;
                    if (gpu.Balanco is null)
                    {
                        // Primeira mexida: parte do branco que o tom de agora produz e passa a usar o balanço (com tom
                        // neutro, senão a correção seria dobrada). Nada muda até mexer de verdade.
                        gpu.Balanco = ToneGains(gpu.Branco);
                        gpu.BrancoAntesDoBalanco = gpu.Branco;
                        gpu.Branco = GpuEffects.WhiteNeutral;
                        _service.GpuLed.Reload();
                        if (_calibrating) SetCalibration(true);
                    }
                    var gains = (int[])gpu.Balanco.Clone();
                    gains[channel] = value;
                    gpu.Balanco = gains; // 100% nos três continua valendo (tom neutro sem correção)
                    _service.GpuLed.Nudge();
                }
                else
                {
                    string group = S("target");
                    foreach (var zone in _config.FansEFitas.Zonas.Where(z => z.Grupo == group))
                    {
                        var gains = (int[])(zone.Balanco ?? new[] { 100, 100, 100 }).Clone();
                        gains[channel] = value;
                        zone.Balanco = WhiteBalance.Clean(gains);
                    }
                    _service.Argb.Nudge();
                }
                if (live) return true; // durante o arrasto: aplica, sem gravar o config
                break;
            }
            case "whiteReset":
                if (S("target") == "gpu")
                {
                    _config.LedGpu.Balanco = null;
                    _config.LedGpu.Branco = _config.LedGpu.BrancoAntesDoBalanco ?? _config.LedGpu.Branco;
                    _config.LedGpu.BrancoAntesDoBalanco = null;
                    _service.GpuLed.Reload();
                    if (_calibrating) SetCalibration(true);
                }
                else
                {
                    foreach (var zone in _config.FansEFitas.Zonas.Where(z => z.Grupo == S("target"))) zone.Balanco = null;
                    _service.Argb.Nudge();
                }
                break;
            default:
                return false;
        }
        _config.Save();
        return true;
    }

    private object FanState()
    {
        var fans = _config.Ventoinhas;
        return new
        {
            on = fans.Ligado,
            output = fans.Saida,
            profile = fans.Perfil,
            profiles = FanCurve.Profiles,
            cpu = fans.CurvaCpu,
            gpu = fans.CurvaGpu,
            floor = fans.Piso,
        };
    }

    private static object FanLive(FanSnapshot s) => new
    {
        board = s.Board,
        outputs = s.Outputs.Select(o => new { index = o.Index, name = o.Name, value = o.Value is double v ? Math.Round(v) : (double?)null }),
        rpms = s.Fans.Select(f => new { name = f.Name, rpm = f.Rpm is double r ? Math.Round(r) : (double?)null }),
        cpu = s.CpuTemp is double c ? Math.Round(c, 1) : (double?)null,
        gpu = s.GpuTemp is double g ? Math.Round(g, 1) : (double?)null,
        target = s.Target is double t ? Math.Round(t) : (double?)null,
        applied = s.Applied,
        status = s.Status,
    };

    private bool HandleFans(string cmd, Func<string, string> S, Func<string, int> I, Func<string, bool> B)
    {
        var fans = _config.Ventoinhas;
        switch (cmd)
        {
            case "fanEnabled":
                if (B("on") && fans.Saida is null)
                {
                    Toast("Escolha antes qual saída de PWM é a do hub (use \"Testar\").", true);
                    return true;
                }
                fans.Ligado = B("on");
                Toast(fans.Ligado ? "A curva assumiu os fans do gabinete." : "A BIOS voltou a mandar nos fans.", false);
                break;
            case "fanOutput" when int.TryParse(S("index"), out int output):
                fans.Saida = Math.Clamp(output, 0, 15);
                break;
            case "fanTest" when int.TryParse(S("index"), out int test):
                _service.Fans.Test(test);
                Toast($"PWM {test + 1} em 100% por 6 s — escute os fans do gabinete.", false);
                return true;
            case "fanProfile" when FanCurve.Profiles.Contains(S("name")):
                fans.Perfil = S("name");
                if (FanCurve.Presets.TryGetValue(fans.Perfil, out var preset))
                {
                    fans.CurvaCpu = preset.Cpu.Select(p => (int[])p.Clone()).ToList();
                    fans.CurvaGpu = preset.Gpu.Select(p => (int[])p.Clone()).ToList();
                }
                break;
            case "fanCurve":
            {
                int[][]? points;
                try
                {
                    points = JsonSerializer.Deserialize<int[][]>(S("points"));
                }
                catch (JsonException)
                {
                    return true;
                }
                if (points is null) return true;
                var fallback = FanCurve.Presets[FanCurve.Balanced];
                if (S("curve") == "gpu") fans.CurvaGpu = FanCurve.Clean(points.ToList(), fallback.Gpu);
                else fans.CurvaCpu = FanCurve.Clean(points.ToList(), fallback.Cpu);
                fans.Perfil = FanCurve.Custom;
                break;
            }
            case "fanFloor":
                fans.Piso = Math.Clamp(I("value"), 15, 60);
                break;
            default:
                return false;
        }
        fans.Normalize();
        _config.Save();
        _service.Fans.Reload();
        return true;
    }

    private static readonly object[] IdleOptions =
    {
        new { value = 0, label = "No tempo padrão dela" },
        new { value = 5, label = "5 segundos" },
        new { value = 30, label = "30 segundos" },
        new { value = 60, label = "1 minuto" },
        new { value = 300, label = "5 minutos" },
        new { value = 65535, label = "Nunca (fica no último quadro)" },
    };

    private bool _nollieAsked;
    private bool _building;

    private string AnimationKey(NollieStoredSettings draft) => NollieBootFile.Key(_config.FansEFitas, draft);

    private int AnimationBytes(NollieStoredSettings draft) =>
        NollieBootFile.EstimateBytes(_config.FansEFitas, draft.BootAnimacao ?? BootAnimation.ShortCircuit, draft.ContinuarMinutos);

    /// <summary>O que está gravado na controladora, o rascunho e o que uma gravação mudaria.</summary>
    private object NollieState()
    {
        var stored = _service.Argb.Stored;
        var argb = _config.FansEFitas;
        if (stored is null)
        {
            if (!_nollieAsked)
            {
                _nollieAsked = true;
                _service.Argb.RefreshStored().ContinueWith(_ => RunOnUi(() => PushState(force: true)), TaskScheduler.Default);
            }
            return new { read = false };
        }
        if (argb.NaControladora is null)
        {
            argb.NaControladora = stored.ToSettings(); // começa do que já está gravado nela
            _config.Save();
        }
        var draft = argb.NaControladora;
        var channels = _service.ArgbChannelsBottomUp();
        var plan = stored.Plan(draft, channels).Select(c => c.What).ToList();
        if (draft.BootAnimacao is { } animationName && AnimationKey(draft) != draft.AnimacaoGravada)
        {
            int bytes = AnimationBytes(draft);
            string what = draft.ContinuarMinutos > 0
                ? $"gravar \"{animationName}\" emendada em {draft.ContinuarMinutos} min do loop dos fans"
                : $"gravar a animação \"{animationName}\"";
            plan.Insert(0, $"{what} na memória dela ({bytes / 1048576.0:0.0} MB, cerca de {Math.Max(5, bytes / 90_000)} s para gravar, conferida pelo CRC)");
        }
        var pending = plan.ToArray();

        static object Effect(NollieEffectSettings e) => new
        {
            effect = e.Efeito,
            color1 = e.Cor1,
            color2 = e.Cor2,
            brightness = e.Brilho,
            speed = e.Velocidade,
            size = e.Tamanho,
        };
        static string Summary(NollieCanvas? c) => c is null ? "nenhum"
            : $"{c.EffectName} · {GpuLighting.ToHex(c.Color1)} · brilho {c.Brightness} · velocidade {c.Step}";
        int[] hardware = Argb.NollieDevice.HardwareChannels;
        var leds = channels.Select(c => stored.LedCount[hardware[c.Logical - 1]]).Distinct().ToArray();
        return new
        {
            read = true,
            writeCount = stored.WriteCount,
            boot = Effect(draft.Boot),
            normal = Effect(draft.SemPrograma),
            idle = draft.VoltaSegundos,
            idleOptions = IdleOptions,
            bootEffects = NollieConfig.BootEffects.Select((name, i) => new { name, color2 = BootUsesColor2(i), size = i != 0 }),
            loopEffects = NollieConfig.LoopEffects.Select((name, i) => new { name, color2 = LoopUsesColor2(i), size = LoopUsesSize(i) }),
            storedBoot = Summary(stored.BootCount > 0 ? stored.Slot(NollieConfig.BootBase) : null),
            storedNormal = Summary(stored.LoopCount > 0 ? stored.Slot(0) : null),
            storedLeds = leds.Length == 1 ? $"{leds[0]} em todos" : string.Join(", ", leds),
            storedIdle = NollieConfig.IdleText(stored.IdleSeconds),
            order = string.Join(" → ", channels.Select(c => ChannelLabel(c.Logical))),
            pending,
            bootAnimation = draft.BootAnimacao,
            continueMinutes = draft.ContinuarMinutos,
            animations = BootAnimation.All.Select(name => new { name, desc = BootAnimation.Descriptions[name] }),
            grb = draft.CoresGrb,
            storedIntro = stored.BootIntro,
        };
    }

    private static bool BootUsesColor2(int i) => i is 1 or 2 or 4 or 5 or 6;
    private static bool LoopUsesColor2(int i) => i is 6 or 8 or 9 or (>= 15 and <= 20);
    private static bool LoopUsesSize(int i) => i is 3 or 5 or 6 or 7 or (>= 8 and <= 23);

    /// <summary>Nome da peça de um canal (o grupo da primeira parte dele).</summary>
    private string ChannelLabel(int logical) =>
        _config.FansEFitas.Zonas.FirstOrDefault(z => z.Canal == logical)?.Grupo is { } group ? group : $"canal {logical}";

    private bool HandleNollie(string cmd, Func<string, string> S, Func<string, int> I, Func<string, bool> B)
    {
        var draft = _config.FansEFitas.NaControladora;
        switch (cmd)
        {
            case "nollieRead":
                _service.Argb.RefreshStored().ContinueWith(t => RunOnUi(() =>
                {
                    Toast(t.Result == "ok" ? "Lido da controladora." : $"Não consegui ler: {t.Result[5..]}", t.Result != "ok");
                    PushState(force: true);
                }), TaskScheduler.Default);
                return true;
            case "nollieRevert":
                if (_service.Argb.Stored is { } stored) _config.FansEFitas.NaControladora = stored.ToSettings();
                break;
            case "nollieIdle" when draft is not null && int.TryParse(S("text"), out int idle):
                draft.VoltaSegundos = Math.Clamp(idle, 0, 65535);
                break;
            case "nollieField" when draft is not null:
            {
                var effect = S("part") == "boot" ? draft.Boot : draft.SemPrograma;
                switch (S("field"))
                {
                    case "effect" when int.TryParse(S("text"), out int index): effect.Efeito = index; break;
                    case "color1": effect.Cor1 = S("hex"); break;
                    case "color2": effect.Cor2 = S("hex"); break;
                    case "brightness": effect.Brilho = I("value"); break;
                    case "speed": effect.Velocidade = I("value"); break;
                    case "size": effect.Tamanho = I("value"); break;
                    default: return false;
                }
                draft.Normalize();
                break;
            }
            case "nollieWrite" when draft is not null:
            {
                if (_building) return true;
                string key = AnimationKey(draft);
                bool upload = draft.BootAnimacao is not null && key != draft.AnimacaoGravada;
                var settings = _config.FansEFitas;
                var channels = _service.ArgbChannelsBottomUp();
                Toast(upload ? "Montando a animação e gravando na memória da controladora — acompanhe no alto da página." : "Gravando na controladora…", false);
                _building = upload;
                Task.Run(() =>
                {
                    if (!upload) return _service.Argb.WriteStored(draft, channels);
                    _service.Argb.SetJobStatus("montando a animação…");
                    var (file, map, source) = NollieBootFile.Build(settings, draft.BootAnimacao!, draft.ContinuarMinutos, draft.CoresGrb);
                    Log.Info($"Nollie: arquivo \"{source.Name}\" montado — {source.Count} quadros de {source.FrameMs} ms, {file.Length} bytes.");
                    return _service.Argb.WriteStored(draft, channels, file, map);
                }).ContinueWith(t => RunOnUi(() =>
                {
                    _building = false;
                    _service.Argb.SetJobStatus(null);
                    string result = t.IsFaulted ? "erro: " + t.Exception!.GetBaseException().Message : t.Result;
                    bool failed = result.StartsWith("erro", StringComparison.Ordinal);
                    if (!failed && upload)
                    {
                        draft.AnimacaoGravada = key;
                        _config.Save();
                    }
                    Toast(failed ? $"Não gravou: {result[5..]}" : $"Na controladora: {result}.", failed);
                    PushState(force: true);
                }), TaskScheduler.Default);
                return true;
            }
            case "nollieContinue" when draft is not null && int.TryParse(S("text"), out int minutes):
                draft.ContinuarMinutos = minutes;
                break;
            case "nollieBootKind" when draft is not null:
                draft.BootAnimacao = S("name") == "Animação do programa" ? draft.BootAnimacao ?? BootAnimation.ShortCircuit : null;
                break;
            case "nollieAnimation" when draft is not null && BootAnimation.All.Contains(S("text")):
                draft.BootAnimacao = S("text");
                break;
            case "nollieGrb" when draft is not null:
                draft.CoresGrb = !B("on"); // o interruptor é "cores trocadas"
                break;
            case "nollieAnimationPreview" when draft?.BootAnimacao is { } previewName:
                _service.Argb.Play(BootAnimation.Render(previewName, _config.FansEFitas.Zonas, _config.FansEFitas));
                Toast($"Olhe o gabinete: \"{previewName}\" tocando pelo programa.", false);
                return true;
            case "nolliePreview" when _service.Argb.Stored?.BootIntro == 2 || draft?.BootAnimacao is not null:
                _service.Argb.PreviewStoredAnimation(draft?.BootAnimacao is not null && draft.ContinuarMinutos > 0 ? 20000 : 8000).ContinueWith(t => RunOnUi(() =>
                    Toast(t.Result == "ok" ? "Olhe o gabinete: a controladora está tocando a animação gravada nela." : $"Não deu: {t.Result[5..]}",
                        t.Result != "ok")), TaskScheduler.Default);
                return true;
            case "nolliePreview":
                _service.Argb.PreviewBoot(12).ContinueWith(t => RunOnUi(() =>
                    Toast(t.Result == "ok" ? "Olhe o gabinete: a controladora está tocando o efeito de boot gravado nela (12 s)." : $"Não deu: {t.Result[5..]}",
                        t.Result != "ok")), TaskScheduler.Default);
                return true;
            default:
                return false;
        }
        _config.Save();
        return true;
    }

    private object ArgbState()
    {
        var argb = _config.FansEFitas;
        var zones = argb.Zonas;
        return new
        {
            on = argb.Ligado,
            other = _service.Argb.OtherApp,
            connected = _service.Argb.Connected,
            effect = argb.Efeito,
            color = argb.Cor,
            brightness = argb.Brilho,
            speed = argb.Velocidade,
            speeds = GpuEffects.Speeds,
            limit = $"{argb.LimiteAmperes} A",
            limits = new[] { "4 A", "6 A", "8 A", "10 A", "12 A" },
            effectGroups = new[]
            {
                ("Cenas do gabinete", ArgbEffects.Scenes), ("Cada parte sozinha", ArgbEffects.Main),
                ("Capas de cabo (estilo Strimer da Lian Li)", ArgbEffects.Cable),
            }.Select(g => new
            {
                title = g.Item1,
                effects = g.Item2.Select(name => new
                {
                    name,
                    desc = ArgbEffects.Descriptions.GetValueOrDefault(name, ""),
                    color = ArgbEffects.UsesColor(name),
                    speed = ArgbEffects.UsesSpeed(name),
                    cable = ArgbEffects.IsCable(name),
                }),
            }),
            layout = new
            {
                photo = CaseLayout.Photo,
                width = CaseLayout.PhotoWidth,
                height = CaseLayout.PhotoHeight,
                view = CaseLayout.View,
                thermalright = CaseLayout.Thermalright,
                proflow = CaseLayout.ProFlow,
                gpu = CaseLayout.Gpu,
                points = zones.Select(ArgbLighting.PhotoPoints),
            },
            zoneEffects = ArgbEffects.ForZone,
            zoneEffectGroups = new[]
            {
                new { title = "Capas de cabo (estilo Strimer)", effects = ArgbEffects.Cable },
                new { title = "Cenas do gabinete", effects = ArgbEffects.Scenes },
                new { title = "Cada parte sozinha", effects = ArgbEffects.Main },
            },
            leds = zones.Sum(z => z.Leds),
            zoneCount = zones.Count,
            // Peças de cima para baixo, como estão no gabinete.
            groups = zones.Select((zone, index) => (zone, index)).GroupBy(p => p.zone.Grupo)
                .OrderBy(g => g.Select(p => CaseLayout.Parse(p.zone.Forma, p.zone.Leds, false)?.Average(q => q.V) ?? 2).Min())
                .Select(g => new
            {
                name = g.Key,
                channels = string.Join(", ", g.Select(p => p.zone.Canal).Distinct()),
                leds = g.Sum(p => p.zone.Leds),
                zones = g.Select(p => new
                {
                    p.index,
                    name = p.zone.Nome,
                    channel = p.zone.Canal,
                    start = p.zone.Inicio,
                    leds = p.zone.Leds,
                    effect = p.zone.Efeito,
                    color = p.zone.Cor,
                    brightness = p.zone.Brilho,
                    invert = p.zone.Inverter,
                }),
            }),
        };
    }

    // ---------------------------------------------------------------- fans e fitas

    /// <summary>Comandos da página "Fans e fitas". False = comando desconhecido.</summary>
    private bool HandleArgb(string cmd, Func<string, string> S, Func<string, int> I, Func<string, bool> B, Func<ArgbZone?> Zone)
    {
        var argb = _config.FansEFitas;
        switch (cmd)
        {
            case "argbTake":
                Toast(SignalRgb.IsRunning() ? "Fechando o SignalRGB pelo caminho normal…" : "Fans e fitas com o programa.", false);
                _service.TakeArgbControl().ContinueWith(t => RunOnUi(() =>
                {
                    if (!t.Result) Toast("O SignalRGB não fechou. Feche ele pela bandeja dele — o programa assume sozinho.", true);
                    PushState(force: true);
                }), TaskScheduler.Default);
                return true;
            case "argbReturn":
                _service.ReturnArgbToSignalRgb();
                Toast("Devolvido ao SignalRGB — abrindo ele.", false);
                return true;
            case "argbEffect":
                if (!ArgbEffects.All.Contains(S("name"))) return false;
                argb.Efeito = S("name");
                break;
            case "argbColor":
                argb.Cor = GpuLighting.ToHex(GpuLighting.ParseColor(S("hex")));
                if (!ArgbEffects.UsesColor(argb.Efeito)) argb.Efeito = ArgbEffects.Fixed;
                break;
            case "argbBrightness":
                argb.Brilho = Math.Clamp(I("value"), 5, 100);
                break;
            case "argbSpeed":
                if (!GpuEffects.Speeds.Contains(S("name"))) return false;
                argb.Velocidade = S("name");
                break;
            case "argbLimit":
                if (!int.TryParse(S("name").Split(' ')[0], out int amps)) return false;
                argb.LimiteAmperes = Math.Clamp(amps, 4, 12);
                break;
            case "argbReimport":
            {
                var zones = SignalRgbImport.Read();
                if (zones.Count == 0)
                {
                    Toast("Não achei a Nollie na configuração do SignalRGB.", true);
                    return true;
                }
                argb.Zonas = zones;
                argb.Normalize();
                CaseLayout.Apply(argb.Zonas);
                Toast($"{zones.Count} partes importadas do SignalRGB.", false);
                break;
            }
            case "zoneEffect" when Zone() is { } zone && ArgbEffects.ForZone.Contains(S("text")):
                zone.Efeito = S("text");
                break;
            case "zoneColor" when Zone() is { } zone:
                zone.Cor = GpuLighting.ToHex(GpuLighting.ParseColor(S("hex")));
                // Uma cor numa parte que segue o geral (ou num efeito sem cor) quer dizer "esta parte nesta cor".
                if (zone.Efeito == ArgbEffects.Global) zone.Efeito = ArgbEffects.UsesColor(argb.Efeito) ? argb.Efeito : ArgbEffects.Fixed;
                else if (!ArgbEffects.UsesColor(zone.Efeito)) zone.Efeito = ArgbEffects.Fixed;
                break;
            case "zoneBrightness" when Zone() is { } zone:
                zone.Brilho = Math.Clamp(I("value"), 0, 200);
                break;
            case "zoneInvert" when Zone() is { } zone:
                zone.Inverter = B("on");
                break;
            case "zoneName" when Zone() is { } zone:
            {
                string name = S("text").Trim();
                if (name.Length == 0) break;
                zone.Nome = name.Length > 40 ? name[..40] : name;
                break;
            }
            case "groupName":
            {
                string old = S("group"), name = S("text").Trim();
                if (name.Length == 0 || name == old) break;
                if (name.Length > 40) name = name[..40];
                foreach (var z in argb.Zonas.Where(z => z.Grupo == old)) z.Grupo = name;
                break;
            }
            case "identify" when Zone() is not null:
                _service.Argb.Identify(int.Parse(S("zone")));
                Toast("A parte escolhida está pulsando em branco por 8 s; o resto apaga.", false);
                return true;
            default:
                return false;
        }
        _config.Save();
        _service.Argb.Reload();
        return true;
    }

    private void RunOnUi(Action action)
    {
        if (IsDisposed || _closing) return;
        try
        {
            BeginInvoke(action);
        }
        catch (InvalidOperationException)
        {
            // Janela fechando.
        }
    }

    private object ScreenState(ScreenKind kind, string id, string name, string size, string theme, int brightness, bool rotated,
        string? data, int fps, string? video, string? gif)
    {
        if (!_themes.TryGetValue(kind, out var themes))
            _themes[kind] = themes = ThemeCatalog.Discover(kind).Select(t => (object)new { name = t.Name, desc = ThemeDescription(kind, t.Name) }).ToArray();
        return new
        {
            id,
            name,
            size,
            connected = _app.Screen(kind).IsReady,
            theme,
            themes,
            brightness,
            rotated,
            data,
            dataStyles = data is null ? null : GifTheme.Styles,
            fps,
            video = FileName(video),
            gif = FileName(gif),
            media = kind == ScreenKind.Ds339 ? Array.Empty<object>() : MediaFiles(kind),
        };
    }

    private static string MediaFolder => Path.Combine(AppContext.BaseDirectory, "media");

    private static object[] MediaFiles(ScreenKind kind)
    {
        if (!Directory.Exists(MediaFolder)) return Array.Empty<object>();
        return Directory.EnumerateFiles(MediaFolder)
            .Where(f => Path.GetExtension(f).ToLowerInvariant() is ".gif" || (kind == ScreenKind.Thermalright && Path.GetExtension(f).Equals(VideoTheme.Extension, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
            .Select(f => (object)new
            {
                file = Path.GetFileName(f),
                kind = Path.GetExtension(f).Equals(".gif", StringComparison.OrdinalIgnoreCase) ? "gif" : "video",
                mb = Math.Round(new FileInfo(f).Length / 1048576.0, 1),
            })
            .ToArray();
    }

    private static string? FileName(string? path) => string.IsNullOrEmpty(path) ? null : Path.GetFileName(path);

    // ---------------------------------------------------------------- ao vivo

    private void PushLive()
    {
        if (!_ready || WindowState == FormWindowState.Minimized || _web.CoreWebView2 is null) return;
        _tick++;
        ScreenPipeline[] screens = { _service.Thermalright, _service.ProFlow, _service.Ds339 };
        string?[] images = new string?[3];
        for (int i = 0; i < screens.Length; i++)
        {
            var preview = screens[i].Preview;
            if (preview is null || ReferenceEquals(preview, _sentPreviews[i])) continue;
            _sentPreviews[i] = preview;
            images[i] = "data:image/jpeg;base64," + Convert.ToBase64String(preview);
        }

        var shown = _service.GpuLed.Shown;
        string? led = shown.A == 0 ? null : ToDisplay(shown, _config.LedGpu.Branco, CardWhite);
        bool slow = _tick % 10 == 0;
        var argb = _service.Argb.Snapshot;
        bool argbChanged = !ReferenceEquals(argb, _lastArgb);
        string argbStatus = _service.Argb.Status;
        var fanSnapshot = _service.Fans.Snapshot;
        bool fansChanged = !ReferenceEquals(fanSnapshot, _lastFans);
        _lastFans = fanSnapshot;
        if (images.All(i => i is null) && led == _lastLed && !slow && !argbChanged && !fansChanged && argbStatus == _lastArgbStatus) return;
        _lastLed = led;
        _lastArgb = argb;
        _lastArgbStatus = argbStatus;

        var readings = slow ? _service.Readings.ToDictionary(r => r.Key, r => double.IsNaN(r.Value) ? (double?)null : Math.Round(r.Value, 1)) : null;
        Post(new
        {
            type = "live",
            thermalright = images[0],
            proflow = images[1],
            ds339 = images[2],
            led,
            ledStatus = _service.GpuLed.Status,
            status = slow ? _service.Status : null,
            readings,
            connected = screens.Select(s => s.IsReady).ToArray(),
            argb = argbChanged ? argb ?? Array.Empty<string>() : null,
            argbStatus,
            fans = fansChanged && fanSnapshot is not null ? FanLive(fanSnapshot) : null,
        });
        if (_tick % 20 == 0) PushState(force: false); // mudanças feitas pelo menu da bandeja
    }

    private void Post(object message)
    {
        if (!_ready || _web.CoreWebView2 is null) return;
        _web.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(message));
    }

    /// <summary>
    /// A cor do LED como o olho a vê, para a tela do PC: o valor de cada canal é proporcional à luz (o monitor espera
    /// valores com gama), e o branco "igual ao da placa" (255, 210, 255, que no LED é branco) vira branco neutro.
    /// </summary>
    private Color CardWhite => _cardWhite ??= AsrockGpuLed.SavedColor() ?? Color.White;

    private static string ToDisplay(Color c, string tone, Color white)
    {
        double r = c.R, g = c.G, b = c.B;
        if (tone == GpuEffects.WhiteCard && Math.Min(white.R, Math.Min(white.G, white.B)) >= 150)
        {
            r = r * 255 / white.R;
            g = g * 255 / white.G;
            b = b * 255 / white.B;
        }
        static int Gamma(double v) => (int)Math.Round(255 * Math.Pow(Math.Clamp(v / 255, 0, 1), 1 / 2.2));
        return $"#{Gamma(r):X2}{Gamma(g):X2}{Gamma(b):X2}";
    }

    // ---------------------------------------------------------------- prévias dos efeitos do LED

    /// <summary>
    /// Roda o motor de efeitos do LED fora da placa (12 s de cada efeito, com uma GPU de mentira subindo e descendo e
    /// alguém digitando) e manda as cores para a página animar as amostras.
    /// </summary>
    private void SendEffectPreviews()
    {
        var card = CardWhite;
        var previews = new Dictionary<string, string[]>();
        foreach (string effect in GpuEffects.All)
        {
            var settings = new GpuLedSettings
            {
                Efeito = effect,
                Cor = _config.LedGpu.Cor,
                Brilho = 100,
                Velocidade = _config.LedGpu.Velocidade,
                Branco = _config.LedGpu.Branco,
            };
            var engine = new LedEffectEngine(settings) { SimulatedInput = t => t % 6000 < 2500 ? (uint)(t / 180 * 180) : (uint)(t / 6000 * 6000) };
            engine.Reset(card, card);
            var colors = new string[PreviewSteps];
            for (int i = 0; i < PreviewSteps; i++)
            {
                long t = i * 40L;
                double busy = 0.5 - 0.5 * Math.Cos(2 * Math.PI * t / 12000);
                var color = engine.Next(t, (float)(42 + 38 * busy), (float)(95 * busy), null, out _);
                colors[i] = ToDisplay(color, settings.Branco, card);
            }
            previews[effect] = colors;
        }

        // Fans e fitas: um anel de 12 LEDs em cada amostra, com a cor e a velocidade escolhidas.
        var argb = _config.FansEFitas;
        var argbPreviews = ArgbEffects.All.ToDictionary(effect => effect,
            effect => ArgbLighting.Preview(effect, GpuLighting.ParseColor(argb.Cor), argb.Velocidade, ArgbEffects.IsCable(effect) ? 60 : 12, PreviewSteps, 40));
        AddUmbraPreviews(argbPreviews);
        Post(new { type = "effectPreviews", previews, argbPreviews });
    }

    // ---------------------------------------------------------------- textos

    private static readonly object[] MangaStories =
    {
        new { label = "Gear 5", hint = "jogo abrindo", story = "gear5" },
        new { label = "Print", hint = "カシャ!", story = "print" },
        new { label = "Edição extra", hint = "download", story = "download" },
        new { label = "Copiar", hint = "balão", story = "copiar" },
        new { label = "Pendrive", hint = "tesouro", story = "pendrive" },
        new { label = "Den Den Mushi", hint = "microfone", story = "microfone" },
        new { label = "Música nova", hint = "♪", story = "musica" },
        new { label = "Mudo", hint = "シーン…", story = "mudo" },
        new { label = "Bem-vindo", hint = "おかえり", story = "bemvindo" },
        new { label = "Capítulo", hint = "hora cheia", story = "capitulo" },
        new { label = "GPU quente", hint = "ゴゴゴゴ", story = "ameaca" },
        new { label = "Impacto", hint = "ドン!!", story = "impacto" },
        new { label = "Haki do Rei", hint = "覇", story = "haki" },
        new { label = "Tambores", hint = "Gear 5", story = "tambores" },
        new { label = "Gura Gura", hint = "tremor", story = "tremor" },
        new { label = "Maré", hint = "ザザーン", story = "mare" },
        new { label = "Despedida", hint = "véu branco", story = "despedida" },
        new { label = "Amanhecer", hint = "fachos de luz", story = "amanhecer" },
        new { label = "Virada", hint = "divisa da página", story = "virada" },
        new { label = "Retícula", hint = "onda de pontos", story = "reticula" },
    };

    private static readonly object[] SceneStories =
    {
        new { label = "Knock Up Stream", hint = "jogo abrindo", story = "knockup" },
        new { label = "Volta no balão", hint = "jogo fechou", story = "return" },
        new { label = "Âncora", hint = "dormir", story = "anchor" },
        new { label = "Recolher âncora", hint = "acordar", story = "weigh" },
        new { label = "News Coo", hint = "download", story = "news" },
        new { label = "Cartaz", hint = "print", story = "poster" },
        new { label = "Bolha", hint = "copiar", story = "bubble" },
        new { label = "Brook", hint = "música", story = "brook" },
        new { label = "Baú", hint = "pendrive", story = "chest" },
        new { label = "Den Den Mushi", hint = "microfone", story = "denden" },
        new { label = "Erupção", hint = "GPU quente", story = "eruption" },
        new { label = "Raio", hint = "tempestade", story = "lightning" },
        new { label = "Laboon", hint = "madrugada", story = "laboon" },
    };

    private static string ThemeDescription(ScreenKind kind, string theme) => theme switch
    {
        GrandLineTheme.Name => "O mar do Modo Grand Line, com o Sunny navegando",
        DeepSeaTheme.Name => "O fundo do mar do Modo Grand Line",
        MareTheme.Name => "Painel de sensores desenhado em código",
        VideoTheme.Name => "Um vídeo em tela cheia (MP4 convertido ou .zt do TRCC)",
        OnePieceTheme.Name => "Painéis do mangá com a câmera passeando e legendas",
        GifTheme.Name => "Um GIF animado em tela cheia",
        AuroraTheme.Name => "Aurora animada com os sensores",
        _ => kind == ScreenKind.Ds339 ? "Tema da Jonsbo" : "",
    };

    private static readonly Dictionary<string, string> EffectDescriptions = new()
    {
        [GpuEffects.Off] = "O programa não mexe no LED",
        [GpuEffects.Fixed] = "A cor escolhida, parada",
        [GpuEffects.Breathing] = "A cor escolhida acendendo e apagando",
        [GpuEffects.Rainbow] = "Volta completa nas cores",
        [GpuEffects.Aurora] = "Verde-água, azul, violeta e rosa",
        [GpuEffects.Temperature] = "De azul (frio) a vermelho (quente)",
        [GpuEffects.Load] = "De azul (parada) a vermelho (100%)",
        [GpuEffects.GrandLine] = "A luz do Modo Grand Line",
        [GpuEffects.Page] = "Acompanha a tinta do painel na Pro Flow",
        [GpuEffects.Manga] = "A página + as viradas e os momentos do mangá",
        [GpuEffects.Drums] = "DON do-tto-to: os tambores do Gear 5",
        [GpuEffects.Candle] = "Chama viva com um sopro de vez em quando",
        [GpuEffects.LivingTide] = "Respira em três ondas lentas",
        [GpuEffects.DistantStorm] = "Penumbra fria com um raio ao longe",
        [GpuEffects.Fireflies] = "Escuro e morno, com piscadas soltas",
        [GpuEffects.StarrySky] = "Branco frio cintilando como estrelas",
        [GpuEffects.Clouds] = "Dia claro com nuvens passando devagar",
        [GpuEffects.Drizzle] = "Gotas de tamanhos diferentes",
        [GpuEffects.Fireworks] = "Foguete, estouro dourado ou prateado e faíscas",
        [GpuEffects.Fluorescent] = "Branco de escritório com o reator falhando",
        [GpuEffects.Heartbeat] = "Tum-tum: acelera com a GPU trabalhando",
        [GpuEffects.SleepBreath] = "A luz de repouso dos MacBooks",
        [GpuEffects.GuidedBreath] = "Inspira 4 s, segura 7 s, expira 8 s",
        [GpuEffects.Pomodoro] = "25 min de foco, 5 de pausa morna",
        [GpuEffects.Lighthouse] = "Um lampejo por hora, como um farol",
        [GpuEffects.SunCycle] = "Acompanha o sol de verdade",
        [GpuEffects.SoundPulse] = "Sobe com o volume e bate com a música",
        [GpuEffects.Ember] = "Uma brasa que aviva quando você digita",
        [GpuEffects.Turbine] = "Sobe com o uso; o tom segue a temperatura",
        [GpuEffects.FansScene] = "A placa entra na cena dos fans (Knock Up, Haki, Aquário…)",
    };
}
