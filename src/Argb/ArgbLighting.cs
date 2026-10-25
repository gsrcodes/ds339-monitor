using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using DS339Monitor.App;
using DS339Monitor.Lighting;
using DS339Monitor.Manga;

namespace DS339Monitor.Argb;

/// <summary>
/// Fans e fitas na controladora Nollie 16CH, numa thread própria (~30 quadros por segundo nas animações; parado, reenvia o
/// quadro a cada 1 s). Cada parte (centro e laterais de cada fan, memórias, cabos, bomba, placa sob a placa-mãe) segue o
/// efeito geral ou tem o seu. Cuidados:
/// <list type="bullet">
/// <item>só escreve com "controlar pelo programa" ligado, e nunca com o SignalRGB, o app da Nollie ou o OpenRGB abertos
/// (conferido a cada 1 s; aberto, solta a controladora sem mandar nada);</item>
/// <item>só manda quadros de cor, exatamente como o plugin do SignalRGB — nenhum comando que grave algo na controladora;</item>
/// <item>limitador de energia: estima a corrente de todos os LEDs (~50 mA por LED no branco total) e, acima do teto, baixa
/// tudo por igual;</item>
/// <item>trava contra pisca-pisca na luz média de todos os LEDs (no máximo 3 clarões por segundo, como o LED da placa);</item>
/// <item>troca de efeito, "desligar tudo" e a volta fazem uma transição suave de meio segundo.</item>
/// </list>
/// </summary>
internal sealed class ArgbLighting : IDisposable
{
    private const int FrameMs = 33;
    private const int KeepAliveMs = 1000;
    private const int CheckMs = 1000;
    private const int RetryMs = 10_000;
    private const int FadeMs = 500;
    private const int IdentifyMs = 8000;
    private const int ChainOrderMs = 14000;
    private int _identifyMs = IdentifyMs;
    private const int MaxConsecutiveErrors = 3;
    private const double MilliampsPerChannel = 50.0 / 3; // R, G e B acesos no máximo ≈ 50 mA por LED
    private const double IdleMilliampsPerLed = 1;
    private static readonly string[] OtherControllers = { "SignalRgb", "NollieRGB", "OpenRGB" };

    private readonly ArgbSettings _settings;
    private readonly Func<Color?> _gpuColor;
    private readonly Func<ScreenLight?> _screenLight;
    private readonly SceneInput _scene = new();
    private readonly Dictionary<ArgbZone, (string Key, LedPoint[]? Points)> _points = new(ReferenceEqualityComparer.Instance);
    private readonly Thread _thread;
    private readonly AutoResetEvent _wake = new(false);
    private readonly FlashGuard _guard = new();
    private readonly UmbraLighting _umbra = new();
    private readonly float[][] _target = new float[NollieDevice.Channels][];
    private readonly float[][] _from = new float[NollieDevice.Channels][];
    private readonly float[][] _out = new float[NollieDevice.Channels][];
    private readonly byte[]?[] _packets = new byte[NollieDevice.Channels][];
    private readonly float[][] _gains = Enumerable.Range(0, NollieDevice.Channels).Select(_ => Enumerable.Repeat(1f, NollieDevice.MaxLedsPerChannel * 3).ToArray()).ToArray();
    private volatile bool _stop;
    private volatile bool _blackout;
    private int _version;
    private volatile string _status = "desligado";
    private volatile string? _otherApp;
    private volatile bool _connected;
    private volatile string[]? _snapshot;
    private int _identifyZone = -1;
    private long _identifyUntil;
    private long _fadeStart = long.MinValue;
    private long _nextSnapshot;
    private int _limited, _held, _frames;
    private long _sendTicks, _computeTicks;
    private volatile float _cpuTemp = float.NaN, _gpuTemp = float.NaN, _gpuLoad = float.NaN;
    private int _keysChanged = 1;
    private double _screen = 0.5;
    private long _lastFrame;
    private int _gpuSample;
    private readonly ConcurrentQueue<(Func<NollieDevice, string> Job, TaskCompletionSource<string> Done)> _jobs = new();
    private long _pauseUntil;
    private volatile Playback? _playback;

    private sealed record Playback(BootFrames Animation, long Start);
    private volatile NollieConfig? _stored;
    private bool _storedRead;

    public ArgbLighting(ArgbSettings settings, Func<Color?> gpuColor, Func<ScreenLight?> screenLight)
    {
        _settings = settings;
        _gpuColor = gpuColor;
        _screenLight = screenLight;
        for (int i = 0; i < NollieDevice.Channels; i++)
        {
            _target[i] = new float[NollieDevice.MaxLedsPerChannel * 3];
            _from[i] = new float[NollieDevice.MaxLedsPerChannel * 3];
            _out[i] = new float[NollieDevice.MaxLedsPerChannel * 3];
        }
        _thread = new Thread(Run) { IsBackground = true, Name = "Fans e fitas", Priority = ThreadPriority.BelowNormal };
    }

    /// <summary>Situação curta para o painel.</summary>
    public string Status => _jobStatus ?? _status;

    private volatile string? _jobStatus;
    private bool _offline;
    private float _calibration; // nível do "comparar brancos" (fração de luz); 0 = efeito

    /// <summary>"Comparar brancos": todas as partes no mesmo branco (com o balanço de cada uma). 0 = efeito.</summary>
    public void SetCalibration(float level)
    {
        Volatile.Write(ref _calibration, level);
        Interlocked.Increment(ref _version);
        _wake.Set();
    }

    /// <summary>Um ajuste que só muda a saída (ex.: o balanço de branco) mudou: manda já.</summary>
    public void Nudge() => _wake.Set();

