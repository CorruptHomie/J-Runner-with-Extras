using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace UI
{
    // Central dark-grey palette + a recursive "reskin whatever's already there" pass.
    // JRunner's forms are large, hand-positioned legacy WinForms layouts (MainForm.cs alone
    // is 150+ named controls) - rebuilding every one as a bespoke owner-drawn control isn't
    // something that can be done reliably without a Windows machine to render and check it
    // on. Recoloring the existing controls in place, plus a handful of purpose-built
    // replacements for the pieces the user specifically called out (title bar, progress
    // bar, menus, confirm/complete dialogs), gets the whole app to a consistent dark theme
    // without touching every individual Designer.cs by hand.
    public static class Theme
    {
        // ---- Palette ----
        public static readonly Color WindowBg = Color.FromArgb(22, 22, 25);      // outermost background
        public static readonly Color PanelBg = Color.FromArgb(30, 30, 34);       // group boxes, panels
        public static readonly Color RaisedBg = Color.FromArgb(45, 45, 51);      // buttons, inputs
        public static readonly Color HoverBg = Color.FromArgb(60, 60, 67);       // hover state
        public static readonly Color PressedBg = Color.FromArgb(36, 36, 41);     // pressed state
        public static readonly Color Border = Color.FromArgb(62, 62, 69);
        public static readonly Color BorderSubtle = Color.FromArgb(46, 46, 52);
        public static readonly Color FieldBg = Color.FromArgb(17, 17, 19);       // text boxes, lists, tracks
        public static readonly Color TextPrimary = Color.FromArgb(232, 232, 235);
        public static readonly Color TextSecondary = Color.FromArgb(150, 150, 158);
        public static readonly Color Accent = Color.FromArgb(116, 199, 87);      // Xbox-logo green
        public static readonly Color AccentDim = Color.FromArgb(70, 128, 55);
        public static readonly Color Danger = Color.FromArgb(214, 90, 80);
        public static readonly Color ChromeButton = Color.FromArgb(160, 160, 166); // "slightly lighter grey" window glyphs
        public static readonly Color ChromeButtonHover = Color.FromArgb(205, 205, 210);

        // Toggled from Settings ("Enable animations"). Every animated piece (fill progress,
        // dialog entrance, title bar glyph hover) reads this and, when false, jumps straight
        // to the end state instead of interpolating - same visuals, no motion.
        public static bool AnimationsEnabled
        {
            get { return JRunner.variables.animationsEnabled; }
        }

        // Controls tagged with this are left alone by ApplyTheme. Needed because the
        // theme pass runs after the title bar is built, and the generic Label/Panel cases
        // below would otherwise overwrite the chrome buttons' deliberate grey and the
        // separator hairline's colour.
        public const string SkipTag = "ui.theme.skip";

        public static readonly Font UiFont = new Font("Segoe UI", 9F);
        public static readonly Font UiFontBold = new Font("Segoe UI", 9F, FontStyle.Bold);

        /// <summary>
        /// Recursively themes a control and everything under it. Safe to call on a whole
        /// Form (typically from its constructor, after InitializeComponent) or on a single
        /// dynamically-created panel.
        /// </summary>
        public static void ApplyTheme(Control root)
        {
            if (root == null) return;

            if (root is Form form)
            {
                form.BackColor = WindowBg;
                form.ForeColor = TextPrimary;
            }

            foreach (Control c in root.Controls)
            {
                if (c.Tag as string == SkipTag) continue;
                StyleControl(c);
                // MenuStrip/ContextMenuStrip/StatusStrip items are themed globally through
                // JRunnerToolStripRenderer (see UI/JRunnerToolStripRenderer.cs) rather than
                // walked here - ToolStrip's own Controls collection doesn't contain its
                // items anyway.
                if (c.HasChildren) ApplyTheme(c);
            }
        }

        private static void StyleControl(Control c)
        {
            switch (c)
            {
                case Button btn when !(btn is UI.SplitButton):
                    StyleButton(btn);
                    break;

                case UI.SplitButton sb:
                    sb.BackColor = RaisedBg;
                    sb.ForeColor = TextPrimary;
                    sb.FlatStyle = FlatStyle.Flat;
                    sb.FlatAppearance.BorderColor = Border;
                    sb.FlatAppearance.BorderSize = 1;
                    sb.FlatAppearance.MouseOverBackColor = HoverBg;
                    sb.FlatAppearance.MouseDownBackColor = PressedBg;
                    sb.Font = UiFont;
                    break;

                case TextBoxBase tb:
                    tb.BackColor = FieldBg;
                    tb.ForeColor = TextPrimary;
                    tb.BorderStyle = BorderStyle.FixedSingle;
                    break;

                case ListControl lc:
                    lc.BackColor = FieldBg;
                    lc.ForeColor = TextPrimary;
                    if (lc is ComboBox cb) cb.FlatStyle = FlatStyle.Flat;
                    break;

                case DataGridView dgv:
                    dgv.BackgroundColor = PanelBg;
                    dgv.GridColor = Border;
                    dgv.ForeColor = TextPrimary;
                    dgv.DefaultCellStyle.BackColor = FieldBg;
                    dgv.DefaultCellStyle.ForeColor = TextPrimary;
                    dgv.DefaultCellStyle.SelectionBackColor = AccentDim;
                    dgv.ColumnHeadersDefaultCellStyle.BackColor = RaisedBg;
                    dgv.ColumnHeadersDefaultCellStyle.ForeColor = TextPrimary;
                    dgv.EnableHeadersVisualStyles = false;
                    dgv.BorderStyle = BorderStyle.None;
                    break;

                case GroupBox gb:
                    gb.BackColor = PanelBg;
                    gb.ForeColor = TextSecondary;
                    gb.Font = UiFont;
                    // GroupBox draws its own etched 3D border from system colours, which
                    // reads as a bright line on a dark background. Painting over it is the
                    // only way to change it - GroupBox has no FlatStyle.
                    gb.Paint -= GroupBox_Paint;
                    gb.Paint += GroupBox_Paint;
                    break;

                case Panel p:
                    // Leave alone: panels carrying a deliberate light background for
                    // embedded diagrams/images, and hairline separator panels whose colour
                    // is the whole point of them.
                    if (p.BackColor != Color.White && p.Height > 2 && p.Width > 2) p.BackColor = PanelBg;
                    break;

                case Label lbl:
                    lbl.ForeColor = TextPrimary;
                    break;

                case CheckBox chk:
                    chk.ForeColor = TextPrimary;
                    chk.Font = UiFont;
                    chk.FlatStyle = FlatStyle.Flat;
                    chk.FlatAppearance.CheckedBackColor = Accent;
                    chk.FlatAppearance.BorderColor = Border;
                    break;

                case RadioButton rb:
                    rb.ForeColor = TextPrimary;
                    rb.Font = UiFont;
                    rb.FlatStyle = FlatStyle.Flat;
                    rb.FlatAppearance.CheckedBackColor = Accent;
                    rb.FlatAppearance.BorderColor = Border;
                    break;

                case NumericUpDown nud:
                    nud.BackColor = FieldBg;
                    nud.ForeColor = TextPrimary;
                    nud.BorderStyle = BorderStyle.FixedSingle;
                    break;

                case StatusStrip ss:
                    // Background comes from JRunnerToolStripRenderer, but each item still
                    // carries the default near-black system fore colour, which is unreadable
                    // once the strip goes dark.
                    ss.BackColor = PanelBg;
                    foreach (ToolStripItem item in ss.Items) item.ForeColor = TextPrimary;
                    break;

                case TabControl tc:
                    tc.Font = UiFont;
                    if (tc.DrawMode != TabDrawMode.OwnerDrawFixed)
                    {
                        // WinForms draws tab headers with system colours and offers no
                        // colour properties for them, so they stay light no matter what
                        // BackColor is set - owner-drawing them is the only way to make
                        // them match the rest of the theme.
                        tc.DrawMode = TabDrawMode.OwnerDrawFixed;
                        tc.DrawItem -= TabControl_DrawItem;
                        tc.DrawItem += TabControl_DrawItem;
                    }
                    foreach (TabPage tp in tc.TabPages)
                    {
                        tp.BackColor = PanelBg;
                        tp.ForeColor = TextPrimary;
                    }
                    break;

                case ProgressBar pgb:
                    // XboxFillProgressBar (a ProgressBar subclass) paints itself; nothing to
                    // set here for a plain ProgressBar beyond leaving the OS chrome alone.
                    break;
            }
        }

        private static void TabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabControl tc = (TabControl)sender;
            if (e.Index < 0 || e.Index >= tc.TabPages.Count) return;

            TabPage page = tc.TabPages[e.Index];
            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Rectangle r = e.Bounds;

            using (SolidBrush b = new SolidBrush(selected ? RaisedBg : PanelBg))
                e.Graphics.FillRectangle(b, r);

            if (selected)
            {
                using (SolidBrush a = new SolidBrush(Accent))
                    e.Graphics.FillRectangle(a, r.Left, r.Bottom - 2, r.Width, 2);
            }

            TextRenderer.DrawText(e.Graphics, page.Text, UiFont, r,
                selected ? TextPrimary : TextSecondary,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        private static void GroupBox_Paint(object sender, PaintEventArgs e)
        {
            GroupBox gb = (GroupBox)sender;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Covers the default etched border underneath. Child controls are separate
            // window handles and paint themselves, so this doesn't erase them.
            using (SolidBrush bg = new SolidBrush(gb.BackColor))
                g.FillRectangle(bg, gb.ClientRectangle);

            int top = string.IsNullOrEmpty(gb.Text) ? 0 : gb.Font.Height / 2;
            Rectangle border = new Rectangle(0, top, gb.Width - 1, gb.Height - top - 1);
            if (border.Width > 2 && border.Height > 2)
            {
                using (GraphicsPath p = MessageDialog.RoundedPath(border, 6))
                using (Pen pen = new Pen(Border))
                    g.DrawPath(pen, p);
            }

            if (!string.IsNullOrEmpty(gb.Text))
            {
                Size ts = TextRenderer.MeasureText(gb.Text, gb.Font);
                using (SolidBrush bg = new SolidBrush(gb.BackColor))
                    g.FillRectangle(bg, new Rectangle(10, 0, ts.Width + 6, ts.Height));
                TextRenderer.DrawText(g, gb.Text, gb.Font, new Point(12, 0), TextSecondary);
            }
        }

        // Flat-style theming for ordinary buttons, mirroring the SplitButton case above
        // (same palette, same flat border/hover/pressed states) so every push button reads
        // consistently regardless of which control type it actually is.
        private static void StyleButton(Button btn)
        {
            btn.BackColor = RaisedBg;
            btn.ForeColor = TextPrimary;
            btn.FlatStyle = FlatStyle.Flat;
            // Border is drawn in Button_Paint instead - the built-in flat border is a hard
            // rectangle and would sit outside the rounded region below.
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = HoverBg;
            btn.FlatAppearance.MouseDownBackColor = PressedBg;
            btn.Font = UiFont;
            RoundButton(btn);
        }

        // Clipping each button to a rounded region keeps WinForms' own flat fill (and its
        // hover/pressed colours, which are already the ones we want) while losing the hard
        // 90-degree corners that make a stock WinForms form look dated.
        private static void RoundButton(Button btn)
        {
            btn.Resize -= Button_Resize;
            btn.Resize += Button_Resize;
            btn.Paint -= Button_Paint;
            btn.Paint += Button_Paint;
            UpdateButtonRegion(btn);
        }

        private static void Button_Resize(object sender, EventArgs e)
        {
            UpdateButtonRegion((Button)sender);
        }

        private static int ButtonRadius(Control b)
        {
            return Math.Max(3, Math.Min(9, b.Height / 4));
        }

        private static void UpdateButtonRegion(Button b)
        {
            if (b.Width < 6 || b.Height < 6) return;
            using (GraphicsPath p = MessageDialog.RoundedPath(new Rectangle(0, 0, b.Width, b.Height), ButtonRadius(b)))
                b.Region = new Region(p);
        }

        private static void Button_Paint(object sender, PaintEventArgs e)
        {
            Button b = (Button)sender;
            if (b.Width < 6 || b.Height < 6) return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath p = MessageDialog.RoundedPath(new Rectangle(0, 0, b.Width - 1, b.Height - 1), ButtonRadius(b)))
            using (Pen pen = new Pen(b.Enabled ? Border : BorderSubtle))
                e.Graphics.DrawPath(pen, p);
        }
    }
}
