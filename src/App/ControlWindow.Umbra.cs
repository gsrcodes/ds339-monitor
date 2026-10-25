using DS339Monitor.Argb;

namespace DS339Monitor.App;

/// <summary>A aba "Modo Umbra" do painel (ver <see cref="UmbraSettings"/>).</summary>
internal sealed partial class ControlWindow
{
    private object UmbraState()
    {
        var argb = _config.FansEFitas;
        var u = argb.Umbra;
        var ports = UmbraLighting.Describe(argb.Zonas).ToList();
        var port = u.SelectedPort;
        int fans = ports.FirstOrDefault(p => p.Name == u.Porta).Fans;
        var slot = u.CurrentSlot;
        int effect = slot is null ? 0 : u.Modo == UmbraModes.Merge ? UmbraEffects.MergeIds[slot.Efeito] : slot.Efeito;
        var rule = UmbraEffects.Fan[effect];
        int colors = rule.Colors;
        // Como no software: a cor estática do centro é uma por fan.
        if (u.Modo == UmbraModes.Async && effect == 0 && fans > 0 && port?.Zona == UmbraZones.Center) colors = Math.Clamp(fans, 1, 4);
        return new
        {
            on = u.Ligado,
            controlling = argb.Ligado,
            lights = u.LuzesAcesas,
            mode = u.Modo,
            modes = UmbraModes.All,
            effects = UmbraEffects.Fan.Select(e => new { id = e.Id, name = e.Nome, original = e.Original, desc = e.Descricao }),
            merge = UmbraEffects.MergeIds.Select((id, index) => new
            {
                id = index,
                name = UmbraEffects.Fan[id].Nome,
                original = UmbraEffects.Fan[id].Original,
                desc = UmbraEffects.Fan[id].Descricao,
            }),
            slot = slot is null ? null : new
            {
                effect = slot.Efeito,
                speed = 60 - slot.Velocidade * 10,
                brightness = slot.Brilho <= 50 ? 0 : slot.Brilho / 51 * 10,
                direction = slot.Direcao,
                colors = slot.Cores.Take(colors),
            },
            rules = new { speed = rule.Speed, direction = rule.Direction, colors },
            ports = ports.Select(p => new { name = p.Name, fans = p.Fans, leds = p.Leds }),
            port = port is null ? null : new { name = port.Grupo, zone = port.Zona, layout = port.Arranjo, fans },
            zones = UmbraZones.Names,
            together = u.FansJuntos,
            fanPorts = ports.Count(p => p.Fans > 0),
            presets = u.Predefinicoes.Select((p, i) => new { index = i, name = p.Nome, desc = p.Descricao }),
            maxPresets = UmbraSettings.MaxPresets,
            canvas = UmbraCanvasState(u.Tela),
            previewKey = $"{u.Modo}|{(slot is null ? "" : System.Text.Json.JsonSerializer.Serialize(slot))}",
        };
    }

    /// <summary>Comandos da aba "Modo Umbra". False = comando desconhecido.</summary>
    private bool HandleUmbra(string cmd, Func<string, string> S, Func<string, int> I, Func<string, bool> B)
    {
        if (!cmd.StartsWith("umbra", StringComparison.Ordinal)) return false;
        var argb = _config.FansEFitas;
        var u = argb.Umbra;
        var slot = u.CurrentSlot;
        switch (cmd)
        {
            case "umbraOn":
                u.Ligado = B("on");
                if (u.Ligado && !argb.Ligado) Toast("O Modo Umbra vale com os fans e fitas no programa (página Fans e fitas › Controlar pelo programa).", true);
                break;
            case "umbraLights":
                u.LuzesAcesas = B("on");
                break;
            case "umbraMode" when UmbraModes.All.Contains(S("name")):
                u.Modo = S("name");
                break;
            case "umbraEffect" when slot is not null:
                slot.Efeito = Math.Clamp(I("id"), 0, (u.Modo == UmbraModes.Merge ? UmbraEffects.MergeIds.Length : UmbraEffects.Fan.Length) - 1);
                break;
            case "umbraSpeed" when slot is not null:
                slot.Velocidade = Math.Clamp((60 - Snap(I("value"))) / 10, 1, 6);
                break;
            case "umbraBrightness" when slot is not null:
            {
                int v = Snap(I("value"));
                slot.Brilho = v == 0 ? 50 : v / 10 * 51;
                break;
            }
            case "umbraDirection" when slot is not null:
                slot.Direcao = I("dir") == 0 ? 0 : 1;
                break;
            case "umbraColor" when slot is not null:
            {
                int index = I("index");
                if (index is < 0 or > 3) return false;
                slot.Cores[index] = Lighting.GpuLighting.ToHex(Lighting.GpuLighting.ParseColor(S("hex")));
                break;
            }
            case "umbraPort" when u.Portas.Any(p => p.Grupo == S("name")):
                u.Porta = S("name");
                break;
            case "umbraZone" when u.SelectedPort is { } port && UmbraZones.Names.Contains(S("name")):
                port.Zona = S("name");
                port.Arranjo = UmbraZones.LayoutFor(port.Zona) ?? port.Arranjo;
                break;
            case "umbraTogether":
                u.FansJuntos = B("on");
                break;
            case "umbraPresetSave":
            {
                string name = S("text").Trim();
                if (name.Length == 0) return false;
                if (u.Predefinicoes.Count >= UmbraSettings.MaxPresets)
                {
                    Toast($"Já são {UmbraSettings.MaxPresets} predefinições; apague uma antes.", true);
                    return true;
                }
                u.Predefinicoes.Add(new UmbraPreset
                {
                    Nome = name.Length > 30 ? name[..30] : name,
                    Descricao = DescribePorts(u),
                    Portas = u.Portas.Select(p => p.Clone()).ToList(),
                });
                Toast($"Predefinição \"{name}\" salva.", false);
                break;
            }
            case "umbraPresetApply":
            {
                int index = I("index");
                if (index < 0 || index >= u.Predefinicoes.Count) return false;
                foreach (var saved in u.Predefinicoes[index].Portas)
                {
                    int at = u.Portas.FindIndex(p => p.Grupo == saved.Grupo);
                    if (at >= 0) u.Portas[at] = saved.Clone();
                }
                break;
            }
            case "umbraPresetDelete":
            {
                int index = I("index");
                if (index < 0 || index >= u.Predefinicoes.Count) return false;
                u.Predefinicoes.RemoveAt(index);
                break;
            }
            default:
                if (!HandleUmbraCanvas(cmd, u.Tela, S, I)) return false;
                break;
        }

        // "Fans juntos": o que se faz num fan vale para todos os fans.
        if (u.FansJuntos && u.Modo == UmbraModes.Async && u.SelectedPort is { } chosen)
        {
            var fanPorts = UmbraLighting.Describe(argb.Zonas).Where(p => p.Fans > 0).Select(p => p.Name).ToHashSet();
            if (fanPorts.Contains(chosen.Grupo))
                foreach (var other in u.Portas.Where(p => p != chosen && fanPorts.Contains(p.Grupo)))
                {
                    other.Arranjo = chosen.Arranjo;
                    other.Zonas = chosen.Clone().Zonas;
                }
        }
        argb.Normalize();
        _config.Save();
        _service.Argb.Reload();
        return true;
    }

