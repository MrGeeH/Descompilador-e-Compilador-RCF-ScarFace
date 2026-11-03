using Extrator_e_Compilador_RCF.UI;

namespace Extrator_e_Compilador_RCF
{
    /// <summary>Configurações: fundo (Mica/Acrílico/Sólido), opacidade e idioma. Aplica na hora.</summary>
    public partial class ConfiguracoesForm : ThemedForm
    {
        private bool _loading;

        public ConfiguracoesForm()
        {
            InitializeComponent();
            if (DesignMode) return;

            rbMica.Enabled = rbAcrylic.Enabled = Native.SupportsBackdrop;
            LoadValues();
        }

        protected override void OnAppearanceChanged()
        {
            lblBackdropNote.Text = L.T(Native.SupportsBackdrop
                ? "O efeito aparece no cabeçalho, no rodapé e na barra de título."
                : "Mica e Acrílico exigem Windows 11 (22H2 ou superior).");
            lblCredits.Text = L.T("Ferramenta criada por MrGeH.");
        }

        private void LoadValues()
        {
            _loading = true;
            var s = AppSettings.Current;
            rbMica.Checked = s.Backdrop == BackdropMode.Mica;
            rbAcrylic.Checked = s.Backdrop == BackdropMode.Acrylic;
            rbSolid.Checked = s.Backdrop == BackdropMode.Solid || !Native.SupportsBackdrop;
            trackOpacity.Value = Math.Clamp(s.Opacity, trackOpacity.Minimum, trackOpacity.Maximum);
            lblOpacity.Text = $"{trackOpacity.Value}%";
            rbPortuguese.Checked = s.Language == AppLanguage.PtBr;
            rbEnglish.Checked = s.Language == AppLanguage.EnUs;
            _loading = false;
        }

        private void Commit()
        {
            AppSettings.Current.Commit();
            LoadValues();
        }

        private void backdrop_CheckedChanged(object? sender, EventArgs e)
        {
            if (_loading || sender is not RadioButton { Checked: true } rb) return;
            AppSettings.Current.Backdrop = rb == rbMica ? BackdropMode.Mica : rb == rbAcrylic ? BackdropMode.Acrylic : BackdropMode.Solid;
            Commit();
        }

        private void language_CheckedChanged(object? sender, EventArgs e)
        {
            if (_loading || sender is not RadioButton { Checked: true } rb) return;
            AppSettings.Current.Language = rb == rbEnglish ? AppLanguage.EnUs : AppLanguage.PtBr;
            Commit();
        }

        private void trackOpacity_Scroll(object? sender, EventArgs e)
        {
            if (_loading) return;
            AppSettings.Current.Opacity = trackOpacity.Value;
            Commit();
        }

        private void btnReset_Click(object? sender, EventArgs e)
        {
            var s = AppSettings.Current;
            s.Backdrop = BackdropMode.Mica;
            s.Opacity = 100;
            Commit();
        }

        private void btnClose_Click(object? sender, EventArgs e) => Close();
    }
}
