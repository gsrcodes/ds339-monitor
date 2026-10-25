using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using DS339Monitor.Display;
using DS339Monitor.Lighting;
using DS339Monitor.Manga;
using DS339Monitor.Render;
using DS339Monitor.Sensors;
using DS339Monitor.Themes;

namespace DS339Monitor.App;

/// <summary>
/// Uma tela física: tema, renderizador, ritmo de quadros e envio. Tudo roda na thread de monitoramento;
/// da interface só chegam <see cref="SelectTheme"/>, <see cref="Reload"/> e <see cref="Invalidate"/>.
/// </summary>
internal abstract class ScreenPipeline : IDisposable
{
    /// <summary>Reenvia o quadro mesmo sem mudança de vez em quando, por garantia.</summary>
    private const long KeepAliveFrameMs = 10_000;

    protected readonly AppConfig Config;
    private volatile string _wantedTheme;
    private int _version;
    private int _loadedVersion = -1;
    private Theme? _theme;
    private ThemeRenderer? _renderer;
    private long _lastSent;
    private long _nextFrameDue;
    private bool _wasReady;
    private bool _pendingRefresh;
    private volatile bool _force;
    private int _frames;
    private long _workTicks;
    private bool _fxShown;
    private bool _fxFast;
    private int _seekCue = -1;
    private bool _blackShown;
    private volatile bool _previewWanted;
    private long _nextPreview;
    private byte[]? _preview;
    private Bitmap? _previewBitmap;
    private readonly MemoryStream _previewStream = new();
    private const int PreviewMs = 350;
    private static readonly ImageCodecInfo JpegCodec = ImageCodecInfo.GetImageEncoders().First(e => e.FormatID == ImageFormat.Jpeg.Guid);

    /// <summary>Miniatura (JPEG) do que a tela está mostrando, para o painel. Só é feita enquanto o painel pede.</summary>
    public byte[]? Preview => Volatile.Read(ref _preview);

    /// <summary>O painel está aberto: a cada ~350 ms o quadro atual vira uma miniatura.</summary>
    public bool PreviewWanted
    {
        get => _previewWanted;
        set
        {
            _previewWanted = value;
            if (!value) Volatile.Write(ref _preview, null);
        }
    }

    /// <summary>"Desligar tudo": a tela fica preta (ou apagada) até voltar a false.</summary>
    public volatile bool Blackout;

    /// <summary>Quadros do Modo Mangá: enquanto há efeito nesta tela, ~30 por segundo.</summary>
    private const int FxFrameMs = 33;

    /// <summary>Os efeitos do Modo Mangá desta tela (null = a tela não participa).</summary>
    protected MangaLayer? Manga { get; init; }

    protected ScreenPipeline(AppConfig config, string theme)
    {
        Config = config;
        _wantedTheme = theme;
    }

    public abstract string Name { get; }
    public abstract ScreenKind Kind { get; }
    public abstract bool IsReady { get; }

    public string? ThemeName => _theme?.Name;
    public IEnumerable<string> SensorKeys => _theme?.SensorKeys ?? Enumerable.Empty<string>();

    /// <summary>Quando esta tela precisa da thread de novo: próximo quadro da animação (long.MaxValue se não há).</summary>
    public long NextWakeDue
    {
        get
        {
            if (!IsReady || _renderer is null) return long.MaxValue;
            if (Blackout) return !_blackShown ? 0 : KeepAliveWhileBlack ? _lastSent + KeepAliveFrameMs : long.MaxValue;
            long due = _renderer.IsAnimated ? _nextFrameDue : long.MaxValue;
            return _fxFast ? Math.Min(due, _lastSent + FxFrameMs) : due;
        }
    }

    /// <summary>Pede para a apresentação de painéis pular para o primeiro painel com o momento <paramref name="cue"/>.</summary>
    public void SeekToCue(PanelCue cue) => Interlocked.Exchange(ref _seekCue, (int)cue);

    protected virtual int MaxFps => 1;

    /// <summary>No "desligar tudo", reenvia o quadro preto de vez em quando (false = só o primeiro).</summary>
    protected virtual bool KeepAliveWhileBlack => true;

    public abstract void Start();
    public abstract void Pump();
    protected abstract bool Send(ThemeRenderer renderer);

