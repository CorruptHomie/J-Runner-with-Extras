using System;
using System.Drawing;
using System.Windows.Forms;

namespace JRunner.Forms
{
    // Standalone, non-modal so the main console log stays usable while this waits for a
    // Pico to show up in BOOTSEL mode. Built in code rather than a Designer.cs pair - it's
    // a small, purpose-built dialog with no need for a separate partial class.
    [System.ComponentModel.DesignerCategory("")]
    public class GlitchChipProgrammer : Form
    {
        private readonly RadioButton _rbRPicoRGH;
        private readonly RadioButton _rbPicoRGH;
        private readonly Label _status;
        private readonly Button _btnProgram;
        private readonly RPicoRGH _rpicorgh;

        public GlitchChipProgrammer(RPicoRGH rpicorgh)
        {
            _rpicorgh = rpicorgh;

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            Size = new Size(420, 300);
            BackColor = UI.Theme.PanelBg;
            DoubleBuffered = true;

            Panel titleBar = new Panel { Dock = DockStyle.Top, Height = 36, BackColor = UI.Theme.WindowBg };
            Label title = new Label
            {
                Text = "Program Glitch Chip",
                ForeColor = UI.Theme.TextPrimary,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(14, 0),
                Size = new Size(300, 36)
            };
            Label close = new Label
            {
                Text = "\u2715",
                ForeColor = UI.Theme.ChromeButton,
                Font = new Font("Segoe UI", 10F),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(384, 0),
                Size = new Size(36, 36),
                Cursor = Cursors.Hand
            };
            close.Click += (s, e) => Close();
            close.MouseEnter += (s, e) => close.ForeColor = UI.Theme.ChromeButtonHover;
            close.MouseLeave += (s, e) => close.ForeColor = UI.Theme.ChromeButton;
            titleBar.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) UI.NativeDrag.ReleaseAndDrag(this); };
            title.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) UI.NativeDrag.ReleaseAndDrag(this); };
            titleBar.Controls.Add(title);
            titleBar.Controls.Add(close);

            Label info = new Label
            {
                Text = "Flashes a Raspberry Pi Pico set to BOOTSEL mode with RGH 1.2 glitcher firmware. "
                     + "Hold the BOOTSEL button while plugging the Pico into this PC, then click Program.",
                ForeColor = UI.Theme.TextSecondary,
                Font = UI.Theme.UiFont,
                Location = new Point(20, 50),
                Size = new Size(380, 54)
            };

            _rbRPicoRGH = new RadioButton
            {
                Text = "RPicoRGH (recommended)",
                ForeColor = UI.Theme.TextPrimary,
                Font = UI.Theme.UiFont,
                Location = new Point(24, 112),
                Size = new Size(300, 22),
                Checked = true
            };
            _rbPicoRGH = new RadioButton
            {
                Text = "PicoRGH (original/legacy)",
                ForeColor = UI.Theme.TextPrimary,
                Font = UI.Theme.UiFont,
                Location = new Point(24, 138),
                Size = new Size(300, 22)
            };

            _status = new Label
            {
                Text = "",
                ForeColor = UI.Theme.TextSecondary,
                Font = UI.Theme.UiFont,
                Location = new Point(20, 178),
                Size = new Size(380, 44)
            };

            _btnProgram = new Button
            {
                Text = "Program",
                Size = new Size(110, 34),
                Location = new Point(20, 228),
                FlatStyle = FlatStyle.Flat,
                Font = UI.Theme.UiFontBold,
                BackColor = UI.Theme.Accent,
                ForeColor = Color.FromArgb(20, 20, 20),
                Cursor = Cursors.Hand
            };
            _btnProgram.FlatAppearance.BorderColor = UI.Theme.Accent;
            _btnProgram.FlatAppearance.MouseOverBackColor = ControlPaint.Light(UI.Theme.Accent);
            _btnProgram.Click += BtnProgram_Click;

            Controls.Add(titleBar);
            Controls.Add(info);
            Controls.Add(_rbRPicoRGH);
            Controls.Add(_rbPicoRGH);
            Controls.Add(_status);
            Controls.Add(_btnProgram);

            _rpicorgh.Completed += RpicoRgh_Completed;
            FormClosed += (s, e) => _rpicorgh.Completed -= RpicoRgh_Completed;
        }

        private void BtnProgram_Click(object sender, EventArgs e)
        {
            if (_rpicorgh.inUse) return;
            _btnProgram.Enabled = false;
            _status.ForeColor = UI.Theme.TextSecondary;
            _status.Text = "Looking for a Pico in BOOTSEL mode...";
            RPicoRGH.Variant variant = _rbRPicoRGH.Checked ? RPicoRGH.Variant.RPicoRGH : RPicoRGH.Variant.PicoRGH;
            _rpicorgh.Program(variant);
        }

        private void RpicoRgh_Completed(bool success, string message)
        {
            if (IsDisposed) return;
            if (InvokeRequired) { BeginInvoke(new Action<bool, string>(RpicoRgh_Completed), success, message); return; }
            _status.ForeColor = success ? UI.Theme.Accent : UI.Theme.Danger;
            _status.Text = message;
            _btnProgram.Enabled = true;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Region = UI.MessageDialog.RoundedRegion(ClientRectangle, 10);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (Pen p = new Pen(UI.Theme.Border, 1))
                e.Graphics.DrawPath(p, UI.MessageDialog.RoundedPath(new Rectangle(0, 0, Width - 1, Height - 1), 10));
        }
    }
}
