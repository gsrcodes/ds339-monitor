using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using static DS339Monitor.Themes.ThemeKit;

namespace DS339Monitor.Themes;

/// <summary>
/// "Vídeo": um clipe em tela cheia, guardado no formato que o TRCC usa (.zt — os vídeos que ele já converteu servem
/// direto) e que o <see cref="VideoImport"/> gera a partir de MP4/MOV/MKV/AVI. Os dados por cima são os do tema GIF.
/// </summary>
internal static class VideoTheme
{
    public const string Name = "Vídeo";
    public const string Extension = ".zt";

    public static Theme Create(string? path, string style, int width, int height)
    {
        if (!string.IsNullOrEmpty(path) && File.Exists(path))
        {
            try
            {
                return GifTheme.WithOverlay(Name, ZtAnimation.Open(path, width, height), style, width, height);
            }
            catch (Exception ex) when (ex is IOException or InvalidDataException or UnauthorizedAccessException)
            {
                Log.Error($"Não consegui abrir o vídeo {path}", ex);
            }
        }
        return Placeholder(width, height, path);
    }

    private static Theme Placeholder(int width, int height, string? path)
    {
        var f = new FontResolver(null);
        var e = new List<ThemeElement>();
        Literal(e, f, "Escolha um vídeo", new RectangleF(0, height / 2f - 80, width, 76), "Bahnschrift SemiBold", 60, Color.White, TextAlign.Center);
        Literal(e, f, "Ícone do DS339 Monitor  ›  Thermalright  ›  Escolher vídeo…", new RectangleF(0, height / 2f + 10, width, 40), "Bahnschrift", 26,
            Color.FromArgb(200, 255, 255, 255), TextAlign.Center);
        if (!string.IsNullOrEmpty(path))
            Literal(e, f, $"(não consegui abrir {Path.GetFileName(path)})", new RectangleF(0, height / 2f + 56, width, 36), "Bahnschrift", 22, Rgb(0xFCA5A5), TextAlign.Center);
        return new Theme { Name = Name, Width = width, Height = height, BackgroundColor = Rgb(0x0B1220), Elements = e };
    }
}

/// <summary>
/// Clipe .zt (formato do TRCC): byte 0xDC, nº de quadros (int32), o instante em que cada quadro termina em ms (int32,
/// acumulado) e cada quadro como [tamanho (int32)][JPEG]. Os quadros ficam no disco — só o índice vai para a memória —
/// e cada um é decodificado na hora de desenhar. Clipe curto (até <see cref="CacheLimit"/> quadros) pede para a tela
/// guardar cada quadro já codificado: da segunda volta em diante, mandar um quadro quase não custa nada.
/// </summary>
internal sealed class ZtAnimation : IAnimation
{
    private const byte Magic = 0xDC;
    private const int CacheLimit = 400; // ~50 MB de quadros codificados na tela

    private readonly FileStream _file;
    private readonly long[] _offsets;
    private readonly int[] _lengths;
    private readonly int[] _delays;
    private byte[] _buffer = new byte[256 * 1024];

    private ZtAnimation(FileStream file, long[] offsets, int[] lengths, int[] delays, int width, int height)
    {
        _file = file;
        _offsets = offsets;
        _lengths = lengths;
        _delays = delays;
        Width = width;
        Height = height;
    }

    public int Count => _offsets.Length;
    public int Width { get; }
    public int Height { get; }
    public bool CacheFrames => Count <= CacheLimit;

    public static ZtAnimation Open(string path, int width, int height)
    {
        var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        try
        {
            using var reader = new BinaryReader(file, System.Text.Encoding.UTF8, leaveOpen: true);
            if (reader.ReadByte() != Magic) throw new InvalidDataException("não é um vídeo .zt");
            int count = reader.ReadInt32();
            if (count <= 0 || count > 100_000) throw new InvalidDataException($"nº de quadros inválido ({count})");
            var ends = new int[count];
            for (int i = 0; i < count; i++) ends[i] = reader.ReadInt32();
            var offsets = new long[count];
            var lengths = new int[count];
            for (int i = 0; i < count; i++)
            {
                int length = reader.ReadInt32();
                if (length <= 0 || file.Position + length > file.Length) throw new InvalidDataException($"quadro {i} cortado");
                offsets[i] = file.Position;
                lengths[i] = length;
                file.Seek(length, SeekOrigin.Current);
            }
            var delays = new int[count];
            for (int i = 0; i < count; i++) delays[i] = Math.Clamp(ends[i] - (i > 0 ? ends[i - 1] : 0), 15, 2000);
            Log.Info($"Vídeo {Path.GetFileName(path)}: {count} quadros, {ends[^1] / 1000.0:0.0} s por volta");
            return new ZtAnimation(file, offsets, lengths, delays, width, height);
        }
        catch
        {
            file.Dispose();
            throw;
        }
    }

