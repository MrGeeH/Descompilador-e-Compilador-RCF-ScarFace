using System.Text;

namespace Extrator_e_Compilador_RCF.Core
{
    /// <summary>Um arquivo a ser gravado no RCF.</summary>
    public sealed class RcfBuildItem
    {
        public string Name { get; init; } = "";
        public required RcfSource Source { get; init; }
        public uint Timestamp { get; init; }
        public uint Alignment { get; init; } = RcfFormat.DefaultAlignment;
        public uint Reserved { get; init; }
        /// <summary>Grava apontando para os mesmos dados do item anterior (como no RCF original).</summary>
        public bool ShareDataWithPrevious { get; init; }
    }

    /// <summary>Monta um RCF novo a partir de uma lista de arquivos (geralmente uma pasta extraída).</summary>
    public sealed class RcfBuilder
    {
        public byte[] Version { get; set; } = (byte[])RcfFormat.DefaultVersion.Clone();
        public uint HeaderReserved { get; set; }
        public uint NameTableAlignment { get; set; } = RcfFormat.DefaultAlignment;
        public uint NameTableReserved { get; set; }
        public List<RcfBuildItem> Items { get; } = new();

        /// <summary>
        /// Prepara a compilação de uma pasta. Se houver <see cref="RcfManifest"/>, mantém a ordem e os metadados
        /// originais; arquivos novos vão para o fim e arquivos ausentes geram erro (ou aviso, se <paramref name="allowMissing"/>).
        /// </summary>
        public static RcfBuilder FromFolder(string folder, bool allowMissing, Action<string>? log)
        {
            folder = Path.GetFullPath(folder);
            var b = new RcfBuilder();
            var files = Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories)
                .Select(f => Path.GetRelativePath(folder, f))
                .Where(r => !string.Equals(r, RcfManifest.FileName, StringComparison.OrdinalIgnoreCase))
                .ToDictionary(r => RcfFormat.NormalizeName(r), r => Path.Combine(folder, r), StringComparer.OrdinalIgnoreCase);

            string manifestPath = Path.Combine(folder, RcfManifest.FileName);
            uint now = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if (File.Exists(manifestPath))
            {
                var m = RcfManifest.Load(manifestPath);
                b.Version = Convert.FromHexString(m.Version);
                b.HeaderReserved = m.HeaderReserved;
                b.NameTableAlignment = m.NameTableAlignment;
                b.NameTableReserved = m.NameTableReserved;
                int missing = 0;
                bool previousIncluded = false;
                foreach (var it in m.Entries)
                {
                    string key = RcfFormat.NormalizeName(it.Name);
                    if (!files.Remove(key, out var src))
                    {
                        missing++;
                        log?.Invoke(L.F("  ! ausente na pasta: {0}", it.Name));
                        previousIncluded = false;
                        continue;
                    }
                    bool share = it.SharesDataWithPrevious && previousIncluded &&
                                 FilesEqual(src, ((FileSource)b.Items[^1].Source).Path);
                    b.Items.Add(new RcfBuildItem
                    {
                        Name = it.Name,
                        Source = new FileSource(src),
                        Timestamp = it.Timestamp,
                        Alignment = it.Alignment,
                        Reserved = it.Reserved,
                        ShareDataWithPrevious = share
                    });
                    previousIncluded = true;
                }
                if (missing > 0 && !allowMissing)
                    throw new FileNotFoundException(L.F("{0} arquivo(s) do manifesto não estão na pasta (veja o log).", missing));
                log?.Invoke(missing > 0
                    ? L.F("Manifesto: {0} arquivo(s) na ordem original, {1} removido(s).", m.Entries.Count - missing, missing)
                    : L.F("Manifesto: {0} arquivo(s) na ordem original.", m.Entries.Count));
            }
            else
            {
                log?.Invoke(L.F("Sem {0}: arquivos em ordem alfabética, data atual.", RcfManifest.FileName));
            }

