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
    public sealed class ChartSeries
    {
        public string Label;
        public IList<ParsedEntry> Entries;
        public DelayStats Stats;
        public Color Color;            // primary line color (auto-picked if Empty)
    }

    // ScottPlot wrapper styled with our theme.
    // - Single-log mode (Render): plots delay over time + dashed P1 + dotted median.
    // - Multi-log mode (RenderMulti): one line per series, color-coded; P1/median lines
    //   omitted to keep the chart readable when comparing.
    // - Downsamples series exceeding MaxDisplayPoints via uniform decimation so huge
    //   logs (millions of points) render smoothly without sacrificing visible shape.
    public sealed class DelayChartPanel : Panel
    {
        public const int MaxDisplayPoints = 5000;

        private readonly FormsPlot _plot = new FormsPlot { Dock = DockStyle.Fill };

        public DelayChartPanel()
        {
            Controls.Add(_plot);
            Dock = DockStyle.Fill;
            ApplyTheme();
            ThemeManager.ThemeChanged += (s, e) => { ApplyTheme(); _plot.Refresh(); };
            BuildContextMenu();
        }

        private void BuildContextMenu()
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add("Save chart as PNG...", null, (s, e) => SavePng());
            menu.Items.Add("Reset zoom", null, (s, e) => { _plot.Plot.AxisAuto(); _plot.Refresh(); });
            _plot.ContextMenuStrip = menu;
        }

        public void Render(IList<ParsedEntry> entries, DelayStats stats)
        {
            var t = ThemeManager.Current;
            RenderMulti(new[] { new ChartSeries { Label = "delay (ms)", Entries = entries, Stats = stats, Color = t.Accent } }, showReferenceLines: true);
        }

        public void RenderMulti(IList<ChartSeries> series, bool showReferenceLines)
        {
            var plt = _plot.Plot;
            plt.Clear();
            if (series == null || series.Count == 0) { _plot.Refresh(); return; }

            var t = ThemeManager.Current;
            var palette = new[] { t.Accent, t.P1, t.Median, t.Warning, t.Error, t.AccentHover };

            for (int i = 0; i < series.Count; i++)
            {
                var s = series[i];
                if (s.Entries == null || s.Entries.Count == 0) continue;

                var col = s.Color.IsEmpty ? palette[i % palette.Length] : s.Color;
                var (xs, ys) = BuildXY(s.Entries, MaxDisplayPoints);
                if (xs.Length == 0) continue;
                var line = plt.AddScatterLines(xs, ys, color: col, lineWidth: 1.2f, label: s.Label);
                line.MarkerSize = 0;
            }

            if (showReferenceLines && series.Count == 1 && series[0].Stats != null)
            {
                var st = series[0].Stats;
                if (st.P1 > 0)
                {
                    var p1Line = plt.AddHorizontalLine(st.P1, color: t.P1, width: 1.5f, style: LineStyle.Dash);
                    p1Line.PositionLabel = true;
                    p1Line.PositionLabelBackground = t.P1;
                    p1Line.PositionLabelOppositeAxis = true;
                }
                if (st.Median > 0)
                    plt.AddHorizontalLine(st.Median, color: t.Median, width: 1f, style: LineStyle.Dot);
            }

            if (series.Count > 1) plt.Legend(location: Alignment.UpperRight);

            plt.XAxis.DateTimeFormat(true);
            plt.XLabel(""); plt.YLabel("");
            plt.AxisAuto();
            _plot.Refresh();
        }

        // Uniform decimation — preserves overall shape; cheap; works at any scale.
        // For more-faithful tail-spike preservation we could swap in LTTB later.
        private static (double[] xs, double[] ys) BuildXY(IList<ParsedEntry> entries, int maxPoints)
        {
            int total = 0;
            for (int i = 0; i < entries.Count; i++) if (entries[i].DelayMs.HasValue) total++;
            if (total == 0) return (new double[0], new double[0]);

            int stride = total <= maxPoints ? 1 : (int)System.Math.Ceiling(total / (double)maxPoints);
            var xs = new List<double>(System.Math.Min(total, maxPoints + 16));
            var ys = new List<double>(System.Math.Min(total, maxPoints + 16));
            int seen = 0;
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                if (!e.DelayMs.HasValue) continue;
                if (seen % stride == 0)
                {
                    xs.Add(e.Timestamp.ToOADate());
                    ys.Add(e.DelayMs.Value);
                }
                seen++;
            }
            return (xs.ToArray(), ys.ToArray());
        }

        private void SavePng()
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "PNG image (*.png)|*.png";
                sfd.FileName = "delay-chart-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".png";
                if (sfd.ShowDialog(FindForm()) != DialogResult.OK) return;
                try
                {
                    _plot.Plot.SaveFig(sfd.FileName);
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = "/select,\"" + sfd.FileName + "\"",
                        UseShellExecute = true,
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(FindForm(), "Failed to save chart:\n" + ex.Message, "Save error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
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
