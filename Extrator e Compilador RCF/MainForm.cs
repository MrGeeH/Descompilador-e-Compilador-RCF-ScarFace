using System.Diagnostics;
using Extrator_e_Compilador_RCF.Core;
using Extrator_e_Compilador_RCF.UI;

namespace Extrator_e_Compilador_RCF
{
    /// <summary>
    /// Explorador do RCF: navegar, exportar (tudo / selecionados / um), importar de volta (substituir um ou vários,
    /// adicionar novos), trocar imagens dos .p3d e salvar um RCF novo. O layout fica em MainForm.Designer.cs.
    /// </summary>
    public partial class MainForm : ThemedForm
    {
        /// <summary>Um arquivo como aparece no explorador (original, substituído ou novo).</summary>
        private sealed class Item
        {
            public required string Name { get; init; }
            public RcfEntry? Entry { get; init; }
            public required RcfSource Source { get; init; }
            public bool Replaced { get; init; }
            public bool Added => Entry == null;
            public string FileName => Path.GetFileName(Name);
            public string Folder => Path.GetDirectoryName(Name) ?? "";
        }

        private RcfArchive? _rcf;
        private readonly Dictionary<int, RcfSource> _replaced = new();
        private readonly List<(string name, RcfSource source)> _added = new();
        private List<Item> _items = new();
        private string _folder = "";
        private CancellationTokenSource? _cts;

        private P3dFile? _previewP3d;
        private List<P3dGraphic> _previewImages = new();
        private readonly Dictionary<P3dGraphic, Image?> _thumbs = new();
        private Item? _previewItem;
        private int _hotRow = -1;

        private readonly TabItem _explorerTab = new() { Text = "Explorador", Glyph = Glyphs.Folder, Closable = false };
        private readonly Dictionary<TabItem, TextEditorView> _editors = new();

        public MainForm()
        {
            InitializeComponent();
            if (DesignMode) return;

            foreach (var m in new[] { menuExport, menuImport, menuImages, menuList, menuTree }) Menus.Style(m);
            searchBox.Box.TextChanged += (_, _) => FillList();
            searchBox.Box.KeyDown += (_, e) => { if (e.KeyCode == Keys.Escape) searchBox.Box.Text = ""; };
            emptyState.OpenButton.Click += btnOpen_Click;
            statusBar.Cancel.Click += (_, _) => _cts?.Cancel();
            statusBar.LogToggle.Click += (_, _) => ToggleLog();
            picturePreview.BackgroundImage = Theme.Checker(this.Dpi(10));
            listFiles.SmallImageList = new ImageList { ImageSize = new Size(1, this.Dpi(32)) }; // altura das linhas
            Native.DarkControl(listFiles);
            Native.DarkControl(treeFolders);
            Native.DarkControl(listImages);
            Native.DarkControl(txtLog);
            Shown += (_, _) =>
            {
                // o modo escuro do .NET aplica "ItemsView" à lista (desenha divisórias na área vazia): troca pelo Explorer
                Native.SetWindowTheme(listFiles.Handle, "DarkMode_Explorer", null);
                FitColumns();
            };
            listFiles.Resize += (_, _) => FitColumns();
            tabStrip.Add(_explorerTab);

            UpdateState();
            UpdatePreview();
            Log(L.T("Abra um arquivo .rcf (cement.rcf, snd_*.rcf…) ou arraste-o para a janela."));
        }

        /// <summary>Abre a janela já carregando um RCF (linha de comando / "Abrir com").</summary>
        public MainForm(string? rcfPath) : this()
        {
            if (rcfPath != null) Shown += (_, _) => LoadRcf(rcfPath);
        }

        protected override int GlassTop => headerBar.Height;
        protected override int GlassBottom => statusBar.Height;

        private AppLanguage _shownLanguage = L.Current;

        protected override void OnAppearanceChanged()
        {
            // o botão mostra o idioma para o qual vai trocar
            btnLanguage.Text = L.English ? "Português" : "English";
            statusBar.Credit = L.T("Ferramenta criada por MrGeH.");
            if (_cts == null) UpdateState();
            if (_shownLanguage != L.Current)
            {
                _shownLanguage = L.Current;
                FitColumns();
                if (_rcf != null) FillList();
                else UpdatePreview();
            }
            foreach (var view in _editors.Values) view.RefreshTexts();
            tabStrip.Invalidate();
            emptyState.Invalidate();
            treeFolders.Invalidate();
            listFiles.Invalidate();
            listImages.Invalidate();
        }

        #region Estado, log e tarefas longas

        private bool HasChanges => _replaced.Count > 0 || _added.Count > 0;

        private void UpdateState()
        {
            bool idle = _cts == null, open = _rcf != null;
            btnOpen.Enabled = btnCompile.Enabled = idle;
            btnSaveAs.Enabled = btnExport.Enabled = btnImport.Enabled = btnImages.Enabled = idle && open;
            btnSave.Enabled = idle && HasChanges;
            listFiles.Enabled = treeFolders.Enabled = searchBox.Enabled = imageButtons.Enabled = idle;
            emptyState.Visible = !open;
            navBar.Visible = open;

            Text = !open ? L.T("Descompilador e Compilador de RCF/IMG/P3D — Scarface: The World Is Yours")
                         : $"{Path.GetFileName(_rcf!.FilePath)}{(HasChanges ? " •" : "")} — {L.T("Descompilador e Compilador de RCF/IMG")}";
            headerBar.Subtitle = !open ? "Scarface: The World Is Yours  ·  " + L.T("nenhum arquivo aberto")
                : $"{_rcf!.FilePath}  ·  {FormatSize(_rcf.FileLength)}" + (HasChanges ? L.F("  ·  {0} alteração(ões) não salva(s)", _replaced.Count + _added.Count) : "");
            cardFolders.Note = open ? L.F("{0:N0} arquivos", _items.Count) : "";
            if (idle)
                statusBar.StatusText = !open ? L.T("Nenhum RCF aberto.") :
                    L.F("{0:N0} arquivos  ·  {1} substituído(s)  ·  {2} novo(s)", _rcf!.Entries.Count, _replaced.Count, _added.Count) +
                    L.T(_rcf.HashesVerified ? "  ·  hashes conferidos ✓" : "  ·  ATENÇÃO: hashes não conferem, nomes ligados pela ordem dos dados");
        }

        private void ToggleLog()
        {
            splitLog.Panel2Collapsed = !splitLog.Panel2Collapsed;
            if (!splitLog.Panel2Collapsed) splitLog.SplitterDistance = Math.Max(200, splitLog.Height - this.Dpi(170));
            statusBar.LogToggle.Checked = !splitLog.Panel2Collapsed;
        }

        private void Log(string text)
        {
            if (InvokeRequired) { BeginInvoke(() => Log(text)); return; }
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {text}{Environment.NewLine}");
        }

