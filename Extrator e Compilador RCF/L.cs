using System.Globalization;
using System.Runtime.CompilerServices;
using Extrator_e_Compilador_RCF.UI;

namespace Extrator_e_Compilador_RCF
{
    public enum AppLanguage { PtBr, EnUs }

    /// <summary>
    /// Localização: o texto em português (como está no código e no designer) é a chave; em inglês ele é trocado
    /// pela tradução do dicionário <see cref="English"/>. Texto sem tradução cai de volta no português.
    /// </summary>
    internal static class L
    {
        public static AppLanguage Current { get; private set; } = AppLanguage.PtBr;
        public static bool English => Current == AppLanguage.EnUs;
        public static CultureInfo Culture => English ? CultureInfo.GetCultureInfo("en-US") : CultureInfo.GetCultureInfo("pt-BR");

        public static void Set(AppLanguage lang)
        {
            Current = lang;
            // números, tamanhos e datas no formato do idioma (inclusive em threads de segundo plano)
            CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = Culture;
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = Culture;
        }

        /// <summary>Traduz um texto fixo.</summary>
        public static string T(string pt) => English && En.TryGetValue(pt, out var en) ? en : pt;

        /// <summary>Traduz um texto com marcadores {0}, {1}… e formata com a cultura do idioma.</summary>
        public static string F(string ptFormat, params object?[] args) => string.Format(Culture, T(ptFormat), args);

        #region Tradução dos textos do designer

        private static readonly ConditionalWeakTable<object, Dictionary<string, string>> Originals = new();

        private static string Original(object owner, string prop, string current)
        {
            var map = Originals.GetOrCreateValue(owner);
            if (!map.TryGetValue(prop, out var pt)) map[prop] = pt = current;
            return pt;
        }

        /// <summary>
        /// Aplica o idioma atual aos textos fixos de uma janela criada no designer: título, botões, rádios, títulos
        /// dos cartões, colunas, placeholders e menus. Textos dinâmicos (status, rótulos de caminho) são do código.
        /// </summary>
        public static void Apply(Control root)
        {
            if (root is Form f) f.Text = T(Original(f, "Text", f.Text));
            Walk(root);
        }

        private static void Walk(Control c)
        {
            switch (c)
            {
                case ModernButton or RadioButton or CheckBox:
                    c.Text = T(Original(c, "Text", c.Text));
                    break;
                case CardPanel card:
                    card.Title = T(Original(card, "Title", card.Title));
                    break;
                case HeaderBar header:
                    header.Title = T(Original(header, "Title", header.Title));
                    break;
                case SearchBox search:
                    search.Placeholder = T(Original(search, "Placeholder", search.Placeholder));
                    break;
                case ListView list:
                    foreach (ColumnHeader col in list.Columns) col.Text = T(Original(col, "Text", col.Text));
                    break;
            }
            if (c is ModernButton { Menu: { } menu }) ApplyMenu(menu);
            if (c.ContextMenuStrip != null) ApplyMenu(c.ContextMenuStrip);
            foreach (Control child in c.Controls) Walk(child);
        }

        private static void ApplyMenu(ToolStrip menu)
        {
            foreach (ToolStripItem item in menu.Items)
                if (item is ToolStripMenuItem mi) mi.Text = T(Original(mi, "Text", mi.Text ?? ""));
        }

        #endregion

