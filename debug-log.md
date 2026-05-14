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

---

## Session #3 — 2026-05-14 (v0.3 feature work)
**Trigger:** PM — v0.3 cycle approved with both features
**Framework:** WinForms / .NET Framework 4.8
**Features added:** 2 (large-file streaming, multi-file comparison view)

### [FEAT-LARGE-FILE] Streaming parser for >100 MB logs without OOM
- **Files:** `Parsing/ParsedEntry.cs`, `Parsing/LogParser.cs`, `Forms/ProgressDialog.cs` (new), `Controls/LogTableGrid.cs`, `Forms/MainForm.cs`
- **Approach:** Files ≤100 MB use the existing in-memory path. Above that, a new `ParseStreaming` path reads the file at byte level, tracks per-line (offset, length) and decodes only the first ~64 bytes for timestamp matching. `RawLine` is left null on each entry; `ParsedLog.GetRawLine(index)` lazy-seeks the file when the table cell needs the value. `LogTableGrid.SetLog(log)` keeps the log reference for lazy lookup; CSV export and Copy Raw also route through it.
- **Progress UI:** New `ProgressDialog` runs the parse on a background thread with a `CancellationTokenSource` and reports `LogParseProgress` to a themed progress bar (with bytes / MB / "streaming mode" detail). `MainForm.ParseWithProgress` chooses inline parse or progress-dialog parse based on file size.
- **Chart downsampling:** `DelayChartPanel` uses uniform decimation (`MaxDisplayPoints = 5000`) so multi-million-row logs render quickly without choking ScottPlot. Stride = ceil(total / 5000).
- **Verified:** 2.5 M entries / 218 MB log parsed end-to-end in ~4.5 s in streaming mode; no OOM; lazy raw-line read works.

### [FEAT-COMPARE] Multi-file comparison view
- **Files:** `Forms/CompareDialog.cs` (new), `Controls/ComparisonStatsPanel.cs` (new), `Controls/DelayChartPanel.cs` (RenderMulti / ChartSeries), `Forms/MainForm.cs`
- **Flow:** New `File → Compare logs...` (Ctrl+Shift+C). Opens `CompareDialog` pre-populated with recent files (CheckedListBox) + Browse for additional. On OK, each picked log is parsed via the same `ParseWithProgress` path (so streaming + cancel applies here too).
- **Tab:** A new comparison tab is built with `DelayChartPanel.RenderMulti(series, showReferenceLines: false)` — color-coded per series with legend in upper-right — and `ComparisonStatsPanel` on the right showing one card per series (color swatch + filename + P1/Median/P95/P99 + count + range).
- **Reuses:** the existing tab control, theme system, parsing pipeline.

