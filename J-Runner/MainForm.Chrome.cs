using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace JRunner
{
    // Merges the native title bar, the menu and the window into one surface. The designer
    // laid this form out with menuStrip1 occupying a 24px strip and pnlTools starting
    // immediately below at y=24; 24px is cramped for a borderless window's title bar, so
    // the bar is 36px and the absolutely-positioned layout is shifted down by the
    // difference once at startup (ShiftContentDown).
    public partial class MainForm
    {
        private const int LegacyMenuStripHeight = 24;
        private const int TitleBarHeight = 36;
        private const int ChromeButtonWidth = 46;

        // Maximize is deliberately absent - the layout is absolutely positioned and doesn't
        // reflow, so the window is fixed at its design size.
        private enum ChromeGlyph { Minimize, Close }

        private Label _btnMinimize;
        private Label _btnClose;
        private Panel _titleSeparator;

        private void SetupCustomChrome()
        {
            FormBorderStyle = FormBorderStyle.None;

            ShiftContentDown(TitleBarHeight - LegacyMenuStripHeight);

            // The designer sets RenderMode = Professional, which makes the strip use its
            // own ToolStripProfessionalRenderer and ignore ToolStripManager.Renderer
            // entirely - that's why the menu text stayed black-on-dark despite the custom
            // renderer being registered. ManagerRenderMode opts back into it.
            menuStrip1.RenderMode = ToolStripRenderMode.ManagerRenderMode;
            menuStrip1.BackColor = UI.Theme.WindowBg;
            menuStrip1.ForeColor = UI.Theme.TextPrimary;
            // menuStrip1's own first item already carries the JR logo (and its menu), so
            // there's deliberately no separate icon control here - having both was showing
            // two JR logos side by side.
            menuStrip1.Location = new Point(4, (TitleBarHeight - LegacyMenuStripHeight) / 2);
            menuStrip1.Size = new Size(Math.Max(0, ClientSize.Width - 4 - (ChromeButtonWidth * 3)), LegacyMenuStripHeight);
            menuStrip1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // Hairline under the title strip so the bar reads as its own surface.
            _titleSeparator = new Panel
            {
                BackColor = UI.Theme.BorderSubtle,
                Location = new Point(0, TitleBarHeight - 1),
                Size = new Size(ClientSize.Width, 1),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Tag = UI.Theme.SkipTag,
            };

            _btnMinimize = MakeChromeButton(ChromeGlyph.Minimize, ClientSize.Width - ChromeButtonWidth * 2);
            _btnClose = MakeChromeButton(ChromeGlyph.Close, ClientSize.Width - ChromeButtonWidth);

            _btnMinimize.Click += (s, e) => WindowState = FormWindowState.Minimized;
            _btnClose.Click += (s, e) => Close();

            Controls.Add(_titleSeparator);
            Controls.Add(_btnMinimize);
            Controls.Add(_btnClose);

            _titleSeparator.BringToFront();
            _btnMinimize.BringToFront();
            _btnClose.BringToFront();
            menuStrip1.BringToFront();

            Resize += (s, e) => LayoutTitleBar();
            LayoutTitleBar();
        }

        // Small post-designer layout corrections, applied once at startup alongside the
        // theme pass. Done in code rather than the designer so the .Designer.cs stays
        // regenerable.
        private void TweakLayout()
        {
            // "Scan IP" was a loose button sitting just below the IP / Get CPU Key group
            // box, visually orphaned from the two controls it belongs with. Reparenting it
            // into that group box makes the grouping match what it actually does.
            if (btnScanner != null && groupBox8 != null && btnScanner.Parent != groupBox8)
            {
                groupBox8.Height = 100;
                Controls.Remove(btnScanner);
                groupBox8.Controls.Add(btnScanner);
                btnScanner.Location = new Point(6, 67);
                btnScanner.Size = new Size(153, 26);
            }
        }

        // Anchors alone weren't enough here: the strip is sized in the constructor, before
        // the form has its final client size, and right-aligned menu items (the version /
        // About entry) were laying out against that stale width - so they sat off the edge
        // until a resize recalculated everything.
        private void LayoutTitleBar()
        {
            if (menuStrip1 == null) return;
            int right = ChromeButtonWidth * 2;
            menuStrip1.Bounds = new Rectangle(4, (TitleBarHeight - LegacyMenuStripHeight) / 2,
                                              Math.Max(0, ClientSize.Width - 4 - right), LegacyMenuStripHeight);
            if (_titleSeparator != null)
                _titleSeparator.Bounds = new Rectangle(0, TitleBarHeight - 1, ClientSize.Width, 1);
        }

        private UI.FlashProgressOverlay _flashOverlay;

        // Called from the write paths, which run on background threads - hence the
        // InvokeRequired marshalling. Both are idempotent so the finally-block safety net
        // can call Hide even if Show never ran or it was already closed.
        internal void ShowFlashOverlay()
        {
            if (InvokeRequired) { Invoke(new Action(ShowFlashOverlay)); return; }
            if (_flashOverlay != null) return;
            try
            {
                _flashOverlay = new UI.FlashProgressOverlay(this, progressBar);
                _flashOverlay.Finished += FlashOverlay_Finished;
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
                _flashOverlay.Finished -= FlashOverlay_Finished;
                _flashOverlay.Close();
                _flashOverlay.Dispose();
            }
            catch (Exception ex) { if (variables.debugme) Console.WriteLine(ex.ToString()); }
            _flashOverlay = null;
        }

        // Raised when the overlay decides the write finished on its own (progress sat at
        // 100% for a moment). Needed because the flasher-device write paths -
        // picoflasher.Write / xflasher.writeNandAuto / mtx_usb.writeNandAuto - never go
        // through writenand(), so nothing else would close it or tell the user.
        private void FlashOverlay_Finished(bool completed)
        {
            if (InvokeRequired) { BeginInvoke(new Action<bool>(FlashOverlay_Finished), completed); return; }
            HideFlashOverlay();
            SetFlashing(false);
            if (completed) UI.ThemedDialogs.ShowFlashComplete(this);
        }

        // Single entry point for "a NAND write is starting / has ended", so the confirm
        // dialog, the logo animation and the overlay stay in step across every device path.
        internal bool BeginFlash()
        {
            if (!UI.ThemedDialogs.ConfirmFlash(this)) return false;
            SetFlashing(true);
            ShowFlashOverlay();
            return true;
        }

        internal void SetFlashing(bool flashing)
        {
            UI.XboxFillProgressBar bar = progressBar as UI.XboxFillProgressBar;
            if (bar != null) bar.IsFlashing = flashing;
        }

        private Label MakeChromeButton(ChromeGlyph kind, int x)
        {
            bool isClose = kind == ChromeGlyph.Close;
            bool hover = false;

            Label l = new Label
            {
                Text = "",
                BackColor = UI.Theme.WindowBg,
                Location = new Point(x, 0),
                Size = new Size(ChromeButtonWidth, TitleBarHeight - 1),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Cursor = Cursors.Hand,
                Tag = UI.Theme.SkipTag,
            };

            // Drawn rather than typed: the box and dash glyphs render tiny and inconsistent
            // at text sizes, which is what made the maximize button hard to hit visually.
            l.Paint += (s, e) => DrawChromeGlyph(e.Graphics, l, kind, hover, isClose);
            l.MouseEnter += (s, e) =>
            {
                hover = true;
                l.BackColor = isClose ? UI.Theme.Danger : UI.Theme.HoverBg;
                l.Invalidate();
            };
            l.MouseLeave += (s, e) =>
            {
                hover = false;
                l.BackColor = UI.Theme.WindowBg;
                l.Invalidate();
            };
            return l;
        }

        private void DrawChromeGlyph(Graphics g, Label host, ChromeGlyph kind, bool hover, bool isClose)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Color colour = hover
                ? (isClose ? Color.White : UI.Theme.ChromeButtonHover)
                : UI.Theme.ChromeButton;

            int cx = host.Width / 2;
            int cy = host.Height / 2;
            const int r = 5;

            using (Pen p = new Pen(colour, 1.3f))
            {
                switch (kind)
                {
                    case ChromeGlyph.Minimize:
                        g.DrawLine(p, cx - r, cy, cx + r, cy);
                        break;

                    case ChromeGlyph.Close:
                        g.DrawLine(p, cx - r, cy - r, cx + r, cy + r);
                        g.DrawLine(p, cx + r, cy - r, cx - r, cy + r);
                        break;
                }
            }
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

            // This layout is absolutely positioned - nothing reflows, so any size other than
            // the design size either clips it or leaves dead space. Pinned rather than just
            // floored, and the maximize button is gone for the same reason.
            Size fixedSize = new Size(ClientSize.Width, ClientSize.Height);
            MinimumSize = fixedSize;
            MaximumSize = fixedSize;
            MaximizeBox = false;
            ResumeLayout();
        }

        // A borderless form loses the OS drop shadow; CS_DROPSHADOW puts it back.
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

        // Standard borderless-window drag/resize: let the default handler compute
        // WM_NCHITTEST first, and only override it (to claim the resize edges, or turn the
        // empty part of the title strip into a draggable caption) when the default result
        // was an ordinary client-area hit.
        private int ChromeHitTest(Point client)
        {
            const int HTCAPTION = 2;

            // No resize edges - the window is a fixed size (see ShiftContentDown).
            if (client.Y < TitleBarHeight && !IsOverInteractiveChrome(client)) return HTCAPTION;
            return 0;
        }

        private bool IsOverInteractiveChrome(Point client)
        {
            if (_btnMinimize != null && _btnMinimize.Bounds.Contains(client)) return true;
            if (_btnClose != null && _btnClose.Bounds.Contains(client)) return true;

            if (menuStrip1.Bounds.Contains(client))
            {
                Point rel = new Point(client.X - menuStrip1.Left, client.Y - menuStrip1.Top);
                foreach (ToolStripItem item in menuStrip1.Items)
                {
                    if (item.Visible && item.Bounds.Contains(rel)) return true;
                }
                return false; // inside the strip but past the last item - draggable
            }

            return false;
        }
    }
}
