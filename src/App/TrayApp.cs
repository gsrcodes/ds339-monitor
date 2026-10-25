using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;
using DS339Monitor.Lighting;
using DS339Monitor.Themes;

namespace DS339Monitor.App;

/// <summary>Ícone na bandeja. O menu é montado na hora de abrir, então não fica nada vivo à toa.</summary>
internal sealed class TrayApp : ApplicationContext
{
    private readonly AppConfig _config;
    private readonly MonitorService _service;
    private readonly NotifyIcon _icon;
    private readonly ContextMenuStrip _menu;
    private readonly System.Windows.Forms.Timer _tooltipTimer;
    private readonly SignalListener? _signals;
    private ControlWindow? _panel;

    public TrayApp(AppConfig config, MonitorService service)
    {
        _config = config;
        _service = service;
        try
        {
            _signals = new SignalListener(); // copiar, prints, pendrive, monitor, app em foco...
        }
        catch (Exception ex)
        {
            Log.Error("Sinais do sistema", ex);
        }
        _menu = new ContextMenuStrip();
        _menu.Opening += (_, e) =>
        {
            BuildMenu();
            e.Cancel = false; // o WinForms já chega com Cancel = true quando o menu estava vazio
        };
        _icon = new NotifyIcon
        {
            Icon = AppIcon.Create(),
            Text = "DS339 Monitor",
            ContextMenuStrip = _menu,
            Visible = true,
        };
        _icon.MouseUp += OnIconClick;

        _tooltipTimer = new System.Windows.Forms.Timer { Interval = 1000 };
        _tooltipTimer.Tick += (_, _) =>
        {
            _icon.Text = Truncate("DS339 — " + _service.Status, 127);
            // "--painel" (ou o programa aberto de novo) pede o painel.
            if (File.Exists(Program.PanelFlagPath))
            {
                string request = "";
                try
                {
                    request = File.ReadAllText(Program.PanelFlagPath).Trim();
                    File.Delete(Program.PanelFlagPath);
                }
                catch (IOException)
                {
                }
                OpenPanel();
                if (request.StartsWith("imagem:", StringComparison.Ordinal)) _panel?.CaptureImage(request["imagem:".Length..]);
            }
            // "--sair" (de outro processo, mesmo sem admin) pede para fechar — usado para atualizar o programa.
            if (File.Exists(Program.ExitFlagPath))
            {
                try { File.Delete(Program.ExitFlagPath); } catch (IOException) { }
                Log.Info("Saindo a pedido de --sair.");
                Exit();
            }
        };
        _tooltipTimer.Start();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _tooltipTimer.Dispose();
            _signals?.Dispose();
            _icon.Visible = false;
            _icon.Icon?.Dispose();
            _icon.Dispose();
            _menu.Dispose();
        }
        base.Dispose(disposing);
    }

    /// <summary>Clique esquerdo abre o painel; o direito, o menu rápido.</summary>
    private void OnIconClick(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left) OpenPanel();
    }

    /// <summary>Abre (ou traz para a frente) o painel de controle.</summary>
    internal void OpenPanel()
    {
        if (_panel is { IsDisposed: false })
        {
            if (_panel.WindowState == FormWindowState.Minimized) _panel.WindowState = FormWindowState.Normal;
            _panel.Activate();
            return;
        }
        _panel = new ControlWindow(this, _config, _service);
        _panel.FormClosed += (_, _) => _panel = null;
        _panel.Show();
    }

    /// <summary>Aviso rápido: no painel, se ele estiver aberto; senão, um balão da bandeja.</summary>
    internal void Notify(string text, bool warning = false)
    {
        if (_panel is { IsDisposed: false } panel && panel.Visible) panel.Toast(text, warning);
        else _icon.ShowBalloonTip(4000, "DS339 Monitor", text, warning ? ToolTipIcon.Warning : ToolTipIcon.Info);
    }

    // ---------------------------------------------------------------- ações do painel (mesma lógica do menu)

    internal ScreenPipeline Screen(ScreenKind kind) => kind switch
    {
        ScreenKind.Thermalright => _service.Thermalright,
        ScreenKind.ProFlow => _service.ProFlow,
        _ => _service.Ds339,
    };

    internal void SetTheme(ScreenKind kind, string name)
    {
        switch (kind)
        {
            case ScreenKind.Thermalright: _config.Thermalright.Tema = name; break;
            case ScreenKind.ProFlow: _config.ProFlow.Tema = name; break;
            default: _config.DS339.Tema = name; break;
        }
        Apply(() => Screen(kind).SelectTheme(name));
        if (kind != ScreenKind.Ds339 && name.Equals(OnePieceTheme.Name, StringComparison.OrdinalIgnoreCase) && !OnePieceTheme.HasPanels())
            OpenOnePieceDownloads();
    }

    internal void SetBrightness(ScreenKind kind, int level)
    {
        level = Math.Clamp(level, 5, 100);
        switch (kind)
        {
            case ScreenKind.Thermalright: _config.Thermalright.Brilho = level; break;
            case ScreenKind.ProFlow: _config.ProFlow.Brilho = level; break;
            default: _config.DS339.Brilho = level; break;
        }
        Apply(Screen(kind).Invalidate);
    }

    /// <summary>"Girar 180°" em relação ao normal de cada tela.</summary>
    internal void SetRotated(ScreenKind kind, bool rotated)
    {
        switch (kind)
        {
            case ScreenKind.Thermalright: _config.Thermalright.Rotacao = rotated ? 0 : 180; break;
            case ScreenKind.ProFlow: _config.ProFlow.Rotacao = rotated ? 0 : 180; break;
            default: _config.DS339.Rotacao = rotated ? 270 : 90; break;
        }
        Apply(Screen(kind).Invalidate);
    }

    internal void SetDataStyle(ScreenKind kind, string style)
    {
        if (!GifTheme.Styles.Contains(style)) return;
        if (kind == ScreenKind.Thermalright) _config.Thermalright.Dados = style;
        else if (kind == ScreenKind.ProFlow) _config.ProFlow.Dados = style;
        else return;
        Apply(Screen(kind).Reload);
    }

    internal void SetFps(ScreenKind kind, int fps)
    {
        fps = Math.Clamp(fps, 1, 30);
        if (kind == ScreenKind.Thermalright) _config.Thermalright.FpsMax = fps;
        else if (kind == ScreenKind.ProFlow) _config.ProFlow.FpsMax = fps;
        else return;
        Apply(() => { });
    }

    /// <summary>Usa um arquivo da biblioteca (pasta media): .zt vira o vídeo da Thermalright; .gif, o GIF da tela.</summary>
    internal void UseMedia(ScreenKind kind, string path)
    {
        if (!File.Exists(path)) return;
        bool video = Path.GetExtension(path).Equals(VideoTheme.Extension, StringComparison.OrdinalIgnoreCase);
        if (kind == ScreenKind.Thermalright && video)
        {
            UseVideo(path);
        }
        else if (kind == ScreenKind.Thermalright)
        {
            _config.Thermalright.Gif = path;
            _config.Thermalright.Tema = GifTheme.Name;
            Apply(() => _service.Thermalright.SelectTheme(GifTheme.Name));
        }
        else if (kind == ScreenKind.ProFlow && !video)
        {
            _config.ProFlow.Gif = path;
            _config.ProFlow.Tema = GifTheme.Name;
            Apply(() => _service.ProFlow.SelectTheme(GifTheme.Name));
        }
    }

    internal void ChooseGifFor(ScreenKind kind)
    {
        if (kind == ScreenKind.ProFlow)
        {
            ChooseGif();
            return;
        }
        ChooseGif("a Thermalright", gif => UseMedia(ScreenKind.Thermalright, gif));
    }

    internal void SetLedEffect(string effect)
    {
        if (!GpuEffects.All.Contains(effect)) return;
        _config.LedGpu.Efeito = effect;
        ApplyLed();
    }

    internal void SetLedTone(string tone)
    {
        if (!GpuEffects.WhiteTones.Contains(tone)) return;
        _config.LedGpu.Branco = tone;
        ApplyLed();
    }

    internal void SetLedSpeed(string speed)
    {
        if (!GpuEffects.Speeds.Contains(speed)) return;
        _config.LedGpu.Velocidade = speed;
        ApplyLed();
    }

    internal void SetLedBrightness(int level)
    {
        _config.LedGpu.Brilho = Math.Clamp(level, 5, 100);
        ApplyLed();
    }

    /// <summary>Uma cor escolhida num efeito que não usa cor quer dizer "quero essa cor" (como no menu).</summary>
    internal void SetLedColor(string hex)
    {
        _config.LedGpu.Cor = GpuLighting.ToHex(GpuLighting.ParseColor(hex));
        if (!GpuEffects.UsesColor(_config.LedGpu.Efeito)) _config.LedGpu.Efeito = GpuEffects.Fixed;
        ApplyLed();
    }

    internal void SetMangaOption(string option, bool on)
    {
        if (option == "capitulos") _config.Manga.CapitulosNaHora = on;
        else if (option == "acoes") _config.Manga.AcoesDoPc = on;
        else return;
        _config.Save();
    }

    internal void ChooseGear5Gif() => ChooseGif("o despertar do Gear 5", gif =>
    {
        _config.Manga.GifGear5 = gif;
        _config.Save();
        Manga.MangaLayer.Preload(gif);
    });

    private void BuildMenu()
    {
        foreach (ToolStripItem old in _menu.Items.Cast<ToolStripItem>().ToArray()) old.Dispose();
        _menu.Items.Clear();

        _menu.Items.Add(new ToolStripMenuItem(_service.Status) { Enabled = false });
        _menu.Items.Add(new ToolStripSeparator());
        var panel = new ToolStripMenuItem("Abrir painel…") { Font = new Font(_menu.Font, FontStyle.Bold) };
        panel.Click += (_, _) => OpenPanel();
        _menu.Items.Add(panel);
        var allOff = new ToolStripMenuItem(_service.AllOff ? "Religar tudo" : "Desligar tudo (telas, LED e SignalRGB)")
        {
            Font = new Font(_menu.Font, FontStyle.Bold),
            ToolTipText = _service.AllOff
                ? "Acende as telas e o LED e reabre o SignalRGB (se foi o programa que fechou)"
                : "Apaga as duas telas e o LED da placa e fecha o SignalRGB (os fans e fitas apagam junto)",
        };
        allOff.Click += (_, _) => _service.SetAllOff(!_service.AllOff);
        _menu.Items.Add(allOff);
        _menu.Items.Add(new ToolStripSeparator());
        allOff.Font = _menu.Font;
        var manga = new ToolStripMenuItem("Modo Mangá (efeitos nas telas e no LED)") { Checked = _config.Manga.Ligado };
        manga.Click += (_, _) => ToggleManga(!_config.Manga.Ligado);
        _menu.Items.Add(manga);
        if (_config.Manga.Ligado) _menu.Items.Add(MangaTestMenu());
        var grandLine = new ToolStripMenuItem("Modo Grand Line (telas e LED juntos)") { Checked = _config.GrandLine };
        grandLine.Click += (_, _) => ToggleGrandLine(!_config.GrandLine);
        _menu.Items.Add(grandLine);
        if (_config.UsesScene && !_config.Manga.Ligado) _menu.Items.Add(SceneTestMenu());
        _menu.Items.Add(new ToolStripSeparator());
        _menu.Items.Add(ThermalrightMenu());
        _menu.Items.Add(ProFlowMenu());
        _menu.Items.Add(Ds339Menu());
        _menu.Items.Add(GpuLedMenu());
        _menu.Items.Add(new ToolStripSeparator());

        var autostart = new ToolStripMenuItem("Iniciar com o Windows") { Checked = Autostart.IsEnabled() };
        autostart.Click += (_, _) => ToggleAutostart(!autostart.Checked);
        _menu.Items.Add(autostart);

        if (!Environment.IsPrivilegedProcess)
        {
            var admin = new ToolStripMenuItem("Reiniciar como administrador")
            {
                ToolTipText = "Necessário para temperatura, clock e potência da CPU",
            };
            admin.Click += (_, _) => RestartElevated();
            _menu.Items.Add(admin);
        }

        _menu.Items.Add("Salvar imagem das telas", null, (_, _) => SaveScreenImages());
        _menu.Items.Add("Abrir log", null, (_, _) => OpenFile(Log.FilePath));
        _menu.Items.Add(new ToolStripSeparator());
        _menu.Items.Add("Sair", null, (_, _) => Exit());
    }

    private ToolStripMenuItem Ds339Menu()
    {
        var settings = _config.DS339;
        var screen = _service.Ds339;
        var root = new ToolStripMenuItem($"DS339 — {Describe(screen)}");

        var themes = ThemeMenu(ScreenKind.Ds339, settings.Tema, name =>
        {
            settings.Tema = name;
            Apply(() => screen.SelectTheme(name));
        });
        themes.DropDownItems.Add(new ToolStripSeparator());
        themes.DropDownItems.Add("Abrir pasta de temas", null, (_, _) => OpenFolder(ThemeCatalog.LocalThemesDirectory));
        root.DropDownItems.Add(themes);
        root.DropDownItems.Add(BrightnessMenu(settings.Brilho, level =>
        {
            settings.Brilho = level;
            Apply(screen.Invalidate);
        }));

        root.DropDownItems.Add(Toggle("Girar 180°", settings.Rotacao == 270, () =>
        {
            settings.Rotacao = settings.Rotacao == 270 ? 90 : 270;
            Apply(screen.Invalidate);
        }));
        return root;
    }

    private ToolStripMenuItem ThermalrightMenu()
    {
        var settings = _config.Thermalright;
        var screen = _service.Thermalright;
        var root = new ToolStripMenuItem($"Thermalright 9,16\" — {Describe(screen)}");

        root.DropDownItems.Add(ThemeMenu(ScreenKind.Thermalright, settings.Tema, name =>
        {
            settings.Tema = name;
            Apply(() => screen.SelectTheme(name));
            if (name.Equals(OnePieceTheme.Name, StringComparison.OrdinalIgnoreCase) && !OnePieceTheme.HasPanels())
                OpenOnePieceDownloads();
        }));
        root.DropDownItems.Add("Escolher vídeo…", null, (_, _) => ChooseVideo());
        root.DropDownItems.Add("Escolher GIF…", null, (_, _) => ChooseGif("a Thermalright", gif =>
        {
            settings.Gif = gif;
            settings.Tema = GifTheme.Name;
            Apply(() => screen.SelectTheme(GifTheme.Name));
        }));

        var data = new ToolStripMenuItem("Dados por cima (vídeo, GIF e One Piece)");
        foreach (var style in GifTheme.Styles)
        {
            data.DropDownItems.Add(Toggle(style, style.Equals(settings.Dados, StringComparison.OrdinalIgnoreCase), () =>
            {
                settings.Dados = style;
                Apply(() =>
                {
                    if (!settings.Tema.Equals(MareTheme.Name, StringComparison.OrdinalIgnoreCase)) screen.Reload();
                });
            }));
        }
        root.DropDownItems.Add(data);

        root.DropDownItems.Add(BrightnessMenu(settings.Brilho, level =>
        {
            settings.Brilho = level;
            Apply(screen.Invalidate);
        }));
        // 180° é o normal desta tela (como o TRCC manda); marcado = invertido em relação a isso.
        root.DropDownItems.Add(Toggle("Girar 180°", settings.Rotacao == 0, () =>
        {
            settings.Rotacao = settings.Rotacao == 0 ? 180 : 0;
            Apply(screen.Invalidate);
        }));
        return root;
    }

    private ToolStripMenuItem ProFlowMenu()
    {
        var settings = _config.ProFlow;
        var screen = _service.ProFlow;
        var root = new ToolStripMenuItem($"Pro Flow 360 — {Describe(screen)}");

        root.DropDownItems.Add(ThemeMenu(ScreenKind.ProFlow, settings.Tema, name =>
        {
            settings.Tema = name;
            Apply(() => screen.SelectTheme(name));
            // Sem os painéis ainda, a tela mostra as instruções — e a página de download já abre.
            if (name.Equals(OnePieceTheme.Name, StringComparison.OrdinalIgnoreCase) && !OnePieceTheme.HasPanels())
                OpenOnePieceDownloads();
        }));
        root.DropDownItems.Add("Escolher GIF…", null, (_, _) => ChooseGif());
        root.DropDownItems.Add("Baixar painéis do One Piece…", null, (_, _) => OpenOnePieceDownloads());

        var data = new ToolStripMenuItem("Dados por cima (GIF e One Piece)");
        foreach (var style in GifTheme.Styles)
        {
            data.DropDownItems.Add(Toggle(style, style.Equals(settings.Dados, StringComparison.OrdinalIgnoreCase), () =>
            {
                settings.Dados = style;
                Apply(() =>
                {
                    if (settings.Tema.Equals(GifTheme.Name, StringComparison.OrdinalIgnoreCase)
                        || settings.Tema.Equals(OnePieceTheme.Name, StringComparison.OrdinalIgnoreCase))
                        screen.Reload();
                });
            }));
        }
        root.DropDownItems.Add(data);

        root.DropDownItems.Add(BrightnessMenu(settings.Brilho, level =>
        {
            settings.Brilho = level;
            Apply(screen.Invalidate);
        }));

        // 180° é o "normal" desta montagem (veio do Jungle Leopard); marcado = invertido em relação a isso.
        root.DropDownItems.Add(Toggle("Girar 180°", settings.Rotacao == 0, () =>
        {
            settings.Rotacao = settings.Rotacao == 0 ? 180 : 0;
            Apply(screen.Invalidate);
        }));
        return root;
    }

    /// <summary>
    /// Liga o Modo Grand Line (Thermalright no Grand Line, Pro Flow no One Piece, LED no Grand Line), guardando o que
    /// estava escolhido; desligar devolve isso.
    /// </summary>
    internal void ToggleGrandLine(bool on)
    {
        var scene = _config.Cena;
        if (on && _config.Manga.Ligado) ToggleManga(false);
        if (on)
        {
            scene.AntesThermalright = _config.Thermalright.Tema;
            scene.AntesProFlow = _config.ProFlow.Tema;
            scene.AntesLed = _config.LedGpu.Efeito;
            _config.Thermalright.Tema = GrandLineTheme.Name;
            _config.ProFlow.Tema = DeepSeaTheme.Name;
            _config.LedGpu.Efeito = GpuEffects.GrandLine;
        }
        else
        {
            _config.Thermalright.Tema = scene.AntesThermalright is { } t && t != GrandLineTheme.Name ? t : VideoTheme.Name;
            _config.ProFlow.Tema = scene.AntesProFlow is { } p && p != DeepSeaTheme.Name ? p : OnePieceTheme.Name;
            _config.LedGpu.Efeito = scene.AntesLed is { } led && led != GpuEffects.GrandLine ? led : GpuEffects.Off;
        }
        _config.Save();
        _service.Thermalright.SelectTheme(_config.Thermalright.Tema);
        _service.ProFlow.SelectTheme(_config.ProFlow.Tema);
        _service.GpuLed.Reload();
        _service.Wake();
        if (on) Notify("Zarpando! O gabinete virou um mar só: céu na Thermalright, fundo do mar na Pro Flow e a luz das profundezas na placa.");
    }

    /// <summary>
    /// Liga o Modo Mangá: a Thermalright fica no vídeo (ou GIF / One Piece, se já estava), a Pro Flow no One Piece e o
    /// LED no "Mangá narrado", e os efeitos entram por cima. Desligar devolve o que estava escolhido.
    /// </summary>
    internal void ToggleManga(bool on)
    {
        var manga = _config.Manga;
        if (on)
        {
            if (_config.GrandLine) ToggleGrandLine(false);
            manga.AntesThermalright = _config.Thermalright.Tema;
            manga.AntesProFlow = _config.ProFlow.Tema;
            manga.AntesLed = _config.LedGpu.Efeito;
            if (_config.Thermalright.Tema is not (VideoTheme.Name or GifTheme.Name or OnePieceTheme.Name))
                _config.Thermalright.Tema = !string.IsNullOrEmpty(_config.Thermalright.Video) ? VideoTheme.Name : GifTheme.Name;
            if (_config.ProFlow.Tema is not (OnePieceTheme.Name or GifTheme.Name)) _config.ProFlow.Tema = OnePieceTheme.Name;
            _config.LedGpu.Efeito = GpuEffects.Manga;
            manga.Ligado = true;
        }
        else
        {
            manga.Ligado = false;
            if (manga.AntesThermalright is { } t && t != GrandLineTheme.Name) _config.Thermalright.Tema = t;
            if (manga.AntesProFlow is { } p && p != DeepSeaTheme.Name) _config.ProFlow.Tema = p;
            _config.LedGpu.Efeito = manga.AntesLed is { } led && led != GpuEffects.GrandLine ? led : GpuEffects.Page;
        }
        _config.Save();
        _service.Thermalright.SelectTheme(_config.Thermalright.Tema);
        _service.ProFlow.SelectTheme(_config.ProFlow.Tema);
        _service.GpuLed.Reload();
        _service.Wake();
        if (on) Notify("ドン!! O vídeo, o mangá e o LED agora reagem juntos ao mangá da Pro Flow e ao que você faz no PC.");
    }

    /// <summary>Dispara na hora cada efeito do Modo Mangá.</summary>
    private ToolStripMenuItem MangaTestMenu()
    {
        var menu = new ToolStripMenuItem("Testar Modo Mangá");
        void Add(string label, string story) => menu.DropDownItems.Add(label, null, (_, _) => _service.TriggerStory(story));
        Add("Gear 5 (jogo abrindo)", "gear5");
        Add("Print (カシャ!)", "print");
        Add("Edição extra (download concluído)", "download");
        Add("Copiar", "copiar");
        Add("Pendrive (tesouro encontrado)", "pendrive");
        Add("Den Den Mushi (microfone)", "microfone");
        Add("Música nova", "musica");
        Add("Mudo (シーン…)", "mudo");
        Add("Bem-vindo de volta", "bemvindo");
        Add("Página de título (hora cheia)", "capitulo");
        Add("GPU esquentando (ゴゴゴゴ)", "ameaca");
        menu.DropDownItems.Add(new ToolStripSeparator());
        Add("Impacto (ドン!!)", "impacto");
        Add("Haki do Rei (覇)", "haki");
        Add("Tambores da Libertação", "tambores");
        Add("Gura Gura (tremor)", "tremor");
        Add("Maré", "mare");
        Add("Despedida", "despedida");
        Add("Amanhecer", "amanhecer");
        Add("Virada de página", "virada");
        Add("Virada em retícula", "reticula");
        return menu;
    }

    /// <summary>Dispara na hora cada história do mundo, para ver como ela fica nas telas e no LED.</summary>
    private ToolStripMenuItem SceneTestMenu()
    {
        var menu = new ToolStripMenuItem("Testar cena");
        void Add(string label, string story) => menu.DropDownItems.Add(label, null, (_, _) => _service.TriggerStory(story));
        Add("Knock Up Stream (jogo abrindo)", "knockup");
        Add("Volta no balão-polvo (jogo fechou)", "return");
        Add("Âncora: dormir", "anchor");
        Add("Âncora: acordar", "weigh");
        menu.DropDownItems.Add(new ToolStripSeparator());
        Add("News Coo (download concluído)", "news");
        Add("Cartaz de procurado (print)", "poster");
        Add("Bolha (copiar)", "bubble");
        Add("Brook (música nova)", "brook");
        Add("Baú do tesouro (pendrive)", "chest");
        Add("Den Den Mushi (microfone)", "denden");
        Add("Erupção (GPU quente)", "eruption");
        Add("Raio que afunda", "lightning");
        menu.DropDownItems.Add(new ToolStripSeparator());
        Add("Gaivota mergulhando", "gull");
        Add("Estrela cadente", "star");
        Add("Sino da hora", "bell");
        Add("Laboon", "laboon");
        Add("Mudo (シーン)", "mute");
        return menu;
    }

    /// <summary>LED da placa de vídeo: efeito, cor, tom do branco, brilho e velocidade. O título mostra a situação.</summary>
    private ToolStripMenuItem GpuLedMenu()
    {
        var settings = _config.LedGpu;
        string status = _service.GpuLed.Status;
        var root = new ToolStripMenuItem($"LED da placa de vídeo — {status}");

        var effects = new ToolStripMenuItem("Efeito");
        foreach (var group in GpuEffects.Groups)
        {
            if (effects.DropDownItems.Count > 0) effects.DropDownItems.Add(new ToolStripSeparator());
            foreach (string effect in group)
            {
                effects.DropDownItems.Add(Toggle(effect, effect == settings.Efeito, () =>
                {
                    settings.Efeito = effect;
                    ApplyLed();
                }));
            }
        }
        root.DropDownItems.Add(effects);
        root.DropDownItems.Add("Escolher cor…", null, (_, _) => ChooseLedColor());

        var white = new ToolStripMenuItem("Tom do branco") { Enabled = GpuEffects.UsesWhite(settings.Efeito) };
        foreach (string tone in GpuEffects.WhiteTones)
        {
            white.DropDownItems.Add(Toggle(tone, tone == settings.Branco, () =>
            {
                settings.Branco = tone;
                ApplyLed();
            }));
        }
        root.DropDownItems.Add(white);
        root.DropDownItems.Add(BrightnessMenu(settings.Brilho, level =>
        {
            settings.Brilho = level;
            ApplyLed();
        }));

        var speed = new ToolStripMenuItem("Velocidade") { Enabled = GpuEffects.UsesSpeed(settings.Efeito) };
        foreach (string option in GpuEffects.Speeds)
        {
            speed.DropDownItems.Add(Toggle(option, option == settings.Velocidade, () =>
            {
                settings.Velocidade = option;
                ApplyLed();
            }));
        }
        root.DropDownItems.Add(speed);

        if (status.StartsWith("pausado (outro programa", StringComparison.Ordinal))
            root.DropDownItems.Add(new ToolStripMenuItem("No SignalRGB, desative a placa de vídeo (volta sozinho em 1 min)") { Enabled = false });
        else if (status.StartsWith("pausado", StringComparison.Ordinal))
            root.DropDownItems.Add(new ToolStripMenuItem("Feche o outro programa de RGB para o efeito voltar") { Enabled = false });
        else if (status.StartsWith("não suportado", StringComparison.Ordinal) || status.StartsWith("desligado (", StringComparison.Ordinal))
            root.DropDownItems.Add(new ToolStripMenuItem("Detalhes no log") { Enabled = false });
        return root;
    }

    internal void ChooseLedColor()
    {
        using var dialog = new ColorDialog
        {
            FullOpen = true,
            AnyColor = true,
            Color = GpuLighting.ParseColor(_config.LedGpu.Cor),
        };
        if (dialog.ShowDialog() != DialogResult.OK) return;
        _config.LedGpu.Cor = GpuLighting.ToHex(dialog.Color);
        // Escolher uma cor num efeito que não usa cor (ou com o LED desligado) quer dizer "quero essa cor".
        if (!GpuEffects.UsesColor(_config.LedGpu.Efeito)) _config.LedGpu.Efeito = GpuEffects.Fixed;
        ApplyLed();
    }

    internal void ApplyLed()
    {
        _config.Save();
        _service.GpuLed.Reload();
        _service.Wake(); // os sensores lidos podem ter mudado (efeitos de temperatura e uso)
    }

    /// <summary>Níveis fixos + o atual (se foi editado à mão no config). O brilho é aplicado na imagem: reenvia já.</summary>
    private static ToolStripMenuItem BrightnessMenu(int current, Action<int> select)
    {
        var menu = new ToolStripMenuItem("Brilho");
        foreach (int level in new[] { 10, 25, 50, 75, 100 }.Append(current).Distinct().Order())
            menu.DropDownItems.Add(Toggle($"{level}%", current == level, () => select(level)));
        return menu;
    }

    private ToolStripMenuItem ThemeMenu(ScreenKind kind, string current, Action<string> select)
    {
        var menu = new ToolStripMenuItem("Tema");
        foreach (var theme in ThemeCatalog.Discover(kind))
        {
            string name = theme.Name;
            menu.DropDownItems.Add(Toggle(name, name.Equals(current, StringComparison.OrdinalIgnoreCase), () => select(name)));
        }
        return menu;
    }

    /// <summary>Abre no navegador a página que baixa os painéis (o servidor da wiki só entrega imagens para navegadores).</summary>
    internal void OpenOnePieceDownloads()
    {
        try
        {
            OpenFile(OnePieceTheme.WriteDownloadPage());
        }
        catch (Exception ex)
        {
            Log.Error("Abrindo a página de download do One Piece", ex);
            Notify($"Não consegui abrir a página: {ex.Message}", warning: true);
        }
    }

    internal void ChooseGif() => ChooseGif("o Pro Flow 360", gif =>
    {
        _config.ProFlow.Gif = gif;
        _config.ProFlow.Tema = GifTheme.Name;
        Apply(() => _service.ProFlow.SelectTheme(GifTheme.Name));
    });

    /// <summary>
    /// Vídeo para a Thermalright: um .zt (formato do TRCC) é só copiado; MP4/MOV/MKV/AVI/WMV são convertidos em segundo
    /// plano pelo Media Foundation do Windows (alguns segundos) e o tema troca sozinho quando termina.
    /// </summary>
    internal void ChooseVideo()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Escolha um vídeo para a Thermalright",
            Filter = "Vídeo (*.mp4;*.mov;*.mkv;*.avi;*.wmv;*.webm;*.zt)|*.mp4;*.mov;*.mkv;*.avi;*.wmv;*.webm;*.zt",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
        };
        if (dialog.ShowDialog() != DialogResult.OK) return;

        string input = dialog.FileName;
        string media = Path.Combine(AppContext.BaseDirectory, "media");
        Directory.CreateDirectory(media);
        string target = Path.Combine(media, Path.GetFileNameWithoutExtension(input) + VideoTheme.Extension);
        for (int n = 2; File.Exists(target) && !Path.GetFullPath(target).Equals(Path.GetFullPath(input), StringComparison.OrdinalIgnoreCase); n++)
            target = Path.Combine(media, $"{Path.GetFileNameWithoutExtension(input)} ({n}){VideoTheme.Extension}");

        if (Path.GetExtension(input).Equals(VideoTheme.Extension, StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                if (!Path.GetFullPath(input).Equals(Path.GetFullPath(target), StringComparison.OrdinalIgnoreCase)) File.Copy(input, target);
                UseVideo(target);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                Log.Error("Copiando vídeo", ex);
                Notify($"Não consegui usar esse vídeo: {ex.Message}", warning: true);
            }
            return;
        }

        Notify($"Convertendo {Path.GetFileName(input)} para a Thermalright…");
        var ui = SynchronizationContext.Current;
        Task.Run(() =>
        {
            try
            {
                var timer = Stopwatch.StartNew();
                int frames = VideoImport.Convert(input, target, Display.ThermalrightDisplay.Width, Display.ThermalrightDisplay.Height);
                Log.Info($"Vídeo convertido: {Path.GetFileName(input)} → {Path.GetFileName(target)}, {frames} quadros em {timer.Elapsed.TotalSeconds:0.0} s");
                ui?.Post(_ => UseVideo(target), null);
            }
            catch (Exception ex)
            {
                Log.Error($"Convertendo {input}", ex);
                ui?.Post(_ => Notify($"Não consegui converter o vídeo: {ex.Message}", warning: true), null);
            }
        });
    }

    internal void UseVideo(string path)
    {
        _config.Thermalright.Video = path;
        _config.Thermalright.Tema = VideoTheme.Name;
        Apply(() => _service.Thermalright.SelectTheme(VideoTheme.Name));
    }

    /// <summary>Escolhe um GIF, copia para a pasta media/ do programa e entrega o caminho da cópia.</summary>
    internal void ChooseGif(string screenName, Action<string> use)
    {
        using var dialog = new OpenFileDialog
        {
            Title = $"Escolha um GIF para {screenName}",
            Filter = "GIF animado (*.gif)|*.gif",
            InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
        };
        if (dialog.ShowDialog() != DialogResult.OK) return;

        try
        {
            // Copia para a pasta do programa: o tema continua funcionando mesmo se o original for apagado.
            string media = Path.Combine(AppContext.BaseDirectory, "media");
            Directory.CreateDirectory(media);
            string target = Path.Combine(media, Path.GetFileName(dialog.FileName));
            if (!string.Equals(Path.GetFullPath(dialog.FileName), Path.GetFullPath(target), StringComparison.OrdinalIgnoreCase))
                File.Copy(dialog.FileName, target, overwrite: true);
            use(target);
        }
        catch (Exception ex)
        {
            Log.Error("Escolhendo GIF", ex);
            Notify($"Não consegui usar esse GIF: {ex.Message}", warning: true);
        }
    }

    /// <summary>Salva a configuração, avisa a tela e acorda a thread para aplicar já.</summary>
    internal void Apply(Action notify)
    {
        _config.Save();
        notify();
        _service.Wake();
    }

    private static ToolStripMenuItem Toggle(string text, bool isChecked, Action onClick)
    {
        var item = new ToolStripMenuItem(text) { Checked = isChecked };
        item.Click += (_, _) => onClick();
        return item;
    }

    private static string Describe(ScreenPipeline screen) => screen.IsReady ? "conectada" : "desconectada";

    internal void ToggleAutostart(bool enable)
    {
        bool ok = Autostart.SetFromUi(enable);
        string message = !ok
            ? "Não foi possível alterar a inicialização automática (veja o log)."
            : enable
                ? "O DS339 Monitor vai abrir sozinho no logon, já como administrador."
                : "Inicialização automática desativada.";
        Notify(message, warning: !ok);
    }

    internal void SaveScreenImages()
    {
        var paths = new[] { MonitorService.CapturePath(ScreenKind.Ds339), MonitorService.CapturePath(ScreenKind.ProFlow) };
        var before = paths.Select(p => File.Exists(p) ? File.GetLastWriteTimeUtc(p) : DateTime.MinValue).ToArray();
        _service.RequestCapture();
        // A captura acontece no próximo ciclo da thread de monitoramento.
        var timer = new System.Windows.Forms.Timer { Interval = 1500 };
        timer.Tick += (_, _) =>
        {
            timer.Dispose();
            bool any = false;
            for (int i = 0; i < paths.Length; i++)
            {
                if (!File.Exists(paths[i]) || File.GetLastWriteTimeUtc(paths[i]) <= before[i]) continue;
                OpenFile(paths[i]);
                any = true;
            }
            if (!any) Notify("Nenhuma tela conectada para salvar a imagem.", warning: true);
        };
        timer.Start();
    }

    internal void RestartElevated()
    {
        try
        {
            // Abre a cópia elevada primeiro: se o UAC for cancelado, esta continua rodando normalmente.
            // A nova instância espera esta liberar a tela (mutex) antes de começar.
            Process.Start(new ProcessStartInfo(Environment.ProcessPath!, Program.WaitFlag) { UseShellExecute = true, Verb = "runas" })?.Dispose();
            Exit();
        }
        catch (System.ComponentModel.Win32Exception)
        {
            // UAC cancelado.
        }
    }

    internal void Exit()
    {
        var timer = Stopwatch.StartNew();
        _tooltipTimer.Stop();
        _icon.Visible = false;
        // O painel só some: o navegador dele termina sozinho junto com o programa (desmontá-lo aqui demora).
        if (_panel is { IsDisposed: false } panel)
        {
            panel.PrepareForExit();
            panel.Hide();
        }
        _service.Stop();
        Log.Info($"Saída: telas, LED e cena parados em {timer.ElapsedMilliseconds} ms.");
        ExitThread();
    }

    private static void OpenFolder(string path)
    {
        Directory.CreateDirectory(path);
        Process.Start(new ProcessStartInfo("explorer.exe", $"\"{path}\"") { UseShellExecute = true })?.Dispose();
    }

    /// <summary>
    /// Abre com o programa padrão. Rodando como admin, passa pelo Explorer: ele repassa ao shell do usuário (não
    /// elevado), e o navegador/visualizador não herda o privilégio de administrador.
    /// </summary>
    public static void OpenFile(string? path)
    {
        if (path is null || !File.Exists(path)) return;
        var start = Environment.IsPrivilegedProcess
            ? new ProcessStartInfo("explorer.exe", $"\"{path}\"") { UseShellExecute = false }
            : new ProcessStartInfo(path) { UseShellExecute = true };
        Process.Start(start)?.Dispose();
    }

    private static string Truncate(string text, int max) => text.Length <= max ? text : text[..(max - 1)] + "…";
}