    /// <summary>Atalho para quadros pré-codificados. null = não se aplica; senão, o resultado do envio.</summary>
    protected virtual bool? TrySendCached(ThemeRenderer renderer) => null;

    protected virtual void OnThemeChanged()
    {
    }

    /// <summary>Logo depois de desenhar um quadro no canvas (quadros que vêm prontos do cache não passam aqui).</summary>
    protected virtual void OnRendered(ThemeRenderer renderer, Theme theme)
    {
    }

    /// <summary>Entrou no "desligar tudo" com o canvas já preto. Padrão: manda o quadro preto.</summary>
    protected virtual void OnBlackout(Bitmap black) => SendBlack(black);

    /// <summary>Saiu do "desligar tudo" (o próximo quadro é redesenhado do zero).</summary>
    protected virtual void OnWake()
    {
    }

    private void SendBlack(Bitmap black)
    {
        if (_renderer is { } renderer && renderer.Canvas == black) Send(renderer);
    }

    public void SelectTheme(string name)
    {
        _wantedTheme = name;
        Interlocked.Increment(ref _version);
    }

    private volatile string? _themeOverride;

    /// <summary>
    /// Um tema por cima do escolhido, sem mudar a escolha (ex.: o painel do Modo jogo enquanto o jogo está aberto).
    /// Null = volta ao tema escolhido.
    /// </summary>
    public string? ThemeOverride
    {
        get => _themeOverride;
        set
        {
            if (value == _themeOverride) return;
            _themeOverride = value;
            Interlocked.Increment(ref _version);
        }
    }

    /// <summary>Recarrega o mesmo tema (ex.: outro GIF ou outro estilo de dados).</summary>
    public void Reload() => Interlocked.Increment(ref _version);

    /// <summary>Força redesenhar e reenviar (ex.: rotação mudou).</summary>
    public void Invalidate() => _force = true;

    /// <summary>Carrega o tema pedido, se mudou. Devolve true quando isso muda os sensores necessários.</summary>
    public bool EnsureTheme()
    {
        int version = Volatile.Read(ref _version);
        if (!IsReady || version == _loadedVersion) return false;
        _loadedVersion = version;

        _renderer?.Dispose();
        _theme?.Dispose();
        _renderer = null;
        _theme = null;

        var (width, height) = ThemeCatalog.Size(Kind);
        var theme = _themeOverride == Game.GameTheme.Name ? Game.GameTheme.Create(width, height) : ThemeCatalog.Load(Kind, _wantedTheme, Config);
        try
        {
            _renderer = new ThemeRenderer(theme);
        }
        catch (Exception ex)
        {
            Log.Error($"{Name}: tema '{theme.Name}' não pôde ser desenhado; usando o padrão", ex);
            theme.Dispose();
            theme = ThemeCatalog.Fallback(Kind);
            _renderer = new ThemeRenderer(theme);
        }
        _theme = theme;
        _force = true;
        _nextFrameDue = 0;
        OnThemeChanged();
        Log.Info($"{Name}: tema {theme.Name}{(_renderer.IsAnimated ? $" (animado, {_renderer.FrameCount} quadros)" : "")}");
        return true;
    }

