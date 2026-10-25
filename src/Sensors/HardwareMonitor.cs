using LibreHardwareMonitor.Hardware;

namespace DS339Monitor.Sensors;

/// <summary>
/// Temperatura, clock, potência e voltagem de CPU/GPU via LibreHardwareMonitor.
/// Só abre o hardware pedido (CPU, GPU e, para a curva dos fans, o chip de sensores da placa-mãe); discos, memória etc.
/// ficam desligados. A CPU e a placa precisam do driver PawnIO e de privilégio de administrador.
/// Tudo o que mexe no LibreHardwareMonitor passa por <see cref="Gate"/>: a curva dos fans escreve de outra thread.
/// </summary>
internal sealed class HardwareMonitor : IDisposable
{
    private const int MaxResolveAttempts = 5;

    private readonly Computer _computer;
    private readonly IHardware? _cpu;
    private readonly IHardware? _gpu;
    private readonly IHardware? _superIo;
    private int _updates;
    private bool _disposed;

    private ISensor? _cpuTemp;
    private ISensor? _cpuPower;
    private ISensor? _cpuVoltage;
    private ISensor? _cpuClockAverage;
    private ISensor[] _cpuClocks = Array.Empty<ISensor>();
    private ISensor[] _cpuVids = Array.Empty<ISensor>();

    private ISensor? _gpuLoad;
    private ISensor? _gpuTemp;
    private ISensor? _gpuHotSpot;
    private ISensor? _gpuClock;
    private ISensor? _gpuMemoryClock;
    private ISensor? _gpuPower;
    private ISensor? _gpuVoltage;
    private ISensor? _gpuMemoryUsed;
    private ISensor? _gpuMemoryTotal;
    private ISensor? _gpuFan;

    private HardwareMonitor(bool cpu, bool gpu, bool board, string? gpuHint)
    {
        HasCpu = cpu;
        HasGpu = gpu;
        HasBoard = board;
        GpuHint = gpuHint;

        _computer = new Computer { IsCpuEnabled = cpu, IsGpuEnabled = gpu, IsMotherboardEnabled = board };
        _computer.Open();

        if (board)
        {
            var motherboard = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Motherboard);
            _superIo = motherboard?.SubHardware.FirstOrDefault(h => h.HardwareType == HardwareType.SuperIO);
            _superIo?.Update();
            BoardName = motherboard is null ? null : $"{motherboard.Name} · {_superIo?.Name ?? "sem chip de sensores"}";
            if (_superIo is not null)
                Log.Info($"Placa-mãe: {BoardName} — {BoardControls.Count} saídas de PWM, {BoardFans.Count} entradas de rotação.");
        }

