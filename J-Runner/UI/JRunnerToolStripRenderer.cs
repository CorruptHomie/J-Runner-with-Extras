using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace UI
{
    // One renderer, assigned once via ToolStripManager.Renderer in Classes/Program.cs,
    // restyles every MenuStrip, ContextMenuStrip and dropdown in the app - the top menu
    // (moved into MainForm's new title bar strip) and every right-click menu like
    // getCpuKeyContextMenu/showWorkingFolderMenu alike. Keeping this global means none of
    // the ~150 existing ToolStripMenuItem declarations in MainForm.Designer.cs had to
    // change to pick up the new look.
    public class JRunnerToolStripRenderer : ToolStripProfessionalRenderer
    {
        public JRunnerToolStripRenderer() : base(new JRunnerColorTable()) { }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            bool isTopMenu = e.ToolStrip is MenuStrip;
            using (SolidBrush b = new SolidBrush(isTopMenu ? Theme.WindowBg : Theme.PanelBg))
                e.Graphics.FillRectangle(b, e.AffectedBounds);
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            // A hairline only on dropdowns/context menus - the top menu sits flush inside
            // the title bar strip and doesn't want its own border.
            if (e.ToolStrip is MenuStrip) return;
            using (Pen p = new Pen(Theme.Border))
                e.Graphics.DrawRectangle(p, 0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            ToolStripItem item = e.Item;
            Rectangle bounds = new Rectangle(Point.Empty, item.Size);
            bool onTopMenu = item.IsOnDropDown == false && item.Owner is MenuStrip;
            bool selected = item.Selected || item.Pressed;

            if (!selected)
            {
                if (!onTopMenu) return; // dropdown rows: no fill when idle, just PanelBg showing through
                return; // idle top-level tab: no fill either, text color alone signals it
            }

            Color fill = item.Pressed ? Theme.PressedBg : Theme.HoverBg;

            using (GraphicsPath path = RoundedRect(bounds, onTopMenu ? 10 : 4))
            using (SolidBrush b = new SolidBrush(fill))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillPath(b, path);
            }

            if (onTopMenu && (item.Selected || item.Pressed))
            {
                // Thin accent underline - the "which tab is active" cue from context.png.
                using (Pen accent = new Pen(Theme.Accent, 2))
                {
                    e.Graphics.DrawLine(accent, bounds.Left + 8, bounds.Bottom - 2, bounds.Right - 8, bounds.Bottom - 2);
                }
            }
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = e.Item.Enabled ? Theme.TextPrimary : Theme.TextSecondary;
            base.OnRenderItemText(e);
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            using (Pen p = new Pen(Theme.Border))
            {
                int y = e.Item.Height / 2;
                e.Graphics.DrawLine(p, 4, y, e.Item.Width - 4, y);
            }
        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            e.ArrowColor = Theme.TextPrimary;
            base.OnRenderArrow(e);
        }

        protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
        {
            using (SolidBrush b = new SolidBrush(Theme.PanelBg))
                e.Graphics.FillRectangle(b, e.AffectedBounds);
        }

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
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
    }

    public class JRunnerColorTable : ProfessionalColorTable
    {
        public override Color MenuItemSelected => Theme.HoverBg;
        public override Color MenuItemSelectedGradientBegin => Theme.HoverBg;
        public override Color MenuItemSelectedGradientEnd => Theme.HoverBg;
        public override Color MenuItemPressedGradientBegin => Theme.PressedBg;
        public override Color MenuItemPressedGradientEnd => Theme.PressedBg;
        public override Color MenuItemBorder => Color.Transparent;
        public override Color MenuBorder => Theme.Border;
        public override Color ToolStripDropDownBackground => Theme.PanelBg;
        public override Color ImageMarginGradientBegin => Theme.PanelBg;
        public override Color ImageMarginGradientMiddle => Theme.PanelBg;
        public override Color ImageMarginGradientEnd => Theme.PanelBg;
        public override Color SeparatorDark => Theme.Border;
        public override Color SeparatorLight => Theme.Border;
        public override Color MenuStripGradientBegin => Theme.WindowBg;
        public override Color MenuStripGradientEnd => Theme.WindowBg;
    }
}
