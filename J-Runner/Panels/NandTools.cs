using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Media;
using System.Threading;
using System.Windows.Forms;

namespace JRunner.Panels
{
    public partial class NandTools : UserControl
    {
        public NandTools()
        {
            InitializeComponent();
            SetupDeviceCard();
        }

        public NandTools(string lptport)
        {
            txtLPTPort.Text = lptport;
        }

        public NandTools(int iterations)
        {
            numericIterations.Value = iterations;
        }

        public NandTools(string lptport, int iterations)
        {
            numericIterations.Value = iterations;
            txtLPTPort.Text = lptport;
        }

        public int getNumericIterations()
        {
            return (int)numericIterations.Value;
        }
        public void setNumericIterations(decimal value)
        {
            numericIterations.Value = value;
        }
        public string getLptPort()
        {
            return txtLPTPort.Text;
        }
        public void setLptPort(string port)
        {
            txtLPTPort.Text = port;
        }
        public bool getRbtnUSB()
        {
            return rbtnUSB.Checked;
        }
        public bool getRbtnLPT()
        {
            return rbtnLPT.Checked;
        }

        public void setbtnCreateECCEnabled(bool b)
        {
            btnCreateECC.Enabled = b;
        }
        public void setbtnCreateECC(string text)
        {
            btnCreateECC.Text = text;
        }
        public string getbtnCreateECC()
        {
            return btnCreateECC.Text;
        }
        public void setbtnWriteECC(string text)
        {
            btnWriteECC.Text = text;
        }
        public string getbtnWriteECC()
        {
            return btnWriteECC.Text;
        }

        // The connected-flasher photos are dark boards shot on transparent/white, so on a
        // dark panel they were invisible. Rather than sitting them on a white rectangle -
        // which punched a hole straight through the colour palette - the picture box is
        // drawn as a soft grey card and the photo is composited onto it with a modest
        // brightness lift, so every device reads clearly and nothing leaves the palette.
        private Image _deviceImage;

        private void SetupDeviceCard()
        {
            pBoxDevice.BackColor = Color.Transparent;
            pBoxDevice.Paint += DeviceCard_Paint;
        }

        public void setImage(Image m)
        {
            _deviceImage = m;
            // Kept null so PictureBox doesn't draw the raw photo underneath the card.
            pBoxDevice.Image = null;
            pBoxDevice.Invalidate();
        }

        private void DeviceCard_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            Rectangle bounds = pBoxDevice.ClientRectangle;
            if (bounds.Width < 8 || bounds.Height < 8) return;

            using (SolidBrush parent = new SolidBrush(UI.Theme.PanelBg))
                g.FillRectangle(parent, bounds);

