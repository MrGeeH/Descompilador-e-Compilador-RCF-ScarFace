using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace Extrator_e_Compilador_RCF.UI
{
    public enum ButtonKind { Standard, Primary, Subtle, Danger }

    /// <summary>Onde o botão fica dentro do <see cref="HeaderBar"/>.</summary>
    public enum HeaderSlot { Command, Tool }

    /// <summary>
    /// Controle desenhado em bitmap PARGB e copiado com alfa: funciona tanto em área opaca quanto sobre
    /// Mica/Acrílico (onde pixels transparentes mostram o fundo do sistema).
    /// </summary>
    public abstract class GlassControl : Control
    {
        /// <summary>Este controle fica numa área de vidro (cabeçalho/rodapé).</summary>
        [Category("Aparência RCF"), DefaultValue(false)]
        public bool OnGlass { get; set; }

        /// <summary>Cor de fundo quando não há vidro.</summary>
        [Category("Aparência RCF"), DefaultValue(typeof(Color), "17, 17, 20")]
        public Color BaseColor { get; set; } = Theme.Bg;

        protected bool GlassNow => OnGlass && Theme.GlassActive;

        protected GlassControl()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque |
                     ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, false);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.Style |= 0x02000000; // WS_CLIPCHILDREN
                return cp;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Width <= 0 || Height <= 0) return;
            using var bmp = new Bitmap(Width, Height, PixelFormat.Format32bppPArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(GlassNow ? Color.Transparent : BaseColor);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.TextRenderingHint = GlassNow ? TextRenderingHint.AntiAliasGridFit : TextRenderingHint.ClearTypeGridFit;
                Render(g);
            }
            Native.BlitWithAlpha(e.Graphics, bmp);
        }

        protected abstract void Render(Graphics g);

        protected static void DrawText(Graphics g, string text, Font font, Color color, RectangleF r,
            StringAlignment h = StringAlignment.Near, StringAlignment v = StringAlignment.Center)
        {
            using var b = new SolidBrush(color);
            using var sf = new StringFormat(StringFormatFlags.NoWrap)
            {
                Alignment = h, LineAlignment = v, Trimming = StringTrimming.EllipsisCharacter, HotkeyPrefix = HotkeyPrefix.None
            };
            g.DrawString(text, font, b, r, sf);
        }

        protected int S(int px) => this.Dpi(px);
    }

    /// <summary>Botão arredondado com ícone, animação de hover/clique e menu opcional.</summary>
    public sealed class ModernButton : GlassControl
    {
        private readonly System.Windows.Forms.Timer _anim = new() { Interval = 15 };
        private float _hover, _press;
        private bool _isHover, _isDown;
        private string _glyph = "";
        private ContextMenuStrip? _menu;
        private ButtonKind _kind = ButtonKind.Standard;
        private bool _checked;

        /// <summary>Ícone da fonte Segoe Fluent Icons (ex.: ).</summary>
        [Category("Aparência RCF"), DefaultValue("")]
        public string Glyph { get => _glyph; set { _glyph = value ?? ""; FitSize(); Invalidate(); } }

        [Category("Aparência RCF"), DefaultValue(ButtonKind.Standard)]
        public ButtonKind Kind { get => _kind; set { _kind = value; Invalidate(); } }

        /// <summary>Menu aberto ao clicar (mostra uma seta no botão).</summary>
        [Category("Comportamento RCF"), DefaultValue(null)]
        public ContextMenuStrip? Menu { get => _menu; set { _menu = value; FitSize(); Invalidate(); } }

        [Category("Aparência RCF"), DefaultValue(false)]
        public bool Checked { get => _checked; set { _checked = value; Invalidate(); } }

        /// <summary>Posição dentro do HeaderBar.</summary>
        [Category("Layout RCF"), DefaultValue(HeaderSlot.Command)]
        public HeaderSlot Slot { get; set; } = HeaderSlot.Command;

        /// <summary>Espaço extra antes deste botão (separa grupos no HeaderBar).</summary>
        [Category("Layout RCF"), DefaultValue(false)]
        public bool GapBefore { get; set; }

        /// <summary>Ajusta o tamanho ao texto automaticamente.</summary>
        [Category("Layout RCF"), DefaultValue(true)]
        public bool AutoFit { get; set; } = true;

        [Browsable(false)]
        public bool IconOnly => string.IsNullOrEmpty(Text);

        public ModernButton() : this("", "") { }

        public ModernButton(string text, string glyph, ButtonKind kind = ButtonKind.Standard)
        {
            Font = Theme.UiSemibold;
            _glyph = glyph;
            _kind = kind;
            Text = text;
            Cursor = Cursors.Hand;
            TabStop = false;
            _anim.Tick += (_, _) =>
            {
                _hover = Approach(_hover, _isHover ? 1 : 0, 0.2f);
                _press = Approach(_press, _isDown ? 1 : 0, 0.35f);
                if (_hover == (_isHover ? 1 : 0) && _press == (_isDown ? 1 : 0)) _anim.Stop();
                Invalidate();
            };
            FitSize();
        }

        private static float Approach(float v, float target, float step) =>
            v < target ? Math.Min(target, v + step) : Math.Max(target, v - step);

        /// <summary>Tamanho ideal para o texto, ícone e seta de menu atuais.</summary>
        public Size FitToContent()
        {
            int h = S(34);
            int w = IconOnly ? h : S(14) + (_glyph.Length > 0 ? S(24) : 0) +
                    TextRenderer.MeasureText(Text ?? "", Font).Width + (_menu != null ? S(20) : 0) + S(8);
            return new Size(w, h);
        }

        public void FitSize()
        {
            if (AutoFit) Size = FitToContent();
        }

        protected override void OnTextChanged(EventArgs e) { base.OnTextChanged(e); FitSize(); Invalidate(); }
        protected override void OnHandleCreated(EventArgs e) { base.OnHandleCreated(e); FitSize(); }
        protected override void OnDpiChangedAfterParent(EventArgs e) { base.OnDpiChangedAfterParent(e); FitSize(); }
        protected override void OnEnabledChanged(EventArgs e) { base.OnEnabledChanged(e); Invalidate(); }
        protected override void OnMouseEnter(EventArgs e) { _isHover = true; _anim.Start(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _isHover = false; _isDown = false; _anim.Start(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { if (e.Button == MouseButtons.Left) { _isDown = true; _anim.Start(); } base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { _isDown = false; _anim.Start(); base.OnMouseUp(e); }

        protected override void OnClick(EventArgs e)
        {
            if (_menu != null)
            {
                _menu.Show(this, new Point(0, Height + S(4)));
                return;
            }
            base.OnClick(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _anim.Dispose();
            base.Dispose(disposing);
        }

        protected override void Render(Graphics g)
        {
            var r = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
            float radius = S(7);
            bool glass = GlassNow;
            Color fg = Theme.Text;
            Color? fill = null, border = null;

            switch (_kind)
            {
                case ButtonKind.Primary:
                    fill = Theme.Blend(Theme.Blend(Theme.Accent, Theme.AccentHover, _hover), Theme.AccentPressed, _press);
                    fg = Theme.AccentText;
                    border = Theme.Alpha(Color.White, 40);
                    break;
                case ButtonKind.Standard:
                    fill = glass ? Theme.Alpha(Color.White, (int)(16 + 18 * _hover - 6 * _press))
                                 : Theme.Blend(Theme.Blend(Theme.Surface2, Theme.SurfaceHover, _hover), Theme.Surface, _press);
                    border = glass ? Theme.Alpha(Color.White, 26) : Theme.Border;
                    break;
                case ButtonKind.Subtle:
                    float level = Math.Max(_hover, _checked ? 0.6f : 0);
                    if (level > 0)
                        fill = glass ? Theme.Alpha(Color.White, (int)(level * 26 - 8 * _press))
                                     : Theme.Alpha(Theme.SurfaceHover, (int)(255 * level));
                    break;
                case ButtonKind.Danger:
                    fill = Theme.Alpha(Theme.Red, (int)(40 + 50 * _hover));
                    fg = Color.White;
                    break;
            }

            if (!Enabled) fg = Theme.Alpha(fg, 90);
            var state = g.Save();
            if (_press > 0) // leve "afundar" ao clicar
            {
                float s = 1 - 0.03f * _press;
                g.TranslateTransform(Width / 2f, Height / 2f);
                g.ScaleTransform(s, s);
                g.TranslateTransform(-Width / 2f, -Height / 2f);
            }
            if (fill is { } f) Theme.FillRound(g, Enabled ? f : Theme.Alpha(f, f.A / 2), r, radius);
            if (border is { } b) Theme.DrawRound(g, Enabled ? b : Theme.Alpha(b, b.A / 2), r, radius);

            if (IconOnly)
            {
                Theme.Glyph(g, _glyph, fg, r, 10.5f);
            }
            else
            {
                float x = S(12);
                if (_glyph.Length > 0)
                {
                    Color ic = _kind is ButtonKind.Standard or ButtonKind.Subtle && Enabled ? Theme.Blend(Theme.Accent, Color.White, 0.15f) : fg;
                    Theme.Glyph(g, _glyph, ic, new RectangleF(x, 0, S(18), Height), 10.5f);
                    x += S(24);
                }
                float right = Width - S(10) - (_menu != null ? S(18) : 0);
                DrawText(g, Text, Font, fg, new RectangleF(x, 0, right - x + S(4), Height - 1));
                if (_menu != null)
                    Theme.Glyph(g, Glyphs.ChevronDown, Theme.Alpha(fg, 170), new RectangleF(Width - S(26), 0, S(18), Height), 7.5f);
            }
            g.Restore(state);
        }
    }

    /// <summary>Barra de progresso fina e arredondada.</summary>
    public sealed class SlimProgress : GlassControl
    {
        private int _value;

        [DefaultValue(0)]
        public int Value { get => _value; set { _value = Math.Clamp(value, 0, 100); Invalidate(); } }

        protected override void Render(Graphics g)
        {
            float h = S(4);
            var track = new RectangleF(0, (Height - h) / 2, Width - 1, h);
            Theme.FillRound(g, GlassNow ? Theme.Alpha(Color.White, 30) : Theme.Surface2, track, h / 2);
            if (_value > 0)
            {
                var fill = track with { Width = Math.Max(h, track.Width * _value / 100f) };
                Theme.FillRound(g, Theme.Accent, fill, h / 2);
            }
        }
    }

    /// <summary>
    /// Cabeçalho do app (sobre Mica/Acrílico): logo, título e subtítulo. Os <see cref="ModernButton"/> filhos se
    /// organizam sozinhos: Slot=Command na linha de baixo (na ordem do designer), Slot=Tool no canto superior direito.
    /// </summary>
    public sealed class HeaderBar : GlassControl
    {
        private string _title = "Descompilador e Compilador de RCF/IMG";
        private string _subtitle = "";

        [Category("Aparência RCF"), DefaultValue("Descompilador e Compilador de RCF/IMG")]
        public string Title { get => _title; set { _title = value ?? ""; Invalidate(); } }

        [Category("Aparência RCF"), DefaultValue("")]
        public string Subtitle { get => _subtitle; set { _subtitle = value ?? ""; Invalidate(); } }

        public HeaderBar()
        {
            OnGlass = true;
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            if (e.Control is GlassControl gc) gc.OnGlass = true;
            if (e.Control != null) e.Control.SizeChanged += (_, _) => PerformLayout(); // botões se ajustam ao texto
        }

        // o tamanho do cabeçalho costuma não mudar ao abrir a janela, então o layout não seria refeito sozinho
        protected override void OnHandleCreated(EventArgs e) { base.OnHandleCreated(e); PerformLayout(); }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            // (não filtra por Visible: antes de a janela aparecer ele é falso para todos)
            var buttons = Controls.OfType<ModernButton>().ToList();
            int x = S(16), y = Height - S(46);
            foreach (var c in buttons.Where(b => b.Slot == HeaderSlot.Command))
            {
                if (c.GapBefore) x += S(14);
                var size = c.AutoFit ? c.FitToContent() : c.Size;
                c.Bounds = new Rectangle(new Point(x, y), size);
                x += size.Width + S(8);
            }
            int rx = Width - S(14);
            foreach (var c in Enumerable.Reverse(buttons.Where(b => b.Slot == HeaderSlot.Tool).ToList()))
            {
                var size = c.AutoFit ? c.FitToContent() : c.Size;
                rx -= size.Width;
                c.Bounds = new Rectangle(new Point(rx, S(12)), size);
                rx -= S(4);
            }
        }

        protected override void Render(Graphics g)
        {
            // Logo: imagem do programa com cantos arredondados (sem ela, quadrado com gradiente na cor de destaque)
            var logo = new RectangleF(S(16), S(10), S(36), S(36));
            using (var path = Theme.Round(logo, S(9)))
            {
                if (AppIcons.Logo is { } img)
                {
                    var state = g.Save();
                    g.SetClip(path);
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.DrawImage(img, logo);
                    g.Restore(state);
                    using var border = new Pen(Theme.Alpha(Color.White, 28));
                    g.DrawPath(border, path);
                }
                else
                {
                    using (var brush = new LinearGradientBrush(logo, Theme.AccentHover, Theme.AccentPressed, 45f))
                        g.FillPath(brush, path);
                    Theme.Glyph(g, Glyphs.Package, Theme.AccentText, logo, 14f);
                }
            }

            float tx = logo.Right + S(12);
            DrawText(g, _title, Theme.Title, Theme.Text, new RectangleF(tx, S(8), Width / 2f, S(24)));
            DrawText(g, _subtitle, Theme.Small, Theme.Muted, new RectangleF(tx, S(30), Math.Max(0, Width - tx - S(240)), S(18)));

            // linha sutil separando o cabeçalho do conteúdo
            using var pen = new Pen(GlassNow ? Theme.Alpha(Color.White, 18) : Theme.Border);
            g.DrawLine(pen, 0, Height - 1, Width, Height - 1);
        }
    }

    /// <summary>Rodapé (sobre Mica/Acrílico): texto de status, progresso e botões.</summary>
    public sealed class StatusBar : GlassControl
    {
        private string _text = "";

        [Category("Aparência RCF"), DefaultValue("")]
        public string StatusText { get => _text; set { _text = value ?? ""; Invalidate(); } }

        /// <summary>Crédito discreto à direita (some enquanto há progresso).</summary>
        [Category("Aparência RCF"), DefaultValue("")]
        public string Credit { get => _credit; set { _credit = value ?? ""; Invalidate(); } }
        private string _credit = "";

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SlimProgress Progress { get; } = new() { OnGlass = true, Visible = false };

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ModernButton Cancel { get; } = new("Cancelar", Glyphs.Cancel, ButtonKind.Danger) { Visible = false };

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ModernButton LogToggle { get; } = new("Log", Glyphs.History, ButtonKind.Subtle);

        public StatusBar()
        {
            OnGlass = true;
            Cancel.OnGlass = LogToggle.OnGlass = true;
            Controls.AddRange([Progress, Cancel, LogToggle]);
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            int h = S(30), y = (Height - h) / 2 + S(1);
            LogToggle.Height = Cancel.Height = h;
            LogToggle.Location = new Point(Width - LogToggle.Width - S(10), y);
            Cancel.Location = new Point(LogToggle.Left - Cancel.Width - S(8), y);
            Progress.Size = new Size(S(220), h);
            Progress.Location = new Point((Cancel.Visible ? Cancel.Left : LogToggle.Left) - Progress.Width - S(12), y);
        }

        protected override void Render(Graphics g)
        {
            using var pen = new Pen(GlassNow ? Theme.Alpha(Color.White, 18) : Theme.Border);
            g.DrawLine(pen, 0, 0, Width, 0);
            float dot = S(8);
            Theme.FillRound(g, Theme.Accent, new RectangleF(S(16), (Height - dot) / 2, dot, dot), dot / 2);
            int right = (Progress.Visible ? Progress.Left : Cancel.Visible ? Cancel.Left : LogToggle.Left) - S(12);
            if (_credit.Length > 0 && !Progress.Visible)
            {
                int w = TextRenderer.MeasureText(g, _credit, Theme.Small, Size.Empty, TextFormatFlags.NoPadding).Width;
                if (right - w - S(48) > S(200))
                {
                    TextRenderer.DrawText(g, _credit, Theme.Small, new Rectangle(right - w, 0, w, Height), Theme.Faint,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
                    right -= w + S(24);
                }
            }
            DrawText(g, _text, Theme.Ui, Theme.Text, new RectangleF(S(32), 0, Math.Max(0, right - S(32)), Height));
        }
    }

    /// <summary>Painel "cartão": fundo arredondado com borda e título opcional.</summary>
    public sealed class CardPanel : Panel
    {
        private string _title = "";
        private string _glyph = "";
        private string _note = "";

        [Category("Aparência RCF"), DefaultValue("")]
        public string Title { get => _title; set { _title = value ?? ""; UpdatePadding(); Invalidate(); } }

        [Category("Aparência RCF"), DefaultValue("")]
        public string Glyph { get => _glyph; set { _glyph = value ?? ""; Invalidate(); } }

        /// <summary>Texto à direita do título (ex.: contagem).</summary>
        [Category("Aparência RCF"), DefaultValue("")]
        public string Note { get => _note; set { _note = value ?? ""; Invalidate(); } }

        /// <summary>Calculado a partir do título e do DPI — não é salvo pelo designer.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Padding Padding { get => base.Padding; set => base.Padding = value; }

        public CardPanel()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            BackColor = Theme.Surface;
            UpdatePadding();
        }

        protected override void OnHandleCreated(EventArgs e) { base.OnHandleCreated(e); UpdatePadding(); }
        protected override void OnDpiChangedAfterParent(EventArgs e) { base.OnDpiChangedAfterParent(e); UpdatePadding(); }

        private void UpdatePadding()
        {
            int p = this.Dpi(10);
            base.Padding = new Padding(p, _title.Length > 0 ? this.Dpi(40) : p, p, p);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Parent?.BackColor ?? Theme.Bg);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
            Theme.FillRound(g, Theme.Surface, r, this.Dpi(10));
            Theme.DrawRound(g, Theme.Border, r, this.Dpi(10));
            if (_title.Length == 0) return;

            int x = this.Dpi(14);
            if (_glyph.Length > 0)
            {
                Theme.Glyph(g, _glyph, Theme.Accent, new RectangleF(x, this.Dpi(10), this.Dpi(18), this.Dpi(22)), 10f);
                x += this.Dpi(24);
            }
            TextRenderer.DrawText(g, _title, Theme.CardTitle, new Rectangle(x, this.Dpi(10), Width / 2, this.Dpi(22)), Theme.Text,
                TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            if (_note.Length > 0)
                TextRenderer.DrawText(g, _note, Theme.Small, new Rectangle(Width / 2, this.Dpi(10), Width / 2 - this.Dpi(14), this.Dpi(22)),
                    Theme.Muted, TextFormatFlags.VerticalCenter | TextFormatFlags.Right | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
        }
    }

    /// <summary>Caixa de busca arredondada com ícone.</summary>
    public sealed class SearchBox : Panel
    {
        private bool _focused;

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TextBox Box { get; } = new()
        {
            BorderStyle = BorderStyle.None, BackColor = Theme.Surface2, ForeColor = Theme.Text, Font = Theme.Ui
        };

        [Category("Aparência RCF"), DefaultValue("")]
        public string Placeholder { get => Box.PlaceholderText; set => Box.PlaceholderText = value; }

        public SearchBox()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            BackColor = Theme.Surface2;
            Box.GotFocus += (_, _) => { _focused = true; Invalidate(); };
            Box.LostFocus += (_, _) => { _focused = false; Invalidate(); };
            Controls.Add(Box);
            Cursor = Cursors.IBeam;
            Click += (_, _) => Box.Focus();
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            int left = this.Dpi(34);
            Box.Location = new Point(left, (Height - Box.Height) / 2);
            Box.Width = Math.Max(10, Width - left - this.Dpi(10));
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Parent?.BackColor ?? Theme.Surface);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
            Theme.FillRound(g, Theme.Surface2, r, this.Dpi(8));
            Theme.DrawRound(g, _focused ? Theme.Accent : Theme.Border, r, this.Dpi(8), _focused ? 1.5f : 1f);
            Theme.Glyph(g, Glyphs.Search, _focused ? Theme.Accent : Theme.Muted, new RectangleF(this.Dpi(8), 0, this.Dpi(20), Height), 9.5f);
        }
    }

    /// <summary>Área exibida quando nenhum RCF está aberto: convida a arrastar ou abrir um arquivo.</summary>
    public sealed class EmptyState : Control
    {
        private bool _drag;

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ModernButton OpenButton { get; } = new("Abrir RCF…", Glyphs.Open, ButtonKind.Primary) { BaseColor = Theme.Surface };

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DragHover { get => _drag; set { _drag = value; Invalidate(); } }

        public EmptyState()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            BackColor = Theme.Surface;
            AllowDrop = true;
            Controls.Add(OpenButton);
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            OpenButton.Location = new Point((Width - OpenButton.Width) / 2, Height / 2 + this.Dpi(54));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Theme.Surface);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new RectangleF(this.Dpi(8), this.Dpi(8), Width - this.Dpi(16), Height - this.Dpi(16));
            using (var path = Theme.Round(r, this.Dpi(12)))
            using (var pen = new Pen(_drag ? Theme.Accent : Theme.Border, _drag ? 2f : 1.5f) { DashStyle = DashStyle.Dash })
            {
                if (_drag) using (var fill = new SolidBrush(Theme.Alpha(Theme.Accent, 18))) g.FillPath(fill, path);
                g.DrawPath(pen, path);
            }
            int cy = Height / 2;
            var circle = new RectangleF(Width / 2f - this.Dpi(34), cy - this.Dpi(96), this.Dpi(68), this.Dpi(68));
            Theme.FillRound(g, Theme.Alpha(Theme.Accent, 34), circle, this.Dpi(34));
            Theme.Glyph(g, Glyphs.Package, Theme.Accent, circle, 22f);
            TextRenderer.DrawText(g, L.T("Nenhum RCF aberto"), Theme.Title, new Rectangle(0, cy - this.Dpi(18), Width, this.Dpi(30)), Theme.Text,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            TextRenderer.DrawText(g, L.T("Arraste um arquivo .rcf (cement.rcf, snd_*.rcf…) para cá ou clique em abrir"),
                Theme.Ui, new Rectangle(0, cy + this.Dpi(14), Width, this.Dpi(24)), Theme.Muted,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }

    /// <summary>ListView com buffer duplo (evita piscar no desenho customizado).</summary>
    public sealed class SmoothListView : ListView
    {
        public SmoothListView() { DoubleBuffered = true; }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == 0x000F /* WM_PAINT */ && View == View.Details) PaintEmptyArea();
        }

        /// <summary>
        /// O tema escuro do Windows desenha divisórias de coluna na área sem itens; cobre essa área com o fundo.
        /// </summary>
        private void PaintEmptyArea()
        {
            int top = Native.ListViewHeaderHeight(this);
            if (Items.Count > 0) top = Math.Max(top, Items[^1].Bounds.Bottom);
            if (top >= ClientSize.Height) return;
            using var g = CreateGraphics();
            using var b = new SolidBrush(BackColor);
            g.FillRectangle(b, 0, top, ClientSize.Width, ClientSize.Height - top);
        }
    }

    /// <summary>Renderizador escuro e arredondado para menus.</summary>
    public sealed class DarkMenuRenderer : ToolStripProfessionalRenderer
    {
        public DarkMenuRenderer() : base(new DarkColors()) { RoundedEdges = true; }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            using var b = new SolidBrush(Theme.Surface);
            e.Graphics.FillRectangle(b, e.AffectedBounds);
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            using var p = new Pen(Theme.Border);
            e.Graphics.DrawRectangle(p, 0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
        }

        protected override void OnRenderImageMargin(ToolStripRenderEventArgs e) { }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (!e.Item.Selected || !e.Item.Enabled) return;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new RectangleF(4, 1, e.Item.Width - 8, e.Item.Height - 2);
            Theme.FillRound(g, Theme.SurfaceHover, r, 5);
            Theme.FillRound(g, Theme.Accent, new RectangleF(5, r.Y + r.Height * 0.28f, 3, r.Height * 0.44f), 1.5f);
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = e.Item.Enabled ? Theme.Text : Theme.Faint;
            base.OnRenderItemText(e);
        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            e.ArrowColor = Theme.Muted;
            base.OnRenderArrow(e);
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            using var p = new Pen(Theme.Border);
            int y = e.Item.Height / 2;
            e.Graphics.DrawLine(p, 10, y, e.Item.Width - 10, y);
        }

        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = e.ImageRectangle;
            r.Inflate(2, 2);
            Theme.FillRound(g, Theme.Alpha(Theme.Accent, 50), r, 4);
            Theme.Glyph(g, Glyphs.Check, Theme.Accent, r, 8f);
        }

        private sealed class DarkColors : ProfessionalColorTable
        {
            public override Color ToolStripDropDownBackground => Theme.Surface;
            public override Color ImageMarginGradientBegin => Theme.Surface;
            public override Color ImageMarginGradientMiddle => Theme.Surface;
            public override Color ImageMarginGradientEnd => Theme.Surface;
            public override Color MenuBorder => Theme.Border;
            public override Color MenuItemBorder => Color.Transparent;
            public override Color MenuItemSelected => Theme.SurfaceHover;
            public override Color SeparatorDark => Theme.Border;
            public override Color SeparatorLight => Theme.Border;
        }
    }

    internal static class Menus
    {
        /// <summary>
        /// Aplica o tema escuro a um menu criado no designer. O ícone de cada item vem do Tag (um glifo
        /// da Segoe Fluent Icons, ex.: "").
        /// </summary>
        public static void Style(ContextMenuStrip m)
        {
            m.Renderer = new DarkMenuRenderer();
            m.Font = Theme.Ui;
            m.BackColor = Theme.Surface;
            m.ForeColor = Theme.Text;
            m.Padding = new Padding(0, 4, 0, 4);
            m.HandleCreated += (_, _) =>
            {
                Native.SetCorner(m.Handle, Native.Corner.Round);
                Native.UseDarkTitleBar(m.Handle);
            };
            foreach (var item in m.Items.OfType<ToolStripMenuItem>())
            {
                item.ForeColor = Theme.Text;
                item.Padding = new Padding(4, 5, 4, 5);
                item.ImageScaling = ToolStripItemImageScaling.SizeToFit;
                if (item.Tag is string glyph && glyph.Length > 0)
                    item.Image = Theme.GlyphBitmap(glyph, Theme.Muted, 32);
            }
        }
    }
}
