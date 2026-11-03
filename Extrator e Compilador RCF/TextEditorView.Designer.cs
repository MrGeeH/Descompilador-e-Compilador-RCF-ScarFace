namespace Extrator_e_Compilador_RCF
{
    partial class TextEditorView
    {
        /// <summary>Required designer variable.</summary>
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            cardTexts = new UI.CardPanel();
            grid = new DataGridView();
            colIndex = new DataGridViewTextBoxColumn();
            colId = new DataGridViewTextBoxColumn();
            colLanguage = new DataGridViewTextBoxColumn();
            colOriginal = new DataGridViewTextBoxColumn();
            colText = new DataGridViewTextBoxColumn();
            toolbar = new Panel();
            actions = new FlowLayoutPanel();
            btnSave = new UI.ModernButton();
            btnImportTxt = new UI.ModernButton();
            btnExportTxt = new UI.ModernButton();
            btnRevert = new UI.ModernButton();
            chkChangedOnly = new UI.ModernButton();
            searchBox = new UI.SearchBox();
            cardTexts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
            toolbar.SuspendLayout();
            actions.SuspendLayout();
            SuspendLayout();
            //
            // cardTexts
            //
            cardTexts.BackColor = Color.FromArgb(26, 26, 31);
            cardTexts.Controls.Add(grid);
            cardTexts.Controls.Add(toolbar);
            cardTexts.Dock = DockStyle.Fill;
            cardTexts.Glyph = UI.Glyphs.Text;
            cardTexts.Location = new Point(0, 0);
            cardTexts.Name = "cardTexts";
            cardTexts.Size = new Size(900, 520);
            cardTexts.TabIndex = 0;
            cardTexts.Title = "Textos";
            //
            // grid
            //
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
            grid.BackgroundColor = Color.FromArgb(26, 26, 31);
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.Columns.AddRange(new DataGridViewColumn[] { colIndex, colId, colLanguage, colOriginal, colText });
            grid.Dock = DockStyle.Fill;
            grid.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
            grid.EnableHeadersVisualStyles = false;
            grid.GridColor = Color.FromArgb(46, 46, 56);
            grid.Location = new Point(14, 92);
            grid.Name = "grid";
            grid.RowHeadersVisible = false;
            grid.SelectionMode = DataGridViewSelectionMode.CellSelect;
            grid.Size = new Size(872, 414);
            grid.TabIndex = 1;
            grid.CellEndEdit += grid_CellEndEdit;
            grid.CellFormatting += grid_CellFormatting;
            grid.CellDoubleClick += grid_CellDoubleClick;
            grid.KeyDown += grid_KeyDown;
            //
            // colIndex
            //
            colIndex.HeaderText = "#";
            colIndex.Name = "colIndex";
            colIndex.ReadOnly = true;
            colIndex.Width = 56;
            //
            // colId
            //
            colId.HeaderText = "ID (hash)";
            colId.Name = "colId";
            colId.ReadOnly = true;
            colId.Width = 96;
            //
            // colLanguage
            //
            colLanguage.HeaderText = "Idioma";
            colLanguage.Name = "colLanguage";
            colLanguage.ReadOnly = true;
            colLanguage.Width = 80;
            //
            // colOriginal
            //
            colOriginal.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colOriginal.FillWeight = 45F;
            colOriginal.HeaderText = "Original";
            colOriginal.Name = "colOriginal";
            colOriginal.ReadOnly = true;
            //
            // colText
            //
            colText.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colText.FillWeight = 55F;
            colText.HeaderText = "Texto (editável)";
            colText.Name = "colText";
            //
            // toolbar
            //
            toolbar.BackColor = Color.FromArgb(26, 26, 31);
            toolbar.Controls.Add(actions);
            toolbar.Controls.Add(searchBox);
            toolbar.Dock = DockStyle.Top;
            toolbar.Location = new Point(14, 40);
            toolbar.Name = "toolbar";
            toolbar.Padding = new Padding(0, 0, 0, 12);
            toolbar.Size = new Size(872, 52);
            toolbar.TabIndex = 0;
            //
            // actions
            //
            actions.BackColor = Color.FromArgb(26, 26, 31);
            actions.Controls.Add(btnSave);
            actions.Controls.Add(btnImportTxt);
            actions.Controls.Add(btnExportTxt);
            actions.Controls.Add(btnRevert);
            actions.Controls.Add(chkChangedOnly);
            actions.Dock = DockStyle.Fill;
            actions.FlowDirection = FlowDirection.RightToLeft;
            actions.Location = new Point(280, 0);
            actions.Name = "actions";
            actions.Padding = new Padding(12, 0, 0, 0);
            actions.Size = new Size(592, 40);
            actions.TabIndex = 1;
            actions.WrapContents = false;
            //
            // btnSave
            //
            btnSave.BaseColor = Color.FromArgb(26, 26, 31);
            btnSave.Glyph = UI.Glyphs.Save;
            btnSave.Kind = UI.ButtonKind.Primary;
            btnSave.Margin = new Padding(8, 3, 0, 3);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(140, 34);
            btnSave.TabIndex = 0;
            btnSave.Text = "Salvar no RCF";
            btnSave.Click += btnSave_Click;
            //
            // btnImportTxt
            //
            btnImportTxt.BaseColor = Color.FromArgb(26, 26, 31);
            btnImportTxt.Glyph = UI.Glyphs.Import;
            btnImportTxt.Margin = new Padding(8, 3, 0, 3);
            btnImportTxt.Name = "btnImportTxt";
            btnImportTxt.Size = new Size(130, 34);
            btnImportTxt.TabIndex = 1;
            btnImportTxt.Text = "Importar TXT";
            btnImportTxt.Click += btnImportTxt_Click;
            //
            // btnExportTxt
            //
            btnExportTxt.BaseColor = Color.FromArgb(26, 26, 31);
            btnExportTxt.Glyph = UI.Glyphs.Export;
            btnExportTxt.Margin = new Padding(8, 3, 0, 3);
            btnExportTxt.Name = "btnExportTxt";
            btnExportTxt.Size = new Size(130, 34);
            btnExportTxt.TabIndex = 2;
            btnExportTxt.Text = "Exportar TXT";
            btnExportTxt.Click += btnExportTxt_Click;
            //
            // btnRevert
            //
            btnRevert.BaseColor = Color.FromArgb(26, 26, 31);
            btnRevert.Glyph = UI.Glyphs.Undo;
            btnRevert.Kind = UI.ButtonKind.Subtle;
            btnRevert.Margin = new Padding(8, 3, 0, 3);
            btnRevert.Name = "btnRevert";
            btnRevert.Size = new Size(150, 34);
            btnRevert.TabIndex = 3;
            btnRevert.Text = "Desfazer edições";
            btnRevert.Click += btnRevert_Click;
            //
            // chkChangedOnly
            //
            chkChangedOnly.BaseColor = Color.FromArgb(26, 26, 31);
            chkChangedOnly.Glyph = UI.Glyphs.Check;
            chkChangedOnly.Kind = UI.ButtonKind.Subtle;
            chkChangedOnly.Margin = new Padding(8, 3, 0, 3);
            chkChangedOnly.Name = "chkChangedOnly";
            chkChangedOnly.Size = new Size(140, 34);
            chkChangedOnly.TabIndex = 4;
            chkChangedOnly.Text = "Só alterados";
            chkChangedOnly.Click += chkChangedOnly_Click;
            //
            // searchBox
            //
            searchBox.BackColor = Color.FromArgb(36, 36, 43);
            searchBox.Dock = DockStyle.Left;
            searchBox.Location = new Point(0, 0);
            searchBox.Name = "searchBox";
            searchBox.Placeholder = "Buscar texto ou ID…";
            searchBox.Size = new Size(280, 40);
            searchBox.TabIndex = 0;
            //
            // TextEditorView
            //
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(17, 17, 20);
            Controls.Add(cardTexts);
            Font = new Font("Segoe UI", 9.75F);
            ForeColor = Color.FromArgb(237, 237, 242);
            Name = "TextEditorView";
            Size = new Size(900, 520);
            cardTexts.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            toolbar.ResumeLayout(false);
            actions.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private UI.CardPanel cardTexts;
        private DataGridView grid;
        private DataGridViewTextBoxColumn colIndex;
        private DataGridViewTextBoxColumn colId;
        private DataGridViewTextBoxColumn colLanguage;
        private DataGridViewTextBoxColumn colOriginal;
        private DataGridViewTextBoxColumn colText;
        private Panel toolbar;
        private FlowLayoutPanel actions;
        private UI.ModernButton btnSave;
        private UI.ModernButton btnImportTxt;
        private UI.ModernButton btnExportTxt;
        private UI.ModernButton btnRevert;
        private UI.ModernButton chkChangedOnly;
        private UI.SearchBox searchBox;
    }
}
