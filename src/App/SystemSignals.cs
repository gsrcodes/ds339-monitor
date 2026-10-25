using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace DS339Monitor.App;

/// <summary>Ações do usuário e do sistema que as telas e o LED podem encenar.</summary>
internal enum SignalKind
{
    /// <summary>Copiou algo (texto, arquivo, imagem).</summary>
    Copy,
    /// <summary>Tirou um print (PrtScn, Win+Shift+S, Ferramenta de Captura).</summary>
    Screenshot,
    /// <summary>Conectou um pendrive/disco (<see cref="SystemSignal.Text"/> = letra e nome).</summary>
    DriveArrived,
    /// <summary>Conectou outro dispositivo USB (controle, fone...).</summary>
    DeviceArrived,
    DeviceRemoved,
    /// <summary>Mudou o volume (<see cref="SystemSignal.Value"/> = 0–1).</summary>
    Volume,
    Muted,
    Unmuted,
    MicOn,
    MicOff,
    CameraOn,
    CameraOff,
    /// <summary>Outro app veio para frente (<see cref="SystemSignal.Text"/> = nome do executável, sem .exe).</summary>
    AppFocused,
    DisplayOff,
    DisplayOn,
    /// <summary>Um download do navegador terminou (<see cref="SystemSignal.Text"/> = nome do arquivo, Value = MB).</summary>
    Download,
    /// <summary>Música nova no Spotify (<see cref="SystemSignal.Text"/> = "Artista - Música").</summary>
    Track,
    /// <summary>O PC voltou da suspensão.</summary>
    Resumed,
}

internal readonly record struct SystemSignal(SignalKind Kind, long At, float Value = 0, string? Text = null);

/// <summary>
/// Fila das ações do sistema: quem detecta (janela oculta na thread da interface, FileSystemWatchers, a leitura de
/// volume/microfone na thread de animação) põe aqui; o diretor de cena tira a cada quadro. Sem trava e sem esperar
/// ninguém. Nada aqui lê conteúdo: só "aconteceu" (e um nome curto, quando ajuda a encenar).
/// </summary>
internal static class SystemSignals
{
    private static readonly ConcurrentQueue<SystemSignal> Queue = new();

    /// <summary>Registra cada sinal no log (para o <c>--sinais</c>).</summary>
    public static volatile bool Logging;

    public static void Post(SignalKind kind, float value = 0, string? text = null)
    {
        if (Logging) Log.Info($"Sinal: {kind}{(text is null ? "" : $" \"{text}\"")}{(value != 0 ? $" ({value:0.##})" : "")}");
        if (Queue.Count < 256) Queue.Enqueue(new SystemSignal(kind, Clock.Ms, value, text));
    }

    public static bool TryTake(out SystemSignal signal) => Queue.TryDequeue(out signal);
}

/// <summary>
/// Janela invisível (na thread da interface, que já tem o laço de mensagens) que recebe do Windows: área de
/// transferência, dispositivos, monitor ligado/desligado e app em foco — todos por notificação, sem polling e sem
/// ganchos de teclado/mouse. Também cuida dos FileSystemWatchers (prints e downloads) e da suspensão.
/// </summary>
internal sealed class SignalListener : NativeWindow, IDisposable
{
    private const int WmClipboardUpdate = 0x031D;
    private const int WmDeviceChange = 0x0219;
    private const int WmPowerBroadcast = 0x0218;
    private const int DbtDeviceArrival = 0x8000;
    private const int DbtDeviceRemoveComplete = 0x8004;
    private const int DbtDevTypVolume = 2;
    private const int DbtDevTypDeviceInterface = 5;
    private const int PbtPowerSettingChange = 0x8013;
    private const uint EventSystemForeground = 3;
    private static readonly Guid UsbDeviceInterface = new("A5DCBF10-6530-11D2-901F-00C04FB951ED");
    private static readonly Guid ConsoleDisplayState = new("6FE69556-704A-47A0-8F24-C28D936FDA47");
    private static readonly Guid ScreenshotsFolder = new("b7bede81-df94-4682-a7d8-57a52620b86f");
    private static readonly Guid DownloadsFolder = new("374DE290-123F-4565-9164-39C4925E467B");

    /// <summary>Dispositivos deste próprio programa (telas, LED, serial): conectar/desconectar não é novidade.</summary>
    private static readonly string[] OwnDevices = { "VID_0416&PID_5408", "VID_33C3", "VID_1A86", "VID_C019", "VID_0483" };
    private static readonly string[] ShellApps =
    {
        "explorer", "StartMenuExperienceHost", "SearchHost", "SearchApp", "ShellExperienceHost", "ShellHost",
        "LockApp", "TextInputHost", "ApplicationFrameHost", "DS339Monitor", "dwm",
    };

