using Extrator_e_Compilador_RCF.Core;
using Extrator_e_Compilador_RCF.UI;

namespace Extrator_e_Compilador_RCF
{
    /// <summary>
    /// Editor dos textos de um .p3d (TextBible ou legenda), aberto numa aba da janela principal.
    /// "Salvar no RCF" gera o .p3d de novo e o grava de volta no RCF (quem faz isso é a MainForm).
    /// </summary>
    public partial class TextEditorView : UserControl
    {
        private readonly IReadOnlyList<string>? _gameOriginal;
        private bool _dirty;

        /// <summary>Caminho do .p3d dentro do RCF.</summary>
        public string EntryName { get; } = "";
        public P3dTextDocument? Document { get; }

        /// <summary>Há edições ainda não gravadas no RCF.</summary>
        public bool Dirty
        {
            get => _dirty;
            private set { if (_dirty == value) return; _dirty = value; DirtyChanged?.Invoke(this); }
        }

        public event Action<TextEditorView>? SaveRequested;
        public event Action<TextEditorView>? DirtyChanged;

        public TextEditorView()
        {
            InitializeComponent();
        }

        /// <param name="gameOriginal">Textos do arquivo como está no RCF original (coluna "Original"); null = os atuais.</param>
        public TextEditorView(string entryName, P3dTextDocument document, IReadOnlyList<string>? gameOriginal) : this()
        {
            EntryName = entryName;
            Document = document;
            _gameOriginal = gameOriginal?.Count == document.Entries.Count ? gameOriginal : null;

            var cell = grid.DefaultCellStyle;
            cell.BackColor = Theme.Surface;
            cell.ForeColor = Theme.Text;
            cell.SelectionBackColor = Theme.Blend(Theme.Surface, Theme.Accent, 0.28f);
            cell.SelectionForeColor = Color.White;
            cell.Font = Theme.Ui;
            cell.WrapMode = DataGridViewTriState.True;
            cell.Padding = new Padding(this.Dpi(6), this.Dpi(5), this.Dpi(6), this.Dpi(5));
            var head = grid.ColumnHeadersDefaultCellStyle;
            head.BackColor = Theme.Surface2;
            head.ForeColor = Theme.Muted;
            head.SelectionBackColor = Theme.Surface2;
            head.Font = Theme.UiSemibold;
            head.Padding = new Padding(this.Dpi(6), 0, 0, 0);
            grid.ColumnHeadersHeight = this.Dpi(34);
            grid.RowTemplate.MinimumHeight = this.Dpi(30);
            colIndex.DefaultCellStyle.ForeColor = Theme.Faint;
            colId.DefaultCellStyle.ForeColor = Theme.Faint;
            colId.DefaultCellStyle.Font = Theme.Mono;
            colLanguage.DefaultCellStyle.ForeColor = Theme.Muted;
            colOriginal.DefaultCellStyle.ForeColor = Theme.Muted;
            Native.DarkControl(grid);

            colId.Visible = !document.IsSubtitle;
            colLanguage.Visible = document.Entries.Select(e => e.Language).Distinct().Count() > 1;
            searchBox.Box.TextChanged += (_, _) => ApplyFilter();
            searchBox.Box.KeyDown += (_, e) => { if (e.KeyCode == Keys.Escape) searchBox.Box.Text = ""; };

            FillGrid();
            RefreshTexts();
        }

        /// <summary>Textos que vêm do código (cabeçalhos da grade, nota do cartão) no idioma atual.</summary>
        public void RefreshTexts()
        {
            if (Document == null) return;
            colIndex.HeaderText = "#";
            colId.HeaderText = L.T("ID (hash)");
            colLanguage.HeaderText = L.T("Idioma");
            colOriginal.HeaderText = L.T(_gameOriginal != null ? "Original (no RCF)" : "Original");
            colText.HeaderText = L.T("Texto (editável)");
            cardTexts.Title = $"{L.T("Textos")} — {Path.GetFileName(EntryName)}";
            UpdateNote();
        }

        /// <summary>As edições foram gravadas no RCF.</summary>
        public void MarkApplied() => Dirty = false;

        private string OriginalAt(int i) => _gameOriginal?[i] ?? Document!.Entries[i].Original;

        private void FillGrid()
        {
            var doc = Document!;
            grid.SuspendLayout();
            grid.Rows.Clear();
            var rows = new DataGridViewRow[doc.Entries.Count];
            for (int i = 0; i < rows.Length; i++)
            {
                var e = doc.Entries[i];
                var row = new DataGridViewRow { Tag = i };
                row.CreateCells(grid, i + 1, doc.IsSubtitle ? "" : e.Hash.ToString("X8"), e.Language, OriginalAt(i), e.Text);
                rows[i] = row;
            }
            grid.Rows.AddRange(rows);
            grid.ResumeLayout();
            ApplyFilter();
        }

