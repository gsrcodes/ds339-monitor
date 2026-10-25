using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using DS339Monitor.Manga;
using DS339Monitor.Themes;

namespace DS339Monitor.Game;

/// <summary>
/// O painel de jogo da Thermalright (1920×462), no visual de mangá do programa: papel branco, traço preto e sombra dura.
/// Três quadros — FPS (com 1% low e as temperaturas), frametime (gráfico dos últimos quadros, picos marcados, o que
/// limita: processador ou placa) e rede (ping até o servidor do jogo) com, no CS2, o placar e o estado do jogador.
/// </summary>
internal static class GameTheme
{
    public const string Name = "Modo jogo";

    public static Theme Create(int width, int height) => new()
    {
        Name = Name,
        Width = width,
        Height = height,
        Animation = new GameHud(width, height),
        BackgroundColor = Color.White,
        Elements = new List<ThemeElement>(),
    };
}

internal sealed class GameHud : IAnimation
{
    private const int FrameMs = 66; // 15 quadros por segundo: o gráfico anda liso e sobra CPU para o jogo
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    private readonly Font _label, _small, _medium, _big, _huge, _score, _money, _tiny;
    private readonly Pen _ink = new(Color.Black, 5) { LineJoin = LineJoin.Miter };
    private readonly Pen _thin = new(Color.Black, 2);
    private readonly Pen _grid = new(Color.FromArgb(70, 0, 0, 0), 2) { DashStyle = DashStyle.Dash };
    private readonly SolidBrush _black = new(Color.Black);
    private readonly SolidBrush _white = new(Color.White);
    private readonly SolidBrush _muted = new(Color.FromArgb(110, 110, 110));
    private readonly TextureBrush _halftone;

    public GameHud(int width, int height)
    {
        Width = width;
        Height = height;
        var comic = MangaFonts.Comic;
        var text = new FontFamily("Bahnschrift");
        _label = new Font(comic, 40, FontStyle.Regular, GraphicsUnit.Pixel);
        _small = new Font(text, 26, FontStyle.Bold, GraphicsUnit.Pixel);
        _medium = new Font(text, 34, FontStyle.Bold, GraphicsUnit.Pixel);
        _big = new Font(comic, 128, FontStyle.Regular, GraphicsUnit.Pixel);
        _huge = new Font(comic, 250, FontStyle.Regular, GraphicsUnit.Pixel);
        _score = new Font(comic, 76, FontStyle.Regular, GraphicsUnit.Pixel);
        _money = new Font(comic, 64, FontStyle.Regular, GraphicsUnit.Pixel);
        _tiny = new Font(text, 21, FontStyle.Bold, GraphicsUnit.Pixel);
        _halftone = Halftone();
    }

    public int Count => 72_000;
    public int Width { get; }
    public int Height { get; }
    public bool CacheFrames => false;
    public int DelayMs(int index) => FrameMs;
    public long FrameKey(int index) => index;

    public void DrawFrame(Bitmap canvas, Graphics g, int index)
    {
        var s = GameTelemetry.Snapshot();
        g.Clear(Color.White);
        // O renderizador chama em modo de cópia direta (rápido para imagens opacas); texto e traços precisam de mistura.
        g.CompositingMode = CompositingMode.SourceOver;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

        var fps = new RectangleF(18, 18, 540, 418);
        var frames = new RectangleF(578, 18, 764, 418);
        var right = new RectangleF(1362, 18, 534, 418);
        Panel(g, fps);
        Panel(g, frames);
        Panel(g, right);

        DrawFps(g, fps, s);
        DrawFrameTimes(g, frames, s);
        if (s.Cs2 is { } cs2) DrawCs2(g, right, s, cs2);
        else DrawNetwork(g, right, s);
        if (Cs2Events.Moment()?.Banner is { } banner) Banner(g, right, banner);
        if (s.Demo) Tag(g, "PRÉVIA", new PointF(Width - 150, Height - 34));
    }

    // ---------------------------------------------------------------- quadros

