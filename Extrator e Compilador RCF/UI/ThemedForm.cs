using System.ComponentModel;

namespace Extrator_e_Compilador_RCF.UI
{
    /// <summary>
    /// Base das janelas do app: barra de título escura, cantos arredondados e fundo Mica/Acrílico (Windows 11).
    /// Janelas que têm cabeçalho/rodapé de vidro informam a altura em <see cref="GlassTop"/>/<see cref="GlassBottom"/>.
    /// </summary>
    public class ThemedForm : Form
    {
        public ThemedForm()
        {
            BackColor = Theme.Bg;
            ForeColor = Theme.Text;
            if (AppIcons.Icon is { } icon) Icon = icon;
            if (!DesignMode) AppSettings.Changed += OnSettingsChanged;
        }

        /// <summary>Altura (px) da área de vidro no topo da janela.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected virtual int GlassTop => 0;

        /// <summary>Altura (px) da área de vidro na base da janela.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected virtual int GlassBottom => 0;

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (DesignMode) return;
            Native.UseDarkTitleBar(Handle);
            Native.SetCorner(Handle, Native.Corner.Round);
            L.Apply(this);
            ApplyAppearance();
        }

        private void OnSettingsChanged()
        {
            if (!IsHandleCreated || IsDisposed) return;
            L.Apply(this);
            ApplyAppearance();
        }

        /// <summary>Reaplica fundo, transparência e cores a partir de <see cref="AppSettings.Current"/>.</summary>
        protected void ApplyAppearance()
        {
            var s = AppSettings.Current;
            bool backdrop = s.Backdrop != BackdropMode.Solid && Native.SupportsBackdrop;
            Native.SetBackdrop(Handle, backdrop ? (s.Backdrop == BackdropMode.Acrylic ? Native.Backdrop.Acrylic : Native.Backdrop.Mica) : Native.Backdrop.None);
            Native.SetCaptionColor(Handle, backdrop ? null : Theme.Bg);
            Native.SetBorderColor(Handle, Theme.Alpha(Theme.Accent, 255));
            bool glass = backdrop && (GlassTop > 0 || GlassBottom > 0);
            Native.ExtendFrame(Handle, glass ? GlassTop : 0, glass ? GlassBottom : 0);
            Theme.GlassActive = backdrop;
            Opacity = s.Opacity / 100.0;
            OnAppearanceChanged();
            Invalidate(true);
        }

        /// <summary>Ponto de extensão para janelas que precisam redesenhar algo ao mudar o tema ou o idioma.</summary>
        protected virtual void OnAppearanceChanged() { }

        protected override void Dispose(bool disposing)
        {
            if (disposing) AppSettings.Changed -= OnSettingsChanged;
            base.Dispose(disposing);
        }
    }
}
