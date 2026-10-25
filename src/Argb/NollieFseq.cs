namespace DS339Monitor.Argb;

/// <summary>
/// Animação de boot gravada na memória da controladora (o recurso "FSEQ" do NollieRGB): um arquivo FSEQ v2 sem
/// compressão (o formato do xLights) na flash externa dela, a partir de 0x10000; com o boot em "FSEQ uma vez", ela toca o
/// arquivo sozinha ao ligar o PC e depois passa aos efeitos normais. Protocolo lido do código do NollieRGB
/// (fseq_file.py, fseq_upload_service.py, n_usb_flash_mixin.py):
/// <list type="bullet">
/// <item>arquivo: cabeçalho de 32 bytes ("PSEQ", versão 2.0), uma faixa esparsa por canal com LEDs (em ordem crescente de
/// canal físico) e os quadros — em cada quadro, os LEDs de cada canal ativo, 3 bytes por LED;</item>
/// <item>FA 10 + 16 × u16 = LEDs de cada canal físico (tem que bater com as faixas do arquivo);</item>
/// <item>F4 05 = apagar um bloco de 64 KiB; F4 06 = gravar até 1016 bytes; F4 03 = situação (0 pronto, 1 ocupado);
/// F4 07 = CRC calculado pela controladora; F4 01 = ler; F3 02/03/04 = tocar / parar / situação; F9 11 = arquivo
/// presente.</item>
/// </list>
/// Nada abaixo de 0x10000 é tocado (a lista fechada em <see cref="NollieDevice"/> não deixa). O cabeçalho é gravado por
/// último: interrompida no meio, a controladora vê "sem animação" em vez de tocar quadros pela metade.
/// </summary>
internal static class NollieFseq
{
    public const int Base = 0x10000;
    private const int Block = 0x10000;
    private const int Chunk = 1016;
    private const int Capacity = 16 * 1024 * 1024;

    /// <summary>
    /// Monta o arquivo: um canal por canal físico com LEDs (as partes mostram onde cada LED está no canal). Devolve também
    /// os LEDs de cada canal físico (o que vai no FA 10).
    /// </summary>
    public static (byte[] File, int[] LedMap) Build(BootFrames animation, IReadOnlyList<ArgbZone> zones, bool grb) =>
        Build(BootAnimation.Alone(animation), zones, grb);

    public static (byte[] File, int[] LedMap) Build(FrameSource animation, IReadOnlyList<ArgbZone> zones, bool grb)
    {
        int[] hardware = NollieDevice.HardwareChannels;
        var ledMap = new int[16];
        // De cada LED de cada canal físico: (parte, LED na parte) — ou nada (LED sem parte: apagado).
        var sources = new (int Zone, int Led)[16][];
        for (int z = 0; z < zones.Count; z++)
        {
            int hw = hardware[zones[z].Canal - 1];
            ledMap[hw] = Math.Max(ledMap[hw], zones[z].Inicio + zones[z].Leds);
        }
        for (int hw = 0; hw < 16; hw++)
        {
            sources[hw] = Enumerable.Repeat((-1, -1), ledMap[hw]).ToArray();
        }
        for (int z = 0; z < zones.Count; z++)
        {
            int hw = hardware[zones[z].Canal - 1];
            for (int i = 0; i < zones[z].Leds; i++) sources[hw][zones[z].Inicio + i] = (z, i);
        }

        int[] active = Enumerable.Range(0, 16).Where(hw => ledMap[hw] > 0).ToArray();
        int frameBytes = active.Sum(hw => ledMap[hw] * 3);
        int dataOffset = 32 + 6 * active.Length;
        var balance = zones.Select(z => Lighting.WhiteBalance.Factors(z.Balanco)).ToArray(); // balanço de branco de cada parte
        int frames = animation.Count;
        if (Base + dataOffset + (long)frames * frameBytes > Capacity) throw new InvalidOperationException("animação maior que a memória da controladora");
        var file = new byte[dataOffset + frames * frameBytes];

        // Cabeçalho FSEQ v2 (32 bytes, little-endian).
        "PSEQ"u8.CopyTo(file);
        WriteU16(file, 4, dataOffset);
        file[6] = 0;                    // versão menor
        file[7] = 2;                    // versão maior
        WriteU16(file, 8, dataOffset);  // tamanho do cabeçalho
        WriteU32(file, 10, frameBytes); // canais (bytes) por quadro
        WriteU32(file, 14, frames);
        file[18] = (byte)animation.FrameMs;
        file[20] = 0;                   // sem compressão
        file[22] = (byte)active.Length; // faixas esparsas
        // Identificador (bytes 24–31) zerado: o mesmo desenho dá sempre o mesmo arquivo (e o mesmo CRC).
        int start = 0;
        for (int r = 0; r < active.Length; r++)
        {
            int count = ledMap[active[r]] * 3;
            WriteU24(file, 32 + r * 6, start);
            WriteU24(file, 35 + r * 6, count);
            start += count;
        }

        for (int f = 0; f < frames; f++)
        {
            int at = dataOffset + f * frameBytes;
            var frame = animation.At(f);
            foreach (int hw in active)
            {
                foreach (var (zone, led) in sources[hw])
                {
                    byte r = 0, g = 0, b = 0;
                    if (zone >= 0)
                    {
                        var data = frame[zone];
                        var (gr, gg, gb) = balance[zone];
                        r = ToByte(data[led * 3] * gr);
                        g = ToByte(data[led * 3 + 1] * gg);
                        b = ToByte(data[led * 3 + 2] * gb);
                    }
                    file[at++] = grb ? g : r;
                    file[at++] = grb ? r : g;
                    file[at++] = b;
                }
            }
        }
        return (file, ledMap);
    }