        if (cpu)
        {
            _cpu = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Cpu);
            _cpu?.Update();
        }
        if (gpu) _gpu = PickGpu(gpuHint);

        Resolve();
        Log.Info($"Hardware: CPU = {_cpu?.Name ?? "(nenhuma)"}, GPU = {_gpu?.Name ?? "(nenhuma)"}");
        if (_cpu is not null)
        {
            Log.Info($"Sensores da CPU: temperatura '{_cpuTemp?.Name}', potência '{_cpuPower?.Name}', voltagem '{_cpuVoltage?.Name ?? $"VID ({(CpuUsesSvi3 ? "SVI3" : "SVI2")})"}', clock '{_cpuClockAverage?.Name ?? $"média de {_cpuClocks.Length} núcleos"}'");
            Log.Info("Voltagens disponíveis: " + string.Join(", ", _cpu.Sensors
                .Where(s => s.SensorType == SensorType.Voltage)
                .Select(s => $"{s.Name}={s.Value?.ToString("0.000") ?? "null"}")));
        }
        if (_cpu is not null && Positive(_cpuTemp?.Value) is null)
            Log.Warn("Temperatura da CPU indisponível. Rode como administrador (e com o driver PawnIO instalado).");
    }

    public bool HasCpu { get; }
    public bool HasGpu { get; }
    public bool HasBoard { get; }
    public string? GpuHint { get; }

    /// <summary>Trava de todo acesso ao LibreHardwareMonitor (leitura dos sensores e escrita nos fans).</summary>
    public object Gate { get; } = new();

    /// <summary>Fechado (os objetos do LibreHardwareMonitor não valem mais).</summary>
    public bool IsDisposed => _disposed;

    /// <summary>"B650M Snow Dream · ITE IT8613E" (null = placa-mãe desligada).</summary>
    public string? BoardName { get; }

    /// <summary>As saídas de PWM do chip da placa (sensores do tipo Control, com o controle de software).</summary>
    public IReadOnlyList<ISensor> BoardControls =>
        _superIo?.Sensors.Where(s => s.SensorType == SensorType.Control && s.Control is not null).OrderBy(s => s.Index).ToArray() ?? Array.Empty<ISensor>();

    /// <summary>As entradas de rotação do chip da placa.</summary>
    public IReadOnlyList<ISensor> BoardFans =>
        _superIo?.Sensors.Where(s => s.SensorType == SensorType.Fan).OrderBy(s => s.Index).ToArray() ?? Array.Empty<ISensor>();

    public static HardwareMonitor? TryCreate(bool cpu, bool gpu, bool board, string? gpuHint)
    {
        try
        {
            return new HardwareMonitor(cpu, gpu, board, gpuHint);
        }
        catch (Exception ex)
        {
            Log.Error("Falha ao abrir sensores de hardware", ex);
            return null;
        }
    }

    public void Update(Readings r)
    {
        lock (Gate)
        {
            if (_disposed) return;
            _cpu?.Update();
            _gpu?.Update();
            _superIo?.Update();
        }

        // Alguns sensores só aparecem depois das primeiras leituras.
        if (++_updates <= MaxResolveAttempts && MissingSensors()) Resolve();

        if (_cpu is not null)
        {
            // Sem acesso ao driver (não-admin) a biblioteca devolve 0 em vez de null.
            r.CpuTemp = Positive(_cpuTemp?.Value);
            r.CpuPower = Positive(_cpuPower?.Value);
            r.CpuVoltage = Positive(_cpuVoltage?.Value) ?? VidVoltage();
            r.CpuClock = Positive(_cpuClockAverage?.Value) ?? Average(_cpuClocks);
        }

        if (_gpu is not null)
        {
            r.GpuName = _gpu.Name;
            r.GpuLoad = _gpuLoad?.Value;
            r.GpuTemp = _gpuTemp?.Value;
            r.GpuHotSpot = _gpuHotSpot?.Value;
            r.GpuClock = _gpuClock?.Value;
            r.GpuMemoryClock = _gpuMemoryClock?.Value;
            r.GpuPower = _gpuPower?.Value;
            r.GpuVoltage = _gpuVoltage?.Value;
            r.GpuMemoryUsedMb = _gpuMemoryUsed?.Value;
            r.GpuMemoryTotalMb = _gpuMemoryTotal?.Value;
            // % de VRAM ocupada (o sensor "GPU Memory" da AMD é atividade do controlador, outra coisa).
            r.GpuMemoryLoad = r.GpuMemoryUsedMb is float used && r.GpuMemoryTotalMb is float total && total > 0 ? used * 100 / total : null;
            r.GpuFan = _gpuFan?.Value;
        }
    }

    /// <summary>Mede quanto custa atualizar cada hardware (diagnóstico).</summary>
    public void LogUpdateCost(int iterations = 20)
    {
        foreach (var hardware in new[] { _cpu, _gpu })
        {
            if (hardware is null) continue;
            long start = System.Diagnostics.Stopwatch.GetTimestamp();
            for (int i = 0; i < iterations; i++) hardware.Update();
            double ms = System.Diagnostics.Stopwatch.GetElapsedTime(start).TotalMilliseconds / iterations;
            Log.Info($"Custo de Update() — {hardware.Name}: {ms:0.00} ms");
        }
    }

    /// <summary>Despeja todos os sensores no log — útil para ajustar os nomes procurados.</summary>
    public void LogAllSensors()
    {
        foreach (var hardware in _computer.Hardware)
        {
            hardware.Update();
            Log.Info($"[{hardware.HardwareType}] {hardware.Name}");
            foreach (var sensor in hardware.Sensors.OrderBy(s => s.SensorType).ThenBy(s => s.Index))
                Log.Info($"    {sensor.SensorType,-12} {sensor.Name,-32} = {sensor.Value?.ToString("0.###") ?? "null"}");
        }
    }

    public void Dispose()
    {
        try
        {
            lock (Gate)
            {
                if (_disposed) return;
                _disposed = true;
                _computer.Close(); // o LibreHardwareMonitor devolve o controle dos fans à BIOS ao fechar
            }
        }
        catch (Exception ex)
        {
            Log.Error("Fechando LibreHardwareMonitor", ex);
        }
    }

    private IHardware? PickGpu(string? hint)
    {
        var gpus = _computer.Hardware
            .Where(h => h.HardwareType is HardwareType.GpuAmd or HardwareType.GpuNvidia or HardwareType.GpuIntel)
            .ToList();
        foreach (var gpu in gpus) gpu.Update();

        if (!string.IsNullOrWhiteSpace(hint))
        {
            var match = gpus.FirstOrDefault(g => g.Name.Contains(hint, StringComparison.OrdinalIgnoreCase));
            if (match is not null) return match;
            Log.Warn($"Nenhuma GPU com '{hint}' no nome; escolhendo automaticamente.");
        }

        // Prefere a placa dedicada: a que tem mais memória de vídeo (a integrada do Ryzen tem ~512 MB).
        return gpus
            .OrderByDescending(g => Find(g, SensorType.SmallData, "GPU Memory Total", "D3D Dedicated Memory Total")?.Value ?? 0)
            .ThenBy(g => g.HardwareType == HardwareType.GpuIntel)
            .FirstOrDefault();
    }

    private bool MissingSensors() =>
        (_cpu is not null && (_cpuTemp is null || _cpuPower is null || _cpuClocks.Length == 0))
        || (_gpu is not null && (_gpuTemp is null || _gpuLoad is null || _gpuPower is null));

    private void Resolve()
    {
        if (_cpu is { } cpu)
        {
            _cpuTemp = Find(cpu, SensorType.Temperature, "Core (Tctl/Tdie)", "CPU Package", "Core (Tctl)", "Core (Tdie)", "Core Max", "Core Average")
                ?? cpu.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Temperature);
            _cpuPower = Find(cpu, SensorType.Power, "Package", "CPU Package")
                ?? cpu.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Power);
            // Telemetria real do regulador, se existir; senão a tensão pedida pelos núcleos (VID), ver VidVoltage().
            _cpuVoltage = Find(cpu, SensorType.Voltage, "Core (SVI3 TFN)", "Core (SVI2 TFN)", "CPU Core", "Vcore");
            _cpuVids = cpu.Sensors
                .Where(s => s.SensorType == SensorType.Voltage && s.Name.EndsWith("VID", StringComparison.OrdinalIgnoreCase))
                .ToArray();
            _cpuClockAverage = Find(cpu, SensorType.Clock, "Cores (Average)");
            _cpuClocks = cpu.Sensors
                .Where(s => s.SensorType == SensorType.Clock && IsCoreClock(s.Name))
                .ToArray();
        }

        if (_gpu is { } gpu)
        {
            _gpuLoad = Find(gpu, SensorType.Load, "GPU Core", "D3D 3D");
            _gpuTemp = Find(gpu, SensorType.Temperature, "GPU Core", "GPU Edge")
                ?? gpu.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Temperature);
            _gpuHotSpot = Find(gpu, SensorType.Temperature, "GPU Hot Spot");
            _gpuClock = Find(gpu, SensorType.Clock, "GPU Core");
            _gpuMemoryClock = Find(gpu, SensorType.Clock, "GPU Memory");
            _gpuPower = Find(gpu, SensorType.Power, "GPU Package", "GPU Core", "GPU Power")
                ?? gpu.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Power);
            _gpuVoltage = Find(gpu, SensorType.Voltage, "GPU Core");
            _gpuMemoryUsed = Find(gpu, SensorType.SmallData, "GPU Memory Used", "D3D Dedicated Memory Used");
            _gpuMemoryTotal = Find(gpu, SensorType.SmallData, "GPU Memory Total", "D3D Dedicated Memory Total");
            _gpuFan = Find(gpu, SensorType.Fan, "GPU Fan");
        }
    }

    private static bool IsCoreClock(string name)
    {
        // "Core #1", "Core #2"... (exclui "Bus Speed", "Cores (Average)" etc.)
        if (!name.StartsWith("Core #", StringComparison.OrdinalIgnoreCase)) return false;
        for (int i = 6; i < name.Length; i++)
            if (!char.IsDigit(name[i])) return false;
        return name.Length > 6;
    }

    private static ISensor? Find(IHardware hardware, SensorType type, params string[] names)
    {
        foreach (var name in names)
        {
            foreach (var sensor in hardware.Sensors)
            {
                if (sensor.SensorType == type && sensor.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return sensor;
            }
        }
        return null;
    }

    private static float? Positive(float? value) => value is > 0 ? value : null;

    private static readonly bool CpuUsesSvi3 = DetectSvi3();

    /// <summary>
    /// Tensão do núcleo a partir dos VIDs (o maior pedido manda no regulador, que é compartilhado).
    /// O LibreHardwareMonitor converte o VID sempre pela fórmula SVI2 (1,55 − 0,00625·vid); no Zen 4/5
    /// o regulador é SVI3 (0,245 + 0,005·vid) e o valor sai absurdo (~0,45 V). Aqui desfazemos e refazemos a conta.
    /// </summary>
    private float? VidVoltage()
    {
        float max = 0;
        foreach (var sensor in _cpuVids)
            if (sensor.Value is float v && v > max) max = v;
        if (max <= 0 || max >= 1.549f) return null; // 1,55 V = VID 0: o driver não respondeu (sem admin)
        if (!CpuUsesSvi3) return max;
        double vid = Math.Round((1.55 - max) / 0.00625);
        return (float)(0.245 + vid * 0.005);
    }

    private static bool DetectSvi3()
    {
        if (!System.Runtime.Intrinsics.X86.X86Base.IsSupported) return false;
        var (_, ebx, ecx, edx) = System.Runtime.Intrinsics.X86.X86Base.CpuId(0, 0);
        bool amd = ebx == 0x68747541 && edx == 0x69746E65 && ecx == 0x444D4163; // "AuthenticAMD"
        if (!amd) return false;
        int eax = System.Runtime.Intrinsics.X86.X86Base.CpuId(1, 0).Eax;
        int baseFamily = (eax >> 8) & 0xF;
        int family = baseFamily == 0xF ? baseFamily + ((eax >> 20) & 0xFF) : baseFamily;
        int model = ((eax >> 4) & 0xF) | (((eax >> 16) & 0xF) << 4);
        return family >= 0x1A                              // Zen 5
            || (family == 0x19 && model is >= 0x60 and <= 0x7F); // Zen 4 desktop/notebook (Raphael, Phoenix…)
    }

    private static float? Average(ISensor[] sensors)
    {
        float sum = 0;
        int count = 0;
        foreach (var sensor in sensors)
        {
            if (sensor.Value is float v && v > 0)
            {
                sum += v;
                count++;
            }
        }
        return count > 0 ? sum / count : null;
    }
}
