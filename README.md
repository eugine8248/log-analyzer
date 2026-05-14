# LogAnzlyzer

A Windows desktop log analyzer that surfaces performance drops and stability issues
by computing per-event delays from timestamped log files and highlighting the **P1
(low 1%) tail latency**.

Built with **C# · WinForms · .NET Framework 4.8**.

![Build](https://github.com/eugine8248/log-analyzer/actions/workflows/build.yml/badge.svg) ![Platform](https://img.shields.io/badge/platform-Windows-lightgrey) ![License](https://img.shields.io/badge/license-MIT-green)

## What it does

- Open any timestamped log file (default format: `YYYY-MM-DD HH:MM:SS.mmm`)
- Compute the time delay (in milliseconds) between consecutive events
- Plot delay-over-time as an interactive chart
- Surface the **P1** worst-case tail latency in a hero stat card
- Tab-based UI — open multiple logs side by side
- Light/dark themes with VS-Code-style modern-IDE aesthetic
- SQLite cache so re-opening the same log is instant

## Quick start

### Requirements
- Windows 10 or later
- .NET Framework 4.8 (preinstalled on modern Windows)
- Visual Studio 2022 or `dotnet` SDK 6.0+ to build from source

### Build & run from source
```powershell
cd src
dotnet restore
dotnet build LogAnzlyzer/LogAnzlyzer.csproj -c Release
dotnet run --project LogAnzlyzer/LogAnzlyzer.csproj
```

The release `.exe` lands in `src/LogAnzlyzer/bin/Release/net48/LogAnzlyzer.exe`.

### Open a log
1. Drag a `.log` or `.txt` file onto the window — or **File → Open** (`Ctrl+O`)
2. Confirm the auto-detected timestamp pattern in the modal that appears
3. New tab opens with the delay chart, sortable data table, and stats sidebar

## Expected log format

Default: each line begins with a timestamp like `2026-05-13 10:40:19.085 message...`

For other formats, edit the regex in the **Confirm timestamp position** dialog
that appears after opening a file. The default regex captures the standard
ISO-8601-like format above.

## Architecture

```
src/
└── LogAnzlyzer/
    ├── Program.cs              entry point
    ├── Forms/
    │   ├── MainForm            tab host, menu, status bar, drag-drop
    │   ├── TimestampDialog     auto-detect + override modal
    │   ├── SettingsDialog      theme + behavior
    │   └── AboutDialog
    ├── Controls/
    │   ├── ClosableTabControl  custom-drawn VS-Code-style tabs
    │   ├── DropZonePanel       empty-state drag-drop
    │   ├── DelayChartPanel     ScottPlot wrapper, themed
    │   ├── LogTableGrid        themed DataGridView
    │   ├── StatsSidebarPanel   P1 hero + small stats + histogram
    │   └── StatusBarPanel      themed bottom bar
    ├── Parsing/
    │   ├── LogParser           adaptive parser (in-memory + streaming)
    │   ├── TimestampDetector   auto-detect timestamp position
    │   └── ParsedEntry         per-line model
    ├── Stats/
    │   └── StatsCalculator     P1 / P95 / P99 / Median / histogram
    ├── Storage/
    │   ├── CacheDatabase       SQLite at %APPDATA%\log-analyzer\cache.db
    │   └── FileHash            SHA-256 for cache invalidation
    └── Theme/
        ├── ThemeColors         all design tokens (mirror of tokens.css)
        └── ThemeManager        runtime dark/light switching
```

Design pack (Claude Design output) lives in `design/pack/loganzlyzer/`.
The `tokens.css` in there is the canonical color/typography source.

## Roadmap

- [x] v0.1 — basic open/parse/chart/table/stats with theme toggle
- [x] v0.1.1 — UI fixes (hero baseline, span format, settings layout)
- [x] v0.2 — drag-select timestamp bounds, custom titlebar, recent files, export PNG/CSV
- [x] v0.3 — large-file streaming with cancellable progress + multi-file comparison view
- [x] v0.4 — auto-update notifier + GitHub Actions CI/CD + code-signing infrastructure
- [ ] v0.5 — in-app diff between two logs, regex pattern library, FAQ docs

## Auto-Update

On launch the app silently queries `https://api.github.com/repos/eugine8248/log-analyzer/releases/latest`.
If a newer tag is found, a non-blocking dialog offers to open the download page.
Skipping a version is remembered so it doesn't re-prompt.
Toggle off via Settings (planned) or directly:
```sql
sqlite3 %APPDATA%/log-analyzer/cache.db "UPDATE app_settings SET value='false' WHERE key='update.check_on_startup';"
```

## Code-Signing (optional — for releases you distribute)

Releases ship unsigned by default — users see SmartScreen "unrecognized publisher" once and
can click through. To sign your own builds:

### Local builds
```powershell
$env:SIGN_CERT_PATH     = "C:\path\to\your.pfx"
$env:SIGN_CERT_PASSWORD = "your-pfx-password"
dotnet build src/LogAnzlyzer/LogAnzlyzer.csproj -c Release
# signtool runs automatically as a post-build step (csproj target SignOutput)
```

### CI builds (GitHub Actions)
Add two repository secrets:
- `SIGN_CERT_PFX_BASE64` — your `.pfx` file, base64-encoded
- `SIGN_CERT_PASSWORD`   — its password

Generate base64 from PowerShell:
```powershell
[Convert]::ToBase64String([IO.File]::ReadAllBytes("your.pfx")) | clip
```

The `release.yml` workflow detects the secret and signs `LogAnzlyzer.exe` automatically.

## Licence

MIT — see `LICENSE` (to be added).