    // O delegate do gancho de eventos tem que ficar vivo enquanto o gancho existir (senão o GC o recolhe).
    private static WinEventProc? _foregroundProc;
    private readonly List<FileSystemWatcher> _watchers = new();
    private readonly Dictionary<uint, string> _names = new();
    private IntPtr _deviceNotify, _powerNotify, _foregroundHook;
    private uint _clipboardSequence;
    private long _lastClipboard, _lastScreenshot, _quietUntil;
    private bool _displayKnown;
    private string? _lastApp;

    public SignalListener()
    {
        CreateHandle(new CreateParams { Caption = "DS339 sinais", Style = unchecked((int)0x80000000), ExStyle = 0x80 }); // WS_POPUP, WS_EX_TOOLWINDOW; nunca mostrada
        _quietUntil = Clock.Ms + 8000; // os dispositivos que já estavam ligados aparecem na partida
        foreach (int message in new[] { WmClipboardUpdate, WmDeviceChange, WmPowerBroadcast })
            ChangeWindowMessageFilterEx(Handle, (uint)message, 1, IntPtr.Zero); // o programa roda elevado

        if (!AddClipboardFormatListener(Handle)) Log.Warn("Sinais: a área de transferência não aceitou o ouvinte.");
        _deviceNotify = RegisterUsbNotification(Handle);
        var display = ConsoleDisplayState;
        _powerNotify = RegisterPowerSettingNotification(Handle, ref display, 0);
        _foregroundProc = OnForeground;
        _foregroundHook = SetWinEventHook(EventSystemForeground, EventSystemForeground, IntPtr.Zero, _foregroundProc, 0, 0, 0x0000 | 0x0002); // OUTOFCONTEXT | SKIPOWNPROCESS
        Watch(KnownFolder(ScreenshotsFolder), w =>
        {
            w.Filter = "*.png";
            w.Created += (_, _) => Screenshot();
        });
        Watch(KnownFolder(DownloadsFolder), w =>
        {
            w.NotifyFilter = NotifyFilters.FileName;
            w.Renamed += (_, e) => DownloadFinished(e);
        });
        SystemEvents.PowerModeChanged += OnPowerModeChanged;
    }

    public void Dispose()
    {
        SystemEvents.PowerModeChanged -= OnPowerModeChanged;
        foreach (var watcher in _watchers) watcher.Dispose();
        if (_foregroundHook != IntPtr.Zero) UnhookWinEvent(_foregroundHook);
        if (_deviceNotify != IntPtr.Zero) UnregisterDeviceNotification(_deviceNotify);
        if (_powerNotify != IntPtr.Zero) UnregisterPowerSettingNotification(_powerNotify);
        if (Handle != IntPtr.Zero)
        {
            RemoveClipboardFormatListener(Handle);
            DestroyHandle();
        }
    }

    protected override void WndProc(ref Message m)
    {
        try
        {
            switch (m.Msg)
            {
                case WmClipboardUpdate:
                    Clipboard();
                    break;
                case WmDeviceChange:
                    Device((int)m.WParam, m.LParam);
                    break;
                case WmPowerBroadcast when (int)m.WParam == PbtPowerSettingChange:
                    Display(m.LParam);
                    break;
            }
        }
        catch (Exception ex)
        {
            Log.Error("Sinais", ex);
        }
        base.WndProc(ref m);
    }

    /// <summary>
    /// Copiou algo. Imagem cujo dono é a ferramenta de captura (ou sem dono: o PrtScn clássico) é print. O conteúdo
    /// nunca é lido — só o tipo.
    /// </summary>
    private void Clipboard()
    {
        uint sequence = GetClipboardSequenceNumber();
        long now = Clock.Ms;
        if (sequence == _clipboardSequence || now - _lastClipboard < 300) return;
        _clipboardSequence = sequence;
        _lastClipboard = now;

        bool image = IsClipboardFormatAvailable(8) || IsClipboardFormatAvailable(2); // CF_DIB, CF_BITMAP
        if (image)
        {
            IntPtr owner = GetClipboardOwner();
            string app = owner == IntPtr.Zero ? "" : ProcessName(owner);
            if (owner == IntPtr.Zero || app is "SnippingTool" or "ScreenClippingHost" or "ScreenSketch")
            {
                Screenshot();
                return;
            }
        }
        SystemSignals.Post(SignalKind.Copy, image ? 1 : 0);
    }

