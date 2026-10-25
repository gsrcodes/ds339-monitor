using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace DS339Monitor.Sensors;

/// <summary>
/// GPU AMD lida direto do driver (atiadlxx.dll / ADL). Uma leitura custa ~0,5 ms, contra ~10 ms do
/// LibreHardwareMonitor — que a cada leitura também varre todos os motores D3D e mantém a contagem
/// de FPS do driver ligada. Se não houver driver AMD, <see cref="TryOpen"/> devolve null e o
/// LibreHardwareMonitor assume.
/// </summary>
internal sealed unsafe class AmdGpu : IDisposable
{
    private const string Dll = "atiadlxx.dll";
    private const int FamilyRdna3 = 145; // a partir daqui a potência certa é BOARD_POWER (igual ao LibreHardwareMonitor)

    // Índices de ADL_PMLOG_SENSORS (adl_defines.h).
    private const int ClockGfx = 1;
    private const int ClockMem = 2;
    private const int TempEdge = 8;
    private const int FanRpm = 14;
    private const int ActivityGfx = 19;
    private const int VoltageGfx = 21;
    private const int PowerAsic = 23;
    private const int TempHotSpot = 27;
    private const int PowerBoard = 73;

    private readonly int _adapter;
    private readonly bool _boardPower;
    private readonly float _vramTotalMb;
    private IntPtr _context;

    private AmdGpu(IntPtr context, int adapter, string name, int family, float vramTotalMb)
    {
        _context = context;
        _adapter = adapter;
        _boardPower = family >= FamilyRdna3;
        _vramTotalMb = vramTotalMb;
        Name = name;
    }

    public string Name { get; }

    public static AmdGpu? TryOpen(string? hint)
    {
        if (!NativeLibrary.TryLoad(Dll, out _)) return null; // sem driver AMD

        IntPtr context = IntPtr.Zero;
        try
        {
            if (ADL2_Main_Control_Create(&Alloc, 1, out context) != 0) return null;
            if (ADL2_Adapter_NumberOfAdapters_Get(context, out int count) != 0 || count <= 0) return Fail(context);

            var infos = new AdapterInfo[count];
            fixed (AdapterInfo* p = infos)
            {
                for (int i = 0; i < count; i++) p[i].Size = sizeof(AdapterInfo);
                if (ADL2_Adapter_AdapterInfo_Get(context, p, sizeof(AdapterInfo) * count) != 0) return Fail(context);
            }

            // O ADL lista uma entrada por saída de vídeo; uma GPU = um barramento PCI.
            var gpus = new List<(int Index, string Name, float VramMb)>();
            var buses = new HashSet<int>();
            foreach (var info in infos)
            {
                if (info.Present == 0 || !buses.Add(info.Bus)) continue;
                var copy = info;
                string name = Encoding.ASCII.GetString(copy.Name, 256).TrimEnd('\0').Trim();
                float vram = ADL2_Adapter_MemoryInfo_Get(context, info.Index, out var memory) == 0 ? memory.Size / 1048576f : 0;
                gpus.Add((info.Index, name, vram));
            }

            var chosen = gpus.FirstOrDefault(g => !string.IsNullOrWhiteSpace(hint) && g.Name.Contains(hint, StringComparison.OrdinalIgnoreCase));
            if (chosen.Name is null)
                chosen = gpus.OrderByDescending(g => g.VramMb).FirstOrDefault(); // a dedicada tem mais VRAM que a integrada
            if (chosen.Name is null) return Fail(context);

            PmLog probe;
            if (ADL2_New_QueryPMLogData_Get(context, chosen.Index, &probe) != 0)
            {
                Log.Warn($"Driver AMD não forneceu sensores para {chosen.Name}; usando LibreHardwareMonitor.");
                return Fail(context);
            }

            int family = ADL2_GcnAsicInfo_Get(context, chosen.Index, out var gcn) == 0 ? gcn.Family : 0;
            Log.Info($"GPU via driver AMD: {chosen.Name} ({chosen.VramMb:0} MB, família {family})");
            return new AmdGpu(context, chosen.Index, chosen.Name, family, chosen.VramMb);
        }
        catch (Exception ex) when (ex is EntryPointNotFoundException or DllNotFoundException or BadImageFormatException)
        {
            Log.Warn($"Driver AMD incompatível ({ex.Message}); usando LibreHardwareMonitor.");
            return Fail(context);
        }
    }

