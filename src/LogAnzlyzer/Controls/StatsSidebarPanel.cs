using System;
using System.Drawing;
using System.Windows.Forms;
using LogAnzlyzer.Stats;
using LogAnzlyzer.Theme;

namespace LogAnzlyzer.Controls
{
    // Right-side stats sidebar: 240px wide. Hero P1 card on top, 4 small stat
    // cards in a 2×2 grid, time span, mini histogram.
    public sealed class StatsSidebarPanel : Panel
    {
        private DelayStats _stats = DelayStats.Empty;
        private int[] _histogram = new int[28];
        private DateTime _spanStart, _spanEnd;
        private string _spanText = "";

        public StatsSidebarPanel()
        {
            DoubleBuffered = true;
            Width = 240;
            Dock = DockStyle.Right;
            ThemeManager.ThemeChanged += (s, e) => Invalidate();
        }

        public void Update(DelayStats stats, int[] histogram, DateTime start, DateTime end)
        {
            _stats = stats;
            _histogram = histogram ?? new int[28];
            _spanStart = start;
            _spanEnd = end;
            _spanText = FormatSpan(end - start);
            Invalidate();
        }

        private static string FormatSpan(TimeSpan d)
        {
            if (d.TotalDays >= 1) return $"{(int)d.TotalDays}d {d.Hours}h {d.Minutes}m";
            if (d.TotalHours >= 1) return $"{(int)d.TotalHours}h {d.Minutes}m {d.Seconds}s";
            if (d.TotalMinutes >= 1) return $"{(int)d.TotalMinutes}m {d.Seconds}s";
            return $"{d.Seconds}.{d.Milliseconds:D3}s";
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            var t = ThemeManager.Current;
            using (var b = new SolidBrush(t.Panel)) g.FillRectangle(b, ClientRectangle);
            using (var p = new Pen(t.Border)) g.DrawLine(p, 0, 0, 0, Height);

            int y = 14;
            DrawSectionLabel(g, t, "Stats", 14, ref y);

            // Hero P1 card
            var heroRect = new Rectangle(14, y, Width - 28, 110);
            DrawHeroCard(g, t, heroRect, _stats.P1);
            y = heroRect.Bottom + 10;

            // 2×2 grid of small stat cards
            int colW = (Width - 28 - 6) / 2;
            int rowH = 60;
            DrawSmallCard(g, t, new Rectangle(14, y, colW, rowH), "Median", _stats.Median, t.Median);
            DrawSmallCard(g, t, new Rectangle(14 + colW + 6, y, colW, rowH), "P95", _stats.P95, t.Text);
            y += rowH + 6;
            DrawSmallCard(g, t, new Rectangle(14, y, colW, rowH), "P99", _stats.P99, t.Text);
            DrawSmallCard(g, t, new Rectangle(14 + colW + 6, y, colW, rowH), "Events", _stats.Count, t.Text, true);
            y += rowH + 12;

            // Span — show date when multi-day, else just time
            DrawDivider(g, t, ref y);
            DrawSectionLabel(g, t, "Span", 14, ref y);
            using (var f = Fonts.Mono)
            {
                bool multiDay = _spanStart.Date != _spanEnd.Date;
                string startFmt = multiDay ? _spanStart.ToString("MM-dd HH:mm:ss.fff") : _spanStart.ToString("HH:mm:ss.fff");
                string endFmt   = multiDay ? _spanEnd.ToString("MM-dd HH:mm:ss.fff")   : _spanEnd.ToString("HH:mm:ss.fff");
                TextRenderer.DrawText(g, "start  " + startFmt, f, new Point(14, y), t.Text);
                y += 18;
                TextRenderer.DrawText(g, "end    " + endFmt, f, new Point(14, y), t.Text);
                y += 18;
                TextRenderer.DrawText(g, _spanText, f, new Point(14, y), t.TextMuted);
                y += 22;
            }

            // Distribution
            DrawDivider(g, t, ref y);
            DrawSectionLabel(g, t, "Distribution", 14, ref y);
            DrawHistogram(g, t, new Rectangle(14, y, Width - 28, 64));
            y += 64 + 6;
            using (var f = Fonts.Tiny)
            {
                int axisY = y;
                TextRenderer.DrawText(g, "0", f, new Point(14, axisY), t.TextMuted);
                TextRenderer.DrawText(g, "50", f, new Point(14 + (Width - 28) / 4, axisY), t.TextMuted);
                TextRenderer.DrawText(g, "100", f, new Point(14 + (Width - 28) / 2, axisY), t.TextMuted);
                TextRenderer.DrawText(g, "200+", f, new Point(Width - 36, axisY), t.TextMuted);
            }
        }

