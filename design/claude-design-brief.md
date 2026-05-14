# Claude Design Brief — log-analyzer

> **Instructions:** Once approved, paste the prompt block below into https://claude.ai/design.
> Export the result as HTML and place all files into `C:\Users\eugin\projects\log-analyzer\design\`.

---

## PROMPT TO PASTE INTO CLAUDE DESIGN

```
PROJECT: log-analyzer — a Windows desktop log analyzer (WinForms, C#, .NET Framework 4.8)
PURPOSE: Open log files, parse timestamped entries, visualise per-event delays in milliseconds,
and surface the P1 (low 1%) tail latency to expose performance drops and stability issues.

TARGET PLATFORM: Windows 10+ desktop only. Use Windows Fluent / VS Code-inspired conventions.
This is built in WinForms — designs should be implementable with standard Windows controls
(no fancy CSS animations, no transparent blur, no mobile patterns).

TARGET USERS: Engineers and ops people analysing log files for performance regressions.
This is an open-source tool, so polish and clarity matter.

WINDOWS NEEDED:

1. MAIN WINDOW (primary)
   Layout: VS Code-inspired
   - Top: native menu bar (File, View, Settings, Help)
   - Below menu: a tab strip (VS Code style — closeable tabs, "+" to open new)
   - Each tab represents one opened log file
   - Inside each tab body: a vertically-split panel
     • Top half: interactive delay graph (line chart, x = timestamp, y = delay in ms)
       — Highlight the P1 (low 1%) threshold line in a contrasting accent color
       — Hover tooltip showing exact delay + timestamp
       — Zoom / pan controls
     • Bottom half: sortable data table (columns: timestamp, delay_ms, raw_line)
       — Click column header to sort
       — Row click highlights the corresponding point in the graph above
   - Right of the graph or above it: a stats sidebar showing key metrics:
     • P1 (low 1%) — large, prominent
     • Median, P95, P99 (smaller)
     • Total events, time span
   - Bottom: status bar showing file path, total events, parse status

2. EMPTY STATE (when no log is open)
   - Centered: large drag-drop zone
   - "Drag a log file here, or click to browse"
   - Icon + supporting text
   - Below: 3 sample lines showing the expected format `2026-05-13 10:40:19.085 your message`

3. TIMESTAMP-SELECT DIALOG (modal, opens after file is loaded)
   - Title: "Confirm timestamp position"
   - Content: shows the first 5 log lines verbatim in a monospace font
   - The auto-detected timestamp portion is highlighted in the accent color in each line
   - Below: "Looks right?" Yes / Adjust buttons
   - If "Adjust" clicked: lines become click-and-drag selectable so user picks the start and (optional) end of the timestamp
   - Bottom: "Show regex" toggle — reveals the inferred regex pattern, editable for power users
   - Buttons: [Cancel] [Use this pattern]

4. SETTINGS DIALOG (modal)
   - Tabs: General / Appearance / Cache
   - General: default parser fallback regex, "Remember timestamp pattern per file" toggle
   - Appearance: Theme selector (Dark / Light), Accent color preview
   - Cache: cache size, "Clear cache" button, cache location path

5. ABOUT DIALOG (modal)
   - App name, version, link to GitHub repo
   - License (MIT or similar)

KEY USER WORKFLOWS:

A) Opening a log file
   1. User drags a .log file onto the main window (or File → Open, or clicks empty-state zone)
   2. Timestamp-Select Dialog appears showing first 5 lines with auto-detected highlight
   3. User confirms or adjusts → dialog closes
   4. New tab created with the filename, parsing happens, graph + table populate
   5. P1 metric updates in stats sidebar

B) Switching between logs
   1. User clicks a different tab
   2. Graph and data update instantly (cached if previously analyzed)

C) Closing a tab
   1. User clicks the X on the tab → tab closes
   2. Cached data remains (file can be reopened from Recent Files in v2)

D) Toggling theme
   1. User opens Settings → Appearance → selects Light
   2. Entire app re-themes immediately without restart

DESKTOP UI ELEMENTS (please design these):

- Native menu bar — File (Open, Close Tab, Exit), View (Theme toggle, Toggle stats sidebar), Settings, Help (About, GitHub)
- VS Code-style tab strip (closeable, draggable to reorder, "+" button at end)
- Drag-and-drop zone (empty state)
- Stats sidebar (P1 prominent, other metrics smaller)
- Time-series line graph with P1 reference line
- Sortable data table
- Status bar at bottom of window
- Modal dialogs (timestamp-select, settings, about)
- Toggle switches and dropdowns inside settings

DESIGN STYLE:
- VS Code-inspired dark theme (default):
  • Background: #1e1e1e
  • Sidebar / panel: #252526
  • Tab strip: #2d2d30
  • Active tab: #1e1e1e (matches main background)
  • Border: #3e3e42
  • Text primary: #d4d4d4
  • Text muted: #858585
  • Accent: #007acc (used for selection, links, primary graph line, P1 highlight)
  • Graph line: #007acc
  • P1 reference line and badge: #f48771 (warm coral, high contrast against dark)
  • Median reference: #4ec9b0 (teal)
  • Success / OK: #4ec9b0
  • Warning: #ce9178
  • Error: #f48771
- Light theme variant (toggleable):
  • Background: #ffffff
  • Sidebar / panel: #f3f3f3
  • Tab strip: #ececec
  • Active tab: #ffffff
  • Border: #d4d4d4
  • Text primary: #333333
  • Text muted: #6c6c6c
  • Accent: #007acc (same accent works on both)
  • Graph line: #007acc
  • P1 highlight: #d83b01

TYPOGRAPHY:
- UI font: Segoe UI (Windows native), 13px body, 18px headings, 11px muted
- Monospace (for log lines and timestamps in dialogs and table): Cascadia Code or Consolas, 12px

COMPONENT TYPES TO INCLUDE IN THE LIBRARY:
- Tab item (default, hover, active, with-close-button states)
- Button (primary, secondary, ghost — default, hover, pressed, disabled)
- Toggle switch
- Dropdown / select
- Text input
- Modal dialog wrapper
- Sortable data table
- Time-series line chart with reference lines
- Stat card (small label + large value)
- Status bar
- Drag-drop zone (default, hover, drag-active states)

OUTPUT REQUEST:
1. Full window mockup — Main Window with one log tab open, showing graph + table + stats sidebar (BOTH dark and light themes)
2. Empty state mockup (BOTH themes)
3. Timestamp-Select dialog mockup with auto-detected highlight (dark theme)
4. Settings dialog (Appearance tab) mockup (dark theme)
5. Component library — every component listed above shown in isolation with all states (dark theme primary, light variants where they differ visually)
6. Export everything as HTML with embedded CSS so I can read computed values for WinForms theming
```
