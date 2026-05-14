# UX Flow — log-analyzer (LogAnzlyzer)

Source: `design/pack/loganzlyzer/project/` (Claude Design pack, 12 artboards across 6 sections).

## Window List

| Window | Type | Default size | Purpose |
|--------|------|--------------|---------|
| Main Window | Primary | 1280 × 780 | Hosts tabs, graph, table, stats sidebar |
| Empty State | View of Main | n/a | What user sees when no log is open |
| Timestamp-Select Dialog | Modal | 620 wide | Confirm/adjust auto-detected timestamp |
| Settings Dialog | Modal | 640 × 360+ | Tabs: General / Appearance / Cache |
| About Dialog | Modal | 460 wide | App info + GitHub link |

## Main Window Layout (top-to-bottom)

```
┌─────────────────────────────────────────────────────────────────┐
│ TitleBar (32px)        Logo · "LogAnzlyzer — file.log"   _ □ ✕ │
├─────────────────────────────────────────────────────────────────┤
│ MenuBar (28px)        File   View   Settings   Help            │
├─────────────────────────────────────────────────────────────────┤
│ TabStrip (34px)  [📄 file1.log ✕][📄 file2.log ✕][📄...][+]   │
├─────────────────────────────────┬───────────────────────────────┤
│ ChartToolbar (36px)             │ StatsSidebar (240px wide)     │
│   "Delay over time"  [10s|all]  │   STATS                       │
│                                 │   ┌────────────────────────┐  │
├─────────────────────────────────┤   │ P1 — low 1% tail       │  │
│                                 │   │     248 ms             │  │
│       Delay Chart Area          │   │     (hero card)        │  │
│       (ScottPlot)               │   └────────────────────────┘  │
│                                 │   [Median][P95][P99][Events]  │
├─────────────────────────────────┤   ───── divider ─────         │
│                                 │   SPAN                        │
│       Log Data Table            │   start  10:40:19.085         │
│       (DataGridView)            │   end    10:42:47.302         │
│                                 │   2 min 28 sec                │
│                                 │   ───── divider ─────         │
│                                 │   DISTRIBUTION                │
│                                 │   [mini histogram]            │
├─────────────────────────────────┴───────────────────────────────┤
│ StatusBar (22px) ●parsed  C:\logs\...  4128 events  pattern:.. │
└─────────────────────────────────────────────────────────────────┘
```

## Navigation Map

- File → Open → file picker → Timestamp-Select Dialog → new tab opens with graph + table
- File → Close Tab → close active tab (cached data persists)
- File → Exit → quit app
- View → Toggle stats sidebar → hide/show right panel
- View → Theme → Dark / Light → re-theme entire app at runtime
- Settings → opens Settings Dialog (Appearance tab default)
- Help → About → opens About Dialog
- Help → GitHub → opens https://github.com/eugine8248/log-analyzer in default browser
- Drag log file onto window → Timestamp-Select Dialog → new tab
- Click tab → switch active log (instant if cached)
- Click X on tab → close that tab
- Click + on tab strip → Open file dialog
- Click chart range chips (10s / 30s / 1m / all) → zoom chart x-axis
- Click chart -/+/⤢ → zoom out / in / fit
- Click table column header → sort by that column
- Click table row → highlight corresponding chart point
- Drop log file onto Empty State → Timestamp-Select Dialog

## Keyboard Shortcuts Map

| Shortcut | Action | Where |
|----------|--------|-------|
| Ctrl+O | Open log file | Anywhere |
| Ctrl+W | Close active tab | Anywhere |
| Ctrl+Tab | Next tab | Anywhere |
| Ctrl+Shift+Tab | Previous tab | Anywhere |
| Ctrl+, | Open Settings | Anywhere |
| Ctrl+T | Toggle theme (Dark ↔ Light) | Anywhere |
| F1 | About | Anywhere |
| Esc | Close active modal | Modals |
| Ctrl+= / Ctrl+- | Zoom chart in / out | Chart focus |
| Ctrl+0 | Reset chart zoom (fit all) | Chart focus |

## Drag-Drop / Right-Click Interactions

- **Drag-drop:** any file dropped onto main window or empty state → opens as new tab
- **Right-click on tab:** Close, Close Others, Close All, Reveal in Explorer
- **Right-click on table row:** Copy raw line, Copy timestamp, Copy as JSON
- **Right-click on chart:** Copy chart as image, Reset zoom

## Theme Switching

Both themes specified in `tokens.css`. Toggle persists in SQLite `app_settings` table.
On startup, app reads theme preference from cache; defaults to Dark on first launch.