## Verification
- `dotnet build`: 0 warnings, 0 errors
- App launches (3s smoke test, custom titlebar visible from v0.2)
- 2.5M-entry / 218MB log parsed in 4.5 s via streaming mode (verified via reflection)
- Multi-series chart and ComparisonStatsPanel are deferred to human visual sign-off (couldn't be verified headlessly without UI automation)

## Files modified / added
- `Parsing/ParsedEntry.cs` (added FileOffset, RawLineByteLength)
- `Parsing/LogParser.cs` (added streaming path, IProgress + CancellationToken support)
- `Forms/ProgressDialog.cs` (new)
- `Forms/CompareDialog.cs` (new)
- `Controls/ComparisonStatsPanel.cs` (new)
- `Controls/DelayChartPanel.cs` (RenderMulti, ChartSeries, downsampling)
- `Controls/LogTableGrid.cs` (SetLog for lazy raw-line lookup)
- `Forms/MainForm.cs` (ParseWithProgress, OpenCompareDialog, BuildComparisonTabPage)
- `LogAnzlyzer.csproj` (version 0.2.0 → 0.3.0)
- `README.md` (roadmap)

## Processes touched
parser (streaming + offset tracking), renderer (compare view), main (progress orchestration)

---

## Session #4 — 2026-05-14 (v0.4 feature work)
**Trigger:** PM — v0.4 cycle approved with full 3-feature scope (auto-update + CI/CD + signing infra)
**Framework:** WinForms / .NET Framework 4.8
**Features added:** 3

### [FEAT-AUTOUPDATE] Auto-update notifier
- **New files:** `Updates/UpdateChecker.cs`, `Forms/UpdateAvailableDialog.cs`
- **Modified:** `Forms/MainForm.cs` (Shown event), `LogAnzlyzer.csproj` (Version 0.4.0)
- **What:** On `MainForm.Shown` (background thread), call `UpdateChecker.CheckAsync()`. It hits `https://api.github.com/repos/eugine8248/log-analyzer/releases/latest`, parses `tag_name` / `name` / `html_url` / `body` via a tiny regex-based extractor (no Newtonsoft dep), compares parsed Version against `Assembly.GetExecutingAssembly().GetName().Version`. Returns the release object only when newer.
- **Dialog:** `UpdateAvailableDialog` shows release notes (read-only multiline), with three actions: Open download page (`Process.Start(html_url)`), Skip this version (persists `update.skipped_version` in SQLite app_settings — never re-prompts that exact tag), Remind me later.
- **Settings:** `update.check_on_startup` defaults to `true`. Disable via SQL until v0.5 surfaces a UI toggle.
- **Verified:** Live API call returns 200, tag_name='v0.3.0' parsed correctly, current 0.4.0.0 → no prompt (expected).

### [FEAT-CICD] GitHub Actions CI/CD
- **New files:** `.github/workflows/build.yml`, `.github/workflows/release.yml`
- **build.yml:** Runs on every push and PR to main. Sets up .NET 8 SDK on windows-latest, runs `dotnet restore`, `dotnet build -c Debug` and `-c Release` both with `-warnaserror`, then a smoke test that loads the assembly via reflection and asserts that 5 critical types exist (`MainForm`, `LogParser`, `StatsCalculator`, `CacheDatabase`, `UpdateChecker`). Catches missing references / accidental deletions before tag.
- **release.yml:** Runs on tag push matching `v*.*.*`. Restores, publishes Release to `dist/`, optionally signs (only if `SIGN_CERT_PFX_BASE64` repo secret is set — else skipped silently), zips the artifact set, then uses `softprops/action-gh-release@v2` to create the GitHub Release with auto-generated notes from commit messages and uploads the zip as an asset.
- **Effect:** Future releases are `git tag -a vX.Y.Z && git push --tags` — that's it. No manual zip, no curl, no PAT pasting.

### [FEAT-SIGN] Code-signing infrastructure
- **Modified:** `LogAnzlyzer.csproj` (added `SignOutput` MSBuild target), `README.md` (docs)
- **What:** Conditional `Target Name="SignOutput" AfterTargets="Build"` runs `signtool sign` only when `SIGN_CERT_PATH` and `SIGN_CERT_PASSWORD` env vars are set AND `Configuration == Release`. Uses DigiCert timestamp authority. No-op (no error) when env vars absent — keeps casual / CI builds running without cert config.
- **CI counterpart:** `release.yml` decodes the `SIGN_CERT_PFX_BASE64` secret to a temp .pfx, runs signtool, then deletes the temp file.
- **No actual cert acquired** — SmartScreen warning still appears for users until you get one. Setup is fully wired so when you obtain a cert (Sectigo / DigiCert / Azure Trusted Signing) you only need to set 2 env vars (local) or 2 GitHub secrets (CI).
- **README docs:** added `Code-Signing` section with both local and CI setup steps.

## Verification
- `dotnet build`: 0 warnings, 0 errors
- App launches; update check fires on `Shown` (background thread, silent on no-update)
- Live `UpdateChecker.CheckAsync()` against GitHub API: parsed v0.3.0 correctly, returned null because current 0.4.0.0 is newer (correct behaviour)
- CI workflows YAML validated by visual inspection; first real run will be when v0.4.0 tag is pushed

## Files modified / added
- `src/LogAnzlyzer/Updates/UpdateChecker.cs` (new)
- `src/LogAnzlyzer/Forms/UpdateAvailableDialog.cs` (new)
- `src/LogAnzlyzer/Forms/MainForm.cs` (TryCheckForUpdate hook)
- `src/LogAnzlyzer/LogAnzlyzer.csproj` (SignOutput target + version 0.4.0)
- `.github/workflows/build.yml` (new)
- `.github/workflows/release.yml` (new)
- `README.md` (auto-update + code-signing sections, CI badge, roadmap)

## Processes touched
main (startup hook), updater (HTTP call + dialog), build (MSBuild target + CI workflows)