    private static object UmbraCanvasState(UmbraCanvasSettings tela) => new
    {
        effect = tela.Efeito,
        brightness = tela.Brilho,
        speed = tela.SpeedOf(tela.Efeito),
        maxSpeed = UmbraCanvasSettings.MaxSpeed(tela.Efeito),
        colors = tela.ColorsOf(tela.Efeito) ?? UmbraCanvas.DefaultColors(tela.Efeito) ?? Array.Empty<string>(),
        changedColors = tela.ColorsOf(tela.Efeito) is not null,
        effects = CanvasEffectList.Value,
    };

    /// <summary>Os efeitos do modo tela com o nome original (calculado uma vez).</summary>
    private static readonly Lazy<object[]> CanvasEffectList = new(() => UmbraCanvasSettings.Effects.Select(e =>
    {
        var created = Argb.Umbra.UmbraEffectCatalog.Create(e.Key, 1);
        return (object)new { key = e.Key, name = e.Nome, original = created is null ? "" : $"{created.NameZh} · {created.NameEn}" };
    }).ToArray());

    /// <summary>Comandos do modo tela. False = desconhecido.</summary>
    private static bool HandleUmbraCanvas(string cmd, UmbraCanvasSettings tela, Func<string, string> S, Func<string, int> I)
    {
        switch (cmd)
        {
            case "umbraCanvasEffect" when UmbraCanvasSettings.Effects.Any(e => e.Key == S("key")):
                tela.Efeito = S("key");
                return true;
            case "umbraCanvasBrightness":
                tela.Brilho = Math.Clamp(I("value"), 1, 10);
                return true;
            case "umbraCanvasSpeed":
                tela.Velocidades[tela.Efeito] = Math.Clamp(I("value"), 1, UmbraCanvasSettings.MaxSpeed(tela.Efeito));
                return true;
            case "umbraCanvasColor":
            {
                var colors = (tela.ColorsOf(tela.Efeito) ?? UmbraCanvas.DefaultColors(tela.Efeito))?.ToArray();
                int index = I("index");
                if (colors is null || index < 0 || index >= colors.Length) return false;
                colors[index] = Lighting.GpuLighting.ToHex(Lighting.GpuLighting.ParseColor(S("hex")));
                tela.Cores[tela.Efeito] = colors;
                return true;
            }
            case "umbraCanvasColorsReset":
                tela.Cores.Remove(tela.Efeito);
                return true;
            default:
                return false;
        }
    }

    /// <summary>Os deslizantes do software andam de 10 em 10 (0–50).</summary>
    private static int Snap(int value) => Math.Clamp((int)Math.Round(value / 10.0) * 10, 0, 50);

    /// <summary>Descrição curta de uma predefinição: o efeito de cada porta.</summary>
    private static string DescribePorts(UmbraSettings u) => string.Join(" · ", u.Portas.Select(p =>
        $"{p.Grupo}: {(p.Arranjo == UmbraZones.LayoutAll ? UmbraEffects.Fan[p.Slot(UmbraZones.All).Efeito].Nome : $"centro {UmbraEffects.Fan[p.Slot(UmbraZones.Center).Efeito].Nome}")}"));

    /// <summary>As prévias dos efeitos do Modo Umbra (um fan de mentira por efeito, com os parâmetros do que está sendo editado).</summary>
    private void AddUmbraPreviews(Dictionary<string, string[]> previews)
    {
        var u = _config.FansEFitas.Umbra;
        var slot = (u.CurrentSlot ?? u.Sincronizado).Clone();
        bool merge = u.Modo == UmbraModes.Merge;
        foreach (var effect in UmbraEffects.Fan)
        {
            if (merge && !UmbraEffects.MergeIds.Contains(effect.Id)) continue;
            string key = merge ? $"umbra:m{Array.IndexOf(UmbraEffects.MergeIds, effect.Id)}" : $"umbra:{effect.Id}";
            previews[key] = UmbraLighting.Preview(effect.Id, slot, merge, 150, 40);
        }
    }
}
