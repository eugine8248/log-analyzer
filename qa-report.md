# QA Report — log-analyzer

**Date:** 2026-05-14
**Framework:** WinForms (.NET Framework 4.8)
**Target Platforms:** Windows 10+ (x64)
**QA Run:** #1
**Tested by:** desktop-builder QA agent (automated where possible, deferred items flagged for human verification)

---

## Summary

| | Count |
|---|---|
| Total checks | 38 |
| ✅ Passed (automated) | 22 |
| 👤 Deferred to human (visual / interactive) | 14 |
| ❌ Failed | 0 |
| ⚠️ Warnings | 2 |
| 🚨 Severity breakdown | HIGH 0 · MEDIUM 0 · LOW 2 (warnings only) |

**Overall:** **CLEAN for build + parser/stats logic.** GUI surface needs human visual sign-off before Package phase.

---

## Test Setup

- Sample log generated: `sample/boot-sequence.log` — 2000 lines, 116 KB, format `YYYY-MM-DD HH:MM:SS.mmm`
- Deliberate delay patterns to exercise stats:
  - Baseline: random 5–40 ms between events
  - Every 47th event: 90–120 ms (medium spike, ~2% of events → exercises P99)
  - Every 200th event: 250–330 ms (heavy spike, ~0.5% of events → exercises P1 / max)
- Build: `dotnet build -c Debug` against .NET SDK 9.0.102, target `net48` — **0 warnings, 0 errors**
- App launch smoke test: `Start-Process LogAnzlyzer.exe` → ran 3s, responsive, killed cleanly
- Reflection-based unit testing of Parser + Stats via PowerShell

---

## Automated Checks — Results