    /// <summary>
    /// Um motor só para calcular quadros fora do ar (o loop gravado na controladora): o mesmo cálculo e acabamento do
    /// motor de verdade, sem thread, sem Pro Flow, sem Modo Mangá e sem a placa de vídeo.
    /// </summary>
    public static ArgbLighting Offline(ArgbSettings settings) => new(settings, () => null, () => null) { _offline = true };

    /// <summary>O quadro do instante <paramref name="ms"/> (chamar em ordem crescente), por parte: R, G, B de cada LED.</summary>
    public float[][] Frame(long ms)
    {
        var zones = _settings.Zonas;
        Render(ms, zones, blackout: false, identifying: false);
        Compose(ms);
        var frame = new float[zones.Count][];
        for (int k = 0; k < zones.Count; k++)
        {
            var zone = zones[k];
            frame[k] = new float[zone.Leds * 3];
            Array.Copy(_out[zone.Canal - 1], zone.Inicio * 3, frame[k], 0, zone.Leds * 3);
        }
        return frame;
    }

    /// <summary>O que uma tarefa longa na controladora está fazendo (aparece como situação no painel).</summary>
    public void SetJobStatus(string? status) => _jobStatus = status;

    /// <summary>O programa que está com a controladora agora (null = nenhum).</summary>
    public string? OtherApp => _otherApp;

    /// <summary>A controladora está aberta por este programa.</summary>
    public bool Connected => _connected;

    /// <summary>
    /// Cores do último quadro, uma string por parte (na ordem de <see cref="ArgbSettings.Zonas"/>) com "RRGGBB" de cada
    /// LED, já como o olho vê (para a tela do PC). Null = nada sendo mandado.
    /// </summary>
    public string[]? Snapshot => _snapshot;

    /// <summary>"Desligar tudo": fans e fitas apagam (com o programa no controle).</summary>
    public bool Blackout
    {
        get => _blackout;
        set
        {
            _blackout = value;
            _wake.Set();
        }
    }

    public void Start() => _thread.Start();

    /// <summary>Configuração mudou pelo painel.</summary>
    public void Reload()
    {
        Interlocked.Increment(ref _version);
        Interlocked.Exchange(ref _keysChanged, 1);
        _wake.Set();
    }

    /// <summary>O que está gravado na controladora (lido ao abri-la e depois de cada gravação); null = ainda não lido.</summary>
    public NollieConfig? Stored => _stored;

    /// <summary>
    /// Roda <paramref name="job"/> na thread dos fans, com a controladora (a aberta, ou uma abertura só para isso se o
    /// programa não está com ela). Assim um comando nunca cruza com um quadro de cor.
    /// </summary>
    public Task<string> RunOnDevice(Func<NollieDevice, string> job)
    {
        var done = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        _jobs.Enqueue((job, done));
        _wake.Set();
        return done.Task;
    }

    /// <summary>
    /// Toca uma animação pronta (quadro a quadro) nos fans e fitas pelo próprio programa, uma vez; depois volta ao efeito.
    /// </summary>
    public void Play(BootFrames animation)
    {
        _playback = new Playback(animation, Clock.Ms + 600); // o efeito atual apaga antes (transição de 0,5 s)
        Interlocked.Increment(ref _version);
        _wake.Set();
    }

    /// <summary>Lê de novo o que está gravado na controladora.</summary>
    public Task<string> RefreshStored() => RunOnDevice(device =>
    {
        var config = NollieConfig.Read(device, out string error);
        if (config is null) return "erro: " + error;
        _stored = config;
        return "ok";
    });

    /// <summary>
    /// Deixa a controladora tocar o efeito de boot agora, por <paramref name="seconds"/> segundos (o programa para de
    /// mandar cores nesse tempo; quando volta a mandar, a controladora volta sozinha a receber).
    /// </summary>
    public Task<string> PreviewBoot(int seconds) => RunOnDevice(device =>
    {
        Volatile.Write(ref _pauseUntil, Clock.Ms + seconds * 1000L);
        device.Command(0xFA, 0x01, 0x01); // efeitos da controladora
        device.Command(0xFA, 0x20);       // tocar o boot
        Log.Info($"Fans e fitas: a controladora está tocando o efeito de boot ({seconds} s sem o programa mandar cores).");
        return "ok";
    });

    /// <summary>
    /// Grava na controladora o efeito de boot e o efeito sem programa do rascunho, só o que mudou, e confere lendo de
    /// volta. Devolve o resumo (ou "erro: …").
    /// </summary>
    public Task<string> WriteStored(NollieStoredSettings wanted, IReadOnlyList<(int Logical, int Leds)> channels,
        byte[]? animation = null, int[]? animationMap = null) => RunOnDevice(device =>
    {
        if (animation is not null && animationMap is not null)
        {
            // A animação primeiro: o "toca ao ligar" só muda se ela estiver gravada e conferida.
            var started = Stopwatch.StartNew();
            string uploaded;
            try
            {
                uploaded = NollieFseq.Upload(device, animation, animationMap, SetJobStatus);
            }
            finally
            {
                SetJobStatus(null);
            }
            if (uploaded != "ok")
            {
                Log.Warn($"Nollie: animação não gravada — {uploaded}");
                return uploaded;
            }
            Log.Info($"Nollie: animação \"{wanted.BootAnimacao}\" gravada e conferida ({animation.Length} bytes, CRC {NollieFseq.Crc(animation):X8}, " +
                $"{started.ElapsedMilliseconds} ms).");
        }
        var before = NollieConfig.Read(device, out string error);
        if (before is null) return "erro: " + error;
        var changes = before.Plan(wanted, channels);
        if (changes.Count == 0)
        {
            _stored = before;
            return "nada para gravar: a controladora já está assim";
        }
        foreach (var change in changes)
        {
            Log.Info($"Nollie: gravando {change.What} — {BitConverter.ToString(change.Payload.Take(24).ToArray())}");
            device.Command(change.Payload);
            Thread.Sleep(40); // tempo de a controladora gravar na flash dela
        }
        Thread.Sleep(300);
        var after = NollieConfig.Read(device, out error);
        if (after is null) return "erro: gravei, mas não consegui conferir — " + error;
        _stored = after;
        var left = after.Plan(wanted, channels);
        Log.Info($"Nollie: {changes.Count} ajustes gravados ({before.WriteCount} → {after.WriteCount} gravações na flash); " +
            (left.Count == 0 ? "conferido." : $"{left.Count} não bateram: {string.Join("; ", left.Select(c => c.What))}") + "\n  " + after.Describe());
        return left.Count == 0
            ? $"gravado e conferido ({changes.Count} {(changes.Count == 1 ? "ajuste" : "ajustes")})"
            : $"erro: {left.Count} ajustes não ficaram gravados (veja o log)";
    });

