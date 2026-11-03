using System.Buffers.Binary;

namespace Extrator_e_Compilador_RCF.Core
{
    /// <summary>
    /// Uma imagem Pure3D: chunk Image (0x19001) com um filho ImageData (0x19002) = uint tamanho + arquivo.
    /// No Scarface PC todas as imagens usam formato 1 (PNG embutido). O pai costuma ser um Texture (0x19000)
    /// ou um Sprite (0x19005, imagens do frontend).
    ///   Image (após o nome):   versão, largura, altura, bpp, paletizada, temAlfa, formato
    ///   Texture (após o nome): versão, largura, altura, bpp, alphaDepth, mipmaps, tipo, uso, prioridade
    /// </summary>
    public sealed class P3dImage
    {
        public const uint ImageId = 0x19001, ImageDataId = 0x19002, TextureId = 0x19000, SpriteId = 0x19005;
        public const uint FormatPng = 1;

        private readonly int _fields; // posição dos campos após o nome
        public P3dChunk Chunk { get; }
        public P3dChunk DataChunk { get; }
        public string Name { get; }
        /// <summary>Nome do arquivo .png usado ao exportar/importar (único dentro do .p3d).</summary>
        public string FileName { get; internal set; } = "";

        public uint Width => Chunk.GetUInt(_fields + 4);
        public uint Height => Chunk.GetUInt(_fields + 8);
        public uint Bpp => Chunk.GetUInt(_fields + 12);
        public bool Palettized => Chunk.GetUInt(_fields + 16) != 0;
        public bool HasAlpha => Chunk.GetUInt(_fields + 20) != 0;
        public uint Format => Chunk.GetUInt(_fields + 24);

        public string Description =>
            $"{Width}×{Height}, {Bpp} bpp{(Palettized ? L.T(", paleta") : "")}{(HasAlpha ? L.T(", alfa") : "")}";

        private P3dImage(P3dChunk chunk, P3dChunk data)
        {
            Chunk = chunk;
            DataChunk = data;
            Name = chunk.ReadName(out _fields);
            if (_fields + 28 > chunk.Data.Length) throw new InvalidDataException($"Chunk Image '{Name}' curto demais.");
        }

        public byte[] GetFileBytes()
        {
            int len = (int)DataChunk.GetUInt(0);
            return DataChunk.Data.AsSpan(4, Math.Min(len, DataChunk.Data.Length - 4)).ToArray();
        }

        public bool IsPng => Format == FormatPng && Png.IsPng(DataChunk.Data.AsSpan(4));

        /// <summary>Lista as imagens de um P3D já com nomes de arquivo únicos e estáveis.</summary>
        public static List<P3dImage> FindAll(P3dFile p3d)
        {
            var list = new List<P3dImage>();
            foreach (var c in p3d.Root.Descendants().Where(c => c.Id == ImageId))
            {
                var data = c.Children.FirstOrDefault(k => k.Id == ImageDataId);
                if (data == null || data.Data.Length < 4) continue;
                try { list.Add(new P3dImage(c, data)); } catch (InvalidDataException) { }
            }
            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var img in list)
            {
                string baseName = RcfArchive.SafeRelativePath(Path.GetFileNameWithoutExtension(img.Name))
                                            .Replace(Path.DirectorySeparatorChar, '_');
                if (baseName.Length == 0) baseName = "imagem";
                string name = baseName + ".png";
                for (int n = 2; !used.Add(name); n++) name = $"{baseName}__{n}.png";
                img.FileName = name;
            }
            return list;
        }

