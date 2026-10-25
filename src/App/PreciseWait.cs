using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace DS339Monitor.App;

/// <summary>
/// Relógio em milissegundos com precisão de verdade. <see cref="Environment.TickCount64"/> só anda a cada "tique" do
/// Windows (15,6 ms): agendar quadros de 33 ms com ele dá intervalos de 31 e 47 ms alternados.
/// </summary>
internal static class Clock
{
    private static readonly long TicksPerMs = Stopwatch.Frequency / 1000;

    public static long Ms => Stopwatch.GetTimestamp() / TicksPerMs;
}

/// <summary>
/// Espera com precisão de ~1 ms ou até o evento <c>wake</c>. O WaitOne comum também acorda no tique de 15,6 ms (pedir
/// 33 ms dá 31 ou 47), e a animação anda aos trancos. O timer de alta resolução do Windows 10 1803+ acorda na hora
/// sem mudar o relógio do sistema todo (timeBeginPeriod faria o PC inteiro gastar mais energia). Sem ele, cai no WaitOne.
/// </summary>
internal sealed class PreciseWait : IDisposable
{
    private const uint HighResolution = 0x2;      // CREATE_WAITABLE_TIMER_HIGH_RESOLUTION
    private const uint TimerAllAccess = 0x1F0003;

    private readonly WaitHandle _wake;
    private readonly TimerHandle? _timer;
    private readonly WaitHandle[] _handles;

    public PreciseWait(WaitHandle wake)
    {
        _wake = wake;
        var handle = CreateWaitableTimerExW(IntPtr.Zero, null, HighResolution, TimerAllAccess);
        if (handle.IsInvalid)
        {
            handle.Dispose();
            Log.Warn("Timer de alta resolução indisponível; animações podem andar menos lisas.");
            _handles = new[] { wake };
            return;
        }
        _timer = new TimerHandle(handle);
        _handles = new WaitHandle[] { wake, _timer };
    }

    public void Wait(int ms)
    {
        long due = -(long)ms * 10_000; // relativo, em unidades de 100 ns
        if (_timer is null || !SetWaitableTimer(_timer.SafeWaitHandle, ref due, 0, IntPtr.Zero, IntPtr.Zero, false))
        {
            _wake.WaitOne(ms);
            return;
        }
        WaitHandle.WaitAny(_handles);
    }

    public void Dispose() => _timer?.Dispose();

    private sealed class TimerHandle : WaitHandle
    {
        public TimerHandle(SafeWaitHandle handle) => SafeWaitHandle = handle;
    }

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern SafeWaitHandle CreateWaitableTimerExW(IntPtr attributes, string? name, uint flags, uint access);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWaitableTimer(SafeWaitHandle timer, ref long dueTime, int period, IntPtr completion,
        IntPtr argument, [MarshalAs(UnmanagedType.Bool)] bool resume);
}
