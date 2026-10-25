using System.Runtime.InteropServices;

namespace DS339Monitor.Display;

// Interop com MSDISPLAYSDKWRRAPER.dll — SDK do chip MacroSilicon MS9132 usado na DS339.
// Assinaturas e structs reconstruídas a partir do JONSBO-AIO (Vmax.Helper.DeviceApi).

[StructLayout(LayoutKind.Sequential)]
internal struct MsResolution
{
    public int Width;
    public int Height;
    public int Refresh;

    public override readonly string ToString() => $"{Width}x{Height}@{Refresh}";
}

[StructLayout(LayoutKind.Sequential)]
internal struct MsPicture
{
    public int Width;
    public int Height;
    public IntPtr Data; // BGRA 32 bits, stride = Width * 4
}

[StructLayout(LayoutKind.Sequential)]
internal struct MsTiming
{
    public uint Vic;
    public uint Polarity;
    public uint HTotal;
    public uint VTotal;
    public uint HActive;
    public uint VActive;
    public uint PixClk;
    public uint VFreq;
    public uint HOffset;
    public uint VOffset;
    public uint HSyncWidth;
    public uint VSyncWidth;
}

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct MsEdid
{
    public const int MaxTimings = 16;

    public int Mode;
    public fixed byte EdidReplace[128];
    public int AppendCount;
    public fixed uint TimingAppend[MaxTimings * 12]; // 16 x MsTiming (12 uints cada)
}

internal static unsafe class MsDisplaySdk
{
    private const string Dll = "MSDISPLAYSDKWRRAPER.dll";

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int Wrraper_MSDisplayStart(int logLevel, MsEdid* edid);

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int Wrraper_MSDisplayStop();

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    public static extern void Wrraper_MSDisplayRegisterCallback(IntPtr attach, IntPtr detach);

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int Wrraper_MSDisplaySetVideoParam(uint handle, MsResolution* resolution);

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    public static extern int Wrraper_MSDisplaySendPicture(uint handle, MsPicture* picture, int wait);

    /// <summary>
    /// Timings customizados que o JONSBO-AIO injeta no EDID (modo 2). A DS339 é o VIC 171 (376x960).
    /// Os demais pertencem a outras telas do mesmo fabricante; mantidos para o chip aceitar o mesmo EDID.
    /// </summary>
    public static readonly MsTiming[] KnownTimings =
    {
        new() { Vic = 143, Polarity = 7, HTotal = 600, VTotal = 490, HActive = 480, VActive = 480, PixClk = 1764, VFreq = 6000, HOffset = 100, VOffset = 8, HSyncWidth = 50, VSyncWidth = 4 },
        new() { Vic = 147, Polarity = 7, HTotal = 452, VTotal = 568, HActive = 240, VActive = 320, PixClk = 1540, VFreq = 6000, HOffset = 190, VOffset = 206, HSyncWidth = 5, VSyncWidth = 20 },
        new() { Vic = 148, Polarity = 7, HTotal = 568, VTotal = 452, HActive = 320, VActive = 240, PixClk = 1540, VFreq = 6000, HOffset = 206, VOffset = 190, HSyncWidth = 20, VSyncWidth = 5 },
        new() { Vic = 156, Polarity = 7, HTotal = 640, VTotal = 352, HActive = 240, VActive = 240, PixClk = 1350, VFreq = 6000, HOffset = 190, VOffset = 51, HSyncWidth = 10, VSyncWidth = 10 },
        new() { Vic = 159, Polarity = 7, HTotal = 600, VTotal = 375, HActive = 480, VActive = 272, PixClk = 1350, VFreq = 6000, HOffset = 100, VOffset = 90, HSyncWidth = 50, VSyncWidth = 5 },
        new() { Vic = 160, Polarity = 7, HTotal = 472, VTotal = 996, HActive = 360, VActive = 960, PixClk = 2820, VFreq = 6000, HOffset = 88, VOffset = 33, HSyncWidth = 32, VSyncWidth = 10 },
        new() { Vic = 171, Polarity = 7, HTotal = 488, VTotal = 996, HActive = 376, VActive = 960, PixClk = 2916, VFreq = 6000, HOffset = 312, VOffset = 66, HSyncWidth = 112, VSyncWidth = 10 },
        new() { Vic = 177, Polarity = 7, HTotal = 672, VTotal = 1684, HActive = 600, VActive = 1600, PixClk = 5658, VFreq = 5000, HOffset = 32, VOffset = 44, HSyncWidth = 2, VSyncWidth = 4 },
    };

    public static MsEdid* AllocEdid()
    {
        var edid = (MsEdid*)NativeMemory.AllocZeroed((nuint)sizeof(MsEdid));
        edid->Mode = 2;
        edid->AppendCount = KnownTimings.Length;
        var dst = (MsTiming*)edid->TimingAppend;
        for (int i = 0; i < KnownTimings.Length; i++)
            dst[i] = KnownTimings[i];
        return edid;
    }
}
