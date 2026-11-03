using System.Text;

namespace Extrator_e_Compilador_RCF.Core
{
    /// <summary>Uma entrada (arquivo) dentro do RCF.</summary>
    public sealed class RcfEntry
    {
        /// <summary>Posição na tabela de nomes (= ordem dos dados no arquivo).</summary>
        public int Index { get; init; }
        public string Name { get; init; } = "";
        public uint Hash { get; init; }
        public uint Offset { get; init; }
        public uint Size { get; init; }
        public uint Timestamp { get; init; }
        public uint Alignment { get; init; }
        public uint Reserved { get; init; }

        public DateTime Date => DateTimeOffset.FromUnixTimeSeconds(Timestamp).LocalDateTime;
        public string Extension => Path.GetExtension(Name).ToLowerInvariant();
    }

    /// <summary>Leitor do formato RCF. Ver <see cref="RcfFormat"/> para o layout.</summary>
    public sealed class RcfArchive
    {
        public string FilePath { get; private set; } = "";
        public long FileLength { get; private set; }
        public byte[] Version { get; private set; } = RcfFormat.DefaultVersion;
        public uint HeaderReserved { get; private set; }
        public uint NameTableAlignment { get; private set; } = RcfFormat.DefaultAlignment;
        public uint NameTableReserved { get; private set; }

        /// <summary>true se todos os nomes bateram com o hash do diretório.</summary>
        public bool HashesVerified { get; private set; }

        /// <summary>Entradas na ordem da tabela de nomes.</summary>
        public List<RcfEntry> Entries { get; } = new();

        public static RcfArchive Open(string path)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var br = new BinaryReader(fs, Encoding.Latin1, leaveOpen: true);
            var rcf = new RcfArchive { FilePath = path, FileLength = fs.Length };

            if (fs.Length < RcfFormat.HeaderSize)
                throw new InvalidDataException(L.T("Arquivo pequeno demais para ser um RCF."));

            string magic = Encoding.ASCII.GetString(br.ReadBytes(32)).TrimEnd('\0');
            if (magic != RcfFormat.Magic)
                throw new InvalidDataException(L.F("Assinatura '{0}' não encontrada — não é um RCF do Scarface.", RcfFormat.Magic));

            rcf.Version = br.ReadBytes(4);
            if (rcf.Version[2] != 0)
                throw new InvalidDataException(L.T("RCF big-endian (versão de console) não é suportado."));

            uint dirOffset = br.ReadUInt32();
            uint dirSize = br.ReadUInt32();
            uint nameOffset = br.ReadUInt32();
            uint nameSize = br.ReadUInt32();
            rcf.HeaderReserved = br.ReadUInt32();
            uint count = br.ReadUInt32();

            if (count > 1_000_000 || (long)count * RcfFormat.DirEntrySize > dirSize ||
                dirOffset + (long)dirSize > fs.Length || nameOffset + (long)nameSize > fs.Length)
                throw new InvalidDataException(L.T("Cabeçalho do RCF inconsistente."));

            fs.Position = dirOffset;
            var dir = new (uint hash, uint offset, uint size)[count];
            for (int i = 0; i < count; i++)
                dir[i] = (br.ReadUInt32(), br.ReadUInt32(), br.ReadUInt32());

            fs.Position = nameOffset;
            rcf.NameTableAlignment = br.ReadUInt32();
            rcf.NameTableReserved = br.ReadUInt32();
            var names = new List<(string name, uint ts, uint align, uint reserved)>((int)count);
            for (int i = 0; i < count; i++)
            {
                uint ts = br.ReadUInt32(), align = br.ReadUInt32(), reserved = br.ReadUInt32();
                int len = br.ReadInt32();
                if (len < 1 || len > 4096)
                    throw new InvalidDataException(L.F("Tabela de nomes corrompida (nome #{0} com tamanho {1}).", i, len));
                string name = Encoding.Latin1.GetString(br.ReadBytes(len)).TrimEnd('\0');
                fs.Position += 3;
                names.Add((name, ts, align, reserved));
            }

