using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.Win32;

namespace DS339Monitor.Game;

/// <summary>
/// Game State Integration do CS2 — o recurso oficial da Valve para apps externos (placares, luzes), sem tocar no jogo: um
/// arquivo .cfg na pasta de configurações do CS2 faz o próprio jogo mandar o estado da partida (vida, colete, dinheiro,
/// placar, abates, arma) por HTTP para este programa, em 127.0.0.1. Só aceita mensagens com o código combinado no arquivo.
/// </summary>
internal sealed class Cs2Gsi : IDisposable
{
    public const int Port = 3339;
    private const string FileName = "gamestate_integration_ds339monitor.cfg";

    private readonly string _token;
    private readonly TcpListener _listener;
    private readonly Thread _thread;
    private volatile bool _stop;

    public Cs2Gsi(string token)
    {
        _token = token;
        _listener = new TcpListener(IPAddress.Loopback, Port);
        _listener.Start();
        _thread = new Thread(Serve) { IsBackground = true, Name = "CS2 (GSI)" };
        _thread.Start();
    }

    /// <summary>Quando chegou a última mensagem do CS2 (Environment.TickCount64; 0 = nunca).</summary>
    public static long LastMessage { get; private set; }

    private void Serve()
    {
        while (!_stop)
        {
            try
            {
                using var client = _listener.AcceptTcpClient();
                client.ReceiveTimeout = 2000;
                using var stream = client.GetStream();
                string? body = ReadRequest(stream);
                byte[] ok = Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\nContent-Length: 0\r\nConnection: close\r\n\r\n");
                stream.Write(ok);
                if (body is not null) Handle(body);
            }
            catch (Exception ex) when (ex is SocketException or IOException or ObjectDisposedException or InvalidOperationException)
            {
                if (_stop) return;
            }
        }
    }

