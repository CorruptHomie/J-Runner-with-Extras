using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace UI
{
    // Central dark-grey palette + a recursive "reskin whatever's already there" pass.
    // JRunner's forms are large, hand-positioned legacy WinForms layouts, so rather than
    // rebuilding every control this recolours the existing ones in place and owner-draws
    // the handful of control types WinForms gives no usable colour properties for.
    public static class Theme
    {
        // ---- Palette ----
        public static readonly Color WindowBg = Color.FromArgb(22, 22, 25);
        public static readonly Color PanelBg = Color.FromArgb(30, 30, 34);
        public static readonly Color RaisedBg = Color.FromArgb(45, 45, 51);
        public static readonly Color HoverBg = Color.FromArgb(60, 60, 67);
        public static readonly Color PressedBg = Color.FromArgb(36, 36, 41);
        public static readonly Color Border = Color.FromArgb(62, 62, 69);
        public static readonly Color BorderSubtle = Color.FromArgb(46, 46, 52);
        public static readonly Color FieldBg = Color.FromArgb(17, 17, 19);
        public static readonly Color TextPrimary = Color.FromArgb(232, 232, 235);
        public static readonly Color TextSecondary = Color.FromArgb(150, 150, 158);
        public static readonly Color TextDisabled = Color.FromArgb(110, 110, 118);
        public static readonly Color Accent = Color.FromArgb(116, 199, 87);
        public static readonly Color AccentDim = Color.FromArgb(70, 128, 55);
        public static readonly Color Danger = Color.FromArgb(214, 90, 80);
        public static readonly Color ChromeButton = Color.FromArgb(186, 186, 193);
        public static readonly Color ChromeButtonHover = Color.FromArgb(240, 240, 245);

        // Controls tagged with this are skipped by ApplyTheme. The theme pass runs after
        // the title bar is built, and the generic Label/Panel cases would otherwise
        // overwrite the chrome buttons' deliberate grey and the separator hairline.
        public const string SkipTag = "ui.theme.skip";

        // Toggled from Settings ("Enable animations"). Only the flashing progress bar
        // animates; everything else is instant.
        public static bool AnimationsEnabled
        {
            get { return JRunner.variables.animationsEnabled; }
        }

        // Used only by controls this code creates itself. Deliberately NOT applied to
        // existing designer controls - the designer sized every button, group box and tab
        // for its original font, and forcing a different one on them re-wraps their text
        // and clips it ("Program Timing File" losing "File", etc).
        public static readonly Font UiFont = new Font("Segoe UI", 9F);
        public static readonly Font UiFontBold = new Font("Segoe UI", 9F, FontStyle.Bold);

        /// <summary>
        /// Recursively themes a control and everything under it.
        /// </summary>
        public static void ApplyTheme(Control root)
        {
            if (root == null) return;

            if (root is Form form)
            {
                form.BackColor = WindowBg;
                form.ForeColor = TextPrimary;

                // Every form's designer baked its own copy of the old icon into its .resx
                // as a binary blob, so replacing Project3.ico alone wouldn't have changed
                // any of them. Assigning it here updates all of them at once - every form
                // already runs through this method - without rewriting ~65 resource files.
                try { form.Icon = JRunner.Properties.Resources.Project3; }
                catch (Exception ex) { if (JRunner.variables.debugme) Console.WriteLine("Theme icon: " + ex.Message); }

                // Forms with a native border keep a white caption otherwise - the title bar
                // is drawn by the window manager, so nothing in managed code reaches it.
                NativeDark.EnableDarkTitleBar(form);
            }

            foreach (Control c in root.Controls)
            {
                if (c.Tag as string == SkipTag) continue;
                StyleControl(c);
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
                    // Left with WinForms' own flat rendering - it draws a dropdown arrow
                    // and split divider that owner-drawing would have to reimplement.
                    sb.BackColor = RaisedBg;
                    sb.ForeColor = TextPrimary;
                    sb.FlatStyle = FlatStyle.Flat;
                    sb.FlatAppearance.BorderColor = Border;
                    sb.FlatAppearance.BorderSize = 1;
                    sb.FlatAppearance.MouseOverBackColor = HoverBg;
                    sb.FlatAppearance.MouseDownBackColor = PressedBg;
                    break;

                case TextBoxBase tb:
                    NativeDark.Apply(tb);
                    tb.BackColor = FieldBg;
                    tb.ForeColor = TextPrimary;
                    // RichTextBox draws FixedSingle in a system colour that shows up as a
                    // bright white frame against a dark form, so it goes borderless and the
                    // surrounding surface provides the edge instead.
                    tb.BorderStyle = tb is RichTextBox ? BorderStyle.None : BorderStyle.FixedSingle;
                    break;

                case ComboBox cbx:
                    // A DropDownList combo paints its closed display area from system
                    // colours and ignores BackColor entirely - that's the white "Kernel
                    // Version" box. Owner-drawing is the only thing that reaches it, and it
                    // covers the dropped-down list items as well.
                    NativeDark.Apply(cbx);
                    cbx.FlatStyle = FlatStyle.Flat;
                    cbx.BackColor = FieldBg;
                    cbx.ForeColor = TextPrimary;
                    if (cbx.DrawMode != DrawMode.OwnerDrawFixed)
                    {
                        cbx.DrawMode = DrawMode.OwnerDrawFixed;
                        cbx.DrawItem -= ComboBox_DrawItem;
                        cbx.DrawItem += ComboBox_DrawItem;
                    }
                    break;

                case ListControl lc:
                    NativeDark.Apply(lc);
                    lc.BackColor = FieldBg;
                    lc.ForeColor = TextPrimary;
                    break;

                case DataGridView dgv:
                    // Every one of these has to be set, not just DefaultCellStyle. The
                    // designer assigns whole DataGridViewCellStyle objects built from
                    // SystemColors - Window (white) for the default cells,
                    // GradientActiveCaption (pale blue) for alternating rows, and Highlight
                    // for selection, which follows the user's Windows accent colour and is
                    // why the selected cell came out green. Row and alternating-row styles
                    // take precedence over DefaultCellStyle, so theming only the latter
                    // left the grid looking untouched.
                    NativeDark.Apply(dgv);
                    dgv.BackgroundColor = PanelBg;
                    dgv.GridColor = BorderSubtle;
                    dgv.ForeColor = TextPrimary;
                    dgv.BorderStyle = BorderStyle.None;
                    dgv.EnableHeadersVisualStyles = false;
                    dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
                    dgv.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

                    StyleGridCells(dgv.DefaultCellStyle, FieldBg);
                    StyleGridCells(dgv.RowsDefaultCellStyle, FieldBg);
                    // Kept a touch lighter so the zebra striping still reads.
                    StyleGridCells(dgv.AlternatingRowsDefaultCellStyle, Color.FromArgb(26, 26, 30));

                    StyleGridCells(dgv.ColumnHeadersDefaultCellStyle, RaisedBg);
                    dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = RaisedBg;
                    dgv.ColumnHeadersDefaultCellStyle.SelectionForeColor = TextPrimary;

                    StyleGridCells(dgv.RowHeadersDefaultCellStyle, RaisedBg);
                    dgv.RowHeadersDefaultCellStyle.ForeColor = TextSecondary;
                    dgv.RowHeadersDefaultCellStyle.SelectionBackColor = HoverBg;
                    dgv.RowHeadersDefaultCellStyle.SelectionForeColor = TextPrimary;
                    break;

                case GroupBox gb:
                    gb.BackColor = PanelBg;
                    gb.ForeColor = TextSecondary;
                    // GroupBox draws its own etched 3D border from system colours, which
                    // reads as a bright line on a dark background, and offers no FlatStyle.
                    gb.Paint -= GroupBox_Paint;
                    gb.Paint += GroupBox_Paint;
                    break;

                case CheckBox chk:
                    chk.ForeColor = TextPrimary;
                    chk.FlatStyle = FlatStyle.Flat;
                    chk.FlatAppearance.BorderSize = 0;
                    chk.Paint -= CheckBox_Paint;
                    chk.Paint += CheckBox_Paint;
                    break;

                case RadioButton rb:
                    rb.ForeColor = TextPrimary;
                    rb.FlatStyle = FlatStyle.Flat;
                    rb.FlatAppearance.BorderSize = 0;
                    rb.Paint -= RadioButton_Paint;
                    rb.Paint += RadioButton_Paint;
                    break;

                case NumericUpDown nud:
                    nud.BackColor = FieldBg;
                    nud.ForeColor = TextPrimary;
                    nud.BorderStyle = BorderStyle.FixedSingle;
                    break;

                case StatusStrip ss:
                    ss.BackColor = PanelBg;
                    foreach (ToolStripItem item in ss.Items) item.ForeColor = TextPrimary;
                    break;

                // DarkTabControl paints its own tabs and frame; only plain TabControls
                // need the DrawItem hookup here.
                case DarkTabControl dtc:
                    // Catches the tab strip's scroll-arrow buttons, which Windows creates
                    // as their own child window when the tabs overflow.
                    NativeDark.Apply(dtc);
                    foreach (TabPage dp in dtc.TabPages)
                    {
                        dp.BackColor = PanelBg;
                        dp.ForeColor = TextPrimary;
                    }
                    break;

                case TabControl tc:
                    if (tc.DrawMode != TabDrawMode.OwnerDrawFixed)
                    {
                        // WinForms draws tab headers with system colours and offers no
                        // colour properties for them - owner-drawing is the only option.
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

                case UserControl uc:
                    // UserControl isn't a Panel and had no case, so panels like NandTools /
                    // XeBuildPanel / NandInfo kept SystemColors.Control - a light grey that
                    // shows through anywhere their children don't cover.
                    uc.BackColor = PanelBg;
                    uc.ForeColor = TextPrimary;
                    break;

                case PictureBox pb:
                    // Left to whatever owns it - device images are drawn on their own card.
                    break;

                case Panel sp when sp.AutoScroll:
                    NativeDark.Apply(sp);
                    sp.BackColor = PanelBg;
                    break;

                case Panel p:
                    // Only hairline separators are left alone - their colour is the point.
                    // The white-panel exemption that used to be here was protecting nothing
                    // (no Panel in the project is white; the white surfaces were AeroWizard
                    // controls, handled below) and was leaving light patches behind.
                    if (p.Height > 2 && p.Width > 2) p.BackColor = PanelBg;
                    break;

                case Label lbl:
                    lbl.ForeColor = TextPrimary;
                    break;

                case ProgressBar pgb:
                    // XboxFillProgressBar paints itself.
                    break;
            }
        }

        private static void ComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            using (SolidBrush bg = new SolidBrush(selected ? AccentDim : FieldBg))
                e.Graphics.FillRectangle(bg, e.Bounds);

            if (e.Index >= 0 && e.Index < cb.Items.Count)
            {
                TextRenderer.DrawText(e.Graphics, cb.GetItemText(cb.Items[e.Index]), cb.Font, e.Bounds,
                    cb.Enabled ? TextPrimary : TextDisabled,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }
        }

        private static void StyleGridCells(DataGridViewCellStyle style, Color back)
        {
            if (style == null) return;
            style.BackColor = back;
            style.ForeColor = TextPrimary;
            style.SelectionBackColor = AccentDim;
            style.SelectionForeColor = TextPrimary;
        }

        // ---- Buttons -------------------------------------------------------------
        // Previously these were clipped to a rounded Region. Regions aren't antialiased,
        // so the corners came out visibly jagged. Owner-drawing gives smooth corners and
        // full control over the disabled/hover/pressed states.

        private class BtnState { public bool Hover; public bool Down; }
        private static readonly Dictionary<Button, BtnState> _btnStates = new Dictionary<Button, BtnState>();

        private static void StyleButton(Button btn)
        {
            // A button with neither text nor image is a colour swatch, not a button -
            // Settings' log-colour picker is a row of these. Repainting them in the theme
            // colour would erase the only thing they convey, so they keep their fill and
            // just get a flat border.
            if (string.IsNullOrEmpty(btn.Text) && btn.Image == null)
            {
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderColor = Border;
                btn.FlatAppearance.BorderSize = 1;
                return;
            }

            btn.ForeColor = TextPrimary;
            btn.BackColor = RaisedBg;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;

            // Buttons carrying an image keep WinForms' own rendering - owner-drawing them
            // would mean reimplementing image/text layout for no real gain.
            if (btn.Image != null)
            {
                btn.FlatAppearance.BorderColor = Border;
                btn.FlatAppearance.BorderSize = 1;
                btn.FlatAppearance.MouseOverBackColor = HoverBg;
                btn.FlatAppearance.MouseDownBackColor = PressedBg;
                return;
            }

            // Suppress the built-in hover/press fills; the Paint handler draws them.
            btn.FlatAppearance.MouseOverBackColor = RaisedBg;
            btn.FlatAppearance.MouseDownBackColor = RaisedBg;

            if (_btnStates.ContainsKey(btn)) return;

            BtnState st = new BtnState();
            _btnStates[btn] = st;
            btn.MouseEnter += (s, e) => { st.Hover = true; btn.Invalidate(); };
            btn.MouseLeave += (s, e) => { st.Hover = false; st.Down = false; btn.Invalidate(); };
            btn.MouseDown += (s, e) => { st.Down = true; btn.Invalidate(); };
            btn.MouseUp += (s, e) => { st.Down = false; btn.Invalidate(); };
            btn.EnabledChanged += (s, e) => btn.Invalidate();
            btn.Paint += Button_Paint;
        }

        private static void Button_Paint(object sender, PaintEventArgs e)
        {
            Button b = (Button)sender;
            if (b.Width < 6 || b.Height < 6) return;

            BtnState st;
            if (!_btnStates.TryGetValue(b, out st)) st = new BtnState();

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Repaint the parent's colour first so the rounded corners blend instead of
            // leaving the button's own square fill showing through.
            using (SolidBrush parent = new SolidBrush(b.Parent != null ? b.Parent.BackColor : WindowBg))
                g.FillRectangle(parent, b.ClientRectangle);

            Color fill = !b.Enabled ? PanelBg : st.Down ? PressedBg : st.Hover ? HoverBg : RaisedBg;
            Color line = !b.Enabled ? BorderSubtle : st.Hover ? Border : BorderSubtle;
            int radius = Math.Max(3, Math.Min(8, b.Height / 5));

            using (GraphicsPath path = MessageDialog.RoundedPath(new Rectangle(0, 0, b.Width - 1, b.Height - 1), radius))
            using (SolidBrush fb = new SolidBrush(fill))
            using (Pen pen = new Pen(line))
            {
                g.FillPath(fb, path);
                g.DrawPath(pen, path);
            }

            TextRenderer.DrawText(g, b.Text, b.Font, b.ClientRectangle,
                b.Enabled ? TextPrimary : TextDisabled,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
        }

        // ---- Check boxes / radio buttons -----------------------------------------
        // The system-drawn glyphs are near-white blocks on a dark surface and their
        // checked state is almost impossible to read, so both are drawn here instead.

        // Only the glyph itself is repainted - the caption is left exactly as WinForms
        // drew it. Drawing the caption here was what truncated it: these controls are
        // AutoSize, so their width is whatever the native glyph+text layout needed, and
        // re-laying the text out even slightly differently pushed it past the edge. Native
        // text also keeps font, alignment and disabled-greying correct for free.
        //
        // 13px is the native glyph size; the strip is 15px so the repaint fully covers it
        // without reaching the caption, which starts at x=16.
        private const int GlyphSize = 13;
        private const int GlyphStrip = 15;

        private static Rectangle GlyphRect(Control c)
        {
            return new Rectangle(0, Math.Max(0, (c.Height - GlyphSize) / 2), GlyphSize, GlyphSize);
        }

        // Wipes just the glyph strip back to the parent colour, leaving the natively-drawn
        // caption to the right of it untouched.
        private static void ClearGlyphStrip(Graphics g, Control c)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (SolidBrush parent = new SolidBrush(c.Parent != null ? c.Parent.BackColor : PanelBg))
                g.FillRectangle(parent, new Rectangle(0, 0, GlyphStrip, c.Height));
        }

        private static void RadioButton_Paint(object sender, PaintEventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            Graphics g = e.Graphics;
            ClearGlyphStrip(g, rb);

            Rectangle box = GlyphRect(rb);
            using (SolidBrush bg = new SolidBrush(rb.Enabled ? FieldBg : PanelBg))
            using (Pen pen = new Pen(rb.Checked ? Accent : Border))
            {
                g.FillEllipse(bg, box);
                g.DrawEllipse(pen, box);
            }

            if (rb.Checked)
            {
                using (SolidBrush a = new SolidBrush(rb.Enabled ? Accent : TextDisabled))
                    g.FillEllipse(a, Rectangle.Inflate(box, -4, -4));
            }
        }

        private static void CheckBox_Paint(object sender, PaintEventArgs e)
        {
            CheckBox chk = (CheckBox)sender;
            Graphics g = e.Graphics;
            ClearGlyphStrip(g, chk);

            Rectangle box = GlyphRect(chk);
            using (GraphicsPath path = MessageDialog.RoundedPath(box, 3))
            using (SolidBrush bg = new SolidBrush(chk.Checked && chk.Enabled ? Accent : FieldBg))
            using (Pen pen = new Pen(chk.Checked && chk.Enabled ? Accent : Border))
            {
                g.FillPath(bg, path);
                g.DrawPath(pen, path);
            }

            if (chk.Checked)
            {
                // Tick drawn as two strokes rather than a glyph font, so it scales with the
                // box and stays crisp.
                using (Pen tick = new Pen(chk.Enabled ? Color.FromArgb(20, 24, 18) : TextDisabled, 2f))
                {
                    tick.StartCap = LineCap.Round;
                    tick.EndCap = LineCap.Round;
                    g.DrawLines(tick, new[]
                    {
                        new Point(box.Left + 3, box.Top + 6),
                        new Point(box.Left + 5, box.Top + 9),
                        new Point(box.Left + 10, box.Top + 4),
                    });
                }
            }
        }

        // ---- Tabs / group boxes ---------------------------------------------------

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

            TextRenderer.DrawText(e.Graphics, page.Text, tc.Font, r,
                selected ? TextPrimary : TextSecondary,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
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

            bool titled = !string.IsNullOrEmpty(gb.Text);
            Size ts = titled ? TextRenderer.MeasureText(gb.Text, gb.Font) : Size.Empty;
            int top = titled ? ts.Height / 2 : 0;

            Rectangle border = new Rectangle(0, top, gb.Width - 1, gb.Height - top - 1);
            if (border.Width > 4 && border.Height > 4)
            {
                using (GraphicsPath p = MessageDialog.RoundedPath(border, 6))
                using (Pen pen = new Pen(BorderSubtle))
                    g.DrawPath(pen, p);
            }

            if (titled)
            {
                // Punch a gap in the border line for the caption, then draw it.
                using (SolidBrush bg2 = new SolidBrush(gb.BackColor))
                    g.FillRectangle(bg2, new Rectangle(9, top - 1, ts.Width + 6, ts.Height + 2));
                TextRenderer.DrawText(g, gb.Text, gb.Font, new Point(11, 0), TextSecondary);
            }
        }
    }
}
