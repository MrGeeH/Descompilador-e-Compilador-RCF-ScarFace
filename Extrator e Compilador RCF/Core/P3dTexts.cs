using System.Buffers.Binary;
using System.Text;

namespace Extrator_e_Compilador_RCF.Core
{
    /// <summary>Um texto do jogo dentro de um .p3d.</summary>
    public sealed class P3dTextEntry
    {
        /// <summary>Hash do identificador do texto (TextBible) — o nome original não fica no arquivo.</summary>
        public uint Hash { get; init; }
        /// <summary>Idioma do bloco: letra da TextBible (E, F, S, I, G) ou nome da legenda (English…).</summary>
        public required string Language { get; init; }
        public required string Original { get; init; }
        public string Text { get; set; } = "";
        public bool Changed => Text != Original;
    }

    /// <summary>
    /// Textos de um .p3d do Scarface, lidos e gravados direto (substitui a Scarface_P3D_Text_Tool).
    /// <para>
    /// TextBible (textbible_E.p3d, HUD_textbible_E.p3d, Toybox_*_E.p3d…): chunk 0x0001800D (nome, versão, idiomas)
    /// com um filho 0x0001800E por idioma: nome, char idioma, u32 quantidade, u32 módulo do hash (65599),
    /// u32 tamanho do buffer, u32 hashes[n] (ordenados), u32 offsets[n] (bytes no buffer),
    /// buffer UTF-16LE com os textos terminados em \0, na ordem dos hashes.
    /// </para>
    /// <para>
    /// Legendas (art\subtitles): chunks 0x0100500A, um por idioma: u32 tamanho + nome do idioma + \0,
    /// i32, i32, u32 caracteres (com \0) e o texto UTF-16LE.
    /// </para>
    /// Ao gravar, só o buffer/offsets (ou o texto da legenda) mudam; o resto é copiado do original, de modo que um
    /// arquivo sem edições volta byte a byte igual e um editado sai igual ao que a ferramenta original geraria.
    /// </summary>
    public sealed class P3dTextDocument
    {
        public const uint TextBibleId = 0x0001800D;
        public const uint LanguageId = 0x0001800E;
        public const uint SubtitleId = 0x0100500A;

        private readonly P3dFile _file;
        private readonly List<Block> _blocks = new();

        public List<P3dTextEntry> Entries { get; } = new();
        public bool IsSubtitle { get; private set; }
        public bool HasChanges => Entries.Any(e => e.Changed);
        /// <summary>Nome da TextBible (ex.: "HUD") ou "Legenda".</summary>
        public string Title { get; private set; } = "";

        private sealed class Block
        {
            public required P3dChunk Chunk { get; init; }
            public int HeaderLength;   // bytes copiados como estão (até o tamanho do buffer / o contador de caracteres)
            public int TailStart;      // início dos bytes depois dos textos (copiados como estão)
            public int First, Count;   // faixa em Entries
        }

        private P3dTextDocument(P3dFile file) => _file = file;

        /// <summary>Lê os textos; devolve null se o arquivo não tiver TextBible nem legenda.</summary>
        public static P3dTextDocument? TryParse(byte[] data)
        {
            if (!P3dFile.LooksLikeP3d(data)) return null;
            P3dFile file;
            try { file = P3dFile.Parse(data); }
            catch { return null; }
            var doc = new P3dTextDocument(file);
            try
            {
                foreach (var c in file.Root.Descendants())
                {
                    if (c.Id == LanguageId) doc.ReadLanguage(c);
                    else if (c.Id == SubtitleId) doc.ReadSubtitle(c);
                    else if (c.Id == TextBibleId && doc.Title.Length == 0) doc.Title = c.ReadName(out _);
                }
            }
            catch (Exception ex) when (ex is ArgumentException or IndexOutOfRangeException or InvalidDataException) { return null; }
            if (doc._blocks.Count == 0) return null;
            doc.IsSubtitle = doc._blocks.All(b => b.Chunk.Id == SubtitleId);
            if (doc.IsSubtitle) doc.Title = "Legenda";
            return doc;
        }

        /// <summary>Teste rápido (só o começo do arquivo) para marcar na lista os .p3d que têm texto.</summary>
        public static bool LooksLikeText(ReadOnlySpan<byte> head) =>
            head.Length >= 16 && P3dFile.LooksLikeP3d(head) &&
            BinaryPrimitives.ReadUInt32LittleEndian(head[12..]) is TextBibleId or SubtitleId;

        private void ReadLanguage(P3dChunk c)
        {
            var d = c.Data;
            c.ReadName(out int q);
            string lang = ((char)d[q]).ToString();
            q += 1;
            int n = checked((int)Read(d, q));
            int bufSize = checked((int)Read(d, q + 8));
            int hashes = q + 12, offsets = hashes + 4 * n, buffer = offsets + 4 * n;
            if (buffer + bufSize > d.Length) throw new InvalidDataException("buffer");
            var block = new Block { Chunk = c, HeaderLength = q + 8, TailStart = buffer + bufSize, First = Entries.Count, Count = n };
            for (int i = 0; i < n; i++)
            {
                int start = buffer + (int)Read(d, offsets + 4 * i), end = start;
                while (end + 1 < block.TailStart && (d[end] | d[end + 1]) != 0) end += 2;
                string text = Encoding.Unicode.GetString(d, start, end - start);
                Entries.Add(new P3dTextEntry { Hash = Read(d, hashes + 4 * i), Language = lang, Original = text, Text = text });
            }
            _blocks.Add(block);
        }