        private static void DrawSectionLabel(Graphics g, ThemeColors t, string text, int x, ref int y)
        {
            using (var f = Fonts.Tiny)
            {
                TextRenderer.DrawText(g, text.ToUpperInvariant(), f, new Point(x, y), t.TextMuted);
            }
            y += 18;
        }

        private void DrawDivider(Graphics g, ThemeColors t, ref int y)
        {
            using (var p = new Pen(t.BorderSoft)) g.DrawLine(p, 14, y, Width - 14, y);
            y += 8;
        }

        private void DrawHeroCard(Graphics g, ThemeColors t, Rectangle r, double p1)
        {
            using (var bg = new SolidBrush(t.PanelElev)) g.FillRectangle(bg, r);
            using (var p = new Pen(t.Border)) g.DrawRectangle(p, r);

            using (var f = Fonts.Tiny)
                TextRenderer.DrawText(g, "P1 — LOW 1% TAIL", f, new Point(r.Left + 12, r.Top + 12), t.TextMuted);

            // Value + unit on same baseline. We measure both fonts to compute baselines.
            string val = p1 > 0 ? p1.ToString("N0") : "—";
            int valueY = r.Top + 30;
            using (var fVal = Fonts.StatHero)
            using (var fUnit = Fonts.Body)
            {
                Size valSz = TextRenderer.MeasureText(g, val, fVal, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding);
                Size unitSz = TextRenderer.MeasureText(g, "ms", fUnit, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding);

                TextRenderer.DrawText(g, val, fVal, new Point(r.Left + 12, valueY), t.P1, TextFormatFlags.NoPadding);
                int unitY = valueY + valSz.Height - unitSz.Height - 6; // align baselines
                TextRenderer.DrawText(g, "ms", fUnit, new Point(r.Left + 12 + valSz.Width + 6, unitY), t.TextMuted, TextFormatFlags.NoPadding);
            }

            // Description — wraps to 2 lines if needed, full available width
            using (var f = Fonts.Tiny)
                TextRenderer.DrawText(g, "worst 1% of delays — where stability slips", f,
                    new Rectangle(r.Left + 12, r.Bottom - 36, r.Width - 24, 32), t.TextMuted,
                    TextFormatFlags.WordBreak | TextFormatFlags.Top);
        }

        private void DrawSmallCard(Graphics g, ThemeColors t, Rectangle r, string label, double value, Color valueColor, bool integerOnly = false)
        {
            using (var bg = new SolidBrush(t.PanelElev)) g.FillRectangle(bg, r);
            using (var p = new Pen(t.Border)) g.DrawRectangle(p, r);

            using (var f = Fonts.Tiny)
                TextRenderer.DrawText(g, label.ToUpperInvariant(), f, new Point(r.Left + 8, r.Top + 6), t.TextMuted);
            using (var f = Fonts.Stat)
            {
                string val = value > 0 ? (integerOnly ? value.ToString("N0") : value.ToString("0")) : "—";
                TextRenderer.DrawText(g, val, f, new Point(r.Left + 8, r.Top + 22), valueColor);
            }
        }

        private void DrawHistogram(Graphics g, ThemeColors t, Rectangle r)
        {
            int bins = _histogram.Length;
            float barW = (float)(r.Width - bins) / bins;
            int max = 1;
            for (int i = 0; i < bins; i++) if (_histogram[i] > max) max = _histogram[i];

            for (int i = 0; i < bins; i++)
            {
                float h = (float)_histogram[i] / max * (r.Height - 4);
                if (h < 1) continue;
                Color barColor = (i >= bins - 3) ? t.P1 : (i == 0 ? t.Median : t.Accent);
                using (var br = new SolidBrush(barColor))
                {
                    g.FillRectangle(br, r.Left + i * (barW + 1), r.Bottom - h, barW, h);
                }
            }
            using (var p = new Pen(t.Border)) g.DrawLine(p, r.Left, r.Bottom, r.Right, r.Bottom);
        }
    }
}
