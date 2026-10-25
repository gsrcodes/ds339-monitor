using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using static DS339Monitor.Themes.ThemeKit;

namespace DS339Monitor.Themes;

/// <summary>
/// "One Piece": painéis marcantes do mangá, em preto e branco, cobrindo a tela inteira. A câmera percorre cada painel
/// devagar (nada fica de fora mesmo quando ele não tem a proporção da tela), uma caixa de narração entra com o capítulo e
/// a legenda, e entre um painel e outro há uma transição de mangá — ver <see cref="PanelSlideshow"/>.
///
/// Os painéis ficam em themes\One Piece\, descritos em quadros.json (arquivo, capítulo, legenda, de onde baixar e,
/// opcionalmente, como enquadrar). Se faltar algum, a página gerada por <see cref="WriteDownloadPage"/> baixa pelo
/// navegador e os arquivos são importados da pasta Downloads ao carregar o tema.
/// </summary>
internal static class OnePieceTheme
{
    public const string Name = "One Piece";

    private const string CaptionFont = "Bahnschrift SemiBold";
    private static readonly string[] ImageExtensions = { ".png", ".jpg", ".jpeg", ".bmp" };

    public static string Folder => Path.Combine(ThemeCatalog.LocalThemesDirectory, Name);
    public static string DownloadsFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
    private static string ManifestPath => Path.Combine(Folder, "quadros.json");

    public static Theme Create(string style, int width, int height)
    {
        var manifest = Manifest.Load(ManifestPath);
        Import(manifest);
        var panels = FindPanels(manifest);
        if (panels.Count == 0) return Placeholder(width, height);

        var timer = Stopwatch.StartNew();
        var slideshow = BuildSlideshow(panels, manifest, width, height);
        Log.Info($"One Piece: {slideshow.SlideCount} painéis, {slideshow.Count} quadros ({slideshow.Count * PanelSlideshow.FrameMs / 60000.0:0.0} min por volta), " +
            $"{slideshow.MemoryBytes / 1048576.0:0.0} MB em memória, preparado em {timer.ElapsedMilliseconds} ms");
        return GifTheme.WithOverlay(Name, slideshow, style, width, height);
    }

    /// <summary>Já existe algum painel (na pasta do tema ou para importar dos Downloads)?</summary>
    public static bool HasPanels()
    {
        var manifest = Manifest.Load(ManifestPath);
        Import(manifest);
        return FindPanels(manifest).Count > 0;
    }

    /// <summary>Gera a página que o navegador usa para baixar os painéis e devolve o caminho dela.</summary>
    public static string WriteDownloadPage()
    {
        var manifest = Manifest.Load(ManifestPath);
        Directory.CreateDirectory(Folder);
        string path = Path.Combine(Folder, "BAIXAR-QUADROS.html");
        File.WriteAllText(path, DownloadPage.Render(manifest), new UTF8Encoding(false));
        return path;
    }

    // ---------------------------------------------------------------- painéis

    private sealed record Panel(string Path, string? Chapter, string? Caption, PanelSlideshow.Framing Framing, PanelCue Cue = PanelCue.None);

    private static List<Panel> FindPanels(Manifest manifest)
    {
        var panels = new List<Panel>();
        if (!Directory.Exists(Folder)) return panels;
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in manifest.Entries)
        {
            string? file = FindImage(Folder, entry.File, exactName: true);
            if (file is null) continue;
            used.Add(file);
            panels.Add(new Panel(file, entry.Chapter, entry.Caption, entry.Framing, entry.Cue));
        }

        // Imagens extras que o usuário pôs na pasta entram no fim, com o nome do arquivo de legenda.
        foreach (var file in Directory.EnumerateFiles(Folder).Where(IsImage).OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
        {
            if (used.Add(file)) panels.Add(new Panel(file, null, Path.GetFileNameWithoutExtension(file), new PanelSlideshow.Framing()));
        }
        return panels;
    }

