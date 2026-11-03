using System.Runtime.InteropServices;

namespace Extrator_e_Compilador_RCF.UI
{
    /// <summary>Interop com DWM/UxTheme/GDI para tema escuro, Mica/Acrílico e desenho com transparência.</summary>
    internal static class Native
    {
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        private const int DWMWA_BORDER_COLOR = 34;
        private const int DWMWA_CAPTION_COLOR = 35;
        private const int DWMWA_SYSTEMBACKDROP_TYPE = 38;
        private const uint DWMWA_COLOR_DEFAULT = 0xFFFFFFFF;

        public enum Backdrop { None = 1, Mica = 2, Acrylic = 3, Tabbed = 4 }
        public enum Corner { Default = 0, DoNotRound = 1, Round = 2, RoundSmall = 3 }

        [StructLayout(LayoutKind.Sequential)]
        private struct MARGINS { public int Left, Right, Top, Bottom; }

        [DllImport("dwmapi.dll")] private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);
        [DllImport("dwmapi.dll")] private static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);
        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)] public static extern int SetWindowTheme(IntPtr hwnd, string? app, string? idList);
        [DllImport("user32.dll")] public static extern IntPtr SendMessage(IntPtr hwnd, int msg, IntPtr w, IntPtr l);
        [DllImport("gdi32.dll")] private static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        [DllImport("gdi32.dll")] private static extern IntPtr SelectObject(IntPtr hdc, IntPtr obj);
        [DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr obj);
        [DllImport("gdi32.dll")] private static extern bool DeleteDC(IntPtr hdc);
        [DllImport("gdi32.dll")] private static extern bool BitBlt(IntPtr dst, int x, int y, int w, int h, IntPtr src, int sx, int sy, int rop);

        private static int Build => Environment.OSVersion.Version.Build;
        /// <summary>Mica/Acrílico via DWMWA_SYSTEMBACKDROP_TYPE: Windows 11 22H2+.</summary>
        public static bool SupportsBackdrop => OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22621);
        public static bool SupportsWin11Frame => OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000);

        private static void Set(IntPtr hwnd, int attr, int value) => DwmSetWindowAttribute(hwnd, attr, ref value, sizeof(int));
        private static int ColorRef(Color c) => c.R | (c.G << 8) | (c.B << 16);

        public static void UseDarkTitleBar(IntPtr hwnd) { if (Build >= 17763) Set(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, 1); }
        public static void SetCorner(IntPtr hwnd, Corner c) { if (SupportsWin11Frame) Set(hwnd, DWMWA_WINDOW_CORNER_PREFERENCE, (int)c); }
        public static void SetBackdrop(IntPtr hwnd, Backdrop b) { if (SupportsBackdrop) Set(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, (int)b); }
        public static void SetCaptionColor(IntPtr hwnd, Color? c) { if (SupportsWin11Frame) Set(hwnd, DWMWA_CAPTION_COLOR, c is { } v ? ColorRef(v) : unchecked((int)DWMWA_COLOR_DEFAULT)); }
        public static void SetBorderColor(IntPtr hwnd, Color? c) { if (SupportsWin11Frame) Set(hwnd, DWMWA_BORDER_COLOR, c is { } v ? ColorRef(v) : unchecked((int)DWMWA_COLOR_DEFAULT)); }

        /// <summary>Estende o fundo do sistema (Mica/Acrílico) para dentro da janela, no topo e na base.</summary>
        public static void ExtendFrame(IntPtr hwnd, int top, int bottom)
        {
            var m = new MARGINS { Top = top, Bottom = bottom };
            DwmExtendFrameIntoClientArea(hwnd, ref m);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int Left, Top, Right, Bottom; }

        [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr hwnd, out RECT r);
        [DllImport("user32.dll")] private static extern bool IsWindowVisible(IntPtr hwnd);

        /// <summary>Altura do cabeçalho de colunas de um ListView (0 se não houver).</summary>
        public static int ListViewHeaderHeight(ListView list)
        {
            IntPtr header = SendMessage(list.Handle, 0x101F /* LVM_GETHEADER */, IntPtr.Zero, IntPtr.Zero);
            return header != IntPtr.Zero && IsWindowVisible(header) && GetWindowRect(header, out var r) ? r.Bottom - r.Top : 0;
        }

        /// <summary>Aplica scrollbars/cabeçalhos escuros do Windows a um controle nativo.</summary>
        public static void DarkControl(Control c, string theme = "DarkMode_Explorer")
        {
            void apply()
            {
                SetWindowTheme(c.Handle, theme, null);
                if (c is ListView)
                {
                    IntPtr header = SendMessage(c.Handle, 0x101F /* LVM_GETHEADER */, IntPtr.Zero, IntPtr.Zero);
                    if (header != IntPtr.Zero) SetWindowTheme(header, "DarkMode_ItemsView", null);
                }
            }
            if (c.IsHandleCreated) apply();
            c.HandleCreated += (_, _) => apply();
        }

        /// <summary>
        /// Copia um bitmap PARGB para o HDC preservando o canal alfa (necessário para desenhar sobre Mica/Acrílico,
        /// onde o DWM compõe usando o alfa de cada pixel).
        /// </summary>
        public static void BlitWithAlpha(Graphics target, Bitmap bmp)
        {
            IntPtr hdc = target.GetHdc();
            IntPtr hbmp = bmp.GetHbitmap(Color.FromArgb(0));
            IntPtr mem = CreateCompatibleDC(hdc);
            IntPtr old = SelectObject(mem, hbmp);
            try { BitBlt(hdc, 0, 0, bmp.Width, bmp.Height, mem, 0, 0, 0x00CC0020 /* SRCCOPY */); }
            finally
            {
                SelectObject(mem, old);
                DeleteDC(mem);
                DeleteObject(hbmp);
                target.ReleaseHdc(hdc);
            }
        }
    }
}