        /// <summary>
        /// Troca o PNG embutido e atualiza largura/altura/bpp/paleta/alfa no Image e no Texture pai.
        /// Retorna avisos (ex.: tamanho diferente do original) para mostrar ao usuário.
        /// </summary>
        public List<string> ReplacePng(byte[] png)
        {
            var info = Png.ReadInfo(png);
            var warnings = new List<string>();
            if (info.Width != Width || info.Height != Height)
                warnings.Add(L.F("{0}: tamanho mudou de {1}×{2} para {3}×{4}.", Name, Width, Height, info.Width, info.Height));
            if (info.Palettized != Palettized || info.Bpp != Bpp)
                warnings.Add(L.F("{0}: formato mudou de {1} para {2} bpp{3}.", Name, Description, info.Bpp, info.Palettized ? L.T(" paleta") : ""));
            if ((info.Width & (info.Width - 1)) != 0 || (info.Height & (info.Height - 1)) != 0)
                warnings.Add(L.F("{0}: {1}×{2} não é potência de 2 — texturas 3D podem falhar.", Name, info.Width, info.Height));

            var data = new byte[4 + png.Length];
            BinaryPrimitives.WriteUInt32LittleEndian(data, (uint)png.Length);
            png.CopyTo(data, 4);
            DataChunk.Data = data;

            Chunk.SetUInt(_fields + 4, info.Width);
            Chunk.SetUInt(_fields + 8, info.Height);
            Chunk.SetUInt(_fields + 12, info.Bpp);
            Chunk.SetUInt(_fields + 16, info.Palettized ? 1u : 0u);
            Chunk.SetUInt(_fields + 20, info.HasAlpha ? 1u : 0u);
            Chunk.SetUInt(_fields + 24, FormatPng);

            var tex = Chunk.Parent;
            if (tex?.Id == TextureId)
            {
                tex.ReadName(out int t);
                if (t + 20 <= tex.Data.Length)
                {
                    uint oldAlphaDepth = tex.GetUInt(t + 16);
                    tex.SetUInt(t + 4, info.Width);
                    tex.SetUInt(t + 8, info.Height);
                    tex.SetUInt(t + 12, info.Bpp);
                    tex.SetUInt(t + 16, info.HasAlpha ? (oldAlphaDepth != 0 ? oldAlphaDepth : 8u) : 0u);
                }
            }
            return warnings;
        }

