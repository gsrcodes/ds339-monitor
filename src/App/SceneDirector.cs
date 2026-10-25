using System.Runtime.InteropServices;
using DS339Monitor.Lighting;
using DS339Monitor.Themes;
using Microsoft.Win32;

namespace DS339Monitor.App;

/// <summary>Acontecimentos que as duas telas e o LED encenam juntos.</summary>
internal enum StageEventKind
{
    /// <summary>Raio da tempestade.</summary>
    Lightning,
    /// <summary>Virada de painel na Pro Flow (<see cref="StageEvent.Variant"/> = <see cref="PanelTurn"/>).</summary>
    Turn,
    /// <summary>O usuário voltou (desbloqueou, ou mexeu depois de muito tempo parado).</summary>
    Welcome,
    /// <summary>Download grande terminou: o Den Den Mushi toca.</summary>
    DenDenMushi,
    /// <summary>Um jogo em tela cheia abriu: o Sunny dispara o Coup de Burst.</summary>
    CoupDeBurst,
    /// <summary>GPU quente demais por um tempo: Buster Call.</summary>
    BusterCall,
}

/// <summary><see cref="At"/> é o <see cref="Clock.Ms"/> em que o acontecimento começa (pode ser um pouco no futuro).</summary>
internal readonly record struct StageEvent(StageEventKind Kind, long At, int Variant = 0, string? Text = null);

/// <summary>Retrato da cena num instante (imutável: as threads só trocam a referência).</summary>
internal sealed record StageState
{
    public static readonly StageState Empty = new() { Daylight = 1, SunArc = 0.5 };

    public long At { get; init; }
    /// <summary>0 = noite, 1 = dia (pelo sol de verdade, com transições de ~40 min no nascer e no pôr).</summary>
    public double Daylight { get; init; }
    /// <summary>0 = luz fria do meio-dia, 1 = hora dourada; à noite fica morna.</summary>
    public double Warmth { get; init; }
    /// <summary>Posição do sol entre o nascer (0) e o pôr (1).</summary>
    public double SunArc { get; init; }
    /// <summary>Volume do som do PC, suavizado (0–1).</summary>
    public double Audio { get; init; }
    /// <summary><see cref="Clock.Ms"/> da última batida percebida no som (0 = nenhuma).</summary>
    public long LastBeat { get; init; }
    public double IdleSeconds { get; init; }
    public bool Locked { get; init; }
    /// <summary>Tela bloqueada ou muito tempo sem mexer: a cena vai dormir.</summary>
    public bool Sleeping { get; init; }
    public bool Storm { get; init; }
    /// <summary>Jogo em tela cheia rodando (a cena economiza CPU para ele).</summary>
    public bool InGame { get; init; }
    public PanelScene? Panel { get; init; }
    public IReadOnlyList<StageEvent> Events { get; init; } = Array.Empty<StageEvent>();
    /// <summary>Ações do sistema dos últimos 30 s (copiar, print, pendrive, volume, microfone...).</summary>
    public IReadOnlyList<SystemSignal> Signals { get; init; } = Array.Empty<SystemSignal>();
    /// <summary>Microfone ou webcam em uso agora.</summary>
    public bool OnAir { get; init; }
    /// <summary>Volume do Windows (0–1; NaN = desconhecido) e mudo.</summary>
    public float Volume { get; init; } = float.NaN;
    public bool Muted { get; init; }
    public float CpuTemp { get; init; } = float.NaN;
    public float GpuTemp { get; init; } = float.NaN;

    /// <summary>Último acontecimento do tipo pedido que já começou, ou null.</summary>
    public StageEvent? Last(StageEventKind kind, long now)
    {
        for (int i = Events.Count - 1; i >= 0; i--)
            if (Events[i].Kind == kind && Events[i].At <= now) return Events[i];
        return null;
    }
}

