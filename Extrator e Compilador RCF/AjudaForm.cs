using Extrator_e_Compilador_RCF.Core;
using Extrator_e_Compilador_RCF.UI;

namespace Extrator_e_Compilador_RCF
{
    /// <summary>Janela de ajuda com o passo a passo de cada recurso (português ou inglês).</summary>
    public partial class AjudaForm : ThemedForm
    {
        /// <summary>Seções da ajuda: título e tópicos, em português e inglês.</summary>
        private static readonly (string Pt, string En, (string Pt, string En)[] Items)[] Sections =
        [
            ("Explorador", "Explorer",
            [
                ("Abra um .rcf pelo botão Abrir RCF ou arrastando o arquivo para a janela.",
                 "Open an .rcf with the Open RCF button or by dropping the file onto the window."),
                ("Navegue pela árvore de pastas ou com duplo clique; ↑ ou Backspace sobe uma pasta.",
                 "Browse with the folder tree or by double-clicking; ↑ or Backspace goes up a folder."),
                ("A busca (Ctrl+F) procura no RCF inteiro; Esc limpa.",
                 "Search (Ctrl+F) looks through the whole RCF; Esc clears it."),
                ("Selecione um .p3d para ver as imagens dele na pré-visualização.",
                 "Select a .p3d to see its images in the preview."),
            ]),
            ("Exportar", "Export",
            [
                ("Um arquivo: selecione e use Exportar (abre “Salvar como”).",
                 "One file: select it and use Export (opens “Save as”)."),
                ("Vários arquivos ou pastas: vão para uma pasta mantendo os caminhos internos.",
                 "Several files or folders: go into a folder keeping their internal paths."),
                ($"Tudo: extrai o RCF inteiro + {RcfManifest.FileName}, que permite recompilar idêntico com “Compilar pasta → RCF”.",
                 $"Everything: extracts the whole RCF + {RcfManifest.FileName}, which lets “Build folder → RCF” rebuild it identically."),
            ]),
            ("Importar (substituir)", "Import (replace)",
            [
                ("Um arquivo: botão direito → Substituir por arquivo, ou arraste um arquivo com o mesmo nome para a lista.",
                 "One file: right-click → Replace with file, or drop a file with the same name onto the list."),
                ("Vários: Importar → Pasta exportada casa os arquivos pelo caminho (art\\…, scriptc\\…).",
                 "Several: Import → Exported folder matches files by path (art\\…, scriptc\\…)."),
                ("Arquivos que não existem no RCF podem ser adicionados como novos.",
                 "Files that don't exist in the RCF can be added as new ones."),
                ("As alterações ficam marcadas (SUBSTITUÍDO / NOVO) até você Salvar — dá para desfazer antes.",
                 "Changes stay marked (REPLACED / NEW) until you Save — you can undo them before that."),
            ]),
            ("Imagens (.p3d)", "Images (.p3d)",
            [
                ("Exporte em PNG, edite no seu editor favorito mantendo o mesmo tamanho e use Substituir por PNG.",
                 "Export as PNG, edit it in your favorite editor keeping the same size, and use Replace with PNG."),
                ("Em lote: Imagens → Exportar cria <pasta>\\<caminho do p3d>\\<imagem>.png; depois Importar imagens editadas na mesma pasta troca só as alteradas.",
                 "In batch: Images → Export creates <folder>\\<p3d path>\\<image>.png; then Import edited images from the same folder replaces only the changed ones."),
            ]),
            ("Editor de textos (.p3d)", "Text editor (.p3d)",
            [
                ("Dê dois cliques num .p3d com textos (textbible_E.p3d, HUD_textbible_E.p3d, legendas…) ou use botão direito → Editar textos: ele abre numa aba nova.",
                 "Double-click a .p3d with texts (textbible_E.p3d, HUD_textbible_E.p3d, subtitles…) or right-click → Edit texts: it opens in a new tab."),
                ("Edite a coluna Texto (duplo clique, Enter ou F2). Os textos alterados ficam destacados; Só alterados filtra a lista.",
                 "Edit the Text column (double-click, Enter or F2). Changed texts are highlighted; Changed only filters the list."),
                ("Salvar no RCF (Ctrl+S) gera o .p3d de novo e regrava o RCF. Exportar/Importar TXT usa o mesmo formato da Scarface P3D Text Tool (um texto por linha).",
                 "Save to RCF (Ctrl+S) rebuilds the .p3d and rewrites the RCF. Export/Import TXT uses the same format as the Scarface P3D Text Tool (one text per line)."),
            ]),
            ("Salvar e compilar", "Save and build",
            [
                ("Salvar regrava o RCF aberto e cria um .bak na primeira vez. Salvar como gera outro arquivo.",
                 "Save overwrites the open RCF and creates a .bak the first time. Save as writes a new file."),
                ("Compilar pasta → RCF monta um RCF a partir de uma pasta extraída (com ou sem manifesto).",
                 "Build folder → RCF assembles an RCF from an extracted folder (with or without a manifest)."),
            ]),
            ("Configurações e idioma", "Settings and language",
            [
                ("O botão English / Português no cabeçalho troca o idioma na hora (o português é o principal).",
                 "The English / Português button in the header switches the language instantly (Portuguese is the main one)."),
                ("Em Configurações escolha Mica ou Acrílico (Windows 11), a opacidade da janela e o idioma.",
                 "In Settings choose Mica or Acrylic (Windows 11), the window opacity and the language."),
            ]),
            ("Créditos", "Credits",
            [
                ("Ferramenta criada por MrGeH.",
                 "Tool created by MrGeH."),
            ]),
        ];

        public AjudaForm()
        {
            InitializeComponent();
            if (DesignMode) return;
            Native.DarkControl(rtbHelp);
            WriteHelp();
        }

        protected override void OnAppearanceChanged() => WriteHelp();

        private void WriteHelp()
        {
            rtbHelp.Clear();
            int indent = this.Dpi(22);
            foreach (var (pt, en, items) in Sections)
            {
                if (rtbHelp.TextLength > 0) rtbHelp.AppendText(Environment.NewLine);
                rtbHelp.SelectionIndent = 0;
                rtbHelp.SelectionHangingIndent = 0;
                rtbHelp.SelectionFont = new Font(rtbHelp.Font.FontFamily, 12f, FontStyle.Bold);
                rtbHelp.SelectionColor = Theme.Accent;
                rtbHelp.AppendText((L.English ? en : pt) + Environment.NewLine);

                foreach (var (itemPt, itemEn) in items)
                {
                    // marcador "•" com recuo deslocado: as linhas quebradas ficam alinhadas ao texto
                    rtbHelp.SelectionIndent = this.Dpi(6);
                    rtbHelp.SelectionHangingIndent = indent;
                    rtbHelp.SelectionTabs = [this.Dpi(6) + indent];
                    rtbHelp.SelectionFont = rtbHelp.Font;
                    rtbHelp.SelectionColor = Theme.Accent;
                    rtbHelp.AppendText("•\t");
                    rtbHelp.SelectionColor = Theme.Text;
                    rtbHelp.AppendText((L.English ? itemEn : itemPt) + Environment.NewLine);
                }
            }
            rtbHelp.Select(0, 0);
        }

        private void btnClose_Click(object? sender, EventArgs e) => Close();
    }
}
