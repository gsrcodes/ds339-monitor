using System.Numerics;

namespace DS339Monitor.Render;

/// <summary>
/// Brilho por software, para as duas telas: a DS339 não tem controle de luz de fundo conhecido (o JONSBO-AIO não
/// mexe nisso) e a do Pro Flow 360 ignora o comando de brilho no modo ao vivo. Escurecer os pixels é o mesmo que o
/// Jungle Leopard faz (uma camada preta translúcida por cima da imagem).
/// </summary>
internal static class Brightness
{
    private static readonly Vector<byte> AlphaMask = CreateAlphaMask();

    /// <summary>Multiplica B, G e R por <paramref name="percent"/>% mantendo o alfa. SIMD: 32 bytes por vez com AVX2.</summary>
    public static void Apply(Span<byte> bgra, int percent)
    {
        if (percent >= 100) return;
        int factor = (int)Math.Round(Math.Max(0, percent) * 2.56); // 0..256 (256 = sem mudança)
        var k = new Vector<ushort>((ushort)factor);
        int width = Vector<byte>.Count;
        int i = 0;
        for (; i <= bgra.Length - width; i += width)
        {
            var bytes = new Vector<byte>(bgra.Slice(i));
            Vector.Widen(bytes, out Vector<ushort> low, out Vector<ushort> high);
            low = Vector.ShiftRightLogical(low * k, 8);
            high = Vector.ShiftRightLogical(high * k, 8);
            (Vector.Narrow(low, high) | AlphaMask).CopyTo(bgra.Slice(i));
        }
        for (; i < bgra.Length; i++)
            bgra[i] = (i & 3) == 3 ? (byte)255 : (byte)(bgra[i] * factor >> 8);
    }

    private static Vector<byte> CreateAlphaMask()
    {
        var mask = new byte[Vector<byte>.Count];
        for (int i = 3; i < mask.Length; i += 4) mask[i] = 255; // BGRA: o 4º byte de cada pixel é o alfa
        return new Vector<byte>(mask);
    }
}
