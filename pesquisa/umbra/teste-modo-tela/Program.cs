using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using DS339Monitor.Argb.Umbra;

static class Program
{
    static void Main(string[] args)
    {
        int frames = args.Length > 0 ? int.Parse(args[0]) : 240;
        string outPng = args.Length > 1 ? args[1] : "sheet.png";
        const int S = 600, T = 150, Cols = 4;
        var keys = UmbraEffectCatalog.PortedKeys;
        int rows = (keys.Length + Cols - 1) / Cols;
        var sheet = new byte[rows * T * Cols * T * 3];
        var canvas = new float[S, S, 3];
        for (int k = 0; k < keys.Length; k++)
        {
            var fx = UmbraEffectCatalog.Create(keys[k], 1234)!;
            var sw = Stopwatch.StartNew();
            double t = 0;
            for (int f = 0; f < frames; f++) { fx.Render(canvas, t, 0.016); t += 0.016; }
            sw.Stop();
            double sum = 0; int nonBlack = 0;
            for (int y = 0; y < S; y++) for (int x = 0; x < S; x++)
            { float v = canvas[y, x, 0] + canvas[y, x, 1] + canvas[y, x, 2]; sum += v; if (v > 0.02f) nonBlack++; }
            Console.WriteLine($"{keys[k],-18} {fx.NameEn,-22} {sw.Elapsed.TotalMilliseconds / frames,7:F2} ms/frame  mean={sum / (S * S * 3):F3} lit={100.0 * nonBlack / (S * S):F1}%");
            int ox = (k % Cols) * T, oy = (k / Cols) * T;
            for (int y = 0; y < T; y++) for (int x = 0; x < T; x++)
                for (int c = 0; c < 3; c++)
                    sheet[((oy + y) * Cols * T + ox + x) * 3 + c] = (byte)Math.Clamp((int)(canvas[y * 4, x * 4, c] * 255f + 0.5f), 0, 255);
        }
        WritePng(outPng, Cols * T, rows * T, sheet);
    }

    static void WritePng(string path, int w, int h, byte[] rgb)
    {
        using var fs = File.Create(path);
        fs.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        var ihdr = new byte[13];
        WriteBE(ihdr, 0, w); WriteBE(ihdr, 4, h); ihdr[8] = 8; ihdr[9] = 2;
        Chunk(fs, "IHDR", ihdr);
        using var ms = new MemoryStream();
        using (var z = new ZLibStream(ms, CompressionLevel.Optimal, true))
            for (int y = 0; y < h; y++) { z.WriteByte(0); z.Write(rgb, y * w * 3, w * 3); }
        Chunk(fs, "IDAT", ms.ToArray());
        Chunk(fs, "IEND", Array.Empty<byte>());
    }

    static void WriteBE(byte[] b, int o, int v) { b[o] = (byte)(v >> 24); b[o + 1] = (byte)(v >> 16); b[o + 2] = (byte)(v >> 8); b[o + 3] = (byte)v; }

    static void Chunk(Stream s, string type, byte[] data)
    {
        var len = new byte[4]; WriteBE(len, 0, data.Length); s.Write(len);
        var td = new byte[4 + data.Length];
        for (int i = 0; i < 4; i++) td[i] = (byte)type[i];
        Buffer.BlockCopy(data, 0, td, 4, data.Length);
        s.Write(td);
        var crc = new byte[4]; WriteBE(crc, 0, (int)Crc(td)); s.Write(crc);
    }

    static uint Crc(byte[] d)
    {
        uint c = 0xFFFFFFFF;
        foreach (byte b in d) { c ^= b; for (int k = 0; k < 8; k++) c = (c & 1) != 0 ? 0xEDB88320 ^ (c >> 1) : c >> 1; }
        return c ^ 0xFFFFFFFF;
    }
}
