using System.Text.Json;

namespace Extrator_e_Compilador_RCF.UI
{
    public enum BackdropMode { Mica, Acrylic, Solid }

    /// <summary>Preferências de aparência, salvas em %AppData%\ExtratorCompiladorRCF\aparencia.json.</summary>
    internal sealed class AppSettings
    {
        public BackdropMode Backdrop { get; set; } = BackdropMode.Mica;
        /// <summary>Opacidade da janela em % (60–100).</summary>
        public int Opacity { get; set; } = 100;
        public AppLanguage Language { get; set; } = AppLanguage.PtBr;

        public static AppSettings Current { get; private set; } = Load();

        /// <summary>Disparado quando a aparência muda (as janelas abertas se reaplicam).</summary>
        public static event Action? Changed;

        private static string FilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ExtratorCompiladorRCF", "aparencia.json");

        private static AppSettings Load()
        {
            AppSettings s;
            try { s = File.Exists(FilePath) ? JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath)) ?? new() : new(); }
            catch { s = new(); }
            s.Opacity = Math.Clamp(s.Opacity, 60, 100);
            L.Set(s.Language);
            return s;
        }

        /// <summary>Salva e avisa as janelas abertas.</summary>
        public void Commit()
        {
            L.Set(Language);
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
                File.WriteAllText(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch { /* preferências são opcionais */ }
            Changed?.Invoke();
        }
    }
}
