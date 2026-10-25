using System.Diagnostics;
using System.Drawing;
using DS339Monitor.App;

namespace DS339Monitor.Lighting;

/// <summary>Efeitos do LED da placa de vídeo (calculados aqui e enviados pelo modo direto, várias vezes por segundo).</summary>
internal static class GpuEffects
{
    public const string Off = "Desligado";
    public const string Fixed = "Cor fixa";
    public const string Breathing = "Respiração";
    public const string Rainbow = "Arco-íris";
    public const string Aurora = "Aurora";
    public const string Temperature = "Temperatura da GPU";
    public const string Load = "Uso da GPU";
    public const string Page = "Luz da página";
    public const string Manga = "Mangá narrado";
    public const string Drums = "Tambores da Libertação";
    public const string GrandLine = "Grand Line";
    public const string Candle = "Vela";
    public const string LivingTide = "Maré viva";
    public const string DistantStorm = "Relâmpago distante";
    public const string SunCycle = "Ciclo do sol";
    public const string Lighthouse = "Farol das horas";
    public const string SoundPulse = "Pulso do som";
    public const string Fireflies = "Vaga-lumes";
    public const string StarrySky = "Céu estrelado";
    public const string Clouds = "Sombra de nuvens";
    public const string Drizzle = "Garoa";
    public const string Fireworks = "Fogos de artifício";
    public const string Fluorescent = "Lâmpada fluorescente";
    public const string Heartbeat = "Coração da máquina";
    public const string SleepBreath = "Respiro (sono)";
    public const string GuidedBreath = "Respiração guiada 4-7-8";
    public const string Pomodoro = "Pomodoro (foco e pausa)";
    public const string Ember = "Brasa (aviva ao digitar)";
    public const string Turbine = "Turbina (uso da GPU)";
    public const string FansScene = "Igual aos fans";

    /// <summary>
    /// Grupos do menu: cores, a cena completa, brancos com a Pro Flow, brancos da natureza, brancos de ritmo e brancos
    /// que reagem ao PC.
    /// </summary>
    public static readonly string[][] Groups =
    {
        new[] { Off, Fixed, Breathing, Rainbow, Aurora, Temperature, Load },
        new[] { GrandLine, FansScene },
        new[] { Page, Manga, Drums },
        new[] { Candle, LivingTide, DistantStorm, Fireflies, StarrySky, Clouds, Drizzle, Fireworks, Fluorescent },
        new[] { Heartbeat, SleepBreath, GuidedBreath, Pomodoro, Lighthouse },
        new[] { SunCycle, SoundPulse, Ember, Turbine },
    };

    public static readonly string[] All = Groups.SelectMany(g => g).ToArray();

    public const string Slow = "Lenta";
    public const string Normal = "Normal";
    public const string Fast = "Rápida";
    public static readonly string[] Speeds = { Slow, Normal, Fast };

    public const string WhiteCard = "Igual ao da placa";
    public const string WhiteNeutral = "Neutro";
    public const string WhiteWarm = "Quente";
    public const string WhiteCool = "Frio";
    public static readonly string[] WhiteTones = { WhiteCard, WhiteNeutral, WhiteWarm, WhiteCool };

    /// <summary>Efeitos que usam a cor escolhida pelo usuário.</summary>
    public static bool UsesColor(string effect) => effect is Fixed or Breathing;

    public static bool UsesSpeed(string effect) => effect is Breathing or Rainbow or Aurora or Drums or Fireflies or StarrySky or Drizzle or Fireworks;

    /// <summary>Efeitos só em branco (no tom escolhido), para setups brancos.</summary>
    public static bool UsesWhite(string effect) =>
        effect is Page or Manga or Drums or GrandLine or Candle or LivingTide or DistantStorm or SunCycle or Lighthouse or SoundPulse
            or Fireflies or StarrySky or Clouds or Drizzle or Fireworks or Fluorescent or Heartbeat or SleepBreath or GuidedBreath
            or Pomodoro or Ember or Turbine or FansScene;

    /// <summary>Efeitos que acompanham a imagem da Pro Flow.</summary>
    public static bool UsesScreen(string effect) => effect is Page or Manga or GrandLine;

    /// <summary>Efeitos que precisam do diretor de cena (sol, som, presença, acontecimentos).</summary>
    public static bool UsesScene(string effect) => effect is GrandLine or SunCycle or SoundPulse;
}