        /// <summary>Português → inglês americano.</summary>
        private static readonly Dictionary<string, string> En = new()
        {
            // Janela principal (designer)
            ["Descompilador e Compilador de RCF/IMG/P3D — Scarface: The World Is Yours"] = "RCF/IMG/P3D Decompiler & Compiler — Scarface: The World Is Yours",
            ["Descompilador e Compilador de RCF/IMG/P3D"] = "RCF/IMG/P3D Decompiler & Compiler",
            ["Abrir RCF"] = "Open RCF",
            ["Abrir RCF…"] = "Open RCF…",
            ["Salvar"] = "Save",
            ["Salvar como"] = "Save as",
            ["Exportar"] = "Export",
            ["Importar"] = "Import",
            ["Imagens"] = "Images",
            ["Compilar pasta → RCF"] = "Build folder → RCF",
            ["Configurações"] = "Settings",
            ["Ajuda"] = "Help",
            ["Selecionados…"] = "Selected…",
            ["Pasta atual…"] = "Current folder…",
            ["Tudo (com manifesto para recompilar)…"] = "Everything (with manifest for rebuilding)…",
            ["Substituir o selecionado por um arquivo…"] = "Replace selected with a file…",
            ["Arquivos para a pasta atual (substitui pelo nome)…"] = "Files into current folder (replace by name)…",
            ["Pasta exportada (substitui pelo caminho)…"] = "Exported folder (replace by path)…",
            ["Desfazer alterações dos selecionados"] = "Undo changes to selected",
            ["Desfazer todas as alterações"] = "Undo all changes",
            ["Exportar imagens dos selecionados / pasta atual…"] = "Export images from selection / current folder…",
            ["Exportar todas as imagens do RCF…"] = "Export all images in the RCF…",
            ["Importar imagens editadas (pasta)…"] = "Import edited images (folder)…",
            ["Exportar esta pasta…"] = "Export this folder…",
            ["Exportar imagens desta pasta…"] = "Export images in this folder…",
            ["Exportar…"] = "Export…",
            ["Substituir por arquivo…"] = "Replace with file…",
            ["Desfazer alteração"] = "Undo change",
            ["Exportar imagens (PNG)…"] = "Export images (PNG)…",
            ["Copiar caminho"] = "Copy path",
            ["Pastas"] = "Folders",
            ["Arquivos"] = "Files",
            ["Pré-visualização"] = "Preview",
            ["Log"] = "Log",
            ["Cancelar"] = "Cancel",
            ["Buscar no RCF inteiro…"] = "Search the whole RCF…",
            ["Nome"] = "Name",
            ["Tamanho"] = "Size",
            ["Tipo"] = "Type",
            ["Data"] = "Date",
            ["Status"] = "Status",
            ["Exportar PNG"] = "Export PNG",
            ["Substituir por PNG"] = "Replace with PNG",
            ["Nenhum RCF aberto"] = "No RCF open",
            ["Arraste um arquivo .rcf (cement.rcf, snd_*.rcf…) para cá ou clique em abrir"] = "Drop an .rcf file (cement.rcf, snd_*.rcf…) here or click open",

            // Janela principal (código)
            ["Abra um arquivo .rcf (cement.rcf, snd_*.rcf…) ou arraste-o para a janela."] = "Open an .rcf file (cement.rcf, snd_*.rcf…) or drop it onto the window.",
            ["nenhum arquivo aberto"] = "no file open",
            ["  ·  {0} alteração(ões) não salva(s)"] = "  ·  {0} unsaved change(s)",
            ["{0:N0} arquivos"] = "{0:N0} files",
            ["Nenhum RCF aberto."] = "No RCF open.",
            ["{0:N0} arquivos  ·  {1} substituído(s)  ·  {2} novo(s)"] = "{0:N0} files  ·  {1} replaced  ·  {2} new",
            ["  ·  hashes conferidos ✓"] = "  ·  hashes verified ✓",
            ["  ·  ATENÇÃO: hashes não conferem, nomes ligados pela ordem dos dados"] = "  ·  WARNING: hashes don't match, names mapped by data order",
            ["ERRO: "] = "ERROR: ",
            ["Erro"] = "Error",
            ["{0} concluído em {1:N1}s."] = "{0} finished in {1:N1}s.",
            ["{0} cancelado."] = "{0} canceled.",
            ["Há alterações não salvas no RCF. Sair mesmo assim?"] = "The RCF has unsaved changes. Exit anyway?",
            ["Arquivos RCF (*.rcf)|*.rcf|Todos os arquivos (*.*)|*.*"] = "RCF files (*.rcf)|*.rcf|All files (*.*)|*.*",
            ["Arquivos RCF (*.rcf)|*.rcf"] = "RCF files (*.rcf)|*.rcf",
            ["Todos os arquivos (*.*)|*.*"] = "All files (*.*)|*.*",
            ["Há alterações não salvas. Descartá-las?"] = "There are unsaved changes. Discard them?",
            ["Aberto: {0} — {1:N0} arquivos, {2:N0} bytes."] = "Opened: {0} — {1:N0} files, {2:N0} bytes.",
            [" Hashes conferidos."] = " Hashes verified.",
            [" AVISO: hashes não conferem com os nomes."] = " WARNING: hashes don't match the names.",
            ["Busca: “{0}”"] = "Search: “{0}”",
            ["{0:N0} resultado(s)"] = "{0:N0} result(s)",
            ["{0:N0} arq."] = "{0:N0} file(s)",
            ["{0} pasta(s)  ·  {1:N0} arquivo(s)"] = "{0} folder(s)  ·  {1:N0} file(s)",
            ["PASTA"] = "FOLDER",
            ["NOVO"] = "NEW",
            ["SUBSTITUÍDO"] = "REPLACED",
            ["dd/MM/yyyy HH:mm"] = "MM/dd/yyyy h:mm tt",
            ["Selecione um ou mais arquivos/pastas na lista."] = "Select one or more files/folders in the list.",
            ["Exportar arquivo"] = "Export file",
            ["Exportado: {0} → {1}"] = "Exported: {0} → {1}",
            ["Exportando"] = "Exporting",
            ["Pasta de destino (os caminhos internos do RCF serão mantidos)"] = "Destination folder (the RCF's internal paths are kept)",
            ["{0:N0} arquivo(s) exportado(s) para {1}."] = "{0:N0} file(s) exported to {1}.",
            ["{0:N0} arquivo(s) exportado(s) para {1} (com {2})."] = "{0:N0} file(s) exported to {1} (with {2}).",
            ["Selecione exatamente um arquivo na lista."] = "Select exactly one file in the list.",
            ["Substituir {0}"] = "Replace {0}",
            ["Substituído: {0} ({1} → {2})"] = "Replaced: {0} ({1} → {2})",
            ["  ! aviso: {0} não parece ser um P3D válido."] = "  ! warning: {0} doesn't look like a valid P3D.",
            ["Importar para \\{0}"] = "Import into \\{0}",
            ["Substituir '{0}' por '{1}'?"] = "Replace '{0}' with '{1}'?",
            ["{0} arquivo(s) não existem em \\{1}:"] = "{0} file(s) don't exist in \\{1}:",
            ["Adicionar como arquivos novos nesta pasta?"] = "Add them as new files in this folder?",
            ["  ! não adicionado: '{0}' tem o mesmo hash de '{1}'."] = "  ! not added: '{0}' has the same hash as '{1}'.",
            ["Novo arquivo: {0} ({1})"] = "New file: {0} ({1})",
            ["Pasta com arquivos exportados (mesma estrutura do RCF)"] = "Folder with exported files (same structure as the RCF)",
            ["Importação de {0}: {1} substituído(s), {2} idêntico(s) ignorado(s), {3} sem correspondência."] = "Import from {0}: {1} replaced, {2} identical skipped, {3} unmatched.",
            ["{0} arquivo(s) da pasta não existem no RCF, por exemplo:"] = "{0} file(s) in the folder don't exist in the RCF, for example:",
            ["Adicioná-los como arquivos novos?"] = "Add them as new files?",
            ["Desfeito: {0}"] = "Undone: {0}",
            ["Removido (novo): {0}"] = "Removed (new): {0}",
            ["Desfazer todas as alterações pendentes?"] = "Undo all pending changes?",
            ["Todas as alterações foram desfeitas."] = "All changes were undone.",
            ["Salvar RCF como"] = "Save RCF as",
            ["Não há alterações para salvar."] = "There are no changes to save.",
            ["Regravar {0}?"] = "Overwrite {0}?",
            ["Uma cópia do original será guardada em {0}."] = "A copy of the original will be kept in {0}.",
            ["Salvando RCF"] = "Saving RCF",
            ["Criando backup {0}…"] = "Creating backup {0}…",
            ["Pasta a compilar (de preferência extraída com \"Exportar tudo\", que gera o {0})"] = "Folder to build (preferably extracted with \"Export everything\", which creates {0})",
            ["Salvar RCF compilado"] = "Save built RCF",
            ["Compilar mesmo assim, sem esses arquivos?"] = "Build anyway, without those files?",
            ["Compilando"] = "Building",
            ["RCF compilado. Abrir no explorador?"] = "RCF built. Open it in the explorer?",
            ["{0:N0} arquivo(s) selecionado(s)."] = "{0:N0} file(s) selected.",
            ["Selecione um arquivo para ver os detalhes."] = "Select a file to see its details.",
            ["  ·  NOVO"] = "  ·  NEW",
            ["  ·  SUBSTITUÍDO"] = "  ·  REPLACED",
            ["{0} imagem(ns)"] = "{0} image(s)",
            ["P3D não pôde ser lido: {0}"] = "P3D could not be read: {0}",
            ["Selecione uma imagem de um .p3d."] = "Select an image from a .p3d.",
            ["Esta imagem não está em PNG."] = "This image is not a PNG.",
            ["Exportar imagem"] = "Export image",
            ["Imagens (*.png;*.bmp)|*.png;*.bmp"] = "Images (*.png;*.bmp)|*.png;*.bmp",
            ["Imagem exportada: {0} → {1}"] = "Image exported: {0} → {1}",
            ["Substituir {0} ({1}×{2})"] = "Replace {0} ({1}×{2})",
            ["Continuar mesmo assim?"] = "Continue anyway?",
            ["Imagem trocada: {0} → {1}"] = "Image replaced: {0} → {1}",
            ["Nenhum arquivo .p3d na seleção."] = "No .p3d files in the selection.",
            ["Pasta para as imagens exportadas (edite-as e use \"Importar imagens editadas\")"] = "Folder for the exported images (edit them and use \"Import edited images\")",
            ["Exportando imagens"] = "Exporting images",
            ["{0:N0} imagem(ns) de {1:N0} arquivo(s) .p3d exportada(s) para {2}"] = "{0:N0} image(s) from {1:N0} .p3d file(s) exported to {2}",
            ["Pasta com as imagens exportadas e editadas"] = "Folder with the exported and edited images",
            ["Nenhuma subpasta corresponde a um .p3d deste RCF.\nUse a mesma pasta gerada por \"Exportar imagens\"."] = "No subfolder matches a .p3d in this RCF.\nUse the same folder created by \"Export images\".",
            ["Importando imagens"] = "Importing images",
            ["Imagens importadas: {0} imagem(ns) alterada(s) em {1} arquivo(s) .p3d."] = "Images imported: {0} image(s) changed in {1} .p3d file(s).",
            ["{0} aviso(s) — veja o log.\nO jogo pode exigir o mesmo tamanho das imagens originais."] = "{0} warning(s) — see the log.\nThe game may require the same size as the original images.",

            // Aparência
            ["Fundo da janela"] = "Window background",
            ["Mica — acompanha o papel de parede"] = "Mica — follows your wallpaper",
            ["Acrílico — transparente, com desfoque"] = "Acrylic — transparent, blurred",
            ["Sólido — escuro, sem efeitos"] = "Solid — dark, no effects",
            ["O efeito aparece no cabeçalho, no rodapé e na barra de título."] = "The effect shows on the header, footer and title bar.",
            ["Mica e Acrílico exigem Windows 11 (22H2 ou superior)."] = "Mica and Acrylic require Windows 11 (22H2 or later).",
            ["Opacidade da janela"] = "Window opacity",
            ["Idioma"] = "Language",
            // Editor de textos
            ["Explorador"] = "Explorer",
            ["Editar textos"] = "Edit texts",
            ["Textos"] = "Texts",
            ["ID (hash)"] = "ID (hash)",
            ["Original (no RCF)"] = "Original (in RCF)",
            ["Texto (editável)"] = "Text (editable)",
            ["Salvar no RCF"] = "Save to RCF",
            ["Importar TXT"] = "Import TXT",
            ["Exportar TXT"] = "Export TXT",
            ["Desfazer edições"] = "Undo edits",
            ["Só alterados"] = "Changed only",
            ["Buscar texto ou ID…"] = "Search text or ID…",
            ["{0:N0} texto(s)"] = "{0:N0} text(s)",
            ["{0:N0} alterado(s)"] = "{0:N0} changed",
            ["Créditos"] = "Credits",
            ["Ferramenta criada por MrGeH."] = "Tool created by MrGeH.",
            ["Texto (*.txt)|*.txt"] = "Text (*.txt)|*.txt",
            ["Texto (*.txt)|*.txt|Todos os arquivos (*.*)|*.*"] = "Text (*.txt)|*.txt|All files (*.*)|*.*",
            ["Exportar textos"] = "Export texts",
            ["Importar textos"] = "Import texts",
            ["{0:N0} texto(s) alterado(s) pelo .txt."] = "{0:N0} text(s) changed by the .txt.",
            ["Desfazer as edições feitas desde que o arquivo foi aberto?"] = "Undo the edits made since the file was opened?",
            ["O .txt tem {0} linha(s), mas o arquivo tem {1} texto(s)."] = "The .txt has {0} line(s), but the file has {1} text(s).",
            ["{0:N0} texto(s) — clique duas vezes para editar."] = "{0:N0} text(s) — double-click to edit.",
            ["{0} não tem textos (TextBible ou legenda)."] = "{0} has no texts (TextBible or subtitle).",
            ["Editor de textos: {0} ({1:N0} textos)."] = "Text editor: {0} ({1:N0} texts).",
            ["Aplicar as edições de {0} ao RCF antes de fechar?"] = "Apply the edits of {0} to the RCF before closing?",
            ["O RCF fica marcado como alterado; use Salvar para gravar o arquivo."] = "The RCF is marked as changed; use Save to write the file.",
            ["{0} não está mais no RCF."] = "{0} is no longer in the RCF.",
            ["Nenhum texto mudou — nada para gravar."] = "No text changed — nothing to save.",
            ["Há textos editados que não foram gravados no RCF. Sair mesmo assim?"] = "There are edited texts not saved to the RCF. Exit anyway?",
            ["Restaurar padrão"] = "Restore defaults",
            ["Fechar"] = "Close",

            // Ajuda
            ["Como usar o Descompilador e Compilador de RCF/IMG/P3D"] = "How to use the RCF/IMG/P3D Decompiler & Compiler",
            ["Entendi"] = "Got it",

            // Núcleo (RCF / P3D)
            ["  ! ausente na pasta: {0}"] = "  ! missing from folder: {0}",
            ["{0} arquivo(s) do manifesto não estão na pasta (veja o log)."] = "{0} file(s) from the manifest are not in the folder (see the log).",
            ["Manifesto: {0} arquivo(s) na ordem original."] = "Manifest: {0} file(s) in the original order.",
            ["Manifesto: {0} arquivo(s) na ordem original, {1} removido(s)."] = "Manifest: {0} file(s) in the original order, {1} removed.",
            ["Sem {0}: arquivos em ordem alfabética, data atual."] = "No {0}: files in alphabetical order, current date.",
            ["  + novo arquivo: {0}"] = "  + new file: {0}",
            ["Nenhum arquivo para compilar."] = "No files to build.",
            ["Nome com caracteres não suportados: {0}"] = "Name with unsupported characters: {0}",
            ["Arquivo duplicado: {0}"] = "Duplicate file: {0}",
            ["Colisão de hash {0:X8} entre '{1}' e '{2}'. Renomeie um deles."] = "Hash collision {0:X8} between '{1}' and '{2}'. Rename one of them.",
            ["O RCF passaria de 4 GB — limite do formato."] = "The RCF would exceed 4 GB — the format's limit.",
            ["'{0}' mudou de tamanho durante a compilação."] = "'{0}' changed size during the build.",
            ["RCF gerado: {0} ({1} arquivos, {2:N0} bytes)."] = "RCF created: {0} ({1} files, {2:N0} bytes).",
            ["Arquivo pequeno demais para ser um RCF."] = "File too small to be an RCF.",
            ["Assinatura '{0}' não encontrada — não é um RCF do Scarface."] = "Signature '{0}' not found — not a Scarface RCF.",
            ["RCF big-endian (versão de console) não é suportado."] = "Big-endian RCF (console version) is not supported.",
            ["Cabeçalho do RCF inconsistente."] = "Inconsistent RCF header.",
            ["Tabela de nomes corrompida (nome #{0} com tamanho {1})."] = "Corrupted name table (name #{0} with length {1}).",
            ["Entrada '{0}' aponta para fora do arquivo."] = "Entry '{0}' points outside the file.",
            ["Fim inesperado do arquivo."] = "Unexpected end of file.",
            [", paleta"] = ", palette",
            [", alfa"] = ", alpha",
            ["{0}: tamanho mudou de {1}×{2} para {3}×{4}."] = "{0}: size changed from {1}×{2} to {3}×{4}.",
            ["{0}: formato mudou de {1} para {2} bpp{3}."] = "{0}: format changed from {1} to {2} bpp{3}.",
            [" paleta"] = " palette",
            ["{0}: {1}×{2} não é potência de 2 — texturas 3D podem falhar."] = "{0}: {1}×{2} is not a power of 2 — 3D textures may fail.",
            ["O arquivo não é um PNG válido."] = "The file is not a valid PNG.",
            ["A imagem de \"{0}\" precisa ter {1}×{2} (recebida: {3}×{4})."] = "The image for \"{0}\" must be {1}×{2} (got {3}×{4}).",
            ["Sprite {0}×{1}, juntado de {2} partes"] = "Sprite {0}×{1}, joined from {2} pieces",
            ["Tipo de cor PNG desconhecido: {0}."] = "Unknown PNG color type: {0}.",
            ["Não é um arquivo P3D (assinatura P3D\\xFF ausente)."] = "Not a P3D file (P3D\\xFF signature missing).",
            ["Chunk {0:X8} inválido na posição {1}."] = "Invalid chunk {0:X8} at offset {1}.",
            ["Sobra de bytes no chunk {0:X8}."] = "Leftover bytes in chunk {0:X8}.",
        };
    }
}