    private void Screenshot()
    {
        long now = Clock.Ms;
        if (now - Interlocked.Read(ref _lastScreenshot) < 2000) return; // o mesmo print chega pela área de transferência e pela pasta
        Interlocked.Exchange(ref _lastScreenshot, now);
        SystemSignals.Post(SignalKind.Screenshot);
    }

    private unsafe void Device(int change, IntPtr data)
    {
        if (data == IntPtr.Zero || change is not (DbtDeviceArrival or DbtDeviceRemoveComplete)) return;
        if (Clock.Ms < _quietUntil) return;
        int type = Marshal.ReadInt32(data, 4);
        if (type == DbtDevTypVolume && change == DbtDeviceArrival)
        {
            uint mask = (uint)Marshal.ReadInt32(data, 12);
            for (int i = 0; i < 26; i++)
            {
                if ((mask & (1u << i)) == 0) continue;
                string drive = $"{(char)('A' + i)}:";
                string label = "";
                try
                {
                    var info = new DriveInfo(drive);
                    if (info.IsReady) label = info.VolumeLabel;
                }
                catch (IOException)
                {
                }
                SystemSignals.Post(SignalKind.DriveArrived, 0, label.Length > 0 ? $"{drive} {label}" : drive);
            }
        }
        else if (type == DbtDevTypDeviceInterface)
        {
            string name = Marshal.PtrToStringUni(data + 28) ?? "";
            if (OwnDevices.Any(own => name.Contains(own, StringComparison.OrdinalIgnoreCase))) return;
            SystemSignals.Post(change == DbtDeviceArrival ? SignalKind.DeviceArrived : SignalKind.DeviceRemoved);
        }
    }

    private void Display(IntPtr data)
    {
        // POWERBROADCAST_SETTING: GUID (16), DataLength (4), Data — 0 apagado, 1 aceso, 2 esmaecido.
        int state = Marshal.ReadInt32(data, 20);
        if (!_displayKnown)
        {
            _displayKnown = true; // o primeiro aviso é só o estado atual
            return;
        }
        if (state == 0) SystemSignals.Post(SignalKind.DisplayOff);
        else if (state == 1) SystemSignals.Post(SignalKind.DisplayOn);
    }

    private void OnForeground(IntPtr hook, uint e, IntPtr hwnd, int obj, int child, uint thread, uint time)
    {
        if (hwnd == IntPtr.Zero) return;
        string app = ProcessName(hwnd);
        if (app.Length == 0 || app == _lastApp || ShellApps.Contains(app, StringComparer.OrdinalIgnoreCase)) return;
        _lastApp = app;
        SystemSignals.Post(SignalKind.AppFocused, 0, app);
    }

