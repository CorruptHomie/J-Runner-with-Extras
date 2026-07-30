using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace JRunner
{
    // Rewritten without AeroWizard. That control paints its own header and page surround
    // from system colours and exposes no way to change them, and its header is a child
    // control so painting over the parent couldn't reach it either - the white strip
    // survived every attempt. This form only ever used the wizard as a titled box with a
    // Close button, so it's built directly instead: no third-party chrome, nothing to fight,
    // and it picks up UI.Theme like every other window.
    public partial class Issues : Form
    {
        private const int TitleBarHeight = 38;
        private const int FooterHeight = 48;

        public Issues()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(554, 401);
            BackColor = UI.Theme.WindowBg;
            DoubleBuffered = true;
            KeyPreview = true;
            Text = "Report Issue";
            ShowInTaskbar = false;
            KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) Close(); };

            BuildTitleBar();
            BuildBody();
            BuildFooter();

            UI.Theme.ApplyTheme(this);
        }

        private void BuildTitleBar()
        {
            Panel bar = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(ClientSize.Width, TitleBarHeight),
                BackColor = UI.Theme.WindowBg,
                Tag = UI.Theme.SkipTag,
            };

            PictureBox icon = new PictureBox
            {
                Image = Properties.Resources.JR,
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point(12, 9),
                Size = new Size(20, 20),
                Tag = UI.Theme.SkipTag,
            };

            Label title = new Label
            {
                Text = "Report Issue",
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = UI.Theme.TextPrimary,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(40, 0),
                Size = new Size(380, TitleBarHeight),
                Tag = UI.Theme.SkipTag,
            };

            bool hover = false;
            Label close = new Label
            {
                Location = new Point(ClientSize.Width - 46, 0),
                Size = new Size(46, TitleBarHeight),
                BackColor = UI.Theme.WindowBg,
                Cursor = Cursors.Hand,
                Tag = UI.Theme.SkipTag,
            };
            close.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                int cx = close.Width / 2, cy = close.Height / 2;
                const int r = 5;
                using (Pen p = new Pen(hover ? Color.White : UI.Theme.ChromeButton, 1.3f))
                {
                    e.Graphics.DrawLine(p, cx - r, cy - r, cx + r, cy + r);
                    e.Graphics.DrawLine(p, cx + r, cy - r, cx - r, cy + r);
                }
            };
            close.MouseEnter += (s, e) => { hover = true; close.BackColor = UI.Theme.Danger; close.Invalidate(); };
            close.MouseLeave += (s, e) => { hover = false; close.BackColor = UI.Theme.WindowBg; close.Invalidate(); };
            close.Click += (s, e) => Close();

            Panel separator = new Panel
            {
                Location = new Point(0, TitleBarHeight - 1),
                Size = new Size(ClientSize.Width, 1),
                BackColor = UI.Theme.BorderSubtle,
                Tag = UI.Theme.SkipTag,
            };

            bar.MouseDown += Drag;
            title.MouseDown += Drag;
            icon.MouseDown += Drag;

            bar.Controls.Add(icon);
            bar.Controls.Add(title);
            bar.Controls.Add(close);
            Controls.Add(bar);
            Controls.Add(separator);
        }

        private void BuildBody()
        {
            Label heading = new Label
            {
                Text = "Find a bug? Have a suggestion?",
                Font = new Font("Segoe UI", 12F, FontStyle.Regular),
                ForeColor = UI.Theme.Accent,
                AutoSize = false,
                Location = new Point(24, TitleBarHeight + 22),
                Size = new Size(500, 26),
                Tag = UI.Theme.SkipTag,
            };

            Label body = new Label
            {
                Text = "We'd love to hear from you in order to make J-Runner Premium better!"
                     + Environment.NewLine + Environment.NewLine
                     + "Please make sure you check open issues first before creating an issue, "
                     + "as duplicates will be closed.",
                Font = UI.Theme.UiFont,
                ForeColor = UI.Theme.TextSecondary,
                AutoSize = false,
                Location = new Point(26, TitleBarHeight + 56),
                Size = new Size(500, 62),
                Tag = UI.Theme.SkipTag,
            };

            Button view = MakeButton("View Open Issues", new Point(26, TitleBarHeight + 130));
            view.Click += ViewButton_Click;

            Button create = MakeButton("Create New Issue", new Point(26, TitleBarHeight + 172));
            create.Click += CreateButton_Click;

            Controls.Add(heading);
            Controls.Add(body);
            Controls.Add(view);
            Controls.Add(create);
        }

        private void BuildFooter()
        {
            Panel footer = new Panel
            {
                Location = new Point(0, ClientSize.Height - FooterHeight),
                Size = new Size(ClientSize.Width, FooterHeight),
                BackColor = UI.Theme.PanelBg,
            };
            Panel edge = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(ClientSize.Width, 1),
                BackColor = UI.Theme.BorderSubtle,
                Tag = UI.Theme.SkipTag,
            };

            Button close = new Button
            {
                Text = "Close",
                Size = new Size(96, 30),
                Location = new Point(ClientSize.Width - 96 - 18, (FooterHeight - 30) / 2),
                FlatStyle = FlatStyle.Flat,
                Font = UI.Theme.UiFontBold,
                Cursor = Cursors.Hand,
            };
            close.Click += (s, e) => Close();

            footer.Controls.Add(edge);
            footer.Controls.Add(close);
            Controls.Add(footer);
            AcceptButton = close;
        }

        private Button MakeButton(string text, Point location)
        {
            return new Button
            {
                Text = text,
                Location = location,
                Size = new Size(232, 30),
                FlatStyle = FlatStyle.Flat,
                Font = UI.Theme.UiFont,
                Cursor = Cursors.Hand,
            };
        }

        private void Drag(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) UI.NativeDrag.ReleaseAndDrag(this);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Region = UI.MessageDialog.RoundedRegion(ClientRectangle, 10);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (Pen p = new Pen(UI.Theme.Border))
                e.Graphics.DrawPath(p, UI.MessageDialog.RoundedPath(new Rectangle(0, 0, Width - 1, Height - 1), 10));
        }

        private void ViewButton_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/ScallywagDude/J-Runner-Premium/issues");
        }

        private void CreateButton_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/ScallywagDude/J-Runner-Premium/issues/new/choose");
        }
    }
}