/// <summary>
/// Controla o LED da placa de vídeo numa thread própria (as escritas I2C levam ~2 ms cada e não podem atrasar a animação
/// das telas). Supervisiona tudo o que pode dar errado:
/// <list type="bullet">
/// <item>Polychrome ou OpenRGB abertos → pausa sem escrever nada (o Polychrome pode até regravar o firmware do
/// controlador: nenhuma escrita nossa pode cruzar com isso);</item>
/// <item>o SignalRGB pode ficar aberto (ele cuida dos outros dispositivos), mas se ele — ou qualquer outro — mexer no LED
/// da placa, a conferência periódica percebe (a cada 2 s o estado é lido de volta) e o efeito pausa por 1 min, sem
/// brigar: no SignalRGB, a placa de vídeo tem que estar desativada;</item>
/// <item>a primeira escrita de cada sessão é conferida por leitura em cada canal; se não bater, desliga até o usuário
/// escolher de novo;</item>
/// <item>3 erros seguidos → solta a placa, espera 30 s e identifica tudo de novo antes de voltar;</item>
/// <item>ao desligar o efeito ou fechar o programa, devolve as cores que a placa tinha.</item>
/// </list>
/// Só escreve quando a cor muda, no máximo 25 vezes por segundo (um comando vale para a placa inteira).
/// </summary>
internal sealed class GpuLighting : IDisposable
{
    private const int FrameMs = 40;
    private const int IdleMs = 500;
    private const int RetryMs = 30_000;
    private const int OtherAppCheckMs = 3000;
    private const int MaxConsecutiveErrors = 3;
    private const int GuardMs = 2000;          // de quanto em quanto tempo o estado é lido de volta
    private const int ForeignPauseMs = 60_000; // pausa quando outro programa mexeu no LED
    private static readonly string[] OtherControllers = { "OpenRGB", "AsrPolychromeRGB" };

    private readonly GpuLedSettings _settings;
    private readonly LedEffectEngine _engine;
    private readonly Thread _thread;
    private readonly AutoResetEvent _wake = new(false);
    private volatile bool _stop;
    private int _version;
    private int _keysChanged = 1;
    private volatile float _gpuTemp = float.NaN;
    private volatile float _gpuLoad = float.NaN;
    private volatile ScreenLight? _screen;
    private volatile string _status = "desligado";
    private int _writes, _failures; // da sessão atual (só a thread do LED mexe)

    public GpuLighting(GpuLedSettings settings)
    {
        _settings = settings;
        _engine = new LedEffectEngine(settings);
        _thread = new Thread(Run) { IsBackground = true, Name = "LED da GPU", Priority = ThreadPriority.BelowNormal };
    }

    /// <summary>Situação para o menu (curta).</summary>
    public string Status => _status;

    /// <summary>Sensores de que o efeito atual precisa (a thread de monitoramento lê e repassa).</summary>
    public IEnumerable<string> SensorKeys => _settings.Efeito switch
    {
        GpuEffects.Temperature => new[] { "GPUTemp" },
        GpuEffects.Load or GpuEffects.Heartbeat => new[] { "GpuUsage" },
        GpuEffects.Turbine => new[] { "GpuUsage", "GPUTemp" },
        _ => Array.Empty<string>(),
    };

    /// <summary>A partida do CS2 por cima do efeito do LED.</summary>
    public Func<Game.Cs2Moment?>? GameReactions
    {
        set => _engine.GameReactions = value;
    }

    /// <summary>De onde vem a cor do efeito "Igual aos fans" (a cena dos fans no lugar da placa).</summary>
    public Func<Color?>? FansSample
    {
        set => _engine.FansSample = value;
    }

    /// <summary>O efeito atual acompanha a imagem da Pro Flow (a thread de monitoramento mede e repassa).</summary>
    public bool WantsScreen => GpuEffects.UsesScreen(_settings.Efeito);

    public void Start() => _thread.Start();

    /// <summary>Configuração mudou pelo menu.</summary>
    public void Reload()
    {
        Interlocked.Increment(ref _version);
        Interlocked.Exchange(ref _keysChanged, 1);
        _wake.Set();
    }

    /// <summary>True uma vez depois de cada mudança de efeito (os sensores lidos podem ter mudado).</summary>
    public bool TakeSensorKeysChanged() => Interlocked.Exchange(ref _keysChanged, 0) == 1;

    public void UpdateSensors(double? gpuTemp, double? gpuLoad)
    {
        _gpuTemp = gpuTemp is double t ? (float)t : float.NaN;
        _gpuLoad = gpuLoad is double l ? (float)l : float.NaN;
    }

    /// <summary>Último quadro da Pro Flow medido (null = sem tela).</summary>
    public void UpdateScreen(ScreenLight? light) => _screen = light;

    private volatile bool _blackout;
    private int _shown;
    private int _calibration;

    /// <summary>
    /// "Comparar brancos": o LED mostra esta cor (já com o balanço de branco), qualquer que seja o efeito. Null = efeito.
    /// </summary>
    public Color? CalibrationWhite
    {
        set
        {
            Volatile.Write(ref _calibration, value?.ToArgb() ?? 0);
            _wake.Set();
        }
    }