/// <summary>
/// Diretor de cena do "Modo Grand Line": junta o que acontece no mundo e no PC — o sol de verdade (nascer e pôr do sol
/// calculados para a posição configurada, sem internet), o volume do som (o medidor de pico do Windows, sem capturar
/// áudio nenhum), se o usuário está ali (tempo sem mexer, tela bloqueada), jogos, downloads, a temperatura da GPU,
/// tempestades e o painel que está na Pro Flow — num <see cref="StageState"/> que as duas telas e o LED leem no mesmo
/// instante. Roda na thread de monitoramento; cada detector custa microssegundos.
/// </summary>
internal sealed class SceneDirector : IDisposable
{
    private const long EventMemoryMs = 30_000;
    private const double SleepAfterSeconds = 300;
    private const long BeatGapMs = 340;           // no máximo ~3 batidas por segundo: nunca vira pisca-pisca
    private const long DetectorIntervalMs = 2000;

    private static volatile StageState _current = StageState.Empty;

    private readonly AudioMeter _audio = new();
    private readonly PolledSignals _polled = new();
    private readonly List<SystemSignal> _signals = new();
    private long _nextVolume;
    private readonly List<StageEvent> _events = new();
    private readonly Random _random = new();
    private double _latitude = -23.55, _longitude = -46.63;
    private long _last;
    private double _level, _average;
    private long _lastBeat;
    private volatile bool _locked;
    private volatile bool _welcomePending;
    private bool _wasSleeping;
    private int _panel = -1;
    private bool _turning;
    private long _nextDetectors;
    private bool _inGame;
    private int _gameCandidate;
    private double _downloadBytes, _downloadSeconds, _quietSeconds;
    private int _hotSeconds;
    private long _lastBusterCall = long.MinValue / 2;
    private long _nextLightning;
    private bool _subscribed;
    private float _cpuTemp = float.NaN, _gpuTemp = float.NaN, _gpuLoad = float.NaN;
    private float _volumeLevel = float.NaN;
    private bool _muted;
    private int _onAir;
    private bool _displayOff;

    /// <summary>O retrato mais recente (qualquer thread pode ler).</summary>
    public static StageState Current => _current;

    /// <summary>Alguém está usando a cena (tema ou efeito Grand Line); sem isso, o diretor não faz nada.</summary>
    public static volatile bool InUse;

    /// <summary>Sensores que a cena precisa quando está em uso.</summary>
    public static readonly string[] SensorKeys = { "GPUTemp", "GpuUsage", "CPUTemp", "NetDownBytes" };

    public void Configure(double latitude, double longitude)
    {
        _latitude = Math.Clamp(latitude, -66, 66);
        _longitude = Math.Clamp(longitude, -180, 180);
    }

    /// <summary>Leituras de um segundo (temperaturas, uso e rede).</summary>
    public void UpdateSensors(double? cpuTemp, double? gpuTemp, double? gpuLoad, double? downBytesPerSecond)
    {
        _cpuTemp = cpuTemp is double c ? (float)c : float.NaN;
        _gpuTemp = gpuTemp is double g ? (float)g : float.NaN;
        _gpuLoad = gpuLoad is double l ? (float)l : float.NaN;
        TrackDownload(downBytesPerSecond ?? 0);
        TrackHeat();
    }

