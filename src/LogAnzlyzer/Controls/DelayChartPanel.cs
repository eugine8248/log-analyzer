using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using LogAnzlyzer.Parsing;
using LogAnzlyzer.Stats;
using LogAnzlyzer.Theme;
using ScottPlot;

namespace LogAnzlyzer.Controls
{
    // ScottPlot wrapper styled with our theme tokens.
    // Plots delay (ms) over event index (or timestamp) and overlays a
    // P1 reference line in coral, and a median reference line in teal.
    public sealed class DelayChartPanel : Panel
    {
        private readonly FormsPlot _plot = new FormsPlot { Dock = DockStyle.Fill };

        public DelayChartPanel()
        {
            Controls.Add(_plot);
            Dock = DockStyle.Fill;
            ApplyTheme();
            ThemeManager.ThemeChanged += (s, e) => { ApplyTheme(); _plot.Refresh(); };
        }

        public void Render(IList<ParsedEntry> entries, DelayStats stats)
        {
            var plt = _plot.Plot;
            plt.Clear();

            if (entries == null || entries.Count == 0)
            {
                _plot.Refresh();
                return;
            }

            // Build x = OLE date, y = delay
            var xs = new List<double>(entries.Count);
            var ys = new List<double>(entries.Count);
            foreach (var e in entries)
            {
                if (!e.DelayMs.HasValue) continue;
                xs.Add(e.Timestamp.ToOADate());
                ys.Add(e.DelayMs.Value);
            }
            if (xs.Count == 0) { _plot.Refresh(); return; }

            var t = ThemeManager.Current;
            var line = plt.AddScatterLines(xs.ToArray(), ys.ToArray(), color: t.Accent, lineWidth: 1.2f, label: "delay (ms)");
            line.MarkerSize = 0;

            // P1 reference line
            if (stats != null && stats.P1 > 0)
            {
                var p1Line = plt.AddHorizontalLine(stats.P1, color: t.P1, width: 1.5f, style: LineStyle.Dash);
                p1Line.PositionLabel = true;
                p1Line.PositionLabelBackground = t.P1;
                p1Line.PositionLabelOppositeAxis = true;
            }

            // Median reference line
            if (stats != null && stats.Median > 0)
            {
                var mLine = plt.AddHorizontalLine(stats.Median, color: t.Median, width: 1f, style: LineStyle.Dot);
            }

            plt.XAxis.DateTimeFormat(true);
            plt.XLabel("");
            plt.YLabel("");
            plt.AxisAuto();
            _plot.Refresh();
        }

        private void ApplyTheme()
        {
            var t = ThemeManager.Current;
            var plt = _plot.Plot;
            plt.Style(
                figureBackground: t.Bg,
                dataBackground: t.Bg,
                grid: t.Grid,
                tick: t.TextMuted,
                axisLabel: t.TextMuted,
                titleLabel: t.TextStrong);
            plt.XAxis.Color(t.TextMuted);
            plt.YAxis.Color(t.TextMuted);
        }
    }
}
