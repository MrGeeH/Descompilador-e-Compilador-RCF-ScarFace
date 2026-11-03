using System.Buffers.Binary;
using System.Drawing.Imaging;
using System.IO.Compression;

namespace Extrator_e_Compilador_RCF.Core
{
    /// <summary>Imagem em memória: pixels ARGB (0xAARRGGBB, não pré-multiplicado), linha a linha.</summary>
    public sealed class ArgbImage
    {
        public int Width { get; }
        public int Height { get; }
        public uint[] Pixels { get; }

        public ArgbImage(int width, int height)
        {
            Width = width;
            Height = height;
            Pixels = new uint[width * height];
        }

        public uint this[int x, int y] { get => Pixels[y * Width + x]; set => Pixels[y * Width + x] = value; }

        /// <summary>Decodifica qualquer imagem que o GDI+ entenda (PNG com paleta/tRNS, RGB, RGBA, BMP…).</summary>
        public static ArgbImage Decode(byte[] file)
        {
            using var bmp = new Bitmap(new MemoryStream(file));
            var img = new ArgbImage(bmp.Width, bmp.Height);
            var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                var row = new int[bmp.Width];
                for (int y = 0; y < bmp.Height; y++)
                {
                    System.Runtime.InteropServices.Marshal.Copy(data.Scan0 + y * data.Stride, row, 0, bmp.Width);
                    for (int x = 0; x < bmp.Width; x++) img.Pixels[y * bmp.Width + x] = (uint)row[x];
                }
            }
            finally { bmp.UnlockBits(data); }
            return img;
        }
    }

    /// <summary>
    /// Codificador PNG próprio (determinístico): RGBA, RGB ou indexado de 8/4 bits com paleta e transparência
    /// (tRNS). Usado para gravar imagens no mesmo formato que o jogo usava originalmente.
    /// </summary>
    public static class PngCodec
    {
        private static readonly byte[] Signature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        /// <summary>Grava no formato indicado pelo cabeçalho Pure3D (bpp, paleta, alfa).</summary>
        public static byte[] EncodeLike(ArgbImage img, uint bpp, bool palettized, bool hasAlpha, uint[]? palette = null)
        {
            if (palettized)
            {
                int bits = bpp <= 4 ? 4 : 8;
                palette ??= Quantizer.BuildPalette(img.Pixels, 1 << bits, hasAlpha);
                return EncodeIndexed(img, palette, bits, hasAlpha);
            }
            return hasAlpha || bpp >= 32 ? EncodeRgba(img) : EncodeRgb(img);
        }

        public static byte[] EncodeRgba(ArgbImage img)
        {
            int w = img.Width;
            return Encode(img.Width, img.Height, 8, 6, null, null, (y, line) =>
            {
                for (int x = 0; x < w; x++)
                {
                    uint p = img[x, y];
                    line[x * 4] = (byte)(p >> 16);
                    line[x * 4 + 1] = (byte)(p >> 8);
                    line[x * 4 + 2] = (byte)p;
                    line[x * 4 + 3] = (byte)(p >> 24);
                }
            }, w * 4);
        }

        public static byte[] EncodeRgb(ArgbImage img)
        {
            int w = img.Width;
            return Encode(img.Width, img.Height, 8, 2, null, null, (y, line) =>
            {
                for (int x = 0; x < w; x++)
                {
                    uint p = img[x, y];
                    line[x * 3] = (byte)(p >> 16);
                    line[x * 3 + 1] = (byte)(p >> 8);
                    line[x * 3 + 2] = (byte)p;
                }
            }, w * 3);
        }

        public static byte[] EncodeIndexed(ArgbImage img, uint[] palette, int bits, bool hasAlpha)
        {
            var map = new Quantizer.Mapper(palette, hasAlpha);
            int w = img.Width;
            int stride = bits == 8 ? w : (w + 1) / 2;
            var plte = new byte[palette.Length * 3];
            for (int i = 0; i < palette.Length; i++)
            {
                plte[i * 3] = (byte)(palette[i] >> 16);
                plte[i * 3 + 1] = (byte)(palette[i] >> 8);
                plte[i * 3 + 2] = (byte)palette[i];
            }
            byte[]? trns = null;
            if (hasAlpha)
            {
                int last = Array.FindLastIndex(palette, p => (p >> 24) != 255);
                if (last >= 0) trns = palette.Take(last + 1).Select(p => (byte)(p >> 24)).ToArray();
            }
            return Encode(img.Width, img.Height, (byte)bits, 3, plte, trns, (y, line) =>
            {
                Array.Clear(line);
                for (int x = 0; x < w; x++)
                {
                    int idx = map.Nearest(img[x, y]);
                    if (bits == 8) line[x] = (byte)idx;
                    else line[x / 2] |= (byte)((x & 1) == 0 ? idx << 4 : idx);
                }
            }, stride);
        }

        private static byte[] Encode(int w, int h, byte depth, byte colorType, byte[]? plte, byte[]? trns,
            Action<int, byte[]> fillLine, int stride)
        {
            using var ms = new MemoryStream();
            ms.Write(Signature);
            var ihdr = new byte[13];
            BinaryPrimitives.WriteInt32BigEndian(ihdr, w);
            BinaryPrimitives.WriteInt32BigEndian(ihdr.AsSpan(4), h);
            ihdr[8] = depth;
            ihdr[9] = colorType;
            WriteChunk(ms, "IHDR", ihdr);
            if (plte != null) WriteChunk(ms, "PLTE", plte);
            if (trns != null) WriteChunk(ms, "tRNS", trns);

            using (var raw = new MemoryStream())
            {
                using (var z = new ZLibStream(raw, CompressionLevel.SmallestSize, leaveOpen: true))
                {
                    var line = new byte[stride];
                    for (int y = 0; y < h; y++)
                    {
                        fillLine(y, line);
                        z.WriteByte(0); // filtro "None"
                        z.Write(line);
                    }
                }
                WriteChunk(ms, "IDAT", raw.ToArray());
            }
            WriteChunk(ms, "IEND", Array.Empty<byte>());
            return ms.ToArray();
        }

        private static void WriteChunk(Stream s, string type, byte[] data)
        {
            Span<byte> head = stackalloc byte[8];
            BinaryPrimitives.WriteInt32BigEndian(head, data.Length);
            for (int i = 0; i < 4; i++) head[4 + i] = (byte)type[i];
            s.Write(head);
            s.Write(data);
            uint crc = Crc32(head[4..], 0xFFFFFFFFu);
            crc = Crc32(data, crc) ^ 0xFFFFFFFFu;
            Span<byte> c = stackalloc byte[4];
            BinaryPrimitives.WriteUInt32BigEndian(c, crc);
            s.Write(c);
        }

        private static readonly uint[] CrcTable = Enumerable.Range(0, 256).Select(n =>
        {
            uint c = (uint)n;
            for (int k = 0; k < 8; k++) c = (c & 1) != 0 ? 0xEDB88320u ^ (c >> 1) : c >> 1;
            return c;
        }).ToArray();

        private static uint Crc32(ReadOnlySpan<byte> data, uint crc)
        {
            foreach (byte b in data) crc = CrcTable[(crc ^ b) & 0xFF] ^ (crc >> 8);
            return crc;
        }
    }

    /// <summary>Redução de cores por "median cut" (RGBA) e mapeamento para a cor mais próxima.</summary>
    public static class Quantizer
    {
        /// <summary>
        /// Gera uma paleta de até <paramref name="maxColors"/> cores. Se a imagem já tem poucas cores, usa as exatas.
        /// Pixels totalmente transparentes ganham uma entrada própria (0x00000000).
        /// </summary>
        public static uint[] BuildPalette(IEnumerable<uint> pixels, int maxColors, bool hasAlpha)
        {
            var hist = new Dictionary<uint, int>();
            bool anyTransparent = false;
            foreach (uint p0 in pixels)
            {
                uint p = hasAlpha ? p0 : p0 | 0xFF000000u;
                if ((p >> 24) == 0) { anyTransparent = true; continue; }
                hist[p] = hist.GetValueOrDefault(p) + 1;
            }
            int budget = maxColors - (anyTransparent ? 1 : 0);
            var palette = new List<uint>();
            if (anyTransparent) palette.Add(0);
            if (hist.Count <= budget) palette.AddRange(hist.Keys.OrderBy(k => k));
            else palette.AddRange(MedianCut(hist.Select(kv => (kv.Key, kv.Value)).ToList(), budget));
            if (palette.Count == 0) palette.Add(0xFF000000u);
            // entradas com alfa primeiro deixam o tRNS menor
            return palette.OrderBy(p => (p >> 24) == 255 ? 1 : 0).ToArray();
        }

        private static int Ch(uint c, int i) => (int)(c >> (i == 3 ? 24 : 16 - i * 8)) & 0xFF;

        private static IEnumerable<uint> MedianCut(List<(uint color, int count)> colors, int boxes)
        {
            var list = new List<List<(uint color, int count)>> { colors };
            while (list.Count < boxes)
            {
                // divide a caixa com maior (amplitude × população)
                int best = -1, bestCh = 0;
                long bestScore = 0;
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].Count < 2) continue;
                    long pop = list[i].Sum(c => (long)c.count);
                    for (int ch = 0; ch < 4; ch++)
                    {
                        int min = 255, max = 0;
                        foreach (var (c, _) in list[i]) { int v = Ch(c, ch); if (v < min) min = v; if (v > max) max = v; }
                        long score = (long)(max - min) * pop;
                        if (score > bestScore) { bestScore = score; best = i; bestCh = ch; }
                    }
                }
                if (best < 0) break;
                var box = list[best].OrderBy(c => Ch(c.color, bestCh)).ToList();
                long half = box.Sum(c => (long)c.count) / 2, acc = 0;
                int cut = 1;
                for (; cut < box.Count - 1; cut++) { acc += box[cut - 1].count; if (acc >= half) break; }
                list[best] = box.GetRange(0, cut);
                list.Add(box.GetRange(cut, box.Count - cut));
            }
            foreach (var box in list)
            {
                double total = box.Sum(c => (double)c.count), r = 0, g = 0, b = 0, a = 0;
                foreach (var (c, n) in box) { r += Ch(c, 0) * n; g += Ch(c, 1) * n; b += Ch(c, 2) * n; a += Ch(c, 3) * n; }
                yield return ((uint)Math.Round(a / total) << 24) | ((uint)Math.Round(r / total) << 16) |
                             ((uint)Math.Round(g / total) << 8) | (uint)Math.Round(b / total);
            }
        }

        /// <summary>Encontra a cor mais próxima da paleta (com cache por cor).</summary>
        public sealed class Mapper(uint[] palette, bool hasAlpha)
        {
            private readonly Dictionary<uint, int> _cache = new();

            public int Nearest(uint color)
            {
                if (!hasAlpha) color |= 0xFF000000u;
                else if ((color >> 24) == 0) color = 0;
                if (_cache.TryGetValue(color, out int idx)) return idx;
                long best = long.MaxValue;
                for (int i = 0; i < palette.Length; i++)
                {
                    long d = 0;
                    for (int ch = 0; ch < 4; ch++)
                    {
                        long diff = Ch(color, ch) - Ch(palette[i], ch);
                        d += diff * diff * (ch == 3 ? 2 : 1);
                    }
                    if (d < best) { best = d; idx = i; if (d == 0) break; }
                }
                _cache[color] = idx;
                return idx;
            }
        }
    }
}