    /// <summary>
    /// A controladora toca a animação gravada nela uma vez (o programa para de mandar cores nesse tempo). Devolve "ok" ou
    /// "erro: …".
    /// </summary>
    public Task<string> PreviewStoredAnimation(int milliseconds) => RunOnDevice(device =>
    {
        Volatile.Write(ref _pauseUntil, Clock.Ms + milliseconds + 800);
        int code = NollieFseq.Play(device);
        Log.Info($"Fans e fitas: a controladora tocou a animação gravada nela — {NollieFseq.PlayStatusText(code)}.");
        if (code == 0) return "ok";
        Volatile.Write(ref _pauseUntil, 0);
        return "erro: " + NollieFseq.PlayStatusText(code);
    });

    private void RunJobs(NollieDevice? device)
    {
        while (_jobs.TryDequeue(out var item))
        {
            NollieDevice? temporary = null;
            try
            {
                string reason = "";
                var target = device ?? (temporary = NollieDevice.TryOpen(out reason));
                item.Done.SetResult(target is null ? "erro: " + reason : item.Job(target));
            }
            catch (Exception ex) when (ex is IOException or TimeoutException or InvalidOperationException or ObjectDisposedException or ArgumentException)
            {
                Log.Error("Nollie", ex);
                item.Done.SetResult("erro: " + ex.Message);
            }
            finally
            {
                temporary?.Dispose();
            }
        }
    }

    /// <summary>Sensores de que os efeitos escolhidos precisam (a thread de monitoramento lê e repassa).</summary>
    public IEnumerable<string> SensorKeys =>
        _settings.Ligado
            ? new[] { _settings.Efeito }.Concat(_settings.Zonas.Select(z => z.Efeito)).Distinct().SelectMany(ArgbEffects.SensorKeys).Distinct().ToArray()
            : Array.Empty<string>();

    /// <summary>True uma vez depois de cada mudança (os sensores lidos podem ter mudado).</summary>
    public bool TakeSensorKeysChanged() => Interlocked.Exchange(ref _keysChanged, 0) == 1;

    /// <summary>Algum efeito escolhido acompanha a Pro Flow ("Mangá no gabinete").</summary>
    public bool WantsScreen =>
        _settings.Ligado && (_settings.Efeito == ArgbEffects.MangaScene || _settings.Zonas.Any(z => z.Efeito == ArgbEffects.MangaScene));

    public void UpdateSensors(double? cpuTemp, double? gpuTemp, double? gpuLoad)
    {
        _cpuTemp = cpuTemp is double c ? (float)c : float.NaN;
        _gpuTemp = gpuTemp is double g ? (float)g : float.NaN;
        _gpuLoad = gpuLoad is double l ? (float)l : float.NaN;
    }

    /// <summary>
    /// A cena geral no lugar da placa de vídeo (para o LED da placa no efeito "Igual aos fans"): null = os fans não estão
    /// com o programa, ou o efeito geral não é uma cena que se possa repetir na placa.
    /// </summary>
    public Color? GpuSample()
    {
        int argb = Volatile.Read(ref _gpuSample);
        return argb == 0 ? null : Color.FromArgb(argb);
    }

    /// <summary>Posição de cada LED de cada parte, em pixels da foto (para o desenho do gabinete no painel).</summary>
    public static int[]? PhotoPoints(ArgbZone zone)
    {
        var points = CaseLayout.Parse(zone.Forma, zone.Leds, zone.Inverter);
        if (points is null) return null;
        var flat = new int[points.Length * 2];
        for (int i = 0; i < points.Length; i++)
        {
            flat[i * 2] = (int)Math.Round(points[i].X);
            flat[i * 2 + 1] = (int)Math.Round(points[i].Y);
        }
        return flat;
    }

    /// <summary>
    /// Acende só a parte <paramref name="zone"/> (pulsando em branco) por alguns segundos, para achar qual é qual. Numa
    /// capa de cabo (grade), mostra a ordem dos LEDs (ver <see cref="ChainOrder"/>).
    /// </summary>
    public void Identify(int zone)
    {
        var zones = _settings.Zonas;
        bool grid = zone >= 0 && zone < zones.Count && CaseLayout.Grid(zones[zone].Forma, zones[zone].Leds).Rows > 1;
        Volatile.Write(ref _identifyMs, grid ? ChainOrderMs : IdentifyMs);
        Volatile.Write(ref _identifyUntil, Clock.Ms + (grid ? ChainOrderMs : IdentifyMs));
        Volatile.Write(ref _identifyZone, zone);
        _wake.Set();
    }

