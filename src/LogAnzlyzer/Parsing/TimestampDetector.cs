using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace LogAnzlyzer.Parsing
{
    // Auto-detects timestamp position by scanning the first N lines for the
    // standard format: yyyy-MM-dd HH:mm:ss.fff (e.g. 2026-05-13 10:40:19.085).
    // Returns the regex pattern + the start/end character positions if a
    // consistent prefix is found; otherwise returns the regex with no fixed bounds.
    public sealed class TimestampDetectionResult
    {
        public string Regex;
        public int? StartIndex;       // character index in line (0-based) — for highlighting
        public int? EndIndex;         // exclusive
        public int Matched;           // count of sample lines that matched
        public int TotalSampled;
        public List<string> SampleLines = new List<string>();
    }

    public static class TimestampDetector
    {
        // Standard pattern: yyyy-MM-dd HH:mm:ss.fff
        public static readonly string DefaultRegex =
            @"(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3})";

        public const string DefaultDateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";

        public static TimestampDetectionResult DetectFromFile(string path, int sampleCount = 5)
        {
            var lines = new List<string>();
            using (var sr = new StreamReader(path))
            {
                string ln;
                while (lines.Count < sampleCount && (ln = sr.ReadLine()) != null)
                {
                    lines.Add(ln);
                }
            }
            return Detect(lines);
        }

        public static TimestampDetectionResult Detect(IList<string> sampleLines)
        {
            var rgx = new Regex(DefaultRegex);
            var result = new TimestampDetectionResult
            {
                Regex = DefaultRegex,
                TotalSampled = sampleLines.Count,
                SampleLines = new List<string>(sampleLines),
            };

            int? consistentStart = null;
            int? consistentEnd = null;
            int matched = 0;
            foreach (var ln in sampleLines)
            {
                var m = rgx.Match(ln);
                if (m.Success)
                {
                    matched++;
                    if (consistentStart == null)
                    {
                        consistentStart = m.Index;
                        consistentEnd = m.Index + m.Length;
                    }
                    else if (m.Index != consistentStart || m.Index + m.Length != consistentEnd)
                    {
                        // position varies — fall back to no fixed bounds
                        consistentStart = null;
                        consistentEnd = null;
                    }
                }
            }
            result.Matched = matched;
            result.StartIndex = consistentStart;
            result.EndIndex = consistentEnd;
            return result;
        }

        // Build a custom regex from a user-selected start/end position in the line.
        // Captures whatever characters appear in that slice using the standard regex
        // for digits/separators commonly seen in timestamps.
        public static string BuildRegexForBounds(string sampleLine, int start, int end)
        {
            // Generic fallback — match the same length of "any non-whitespace" then anchor with surrounding.
            int len = end - start;
            string body = "(\\S{" + len + "})";
            string prefix = start > 0 ? Regex.Escape(sampleLine.Substring(0, start)) : "^";
            return prefix + body;
        }
    }
}
