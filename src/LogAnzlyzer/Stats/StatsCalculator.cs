using System.Collections.Generic;
using System.Linq;
using LogAnzlyzer.Parsing;

namespace LogAnzlyzer.Stats
{
    public sealed class DelayStats
    {
        public double P1;       // low 1% — the headline metric
        public double P5;
        public double Median;   // P50
        public double P95;
        public double P99;
        public double Mean;
        public double Min;
        public double Max;
        public int    Count;    // number of delays (= entries - 1)

        public static DelayStats Empty => new DelayStats();
    }

    public static class StatsCalculator
    {
        // P1 = "low 1%" per the user's spec — interpreted as the worst 1% (the slowest delays).
        // Convention: rank descending, take the value at the 1% boundary so P1 is the threshold
        // above which the slowest 1% live. Equivalent to the 99th percentile in standard stats.
        // We surface it as "P1 — low 1% tail".
        public static DelayStats Compute(IList<ParsedEntry> entries)
        {
            var delays = new List<double>(entries.Count);
            foreach (var e in entries)
            {
                if (e.DelayMs.HasValue) delays.Add(e.DelayMs.Value);
            }
            if (delays.Count == 0) return DelayStats.Empty;

            delays.Sort();
            return new DelayStats
            {
                Count = delays.Count,
                Min = delays[0],
                Max = delays[delays.Count - 1],
                Mean = delays.Average(),
                Median = Percentile(delays, 0.50),
                P95 = Percentile(delays, 0.95),
                P99 = Percentile(delays, 0.99),
                P1  = Percentile(delays, 0.99),   // see comment above
                P5  = Percentile(delays, 0.95),
            };
        }

        // Tags entries above the P1 threshold so the table can highlight them coral,
        // and tags around-median entries with a teal dot.
        public static void TagSeverity(IList<ParsedEntry> entries, DelayStats stats)
        {
            double medianLow = stats.Median * 0.85;
            double medianHigh = stats.Median * 1.15;
            foreach (var e in entries)
            {
                if (!e.DelayMs.HasValue) continue;
                if (e.DelayMs.Value >= stats.P1) e.Severity = "p1";
                else if (e.DelayMs.Value >= medianLow && e.DelayMs.Value <= medianHigh) e.Severity = "median";
                else e.Severity = null;
            }
        }

        // Returns 28-bin histogram [0..200+] for the sidebar mini chart.
        public static int[] Histogram(IList<ParsedEntry> entries, int bins = 28, double maxMs = 200)
        {
            var hist = new int[bins];
            double binSize = maxMs / (bins - 1);
            foreach (var e in entries)
            {
                if (!e.DelayMs.HasValue) continue;
                int idx = e.DelayMs.Value >= maxMs ? bins - 1 : (int)(e.DelayMs.Value / binSize);
                if (idx < 0) idx = 0;
                hist[idx]++;
            }
            return hist;
        }

        private static double Percentile(List<double> sorted, double p)
        {
            if (sorted.Count == 0) return 0;
            double rank = p * (sorted.Count - 1);
            int lo = (int)System.Math.Floor(rank);
            int hi = (int)System.Math.Ceiling(rank);
            if (lo == hi) return sorted[lo];
            double frac = rank - lo;
            return sorted[lo] + (sorted[hi] - sorted[lo]) * frac;
        }
    }
}