    public void Stop()
    {
        if (_stop) return;
        _stop = true;
        _wake.Set();
        if (_thread.IsAlive && !_thread.Join(3000)) Log.Warn("Fans e fitas: a thread não terminou a tempo.");
    }

    public void Dispose()
    {
        Stop();
        _wake.Dispose();
    }

    // ---------------------------------------------------------------- laço

    private void Run()
    {
        NollieDevice? device = null;
        using var wait = new PreciseWait(_wake);
        long nextCheck = 0, retryAt = 0;
        string? lastReason = null;
        int seenVersion = -1, errors = 0;
        bool wasBlackout = false, wasIdentifying = false;

        void Release(string why)
        {
            if (device is null) return;
            device.Dispose();
            device = null;
            _connected = false;
            double average = _frames > 0 ? _sendTicks * 1000.0 / Stopwatch.Frequency / _frames : 0;
            Log.Info($"Fans e fitas: controladora solta ({why}; {_frames} quadros enviados, {average:0.0} ms cada" +
                (_limited > 0 ? $"; o limitador de energia baixou o brilho em {_limited} quadros" : "") +
                (_held > 0 ? $"; a trava contra pisca-pisca segurou {_held}" : "") + ").");
            _frames = _limited = _held = 0;
            _sendTicks = _computeTicks = 0;
        }

        try
        {
            while (!_stop)
            {
                RunJobs(device);
                long now = Clock.Ms;
                if (!_settings.Ligado)
                {
                    Volatile.Write(ref _gpuSample, 0);
                    Release("controle desligado");
                    _status = "desligado";
                    _snapshot = null;
                    _otherApp = null;
                    _wake.WaitOne();
                    continue;
                }

                if (now >= nextCheck)
                {
                    nextCheck = now + CheckMs;
                    string? found = FindOtherController();
                    if (found != _otherApp)
                    {
                        if (found is not null) Log.Info($"Fans e fitas: {found} aberto — a controladora fica com ele (só um programa por vez).");
                        else if (_otherApp is not null) Log.Info($"Fans e fitas: {_otherApp} fechou; assumindo a controladora.");
                        _otherApp = found;
                    }
                }
                if (_otherApp is not null)
                {
                    Volatile.Write(ref _gpuSample, 0);
                    Release($"{_otherApp} aberto");
                    _status = $"pausado ({_otherApp} aberto)";
                    _snapshot = null;
                    _wake.WaitOne(CheckMs);
                    continue;
                }

                if (device is null)
                {
                    if (now < retryAt)
                    {
                        _wake.WaitOne((int)Math.Min(retryAt - now, CheckMs));
                        continue;
                    }
                    device = NollieDevice.TryOpen(out string reason);
                    if (device is null)
                    {
                        if (reason != lastReason) Log.Warn($"Fans e fitas: {reason}.");
                        lastReason = reason;
                        _status = "controladora não encontrada";
                        retryAt = now + RetryMs;
                        continue;
                    }
                    lastReason = null;
                    errors = 0;
                    _connected = true;
                    _guard.Reset();
                    // Sem transição a partir do apagado: a controladora pode estar tocando o mesmo loop gravado nela (desde
                    // o boot), e um mergulho no escuro ao assumir apareceria.
                    _fadeStart = long.MinValue;
                    var zonesNow = _settings.Zonas;
                    Log.Info($"Fans e fitas: Nollie 16CH aberta ({zonesNow.Count} partes, {zonesNow.Sum(z => z.Leds)} LEDs, " +
                        $"limite de {_settings.LimiteAmperes} A).");
                    if (!_storedRead)
                    {
                        // Uma vez por execução: o que está guardado nela (só perguntas).
                        _storedRead = true;
                        _stored = NollieConfig.Read(device, out string readError);
                        if (_stored is null) Log.Warn($"Nollie: não consegui ler o que está gravado nela — {readError}.");
                        else Log.Info($"Nollie: gravado nela — {_stored.Describe()}");
                    }
                }

                long pause = Volatile.Read(ref _pauseUntil);
                if (now < pause)
                {
                    // A controladora está tocando o efeito dela (prévia do boot): nenhum quadro até acabar.
                    _status = "mostrando o efeito de boot";
                    _wake.WaitOne((int)Math.Min(pause - now, 500));
                    if (Clock.Ms >= Volatile.Read(ref _pauseUntil)) BeginFade(Clock.Ms);
                    continue;
                }

                var zones = _settings.Zonas;
                int version = Volatile.Read(ref _version);
                bool blackout = _blackout;
                bool identifying = now < Volatile.Read(ref _identifyUntil) && Volatile.Read(ref _identifyZone) >= 0;
                if (version != seenVersion || blackout != wasBlackout || identifying != wasIdentifying)
                {
                    if (blackout != wasBlackout) Log.Info(blackout ? "Fans e fitas: apagados (desligar tudo)." : "Fans e fitas: acesos de novo.");
                    seenVersion = version;
                    wasBlackout = blackout;
                    wasIdentifying = identifying;
                    BeginFade(now);
                }

                long computeStart = Stopwatch.GetTimestamp();
                bool animating = Render(now, zones, blackout, identifying);
                bool fading = Compose(now);
                var packets = Pack(zones);
                long sendStart = Stopwatch.GetTimestamp();
                _computeTicks += sendStart - computeStart;
                bool sent = device.SendFrame(packets);
                _sendTicks += Stopwatch.GetTimestamp() - sendStart;
                if (!sent)
                {
                    if (++errors >= MaxConsecutiveErrors)
                    {
                        Release($"{errors} erros seguidos de escrita");
                        _status = "sem resposta (tentando de novo)";
                        retryAt = now + RetryMs;
                        continue;
                    }
                }
                else
                {
                    errors = 0;
                    _frames++;
                }
                if (_frames == 30)
                    Log.Info($"Fans e fitas: primeiros 30 quadros enviados (cálculo {_computeTicks * 1000.0 / Stopwatch.Frequency / 30:0.00} ms e " +
                        $"envio {_sendTicks * 1000.0 / Stopwatch.Frequency / 30:0.0} ms cada).");
                if (now >= _nextSnapshot)
                {
                    _nextSnapshot = now + 100;
                    _snapshot = TakeSnapshot(zones);
                }

                _status = blackout ? "apagado (desligar tudo)" : identifying ? "identificando" : "ativo";
                wait.Wait(animating || fading || identifying ? FrameMs : KeepAliveMs);
            }
        }
        catch (Exception ex)
        {
            Log.Error("Fans e fitas", ex);
            _status = "erro (veja o log)";
        }
        finally
        {
            Release("programa fechando");
        }
    }