    /// <summary>Um ajuste (ex.: o balanço de branco) mudou: aplica já, sem esperar o próximo quadro.</summary>
    public void Nudge() => _wake.Set();

    /// <summary>A cor que está na placa agora (transparente = o programa não está mandando no LED).</summary>
    public Color Shown => Color.FromArgb(Volatile.Read(ref _shown));

    /// <summary>
    /// "Desligar tudo": o LED fica preto (pelo mesmo modo direto dos efeitos), qualquer que seja o efeito escolhido.
    /// Ao voltar, o efeito continua de onde estava. Ao fechar o programa, as cores originais voltam como sempre.
    /// </summary>
    public bool Blackout
    {
        get => _blackout;
        set
        {
            _blackout = value;
            _wake.Set();
        }
    }

    /// <summary>Para a thread e devolve as cores originais (idempotente).</summary>
    public void Stop()
    {
        if (_stop) return;
        _stop = true;
        _wake.Set();
        if (_thread.IsAlive && !_thread.Join(5000)) Log.Warn("LED da GPU: a thread não terminou a tempo.");
    }

    public void Dispose()
    {
        Stop();
        _wake.Dispose();
    }

    private void Run()
    {
        AsrockGpuLed? led = null;
        Color? written = null;
        bool verified = false;
        int errors = 0;
        long retryAt = 0, nextCheck = 0;
        string? otherApp = null;
        string? lastReason = null;
        int seenVersion = -1;
        bool disabled = false;           // verificação falhou: só volta quando o usuário escolher de novo
        Color? attempted = null;         // última cor enviada (mesmo que o driver tenha acusado erro)
        long foreignUntil = 0, nextGuard = 0;
        bool foreignLogged = false;
        bool wasBlackout = false;

        try
        {
            while (!_stop)
            {
                long now = Clock.Ms;
                int version = Volatile.Read(ref _version);
                if (version != seenVersion)
                {
                    seenVersion = version;
                    disabled = false;
                    retryAt = 0;
                    foreignUntil = 0;
                }

                string effect = _settings.Efeito;
                bool blackout = _blackout;
                int calibration = Volatile.Read(ref _calibration);
                if (disabled || (effect == GpuEffects.Off && !blackout && calibration == 0))
                {
                    if (led is not null)
                    {
                        if (now < foreignUntil) led.Dispose(); // outro programa está mandando no LED: não briga
                        else RestoreAndClose(led);
                        led = null;
                    }
                    if (!disabled) _status = "desligado";
                    Volatile.Write(ref _shown, 0);
                    _wake.WaitOne();
                    continue;
                }

                if (now >= nextCheck)
                {
                    nextCheck = now + OtherAppCheckMs;
                    string? found = FindOtherController();
                    if (found != otherApp)
                    {
                        otherApp = found;
                        if (found is not null) Log.Info($"LED da GPU: pausado — {found} está aberto (só um programa pode controlar o LED).");
                        else Log.Info("LED da GPU: nenhum outro programa de RGB aberto; retomando.");
                    }
                }
                if (otherApp is not null)
                {
                    if (led is not null)
                    {
                        led.Dispose(); // o outro programa manda agora: não escreve nem restaura
                        led = null;
                    }
                    _status = $"pausado ({otherApp} aberto)";
                    Volatile.Write(ref _shown, 0);
                    _wake.WaitOne(OtherAppCheckMs);
                    continue;
                }

                if (led is null)
                {
                    if (now < retryAt)
                    {
                        _wake.WaitOne((int)Math.Min(retryAt - now, OtherAppCheckMs));
                        continue;
                    }
                    led = AsrockGpuLed.TryOpen(out string reason);
                    if (led is null)
                    {
                        if (reason != lastReason) Log.Warn($"LED da GPU: não vou controlar — {reason}.");
                        lastReason = reason;
                        _status = "não suportado";
                        retryAt = now + RetryMs;
                        continue;
                    }
                    lastReason = null;
                    Log.Info($"LED da GPU: ASRock RX 6900 XT — {led.Description}. Estado original: " +
                        string.Join("; ", led.Channels.Select(ch => $"canal {ch}: {led.Original[ch]}")));
                    written = null;
                    verified = false;
                    errors = 0;
                    _writes = _failures = 0;
                    var start = led.Original[led.Channels[0]].Color;
                    _engine.Reset(start, start);
                }

                if (now < foreignUntil)
                {
                    // Continua com a placa aberta (as cores originais da abertura valem para a devolução), só não escreve.
                    _status = "pausado (outro programa mexeu no LED)";
                    _wake.WaitOne((int)Math.Min(foreignUntil - now, OtherAppCheckMs));
                    continue;
                }

                Color color;
                bool animating;
                if (blackout)
                {
                    color = Color.Black;
                    animating = false;
                }
                else if (calibration != 0)
                {
                    color = Color.FromArgb(calibration);
                    animating = false;
                }
                else
                {
                    color = _engine.Next(now, _gpuTemp, _gpuLoad, _screen, out animating);
                }
                // O que o efeito quer mostrar (para o painel e para os fans que acompanham a placa) e o que vai para o
                // LED, com o balanço de branco.
                var intended = color;
                color = WhiteBalance.Apply(color, _settings.Balanco);

                if (written != color)
                {
                    _writes++;
                    attempted = color;
                    if (!led.SetColor(color.R, color.G, color.B))
                    {
                        errors++;
                        _failures++;
                    }
                    else
                    {
                        errors = 0;
                        written = color;
                        Volatile.Write(ref _shown, intended.ToArgb());
                        if (!verified)
                        {
                            // A primeira escrita da sessão é conferida: todo canal tem que devolver exatamente o enviado.
                            var expected = led.DirectState(color.R, color.G, color.B);
                            foreach (int channel in led.Channels)
                            {
                                var back = led.Read(channel);
                                if (back == expected) continue;
                                Log.Warn($"LED da GPU: a conferência falhou no canal {channel} (enviado {expected}, lido {back?.ToString() ?? "nada"}). Desligado por segurança.");
                                disabled = true;
                                _status = "desligado (conferência falhou)";
                                break;
                            }
                            if (!disabled)
                            {
                                verified = true;
                                Log.Info("LED da GPU: primeira escrita conferida; efeito ativo.");
                            }
                        }
                    }
                }

                if (disabled) continue; // restaura e para no início do próximo ciclo
                if (blackout != wasBlackout && written == color)
                {
                    wasBlackout = blackout;
                    Log.Info(blackout ? "LED da GPU: apagado (desligar tudo)." : $"LED da GPU: aceso de novo ({effect}).");
                }

                if (verified && written is Color shown && now >= nextGuard)
                {
                    // Outro programa escrevendo no LED (ex.: SignalRGB com a placa ativada) tira o controlador do modo
                    // direto ou troca a cor: o que está lá tem que ser o que mandamos por último.
                    nextGuard = now + GuardMs;
                    if (led.Read(led.Channels[0]) is LedChannelState state &&
                        state != led.DirectState(shown.R, shown.G, shown.B) &&
                        !(attempted is Color last && state == led.DirectState(last.R, last.G, last.B)))
                    {
                        if (!foreignLogged)
                            Log.Warn($"LED da GPU: outro programa mudou o LED (lido {state}); pausado por {ForeignPauseMs / 1000} s. " +
                                "Se for o SignalRGB, desative a placa de vídeo nele para os dois não brigarem.");
                        foreignLogged = true;
                        foreignUntil = now + ForeignPauseMs;
                        written = null;   // ao voltar, manda de novo e confere
                        verified = false;
                        continue;
                    }
                }
                if (errors >= MaxConsecutiveErrors)
                {
                    Log.Warn($"LED da GPU: {errors} erros seguidos do driver; tentando de novo em {RetryMs / 1000} s.");
                    led.Dispose();
                    led = null;
                    errors = 0;
                    retryAt = now + RetryMs;
                    _status = "sem resposta (tentando de novo)";
                    continue;
                }

                _status = blackout ? "apagado (desligar tudo)" : $"{effect.ToLowerInvariant()}";
                _wake.WaitOne(animating ? FrameMs : IdleMs);
            }
        }
        catch (Exception ex)
        {
            Log.Error("LED da GPU", ex);
            _status = "erro (veja o log)";
        }
        finally
        {
            if (led is not null)
            {
                if (otherApp is null && Clock.Ms >= foreignUntil) RestoreAndClose(led);
                else led.Dispose();
            }
        }
    }

    private void RestoreAndClose(AsrockGpuLed led)
    {
        bool ok = led.RestoreAll();
        int holds = _engine.TakeFlashHolds();
        if (ok) Log.Info($"LED da GPU: cores originais devolvidas ({_writes} escritas na sessão, {_failures} com erro" +
            (holds > 0 ? $"; a trava contra pisca-pisca segurou a luz {holds} vezes)." : ")."));
        else Log.Warn("LED da GPU: não consegui confirmar a devolução das cores originais.");
        led.Dispose();
    }

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
                "AsrPolychromeRGB" => "Polychrome",
                _ => name,
            };
        }
        return null;
    }

    public static Color ParseColor(string? hex)
    {
        if (hex is { Length: 7 } && hex[0] == '#' && int.TryParse(hex.AsSpan(1), System.Globalization.NumberStyles.HexNumber, null, out int value))
            return LedEffectEngine.Rgb(value);
        return LedEffectEngine.Rgb(0x00A2FF);
    }

    public static string ToHex(Color color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";
}