    /// <summary>
    /// Confere o arquivo como o NollieRGB confere antes de mandar (o mesmo que o firmware confere): "PSEQ", v2, sem
    /// compressão, quadros, faixas iguais aos LEDs de cada canal ativo. Devolve o problema, ou null.
    /// </summary>
    public static string? Validate(byte[] file, int[] ledMap)
    {
        if (file.Length < 32 || file[0] != 'P' || file[1] != 'S' || file[2] != 'E' || file[3] != 'Q') return "não é FSEQ";
        if (file[7] != 2) return "não é FSEQ v2";
        if ((file[20] & 0x0F) != 0) return "comprimido";
        int frames = BitConverter.ToInt32(file, 14), step = file[18], ranges = file[22];
        if (frames == 0 || step == 0) return "sem quadros";
        int[] active = Enumerable.Range(0, 16).Where(hw => ledMap[hw] > 0).ToArray();
        if (ranges != active.Length) return "faixas diferentes dos canais";
        int frameBytes = 0;
        for (int r = 0; r < ranges; r++)
        {
            int count = file[35 + r * 6] | file[36 + r * 6] << 8 | file[37 + r * 6] << 16;
            if (count != ledMap[active[r]] * 3) return $"faixa {r} com {count} bytes, canal com {ledMap[active[r]]} LEDs";
            frameBytes += count;
        }
        int dataOffset = file[4] | file[5] << 8;
        if (file.Length < dataOffset + (long)frames * frameBytes) return "arquivo curto";
        if (Base + file.Length > Capacity) return "grande demais";
        return null;
    }

    /// <summary>CRC da controladora (nos_crc32): CRC-32/MPEG-2 com cada palavra de 4 bytes lida do byte mais alto.</summary>
    public static uint Crc(ReadOnlySpan<byte> data)
    {
        static uint Step(uint c, byte b)
        {
            c ^= (uint)b << 24;
            for (int j = 0; j < 8; j++) c = (c & 0x80000000) != 0 ? (c << 1) ^ 0x04C11DB7u : c << 1;
            return c;
        }
        uint crc = 0xFFFFFFFF;
        int whole = data.Length & ~3;
        for (int i = 0; i < whole; i += 4)
            for (int k = 3; k >= 0; k--) crc = Step(crc, data[i + k]);
        for (int i = whole; i < data.Length; i++) crc = Step(crc, data[i]);
        return crc;
    }

    // ---------------------------------------------------------------- gravação

