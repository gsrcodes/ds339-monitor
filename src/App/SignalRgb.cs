using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace DS339Monitor.App;

/// <summary>
/// Fecha e reabre o SignalRGB. Fechar é pelo Gerenciador de Reinicialização do Windows (o mesmo que instaladores usam):
/// ele pede ao programa que encerre como se a sessão estivesse acabando, e o SignalRGB faz a saída normal (cada plugin
/// roda o próprio Shutdown). Nunca mata o processo.
/// Reabrir usa o mesmo comando do início com o Windows (SignalRgbLauncher.exe --silent), sem privilégio de
/// administrador — como o Windows abriria —, mesmo que este programa esteja elevado.
/// </summary>
internal static class SignalRgb
{
    private const string ProcessName = "SignalRgb";
    private const string LauncherProcess = "SignalRgbLauncher";

    public static bool IsRunning()
    {
        var processes = Process.GetProcessesByName(ProcessName);
        bool running = processes.Length > 0;
        foreach (var process in processes) process.Dispose();
        return running;
    }

    /// <summary>Pede ao SignalRGB que feche e espera até <paramref name="timeout"/>. True = fechou.</summary>
    public static bool Close(TimeSpan timeout)
    {
        var processes = Process.GetProcessesByName(ProcessName);
        try
        {
            if (processes.Length == 0) return true;
            var targets = new List<RmUniqueProcess>();
            foreach (var process in processes)
            {
                try
                {
                    long started = process.StartTime.ToFileTime();
                    targets.Add(new RmUniqueProcess
                    {
                        ProcessId = process.Id,
                        StartTime = new FILETIME { dwLowDateTime = (int)(started & 0xFFFFFFFF), dwHighDateTime = (int)(started >> 32) },
                    });
                }
                catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception)
                {
                    // Já saiu, ou sem acesso: fica de fora.
                }
            }

            int result = RmStartSession(out uint session, 0, new StringBuilder(SessionKeyLength + 1));
            if (result != 0)
            {
                Log.Warn($"SignalRGB: o Gerenciador de Reinicialização não abriu (erro {result}).");
                return false;
            }
            try
            {
                result = RmRegisterResources(session, 0, null, (uint)targets.Count, targets.ToArray(), 0, null);
                if (result == 0) result = RmShutdown(session, 0, IntPtr.Zero); // 0 = sem forçar: nunca mata
            }
            finally
            {
                RmEndSession(session);
            }

            var deadline = DateTime.UtcNow + timeout;
            foreach (var process in processes)
            {
                var left = deadline - DateTime.UtcNow;
                if (left > TimeSpan.Zero) process.WaitForExit(left);
            }
            bool closed = processes.All(p => p.HasExited);
            if (closed) Log.Info("SignalRGB fechado (saída normal: os dispositivos ficam na cor de desligamento).");
            else Log.Warn($"SignalRGB não fechou (resposta do Windows: {result}); deixei aberto.");
            return closed;
        }
        finally
        {
            foreach (var process in processes) process.Dispose();
        }
    }

    /// <summary>Abre o SignalRGB como no início do Windows (minimizado, sem administrador). True = pediu para abrir.</summary>
    public static bool Start()
    {
        string? launcher = LauncherPath();
        if (launcher is null)
        {
            Log.Warn("SignalRGB: não achei o SignalRgbLauncher.exe para reabrir.");
            return false;
        }
        try
        {
            if (Environment.IsPrivilegedProcess) StartAsShellUser(launcher, "--silent");
            else Process.Start(new ProcessStartInfo(launcher, "--silent") { UseShellExecute = true })?.Dispose();
            Log.Info("SignalRGB reaberto.");
            return true;
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException or IOException)
        {
            Log.Error("SignalRGB: não consegui reabrir", ex);
            return false;
        }
    }

    /// <summary>O lançador do início com o Windows (chave Run), ou o lugar padrão da instalação.</summary>
    private static string? LauncherPath()
    {
        using var run = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
        if (run?.GetValue("SignalRgb") is string command)
        {
            string path = command.StartsWith('"') ? command[1..command.IndexOf('"', 1)] : command.Split(' ')[0];
            if (File.Exists(path)) return path;
        }
        string standard = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VortxEngine", LauncherProcess + ".exe");
        return File.Exists(standard) ? standard : null;
    }

    /// <summary>
    /// Abre um programa com o token do Explorer (o usuário sem elevação): o processo novo nasce como filho do Explorer
    /// (atributo PROC_THREAD_ATTRIBUTE_PARENT_PROCESS) e herda o token dele.
    /// </summary>
    private static void StartAsShellUser(string path, string arguments)
    {
        GetWindowThreadProcessId(GetShellWindow(), out uint shellId);
        if (shellId == 0) throw new InvalidOperationException("Explorer não encontrado");
        IntPtr shell = OpenProcess(ProcessCreateProcess, false, shellId);
        if (shell == IntPtr.Zero) throw new System.ComponentModel.Win32Exception();
        IntPtr list = IntPtr.Zero, parent = IntPtr.Zero;
        try
        {
            IntPtr size = IntPtr.Zero;
            InitializeProcThreadAttributeList(IntPtr.Zero, 1, 0, ref size);
            list = Marshal.AllocHGlobal(size);
            if (!InitializeProcThreadAttributeList(list, 1, 0, ref size)) throw new System.ComponentModel.Win32Exception();
            parent = Marshal.AllocHGlobal(IntPtr.Size);
            Marshal.WriteIntPtr(parent, shell);
            if (!UpdateProcThreadAttribute(list, 0, (IntPtr)AttributeParentProcess, parent, (IntPtr)IntPtr.Size, IntPtr.Zero, IntPtr.Zero))
                throw new System.ComponentModel.Win32Exception();

            var info = new StartupInfoEx { AttributeList = list };
            info.StartupInfo.cb = Marshal.SizeOf<StartupInfoEx>();
            var commandLine = new StringBuilder($"\"{path}\" {arguments}");
            if (!CreateProcess(path, commandLine, IntPtr.Zero, IntPtr.Zero, false, ExtendedStartupInfoPresent, IntPtr.Zero,
                    Path.GetDirectoryName(path), ref info, out var created))
                throw new System.ComponentModel.Win32Exception();
            CloseHandle(created.Thread);
            CloseHandle(created.Process);
        }
        finally
        {
            if (list != IntPtr.Zero)
            {
                DeleteProcThreadAttributeList(list);
                Marshal.FreeHGlobal(list);
            }
            if (parent != IntPtr.Zero) Marshal.FreeHGlobal(parent);
            CloseHandle(shell);
        }
    }

    // ---------------------------------------------------------------- Win32

    private const int SessionKeyLength = 32;
    private const uint ProcessCreateProcess = 0x0080;
    private const int AttributeParentProcess = 0x00020000;
    private const uint ExtendedStartupInfoPresent = 0x00080000;

    [StructLayout(LayoutKind.Sequential)]
    private struct RmUniqueProcess
    {
        public int ProcessId;
        public FILETIME StartTime;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct StartupInfo
    {
        public int cb;
        public string? Reserved, Desktop, Title;
        public int X, Y, XSize, YSize, XCountChars, YCountChars, FillAttribute, Flags;
        public short ShowWindow, Reserved2;
        public IntPtr Reserved2Pointer, StdInput, StdOutput, StdError;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct StartupInfoEx
    {
        public StartupInfo StartupInfo;
        public IntPtr AttributeList;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ProcessInformation
    {
        public IntPtr Process, Thread;
        public int ProcessId, ThreadId;
    }

    [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
    private static extern int RmStartSession(out uint session, int flags, StringBuilder key);

    [DllImport("rstrtmgr.dll")]
    private static extern int RmEndSession(uint session);

    [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
    private static extern int RmRegisterResources(uint session, uint fileCount, string[]? files, uint applicationCount,
        [In] RmUniqueProcess[] applications, uint serviceCount, string[]? services);

    [DllImport("rstrtmgr.dll")]
    private static extern int RmShutdown(uint session, uint flags, IntPtr status);

    [DllImport("user32.dll")]
    private static extern IntPtr GetShellWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(uint access, bool inherit, uint processId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool InitializeProcThreadAttributeList(IntPtr list, int count, int flags, ref IntPtr size);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool UpdateProcThreadAttribute(IntPtr list, uint flags, IntPtr attribute, IntPtr value, IntPtr size, IntPtr previous, IntPtr returnSize);

    [DllImport("kernel32.dll")]
    private static extern void DeleteProcThreadAttributeList(IntPtr list);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CreateProcess(string application, StringBuilder commandLine, IntPtr processAttributes, IntPtr threadAttributes,
        bool inheritHandles, uint flags, IntPtr environment, string? directory, ref StartupInfoEx startup, out ProcessInformation info);

    [DllImport("kernel32.dll")]
    private static extern bool CloseHandle(IntPtr handle);
}
