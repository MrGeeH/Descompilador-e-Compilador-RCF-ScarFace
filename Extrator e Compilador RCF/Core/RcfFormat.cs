namespace Extrator_e_Compilador_RCF.Core
{
    /// <summary>
    /// Formato RCF ("ATG CORE CEMENT LIBRARY") do Scarface: The World Is Yours (PC), little-endian.
    ///
    ///   0x00  char[32]  "ATG CORE CEMENT LIBRARY" + zeros
    ///   0x20  byte[4]   versão (02 01 00 01 no PC; o 3º byte = 0 indica little-endian)
    ///   0x24  uint      offset do diretório (sempre 0x3C)
    ///   0x28  uint      tamanho do diretório (= qtd × 12)
    ///   0x2C  uint      offset da tabela de nomes (diretório alinhado a 0x800)
    ///   0x30  uint      tamanho da tabela de nomes
    ///   0x34  uint      reservado (0)
    ///   0x38  uint      quantidade de arquivos
    ///
    ///   Diretório: qtd × { uint hash, uint offset, uint size }, ordenado por hash (o jogo faz busca binária).
    ///   Tabela de nomes: uint alinhamento (0x800), uint 0, depois qtd × {
    ///       uint timestamp (unix), uint alinhamento (0x800), uint 0, uint len (com o \0), char[len], byte[3] zeros }
    ///   na mesma ordem em que os dados estão gravados (offset crescente).
    ///   Dados: cada arquivo começa alinhado a 0x800; o último não recebe padding no fim do arquivo.
    /// </summary>
    public static class RcfFormat
    {
        public const string Magic = "ATG CORE CEMENT LIBRARY";
        public const int HeaderSize = 0x3C;
        public const int DirEntrySize = 12;
        public const uint DefaultAlignment = 0x800;
        public static readonly byte[] DefaultVersion = { 0x02, 0x01, 0x00, 0x01 };

        /// <summary>
        /// Hash de nome usado no diretório. Confirmado em 100% das entradas do cement.rcf e dos snd_*.rcf:
        /// h = 0; para cada caractere: h = h * 31 + (MAIÚSCULA(c) + 0x20).
        /// </summary>
        public static uint NameHash(string name)
        {
            uint h = 0;
            foreach (char ch in name)
            {
                uint c = (byte)ch;
                if (c >= 'a' && c <= 'z') c -= 0x20;
                h = unchecked(h * 31u + c + 0x20u);
            }
            return h;
        }

        /// <summary>Normaliza um caminho para o padrão do RCF: separador "\" e sem barra inicial.</summary>
        public static string NormalizeName(string path) => path.Replace('/', '\\').TrimStart('\\');

        public static long Align(long value, long alignment) =>
            alignment <= 1 ? value : (value + alignment - 1) / alignment * alignment;
    }
}
