using System.Text;
using System.Text.Json;
using Microsoft.Win32;

namespace DS339Monitor.Argb;

/// <summary>
/// Lê do registro do SignalRGB como a Nollie está montada: em cada canal ("Channel 01".."Channel 16" da chave
/// HKCU\Software\WhirlwindFX\SignalRgb\devices\16d5:2a16:&lt;série&gt;), a lista de componentes que o usuário pôs ali, na
/// ordem dos LEDs — cada componente vira uma parte com o mesmo trecho de LEDs. Só lê; nada é mudado no SignalRGB.
/// </summary>
internal static class SignalRgbImport
{
    private const string DevicesKey = @"Software\WhirlwindFX\SignalRgb\devices";

    public static List<ArgbZone> Read()
    {
        var zones = new List<ArgbZone>();
        using var devices = Registry.CurrentUser.OpenSubKey(DevicesKey);
        if (devices is null) return zones;
        string? name = devices.GetSubKeyNames().FirstOrDefault(n => n.StartsWith("16d5:2a16:", StringComparison.OrdinalIgnoreCase))
            ?? devices.GetSubKeyNames().FirstOrDefault(n => n.StartsWith("16d5:4716:", StringComparison.OrdinalIgnoreCase));
        if (name is null) return zones;
        using var device = devices.OpenSubKey(name);
        if (device is null) return zones;

        var channels = new List<(int Channel, List<(string Name, int Leds)> Parts)>();
        for (int channel = 1; channel <= NollieDevice.Channels; channel++)
        {
            string? json = Text(device.GetValue($"Channel {channel:D2}"));
            if (json is null) continue;
            var parts = new List<(string, int)>();
            try
            {
                using var document = JsonDocument.Parse(json);
                if (document.RootElement.ValueKind != JsonValueKind.Array) continue;
                foreach (var component in document.RootElement.EnumerateArray())
                {
                    int leds = component.TryGetProperty("LedCount", out var count) && count.TryGetInt32(out int n) ? n : 0;
                    string display = component.TryGetProperty("DisplayName", out var d) ? d.GetString() ?? "" : "";
                    if (leds > 0) parts.Add((display, leds));
                }
            }
            catch (JsonException ex)
            {
                Log.Warn($"Fans e fitas: canal {channel} do SignalRGB ilegível ({ex.Message}).");
                continue;
            }
            if (parts.Count > 0) channels.Add((channel, parts));
        }

        // Nomes de partida, pelo formato de cada canal; o usuário confere com "Identificar" e renomeia.
        var used = new Dictionary<string, int>();
        foreach (var (channel, parts) in channels)
        {
            string group;
            string[] names;
            bool fan120 = parts.Count == 3 && parts.Any(p => p.Name.Contains("MEIO", StringComparison.OrdinalIgnoreCase));
            bool fan360 = parts.Count == 3 && !fan120 && parts[1].Leds == parts[2].Leds;
            if (fan120 || fan360)
            {
                group = fan120 ? "Fan 120" : "Fans 360";
                names = new[] { "Centro", "Lateral 1", "Lateral 2" };
            }
            else if (parts.Count == 1)
            {
                group = parts[0].Leds switch
                {
                    9 => "Memória RAM",
                    110 => "Capa de cabo",
                    5 => "Bomba do water cooler",
                    50 => "Placa sob a placa-mãe",
                    _ => Clean(parts[0].Name, channel),
                };
                names = new[] { "Tudo" };
            }
            else
            {
                group = $"Canal {channel}";
                names = parts.Select((p, i) => Clean(p.Name, channel)).ToArray();
            }
            used[group] = used.GetValueOrDefault(group) + 1;

            int start = 0;
            for (int i = 0; i < parts.Count; i++)
            {
                zones.Add(new ArgbZone
                {
                    Grupo = group + "#" + used[group],
                    Nome = names[i],
                    Canal = channel,
                    Inicio = start,
                    Leds = parts[i].Leds,
                    Brilho = group == "Bomba do water cooler" ? 200 : 100, // a luz da bomba é bem mais fraca
                });
                start += parts[i].Leds;
            }
        }

        // Numera só os grupos que se repetem ("Fans 360 · 1" e "Fans 360 · 2"; a bomba fica só "Bomba do water cooler").
        foreach (var zone in zones)
        {
            int mark = zone.Grupo.LastIndexOf('#');
            string baseName = zone.Grupo[..mark];
            zone.Grupo = used[baseName] > 1 ? $"{baseName} · {zone.Grupo[(mark + 1)..]}" : baseName;
        }
        Log.Info($"Fans e fitas: {zones.Count} partes importadas do SignalRGB ({zones.Sum(z => z.Leds)} LEDs em {channels.Count} canais).");
        return zones;
    }

    private static string Clean(string name, int channel)
    {
        name = name.Replace("Componente Genérico", "Componente").Trim(' ', '-');
        return name.Length == 0 ? $"Canal {channel}" : name;
    }

    /// <summary>O valor pode vir como texto ou como QVariant em UTF-16 (bytes).</summary>
    private static string? Text(object? value) => value switch
    {
        string s => s,
        byte[] bytes => Encoding.Unicode.GetString(bytes) is var text && text.IndexOf('[') is int i and >= 0 ? text[i..].TrimEnd('\0') : null,
        _ => null,
    };
}
