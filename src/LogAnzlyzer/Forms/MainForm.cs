using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using LogAnzlyzer.Controls;
using LogAnzlyzer.Parsing;
using LogAnzlyzer.Stats;
using LogAnzlyzer.Theme;

namespace LogAnzlyzer.Forms
{
    public sealed class MainForm : Form
    {
        private readonly MenuStrip _menu = new MenuStrip();
        private readonly ClosableTabControl _tabs = new ClosableTabControl { Dock = DockStyle.Fill };
        private readonly DropZonePanel _emptyDrop = new DropZonePanel { Width = 560, Height = 200 };
        private readonly Panel _emptyHost = new Panel { Dock = DockStyle.Fill };
        private readonly ThemedStatusBar _status = new ThemedStatusBar();

        // Per-tab state
        private readonly Dictionary<TabPage, TabState> _tabState = new Dictionary<TabPage, TabState>();

        public MainForm()
        {
            Text = "LogAnzlyzer";
            Width = 1280;
            Height = 780;
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(900, 560);
            AllowDrop = true;
            DragEnter += (s, e) => e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
            DragDrop += (s, e) =>
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (var f in files) OpenLog(f);
            };

            BuildMenu();
            BuildEmptyState();

            Controls.Add(_tabs);
            Controls.Add(_emptyHost);
            Controls.Add(_status);
            Controls.Add(_menu);
            MainMenuStrip = _menu;

            _tabs.TabCloseRequested += (s, idx) => CloseTab(idx);
            _tabs.NewTabRequested += (s, e) => MenuOpen(s, e);

            ApplyTheme();
            ThemeManager.ThemeChanged += (s, e) => ApplyTheme();

            ShowEmptyState(true);
            UpdateStatus("Ready", null, null);
        }

        // ----- chrome -----
        private void BuildMenu()
        {
            var fileMenu = new ToolStripMenuItem("&File");
            fileMenu.DropDownItems.Add(MakeMenu("&Open...", Keys.Control | Keys.O, MenuOpen));
            fileMenu.DropDownItems.Add(MakeMenu("&Close Tab", Keys.Control | Keys.W, (s, e) => CloseTab(_tabs.SelectedIndex)));
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(MakeMenu("E&xit", Keys.None, (s, e) => Close()));

            var viewMenu = new ToolStripMenuItem("&View");
            viewMenu.DropDownItems.Add(MakeMenu("Toggle &Theme", Keys.Control | Keys.T, (s, e) => ThemeManager.Toggle()));

            var settingsMenu = new ToolStripMenuItem("&Settings");
            settingsMenu.DropDownItems.Add(MakeMenu("&Preferences...", Keys.Control | Keys.OemQuestion, (s, e) => new SettingsDialog().ShowDialog(this)));

            var helpMenu = new ToolStripMenuItem("&Help");
            helpMenu.DropDownItems.Add(MakeMenu("&About", Keys.F1, (s, e) => new AboutDialog().ShowDialog(this)));
            helpMenu.DropDownItems.Add(MakeMenu("&GitHub", Keys.None, (s, e) => Process.Start("https://github.com/eugine8248/log-analyzer")));

            _menu.Items.AddRange(new ToolStripItem[] { fileMenu, viewMenu, settingsMenu, helpMenu });
            _menu.Dock = DockStyle.Top;
            _menu.Renderer = new ThemedMenuRenderer();
        }

        private static ToolStripMenuItem MakeMenu(string text, Keys shortcut, EventHandler handler)
        {
            var item = new ToolStripMenuItem(text, null, handler);
            if (shortcut != Keys.None) item.ShortcutKeys = shortcut;
            return item;
        }

        private void BuildEmptyState()
        {
            _emptyHost.Controls.Add(_emptyDrop);
            _emptyHost.Resize += (s, e) =>
            {
                _emptyDrop.Left = (_emptyHost.Width - _emptyDrop.Width) / 2;
                _emptyDrop.Top = (_emptyHost.Height - _emptyDrop.Height) / 2;
            };
            _emptyDrop.FileDropped += (s, path) => OpenLog(path);
        }

