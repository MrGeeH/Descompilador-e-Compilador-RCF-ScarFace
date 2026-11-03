using System.Text.Json;
using System.Text.Json.Serialization;

namespace Extrator_e_Compilador_RCF.Core
{
    /// <summary>
    /// Guarda, junto dos arquivos extraídos, tudo que não dá para deduzir só da pasta:
    /// ordem original dos dados, timestamps, campos reservados e entradas que compartilham dados.
    /// Com ele, extrair e recompilar sem mudanças gera um RCF idêntico byte a byte ao original.
    /// </summary>
    public sealed class RcfManifest
    {
        public const string FileName = "_rcf_manifest.json";

        public string SourceFile { get; set; } = "";
        public string Version { get; set; } = Convert.ToHexString(RcfFormat.DefaultVersion);
        public uint HeaderReserved { get; set; }
        public uint NameTableAlignment { get; set; } = RcfFormat.DefaultAlignment;
        public uint NameTableReserved { get; set; }
        public List<Item> Entries { get; set; } = new();

        public sealed class Item
        {
            public string Name { get; set; } = "";
            public uint Timestamp { get; set; }
            public uint Alignment { get; set; } = RcfFormat.DefaultAlignment;
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
            public uint Reserved { get; set; }
            /// <summary>Hash original (só informativo — o compilador sempre recalcula).</summary>
            public string Hash { get; set; } = "";
            /// <summary>No original, esta entrada apontava para os mesmos dados da anterior.</summary>
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
            public bool SharesDataWithPrevious { get; set; }
        }

        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public static RcfManifest FromArchive(RcfArchive rcf)
        {
            var m = new RcfManifest
            {
                SourceFile = Path.GetFileName(rcf.FilePath),
                Version = Convert.ToHexString(rcf.Version),
                HeaderReserved = rcf.HeaderReserved,
                NameTableAlignment = rcf.NameTableAlignment,
                NameTableReserved = rcf.NameTableReserved
            };
            RcfEntry? prev = null;
            foreach (var e in rcf.Entries)
            {
                m.Entries.Add(new Item
                {
                    Name = e.Name,
                    Timestamp = e.Timestamp,
                    Alignment = e.Alignment,
                    Reserved = e.Reserved,
                    Hash = e.Hash.ToString("X8"),
                    SharesDataWithPrevious = prev != null && prev.Offset == e.Offset && prev.Size == e.Size
                });
                prev = e;
            }
            return m;
        }

        public void Save(string path) => File.WriteAllText(path, JsonSerializer.Serialize(this, Options));

        public static RcfManifest Load(string path) =>
            JsonSerializer.Deserialize<RcfManifest>(File.ReadAllText(path), Options)
            ?? throw new InvalidDataException("Manifesto vazio ou inválido: " + path);
    }
}