    private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
        if (e.Mode != PowerModes.Resume) return;
        _quietUntil = Clock.Ms + 10_000; // tudo se reconecta ao voltar
        SystemSignals.Post(SignalKind.Resumed);
    }

    /// <summary>Download do navegador terminou: o arquivo provisório (.crdownload, .part, .tmp) ganhou o nome final.</summary>
    private static void DownloadFinished(RenamedEventArgs e)
    {
        string old = e.OldName ?? "";
        if (!(old.EndsWith(".crdownload", StringComparison.OrdinalIgnoreCase) || old.EndsWith(".part", StringComparison.OrdinalIgnoreCase)
              || old.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase))) return;
        string name = e.Name ?? "";
        if (name.EndsWith(".crdownload", StringComparison.OrdinalIgnoreCase) || name.EndsWith(".part", StringComparison.OrdinalIgnoreCase)
            || name.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase)) return;
        float mb = 0;
        try
        {
            mb = new FileInfo(e.FullPath).Length / 1048576f;
        }
        catch (IOException)
        {
        }
        SystemSignals.Post(SignalKind.Download, mb, Path.GetFileName(name));
    }

    private void Watch(string? folder, Action<FileSystemWatcher> setup)
    {
        if (folder is null || !Directory.Exists(folder)) return;
        try
        {
            var watcher = new FileSystemWatcher(folder) { IncludeSubdirectories = false, InternalBufferSize = 16 * 1024 };
            setup(watcher);
            watcher.EnableRaisingEvents = true;
            _watchers.Add(watcher);
        }
        catch (Exception ex) when (ex is IOException or ArgumentException or UnauthorizedAccessException)
        {
            Log.Warn($"Sinais: não consegui vigiar {folder} ({ex.Message}).");
        }
    }

    /// <summary>Nome do executável dono da janela (sem .exe), com cache por processo.</summary>
    private string ProcessName(IntPtr hwnd)
    {
        GetWindowThreadProcessId(hwnd, out uint pid);
        if (pid == 0) return "";
        if (_names.TryGetValue(pid, out var cached)) return cached;
        string name = "";
        IntPtr process = OpenProcess(0x1000, false, pid); // PROCESS_QUERY_LIMITED_INFORMATION
        if (process != IntPtr.Zero)
        {
            var buffer = new StringBuilder(512);
            int size = buffer.Capacity;
            if (QueryFullProcessImageName(process, 0, buffer, ref size)) name = Path.GetFileNameWithoutExtension(buffer.ToString());
            CloseHandle(process);
        }
        if (_names.Count > 512) _names.Clear();
        _names[pid] = name;
        return name;
    }

    private static string? KnownFolder(Guid id)
    {
        if (SHGetKnownFolderPath(ref id, 0, IntPtr.Zero, out IntPtr path) != 0) return null;
        try
        {
            return Marshal.PtrToStringUni(path);
        }
        finally
        {
            Marshal.FreeCoTaskMem(path);
        }
    }

    private static IntPtr RegisterUsbNotification(IntPtr hwnd)
    {
        // DEV_BROADCAST_DEVICEINTERFACE_W: size, type, reserved, GUID, nome (vazio).
        int size = 4 + 4 + 4 + 16 + 2;
        IntPtr filter = Marshal.AllocHGlobal(size);
        try
        {
            for (int i = 0; i < size; i++) Marshal.WriteByte(filter, i, 0);
            Marshal.WriteInt32(filter, 0, size);
            Marshal.WriteInt32(filter, 4, DbtDevTypDeviceInterface);
            Marshal.Copy(UsbDeviceInterface.ToByteArray(), 0, filter + 12, 16);
            return RegisterDeviceNotification(hwnd, filter, 0); // DEVICE_NOTIFY_WINDOW_HANDLE
        }
        finally
        {
            Marshal.FreeHGlobal(filter);
        }
    }

    private delegate void WinEventProc(IntPtr hook, uint e, IntPtr hwnd, int obj, int child, uint thread, uint time);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool AddClipboardFormatListener(IntPtr hwnd);

    [DllImport("user32.dll")]
    private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

    [DllImport("user32.dll")]
    private static extern uint GetClipboardSequenceNumber();

    [DllImport("user32.dll")]
    private static extern bool IsClipboardFormatAvailable(uint format);

    [DllImport("user32.dll")]
    private static extern IntPtr GetClipboardOwner();

    [DllImport("user32.dll")]
    private static extern bool ChangeWindowMessageFilterEx(IntPtr hwnd, uint message, uint action, IntPtr changeInfo);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr RegisterDeviceNotification(IntPtr recipient, IntPtr filter, uint flags);

    [DllImport("user32.dll")]
    private static extern bool UnregisterDeviceNotification(IntPtr handle);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr RegisterPowerSettingNotification(IntPtr recipient, ref Guid setting, uint flags);

    [DllImport("user32.dll")]
    private static extern bool UnregisterPowerSettingNotification(IntPtr handle);

    [DllImport("user32.dll")]
    private static extern IntPtr SetWinEventHook(uint min, uint max, IntPtr module, WinEventProc proc, uint process, uint thread, uint flags);

    [DllImport("user32.dll")]
    private static extern bool UnhookWinEvent(IntPtr hook);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint pid);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(uint access, bool inherit, uint pid);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern bool QueryFullProcessImageName(IntPtr process, uint flags, StringBuilder name, ref int size);

    [DllImport("kernel32.dll")]
    private static extern bool CloseHandle(IntPtr handle);

    [DllImport("shell32.dll")]
    private static extern int SHGetKnownFolderPath(ref Guid id, uint flags, IntPtr token, out IntPtr path);
}

/// <summary>
/// O que muda sem avisar: volume e mudo do dispositivo de saída, microfone/webcam em uso (a mesma marca que acende o
/// ícone de privacidade do Windows) e a música do Spotify (título da janela). Lido pela thread de animação de tempos em
/// tempos — cada leitura custa microssegundos.
/// </summary>
internal sealed class PolledSignals
{
    private const string ConsentStore = @"Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore";
    private float _volume = -1;
    private bool? _muted;
    private bool? _mic, _camera;
    private long _nextPrivacy, _nextSpotify, _nextSpotifyPid;
    private int _spotifyPid;
    private string? _track;

