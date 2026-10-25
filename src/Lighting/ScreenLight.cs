using System.Drawing;
using System.Drawing.Imaging;
using DS339Monitor.Themes;

namespace DS339Monitor.Lighting;

/// <summary>
/// Um quadro da Pro Flow visto pelo LED da placa: o brilho médio da imagem (0–1) e, no tema One Piece, o que a
/// apresentação de painéis está mostrando. <see cref="At"/> é o <see cref="App.Clock"/> de quando o quadro foi desenhado.
/// </summary>
internal sealed record ScreenLight(long At, float Luminance, PanelScene? Panel)
{
    /// <summary>Brilho médio do canvas (amostra de 1 pixel a cada 8 nos dois eixos — uns 10 µs).</summary>
    public static unsafe float Measure(Bitmap canvas)
    {
        var data = canvas.LockBits(new Rectangle(0, 0, canvas.Width, canvas.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);
        try
        {
            long sum = 0;
            int count = 0;
            for (int y = 4; y < canvas.Height; y += 8)
            {
                byte* row = (byte*)data.Scan0 + (long)y * data.Stride;
                for (int x = 4; x < canvas.Width; x += 8)
                {
                    byte* p = row + x * 4;
                    sum += p[2] * 77 + p[1] * 150 + p[0] * 29;
                    count++;
                }
            }
            return count == 0 ? 0 : sum / (count * 256f * 255f);
        }
        finally
        {
            canvas.UnlockBits(data);
        }
    }
}
