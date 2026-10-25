using System.Globalization;

namespace DS339Monitor.Sensors;

/// <summary>Grupos de sensores. Só os grupos usados pelo tema ativo são lidos.</summary>
[Flags]
internal enum SensorGroup
{
    None = 0,
    Clock = 1,
    CpuLoad = 2,
    CpuHardware = 4, // temperatura/clock/potência/voltagem — LibreHardwareMonitor (PawnIO, requer admin)
    Gpu = 8,
    Memory = 16,
    Disk = 32,
    Network = 64,
}

internal sealed class SensorKey
{
    public SensorKey(SensorGroup group, Func<Readings, double?>? number, Func<Readings, string?>? text, string format)
    {
        Group = group;
        Number = number;
        Text = text;
        Format = format;
    }

    public SensorGroup Group { get; }
    public Func<Readings, double?>? Number { get; }
    public Func<Readings, string?>? Text { get; }
    public string Format { get; }
}

/// <summary>
/// Catálogo de dados que um tema pode exibir. Os nomes seguem os do JONSBO-AIO (Vmax.Model.SystemData),
/// para que os temas oficiais funcionem sem alteração.
/// </summary>
internal static class SensorKeys
{
    public const string Missing = "--";

    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
    private static readonly Dictionary<string, SensorKey> All = new(StringComparer.OrdinalIgnoreCase);

    public static CultureInfo Culture { get; set; } = CultureInfo.GetCultureInfo("pt-BR");

    static SensorKeys()
    {
        // Relógio e data
        Txt("CurrentTime", SensorGroup.Clock, r => r.Now.ToString("HH:mm:ss", Inv));
        Txt("CurrentTimeShut", SensorGroup.Clock, r => r.Now.ToString("HH:mm", Inv));
        Txt("CurrentH", SensorGroup.Clock, r => r.Now.ToString("HH", Inv));
        Txt("CurrentM", SensorGroup.Clock, r => r.Now.ToString("mm", Inv));
        Txt("CurrentDate", SensorGroup.Clock, r => r.Now.ToString("dd/MM/yyyy", Inv));
        Txt("CurrentDates", SensorGroup.Clock, r => r.Now.ToString("yyyy.MM.dd", Inv));
        Txt("CurrentDatesMD", SensorGroup.Clock, r => r.Now.ToString("dd/MM", Inv));
        Txt("CurrentDatesY", SensorGroup.Clock, r => r.Now.ToString("yyyy", Inv));
        Txt("CurrentDatesM", SensorGroup.Clock, r => r.Now.ToString("MM", Inv));
        Txt("CurrentDatesD", SensorGroup.Clock, r => r.Now.ToString("dd", Inv));
        Txt("WeekDays", SensorGroup.Clock, r => WeekDay(r.Now));
        Txt("CurrentWeek", SensorGroup.Clock, r => WeekDay(r.Now));
        Txt("WeekDaysL3", SensorGroup.Clock, r => WeekDayShort(r.Now));
        // "Ter, 13 out" — curto para caber em letra grande
        Txt("CurrentDateShort", SensorGroup.Clock, r => string.Create(Culture, $"{Capitalize(Culture.DateTimeFormat.GetAbbreviatedDayName(r.Now.DayOfWeek).TrimEnd('.'))}, {r.Now.Day} {Culture.DateTimeFormat.GetAbbreviatedMonthName(r.Now.Month).TrimEnd('.')}"));
        // "Terça-feira, 13 de outubro" (o formato dia/mês segue o idioma configurado)
        Txt("CurrentDateLong", SensorGroup.Clock, r => WeekDay(r.Now) + ", " + r.Now.ToString(Culture.DateTimeFormat.MonthDayPattern, Culture));

        // CPU
        Num("CpuUsage", SensorGroup.CpuLoad, r => r.CpuLoad, aliases: new[] { "CpuUse", "CPULoad" });
        Num("CPUTemp", SensorGroup.CpuHardware, r => r.CpuTemp, aliases: new[] { "CPUT" });
        Num("CpuFrequency", SensorGroup.CpuHardware, r => r.CpuClock);
        Num("CpuFrequencyGhz", SensorGroup.CpuHardware, r => r.CpuClock / 1000, "0.00");
        Num("CpuVoltage", SensorGroup.CpuHardware, r => r.CpuVoltage, "0.00");
        Num("CpuTEC", SensorGroup.CpuHardware, r => r.CpuPower, aliases: new[] { "CPUPower" });
        Txt("CPUName", SensorGroup.None, _ => SystemInfo.CpuName);

        // GPU
        Num("GpuUsage", SensorGroup.Gpu, r => r.GpuLoad, aliases: new[] { "GPULoad" });
        Num("GPUTemp", SensorGroup.Gpu, r => r.GpuTemp, aliases: new[] { "GPUT" });
        Num("GpuHotSpot", SensorGroup.Gpu, r => r.GpuHotSpot);
        Num("GpuFrequency", SensorGroup.Gpu, r => r.GpuClock);
        Num("GpuFrequencyGhz", SensorGroup.Gpu, r => r.GpuClock / 1000, "0.00");
        Num("GPUMemoryFrequency", SensorGroup.Gpu, r => r.GpuMemoryClock);
        Num("GpuVoltage", SensorGroup.Gpu, r => r.GpuVoltage, "0.00");
        Num("GpuTEC", SensorGroup.Gpu, r => r.GpuPower, aliases: new[] { "GPUPower" });
        Num("GPUMemoryLoad", SensorGroup.Gpu, r => r.GpuMemoryLoad, aliases: new[] { "GPUMemoryUseInt" });
        Txt("GPUMemoryUse", SensorGroup.Gpu, r => r.GpuMemoryUsedMb is float used && r.GpuMemoryTotalMb is float total && total > 0
            ? string.Create(Inv, $"{used / 1024:0.0}/{total / 1024:0} GB")
            : null);
        Num("GpuFan", SensorGroup.Gpu, r => r.GpuFan);
        Txt("GPUName", SensorGroup.Gpu, r => r.GpuName);

        // Memória
        Num("MemorySize", SensorGroup.Memory, r => r.MemoryTotalGb, "0.0");
        Num("TotalMemory", SensorGroup.Memory, r => r.MemoryTotalGb);
        Num("MemoryUsage", SensorGroup.Memory, r => r.MemoryLoad, aliases: new[] { "MemoryUseInt" });
        Num("MemoryUsageSize", SensorGroup.Memory, r => r.MemoryUsedGb, "0.0");
        Num("MemoryCanUsageSize", SensorGroup.Memory, r => r.MemoryAvailableGb, "0.0");
        Num("MemoryCanUsage", SensorGroup.Memory, r => 100 - r.MemoryLoad);
        Txt("MemoryUse", SensorGroup.Memory, r => r.MemoryUsedGb is double used && r.MemoryTotalGb is double total
            ? string.Create(Inv, $"{used:0.0}/{total:0.0} GB")
            : null);

        // Disco (unidade do sistema e C:..F:)
        Num("HardDiskCapacity", SensorGroup.Disk, r => SystemDisk(r)?.TotalGb);
        Num("HardDiskUsedSize", SensorGroup.Disk, r => SystemDisk(r)?.UsedGb);
        Num("HardDiskFree", SensorGroup.Disk, r => SystemDisk(r)?.FreeGb);
        Num("HardDiskUsed", SensorGroup.Disk, r => SystemDisk(r)?.UsedPercent);
        foreach (char drive in "CDEF")
        {
            char d = drive;
            Num($"Disk{d}Used", SensorGroup.Disk, r => Disk(r, d)?.UsedPercent);
            Num($"Disk{d}UsedSize", SensorGroup.Disk, r => Disk(r, d)?.UsedGb);
        }

        // Rede
        Txt("UpNetSpeed", SensorGroup.Network, r => Speed(r.NetUpBytesPerSec));
        Txt("DownNetSpeed", SensorGroup.Network, r => Speed(r.NetDownBytesPerSec));
        Num("NetDownBytes", SensorGroup.Network, r => r.NetDownBytesPerSec); // bytes/s, para o "download concluído" da cena
    }