    /// <summary>Volume (0–1) e mudo lidos do dispositivo de saída agora; NaN = sem dispositivo.</summary>
    public void Volume(float level, bool muted)
    {
        if (float.IsNaN(level)) return;
        if (_volume >= 0 && Math.Abs(level - _volume) >= 0.01f) SystemSignals.Post(SignalKind.Volume, level);
        if (_muted is bool was && was != muted) SystemSignals.Post(muted ? SignalKind.Muted : SignalKind.Unmuted, level);
        _volume = level;
        _muted = muted;
    }

    public void Poll(long now)
    {
        if (now >= _nextPrivacy)
        {
            _nextPrivacy = now + 2000;
            Privacy("microphone", ref _mic, SignalKind.MicOn, SignalKind.MicOff);
            Privacy("webcam", ref _camera, SignalKind.CameraOn, SignalKind.CameraOff);
        }
        if (now >= _nextSpotify)
        {
            _nextSpotify = now + 2000;
            Spotify(now);
        }
    }

    private static void Privacy(string capability, ref bool? state, SignalKind on, SignalKind off)
    {
        bool inUse = false;
        try
        {
            using var root = Registry.CurrentUser.OpenSubKey($@"{ConsentStore}\{capability}");
            inUse = root is not null && InUse(root, 0);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Security.SecurityException)
        {
            return;
        }
        if (state is bool was && was != inUse) SystemSignals.Post(inUse ? on : off);
        state = inUse;
    }

    /// <summary>Algum app com início de uso marcado e sem fim (o Windows zera o fim enquanto usa).</summary>
    private static bool InUse(RegistryKey key, int depth)
    {
        foreach (string name in key.GetSubKeyNames())
        {
            using var sub = key.OpenSubKey(name);
            if (sub is null) continue;
            if (sub.GetValue("LastUsedTimeStart") is long start && start != 0 && sub.GetValue("LastUsedTimeStop") is long stop && stop == 0) return true;
            if (depth == 0 && name == "NonPackaged" && InUse(sub, 1)) return true;
        }
        return false;
    }

    private void Spotify(long now)
    {
        if (now >= _nextSpotifyPid || (_spotifyPid != 0 && !Alive(_spotifyPid)))
        {
            _nextSpotifyPid = now + 15_000;
            var processes = Process.GetProcessesByName("Spotify");
            _spotifyPid = processes.FirstOrDefault(p => p.MainWindowHandle != IntPtr.Zero)?.Id ?? 0;
            foreach (var p in processes) p.Dispose();
        }
        if (_spotifyPid == 0) return;
        string? title = MainTitle(_spotifyPid);
        // "Spotify" / "Spotify Premium" = pausado; "Artista - Música" = tocando.
        if (title is null || !title.Contains(" - ", StringComparison.Ordinal)) return;
        if (_track is not null && title != _track) SystemSignals.Post(SignalKind.Track, 0, title);
        _track = title;
    }

    private static bool Alive(int pid)
    {
        IntPtr process = OpenProcess(0x1000, false, (uint)pid);
        if (process == IntPtr.Zero) return false;
        bool alive = GetExitCodeProcess(process, out uint code) && code == 259; // STILL_ACTIVE
        CloseHandle(process);
        return alive;
    }

    private static string? MainTitle(int pid)
    {
        string? found = null;
        EnumWindows((hwnd, _) =>
        {
            GetWindowThreadProcessId(hwnd, out uint owner);
            if (owner != pid || !IsWindowVisible(hwnd)) return true;
            int length = GetWindowTextLength(hwnd);
            if (length == 0) return true;
            var text = new StringBuilder(length + 1);
            GetWindowText(hwnd, text, text.Capacity);
            found = text.ToString();
            return false;
        }, IntPtr.Zero);
        return found;
    }

    private delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr param);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc proc, IntPtr param);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint pid);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hwnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowTextLength(IntPtr hwnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hwnd, StringBuilder text, int max);

    [DllImport("kernel32.dll")]
    private static extern IntPtr OpenProcess(uint access, bool inherit, uint pid);

    [DllImport("kernel32.dll")]
    private static extern bool GetExitCodeProcess(IntPtr process, out uint code);

    [DllImport("kernel32.dll")]
    private static extern bool CloseHandle(IntPtr handle);
}
