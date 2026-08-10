using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace UI
{
    public enum DialogKind { YesNo, Ok }

    /// <summary>
    /// Small dark, rounded, borderless modal used for the two dialogs the user specifically
    /// asked for (flash confirmation, flash-complete reminder) and reusable anywhere else in
    /// the app that wants a themed UI.Msg.Show() replacement.
    /// </summary>
    [System.ComponentModel.DesignerCategory("")]
    public class MessageDialog : Form
    {
        public bool Result { get; private set; }

        private readonly Label _messageLabel;
        private Form _overlay;

        public MessageDialog(string title, string message, DialogKind kind)
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            BackColor = Theme.PanelBg;
            Size = new Size(400, 190);
            DoubleBuffered = true;

            Label titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Theme.TextPrimary,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(20, 18),
                Size = new Size(360, 26)
            };

            _messageLabel = new Label
            {
                Text = message,
                Font = Theme.UiFont,
                ForeColor = Theme.TextSecondary,
                AutoSize = false,
                Location = new Point(20, 52),
                Size = new Size(360, 70)
            };

            Controls.Add(titleLabel);
            Controls.Add(_messageLabel);

            if (kind == DialogKind.YesNo)
            {
                Button yes = MakeButton("Yes", true);
                Button no = MakeButton("No", false);
                yes.Location = new Point(Width - 2 * 100 - 20 - 10, 130);
                no.Location = new Point(Width - 100 - 20, 130);
                yes.Click += (s, e) => { Result = true; Close(); };
                no.Click += (s, e) => { Result = false; Close(); };
                Controls.Add(yes);
                Controls.Add(no);
                AcceptButton = null;
                CancelButton = null;
            }
            else
            {
                Button ok = MakeButton("Ok", true);
                ok.Location = new Point(Width - 100 - 20, 130);
                ok.Click += (s, e) => { Result = true; Close(); };
                Controls.Add(ok);
                AcceptButton = ok;
            }

            titleLabel.MouseDown += Drag_MouseDown;
            _messageLabel.MouseDown += Drag_MouseDown;
        }

        private Button MakeButton(string text, bool primary)
        {
            Button b = new Button
            {
                Text = text,
                Size = new Size(90, 32),
                FlatStyle = FlatStyle.Flat,
                Font = Theme.UiFontBold,
                BackColor = primary ? Theme.Accent : Theme.RaisedBg,
                ForeColor = primary ? Color.FromArgb(20, 20, 20) : Theme.TextPrimary,
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderColor = primary ? Theme.Accent : Theme.Border;
            b.FlatAppearance.MouseOverBackColor = primary ? ControlPaint.Light(Theme.Accent) : Theme.HoverBg;
            b.FlatAppearance.MouseDownBackColor = primary ? Theme.AccentDim : Theme.PressedBg;
            return b;
        }

        // Small custom dialogs like this one have no title bar to drag from, so let a
        // mouse-down on the title/message text drag the whole window instead.
        private void Drag_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            NativeDrag.ReleaseAndDrag(this);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Region = RoundedRegion(ClientRectangle, 12);

            if (Owner != null)
            {
                _overlay = new Form
                {
                    FormBorderStyle = FormBorderStyle.None,
                    StartPosition = FormStartPosition.Manual,
                    ShowInTaskbar = false,
                    BackColor = Color.Black,
                    Opacity = 0.45,
                    Bounds = Owner.Bounds
                };
                _overlay.Show(Owner);
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            if (_overlay != null) { _overlay.Close(); _overlay.Dispose(); _overlay = null; }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (Pen p = new Pen(Theme.Border, 1))
                e.Graphics.DrawPath(p, RoundedPath(new Rectangle(0, 0, Width - 1, Height - 1), 12));
        }

        internal static GraphicsPath RoundedPath(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        /// <summary>
        /// Rounded path for a 1px <b>stroke</b>, shifted half a pixel.
        ///
        /// A 1px pen centred on integer coordinate x covers x-0.5 to x+0.5 - half of each
        /// adjacent pixel - so an antialiased straight edge renders at ~50% strength while
        /// the curves, which cover their pixels more fully, come out solid. That is exactly
        /// the "corners visible, straight edges missing" look, and it is measurable: a border
        /// drawn this way on the panel background lands on (46,46,52), which is precisely the
        /// 50% blend of Border (62,62,69) and PanelBg (30,30,34).
        ///
        /// PixelOffsetMode.Half does not fix it - that shifts sampling for fills, not the
        /// centre line of a stroke. Offsetting the geometry does: the stroke then spans x to
        /// x+1 and fills one whole pixel.
        /// </summary>
        internal static GraphicsPath RoundedPathStroke(Rectangle bounds, int radius)
        {
            float d = radius * 2f;
            float x = bounds.X + 0.5f, y = bounds.Y + 0.5f;
            float r = bounds.Right - 0.5f, b = bounds.Bottom - 0.5f;

            GraphicsPath path = new GraphicsPath();
            if (d <= 0 || bounds.Width <= d || bounds.Height <= d)
            {
                path.AddRectangle(new RectangleF(x, y, r - x, b - y));
                return path;
            }
            path.AddArc(x, y, d, d, 180, 90);
            path.AddArc(r - d, y, d, d, 270, 90);
            path.AddArc(r - d, b - d, d, d, 0, 90);
            path.AddArc(x, b - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        internal static Region RoundedRegion(Rectangle bounds, int radius)
        {
            using (GraphicsPath p = RoundedPath(bounds, radius)) return new Region(p);
        }
    }

    // WM_NCLBUTTONDOWN-based window drag, shared by MessageDialog and MainForm's title bar.
    internal static class NativeDrag
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;

        public static void ReleaseAndDrag(Form f)
        {
            ReleaseCapture();
            SendMessage(f.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
        }
    }

    public static class ThemedDialogs
    {
        /// <summary>
        /// "Are you sure? Did you make sure to select the correct options and patches?"
        /// Shown before any NAND flasher write begins. Returns true only for "Yes".
        /// </summary>
        public static bool ConfirmFlash(IWin32Window owner)
        {
            using (MessageDialog d = new MessageDialog(
                "Confirm Flash",
                "Are you sure? Did you make sure to select the correct options and patches?",
                DialogKind.YesNo))
            {
                if (owner is Form f) d.Owner = f;
                d.ShowDialog(owner);
                return d.Result;
            }
        }

        /// <summary>
        /// "Remember to disconnect the flasher from the computer before booting!"
        /// Shown once a NAND flasher write completes successfully.
        /// </summary>
        public static void ShowFlashComplete(IWin32Window owner)
        {
            using (MessageDialog d = new MessageDialog(
                "Flash Complete",
                "Remember to disconnect the flasher from the computer before booting!",
                DialogKind.Ok))
            {
                if (owner is Form f) d.Owner = f;
                d.ShowDialog(owner);
            }
        }
    }
}
