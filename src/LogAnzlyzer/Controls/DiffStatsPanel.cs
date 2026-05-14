using System.Drawing;
using System.IO;
using System.Windows.Forms;
using LogAnzlyzer.Stats;
using LogAnzlyzer.Theme;

namespace LogAnzlyzer.Controls
{
    // Sidebar for the diff tab: shows baseline (A) vs current (B) with deltas.
    // Improvement (lower delay) renders teal; regression (higher delay) renders coral.
    public sealed class DiffStatsPanel : Panel
    {
        public string LabelA = "Baseline";
        public string LabelB = "Current";
        public string FileA  = "";
        public string FileB  = "";
        public Color  ColorA;
        public Color  ColorB;

        private DelayStats _a = DelayStats.Empty;
        private DelayStats _b = DelayStats.Empty;

        public DiffStatsPanel()
        {
            DoubleBuffered = true;
            Width = 360;
            Dock = DockStyle.Right;
            ThemeManager.ThemeChanged += (s, e) => Invalidate();
        }

        public void Set(DelayStats a, DelayStats b) { _a = a; _b = b; Invalidate(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            var t = ThemeManager.Current;
            using (var b = new SolidBrush(t.Panel)) g.FillRectangle(b, ClientRectangle);
            using (var p = new Pen(t.Border)) g.DrawLine(p, 0, 0, 0, Height);

            int y = 14;
            using (var f = Fonts.Tiny)
                TextRenderer.DrawText(g, "DIFF", f, new Point(14, y), t.TextMuted);
            y += 22;

            // Source rows
            y = DrawSource(g, t, LabelA, FileA, ColorA, 14, y);
            y = DrawSource(g, t, LabelB, FileB, ColorB, 14, y) + 6;

            // Divider
            using (var p = new Pen(t.BorderSoft)) g.DrawLine(p, 14, y, Width - 14, y);
            y += 12;

            // Header
            using (var f = Fonts.Tiny)
            {
                TextRenderer.DrawText(g, "METRIC",   f, new Point(14, y), t.TextMuted);
                TextRenderer.DrawText(g, LabelA.ToUpperInvariant(), f, new Point(140, y), t.TextMuted);
                TextRenderer.DrawText(g, LabelB.ToUpperInvariant(), f, new Point(220, y), t.TextMuted);
                TextRenderer.DrawText(g, "DELTA",    f, new Point(290, y), t.TextMuted);
            }
            y += 18;

            DrawRow(g, t, "P1",     _a.P1,     _b.P1,     y, lowerIsBetter: true,  highlight: t.P1); y += 26;
            DrawRow(g, t, "P99",    _a.P99,    _b.P99,    y, lowerIsBetter: true);                    y += 26;
            DrawRow(g, t, "P95",    _a.P95,    _b.P95,    y, lowerIsBetter: true);                    y += 26;
            DrawRow(g, t, "Median", _a.Median, _b.Median, y, lowerIsBetter: true);                    y += 26;
            DrawRow(g, t, "Mean",   _a.Mean,   _b.Mean,   y, lowerIsBetter: true);                    y += 26;
            DrawRow(g, t, "Max",    _a.Max,    _b.Max,    y, lowerIsBetter: true);                    y += 26;
            DrawRow(g, t, "Events", _a.Count,  _b.Count,  y, lowerIsBetter: false, integer: true);    y += 26;
        }

        private int DrawSource(Graphics g, ThemeColors t, string label, string file, Color color, int x, int y)
        {
            using (var b = new SolidBrush(color)) g.FillRectangle(b, x, y + 2, 12, 12);
            using (var f = Fonts.Tiny)
                TextRenderer.DrawText(g, label.ToUpperInvariant(), f, new Point(x + 20, y), t.TextMuted);
            using (var f = Fonts.Body)
                TextRenderer.DrawText(g, Path.GetFileName(file ?? ""), f,
                    new Rectangle(x + 20, y + 12, Width - x - 28, 18),
                    t.Text, TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
            return y + 36;
        }

        private void DrawRow(Graphics g, ThemeColors t, string metric, double a, double b, int y,
            bool lowerIsBetter, bool integer = false, Color? highlight = null)
        {
            using (var f = Fonts.Body)
            {
                TextRenderer.DrawText(g, metric, f, new Point(14, y), highlight ?? t.Text);
                TextRenderer.DrawText(g, FormatVal(a, integer), f, new Point(140, y), t.Text);
                TextRenderer.DrawText(g, FormatVal(b, integer), f, new Point(220, y), t.Text);

                double delta = b - a;
                bool improved = lowerIsBetter ? delta < 0 : delta > 0;
                Color deltaColor = (a == 0 && b == 0) ? t.TextMuted
                    : improved ? t.Median
                    : (delta == 0 ? t.TextMuted : t.P1);

                string deltaTxt;
                if (a == 0 && b == 0) deltaTxt = "—";
                else if (a == 0)      deltaTxt = "+∞%";
                else
                {
                    double pct = (delta / a) * 100.0;
                    string sign = delta >= 0 ? "+" : "";
                    deltaTxt = integer ? $"{sign}{(int)delta} ({sign}{pct:0.#}%)"
                                       : $"{sign}{delta:0} ms ({sign}{pct:0.#}%)";
                }
                TextRenderer.DrawText(g, deltaTxt, f, new Point(290, y), deltaColor);
            }
        }

        private static string FormatVal(double v, bool integer)
            => v <= 0 ? "—" : (integer ? v.ToString("N0") : v.ToString("0") + " ms");
    }
}
