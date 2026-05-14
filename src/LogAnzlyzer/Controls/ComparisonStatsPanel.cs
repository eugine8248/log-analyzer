using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using LogAnzlyzer.Stats;
using LogAnzlyzer.Theme;

namespace LogAnzlyzer.Controls
{
    // Side panel for the comparison view: one row per series with
    // color swatch + filename + key stats.
    public sealed class ComparisonStatsPanel : Panel
    {
        public sealed class Row
        {
            public string FilePath;
            public DelayStats Stats;
            public Color Color;
        }

        private List<Row> _rows = new List<Row>();

        public ComparisonStatsPanel()
        {
            DoubleBuffered = true;
            Width = 320;
            Dock = DockStyle.Right;
            ThemeManager.ThemeChanged += (s, e) => Invalidate();
        }

        public void Set(IList<Row> rows)
        {
            _rows = new List<Row>(rows);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            var t = ThemeManager.Current;
            using (var b = new SolidBrush(t.Panel)) g.FillRectangle(b, ClientRectangle);
            using (var p = new Pen(t.Border)) g.DrawLine(p, 0, 0, 0, Height);

            int y = 14;
            using (var f = Fonts.Tiny)
                TextRenderer.DrawText(g, "COMPARISON", f, new Point(14, y), t.TextMuted);
            y += 22;

            for (int i = 0; i < _rows.Count; i++) y = DrawRow(g, t, _rows[i], 14, y);
        }

        private int DrawRow(Graphics g, ThemeColors t, Row r, int x, int y)
        {
            int rowH = 92;
            using (var bg = new SolidBrush(t.PanelElev)) g.FillRectangle(bg, x, y, Width - x - 14, rowH);
            using (var pen = new Pen(t.Border)) g.DrawRectangle(pen, x, y, Width - x - 14 - 1, rowH - 1);

            // color swatch
            using (var b = new SolidBrush(r.Color)) g.FillRectangle(b, x + 10, y + 12, 14, 14);

            // filename
            string fname = Path.GetFileName(r.FilePath ?? "");
            using (var f = Fonts.Body)
                TextRenderer.DrawText(g, fname, f, new Rectangle(x + 32, y + 8, Width - x - 14 - 40, 22),
                    t.TextStrong, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

            // stats line — P1 prominent, others compact
            using (var fLabel = Fonts.Tiny)
            using (var fVal = Fonts.Body)
            {
                int rowY = y + 36;
                DrawStat(g, fLabel, fVal, t, "P1",     r.Stats?.P1     ?? 0, x + 12,  rowY, t.P1);
                DrawStat(g, fLabel, fVal, t, "Median", r.Stats?.Median ?? 0, x + 90,  rowY, t.Median);
                DrawStat(g, fLabel, fVal, t, "P95",    r.Stats?.P95    ?? 0, x + 168, rowY, t.Text);
                DrawStat(g, fLabel, fVal, t, "P99",    r.Stats?.P99    ?? 0, x + 246, rowY, t.Text);

                int rowY2 = y + 64;
                using (var fInfo = Fonts.Tiny)
                    TextRenderer.DrawText(g, $"{r.Stats?.Count ?? 0:N0} delays · range {r.Stats?.Min ?? 0:0}–{r.Stats?.Max ?? 0:0} ms",
                        fInfo, new Point(x + 12, rowY2), t.TextMuted);
            }

            return y + rowH + 8;
        }

        private static void DrawStat(Graphics g, Font fLabel, Font fVal, ThemeColors t, string label, double v, int x, int y, Color valueColor)
        {
            TextRenderer.DrawText(g, label.ToUpperInvariant(), fLabel, new Point(x, y), t.TextMuted);
            TextRenderer.DrawText(g, v > 0 ? v.ToString("0") : "—", fVal, new Point(x, y + 12), valueColor);
        }
    }
}