    /// <summary>Um passo (a cada quadro da animação, ~30 por segundo).</summary>
    public void Update(ScreenLight? proFlow)
    {
        long now = Clock.Ms;
        double dt = _last == 0 ? 0 : Math.Clamp((now - _last) / 1000.0, 0, 1);
        _last = now;
        if (!_subscribed)
        {
            _subscribed = true;
            SystemEvents.SessionSwitch += OnSessionSwitch;
        }

        // Ações do sistema: o que chegou pela fila e o que muda sem avisar (volume, microfone, Spotify).
        if (now >= _nextVolume)
        {
            _nextVolume = now + 100;
            var (volume, muted) = _audio.Volume();
            _polled.Volume(volume, muted);
            _volumeLevel = volume;
            _muted = muted;
        }
        _polled.Poll(now);
        while (SystemSignals.TryTake(out var signal))
        {
            _signals.Add(signal);
            if (signal.Kind is SignalKind.MicOn or SignalKind.CameraOn) _onAir++;
            else if (signal.Kind is SignalKind.MicOff or SignalKind.CameraOff) _onAir = Math.Max(0, _onAir - 1);
            else if (signal.Kind == SignalKind.Resumed) Add(StageEventKind.Welcome, now + 300);
            else if (signal.Kind == SignalKind.DisplayOff) _displayOff = true;
            else if (signal.Kind == SignalKind.DisplayOn) _displayOff = false;
        }
        _signals.RemoveAll(s => now - s.At > EventMemoryMs);

        // Som: volume suavizado (sobe rápido, desce devagar) e batidas por pico acima da média recente.
        float peak = _audio.Peak(now);
        _level += (peak - _level) * (1 - Math.Exp(-dt / (peak > _level ? 0.05 : 0.4)));
        _average += (peak - _average) * (1 - Math.Exp(-dt / 1.5));
        if (peak > 0.12 && peak > _average * 1.6 && now - _lastBeat >= BeatGapMs) _lastBeat = now;

        // Presença: tempo sem mexer e tela bloqueada. Voltar depois de dormir é um acontecimento.
        double idle = IdleSeconds();
        bool sleeping = _locked || _displayOff || idle >= SleepAfterSeconds;
        if ((_wasSleeping && !sleeping) || _welcomePending)
        {
            _welcomePending = false;
            if (!sleeping) Add(StageEventKind.Welcome, now + 100);
        }
        _wasSleeping = sleeping;

        // Virada de painel na Pro Flow, com o instante exato em que começou.
        var panel = proFlow is not null && now - proFlow.At < 1500 ? proFlow.Panel : null;
        if (panel is PanelScene scene)
        {
            if ((scene.Panel != _panel || scene.Turning != _turning) && scene.Turning && _panel >= 0 && scene.Seconds < 0.5)
                Add(StageEventKind.Turn, now - (long)(scene.Seconds * 1000), (int)scene.Turn);
            _panel = scene.Panel;
            _turning = scene.Turning;
        }

        var (daylight, warmth, arc) = Sun(DateTime.Now);
        bool storm = StormActive(DateTime.Now);
        if (storm && !sleeping && now >= _nextLightning)
        {
            if (_nextLightning != 0) Add(StageEventKind.Lightning, now + 150, _random.Next(3));
            _nextLightning = now + 7000 + _random.Next(15000);
        }

        if (now >= _nextDetectors)
        {
            _nextDetectors = now + DetectorIntervalMs;
            TrackGame(now);
        }

        StageEvent[] events;
        lock (_events)
        {
            _events.RemoveAll(e => now - e.At > EventMemoryMs);
            events = _events.ToArray();
        }
        _current = new StageState
        {
            At = now,
            Daylight = daylight,
            Warmth = warmth,
            SunArc = arc,
            Audio = Math.Clamp(_level * 1.6, 0, 1),
            LastBeat = _lastBeat,
            IdleSeconds = idle,
            Locked = _locked,
            Sleeping = sleeping,
            Storm = storm,
            InGame = _inGame,
            Signals = _signals.ToArray(),
            OnAir = _onAir > 0,
            Volume = _volumeLevel,
            Muted = _muted,
            Panel = panel,
            Events = events,
            CpuTemp = _cpuTemp,
            GpuTemp = _gpuTemp,
        };
    }

    /// <summary>Acontecimento de teste (menu "Testar"): começa já.</summary>
    public void Trigger(StageEventKind kind, string? text = null) => Add(kind, Clock.Ms + 100, 0, text);

    public void Dispose()
    {
        if (_subscribed) SystemEvents.SessionSwitch -= OnSessionSwitch;
        _audio.Dispose();
    }

