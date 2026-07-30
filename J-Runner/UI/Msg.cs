using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace UI
{
    /// <summary>
    /// Drop-in replacement for <see cref="MessageBox"/>. A Win32 message box is drawn by
    /// the OS and cannot be recoloured, so every dialog in the app stayed light no matter
    /// what the theme did - the only way to get dark dialogs is to stop using it.
    ///
    /// The overloads mirror MessageBox.Show's and return the same DialogResult, so call
    /// sites only change the type name.
    /// </summary>
    public static class Msg
    {
        public static DialogResult Show(string text)
            => Show(null, text, "", MessageBoxButtons.OK, MessageBoxIcon.None);

        public static DialogResult Show(string text, string caption)
            => Show(null, text, caption, MessageBoxButtons.OK, MessageBoxIcon.None);

        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons)
            => Show(null, text, caption, buttons, MessageBoxIcon.None);

        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
            => Show(null, text, caption, buttons, icon);

        public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton def)
            => Show(null, text, caption, buttons, icon);

        public static DialogResult Show(IWin32Window owner, string text)
            => Show(owner, text, "", MessageBoxButtons.OK, MessageBoxIcon.None);

        public static DialogResult Show(IWin32Window owner, string text, string caption)
            => Show(owner, text, caption, MessageBoxButtons.OK, MessageBoxIcon.None);

        public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons)
            => Show(owner, text, caption, buttons, MessageBoxIcon.None);

        public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton def)
            => Show(owner, text, caption, buttons, icon);

        public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            // MessageBox.Show can be called from any thread; a Form can't. Several call
            // sites here run on flashing/worker threads, so marshal onto the UI thread
            // rather than spinning up a second message loop on a background one.
            Form main = JRunner.MainForm.mainForm;
            if (main != null && !main.IsDisposed && main.InvokeRequired)
            {
                return (DialogResult)main.Invoke(new Func<DialogResult>(
                    () => ShowCore(owner ?? main, text, caption, buttons, icon)));
            }
            return ShowCore(owner ?? main, text, caption, buttons, icon);
        }

        private static DialogResult ShowCore(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            try
            {
                using (ThemedMessageForm f = new ThemedMessageForm(text, caption, buttons, icon))
                {
                    Form parent = owner as Form;
                    f.Owner = parent;
                    // Program.cs shows dependency-check dialogs before MainForm exists, so
                    // there may be no parent to centre on.
                    f.StartPosition = parent != null
                        ? FormStartPosition.CenterParent
                        : FormStartPosition.CenterScreen;
                    f.ShowDialog(owner);
                    return f.Result;
                }
            }
            catch (Exception ex)
            {
                // Never let a cosmetic dialog swallow the message it was meant to deliver.
                if (JRunner.variables.debugme) Console.WriteLine("Msg: " + ex);
                return MessageBox.Show(text, caption, buttons, icon);
            }
        }
    }

    [System.ComponentModel.DesignerCategory("")]
    internal class ThemedMessageForm : Form
    {
        public DialogResult Result { get; private set; } = DialogResult.Cancel;

        private readonly MessageBoxIcon _icon;

        public ThemedMessageForm(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            _icon = icon;

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            BackColor = Theme.PanelBg;
            DoubleBuffered = true;
            KeyPreview = true;

            bool hasIcon = icon != MessageBoxIcon.None;
            int left = hasIcon ? 74 : 24;

            // Size to the message, within sane bounds.
            Size measured = TextRenderer.MeasureText(text ?? "", Theme.UiFont,
                new Size(420, 600), TextFormatFlags.WordBreak);
            int bodyW = Math.Max(260, Math.Min(420, measured.Width));
            int bodyH = Math.Max(40, measured.Height);

            int width = left + bodyW + 24;
            width = Math.Max(width, 340);
            int height = 58 + bodyH + 62;

            Size = new Size(width, height);

            Label title = new Label
            {
                Text = string.IsNullOrEmpty(caption) ? "J-Runner" : caption,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = Theme.TextPrimary,
                AutoSize = false,
                Location = new Point(left, 18),
                Size = new Size(bodyW, 22),
                Tag = Theme.SkipTag,
            };

            Label body = new Label
            {
                Text = text ?? "",
                Font = Theme.UiFont,
                ForeColor = Theme.TextSecondary,
                AutoSize = false,
                Location = new Point(left, 46),
                Size = new Size(bodyW, bodyH + 8),
                Tag = Theme.SkipTag,
            };

            Controls.Add(title);
            Controls.Add(body);
            title.MouseDown += Drag;
            body.MouseDown += Drag;

            // Buttons, right-aligned, in the same order MessageBox uses.
            int y = height - 46;
            int x = width - 24;
            foreach (ButtonSpec spec in SpecsFor(buttons))
            {
                Button b = MakeButton(spec.Text, spec.Primary);
                x -= b.Width;
                b.Location = new Point(x, y);
                x -= 8;
                DialogResult dr = spec.Result;
                b.Click += (s, e) => { Result = dr; Close(); };
                Controls.Add(b);
                if (spec.Primary) AcceptButton = b;
            }

            KeyDown += (s, e) =>
            {
                if (e.KeyCode != Keys.Escape) return;
                // Esc maps to the same result MessageBox would give.
                if (buttons == MessageBoxButtons.OK) Result = DialogResult.OK;
                else if (buttons == MessageBoxButtons.YesNo) return;   // YesNo has no cancel
                else Result = DialogResult.Cancel;
                Close();
            };
        }

        private struct ButtonSpec
        {
            public string Text;
            public DialogResult Result;
            public bool Primary;
            public ButtonSpec(string t, DialogResult r, bool p) { Text = t; Result = r; Primary = p; }
        }

        // Returned right-to-left, since the layout above places from the right edge.
        private static ButtonSpec[] SpecsFor(MessageBoxButtons buttons)
        {
            switch (buttons)
            {
                case MessageBoxButtons.OKCancel:
                    return new[] { new ButtonSpec("Cancel", DialogResult.Cancel, false),
                                   new ButtonSpec("OK", DialogResult.OK, true) };
                case MessageBoxButtons.YesNo:
                    return new[] { new ButtonSpec("No", DialogResult.No, false),
                                   new ButtonSpec("Yes", DialogResult.Yes, true) };
                case MessageBoxButtons.YesNoCancel:
                    return new[] { new ButtonSpec("Cancel", DialogResult.Cancel, false),
                                   new ButtonSpec("No", DialogResult.No, false),
                                   new ButtonSpec("Yes", DialogResult.Yes, true) };
                case MessageBoxButtons.RetryCancel:
                    return new[] { new ButtonSpec("Cancel", DialogResult.Cancel, false),
                                   new ButtonSpec("Retry", DialogResult.Retry, true) };
                case MessageBoxButtons.AbortRetryIgnore:
                    return new[] { new ButtonSpec("Ignore", DialogResult.Ignore, false),
                                   new ButtonSpec("Retry", DialogResult.Retry, false),
                                   new ButtonSpec("Abort", DialogResult.Abort, true) };
                default:
                    return new[] { new ButtonSpec("OK", DialogResult.OK, true) };
            }
        }

        private static Button MakeButton(string text, bool primary)
        {
            Button b = new Button
            {
                Text = text,
                Size = new Size(88, 30),
                FlatStyle = FlatStyle.Flat,
                Font = Theme.UiFontBold,
                BackColor = primary ? Theme.Accent : Theme.RaisedBg,
                ForeColor = primary ? Color.FromArgb(20, 24, 18) : Theme.TextPrimary,
                Cursor = Cursors.Hand,
                Tag = Theme.SkipTag,
            };
            b.FlatAppearance.BorderColor = primary ? Theme.Accent : Theme.Border;
            b.FlatAppearance.MouseOverBackColor = primary ? ControlPaint.Light(Theme.Accent) : Theme.HoverBg;
            b.FlatAppearance.MouseDownBackColor = primary ? Theme.AccentDim : Theme.PressedBg;
            return b;
        }

        private void Drag(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) NativeDrag.ReleaseAndDrag(this);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Region = MessageDialog.RoundedRegion(ClientRectangle, 10);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (Pen p = new Pen(Theme.Border))
                g.DrawPath(p, MessageDialog.RoundedPath(new Rectangle(0, 0, Width - 1, Height - 1), 10));

            if (_icon == MessageBoxIcon.None) return;
            DrawIcon(g, new Rectangle(24, 22, 34, 34));
        }

        // Drawn rather than using the system icons, which are full-colour and look pasted-on
        // against a flat dark dialog.
        private void DrawIcon(Graphics g, Rectangle r)
        {
            Color c;
            string glyph;
            switch (_icon)
            {
                case MessageBoxIcon.Error:      c = Theme.Danger; glyph = "!"; break;
                case MessageBoxIcon.Warning:    c = Color.FromArgb(226, 178, 74); glyph = "!"; break;
                case MessageBoxIcon.Question:   c = Theme.Accent; glyph = "?"; break;
                default:                        c = Color.FromArgb(96, 156, 220); glyph = "i"; break;
            }

            using (SolidBrush fill = new SolidBrush(Color.FromArgb(38, c)))
            using (Pen ring = new Pen(c, 1.6f))
            {
                g.FillEllipse(fill, r);
                g.DrawEllipse(ring, r);
            }
            using (SolidBrush tb = new SolidBrush(c))
            using (StringFormat sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            })
            using (Font f = new Font("Segoe UI", 15F, FontStyle.Bold))
            {
                g.DrawString(glyph, f, tb, r, sf);
            }
        }
    }
}
