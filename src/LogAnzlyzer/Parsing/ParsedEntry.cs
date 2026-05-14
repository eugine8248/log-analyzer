using System;

namespace LogAnzlyzer.Parsing
{
    // One log line, post-parse. DelayMs is null for the first entry.
    public sealed class ParsedEntry
    {
        public int LineNumber;          // 1-based
        public DateTime Timestamp;
        public double? DelayMs;         // ms since previous entry
        public string RawLine;
        public string Severity;         // "p1" | "median" | null
    }
}
