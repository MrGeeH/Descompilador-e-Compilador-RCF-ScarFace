using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace Extrator_e_Compilador_RCF.UI
{
    /// <summary>Paleta, fontes e utilitários de desenho do tema escuro.</summary>
    internal static class Theme
    {
        public static readonly Color Bg = Hex(0x111114);
        public static readonly Color Surface = Hex(0x1A1A1F);
        public static readonly Color Surface2 = Hex(0x24242B);
        public static readonly Color SurfaceHover = Hex(0x2C2C35);
        public static readonly Color Border = Hex(0x2E2E38);
        public static readonly Color Text = Hex(0xEDEDF2);
        public static readonly Color Muted = Hex(0x9C9CA8);
        public static readonly Color Faint = Hex(0x6C6C78);
        public static readonly Color Blue = Hex(0x5B9DFF);
        public static readonly Color Green = Hex(0x3DD68C);
        public static readonly Color Red = Hex(0xFF6B6B);
        public static readonly Color Folder = Hex(0xE8B44C);

        /// <summary>Cor de destaque do app (vermelho, fixa).</summary>
        public static readonly Color Accent = Hex(0xF0524D);
        public static readonly Color AccentHover = Blend(Accent, Color.White, 0.14f);
        public static readonly Color AccentPressed = Blend(Accent, Color.Black, 0.18f);
        public static readonly Color AccentText = Color.White;

        /// <summary>true quando cabeçalho/rodapé estão sobre Mica/Acrílico (desenho com alfa real).</summary>
        public static bool GlassActive { get; set; }

        // Fontes
        private static readonly string UiFamily = FontExists("Segoe UI Variable Text") ? "Segoe UI Variable Text" : "Segoe UI";
        private static readonly string DisplayFamily = FontExists("Segoe UI Variable Display") ? "Segoe UI Variable Display" : "Segoe UI";
        public static readonly string IconFamily = FontExists("Segoe Fluent Icons") ? "Segoe Fluent Icons" : "Segoe MDL2 Assets";
        public static readonly Font Ui = new(UiFamily, 9.5f);
        private static readonly bool HasSemibold = FontExists("Segoe UI Semibold");
        public static readonly Font UiSemibold = new(HasSemibold ? "Segoe UI Semibold" : UiFamily, 9.5f, HasSemibold ? FontStyle.Regular : FontStyle.Bold);
        public static readonly Font Small = new(UiFamily, 8.25f);
        public static readonly Font SmallBold = new(UiFamily, 7.5f, FontStyle.Bold);
        public static readonly Font Title = new(DisplayFamily, 13f, FontStyle.Bold);
        public static readonly Font CardTitle = new(DisplayFamily, 10f, FontStyle.Bold);
        public static readonly Font Mono = new(FontExists("Cascadia Mono") ? "Cascadia Mono" : "Consolas", 8.5f);
        private static readonly Dictionary<float, Font> IconFonts = new();

        public static Font Icon(float size)
        {
            if (!IconFonts.TryGetValue(size, out var f)) IconFonts[size] = f = new Font(IconFamily, size);
            return f;
        }

        private static bool FontExists(string name)
        {
            using var fonts = new InstalledFontCollection();
            return fonts.Families.Any(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public static Color Hex(int rgb) => Color.FromArgb(255, (rgb >> 16) & 0xFF, (rgb >> 8) & 0xFF, rgb & 0xFF);
        public static Color Alpha(Color c, int a) => Color.FromArgb(Math.Clamp(a, 0, 255), c.R, c.G, c.B);

        public static Color Blend(Color a, Color b, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return Color.FromArgb(
                (int)(a.A + (b.A - a.A) * t), (int)(a.R + (b.R - a.R) * t),
                (int)(a.G + (b.G - a.G) * t), (int)(a.B + (b.B - a.B) * t));
        }

        public static GraphicsPath Round(RectangleF r, float radius)
        {
            var p = new GraphicsPath();
            float d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
            if (d <= 0.5f) { p.AddRectangle(r); return p; }
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        public static void FillRound(Graphics g, Color c, RectangleF r, float radius)
        {
            using var path = Round(r, radius);
            using var b = new SolidBrush(c);
            g.FillPath(b, path);
        }

        public static void DrawRound(Graphics g, Color c, RectangleF r, float radius, float width = 1f)
        {
            using var path = Round(r, radius);
            using var p = new Pen(c, width);
            g.DrawPath(p, path);
        }

        /// <summary>Desenha um ícone (glifo da fonte Segoe Fluent/MDL2) centralizado no retângulo.</summary>
        public static void Glyph(Graphics g, string glyph, Color c, RectangleF r, float size)
        {
            using var b = new SolidBrush(c);
            using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            var hint = g.TextRenderingHint;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            g.DrawString(glyph, Icon(size), b, r, sf);
            g.TextRenderingHint = hint;
        }

        /// <summary>Ícone como bitmap (para itens de menu).</summary>
        public static Bitmap GlyphBitmap(string glyph, Color c, int px)
        {
            var bmp = new Bitmap(px, px);
            using var g = Graphics.FromImage(bmp);
            Glyph(g, glyph, c, new RectangleF(0, 0, px, px), px * 0.5f);
            return bmp;
        }

        /// <summary>Tile xadrez para mostrar transparência das imagens.</summary>
        public static Bitmap Checker(int cell)
        {
            var bmp = new Bitmap(cell * 2, cell * 2);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Hex(0x26262D));
            using var b = new SolidBrush(Hex(0x1E1E24));
            g.FillRectangle(b, 0, 0, cell, cell);
            g.FillRectangle(b, cell, cell, cell, cell);
            return bmp;
        }

        public static int Dpi(this Control c, int px) => (int)Math.Round(px * c.DeviceDpi / 96f);
    }

    /// <summary>Glifos da fonte Segoe Fluent Icons / MDL2 Assets.</summary>
    internal static class Glyphs
    {
        public const string Open = "", Save = "", SaveAs = "", Export = "", Import = "",
            Picture = "", Build = "", Help = "", Palette = "", ChevronDown = "",
            ChevronRight = "", Search = "", Folder = "", FolderOpen = "", Page = "",
            Package = "", Code = "", Audio = "", Undo = "", Cancel = "", Add = "",
            Up = "", Copy = "", Check = "", History = "", Text = "", Swap = "",
            Settings = "", Info = "";
    }
}