    private void BeginFade(long now)
    {
        for (int c = 0; c < NollieDevice.Channels; c++) Array.Copy(_out[c], _from[c], _out[c].Length);
        _fadeStart = now;
    }

    // ---------------------------------------------------------------- quadro

    /// <summary>Calcula o quadro alvo em _target (fração de cada canal R, G, B, 0–1). True = animado.</summary>
    private bool Render(long now, List<ArgbZone> zones, bool blackout, bool identifying)
    {
        foreach (var channel in _target) Array.Clear(channel);
        if (blackout) return false;

        float calibration = Volatile.Read(ref _calibration);
        if (calibration > 0 && !_offline)
        {
            foreach (var zone in zones)
            {
                var buffer = _target[zone.Canal - 1];
                for (int i = zone.Inicio * 3; i < (zone.Inicio + zone.Leds) * 3; i++) buffer[i] = calibration;
            }
            return false;
        }

        if (_playback is { } playback)
        {
            int frame = (int)((now - playback.Start) / playback.Animation.FrameMs);
            if (frame >= playback.Animation.Frames.Count)
            {
                _playback = null;
                Interlocked.Increment(ref _version); // volta ao efeito com a transição suave
                Log.Info($"Fans e fitas: animação {playback.Animation.Name} tocada.");
            }
            else
            {
                if (frame < 0) return true; // um instante de escuro antes de começar
                var data = playback.Animation.Frames[frame];
                for (int k = 0; k < zones.Count && k < data.Length; k++)
                {
                    var zone = zones[k];
                    if (data[k].Length != zone.Leds * 3) continue;
                    Array.Copy(data[k], 0, _target[zone.Canal - 1], zone.Inicio * 3, data[k].Length);
                }
                return true;
            }
        }

        double t = now / 1000.0;
        double speed = _settings.Velocidade switch { GpuEffects.Slow => 0.6, GpuEffects.Fast => 1.6, _ => 1.0 };
        var globalColor = GpuLighting.ParseColor(_settings.Cor);
        PrepareScene(now, t, speed, globalColor);
        double master = Math.Pow(_settings.Brilho / 100.0, 2); // o deslizante é perceptivo; o LED é linear
        int identify = Volatile.Read(ref _identifyZone);
        bool animating = false;
        Color? gpu = null;
        bool gpuRead = false;

        // O Modo Umbra, ligado, desenha no lugar dos efeitos de sempre (identificar uma parte continua valendo).
        bool umbra = _settings.Umbra is { Ligado: true } && !identifying;
        if (umbra)
        {
            _umbra.Render(_settings.Umbra, zones, _target, t, master, Points);
            animating = true;
        }

        for (int k = 0; !umbra && k < zones.Count; k++)
        {
            var zone = zones[k];
            var buffer = _target[zone.Canal - 1];
            string effect = zone.Efeito == ArgbEffects.Global ? _settings.Efeito : zone.Efeito;
            Color color = zone.Efeito == ArgbEffects.Global ? globalColor : GpuLighting.ParseColor(zone.Cor);
            double gain = master * zone.Brilho / 100.0;
            if (identifying)
            {
                // Só a parte procurada, em branco pulsando uma vez por segundo, num brilho fixo e visível.
                effect = k != identify ? ArgbEffects.Off : CaseLayout.Grid(zone.Forma, zone.Leds).Rows > 1 ? "ordem" : "identificar";
                color = Color.White;
                gain = 0.55;
            }
            if (effect == ArgbEffects.Off) continue;
            if (ArgbEffects.Animated(effect)) animating = true;
            if (effect == ArgbEffects.GpuSync && !gpuRead)
            {
                gpu = _gpuColor();
                gpuRead = true;
            }

            int n = zone.Leds;
            bool cable = ArgbEffects.IsCable(effect);
            var points = ArgbEffects.IsScene(effect) ? Points(zone) : null;
            var grid = cable ? CaseLayout.Grid(zone.Forma, n) : default;
            for (int i = 0; i < n; i++)
            {
                int index = zone.Inverter ? n - 1 - i : i;
                double x = index / (double)n;
                var (r, g, b) = effect == "ordem" ? ChainOrder(i, n, now - (Volatile.Read(ref _identifyUntil) - Volatile.Read(ref _identifyMs)))
                    : ArgbEffects.IsScene(effect) ? CaseScenes.Shade(effect, color, points?[i] ?? Fallback(x), _scene)
                    : cable ? CableEffects.Shade(effect, color, grid, index, t, speed, k)
                    : Pixel(effect, color, gpu, x, t, speed, k, i);
                int at = (zone.Inicio + i) * 3;
                buffer[at] = (float)(r * gain);
                buffer[at + 1] = (float)(g * gain);
                buffer[at + 2] = (float)(b * gain);
            }
        }

        // A partida do CS2 por cima do efeito (só com o programa no ar e as reações ligadas).
        if (!_offline && !identifying && GameReactions?.Invoke() is { } moment)
        {
            double boost = Math.Max(master, 0.35); // os clarões aparecem mesmo com o brilho geral baixo
            foreach (var zone in zones)
            {
                var buffer = _target[zone.Canal - 1];
                var points = Points(zone);
                for (int i = 0; i < zone.Leds; i++)
                {
                    var shade = Game.GameLights.At(moment, points?[i] ?? Fallback(i / (double)zone.Leds));
                    int at = (zone.Inicio + i) * 3;
                    buffer[at] = (float)(buffer[at] * shade.Dim + shade.R * boost);
                    buffer[at + 1] = (float)(buffer[at + 1] * shade.Dim + shade.G * boost);
                    buffer[at + 2] = (float)(buffer[at + 2] * shade.Dim + shade.B * boost);
                }
            }
            animating = true;
        }
        return animating;
    }