            Rectangle card = new Rectangle(bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
            using (GraphicsPath path = UI.MessageDialog.RoundedPath(card, 8))
            using (LinearGradientBrush fill = new LinearGradientBrush(
                       card, Color.FromArgb(104, 107, 116), Color.FromArgb(66, 68, 76), 90f))
            using (Pen edge = new Pen(UI.Theme.Border))
            {
                g.FillPath(fill, path);
                g.DrawPath(edge, path);
            }

            if (_deviceImage == null)
            {
                TextRenderer.DrawText(g, "No flasher detected", UI.Theme.UiFont, bounds,
                    Color.FromArgb(196, 199, 206),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }

            // Fit (never upscale past the card) and centre.
            Rectangle inner = Rectangle.Inflate(card, -10, -10);
            float scale = Math.Min((float)inner.Width / _deviceImage.Width,
                                   (float)inner.Height / _deviceImage.Height);
            int w = Math.Max(1, (int)(_deviceImage.Width * scale));
            int h = Math.Max(1, (int)(_deviceImage.Height * scale));
            Rectangle dest = new Rectangle(inner.X + (inner.Width - w) / 2,
                                           inner.Y + (inner.Height - h) / 2, w, h);

            // Slight lift so very dark boards separate from the card without washing out
            // the lighter device photos.
            ColorMatrix cm = new ColorMatrix(new float[][]
            {
                new float[] {1.12f, 0, 0, 0, 0},
                new float[] {0, 1.12f, 0, 0, 0},
                new float[] {0, 0, 1.12f, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new float[] {0.04f, 0.04f, 0.04f, 0, 1},
            });
            using (ImageAttributes attr = new ImageAttributes())
            {
                attr.SetColorMatrix(cm, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                g.DrawImage(_deviceImage, dest, 0, 0, _deviceImage.Width, _deviceImage.Height,
                            GraphicsUnit.Pixel, attr);
            }
        }

        public delegate void ClickedRead();
        public event ClickedRead ReadClick;
        public delegate void ClickedCreateECC();
        public event ClickedCreateECC CreateEccClick;
        public delegate void ClickedWriteECC();
        public event ClickedWriteECC WriteEccClick;
        public delegate void ClickedXeBuild();
        public event ClickedXeBuild XeBuildClick;
        public delegate void ClickedWrite();
        public event ClickedWrite WriteClick;
        public delegate void ClickedProgramCR();
        public event ClickedProgramCR ProgramCRClick;
        public delegate void ClickedCPUDB();
        public event ClickedCPUDB CPUDBClick;
        public delegate void ChangedIter(int iter);
        public event ChangedIter IterChange;
        public delegate void ClickedCreateDonor();
        public event ClickedCreateDonor CreateDonorClick;
        public delegate void ClickedExtractFiles();
        public event ClickedExtractFiles ExtractFilesClick;
        public delegate void ClickedPatchKv();
        public event ClickedPatchKv PatchKvClick;
        //public delegate void CheckedChanged();
        //public event CheckedChanged ChangedChecked;
        //public delegate void PortChanged();
        //public event PortChanged ChangedPort;

        private void btnRead_Click(object sender, EventArgs e)
        {
            ReadClick();
        }

        private void btnCreateECC_Click(object sender, EventArgs e)
        {
            CreateEccClick();
        }

        private void btnWriteECC_Click(object sender, EventArgs e)
        {
            WriteEccClick();
        }

        private void btnXeBuild_Click(object sender, EventArgs e)
        {
            XeBuildClick();
        }

        private void btnWrite_Click(object sender, EventArgs e)
        {
            WriteClick();
        }

        private void btnProgramCR_Click(object sender, EventArgs e)
        {
            ProgramCRClick();
        }

        private void rbtn_CheckedChanged(object sender, EventArgs e)
        {
            //ChangedChecked();
            txtLPTPort.Visible = (rbtnLPT.Checked);
            lblLPTPort.Visible = txtLPTPort.Visible;
        }

        private void numericIterations_ValueChanged(object sender, EventArgs e)
        {
            IterChange((int)numericIterations.Value);
        }

        private void btnCPUDB_Click(object sender, EventArgs e)
        {
            CPUDBClick();
        }

        private void btnCreateDonor_Click(object sender, EventArgs e)
        {
            CreateDonorClick();
        }

        private void btnExtractFiles_Click(object sender, EventArgs e)
        {
            ExtractFilesClick();
        }

        private void btnPatchKv_Click(object sender, EventArgs e)
        {
            PatchKvClick();
        }

        private void txtLPTPort_TextChanged(object sender, EventArgs e)
        {
            //ChangedPort();
        }

        private int eeCount = 0;
        private void pBoxDevice_Click(object sender, EventArgs e)
        {
            if (eeCount == 5)
            {
                UI.Msg.Show("Wtf are you doing!?!?!", "Confusion!", MessageBoxButtons.OK, MessageBoxIcon.Question);
            }
            else if (eeCount == 8)
            {
                UI.Msg.Show("#%&@ Stop doing that!!!!!", "#%&@", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (eeCount == 10)
            {
                UI.Msg.Show("Cut that shit out!!!!!", "You're Annoying!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (eeCount == 12)
            {
                UI.Msg.Show("CLICK ME AGAIN!\nI DARE YOU!", "You Gon Get It", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (eeCount == 13)
            {
                SoundPlayer goodbye = new SoundPlayer(Properties.Resources.goodbye);
                goodbye.Play();
                Thread.Sleep(1000);
                Application.Exit();
            }
            eeCount += 1;
        }
    }
}
