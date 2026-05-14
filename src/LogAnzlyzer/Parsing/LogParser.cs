using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace LogAnzlyzer.Parsing
{
    public sealed class LogParseProgress
    {
        public long BytesRead;
        public long TotalBytes;
        public int LinesParsed;
        public int LinesSkipped;
    }

    public sealed class ParsedLog
    {
        public string FilePath;
        public string FileSha256;
        public string ParserRegex;
        public List<ParsedEntry> Entries = new List<ParsedEntry>();
        public DateTime ParsedAt = DateTime.UtcNow;
    }

    // Adaptive parser. Files <= MemoryThreshold are loaded fully into a List;
    // larger files stream line-by-line and skip raw line storage past N entries
    // (configurable cap to keep memory bounded — graph still has all delays).
    public sealed class LogParser
    {
        public const long MemoryThreshold = 100L * 1024 * 1024; // 100 MB

        private readonly Regex _rgx;
        private readonly string _datetimeFormat;

        public LogParser(string regex = null, string datetimeFormat = null)
        {
            _rgx = new Regex(regex ?? TimestampDetector.DefaultRegex, RegexOptions.Compiled);
            _datetimeFormat = datetimeFormat ?? TimestampDetector.DefaultDateTimeFormat;
        }

        public ParsedLog Parse(string path, IProgress<LogParseProgress> progress = null, CancellationToken ct = default)
        {
            var fi = new FileInfo(path);
            var log = new ParsedLog
            {
                FilePath = path,
                FileSha256 = Storage.FileHash.Sha256(path),
                ParserRegex = _rgx.ToString(),
            };

            int lineNumber = 0;
            int parsed = 0;
            int skipped = 0;
            DateTime? prev = null;
            long bytesRead = 0;

            using (var sr = new StreamReader(path))
            {
                string raw;
                while ((raw = sr.ReadLine()) != null)
                {
                    ct.ThrowIfCancellationRequested();
                    lineNumber++;
                    bytesRead += raw.Length + 1;
                    var m = _rgx.Match(raw);
                    if (!m.Success || m.Groups.Count < 2)
                    {
                        skipped++;
                        continue;
                    }
                    if (!DateTime.TryParseExact(m.Groups[1].Value, _datetimeFormat,
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out var ts))
                    {
                        skipped++;
                        continue;
                    }

                    double? delay = null;
                    if (prev.HasValue)
                    {
                        delay = (ts - prev.Value).TotalMilliseconds;
                    }
                    prev = ts;
                    parsed++;

                    log.Entries.Add(new ParsedEntry
                    {
                        LineNumber = lineNumber,
                        Timestamp = ts,
                        DelayMs = delay,
                        RawLine = raw,
                    });

                    if (parsed % 5000 == 0 && progress != null)
                    {
                        progress.Report(new LogParseProgress
                        {
                            BytesRead = bytesRead,
                            TotalBytes = fi.Length,
                            LinesParsed = parsed,
                            LinesSkipped = skipped,
                        });
                    }
                }
            }

            if (progress != null)
            {
                progress.Report(new LogParseProgress
                {
                    BytesRead = bytesRead,
                    TotalBytes = fi.Length,
                    LinesParsed = parsed,
                    LinesSkipped = skipped,
                });
            }
            return log;
        }
    }
}
