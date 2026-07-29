using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace UI
{
    /// <summary>
    /// TabControl that paints everything itself.
    ///
    /// Owner-drawing via DrawMode = OwnerDrawFixed only hands over the interior of each tab
    /// button - WinForms still draws the tab strip's 3D edges and the frame around the page
    /// area using SystemColors.ControlLightLight/ControlDark. Against a dark theme those
    /// render as bright white outlines around every tab and every page, which is what the
    /// "whitespace around the boxes" is. There's no property to recolour them, so the whole
    /// control has to take over painting.
    /// </summary>
    [System.ComponentModel.DesignerCategory("")]
    public class DarkTabControl : TabControl
    {
        public DarkTabControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.OptimizedDoubleBuffer
                   | ControlStyles.ResizeRedraw
                   | ControlStyles.UserPaint, true);
            DrawMode = TabDrawMode.OwnerDrawFixed;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Everything is drawn in OnPaint; skipping the default keeps the system frame
            // from ever reaching the surface.
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.None;

            using (SolidBrush bg = new SolidBrush(Theme.PanelBg))
                g.FillRectangle(bg, ClientRectangle);

            // The page area gets a single subtle hairline instead of the 3D frame. The
            // TabPages themselves are real child windows and paint over the middle of this,
            // so only the outline shows.
            Rectangle page = ClientRectangle;
            if (TabCount > 0)
            {
                Rectangle first = GetTabRect(0);
                page = new Rectangle(0, first.Bottom, Width - 1, Height - first.Bottom - 1);
            }
            if (page.Width > 2 && page.Height > 2)
            {
                using (Pen edge = new Pen(Theme.BorderSubtle))
                    g.DrawRectangle(edge, page);
            }

            for (int i = 0; i < TabCount; i++) DrawTab(g, i);
        }

        private void DrawTab(Graphics g, int index)
        {
            Rectangle r;
            try { r = GetTabRect(index); }
            catch { return; }   // can throw mid-relayout

            bool selected = SelectedIndex == index;
            TabPage page = TabPages[index];

            using (SolidBrush fill = new SolidBrush(selected ? Theme.RaisedBg : Theme.PanelBg))
                g.FillRectangle(fill, r);

            if (selected)
            {
                using (SolidBrush accent = new SolidBrush(Theme.Accent))
                    g.FillRectangle(accent, r.Left, r.Bottom - 2, r.Width, 2);
            }
            else
            {
                using (Pen sep = new Pen(Theme.BorderSubtle))
                    g.DrawLine(sep, r.Right - 1, r.Top + 4, r.Right - 1, r.Bottom - 4);
            }

            TextRenderer.DrawText(g, page.Text, Font, r,
                selected ? Theme.TextPrimary : Theme.TextSecondary,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
            Invalidate();
        }
    }
}
