using System.Diagnostics;
using System.Globalization;

namespace DS339Monitor.Game;

/// <summary>
/// Os quadros do jogo, pelo PresentMon (ferramenta de código aberto da Intel, assinada; tools\PresentMon.exe): ele lê os
/// eventos de "apresentar quadro" do Windows (ETW) e escreve uma linha CSV por quadro. Precisa de administrador — o
/// programa já roda assim. Uma sessão só, com nome próprio, parada ao trocar de jogo ou fechar o programa.
/// </summary>
internal sealed class FrameReader : IDisposable
{
    private const string Session = "DS339MonitorJogo";
    private readonly Process? _process;
    private readonly Thread? _thread;
    private volatile bool _stopped;

    public FrameReader(int pid)
    {
        string exe = Path.Combine(AppContext.BaseDirectory, "tools", "PresentMon.exe");
        if (!File.Exists(exe))
        {
            Log.Warn("Modo jogo: tools\\PresentMon.exe não encontrado; sem FPS e frametime.");
            return;
        }
        var info = new ProcessStartInfo(exe,
            $"--process_id {pid} --output_stdout --no_console_stats --stop_existing_session --session_name {Session} --terminate_on_proc_exit")
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        try
        {
            _process = Process.Start(info);
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            Log.Error("Modo jogo: o PresentMon não abriu", ex);
            return;
        }
        if (_process is null) return;
        _process.ErrorDataReceived += (_, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.Data)) Log.Warn($"PresentMon: {e.Data}");
        };
        _process.BeginErrorReadLine();
        _thread = new Thread(Read) { IsBackground = true, Name = "PresentMon" };
        _thread.Start();
        Log.Info($"Modo jogo: lendo os quadros do processo {pid} pelo PresentMon.");
    }

    private void Read()
    {
        try
        {
            var reader = _process!.StandardOutput;
            string? header = reader.ReadLine();
            if (header is null) return;
            var columns = header.TrimStart('﻿').Split(',');
            int Index(params string[] names) => names.Select(n => Array.IndexOf(columns, n)).FirstOrDefault(i => i >= 0, -1);
            int frameTime = Index("MsBetweenPresents", "FrameTime");
            int gpuBusy = Index("MsGPUBusy", "GPUBusy");
            int cpuBusy = Index("MsCPUWait", "CPUWait"); // tempo preso na apresentação do quadro (sincronia)
            int time = Index("TimeInMs", "CPUStartTimeInMs", "CPUStartTime");
            int latency = Index("MsAllInputToPhotonLatency", "MsClickToPhotonLatency", "ClickToPhotonLatency");
            int mode = Index("PresentMode");
            int swapChain = Index("SwapChainAddress");
            if (frameTime < 0)
            {
                Log.Warn($"PresentMon: colunas inesperadas ({header}).");
                return;
            }
            // Jogos com mais de uma cadeia de troca (ex.: menu e jogo): vale a que apresenta mais quadros.
            var counts = new Dictionary<string, int>();
            string? main = null;
            string? line;
            while (!_stopped && (line = reader.ReadLine()) is not null)
            {
                var f = line.Split(',');
                if (f.Length <= frameTime) continue;
                string chain = swapChain >= 0 ? f[swapChain] : "";
                counts[chain] = counts.GetValueOrDefault(chain) + 1;
                if (main is null || counts[chain] > counts.GetValueOrDefault(main) + 30) main = chain;
                if (chain != main) continue;
                AddFrame(f);
            }

            void AddFrame(string[] f)
            {
                double Number(int i) => i >= 0 && i < f.Length && double.TryParse(f[i], NumberStyles.Float, CultureInfo.InvariantCulture, out double v) ? v : double.NaN;
                double ft = Number(frameTime);
                if (double.IsNaN(ft)) return;
                double at = Number(time);
                double lat = Number(latency);
                double gb = Number(gpuBusy), cb = Number(cpuBusy);
                GameTelemetry.AddFrame(double.IsNaN(at) ? Environment.TickCount64 : at, ft, double.IsNaN(gb) ? 0 : gb, double.IsNaN(cb) ? 0 : cb,
                    double.IsNaN(lat) ? null : lat, mode >= 0 && mode < f.Length ? f[mode] : "");
            }
        }
        catch (Exception ex) when (ex is IOException or ObjectDisposedException or InvalidOperationException)
        {
            // PresentMon fechou.
        }
    }

    public void Dispose()
    {
        _stopped = true;
        if (_process is null) return;
        try
        {
            if (!_process.HasExited) _process.Kill();
            _process.WaitForExit(2000);
        }
        catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception)
        {
        }
        _process.Dispose();
        // A sessão de rastreamento pode sobrar se o PresentMon morrer: encerra pelo nome.
        try
        {
            using var cleanup = Process.Start(new ProcessStartInfo(Path.Combine(AppContext.BaseDirectory, "tools", "PresentMon.exe"),
                $"--terminate_existing_session --session_name {Session}") { UseShellExecute = false, CreateNoWindow = true });
            cleanup?.WaitForExit(3000);
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
        }
    }
}
