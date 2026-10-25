using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using DS339Monitor.Display;

namespace DS339Monitor.Render;

/// <summary>Padrão de teste: confirma que a tela recebe imagem e em qual orientação.</summary>
internal static class TestPattern
{
    public const int Width = 960;
    public const int Height = 376;

    public static int Run(int seconds, int rotation)
    {
        Log.Info($"Modo teste: {seconds}s, rotação {rotation}°");
        using var display = new Ds339Display();
        if (!display.Start()) return 1;

        using var canvas = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
        long end = Environment.TickCount64 + seconds * 1000L;
        int sent = 0;
        while (Environment.TickCount64 < end)
        {
            display.Pump();
            if (display.IsReady)
            {
                Draw(canvas, sent, rotation);
                FrameRotator.Blit(canvas, display.FrameBuffer, Ds339Display.PanelWidth, Ds339Display.PanelHeight, rotation);
                if (display.Present())
                {
                    if (sent == 0) Log.Info("Primeiro frame enviado.");
                    sent++;
                }
            }
            Thread.Sleep(250);
        }

        Log.Info($"Teste encerrado. Frames enviados: {sent}");
        return sent > 0 ? 0 : 2;
    }

    private static void Draw(Bitmap canvas, int frame, int rotation)
    {
        using var g = Graphics.FromImage(canvas);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

        using (var bg = new LinearGradientBrush(new Rectangle(0, 0, Width, Height), Color.FromArgb(18, 24, 48), Color.FromArgb(8, 84, 72), 0f))
            g.FillRectangle(bg, 0, 0, Width, Height);

        using (var border = new Pen(Color.White, 4))
            g.DrawRectangle(border, 2, 2, Width - 5, Height - 5);

        // Faixas de cor nas bordas para identificar a orientação mesmo sem ler o texto.
        using (var red = new SolidBrush(Color.FromArgb(230, 60, 60)))
            g.FillRectangle(red, 0, 0, Width, 14);           // topo = vermelho
        using (var blue = new SolidBrush(Color.FromArgb(60, 120, 240)))
            g.FillRectangle(blue, 0, 0, 14, Height);         // esquerda = azul

        using var big = new Font("Segoe UI", 54, FontStyle.Bold, GraphicsUnit.Pixel);
        using var mid = new Font("Segoe UI", 26, FontStyle.Bold, GraphicsUnit.Pixel);
        using var white = new SolidBrush(Color.White);
        using var yellow = new SolidBrush(Color.FromArgb(255, 214, 64));

        var center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        g.DrawString("▲ CIMA (faixa vermelha) ▲", mid, yellow, new RectangleF(0, 18, Width, 40), center);
        g.DrawString("DS339 FUNCIONANDO", big, white, new RectangleF(0, 110, Width, 80), center);
        g.DrawString($"frame {frame}   •   {DateTime.Now:HH:mm:ss}   •   rotação {rotation}°", mid, white, new RectangleF(0, 200, Width, 40), center);
        g.DrawString("◀ ESQUERDA (azul)", mid, yellow, 24, Height - 52);
        var right = new StringFormat { Alignment = StringAlignment.Far };
        g.DrawString("DIREITA ▶", mid, yellow, new RectangleF(0, Height - 52, Width - 20, 40), right);
    }
}