    /// <summary>Lê uma requisição HTTP simples (cabeçalhos + corpo pelo Content-Length, até 256 KB).</summary>
    private static string? ReadRequest(NetworkStream stream)
    {
        var buffer = new MemoryStream();
        var chunk = new byte[8192];
        int headerEnd = -1, length = 0;
        while (buffer.Length < 262_144)
        {
            int read = stream.Read(chunk, 0, chunk.Length);
            if (read <= 0) break;
            buffer.Write(chunk, 0, read);
            var data = buffer.GetBuffer().AsSpan(0, (int)buffer.Length);
            if (headerEnd < 0)
            {
                headerEnd = data.IndexOf("\r\n\r\n"u8);
                if (headerEnd < 0) continue;
                string headers = Encoding.ASCII.GetString(data[..headerEnd]);
                foreach (string line in headers.Split("\r\n"))
                    if (line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase)) int.TryParse(line[15..].Trim(), out length);
            }
            if (data.Length >= headerEnd + 4 + length) return Encoding.UTF8.GetString(data.Slice(headerEnd + 4, length));
        }
        return null;
    }

    private void Handle(string body)
    {
        try
        {
            using var json = JsonDocument.Parse(body);
            var root = json.RootElement;
            if (!root.TryGetProperty("auth", out var auth) || Str(auth, "token") != _token) return;
            LastMessage = Environment.TickCount64;
            root.TryGetProperty("map", out var map);
            root.TryGetProperty("round", out var round);
            root.TryGetProperty("player", out var player);
            player.TryGetProperty("state", out var state);
            player.TryGetProperty("match_stats", out var stats);
            string? weapon = null;
            int? ammo = null, reserve = null;
            if (player.ValueKind == JsonValueKind.Object && player.TryGetProperty("weapons", out var weapons) && weapons.ValueKind == JsonValueKind.Object)
            {
                foreach (var w in weapons.EnumerateObject())
                {
                    if (Str(w.Value, "state") != "active") continue;
                    weapon = Pretty(Str(w.Value, "name"));
                    ammo = w.Value.TryGetProperty("ammo_clip", out var c) ? Int(c) : null;
                    reserve = w.Value.TryGetProperty("ammo_reserve", out var r) ? Int(r) : null;
                }
            }
            map.TryGetProperty("team_ct", out var ct);
            map.TryGetProperty("team_t", out var t);
            // Morto, o jogo passa a mandar o jogador que você está assistindo: o "provider" diz quem é você.
            root.TryGetProperty("provider", out var provider);
            string? self = Str(provider, "steamid"), shown = Str(player, "steamid");
            var cs2 = new Cs2State(
                Str(map, "name") ?? "", Str(map, "mode") ?? "", Str(map, "phase") ?? "", Num(map, "round"),
                Num(ct, "score"), Num(t, "score"), Str(player, "team") ?? "",
                Num(state, "health"), Num(state, "armor"), state.ValueKind == JsonValueKind.Object && state.TryGetProperty("helmet", out var h) && h.ValueKind == JsonValueKind.True,
                Num(state, "money"), Num(state, "round_kills"), Num(stats, "kills"), Num(stats, "assists"), Num(stats, "deaths"), Num(stats, "mvps"),
                weapon, ammo, reserve, Str(round, "bomb"), Environment.TickCount64)
            {
                Flashed = Num(state, "flashed"),
                Smoked = Num(state, "smoked"),
                Burning = Num(state, "burning"),
                RoundHeadshots = Num(state, "round_killhs"),
                RoundPhase = Str(round, "phase") ?? "",
                WinTeam = Str(round, "win_team"),
                Self = self is null || shown is null || self == shown,
            };
            GameTelemetry.SetCs2(cs2);
            Cs2Events.Update(cs2);
        }
        catch (JsonException)
        {
        }
    }

    private static string? Str(JsonElement e, string name) =>
        e.ValueKind == JsonValueKind.Object && e.TryGetProperty(name, out var v) ? v.ValueKind == JsonValueKind.String ? v.GetString() : v.ToString() : null;

    private static int Num(JsonElement e, string name) => e.ValueKind == JsonValueKind.Object && e.TryGetProperty(name, out var v) ? Int(v) ?? 0 : 0;

    private static int? Int(JsonElement v) => v.ValueKind switch
    {
        JsonValueKind.Number when v.TryGetInt32(out int i) => i,
        JsonValueKind.String when int.TryParse(v.GetString(), out int s) => s,
        _ => null,
    };

    /// <summary>"weapon_ak47" → "AK-47".</summary>
    private static string? Pretty(string? name)
    {
        if (name is null) return null;
        string n = name.Replace("weapon_", "");
        return n switch
        {
            "ak47" => "AK-47",
            "m4a1" => "M4A4",
            "m4a1_silencer" => "M4A1-S",
            "awp" => "AWP",
            "deagle" => "Desert Eagle",
            "usp_silencer" => "USP-S",
            "glock" => "Glock-18",
            "knife" or "knife_t" => "Faca",
            "hegrenade" => "HE",
            "flashbang" => "Flash",
            "smokegrenade" => "Smoke",
            "molotov" or "incgrenade" => "Molotov",
            "c4" => "C4",
            _ => n.ToUpperInvariant(),
        };
    }

    public void Dispose()
    {
        _stop = true;
        _listener.Stop();
        _thread.Join(1000);
    }

    // ---------------------------------------------------------------- instalação no CS2

    /// <summary>A pasta de configurações do CS2 (procurada nas bibliotecas da Steam), ou null.</summary>
    public static string? FindCfgFolder()
    {
        using var steam = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
        if (steam?.GetValue("SteamPath") is not string steamPath) return null;
        var libraries = new List<string> { steamPath };
        string vdf = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
        if (File.Exists(vdf))
        {
            foreach (string line in File.ReadAllLines(vdf))
            {
                string trimmed = line.Trim();
                if (!trimmed.StartsWith("\"path\"", StringComparison.Ordinal)) continue;
                int start = trimmed.IndexOf('"', 6), end = trimmed.LastIndexOf('"');
                if (start >= 0 && end > start) libraries.Add(trimmed[(start + 1)..end].Replace(@"\\", @"\"));
            }
        }
        foreach (string library in libraries.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            string cfg = Path.Combine(library, "steamapps", "common", "Counter-Strike Global Offensive", "game", "csgo", "cfg");
            if (Directory.Exists(cfg)) return cfg;
        }
        return null;
    }

    public static bool IsInstalled(string? folder) => folder is not null && File.Exists(Path.Combine(folder, FileName));

    /// <summary>Grava o arquivo da integração (vale a partir da próxima vez que o CS2 abrir).</summary>
    public static string Install(string folder, string token)
    {
        string path = Path.Combine(folder, FileName);
        File.WriteAllText(path, $$"""
            "DS339 Monitor"
            {
                "uri" "http://127.0.0.1:{{Port}}"
                "timeout" "1.0"
                "buffer" "0.1"
                "throttle" "0.25"
                "heartbeat" "5.0"
                "auth"
                {
                    "token" "{{token}}"
                }
                "data"
                {
                    "provider" "1"
                    "map" "1"
                    "round" "1"
                    "player_id" "1"
                    "player_state" "1"
                    "player_weapons" "1"
                    "player_match_stats" "1"
                }
            }
            """);
        Log.Info($"Modo jogo: integração do CS2 gravada em {path}.");
        return path;
    }

    public static void Uninstall(string folder)
    {
        string path = Path.Combine(folder, FileName);
        if (File.Exists(path)) File.Delete(path);
        Log.Info("Modo jogo: integração do CS2 removida.");
    }
}