    public void Tick(SensorHub sensors, bool freshValues)
    {
        var renderer = _renderer;
        if (!IsReady || renderer is null)
        {
            _wasReady = false;
            _blackShown = false; // reconectou no meio do "desligar tudo": apaga de novo
            return;
        }

        long now = Clock.Ms;
        bool force = _force || !_wasReady;
        _force = false;
        _wasReady = true;
        if (freshValues) _pendingRefresh = true;

        // Tudo desligado: um quadro preto (e a tela apaga, onde há como) e nada mais até religar.
        if (Blackout)
        {
            if (!_blackShown || (KeepAliveWhileBlack && now - _lastSent >= KeepAliveFrameMs))
            {
                using (var g = Graphics.FromImage(renderer.Canvas)) g.Clear(Color.Black);
                if (!_blackShown) OnBlackout(renderer.Canvas);
                else Send(renderer); // de vez em quando, por garantia
                _blackShown = true;
                _lastSent = now;
            }
            UpdatePreview(renderer.Canvas, now);
            return;
        }
        if (_blackShown)
        {
            _blackShown = false;
            OnWake();
            force = true;
            _nextFrameDue = 0;
        }

        // Modo Mangá: pulo da apresentação (o Gear 5 leva a Pro Flow ao painel dele) e efeitos por cima do quadro.
        int seek = Interlocked.Exchange(ref _seekCue, -1);
        if (seek >= 0 && _theme?.Animation is PanelSlideshow slideshow && slideshow.FirstFrameOf((PanelCue)seek) is int target)
        {
            renderer.Seek(target);
            _nextFrameDue = now + renderer.FrameDelayMs;
            force = true;
        }
        var manga = MangaStage.InUse && Manga is not null ? MangaStage.Current : null;
        var pace = manga is null ? MangaPace.None : Manga!.Need(manga, now);
        bool fx = pace != MangaPace.None;
        if (_fxShown && !fx) force = true; // um quadro limpo para encerrar

        bool frameDue = false;
        if (renderer.IsAnimated && now >= _nextFrameDue)
        {
            if (_nextFrameDue != 0) renderer.AdvanceFrame();
            int delay = Math.Max(renderer.FrameDelayMs, 1000 / Math.Max(1, MaxFps));
            // Agenda pelo horário previsto (não pelo atual) para manter o ritmo; se atrasou demais, recomeça do agora.
            _nextFrameDue = _nextFrameDue != 0 && now - _nextFrameDue < delay ? _nextFrameDue + delay : now + delay;
            frameDue = true;
        }

        bool keepAlive = now - _lastSent >= KeepAliveFrameMs;
        // Com animação, valores novos esperam o próximo quadro (não gera quadro extra) — a não ser que o quadro atual
        // seja longo (ex.: painel parado 8 s no tema One Piece): aí redesenha com os valores novos a cada segundo.
        // Sem animação, valores novos são o próprio gatilho.
        bool due = renderer.IsAnimated
            ? frameDue || (_pendingRefresh && renderer.HasDynamicContent && now - _lastSent >= 1000)
            : _pendingRefresh;
        if (pace == MangaPace.Fast && now - _lastSent >= FxFrameMs) due = true;
        _fxShown = fx;
        _fxFast = pace == MangaPace.Fast;
        if (!due && !force && !keepAlive)
        {
            UpdatePreview(renderer.Canvas, now);
            return;
        }

        long start = Stopwatch.GetTimestamp();
        bool sent;
        if (!fx && TrySendCached(renderer) is bool cached)
        {
            sent = cached;
            // O quadro saiu pronto do cache: o canvas não foi redesenhado. Para a miniatura, desenha só quando ela vence.
            if (_previewWanted && now >= _nextPreview) renderer.Render(null, force: true, refreshValues: false);
        }
        else
        {
            bool refresh = _pendingRefresh || force;
            _pendingRefresh = false;
            bool changed = renderer.Render(sensors, force || fx, refresh);
            if (changed && _theme is not null) OnRendered(renderer, _theme);
            if (fx)
            {
                Manga!.Apply(renderer.Canvas, now, manga!);
                changed = true;
            }
            sent = (changed || keepAlive) && Send(renderer);
        }

        if (sent)
        {
            _lastSent = now;
            _frames++;
            _workTicks += Stopwatch.GetTimestamp() - start;
        }
        UpdatePreview(renderer.Canvas, now);
    }

    private void UpdatePreview(Bitmap canvas, long now)
    {
        if (!_previewWanted || now < _nextPreview) return;
        _nextPreview = now + PreviewMs;
        int width = Math.Min(canvas.Width, 720), height = canvas.Height * width / canvas.Width;
        if (_previewBitmap is null || _previewBitmap.Width != width || _previewBitmap.Height != height)
        {
            _previewBitmap?.Dispose();
            _previewBitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
        }
        using (var g = Graphics.FromImage(_previewBitmap))
        {
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
            g.DrawImage(canvas, 0, 0, width, height);
        }
        _previewStream.SetLength(0);
        using (var parameters = new EncoderParameters(1))
        using (var quality = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 82L))
        {
            parameters.Param[0] = quality;
            _previewBitmap.Save(_previewStream, JpegCodec, parameters);
        }
        Volatile.Write(ref _preview, _previewStream.ToArray());
    }

    /// <summary>Quadros enviados e custo médio (desenho + codificação + envio) desde a última chamada.</summary>
    public virtual string TakeStats()
    {
        string result = $"{Name}: {_frames} quadros, {(_frames == 0 ? 0 : _workTicks * 1000.0 / Stopwatch.Frequency / _frames):0.00} ms cada";
        _frames = 0;
        _workTicks = 0;
        return result;
    }

    public void SaveCapture(string path) => _renderer?.Canvas.Save(path, ImageFormat.Png);

    public virtual void Dispose()
    {
        _renderer?.Dispose();
        _theme?.Dispose();
        _renderer = null;
        _theme = null;
        Manga?.Dispose();
        _previewBitmap?.Dispose();
        _previewStream.Dispose();
    }
}