    public int DelayMs(int index) => _delays[index];

    public long FrameKey(int index) => index;

    public void DrawFrame(Bitmap canvas, Graphics g, int index)
    {
        int length = _lengths[index];
        if (_buffer.Length < length) _buffer = new byte[length];
        _file.Seek(_offsets[index], SeekOrigin.Begin);
        _file.ReadExactly(_buffer, 0, length);
        using var stream = new MemoryStream(_buffer, 0, length, writable: false);
        using var image = Image.FromStream(stream, useEmbeddedColorManagement: false, validateImageData: false);
        DrawCover(g, image, Width, Height);
    }

    public void Dispose() => _file.Dispose();

    /// <summary>Preenche <paramref name="width"/>x<paramref name="height"/> cortando o que sobrar, centrado.</summary>
    public static void DrawCover(Graphics g, Image image, int width, int height)
    {
        float scale = Math.Max((float)width / image.Width, (float)height / image.Height);
        float w = width / scale, h = height / scale;
        var source = new RectangleF((image.Width - w) / 2, (image.Height - h) / 2, w, h);
        var mode = g.CompositingMode;
        g.CompositingMode = CompositingMode.SourceCopy;
        g.InterpolationMode = Math.Abs(scale - 1) < 0.01 ? InterpolationMode.NearestNeighbor : InterpolationMode.HighQualityBilinear;
        g.PixelOffsetMode = PixelOffsetMode.Half;
        g.DrawImage(image, new RectangleF(0, 0, width, height), source, GraphicsUnit.Pixel);
        g.CompositingMode = mode;
    }
}

/// <summary>
/// Converte um vídeo (MP4, MOV, MKV, AVI, WMV…) num .zt do tamanho da tela com o Media Foundation do próprio Windows —
/// o mesmo decodificador do app Filmes e TV, com aceleração da placa de vídeo, sem ffmpeg. Até
/// <see cref="MaxFps"/> quadros por segundo e <see cref="MaxSeconds"/> s (é um fundo em loop, não um player).
/// </summary>
internal static unsafe class VideoImport
{
    public const int MaxFps = 24;
    public const double MaxSeconds = 60;
    private const int Quality = 88;

    /// <summary>Devolve a quantidade de quadros gravados. Lança exceção com mensagem legível se não der.</summary>
    public static int Convert(string input, string output, int width, int height, Action<double>? progress = null)
    {
        Check(MFStartup(MfVersion, 0), "o Media Foundation do Windows não iniciou (Windows N sem o Media Feature Pack?)");
        IntPtr attributes = IntPtr.Zero, reader = IntPtr.Zero, wanted = IntPtr.Zero, current = IntPtr.Zero;
        string frames = output + ".quadros";
        try
        {
            Check(MFCreateAttributes(&attributes, 1), "MFCreateAttributes");
            var processing = EnableVideoProcessing;
            Check(SetUInt32(attributes, &processing, 1), "atributos do leitor");
            Check(MFCreateSourceReaderFromURL(Path.GetFullPath(input), attributes, &reader), "não consegui abrir o vídeo (formato sem decodificador no Windows?)");
            Check(SetStreamSelection(reader, AllStreams, 0), "seleção de faixas");
            Check(SetStreamSelection(reader, FirstVideoStream, 1), "o arquivo não tem vídeo");

            Check(MFCreateMediaType(&wanted), "MFCreateMediaType");
            Guid major = MtMajorType, video = MediaTypeVideo, subtype = MtSubtype, rgb32 = VideoFormatRgb32;
            Check(SetGuid(wanted, &major, &video), "tipo de mídia");
            Check(SetGuid(wanted, &subtype, &rgb32), "tipo de mídia");
            Check(SetCurrentMediaType(reader, FirstVideoStream, wanted), "o Windows não converte este vídeo para RGB");
            var (sourceWidth, sourceHeight, stride) = FrameFormat(reader, ref current);

            var codec = ImageCodecInfo.GetImageEncoders().First(e => e.FormatID == ImageFormat.Jpeg.Guid);
            using var parameters = new EncoderParameters(1);
            parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)Quality);
            using var canvas = new Bitmap(width, height, PixelFormat.Format32bppRgb);
            using var g = Graphics.FromImage(canvas);
            using var jpeg = new MemoryStream(256 * 1024);
            var starts = new List<long>(); // início de cada quadro guardado, em unidades de 100 ns
            long step = 10_000_000L / MaxFps, next = 0, first = -1;

