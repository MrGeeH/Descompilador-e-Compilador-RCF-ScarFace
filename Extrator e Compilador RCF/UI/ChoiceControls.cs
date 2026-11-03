using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace Extrator_e_Compilador_RCF.UI
{
    /// <summary>Botão de opção desenhado pelo app, com a bolinha na cor de destaque (o nativo usa a cor do Windows).</summary>
    public sealed class ModernRadio : RadioButton
    {
        private bool _hover;

        public ModernRadio()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            ForeColor = Theme.Text;
            Cursor = Cursors.Hand;
        }

        private int Circle => this.Dpi(18);
        private int Gap => this.Dpi(8);

        public override Size GetPreferredSize(Size proposedSize)
        {
            var text = TextRenderer.MeasureText(Text, Font, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
            return new Size(Circle + Gap + text.Width + this.Dpi(2), Math.Max(Circle, text.Height) + this.Dpi(4));
        }

        protected override void OnTextChanged(EventArgs e) { base.OnTextChanged(e); if (AutoSize) Size = GetPreferredSize(Size.Empty); }
        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); _hover = true; Invalidate(); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _hover = false; Invalidate(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Parent?.BackColor ?? Theme.Surface);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            float d = Circle - 1;
            var c = new RectangleF(0.5f, (Height - d) / 2f, d, d);
            if (Checked)
            {
                using (var b = new SolidBrush(!Enabled ? Theme.Faint : _hover ? Theme.AccentHover : Theme.Accent)) g.FillEllipse(b, c);
                float inner = d * (_hover ? 0.44f : 0.38f);
                using var w = new SolidBrush(Color.White);
                g.FillEllipse(w, c.X + (d - inner) / 2, c.Y + (d - inner) / 2, inner, inner);
            }
            else
            {
                if (_hover && Enabled) using (var b = new SolidBrush(Theme.SurfaceHover)) g.FillEllipse(b, c);
                using var p = new Pen(!Enabled ? Theme.Border : _hover ? Theme.Text : Theme.Muted, 1.4f);
                g.DrawEllipse(p, c.X + 0.7f, c.Y + 0.7f, d - 1.4f, d - 1.4f);
            }

            var tr = new Rectangle(Circle + Gap, 0, Width - Circle - Gap, Height);
            TextRenderer.DrawText(g, Text, Font, tr, Enabled ? ForeColor : Theme.Faint,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
            if (Focused && ShowFocusCues)
            {
                using var p = new Pen(Theme.Alpha(Theme.Accent, 140)) { DashStyle = DashStyle.Dot };
                g.DrawEllipse(p, c.X - 2, c.Y - 2, d + 4, d + 4);
            }
        }
    }

    /// <summary>Barra deslizante desenhada pelo app (substitui o TrackBar nativo), na cor de destaque.</summary>
    [DefaultEvent(nameof(Scroll))]
    public sealed class ModernSlider : Control
    {
        private int _min, _max = 100, _value;
        private bool _drag, _hover;

        [Category("Aparência RCF")] public event EventHandler? Scroll;

        [Category("Aparência RCF"), DefaultValue(0)]
        public int Minimum { get => _min; set { _min = value; if (_max < _min) _max = _min; Value = _value; Invalidate(); } }

        [Category("Aparência RCF"), DefaultValue(100)]
        public int Maximum { get => _max; set { _max = Math.Max(value, _min); Value = _value; Invalidate(); } }

        [Category("Aparência RCF"), DefaultValue(0)]
        public int Value { get => _value; set { int v = Math.Clamp(value, _min, _max); if (v == _value) return; _value = v; Invalidate(); } }

        [Category("Aparência RCF"), DefaultValue(1)] public int SmallChange { get; set; } = 1;
        [Category("Aparência RCF"), DefaultValue(5)] public int LargeChange { get; set; } = 5;
        /// <summary>Intervalo entre as marcas (0 = sem marcas).</summary>
        [Category("Aparência RCF"), DefaultValue(5)] public int TickFrequency { get; set; } = 5;

        public ModernSlider()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            TabStop = true;
            Cursor = Cursors.Hand;
        }

        private float Thumb => this.Dpi(20);
        private float Left0 => Thumb / 2 + 1;
        private float TrackWidth => Math.Max(1, Width - Thumb - 2);
        private float TrackY => this.Dpi(14);

        private float XOf(int v) => Left0 + (_max == _min ? 0 : (v - _min) * TrackWidth / (_max - _min));

        private void SetFromUser(int v)
        {
            v = Math.Clamp(v, _min, _max);
            if (v == _value) return;
            _value = v;
            Invalidate();
            Scroll?.Invoke(this, EventArgs.Empty);
        }

        private void SetFromMouse(int x) =>
            SetFromUser(_min + (int)Math.Round((x - Left0) / TrackWidth * (_max - _min)));

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;
            Focus();
            _drag = true;
            SetFromMouse(e.X);
        }

        protected override void OnMouseMove(MouseEventArgs e) { base.OnMouseMove(e); if (_drag) SetFromMouse(e.X); }
        protected override void OnMouseUp(MouseEventArgs e) { base.OnMouseUp(e); _drag = false; Invalidate(); }
        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); _hover = true; Invalidate(); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _hover = false; Invalidate(); }
        protected override void OnMouseWheel(MouseEventArgs e) { base.OnMouseWheel(e); SetFromUser(_value + Math.Sign(e.Delta) * SmallChange); }
        protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
        protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }

        protected override bool IsInputKey(Keys k) => k is Keys.Left or Keys.Right or Keys.Up or Keys.Down || base.IsInputKey(k);

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            switch (e.KeyCode)
            {
                case Keys.Left or Keys.Down: SetFromUser(_value - SmallChange); break;
                case Keys.Right or Keys.Up: SetFromUser(_value + SmallChange); break;
                case Keys.PageDown: SetFromUser(_value - LargeChange); break;
                case Keys.PageUp: SetFromUser(_value + LargeChange); break;
                case Keys.Home: SetFromUser(_min); break;
                case Keys.End: SetFromUser(_max); break;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(BackColor);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            float h = this.Dpi(4), y = TrackY, x = XOf(_value);
            Theme.FillRound(g, Theme.Surface2, new RectangleF(Left0, y - h / 2, TrackWidth, h), h / 2);
            Theme.FillRound(g, Enabled ? Theme.Accent : Theme.Faint, new RectangleF(Left0, y - h / 2, x - Left0, h), h / 2);

            if (TickFrequency > 0 && _max > _min)
            {
                using var p = new Pen(Theme.Faint);
                float ty = y + Thumb / 2 + this.Dpi(4);
                for (int v = _min; v <= _max; v += TickFrequency)
                    g.DrawLine(p, XOf(v), ty, XOf(v), ty + this.Dpi(4));
            }

            // botão: anel escuro com miolo na cor de destaque (cresce ao passar o mouse/arrastar)
            float t = Thumb;
            var outer = new RectangleF(x - t / 2, y - t / 2, t, t);
            using (var b = new SolidBrush(Theme.Surface2)) g.FillEllipse(b, outer);
            using (var p = new Pen(Theme.Border)) g.DrawEllipse(p, outer);
            float inner = t * (_drag ? 0.42f : _hover || Focused ? 0.58f : 0.5f);
            using (var b = new SolidBrush(!Enabled ? Theme.Faint : _drag ? Theme.AccentPressed : _hover ? Theme.AccentHover : Theme.Accent))
                g.FillEllipse(b, x - inner / 2, y - inner / 2, inner, inner);
        }
    }
}
