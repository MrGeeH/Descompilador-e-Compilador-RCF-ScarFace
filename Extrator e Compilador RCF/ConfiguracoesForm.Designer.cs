namespace Extrator_e_Compilador_RCF
{
    partial class ConfiguracoesForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            cardBackdrop = new Extrator_e_Compilador_RCF.UI.CardPanel();
            lblBackdropNote = new Label();
            rbSolid = new Extrator_e_Compilador_RCF.UI.ModernRadio();
            rbAcrylic = new Extrator_e_Compilador_RCF.UI.ModernRadio();
            rbMica = new Extrator_e_Compilador_RCF.UI.ModernRadio();
            cardOpacity = new Extrator_e_Compilador_RCF.UI.CardPanel();
            lblOpacity = new Label();
            trackOpacity = new Extrator_e_Compilador_RCF.UI.ModernSlider();
            cardLanguage = new Extrator_e_Compilador_RCF.UI.CardPanel();
            rbEnglish = new Extrator_e_Compilador_RCF.UI.ModernRadio();
            rbPortuguese = new Extrator_e_Compilador_RCF.UI.ModernRadio();
            cardCredits = new Extrator_e_Compilador_RCF.UI.CardPanel();
            lblCredits = new Label();
            bottomPanel = new FlowLayoutPanel();
            btnClose = new Extrator_e_Compilador_RCF.UI.ModernButton();
            btnReset = new Extrator_e_Compilador_RCF.UI.ModernButton();
            cardBackdrop.SuspendLayout();
            cardOpacity.SuspendLayout();
            cardLanguage.SuspendLayout();
            cardCredits.SuspendLayout();
            bottomPanel.SuspendLayout();
            SuspendLayout();
            // 
            // cardBackdrop
            // 
            cardBackdrop.BackColor = Color.FromArgb(26, 26, 31);
            cardBackdrop.Controls.Add(lblBackdropNote);
            cardBackdrop.Controls.Add(rbSolid);
            cardBackdrop.Controls.Add(rbAcrylic);
            cardBackdrop.Controls.Add(rbMica);
            cardBackdrop.Glyph = "";
            cardBackdrop.Location = new Point(14, 14);
            cardBackdrop.Name = "cardBackdrop";
            cardBackdrop.Size = new Size(432, 160);
            cardBackdrop.TabIndex = 0;
            cardBackdrop.Title = "Fundo da janela";
            // 
            // lblBackdropNote
            // 
            lblBackdropNote.AutoSize = true;
            lblBackdropNote.Font = new Font("Segoe UI", 8.25F);
            lblBackdropNote.ForeColor = Color.FromArgb(156, 156, 168);
            lblBackdropNote.Location = new Point(18, 128);
            lblBackdropNote.Name = "lblBackdropNote";
            lblBackdropNote.Size = new Size(0, 13);
            lblBackdropNote.TabIndex = 3;
            // 
            // rbSolid
            // 
            rbSolid.AutoSize = true;
            rbSolid.ForeColor = Color.FromArgb(237, 237, 242);
            rbSolid.Location = new Point(18, 100);
            rbSolid.Name = "rbSolid";
            rbSolid.Size = new Size(199, 22);
            rbSolid.TabIndex = 2;
            rbSolid.Text = "Sólido — escuro, sem efeitos";
            rbSolid.CheckedChanged += backdrop_CheckedChanged;
            // 
            // rbAcrylic
            // 
            rbAcrylic.AutoSize = true;
            rbAcrylic.ForeColor = Color.FromArgb(237, 237, 242);
            rbAcrylic.Location = new Point(18, 72);
            rbAcrylic.Name = "rbAcrylic";
            rbAcrylic.Size = new Size(256, 22);
            rbAcrylic.TabIndex = 1;
            rbAcrylic.Text = "Acrílico — transparente, com desfoque";
            rbAcrylic.CheckedChanged += backdrop_CheckedChanged;
            // 
            // rbMica
            // 
            rbMica.AutoSize = true;
            rbMica.ForeColor = Color.FromArgb(237, 237, 242);
            rbMica.Location = new Point(18, 44);
            rbMica.Name = "rbMica";
            rbMica.Size = new Size(259, 22);
            rbMica.TabIndex = 0;
            rbMica.Text = "Mica — acompanha o papel de parede";
            rbMica.CheckedChanged += backdrop_CheckedChanged;
            // 
            // cardOpacity
            // 
            cardOpacity.BackColor = Color.FromArgb(26, 26, 31);
            cardOpacity.Controls.Add(lblOpacity);
            cardOpacity.Controls.Add(trackOpacity);
            cardOpacity.Glyph = "";
            cardOpacity.Location = new Point(14, 184);
            cardOpacity.Name = "cardOpacity";
            cardOpacity.Size = new Size(432, 104);
            cardOpacity.TabIndex = 1;
            cardOpacity.Title = "Opacidade da janela";
            // 
            // lblOpacity
            // 
            lblOpacity.Font = new Font("Segoe UI Semibold", 10.5F);
            lblOpacity.ForeColor = Color.FromArgb(237, 237, 242);
            lblOpacity.Location = new Point(354, 46);
            lblOpacity.Name = "lblOpacity";
            lblOpacity.Size = new Size(62, 28);
            lblOpacity.TabIndex = 1;
            lblOpacity.Text = "100%";
            lblOpacity.TextAlign = ContentAlignment.MiddleRight;
            // 
            // trackOpacity
            // 
            trackOpacity.BackColor = Color.FromArgb(26, 26, 31);
            trackOpacity.Location = new Point(12, 44);
            trackOpacity.Minimum = 60;
            trackOpacity.Name = "trackOpacity";
            trackOpacity.Size = new Size(336, 45);
            trackOpacity.TabIndex = 0;
            trackOpacity.Value = 100;
            trackOpacity.Scroll += trackOpacity_Scroll;
            // 
            // cardLanguage
            // 
            cardLanguage.BackColor = Color.FromArgb(26, 26, 31);
            cardLanguage.Controls.Add(rbEnglish);
            cardLanguage.Controls.Add(rbPortuguese);
            cardLanguage.Glyph = "";
            cardLanguage.Location = new Point(14, 298);
            cardLanguage.Name = "cardLanguage";
            cardLanguage.Size = new Size(432, 82);
            cardLanguage.TabIndex = 2;
            cardLanguage.Title = "Idioma";
            // 
            // rbEnglish
            // 
            rbEnglish.AutoSize = true;
            rbEnglish.ForeColor = Color.FromArgb(237, 237, 242);
            rbEnglish.Location = new Point(220, 46);
            rbEnglish.Name = "rbEnglish";
            rbEnglish.Size = new Size(97, 22);
            rbEnglish.TabIndex = 1;
            rbEnglish.Text = "English (US)";
            rbEnglish.CheckedChanged += language_CheckedChanged;
            // 
            // rbPortuguese
            // 
            rbPortuguese.AutoSize = true;
            rbPortuguese.ForeColor = Color.FromArgb(237, 237, 242);
            rbPortuguese.Location = new Point(18, 46);
            rbPortuguese.Name = "rbPortuguese";
            rbPortuguese.Size = new Size(130, 22);
            rbPortuguese.TabIndex = 0;
            rbPortuguese.Text = "Português (Brasil)";
            rbPortuguese.CheckedChanged += language_CheckedChanged;
            //
            // cardCredits
            //
            cardCredits.BackColor = Color.FromArgb(26, 26, 31);
            cardCredits.Controls.Add(lblCredits);
            cardCredits.Glyph = "";
            cardCredits.Location = new Point(14, 390);
            cardCredits.Name = "cardCredits";
            cardCredits.Size = new Size(432, 76);
            cardCredits.TabIndex = 3;
            cardCredits.Title = "Créditos";
            //
            // lblCredits
            //
            lblCredits.ForeColor = Color.FromArgb(156, 156, 168);
            lblCredits.Location = new Point(18, 44);
            lblCredits.Name = "lblCredits";
            lblCredits.Size = new Size(396, 22);
            lblCredits.TabIndex = 0;
            lblCredits.Text = "Ferramenta criada por MrGeH.";
            //
            // bottomPanel
            //
            bottomPanel.Controls.Add(btnClose);
            bottomPanel.Controls.Add(btnReset);
            bottomPanel.FlowDirection = FlowDirection.RightToLeft;
            bottomPanel.Location = new Point(14, 478);
            bottomPanel.Name = "bottomPanel";
            bottomPanel.Size = new Size(432, 40);
            bottomPanel.TabIndex = 4;
            bottomPanel.WrapContents = false;
            // 
            // btnClose
            // 
            btnClose.Font = new Font("Segoe UI Semibold", 9.5F);
            btnClose.Glyph = "";
            btnClose.Kind = UI.ButtonKind.Primary;
            btnClose.Location = new Point(338, 3);
            btnClose.Margin = new Padding(8, 3, 0, 3);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(94, 34);
            btnClose.TabIndex = 1;
            btnClose.TabStop = false;
            btnClose.Text = "Fechar";
            btnClose.Click += btnClose_Click;
            // 
            // btnReset
            // 
            btnReset.Font = new Font("Segoe UI Semibold", 9.5F);
            btnReset.Glyph = "";
            btnReset.Location = new Point(171, 3);
            btnReset.Margin = new Padding(8, 3, 0, 3);
            btnReset.Name = "btnReset";
            btnReset.Size = new Size(159, 34);
            btnReset.TabIndex = 0;
            btnReset.TabStop = false;
            btnReset.Text = "Restaurar padrão";
            btnReset.Click += btnReset_Click;
            // 
            // ConfiguracoesForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(17, 17, 20);
            ClientSize = new Size(460, 532);
            Controls.Add(bottomPanel);
            Controls.Add(cardCredits);
            Controls.Add(cardLanguage);
            Controls.Add(cardOpacity);
            Controls.Add(cardBackdrop);
            Font = new Font("Segoe UI", 9.75F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ConfiguracoesForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Configurações";
            cardBackdrop.ResumeLayout(false);
            cardBackdrop.PerformLayout();
            cardOpacity.ResumeLayout(false);
            cardLanguage.ResumeLayout(false);
            cardLanguage.PerformLayout();
            cardCredits.ResumeLayout(false);
            bottomPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private UI.CardPanel cardBackdrop;
        private Label lblBackdropNote;
        private UI.ModernRadio rbSolid;
        private UI.ModernRadio rbAcrylic;
        private UI.ModernRadio rbMica;
        private UI.CardPanel cardOpacity;
        private Label lblOpacity;
        private UI.ModernSlider trackOpacity;
        private UI.CardPanel cardLanguage;
        private UI.ModernRadio rbEnglish;
        private UI.ModernRadio rbPortuguese;
        private UI.CardPanel cardCredits;
        private Label lblCredits;
        private FlowLayoutPanel bottomPanel;
        private UI.ModernButton btnClose;
        private UI.ModernButton btnReset;
    }
}