    private void DrawFps(Graphics g, RectangleF box, GameSnapshot s)
    {
        g.DrawString("FPS", _label, _black, box.X + 22, box.Y + 14);
        if (s.PresentMode is { Length: > 0 } mode) Right(g, ModeText(mode), _small, _muted, box.Right - 22, box.Y + 26);
        string value = s.Fps is double f ? Math.Round(f).ToString(PtBr) : "—";
        // A sombra de retícula atrás do número (traço de mangá) e o número por cima.
        var size = g.MeasureString(value, _huge);
        float x = box.X + (box.Width - size.Width) / 2, y = box.Y + 56;
        g.TranslateTransform(8, 8);
        g.DrawString(value, _huge, _halftone, x, y);
        g.ResetTransform();
        g.DrawString(value, _huge, _black, x, y);

        string low = s.Low1Fps is double l ? $"1% LOW {Math.Round(l)}" : "1% LOW —";
        string avg = s.FrameTimeMs is double ft ? $"{ft.ToString("0.0", PtBr)} ms" : "— ms";
        g.DrawString(low, _medium, _black, box.X + 24, box.Bottom - 120);
        Right(g, avg, _medium, _black, box.Right - 24, box.Bottom - 120);
        g.DrawLine(_thin, box.X + 22, box.Bottom - 70, box.Right - 22, box.Bottom - 70);
        string cpu = $"CPU {Temp(s.CpuTemp)} {Pct(s.CpuLoad)}";
        string gpu = $"GPU {Temp(s.GpuTemp)} {Pct(s.GpuLoad)}{(s.GpuPower is double w ? $" {Math.Round(w)} W" : "")}";
        g.DrawString(cpu, _small, _black, box.X + 24, box.Bottom - 56);
        Right(g, gpu, _small, _black, box.Right - 24, box.Bottom - 56);
    }

    private void DrawFrameTimes(Graphics g, RectangleF box, GameSnapshot s)
    {
        g.DrawString("FRAMETIME", _label, _black, box.X + 22, box.Y + 14);
        if (s.FrameTimeMs is double now) Right(g, $"{now.ToString("0.0", PtBr)} ms", _medium, _black, box.Right - 22, box.Y + 22);

        var plot = new RectangleF(box.X + 70, box.Y + 84, box.Width - 96, box.Height - 170);
        var data = s.FrameTimes;
        if (data.Length < 2)
        {
            Center(g, "esperando os quadros do jogo…", _small, _muted, plot);
        }
        else
        {
            var sorted = data.OrderBy(v => v).ToArray();
            float median = sorted[sorted.Length / 2], p99 = sorted[Math.Min(sorted.Length - 1, sorted.Length * 99 / 100)];
            float top = Math.Clamp(Math.Max(p99 * 1.5f, median * 2.2f), 6, 60);
            float Y(float ms) => plot.Bottom - Math.Min(1, ms / top) * plot.Height;

            // Linhas de referência: 240, 144 e 60 quadros por segundo (as que cabem na escala).
            float lastLabel = float.MaxValue;
            foreach (var (ms, label) in new[] { (1000f / 240, "240"), (1000f / 144, "144"), (1000f / 60, "60") })
            {
                if (ms > top * 0.95f) continue;
                float y = Y(ms);
                g.DrawLine(_grid, plot.X, y, plot.Right, y);
                if (lastLabel - y < 26) continue; // perto demais do rótulo de baixo
                Right(g, label, _small, _muted, plot.X - 8, y - 15);
                lastLabel = y;
            }

            // Um traço por quadro (os mais novos à direita); os picos (mais que o dobro do normal) em preto cheio.
            int n = Math.Min(data.Length, (int)plot.Width / 2);
            float step = plot.Width / n;
            var points = new PointF[n];
            var spikes = new List<(int Index, float Ms)>();
            for (int i = 0; i < n; i++)
            {
                float ms = data[data.Length - n + i];
                points[i] = new PointF(plot.X + i * step, Y(ms));
                if (ms > median * 2 && ms > 2)
                {
                    g.FillRectangle(_black, plot.X + i * step - 2, Y(ms), 5, plot.Bottom - Y(ms));
                    if (ms > median * 3) spikes.Add((i, ms));
                }
            }
            // "!" só nos 4 maiores picos, separados entre si (senão vira borrão).
            var marked = new List<int>();
            foreach (var (index, ms) in spikes.OrderByDescending(p => p.Ms))
            {
                if (marked.Count >= 4) break;
                if (marked.Any(m => Math.Abs(m - index) * step < 60)) continue;
                marked.Add(index);
                g.DrawString("!", _label, _black, plot.X + index * step - 8, Math.Max(plot.Y - 10, Y(ms) - 44));
            }
            using var area = new GraphicsPath();
            area.AddLines(points);
            area.AddLine(points[^1].X, points[^1].Y, points[^1].X, plot.Bottom);
            area.AddLine(points[^1].X, plot.Bottom, points[0].X, plot.Bottom);
            area.CloseFigure();
            g.FillPath(_halftone, area);
            using var line = new Pen(Color.Black, 3) { LineJoin = LineJoin.Round };
            g.DrawLines(line, points);
            g.DrawLine(_thin, plot.X, plot.Bottom, plot.Right, plot.Bottom);
        }

        // O que limita: a placa ocupada quase o quadro todo é ela; o processador ocupado, ele; os dois com folga, é um
        // teto de FPS ou a sincronia com o monitor (V-Sync).
        string limit = s.GpuBusyPct is not double gpuBusy ? ""
            : gpuBusy >= 90 ? $"LIMITE: PLACA DE VÍDEO ({Math.Round(gpuBusy)}%)"
            : s.WaitPct >= 20 ? $"LIMITE: SINCRONIA COM O MONITOR (PLACA {Math.Round(gpuBusy)}%)"
            : s.Variation < 0.05 ? $"LIMITE: TETO DE FPS (PLACA {Math.Round(gpuBusy)}%)"
            : $"LIMITE: PROCESSADOR (PLACA {Math.Round(gpuBusy)}%)";
        g.DrawString(limit, _small, _black, box.X + 24, box.Bottom - 52);
        if (s.LatencyMs is double latency) Right(g, $"LATÊNCIA {Math.Round(latency)} ms", _small, _black, box.Right - 24, box.Bottom - 52);
    }