    /// <summary>Copia para a pasta do tema os painéis que o navegador baixou ("op-painel-01.png", "op-painel-01 (1).png"...).</summary>
    private static void Import(Manifest manifest)
    {
        try
        {
            Directory.CreateDirectory(Folder);
            if (!Directory.Exists(DownloadsFolder)) return;
            foreach (var entry in manifest.Entries)
            {
                if (FindImage(Folder, entry.File, exactName: true) is not null) continue;
                string? downloaded = FindImage(DownloadsFolder, entry.File, exactName: false);
                if (downloaded is null) continue;
                string target = Path.Combine(Folder, entry.File + Path.GetExtension(downloaded).ToLowerInvariant());
                File.Copy(downloaded, target);
                Log.Info($"One Piece: {Path.GetFileName(downloaded)} importado dos Downloads.");
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Log.Warn($"One Piece: não consegui importar dos Downloads: {ex.Message}");
        }
    }

    private static string? FindImage(string folder, string baseName, bool exactName)
    {
        if (!Directory.Exists(folder)) return null;
        return Directory.EnumerateFiles(folder, baseName + "*")
            .Where(IsImage)
            .Where(f =>
            {
                string name = Path.GetFileNameWithoutExtension(f);
                return exactName
                    ? name.Equals(baseName, StringComparison.OrdinalIgnoreCase)
                    : name.Equals(baseName, StringComparison.OrdinalIgnoreCase) || name.StartsWith(baseName + " (", StringComparison.OrdinalIgnoreCase);
            })
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();
    }

    private static bool IsImage(string path) => ImageExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);

    // ---------------------------------------------------------------- montagem

    /// <summary>Prepara os painéis em paralelo (abrir PNGs grandes e redimensionar é o que pesa) e desenha as legendas.</summary>
    private static PanelSlideshow BuildSlideshow(List<Panel> panels, Manifest manifest, int width, int height)
    {
        var slides = new PanelSlideshow.Slide?[panels.Count];
        var options = new ParallelOptions { MaxDegreeOfParallelism = Math.Clamp(Environment.ProcessorCount / 2, 1, 4) };
        Parallel.For(0, panels.Count, options, i =>
        {
            try
            {
                using var source = new Bitmap(panels[i].Path);
                slides[i] = PanelSlideshow.Prepare(source, panels[i].Framing, width, height, Math.Max(3, manifest.SecondsPerPanel));
            }
            catch (Exception ex) when (ex is ArgumentException or OutOfMemoryException or IOException or ExternalException)
            {
                // ArgumentException/OutOfMemory = formato que o GDI+ não lê (ex.: WebP salvo pelo navegador).
                Log.Warn($"One Piece: não consegui abrir {Path.GetFileName(panels[i].Path)} ({ex.Message}). Salve como PNG ou JPG.");
            }
        });

        var ready = new List<PanelSlideshow.Slide>();
        var fonts = new FontResolver(null);
        using var chapterFont = fonts.Create(CaptionFont, 14);
        using var captionFont = fonts.Create(CaptionFont, 22);
        for (int i = 0; i < panels.Count; i++)
        {
            if (slides[i] is not { } slide) continue;
            if (manifest.Captions) slide.Caption = RenderCaption(panels[i], chapterFont, captionFont);
            slide.Cue = panels[i].Cue;
            ready.Add(slide);
        }
        if (ready.Count == 0) throw new InvalidOperationException("nenhum painel pôde ser aberto");
        return new PanelSlideshow(ready, width, height);
    }

    /// <summary>Caixa de narração de mangá: papel branco, traço preto, sombra dura; capítulo em cima e a legenda.</summary>
    private static Bitmap? RenderCaption(Panel panel, Font chapterFont, Font captionFont)
    {
        if (string.IsNullOrWhiteSpace(panel.Caption)) return null;
        const int Padding = 12, MaxText = 380, Shadow = 5;
        string chapter = panel.Chapter is null ? "" : $"CAP. {panel.Chapter}";
        using var format = new StringFormat(StringFormat.GenericTypographic) { Trimming = StringTrimming.Word };

        SizeF captionSize, chapterSize;
        using (var probe = new Bitmap(1, 1))
        using (var measure = Graphics.FromImage(probe))
        {
            measure.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            captionSize = measure.MeasureString(panel.Caption, captionFont, MaxText, format);
            chapterSize = chapter.Length > 0 ? measure.MeasureString(chapter, chapterFont, int.MaxValue, format) : SizeF.Empty;
        }
        int boxWidth = (int)Math.Ceiling(Math.Max(captionSize.Width, chapterSize.Width)) + Padding * 2 + 2;
        int boxHeight = (int)Math.Ceiling(captionSize.Height + (chapter.Length > 0 ? chapterSize.Height + 4 : 0)) + Padding * 2;

        var bitmap = new Bitmap(boxWidth + Shadow + 2, boxHeight + Shadow + 2, PixelFormat.Format32bppPArgb);
        using var g = Graphics.FromImage(bitmap);
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
        var box = new Rectangle(1, 1, boxWidth, boxHeight);
        using (var shadow = new SolidBrush(Color.FromArgb(150, 0, 0, 0)))
            g.FillRectangle(shadow, box.X + Shadow, box.Y + Shadow, box.Width, box.Height);
        g.FillRectangle(Brushes.White, box);
        using (var ink = new Pen(Color.Black, 2.5f))
            g.DrawRectangle(ink, box.X + 1, box.Y + 1, box.Width - 2, box.Height - 2);

        float y = box.Y + Padding;
        if (chapter.Length > 0)
        {
            using var muted = new SolidBrush(Color.FromArgb(0x5A, 0x5A, 0x5A));
            g.DrawString(chapter, chapterFont, muted, box.X + Padding, y, format);
            y += chapterSize.Height + 4;
        }
        g.DrawString(panel.Caption, captionFont, Brushes.Black, new RectangleF(box.X + Padding, y, MaxText, captionSize.Height + 4), format);
        return bitmap;
    }

