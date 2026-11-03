namespace Extrator_e_Compilador_RCF
{
    partial class MainForm
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
            components = new System.ComponentModel.Container();
            headerBar = new Extrator_e_Compilador_RCF.UI.HeaderBar();
            btnOpen = new Extrator_e_Compilador_RCF.UI.ModernButton();
            btnSave = new Extrator_e_Compilador_RCF.UI.ModernButton();
            btnSaveAs = new Extrator_e_Compilador_RCF.UI.ModernButton();
            btnExport = new Extrator_e_Compilador_RCF.UI.ModernButton();
            menuExport = new ContextMenuStrip(components);
            miExportSelected = new ToolStripMenuItem();
            miExportFolder = new ToolStripMenuItem();
            miExportAll = new ToolStripMenuItem();
            btnImport = new Extrator_e_Compilador_RCF.UI.ModernButton();
            menuImport = new ContextMenuStrip(components);
            miReplaceSelected = new ToolStripMenuItem();
            miImportFiles = new ToolStripMenuItem();
            miImportFolder = new ToolStripMenuItem();
            sepImport = new ToolStripSeparator();
            miRevertSelected = new ToolStripMenuItem();
            miRevertAll = new ToolStripMenuItem();
            btnImages = new Extrator_e_Compilador_RCF.UI.ModernButton();
            menuImages = new ContextMenuStrip(components);
            miImagesExportSelection = new ToolStripMenuItem();
            miImagesExportAll = new ToolStripMenuItem();
            miImagesImport = new ToolStripMenuItem();
            btnCompile = new Extrator_e_Compilador_RCF.UI.ModernButton();
            btnLanguage = new Extrator_e_Compilador_RCF.UI.ModernButton();
            btnSettings = new Extrator_e_Compilador_RCF.UI.ModernButton();
            btnHelp = new Extrator_e_Compilador_RCF.UI.ModernButton();
            statusBar = new Extrator_e_Compilador_RCF.UI.StatusBar();
            tabStrip = new Extrator_e_Compilador_RCF.UI.TabStrip();
            contentPanel = new Panel();
            splitLog = new SplitContainer();
            splitMain = new SplitContainer();
            cardFolders = new Extrator_e_Compilador_RCF.UI.CardPanel();
            treeFolders = new TreeView();
            menuTree = new ContextMenuStrip(components);
            miTreeExport = new ToolStripMenuItem();
            miTreeExportImages = new ToolStripMenuItem();
            splitRight = new SplitContainer();
            cardFiles = new Extrator_e_Compilador_RCF.UI.CardPanel();
            emptyState = new Extrator_e_Compilador_RCF.UI.EmptyState();
            listFiles = new Extrator_e_Compilador_RCF.UI.SmoothListView();
            colName = new ColumnHeader();
            colSize = new ColumnHeader();
            colType = new ColumnHeader();
            colDate = new ColumnHeader();
            colHash = new ColumnHeader();
            colStatus = new ColumnHeader();
            menuList = new ContextMenuStrip(components);
            miListEditTexts = new ToolStripMenuItem();
            miListExport = new ToolStripMenuItem();
            miListReplace = new ToolStripMenuItem();
            miListRevert = new ToolStripMenuItem();
            sepList = new ToolStripSeparator();
            miListExportImages = new ToolStripMenuItem();
            miListCopyPath = new ToolStripMenuItem();
            navBar = new Panel();
            lblPath = new Label();
            searchBox = new Extrator_e_Compilador_RCF.UI.SearchBox();
            btnUp = new Extrator_e_Compilador_RCF.UI.ModernButton();
            cardPreview = new Extrator_e_Compilador_RCF.UI.CardPanel();
            splitPreview = new SplitContainer();
            picturePreview = new PictureBox();
            listImages = new ListBox();
            imageButtons = new FlowLayoutPanel();
            btnReplaceImage = new Extrator_e_Compilador_RCF.UI.ModernButton();
            btnExportImage = new Extrator_e_Compilador_RCF.UI.ModernButton();
            lblInfo = new Label();
            cardLog = new Extrator_e_Compilador_RCF.UI.CardPanel();
            txtLog = new TextBox();
            headerBar.SuspendLayout();
            menuExport.SuspendLayout();
            menuImport.SuspendLayout();
            menuImages.SuspendLayout();
            contentPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitLog).BeginInit();
            splitLog.Panel1.SuspendLayout();
            splitLog.Panel2.SuspendLayout();
            splitLog.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            splitMain.SuspendLayout();
            cardFolders.SuspendLayout();
            menuTree.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitRight).BeginInit();
            splitRight.Panel1.SuspendLayout();
            splitRight.Panel2.SuspendLayout();
            splitRight.SuspendLayout();
            cardFiles.SuspendLayout();
            menuList.SuspendLayout();
            navBar.SuspendLayout();
            cardPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitPreview).BeginInit();
            splitPreview.Panel1.SuspendLayout();
            splitPreview.Panel2.SuspendLayout();
            splitPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picturePreview).BeginInit();
            imageButtons.SuspendLayout();
            cardLog.SuspendLayout();
            SuspendLayout();
            // 
            // headerBar
            // 
            headerBar.Controls.Add(btnOpen);
            headerBar.Controls.Add(btnSave);
            headerBar.Controls.Add(btnSaveAs);
            headerBar.Controls.Add(btnExport);
            headerBar.Controls.Add(btnImport);
            headerBar.Controls.Add(btnImages);
            headerBar.Controls.Add(btnCompile);
            headerBar.Controls.Add(btnLanguage);
            headerBar.Controls.Add(btnSettings);
            headerBar.Controls.Add(btnHelp);
            headerBar.Dock = DockStyle.Top;
            headerBar.Location = new Point(0, 0);
            headerBar.Name = "headerBar";
            headerBar.OnGlass = true;
            headerBar.Size = new Size(1280, 100);
            headerBar.Subtitle = "Scarface: The World Is Yours  ·  nenhum arquivo aberto";
            headerBar.TabIndex = 0;
            // 
            // btnOpen
            // 
            btnOpen.Font = new Font("Segoe UI Semibold", 9.5F);
            btnOpen.Glyph = "";
            btnOpen.Kind = UI.ButtonKind.Primary;
            btnOpen.Location = new Point(16, 54);
            btnOpen.Name = "btnOpen";
            btnOpen.OnGlass = true;
            btnOpen.Size = new Size(111, 34);
            btnOpen.TabIndex = 0;
            btnOpen.TabStop = false;
            btnOpen.Text = "Abrir RCF";
            btnOpen.Click += btnOpen_Click;
            // 
            // btnSave
            // 
            btnSave.Font = new Font("Segoe UI Semibold", 9.5F);
            btnSave.Glyph = "";
            btnSave.Location = new Point(135, 54);
            btnSave.Name = "btnSave";
            btnSave.OnGlass = true;
            btnSave.Size = new Size(90, 34);
            btnSave.TabIndex = 1;
            btnSave.TabStop = false;
            btnSave.Text = "Salvar";
            btnSave.Click += btnSave_Click;
            // 
            // btnSaveAs
            // 
            btnSaveAs.Font = new Font("Segoe UI Semibold", 9.5F);
            btnSaveAs.Glyph = "";
            btnSaveAs.Location = new Point(233, 54);
            btnSaveAs.Name = "btnSaveAs";
            btnSaveAs.OnGlass = true;
            btnSaveAs.Size = new Size(128, 34);
            btnSaveAs.TabIndex = 2;
            btnSaveAs.TabStop = false;
            btnSaveAs.Text = "Salvar como";
            btnSaveAs.Click += btnSaveAs_Click;
            // 
            // btnExport
            // 
            btnExport.Font = new Font("Segoe UI Semibold", 9.5F);
            btnExport.GapBefore = true;
            btnExport.Glyph = "";
            btnExport.Location = new Point(383, 54);
            btnExport.Menu = menuExport;
            btnExport.Name = "btnExport";
            btnExport.OnGlass = true;
            btnExport.Size = new Size(126, 34);
            btnExport.TabIndex = 3;
            btnExport.TabStop = false;
            btnExport.Text = "Exportar";
            // 
            // menuExport
            // 
            menuExport.Items.AddRange(new ToolStripItem[] { miExportSelected, miExportFolder, miExportAll });
            menuExport.Name = "menuExport";
            menuExport.Size = new Size(289, 70);
            // 
            // miExportSelected
            // 
            miExportSelected.Name = "miExportSelected";
            miExportSelected.Size = new Size(288, 22);
            miExportSelected.Tag = "";
            miExportSelected.Text = "Selecionados…";
            miExportSelected.Click += miExportSelected_Click;
            // 
            // miExportFolder
            // 
            miExportFolder.Name = "miExportFolder";
            miExportFolder.Size = new Size(288, 22);
            miExportFolder.Tag = "";
            miExportFolder.Text = "Pasta atual…";
            miExportFolder.Click += miExportFolder_Click;
            // 
            // miExportAll
            // 
            miExportAll.Name = "miExportAll";
            miExportAll.Size = new Size(288, 22);
            miExportAll.Tag = "";
            miExportAll.Text = "Tudo (com manifesto para recompilar)…";
            miExportAll.Click += miExportAll_Click;
            // 
            // btnImport
            // 
            btnImport.Font = new Font("Segoe UI Semibold", 9.5F);
            btnImport.Glyph = "";
            btnImport.Location = new Point(517, 54);
            btnImport.Menu = menuImport;
            btnImport.Name = "btnImport";
            btnImport.OnGlass = true;
            btnImport.Size = new Size(128, 34);
            btnImport.TabIndex = 4;
            btnImport.TabStop = false;
            btnImport.Text = "Importar";
            // 
            // menuImport
            // 
            menuImport.Items.AddRange(new ToolStripItem[] { miReplaceSelected, miImportFiles, miImportFolder, sepImport, miRevertSelected, miRevertAll });
            menuImport.Name = "menuImport";
            menuImport.Size = new Size(342, 120);
            // 
            // miReplaceSelected
            // 
            miReplaceSelected.Name = "miReplaceSelected";
            miReplaceSelected.Size = new Size(341, 22);
            miReplaceSelected.Tag = "";
            miReplaceSelected.Text = "Substituir o selecionado por um arquivo…";
            miReplaceSelected.Click += miReplaceSelected_Click;
            // 
            // miImportFiles
            // 
            miImportFiles.Name = "miImportFiles";
            miImportFiles.Size = new Size(341, 22);
            miImportFiles.Tag = "";
            miImportFiles.Text = "Arquivos para a pasta atual (substitui pelo nome)…";
            miImportFiles.Click += miImportFiles_Click;
            // 
            // miImportFolder
            // 
            miImportFolder.Name = "miImportFolder";
            miImportFolder.Size = new Size(341, 22);
            miImportFolder.Tag = "";
            miImportFolder.Text = "Pasta exportada (substitui pelo caminho)…";
            miImportFolder.Click += miImportFolder_Click;
            // 
            // sepImport
            // 
            sepImport.Name = "sepImport";
            sepImport.Size = new Size(338, 6);
            // 
            // miRevertSelected
            // 
            miRevertSelected.Name = "miRevertSelected";
            miRevertSelected.Size = new Size(341, 22);
            miRevertSelected.Tag = "";
            miRevertSelected.Text = "Desfazer alterações dos selecionados";
            miRevertSelected.Click += miRevertSelected_Click;
            // 
            // miRevertAll
            // 
            miRevertAll.Name = "miRevertAll";
            miRevertAll.Size = new Size(341, 22);
            miRevertAll.Tag = "";
            miRevertAll.Text = "Desfazer todas as alterações";
            miRevertAll.Click += miRevertAll_Click;
            // 
            // btnImages
            // 
            btnImages.Font = new Font("Segoe UI Semibold", 9.5F);
            btnImages.Glyph = "";
            btnImages.Location = new Point(653, 54);
            btnImages.Menu = menuImages;
            btnImages.Name = "btnImages";
            btnImages.OnGlass = true;
            btnImages.Size = new Size(126, 34);
            btnImages.TabIndex = 5;
            btnImages.TabStop = false;
            btnImages.Text = "Imagens";
            // 
            // menuImages
            // 
            menuImages.Items.AddRange(new ToolStripItem[] { miImagesExportSelection, miImagesExportAll, miImagesImport });
            menuImages.Name = "menuImages";
            menuImages.Size = new Size(336, 70);
            // 
            // miImagesExportSelection
            // 
            miImagesExportSelection.Name = "miImagesExportSelection";
            miImagesExportSelection.Size = new Size(335, 22);
            miImagesExportSelection.Tag = "";
            miImagesExportSelection.Text = "Exportar imagens dos selecionados / pasta atual…";
            miImagesExportSelection.Click += miImagesExportSelection_Click;
            // 
            // miImagesExportAll
            // 
            miImagesExportAll.Name = "miImagesExportAll";
            miImagesExportAll.Size = new Size(335, 22);
            miImagesExportAll.Tag = "";
            miImagesExportAll.Text = "Exportar todas as imagens do RCF…";
            miImagesExportAll.Click += miImagesExportAll_Click;
            // 
            // miImagesImport
            // 
            miImagesImport.Name = "miImagesImport";
            miImagesImport.Size = new Size(335, 22);
            miImagesImport.Tag = "";
            miImagesImport.Text = "Importar imagens editadas (pasta)…";
            miImagesImport.Click += miImagesImport_Click;
            // 
            // btnCompile
            // 
            btnCompile.Font = new Font("Segoe UI Semibold", 9.5F);
            btnCompile.GapBefore = true;
            btnCompile.Glyph = "";
            btnCompile.Location = new Point(801, 54);
            btnCompile.Name = "btnCompile";
            btnCompile.OnGlass = true;
            btnCompile.Size = new Size(187, 34);
            btnCompile.TabIndex = 6;
            btnCompile.TabStop = false;
            btnCompile.Text = "Compilar pasta → RCF";
            btnCompile.Click += btnCompile_Click;
            // 
            // btnLanguage
            // 
            btnLanguage.Font = new Font("Segoe UI Semibold", 9.5F);
            btnLanguage.Glyph = "";
            btnLanguage.Kind = UI.ButtonKind.Subtle;
            btnLanguage.Location = new Point(932, 12);
            btnLanguage.Name = "btnLanguage";
            btnLanguage.OnGlass = true;
            btnLanguage.Size = new Size(97, 34);
            btnLanguage.Slot = UI.HeaderSlot.Tool;
            btnLanguage.TabIndex = 7;
            btnLanguage.TabStop = false;
            btnLanguage.Text = "English";
            btnLanguage.Click += btnLanguage_Click;
            // 
            // btnSettings
            // 
            btnSettings.Font = new Font("Segoe UI Semibold", 9.5F);
            btnSettings.Glyph = "";
            btnSettings.Kind = UI.ButtonKind.Subtle;
            btnSettings.Location = new Point(1033, 12);
            btnSettings.Name = "btnSettings";
            btnSettings.OnGlass = true;
            btnSettings.Size = new Size(140, 34);
            btnSettings.Slot = UI.HeaderSlot.Tool;
            btnSettings.TabIndex = 8;
            btnSettings.TabStop = false;
            btnSettings.Text = "Configurações";
            btnSettings.Click += btnSettings_Click;
            // 
            // btnHelp
            // 
            btnHelp.Font = new Font("Segoe UI Semibold", 9.5F);
            btnHelp.Glyph = "";
            btnHelp.Kind = UI.ButtonKind.Subtle;
            btnHelp.Location = new Point(1177, 12);
            btnHelp.Name = "btnHelp";
            btnHelp.OnGlass = true;
            btnHelp.Size = new Size(89, 34);
            btnHelp.Slot = UI.HeaderSlot.Tool;
            btnHelp.TabIndex = 9;
            btnHelp.TabStop = false;
            btnHelp.Text = "Ajuda";
            btnHelp.Click += btnHelp_Click;
            // 
            // statusBar
            // 
            statusBar.Dock = DockStyle.Bottom;
            statusBar.Location = new Point(0, 740);
            statusBar.Name = "statusBar";
            statusBar.OnGlass = true;
            statusBar.Size = new Size(1280, 40);
            statusBar.StatusText = "Nenhum RCF aberto.";
            statusBar.TabIndex = 2;
            // 
            // tabStrip
            // 
            tabStrip.BackColor = Color.FromArgb(17, 17, 20);
            tabStrip.Dock = DockStyle.Top;
            tabStrip.ForeColor = Color.FromArgb(237, 237, 242);
            tabStrip.Location = new Point(0, 100);
            tabStrip.Name = "tabStrip";
            tabStrip.Size = new Size(1280, 40);
            tabStrip.TabIndex = 3;
            tabStrip.SelectedChanged += tabStrip_SelectedChanged;
            tabStrip.CloseRequested += tabStrip_CloseRequested;
            // 
            // contentPanel
            // 
            contentPanel.Controls.Add(splitLog);
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.Location = new Point(0, 100);
            contentPanel.Name = "contentPanel";
            contentPanel.Padding = new Padding(10);
            contentPanel.Size = new Size(1280, 640);
            contentPanel.TabIndex = 1;
            // 
            // splitLog
            // 
            splitLog.Dock = DockStyle.Fill;
            splitLog.FixedPanel = FixedPanel.Panel2;
            splitLog.Location = new Point(10, 10);
            splitLog.Name = "splitLog";
            splitLog.Orientation = Orientation.Horizontal;
            // 
            // splitLog.Panel1
            // 
            splitLog.Panel1.Controls.Add(splitMain);
            // 
            // splitLog.Panel2
            // 
            splitLog.Panel2.Controls.Add(cardLog);
            splitLog.Panel2Collapsed = true;
            splitLog.Size = new Size(1260, 620);
            splitLog.SplitterDistance = 460;
            splitLog.SplitterWidth = 10;
            splitLog.TabIndex = 0;
            // 
            // splitMain
            // 
            splitMain.Dock = DockStyle.Fill;
            splitMain.FixedPanel = FixedPanel.Panel1;
            splitMain.Location = new Point(0, 0);
            splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            splitMain.Panel1.Controls.Add(cardFolders);
            // 
            // splitMain.Panel2
            // 
            splitMain.Panel2.Controls.Add(splitRight);
            splitMain.Size = new Size(1260, 620);
            splitMain.SplitterDistance = 270;
            splitMain.SplitterWidth = 10;
            splitMain.TabIndex = 0;
            // 
            // cardFolders
            // 
            cardFolders.BackColor = Color.FromArgb(26, 26, 31);
            cardFolders.Controls.Add(treeFolders);
            cardFolders.Dock = DockStyle.Fill;
            cardFolders.Glyph = "";
            cardFolders.Location = new Point(0, 0);
            cardFolders.Name = "cardFolders";
            cardFolders.Size = new Size(270, 620);
            cardFolders.TabIndex = 0;
            cardFolders.Title = "Pastas";
            // 
            // treeFolders
            // 
            treeFolders.BackColor = Color.FromArgb(26, 26, 31);
            treeFolders.BorderStyle = BorderStyle.None;
            treeFolders.ContextMenuStrip = menuTree;
            treeFolders.Dock = DockStyle.Fill;
            treeFolders.DrawMode = TreeViewDrawMode.OwnerDrawAll;
            treeFolders.ForeColor = Color.FromArgb(237, 237, 242);
            treeFolders.FullRowSelect = true;
            treeFolders.HideSelection = false;
            treeFolders.HotTracking = true;
            treeFolders.Indent = 20;
            treeFolders.ItemHeight = 30;
            treeFolders.Location = new Point(10, 40);
            treeFolders.Name = "treeFolders";
            treeFolders.ShowLines = false;
            treeFolders.Size = new Size(250, 570);
            treeFolders.TabIndex = 0;
            treeFolders.DrawNode += treeFolders_DrawNode;
            treeFolders.AfterSelect += treeFolders_AfterSelect;
            treeFolders.NodeMouseClick += treeFolders_NodeMouseClick;
            // 
            // menuTree
            // 
            menuTree.Items.AddRange(new ToolStripItem[] { miTreeExport, miTreeExportImages });
            menuTree.Name = "menuTree";
            menuTree.Size = new Size(237, 48);
            // 
            // miTreeExport
            // 
            miTreeExport.Name = "miTreeExport";
            miTreeExport.Size = new Size(236, 22);
            miTreeExport.Tag = "";
            miTreeExport.Text = "Exportar esta pasta…";
            miTreeExport.Click += miExportFolder_Click;
            // 
            // miTreeExportImages
            // 
            miTreeExportImages.Name = "miTreeExportImages";
            miTreeExportImages.Size = new Size(236, 22);
            miTreeExportImages.Tag = "";
            miTreeExportImages.Text = "Exportar imagens desta pasta…";
            miTreeExportImages.Click += miTreeExportImages_Click;
            // 
            // splitRight
            // 
            splitRight.Dock = DockStyle.Fill;
            splitRight.FixedPanel = FixedPanel.Panel2;
            splitRight.Location = new Point(0, 0);
            splitRight.Name = "splitRight";
            // 
            // splitRight.Panel1
            // 
            splitRight.Panel1.Controls.Add(cardFiles);
            // 
            // splitRight.Panel2
            // 
            splitRight.Panel2.Controls.Add(cardPreview);
            splitRight.Size = new Size(980, 620);
            splitRight.SplitterDistance = 594;
            splitRight.SplitterWidth = 10;
            splitRight.TabIndex = 0;
            // 
            // cardFiles
            // 
            cardFiles.BackColor = Color.FromArgb(26, 26, 31);
            cardFiles.Controls.Add(emptyState);
            cardFiles.Controls.Add(listFiles);
            cardFiles.Controls.Add(navBar);
            cardFiles.Dock = DockStyle.Fill;
            cardFiles.Glyph = "";
            cardFiles.Location = new Point(0, 0);
            cardFiles.Name = "cardFiles";
            cardFiles.Size = new Size(594, 620);
            cardFiles.TabIndex = 0;
            cardFiles.Title = "Arquivos";
            // 
            // emptyState
            // 
            emptyState.AllowDrop = true;
            emptyState.BackColor = Color.FromArgb(26, 26, 31);
            emptyState.Dock = DockStyle.Fill;
            emptyState.Location = new Point(10, 84);
            emptyState.Name = "emptyState";
            emptyState.Size = new Size(574, 526);
            emptyState.TabIndex = 2;
            emptyState.DragDrop += files_DragDrop;
            emptyState.DragEnter += emptyState_DragEnter;
            emptyState.DragLeave += emptyState_DragLeave;
            // 
            // listFiles
            // 
            listFiles.AllowDrop = true;
            listFiles.BackColor = Color.FromArgb(26, 26, 31);
            listFiles.BorderStyle = BorderStyle.None;
            listFiles.Columns.AddRange(new ColumnHeader[] { colName, colSize, colType, colDate, colHash, colStatus });
            listFiles.ContextMenuStrip = menuList;
            listFiles.Dock = DockStyle.Fill;
            listFiles.ForeColor = Color.FromArgb(237, 237, 242);
            listFiles.FullRowSelect = true;
            listFiles.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            listFiles.Location = new Point(10, 84);
            listFiles.Name = "listFiles";
            listFiles.OwnerDraw = true;
            listFiles.ShowItemToolTips = true;
            listFiles.Size = new Size(574, 526);
            listFiles.TabIndex = 1;
            listFiles.UseCompatibleStateImageBehavior = false;
            listFiles.View = View.Details;
            listFiles.DrawColumnHeader += listFiles_DrawColumnHeader;
            listFiles.DrawItem += listFiles_DrawItem;
            listFiles.DrawSubItem += listFiles_DrawSubItem;
            listFiles.SelectedIndexChanged += listFiles_SelectedIndexChanged;
            listFiles.DragDrop += files_DragDrop;
            listFiles.DragEnter += files_DragEnter;
            listFiles.DoubleClick += listFiles_DoubleClick;
            listFiles.KeyDown += listFiles_KeyDown;
            listFiles.MouseLeave += listFiles_MouseLeave;
            listFiles.MouseMove += listFiles_MouseMove;
            // 
            // colName
            // 
            colName.Text = "Nome";
            colName.Width = 300;
            // 
            // colSize
            // 
            colSize.Text = "Tamanho";
            colSize.TextAlign = HorizontalAlignment.Right;
            colSize.Width = 90;
            // 
            // colType
            // 
            colType.Text = "Tipo";
            colType.Width = 64;
            // 
            // colDate
            // 
            colDate.Text = "Data";
            colDate.Width = 124;
            // 
            // colHash
            // 
            colHash.Text = "Hash";
            colHash.Width = 86;
            // 
            // colStatus
            // 
            colStatus.Text = "Status";
            colStatus.Width = 104;
            // 
            // menuList
            // 
            menuList.Items.AddRange(new ToolStripItem[] { miListEditTexts, miListExport, miListReplace, miListRevert, sepList, miListExportImages, miListCopyPath });
            menuList.Name = "menuList";
            menuList.Size = new Size(210, 120);
            // 
            // miListEditTexts
            // 
            miListEditTexts.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            miListEditTexts.Name = "miListEditTexts";
            miListEditTexts.Size = new Size(209, 22);
            miListEditTexts.Tag = UI.Glyphs.Text;
            miListEditTexts.Text = "Editar textos";
            miListEditTexts.Click += miListEditTexts_Click;
            // 
            // miListExport
            // 
            miListExport.Name = "miListExport";
            miListExport.Size = new Size(209, 22);
            miListExport.Tag = "";
            miListExport.Text = "Exportar…";
            miListExport.Click += miExportSelected_Click;
            // 
            // miListReplace
            // 
            miListReplace.Name = "miListReplace";
            miListReplace.Size = new Size(209, 22);
            miListReplace.Tag = "";
            miListReplace.Text = "Substituir por arquivo…";
            miListReplace.Click += miReplaceSelected_Click;
            // 
            // miListRevert
            // 
            miListRevert.Name = "miListRevert";
            miListRevert.Size = new Size(209, 22);
            miListRevert.Tag = "";
            miListRevert.Text = "Desfazer alteração";
            miListRevert.Click += miRevertSelected_Click;
            // 
            // sepList
            // 
            sepList.Name = "sepList";
            sepList.Size = new Size(206, 6);
            // 
            // miListExportImages
            // 
            miListExportImages.Name = "miListExportImages";
            miListExportImages.Size = new Size(209, 22);
            miListExportImages.Tag = "";
            miListExportImages.Text = "Exportar imagens (PNG)…";
            miListExportImages.Click += miImagesExportSelection_Click;
            // 
            // miListCopyPath
            // 
            miListCopyPath.Name = "miListCopyPath";
            miListCopyPath.Size = new Size(209, 22);
            miListCopyPath.Tag = "";
            miListCopyPath.Text = "Copiar caminho";
            miListCopyPath.Click += miListCopyPath_Click;
            // 
            // navBar
            // 
            navBar.BackColor = Color.FromArgb(26, 26, 31);
            navBar.Controls.Add(lblPath);
            navBar.Controls.Add(searchBox);
            navBar.Controls.Add(btnUp);
            navBar.Dock = DockStyle.Top;
            navBar.Location = new Point(10, 40);
            navBar.Name = "navBar";
            navBar.Size = new Size(574, 44);
            navBar.TabIndex = 0;
            // 
            // lblPath
            // 
            lblPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblPath.AutoEllipsis = true;
            lblPath.Font = new Font("Segoe UI Semibold", 9.75F);
            lblPath.ForeColor = Color.FromArgb(237, 237, 242);
            lblPath.Location = new Point(40, 2);
            lblPath.Name = "lblPath";
            lblPath.Size = new Size(254, 34);
            lblPath.TabIndex = 1;
            lblPath.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // searchBox
            // 
            searchBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            searchBox.BackColor = Color.FromArgb(36, 36, 43);
            searchBox.Location = new Point(304, 2);
            searchBox.Name = "searchBox";
            searchBox.Placeholder = "Buscar no RCF inteiro…";
            searchBox.Size = new Size(270, 34);
            searchBox.TabIndex = 2;
            // 
            // btnUp
            // 
            btnUp.BaseColor = Color.FromArgb(26, 26, 31);
            btnUp.Font = new Font("Segoe UI Semibold", 9.5F);
            btnUp.Glyph = "";
            btnUp.Kind = UI.ButtonKind.Subtle;
            btnUp.Location = new Point(0, 2);
            btnUp.Name = "btnUp";
            btnUp.Size = new Size(34, 34);
            btnUp.TabIndex = 0;
            btnUp.TabStop = false;
            btnUp.Click += btnUp_Click;
            // 
            // cardPreview
            // 
            cardPreview.BackColor = Color.FromArgb(26, 26, 31);
            cardPreview.Controls.Add(splitPreview);
            cardPreview.Controls.Add(lblInfo);
            cardPreview.Dock = DockStyle.Fill;
            cardPreview.Glyph = "";
            cardPreview.Location = new Point(0, 0);
            cardPreview.Name = "cardPreview";
            cardPreview.Size = new Size(376, 620);
            cardPreview.TabIndex = 0;
            cardPreview.Title = "Pré-visualização";
            // 
            // splitPreview
            // 
            splitPreview.BackColor = Color.FromArgb(26, 26, 31);
            splitPreview.Dock = DockStyle.Fill;
            splitPreview.Location = new Point(10, 102);
            splitPreview.Name = "splitPreview";
            splitPreview.Orientation = Orientation.Horizontal;
            // 
            // splitPreview.Panel1
            // 
            splitPreview.Panel1.Controls.Add(picturePreview);
            // 
            // splitPreview.Panel2
            // 
            splitPreview.Panel2.Controls.Add(listImages);
            splitPreview.Panel2.Controls.Add(imageButtons);
            splitPreview.Size = new Size(356, 508);
            splitPreview.SplitterDistance = 260;
            splitPreview.SplitterWidth = 8;
            splitPreview.TabIndex = 1;
            // 
            // picturePreview
            // 
            picturePreview.BackColor = Color.FromArgb(30, 30, 36);
            picturePreview.Dock = DockStyle.Fill;
            picturePreview.Location = new Point(0, 0);
            picturePreview.Name = "picturePreview";
            picturePreview.Size = new Size(356, 260);
            picturePreview.SizeMode = PictureBoxSizeMode.Zoom;
            picturePreview.TabIndex = 0;
            picturePreview.TabStop = false;
            // 
            // listImages
            // 
            listImages.BackColor = Color.FromArgb(26, 26, 31);
            listImages.BorderStyle = BorderStyle.None;
            listImages.Dock = DockStyle.Fill;
            listImages.DrawMode = DrawMode.OwnerDrawFixed;
            listImages.ForeColor = Color.FromArgb(237, 237, 242);
            listImages.IntegralHeight = false;
            listImages.ItemHeight = 54;
            listImages.Location = new Point(0, 0);
            listImages.Name = "listImages";
            listImages.Size = new Size(356, 192);
            listImages.TabIndex = 0;
            listImages.DrawItem += listImages_DrawItem;
            listImages.SelectedIndexChanged += listImages_SelectedIndexChanged;
            listImages.DoubleClick += btnExportImage_Click;
            // 
            // imageButtons
            // 
            imageButtons.BackColor = Color.FromArgb(26, 26, 31);
            imageButtons.Controls.Add(btnReplaceImage);
            imageButtons.Controls.Add(btnExportImage);
            imageButtons.Dock = DockStyle.Bottom;
            imageButtons.FlowDirection = FlowDirection.RightToLeft;
            imageButtons.Location = new Point(0, 192);
            imageButtons.Name = "imageButtons";
            imageButtons.Padding = new Padding(0, 8, 0, 0);
            imageButtons.Size = new Size(356, 48);
            imageButtons.TabIndex = 1;
            // 
            // btnReplaceImage
            // 
            btnReplaceImage.BaseColor = Color.FromArgb(26, 26, 31);
            btnReplaceImage.Font = new Font("Segoe UI Semibold", 9.5F);
            btnReplaceImage.Glyph = "";
            btnReplaceImage.Kind = UI.ButtonKind.Primary;
            btnReplaceImage.Location = new Point(185, 11);
            btnReplaceImage.Name = "btnReplaceImage";
            btnReplaceImage.Size = new Size(168, 34);
            btnReplaceImage.TabIndex = 0;
            btnReplaceImage.TabStop = false;
            btnReplaceImage.Text = "Substituir por PNG";
            btnReplaceImage.Click += btnReplaceImage_Click;
            // 
            // btnExportImage
            // 
            btnExportImage.BaseColor = Color.FromArgb(26, 26, 31);
            btnExportImage.Font = new Font("Segoe UI Semibold", 9.5F);
            btnExportImage.Glyph = "";
            btnExportImage.Location = new Point(42, 11);
            btnExportImage.Name = "btnExportImage";
            btnExportImage.Size = new Size(137, 34);
            btnExportImage.TabIndex = 1;
            btnExportImage.TabStop = false;
            btnExportImage.Text = "Exportar PNG";
            btnExportImage.Click += btnExportImage_Click;
            // 
            // lblInfo
            // 
            lblInfo.AutoEllipsis = true;
            lblInfo.Dock = DockStyle.Top;
            lblInfo.ForeColor = Color.FromArgb(156, 156, 168);
            lblInfo.Location = new Point(10, 40);
            lblInfo.Name = "lblInfo";
            lblInfo.Padding = new Padding(2, 0, 2, 8);
            lblInfo.Size = new Size(356, 62);
            lblInfo.TabIndex = 0;
            lblInfo.Text = "Selecione um arquivo para ver os detalhes.";
            // 
            // cardLog
            // 
            cardLog.BackColor = Color.FromArgb(26, 26, 31);
            cardLog.Controls.Add(txtLog);
            cardLog.Dock = DockStyle.Fill;
            cardLog.Glyph = "";
            cardLog.Location = new Point(0, 0);
            cardLog.Name = "cardLog";
            cardLog.Size = new Size(150, 46);
            cardLog.TabIndex = 0;
            cardLog.Title = "Log";
            // 
            // txtLog
            // 
            txtLog.BackColor = Color.FromArgb(26, 26, 31);
            txtLog.BorderStyle = BorderStyle.None;
            txtLog.Dock = DockStyle.Fill;
            txtLog.Font = new Font("Consolas", 9F);
            txtLog.ForeColor = Color.FromArgb(200, 200, 210);
            txtLog.Location = new Point(10, 40);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.Size = new Size(130, 0);
            txtLog.TabIndex = 0;
            // 
            // MainForm
            // 
            AllowDrop = true;
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(17, 17, 20);
            ClientSize = new Size(1280, 780);
            Controls.Add(contentPanel);
            Controls.Add(tabStrip);
            Controls.Add(statusBar);
            Controls.Add(headerBar);
            Font = new Font("Segoe UI", 9.75F);
            MinimumSize = new Size(980, 600);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Descompilador e Compilador de RCF/IMG — Scarface: The World Is Yours";
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            DragDrop += files_DragDrop;
            DragEnter += files_DragEnter;
            headerBar.ResumeLayout(false);
            menuExport.ResumeLayout(false);
            menuImport.ResumeLayout(false);
            menuImages.ResumeLayout(false);
            contentPanel.ResumeLayout(false);
            splitLog.Panel1.ResumeLayout(false);
            splitLog.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitLog).EndInit();
            splitLog.ResumeLayout(false);
            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            cardFolders.ResumeLayout(false);
            menuTree.ResumeLayout(false);
            splitRight.Panel1.ResumeLayout(false);
            splitRight.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitRight).EndInit();
            splitRight.ResumeLayout(false);
            cardFiles.ResumeLayout(false);
            menuList.ResumeLayout(false);
            navBar.ResumeLayout(false);
            cardPreview.ResumeLayout(false);
            splitPreview.Panel1.ResumeLayout(false);
            splitPreview.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitPreview).EndInit();
            splitPreview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picturePreview).EndInit();
            imageButtons.ResumeLayout(false);
            cardLog.ResumeLayout(false);
            cardLog.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private UI.HeaderBar headerBar;
        private UI.ModernButton btnOpen;
        private UI.ModernButton btnSave;
        private UI.ModernButton btnSaveAs;
        private UI.ModernButton btnExport;
        private UI.ModernButton btnImport;
        private UI.ModernButton btnImages;
        private UI.ModernButton btnCompile;
        private UI.ModernButton btnLanguage;
        private UI.ModernButton btnSettings;
        private UI.ModernButton btnHelp;
        private ContextMenuStrip menuExport;
        private ToolStripMenuItem miExportSelected;
        private ToolStripMenuItem miExportFolder;
        private ToolStripMenuItem miExportAll;
        private ContextMenuStrip menuImport;
        private ToolStripMenuItem miReplaceSelected;
        private ToolStripMenuItem miImportFiles;
        private ToolStripMenuItem miImportFolder;
        private ToolStripSeparator sepImport;
        private ToolStripMenuItem miRevertSelected;
        private ToolStripMenuItem miRevertAll;
        private ContextMenuStrip menuImages;
        private ToolStripMenuItem miImagesExportSelection;
        private ToolStripMenuItem miImagesExportAll;
        private ToolStripMenuItem miImagesImport;
        private UI.StatusBar statusBar;
        private Extrator_e_Compilador_RCF.UI.TabStrip tabStrip;
        private Panel contentPanel;
        private SplitContainer splitLog;
        private SplitContainer splitMain;
        private UI.CardPanel cardFolders;
        private TreeView treeFolders;
        private ContextMenuStrip menuTree;
        private ToolStripMenuItem miTreeExport;
        private ToolStripMenuItem miTreeExportImages;
        private SplitContainer splitRight;
        private UI.CardPanel cardFiles;
        private UI.EmptyState emptyState;
        private UI.SmoothListView listFiles;
        private ColumnHeader colName;
        private ColumnHeader colSize;
        private ColumnHeader colType;
        private ColumnHeader colDate;
        private ColumnHeader colHash;
        private ColumnHeader colStatus;
        private ContextMenuStrip menuList;
        private ToolStripMenuItem miListEditTexts;
        private ToolStripMenuItem miListExport;
        private ToolStripMenuItem miListReplace;
        private ToolStripMenuItem miListRevert;
        private ToolStripSeparator sepList;
        private ToolStripMenuItem miListExportImages;
        private ToolStripMenuItem miListCopyPath;
        private Panel navBar;
        private Label lblPath;
        private UI.SearchBox searchBox;
        private UI.ModernButton btnUp;
        private UI.CardPanel cardPreview;
        private SplitContainer splitPreview;
        private PictureBox picturePreview;
        private ListBox listImages;
        private FlowLayoutPanel imageButtons;
        private UI.ModernButton btnReplaceImage;
        private UI.ModernButton btnExportImage;
        private Label lblInfo;
        private UI.CardPanel cardLog;
        private TextBox txtLog;
    }
}
