using System.Drawing;
using System.Drawing.Imaging;

namespace DS339Monitor.Render;

/// <summary>Copia um bitmap BGRA para o buffer do painel aplicando rotação de 0/90/180/270 graus (sentido horário).</summary>
internal static unsafe class FrameRotator
{
    public static void Blit(Bitmap source, IntPtr panel, int panelWidth, int panelHeight, int rotation)
    {
        int w = source.Width;
        int h = source.Height;
        bool sideways = rotation is 90 or 270;
        if ((sideways ? h : w) != panelWidth || (sideways ? w : h) != panelHeight)
            throw new ArgumentException($"Imagem {w}x{h} com rotação {rotation} não cabe no painel {panelWidth}x{panelHeight}.");

        // O painel ignora o alfa e os frames são opacos, então PArgb e Argb têm os mesmos bytes:
        // trava no formato nativo do bitmap para o GDI+ não converter à toa.
        var format = source.PixelFormat == PixelFormat.Format32bppPArgb ? PixelFormat.Format32bppPArgb : PixelFormat.Format32bppArgb;
        var data = source.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, format);
        try
        {
            uint* src = (uint*)data.Scan0;
            int srcStride = data.Stride / 4;
            uint* dst = (uint*)panel;

            switch (rotation)
            {
                case 0:
                    for (int y = 0; y < h; y++)
                        new ReadOnlySpan<uint>(src + y * srcStride, w).CopyTo(new Span<uint>(dst + y * panelWidth, w));
                    break;

                case 90: // (x, y) -> (h-1-y, x)
                    for (int y = 0; y < h; y++)
                    {
                        uint* row = src + y * srcStride;
                        uint* col = dst + (h - 1 - y);
                        for (int x = 0; x < w; x++)
                            col[x * panelWidth] = row[x];
                    }
                    break;

                case 180: // (x, y) -> (w-1-x, h-1-y)
                    for (int y = 0; y < h; y++)
                    {
                        uint* row = src + y * srcStride;
                        uint* outRow = dst + (h - 1 - y) * panelWidth;
                        for (int x = 0; x < w; x++)
                            outRow[w - 1 - x] = row[x];
                    }
                    break;

                case 270: // (x, y) -> (y, w-1-x)
                    for (int y = 0; y < h; y++)
                    {
                        uint* row = src + y * srcStride;
                        uint* col = dst + y;
                        for (int x = 0; x < w; x++)
                            col[(w - 1 - x) * panelWidth] = row[x];
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(rotation), rotation, "Use 0, 90, 180 ou 270.");
            }
        }
        finally
        {
            source.UnlockBits(data);
        }
    }
}
