using System.Drawing;

namespace DS339Monitor.Lighting;

/// <summary>
/// Balanço de branco: cada tipo de LED (o da placa de vídeo, as capas de cabo, os fans) puxa o branco para um lado. A
/// correção é quanto de vermelho, verde e azul sai (0–100%), aplicada na saída de tudo o que a peça mostra — os efeitos
/// continuam pensando em branco puro.
/// </summary>
internal static class WhiteBalance
{
    /// <summary>Três valores 0–100; null quando não há balanço (100% os três).</summary>
    public static int[]? Clean(int[]? gains)
    {
        if (gains is not { Length: 3 }) return null;
        var clean = gains.Select(g => Math.Clamp(g, 0, 100)).ToArray();
        return clean.All(g => g == 100) ? null : clean;
    }

    public static (float R, float G, float B) Factors(int[]? gains) =>
        gains is { Length: 3 } ? (gains[0] / 100f, gains[1] / 100f, gains[2] / 100f) : (1f, 1f, 1f);

    public static Color Apply(Color color, int[]? gains)
    {
        if (gains is not { Length: 3 }) return color;
        return Color.FromArgb(color.A,
            (int)Math.Round(color.R * gains[0] / 100.0),
            (int)Math.Round(color.G * gains[1] / 100.0),
            (int)Math.Round(color.B * gains[2] / 100.0));
    }
}