    /// <summary>A partida do CS2 agora (null = sem reações: modo jogo desligado, reações desligadas ou CS2 fechado).</summary>
    public Func<Game.Cs2Moment?>? GameReactions { get; set; }

    /// <summary>
    /// Prepara o que as cenas do gabinete leem neste quadro (sensores, Pro Flow, Modo Mangá) e calcula a cena geral no
    /// lugar da placa de vídeo, para o LED dela poder acompanhar.
    /// </summary>
    private void PrepareScene(long now, double t, double speed, Color color)
    {
        double dt = _lastFrame == 0 ? 0 : Math.Clamp((now - _lastFrame) / 1000.0, 0, 1);
        _lastFrame = now;
        if (_screenLight() is { } light) _screen += (light.Luminance - _screen) * (1 - Math.Exp(-dt / 0.4));
        _scene.T = t;
        _scene.NowMs = now;
        _scene.Speed = speed;
        _scene.CpuTemp = _cpuTemp;
        _scene.GpuTemp = _gpuTemp;
        _scene.GpuLoad = _gpuLoad;
        _scene.Screen = _screen;
        _scene.Manga = !_offline && MangaStage.InUse ? MangaStage.Current : null;

        string effect = _settings.Efeito;
        int sample = 0;
        if (ArgbEffects.IsScene(effect))
        {
            var (u, v) = CaseLayout.GpuCenter;
            var (r, g, b) = CaseScenes.Shade(effect, color, new LedPoint(0, 0, u, v, double.NaN), _scene);
            sample = Color.FromArgb(255, ToByte((float)r), ToByte((float)g), ToByte((float)b)).ToArgb();
        }
        Volatile.Write(ref _gpuSample, sample);
    }

    /// <summary>Posições dos LEDs da parte no gabinete (guardadas enquanto a forma não muda).</summary>
    private LedPoint[]? Points(ArgbZone zone)
    {
        string key = $"{zone.Forma}|{zone.Leds}|{zone.Inverter}";
        if (_points.TryGetValue(zone, out var cached) && cached.Key == key) return cached.Points;
        var points = CaseLayout.Parse(zone.Forma, zone.Leds, zone.Inverter);
        _points[zone] = (key, points);
        return points;
    }

    /// <summary>
    /// "Identificar" numa capa de cabo: a ordem dos LEDs no fio — um degradê do vermelho (primeiro LED) ao violeta (o
    /// último) e um ponto branco correndo do primeiro ao último, 10 por segundo. A olho ou numa foto, mostra como as
    /// fileiras da capa estão ligadas (quantos LEDs por fileira, e se a seguinte volta pelo outro lado).
    /// </summary>
    private static (double R, double G, double B) ChainOrder(int led, int leds, long elapsedMs)
    {
        double dot = elapsedMs / 100.0 - 10; // começa depois de 1 s
        if (led == (int)Math.Floor(dot)) return (1, 1, 1);
        var (r, g, b) = FromHue(led / (double)Math.Max(1, leds) * 0.83);
        return (r * 0.25, g * 0.25, b * 0.25);
    }

    /// <summary>Parte sem lugar no gabinete: os LEDs numa linha no meio.</summary>
    private static LedPoint Fallback(double x) => new(0, 0, x, 0.5, double.NaN);