/// <summary>Jonsbo DS339: painel retrato 376x960, temas deitados girados 90°/270°, pixels BGRA crus.</summary>
internal sealed class Ds339Screen : ScreenPipeline
{
    private readonly Ds339Display _display = new();

    public Ds339Screen(AppConfig config) : base(config, config.DS339.Tema)
    {
    }

    public override string Name => "DS339";
    public override ScreenKind Kind => ScreenKind.Ds339;
    public override bool IsReady => _display.IsReady;

    /// <summary>
    /// Cada quadro, mesmo preto, acende a luz de fundo de novo (cinza), e sem quadros ela apaga sozinha em alguns
    /// segundos: no "desligar tudo", só o primeiro quadro preto. (Desativar a USB não serve: sem o computador ela mostra
    /// a tela de espera, cinza com o logo da Jonsbo.)
    /// </summary>
    protected override bool KeepAliveWhileBlack => false;

    public override void Start()
    {
        if (!_display.Start()) Log.Warn("DS339: o SDK da tela não iniciou (veja acima).");
    }

    public override void Pump() => _display.Pump();

    protected override bool Send(ThemeRenderer renderer)
    {
        FrameRotator.Blit(renderer.Canvas, _display.FrameBuffer, Ds339Display.PanelWidth, Ds339Display.PanelHeight, Config.DS339.Rotacao);
        Brightness.Apply(_display.FrameSpan, Config.DS339.Brilho);
        return _display.Present();
    }


    public override void Dispose()
    {
        base.Dispose();
        _display.Dispose();
    }
}

/// <summary>
/// Jungle Leopard Pro Flow 360: 960x480 por serial, um JPEG por quadro. GIF sem nada por cima vira um cache de
/// JPEGs prontos (cada quadro é codificado uma única vez); com dados por cima, cada quadro é desenhado e codificado.
/// </summary>
internal sealed class ProFlowScreen : ScreenPipeline
{
    private readonly ProFlowDisplay _display = new();
    private readonly JpegEncoder _encoder = new();
    private byte[]?[] _cache = Array.Empty<byte[]?>();
    private float[] _cacheLuminance = Array.Empty<float>();
    private ThemeRenderer? _cacheOwner;
    private int _cacheRotation;
    private int _cacheBrightness;
    private volatile bool _measureLight;
    private volatile ScreenLight? _light;

    public ProFlowScreen(AppConfig config) : base(config, config.ProFlow.Tema)
    {
        Manga = new MangaLayer(ScreenKind.ProFlow);
    }

    public override string Name => "Pro Flow 360";
    public override ScreenKind Kind => ScreenKind.ProFlow;
    public override bool IsReady => _display.IsReady;
    protected override int MaxFps => Config.ProFlow.FpsMax;

    /// <summary>Ligado enquanto o efeito do LED da placa acompanha a imagem: cada quadro é medido.</summary>
    public bool MeasureLight
    {
        get => _measureLight;
        set => _measureLight = value;
    }

    /// <summary>Último quadro medido (para o LED da placa).</summary>
    public ScreenLight? Light => _light;

    public override string TakeStats()
    {
        var (writes, average, repeats) = _display.TakeStats();
        return $"{base.TakeStats()} (envios {writes}, {average:0.00} ms cada, {repeats} repetições)";
    }

    public override void Start()
    {
    }

    public override void Pump() => _display.Pump();

    protected override bool Send(ThemeRenderer renderer)
    {
        int brightness = Math.Max(5, (int)Math.Round(Config.ProFlow.Brilho * (SceneDirector.InUse ? _sceneDim : 1)));
        var (buffer, length) = _encoder.Encode(renderer.Canvas, Config.ProFlow.Rotacao, brightness, ProFlowDisplay.MaxJpegBytes);
        return _display.SendFrame(buffer, length);
    }

