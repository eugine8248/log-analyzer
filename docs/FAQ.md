# LogAnzlyzer — FAQ

## What is this for?

A Windows desktop tool for inspecting per-event delays in any timestamped log
and surfacing the **P1 (low 1%)** tail latency — the slowest 1% of delays. P1
is where stability slips: things look fine on average but a small fraction of
events are several times slower than the rest.

## What does P1 mean here exactly?

By LogAnzlyzer's convention, **P1 = the worst 1% of delays**, i.e. the value
above which the slowest 1% of events live. In standard percentile notation
that's the 99th percentile — but framed as a tail-latency metric (low 1% =
worst 1%) rather than a "fast 1%". One number, prominently shown, that tells
you whether the system was stable.

## How do I open a log?

Three ways:
1. **Drag-and-drop** any `.log` or `.txt` file onto the window
2. **File → Open** (`Ctrl+O`)
3. **File → Open Recent** to re-open something from the SQLite cache

After opening, a "Confirm timestamp position" dialog asks you to verify
where the timestamp lives in each line. Auto-detect handles the standard
ISO format. For others, see the next question.

## My log doesn't use the default timestamp format. What now?

In the **Confirm timestamp position** dialog you have three options:
1. Pick from the **Pattern library** dropdown — covers ISO 8601 (with/without T,
   with/without millis), Java/Log4j, Python logging, syslog (RFC 3164),
   Apache Common Log Format, and IIS W3C
2. Click **Adjust** then click-and-drag across the timestamp on any sample line —
   the regex updates live
3. **Edit the regex directly** in the bottom textbox (power-user mode). Group 1
   must capture the timestamp; the .NET DateTime format must match.

## How big a log can I open?

- **Up to ~100 MB:** parses fully in memory. Fastest path.
- **Above 100 MB:** automatic switch to streaming mode. The parser tracks each
  line's `(file_offset, length)` and lazy-reads raw text only when the table
  cell needs it. A progress dialog with **Cancel** appears.
- **Verified in dev:** 2.5 million entries / 218 MB parsed in ~4.5 s.
- **Multi-GB logs** are supported in principle but watch RAM during parse —
  the offset list still grows linearly with line count.

## Why is the chart smooth even though my log has millions of points?

The chart **downsamples to 5,000 points** via uniform decimation. The graph
preserves visual shape; the stats sidebar uses ALL data points, not the
downsampled set, so P1/Median/etc are exact.

## Where does the SQLite cache live? Can I delete it?

`%APPDATA%\log-analyzer\cache.db`

It stores:
- App settings (theme, "skipped update" version, "check for updates" toggle)
- Recent files list
- (Future) Cached parse results so re-opening the same file is instant

Deleting it is safe — the app re-creates the schema on next launch.

## How do I compare two log runs?

Two distinct features:

### Compare (overlay)
**File → Compare logs...** (`Ctrl+Shift+C`)
Pick 2 or more logs, get a single chart with each as a colored line.
Per-series stats sidebar. Use this when you want to *eyeball regressions*.

### Diff (baseline vs current)
**File → Diff two logs...** (`Ctrl+Shift+D`)
Pick exactly 2 logs. The first is the baseline, the second is the current.
Stats sidebar shows side-by-side values + **delta column** with %-change.
Improvements (lower delay) render teal, regressions render coral.

## Does the app phone home?

Only one outbound call: a single GET to
`https://api.github.com/repos/eugine8248/log-analyzer/releases/latest` on
startup, to check for updates. Network errors are silently ignored.

To disable:
```sql
sqlite3 %APPDATA%/log-analyzer/cache.db "UPDATE app_settings SET value='false' WHERE key='update.check_on_startup';"
```

(A UI toggle is on the v0.6+ roadmap.)

## I don't trust the binary — can I build from source?

Yes:
```powershell
git clone https://github.com/eugine8248/log-analyzer
cd log-analyzer
dotnet publish src/LogAnzlyzer/LogAnzlyzer.csproj -c Release -o dist
.\dist\LogAnzlyzer.exe
```
Requires .NET SDK 6.0+ and .NET Framework 4.8 reference assemblies.

## SmartScreen says "unrecognized publisher"

Releases are unsigned. Click **More info → Run anyway**, or build from source.
We have the signing infrastructure wired (`SignOutput` MSBuild target,
`SIGN_CERT_PFX_BASE64` GitHub Actions secret) — just no certificate yet.

## Can I export results?

- **Right-click the chart → Save chart as PNG**
- **Right-click the table → Export to CSV**
- **Right-click table row → Copy raw line**

CSV uses UTF-8 with BOM and RFC-4180 quoting.

## What's the keyboard map?

| Shortcut | Action |
|---|---|
| Ctrl+O | Open log |
| Ctrl+W | Close active tab |
| Ctrl+Shift+C | Compare logs (overlay) |
| Ctrl+Shift+D | Diff two logs (baseline vs current) |
| Ctrl+T | Toggle theme |
| Ctrl+, | Settings |
| F1 | About |

## Where can I report bugs / request features?

https://github.com/eugine8248/log-analyzer/issues

When reporting a bug, please include:
- App version (Help → About)
- Log file size and approximate line count
- The first 3 lines of your log (so we can reproduce the parser path)