            using (var store = new FileStream(frames, FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(store))
            {
                while (true)
                {
                    uint actual, flags;
                    long time;
                    IntPtr sample = IntPtr.Zero;
                    Check(ReadSample(reader, FirstVideoStream, 0, &actual, &flags, &time, &sample), "erro ao decodificar");
                    try
                    {
                        if ((flags & ReaderEndOfStream) != 0) break;
                        if ((flags & ReaderError) != 0) throw new IOException("erro ao decodificar o vídeo");
                        if ((flags & ReaderMediaTypeChanged) != 0) (sourceWidth, sourceHeight, stride) = FrameFormat(reader, ref current);
                        if (sample == IntPtr.Zero) continue;
                        if (first < 0) first = time;
                        time -= first;
                        if (time > MaxSeconds * 10_000_000) break;
                        if (time < next) continue; // mais quadros por segundo que o limite: pula
                        next = Math.Max(next + step, time);

                        DrawSample(sample, g, sourceWidth, sourceHeight, stride, width, height);
                        jpeg.SetLength(0);
                        canvas.Save(jpeg, codec, parameters);
                        writer.Write((int)jpeg.Length);
                        writer.Write(jpeg.GetBuffer(), 0, (int)jpeg.Length);
                        starts.Add(time);
                        progress?.Invoke(Math.Min(1, time / (MaxSeconds * 10_000_000)));
                    }
                    finally
                    {
                        Release(sample);
                    }
                }
            }
            if (starts.Count == 0) throw new IOException("o vídeo não tem nenhum quadro legível");
            // Cada quadro termina quando o próximo começa; o último dura um passo. Em ms, acumulado (formato do TRCC).
            var ends = starts.Skip(1).Append(starts[^1] + step).Select(t => (int)(t / 10_000)).ToList();

            using (var target = new FileStream(output, FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(target))
            {
                writer.Write((byte)0xDC);
                writer.Write(ends.Count);
                foreach (int end in ends) writer.Write(end);
                writer.Flush();
                using var source = new FileStream(frames, FileMode.Open, FileAccess.Read);
                source.CopyTo(target);
            }
            return ends.Count;
        }
        finally
        {
            Release(current);
            Release(wanted);
            Release(reader);
            Release(attributes);
            try { File.Delete(frames); } catch (IOException) { }
            MFShutdown();
        }
    }

    private static (int Width, int Height, int Stride) FrameFormat(IntPtr reader, ref IntPtr current)
    {
        Release(current);
        IntPtr type;
        Check(GetCurrentMediaType(reader, FirstVideoStream, &type), "formato do vídeo");
        current = type;
        Guid sizeKey = MtFrameSize, strideKey = MtDefaultStride;
        ulong size;
        Check(GetUInt64(type, &sizeKey, &size), "tamanho do quadro");
        int width = (int)(size >> 32), height = (int)(size & 0xFFFFFFFF);
        uint strideValue;
        int stride = GetUInt32(type, &strideKey, &strideValue) >= 0 ? (int)strideValue : width * 4;
        return (width, height, stride);
    }

    /// <summary>Copia o quadro decodificado (RGB32) para o canvas, cobrindo a tela inteira.</summary>
    private static void DrawSample(IntPtr sample, Graphics g, int width, int height, int stride, int targetWidth, int targetHeight)
    {
        IntPtr buffer;
        Check(ConvertToContiguousBuffer(sample, &buffer), "buffer do quadro");
        try
        {
            byte* data;
            uint max, length;
            Check(Lock(buffer, &data, &max, &length), "buffer do quadro");
            try
            {
                if (length < (ulong)Math.Abs(stride) * (ulong)height) throw new IOException("quadro menor que o esperado");
                // Passo negativo = imagem de baixo para cima: a primeira linha está no fim do buffer.
                IntPtr scan0 = stride >= 0 ? (IntPtr)data : (IntPtr)(data + (long)(-stride) * (height - 1));
                using var frame = new Bitmap(width, height, stride, PixelFormat.Format32bppRgb, scan0);
                ZtAnimation.DrawCover(g, frame, targetWidth, targetHeight);
            }
            finally
            {
                Unlock(buffer);
            }
        }
        finally
        {
            Release(buffer);
        }
    }

    private static void Check(int hr, string what)
    {
        if (hr < 0) throw new IOException($"{what} (0x{hr:X8})");
    }

    // ---------------------------------------------------------------- Media Foundation (vtable direto, sem wrappers)

