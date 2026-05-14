using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using LogAnzlyzer.Theme;

namespace LogAnzlyzer.Controls
{
    // Custom-drawn TabControl that mirrors the design's VS-Code-style strip:
    // 34px tall, 1.5px accent top-border on active tab, close X per tab,
    // file icon, and a "+" button at the end.
    public sealed class ClosableTabControl : TabControl
    {
        public event EventHandler<int> TabCloseRequested;
        public event EventHandler NewTabRequested;

        private const int TabHeight = 34;
        private const int CloseBoxSize = 18;
        private const int PlusButtonWidth = 32;

        public ClosableTabControl()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            DrawMode = TabDrawMode.OwnerDrawFixed;
            SizeMode = TabSizeMode.Fixed;
            ItemSize = new Size(220, TabHeight);
            Padding = new Point(0, 0);
            Appearance = TabAppearance.Normal;
            ThemeManager.ThemeChanged += (s, e) => Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            var t = ThemeManager.Current;
            // background of strip
            using (var b = new SolidBrush(t.Tabstrip))
                g.FillRectangle(b, new Rectangle(0, 0, Width, TabHeight));
            // bottom border under strip
            using (var p = new Pen(t.Border))
                g.DrawLine(p, 0, TabHeight, Width, TabHeight);

            for (int i = 0; i < TabPages.Count; i++)
            {
                var rect = GetTabRect(i);
                rect.Height = TabHeight;
                bool active = (SelectedIndex == i);
                DrawTab(g, t, TabPages[i].Text, rect, active);
            }

            // "+" button at end
            int plusX = TabPages.Count * ItemSize.Width;
            var plusRect = new Rectangle(plusX, 0, PlusButtonWidth, TabHeight);
            DrawPlus(g, t, plusRect);
        }

        private void DrawTab(Graphics g, ThemeColors t, string label, Rectangle rect, bool active)
        {
            // background
            using (var b = new SolidBrush(active ? t.TabActive : Color.Transparent))
                g.FillRectangle(b, rect);

            // active accent top border (1.5px)
            if (active)
            {
                using (var p = new Pen(t.Accent, 1.5f))
                    g.DrawLine(p, rect.Left, rect.Top + 1, rect.Right, rect.Top + 1);
            }

            // right divider
            using (var p = new Pen(t.Border))
                g.DrawLine(p, rect.Right - 1, rect.Top + 4, rect.Right - 1, rect.Bottom - 4);

            // file icon (small box+fold)
            int iconX = rect.Left + 12;
            int iconY = rect.Top + (rect.Height - 13) / 2;
            using (var pen = new Pen(active ? t.Accent : t.TextMuted, 1f))
            {
                g.DrawRectangle(pen, iconX, iconY, 9, 11);
                g.DrawLine(pen, iconX + 6, iconY, iconX + 9, iconY + 3);
            }

            // label
            using (var f = Fonts.Small)
            using (var br = new SolidBrush(active ? t.TextStrong : t.TextMuted))
            {
                var labelRect = new Rectangle(iconX + 18, rect.Top, rect.Width - 60, rect.Height);
                TextRenderer.DrawText(g, label, f, labelRect, active ? t.TextStrong : t.TextMuted,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }

            // close X
            int xCx = rect.Right - CloseBoxSize - 6;
            int xCy = rect.Top + (rect.Height - CloseBoxSize) / 2;
            using (var pen = new Pen(active ? t.Text : t.TextFaint, 1.4f) { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round })
            {
                g.DrawLine(pen, xCx + 5, xCy + 5, xCx + 13, xCy + 13);
                g.DrawLine(pen, xCx + 13, xCy + 5, xCx + 5, xCy + 13);
            }
        }

        private void DrawPlus(Graphics g, ThemeColors t, Rectangle rect)
        {
            int cx = rect.Left + rect.Width / 2;
            int cy = rect.Top + rect.Height / 2;
            using (var pen = new Pen(t.TextMuted, 1.4f))
            {
                g.DrawLine(pen, cx, cy - 4, cx, cy + 4);
                g.DrawLine(pen, cx - 4, cy, cx + 4, cy);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            // Hit-test close X
            for (int i = 0; i < TabPages.Count; i++)
            {
                var rect = GetTabRect(i);
                int xCx = rect.Right - CloseBoxSize - 6;
                int xCy = rect.Top + (rect.Height - CloseBoxSize) / 2;
                var closeBox = new Rectangle(xCx, xCy, CloseBoxSize, CloseBoxSize);
                if (closeBox.Contains(e.Location))
                {
                    TabCloseRequested?.Invoke(this, i);
                    return;
                }
            }
            // Hit-test "+" button
            int plusX = TabPages.Count * ItemSize.Width;
            var plusRect = new Rectangle(plusX, 0, PlusButtonWidth, TabHeight);
            if (plusRect.Contains(e.Location))
            {
                NewTabRequested?.Invoke(this, EventArgs.Empty);
                return;
            }
            base.OnMouseDown(e);
        }
    }
}
