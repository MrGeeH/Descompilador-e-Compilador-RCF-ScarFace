namespace Extrator_e_Compilador_RCF
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {
            // Um único .rcf como argumento ("Abrir com…" / arrastar no .exe) abre a interface com ele.
            string? open = args.Length == 1 && File.Exists(args[0]) &&
                           args[0].EndsWith(".rcf", StringComparison.OrdinalIgnoreCase) ? args[0] : null;
            if (args.Length > 0 && open == null)
                return Cli.Run(args);

            ApplicationConfiguration.Initialize();
#pragma warning disable WFO5001 // modo escuro dos controles nativos (scrollbars, diálogos) — recurso marcado como experimental
            Application.SetColorMode(SystemColorMode.Dark);
#pragma warning restore WFO5001
            Application.Run(new MainForm(open));
            return 0;
        }
    }
}
