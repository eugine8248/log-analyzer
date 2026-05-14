using System.Drawing;
using System.Windows.Forms;
using LogAnzlyzer.Theme;

namespace LogAnzlyzer.Controls
{
    // 22px-tall themed status bar. Two segment groups (left and right),
    // accent flag highlights one segment with a subtle white overlay.
    public sealed class ThemedStatusBar : Panel
    {
        public class Segment { public string Text; public bool Accent; public bool Mono; }

        private Segment[] _left = new Segment[0];
        private Segment[] _right = new Segment[0];

        public ThemedStatusBar()
        {
            DoubleBuffered = true;
            Height = 22;
            Dock = DockStyle.Bottom;
            ThemeManager.ThemeChanged += (s, e) => Invalidate();
        }

        public void Set(Segment[] left, Segment[] right)
        {
            _left = left ?? new Segment[0];
            _right = right ?? new Segment[0];
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            var t = ThemeManager.Current;

            using (var b = new SolidBrush(t.Statusbar)) g.FillRectangle(b, ClientRectangle);

            // Left segments
            int x = 0;
            foreach (var seg in _left) x = DrawSegment(g, t, seg, x, leftAlign: true);

            // Right segments — measure first to right-align as a group
            int rightWidth = 0;
            foreach (var seg in _right)
            {
                using (var f = seg.Mono ? Fonts.Mono : Fonts.Tiny)
                    rightWidth += TextRenderer.MeasureText(seg.Text, f).Width + 20;
            }
            int rx = Width - rightWidth;
            foreach (var seg in _right) rx = DrawSegment(g, t, seg, rx, leftAlign: true);
        }

        private int DrawSegment(Graphics g, ThemeColors t, Segment seg, int x, bool leftAlign)
        {
            using (var f = seg.Mono ? Fonts.Mono : Fonts.Tiny)
            {
                var sz = TextRenderer.MeasureText(seg.Text, f);
                int w = sz.Width + 20;
                if (seg.Accent)
                {
                    using (var br = new SolidBrush(Color.FromArgb(30, 255, 255, 255)))
                        g.FillRectangle(br, x, 0, w, Height);
                }
                TextRenderer.DrawText(g, seg.Text, f, new Rectangle(x + 10, 0, w, Height),
                    t.StatusbarFg, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
                return x + w;
            }
        }
    }
}