    public void Update(Readings r)
    {
        r.GpuName = Name;
        PmLog log;
        if (ADL2_New_QueryPMLogData_Get(_context, _adapter, &log) != 0)
        {
            r.GpuClock = r.GpuMemoryClock = r.GpuTemp = r.GpuHotSpot = r.GpuFan = r.GpuLoad = r.GpuVoltage = r.GpuPower = null;
            return;
        }

        r.GpuClock = Sensor(&log, ClockGfx);
        r.GpuMemoryClock = Sensor(&log, ClockMem);
        r.GpuTemp = Sensor(&log, TempEdge);
        r.GpuHotSpot = Sensor(&log, TempHotSpot);
        r.GpuFan = Sensor(&log, FanRpm);
        r.GpuLoad = Sensor(&log, ActivityGfx);
        r.GpuVoltage = Sensor(&log, VoltageGfx) / 1000f;
        r.GpuPower = (_boardPower ? Sensor(&log, PowerBoard) : null) ?? Sensor(&log, PowerAsic);

        if (ADL2_Adapter_DedicatedVRAMUsage_Get(_context, _adapter, out int usedMb) == 0 && _vramTotalMb > 0)
        {
            r.GpuMemoryUsedMb = usedMb;
            r.GpuMemoryTotalMb = _vramTotalMb;
            r.GpuMemoryLoad = usedMb * 100f / _vramTotalMb;
        }
    }

    public void Dispose()
    {
        if (_context == IntPtr.Zero) return;
        ADL2_Main_Control_Destroy(_context);
        _context = IntPtr.Zero;
    }

    private static float? Sensor(PmLog* log, int index) =>
        log->Sensors[index * 2] != 0 ? log->Sensors[index * 2 + 1] : null;

    private static AmdGpu? Fail(IntPtr context)
    {
        if (context != IntPtr.Zero) ADL2_Main_Control_Destroy(context);
        return null;
    }

    /// <summary>O ADL pede memória por callback.</summary>
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    private static IntPtr Alloc(int size) => Marshal.AllocHGlobal(size);

    [StructLayout(LayoutKind.Sequential)]
    private struct PmLog
    {
        public int Size;
        public fixed int Sensors[256 * 2]; // ADLSingleSensorData { int supported; int value; } x 256
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct AdapterInfo
    {
        public int Size;
        public int Index;
        public fixed byte Udid[256];
        public int Bus;
        public int Device;
        public int Function;
        public int Vendor;
        public fixed byte Name[256];
        public fixed byte DisplayName[256];
        public int Present;
        public int Exist;
        public fixed byte DriverPath[256];
        public fixed byte DriverPathExt[256];
        public fixed byte PnpString[256];
        public int OsDisplayIndex;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryInfo
    {
        public long Size;
        public fixed byte Type[256];
        public long Bandwidth;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct GcnInfo
    {
        public int CuCount;
        public int TexCount;
        public int RopCount;
        public int Family;
        public int Revision;
    }

    [DllImport(Dll)]
    private static extern int ADL2_Main_Control_Create(delegate* unmanaged[Stdcall]<int, IntPtr> callback, int connectedOnly, out IntPtr context);

    [DllImport(Dll)]
    private static extern int ADL2_Main_Control_Destroy(IntPtr context);

    [DllImport(Dll)]
    private static extern int ADL2_Adapter_NumberOfAdapters_Get(IntPtr context, out int count);

    [DllImport(Dll)]
    private static extern int ADL2_Adapter_AdapterInfo_Get(IntPtr context, AdapterInfo* info, int size);

    [DllImport(Dll)]
    private static extern int ADL2_Adapter_MemoryInfo_Get(IntPtr context, int adapter, out MemoryInfo info);

    [DllImport(Dll)]
    private static extern int ADL2_GcnAsicInfo_Get(IntPtr context, int adapter, out GcnInfo info);

    [DllImport(Dll)]
    private static extern int ADL2_Adapter_DedicatedVRAMUsage_Get(IntPtr context, int adapter, out int usedMb);

    [DllImport(Dll)]
    private static extern int ADL2_New_QueryPMLogData_Get(IntPtr context, int adapter, PmLog* data);
}