        // ----- actions -----
        private void MenuOpen(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Log files (*.log;*.txt)|*.log;*.txt|All files (*.*)|*.*";
                if (ofd.ShowDialog(this) == DialogResult.OK) OpenLog(ofd.FileName);
            }
        }

        public void OpenLog(string path)
        {
            // 1) Detect timestamp position, ask user to confirm.
            var det = TimestampDetector.DetectFromFile(path, 5);
            string regex;
            using (var dlg = new TimestampDialog(Path.GetFileName(path), det))
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                regex = dlg.SelectedRegex;
            }

            // 2) Parse the file (sync for now — wrap in Task for big files).
            var parser = new LogParser(regex);
            ParsedLog log = null;
            UpdateStatus("Parsing…", path, null);
            Cursor = Cursors.WaitCursor;
            try
            {
                log = parser.Parse(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Failed to parse log:\n" + ex.Message, "Parse error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("Failed", path, null);
                return;
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            // 3) Compute stats and tag severities.
            var stats = StatsCalculator.Compute(log.Entries);
            StatsCalculator.TagSeverity(log.Entries, stats);
            var hist = StatsCalculator.Histogram(log.Entries);

            // 4) Build a new tab with chart + table + sidebar.
            var page = BuildTabPage(log, stats, hist);
            _tabs.TabPages.Add(page);
            _tabs.SelectedTab = page;
            ShowEmptyState(false);
            UpdateStatus("Parsed", path, log.Entries.Count);
        }

        private TabPage BuildTabPage(ParsedLog log, DelayStats stats, int[] hist)
        {
            var page = new TabPage(Path.GetFileName(log.FilePath));
            page.BackColor = ThemeManager.Current.Bg;

            // Right: stats sidebar
            var sidebar = new StatsSidebarPanel();
            sidebar.Update(stats, hist, log.Entries.First().Timestamp, log.Entries.Last().Timestamp);
            page.Controls.Add(sidebar);

            // Left main split: chart on top, table on bottom (resizable)
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 320,
            };

            var chart = new DelayChartPanel();
            chart.Render(log.Entries, stats);
            split.Panel1.Controls.Add(chart);

            var grid = new LogTableGrid();
            grid.SetEntries(log.Entries);
            split.Panel2.Controls.Add(grid);

            page.Controls.Add(split);

            _tabState[page] = new TabState { Log = log, Stats = stats, Histogram = hist };
            return page;
        }

        private void CloseTab(int idx)
        {
            if (idx < 0 || idx >= _tabs.TabPages.Count) return;
            var page = _tabs.TabPages[idx];
            _tabState.Remove(page);
            _tabs.TabPages.RemoveAt(idx);
            if (_tabs.TabPages.Count == 0)
            {
                ShowEmptyState(true);
                UpdateStatus("Ready", null, null);
            }
        }

        private void ShowEmptyState(bool show)
        {
            _emptyHost.Visible = show;
            _tabs.Visible = !show;
            if (show) _emptyHost.BringToFront();
            else _tabs.BringToFront();
        }

        private void UpdateStatus(string state, string path, int? events)
        {
            var t = ThemeManager.Current;
            var left = new List<ThemedStatusBar.Segment>
            {
                new ThemedStatusBar.Segment { Text = "● " + state },
            };
            if (path != null) left.Add(new ThemedStatusBar.Segment { Text = path, Mono = true });
            var right = new List<ThemedStatusBar.Segment>();
            if (events.HasValue) right.Add(new ThemedStatusBar.Segment { Text = events.Value.ToString("N0") + " events" });
            right.Add(new ThemedStatusBar.Segment { Text = "UTF-8" });
            right.Add(new ThemedStatusBar.Segment { Text = "pattern: " + TimestampDetector.DefaultDateTimeFormat, Accent = true });
            _status.Set(left.ToArray(), right.ToArray());
        }

        // ----- theming -----
        private void ApplyTheme()
        {
            var t = ThemeManager.Current;
            BackColor = t.Bg;
            ForeColor = t.Text;
            _menu.BackColor = t.Menubar;
            _menu.ForeColor = t.Text;
            _emptyHost.BackColor = t.Bg;
            foreach (TabPage p in _tabs.TabPages) p.BackColor = t.Bg;
            Invalidate(true);
        }

        private sealed class TabState
        {
            public ParsedLog Log;
            public DelayStats Stats;
            public int[] Histogram;
        }

        // Themed menu renderer — applies token colors to MenuStrip.
        private sealed class ThemedMenuRenderer : ToolStripProfessionalRenderer
        {
            public ThemedMenuRenderer() : base(new MenuColorTable()) { }
            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                e.TextColor = ThemeManager.Current.Text;
                base.OnRenderItemText(e);
            }
        }

        private sealed class MenuColorTable : ProfessionalColorTable
        {
            public override Color MenuItemSelected => ThemeManager.Current.AccentSoft;
            public override Color MenuItemSelectedGradientBegin => ThemeManager.Current.AccentSoft;
            public override Color MenuItemSelectedGradientEnd => ThemeManager.Current.AccentSoft;
            public override Color MenuItemBorder => ThemeManager.Current.Border;
            public override Color MenuStripGradientBegin => ThemeManager.Current.Menubar;
            public override Color MenuStripGradientEnd => ThemeManager.Current.Menubar;
            public override Color ToolStripDropDownBackground => ThemeManager.Current.PanelElev;
            public override Color ImageMarginGradientBegin => ThemeManager.Current.Menubar;
            public override Color ImageMarginGradientMiddle => ThemeManager.Current.Menubar;
            public override Color ImageMarginGradientEnd => ThemeManager.Current.Menubar;
        }
    }
}