    public static bool TryGet(string key, out SensorKey definition) => All.TryGetValue(key, out definition!);

    public static string Format(SensorKey definition, Readings readings)
    {
        if (definition.Text is not null)
            return definition.Text(readings) ?? Missing;

        double? value = definition.Number!(readings);
        return value is double d && double.IsFinite(d) ? d.ToString(definition.Format, Inv) : Missing;
    }

    private static void Num(string name, SensorGroup group, Func<Readings, double?> number, string format = "0", string[]? aliases = null)
    {
        var key = new SensorKey(group, number, null, format);
        All[name] = key;
        foreach (var alias in aliases ?? Array.Empty<string>())
            All[alias] = key;
    }

    private static void Txt(string name, SensorGroup group, Func<Readings, string?> text) =>
        All[name] = new SensorKey(group, null, text, "");

    private static string WeekDay(DateTime now) => Capitalize(Culture.DateTimeFormat.GetDayName(now.DayOfWeek));

    private static string Capitalize(string text) => text.Length == 0 ? text : char.ToUpper(text[0], Culture) + text[1..];

    private static string WeekDayShort(DateTime now)
    {
        string name = Culture.DateTimeFormat.GetDayName(now.DayOfWeek);
        return (name.Length > 3 ? name[..3] : name).ToUpper(Culture);
    }

    private static DiskSpace? SystemDisk(Readings r) => Disk(r, r.SystemDrive);

    private static DiskSpace? Disk(Readings r, char drive) => r.Disks.TryGetValue(drive, out var space) ? space : null;

    private static string? Speed(double? bytesPerSec)
    {
        if (bytesPerSec is not double b) return null;
        return b >= 1024 * 1024
            ? string.Create(Inv, $"{b / (1024 * 1024):0.0} MB/s")
            : string.Create(Inv, $"{b / 1024:0} KB/s");
    }
}