            foreach (var kv in files.OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase))
            {
                b.Items.Add(new RcfBuildItem { Name = kv.Key, Source = new FileSource(kv.Value), Timestamp = now });
                if (File.Exists(manifestPath)) log?.Invoke(L.F("  + novo arquivo: {0}", kv.Key));
            }
            return b;
        }

        /// <summary>
        /// Prepara a recompilação de um RCF aberto: entradas não substituídas são copiadas do original,
        /// <paramref name="replacements"/> (chave = <see cref="RcfEntry.Index"/>) trocam o conteúdo e
        /// <paramref name="additions"/> entram no fim como arquivos novos.
        /// </summary>
        public static RcfBuilder FromArchive(RcfArchive rcf, IReadOnlyDictionary<int, RcfSource> replacements,
            IEnumerable<(string name, RcfSource source)>? additions = null)
        {
            var b = new RcfBuilder
            {
                Version = (byte[])rcf.Version.Clone(),
                HeaderReserved = rcf.HeaderReserved,
                NameTableAlignment = rcf.NameTableAlignment,
                NameTableReserved = rcf.NameTableReserved
            };
            RcfEntry? prev = null;
            foreach (var e in rcf.Entries)
            {
                bool replaced = replacements.TryGetValue(e.Index, out var src);
                bool share = !replaced && prev != null && !replacements.ContainsKey(prev.Index) &&
                             prev.Offset == e.Offset && prev.Size == e.Size;
                b.Items.Add(new RcfBuildItem
                {
                    Name = e.Name,
                    Source = src ?? new ArchiveSource(rcf.FilePath, e.Offset, e.Size),
                    Timestamp = replaced ? (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds() : e.Timestamp,
                    Alignment = e.Alignment,
                    Reserved = e.Reserved,
                    ShareDataWithPrevious = share
                });
                prev = e;
            }
            uint now = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            foreach (var (name, source) in additions ?? [])
                b.Items.Add(new RcfBuildItem { Name = RcfFormat.NormalizeName(name), Source = source, Timestamp = now });
            return b;
        }

        public void Build(string targetPath, IProgress<RcfProgress>? progress, Action<string>? log, CancellationToken ct)
        {
            if (Items.Count == 0) throw new InvalidOperationException(L.T("Nenhum arquivo para compilar."));

            // Hashes únicos são obrigatórios: o jogo localiza arquivos só pelo hash.
            var hashes = new Dictionary<uint, string>();
            foreach (var it in Items)
            {
                if (Encoding.Latin1.GetByteCount(it.Name) != it.Name.Length || it.Name.Any(c => c > 0xFF))
                    throw new InvalidDataException(L.F("Nome com caracteres não suportados: {0}", it.Name));
                uint h = RcfFormat.NameHash(it.Name);
                if (!hashes.TryAdd(h, it.Name))
                    throw new InvalidDataException(hashes[h].Equals(it.Name, StringComparison.OrdinalIgnoreCase)
                        ? L.F("Arquivo duplicado: {0}", it.Name)
                        : L.F("Colisão de hash {0:X8} entre '{1}' e '{2}'. Renomeie um deles.", h, hashes[h], it.Name));
            }

            byte[][] nameBytes = Items.Select(i => Encoding.Latin1.GetBytes(i.Name)).ToArray();
            long dirOffset = RcfFormat.HeaderSize;
            long dirSize = (long)Items.Count * RcfFormat.DirEntrySize;
            long nameOffset = RcfFormat.Align(dirOffset + dirSize, RcfFormat.DefaultAlignment);
            long nameSize = 8 + nameBytes.Sum(n => 16L + n.Length + 4);
            long dataStart = RcfFormat.Align(nameOffset + nameSize, RcfFormat.DefaultAlignment);

            var sizes = Items.Select(i => i.Source.Length).ToArray();
            long total = sizes.Sum();
            var offsets = new uint[Items.Count];
            var buffer = new byte[4 << 20];
            string tempPath = targetPath + ".tmp";

            try
            {
                using (var output = new FileStream(tempPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 1 << 20))
                using (var bw = new BinaryWriter(output, Encoding.Latin1, leaveOpen: true))
                {
                    // Tabela de nomes
                    output.Position = nameOffset;
                    bw.Write(NameTableAlignment);
                    bw.Write(NameTableReserved);
                    for (int i = 0; i < Items.Count; i++)
                    {
                        bw.Write(Items[i].Timestamp);
                        bw.Write(Items[i].Alignment);
                        bw.Write(Items[i].Reserved);
                        bw.Write(nameBytes[i].Length + 1);
                        bw.Write(nameBytes[i]);
                        bw.Write(0u); // \0 + 3 bytes de preenchimento
                    }
                    bw.Flush();

                    // Dados
                    long pos = dataStart, done = 0;
                    for (int i = 0; i < Items.Count; i++)
                    {
                        ct.ThrowIfCancellationRequested();
                        var it = Items[i];
                        if (it.ShareDataWithPrevious && i > 0 && sizes[i] == sizes[i - 1])
                        {
                            offsets[i] = offsets[i - 1];
                        }
                        else
                        {
                            pos = RcfFormat.Align(pos, Math.Max(it.Alignment, 1u));
                            if (pos + sizes[i] > uint.MaxValue)
                                throw new IOException(L.T("O RCF passaria de 4 GB — limite do formato."));
                            output.Position = pos;
                            offsets[i] = (uint)pos;
                            it.Source.CopyTo(output, buffer, ct);
                            if (output.Position != pos + sizes[i])
                                throw new IOException(L.F("'{0}' mudou de tamanho durante a compilação.", it.Name));
                            pos += sizes[i];
                        }
                        done += sizes[i];
                        progress?.Report(new RcfProgress(i + 1, Items.Count, done, total, it.Name));
                    }
                    output.SetLength(Math.Max(pos, dataStart));

                    // Cabeçalho
                    output.Position = 0;
                    var magic = new byte[32];
                    Encoding.ASCII.GetBytes(RcfFormat.Magic).CopyTo(magic, 0);
                    bw.Write(magic);
                    bw.Write(Version);
                    bw.Write((uint)dirOffset);
                    bw.Write((uint)dirSize);
                    bw.Write((uint)nameOffset);
                    bw.Write((uint)nameSize);
                    bw.Write(HeaderReserved);
                    bw.Write((uint)Items.Count);

                    // Diretório ordenado por hash
                    foreach (int i in Enumerable.Range(0, Items.Count).OrderBy(i => RcfFormat.NameHash(Items[i].Name)))
                    {
                        bw.Write(RcfFormat.NameHash(Items[i].Name));
                        bw.Write(offsets[i]);
                        bw.Write((uint)sizes[i]);
                    }
                    bw.Flush();
                    output.Flush(true);
                }
                File.Move(tempPath, targetPath, overwrite: true);
            }
            catch
            {
                try { File.Delete(tempPath); } catch { /* melhor esforço */ }
                throw;
            }
            log?.Invoke(L.F("RCF gerado: {0} ({1} arquivos, {2:N0} bytes).", targetPath, Items.Count, new FileInfo(targetPath).Length));
        }

        private static bool FilesEqual(string a, string b)
        {
            if (string.Equals(Path.GetFullPath(a), Path.GetFullPath(b), StringComparison.OrdinalIgnoreCase)) return true;
            using var fa = File.OpenRead(a);
            using var fb = File.OpenRead(b);
            if (fa.Length != fb.Length) return false;
            var ba = new byte[1 << 16];
            var bb = new byte[1 << 16];
            int n;
            while ((n = fa.Read(ba, 0, ba.Length)) > 0)
            {
                fb.ReadExactly(bb, 0, n);
                if (!ba.AsSpan(0, n).SequenceEqual(bb.AsSpan(0, n))) return false;
            }
            return true;
        }
    }
}
