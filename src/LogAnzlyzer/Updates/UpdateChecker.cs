using System;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LogAnzlyzer.Updates
{
    public sealed class ReleaseInfo
    {
        public string TagName;        // "v0.4.0"
        public string Name;           // "v0.4.0 — auto-update + CI"
        public string HtmlUrl;        // browser URL to release
        public string Body;           // release notes (markdown)
        public Version Version;       // parsed from TagName
    }

    // Polls GitHub's Releases API for the project's latest published release.
    // No auth needed (public repo). Compares against Application.ProductVersion.
    public static class UpdateChecker
    {
        private const string ApiUrl = "https://api.github.com/repos/eugine8248/log-analyzer/releases/latest";
        private const string UserAgent = "LogAnzlyzer-UpdateChecker";

        public static Version CurrentVersion =>
            Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0);

        // Returns the latest release if newer than CurrentVersion; null otherwise (or on error).
        public static async Task<ReleaseInfo> CheckAsync()
        {
            try
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
                using (var wc = new WebClient())
                {
                    wc.Headers.Add(HttpRequestHeader.UserAgent, UserAgent);
                    wc.Headers.Add(HttpRequestHeader.Accept, "application/vnd.github+json");
                    string json = await wc.DownloadStringTaskAsync(ApiUrl).ConfigureAwait(false);
                    var info = ParseRelease(json);
                    if (info?.Version == null) return null;
                    return info.Version > CurrentVersion ? info : null;
                }
            }
            catch
            {
                return null; // network error / API unreachable: silent skip
            }
        }

        // Minimal JSON extraction without bringing in Newtonsoft.
        // We only need: tag_name, name, html_url, body. Each is a top-level string.
        internal static ReleaseInfo ParseRelease(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;
            string tag  = ExtractString(json, "tag_name");
            string name = ExtractString(json, "name");
            string url  = ExtractString(json, "html_url");
            string body = ExtractString(json, "body");
            if (string.IsNullOrEmpty(tag)) return null;
            var ver = ParseVersion(tag);
            return new ReleaseInfo
            {
                TagName = tag,
                Name = name,
                HtmlUrl = url,
                Body = body,
                Version = ver,
            };
        }

        private static string ExtractString(string json, string key)
        {
            // Match: "key": "value" with escapes. Cheap regex scoped to top-level pairs;
            // good enough for the GitHub releases payload (no nested objects in values we need).
            var m = Regex.Match(json, "\"" + Regex.Escape(key) + "\"\\s*:\\s*\"((?:\\\\.|[^\"\\\\])*)\"", RegexOptions.Singleline);
            if (!m.Success) return null;
            return Regex.Unescape(m.Groups[1].Value);
        }

        private static Version ParseVersion(string tag)
        {
            // Accept v1.2.3 or 1.2.3 (with optional 4th segment)
            var m = Regex.Match(tag ?? "", @"v?(\d+)\.(\d+)\.(\d+)(?:\.(\d+))?");
            if (!m.Success) return null;
            int major = int.Parse(m.Groups[1].Value);
            int minor = int.Parse(m.Groups[2].Value);
            int build = int.Parse(m.Groups[3].Value);
            int rev   = m.Groups[4].Success ? int.Parse(m.Groups[4].Value) : 0;
            return new Version(major, minor, build, rev);
        }
    }
}
