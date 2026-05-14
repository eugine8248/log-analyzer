# Debug Log — log-analyzer

## Session #1 — 2026-05-14
**Trigger:** On-demand from PM (user reported UI issues from screenshots after deployment)
**Framework:** WinForms / .NET Framework 4.8
**Failures received:** 4 (LOW–MEDIUM severity, post-Package phase)
**Fixes applied:** 4
**Blocked:** 0

### [FIX-001] Hero card "ms" unit not aligned with value baseline
- **File:** `src/LogAnzlyzer/Controls/StatsSidebarPanel.cs`
- **Process:** renderer
- **Root cause:** "ms" was drawn at a fixed Y offset (`r.Top + 60`) without accounting for the actual baselines of the value font (StatHero, ~44px) and unit font (Body, ~13px). Worked at small values but visually drifted with large numbers.
- **Fix applied:** Measure both `val` and `"ms"` with `TextRenderer.MeasureText` using `NoPadding`, then compute `unitY = valueY + valSz.Height - unitSz.Height - 6` so unit baseline aligns with value baseline.
- **Cross-impact:** none

### [FIX-002] P1 description "worst 1% of delays — performance drops here" truncated to "performan..."
- **File:** `src/LogAnzlyzer/Controls/StatsSidebarPanel.cs`
- **Process:** renderer
- **Root cause:** Description rect was `r.Width - 24` (188px) and used `WordEllipsis` with single-line height (18px). Didn't fit on one line.
- **Fix applied:** Shortened text to "worst 1% of delays — where stability slips" (better UX too) and switched to `WordBreak | Top` flags with 2-line height (32px) so it wraps if still too narrow on smaller widths. Hero card description now always shows full message.
- **Cross-impact:** none

### [FIX-003] Span text "1016 min 14 sec" unreadable for cross-day spans
- **File:** `src/LogAnzlyzer/Controls/StatsSidebarPanel.cs`
- **Process:** renderer
- **Root cause:** `_spanText` was always formatted as `{minutes} min {seconds} sec` regardless of duration. For 17-hour spans this produced "1016 min" which is hard to read. Also, when start/end were on different days, only times showed (e.g. "17:12" → "10:08") which made it look like time went backwards.
- **Fix applied:**
  1. Added `FormatSpan(TimeSpan)` helper that picks units appropriately: `Xd Yh Zm` for days, `Xh Ym Zs` for hours, `Xm Ys` for minutes, `S.fffs` for seconds.
  2. When `_spanStart.Date != _spanEnd.Date`, format start/end with `MM-dd HH:mm:ss.fff` instead of just time.
- **Cross-impact:** none

### [FIX-004] Settings dialog rows clipped — only end of label visible
- **File:** `src/LogAnzlyzer/Forms/SettingsDialog.cs`
- **Process:** renderer
- **Root cause:** Multiple issues compounded:
  1. Labels in `AddRow` were `AutoSize=true` with no explicit width. The toggle was positioned at `body.Width - 80` but body width was being read at construction time, before Dock layout resolved. This meant the toggle could end up at an unexpected position, and the label overflowed.
  2. Theme cards were positioned at fixed `x=0` and `x=140` but the Dark card at x=0 fell behind the rail panel because of a Z-order quirk between `Dock=Left` and `Dock=Fill` siblings added in the wrong sequence.
  3. Body's actual padding wasn't being respected because controls were positioned at literal pixel coordinates.
- **Fix applied:** Full rewrite of `BuildContents` and `AddRow`:
  - `ClientSize` (not `Size`) so the dialog reserves room for chrome correctly
  - Footer added FIRST so it claims its bottom area before body fills
  - Body added LAST with `Dock=Fill` so it picks up the remaining client area cleanly
  - Each row is now a child Panel with explicit Width tied to host's ClientSize, and resize handlers keep it in sync
  - Toggle uses `Anchor=Top|Right` so it stays glued to row's right edge
  - Labels use `AutoSize=false` with explicit Width = `row.Width - 110` so they reserve room for the toggle without overlapping
  - Theme cards repositioned to x=0 and x=152 with width=140; visible side-by-side
  - Dialog widened from 660 to 720 to fit comfortably
- **Cross-impact:** none — settings interactions still trigger `ThemeManager.Set`

---

## Verification

- **Build:** `dotnet build` → 0 warnings, 0 errors
- **Smoke test:** App launches, stays alive, exits cleanly via Stop-Process
- **Release rebuild:** `dotnet publish -c Release` succeeded; `dist/LogAnzlyzer.exe` updated

