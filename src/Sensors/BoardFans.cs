using LibreHardwareMonitor.Hardware;

namespace DS339Monitor.Sensors;

/// <summary>
/// Fans ligados na placa-mãe (o chip de sensores dela, o "Super I/O"): RPM e o PWM que a placa está mandando em cada
/// header. Precisa de administrador e do driver PawnIO.
/// </summary>
internal static class BoardFans
{
    /// <summary>Registra no log todos os fans, controles e temperaturas da placa-mãe (só leitura).</summary>
    public static void LogAll()
    {
        Log.Info($"=== Placa-mãe — admin: {Environment.IsPrivilegedProcess} ===");
        var computer = new Computer { IsMotherboardEnabled = true, IsControllerEnabled = true };
        try
        {
            computer.Open();
            foreach (var hardware in computer.Hardware)
            {
                hardware.Update();
                Log.Info($"[{hardware.HardwareType}] {hardware.Name} ({hardware.Identifier})");
                foreach (var sub in hardware.SubHardware)
                {
                    sub.Update();
                    Thread.Sleep(300);
                    sub.Update();
                    Log.Info($"  [{sub.HardwareType}] {sub.Name} ({sub.Identifier})");
                    foreach (var sensor in sub.Sensors.Where(s => s.SensorType is SensorType.Fan or SensorType.Control or SensorType.Temperature))
                    {
                        string control = sensor.Control is { } c
                            ? $" — controle: modo {c.ControlMode}, valor de software {c.SoftwareValue}, faixa {c.MinSoftwareValue}–{c.MaxSoftwareValue}"
                            : "";
                        Log.Info($"    {sensor.SensorType,-12} {sensor.Name,-28} = {sensor.Value?.ToString("0.#") ?? "null"} ({sensor.Identifier}){control}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("Placa-mãe", ex);
        }
        finally
        {
            computer.Close();
        }
    }
}