/// <summary>Ícone desenhado em código: uma telinha deitada com três barras (CPU, GPU, RAM).</summary>
internal static class AppIcon
{
    /// <summary>O ícone do programa ("!!" de mangá), com todos os tamanhos — o mesmo do .exe e dos atalhos.</summary>
    public static string FilePath => Path.Combine(AppContext.BaseDirectory, "app.ico");

    /// <summary>Ícone completo (a janela escolhe o tamanho de cada lugar: barra de título, barra de tarefas, Alt+Tab).</summary>
    public static Icon Load() => File.Exists(FilePath) ? new Icon(FilePath) : Create();

    /// <summary>Ícone pequeno, para a bandeja.</summary>
    public static Icon Create()
    {
        int size = Math.Max(16, SystemInformation.SmallIconSize.Width);
        if (File.Exists(FilePath))
        {
            try
            {
                return new Icon(FilePath, size, size);
            }
            catch (Exception ex) when (ex is ArgumentException or IOException)
            {
                // Arquivo estragado: desenha o antigo.
            }
        }
        using var bitmap = new Bitmap(size, size, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);
            float u = size / 16f;

            using (var frame = Shapes.RoundedRect(new RectangleF(0.5f * u, 2.5f * u, 15 * u, 11 * u), 2.5f * u))
            using (var fill = new SolidBrush(Color.FromArgb(0x14, 0x1B, 0x29)))
            using (var edge = new Pen(Color.FromArgb(0x94, 0xA3, 0xB8), Math.Max(1, u)))
            {
                g.FillPath(fill, frame);
                g.DrawPath(edge, frame);
            }

            var bars = new[]
            {
                (X: 3.5f, H: 6f, Color: Color.FromArgb(0x38, 0xBD, 0xF8)),
                (X: 7f, H: 4f, Color: Color.FromArgb(0xFB, 0x92, 0x3C)),
                (X: 10.5f, H: 7.5f, Color: Color.FromArgb(0xA7, 0x8B, 0xFA)),
            };
            foreach (var bar in bars)
            {
                using var brush = new SolidBrush(bar.Color);
                g.FillRectangle(brush, bar.X * u, (11.5f - bar.H) * u, 2.5f * u, bar.H * u);
            }
        }

        IntPtr handle = bitmap.GetHicon();
        try
        {
            using var temporary = Icon.FromHandle(handle);
            return (Icon)temporary.Clone(); // cópia dona do próprio handle
        }
        finally
        {
            DestroyIcon(handle);
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyIcon(IntPtr handle);
}