    protected override bool? TrySendCached(ThemeRenderer renderer)
    {
        if (!renderer.CanCacheFrames) return null;

        int rotation = Config.ProFlow.Rotacao;
        int brightness = Config.ProFlow.Brilho;
        if (!ReferenceEquals(_cacheOwner, renderer) || _cacheRotation != rotation || _cacheBrightness != brightness)
        {
            _cache = new byte[]?[renderer.FrameCount];
            _cacheLuminance = new float[renderer.FrameCount];
            _cacheOwner = renderer;
            _cacheRotation = rotation;
            _cacheBrightness = brightness;
        }

        int index = renderer.FrameIndex;
        var jpeg = _cache[index];
        if (jpeg is null)
        {
            renderer.Render(null, force: true, refreshValues: false); // sem elementos dinâmicos: não lê sensores
            var (buffer, length) = _encoder.Encode(renderer.Canvas, rotation, brightness, ProFlowDisplay.MaxJpegBytes);
            jpeg = buffer.AsSpan(0, length).ToArray();
            _cache[index] = jpeg;
            _cacheLuminance[index] = ScreenLight.Measure(renderer.Canvas); // ~10 µs, uma vez por quadro do GIF
        }
        if (_measureLight) _light = new ScreenLight(Clock.Ms, _cacheLuminance[index], null);
        return _display.SendFrame(jpeg, jpeg.Length);
    }

    protected override void OnRendered(ThemeRenderer renderer, Theme theme)
    {
        if (!_measureLight) return;
        var scene = theme.Animation is PanelSlideshow slideshow ? slideshow.Describe(renderer.FrameIndex) : (PanelScene?)null;
        _light = new ScreenLight(Clock.Ms, ScreenLight.Measure(renderer.Canvas), scene);
        // O Fundo do Mar já é parte do mundo; os outros temas entram na cena por cima (raio, varredura, noite).
        _sceneDim = SceneDirector.InUse && !MangaStage.InUse && theme.Name != DeepSeaTheme.Name ? JoinScene(renderer.Canvas) : 1;
    }

    private double _sceneDim = 1;

    /// <summary>
    /// A Pro Flow entra na cena do Modo Grand Line: o painel pisca junto com o raio da tempestade, recebe a varredura de
    /// luz do "bem-vindo", escurece no Buster Call e à noite, e quase apaga com o PC dormindo. Devolve o fator de brilho.
    /// </summary>
    private double JoinScene(Bitmap canvas)
    {
        var stage = SceneDirector.Current;
        long now = Clock.Ms;
        double dim = Choreo.Lerp(0.7, 1, stage.Daylight);
        if (stage.Sleeping) dim = 0.3;
        int white = 0, black = 0;
        foreach (var e in stage.Events)
        {
            double x = (now - e.At) / 1000.0;
            if (x < 0) continue;
            switch (e.Kind)
            {
                case StageEventKind.Lightning:
                    white = Math.Max(white, (int)(140 * Choreo.Lightning(x, e.Variant)));
                    break;
                case StageEventKind.Welcome when x < Choreo.WelcomeSweep:
                    white = Math.Max(white, (int)(170 * Math.Sin(Math.PI * x / Choreo.WelcomeSweep)));
                    break;
                case StageEventKind.BusterCall when x < Choreo.BusterCallSeconds:
                    black = Math.Max(black, (int)(110 * Choreo.Smooth(0, 0.5, x) * Choreo.Smooth(0, 0.5, Choreo.BusterCallSeconds - x)));
                    break;
            }
        }
        if (white > 0 || black > 0)
        {
            using var g = Graphics.FromImage(canvas);
            using var brush = new SolidBrush(white >= black ? Color.FromArgb(white, 255, 255, 255) : Color.FromArgb(black, 0, 0, 0));
            g.FillRectangle(brush, 0, 0, canvas.Width, canvas.Height);
        }
        return dim;
    }

    protected override void OnThemeChanged()
    {
        _cache = Array.Empty<byte[]?>();
        _cacheLuminance = Array.Empty<float>();
        _cacheOwner = null;
        _light = null;
    }

