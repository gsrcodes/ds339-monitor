namespace DS339Monitor.Argb;

/// <summary>
/// O arquivo da animação de boot como ele vai para a controladora: a animação sozinha, ou emendada em alguns minutos do
/// loop dos fans. A "chave" resume tudo o que muda o arquivo — igual à gravada, não precisa gravar de novo.
/// </summary>
internal static class NollieBootFile
{
    public static string Key(ArgbSettings argb, string name, int minutes, bool grb)
    {
        string text = $"{name}|{minutes}|{grb}|{argb.Efeito}|{argb.Cor}|{argb.Brilho}|{argb.Velocidade}|{argb.LimiteAmperes}|" +
            string.Join(";", argb.Zonas.Select(z => $"{z.Canal}.{z.Inicio}.{z.Leds}.{z.Efeito}.{z.Cor}.{z.Brilho}.{z.Inverter}.{z.Forma}" +
                (z.Balanco is { } wb ? $".{wb[0]}.{wb[1]}.{wb[2]}" : "")));
        var hash = System.Security.Cryptography.SHA1.HashData(System.Text.Encoding.UTF8.GetBytes(text));
        return $"{name}|{Convert.ToHexString(hash, 0, 6)}";
    }

    public static string Key(ArgbSettings argb, NollieStoredSettings draft) =>
        Key(argb, draft.BootAnimacao ?? "", draft.ContinuarMinutos, draft.CoresGrb);

    /// <summary>Tamanho aproximado (para avisar antes de gravar).</summary>
    public static int EstimateBytes(ArgbSettings argb, string name, int minutes)
    {
        int leds = argb.Zonas.GroupBy(z => z.Canal).Sum(g => g.Max(z => z.Inicio + z.Leds));
        int frames = minutes > 0
            ? (int)Math.Round(BootAnimation.HandOff(name) * 1000 / BootAnimation.LongFrameMs) + minutes * 60_000 / BootAnimation.LongFrameMs
            : 220;
        return frames * leds * 3;
    }

    public static (byte[] File, int[] Map, FrameSource Source) Build(ArgbSettings argb, string name, int minutes, bool grb)
    {
        var source = minutes > 0
            ? BootAnimation.Combined(name, argb, minutes * 60)
            : BootAnimation.Alone(BootAnimation.Render(name, argb.Zonas, argb));
        var (file, map) = NollieFseq.Build(source, argb.Zonas, grb);
        return (file, map, source);
    }
}