    /// <summary>Tela de instruções enquanto os painéis não foram baixados.</summary>
    private static Theme Placeholder(int width, int height)
    {
        var f = new FontResolver(null);
        var e = new List<ThemeElement>();
        var dim = Color.FromArgb(200, 255, 255, 255);
        Literal(e, f, "ONE PIECE", new RectangleF(0, height / 2f - 118, width, 70), "Bahnschrift SemiBold", 58, Color.White, TextAlign.Center);
        Literal(e, f, "Os painéis do mangá ainda não foram baixados.", new RectangleF(0, height / 2f - 26, width, 40), "Bahnschrift", 26, dim, TextAlign.Center);
        Literal(e, f, "Ícone do DS339 Monitor  ›  Pro Flow 360  ›  Baixar painéis do One Piece…", new RectangleF(0, height / 2f + 22, width, 36), "Bahnschrift", 22, dim, TextAlign.Center);
        Literal(e, f, "Depois, escolha o tema One Piece de novo.", new RectangleF(0, height / 2f + 60, width, 36), "Bahnschrift", 22, dim, TextAlign.Center);
        return new Theme
        {
            Name = Name,
            Width = width,
            Height = height,
            BackgroundColor = Color.FromArgb(14, 14, 16),
            Elements = e,
        };
    }

    // ---------------------------------------------------------------- manifesto

    private sealed record Entry(string File, string? Chapter, string? Caption, string? Image, string? Wiki, PanelSlideshow.Framing Framing, PanelCue Cue);

    /// <summary>
    /// quadros.json: <c>segundosPorQuadro</c>, <c>legendas</c> (true/false), <c>creditos</c> e a lista <c>quadros</c>.
    /// Cada quadro: <c>arquivo</c>, <c>capitulo</c>, <c>legenda</c>, <c>imagem</c> (de onde baixar), <c>wiki</c> e,
    /// opcionais, <c>movimento</c> (auto, descer, subir, esquerda, direita, parado), <c>de</c>/<c>ate</c> (trecho do
    /// caminho da câmera, 0 a 1; no "parado", <c>de</c> é onde o enquadramento fica), <c>recorte</c>
    /// ([esquerda, cima, direita, baixo], frações a cortar de cada lado) e <c>luz</c> (momento que o LED da placa encena
    /// no efeito "Mangá narrado": impacto, tambores, tremor, mare, haki, despedida, amanhecer).
    /// </summary>
    private sealed record Manifest(int SecondsPerPanel, bool Captions, string? Credits, List<Entry> Entries)
    {
        private const int DefaultSeconds = 9;

