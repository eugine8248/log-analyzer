# Component Map — log-analyzer (LogAnzlyzer)

Components extracted from `design/pack/loganzlyzer/project/`. Each maps to a WinForms
control or custom UserControl. All colors come from the design tokens (see
`design/pack/loganzlyzer/project/tokens.css`) which we'll mirror in C# as `ThemeColors`.

## Window-Level

### WindowFrame
- **WinForms equivalent:** `MainForm : Form` with custom title bar (FormBorderStyle.None)
- **Notes:** Logo (LA mark) + title + native min/max/close buttons. Background = bg token.
- **Theming:** all child controls re-paint on ThemeChanged event.

### TitleBar (32px tall)
- **Type:** custom Panel with paint
- **Contents:** logo (var(--la-accent)) + title text + window control buttons (46px wide each)
- **Colors:** background `--la-titlebar`, text `--la-text-muted`, border `--la-border-soft`

### MenuBar (28px tall)
- **WinForms equivalent:** `MenuStrip` with custom rendering (override `ProfessionalColorTable`)
- **Items:** File, View, Settings, Help (first letter underlined: <u>F</u>ile)
- **Background:** `--la-menubar`, hover background `--la-accent-soft`

## Tab System

### ClosableTabControl (34px tall)
- **WinForms equivalent:** custom `TabControl` with owner-draw tabs
- **Tab:** padding 0 10 0 14, file icon + label + close X
- **Active tab:** top border 1.5px `--la-accent`, background `--la-tab-active`, text `--la-text-strong`
- **Inactive tab:** transparent background, text `--la-text-muted`
- **Dirty marker:** ` •` in `--la-warning`
- **States:** default, hover, active, with-close, dirty
- **Plus button:** 32px wide at end of tab strip → opens file dialog

### TabPage Body (per log)
- Vertical split: ChartToolbar (36px) + DelayChart (~50% remainder) + LogTable (rest)
- Right side: StatsSidebar (240px fixed)

## Chart Area

### ChartToolbar (36px)
- Left: bold "Delay over time" + monospace muted "· y-axis: ms · x-axis: timestamp"
- Right: range chip group [10s, 30s, 1m, all], each 3×9px padding
- Right: zoom buttons [−, +, ⤢] (24×24 each, border `--la-border`)
- Active range chip: background `--la-accent-soft`, text `--la-accent`