    private void Add(StageEventKind kind, long at, int variant = 0, string? text = null)
    {
        lock (_events) _events.Add(new StageEvent(kind, at, variant, text));
    }

    private void OnSessionSwitch(object? sender, SessionSwitchEventArgs e)
    {
        if (e.Reason is SessionSwitchReason.SessionLock or SessionSwitchReason.ConsoleDisconnect or SessionSwitchReason.RemoteDisconnect)
        {
            _locked = true;
        }
        else if (e.Reason is SessionSwitchReason.SessionUnlock or SessionSwitchReason.ConsoleConnect or SessionSwitchReason.SessionLogon)
        {
            _locked = false;
            _welcomePending = true;
        }
    }

    // ---------------------------------------------------------------- detectores

    /// <summary>
    /// Jogo em tela cheia: o shell diz que há um app Direct3D em tela cheia (ou "ocupado", que inclui tela cheia sem
    /// borda) e a GPU está trabalhando — vídeo em tela cheia não puxa a GPU assim. Precisa se manter por 4 s.
    /// </summary>
    private void TrackGame(long now)
    {
        bool fullscreen = SHQueryUserNotificationState(out int state) == 0 && state is 2 or 3;
        bool busy = !float.IsNaN(_gpuLoad) && _gpuLoad >= 50;
        _gameCandidate = fullscreen && busy ? _gameCandidate + 1 : 0;
        bool inGame = _gameCandidate >= 2 || (_inGame && fullscreen);
        if (inGame && !_inGame) Add(StageEventKind.CoupDeBurst, now + 200);
        _inGame = inGame;
    }

    /// <summary>Download grande (≥ 2 MB/s por 8 s e ≥ 200 MB) que parou (&lt; 100 KB/s por 5 s): o Den Den Mushi toca.</summary>
    private void TrackDownload(double bytesPerSecond)
    {
        if (bytesPerSecond >= 2_000_000)
        {
            _downloadBytes += bytesPerSecond;
            _downloadSeconds++;
            _quietSeconds = 0;
            return;
        }
        if (bytesPerSecond < 100_000) _quietSeconds++;
        if (_downloadSeconds >= 8 && _downloadBytes >= 200_000_000 && _quietSeconds >= 5)
        {
            Add(StageEventKind.DenDenMushi, Clock.Ms + 100, 0, $"Download concluído — {_downloadBytes / 1_073_741_824:0.0} GB");
            _downloadBytes = _downloadSeconds = 0;
        }
        else if (_quietSeconds >= 30)
        {
            _downloadBytes = _downloadSeconds = 0;
        }
    }

    /// <summary>GPU ≥ 86 °C por 10 s: Buster Call (no máximo a cada 10 min; rearma abaixo de 80 °C).</summary>
    private void TrackHeat()
    {
        if (float.IsNaN(_gpuTemp)) return;
        if (_gpuTemp < 80) _hotSeconds = 0;
        else if (_gpuTemp >= 86) _hotSeconds++;
        long now = Clock.Ms;
        if (_hotSeconds >= 10 && now - _lastBusterCall >= 600_000)
        {
            _lastBusterCall = now;
            Add(StageEventKind.BusterCall, now + 100, 0, $"GPU {_gpuTemp:0}°C");
        }
    }

    /// <summary>
    /// Tempestades: em ~30% das horas, uma janela de 4 min sorteada pela própria hora (todo mundo que olha a cena chega
    /// à mesma conclusão, sem combinar nada).
    /// </summary>
    private static bool StormActive(DateTime time)
    {
        long hour = (long)(time - DateTime.UnixEpoch).TotalHours;
        ulong h = (ulong)hour * 0x9E3779B97F4A7C15UL;
        h ^= h >> 29;
        if (h % 100 >= 30) return false;
        int start = (int)(h / 100 % 52);
        return time.Minute >= start && time.Minute < start + 4;
    }

