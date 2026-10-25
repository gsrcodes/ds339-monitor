namespace DS339Monitor.Sensors;

/// <summary>Última leitura de cada sensor. <c>null</c> = indisponível.</summary>
internal sealed class Readings
{
    public DateTime Now;

    public float? CpuLoad;
    public float? CpuTemp;
    public float? CpuClock;
    public float? CpuPower;
    public float? CpuVoltage;

    public string? GpuName;
    public float? GpuLoad;
    public float? GpuTemp;
    public float? GpuHotSpot;
    public float? GpuClock;
    public float? GpuMemoryClock;
    public float? GpuPower;
    public float? GpuVoltage;
    public float? GpuMemoryLoad;
    public float? GpuMemoryUsedMb;
    public float? GpuMemoryTotalMb;
    public float? GpuFan;

    public double? MemoryTotalGb;
    public double? MemoryUsedGb;
    public double? MemoryAvailableGb;
    public double? MemoryLoad;

    public readonly Dictionary<char, DiskSpace> Disks = new();
    public char SystemDrive = 'C';

    public double? NetUpBytesPerSec;
    public double? NetDownBytesPerSec;
}

internal readonly record struct DiskSpace(double TotalGb, double FreeGb)
{
    public double UsedGb => TotalGb - FreeGb;
    public double UsedPercent => TotalGb > 0 ? UsedGb / TotalGb * 100 : 0;
}
