using System.Drawing;
using System.Windows.Forms;
using LogAnzlyzer.Parsing;
using LogAnzlyzer.Theme;
using System.Collections.Generic;

namespace LogAnzlyzer.Controls
{
    // Themed DataGridView showing parsed entries: line #, timestamp, delay (ms), severity dot, raw line.
    // Supports lazy raw-line lookup for streaming-parsed (large) logs.
    public sealed class LogTableGrid : DataGridView
    {
        private List<ParsedEntry> _entries = new List<ParsedEntry>();
        private ParsedLog _log;        // for lazy raw-line lookup when streaming

        public LogTableGrid()
        {
            Dock = DockStyle.Fill;
            ReadOnly = true;
            AllowUserToAddRows = false;
            AllowUserToDeleteRows = false;
            AllowUserToResizeRows = false;
            BorderStyle = BorderStyle.None;
            CellBorderStyle = DataGridViewCellBorderStyle.None;
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            RowHeadersVisible = false;
            EnableHeadersVisualStyles = false;
            BackgroundColor = Color.Black;
            SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            MultiSelect = false;
            VirtualMode = true;
            CellValueNeeded += OnCellValueNeeded;
            ColumnHeaderMouseClick += OnHeaderClick;
            BuildColumns();
            ApplyTheme();
            ThemeManager.ThemeChanged += (s, e) => { ApplyTheme(); Invalidate(); };

            var menu = new ContextMenuStrip();
            menu.Items.Add("Export to CSV...", null, (s, e) => ExportCsv());
            menu.Items.Add("Copy raw line", null, (s, e) => CopyRaw());
            ContextMenuStrip = menu;
        }

        private void ExportCsv()
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV file (*.csv)|*.csv";
                sfd.FileName = "log-data-" + System.DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".csv";
                if (sfd.ShowDialog(FindForm()) != DialogResult.OK) return;
                try
                {
                    using (var sw = new System.IO.StreamWriter(sfd.FileName, false, new System.Text.UTF8Encoding(true)))
                    {
                        sw.WriteLine("line_number,timestamp,delay_ms,severity,raw_line");
                        for (int i = 0; i < _entries.Count; i++)
                        {
                            var p = _entries[i];
                            string raw = (p.RawLine ?? (_log != null ? _log.GetRawLine(i) : "")).Replace("\"", "\"\"");
                            sw.WriteLine(string.Join(",",
                                p.LineNumber,
                                p.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                                p.DelayMs.HasValue ? p.DelayMs.Value.ToString("0.000") : "",
                                p.Severity ?? "",
                                "\"" + raw + "\""));
                        }
                    }
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = "/select,\"" + sfd.FileName + "\"",
                        UseShellExecute = true
                    });
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(FindForm(), "Failed to export:\n" + ex.Message, "Export error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void CopyRaw()
        {
            if (CurrentRow == null || CurrentRow.Index < 0 || CurrentRow.Index >= _entries.Count) return;
            try
            {
                var p = _entries[CurrentRow.Index];
                string raw = p.RawLine ?? (_log != null ? _log.GetRawLine(CurrentRow.Index) : "");
                Clipboard.SetText(raw);
            }
            catch { }
        }

        public void SetEntries(IEnumerable<ParsedEntry> entries)
        {
            _entries = new List<ParsedEntry>(entries);
            RowCount = _entries.Count;
            Invalidate();
        }

        public void SetLog(ParsedLog log)
        {
            _log = log;
            SetEntries(log?.Entries ?? new List<ParsedEntry>());
        }

        private void BuildColumns()
        {
            Columns.Clear();
            Columns.Add(new DataGridViewTextBoxColumn { Name = "ln", HeaderText = "#", Width = 56 });
            Columns.Add(new DataGridViewTextBoxColumn { Name = "ts", HeaderText = "Timestamp", Width = 200 });
            Columns.Add(new DataGridViewTextBoxColumn { Name = "delay", HeaderText = "Delay (ms)", Width = 110, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });
            Columns.Add(new DataGridViewTextBoxColumn { Name = "sev", HeaderText = "", Width = 24 });
            Columns.Add(new DataGridViewTextBoxColumn { Name = "raw", HeaderText = "Raw line", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        }

        private void OnCellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _entries.Count) return;
            var p = _entries[e.RowIndex];
            switch (e.ColumnIndex)
            {
                case 0: e.Value = p.LineNumber; break;
                case 1: e.Value = p.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"); break;
                case 2: e.Value = p.DelayMs.HasValue ? p.DelayMs.Value.ToString("0.0") : ""; break;
                case 3: e.Value = p.Severity == "p1" ? "●" : (p.Severity == "median" ? "·" : ""); break;
                case 4: e.Value = p.RawLine ?? (_log != null ? _log.GetRawLine(e.RowIndex) : ""); break;
            }
        }

        protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e)
        {
            base.OnCellPainting(e);
            if (e.RowIndex < 0 || e.RowIndex >= _entries.Count) return;
            var p = _entries[e.RowIndex];
            var t = ThemeManager.Current;
            if (p.Severity == "p1" && (e.ColumnIndex == 2 || e.ColumnIndex == 3))
            {
                e.CellStyle.ForeColor = t.P1;
                e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
            }
            else if (p.Severity == "median" && e.ColumnIndex == 3)
            {
                e.CellStyle.ForeColor = t.Median;
            }
        }

        private void OnHeaderClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            // Sort logic — basic: re-sort the in-memory list
            var col = e.ColumnIndex;
            switch (col)
            {
                case 0: _entries.Sort((a, b) => a.LineNumber.CompareTo(b.LineNumber)); break;
                case 1: _entries.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp)); break;
                case 2: _entries.Sort((a, b) => (a.DelayMs ?? 0).CompareTo(b.DelayMs ?? 0)); break;
            }
            Invalidate();
        }

        private void ApplyTheme()
        {
            var t = ThemeManager.Current;
            BackgroundColor = t.Bg;
            DefaultCellStyle.BackColor = t.Bg;
            DefaultCellStyle.ForeColor = t.Text;
            DefaultCellStyle.SelectionBackColor = t.AccentSoft;
            DefaultCellStyle.SelectionForeColor = t.TextStrong;
            DefaultCellStyle.Font = Fonts.Mono;
            ColumnHeadersDefaultCellStyle.BackColor = t.PanelElev;
            ColumnHeadersDefaultCellStyle.ForeColor = t.TextStrong;
            ColumnHeadersDefaultCellStyle.Font = Fonts.Small;
            GridColor = t.BorderSoft;
            RowsDefaultCellStyle.BackColor = t.Bg;
            AlternatingRowsDefaultCellStyle.BackColor = t.BgElev;
        }
    }
}
