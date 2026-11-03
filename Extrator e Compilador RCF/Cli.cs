using System.Runtime.InteropServices;
using Extrator_e_Compilador_RCF.Core;

namespace Extrator_e_Compilador_RCF
{
    /// <summary>Modo linha de comando (para scripts/lotes). Sem argumentos o programa abre a interface.</summary>
    internal static class Cli
    {
        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int processId);

        public static int Run(string[] args)
        {
            if (!Console.IsOutputRedirected) AttachConsole(-1);
            try
            {
                switch (args[0].ToLowerInvariant())
                {
                    case "list" when args.Length == 2:
                        var rcf = RcfArchive.Open(args[1]);
                        foreach (var e in rcf.Entries)
                            Console.WriteLine($"{e.Hash:X8} {e.Offset,12} {e.Size,12}  {e.Name}");
                        Console.WriteLine($"{rcf.Entries.Count} arquivo(s). Hashes verificados: {(rcf.HashesVerified ? "sim" : "NÃO")}");
                        return 0;

                    case "extract" when args.Length == 3:
                        var src = RcfArchive.Open(args[1]);
                        src.Extract(src.Entries, args[2], null, CancellationToken.None);
                        Console.WriteLine($"{src.Entries.Count} arquivo(s) extraído(s) em {args[2]}");
                        return 0;

                    case "build" when args.Length >= 3:
                        bool allowMissing = args.Skip(3).Contains("--permitir-ausentes");
                        RcfBuilder.FromFolder(args[1], allowMissing, Console.WriteLine)
                                  .Build(args[2], null, Console.WriteLine, CancellationToken.None);
                        return 0;

                    default:
                        Console.WriteLine("""
                            Uso:
                              "Extrator e Compilador RCF.exe" list    <arquivo.rcf>
                              "Extrator e Compilador RCF.exe" extract <arquivo.rcf> <pasta_destino>
                              "Extrator e Compilador RCF.exe" build   <pasta> <novo.rcf> [--permitir-ausentes]
                            """);
                        return 2;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Erro: " + ex.Message);
                return 1;
            }
        }
    }
}