    /// <summary>
    /// Grava o arquivo na controladora e confere pelo CRC dela (ou lendo de volta). Não muda o que toca ao ligar — isso é
    /// o FA 13, à parte. Devolve "ok" ou "erro: …".
    /// </summary>
    public static string Upload(NollieDevice device, byte[] file, int[] ledMap, Action<string>? progress = null)
    {
        if (Validate(file, ledMap) is { } problem) return "erro: arquivo inválido — " + problem;
        var info = device.Query(0xF4, 0x04, 0, 0, 0, 0, 0, 0);
        if (info is null || info[0] != 0xF4 || info[7] == 5) return "erro: a controladora não tem memória de animação";
        long capacity = info[10] is >= 20 and <= 30 ? 1L << info[10] : Capacity;
        if (Base + file.Length > capacity) return "erro: animação maior que a memória dela";

        device.Command(0xF3, 0x03); // para o que estiver tocando
        Thread.Sleep(20);
        if (device.Query(0xF9, 0x10) is not { } current || !ReadWords(current).SequenceEqual(ledMap))
        {
            var map = new byte[34];
            map[0] = 0xFA;
            map[1] = 0x10;
            for (int i = 0; i < 16; i++)
            {
                map[2 + i * 2] = (byte)(ledMap[i] & 0xFF);
                map[3 + i * 2] = (byte)(ledMap[i] >> 8);
            }
            device.Command(map);
            Thread.Sleep(40);
        }

        int blocks = (file.Length + Block - 1) / Block;
        for (int k = 0; k < blocks; k++)
        {
            if (k % 8 == 0) progress?.Invoke($"apagando a memória dela: {k * 100 / blocks}%");
            int address = Base + k * Block;
            if (Submit(device, Header(0x05, address, 1)) != 1) return $"erro: a controladora recusou apagar o bloco {k}";
            if (!WaitReady(device, TimeSpan.FromSeconds(2), 0)) return $"erro: o bloco {k} não terminou de apagar";
        }

        // Os pedaços 1…n primeiro e o 0 (com o cabeçalho) por último.
        int chunks = (file.Length + Chunk - 1) / Chunk;
        int done = 0;
        foreach (int c in Enumerable.Range(1, chunks - 1).Append(0))
        {
            if (done++ % 100 == 0) progress?.Invoke($"gravando a animação: {done * 100 / chunks}%");
            int offset = c * Chunk, length = Math.Min(Chunk, file.Length - offset);
            var packet = new byte[8 + length];
            Header(0x06, Base + offset, length).CopyTo(packet, 0);
            Buffer.BlockCopy(file, offset, packet, 8, length);
            if (Submit(device, packet) != 1) return $"erro: a controladora recusou gravar em {Base + offset:X6}";
            if (!WaitReady(device, TimeSpan.FromSeconds(5), 1)) return $"erro: a gravação em {Base + offset:X6} não terminou";
        }

        progress?.Invoke("conferindo pelo CRC");
        string check = VerifyByCrc(device, file);
        if (check != "ok")
        {
            Log.Warn($"Nollie: conferência pelo CRC — {check}; conferindo pela leitura de volta.");
            check = VerifyByReading(device, file);
            if (check != "ok") return "erro: " + check;
        }

        for (int i = 0; i < 12; i++)
        {
            if (device.Query(0xF9, 0x11) is { } present && present[0] != 0 && BitConverter.ToUInt32(present, 1) > 0) return "ok";
            Thread.Sleep(80);
        }
        return "erro: gravada e conferida, mas a controladora não reconheceu a animação";
    }

    /// <summary>A controladora calcula o CRC do que tem gravado e devolve; tem que bater com o do arquivo.</summary>
    private static string VerifyByCrc(NollieDevice device, byte[] file)
    {
        int length = file.Length;
        if (Submit(device, new byte[] { 0xF4, 0x07, 0x00, 0x00, 0x01, (byte)length, (byte)(length >> 8), (byte)(length >> 16) }) != 1)
            return "a controladora não calcula CRC";
        var timeout = TimeSpan.FromSeconds(Math.Max(15, 3.0 * length / (1 << 20)));
        if (!WaitReady(device, timeout, 100)) return "o CRC não ficou pronto";
        var result = device.Query(0xF4, 0x07, 0, 0, 0, 0, 0, 0);
        if (result is null || result[0] != 0xF4 || result[7] != 0) return "sem resposta do CRC";
        uint theirs = BitConverter.ToUInt32(result, 8), ours = Crc(file);
        return theirs == ours ? "ok" : $"CRC {theirs:X8} ≠ {ours:X8}";
    }

