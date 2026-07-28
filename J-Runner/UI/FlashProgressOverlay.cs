using System;
using System.Drawing;
using System.Windows.Forms;

namespace UI
{
    /// <summary>
    /// Shown only while a NAND flash is running. Dims the main window and puts the Xbox
    /// logo up large and centered, black and white, filling with color as the write
    /// progresses - which is where the "NaND flasher progress bar = the logo filling up"
    /// actually lives. The thin bar on MainForm is shared by reads, ECC builds and other
    /// operations, and is only 20px tall, so it stays a normal themed bar.
    ///
    /// Mirrors the existing progress bar rather than hooking into the write path, so
    /// nothing in NandX / VNand / the flashing code had to change - whatever already
    /// drives that bar drives this too.
    /// </summary>
    [System.ComponentModel.DesignerCategory("")]
    public class FlashProgressOverlay : Form
    {
        private readonly ProgressBar _source;
        private readonly XboxFillProgressBar _logo;
        private readonly Label _caption;
        private readonly Label _hint;
        private readonly Timer _mirror;

        public FlashProgressOverlay(Form owner, ProgressBar source)
        {
            _source = source;

            Owner = owner;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            BackColor = Theme.WindowBg;
            Opacity = 0.97;
            Bounds = owner.Bounds;
            DoubleBuffered = true;

            _caption = new Label
            {
                Text = "Writing NAND",
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = Theme.TextPrimary,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Height = 34,
            };

            _logo = new XboxFillProgressBar
            {
                IsFlashing = true,
                ShowPercentText = true,
                Minimum = source.Minimum,
                Maximum = source.Maximum,
                Value = source.Value,
                Style = source.Style,
            };

            _hint = new Label
            {
                Text = "Do not disconnect the flasher or close J-Runner.",
                Font = Theme.UiFont,
                ForeColor = Theme.TextSecondary,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Height = 22,
            };

            Controls.Add(_logo);
            Controls.Add(_caption);
            Controls.Add(_hint);

            LayoutChildren();
            Resize += (s, e) => LayoutChildren();

            _mirror = new Timer { Interval = 40 };
            _mirror.Tick += (s, e) => Mirror();
            _mirror.Start();
        }

        private void LayoutChildren()
        {
            int size = Math.Max(120, Math.Min(280, Math.Min(ClientSize.Width, ClientSize.Height) - 220));
            int cx = ClientSize.Width / 2;
            int cy = ClientSize.Height / 2;

            _logo.Bounds = new Rectangle(cx - size / 2, cy - size / 2, size, size);
            _caption.Bounds = new Rectangle(0, _logo.Top - 48, ClientSize.Width, _caption.Height);
            _hint.Bounds = new Rectangle(0, _logo.Bottom + 16, ClientSize.Width, _hint.Height);
        }

        private void Mirror()
        {
            if (_source == null || _source.IsDisposed) return;
            try
            {
                _logo.Style = _source.Style;
                _logo.Minimum = _source.Minimum;
                _logo.Maximum = _source.Maximum;
                int v = _source.Value;
                if (v < _logo.Minimum) v = _logo.Minimum;
                if (v > _logo.Maximum) v = _logo.Maximum;
                _logo.Value = v;
            }
            catch (Exception ex)
            {
                if (JRunner.variables.debugme) Console.WriteLine(ex.ToString());
            }
        }

        // Keep the overlay glued to the main window if it gets moved or resized mid-flash.
        public void Follow()
        {
            if (Owner != null && !Owner.IsDisposed) Bounds = Owner.Bounds;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _mirror?.Stop();
            _mirror?.Dispose();
            base.OnFormClosed(e);
        }

        // Never take focus from the main window - this is a status display, not a dialog.
        protected override bool ShowWithoutActivation => true;
    }
}
