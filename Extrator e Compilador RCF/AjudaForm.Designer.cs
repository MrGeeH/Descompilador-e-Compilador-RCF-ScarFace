namespace Extrator_e_Compilador_RCF
{
    partial class AjudaForm
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
            cardHelp = new UI.CardPanel();
            rtbHelp = new RichTextBox();
            bottomPanel = new FlowLayoutPanel();
            btnClose = new UI.ModernButton();
            cardHelp.SuspendLayout();
            bottomPanel.SuspendLayout();
            SuspendLayout();
            //
            // cardHelp
            //
            cardHelp.Controls.Add(rtbHelp);
            cardHelp.Dock = DockStyle.Fill;
            cardHelp.Glyph = "";
            cardHelp.Location = new Point(14, 14);
            cardHelp.Name = "cardHelp";
            cardHelp.Size = new Size(672, 524);
            cardHelp.TabIndex = 0;
            cardHelp.Title = "Como usar o Descompilador e Compilador de RCF/IMG/P3D";
            //
            // rtbHelp
            //
            rtbHelp.BackColor = Color.FromArgb(26, 26, 31);
            rtbHelp.BorderStyle = BorderStyle.None;
            rtbHelp.Dock = DockStyle.Fill;
            rtbHelp.Font = new Font("Segoe UI", 10F);
            rtbHelp.ForeColor = Color.FromArgb(237, 237, 242);
            rtbHelp.Location = new Point(10, 40);
            rtbHelp.Name = "rtbHelp";
            rtbHelp.ReadOnly = true;
            rtbHelp.ScrollBars = RichTextBoxScrollBars.Vertical;
            rtbHelp.Size = new Size(652, 474);
            rtbHelp.TabIndex = 0;
            rtbHelp.Text = "";
            //
            // bottomPanel
            //
            bottomPanel.Controls.Add(btnClose);
            bottomPanel.Dock = DockStyle.Bottom;
            bottomPanel.FlowDirection = FlowDirection.RightToLeft;
            bottomPanel.Location = new Point(14, 538);
            bottomPanel.Name = "bottomPanel";
            bottomPanel.Padding = new Padding(0, 12, 0, 0);
            bottomPanel.Size = new Size(672, 54);
            bottomPanel.TabIndex = 1;
            bottomPanel.WrapContents = false;
            //
            // btnClose
            //
            btnClose.Glyph = "";
            btnClose.Kind = UI.ButtonKind.Primary;
            btnClose.Location = new Point(578, 15);
            btnClose.Margin = new Padding(0);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(94, 34);
            btnClose.TabIndex = 0;
            btnClose.Text = "Entendi";
            btnClose.Click += btnClose_Click;
            //
            // AjudaForm
            //
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(17, 17, 20);
            ClientSize = new Size(700, 606);
            Controls.Add(cardHelp);
            Controls.Add(bottomPanel);
            Font = new Font("Segoe UI", 9.75F);
            ForeColor = Color.FromArgb(237, 237, 242);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AjudaForm";
            Padding = new Padding(14);
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Ajuda";
            cardHelp.ResumeLayout(false);
            bottomPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private UI.CardPanel cardHelp;
        private RichTextBox rtbHelp;
        private FlowLayoutPanel bottomPanel;
        private UI.ModernButton btnClose;
    }
}
