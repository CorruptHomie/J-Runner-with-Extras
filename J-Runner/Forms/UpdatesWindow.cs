using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;

namespace JRunner.Forms
{
    // Changelog on the left, "Updates" and "Update Channels" boxes stacked on the right.
    // Built in code rather than as a Designer pair - it's a self-contained window with no
    // legacy layout to preserve, and this way it picks up UI.Theme without needing the
    // theme pass to undo designer defaults first.
    [System.ComponentModel.DesignerCategory("")]
    public class UpdatesWindow : Form
    {
        private readonly RichTextBox _changelog;
        private readonly Button _actionButton;
        private readonly Label _status;
        private readonly Label _channelHint;
        private readonly RadioButton _rbRelease, _rbPreRelease, _rbDev;

        private bool _busy;
        private bool _updateReady;   // action button is in "Download ..." mode
        private bool _suppressChannelEvents;

        public UpdatesWindow()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            BackColor = UI.Theme.WindowBg;
            Size = new Size(780, 420);
            DoubleBuffered = true;
            KeyPreview = true;
            KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape && !_busy) Close(); };

            // ---- title bar ----
            // Sized to the form up front: the close button is positioned from the right
            // edge, and a freshly-constructed Panel is only 200px wide until docking runs,
            // so adding the button first put it outside the panel's bounds and it never
            // appeared.
            Panel titleBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 38,
                Width = ClientSize.Width,
                BackColor = UI.Theme.WindowBg,
                Tag = UI.Theme.SkipTag,
            };
            Label title = new Label
            {
                Text = "Updates",
                ForeColor = UI.Theme.TextPrimary,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(18, 0),
                Size = new Size(400, 38),
                Tag = UI.Theme.SkipTag,
            };

            bool closeHover = false;
            Label close = new Label
            {
                Text = "",
                BackColor = UI.Theme.WindowBg,
                Location = new Point(titleBar.Width - 46, 0),
                Size = new Size(46, 38),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Tag = UI.Theme.SkipTag,
            };
            // Drawn rather than a text glyph, to match the main window's window buttons.
            close.Paint += (s2, e2) =>
            {
                e2.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Color c = closeHover ? Color.White : UI.Theme.ChromeButton;
                int cx = close.Width / 2, cy = close.Height / 2;
                const int r = 5;
                using (Pen pen = new Pen(c, 1.3f))
                {
                    e2.Graphics.DrawLine(pen, cx - r, cy - r, cx + r, cy + r);
                    e2.Graphics.DrawLine(pen, cx + r, cy - r, cx - r, cy + r);
                }
            };
            close.Click += (s2, e2) => { if (!_busy) Close(); };
            close.MouseEnter += (s2, e2) => { closeHover = true; close.BackColor = UI.Theme.Danger; close.Invalidate(); };
            close.MouseLeave += (s2, e2) => { closeHover = false; close.BackColor = UI.Theme.WindowBg; close.Invalidate(); };

            titleBar.MouseDown += (s2, e2) => { if (e2.Button == MouseButtons.Left) UI.NativeDrag.ReleaseAndDrag(this); };
            title.MouseDown += (s2, e2) => { if (e2.Button == MouseButtons.Left) UI.NativeDrag.ReleaseAndDrag(this); };
            titleBar.Controls.Add(title);
            titleBar.Controls.Add(close);

            // ---- changelog (left) ----
            Label changelogCaption = new Label
            {
                Text = "Changelog",
                ForeColor = UI.Theme.TextSecondary,
                Font = UI.Theme.UiFontBold,
                AutoSize = false,
                Location = new Point(18, 48),
                Size = new Size(200, 18),
                Tag = UI.Theme.SkipTag,
            };
            _changelog = new RichTextBox
            {
                Location = new Point(18, 70),
                Size = new Size(452, 326),
                BackColor = UI.Theme.FieldBg,
                ForeColor = UI.Theme.TextPrimary,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                Font = new Font("Consolas", 9F),
                DetectUrls = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right,
            };
            _changelog.LinkClicked += (s, e) =>
            {
                try { System.Diagnostics.Process.Start(e.LinkText); } catch { }
            };

            // ---- "Updates" box (right, top) ----
            GroupBox updatesBox = MakeBox("Updates", new Point(490, 60), new Size(272, 140));

            Label current = new Label
            {
                Text = "Installed: " + variables.version,
                ForeColor = UI.Theme.TextSecondary,
                Font = UI.Theme.UiFont,
                AutoSize = false,
                Location = new Point(14, 26),
                Size = new Size(244, 18),
                Tag = UI.Theme.SkipTag,
            };

            _actionButton = new Button
            {
                Text = "Check for update",
                Location = new Point(14, 50),
                Size = new Size(244, 34),
                FlatStyle = FlatStyle.Flat,
                Font = UI.Theme.UiFontBold,
                BackColor = UI.Theme.RaisedBg,
                ForeColor = UI.Theme.TextPrimary,
                Cursor = Cursors.Hand,
            };
            _actionButton.FlatAppearance.BorderColor = UI.Theme.Border;
            _actionButton.Click += ActionButton_Click;

            _status = new Label
            {
                Text = "",
                ForeColor = UI.Theme.TextSecondary,
                Font = UI.Theme.UiFont,
                AutoSize = false,
                Location = new Point(14, 90),
                Size = new Size(244, 40),
                Tag = UI.Theme.SkipTag,
            };

            updatesBox.Controls.Add(current);
            updatesBox.Controls.Add(_actionButton);
            updatesBox.Controls.Add(_status);

            // ---- "Update Channels" box (right, bottom) ----
            GroupBox channelsBox = MakeBox("Update Channels", new Point(490, 212), new Size(272, 148));

            _rbRelease = MakeChannelRadio("Release", 26);
            _rbPreRelease = MakeChannelRadio("Pre-release", 52);
            _rbDev = MakeChannelRadio("Dev", 78);

            _channelHint = new Label
            {
                Text = "",
                ForeColor = UI.Theme.TextSecondary,
                Font = UI.Theme.UiFont,
                AutoSize = false,
                Location = new Point(14, 102),
                Size = new Size(244, 36),
                Tag = UI.Theme.SkipTag,
            };

            channelsBox.Controls.Add(_rbRelease);
            channelsBox.Controls.Add(_rbPreRelease);
            channelsBox.Controls.Add(_rbDev);
            channelsBox.Controls.Add(_channelHint);

            Controls.Add(_changelog);
            Controls.Add(changelogCaption);
            Controls.Add(updatesBox);
            Controls.Add(channelsBox);
            Controls.Add(titleBar);

            SelectChannelRadio(Upd.CurrentChannel);
            UpdateChannelHint();
            ShowIdleChangelog();

            UI.Theme.ApplyTheme(this);
        }

        private GroupBox MakeBox(string text, Point location, Size size)
        {
            return new GroupBox
            {
                Text = text,
                Location = location,
                Size = size,
                ForeColor = UI.Theme.TextSecondary,
                BackColor = UI.Theme.PanelBg,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
            };
        }

        private RadioButton MakeChannelRadio(string text, int y)
        {
            RadioButton rb = new RadioButton
            {
                Text = text,
                Location = new Point(14, y),
                Size = new Size(240, 22),
                ForeColor = UI.Theme.TextPrimary,
                Cursor = Cursors.Hand,
            };
            rb.CheckedChanged += Channel_CheckedChanged;
            return rb;
        }

        private void SelectChannelRadio(Upd.UpdateChannel channel)
        {
            _suppressChannelEvents = true;
            _rbRelease.Checked = channel == Upd.UpdateChannel.Release;
            _rbPreRelease.Checked = channel == Upd.UpdateChannel.PreRelease;
            _rbDev.Checked = channel == Upd.UpdateChannel.Dev;
            _suppressChannelEvents = false;
        }

        private Upd.UpdateChannel SelectedChannel()
        {
            if (_rbPreRelease.Checked) return Upd.UpdateChannel.PreRelease;
            if (_rbDev.Checked) return Upd.UpdateChannel.Dev;
            return Upd.UpdateChannel.Release;
        }

        private void Channel_CheckedChanged(object sender, EventArgs e)
        {
            if (_suppressChannelEvents || _busy) return;
            RadioButton rb = sender as RadioButton;
            if (rb != null && !rb.Checked) return;   // only react to the one being turned on

            Upd.CurrentChannel = SelectedChannel();
            UpdateChannelHint();

            // A result from the previous channel says nothing about this one, so drop back
            // to "Check for update" rather than leaving a stale Download button pointing at
            // a release from a different stream.
            _updateReady = false;
            _actionButton.Text = "Check for update";
            _status.Text = "";
            ShowIdleChangelog();
        }

        private void UpdateChannelHint()
        {
            _channelHint.Text = Upd.DescribeChannel(SelectedChannel());
        }

        private void ShowIdleChangelog()
        {
            _changelog.Clear();
            _changelog.SelectionColor = UI.Theme.TextSecondary;
            _changelog.AppendText("Check for an update to see what's changed in the latest "
                                + SelectedChannel() + " build.");
        }

        private void ActionButton_Click(object sender, EventArgs e)
        {
            if (_busy) return;

            if (_updateReady)
            {
                // Hands off to the existing update wizard, which downloads, backs up the
                // current exe and extracts over the install.
                Upd.startFull();
                return;
            }

            _busy = true;
            _actionButton.Enabled = false;
            _actionButton.Text = "Checking...";
            _status.ForeColor = UI.Theme.TextSecondary;
            _status.Text = "Contacting GitHub...";

            Thread t = new Thread(() =>
            {
                Upd.check();
                try { Invoke(new Action(CheckFinished)); } catch { /* window closed mid-check */ }
            });
            t.IsBackground = true;
            t.Start();
        }

        private void CheckFinished()
        {
            _busy = false;
            _actionButton.Enabled = true;

            if (!Upd.checkSuccess)
            {
                _updateReady = false;
                _actionButton.Text = "Check for update";
                _status.ForeColor = UI.Theme.Danger;
                _status.Text = "Check failed: " + (Upd.failedReason ?? "unknown error");
                return;
            }

            if (Upd.upToDate)
            {
                _updateReady = false;
                _actionButton.Text = "Check for update";
                _status.ForeColor = UI.Theme.Accent;
                _status.Text = "You're on the latest " + SelectedChannel() + " build.";
                ShowIdleChangelog();
                return;
            }

            _updateReady = true;
            _actionButton.Text = "Download version " + Upd.pendingVersion;
            _status.ForeColor = UI.Theme.Accent;
            _status.Text = "Update available.";
            ShowChangelog(Upd.pendingVersion, Upd.changelog);
        }

        private void ShowChangelog(string version, string body)
        {
            _changelog.Clear();
            _changelog.SelectionColor = UI.Theme.Accent;
            _changelog.SelectionFont = new Font("Consolas", 10F, FontStyle.Bold);
            _changelog.AppendText(version + Environment.NewLine + Environment.NewLine);
            _changelog.SelectionFont = new Font("Consolas", 9F);
            _changelog.SelectionColor = UI.Theme.TextPrimary;
            _changelog.AppendText(string.IsNullOrWhiteSpace(body)
                ? "(This release has no changelog notes.)"
                : body);
            _changelog.SelectionStart = 0;
            _changelog.ScrollToCaret();
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
            using (Pen p = new Pen(UI.Theme.Border, 1))
                e.Graphics.DrawPath(p, UI.MessageDialog.RoundedPath(new Rectangle(0, 0, Width - 1, Height - 1), 10));
        }
    }
}