    /// <summary>Desligar tudo: um quadro preto e a tela sai do modo ao vivo (apaga), como no bloqueio do app original.</summary>
    protected override void OnBlackout(Bitmap black)
    {
        var (buffer, length) = _encoder.Encode(black, Config.ProFlow.Rotacao, 100, ProFlowDisplay.MaxJpegBytes);
        _display.Sleep(buffer, length);
        _light = null;
    }

    protected override void OnWake() => _display.Wake();

    public override void Dispose()
    {
        base.Dispose();
        _display.Dispose();
        _encoder.Dispose();
    }
}

/// <summary>
/// Thermalright 9,16" (1920x462) por WinUSB, um JPEG por quadro (ver <see cref="ThermalrightDisplay"/>). A USB 2.0 dá
/// folga: a qualidade sobe até 90 (o TRCC usa 95; acima de 90 o arquivo dobra e a diferença não aparece).
/// </summary>
internal sealed class ThermalrightScreen : ScreenPipeline
{
    private readonly ThermalrightDisplay _display = new();
    private readonly JpegEncoder _encoder = new(maxQuality: 90);
    private byte[]?[] _cache = Array.Empty<byte[]?>();
    private ThemeRenderer? _cacheOwner;
    private int _cacheRotation;
    private int _cacheBrightness;

    public ThermalrightScreen(AppConfig config) : base(config, config.Thermalright.Tema)
    {
        Manga = new MangaLayer(ScreenKind.Thermalright);
    }

    public override string Name => "Thermalright";
    public override ScreenKind Kind => ScreenKind.Thermalright;
    public override bool IsReady => _display.IsReady;
    protected override int MaxFps => Config.Thermalright.FpsMax;

    public override string TakeStats()
    {
        var (writes, average, repeats) = _display.TakeStats();
        return $"{base.TakeStats()} (envios {writes}, {average:0.00} ms cada, {repeats} repetições)";
    }

    public override void Start()
    {
    }

    public override void Pump() => _display.Pump();

    protected override bool Send(ThemeRenderer renderer)
    {
        var (buffer, length) = _encoder.Encode(renderer.Canvas, Config.Thermalright.Rotacao, Config.Thermalright.Brilho, ThermalrightDisplay.MaxJpegBytes);
        return _display.SendFrame(buffer, length);
    }

    /// <summary>Vídeo curto ou GIF sem nada por cima: cada quadro é codificado uma vez e reaproveitado nas voltas seguintes.</summary>
    protected override bool? TrySendCached(ThemeRenderer renderer)
    {
        if (!renderer.CanCacheFrames) return null;

        int rotation = Config.Thermalright.Rotacao;
        int brightness = Config.Thermalright.Brilho;
        if (!ReferenceEquals(_cacheOwner, renderer) || _cacheRotation != rotation || _cacheBrightness != brightness)
        {
            _cache = new byte[]?[renderer.FrameCount];
            _cacheOwner = renderer;
            _cacheRotation = rotation;
            _cacheBrightness = brightness;
        }

        int index = renderer.FrameIndex;
        var jpeg = _cache[index];
        if (jpeg is null)
        {
            renderer.Render(null, force: true, refreshValues: false);
            var (buffer, length) = _encoder.Encode(renderer.Canvas, rotation, brightness, ThermalrightDisplay.MaxJpegBytes);
            jpeg = buffer.AsSpan(0, length).ToArray();
            _cache[index] = jpeg;
        }
        return _display.SendFrame(jpeg, jpeg.Length);
    }

    protected override void OnThemeChanged()
    {
        _cache = Array.Empty<byte[]?>();
        _cacheOwner = null;
    }

    /// <summary>Desligar tudo: um quadro preto e a tela é desativada (a luz de fundo apaga com a porta USB suspensa).</summary>
    protected override void OnBlackout(Bitmap black)
    {
        var (buffer, length) = _encoder.Encode(black, Config.Thermalright.Rotacao, 100, ThermalrightDisplay.MaxJpegBytes);
        _display.SendFrame(buffer, length);
        _display.Sleep();
    }

    protected override void OnWake() => _display.Wake();

    public override void Dispose()
    {
        base.Dispose();
        _display.Dispose();
        _encoder.Dispose();
    }
}