    /// <summary>Cor de um LED (fração de R, G, B) no efeito; x = posição ao longo da parte (0–1).</summary>
    private static (double R, double G, double B) Pixel(string effect, Color color, Color? gpu, double x, double t, double speed, int zone, int led)
    {
        double level;
        switch (effect)
        {
            case ArgbEffects.Fixed:
                level = 1;
                break;
            case ArgbEffects.Breathing:
                level = 0.1 + 0.9 * (0.5 - 0.5 * Math.Cos(2 * Math.PI * t * speed / 6.0));
                break;
            case ArgbEffects.Wave:
            {
                double head = Frac(t * speed / 3.2);
                double d = Math.Abs(x - head);
                d = Math.Min(d, 1 - d);
                level = 0.22 + 0.78 * Math.Exp(-d * d / (2 * 0.1 * 0.1));
                break;
            }
            case ArgbEffects.Comet:
            {
                double behind = Frac(t * speed / 2.4 - x);
                const double tail = 0.45;
                level = behind < tail ? Math.Pow(1 - behind / tail, 1.5) : 0;
                level = Math.Max(0.06, level);
                break;
            }
            case ArgbEffects.Twinkle:
            {
                // Cada LED com o próprio ritmo (1,6–4 s) e fase: estrelas acendendo devagar sobre um fundo fraco.
                double h1 = Hash(zone, led, 1), h2 = Hash(zone, led, 2);
                double v = Math.Sin(2 * Math.PI * (t * speed / (1.6 + 2.4 * h1) + h2));
                level = 0.16 + 0.84 * Math.Pow(Math.Max(0, v), 4);
                break;
            }
            case ArgbEffects.Rainbow:
            {
                var c = FromHue(Frac(x + t * speed / 8.0));
                return (c.R, c.G, c.B);
            }
            case ArgbEffects.Aurora:
            {
                double seed = zone * 0.37;
                double a = 0.5 + 0.5 * Math.Sin(2 * Math.PI * (x * 0.9 + t * speed / 9.0 + seed));
                double b = 0.5 + 0.5 * Math.Sin(2 * Math.PI * (x * 1.7 - t * speed / 13.0) + 1.3 + seed);
                double l = 0.45 + 0.55 * (0.5 + 0.5 * Math.Sin(2 * Math.PI * (x * 1.1 + t * speed / 7.0) + seed * 2));
                // Branco → verde-água → violeta.
                double r = Mix(Mix(1.0, 0.25, a), 0.6, b * 0.7), g = Mix(Mix(1.0, 0.94, a), 0.42, b * 0.7), bl = Mix(Mix(1.0, 0.82, a), 1.0, b * 0.7);
                double k = Linear(l);
                return (r * k, g * k, bl * k);
            }
            case ArgbEffects.GpuSync:
                // A cor que está na placa agora (já com o efeito e o brilho dela); sem o LED da placa ativo, a cor escolhida.
                if (gpu is Color shown) return (shown.R / 255.0, shown.G / 255.0, shown.B / 255.0);
                level = 1;
                break;
            case "identificar":
                level = 0.35 + 0.65 * (0.5 - 0.5 * Math.Cos(2 * Math.PI * t));
                break;
            default:
                level = 1;
                break;
        }
        double kk = Linear(level);
        return (color.R / 255.0 * kk, color.G / 255.0 * kk, color.B / 255.0 * kk);
    }

    /// <summary>
    /// Saída = transição do quadro anterior para o alvo, limitador de energia e trava contra pisca-pisca. True = ainda
    /// na transição.
    /// </summary>
    private bool Compose(long now)
    {
        double k = _fadeStart == long.MinValue ? 1 : Math.Clamp((now - _fadeStart) / (double)FadeMs, 0, 1); // MinValue = sem transição
        k = k * k * (3 - 2 * k);
        double sum = 0, luminance = 0;
        int leds = 0;
        for (int c = 0; c < NollieDevice.Channels; c++)
        {
            float[] from = _from[c], to = _target[c], output = _out[c];
            for (int i = 0; i < output.Length; i += 3)
            {
                float r = Math.Clamp((float)(from[i] + (to[i] - from[i]) * k), 0, 1);
                float g = Math.Clamp((float)(from[i + 1] + (to[i + 1] - from[i + 1]) * k), 0, 1);
                float b = Math.Clamp((float)(from[i + 2] + (to[i + 2] - from[i + 2]) * k), 0, 1);
                output[i] = r;
                output[i + 1] = g;
                output[i + 2] = b;
                sum += r + g + b;
                luminance += 0.2126 * r + 0.7152 * g + 0.0722 * b;
            }
        }
        foreach (var zone in _settings.Zonas) leds += zone.Leds;

        // Limitador de energia: a corrente estimada não passa do teto.
        double budget = _settings.LimiteAmperes * 1000.0 - leds * IdleMilliampsPerLed;
        double current = sum * MilliampsPerChannel;
        double scale = current > budget && current > 0 ? Math.Max(0, budget) / current : 1;
        if (scale < 1) _limited++;

        // Trava contra pisca-pisca na luz média de todos os LEDs.
        if (leds > 0)
        {
            double y = luminance * scale / leds;
            int gray = (int)Math.Round(Math.Clamp(y, 0, 1) * 255);
            var guarded = _guard.Apply(Color.FromArgb(gray, gray, gray), now);
            if (gray > 0 && guarded.R < gray)
            {
                scale *= guarded.R / (double)gray;
                _held++;
            }
        }
        if (scale < 1)
        {
            foreach (var channel in _out)
                for (int i = 0; i < channel.Length; i++) channel[i] = (float)(channel[i] * scale);
        }
        return k < 1;
    }

