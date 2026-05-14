using System;

namespace LogAnzlyzer.Parsing
{
    // One log line, post-parse.
    // For small files (≤MemoryThreshold), RawLine is populated up-front.
    // For large files, RawLine is null and FileOffset+RawLineByteLength let
    // ParsedLog.GetRawLine(index) lazy-read the line on demand.
    public sealed class ParsedEntry
    {
        public int LineNumber;          // 1-based
        public DateTime Timestamp;
        public double? DelayMs;         // ms since previous entry; null for first entry
        public string RawLine;          // null for large files — use ParsedLog.GetRawLine
        public string Severity;         // "p1" | "median" | null
        public long FileOffset;         // byte offset into source file (for lazy read)
        public int RawLineByteLength;   // line length in bytes (no trailing newline)
    }
}