### 1. Build & Compile
- ✅ Project restores all NuGet packages (ScottPlot.WinForms 4.1.74, System.Data.SQLite.Core 1.0.118)
- ✅ `dotnet build` succeeds with 0 warnings, 0 errors
- ✅ Output `LogAnzlyzer.exe` produced at `src\LogAnzlyzer\bin\Debug\net48\`

### 2. App Launch
- ✅ Process starts without crash
- ✅ Process stays alive (3s monitored), no immediate exit
- ✅ Process terminates cleanly when killed

### 3. Timestamp Detector (`TimestampDetector`)
- ✅ Default regex `(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3})` correctly identified
- ✅ Auto-detected position: chars 0–23 in sample log (consistent across all 5 sample lines)
- ✅ Match rate: 5/5 sample lines

### 4. Log Parser (`LogParser`)
- ✅ Parses 2000 entries from sample log without error
- ✅ All entries have valid Timestamp + DelayMs (except first, where DelayMs is null by design)
- ✅ No exceptions on either small file or simulated streaming path

### 5. Stats Calculator (`StatsCalculator`)
Computed values from 1999 delays (entries-1):

| Metric | Value | Sanity check |
|--------|-------|--------------|
| Min | 5 ms | ✅ matches generator floor |
| Max | 321 ms | ✅ matches heaviest planted spike (250–330ms range) |
| Mean | 24.57 ms | ✅ slightly above median due to spikes |
| Median | 22 ms | ✅ matches baseline midpoint (5–40ms range) |
| P95 | 38.1 ms | ✅ baseline upper bound — heavy spikes are <2% so P95 stays in baseline |
| P99 | 112 ms | ✅ captures the medium spike pattern (every 47th) |
| **P1 (worst 1%)** | **112 ms** | ✅ equals statistical P99 by design — this is the headline metric |
| Count | 1999 | ✅ correct (n entries → n-1 delays) |

P1 / P99 math is correct per the documented convention in `StatsCalculator.cs` (P1 = "low 1%" = "worst 1% of delays" = statistical P99).

### 6. Severity Tagging
- ✅ Entries with delay ≥ P1 (112ms) tagged as `"p1"` (will render coral/bold in table)
- ✅ Entries within ±15% of median tagged as `"median"` (will render teal dot)

### 7. Histogram
- ✅ 28-bin histogram generated for sidebar mini-chart
- ✅ Bin 0 = fastest, bin 27 = 200ms+ (where the heavy spikes land)

### 8. SQLite Cache Init
- ✅ `CacheDatabase.Initialize()` runs on Program startup
- ✅ Schema creates `app_settings`, `analyzed_files`, `summary_stats` tables
- ✅ `GetSetting`/`SetSetting` round-trip works (theme persistence)

### 9. Theme Color Tokens
- ✅ All 30 color tokens from `tokens.css` mirrored in `ThemeColors.cs` (both Dark and Light variants)
- ✅ `ThemeManager.Toggle()` swaps between Dark and Light, fires `ThemeChanged` event
- ✅ Theme persisted to SQLite on change

### 10. File Structure Compliance
- ✅ All directories from spec exist: Forms/, Controls/, Parsing/, Stats/, Storage/, Theme/
- ✅ All `*.cs` files included via SDK-style csproj globbing (no missing-file warnings)

---

## Items Deferred to Human Verification

These need a human in front of the app to verify visually or by interacting with the GUI. The QA agent cannot validate these headlessly without screenshots or a UI-automation framework (out of scope for v1).

### GUI Visual Fidelity (vs. design pack)
- 👤 Empty state drop zone matches design (dashed rounded border, upload icon, sample format box)
- 👤 ClosableTabControl renders VS-Code-style tabs with accent top-border on active tab
- 👤 Stats sidebar P1 hero card displays large coral value as designed
- 👤 Mini histogram colors (median/accent/p1) render correctly
- 👤 Status bar shows deep blue background with white text segments
- 👤 Title bar custom layout (currently uses default Windows chrome — design called for custom titlebar; deferred to v0.2)

### Interactive Behavior
- 👤 Drag-and-drop a `.log` file onto the main window opens the Timestamp dialog
- 👤 Timestamp dialog shows highlighted timestamp on first 5 lines
- 👤 "Use this pattern" button parses the file and creates a new tab
- 👤 Tab close X button removes the tab; tab + button opens file dialog
- 👤 ScottPlot delay chart renders with P1 reference line (coral) and median line (teal)
- 👤 LogTableGrid sorts on column header click
- 👤 Table P1 rows render with coral bold delay value
- 👤 Settings dialog Appearance tab — clicking Dark / Light card switches theme live

---

## Warnings (LOW severity, non-blocking)

### [WARN-001] Custom title bar not implemented
- **Severity:** LOW
- **Area:** Renderer / WindowFrame
- **Description:** Design pack specified a custom 32px title bar with logo and themed window control buttons. Current implementation uses default Windows form chrome. Functional but visually less polished than the design.
- **Suggested fix (v0.2):** Set `FormBorderStyle = None` and implement custom paint + draggable region (added effort: ~1 day for proper minimize/maximize/close handling).

### [WARN-002] TimestampDialog "Adjust" button shows placeholder message
- **Severity:** LOW
- **Area:** Renderer / TimestampDialog
- **Description:** Design pack showed a click-and-drag adjustment UI for the timestamp position. Current implementation has the button but it shows a "not yet implemented in v0.1 — edit the regex directly" message. Power-user regex override IS fully functional.
- **Suggested fix (v0.2):** Implement drag-select on the LinesPreview panel that captures start/end character positions and rebuilds the regex via `TimestampDetector.BuildRegexForBounds`.

---

## Cross-Platform Notes

Windows-only target — no cross-platform tests required. .NET Framework 4.8 is Windows-only by definition.

---

## Routing Decision

**No HIGH or MEDIUM severity failures found in the testable surface.**

The 2 LOW warnings are deferred features explicitly scoped out of v0.1 and documented in the README roadmap (custom titlebar = v0.2, drag-select adjust = v0.2).

**Recommendation:** No Debugger cycle needed. Proceed to Package phase pending human visual sign-off on the 14 deferred items.

---

## Handoff

Returning to PM with `result: CLEAN — ready for Package phase` and the caveat that human sign-off on visual fidelity is recommended before tagging a v0.1 release.
