using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace Extrator_e_Compilador_RCF.UI
{
    /// <summary>Uma aba do <see cref="TabStrip"/>.</summary>
    public sealed class TabItem
    {
        public string Text { get; set; } = "";
        public string Glyph { get; set; } = "";
        public bool Closable { get; set; } = true;
        /// <summary>Tem alterações não aplicadas (mostra um ponto no lugar do X).</summary>
        public bool Dirty { get; set; }
        public object? Tag { get; set; }
        public string? ToolTip { get; set; }
    }

    /// <summary>Faixa de abas desenhada pelo app (Explorador + editores abertos).</summary>
    public sealed class TabStrip : Control
    {
        private readonly List<TabItem> _tabs = new();
        private readonly List<(RectangleF tab, RectangleF close)> _rects = new();
        private readonly ToolTip _tip = new();
        private int _selected = -1, _hover = -1;
        private bool _hoverClose;

        public event EventHandler? SelectedChanged;
        /// <summary>O usuário pediu para fechar a aba (X ou botão do meio); quem usa decide se fecha.</summary>
        public event Action<TabItem>? CloseRequested;

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IReadOnlyList<TabItem> Tabs => _tabs;

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TabItem? Selected
        {
            get => _selected >= 0 && _selected < _tabs.Count ? _tabs[_selected] : null;
            set
            {
                int i = value == null ? -1 : _tabs.IndexOf(value);
                if (i == _selected) return;
                _selected = i;
                Invalidate();
                SelectedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public TabStrip()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            BackColor = Theme.Bg;
            ForeColor = Theme.Text;
        }

        public void Add(TabItem tab, bool select = true)
        {
            _tabs.Add(tab);
            if (select) Selected = tab; else Invalidate();
        }

        public void Remove(TabItem tab)
        {
            int i = _tabs.IndexOf(tab);
            if (i < 0) return;
            var current = Selected;
            _tabs.RemoveAt(i);
            _hover = -1;
            if (current == tab) { _selected = -1; Selected = _tabs.Count == 0 ? null : _tabs[Math.Max(0, i - 1)]; }
            else _selected = current == null ? -1 : _tabs.IndexOf(current);
            Invalidate();
        }


        private void Measure(Graphics g)
        {
            _rects.Clear();
            float x = this.Dpi(10), h = Height - this.Dpi(6), y = this.Dpi(6);
            foreach (var t in _tabs)
            {
                float text = TextRenderer.MeasureText(g, L.T(t.Text), Theme.UiSemibold, Size.Empty, TextFormatFlags.NoPadding).Width;
                float w = this.Dpi(14) + this.Dpi(18) + this.Dpi(8) + text + this.Dpi(t.Closable ? 30 : 14);
                w = Math.Min(w, this.Dpi(300));
                var r = new RectangleF(x, y, w, h);
                var close = t.Closable ? new RectangleF(r.Right - this.Dpi(26), y + (h - this.Dpi(20)) / 2, this.Dpi(20), this.Dpi(20)) : RectangleF.Empty;
                _rects.Add((r, close));
                x += w + this.Dpi(4);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(BackColor);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            Measure(g);

            using (var line = new Pen(Theme.Border)) g.DrawLine(line, 0, Height - 1, Width, Height - 1);
            for (int i = 0; i < _tabs.Count; i++)
            {
                var t = _tabs[i];
                var (r, close) = _rects[i];
                bool sel = i == _selected, hot = i == _hover;
                if (sel || hot)
                {
                    using var path = TopRounded(r, this.Dpi(8));
                    using var b = new SolidBrush(sel ? Theme.Surface : Theme.Alpha(Theme.SurfaceHover, 160));
                    g.FillPath(b, path);
                    if (sel)
                    {
                        using var p = new Pen(Theme.Border);
                        g.DrawPath(p, path);
                        using var cover = new SolidBrush(Theme.Surface);
                        g.FillRectangle(cover, r.X + 1, r.Bottom - 1, r.Width - 2, 2);
                        Theme.FillRound(g, Theme.Accent, new RectangleF(r.X + this.Dpi(10), r.Y, r.Width - this.Dpi(20), this.Dpi(2)), 1);
                    }
                }
                var icon = new RectangleF(r.X + this.Dpi(12), r.Y, this.Dpi(18), r.Height);
                Theme.Glyph(g, t.Glyph, sel ? Theme.Accent : Theme.Muted, icon, 10f);
                float tx = icon.Right + this.Dpi(6);
                var textRect = Rectangle.Round(new RectangleF(tx, r.Y, (t.Closable ? close.X : r.Right - this.Dpi(10)) - tx, r.Height));
                TextRenderer.DrawText(g, L.T(t.Text), Theme.UiSemibold, textRect, sel ? Theme.Text : Theme.Muted,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
                if (t.Closable)
                {
                    bool overClose = hot && _hoverClose;
                    if (t.Dirty && !overClose)
                    {
                        float d = this.Dpi(8);
                        using var b = new SolidBrush(Theme.Accent);
                        g.FillEllipse(b, close.X + (close.Width - d) / 2, close.Y + (close.Height - d) / 2, d, d);
                    }
                    else if (sel || hot)
                    {
                        if (overClose) Theme.FillRound(g, Theme.SurfaceHover, close, this.Dpi(5));
                        Theme.Glyph(g, Glyphs.Cancel, overClose ? Theme.Text : Theme.Muted, close, 7.5f);
                    }
                }
            }
        }

        private static GraphicsPath TopRounded(RectangleF r, float radius)
        {
            var p = new GraphicsPath();
            float d = radius * 2;
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddLine(r.Right, r.Bottom, r.X, r.Bottom);
            p.CloseFigure();
            return p;
        }

        private int HitTest(Point pt, out bool onClose)
        {
            onClose = false;
            for (int i = 0; i < _rects.Count; i++)
                if (_rects[i].tab.Contains(pt))
                {
                    onClose = _tabs[i].Closable && _rects[i].close.Contains(pt);
                    return i;
                }
            return -1;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int i = HitTest(e.Location, out bool c);
            if (i == _hover && c == _hoverClose) return;
            _hover = i;
            _hoverClose = c;
            _tip.SetToolTip(this, i >= 0 ? _tabs[i].ToolTip ?? "" : "");
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _hover = -1; Invalidate(); }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            int i = HitTest(e.Location, out bool c);
            if (i < 0) return;
            if (e.Button == MouseButtons.Left && !c) Selected = _tabs[i];
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            int i = HitTest(e.Location, out bool c);
            if (i < 0 || !_tabs[i].Closable) return;
            if ((e.Button == MouseButtons.Left && c) || e.Button == MouseButtons.Middle) CloseRequested?.Invoke(_tabs[i]);
        }
    }
}
