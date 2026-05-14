# Project Requirements — log-analyzer

**Date:** 2026-05-14
**Mode:** Approval
**Author:** Requirements Gatherer (via PM in approval-mode interview)

---

## 1. Project Purpose
A desktop log analyzer that automates parsing of timestamped log files to detect
**performance drops and stability issues**. Users upload logs, the app computes
per-event delays in milliseconds (time between consecutive timestamps), graphs
those delays over time, and surfaces tail-latency outliers via the **P1 (low 1%)
percentile** so degradations are visible at a glance.

### Success criteria
- User can open a log file and see a delay-over-time graph within 5 seconds (small files)
- P1 percentile delay is shown prominently next to the graph
- Multiple logs can be open simultaneously, each in its own tab
- Re-opening a previously analyzed log loads instantly from cache

---

## 2. Target Users
**Wider team / open-source release.**
- Polished UX with sensible defaults
- Comprehensive error handling (bad log format, file unreadable, parse errors)
- Full README with screenshots
- Distributed via GitHub (open source)

---

## 3. Target Platforms
- **Windows** only (x64)
- Min OS: Windows 10 or later
- Runtime: .NET Framework 4.8 (ships with Windows by default — no installer dependency)

---

## 4. Core Features

### Must-Have
- **Upload logs** — Open file via menu, button, or drag-and-drop onto the window
- **Timestamp parsing** — Default format `YYYY-MM-DD HH:MM:SS.mmm` (e.g. `2026-05-13 10:40:19.085`)
- **User-defined timestamp position** — User selects where the timestamp starts in each line via a preview-and-select UI; app auto-detects but user can override; advanced regex option
- **Delay computation** — Compute time difference (ms) between consecutive log entries
- **Delay graph** — Line/scatter chart of delay (y-axis ms) over time (x-axis timestamp), zoomable
- **P1 percentile** — Display the low 1% (worst-case) delay value prominently
- **Tabbed UI (VS Code style)** — Each opened log gets its own tab, closeable with X. Each tab contains:
  - **Graph view** — interactive delay chart
  - **Data view** — sortable table of (timestamp, delay_ms, raw_line)
- **Theme toggle** — Dark mode (default, VS Code palette) and light mode, switchable at runtime

### Nice-to-Have (phase 2, not for v1)
- Recent files menu
- Export graph as PNG / data as CSV
- Auto-update from GitHub Releases
- System tray quick-open
- Native notifications when long parses complete
- Multiple percentiles (P5, P10, P50, P95, P99)

### Out of Scope
- Real-time log tailing (this is post-hoc analysis)
- Log streaming over network
- Multi-machine log aggregation
- Authentication / multi-user

---

## 5. Framework
- **Choice:** .NET Framework 4.8 + WinForms
- **Frontend stack:** WinForms controls + `TabControl` for VS Code-style tabs
- **Backend stack:** C# (.NET Framework 4.8) — same process; no separate backend server
- **Charting library candidate:** **ScottPlot** (excellent for time-series, performant on large datasets) or built-in `System.Windows.Forms.DataVisualization.Charting`. UX Designer to confirm based on visual fit.
- **Theming:** Custom — WinForms doesn't ship with theming; use color palette constants applied at control level. Consider `MetroFramework` or `Krypton` if needed for polished look.

---

## 6. Phases
Default: `Discovery → Design → Build → QA → Package`

---

## 7. Design Preferences
- **Default theme:** Dark (VS Code-inspired)
  - Background `#1e1e1e`, Sidebar `#252526`, Tabs `#2d2d30`, Active tab `#1e1e1e`
  - Text `#d4d4d4`, Muted text `#858585`
  - Accent `#007acc` (VS Code blue) for selection, links, graph primary color
  - Graph palette: `#007acc` (line), `#f48771` (P1 marker), `#4ec9b0` (median reference)
- **Light theme:** Standard light palette (white bg, dark text, same accent)
- **Toggle:** Settings menu → Theme → Dark / Light (persists in SQLite)
- **Reference:** VS Code dark — tabs at top, optional left sidebar (file list), main panel for graph + data

---

## 8. Connectivity
**Fully offline.** No internet required for any feature. App never makes network calls
in v1. Future auto-update would be the only outbound network call.

---

## 9. File System Access
- **Read:** User-chosen `.log`, `.txt` files via OpenFileDialog or drag-drop
- **Write:** SQLite cache file at `%APPDATA%\log-analyzer\cache.db`
- **Watch:** Not required (no real-time tailing)
- **Drag-drop:** Yes — drop a file onto the window to open it as a new tab

---

## 10. OS Integration
- **Drag-and-drop:** Yes (must-have)
- **System tray:** No
- **Native notifications:** No
- **Auto-launch on startup:** No
- **Global shortcuts:** No
- **Deep links:** No
- **Native menu:** Standard Windows menu bar (File, View, Settings, Help)

---

## 11. Auto-Update
**No.** Users download new versions manually from GitHub Releases.
README will document how to check for updates.

---

## 12. Local Database
**SQLite** (via `System.Data.SQLite` NuGet package).
- Location: `%APPDATA%\log-analyzer\cache.db`
- Tables (initial):
  - `analyzed_files` — file path, file hash, last analyzed timestamp, parser pattern used
  - `parsed_entries` — file_id, timestamp, delay_ms, raw_line (or pointer to file offset for large files)
  - `summary_stats` — file_id, p1, p5, p50, p95, p99, mean, stddev
  - `app_settings` — key/value (theme preference, recent files list)
- Cache invalidation: SHA-256 of file contents — re-parse if file changes

---

## 13. External APIs
**None.** Fully offline tool.

---

## 14. Authentication
**None.** Local single-user tool.

---

## 15. Packaging & Distribution
- **Build:** `dotnet build` produces a self-contained release in `dist/`
- **Distribution:** GitHub Releases — upload built `.exe` (or zipped folder) to https://github.com/eugine8248/log-analyzer
- **Code signing:** Not for v1 (users will see SmartScreen warning on first run — acceptable for OSS tool)
- **Installer:** Not for v1 — portable executable

---

## 16. Timeline & Constraints
- **Timeline:** No hard deadline
- **Performance constraints:**
  - Small files (≤100 MB): in-memory parse, full graph in <5s
  - Large files (100 MB – 2 GB): streaming parse, batch into SQLite, progressive graph rendering
  - Memory budget: under 1 GB even on 2 GB log files (streaming required)
- **Accessibility:** Standard keyboard navigation; not WCAG-compliant (open-source, can be added later)
- **Regulatory:** None

---

## Engineering Notes Surfaced for Downstream Specialists

- **Adaptive parser:** Detect file size on open; use `File.ReadAllLines` (in-memory) for ≤100MB, `StreamReader` line-by-line for larger files
- **Timestamp pattern detection:** On file open, scan first 50 lines, regex-search for the standard `YYYY-MM-DD HH:MM:SS.mmm` pattern; surface position to user via preview-and-select UI; allow regex override
- **VS Code tab UX in WinForms:** Standard `TabControl` styled with custom `DrawItem` handler for close buttons and dark theme; or use `Krypton.Toolkit` for ready-made styled tabs
- **Graph performance:** For 1M+ data points, downsample for display (LTTB algorithm) — keep raw data for stats
- **P1 calculation:** Sort delays, take value at index `length * 0.01` — same data backs the P1 display, the graph marker, and any future P5/P95 features