        private void UpdateNote()
        {
            var doc = Document!;
            int changed = Enumerable.Range(0, doc.Entries.Count).Count(i => doc.Entries[i].Text != OriginalAt(i));
            cardTexts.Note = L.F("{0:N0} texto(s)", doc.Entries.Count) + (changed > 0 ? "  ·  " + L.F("{0:N0} alterado(s)", changed) : "") +
                             "  ·  " + L.T("Ferramenta criada por MrGeH.");
        }

        private void ApplyFilter()
        {
            if (Document == null) return;
            string q = searchBox.Box.Text.Trim();
            bool onlyChanged = chkChangedOnly.Checked;
            grid.CurrentCell = null;
            foreach (DataGridViewRow row in grid.Rows)
            {
                int i = (int)row.Tag!;
                var e = Document.Entries[i];
                bool match = q.Length == 0 ||
                             e.Text.Contains(q, StringComparison.CurrentCultureIgnoreCase) ||
                             OriginalAt(i).Contains(q, StringComparison.CurrentCultureIgnoreCase) ||
                             (!Document.IsSubtitle && e.Hash.ToString("X8").Contains(q, StringComparison.OrdinalIgnoreCase));
                row.Visible = match && (!onlyChanged || e.Text != OriginalAt(i));
            }
        }

        private void grid_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != colText.Index) return;
            var row = grid.Rows[e.RowIndex];
            var entry = Document!.Entries[(int)row.Tag!];
            string value = row.Cells[colText.Index].Value as string ?? "";
            if (value == entry.Text) return;
            entry.Text = value;
            Dirty = true;
            UpdateNote();
            grid.InvalidateRow(e.RowIndex);
        }

        private void grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != colText.Index || Document == null || e.CellStyle == null) return;
            int i = (int)grid.Rows[e.RowIndex].Tag!;
            if (Document.Entries[i].Text != OriginalAt(i)) e.CellStyle.ForeColor = Theme.AccentHover;
        }

        private void grid_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            grid.CurrentCell = grid.Rows[e.RowIndex].Cells[colText.Index];
            grid.BeginEdit(false);
        }

        private void grid_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F && e.Control) { searchBox.Box.Focus(); e.Handled = true; }
            else if (e.KeyCode == Keys.Enter && grid.CurrentCell != null && !grid.IsCurrentCellInEditMode)
            {
                grid.CurrentCell = grid.Rows[grid.CurrentCell.RowIndex].Cells[colText.Index];
                grid.BeginEdit(false);
                e.Handled = true;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.S))
            {
                grid.EndEdit();
                SaveRequested?.Invoke(this);
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void btnSave_Click(object? sender, EventArgs e)
        {
            grid.EndEdit();
            SaveRequested?.Invoke(this);
        }

        private void btnExportTxt_Click(object? sender, EventArgs e)
        {
            grid.EndEdit();
            using var dlg = new SaveFileDialog
            {
                Filter = L.T("Texto (*.txt)|*.txt"),
                FileName = Path.GetFileNameWithoutExtension(EntryName) + ".txt",
                Title = L.T("Exportar textos")
            };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            try { File.WriteAllBytes(dlg.FileName, Document!.ExportTxt()); }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void btnImportTxt_Click(object? sender, EventArgs e)
        {
            grid.EndEdit();
            using var dlg = new OpenFileDialog { Filter = L.T("Texto (*.txt)|*.txt|Todos os arquivos (*.*)|*.*"), Title = L.T("Importar textos") };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            try
            {
                int n = Document!.ImportTxt(File.ReadAllBytes(dlg.FileName));
                if (n > 0) Dirty = true;
                FillGrid();
                UpdateNote();
                MessageBox.Show(this, L.F("{0:N0} texto(s) alterado(s) pelo .txt.", n), L.T("Importar textos"));
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, L.T("Importar textos"), MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        private void btnRevert_Click(object? sender, EventArgs e)
        {
            grid.CancelEdit();
            var doc = Document!;
            if (!doc.Entries.Any(x => x.Changed)) return;
            if (MessageBox.Show(this, L.T("Desfazer as edições feitas desde que o arquivo foi aberto?"), L.T("Desfazer edições"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            foreach (var x in doc.Entries) x.Text = x.Original;
            Dirty = true;
            FillGrid();
            UpdateNote();
        }

        private void chkChangedOnly_Click(object? sender, EventArgs e)
        {
            chkChangedOnly.Checked = !chkChangedOnly.Checked;
            ApplyFilter();
        }
    }
}
