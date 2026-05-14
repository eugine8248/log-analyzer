using System.Collections.Generic;

namespace LogAnzlyzer.Parsing
{
    public sealed class RegexPattern
    {
        public string Name;            // human label
        public string Description;     // one-line hint
        public string Pattern;         // captures the timestamp in group 1
        public string DateTimeFormat;  // matching .NET format string
        public string Example;         // sample line that would match
    }

    // Curated set of timestamp patterns we ship with the app.
    // Pickable from the Timestamp dialog so users don't have to author regex
    // by hand for the formats we already understand.
    public static class RegexLibrary
    {
        public static readonly IReadOnlyList<RegexPattern> Presets = new[]
        {
            new RegexPattern
            {
                Name = "ISO 8601 (default)",
                Description = "yyyy-MM-dd HH:mm:ss.fff — the format LogAnzlyzer assumes by default",
                Pattern = TimestampDetector.DefaultRegex,
                DateTimeFormat = TimestampDetector.DefaultDateTimeFormat,
                Example = "2026-05-13 10:40:19.085 worker booted",
            },
            new RegexPattern
            {
                Name = "ISO 8601 with T separator",
                Description = "yyyy-MM-ddTHH:mm:ss.fff — common in JSON logs and Kubernetes",
                Pattern = @"(\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3})",
                DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fff",
                Example = "2026-05-13T10:40:19.085Z worker booted",
            },
            new RegexPattern
            {
                Name = "ISO 8601 (no millis)",
                Description = "yyyy-MM-dd HH:mm:ss — second precision only",
                Pattern = @"(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})",
                DateTimeFormat = "yyyy-MM-dd HH:mm:ss",
                Example = "2026-05-13 10:40:19 worker booted",
            },
            new RegexPattern
            {
                Name = "Java / Log4j default",
                Description = "yyyy-MM-dd HH:mm:ss,fff — comma instead of dot before millis",
                Pattern = @"(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2},\d{3})",
                DateTimeFormat = "yyyy-MM-dd HH:mm:ss,fff",
                Example = "2026-05-13 10:40:19,085 INFO worker booted",
            },
            new RegexPattern
            {
                Name = "Python logging default",
                Description = "yyyy-MM-dd HH:mm:ss,fff (same shape as Log4j)",
                Pattern = @"(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2},\d{3})",
                DateTimeFormat = "yyyy-MM-dd HH:mm:ss,fff",
                Example = "2026-05-13 10:40:19,085 - root - INFO - worker booted",
            },
            new RegexPattern
            {
                Name = "Syslog (RFC 3164)",
                Description = "MMM d HH:mm:ss — no year (year inferred to current)",
                Pattern = @"([A-Z][a-z]{2} {1,2}\d{1,2} \d{2}:\d{2}:\d{2})",
                DateTimeFormat = "MMM  d HH:mm:ss",
                Example = "May 13 10:40:19 host process[123]: worker booted",
            },
            new RegexPattern
            {
                Name = "Apache / Common Log Format",
                Description = "[dd/MMM/yyyy:HH:mm:ss zzz]",
                Pattern = @"\[(\d{2}/[A-Z][a-z]{2}/\d{4}:\d{2}:\d{2}:\d{2}) [+\-]\d{4}\]",
                DateTimeFormat = "dd/MMM/yyyy:HH:mm:ss",
                Example = "127.0.0.1 - - [13/May/2026:10:40:19 +0800] \"GET /...\"",
            },
            new RegexPattern
            {
                Name = "IIS W3C",
                Description = "yyyy-MM-dd HH:mm:ss — date and time as separate fields, joined by space",
                Pattern = @"^(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})",
                DateTimeFormat = "yyyy-MM-dd HH:mm:ss",
                Example = "2026-05-13 10:40:19 GET /index.html - 80 - 127.0.0.1",
            },
        };
    }
}