/// <summary>
/// JPEG com o codificador do GDI+ (~1 ms para 960x480). Antes de codificar, uma única passada gira 180° e/ou
/// escurece a imagem (é assim que o brilho funciona nessa tela — ver <see cref="ProFlowDisplay"/>).
/// Nunca passa do limite da tela (quadro maior ela não mostra direito: pisca ou fica no anterior), como o app original,
/// que desce a qualidade até caber. A qualidade fica estável de um quadro para o outro — se ela oscilasse, os artefatos
/// "ferveriam" numa imagem em movimento: desce na hora quando o quadro não cabe, sobe devagar quando sobra espaço, e
/// sobe rápido quando sobra muito (fim de uma transição de retícula, que é pesada, para um painel leve).
/// </summary>
internal sealed class JpegEncoder : IDisposable
{
    private const int MinQuality = 8;

    private static readonly ImageCodecInfo Codec = ImageCodecInfo.GetImageEncoders().First(e => e.FormatID == ImageFormat.Jpeg.Guid);
    private readonly MemoryStream _stream = new(128 * 1024);
    private Bitmap? _work;
    private const int EncodesPerCollection = 600;

    private readonly int _maxQuality;
    private int _quality;              // qualidade atual
    private int _roomy;                // quadros seguidos com folga
    private int _sinceCollection;

    public JpegEncoder(int maxQuality = 85)
    {
        _maxQuality = maxQuality;
        _quality = maxQuality;
    }

    public (byte[] Buffer, int Length) Encode(Bitmap canvas, int rotation, int brightness, int maxBytes)
    {
        var source = canvas;
        if (rotation == 180 || brightness < 100)
        {
            if (_work is null || _work.Width != canvas.Width || _work.Height != canvas.Height)
            {
                _work?.Dispose();
                _work = new Bitmap(canvas.Width, canvas.Height, PixelFormat.Format32bppPArgb);
            }
            Prepare(canvas, _work, rotation == 180, brightness);
            source = _work;
        }

        int quality = _quality;
        while (true)
        {
            _stream.SetLength(0);
            using (var parameters = new EncoderParameters(1))
            using (var value = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)quality))
            {
                parameters.Param[0] = value;
                source.Save(_stream, Codec, parameters);
            }
            if (_stream.Length <= maxBytes || quality <= MinQuality) break;
            quality = Math.Max(MinQuality, quality - (_stream.Length > maxBytes * 1.25 ? 10 : 3));
        }

        // Ajuste para os próximos quadros (este já foi).
        double fill = (double)_stream.Length / maxBytes;
        if (quality < _quality || fill >= 0.85) _roomy = 0;
        else if (fill < 0.6) quality += 6;
        else if (++_roomy >= 8)
        {
            quality += 2;
            _roomy = 0;
        }
        _quality = Math.Min(_maxQuality, quality);

        // Cada Save do GDI+ cria um invólucro COM para o stream (~1 KB) que só some numa coleta. O programa quase não
        // aloca, e com o GC não concorrente o orçamento da geração 0 chega a 200 MB: sem isto a memória subia ~50 MB a
        // cada meia hora de animação. Uma coleta da geração 0 a cada 600 quadros (20 s a 30 por segundo) custa ~0,2 ms.
        if (++_sinceCollection >= EncodesPerCollection)
        {
            _sinceCollection = 0;
            GC.Collect(0, GCCollectionMode.Forced, blocking: true, compacting: false);
        }
        return (_stream.GetBuffer(), (int)_stream.Length);
    }

    public void Dispose()
    {
        _work?.Dispose();
        _stream.Dispose();
    }

    /// <summary>Copia <paramref name="source"/> para <paramref name="target"/>, girando 180° e escurecendo se pedido.</summary>
    private static unsafe void Prepare(Bitmap source, Bitmap target, bool rotate180, int brightness)
    {
        var rect = new Rectangle(0, 0, source.Width, source.Height);
        var src = source.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);
        var dst = target.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
        try
        {
            for (int y = 0; y < rect.Height; y++)
            {
                var from = new Span<uint>((byte*)src.Scan0 + y * src.Stride, rect.Width);
                int targetRow = rotate180 ? rect.Height - 1 - y : y;
                var to = new Span<uint>((byte*)dst.Scan0 + targetRow * dst.Stride, rect.Width);
                from.CopyTo(to);
                if (rotate180) to.Reverse();
                Brightness.Apply(MemoryMarshal.AsBytes(to), brightness);
            }
        }
        finally
        {
            source.UnlockBits(src);
            target.UnlockBits(dst);
        }
    }
}