    private void DrawNetwork(Graphics g, RectangleF box, GameSnapshot s)
    {
        g.DrawString("PING", _label, _black, box.X + 22, box.Y + 14);
        string ping = s.PingMs is double p ? $"{Math.Round(p)}" : "—";
        var size = g.MeasureString(ping, _big);
        g.DrawString(ping, _big, _black, box.X + 22, box.Y + 70);
        g.DrawString("ms", _label, _black, box.X + 30 + size.Width - 20, box.Y + 152);
        string quality = s.PingMs is null
            ? (s.Server is null ? "procurando o servidor…" : "o servidor não responde ao ping")
            : $"JITTER {Format(s.JitterMs)} ms · PERDA {Math.Round(s.LossPct ?? 0)}%";
        g.DrawString(quality, _small, _black, box.X + 24, box.Y + 230);
        if (s.Server is { } server) g.DrawString(server, _small, _muted, box.X + 24, box.Y + 266);
        g.DrawLine(_thin, box.X + 22, box.Bottom - 120, box.Right - 22, box.Bottom - 120);
        g.DrawString($"↓ {Rate(s.KbpsIn)}", _medium, _black, box.X + 24, box.Bottom - 104);
        Right(g, $"↑ {Rate(s.KbpsOut)}", _medium, _black, box.Right - 24, box.Bottom - 104);
        g.DrawString(GameLabel(s.Game), _small, _muted, box.X + 24, box.Bottom - 52);
    }

    private void DrawCs2(Graphics g, RectangleF box, GameSnapshot s, Cs2State c)
    {
        // Topo: placar, rodada e ping.
        string score = $"CT {c.ScoreCt}  ×  {c.ScoreT} TR";
        g.DrawString(score, _score, _black, box.X + 18, box.Y + 6);
        g.DrawString($"{MapName(c.Map)} · RODADA {Math.Max(1, c.Round + (c.Phase == "live" ? 1 : 0))}", _small, _muted, box.X + 24, box.Y + 92);
        Right(g, s.PingMs is double p ? $"{Math.Round(p)} ms" : "— ms", _medium, _black, box.Right - 22, box.Y + 88);

        // Vida e colete.
        Bar(g, new RectangleF(box.X + 24, box.Y + 146, box.Width - 48, 40), c.Health / 100f, $"VIDA {c.Health}");
        Bar(g, new RectangleF(box.X + 24, box.Y + 198, box.Width - 48, 30), c.Armor / 100f, $"COLETE {c.Armor}{(c.Helmet ? " + CAPACETE" : "")}");

        // Dinheiro, abates e arma.
        g.DrawString($"$ {c.Money.ToString("N0", PtBr)}", _money, _black, box.X + 18, box.Y + 246);
        string kad = $"{c.Kills} / {c.Assists} / {c.Deaths}";
        Right(g, kad, _medium, _black, box.Right - 24, box.Y + 252);
        Right(g, "ABATES / ASSIST. / MORTES", _tiny, _muted, box.Right - 24, box.Y + 292);
        g.DrawLine(_thin, box.X + 22, box.Bottom - 72, box.Right - 22, box.Bottom - 72);
        string weapon = c.Weapon is null ? "" : c.Ammo is int a ? $"{c.Weapon}  {a} / {c.AmmoReserve ?? 0}" : c.Weapon;
        g.DrawString(weapon, _small, _black, box.X + 24, box.Bottom - 56);
        if (c.RoundKills > 0) Right(g, $"{c.RoundKills} NA RODADA", _small, _black, box.Right - 24, box.Bottom - 56);
    }

    // ---------------------------------------------------------------- peças

    private void Panel(Graphics g, RectangleF box)
    {
        g.FillRectangle(_black, box.X + 7, box.Y + 7, box.Width, box.Height); // sombra dura
        g.FillRectangle(_white, box);
        g.DrawRectangle(_ink, box.X, box.Y, box.Width, box.Height);
    }

