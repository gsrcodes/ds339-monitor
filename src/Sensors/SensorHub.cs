namespace DS339Monitor.Sensors;

/// <summary>
/// Ponto único de leitura. <see cref="Configure"/> recebe as chaves que o tema usa e liga só os grupos
/// necessários; <see cref="Update"/> lê esses grupos uma vez por ciclo.
/// </summary>
internal sealed class SensorHub : IDisposable
{
    private const long DiskIntervalMs = 30_000;

    private readonly Readings _readings = new();
    private readonly CpuLoadCounter _cpuLoad = new();
    private readonly NetworkCounter _network = new();
    private readonly HashSet<string> _warned = new(StringComparer.OrdinalIgnoreCase);
    private HardwareMonitor? _hardware;
    private AmdGpu? _amdGpu;
    private SensorGroup _groups;
    private long _nextDiskRead;

    public HardwareMonitor? Hardware => _hardware;

    /// <summary>Abrir também o chip de sensores da placa-mãe (fans), para a curva dos fans. Vale no próximo Configure.</summary>
    public bool Board { get; set; }

    public void Configure(IEnumerable<string> keys, string? gpuHint)
    {
        var groups = SensorGroup.Clock;
        foreach (var key in keys)
        {
            if (SensorKeys.TryGet(key, out var definition))
                groups |= definition.Group;
            else if (_warned.Add(key))
                Log.Warn($"Dado '{key}' não é suportado; vai aparecer como {SensorKeys.Missing}.");
        }

        bool cpu = (groups & SensorGroup.CpuHardware) != 0;
        bool gpu = (groups & SensorGroup.Gpu) != 0;

        // GPU AMD: direto pelo driver, bem mais barato. Os demais casos ficam com o LibreHardwareMonitor.
        if (gpu && _amdGpu is null) _amdGpu = AmdGpu.TryOpen(gpuHint);
        if (!gpu)
        {
            _amdGpu?.Dispose();
            _amdGpu = null;
        }
        bool lhmGpu = gpu && _amdGpu is null;

        bool board = Board;
        bool rebuild = _hardware is null
            ? cpu || lhmGpu || board
            : _hardware.HasCpu != cpu || _hardware.HasGpu != lhmGpu || _hardware.HasBoard != board || _hardware.GpuHint != gpuHint;
        if (rebuild)
        {
            _hardware?.Dispose();
            _hardware = cpu || lhmGpu || board ? HardwareMonitor.TryCreate(cpu, lhmGpu, board, gpuHint) : null;
        }

        _groups = groups;
        _nextDiskRead = 0;
        if ((groups & SensorGroup.CpuLoad) != 0) _cpuLoad.Sample(); // primeira amostra só serve de referência
        Log.Info($"Sensores ativos: {groups}");
    }

    public void Update()
    {
        var r = _readings;
        r.Now = DateTime.Now;

        if ((_groups & SensorGroup.CpuLoad) != 0)
            r.CpuLoad = _cpuLoad.Sample() ?? r.CpuLoad;

        // Uma leitura que trava segura a thread das telas: as lentas vão para o log, com a parte culpada.
        // A CPU (e a placa) primeiro: a curva dos fans usa a temperatura dela mesmo se o driver da AMD travar depois.
        long start = System.Diagnostics.Stopwatch.GetTimestamp();
        _hardware?.Update(r);
        CpuReadAt = Environment.TickCount64;
        long afterHardware = System.Diagnostics.Stopwatch.GetTimestamp();
        _amdGpu?.Update(r);
        long afterGpu = System.Diagnostics.Stopwatch.GetTimestamp();
        double hardwareMs = (afterHardware - start) * 1000.0 / System.Diagnostics.Stopwatch.Frequency;
        double gpuMs = (afterGpu - afterHardware) * 1000.0 / System.Diagnostics.Stopwatch.Frequency;
        if (gpuMs + hardwareMs > 1000)
            Log.Warn($"Sensores: leitura lenta — GPU (driver AMD) {gpuMs:0} ms, CPU (LibreHardwareMonitor) {hardwareMs:0} ms.");

        if ((_groups & SensorGroup.Memory) != 0)
            NativeSensors.ReadMemory(r);

        if ((_groups & SensorGroup.Disk) != 0 && Environment.TickCount64 >= _nextDiskRead)
        {
            NativeSensors.ReadDisks(r);
            _nextDiskRead = Environment.TickCount64 + DiskIntervalMs;
        }

        if ((_groups & SensorGroup.Network) != 0)
            _network.Sample(r);
    }

    /// <summary>Quando a CPU foi lida pela última vez (Environment.TickCount64).</summary>
    public long CpuReadAt { get; private set; }

    /// <summary>Resumo curto para a dica do ícone e o log, ex.: "CPU 52°C · GPU 47°C · RAM 63%".</summary>
    public string Summary()
    {
        var r = _readings;
        var parts = new List<string>(3);
        if (r.CpuTemp is float cpuTemp) parts.Add($"CPU {cpuTemp:0}°C");
        else if (r.CpuLoad is float cpuLoad) parts.Add($"CPU {cpuLoad:0}%");
        if (r.GpuTemp is float gpuTemp) parts.Add($"GPU {gpuTemp:0}°C");
        if (r.MemoryLoad is double memory) parts.Add($"RAM {memory:0}%");
        return string.Join(" · ", parts);
    }

    public string Text(string key) =>
        SensorKeys.TryGet(key, out var definition) ? SensorKeys.Format(definition, _readings) : SensorKeys.Missing;

    public double? Number(string key) =>
        SensorKeys.TryGet(key, out var definition) && definition.Number is not null ? definition.Number(_readings) : null;

    public void Dispose()
    {
        _amdGpu?.Dispose();
        _amdGpu = null;
        _hardware?.Dispose();
        _hardware = null;
    }
}