### DelayChart
- **Library:** ScottPlot 4.x (NuGet `ScottPlot.WinForms`)
- **Background:** `--la-bg`
- **Line color:** `--la-accent` (#5b8def dark / #3461d6 light)
- **P1 reference line:** dashed `--la-p1` (#ff8b6b dark / #c43c10 light), labeled "P1 248ms"
- **Median reference line:** dashed `--la-median`
- **Grid color:** `--la-grid`
- **Hover tooltip:** timestamp + delay_ms in monospace
- **Selected point:** larger marker, color `--la-accent`
- **Zoom controls:** wired to toolbar buttons + Ctrl+= / Ctrl+- / Ctrl+0

## Data Table

### LogTable
- **WinForms equivalent:** `DataGridView` (themed)
- **Columns:** # (line number), Timestamp (mono, 180px), Delay (ms, right-aligned), Severity dot, Raw line (mono, fill remaining)
- **Header:** background `--la-panel-elev`, text `--la-text-strong`, sortable indicator
- **Row hover:** `--la-row-hover`
- **Row selected:** `--la-row-selected`
- **P1 events:** delay cell text in `--la-p1`, bold; row marker dot in `--la-p1`
- **Median events:** small `--la-median` dot
- **Vertical scrollbar:** themed (`--la-scroll-thumb`)
- **Click header:** sort ascending/descending

## Stats Sidebar (240px wide)

### StatCard (hero variant)
- **Use:** P1 — low 1% tail
- **Layout:** label "P1 — low 1% tail" (xs uppercase muted) + value 44px (`--la-fs-stat-hero`) + unit "ms" (lg muted) + sub "worst 1% of delays — performance drops here"
- **Padding:** 14×14
- **Background:** `--la-panel-elev`
- **Border:** 1px `--la-border`, radius 6

### StatCard (small variant, 4 in 2×2 grid)
- Median, P95, P99, Events
- Value 28px (`--la-fs-stat`), label xs uppercase muted
- Median accent: value text in `--la-median`

### Span Block
- Label "SPAN" + monospace start + end + duration

### Distribution Block
- Label "DISTRIBUTION" + mini histogram (28 bars in SVG → render via custom Paint)
- Bar colors: `--la-accent` (default), `--la-median` (first bar), `--la-p1` (last 3 bars / tail)

## Status Bar (22px)

### StatusBar
- Background `--la-statusbar` (deep blue `#20467a` dark / `#2f5fbe` light)
- Foreground `--la-statusbar-fg`
- Left segments: status dot (parsed/parsing) + file path (mono)
- Right segments: event count + encoding + parser pattern (accent highlighted)
- Each segment 0×10 padding, height 22

## Empty State

### DropZone
- 560 wide, centered, dashed border `--la-border` 1.5px, radius 8
- Padding 40
- Upload icon (28px stroke), label "Drag a log file here, or click to browse"
- Hover state: border `--la-accent`, background `--la-accent-soft`
- Drag-active state: border `--la-accent` solid, scale 1.02

### Sample format block (under DropZone)
- Background `--la-panel-elev`, border `--la-border`, radius 6, padding 10×14
- Mono font, 3 sample lines with timestamp portion highlighted (`--la-accent` text on `--la-accent-soft` bg)

## Modal Dialogs

### Modal wrapper
- **WinForms equivalent:** `Form` with `FormBorderStyle.FixedDialog`, `StartPosition.CenterParent`
- Title bar (height 36): bold title + X close button
- Body padding 16/18
- Footer 60px tall: right-aligned buttons (Cancel / Action)
- Background `--la-bg`, body `--la-panel-elev` for nested cards
- Box shadow / border via Panel paint

### TimestampDialog
- 620 wide
- Header: instructions + file metadata (filename mono, line count)
- Lines block: 5 first lines, monospace, line numbers (faint, right-aligned, 22px wide), timestamp portion highlighted in `--la-accent`+`--la-accent-soft`
- Legend below: detected timestamp swatch + "Click Adjust to drag-select"
- Toggle row: "Show regex" (Toggle on) + power-user override
- Regex display: monospace input with syntax-colored regex, "matches 5/5" indicator (`--la-median` checkmark)
- Footer buttons: [Cancel ghost] [Adjust secondary] [Use this pattern primary + check icon]

### SettingsDialog
- 640 wide, 360+ tall
- Left tab rail (140px): General / Appearance / Cache (active = `--la-accent` left border 3px + bg `--la-row-selected`)
- Body: padding 18/22, gap 18
- Section: SectionLabel (xs uppercase muted) + content
- Theme section: 3 cards (Dark / Light / Match system), 130px wide each, with mini preview
- Active theme card: 1.5px `--la-accent` border + check icon
- Accent color row: 6 swatches (26×26 circles), selected has check + 2px text-strong border
- SettingRow: label + hint (left) + control (right)
- Footer: [Cancel ghost] [Apply primary]

### AboutDialog
- 460 wide
- Hero: app icon (56×56, accent-soft bg, accent fg) + name (22px bold) + version (mono muted)
- Description block (320 max-width)
- Info card: Repository / Licence / Runtime / Author rows
- Footer: [View licence ghost] [Close primary]

## Form Controls

### Button
- **Kinds:** primary, secondary, ghost
- **States:** default, hover, pressed, disabled
- **Primary:** bg `--la-accent`, text `--la-accent-fg`, hover `--la-accent-hover`, press `--la-accent-press`
- **Secondary:** bg `--la-panel-elev`, border `--la-border`, text `--la-text`
- **Ghost:** transparent bg, text `--la-text-muted`, hover bg `--la-row-hover`
- **Padding:** 8×16, font 13px, radius `--la-radius` (5px)

### Toggle
- 32×16 switch, knob 12×12, padding 2
- Off: bg `--la-input-border`
- On: bg `--la-accent`, knob right-aligned

### Dropdown / Select
- Border `--la-input-border`, bg `--la-input-bg`, padding 6×10
- Caret icon right side, color `--la-text-muted`
- Open menu: `--la-shadow-pop`, items hover `--la-row-hover`

### Text Input
- Border `--la-input-border`, focus border `--la-input-focus` 1.5px
- bg `--la-input-bg`, padding 6×10, mono optional

## Color Token Mapping (for ThemeColors.cs)

Exact values from `tokens.css` — these become `Color.FromArgb(...)` constants.

### Dark theme key tokens
- bg: #11141a · panel: #181c24 · panelElev: #1d222b
- tabstrip: #1a1e26 · tabActive: #11141a
- titlebar: #0e1116 · menubar: #14171e
- statusbar: #20467a · statusbarFg: #e5edf8
- border: #262c36 · borderSoft: #1f242d · divider: #232831
- text: #d6dae3 · textStrong: #f1f3f7 · textMuted: #7a8497 · textFaint: #545d6e
- accent: #5b8def · accentHover: #6d9bf7 · accentPress: #4a78d8
- p1: #ff8b6b · median: #59c2a8 · warning: #e8a86a · error: #ff7a72
- inputBg: #0d1015 · inputBorder: #2d3340

### Light theme key tokens
- bg: #fbfbfd · panel: #f1f3f6 · panelElev: #ffffff
- tabstrip: #eaecf0 · tabActive: #fbfbfd
- titlebar: #e3e6eb · menubar: #f1f3f6
- statusbar: #2f5fbe · statusbarFg: #ffffff
- border: #d8dce3 · borderSoft: #e4e7ec · divider: #e4e7ec
- text: #20242b · textStrong: #0c0e12 · textMuted: #6c7484 · textFaint: #9ba2b0
- accent: #3461d6 · accentHover: #4571e3 · accentPress: #2851bf
- p1: #c43c10 · median: #1a8a73 · warning: #b8721d · error: #b81f15
- inputBg: #ffffff · inputBorder: #c8ccd4

### Typography
- UI font: Segoe UI — 13px body, 18px headings, 11px muted, 12px sm, 14px md, 16px lg
- Mono font: Cascadia Code, Consolas fallback — 12px small
- Stat sizes: 28px regular, 44px hero