        public static Manifest Load(string path)
        {
            try
            {
                if (!System.IO.File.Exists(path)) return Empty();
                using var doc = JsonDocument.Parse(System.IO.File.ReadAllText(path), new JsonDocumentOptions { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip });
                var root = doc.RootElement;
                int seconds = root.TryGetProperty("segundosPorQuadro", out var s) && s.TryGetInt32(out int v) ? v : DefaultSeconds;
                bool captions = !root.TryGetProperty("legendas", out var l) || l.ValueKind != JsonValueKind.False;
                string? credits = root.TryGetProperty("creditos", out var c) ? c.GetString() : null;
                var entries = new List<Entry>();
                if (root.TryGetProperty("quadros", out var list))
                {
                    foreach (var item in list.EnumerateArray())
                    {
                        string? file = Text(item, "arquivo");
                        if (string.IsNullOrWhiteSpace(file)) continue;
                        entries.Add(new Entry(file, Text(item, "capitulo"), Text(item, "legenda"), Text(item, "imagem"), Text(item, "wiki"), ReadFraming(item), ReadCue(item)));
                    }
                }
                return new Manifest(seconds, captions, credits, entries);
            }
            catch (Exception ex) when (ex is IOException or JsonException or InvalidOperationException or FormatException)
            {
                Log.Error("One Piece: quadros.json inválido", ex);
                return Empty();
            }
        }

        private static Manifest Empty() => new(DefaultSeconds, true, null, new List<Entry>());

        private static PanelSlideshow.Framing ReadFraming(JsonElement item)
        {
            var motion = Text(item, "movimento")?.Trim().ToLowerInvariant() switch
            {
                "descer" => PanelMotion.Down,
                "subir" => PanelMotion.Up,
                "esquerda" => PanelMotion.Left,
                "direita" => PanelMotion.Right,
                "parado" => PanelMotion.Still,
                _ => PanelMotion.Auto,
            };
            double start = item.TryGetProperty("de", out var d) && d.TryGetDouble(out double dv) ? dv : 0;
            double end = item.TryGetProperty("ate", out var a) && a.TryGetDouble(out double av) ? av : 1;
            var trim = RectangleF.Empty;
            if (item.TryGetProperty("recorte", out var r) && r.ValueKind == JsonValueKind.Array && r.GetArrayLength() == 4)
                trim = new RectangleF(r[0].GetSingle(), r[1].GetSingle(), r[2].GetSingle(), r[3].GetSingle());
            return new PanelSlideshow.Framing(motion, start, end, trim);
        }

        private static PanelCue ReadCue(JsonElement item) => Text(item, "luz")?.Trim().ToLowerInvariant() switch
        {
            "impacto" => PanelCue.Impact,
            "tambores" => PanelCue.Drums,
            "tremor" => PanelCue.Quake,
            "mare" or "maré" => PanelCue.Tide,
            "haki" => PanelCue.Haki,
            "despedida" => PanelCue.Farewell,
            "amanhecer" => PanelCue.Dawn,
            _ => PanelCue.None,
        };

        private static string? Text(JsonElement item, string name) =>
            item.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    }

    // ---------------------------------------------------------------- página de download

    private static class DownloadPage
    {
        public static string Render(Manifest manifest)
        {
            var items = manifest.Entries.Where(e => e.Image is not null).Select(e => new
            {
                arquivo = e.File,
                capitulo = e.Chapter,
                legenda = e.Caption,
                imagem = e.Image + (e.Image!.Contains('?') ? "&" : "?") + "format=original",
                wiki = e.Wiki,
            });
            string data = JsonSerializer.Serialize(items).Replace("</", "<\\/");
            string credits = System.Net.WebUtility.HtmlEncode(manifest.Credits ?? "");
            return Template.Replace("__DATA__", data).Replace("__CREDITOS__", credits).Replace("__PASTA__",
                System.Net.WebUtility.HtmlEncode(DownloadsFolder));
        }

