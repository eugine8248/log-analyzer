using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
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
        public bool IsStreaming;
    }

    public sealed class ParsedLog
    {
        public string FilePath;
        public string FileSha256;
        public string ParserRegex;
        public List<ParsedEntry> Entries = new List<ParsedEntry>();
        public DateTime ParsedAt = DateTime.UtcNow;
        public bool IsStreaming;        // true when raw lines were not loaded into memory

        // Lazy-fetch the raw line for an entry when the parser was in streaming mode.
        // Falls back to ParsedEntry.RawLine when populated.
        public string GetRawLine(int index)
        {
            if (index < 0 || index >= Entries.Count) return "";
            var e = Entries[index];
            if (e.RawLine != null) return e.RawLine;
            if (e.RawLineByteLength <= 0) return "";
            try
            {
                using (var fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096))
                {
                    fs.Position = e.FileOffset;
                    byte[] buf = new byte[e.RawLineByteLength];
                    int read = 0;
                    while (read < buf.Length)
                    {
                        int n = fs.Read(buf, read, buf.Length - read);
                        if (n <= 0) break;
                        read += n;
                    }
                    return Encoding.UTF8.GetString(buf, 0, read);
                }
            }
            catch
            {
                return "";
            }
        }
    }

    // Adaptive parser:
    //  - File size ≤ MemoryThreshold (100 MB): full in-memory parse with RawLine populated.
    //  - File size  > MemoryThreshold: byte-level streaming with offset tracking; RawLine
    //    left null on each entry. Use ParsedLog.GetRawLine(index) for lazy access.
    public sealed class LogParser
    {
        public const long MemoryThreshold = 100L * 1024 * 1024; // 100 MB
        private const int StreamBufferSize = 64 * 1024;

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
            var streaming = fi.Length > MemoryThreshold;
            return streaming ? ParseStreaming(path, fi.Length, progress, ct)
                             : ParseInMemory(path, fi.Length, progress, ct);
        }

        // -------- small-file path --------
        private ParsedLog ParseInMemory(string path, long fileLength, IProgress<LogParseProgress> progress, CancellationToken ct)
        {
            var log = new ParsedLog
            {
                FilePath = path,
                FileSha256 = Storage.FileHash.Sha256(path),
                ParserRegex = _rgx.ToString(),
                IsStreaming = false,
            };

            int lineNumber = 0, parsed = 0, skipped = 0;
            DateTime? prev = null;
            long bytesRead = 0;

            using (var sr = new StreamReader(path, Encoding.UTF8, true, StreamBufferSize))
            {
                string raw;
                while ((raw = sr.ReadLine()) != null)
                {
                    ct.ThrowIfCancellationRequested();
                    lineNumber++;
                    bytesRead += Encoding.UTF8.GetByteCount(raw) + 1;
                    var m = _rgx.Match(raw);
                    if (!m.Success || m.Groups.Count < 2) { skipped++; continue; }
                    if (!DateTime.TryParseExact(m.Groups[1].Value, _datetimeFormat,
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out var ts))
                    { skipped++; continue; }

                    double? delay = prev.HasValue ? (double?)(ts - prev.Value).TotalMilliseconds : null;
                    prev = ts;
                    parsed++;
                    log.Entries.Add(new ParsedEntry
                    {
                        LineNumber = lineNumber,
                        Timestamp = ts,
                        DelayMs = delay,
                        RawLine = raw,
                    });

                    if (parsed % 5000 == 0) Report(progress, bytesRead, fileLength, parsed, skipped, false);
                }
            }
            Report(progress, bytesRead, fileLength, parsed, skipped, false);
            return log;
        }

        // -------- large-file path: byte-level streaming + offset tracking --------
        // Tracks byte offset of every line so RawLine can be lazy-read later.
        // Avoids holding the raw line text in memory.
        private ParsedLog ParseStreaming(string path, long fileLength, IProgress<LogParseProgress> progress, CancellationToken ct)
        {
            var log = new ParsedLog
            {
                FilePath = path,
                FileSha256 = Storage.FileHash.Sha256(path),
                ParserRegex = _rgx.ToString(),
                IsStreaming = true,
            };

            int lineNumber = 0, parsed = 0, skipped = 0;
            DateTime? prev = null;
            long fileOffset = 0;
            long lineStart = 0;
            int lineLength = 0;
            var lineBuf = new List<byte>(256);
            var buf = new byte[StreamBufferSize];

            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, StreamBufferSize))
            {
                int read;
                while ((read = fs.Read(buf, 0, buf.Length)) > 0)
                {
                    ct.ThrowIfCancellationRequested();
                    for (int i = 0; i < read; i++)
                    {
                        byte b = buf[i];
                        if (b == (byte)'\n')
                        {
                            ProcessLine(log, lineBuf, lineStart, lineLength, ref lineNumber, ref parsed, ref skipped, ref prev);
                            lineBuf.Clear();
                            lineStart = fileOffset + i + 1;
                            lineLength = 0;
                        }
                        else if (b == (byte)'\r')
                        {
                            // skip but don't include in length
                        }
                        else
                        {
                            lineBuf.Add(b);
                            lineLength++;
                        }
                    }
                    fileOffset += read;
                    if (parsed % 5000 == 0) Report(progress, fileOffset, fileLength, parsed, skipped, true);
                }
                // trailing line without newline
                if (lineBuf.Count > 0)
                {
                    ProcessLine(log, lineBuf, lineStart, lineLength, ref lineNumber, ref parsed, ref skipped, ref prev);
                }
            }
            Report(progress, fileLength, fileLength, parsed, skipped, true);
            return log;
        }

        private void ProcessLine(ParsedLog log, List<byte> lineBuf, long lineStart, int lineLength,
            ref int lineNumber, ref int parsed, ref int skipped, ref DateTime? prev)
        {
            lineNumber++;
            // Decode just the prefix (first ~64 bytes is enough for the timestamp)
            int prefixLen = System.Math.Min(64, lineBuf.Count);
            var prefix = Encoding.UTF8.GetString(lineBuf.ToArray(), 0, prefixLen);
            var m = _rgx.Match(prefix);
            if (!m.Success || m.Groups.Count < 2) { skipped++; return; }
            if (!DateTime.TryParseExact(m.Groups[1].Value, _datetimeFormat,
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var ts))
            { skipped++; return; }

            double? delay = prev.HasValue ? (double?)(ts - prev.Value).TotalMilliseconds : null;
            prev = ts;
            parsed++;
            log.Entries.Add(new ParsedEntry
            {
                LineNumber = lineNumber,
                Timestamp = ts,
                DelayMs = delay,
                RawLine = null,                  // lazy
                FileOffset = lineStart,
                RawLineByteLength = lineLength,
            });
        }

        private static void Report(IProgress<LogParseProgress> progress, long bytesRead, long total, int parsed, int skipped, bool streaming)
        {
            progress?.Report(new LogParseProgress
            {
                BytesRead = bytesRead,
                TotalBytes = total,
                LinesParsed = parsed,
                LinesSkipped = skipped,
                IsStreaming = streaming,
            });
        }
    }
}
