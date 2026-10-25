namespace DS339Monitor.Argb;

/// <summary>Ferramentas de linha de comando da controladora Nollie.</summary>
internal static class NollieTools
{
    /// <summary>
    /// <c>--nollie-ler</c>: lê a configuração guardada na controladora (só perguntas, nada é gravado), registra no log e
    /// guarda uma cópia em nollie-config-AAAAMMDD-HHMMSS.json, ao lado do programa.
    /// </summary>
    public static int ReadConfig()
    {
        using var device = NollieDevice.TryOpen(out string reason);
        if (device is null)
        {
            Log.Warn($"Nollie: {reason}.");
            return 1;
        }
        var config = NollieConfig.Read(device, out string error);
        if (config is null)
        {
            Log.Warn($"Nollie: não consegui ler a configuração — {error}.");
            return 1;
        }
        string path = Path.Combine(AppContext.BaseDirectory, $"nollie-config-{DateTime.Now:yyyyMMdd-HHmmss}.json");
        File.WriteAllText(path, config.ToJson());
        Log.Info($"Nollie: configuração guardada nela — {config.Describe()}\n  cópia em {path}");
        return 0;
    }

    /// <summary>
    /// <c>--nollie-fseq-teste [animação]</c>: sem tocar na controladora — confere o CRC com os valores conhecidos, monta o
    /// arquivo FSEQ da animação, confere como o firmware confere e grava uma cópia em nollie-boot.fseq.
    /// </summary>
    public static int FseqSelfTest(string[] args)
    {
        (string Input, uint Expected)[] vectors = { ("", 0xFFFFFFFF), ("\0\0\0\0", 0xC704DD7B), ("123456789", 0xBF99399C) };
        foreach (var (input, expected) in vectors)
        {
            uint crc = NollieFseq.Crc(System.Text.Encoding.Latin1.GetBytes(input));
            Log.Info($"FSEQ: CRC de \"{input.Replace("\0", "\\0")}\" = {crc:X8} ({(crc == expected ? "certo" : $"ERRADO, esperado {expected:X8}")})");
            if (crc != expected) return 1;
        }
        var config = App.AppConfig.Load(Path.Combine(AppContext.BaseDirectory, "config.json"));
        // "--nollie-fseq-teste Curto-circuito 3" = a animação emendada em 3 min do loop dos fans.
        int minutes = args.Length > 2 && int.TryParse(args[^1], out int m) ? m : 0;
        string name = string.Join(' ', args.Skip(1).Take(args.Length - 1 - (minutes > 0 ? 1 : 0)));
        if (name.Length == 0) name = BootAnimation.ShortCircuit;
        var started = System.Diagnostics.Stopwatch.StartNew();
        var animation = minutes > 0
            ? BootAnimation.Combined(name, config.FansEFitas, minutes * 60)
            : BootAnimation.Alone(BootAnimation.Render(name, config.FansEFitas.Zonas, config.FansEFitas));
        var (file, map) = NollieFseq.Build(animation, config.FansEFitas.Zonas, grb: true);
        long buildMs = started.ElapsedMilliseconds;
        string? problem = NollieFseq.Validate(file, map);
        string path = Path.Combine(AppContext.BaseDirectory, "nollie-boot.fseq");
        File.WriteAllBytes(path, file);
        Log.Info($"FSEQ: {animation.Name} — {animation.Count} quadros de {animation.FrameMs} ms, {file.Length} bytes (montado em {buildMs} ms), CRC {NollieFseq.Crc(file):X8}, " +
            $"LEDs por canal físico {string.Join(",", map)} — {(problem is null ? "válido" : "INVÁLIDO: " + problem)}; cópia em {path}");
        return problem is null ? 0 : 1;
    }

    /// <summary>
    /// <c>--nollie-conferir-gravacao</c>: confere o caminho de gravação sem mudar nada — regrava no canvas de boot os mesmos
    /// 16 bytes que já estão nele e confere lendo tudo de volta (custa uma gravação na flash dela).
    /// </summary>
    public static int CheckWrite()
    {
        using var device = NollieDevice.TryOpen(out string reason);
        if (device is null)
        {
            Log.Warn($"Nollie: {reason}.");
            return 1;
        }
        var before = NollieConfig.Read(device, out string error);
        if (before?.Slot(NollieConfig.BootBase + 1) is { } spare && before.BootCount < 2)
        {
            // O slot de boot 15 está desligado (só 1 canvas de boot): muda um byte nele, confere e devolve.
            byte[] original = spare.ToBytes();
            byte[] probe = (byte[])original.Clone();
            probe[1] = (byte)(original[1] == 42 ? 43 : 42); // brilho
            device.Command(new byte[] { 0xFA, 0x02, NollieConfig.BootBase + 1 }.Concat(probe).ToArray());
            Thread.Sleep(400);
            var changed = NollieConfig.Read(device, out error);
            bool took = changed?.Slot(NollieConfig.BootBase + 1)?.ToBytes().SequenceEqual(probe) == true;
            device.Command(new byte[] { 0xFA, 0x02, NollieConfig.BootBase + 1 }.Concat(original).ToArray());
            Thread.Sleep(400);
            var restored = NollieConfig.Read(device, out error);
            bool back = restored?.Slot(NollieConfig.BootBase + 1)?.ToBytes().SequenceEqual(original) == true;
            Log.Info($"Nollie: slot 15 (desligado) — mudança {(took ? "aceita" : "NÃO aceita")}, restaurado {(back ? "sim" : "NÃO")}; " +
                $"gravações na flash {before.WriteCount} → {changed?.WriteCount} → {restored?.WriteCount}.");
        }
        if (before?.Slot(NollieConfig.BootBase) is not { } boot)
        {
            Log.Warn($"Nollie: não consegui ler — {error}.");
            return 1;
        }
        device.Command(new byte[] { 0xFA, 0x02, NollieConfig.BootBase }.Concat(boot.ToBytes()).ToArray());
        Thread.Sleep(400);
        var after = NollieConfig.Read(device, out error);
        if (after is null)
        {
            Log.Warn($"Nollie: gravei, mas não consegui ler de volta — {error}.");
            return 1;
        }
        bool same = after.Slots.Length == before.Slots.Length &&
            after.Slots.Zip(before.Slots).All(p => p.First.ToBytes().SequenceEqual(p.Second.ToBytes())) &&
            after.BootMap.SequenceEqual(before.BootMap) && after.LedCount.SequenceEqual(before.LedCount);
        Log.Info($"Nollie: conferência da gravação — mesmos bytes regravados no boot; leitura de volta {(same ? "igual" : "DIFERENTE")}; " +
            $"gravações na flash {before.WriteCount} → {after.WriteCount}.");
        return same ? 0 : 1;
    }
}