        private const string Template = """
<!doctype html>
<html lang="pt-BR">
<head>
<meta charset="utf-8">
<title>Painéis do One Piece — Pro Flow 360</title>
<style>
  :root { color-scheme: dark; --bg:#111113; --card:#1b1b1f; --line:#2c2c33; --ink:#f4f4f5; --muted:#a1a1aa; --ok:#4ade80; --bad:#f87171; }
  * { box-sizing: border-box; }
  body { margin:0; padding:32px clamp(16px,4vw,48px); background:var(--bg); color:var(--ink); font:15px/1.5 "Segoe UI", system-ui, sans-serif; }
  h1 { font:700 28px/1.2 "Bahnschrift", "Segoe UI", sans-serif; letter-spacing:.02em; margin:0 0 6px; }
  p { margin:0 0 10px; color:var(--muted); max-width:70ch; }
  .bar { display:flex; flex-wrap:wrap; gap:12px; align-items:center; margin:20px 0 24px; }
  button { font:600 16px "Segoe UI", sans-serif; padding:12px 22px; border-radius:10px; border:0; background:var(--ink); color:#111; cursor:pointer; }
  button:disabled { opacity:.5; cursor:default; }
  #status { color:var(--muted); }
  .grid { display:grid; grid-template-columns:repeat(auto-fill,minmax(240px,1fr)); gap:16px; }
  .card { background:var(--card); border:1px solid var(--line); border-radius:12px; overflow:hidden; display:flex; flex-direction:column; }
  .card img { width:100%; aspect-ratio:4/3; object-fit:cover; background:#000; filter:grayscale(1); }
  .info { padding:12px 14px 14px; display:flex; flex-direction:column; gap:4px; }
  .cap { font:600 12px "Bahnschrift", sans-serif; color:var(--muted); letter-spacing:.06em; }
  .legenda { font-weight:600; }
  .arquivo { font:12px Consolas, monospace; color:var(--muted); }
  .estado { font-size:13px; min-height:1.5em; }
  .ok .estado { color:var(--ok); } .erro .estado { color:var(--bad); }
  .ok { border-color:#1f5134; } .erro { border-color:#5b2020; }
  footer { margin-top:28px; font-size:12px; color:var(--muted); }
</style>
</head>
<body>
<h1>Painéis do One Piece para o Pro Flow 360</h1>
<p>O site da wiki só entrega as imagens para um navegador, então é o seu navegador que baixa. Clique no botão: os arquivos
vão para <b>__PASTA__</b> (se o navegador perguntar se pode baixar vários arquivos, permita).</p>
<p>Depois, no ícone do DS339 Monitor: <b>Pro Flow 360 › Tema › One Piece</b>. Os painéis são importados sozinhos.</p>
<div class="bar"><button id="todos">Baixar todos os painéis</button><span id="status"></span></div>
<div class="grid" id="grid"></div>
<footer>__CREDITOS__</footer>
<script>
const quadros = __DATA__;
const grid = document.getElementById('grid');
const cards = quadros.map(q => {
  const card = document.createElement('div');
  card.className = 'card';
  card.innerHTML = `<img referrerpolicy="no-referrer" loading="lazy" alt="">
    <div class="info"><span class="cap"></span><span class="legenda"></span><span class="arquivo"></span><span class="estado"></span></div>`;
  card.querySelector('img').src = q.imagem;
  card.querySelector('.cap').textContent = q.capitulo ? 'CAPÍTULO ' + q.capitulo : '';
  card.querySelector('.legenda').textContent = q.legenda || '';
  card.querySelector('.arquivo').textContent = q.arquivo + '.png';
  grid.appendChild(card);
  return card;
});

async function baixar(q, card) {
  const estado = card.querySelector('.estado');
  try {
    const resposta = await fetch(q.imagem, { mode: 'cors', referrerPolicy: 'no-referrer' });
    if (!resposta.ok) throw new Error('HTTP ' + resposta.status);
    const blob = await resposta.blob();
    const ext = blob.type.includes('jpeg') ? 'jpg' : blob.type.includes('webp') ? 'webp' : 'png';
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = q.arquivo + '.' + ext;
    document.body.appendChild(link);
    link.click();
    link.remove();
    setTimeout(() => URL.revokeObjectURL(link.href), 10000);
    card.classList.remove('erro'); card.classList.add('ok');
    estado.textContent = ext === 'webp' ? 'Baixado como WebP — salve de novo como PNG (botão direito na imagem)' : 'Baixado ✓';
    return true;
  } catch (e) {
    card.classList.add('erro');
    estado.textContent = 'Não deu pelo botão: clique com o direito na imagem › Salvar imagem como… › ' + q.arquivo + '.png';
    return false;
  }
}

document.getElementById('todos').addEventListener('click', async ev => {
  ev.target.disabled = true;
  let ok = 0;
  for (let i = 0; i < quadros.length; i++) {
    document.getElementById('status').textContent = `Baixando ${i + 1} de ${quadros.length}…`;
    if (await baixar(quadros[i], cards[i])) ok++;
    await new Promise(r => setTimeout(r, 350));
  }
  document.getElementById('status').textContent = `${ok} de ${quadros.length} baixados.`;
  ev.target.disabled = false;
});
</script>
</body>
</html>
""";
    }
}
