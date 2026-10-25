using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DS339Monitor.Display;

/// <summary>
/// Ciclo de vida da DS339: inicia o SDK, acompanha conexão/desconexão USB, configura a resolução
/// nativa (376x960, retrato) e envia frames BGRA.
///
/// Os callbacks do SDK chegam numa thread nativa e só enfileiram eventos; quem chama <see cref="Pump"/>,
/// <see cref="Present"/> etc. é sempre a mesma thread (o loop principal). Assim nenhuma trava é
/// compartilhada com o SDK.
/// </summary>
internal sealed unsafe class Ds339Display : IDisposable
{
    public const int PanelWidth = 376;
    public const int PanelHeight = 960;
    private const int FrameBytes = PanelWidth * PanelHeight * 4;

    // O JONSBO-AIO espera ~1 s depois do attach antes de configurar a resolução.
    private const long SettleMs = 1000;
    private const long RetryConfigureMs = 5000;

    private static Ds339Display? _current;

    private readonly ConcurrentQueue<DeviceEvent> _events = new();
    private MsEdid* _edid;
    private MsPicture* _picture;
    private byte* _frame;
    private bool _started;
    private bool _disposed;

    private uint _handle;
    private bool _attached;
    private bool _configured;
    private long _attachedAt;
    private long _lastConfigureAttempt;
    private MsResolution[] _resolutions = Array.Empty<MsResolution>();
    private int _sendFailures;

    /// <summary>Buffer nativo do painel: 376x960, BGRA, stride 376*4. Escreva aqui antes de <see cref="Present"/>.</summary>
    public IntPtr FrameBuffer => (IntPtr)_frame;

    /// <summary>O mesmo buffer de <see cref="FrameBuffer"/>, como bytes BGRA.</summary>
    public Span<byte> FrameSpan => new(_frame, FrameBytes);

    public bool IsReady => _attached && _configured;

    public bool Start()
    {
        if (_started) return true;
        _current = this;
        if (_frame == null)
        {
            _frame = (byte*)NativeMemory.AllocZeroed(FrameBytes);
            _picture = (MsPicture*)NativeMemory.AllocZeroed((nuint)sizeof(MsPicture));
            _edid = MsDisplaySdk.AllocEdid();
        }

        try
        {
            MsDisplaySdk.Wrraper_MSDisplayRegisterCallback(
                (IntPtr)(delegate* unmanaged[Cdecl]<uint, MsResolution*, int, void>)&OnAttachNative,
                (IntPtr)(delegate* unmanaged[Cdecl]<uint, void>)&OnDetachNative);

            int status = MsDisplaySdk.Wrraper_MSDisplayStart(1, _edid);
            Log.Info($"MSDisplayStart = {status}");
            _started = status == 0;
            return _started;
        }
        catch (DllNotFoundException ex)
        {
            Log.Error("MSDISPLAYSDKWRRAPER.dll ou libusb0.dll não encontrada (driver da tela instalado?)", ex);
            return false;
        }
    }

    /// <summary>Processa conexões/desconexões pendentes e configura a tela quando ela estiver pronta.</summary>
    public void Pump()
    {
        while (_events.TryDequeue(out var ev))
        {
            if (ev.Attached)
            {
                // Adota sempre a conexão mais recente: ao voltar da suspensão o "conectou" do handle novo
                // pode chegar antes do "desconectou" do antigo.
                if (_attached && _handle != ev.Handle)
                    Log.Warn($"Nova conexão (handle {ev.Handle}) substitui a anterior (handle {_handle}).");
                _handle = ev.Handle;
                _resolutions = ev.Resolutions;
                _attached = true;
                _configured = false;
                _attachedAt = Environment.TickCount64;
                _lastConfigureAttempt = 0;
                Log.Info($"Tela conectada (handle {ev.Handle}). Resoluções: {string.Join(", ", ev.Resolutions)}");
            }
            else if (_attached && ev.Handle == _handle)
            {
                _attached = false;
                _configured = false;
                Log.Info($"Tela desconectada (handle {ev.Handle}).");
            }
        }

        if (_attached && !_configured)
            TryConfigure();
    }

    private void TryConfigure()
    {
        long now = Environment.TickCount64;
        if (now - _attachedAt < SettleMs) return;
        if (_lastConfigureAttempt != 0 && now - _lastConfigureAttempt < RetryConfigureMs) return;
        _lastConfigureAttempt = now;

        int index = Array.FindIndex(_resolutions, r => r.Width == PanelWidth && r.Height == PanelHeight);
        if (index < 0)
        {
            Log.Error($"A tela não anunciou {PanelWidth}x{PanelHeight}. Resoluções: {string.Join(", ", _resolutions)}");
            return;
        }

        var resolution = _resolutions[index];
        int status = MsDisplaySdk.Wrraper_MSDisplaySetVideoParam(_handle, &resolution);
        Log.Info($"SetVideoParam({resolution}) = {status}");
        Thread.Sleep(100);
        _configured = status == 0;
        _sendFailures = 0;
    }

    /// <summary>Envia o conteúdo atual de <see cref="FrameBuffer"/> para a tela.</summary>
    public bool Present()
    {
        if (!IsReady) return false;

        _picture->Width = PanelWidth;
        _picture->Height = PanelHeight;
        _picture->Data = (IntPtr)_frame;
        int status = MsDisplaySdk.Wrraper_MSDisplaySendPicture(_handle, _picture, 0);
        if (status == 0)
        {
            _sendFailures = 0;
            return true;
        }

        _sendFailures++;
        if (_sendFailures is 1 or 10 or 100)
            Log.Warn($"SendPicture falhou ({status}), {_sendFailures}x seguidas.");
        if (_sendFailures >= 10)
        {
            // Reconfigura do zero: costuma resolver depois de suspensão/hibernação.
            _configured = false;
            _lastConfigureAttempt = 0;
        }
        return false;
    }

    public void Clear() => NativeMemory.Clear(_frame, FrameBytes);

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_started)
        {
            // Não deixa a tela congelada mostrando números velhos.
            if (IsReady)
            {
                Clear();
                Present();
            }
            try
            {
                MsDisplaySdk.Wrraper_MSDisplayStop();
            }
            catch (Exception ex)
            {
                Log.Error("MSDisplayStop", ex);
            }
        }

        if (ReferenceEquals(_current, this)) _current = null;
        NativeMemory.Free(_frame);
        NativeMemory.Free(_picture);
        NativeMemory.Free(_edid);
        _frame = null;
        _picture = null;
        _edid = null;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void OnAttachNative(uint handle, MsResolution* resolutions, int count)
    {
        try
        {
            var list = new MsResolution[Math.Max(0, count)];
            for (int i = 0; i < list.Length; i++)
                list[i] = resolutions[i];
            _current?._events.Enqueue(new DeviceEvent(true, handle, list));
        }
        catch
        {
            // Exceção não pode atravessar para o código nativo.
        }
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void OnDetachNative(uint handle)
    {
        try
        {
            _current?._events.Enqueue(new DeviceEvent(false, handle, Array.Empty<MsResolution>()));
        }
        catch
        {
        }
    }

    private readonly record struct DeviceEvent(bool Attached, uint Handle, MsResolution[] Resolutions);

}
