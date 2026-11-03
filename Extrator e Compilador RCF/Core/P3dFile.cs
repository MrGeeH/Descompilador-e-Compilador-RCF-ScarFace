using System.Buffers.Binary;
using System.Text;

namespace Extrator_e_Compilador_RCF.Core
{
    /// <summary>
    /// Chunk Pure3D: uint id, uint tamanhoDados (cabeçalho de 12 bytes + dados), uint tamanhoTotal (inclui filhos).
    /// </summary>
    public sealed class P3dChunk
    {
        public uint Id { get; set; }
        /// <summary>Dados próprios do chunk (sem o cabeçalho de 12 bytes e sem os filhos).</summary>
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public List<P3dChunk> Children { get; } = new();
        public P3dChunk? Parent { get; private set; }

        public long TotalSize => 12 + Data.Length + Children.Sum(c => c.TotalSize);

        internal void Adopt(P3dChunk child) { child.Parent = this; Children.Add(child); }

        /// <summary>Nome Pure3D no início dos dados (byte de tamanho + texto com \0).</summary>
        public string ReadName(out int after)
        {
            int len = Data.Length > 0 ? Data[0] : 0;
            after = 1 + len;
            if (after > Data.Length) { after = Data.Length; return ""; }
            return Encoding.Latin1.GetString(Data, 1, len).Split('\0')[0];
        }

        public uint GetUInt(int offset) => BinaryPrimitives.ReadUInt32LittleEndian(Data.AsSpan(offset));
        public void SetUInt(int offset, uint value) => BinaryPrimitives.WriteUInt32LittleEndian(Data.AsSpan(offset), value);

        public IEnumerable<P3dChunk> Descendants()
        {
            foreach (var c in Children)
            {
                yield return c;
                foreach (var d in c.Descendants()) yield return d;
            }
        }
    }

    /// <summary>Arquivo .p3d (Pure3D) com a árvore completa de chunks, regravável.</summary>
    public sealed class P3dFile
    {
        public const uint RootId = 0xFF443350; // "P3D\xFF"
        public P3dChunk Root { get; private set; } = new() { Id = RootId };
        /// <summary>Bytes após o fim do chunk raiz (normalmente vazio).</summary>
        public byte[] Trailing { get; private set; } = Array.Empty<byte>();

        public static bool LooksLikeP3d(ReadOnlySpan<byte> data) =>
            data.Length >= 12 && BinaryPrimitives.ReadUInt32LittleEndian(data) == RootId;

        public static P3dFile Parse(byte[] data)
        {
            if (!LooksLikeP3d(data))
                throw new InvalidDataException(L.T("Não é um arquivo P3D (assinatura P3D\\xFF ausente)."));
            var f = new P3dFile { Root = ReadChunk(data, 0, data.Length) };
            // Alguns arquivos (ex.: textbibles editados) têm zeros sobrando após o chunk raiz: preserva.
            f.Trailing = data.AsSpan((int)f.Root.TotalSize).ToArray();
            return f;
        }

        private static P3dChunk ReadChunk(byte[] b, int offset, int limit)
        {
            uint id = BinaryPrimitives.ReadUInt32LittleEndian(b.AsSpan(offset));
            uint dataSize = BinaryPrimitives.ReadUInt32LittleEndian(b.AsSpan(offset + 4));
            uint totalSize = BinaryPrimitives.ReadUInt32LittleEndian(b.AsSpan(offset + 8));
            if (dataSize < 12 || totalSize < dataSize || offset + (long)totalSize > limit)
                throw new InvalidDataException(L.F("Chunk {0:X8} inválido na posição {1}.", id, offset));

            var chunk = new P3dChunk { Id = id, Data = b.AsSpan(offset + 12, (int)dataSize - 12).ToArray() };
            int pos = offset + (int)dataSize, end = offset + (int)totalSize;
            while (pos < end)
            {
                if (end - pos < 12) throw new InvalidDataException(L.F("Sobra de bytes no chunk {0:X8}.", id));
                var child = ReadChunk(b, pos, end);
                chunk.Adopt(child);
                pos += (int)child.TotalSize;
            }
            return chunk;
        }

        public byte[] ToArray()
        {
            using var ms = new MemoryStream();
            Write(ms, Root);
            ms.Write(Trailing);
            return ms.ToArray();
        }

        private static void Write(Stream s, P3dChunk c)
        {
            Span<byte> head = stackalloc byte[12];
            BinaryPrimitives.WriteUInt32LittleEndian(head, c.Id);
            BinaryPrimitives.WriteUInt32LittleEndian(head[4..], (uint)(12 + c.Data.Length));
            BinaryPrimitives.WriteUInt32LittleEndian(head[8..], (uint)c.TotalSize);
            s.Write(head);
            s.Write(c.Data);
            foreach (var child in c.Children) Write(s, child);
        }
    }
}
