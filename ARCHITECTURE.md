# log-analyzer — Architecture

**Date:** 2026-05-14
**Approval Mode:** Approval
**Phases:** Discovery → Design → Build → QA → Package
**Tech Stack:** WinForms (.NET Framework 4.8), C# — full populated by Requirements Gatherer
**Target Platforms:** Windows (x64)

---

## Phase Log
- 2026-05-14 — Project initiated, routed to Desktop App Builder PM, approval mode set
- 2026-05-14 — Discovery phase started: Requirements Gatherer in approval mode interview

_(Each specialist appends their section below as they complete their work.)_

---

## Project Overview
A Windows desktop log analyzer (WinForms / .NET Framework 4.8) that parses timestamped
log files and graphs per-event delays in milliseconds. Surfaces the **P1 (low 1%)** tail
latency to make performance drops and instability visible. Each opened log lives in its
own VS Code-style tab containing a delay graph and a sortable data table.

## Tech Stack
- Framework: WinForms on .NET Framework 4.8
- Language: C#
- UI: WinForms controls + custom-themed TabControl (VS Code style); ScottPlot or Charting library for graphs
- Database: SQLite (System.Data.SQLite) at %APPDATA%\log-analyzer\cache.db
- Theming: Custom palette (dark default — VS Code colors; light toggle)
- Auto-update: None (manual download from GitHub Releases)
- Authentication: None
- Connectivity: Fully offline

## Target Platforms
- Windows 10 or later (x64)
- Runtime: .NET Framework 4.8 (preinstalled on modern Windows)

## UX Flow
See `design/ux-flow.md` — 5 surfaces (Main Window, Empty State, Timestamp Dialog, Settings, About)
with full keyboard shortcut map and drag-drop interactions.

## Design Source
Claude Design pack at `design/pack/loganzlyzer/` (12 artboards, original modern-IDE palette
— deliberately not VS Code's exact hex codes for trade-dress reasons). All design tokens
live in `design/pack/loganzlyzer/project/tokens.css` and are mirrored in `Theme/ThemeColors.cs`.

## Component Map
See `design/component-map.md` — 22 components mapped to WinForms equivalents:
WindowFrame (custom MainForm), TitleBar, MenuBar (MenuStrip), ClosableTabControl,
ChartToolbar, DelayChart (ScottPlot), LogTable (DataGridView), StatsSidebar, StatCard,
DropZone, StatusBar, Modal (Form), TimestampDialog, SettingsDialog, AboutDialog,
Button (3 kinds × 5 states), Toggle, Dropdown, TextInput.

## Folder Structure
```
log-analyzer/
├── ARCHITECTURE.md
├── README.md
├── requirements.md
├── session-snapshot.yaml
├── .gitignore
├── design/
│   ├── claude-design-brief.md
│   ├── ux-flow.md
│   ├── component-map.md
│   └── pack/loganzlyzer/        (Claude Design source export)
└── src/
    ├── LogAnzlyzer.sln
    └── LogAnzlyzer/
        ├── LogAnzlyzer.csproj
        ├── App.config
        ├── Program.cs
        ├── Forms/
        │   ├── MainForm.cs
        │   ├── TimestampDialog.cs
        │   ├── SettingsDialog.cs
        │   └── AboutDialog.cs
        ├── Controls/
        │   ├── ClosableTabControl.cs
        │   ├── DropZonePanel.cs
        │   ├── DelayChartPanel.cs
        │   ├── LogTableGrid.cs
        │   ├── StatsSidebarPanel.cs
        │   └── StatusBarPanel.cs (ThemedStatusBar)
        ├── Parsing/
        │   ├── LogParser.cs
        │   ├── TimestampDetector.cs
        │   └── ParsedEntry.cs
        ├── Stats/
        │   └── StatsCalculator.cs
        ├── Storage/
        │   ├── CacheDatabase.cs
        │   └── FileHash.cs
        └── Theme/
            ├── ThemeColors.cs
            └── ThemeManager.cs
```

## Frontend Notes
- Framework: WinForms on .NET Framework 4.8 (SDK-style csproj for cleaner build)
- Charting: ScottPlot 4.1.74 (NuGet) — themed via ThemeManager
- DB: System.Data.SQLite.Core 1.0.118 (NuGet)
- All controls subscribe to `ThemeManager.ThemeChanged` for runtime theme switching
- Custom-drawn `ClosableTabControl` for VS-Code-style tabs (close X + plus button)
- `DropZonePanel` paints dashed rounded border with hover/drag-active states
- `StatsSidebarPanel` paints hero P1 card + 2×2 stat grid + span + mini histogram
- `ThemedStatusBar` paints two-segment status bar with optional accent highlights
- Build verified: **0 warnings, 0 errors** with .NET SDK 9.0.102 targeting net48

## QA Report Summary
**Run #1 — 2026-05-14**
- Build: clean (0 warnings, 0 errors)
- App launch smoke test: pass
- Parser: 2000/2000 entries parsed from `sample/boot-sequence.log`
- Stats sanity check: P1=112ms, Median=22ms, Max=321ms — all match expected values from planted spikes
- 22 automated checks passed, 14 deferred to human visual verification, 0 failures
- 2 LOW-severity warnings (custom titlebar + drag-select adjust both scoped to v0.2 in README roadmap)
- Status: **CLEAN — ready for Package phase**
- Full report: `qa-report.md`