    private static string VerifyByReading(NollieDevice device, byte[] file)
    {
        for (int offset = 0; offset < file.Length; offset += Chunk)
        {
            int length = Math.Min(Chunk, file.Length - offset);
            bool same = false;
            for (int attempt = 0; attempt < 3 && !same; attempt++)
            {
                var reply = device.Query(Header(0x01, Base + offset, length));
                same = reply is not null && reply[0] == 0xF4 && reply[7] == 0 && reply.AsSpan(8, length).SequenceEqual(file.AsSpan(offset, length));
            }
            if (!same) return $"o que está gravado em {Base + offset:X6} não bate com o arquivo";
        }
        return "ok";
    }

    /// <summary>Manda e espera o "aceito" (1); repete se a resposta não vier ou se ela pedir (6), como o NollieRGB.</summary>
    private static int Submit(NollieDevice device, byte[] packet)
    {
        int misses = 0;
        for (int attempt = 0; attempt < 400; attempt++)
        {
            var reply = device.Query(packet, 200);
            if (reply is null || reply[0] != 0xF4)
            {
                if (++misses >= 9) return -1;
                Thread.Sleep(5);
                continue;
            }
            if (reply[7] == 6)
            {
                Thread.Sleep(5);
                continue;
            }
            return reply[7];
        }
        return -1;
    }

    /// <summary>Espera a controladora ficar pronta (situação 0).</summary>
    private static bool WaitReady(NollieDevice device, TimeSpan timeout, int sleepMs)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            var status = device.Query(0xF4, 0x03, 0, 0, 0, 0, 0, 0);
            if (status is not null && status[0] == 0xF4)
            {
                if (status[7] == 0) return true;
                if (status[7] is 2 or 3 or 4 or 5) return false;
            }
            if (sleepMs > 0) Thread.Sleep(sleepMs);
        }
        return false;
    }

    // ---------------------------------------------------------------- tocar

    /// <summary>Toca a animação gravada uma vez. Devolve a situação da controladora ao começar (0 = tocando).</summary>
    public static int Play(NollieDevice device)
    {
        device.Command(0xF3, 0x02, 0x00, 0x00, 0x00);
        for (int i = 0; i < 30; i++)
        {
            Thread.Sleep(100);
            if (device.Query(0xF3, 0x04) is { } status && status[6] != 255) return status[6];
        }
        return 255;
    }

    public static string PlayStatusText(int code) => code switch
    {
        0 => "tocando",
        1 => "a animação foi apagada",
        2 => "memória ocupada",
        3 => "sem os LEDs de cada canal",
        4 => "sem animação gravada",
        5 => "arquivo inválido",
        6 => "LEDs dos canais diferentes do arquivo",
        7 => "arquivo comprimido",
        8 => "erro de leitura da memória",
        _ => "sem resposta",
    };

    private static byte[] Header(byte sub, int address, int length) => new byte[]
    {
        0xF4, sub, (byte)address, (byte)(address >> 8), (byte)(address >> 16), (byte)length, (byte)(length >> 8), 0x00,
    };

    private static int[] ReadWords(byte[] data)
    {
        var words = new int[16];
        for (int i = 0; i < 16; i++) words[i] = data[i * 2] | data[i * 2 + 1] << 8;
        return words;
    }

    private static byte ToByte(float v) => (byte)Math.Round(Math.Clamp(v, 0, 1) * 255);

    private static void WriteU16(byte[] b, int at, int v)
    {
        b[at] = (byte)v;
        b[at + 1] = (byte)(v >> 8);
    }

    private static void WriteU24(byte[] b, int at, int v)
    {
        b[at] = (byte)v;
        b[at + 1] = (byte)(v >> 8);
        b[at + 2] = (byte)(v >> 16);
    }

    private static void WriteU32(byte[] b, int at, int v)
    {
        WriteU16(b, at, v);
        WriteU16(b, at + 2, v >> 16);
    }
}