    /// <summary>
    /// Sol pelo algoritmo da NOAA (bom para alguns minutos): nascer e pôr do sol no fuso local, e daí a claridade (com
    /// ~40 min de transição de cada lado), o calor da luz (hora dourada e noite) e a posição do sol no céu.
    /// </summary>
    private (double Daylight, double Warmth, double Arc) Sun(DateTime local)
    {
        double gamma = 2 * Math.PI / 365 * (local.DayOfYear - 1);
        double eqTime = 229.18 * (0.000075 + 0.001868 * Math.Cos(gamma) - 0.032077 * Math.Sin(gamma)
            - 0.014615 * Math.Cos(2 * gamma) - 0.040849 * Math.Sin(2 * gamma));
        double decl = 0.006918 - 0.399912 * Math.Cos(gamma) + 0.070257 * Math.Sin(gamma) - 0.006758 * Math.Cos(2 * gamma)
            + 0.000907 * Math.Sin(2 * gamma) - 0.002697 * Math.Cos(3 * gamma) + 0.00148 * Math.Sin(3 * gamma);
        double lat = _latitude * Math.PI / 180;
        double cosHa = Math.Cos(90.833 * Math.PI / 180) / (Math.Cos(lat) * Math.Cos(decl)) - Math.Tan(lat) * Math.Tan(decl);
        double ha = Math.Acos(Math.Clamp(cosHa, -1, 1)) * 180 / Math.PI;
        double offset = TimeZoneInfo.Local.GetUtcOffset(local).TotalMinutes;
        double sunrise = 720 - 4 * (_longitude + ha) - eqTime + offset;
        double sunset = 720 - 4 * (_longitude - ha) - eqTime + offset;
        double minutes = local.TimeOfDay.TotalMinutes;

        double daylight = Smooth(sunrise - 40, sunrise + 40, minutes) * (1 - Smooth(sunset - 40, sunset + 40, minutes));
        double golden = Math.Max(Bump(minutes, sunrise, 60), Bump(minutes, sunset, 60));
        double warmth = Math.Max(golden, (1 - daylight) * 0.55);
        double arc = Math.Clamp((minutes - sunrise) / (sunset - sunrise), 0, 1);
        return (daylight, warmth, arc);
    }

    private static double Bump(double x, double center, double width)
    {
        double d = Math.Abs(x - center) / width;
        return d >= 1 ? 0 : 0.5 + 0.5 * Math.Cos(Math.PI * d);
    }

    private static double Smooth(double from, double to, double x)
    {
        double t = Math.Clamp((x - from) / (to - from), 0, 1);
        return t * t * (3 - 2 * t);
    }

