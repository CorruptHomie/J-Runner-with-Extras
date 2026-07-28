using System;
using System.Drawing;
using System.Windows.Forms;

namespace JRunner
{
    // "Make the entire window one whole thing rather than having the actual window, a
    // context menu bar, and a title bar" - this removes the native title bar
    // (FormBorderStyle.None) and turns the existing menuStrip1 into a title bar by sharing
    // its row with a small icon and three window-control glyphs, all within the exact same
    // 24px strip menuStrip1 already occupied. Nothing else on the form moves, since that
    // strip was already reserved at the top of the client area.
    public partial class MainForm
    {
        // The designer laid this form out with menuStrip1 occupying a 24px strip and
        // pnlTools starting immediately below it at y=24. 24px is cramped for a borderless
        // window's title bar, so the bar is 36px and every absolutely-positioned top-level
        // control gets shifted down by the difference once at startup (ShiftContentDown).
        private const int LegacyMenuStripHeight = 24;
        private const int TitleBarHeight = 36;
        private const int ChromeButtonWidth = 46;
        private const int ResizeMargin = 6;

        private Label _btnMinimize;
        private Label _btnMaximize;
        private Label _btnClose;
        private PictureBox _titleIcon;
        private Panel _titleSeparator;

        private void SetupCustomChrome()
        {
            FormBorderStyle = FormBorderStyle.None;

            ShiftContentDown(TitleBarHeight - LegacyMenuStripHeight);

            menuStrip1.BackColor = UI.Theme.WindowBg;
            menuStrip1.Location = new Point(40, (TitleBarHeight - LegacyMenuStripHeight) / 2);
            menuStrip1.Size = new Size(Math.Max(0, ClientSize.Width - 40 - (ChromeButtonWidth * 3)), LegacyMenuStripHeight);
            menuStrip1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            _titleIcon = new PictureBox
            {
                Image = Properties.Resources.JR,
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point(10, (TitleBarHeight - 20) / 2),
                Size = new Size(20, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Tag = UI.Theme.SkipTag,
            };

            // Hairline under the title strip so the bar reads as its own surface instead of
            // blending into the content below it.
            _titleSeparator = new Panel
            {
                BackColor = UI.Theme.BorderSubtle,
                Location = new Point(0, TitleBarHeight - 1),
                Size = new Size(ClientSize.Width, 1),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Tag = UI.Theme.SkipTag,
            };

            _btnMinimize = MakeChromeButton("\u2013", ClientSize.Width - ChromeButtonWidth * 3, isClose: false);
            _btnMaximize = MakeChromeButton("\u25A1", ClientSize.Width - ChromeButtonWidth * 2, isClose: false);
            _btnClose = MakeChromeButton("\u2715", ClientSize.Width - ChromeButtonWidth, isClose: true);

            _btnMinimize.Click += (s, e) => WindowState = FormWindowState.Minimized;
            _btnMaximize.Click += (s, e) => ToggleMaximize();
            _btnClose.Click += (s, e) => Close();

            Controls.Add(_titleIcon);
            Controls.Add(_titleSeparator);
            Controls.Add(_btnMinimize);
            Controls.Add(_btnMaximize);
            Controls.Add(_btnClose);

            _titleIcon.BringToFront();
            _titleSeparator.BringToFront();
            _btnMinimize.BringToFront();
            _btnMaximize.BringToFront();
            _btnClose.BringToFront();
            menuStrip1.BringToFront();

            Resize += (s, e) =>
            {
                if (_btnMaximize != null)
                    _btnMaximize.Text = WindowState == FormWindowState.Maximized ? "\u2752" : "\u25A1";
            };

            // Theme.ApplyTheme is deliberately NOT called here - at this point in the
            // constructor, pnlInfo/pnlTools/pnlExtra don't have nandInfo/nTools/xPanel
            // added to them yet (that happens a few lines later in MainForm()), so a theme
            // walk here would never reach any of the controls those panels actually hold.
            // See startMainForm() in MainForm.cs, which runs once everything is in place.
        }

        private UI.FlashProgressOverlay _flashOverlay;

        // Called from writenand()'s flash path, which runs on a background thread - hence
        // the InvokeRequired marshalling. Both are idempotent so the finally-block safety
        // net can call Hide even if Show never ran or it was already closed.
        internal void ShowFlashOverlay()
        {
            if (InvokeRequired) { Invoke(new Action(ShowFlashOverlay)); return; }
            if (_flashOverlay != null) return;
            try
            {
                _flashOverlay = new UI.FlashProgressOverlay(this, progressBar);
                _flashOverlay.Show(this);
            }
            catch (Exception ex)
            {
                // A cosmetic overlay must never take a flash down with it.
                if (variables.debugme) Console.WriteLine(ex.ToString());
                _flashOverlay = null;
            }
        }

        internal void HideFlashOverlay()
        {
            if (InvokeRequired) { Invoke(new Action(HideFlashOverlay)); return; }
            if (_flashOverlay == null) return;
            try
            {
                _flashOverlay.Close();
                _flashOverlay.Dispose();
            }
            catch (Exception ex) { if (variables.debugme) Console.WriteLine(ex.ToString()); }
            _flashOverlay = null;
        }

        // One-time downward shift of the designer's absolutely-positioned layout to make
        // room for the taller title bar. Docked controls (splitter1, statusStrip1) position
        // themselves and are skipped; the chrome controls don't exist yet when this runs.
        private void ShiftContentDown(int offset)
        {
            if (offset <= 0) return;

            SuspendLayout();
            foreach (Control c in Controls)
            {
                if (c == menuStrip1) continue;
                if (c.Dock != DockStyle.None) continue;
                c.Top += offset;
            }
            ClientSize = new Size(ClientSize.Width, ClientSize.Height + offset);
            ResumeLayout();
        }

        // A borderless form loses the OS drop shadow; CS_DROPSHADOW puts it back so the
        // window still separates from whatever is behind it.
        protected override CreateParams CreateParams
        {
            get
            {
                const int CS_DROPSHADOW = 0x00020000;
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        private Label MakeChromeButton(string glyph, int x, bool isClose)
        {
            Color idleBg = UI.Theme.WindowBg;
            Color hoverBg = isClose ? UI.Theme.Danger : UI.Theme.HoverBg;
            Color idleFg = UI.Theme.ChromeButton;
            Color hoverFg = isClose ? Color.White : UI.Theme.ChromeButtonHover;

            Label l = new Label
            {
                Text = glyph,
                Font = new Font("Segoe UI", 10F),
                ForeColor = idleFg,
                BackColor = idleBg,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(x, 0),
                Size = new Size(ChromeButtonWidth, TitleBarHeight - 1),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Cursor = Cursors.Hand,
                Tag = UI.Theme.SkipTag,
            };
            l.MouseEnter += (s, e) => { l.BackColor = hoverBg; l.ForeColor = hoverFg; };
            l.MouseLeave += (s, e) => { l.BackColor = idleBg; l.ForeColor = idleFg; };
            return l;
        }

        private void ToggleMaximize()
        {
            WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
        }

        // Standard borderless-window drag/resize pattern: let the default handler compute
        // WM_NCHITTEST first, and only override it (to claim the resize edges, or turn the
        // empty part of the title strip into a caption you can drag/double-click-maximize
        // by) when the default result was an ordinary client-area hit.
        //
        // The WndProc override itself lives in MainForm.cs — a partial class can only
        // define it once — where it calls HitTestChrome below before falling through to
        // that file's own WM_DEVICECHANGE / WM_SHOWAPP handling.
        private int HitTestChrome(Point client)
        {
            const int HTCAPTION = 2, HTLEFT = 10, HTRIGHT = 11, HTTOP = 12,
                      HTTOPLEFT = 13, HTTOPRIGHT = 14, HTBOTTOM = 15, HTBOTTOMLEFT = 16, HTBOTTOMRIGHT = 17;

            if (WindowState != FormWindowState.Normal) return 0;

            bool left = client.X <= ResizeMargin;
            bool right = client.X >= ClientSize.Width - ResizeMargin;
            bool top = client.Y <= ResizeMargin;
            bool bottom = client.Y >= ClientSize.Height - ResizeMargin;

            if (top && left) return HTTOPLEFT;
            if (top && right) return HTTOPRIGHT;
            if (bottom && left) return HTBOTTOMLEFT;
            if (bottom && right) return HTBOTTOMRIGHT;
            if (left) return HTLEFT;
            if (right) return HTRIGHT;
            if (top) return HTTOP;
            if (bottom) return HTBOTTOM;

            if (client.Y < TitleBarHeight && !IsOverInteractiveChrome(client)) return HTCAPTION;

            return 0;
        }

        private bool IsOverInteractiveChrome(Point client)
        {
            if (_titleIcon.Bounds.Contains(client)) return true;
            if (_btnMinimize.Bounds.Contains(client)) return true;
            if (_btnMaximize.Bounds.Contains(client)) return true;
            if (_btnClose.Bounds.Contains(client)) return true;

            if (menuStrip1.Bounds.Contains(client))
            {
                Point rel = new Point(client.X - menuStrip1.Left, client.Y - menuStrip1.Top);
                foreach (ToolStripItem item in menuStrip1.Items)
                {
                    if (item.Visible && item.Bounds.Contains(rel)) return true;
                }
                return false; // inside the strip but past the last item - treat as draggable
            }

            return false;
        }
    }
}
