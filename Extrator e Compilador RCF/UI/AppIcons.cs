namespace Extrator_e_Compilador_RCF.UI
{
    /// <summary>Ícone e logo do programa (Logo\logo.ico e Logo\logo.png embutidos no executável).</summary>
    internal static class AppIcons
    {
        private static Icon? _icon;
        private static Image? _logo;

        public static Icon? Icon => _icon ??= Load(s => new Icon(s));
        public static Image? Logo => _logo ??= Load(Image.FromStream);

        private static T? Load<T>(Func<Stream, T> create) where T : class
        {
            string name = typeof(T) == typeof(Icon) ? "Logo.logo.ico" : "Logo.logo.png";
            try
            {
                using var s = typeof(AppIcons).Assembly.GetManifestResourceStream(name);
                if (s == null) return null;
                var ms = new MemoryStream();   // Image.FromStream exige o stream aberto enquanto a imagem existir
                s.CopyTo(ms);
                ms.Position = 0;
                return create(ms);
            }
            catch { return null; }
        }
    }
}