        private void ReadSubtitle(P3dChunk c)
        {
            var d = c.Data;
            int len = checked((int)Read(d, 0));
            if (len is < 0 or > 64) throw new InvalidDataException("idioma");
            string lang = Encoding.ASCII.GetString(d, 4, len).TrimEnd('\0');
            int q = 4 + len + 1 + 8;
            int count = checked((int)Read(d, q));
            int textStart = q + 4;
            if (textStart + count * 2 > d.Length) throw new InvalidDataException("texto");
            string text = Encoding.Unicode.GetString(d, textStart, count * 2).TrimEnd('\0');
            _blocks.Add(new Block { Chunk = c, HeaderLength = q, TailStart = textStart + count * 2, First = Entries.Count, Count = 1 });
            Entries.Add(new P3dTextEntry { Language = lang, Original = text, Text = text });
        }

        /// <summary>Gera o .p3d com os textos atuais (sem alterações, devolve exatamente os bytes originais).</summary>
        public byte[] ToArray()
        {
            foreach (var b in _blocks)
            {
                var d = b.Chunk.Data;
                using var ms = new MemoryStream();
                ms.Write(d, 0, b.HeaderLength);
                if (b.Chunk.Id == SubtitleId)
                {
                    var text = Encoding.Unicode.GetBytes(Entries[b.First].Text + "\0");
                    WriteU32(ms, (uint)(text.Length / 2));
                    ms.Write(text);
                }
                else
                {
                    var buffer = new MemoryStream();
                    var offsets = new uint[b.Count];
                    for (int i = 0; i < b.Count; i++)
                    {
                        offsets[i] = (uint)buffer.Length;
                        buffer.Write(Encoding.Unicode.GetBytes(Entries[b.First + i].Text + "\0"));
                    }
                    WriteU32(ms, (uint)buffer.Length);
                    ms.Write(d, b.HeaderLength + 4, 4 * b.Count); // hashes
                    foreach (uint o in offsets) WriteU32(ms, o);
                    buffer.WriteTo(ms);
                }
                ms.Write(d, b.TailStart, d.Length - b.TailStart);
                var data = ms.ToArray();
                b.Chunk.Data = data;
                // os índices dos blocos continuam válidos: o cabeçalho não muda de tamanho
                b.TailStart = data.Length - (d.Length - b.TailStart);
            }
            return _file.ToArray();
        }

        // ---------------------------------------------------------------- .txt (compatível com a Text Tool)

        /// <summary>
        /// .txt no formato da Scarface_P3D_Text_Tool: UTF-8 com BOM, um texto por linha (CRLF), na ordem do arquivo.
        /// Quebras de linha dentro de um texto viram "\n".
        /// </summary>
        public byte[] ExportTxt()
        {
            var sb = new StringBuilder();
            foreach (var e in Entries) sb.Append(Escape(e.Text)).Append("\r\n");
            return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        }

        /// <summary>Lê um .txt (UTF-8/UTF-16, com ou sem BOM). Devolve quantos textos mudaram.</summary>
        public int ImportTxt(byte[] file)
        {
            string s = DecodeText(file);
            var lines = s.Replace("\r\n", "\n").Split('\n').ToList();
            if (lines.Count > Entries.Count && lines[^1].Length == 0) lines.RemoveAt(lines.Count - 1);
            if (lines.Count != Entries.Count)
                throw new InvalidDataException(L.F("O .txt tem {0} linha(s), mas o arquivo tem {1} texto(s).", lines.Count, Entries.Count));
            int changed = 0;
            for (int i = 0; i < lines.Count; i++)
            {
                string t = Unescape(lines[i]);
                if (t != Entries[i].Text) { Entries[i].Text = t; changed++; }
            }
            return changed;
        }

        public static string Escape(string s) => s.Replace("\\", "\\\\").Replace("\r\n", "\\n").Replace("\n", "\\n").Replace("\r", "\\r");

        public static string Unescape(string s)
        {
            if (!s.Contains('\\')) return s;
            var sb = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '\\' && i + 1 < s.Length)
                {
                    char n = s[i + 1];
                    if (n is 'n' or 'r' or '\\') { sb.Append(n == 'n' ? '\n' : n == 'r' ? '\r' : '\\'); i++; continue; }
                }
                sb.Append(s[i]);
            }
            return sb.ToString();
        }

        private static string DecodeText(byte[] b)
        {
            if (b.Length >= 3 && b[0] == 0xEF && b[1] == 0xBB && b[2] == 0xBF) return Encoding.UTF8.GetString(b, 3, b.Length - 3);
            if (b.Length >= 2 && b[0] == 0xFF && b[1] == 0xFE) return Encoding.Unicode.GetString(b, 2, b.Length - 2);
            if (b.Length >= 2 && b[0] == 0xFE && b[1] == 0xFF) return Encoding.BigEndianUnicode.GetString(b, 2, b.Length - 2);
            try { return new UTF8Encoding(false, true).GetString(b); }
            catch (DecoderFallbackException) { return Encoding.Latin1.GetString(b); }
        }

        private static uint Read(byte[] d, int offset) => BinaryPrimitives.ReadUInt32LittleEndian(d.AsSpan(offset));

        private static void WriteU32(Stream s, uint v)
        {
            Span<byte> b = stackalloc byte[4];
            BinaryPrimitives.WriteUInt32LittleEndian(b, v);
            s.Write(b);
        }
    }
}
