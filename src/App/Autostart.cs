using System.ComponentModel;
using System.Diagnostics;
using System.Security;
using System.Security.Principal;
using System.Text;

namespace DS339Monitor.App;

/// <summary>
/// Inicialização junto com o Windows via Agendador de Tarefas (e não pela chave Run do registro):
/// é o único jeito de subir já como administrador sem aparecer o UAC a cada logon — e admin é
/// necessário para ler temperatura/potência da CPU.
/// </summary>
internal static class Autostart
{
    private const string TaskName = "DS339Monitor";

    public static bool IsEnabled() => Schtasks($"/Query /TN \"{TaskName}\"", logFailure: false) == 0;

    /// <summary>Abre o programa agora pela tarefa agendada (como administrador, sem a pergunta do UAC).</summary>
    public static bool RunNow() => Schtasks($"/Run /TN \"{TaskName}\"") == 0;

    /// <summary>Liga/desliga. Se este processo não for admin, relança a si mesmo elevado (UAC) só para isso.</summary>
    public static bool SetFromUi(bool enable)
    {
        if (Environment.IsPrivilegedProcess) return Set(enable);
        try
        {
            using var helper = Process.Start(new ProcessStartInfo(Environment.ProcessPath!, $"--autostart {(enable ? "on" : "off")}")
            {
                UseShellExecute = true,
                Verb = "runas",
            });
            if (helper is null) return false;
            helper.WaitForExit();
            return helper.ExitCode == 0;
        }
        catch (Win32Exception)
        {
            return false; // UAC cancelado
        }
    }

    public static bool Set(bool enable)
    {
        if (!enable)
            return Schtasks($"/Delete /TN \"{TaskName}\" /F") == 0 || !IsEnabled();

        string exe = Environment.ProcessPath!;
        string xmlPath = Path.Combine(Path.GetTempPath(), "ds339monitor-task.xml");
        try
        {
            File.WriteAllText(xmlPath, TaskXml(exe, WindowsIdentity.GetCurrent().Name), Encoding.Unicode);
            return Schtasks($"/Create /TN \"{TaskName}\" /XML \"{xmlPath}\" /F") == 0;
        }
        finally
        {
            try { File.Delete(xmlPath); } catch (IOException) { }
        }
    }

    private static string TaskXml(string exe, string user)
    {
        string e(string s) => SecurityElement.Escape(s);
        // Prioridade 7 = abaixo do normal. Sem limite de tempo e sem restrição de bateria.
        return $"""
            <?xml version="1.0" encoding="UTF-16"?>
            <Task version="1.2" xmlns="http://schemas.microsoft.com/windows/2004/02/mit/task">
              <RegistrationInfo>
                <Description>Monitor de hardware para a tela Jonsbo DS339</Description>
              </RegistrationInfo>
              <Triggers>
                <LogonTrigger>
                  <Enabled>true</Enabled>
                  <UserId>{e(user)}</UserId>
                  <Delay>PT10S</Delay>
                </LogonTrigger>
              </Triggers>
              <Principals>
                <Principal id="Author">
                  <UserId>{e(user)}</UserId>
                  <LogonType>InteractiveToken</LogonType>
                  <RunLevel>HighestAvailable</RunLevel>
                </Principal>
              </Principals>
              <Settings>
                <MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy>
                <DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries>
                <StopIfGoingOnBatteries>false</StopIfGoingOnBatteries>
                <AllowHardTerminate>true</AllowHardTerminate>
                <StartWhenAvailable>false</StartWhenAvailable>
                <RunOnlyIfNetworkAvailable>false</RunOnlyIfNetworkAvailable>
                <IdleSettings>
                  <StopOnIdleEnd>false</StopOnIdleEnd>
                  <RestartOnIdle>false</RestartOnIdle>
                </IdleSettings>
                <AllowStartOnDemand>true</AllowStartOnDemand>
                <Enabled>true</Enabled>
                <Hidden>false</Hidden>
                <RunOnlyIfIdle>false</RunOnlyIfIdle>
                <ExecutionTimeLimit>PT0S</ExecutionTimeLimit>
                <Priority>7</Priority>
              </Settings>
              <Actions Context="Author">
                <Exec>
                  <Command>{e(exe)}</Command>
                  <WorkingDirectory>{e(Path.GetDirectoryName(exe)!)}</WorkingDirectory>
                </Exec>
              </Actions>
            </Task>
            """;
    }

    private static int Schtasks(string arguments, bool logFailure = true)
    {
        var psi = new ProcessStartInfo("schtasks.exe", arguments)
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        using var process = Process.Start(psi)!;
        string output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
        process.WaitForExit();
        if (process.ExitCode != 0 && logFailure)
            Log.Warn($"schtasks {arguments} → {process.ExitCode}: {output.Trim()}");
        return process.ExitCode;
    }
}
