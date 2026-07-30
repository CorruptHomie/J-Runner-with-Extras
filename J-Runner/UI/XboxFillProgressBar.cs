using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace UI
{
    // Drop-in replacement for the stock ProgressBar used for NAND flash progress. Inherits
    // from ProgressBar (rather than Control) specifically so every existing
    // progressBar.Value / .Minimum / .Maximum / .Style assignment throughout MainForm.cs -
    // and the couple of call sites that take a `ref ProgressBar` - keep compiling and
    // working unchanged. Only the painting changes.
    [System.ComponentModel.DesignerCategory("")]
    public class XboxFillProgressBar : ProgressBar
    {
        private readonly Timer _animTimer;
        private float _displayedFraction; // 0..1, eases toward the real Value
        private float _marqueePhase;      // 0..1, sweeps continuously in Marquee style
        private static Bitmap _logoCached;

        public bool ShowPercentText { get; set; } = true;

        // Set by MainForm right before/after an actual NAND flash (see ConfirmFlash's
        // call sites and writenand() in MainForm.cs) - "put the animation only when the
        // nand is flashing" means every other use of this same shared progress bar
        // (reading, building, etc.) still shows the fill, just snapped straight to value
        // instead of eased, and Marquee mode holds still instead of sweeping.
        public bool IsFlashing { get; set; } = false;

        public XboxFillProgressBar()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            _animTimer = new Timer { Interval = 30 };
            _animTimer.Tick += (s, e) => Tick();
            _animTimer.Start();
        }

        private static Bitmap Logo => _logoCached ?? (_logoCached = JRunner.Properties.Resources.xbox);

        private float TargetFraction
        {
            get
            {
                int range = Maximum - Minimum;
                if (range <= 0) return 0f;
                return Math.Max(0f, Math.Min(1f, (Value - Minimum) / (float)range));
            }
        }

        private void Tick()
        {
            bool changed = false;
            bool animate = IsFlashing && Theme.AnimationsEnabled;

            if (Style == ProgressBarStyle.Marquee)
            {
                if (animate)
                {
                    float step = 0.02f;
                    _marqueePhase += step;
                    if (_marqueePhase > 1f) _marqueePhase -= 1f;
                    changed = true;
                }
            }
            else if (!animate)
            {
                if (_displayedFraction != TargetFraction) { _displayedFraction = TargetFraction; changed = true; }
            }
            else
            {
                float target = TargetFraction;
                float diff = target - _displayedFraction;
                if (Math.Abs(diff) > 0.001f)
                {
                    // Ease toward the target rather than jumping - this is the "slowly
                    // filling up" behaviour that was asked for. A fixed fraction of the
                    // remaining distance per tick gives a natural ease-out.
                    _displayedFraction += diff * 0.12f;
                    changed = true;
                }
                else if (_displayedFraction != target)
                {
                    _displayedFraction = target;
                    changed = true;
                }
            }

            if (changed) Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs pevent) { /* fully custom-painted below */ }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            Rectangle bounds = ClientRectangle;
            using (SolidBrush bg = new SolidBrush(Theme.WindowBg))
                g.FillRectangle(bg, bounds);

            Bitmap logo = Logo;

            // This same control instance is the one on MainForm, which the designer sizes
            // as a thin 347x20 strip. A logo fill needs real height - at that size the
            // logo would come out a few pixels tall - so anything short renders as a
            // proper themed bar instead. The big logo fill is what FlashProgressOverlay
            // uses, where it's given a large square to draw into.
            if (logo == null || bounds.Height < 48)
            {
                DrawBarMode(g, bounds);
                return;
            }

            // Fit the logo into the control, centered, leaving room for the % label.
            int textReserve = ShowPercentText ? 18 : 0;
            int avail = Math.Min(bounds.Width, bounds.Height - textReserve);
            avail = Math.Max(avail, 4);
            Rectangle logoRect = new Rectangle(
                bounds.Left + (bounds.Width - avail) / 2,
                bounds.Top + (bounds.Height - textReserve - avail) / 2,
                avail, avail);

            // Base layer: the whole logo in black and white, so the unfilled shape always
            // reads clearly and color visibly "arrives" as the fill rises over it.
            DrawLogo(g, logo, logoRect, grayscale: true);

            if (Style == ProgressBarStyle.Marquee)
            {
                DrawMarqueeSweep(g, logo, logoRect);
            }
            else
            {
                // Filled layer: full color, clipped to the bottom fraction of the logo -
                // a liquid-fill effect rising as Value increases.
                int fillHeight = (int)Math.Round(logoRect.Height * _displayedFraction);
                if (fillHeight > 0)
                {
                    Rectangle clip = new Rectangle(logoRect.Left, logoRect.Bottom - fillHeight, logoRect.Width, fillHeight);
                    Region old = g.Clip;
                    using (Region r = new Region(clip))
                    {
                        g.Clip = r;
                        DrawLogo(g, logo, logoRect, grayscale: false);
                    }
                    g.Clip = old;
                }
            }

            if (ShowPercentText)
            {
                string text = Style == ProgressBarStyle.Marquee ? "Working..." : (int)Math.Round(TargetFraction * 100) + "%";
                using (SolidBrush tb = new SolidBrush(Theme.TextSecondary))
                using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center })
                {
                    g.DrawString(text, Theme.UiFont, tb, bounds.Left + bounds.Width / 2f, bounds.Bottom - textReserve + 1, sf);
                }
            }
        }

        private void DrawBarMode(Graphics g, Rectangle bounds)
        {
            if (bounds.Width <= 2 || bounds.Height <= 2) return;

            Rectangle track = new Rectangle(bounds.Left, bounds.Top, bounds.Width - 1, bounds.Height - 1);
            int radius = Math.Max(2, Math.Min(8, track.Height / 2));

            using (GraphicsPath path = MessageDialog.RoundedPath(track, radius))
            using (SolidBrush trackBrush = new SolidBrush(Color.FromArgb(18, 18, 20)))
            using (Pen border = new Pen(Theme.Border))
            {
                g.FillPath(trackBrush, path);

                float frac = Style == ProgressBarStyle.Marquee ? 1f : _displayedFraction;
                int fillWidth = (int)Math.Round(track.Width * frac);
                if (fillWidth > 2)
                {
                    Rectangle fill = new Rectangle(track.Left, track.Top, fillWidth, track.Height);
                    Region old = g.Clip;
                    using (Region r = new Region(fill))
                    using (SolidBrush fillBrush = new SolidBrush(Theme.Accent))
                    {
                        g.Clip = r;
                        g.FillPath(fillBrush, path);
                    }
                    g.Clip = old;
                }

                g.DrawPath(border, path);
            }

            if (ShowPercentText)
            {
                string text = Style == ProgressBarStyle.Marquee
                    ? "Working..."
                    : (int)Math.Round(TargetFraction * 100) + "%";
                using (SolidBrush tb = new SolidBrush(Theme.TextPrimary))
                using (StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                })
                {
                    g.DrawString(text, Theme.UiFont, tb, bounds, sf);
                }
            }
        }

        private void DrawMarqueeSweep(Graphics g, Bitmap logo, Rectangle logoRect)
        {
            // A color band sweeping top-to-bottom across the black-and-white logo while an
            // operation of unknown length is in progress, clipped to the logo's own shape
            // via a second, full-color draw.
            bool animate = IsFlashing && Theme.AnimationsEnabled;
            int bandHeight = Math.Max(6, logoRect.Height / 5);
            int travel = logoRect.Height + bandHeight;
            int y = logoRect.Top - bandHeight + (int)(travel * (animate ? _marqueePhase : 0.5f));

            Rectangle band = new Rectangle(logoRect.Left, y, logoRect.Width, bandHeight);
            Region old = g.Clip;
            using (Region r = new Region(logoRect))
            {
                r.Intersect(band);
                g.Clip = r;
                DrawLogo(g, logo, logoRect, grayscale: false);
            }
            g.Clip = old;
        }

        private static void DrawLogo(Graphics g, Bitmap logo, Rectangle dest, bool grayscale)
        {
            if (!grayscale)
            {
                g.DrawImage(logo, dest);
                return;
            }

            // Standard luminance-weighted grayscale matrix. Alpha (row/col 3) is left as
            // an identity mapping so the logo's own transparency is untouched - only color
            // is affected.
            ColorMatrix cm = new ColorMatrix(new float[][]
            {
                new float[] {0.30f, 0.30f, 0.30f, 0, 0},
                new float[] {0.59f, 0.59f, 0.59f, 0, 0},
                new float[] {0.11f, 0.11f, 0.11f, 0, 0},
                new float[] {0,     0,     0,     1, 0},
                new float[] {0,     0,     0,     0, 1},
            });
            using (ImageAttributes attr = new ImageAttributes())
            {
                attr.SetColorMatrix(cm, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                g.DrawImage(logo, dest, 0, 0, logo.Width, logo.Height, GraphicsUnit.Pixel, attr);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _animTimer?.Dispose();
            base.Dispose(disposing);
        }
    }
}