    /// <summary>Pacotes por canal (G, R, B), só até o último LED usado em cada canal; canal sem parte nenhuma fica null.</summary>
    private IReadOnlyList<byte[]?> Pack(List<ArgbZone> zones)
    {
        Span<int> extent = stackalloc int[NollieDevice.Channels];
        foreach (var zone in zones) extent[zone.Canal - 1] = Math.Max(extent[zone.Canal - 1], zone.Inicio + zone.Leds);
        // Balanço de branco de cada LED (pela parte dele): só na saída — o quadro (e o que o painel mostra) segue em branco puro.
        foreach (var zone in zones)
        {
            var (r, g, b) = WhiteBalance.Factors(zone.Balanco);
            var gains = _gains[zone.Canal - 1];
            for (int i = zone.Inicio; i < zone.Inicio + zone.Leds; i++)
            {
                gains[i * 3] = r;
                gains[i * 3 + 1] = g;
                gains[i * 3 + 2] = b;
            }
        }
        for (int c = 0; c < NollieDevice.Channels; c++)
        {
            int count = extent[c];
            if (count == 0)
            {
                _packets[c] = null;
                continue;
            }
            if (_packets[c]?.Length != count * 3) _packets[c] = new byte[count * 3];
            var packet = _packets[c]!;
            var source = _out[c];
            var gain = _gains[c];
            for (int i = 0; i < count; i++)
            {
                packet[i * 3] = ToByte(source[i * 3 + 1] * gain[i * 3 + 1]);     // G
                packet[i * 3 + 1] = ToByte(source[i * 3] * gain[i * 3]);         // R
                packet[i * 3 + 2] = ToByte(source[i * 3 + 2] * gain[i * 3 + 2]); // B
            }
        }
        return _packets;
    }

    private string[] TakeSnapshot(List<ArgbZone> zones)
    {
        var result = new string[zones.Count];
        var text = new StringBuilder();
        for (int k = 0; k < zones.Count; k++)
        {
            var zone = zones[k];
            var source = _out[zone.Canal - 1];
            text.Clear();
            for (int i = 0; i < zone.Leds; i++)
            {
                int at = (zone.Inicio + i) * 3;
                text.Append(Display(source[at])).Append(Display(source[at + 1])).Append(Display(source[at + 2]));
            }
            result[k] = text.ToString();
        }
        return result;
    }

    /// <summary>
    /// Prévia de um efeito para o painel: <paramref name="steps"/> quadros de um anel de <paramref name="leds"/> LEDs (nos
    /// efeitos de capa de cabo, uma grade de 5 fileiras), cada um como "RRGGBB" por LED (como o olho vê), com o brilho cheio.
    /// </summary>
    public static string[] Preview(string effect, Color color, string speedName, int leds, int steps, int stepMs)
    {
        double speed = speedName switch { GpuEffects.Slow => 0.6, GpuEffects.Fast => 1.6, _ => 1.0 };
        var frames = new string[steps];
        var text = new StringBuilder(leds * 6);
        var scene = new SceneInput { Speed = speed, CpuTemp = 62, GpuTemp = 58, GpuLoad = 60, Screen = 0.6 };
        // Nas cenas, o anel da amostra é uma volta pelo gabinete: em cima, na bomba, no radiador, no piso, no fan de trás.
        var ring = Enumerable.Range(0, leds).Select(i =>
        {
            double a = -Math.PI / 2 + 2 * Math.PI * i / leds;
            return new LedPoint(0, 0, 0.5 + 0.42 * Math.Cos(a), 0.5 + 0.45 * Math.Sin(a), a);
        }).ToArray();
        var grid = (5, Math.Max(1, leds / 5));
        for (int s = 0; s < steps; s++)
        {
            text.Clear();
            double t = s * stepMs / 1000.0;
            scene.T = t;
            scene.NowMs = (long)(t * 1000);
            for (int i = 0; i < leds; i++)
            {
                var (r, g, b) = effect == ArgbEffects.Off ? (0.0, 0.0, 0.0)
                    : ArgbEffects.IsScene(effect) ? CaseScenes.Shade(effect, color, ring[i], scene)
                    : ArgbEffects.IsCable(effect) ? CableEffects.Shade(effect, color, grid, i, t, speed, 0)
                    : Pixel(effect, color, null, i / (double)leds, t, speed, 0, i);
                text.Append(Display((float)r)).Append(Display((float)g)).Append(Display((float)b));
            }
            frames[s] = text.ToString();
        }
        return frames;
    }

    // ---------------------------------------------------------------- utilidades

    private static string? FindOtherController()
    {
        foreach (string name in OtherControllers)
        {
            var processes = Process.GetProcessesByName(name);
            bool found = processes.Length > 0;
            foreach (var process in processes) process.Dispose();
            if (found) return name switch
            {
                "SignalRgb" => "SignalRGB",
                "NollieRGB" => "NollieRGB",
                _ => name,
            };
        }
        return null;
    }

    /// <summary>Nível perceptivo (0–1) → fração de luz do LED.</summary>
    private static double Linear(double level) => Math.Pow(Math.Clamp(level, 0, 1), 2.2);

    /// <summary>Fração de luz → valor para a tela do PC (com gama), em hexadecimal.</summary>
    private static string Display(float v) => ((int)Math.Round(255 * Math.Pow(Math.Clamp(v, 0, 1), 1 / 2.2))).ToString("X2");

    private static byte ToByte(float v) => (byte)Math.Round(Math.Clamp(v, 0, 1) * 255);

    private static double Frac(double v) => v - Math.Floor(v);

    private static double Mix(double a, double b, double t) => a + (b - a) * t;

    private static double Hash(int zone, int led, int salt)
    {
        unchecked
        {
            uint h = (uint)(zone * 73856093) ^ (uint)(led * 19349663) ^ (uint)(salt * 83492791);
            h ^= h >> 13;
            h *= 0x5BD1E995;
            h ^= h >> 15;
            return (h & 0xFFFFFF) / (double)0x1000000;
        }
    }

    private static (double R, double G, double B) FromHue(double hue)
    {
        double h = hue * 6;
        double x = 1 - Math.Abs(h % 2 - 1);
        return (int)h switch
        {
            0 => (1, x, 0),
            1 => (x, 1, 0),
            2 => (0, 1, x),
            3 => (0, x, 1),
            4 => (x, 0, 1),
            _ => (1, 0, x),
        };
    }
}