    private void Bar(Graphics g, RectangleF box, float value, string label)
    {
        g.FillRectangle(_white, box);
        float width = box.Width * Math.Clamp(value, 0, 1);
        g.FillRectangle(value < 0.3f ? _black : _halftone, box.X, box.Y, width, box.Height);
        g.DrawRectangle(_thin, box.X, box.Y, box.Width, box.Height);
        using var font = new Font(_small.FontFamily, box.Height * 0.62f, FontStyle.Bold, GraphicsUnit.Pixel);
        var size = g.MeasureString(label, font);
        float y = box.Y + (box.Height - size.Height) / 2;
        g.FillRectangle(_white, box.X + 6, y + 2, size.Width - 4, size.Height - 4);
        g.DrawString(label, font, _black, box.X + 6, y);
    }

    /// <summary>Aviso da partida em cima do quadro da direita, como uma faixa de mangá (sem contagem nenhuma).</summary>
    private void Banner(Graphics g, RectangleF box, Cs2Banner banner)
    {
        var strip = new RectangleF(box.X - 10, box.Y + 6, box.Width + 20, 92);
        using (var back = new SolidBrush(banner.Back)) g.FillRectangle(back, strip);
        string text = banner.Text;
        var size = g.MeasureString(text, _score);
        float scale = Math.Min(1, (strip.Width - 30) / size.Width);
        var state = g.Save();
        g.TranslateTransform(strip.X + strip.Width / 2, strip.Y + strip.Height / 2);
        g.ScaleTransform(scale, scale);
        g.DrawString(text, _score, _white, -size.Width / 2, -size.Height / 2);
        g.Restore(state);
    }

    private void Tag(Graphics g, string text, PointF at)
    {
        var size = g.MeasureString(text, _small);
        g.FillRectangle(_black, at.X, at.Y, size.Width + 12, size.Height);
        g.DrawString(text, _small, _white, at.X + 6, at.Y);
    }

    private static void Right(Graphics g, string text, Font font, Brush brush, float right, float y)
    {
        var size = g.MeasureString(text, font);
        g.DrawString(text, font, brush, right - size.Width, y);
    }

    private static void Center(Graphics g, string text, Font font, Brush brush, RectangleF box)
    {
        var size = g.MeasureString(text, font);
        g.DrawString(text, font, brush, box.X + (box.Width - size.Width) / 2, box.Y + (box.Height - size.Height) / 2);
    }

    private static TextureBrush Halftone()
    {
        // Retícula de mangá: pontos pretos em diagonal, num ladrilho de 8 px.
        var tile = new Bitmap(8, 8);
        using (var g = Graphics.FromImage(tile))
        {
            g.Clear(Color.White);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.FillEllipse(Brushes.Black, 1, 1, 3.4f, 3.4f);
            g.FillEllipse(Brushes.Black, 5, 5, 3.4f, 3.4f);
        }
        return new TextureBrush(tile);
    }

    private static string Temp(double? v) => v is double t ? $"{Math.Round(t)}°" : "—";
    private static string Pct(double? v) => v is double p ? $"{Math.Round(p)}%" : "";
    private static string Format(double? v) => v is double x ? x.ToString("0.0", PtBr) : "—";
    private static string Rate(double? kbps) => kbps switch
    {
        null => "—",
        >= 1000 => $"{(kbps.Value / 1000).ToString("0.0", PtBr)} Mbit/s",
        _ => $"{Math.Round(kbps.Value)} kbit/s",
    };

    private static string ModeText(string mode) => mode switch
    {
        _ when mode.Contains("Independent Flip", StringComparison.OrdinalIgnoreCase) => "TELA CHEIA (FLIP)",
        _ when mode.Contains("Composed", StringComparison.OrdinalIgnoreCase) => "JANELA",
        _ when mode.Contains("Legacy", StringComparison.OrdinalIgnoreCase) => "TELA CHEIA",
        _ => "",
    };

    private static string MapName(string map) => map.StartsWith("de_", StringComparison.Ordinal) || map.StartsWith("cs_", StringComparison.Ordinal)
        ? char.ToUpperInvariant(map[3]) + map[4..]
        : map;

    private static string GameLabel(string game) => game.Equals("cs2", StringComparison.OrdinalIgnoreCase) ? "COUNTER-STRIKE 2" : game.ToUpperInvariant();

    public void Dispose()
    {
        foreach (var d in new IDisposable[] { _label, _small, _medium, _big, _huge, _score, _money, _tiny, _ink, _thin, _grid, _black, _white, _muted, _halftone })
            d.Dispose();
    }
}