        /// <summary>
        /// Troca a imagem convertendo para o formato original (bpp, paleta, alfa), para o jogo receber o mesmo tipo de
        /// PNG que esperava. Aceita PNG, BMP etc. Se o arquivo já estiver no formato certo, os bytes são mantidos.
        /// </summary>
        public List<string> ReplaceKeepingFormat(byte[] file)
        {
            if (Png.IsPng(file))
            {
                var info = Png.ReadInfo(file);
                if (info.Bpp == Bpp && info.Palettized == Palettized && info.HasAlpha == HasAlpha) return ReplacePng(file);
            }
            return ReplacePng(PngCodec.EncodeLike(ArgbImage.Decode(file), Bpp, Palettized, HasAlpha));
        }
    }

    /// <summary>
    /// Sprite Pure3D (0x19005, imagens de menu/tela): uma imagem grande guardada em pedaços com lados em potência
    /// de 2, linha a linha. Pedaços vizinhos se sobrepõem em 2 px (borda para o filtro bilinear não mostrar emendas)
    /// e o que passa do tamanho da imagem fica transparente.
    ///   Sprite (após o nome): nativeX, nativeY, nome do shader, largura, altura, qtd. de imagens, blitBorder
    /// </summary>
    public sealed class P3dSprite
    {
        public P3dChunk Chunk { get; }
        public string Name { get; }
        public int Width { get; }
        public int Height { get; }
        public int Overlap { get; }
        public List<P3dImage> Tiles { get; }
        public int Columns { get; }
        public int Rows => Tiles.Count / Columns;

        private P3dSprite(P3dChunk chunk, string name, int width, int height, List<P3dImage> tiles, int cols, int overlap)
        {
            Chunk = chunk; Name = name; Width = width; Height = height; Tiles = tiles; Columns = cols; Overlap = overlap;
        }

        /// <summary>Reconhece um Sprite fatiado; null se não houver grade consistente (ex.: uma única imagem).</summary>
        public static P3dSprite? TryCreate(P3dChunk chunk, List<P3dImage> allImages)
        {
            if (chunk.Id != P3dImage.SpriteId) return null;
            var tiles = allImages.Where(i => i.Chunk.Parent == chunk).ToList();
            if (tiles.Count < 2 || tiles.Any(t => !t.IsPng)) return null;
            try
            {
                string name = chunk.ReadName(out int p);
                p += 8; // nativeX, nativeY
                int shaderLen = chunk.Data[p];
                p += 1 + shaderLen;
                int width = (int)chunk.GetUInt(p), height = (int)chunk.GetUInt(p + 4);
                if (width <= 0 || height <= 0) return null;

                // colunas = menor período em que as larguras se repetem e cada linha tem altura única
                int cols = Enumerable.Range(1, tiles.Count).First(n => tiles.Count % n == 0 &&
                    tiles.Select((t, i) => t.Width == tiles[i % n].Width && t.Height == tiles[i - i % n].Height).All(ok => ok));
                var widths = tiles.Take(cols).Select(t => (int)t.Width).ToArray();
                var heights = tiles.Where((_, i) => i % cols == 0).Select(t => (int)t.Height).ToArray();

                foreach (int overlap in new[] { 2, 0, 1 })
                    if (Covers(widths, width, overlap) && Covers(heights, height, overlap))
                        return new P3dSprite(chunk, name, width, height, tiles, cols, overlap);
            }
            catch (Exception ex) when (ex is IndexOutOfRangeException or ArgumentOutOfRangeException or InvalidOperationException) { }
            return null;
        }

        /// <summary>Os pedaços cobrem exatamente o tamanho (o último é necessário e basta).</summary>
        private static bool Covers(int[] sizes, int total, int overlap)
        {
            int start = 0;
            for (int i = 0; i < sizes.Length; i++)
            {
                if (start >= total) return false;          // pedaço sobrando
                if (i == sizes.Length - 1) return start + sizes[i] >= total;
                start += sizes[i] - overlap;
            }
            return false;
        }

        private IEnumerable<(P3dImage tile, int x, int y)> Layout()
        {
            int y = 0;
            for (int r = 0; r < Rows; r++)
            {
                int x = 0, h = (int)Tiles[r * Columns].Height;
                for (int c = 0; c < Columns; c++)
                {
                    var t = Tiles[r * Columns + c];
                    yield return (t, x, y);
                    x += (int)t.Width - Overlap;
                }
                y += h - Overlap;
            }
        }

        /// <summary>Junta os pedaços numa imagem só, do tamanho real do Sprite.</summary>
        public ArgbImage Compose()
        {
            var full = new ArgbImage(Width, Height);
            foreach (var (tile, x0, y0) in Layout())
            {
                var t = ArgbImage.Decode(tile.GetFileBytes());
                for (int y = 0; y < t.Height && y0 + y < Height; y++)
                    for (int x = 0; x < t.Width && x0 + x < Width; x++)
                        full[x0 + x, y0 + y] = t[x, y];
            }
            return full;
        }

        /// <summary>
        /// Recorta uma imagem inteira (mesmo tamanho do Sprite) nos pedaços originais, repetindo as bordas
        /// sobrepostas, e grava cada pedaço no formato original. Pedaços com paleta compartilham a mesma paleta.
        /// </summary>
        public void Replace(ArgbImage full)
        {
            if (full.Width != Width || full.Height != Height)
                throw new InvalidDataException(L.F("A imagem de \"{0}\" precisa ter {1}×{2} (recebida: {3}×{4}).", Name, Width, Height, full.Width, full.Height));

            var palettes = new Dictionary<(int bits, bool alpha), uint[]>();
            foreach (var (tile, x0, y0) in Layout().ToList())
            {
                var part = new ArgbImage((int)tile.Width, (int)tile.Height);
                for (int y = 0; y < part.Height; y++)
                    for (int x = 0; x < part.Width; x++)
                        part[x, y] = x0 + x < Width && y0 + y < Height ? full[x0 + x, y0 + y] : 0u;

                uint[]? palette = null;
                if (tile.Palettized)
                {
                    var key = (tile.Bpp <= 4 ? 4 : 8, tile.HasAlpha);
                    if (!palettes.TryGetValue(key, out palette))
                    {
                        // paleta única calculada da imagem inteira (+ transparente para as sobras fora da imagem)
                        palette = Quantizer.BuildPalette(full.Pixels.Append(0u), 1 << key.Item1, key.Item2);
                        palettes[key] = palette;
                    }
                }
                tile.ReplacePng(PngCodec.EncodeLike(part, tile.Bpp, tile.Palettized, tile.HasAlpha, palette));
            }
        }
    }

    /// <summary>
    /// Uma imagem como o usuário a vê: uma imagem Pure3D comum ou um Sprite fatiado já juntado.
    /// Exportar gera um PNG; importar aceita PNG/BMP e grava no formato original.
    /// </summary>
    public sealed class P3dGraphic
    {
        public P3dImage? Image { get; private init; }
        public P3dSprite? Sprite { get; private init; }
        public string Name => Sprite?.Name ?? Image!.Name;
        public int Width => Sprite?.Width ?? (int)Image!.Width;
        public int Height => Sprite?.Height ?? (int)Image!.Height;
        /// <summary>Nome do .png ao exportar/importar (único dentro do .p3d).</summary>
        public string FileName { get; private set; } = "";
        public bool CanExport => Sprite != null || Image!.IsPng;

        public string Description => Sprite != null
            ? L.F("Sprite {0}×{1}, juntado de {2} partes", Sprite.Width, Sprite.Height, Sprite.Tiles.Count)
            : Image!.Description;

        private byte[]? _export;

        /// <summary>PNG para o usuário editar (Sprites saem juntados, em RGBA).</summary>
        public byte[] ExportPng() =>
            _export ??= Sprite != null ? PngCodec.EncodeRgba(Sprite.Compose()) : Image!.GetFileBytes();

        /// <summary>Troca pelo arquivo editado; retorna avisos.</summary>
        public List<string> Import(byte[] file)
        {
            _export = null;
            if (Sprite == null) return Image!.ReplaceKeepingFormat(file);
            Sprite.Replace(ArgbImage.Decode(file));
            return new List<string>();
        }

        public static List<P3dGraphic> FindAll(P3dFile p3d)
        {
            var images = P3dImage.FindAll(p3d);
            var list = new List<P3dGraphic>();
            var inSprite = new HashSet<P3dImage>();
            var sprites = new Dictionary<P3dChunk, P3dSprite?>();
            foreach (var img in images)
            {
                var parent = img.Chunk.Parent;
                if (parent?.Id == P3dImage.SpriteId)
                {
                    if (!sprites.TryGetValue(parent, out var sprite))
                    {
                        sprites[parent] = sprite = P3dSprite.TryCreate(parent, images);
                        if (sprite != null)
                        {
                            list.Add(new P3dGraphic { Sprite = sprite });
                            inSprite.UnionWith(sprite.Tiles);
                        }
                    }
                    if (sprite != null) continue;
                }
                if (!inSprite.Contains(img)) list.Add(new P3dGraphic { Image = img });
            }

            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var g in list)
            {
                string baseName = RcfArchive.SafeRelativePath(Path.GetFileNameWithoutExtension(g.Name))
                                            .Replace(Path.DirectorySeparatorChar, '_');
                if (baseName.Length == 0) baseName = "imagem";
                string name = baseName + ".png";
                for (int n = 2; !used.Add(name); n++) name = $"{baseName}__{n}.png";
                g.FileName = name;
            }
            return list;
        }
    }

    /// <summary>Leitura mínima do cabeçalho PNG (IHDR + presença de tRNS).</summary>
    public static class Png
    {
        private static readonly byte[] Signature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        public readonly record struct Info(uint Width, uint Height, uint Bpp, bool Palettized, bool HasAlpha);

        public static bool IsPng(ReadOnlySpan<byte> data) => data.Length >= 8 && data[..8].SequenceEqual(Signature);

        public static Info ReadInfo(byte[] png)
        {
            if (!IsPng(png) || png.Length < 33 || BinaryPrimitives.ReadUInt32BigEndian(png.AsSpan(12)) != 0x49484452)
                throw new InvalidDataException(L.T("O arquivo não é um PNG válido."));
            uint w = BinaryPrimitives.ReadUInt32BigEndian(png.AsSpan(16));
            uint h = BinaryPrimitives.ReadUInt32BigEndian(png.AsSpan(20));
            byte depth = png[24], colorType = png[25];

            bool trns = false;
            for (int p = 8; p + 8 <= png.Length;)
            {
                uint len = BinaryPrimitives.ReadUInt32BigEndian(png.AsSpan(p));
                uint type = BinaryPrimitives.ReadUInt32BigEndian(png.AsSpan(p + 4));
                if (type == 0x74524E53) { trns = true; break; } // tRNS
                if (type == 0x49444154 || type == 0x49454E44) break; // IDAT / IEND
                p += 12 + (int)len;
            }

            uint bpp = colorType switch
            {
                0 => depth,          // cinza
                2 => depth * 3u,     // RGB
                3 => depth,          // paleta
                4 => depth * 2u,     // cinza + alfa
                6 => depth * 4u,     // RGBA
                _ => throw new InvalidDataException(L.F("Tipo de cor PNG desconhecido: {0}.", colorType))
            };
            return new Info(w, h, bpp, colorType == 3, colorType is 4 or 6 || trns);
        }
    }
}
