using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace DS339Monitor.Sensors;

/// <summary>Sensores que o próprio Windows fornece de graça (sem driver, sem admin).</summary>
internal static class NativeSensors
{
    private const double GiB = 1024d * 1024 * 1024;

    public static void ReadMemory(Readings r)
    {
        var status = new MemoryStatusEx { Length = (uint)Marshal.SizeOf<MemoryStatusEx>() };
        if (!GlobalMemoryStatusEx(ref status)) return;
        r.MemoryTotalGb = status.TotalPhys / GiB;
        r.MemoryAvailableGb = status.AvailPhys / GiB;
        r.MemoryUsedGb = (status.TotalPhys - status.AvailPhys) / GiB;
        r.MemoryLoad = status.TotalPhys > 0 ? (status.TotalPhys - status.AvailPhys) * 100.0 / status.TotalPhys : null;
    }

    public static void ReadDisks(Readings r)
    {
        r.Disks.Clear();
        string? system = Path.GetPathRoot(Environment.SystemDirectory);
        if (!string.IsNullOrEmpty(system)) r.SystemDrive = char.ToUpperInvariant(system[0]);

        foreach (var drive in DriveInfo.GetDrives())
        {
            try
            {
                if (drive.DriveType != DriveType.Fixed || !drive.IsReady) continue;
                r.Disks[char.ToUpperInvariant(drive.Name[0])] = new DiskSpace(drive.TotalSize / GiB, drive.TotalFreeSpace / GiB);
            }
            catch (IOException)
            {
                // Unidade sumiu entre a enumeração e a leitura.
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryStatusEx
    {
        public uint Length;
        public uint MemoryLoad;
        public ulong TotalPhys;
        public ulong AvailPhys;
        public ulong TotalPageFile;
        public ulong AvailPageFile;
        public ulong TotalVirtual;
        public ulong AvailVirtual;
        public ulong AvailExtendedVirtual;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx buffer);
}

/// <summary>Uso total da CPU via GetSystemTimes (o mesmo número do Gerenciador de Tarefas).</summary>
internal sealed class CpuLoadCounter
{
    private long _idle;
    private long _total;
    private bool _primed;

    public float? Sample()
    {
        if (!GetSystemTimes(out long idle, out long kernel, out long user)) return null;
        long total = kernel + user; // kernel já inclui o tempo ocioso
        float? result = null;
        if (_primed)
        {
            long dTotal = total - _total;
            long dIdle = idle - _idle;
            if (dTotal > 0) result = (float)Math.Clamp(100.0 * (dTotal - dIdle) / dTotal, 0, 100);
        }
        _idle = idle;
        _total = total;
        _primed = true;
        return result;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetSystemTimes(out long idleTime, out long kernelTime, out long userTime);
}

/// <summary>Velocidade de upload/download somando as interfaces físicas ativas.</summary>
internal sealed class NetworkCounter
{
    private NetworkInterface[] _interfaces = Array.Empty<NetworkInterface>();
    private long _nextRefresh;
    private long _lastTick;
    private long _lastSent;
    private long _lastReceived;

    public void Sample(Readings r)
    {
        long now = Environment.TickCount64;
        if (now >= _nextRefresh)
        {
            _interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up
                    && n.NetworkInterfaceType is not (NetworkInterfaceType.Loopback or NetworkInterfaceType.Tunnel)
                    && !n.Name.StartsWith("vEthernet", StringComparison.OrdinalIgnoreCase)
                    && !n.Description.Contains("Hyper-V", StringComparison.OrdinalIgnoreCase))
                .ToArray();
            _nextRefresh = now + 60_000;
            _lastTick = 0; // a lista mudou; recomeça a medição
        }

        long sent = 0, received = 0;
        foreach (var nic in _interfaces)
        {
            try
            {
                var stats = nic.GetIPStatistics();
                sent += stats.BytesSent;
                received += stats.BytesReceived;
            }
            catch (NetworkInformationException)
            {
            }
        }

        if (_lastTick != 0 && now > _lastTick)
        {
            double seconds = (now - _lastTick) / 1000.0;
            r.NetUpBytesPerSec = Math.Max(0, (sent - _lastSent) / seconds);
            r.NetDownBytesPerSec = Math.Max(0, (received - _lastReceived) / seconds);
        }
        _lastTick = now;
        _lastSent = sent;
        _lastReceived = received;
    }
}

internal static class SystemInfo
{
    private static string? _cpuName;

    public static string? CpuName => _cpuName ??= ReadCpuName();

    private static string? ReadCpuName()
    {
        using var key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
        string? name = (key?.GetValue("ProcessorNameString") as string)?.Trim();
        if (string.IsNullOrEmpty(name)) return null;
        // "AMD Ryzen 7 7800X3D 8-Core Processor" -> "AMD Ryzen 7 7800X3D"
        foreach (var suffix in new[] { " Processor", " CPU" })
            if (name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)) name = name[..^suffix.Length];
        int coreTag = name.IndexOf("-Core", StringComparison.OrdinalIgnoreCase);
        if (coreTag > 0)
        {
            int start = name.LastIndexOf(' ', coreTag);
            if (start > 0) name = name[..start];
        }
        return name.Replace("(R)", "").Replace("(TM)", "").Trim();
    }
}