            // Liga nome ↔ entrada do diretório pelo hash; se algo não bater, cai para a ordem dos offsets.
            var byHash = new Dictionary<uint, int>();
            bool unique = true;
            for (int i = 0; i < dir.Length; i++) unique &= byHash.TryAdd(dir[i].hash, i);
            var match = new int[count];
            bool ok = unique;
            for (int i = 0; ok && i < count; i++)
                ok = byHash.TryGetValue(RcfFormat.NameHash(names[i].name), out match[i]);
            if (ok) ok = match.Distinct().Count() == count;
            if (!ok)
            {
                var byOffset = Enumerable.Range(0, (int)count).OrderBy(i => dir[i].offset).ThenBy(i => i).ToArray();
                Array.Copy(byOffset, match, count);
            }
            rcf.HashesVerified = ok;

            for (int i = 0; i < count; i++)
            {
                var d = dir[match[i]];
                if (d.offset + (long)d.size > fs.Length)
                    throw new InvalidDataException(L.F("Entrada '{0}' aponta para fora do arquivo.", names[i].name));
                rcf.Entries.Add(new RcfEntry
                {
                    Index = i,
                    Name = names[i].name,
                    Hash = d.hash,
                    Offset = d.offset,
                    Size = d.size,
                    Timestamp = names[i].ts,
                    Alignment = names[i].align,
                    Reserved = names[i].reserved
                });
            }
            return rcf;
        }

        public byte[] ReadAllBytes(RcfEntry e)
        {
            using var fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var data = new byte[e.Size];
            fs.Position = e.Offset;
            fs.ReadExactly(data);
            return data;
        }

        /// <summary>
        /// Extrai as entradas para <paramref name="outputFolder"/>. Quando todas as entradas são extraídas,
        /// grava também o manifesto (ordem, timestamps, hashes) usado para recompilar de forma idêntica.
        /// </summary>
        public void Extract(IReadOnlyCollection<RcfEntry> entries, string outputFolder,
            IProgress<RcfProgress>? progress, CancellationToken ct)
        {
            Directory.CreateDirectory(outputFolder);
            var buffer = new byte[1 << 20];
            long total = entries.Sum(e => (long)e.Size), done = 0;
            int n = 0;
            using var fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 20);
            foreach (var e in entries.OrderBy(x => x.Offset))
            {
                ct.ThrowIfCancellationRequested();
                string dest = Path.Combine(outputFolder, SafeRelativePath(e.Name));
                Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
                using (var output = new FileStream(dest, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    fs.Position = e.Offset;
                    StreamUtil.Copy(fs, output, e.Size, buffer, ct);
                }
                File.SetLastWriteTimeUtc(dest, DateTimeOffset.FromUnixTimeSeconds(e.Timestamp).UtcDateTime);
                done += e.Size;
                progress?.Report(new RcfProgress(++n, entries.Count, done, total, e.Name));
            }

            if (entries.Count == Entries.Count)
                RcfManifest.FromArchive(this).Save(Path.Combine(outputFolder, RcfManifest.FileName));
        }

        /// <summary>Remove letra de unidade, "..", caracteres inválidos etc. para extrair com segurança.</summary>
        public static string SafeRelativePath(string archivePath)
        {
            string p = RcfFormat.NormalizeName(archivePath);
            int colon = p.IndexOf(':');
            if (colon >= 0) p = p[(colon + 1)..];
            var invalid = Path.GetInvalidFileNameChars();
            var parts = p.Split('\\', StringSplitOptions.RemoveEmptyEntries)
                         .Where(s => s != "." && s != "..")
                         .Select(s => string.Concat(s.Select(c => invalid.Contains(c) ? '_' : c)));
            return string.Join(Path.DirectorySeparatorChar, parts);
        }
    }

    public readonly record struct RcfProgress(int Files, int TotalFiles, long Bytes, long TotalBytes, string Current);

    internal static class StreamUtil
    {
        public static void Copy(Stream input, Stream output, long count, byte[] buffer, CancellationToken ct)
        {
            while (count > 0)
            {
                ct.ThrowIfCancellationRequested();
                int n = input.Read(buffer, 0, (int)Math.Min(buffer.Length, count));
                if (n <= 0) throw new EndOfStreamException(L.T("Fim inesperado do arquivo."));
                output.Write(buffer, 0, n);
                count -= n;
            }
        }
    }
}