## Files modified
- `src/LogAnzlyzer/Controls/StatsSidebarPanel.cs` (FIX-001, FIX-002, FIX-003)
- `src/LogAnzlyzer/Forms/SettingsDialog.cs` (FIX-004 — full rewrite of layout)

## Processes touched
renderer (no main process or DB changes)

---

## Session #2 — 2026-05-14 (v0.2 feature work, not bug-fixes)
**Trigger:** On-demand from PM — v0.2 cycle approved with full 4-feature scope
**Framework:** WinForms / .NET Framework 4.8
**Features added:** 4

### [FEAT-A] Drag-select timestamp bounds (replaces v0.1 placeholder)
- **File:** `src/LogAnzlyzer/Forms/TimestampDialog.cs` (rewritten)
- **What:** `Adjust` toggles `LinesPreview.AdjustMode`. While on, mouse-down captures a character index; drag tracks a selection range; mouse-up calls `TimestampDetector.BuildRegexForBounds()` and pushes the new regex into the textbox.
- **Hit-test:** `HitTestChar(line, x)` walks character widths via `TextRenderer.MeasureText` with `NoPadding` so the picked index matches the painted glyph centre.

### [FEAT-B] Custom titlebar
- **New file:** `src/LogAnzlyzer/Controls/CustomTitleBar.cs`
- **Modified:** `src/LogAnzlyzer/Forms/MainForm.cs`
- **What:** `FormBorderStyle.None`; titlebar paints logo + title + themed min/max/restore/close buttons. `MainForm.WndProc` overrides `WM_NCHITTEST` to return `HTCAPTION` for the titlebar drag zone, and `HTLEFT`/`HTRIGHT`/`HTTOP`/`HTBOTTOM` (and the four corners) for the outer 6px ring so the OS still gives us native resize cursors and behaviour. Double-click on titlebar toggles maximize.
- **Trade-off:** loses Windows snap layouts (the multi-monitor zone overlay on hover-max). Acceptable for v0.2; v0.3 can reintroduce via `WM_NCCALCSIZE` if needed.

### [FEAT-C] Recent files
- **Modified:** `src/LogAnzlyzer/Storage/CacheDatabase.cs`, `src/LogAnzlyzer/Forms/MainForm.cs`
- **What:** New `recent_files(path PRIMARY KEY, opened_at)` table with descending index on `opened_at`. `AddRecentFile` uses `ON CONFLICT(path) DO UPDATE SET opened_at = datetime('now')` so re-opening bumps the entry. `File → Open Recent` rebuilds on `DropDownOpening` to stay fresh. Includes `Clear recent files` action.

### [FEAT-D] Export chart PNG / data CSV
- **Modified:** `src/LogAnzlyzer/Controls/DelayChartPanel.cs`, `src/LogAnzlyzer/Controls/LogTableGrid.cs`
- **What:** Right-click on chart → `Save chart as PNG...` (uses `_plot.Plot.SaveFig(path)`) + `Reset zoom`. Right-click on table → `Export to CSV...` (UTF-8 BOM-prefixed, RFC-4180 quoting on raw_line) + `Copy raw line`. Both reveal in Explorer after save.

## Verification
- `dotnet build`: 0 warnings, 0 errors
- App launches (3s smoke test, custom titlebar visible)
- `dotnet publish -c Release`: succeeded
- Recent-files SQL verified by code review (trivial CRUD with try/catch wrappers)
- Drag-select / titlebar / context menus need human visual sign-off

## Files modified
- `src/LogAnzlyzer/Forms/TimestampDialog.cs` (FEAT-A — rewrite)
- `src/LogAnzlyzer/Forms/MainForm.cs` (FEAT-B + FEAT-C wiring)
- `src/LogAnzlyzer/Controls/CustomTitleBar.cs` (FEAT-B — new file)
- `src/LogAnzlyzer/Storage/CacheDatabase.cs` (FEAT-C — schema + API)
- `src/LogAnzlyzer/Controls/DelayChartPanel.cs` (FEAT-D — PNG export)
- `src/LogAnzlyzer/Controls/LogTableGrid.cs` (FEAT-D — CSV export, copy)
- `src/LogAnzlyzer/LogAnzlyzer.csproj` (version 0.1.0 → 0.2.0)
- `README.md` (roadmap update)

## Processes touched
renderer (titlebar, dialogs, controls), storage (new table), main (WndProc override)