    private const uint MfVersion = 0x00020070;
    private const uint FirstVideoStream = 0xFFFFFFFC;
    private const uint AllStreams = 0xFFFFFFFE;
    private const uint ReaderError = 0x1;
    private const uint ReaderEndOfStream = 0x2;
    private const uint ReaderMediaTypeChanged = 0x20;
    private static readonly Guid EnableVideoProcessing = new("fb394f3d-ccf1-42ee-bbb3-f9b845d5681d");
    private static readonly Guid MtMajorType = new("48eba18e-f8c9-4687-bf11-0a74c9f96a8f");
    private static readonly Guid MtSubtype = new("f7e34c9a-42e8-4714-b74b-cb29d72c35e5");
    private static readonly Guid MediaTypeVideo = new("73646976-0000-0010-8000-00AA00389B71");
    private static readonly Guid VideoFormatRgb32 = new("00000016-0000-0010-8000-00AA00389B71");
    private static readonly Guid MtFrameSize = new("1652c33d-d6b2-4012-b834-72030849a37d");
    private static readonly Guid MtDefaultStride = new("644b4e48-1e02-4516-b0eb-c01ca9d49ac6");

    private static void* Slot(IntPtr obj, int index) => (*(void***)obj)[index];

    private static void Release(IntPtr obj)
    {
        if (obj != IntPtr.Zero) ((delegate* unmanaged[Stdcall]<IntPtr, uint>)Slot(obj, 2))(obj);
    }

    // IMFAttributes: GetUINT32 = 7, GetUINT64 = 8, SetUINT32 = 21, SetGUID = 24.
    private static int GetUInt32(IntPtr obj, Guid* key, uint* value) =>
        ((delegate* unmanaged[Stdcall]<IntPtr, Guid*, uint*, int>)Slot(obj, 7))(obj, key, value);

    private static int GetUInt64(IntPtr obj, Guid* key, ulong* value) =>
        ((delegate* unmanaged[Stdcall]<IntPtr, Guid*, ulong*, int>)Slot(obj, 8))(obj, key, value);

    private static int SetUInt32(IntPtr obj, Guid* key, uint value) =>
        ((delegate* unmanaged[Stdcall]<IntPtr, Guid*, uint, int>)Slot(obj, 21))(obj, key, value);

    private static int SetGuid(IntPtr obj, Guid* key, Guid* value) =>
        ((delegate* unmanaged[Stdcall]<IntPtr, Guid*, Guid*, int>)Slot(obj, 24))(obj, key, value);

    // IMFSourceReader: SetStreamSelection = 4, GetCurrentMediaType = 6, SetCurrentMediaType = 7, ReadSample = 9.
    private static int SetStreamSelection(IntPtr reader, uint stream, int selected) =>
        ((delegate* unmanaged[Stdcall]<IntPtr, uint, int, int>)Slot(reader, 4))(reader, stream, selected);

    private static int GetCurrentMediaType(IntPtr reader, uint stream, IntPtr* type) =>
        ((delegate* unmanaged[Stdcall]<IntPtr, uint, IntPtr*, int>)Slot(reader, 6))(reader, stream, type);

    private static int SetCurrentMediaType(IntPtr reader, uint stream, IntPtr type) =>
        ((delegate* unmanaged[Stdcall]<IntPtr, uint, uint*, IntPtr, int>)Slot(reader, 7))(reader, stream, null, type);

    private static int ReadSample(IntPtr reader, uint stream, uint control, uint* actual, uint* flags, long* time, IntPtr* sample) =>
        ((delegate* unmanaged[Stdcall]<IntPtr, uint, uint, uint*, uint*, long*, IntPtr*, int>)Slot(reader, 9))(reader, stream, control, actual, flags, time, sample);

    // IMFSample: ConvertToContiguousBuffer = 41. IMFMediaBuffer: Lock = 3, Unlock = 4.
    private static int ConvertToContiguousBuffer(IntPtr sample, IntPtr* buffer) =>
        ((delegate* unmanaged[Stdcall]<IntPtr, IntPtr*, int>)Slot(sample, 41))(sample, buffer);

    private static int Lock(IntPtr buffer, byte** data, uint* max, uint* length) =>
        ((delegate* unmanaged[Stdcall]<IntPtr, byte**, uint*, uint*, int>)Slot(buffer, 3))(buffer, data, max, length);

    private static int Unlock(IntPtr buffer) =>
        ((delegate* unmanaged[Stdcall]<IntPtr, int>)Slot(buffer, 4))(buffer);

    [DllImport("mfplat.dll")]
    private static extern int MFStartup(uint version, uint flags);

    [DllImport("mfplat.dll")]
    private static extern int MFShutdown();

    [DllImport("mfplat.dll")]
    private static extern int MFCreateAttributes(IntPtr* attributes, uint size);

    [DllImport("mfplat.dll")]
    private static extern int MFCreateMediaType(IntPtr* type);

    [DllImport("mfreadwrite.dll", CharSet = CharSet.Unicode)]
    private static extern int MFCreateSourceReaderFromURL(string url, IntPtr attributes, IntPtr* reader);
}