        private void ShowError(Exception ex)
        {
            Log(L.T("ERRO: ") + ex.Message);
            MessageBox.Show(this, ex.Message, L.T("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>Executa uma operação longa em segundo plano com barra de progresso e cancelamento.</summary>
        private async Task<bool> RunBusy(string title, Action<IProgress<RcfProgress>, CancellationToken> work)
        {
            if (_cts != null) return false;
            _cts = new CancellationTokenSource();
            statusBar.Progress.Visible = statusBar.Cancel.Visible = true;
            statusBar.Progress.Value = 0;
            statusBar.StatusText = title + "…";
            statusBar.PerformLayout();
            UpdateState();
            var progress = new Progress<RcfProgress>(p =>
            {
                double frac = p.TotalBytes > 0 ? (double)p.Bytes / p.TotalBytes : p.TotalFiles > 0 ? (double)p.Files / p.TotalFiles : 0;
                statusBar.Progress.Value = Math.Clamp((int)(frac * 100), 0, 100);
                statusBar.StatusText = $"{title}  {p.Files:N0}/{p.TotalFiles:N0}  ·  {p.Current}";
            });
            var token = _cts.Token;
            var sw = Stopwatch.StartNew();
            try
            {
                await Task.Run(() => work(progress, token), token);
                Log(L.F("{0} concluído em {1:N1}s.", title, sw.Elapsed.TotalSeconds));
                return true;
            }
            catch (OperationCanceledException) { Log(L.F("{0} cancelado.", title)); return false; }
            catch (Exception ex) { ShowError(ex); return false; }
            finally
            {
                _cts.Dispose();
                _cts = null;
                statusBar.Progress.Visible = statusBar.Cancel.Visible = false;
                statusBar.PerformLayout();
                UpdateState();
            }
        }

        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (_cts != null) { e.Cancel = true; return; }
            if (HasChanges && MessageBox.Show(this, L.T("Há alterações não salvas no RCF. Sair mesmo assim?"), Text,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) e.Cancel = true;
            else if (!HasChanges && EditorsDirty && MessageBox.Show(this, L.T("Há textos editados que não foram gravados no RCF. Sair mesmo assim?"), Text,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) e.Cancel = true;
        }

        #endregion

        #region Botões e menus (eventos do designer)

        private void btnOpen_Click(object? sender, EventArgs e) => OpenRcf();
        private void btnSave_Click(object? sender, EventArgs e) => SaveRcf(false);
        private void btnSaveAs_Click(object? sender, EventArgs e) => SaveRcf(true);
        private void btnCompile_Click(object? sender, EventArgs e) => CompileFolder();
        private void btnUp_Click(object? sender, EventArgs e) => GoUp();
        private void btnHelp_Click(object? sender, EventArgs e) { using var f = new AjudaForm(); f.ShowDialog(this); }
        private void btnSettings_Click(object? sender, EventArgs e) { using var f = new ConfiguracoesForm(); f.ShowDialog(this); }

        /// <summary>Troca rápida PT-BR ⇄ EN-US (o português é o idioma principal).</summary>
        private void btnLanguage_Click(object? sender, EventArgs e)
        {
            AppSettings.Current.Language = L.English ? AppLanguage.PtBr : AppLanguage.EnUs;
            AppSettings.Current.Commit();
        }
        private void miExportSelected_Click(object? sender, EventArgs e) => ExportSelected();
        private void miExportFolder_Click(object? sender, EventArgs e) => ExportItems(ItemsUnder(_folder), false);
        private void miExportAll_Click(object? sender, EventArgs e) => ExportItems(_items, true);
        private void miReplaceSelected_Click(object? sender, EventArgs e) => ReplaceSelectedWithFile();
        private void miImportFiles_Click(object? sender, EventArgs e) => ImportFilesToFolder();
        private void miImportFolder_Click(object? sender, EventArgs e) => ImportFolder();
        private void miRevertSelected_Click(object? sender, EventArgs e) => RevertSelected();
        private void miRevertAll_Click(object? sender, EventArgs e) => RevertAll();
        private void miImagesExportSelection_Click(object? sender, EventArgs e) => ExportImages(SelectedOrFolderItems());
        private void miImagesExportAll_Click(object? sender, EventArgs e) => ExportImages(_items);
        private void miImagesImport_Click(object? sender, EventArgs e) => ImportImages();
        private void miTreeExportImages_Click(object? sender, EventArgs e) => ExportImages(ItemsUnder(_folder));
        private void btnExportImage_Click(object? sender, EventArgs e) => ExportPreviewImage();
        private void btnReplaceImage_Click(object? sender, EventArgs e) => ReplacePreviewImage();

        private void miListEditTexts_Click(object? sender, EventArgs e)
        {
            if (listFiles.SelectedItems.Count == 1 && listFiles.SelectedItems[0].Tag is Item it) OpenTextEditor(it);
            else MessageBox.Show(this, L.T("Selecione exatamente um arquivo na lista."), Text);
        }

        private void miListCopyPath_Click(object? sender, EventArgs e)
        {
            var names = SelectedItems().Select(i => i.Name).ToList();
            if (names.Count > 0) Clipboard.SetText(string.Join(Environment.NewLine, names));
        }

        private void files_DragEnter(object? sender, DragEventArgs e) =>
            e.Effect = e.Data?.GetDataPresent(DataFormats.FileDrop) == true ? DragDropEffects.Copy : DragDropEffects.None;

        private void files_DragDrop(object? sender, DragEventArgs e)
        {
            emptyState.DragHover = false;
            if (e.Data?.GetData(DataFormats.FileDrop) is string[] f) DropFiles(f);
        }

        private void emptyState_DragEnter(object? sender, DragEventArgs e)
        {
            files_DragEnter(sender, e);
            emptyState.DragHover = e.Effect != DragDropEffects.None;
        }

        private void emptyState_DragLeave(object? sender, EventArgs e) => emptyState.DragHover = false;

        #endregion

        #region Abrir / navegar

        private void OpenRcf()
        {
            if (!ConfirmDiscard()) return;
            using var dlg = new OpenFileDialog { Filter = L.T("Arquivos RCF (*.rcf)|*.rcf|Todos os arquivos (*.*)|*.*"), Title = L.T("Abrir RCF") };
            if (dlg.ShowDialog(this) == DialogResult.OK) LoadRcf(dlg.FileName);
        }

        private bool ConfirmDiscard() => (!HasChanges && !EditorsDirty) || MessageBox.Show(this,
            L.T("Há alterações não salvas. Descartá-las?"), Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;

        private void LoadRcf(string path, bool keepEditors = false)
        {
            if (!keepEditors) foreach (var tab in _editors.Keys.ToList()) CloseEditor(tab, ask: false);
            try
            {
                var rcf = RcfArchive.Open(path);
                _rcf = rcf;
                _replaced.Clear();
                _added.Clear();
                _folder = "";
                searchBox.Box.Text = "";
                RebuildItems();
                Log(L.F("Aberto: {0} — {1:N0} arquivos, {2:N0} bytes.", path, rcf.Entries.Count, rcf.FileLength) +
                    L.T(rcf.HashesVerified ? " Hashes conferidos." : " AVISO: hashes não conferem com os nomes."));
            }
            catch (Exception ex) { ShowError(ex); }
            UpdateState();
        }

        /// <summary>Recalcula a lista de itens (original + substituições + novos) e redesenha tudo.</summary>
        private void RebuildItems()
        {
            if (_rcf == null) return;
            var items = _rcf.Entries.Select(e => new Item
            {
                Name = e.Name,
                Entry = e,
                Replaced = _replaced.ContainsKey(e.Index),
                Source = _replaced.TryGetValue(e.Index, out var s) ? s : new ArchiveSource(_rcf.FilePath, e.Offset, e.Size)
            }).ToList();
            items.AddRange(_added.Select(a => new Item { Name = a.name, Source = a.source }));
            _items = items;
            FillTree();
            FillList();
            UpdateState();
        }

        private void FillTree()
        {
            treeFolders.BeginUpdate();
            treeFolders.Nodes.Clear();
            if (_rcf != null)
            {
                var root = new TreeNode(Path.GetFileName(_rcf.FilePath)) { Tag = "" };
                var nodes = new Dictionary<string, TreeNode>(StringComparer.OrdinalIgnoreCase) { [""] = root };
                foreach (var folder in _items.Select(i => i.Folder).Distinct(StringComparer.OrdinalIgnoreCase)
                                             .OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
                    EnsureNode(folder, nodes);
                treeFolders.Nodes.Add(root);
                root.Expand();
                if (nodes.TryGetValue(_folder, out var sel)) treeFolders.SelectedNode = sel;
            }
            treeFolders.EndUpdate();
        }

        private static TreeNode EnsureNode(string folder, Dictionary<string, TreeNode> nodes)
        {
            if (nodes.TryGetValue(folder, out var n)) return n;
            var parent = EnsureNode(Path.GetDirectoryName(folder) ?? "", nodes);
            n = new TreeNode(Path.GetFileName(folder)) { Tag = folder };
            parent.Nodes.Add(n);
            nodes[folder] = n;
            return n;
        }

        private void treeFolders_AfterSelect(object? sender, TreeViewEventArgs e)
        {
            if (e.Node?.Tag is string f && f != _folder) Navigate(f, fromTree: true);
        }

        private void treeFolders_NodeMouseClick(object? sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right) treeFolders.SelectedNode = e.Node;
        }

        private void Navigate(string folder, bool fromTree = false)
        {
            _folder = folder;
            if (searchBox.Box.Text.Length > 0) searchBox.Box.Text = ""; // FillList pelo TextChanged
            else FillList();
            if (!fromTree) SelectTreeNode(folder);
        }

        private void SelectTreeNode(string folder)
        {
            foreach (TreeNode root in treeFolders.Nodes)
            {
                var node = Find(root);
                if (node != null) { treeFolders.SelectedNode = node; node.EnsureVisible(); }
            }
            TreeNode? Find(TreeNode n)
            {
                if (string.Equals(n.Tag as string, folder, StringComparison.OrdinalIgnoreCase)) return n;
                foreach (TreeNode c in n.Nodes) { var r = Find(c); if (r != null) return r; }
                return null;
            }
        }

        private void GoUp()
        {
            if (searchBox.Box.Text.Length > 0) { searchBox.Box.Text = ""; return; }
            if (_folder.Length > 0) Navigate(Path.GetDirectoryName(_folder) ?? "");
        }

        private void FillList()
        {
            _hotRow = -1;
            listFiles.BeginUpdate();
            listFiles.Items.Clear();
            string q = searchBox.Box.Text.Trim();
            if (q.Length > 0)
            {
                foreach (var it in _items.Where(i => i.Name.Contains(q, StringComparison.OrdinalIgnoreCase)))
                    listFiles.Items.Add(MakeListItem(it, it.Name));
                lblPath.Text = L.F("Busca: “{0}”", q);
                cardFiles.Note = L.F("{0:N0} resultado(s)", listFiles.Items.Count);
            }
            else
            {
                string prefix = _folder.Length > 0 ? _folder + "\\" : "";
                var subfolders = _items.Where(i => i.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                                       .Select(i => i.Name[prefix.Length..])
                                       .Where(r => r.Contains('\\'))
                                       .Select(r => r[..r.IndexOf('\\')])
                                       .Distinct(StringComparer.OrdinalIgnoreCase)
                                       .OrderBy(s => s, StringComparer.OrdinalIgnoreCase);
                int folders = 0;
                foreach (var sub in subfolders)
                {
                    string full = prefix + sub;
                    var lvi = new ListViewItem(sub) { Tag = full, ToolTipText = full };
                    lvi.SubItems.Add(L.F("{0:N0} arq.", ItemsUnder(full).Count));
                    lvi.SubItems.Add("pasta");
                    listFiles.Items.Add(lvi);
                    folders++;
                }
                foreach (var it in _items.Where(i => string.Equals(i.Folder, _folder, StringComparison.OrdinalIgnoreCase))
                                         .OrderBy(i => i.Name, StringComparer.OrdinalIgnoreCase))
                    listFiles.Items.Add(MakeListItem(it, it.FileName));
                lblPath.Text = _rcf == null ? "" :
                    string.Join("  ›  ", new[] { Path.GetFileName(_rcf.FilePath) }.Concat(_folder.Split('\\', StringSplitOptions.RemoveEmptyEntries)));
                cardFiles.Note = _rcf == null ? "" : L.F("{0} pasta(s)  ·  {1:N0} arquivo(s)", folders, listFiles.Items.Count - folders);
            }
            listFiles.EndUpdate();
            UpdatePreview();
        }

        private static ListViewItem MakeListItem(Item it, string display)
        {
            var lvi = new ListViewItem(display) { Tag = it, ToolTipText = it.Name };
            lvi.SubItems.Add(FormatSize(it.Source.Length));
            lvi.SubItems.Add(Path.GetExtension(it.Name).TrimStart('.').ToLowerInvariant());
            lvi.SubItems.Add(it.Entry != null ? it.Entry.Date.ToString(L.T("dd/MM/yyyy HH:mm"), L.Culture) : "");
            lvi.SubItems.Add(RcfFormat.NameHash(it.Name).ToString("X8"));
            lvi.SubItems.Add(it.Added ? "novo" : it.Replaced ? "substituído" : "");
            return lvi;
        }

        private static string FormatSize(long b) =>
            b >= 1L << 30 ? $"{b / 1073741824.0:N2} GB" : b >= 1 << 20 ? $"{b / 1048576.0:N1} MB" : b >= 1024 ? $"{b / 1024.0:N1} KB" : $"{b:N0} B";

        private void listFiles_SelectedIndexChanged(object? sender, EventArgs e) => UpdatePreview();
        private void listFiles_DoubleClick(object? sender, EventArgs e) => OpenListSelection();

        private void listFiles_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) OpenListSelection();
            else if (e.KeyCode == Keys.Back) GoUp();
            else if (e.KeyCode == Keys.F && e.Control) searchBox.Box.Focus();
            else if (e.KeyCode == Keys.A && e.Control) { foreach (ListViewItem i in listFiles.Items) i.Selected = true; }
        }

        private void OpenListSelection()
        {
            if (listFiles.SelectedItems.Count != 1) return;
            var sel = listFiles.SelectedItems[0].Tag;
            if (sel is string folder) Navigate(folder);
            else if (sel is Item it)
            {
                if (IsTextP3d(it)) OpenTextEditor(it);
                else if (searchBox.Box.Text.Length > 0) { Navigate(it.Folder); SelectByName(it.Name); }
            }
        }

        private void SelectByName(string name)
        {
            foreach (ListViewItem lvi in listFiles.Items)
                if (lvi.Tag is Item i && i.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    lvi.Selected = true;
                    lvi.EnsureVisible();
                }
        }

        private List<Item> ItemsUnder(string folder)
        {
            if (folder.Length == 0) return _items.ToList();
            string prefix = folder + "\\";
            return _items.Where(i => i.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        /// <summary>Arquivos selecionados na lista, expandindo pastas selecionadas.</summary>
        private List<Item> SelectedItems()
        {
            var result = new List<Item>();
            foreach (ListViewItem lvi in listFiles.SelectedItems)
            {
                if (lvi.Tag is Item it) result.Add(it);
                else if (lvi.Tag is string folder) result.AddRange(ItemsUnder(folder));
            }
            return result.Distinct().ToList();
        }

        private List<Item> SelectedOrFolderItems()
        {
            var sel = SelectedItems();
            return sel.Count > 0 ? sel : ItemsUnder(_folder);
        }

        #endregion

        #region Desenho da árvore e da lista

        private void treeFolders_DrawNode(object? sender, DrawTreeNodeEventArgs e)
        {
            if (e.Node == null || e.Bounds.IsEmpty) return;
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var row = new Rectangle(0, e.Bounds.Y, treeFolders.ClientSize.Width, e.Bounds.Height);
            using (var bg = new SolidBrush(Theme.Surface)) g.FillRectangle(bg, row);

            bool selected = (e.State & TreeNodeStates.Selected) != 0;
            bool hot = (e.State & TreeNodeStates.Hot) != 0;
            var pill = new RectangleF(row.X + 2, row.Y + 2, row.Width - 6, row.Height - 4);
            if (selected)
            {
                Theme.FillRound(g, Theme.Alpha(Theme.Accent, 46), pill, 6);
                Theme.FillRound(g, Theme.Accent, new RectangleF(pill.X + 1, pill.Y + pill.Height * 0.25f, 3, pill.Height * 0.5f), 1.5f);
            }
            else if (hot) Theme.FillRound(g, Theme.Surface2, pill, 6);

            int x = this.Dpi(9) + e.Node.Level * treeFolders.Indent;
            if (e.Node.Nodes.Count > 0)
                Theme.Glyph(g, e.Node.IsExpanded ? Glyphs.ChevronDown : Glyphs.ChevronRight, Theme.Muted,
                    new RectangleF(x, row.Y, this.Dpi(16), row.Height), 7f);
            x += this.Dpi(18);
            bool root = e.Node.Level == 0;
            Theme.Glyph(g, root ? Glyphs.Package : e.Node.IsExpanded ? Glyphs.FolderOpen : Glyphs.Folder,
                root ? Theme.Accent : Theme.Folder, new RectangleF(x, row.Y, this.Dpi(20), row.Height), 10f);
            x += this.Dpi(26);
            TextRenderer.DrawText(g, e.Node.Text, root ? Theme.UiSemibold : Theme.Ui, new Rectangle(x, row.Y, row.Right - x - 4, row.Height),
                selected ? Theme.Text : Theme.Blend(Theme.Text, Theme.Muted, 0.25f),
                TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        /// <summary>Nome ocupa o espaço livre; em listas estreitas Hash e depois Data são ocultadas.</summary>
        private void FitColumns()
        {
            int avail = listFiles.ClientSize.Width - 2;
            int size = this.Dpi(84), type = this.Dpi(64), date = this.Dpi(124), hash = this.Dpi(86), status = this.Dpi(104);
            int minName = this.Dpi(220);
            if (avail - (size + type + date + hash + status) < minName) hash = 0;
            if (avail - (size + type + date + status) < minName) date = 0;
            listFiles.BeginUpdate();
            colSize.Width = size; colType.Width = type; colDate.Width = date; colHash.Width = hash; colStatus.Width = status;
            colName.Width = Math.Max(this.Dpi(140), avail - (size + type + date + hash + status));
            listFiles.EndUpdate();
        }

        private void listFiles_DrawColumnHeader(object? sender, DrawListViewColumnHeaderEventArgs e)
        {
            var g = e.Graphics;
            using (var bg = new SolidBrush(Theme.Surface)) g.FillRectangle(bg, e.Bounds);
            using (var pen = new Pen(Theme.Border)) g.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
            var flags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix |
                        (e.Header?.TextAlign == HorizontalAlignment.Right ? TextFormatFlags.Right : TextFormatFlags.Left);
            var r = Rectangle.Inflate(e.Bounds, -this.Dpi(10), 0);
            TextRenderer.DrawText(g, e.Header?.Text?.ToUpperInvariant(), Theme.SmallBold, r, Theme.Muted, flags);
        }

        private void listFiles_DrawItem(object? sender, DrawListViewItemEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var row = new Rectangle(0, e.Bounds.Y, listFiles.ClientSize.Width, e.Bounds.Height);
            using (var bg = new SolidBrush(Theme.Surface)) g.FillRectangle(bg, row);
            var pill = new RectangleF(row.X + 2, row.Y + 1, row.Width - 5, row.Height - 2);
            if (e.Item.Selected)
            {
                Theme.FillRound(g, Theme.Alpha(Theme.Accent, listFiles.Focused ? 52 : 36), pill, 6);
                Theme.FillRound(g, Theme.Accent, new RectangleF(pill.X + 1, pill.Y + pill.Height * 0.25f, 3, pill.Height * 0.5f), 1.5f);
            }
            else if (e.ItemIndex == _hotRow) Theme.FillRound(g, Theme.Surface2, pill, 6);
        }

        private void listFiles_DrawSubItem(object? sender, DrawListViewSubItemEventArgs e)
        {
            if (e.Item == null || e.SubItem == null) return;
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var r = Rectangle.Inflate(e.Bounds, -this.Dpi(8), 0);
            const TextFormatFlags baseFlags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix;
            bool isFolder = e.Item.Tag is string;
            var item = e.Item.Tag as Item;

            switch (e.ColumnIndex)
            {
                case 0:
                    var (glyph, color) = isFolder ? (Glyphs.Folder, Theme.Folder) : IconFor(e.Item.Text);
                    Theme.Glyph(g, glyph, color, new RectangleF(r.X, r.Y, this.Dpi(20), r.Height), 10.5f);
                    var tr = new Rectangle(r.X + this.Dpi(28), r.Y, r.Width - this.Dpi(28), r.Height);
                    TextRenderer.DrawText(g, e.SubItem.Text, isFolder ? Theme.UiSemibold : Theme.Ui, tr, Theme.Text, baseFlags);
                    break;
                case 1:
                    TextRenderer.DrawText(g, e.SubItem.Text, Theme.Small, r, Theme.Muted, baseFlags | TextFormatFlags.Right);
                    break;
                case 2:
                    if (e.SubItem.Text.Length > 0) Pill(g, r, isFolder ? L.T("PASTA") : e.SubItem.Text.ToUpperInvariant(), Theme.Muted, Theme.Surface2);
                    break;
                case 3:
                    TextRenderer.DrawText(g, e.SubItem.Text, Theme.Small, r, Theme.Muted, baseFlags);
                    break;
                case 4:
                    TextRenderer.DrawText(g, e.SubItem.Text, Theme.Mono, r, Theme.Faint, baseFlags);
                    break;
                case 5:
                    if (item?.Added == true) Pill(g, r, L.T("NOVO"), Theme.Green, Theme.Alpha(Theme.Green, 40));
                    else if (item?.Replaced == true) Pill(g, r, L.T("SUBSTITUÍDO"), Theme.Blue, Theme.Alpha(Theme.Blue, 40));
                    break;
            }
        }

        private void Pill(Graphics g, Rectangle cell, string text, Color fg, Color bg)
        {
            var size = TextRenderer.MeasureText(text, Theme.SmallBold);
            var r = new RectangleF(cell.X, cell.Y + (cell.Height - this.Dpi(20)) / 2f,
                Math.Min(cell.Width, size.Width + this.Dpi(6)), this.Dpi(20));
            Theme.FillRound(g, bg, r, r.Height / 2);
            TextRenderer.DrawText(g, text, Theme.SmallBold, Rectangle.Round(r), fg,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
        }

        private static (string glyph, Color color) IconFor(string name) => Path.GetExtension(name).ToLowerInvariant() switch
        {
            ".p3d" => (Glyphs.Package, Theme.Accent),
            ".rcf" => (Glyphs.Package, Theme.Folder),
            ".cso" or ".dso" or ".cs" or ".fsc" => (Glyphs.Code, Theme.Blue),
            ".rsd" or ".wav" or ".mp3" => (Glyphs.Audio, Theme.Green),
            ".txt" or ".dtr" => (Glyphs.Text, Theme.Muted),
            ".png" or ".bmp" or ".tga" or ".dds" => (Glyphs.Picture, Theme.Accent),
            _ => (Glyphs.Page, Theme.Muted)
        };

        private void listFiles_MouseMove(object? sender, MouseEventArgs e)
        {
            int idx = listFiles.GetItemAt(e.X, e.Y)?.Index ?? -1;
            if (idx == _hotRow) return;
            InvalidateRow(_hotRow);
            _hotRow = idx;
            InvalidateRow(_hotRow);
        }

        private void listFiles_MouseLeave(object? sender, EventArgs e)
        {
            InvalidateRow(_hotRow);
            _hotRow = -1;
        }

        private void InvalidateRow(int index)
        {
            if (index < 0 || index >= listFiles.Items.Count) return;
            var b = listFiles.Items[index].Bounds;
            listFiles.Invalidate(new Rectangle(0, b.Y, listFiles.ClientSize.Width, b.Height));
        }

        private void listImages_DrawItem(object? sender, DrawItemEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (var bg = new SolidBrush(Theme.Surface)) g.FillRectangle(bg, e.Bounds);
            if (e.Index < 0 || e.Index >= _previewImages.Count) return;
            var img = _previewImages[e.Index];
            bool sel = (e.State & DrawItemState.Selected) != 0;
            var pill = new RectangleF(e.Bounds.X + 2, e.Bounds.Y + 2, e.Bounds.Width - 5, e.Bounds.Height - 4);
            if (sel) Theme.FillRound(g, Theme.Alpha(Theme.Accent, 46), pill, 7);

            int t = e.Bounds.Height - this.Dpi(12);
            var thumbRect = new Rectangle(e.Bounds.X + this.Dpi(8), e.Bounds.Y + this.Dpi(6), t, t);
            using (var checker = new TextureBrush(picturePreview.BackgroundImage ?? Theme.Checker(6)))
            using (var path = Theme.Round(thumbRect, 5))
                g.FillPath(checker, path);
            if (Thumbnail(img) is { } thumb)
            {
                float k = Math.Min((float)t / thumb.Width, (float)t / thumb.Height);
                var dst = new RectangleF(thumbRect.X + (t - thumb.Width * k) / 2, thumbRect.Y + (t - thumb.Height * k) / 2, thumb.Width * k, thumb.Height * k);
                g.DrawImage(thumb, dst);
            }
            Theme.DrawRound(g, Theme.Border, thumbRect, 5);

            int x = thumbRect.Right + this.Dpi(10);
            var nameRect = new Rectangle(x, e.Bounds.Y + this.Dpi(7), e.Bounds.Right - x - 6, e.Bounds.Height / 2 - this.Dpi(4));
            var descRect = new Rectangle(x, e.Bounds.Y + e.Bounds.Height / 2, e.Bounds.Right - x - 6, e.Bounds.Height / 2 - this.Dpi(6));
            TextRenderer.DrawText(g, img.Name, Theme.UiSemibold, nameRect, Theme.Text, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter);
            TextRenderer.DrawText(g, img.Description, Theme.Small, descRect, Theme.Muted, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter);
        }

        private Image? Thumbnail(P3dGraphic img)
        {
            if (_thumbs.TryGetValue(img, out var cached)) return cached;
            Image? thumb = null;
            try
            {
                if (img.CanExport)
                {
                    using var full = Image.FromStream(new MemoryStream(img.ExportPng()));
                    int s = this.Dpi(64);
                    float k = Math.Min(1f, Math.Min((float)s / full.Width, (float)s / full.Height));
                    thumb = new Bitmap(full, Math.Max(1, (int)(full.Width * k)), Math.Max(1, (int)(full.Height * k)));
                }
            }
            catch { }
            _thumbs[img] = thumb;
            return thumb;
        }

        private void ClearThumbs()
        {
            foreach (var t in _thumbs.Values) t?.Dispose();
            _thumbs.Clear();
        }

        #endregion

        #region Exportar

        private void ExportSelected()
        {
            var sel = SelectedItems();
            if (sel.Count == 0) { MessageBox.Show(this, L.T("Selecione um ou mais arquivos/pastas na lista."), Text); return; }
            if (sel.Count == 1 && listFiles.SelectedItems.Count == 1 && listFiles.SelectedItems[0].Tag is Item one)
            {
                using var dlg = new SaveFileDialog { FileName = one.FileName, Title = L.T("Exportar arquivo"), Filter = L.T("Todos os arquivos (*.*)|*.*") };
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    using var fs = File.Create(dlg.FileName);
                    one.Source.CopyTo(fs, new byte[1 << 20], CancellationToken.None);
                    Log(L.F("Exportado: {0} → {1}", one.Name, dlg.FileName));
                }
                catch (Exception ex) { ShowError(ex); }
                return;
            }
            ExportItems(sel, false);
        }

        private async void ExportItems(List<Item> items, bool writeManifest)
        {
            if (_rcf == null || items.Count == 0) return;
            string? folder = PickFolder(L.T("Pasta de destino (os caminhos internos do RCF serão mantidos)"));
            if (folder == null) return;
            var rcf = _rcf;
            await RunBusy(L.T("Exportando"), (progress, ct) =>
            {
                var buffer = new byte[1 << 20];
                long total = items.Sum(i => i.Source.Length), done = 0;
                int n = 0;
                foreach (var it in items)
                {
                    ct.ThrowIfCancellationRequested();
                    string dest = Path.Combine(folder, RcfArchive.SafeRelativePath(it.Name));
                    Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
                    using (var fs = File.Create(dest)) it.Source.CopyTo(fs, buffer, ct);
                    if (it.Entry != null && !it.Replaced)
                        File.SetLastWriteTimeUtc(dest, DateTimeOffset.FromUnixTimeSeconds(it.Entry.Timestamp).UtcDateTime);
                    done += it.Source.Length;
                    progress.Report(new RcfProgress(++n, items.Count, done, total, it.Name));
                }
                if (writeManifest)
                    RcfManifest.FromArchive(rcf).Save(Path.Combine(folder, RcfManifest.FileName));
                Log(writeManifest
                    ? L.F("{0:N0} arquivo(s) exportado(s) para {1} (com {2}).", items.Count, folder, RcfManifest.FileName)
                    : L.F("{0:N0} arquivo(s) exportado(s) para {1}.", items.Count, folder));
            });
        }

        private string? PickFolder(string description)
        {
            using var dlg = new FolderBrowserDialog { Description = description, UseDescriptionForTitle = true, ShowNewFolderButton = true };
            return dlg.ShowDialog(this) == DialogResult.OK ? dlg.SelectedPath : null;
        }

        #endregion

        #region Importar / substituir

        private void ReplaceSelectedWithFile()
        {
            if (listFiles.SelectedItems.Count != 1 || listFiles.SelectedItems[0].Tag is not Item it)
            {
                MessageBox.Show(this, L.T("Selecione exatamente um arquivo na lista."), Text);
                return;
            }
            using var dlg = new OpenFileDialog
            {
                Title = L.F("Substituir {0}", it.FileName),
                FileName = it.FileName,
                Filter = $"{Path.GetExtension(it.Name)}|*{Path.GetExtension(it.Name)}|" + L.T("Todos os arquivos (*.*)|*.*")
            };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            ReplaceItem(it, new FileSource(dlg.FileName));
            RebuildItems();
            SelectByName(it.Name);
        }

        private void ReplaceItem(Item it, RcfSource source)
        {
            if (it.Entry != null) _replaced[it.Entry.Index] = source;
            else
            {
                int i = _added.FindIndex(a => a.name.Equals(it.Name, StringComparison.OrdinalIgnoreCase));
                if (i >= 0) _added[i] = (it.Name, source);
            }
            Log(L.F("Substituído: {0} ({1} → {2})", it.Name, FormatSize(it.Source.Length), FormatSize(source.Length)));
            if (it.Name.EndsWith(".p3d", StringComparison.OrdinalIgnoreCase) && source is FileSource fs)
            {
                var head = new byte[12];
                using (var f = File.OpenRead(fs.Path)) f.ReadAtLeast(head, head.Length, throwOnEndOfStream: false);
                if (!P3dFile.LooksLikeP3d(head)) Log(L.F("  ! aviso: {0} não parece ser um P3D válido.", Path.GetFileName(fs.Path)));
            }
        }

        private void ImportFilesToFolder()
        {
            using var dlg = new OpenFileDialog { Multiselect = true, Title = L.F("Importar para \\{0}", _folder) };
            if (dlg.ShowDialog(this) == DialogResult.OK) DropFiles(dlg.FileNames);
        }

        /// <summary>
        /// Arquivos soltos na janela: um .rcf abre o arquivo; os demais substituem, pelo nome, os da pasta atual
        /// (ou o selecionado, se for um só arquivo com nome diferente); pastas são importadas pelo caminho.
        /// </summary>
        private void DropFiles(string[] paths)
        {
            if (paths.Length == 1 && File.Exists(paths[0]) && paths[0].EndsWith(".rcf", StringComparison.OrdinalIgnoreCase) &&
                (_rcf == null || !ItemsUnder(_folder).Any(i => i.FileName.Equals(Path.GetFileName(paths[0]), StringComparison.OrdinalIgnoreCase))))
            {
                if (ConfirmDiscard()) LoadRcf(paths[0]);
                return;
            }
            if (_rcf == null) return;

            if (paths.Length == 1 && Directory.Exists(paths[0])) { ImportFolder(paths[0]); return; }

            var files = paths.Where(File.Exists).ToList();
            var inFolder = _items.Where(i => string.Equals(i.Folder, _folder, StringComparison.OrdinalIgnoreCase))
                                 .ToDictionary(i => i.FileName, StringComparer.OrdinalIgnoreCase);
            var map = new List<(string file, Item? target)>();
            foreach (var f in files) map.Add((f, inFolder.GetValueOrDefault(Path.GetFileName(f))));

            if (files.Count == 1 && map[0].target == null && listFiles.SelectedItems.Count == 1 && listFiles.SelectedItems[0].Tag is Item sel &&
                MessageBox.Show(this, L.F("Substituir '{0}' por '{1}'?", sel.FileName, Path.GetFileName(files[0])), Text,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                map[0] = (files[0], sel);

            int replaced = 0;
            foreach (var (file, target) in map.Where(m => m.target != null))
            {
                ReplaceItem(target!, new FileSource(file));
                replaced++;
            }
            var unmatched = map.Where(m => m.target == null).Select(m => m.file).ToList();
            if (unmatched.Count > 0 && MessageBox.Show(this,
                    L.F("{0} arquivo(s) não existem em \\{1}:", unmatched.Count, _folder) + "\n\n" +
                    string.Join("\n", unmatched.Take(10).Select(Path.GetFileName)) + (unmatched.Count > 10 ? "\n…" : "") +
                    "\n\n" + L.T("Adicionar como arquivos novos nesta pasta?"), Text,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                foreach (var f in unmatched) AddNew(Path.Combine(_folder, Path.GetFileName(f)), new FileSource(f));
            }
            if (replaced > 0 || unmatched.Count > 0) RebuildItems();
        }

        private void AddNew(string name, RcfSource source)
        {
            name = RcfFormat.NormalizeName(name);
            uint h = RcfFormat.NameHash(name);
            var clash = _items.FirstOrDefault(i => RcfFormat.NameHash(i.Name) == h);
            if (clash != null)
            {
                Log(L.F("  ! não adicionado: '{0}' tem o mesmo hash de '{1}'.", name, clash.Name));
                return;
            }
            _added.Add((name, source));
            Log(L.F("Novo arquivo: {0} ({1})", name, FormatSize(source.Length)));
        }

        private void ImportFolder() { var f = PickFolder(L.T("Pasta com arquivos exportados (mesma estrutura do RCF)")); if (f != null) ImportFolder(f); }

        private void ImportFolder(string folder)
        {
            var byName = _items.ToDictionary(i => i.Name, StringComparer.OrdinalIgnoreCase);
            int replaced = 0, same = 0;
            var unmatched = new List<(string name, string path)>();
            foreach (var file in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories))
            {
                string rel = RcfFormat.NormalizeName(Path.GetRelativePath(folder, file));
                if (rel.Equals(RcfManifest.FileName, StringComparison.OrdinalIgnoreCase)) continue;
                if (!byName.TryGetValue(rel, out var it)) { unmatched.Add((rel, file)); continue; }
                if (SameContent(it.Source, file)) { same++; continue; }
                ReplaceItem(it, new FileSource(file));
                replaced++;
            }
            Log(L.F("Importação de {0}: {1} substituído(s), {2} idêntico(s) ignorado(s), {3} sem correspondência.", folder, replaced, same, unmatched.Count));
            if (unmatched.Count > 0 && MessageBox.Show(this,
                    L.F("{0} arquivo(s) da pasta não existem no RCF, por exemplo:", unmatched.Count) + "\n\n" +
                    string.Join("\n", unmatched.Take(10).Select(u => u.name)) + (unmatched.Count > 10 ? "\n…" : "") +
                    "\n\n" + L.T("Adicioná-los como arquivos novos?"), Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                foreach (var (name, path) in unmatched) AddNew(name, new FileSource(path));
            }
            RebuildItems();
        }

        private static bool SameContent(RcfSource current, string file)
        {
            var fi = new FileInfo(file);
            if (fi.Length != current.Length) return false;
            if (fi.Length > 64 << 20) return false; // arquivos enormes: assume alterado
            return current.ReadAll().AsSpan().SequenceEqual(File.ReadAllBytes(file));
        }

        private void RevertSelected()
        {
            foreach (var it in SelectedItems())
            {
                if (it.Entry != null && _replaced.Remove(it.Entry.Index)) Log(L.F("Desfeito: {0}", it.Name));
                else if (it.Added && _added.RemoveAll(a => a.name.Equals(it.Name, StringComparison.OrdinalIgnoreCase)) > 0)
                    Log(L.F("Removido (novo): {0}", it.Name));
            }
            RebuildItems();
        }

        private void RevertAll()
        {
            if (!HasChanges || MessageBox.Show(this, L.T("Desfazer todas as alterações pendentes?"), Text,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            _replaced.Clear();
            _added.Clear();
            Log(L.T("Todas as alterações foram desfeitas."));
            RebuildItems();
        }

        #endregion

        #region Salvar / compilar

        private async void SaveRcf(bool saveAs) => await SaveRcfAsync(saveAs);

        private async Task<bool> SaveRcfAsync(bool saveAs)
        {
            if (_rcf == null) return false;
            string target = _rcf.FilePath;
            if (saveAs)
            {
                using var dlg = new SaveFileDialog
                {
                    Filter = L.T("Arquivos RCF (*.rcf)|*.rcf"),
                    FileName = Path.GetFileName(_rcf.FilePath),
                    InitialDirectory = Path.GetDirectoryName(_rcf.FilePath),
                    Title = L.T("Salvar RCF como")
                };
                if (dlg.ShowDialog(this) != DialogResult.OK) return false;
                target = dlg.FileName;
            }
            else
            {
                if (!HasChanges) { MessageBox.Show(this, L.T("Não há alterações para salvar."), Text); return false; }
                string bak = _rcf.FilePath + ".bak";
                if (MessageBox.Show(this, L.F("Regravar {0}?", Path.GetFileName(_rcf.FilePath)) +
                        (File.Exists(bak) ? "" : "\n\n" + L.F("Uma cópia do original será guardada em {0}.", Path.GetFileName(bak))),
                        Text, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK) return false;
            }

            var rcf = _rcf;
            var builder = RcfBuilder.FromArchive(rcf, new Dictionary<int, RcfSource>(_replaced), _added.ToList());
            bool ok = await RunBusy(L.T("Salvando RCF"), (progress, ct) =>
            {
                string bak = rcf.FilePath + ".bak";
                if (string.Equals(Path.GetFullPath(target), Path.GetFullPath(rcf.FilePath), StringComparison.OrdinalIgnoreCase) && !File.Exists(bak))
                {
                    Log(L.F("Criando backup {0}…", bak));
                    File.Copy(rcf.FilePath, bak);
                }
                builder.Build(target, progress, Log, ct);
            });
            if (ok)
            {
                string folder = _folder;
                LoadRcf(target, keepEditors: true);
                Navigate(folder);
            }
            return ok;
        }

        private async void CompileFolder()
        {
            string? folder = PickFolder(L.F("Pasta a compilar (de preferência extraída com \"Exportar tudo\", que gera o {0})", RcfManifest.FileName));
            if (folder == null) return;
            string suggested = Path.GetFileName(folder.TrimEnd('\\')) + ".rcf";
            if (File.Exists(Path.Combine(folder, RcfManifest.FileName)))
            {
                try { suggested = RcfManifest.Load(Path.Combine(folder, RcfManifest.FileName)).SourceFile; } catch { }
            }
            using var dlg = new SaveFileDialog { Filter = L.T("Arquivos RCF (*.rcf)|*.rcf"), FileName = suggested, Title = L.T("Salvar RCF compilado") };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            RcfBuilder builder;
            try { builder = RcfBuilder.FromFolder(folder, false, Log); }
            catch (FileNotFoundException ex)
            {
                if (MessageBox.Show(this, ex.Message + "\n\n" + L.T("Compilar mesmo assim, sem esses arquivos?"), Text,
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                builder = RcfBuilder.FromFolder(folder, true, Log);
            }
            catch (Exception ex) { ShowError(ex); return; }

            bool ok = await RunBusy(L.T("Compilando"), (progress, ct) => builder.Build(dlg.FileName, progress, Log, ct));
            if (ok && MessageBox.Show(this, L.T("RCF compilado. Abrir no explorador?"), Text, MessageBoxButtons.YesNo) == DialogResult.Yes && ConfirmDiscard())
                LoadRcf(dlg.FileName);
        }

        #endregion

        #region Pré-visualização e imagens

        private void UpdatePreview()
        {
            listImages.Items.Clear();
            ClearThumbs();
            _previewImages = new();
            _previewP3d = null;
            _previewItem = null;
            SetPicture(null);
            cardPreview.Note = "";
            btnExportImage.Enabled = btnReplaceImage.Enabled = false;

            if (listFiles.SelectedItems.Count != 1 || listFiles.SelectedItems[0].Tag is not Item it)
            {
                int n = SelectedItems().Count;
                lblInfo.Text = n > 0 ? L.F("{0:N0} arquivo(s) selecionado(s).", n) : _rcf == null ? "" : L.T("Selecione um arquivo para ver os detalhes.");
                return;
            }

            _previewItem = it;
            lblInfo.Text = $"{it.FileName}\n{FormatSize(it.Source.Length)}  ·  hash {RcfFormat.NameHash(it.Name):X8}" +
                           (it.Entry != null ? $"  ·  offset {it.Entry.Offset:N0}" : "") +
                           (it.Added ? L.T("  ·  NOVO") : it.Replaced ? L.T("  ·  SUBSTITUÍDO") : "");
            if (!it.Name.EndsWith(".p3d", StringComparison.OrdinalIgnoreCase) || it.Source.Length > 256 << 20) return;
            try
            {
                var bytes = it.Source.ReadAll();
                var p3d = P3dFile.Parse(bytes);
                _previewP3d = p3d;
                if (P3dTextDocument.LooksLikeText(bytes) && P3dTextDocument.TryParse(bytes) is { } texts)
                    lblInfo.Text += "\n" + L.F("{0:N0} texto(s) — clique duas vezes para editar.", texts.Entries.Count);
                _previewImages = P3dGraphic.FindAll(p3d);
                foreach (var img in _previewImages) listImages.Items.Add(img.Name);
                cardPreview.Note = L.F("{0} imagem(ns)", _previewImages.Count);
                if (listImages.Items.Count > 0) listImages.SelectedIndex = 0;
            }
            catch (Exception ex) { lblInfo.Text += "\n" + L.F("P3D não pôde ser lido: {0}", ex.Message); }
        }

        private void listImages_SelectedIndexChanged(object? sender, EventArgs e)
        {
            int i = listImages.SelectedIndex;
            bool ok = i >= 0 && i < _previewImages.Count;
            btnExportImage.Enabled = btnReplaceImage.Enabled = ok;
            if (!ok || !_previewImages[i].CanExport) { SetPicture(null); return; }
            try { SetPicture(Image.FromStream(new MemoryStream(_previewImages[i].ExportPng()))); }
            catch { SetPicture(null); }
        }

        private void SetPicture(Image? img)
        {
            var old = picturePreview.Image;
            picturePreview.Image = img;
            old?.Dispose();
        }

        private void ExportPreviewImage()
        {
            int i = listImages.SelectedIndex;
            if (i < 0 || i >= _previewImages.Count) { MessageBox.Show(this, L.T("Selecione uma imagem de um .p3d."), Text); return; }
            var img = _previewImages[i];
            if (!img.CanExport) { MessageBox.Show(this, L.T("Esta imagem não está em PNG."), Text); return; }
            using var dlg = new SaveFileDialog { FileName = img.FileName, Filter = "PNG (*.png)|*.png", Title = L.T("Exportar imagem") };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            File.WriteAllBytes(dlg.FileName, img.ExportPng());
            Log(L.F("Imagem exportada: {0} → {1}", img.Name, dlg.FileName));
        }

        private void ReplacePreviewImage()
        {
            int i = listImages.SelectedIndex;
            if (_previewItem == null || _previewP3d == null || i < 0 || i >= _previewImages.Count)
            {
                MessageBox.Show(this, L.T("Selecione uma imagem de um .p3d."), Text);
                return;
            }
            var img = _previewImages[i];
            using var dlg = new OpenFileDialog { Filter = L.T("Imagens (*.png;*.bmp)|*.png;*.bmp"), Title = L.F("Substituir {0} ({1}×{2})", img.Name, img.Width, img.Height) };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            try
            {
                var warnings = img.Import(File.ReadAllBytes(dlg.FileName));
                if (warnings.Count > 0 && MessageBox.Show(this, string.Join("\n", warnings) + "\n\n" + L.T("Continuar mesmo assim?"), Text,
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                {
                    UpdatePreview(); // descarta a alteração em memória
                    return;
                }
                foreach (var w in warnings) Log("  ! " + w);
                var item = _previewItem;
                ReplaceItem(item, new BytesSource(_previewP3d.ToArray()));
                Log(L.F("Imagem trocada: {0} → {1}", item.Name, img.Name));
                RebuildItems();
                SelectByName(item.Name);
                if (i < listImages.Items.Count) listImages.SelectedIndex = i;
            }
            catch (Exception ex) { ShowError(ex); }
        }

        /// <summary>Exporta as imagens dos .p3d para &lt;pasta&gt;\&lt;caminho do p3d sem extensão&gt;\&lt;imagem&gt;.png.</summary>
        private async void ExportImages(List<Item> items)
        {
            var p3ds = items.Where(i => i.Name.EndsWith(".p3d", StringComparison.OrdinalIgnoreCase)).ToList();
            if (p3ds.Count == 0) { MessageBox.Show(this, L.T("Nenhum arquivo .p3d na seleção."), Text); return; }
            string? folder = PickFolder(L.T("Pasta para as imagens exportadas (edite-as e use \"Importar imagens editadas\")"));
            if (folder == null) return;
            await RunBusy(L.T("Exportando imagens"), (progress, ct) =>
            {
                int files = 0, images = 0, n = 0;
                foreach (var it in p3ds)
                {
                    ct.ThrowIfCancellationRequested();
                    progress.Report(new RcfProgress(++n, p3ds.Count, 0, 0, it.Name));
                    List<P3dGraphic> list;
                    try { list = P3dGraphic.FindAll(P3dFile.Parse(it.Source.ReadAll())); }
                    catch (Exception ex) { Log($"  ! {it.Name}: {ex.Message}"); continue; }
                    if (list.Count == 0) continue;
                    string dir = Path.Combine(folder, ImageFolderFor(it.Name));
                    Directory.CreateDirectory(dir);
                    foreach (var img in list.Where(x => x.CanExport))
                    {
                        File.WriteAllBytes(Path.Combine(dir, img.FileName), img.ExportPng());
                        images++;
                    }
                    files++;
                }
                Log(L.F("{0:N0} imagem(ns) de {1:N0} arquivo(s) .p3d exportada(s) para {2}", images, files, folder));
            });
        }

        private static string ImageFolderFor(string p3dName) =>
            RcfArchive.SafeRelativePath(Path.ChangeExtension(p3dName, null));

        /// <summary>Lê uma pasta gerada por ExportImages e troca, nos .p3d, só as imagens que mudaram.</summary>
        private async void ImportImages()
        {
            string? folder = PickFolder(L.T("Pasta com as imagens exportadas e editadas"));
            if (folder == null) return;
            var candidates = _items.Where(i => i.Name.EndsWith(".p3d", StringComparison.OrdinalIgnoreCase) &&
                                               Directory.Exists(Path.Combine(folder, ImageFolderFor(i.Name)))).ToList();
            if (candidates.Count == 0)
            {
                MessageBox.Show(this, L.T("Nenhuma subpasta corresponde a um .p3d deste RCF.\nUse a mesma pasta gerada por \"Exportar imagens\"."), Text);
                return;
            }
            var results = new List<(Item item, byte[] data)>();
            var warnings = new List<string>();
            int changedImages = 0;
            bool ok = await RunBusy(L.T("Importando imagens"), (progress, ct) =>
            {
                int n = 0;
                foreach (var it in candidates)
                {
                    ct.ThrowIfCancellationRequested();
                    progress.Report(new RcfProgress(++n, candidates.Count, 0, 0, it.Name));
                    string dir = Path.Combine(folder, ImageFolderFor(it.Name));
                    try
                    {
                        var p3d = P3dFile.Parse(it.Source.ReadAll());
                        bool changed = false;
                        foreach (var img in P3dGraphic.FindAll(p3d))
                        {
                            string png = Path.Combine(dir, img.FileName);
                            if (!File.Exists(png)) continue;
                            byte[] bytes = File.ReadAllBytes(png);
                            if (!img.CanExport || bytes.AsSpan().SequenceEqual(img.ExportPng())) continue;
                            warnings.AddRange(img.Import(bytes));
                            changed = true;
                            changedImages++;
                        }
                        if (changed) results.Add((it, p3d.ToArray()));
                    }
                    catch (Exception ex) { warnings.Add($"{it.Name}: {ex.Message}"); }
                }
            });
            if (!ok) return;
            foreach (var w in warnings) Log("  ! " + w);
            foreach (var (item, data) in results) ReplaceItem(item, new BytesSource(data));
            Log(L.F("Imagens importadas: {0} imagem(ns) alterada(s) em {1} arquivo(s) .p3d.", changedImages, results.Count));
            if (warnings.Count > 0)
                MessageBox.Show(this, L.F("{0} aviso(s) — veja o log.\nO jogo pode exigir o mesmo tamanho das imagens originais.", warnings.Count),
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            RebuildItems();
        }

        #endregion

        #region Editor de textos (abas)

        private bool EditorsDirty => _editors.Values.Any(v => v.Dirty);

        private static bool IsTextP3d(Item it)
        {
            if (!it.Name.EndsWith(".p3d", StringComparison.OrdinalIgnoreCase)) return false;
            try { return P3dTextDocument.LooksLikeText(it.Source.ReadHead(16)); }
            catch (IOException) { return false; }
        }

        /// <summary>Abre (ou mostra, se já estiver aberto) o editor de textos de um .p3d numa aba.</summary>
        private void OpenTextEditor(Item it)
        {
            var open = _editors.FirstOrDefault(p => p.Value.EntryName.Equals(it.Name, StringComparison.OrdinalIgnoreCase)).Key;
            if (open != null) { tabStrip.Selected = open; return; }

            P3dTextDocument? doc;
            try { doc = P3dTextDocument.TryParse(it.Source.ReadAll()); }
            catch (Exception ex) { ShowError(ex); return; }
            if (doc == null)
            {
                MessageBox.Show(this, L.F("{0} não tem textos (TextBible ou legenda).", it.FileName), Text);
                return;
            }
            // se o arquivo já foi trocado, a coluna "Original" mostra o texto que está no RCF do disco
            List<string>? original = null;
            if (it.Replaced && it.Entry != null && _rcf != null)
            {
                try { original = P3dTextDocument.TryParse(new ArchiveSource(_rcf.FilePath, it.Entry.Offset, it.Entry.Size).ReadAll())?.Entries.Select(x => x.Text).ToList(); }
                catch (IOException) { }
            }

            var view = new TextEditorView(it.Name, doc, original) { Dock = DockStyle.Fill, Visible = false };
            var tab = new TabItem { Text = it.FileName, Glyph = Glyphs.Text, ToolTip = it.Name };
            view.SaveRequested += SaveTextEditor;
            view.DirtyChanged += v => { tab.Dirty = v.Dirty; tabStrip.Invalidate(); };
            contentPanel.Controls.Add(view);
            L.Apply(view);
            view.RefreshTexts();
            _editors[tab] = view;
            tabStrip.Add(tab);
            Log(L.F("Editor de textos: {0} ({1:N0} textos).", it.Name, doc.Entries.Count));
        }

        private void tabStrip_SelectedChanged(object? sender, EventArgs e)
        {
            var view = tabStrip.Selected is { } t && _editors.TryGetValue(t, out var v) ? v : null;
            contentPanel.SuspendLayout();
            splitLog.Visible = view == null;
            foreach (var ed in _editors.Values) ed.Visible = ed == view;
            contentPanel.ResumeLayout();
        }

        private void tabStrip_CloseRequested(TabItem tab) => CloseEditor(tab, ask: true);

        private void CloseEditor(TabItem tab, bool ask)
        {
            if (!_editors.TryGetValue(tab, out var view)) return;
            if (ask && view.Dirty)
            {
                var answer = MessageBox.Show(this,
                    L.F("Aplicar as edições de {0} ao RCF antes de fechar?", Path.GetFileName(view.EntryName)) + "\n\n" +
                    L.T("O RCF fica marcado como alterado; use Salvar para gravar o arquivo."),
                    Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (answer == DialogResult.Cancel) return;
                if (answer == DialogResult.Yes && !ApplyTextEditor(view)) return;
            }
            _editors.Remove(tab);
            tabStrip.Remove(tab);
            contentPanel.Controls.Remove(view);
            view.Dispose();
            if (tabStrip.Selected == null) tabStrip.Selected = _explorerTab;
        }

        /// <summary>Gera o .p3d com os textos do editor e o coloca no lugar do arquivo no RCF (em memória).</summary>
        private bool ApplyTextEditor(TextEditorView view)
        {
            var it = _items.FirstOrDefault(i => i.Name.Equals(view.EntryName, StringComparison.OrdinalIgnoreCase));
            if (it == null)
            {
                MessageBox.Show(this, L.F("{0} não está mais no RCF.", view.EntryName), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            byte[] data;
            try { data = view.Document!.ToArray(); }
            catch (Exception ex) { ShowError(ex); return false; }
            if (!data.AsSpan().SequenceEqual(it.Source.ReadAll()))
            {
                ReplaceItem(it, new BytesSource(data));
                RebuildItems();
            }
            view.MarkApplied();
            return true;
        }

        /// <summary>"Salvar no RCF": aplica os textos e regrava o RCF.</summary>
        private async void SaveTextEditor(TextEditorView view)
        {
            if (_cts != null || !ApplyTextEditor(view)) return;
            if (!HasChanges) { statusBar.StatusText = L.T("Nenhum texto mudou — nada para gravar."); return; }
            await SaveRcfAsync(false);
        }

        #endregion

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}