    private static double IdleSeconds()
    {
        var info = new LastInputInfo { Size = (uint)Marshal.SizeOf<LastInputInfo>() };
        if (!GetLastInputInfo(ref info)) return 0;
        return unchecked((uint)Environment.TickCount - info.Time) / 1000.0;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct LastInputInfo
    {
        public uint Size;
        public uint Time;
    }

    [DllImport("user32.dll")]
    private static extern bool GetLastInputInfo(ref LastInputInfo info);

    [DllImport("shell32.dll")]
    private static extern int SHQueryUserNotificationState(out int state);

    /// <summary>
    /// Volume do que está tocando, pelo medidor de pico do Windows (IAudioMeterInformation do dispositivo de saída
    /// padrão) — o mesmo do mixer de volume. Não captura áudio nenhum. Reabre a cada 10 s para seguir a troca de
    /// dispositivo (fone, caixa); sem som disponível, devolve 0.
    /// </summary>
    private sealed unsafe class AudioMeter : IDisposable
    {
        private static readonly Guid EnumeratorClass = new("BCDE0395-E52F-467C-8E3D-C4579291692E");
        private static readonly Guid EnumeratorInterface = new("A95664D2-9614-4F35-A746-DE8DB63617E6");
        private static readonly Guid MeterInterface = new("C02216F6-8C67-4B5B-9D00-D008E73E0064");
        private static readonly Guid VolumeInterface = new("5CDF2C82-841E-4546-9722-0CF74078229A");
        private IntPtr _meter, _volume;
        private long _reopenAt;

        /// <summary>Volume do Windows (0–1) e mudo, lidos agora (NaN sem dispositivo).</summary>
        public (float Level, bool Muted) Volume()
        {
            if (_volume == IntPtr.Zero) return (float.NaN, false);
            float level;
            int muted;
            // IAudioEndpointVolume::GetMasterVolumeLevelScalar (9) e GetMute (15)
            if (((delegate* unmanaged[Stdcall]<IntPtr, float*, int>)(*(void***)_volume)[9])(_volume, &level) < 0) return (float.NaN, false);
            if (((delegate* unmanaged[Stdcall]<IntPtr, int*, int>)(*(void***)_volume)[15])(_volume, &muted) < 0) return (float.NaN, false);
            return (level, muted != 0);
        }

        public float Peak(long now)
        {
            if (now >= _reopenAt)
            {
                _reopenAt = now + 10_000;
                Release(ref _meter);
                Release(ref _volume);
                (_meter, _volume) = Open();
            }
            if (_meter == IntPtr.Zero) return 0;
            float value;
            int hr = ((delegate* unmanaged[Stdcall]<IntPtr, float*, int>)(*(void***)_meter)[3])(_meter, &value);
            if (hr < 0)
            {
                Release(ref _meter);
                return 0;
            }
            return value;
        }

        public void Dispose()
        {
            Release(ref _meter);
            Release(ref _volume);
        }

        private static (IntPtr Meter, IntPtr Volume) Open()
        {
            CoInitializeEx(IntPtr.Zero, 0); // MTA; se já estiver iniciado, tanto faz
            IntPtr enumerator = IntPtr.Zero, device = IntPtr.Zero, meter = IntPtr.Zero, volume = IntPtr.Zero;
            try
            {
                var classId = EnumeratorClass;
                var interfaceId = EnumeratorInterface;
                if (CoCreateInstance(&classId, IntPtr.Zero, 0x17, &interfaceId, &enumerator) < 0) return (IntPtr.Zero, IntPtr.Zero);
                // IMMDeviceEnumerator::GetDefaultAudioEndpoint(eRender, eMultimedia, &device)
                if (((delegate* unmanaged[Stdcall]<IntPtr, int, int, IntPtr*, int>)(*(void***)enumerator)[4])(enumerator, 0, 1, &device) < 0)
                    return (IntPtr.Zero, IntPtr.Zero);
                var meterId = MeterInterface;
                var volumeId = VolumeInterface;
                var activate = (delegate* unmanaged[Stdcall]<IntPtr, Guid*, uint, IntPtr, IntPtr*, int>)(*(void***)device)[3];
                // IMMDevice::Activate(IID, CLSCTX_ALL, NULL, &objeto)
                if (activate(device, &meterId, 0x17, IntPtr.Zero, &meter) < 0) meter = IntPtr.Zero;
                if (activate(device, &volumeId, 0x17, IntPtr.Zero, &volume) < 0) volume = IntPtr.Zero;
                return (meter, volume);
            }
            finally
            {
                Release(ref device);
                Release(ref enumerator);
            }
        }

        private static void Release(ref IntPtr obj)
        {
            if (obj != IntPtr.Zero) ((delegate* unmanaged[Stdcall]<IntPtr, uint>)(*(void***)obj)[2])(obj);
            obj = IntPtr.Zero;
        }

        [DllImport("ole32.dll")]
        private static extern int CoInitializeEx(IntPtr reserved, uint mode);

        [DllImport("ole32.dll")]
        private static extern int CoCreateInstance(Guid* classId, IntPtr outer, uint context, Guid* interfaceId, IntPtr* result);
    }
}
