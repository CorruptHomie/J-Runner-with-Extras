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
        public static readonly Color BorderHover = Color.FromArgb(96, 96, 106);
        // For disabled controls: dimmer than Border, but still clearly separated from both
        // RaisedBg (45,45,51) and PanelBg (30,30,34) - unlike BorderSubtle, which is not.
        public static readonly Color BorderDim = Color.FromArgb(58, 58, 65);
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

        /// <summary>
        /// True while a NAND write is in progress. Set from MainForm.SetFlashing, which
        /// already runs on every write path, so the background can react to it.
        /// </summary>
        public static bool FlashActive;

        /// <summary>Background snowfall on/off, toggled from the JR menu.</summary>
        public static bool SnowfallEnabled
        {
            get { return JRunner.variables.snowfallEnabled; }
        }

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

            // Anything added after this pass would otherwise keep its default light colours -
            // which is exactly how the timing-selector panel stayed white: it's created as a
            // field initialiser and swapped into MainForm at runtime, so it wasn't in Controls
            // when the form was themed. Themeing new children on arrival closes that gap for
            // every container, not just the five panels that hit it.
            root.ControlAdded -= Root_ControlAdded;
            root.ControlAdded += Root_ControlAdded;

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
            else
            {
                // The root itself was never styled - only its children. A Form got away with
                // it because the branch above sets its BackColor explicitly, but any other
                // root kept its default. That is why the timing panel stayed light: a
                // UserControl's default BackColor is SystemColors.Control, which is
                // (240,240,240) - exactly the colour measured in the screenshots.
                StyleControl(root);
            }

            foreach (Control c in root.Controls)
            {
                if (c.Tag as string == SkipTag) continue;
                // Recursing unconditionally styles each control exactly once - the previous
                // form styled the child here and then styled it again inside the recursive
                // call, and skipped recursion entirely for childless controls.
                ApplyTheme(c);
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
                    // BorderStyle.FixedSingle is painted by the OS in a system colour that
                    // no managed property overrides, so on a dark form it reads as a bright
                    // white frame - that's the ring around the console. I previously exempted
                    // only RichTextBox, wrongly assuming a plain TextBox drew its own border.
                    // All text boxes are borderless now and get a themed border painted by
                    // the parent instead (see PaintFieldBorder), so the edge is still there
                    // but in the right colour.
                    tb.BorderStyle = BorderStyle.None;
                    AddFieldBorder(tb);
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
                    chk.EnabledChanged -= Control_EnabledChanged;
                    chk.EnabledChanged += Control_EnabledChanged;
                    break;

                case RadioButton rb:
                    rb.ForeColor = TextPrimary;
                    rb.FlatStyle = FlatStyle.Flat;
                    rb.FlatAppearance.BorderSize = 0;
                    rb.Paint -= RadioButton_Paint;
                    rb.Paint += RadioButton_Paint;
                    rb.EnabledChanged -= Control_EnabledChanged;
                    rb.EnabledChanged += Control_EnabledChanged;
                    break;

                case ListView lvw:
                    // Column headers are drawn by the OS from system colours and there's no
                    // property for them, so the control has to be owner-drawn. Group headers
                    // still aren't reachable - Win32 draws those - but they render their own
                    // accent text, which reads fine once the background is dark.
                    NativeDark.Apply(lvw);
                    lvw.BackColor = FieldBg;
                    lvw.ForeColor = TextPrimary;
                    lvw.BorderStyle = BorderStyle.None;
                    if (!lvw.OwnerDraw)
                    {
                        lvw.OwnerDraw = true;
                        lvw.DrawColumnHeader -= ListView_DrawColumnHeader;
                        lvw.DrawColumnHeader += ListView_DrawColumnHeader;
                        lvw.DrawItem -= ListView_DrawItem;
                        lvw.DrawItem += ListView_DrawItem;
                        lvw.DrawSubItem -= ListView_DrawSubItem;
                        lvw.DrawSubItem += ListView_DrawSubItem;
                    }
                    break;

                case TreeView trv:
                    NativeDark.Apply(trv);
                    trv.BackColor = FieldBg;
                    trv.ForeColor = TextPrimary;
                    trv.LineColor = Border;
                    trv.BorderStyle = BorderStyle.None;
                    break;

                case TrackBar trk:
                    // The slider and track are drawn by the OS; only the surround is ours.
                    // NativeDark is what gets the rest closer to matching.
                    NativeDark.Apply(trk);
                    trk.BackColor = PanelBg;
                    break;

                case ScrollBar scr:
                    NativeDark.Apply(scr);
                    break;

                case Splitter spl:
                    spl.BackColor = Border;
                    break;

                case NumericUpDown nud:
                    nud.BackColor = FieldBg;
                    nud.ForeColor = TextPrimary;
                    nud.BorderStyle = BorderStyle.None;
                    AddFieldBorder(nud);
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
                        // UseVisualStyleBackColor must be cleared FIRST. While it is true - and
                        // it defaults to true, plus the designers set it explicitly in 470
                        // places - a TabPage paints the visual-style background and ignores
                        // BackColor completely. Setting BackColor alone did nothing, which is
                        // why every tabbed panel stayed light no matter what the theme did.
                        dp.UseVisualStyleBackColor = false;
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

                case TabPage tp:
                    // TabPage derives from Panel, so this has to come before the Panel cases
                    // or it would be handled as a plain panel and keep the visual-style
                    // background. Same flag as above.
                    tp.UseVisualStyleBackColor = false;
                    tp.BackColor = PanelBg;
                    tp.ForeColor = TextPrimary;
                    break;

                case Panel bp when bp.BorderStyle != BorderStyle.None:
                    // A Panel's FixedSingle/Fixed3D border is OS-drawn from system colours,
                    // same as a TextBox's - it shows as a light frame on a dark form. Dropped
                    // and replaced with a themed one painted by its own parent.
                    bp.BorderStyle = BorderStyle.None;
                    bp.BackColor = PanelBg;
                    AddFieldBorder(bp);
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
                    // Disabled labels are drawn by WinForms in SystemColors.GrayText, which
                    // is invisible on this theme - handled in Label_Paint. Repainting on
                    // EnabledChanged matters because these are toggled at runtime.
                    lbl.Paint -= Label_Paint;
                    lbl.Paint += Label_Paint;
                    lbl.EnabledChanged -= Control_EnabledChanged;
                    lbl.EnabledChanged += Control_EnabledChanged;
                    lbl.ForeColor = TextPrimary;
                    break;

                case ProgressBar pgb:
                    // XboxFillProgressBar paints itself.
                    break;
            }
        }

        private static void ListView_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            using (SolidBrush bg = new SolidBrush(RaisedBg))
                e.Graphics.FillRectangle(bg, e.Bounds);
            using (Pen sep = new Pen(BorderSubtle))
            {
                e.Graphics.DrawLine(sep, e.Bounds.Right - 1, e.Bounds.Top + 3, e.Bounds.Right - 1, e.Bounds.Bottom - 3);
                e.Graphics.DrawLine(sep, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
            }

            Rectangle r = new Rectangle(e.Bounds.X + 6, e.Bounds.Y, Math.Max(0, e.Bounds.Width - 8), e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, e.Header.Text, e.Font ?? UiFont, r, TextPrimary,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        private static void ListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            // In Details view each column arrives via DrawSubItem instead, so drawing here
            // as well would paint over them.
            ListView lv = sender as ListView;
            if (lv != null && lv.View == View.Details) return;

            using (SolidBrush bg = new SolidBrush(e.Item.Selected ? AccentDim : FieldBg))
                e.Graphics.FillRectangle(bg, e.Bounds);

            TextRenderer.DrawText(e.Graphics, e.Item.Text, e.Item.Font ?? UiFont, e.Bounds,
                ResolveItemColor(e.Item.ForeColor),
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        private static void ListView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            using (SolidBrush bg = new SolidBrush(e.Item.Selected ? AccentDim : FieldBg))
                e.Graphics.FillRectangle(bg, e.Bounds);

            Rectangle r = new Rectangle(e.Bounds.X + 4, e.Bounds.Y, Math.Max(0, e.Bounds.Width - 6), e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, e.SubItem.Text, e.SubItem.Font ?? e.Item.Font ?? UiFont, r,
                ResolveItemColor(e.SubItem.ForeColor),
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        // Deliberate per-item colours are kept; the default black is not, since it would be
        // invisible on a dark row.
        private static Color ResolveItemColor(Color c)
        {
            if (c.IsEmpty) return TextPrimary;
            if (c.ToArgb() == SystemColors.WindowText.ToArgb()) return TextPrimary;
            if (c.ToArgb() == Color.Black.ToArgb()) return TextPrimary;
            return c;
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

        // A control's own border is drawn by the OS; the parent's surface is ours. Drawing
        // the outline just outside the control's bounds gives every field a border in the
        // theme colour, with rounded corners to match the buttons and group boxes.
        private static readonly HashSet<Control> _borderedFields = new HashSet<Control>();

        private static void AddFieldBorder(Control c)
        {
            if (c.Parent == null)
            {
                // Not parented yet during a theme pass in a constructor - try again once it is.
                c.ParentChanged -= Field_ParentChanged;
                c.ParentChanged += Field_ParentChanged;
                return;
            }
            if (!_borderedFields.Add(c)) return;

            Control parent = c.Parent;
            parent.Paint -= Parent_PaintFieldBorders;
            parent.Paint += Parent_PaintFieldBorders;

            // The outline lives outside the control, so the parent has to repaint when the
            // control moves, resizes or is hidden.
            c.LocationChanged += (s, e) => parent.Invalidate();
            c.SizeChanged += (s, e) => parent.Invalidate();
            c.VisibleChanged += (s, e) => parent.Invalidate();
            c.Disposed += (s, e) => _borderedFields.Remove(c);
            parent.Invalidate();
        }

        private static void Field_ParentChanged(object sender, EventArgs e)
        {
            Control c = sender as Control;
            if (c != null && c.Parent != null) AddFieldBorder(c);
        }

        private static void Parent_PaintFieldBorders(object sender, PaintEventArgs e)
        {
            Control parent = (Control)sender;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            foreach (Control child in parent.Controls)
            {
                if (!child.Visible || !_borderedFields.Contains(child)) continue;

                // The outline sits one pixel outside the control, so a control flush against
                // its parent's edge would have its border drawn at -1 and clipped away -
                // which is what happened when a layout shift pushed three controls to x=0.
                // Nudged inward in that case: a slightly tight border beats a missing one.
                int bx = Math.Max(0, child.Left - 1);
                int by = Math.Max(0, child.Top - 1);
                Rectangle r = new Rectangle(bx, by,
                                            child.Right - bx, child.Bottom - by);
                if (r.Width < 4 || r.Height < 4) continue;

                int radius = Math.Max(2, Math.Min(6, r.Height / 4));
                using (GraphicsPath path = MessageDialog.RoundedPathStroke(r, radius))
                using (Pen pen = new Pen(child.Enabled ? Border : BorderDim))
                    e.Graphics.DrawPath(pen, path);
            }
        }

        // ---- Buttons -------------------------------------------------------------
        // Previously these were clipped to a rounded Region. Regions aren't antialiased,
        // so the corners came out visibly jagged. Owner-drawing gives smooth corners and
        // full control over the disabled/hover/pressed states.

        private class BtnState { public bool Hover; public bool Down; }
        private static readonly Dictionary<Button, BtnState> _btnStates = new Dictionary<Button, BtnState>();

        private static void StyleButton(Button btn)
        {
            // Buttons have the same trap: with UseVisualStyleBackColor set, BackColor is
            // ignored and the button paints the system style.
            btn.UseVisualStyleBackColor = false;

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
            // BorderSubtle (46,46,52) against RaisedBg (45,45,51) is a one-value
            // difference - the border was being drawn correctly and was simply invisible,
            // which is why only the rounded corners appeared to have an edge. Idle buttons
            // use Border now; hover brightens further so the state is still distinguishable.
            // A disabled button still needs a visible edge - BorderSubtle against RaisedBg
            // is a luminance delta of 1, i.e. invisible.
            Color line = !b.Enabled ? BorderDim : st.Hover ? BorderHover : Border;
            int radius = Math.Max(3, Math.Min(8, b.Height / 5));

            Rectangle bounds = new Rectangle(0, 0, b.Width - 1, b.Height - 1);
            using (GraphicsPath fillPath = MessageDialog.RoundedPath(bounds, radius))
            using (SolidBrush fb = new SolidBrush(fill))
                g.FillPath(fb, fillPath);

            // Stroked with the half-pixel-offset path so the straight runs render at full
            // strength. PixelOffsetMode.Half did not do this - it shifts sampling for fills,
            // not the centre line of a stroke.
            using (GraphicsPath strokePath = MessageDialog.RoundedPathStroke(bounds, radius))
            using (Pen pen = new Pen(line))
                g.DrawPath(pen, strokePath);

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

        /// <summary>
        /// Redraws a control's caption in the theme's disabled colour.
        ///
        /// WinForms paints disabled text with SystemColors.GrayText, which no managed
        /// property overrides - it's a dark grey chosen for light backgrounds, so on this
        /// theme a disabled label is effectively invisible rather than dimmed. That's why
        /// the timing radio buttons vanished entirely whenever their group was disabled.
        ///
        /// Only done when disabled: enabled captions are still left to WinForms, because
        /// drawing them ourselves is what truncated them previously (these are AutoSize
        /// controls whose width was set by the native layout). x=16 and NoPadding match
        /// where WinForms puts the text next to the glyph.
        /// </summary>
        private static void DrawDisabledCaption(Graphics g, Control c, int textLeft)
        {
            if (c.Enabled || string.IsNullOrEmpty(c.Text)) return;

            Rectangle r = new Rectangle(textLeft, 0, Math.Max(0, c.Width - textLeft), c.Height);
            using (SolidBrush bg = new SolidBrush(c.Parent != null ? c.Parent.BackColor : PanelBg))
                g.FillRectangle(bg, r);

            TextRenderer.DrawText(g, c.Text, c.Font, r, TextDisabled,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }

        // A control's caption changes colour when it is enabled or disabled, but WinForms
        // doesn't always repaint it - and a stale caption is what leaves an enabled control
        // still looking greyed out.
        private static void Root_ControlAdded(object sender, ControlEventArgs e)
        {
            if (e.Control != null) ApplyTheme(e.Control);
        }

        private static void Control_EnabledChanged(object sender, EventArgs e)
        {
            Control c = sender as Control;
            if (c != null) c.Invalidate();
        }

        private static void Label_Paint(object sender, PaintEventArgs e)
        {
            Label lbl = (Label)sender;
            if (lbl.Enabled) return;   // enabled labels paint normally

            e.Graphics.SmoothingMode = SmoothingMode.None;
            using (SolidBrush bg = new SolidBrush(lbl.Parent != null ? lbl.Parent.BackColor : PanelBg))
                e.Graphics.FillRectangle(bg, lbl.ClientRectangle);

            TextFormatFlags flags = TextFormatFlags.WordBreak | TextFormatFlags.NoPadding;
            if (lbl.TextAlign == ContentAlignment.MiddleCenter || lbl.TextAlign == ContentAlignment.TopCenter)
                flags |= TextFormatFlags.HorizontalCenter;
            if (lbl.TextAlign == ContentAlignment.MiddleLeft || lbl.TextAlign == ContentAlignment.MiddleCenter)
                flags |= TextFormatFlags.VerticalCenter;

            TextRenderer.DrawText(e.Graphics, lbl.Text, lbl.Font, lbl.ClientRectangle, TextDisabled, flags);
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

            DrawDisabledCaption(g, rb, GlyphSize + 3);
        }

        private static void CheckBox_Paint(object sender, PaintEventArgs e)
        {
            CheckBox chk = (CheckBox)sender;
            Graphics g = e.Graphics;
            ClearGlyphStrip(g, chk);

            Rectangle box = GlyphRect(chk);
            using (GraphicsPath path = MessageDialog.RoundedPath(box, 3))
            using (GraphicsPath strokePath = MessageDialog.RoundedPathStroke(box, 3))
            using (SolidBrush bg = new SolidBrush(chk.Checked && chk.Enabled ? Accent : FieldBg))
            using (Pen pen = new Pen(chk.Checked && chk.Enabled ? Accent : Border))
            {
                g.FillPath(bg, path);
                g.DrawPath(pen, strokePath);
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

            DrawDisabledCaption(g, chk, GlyphSize + 3);
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
                using (GraphicsPath p = MessageDialog.RoundedPathStroke(border, 6))
                using (Pen pen = new Pen(gb.Enabled ? Border : BorderDim))
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
