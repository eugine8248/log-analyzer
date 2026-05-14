using System;
using System.Drawing;
using System.Windows.Forms;
using LogAnzlyzer.Parsing;
using LogAnzlyzer.Theme;

namespace LogAnzlyzer.Forms
{
    // Modal that shows the first 5 lines of the log with the auto-detected
    // timestamp portion highlighted. User can confirm, click "Adjust" to
    // enter drag-select mode (click and drag on any line to pick the
    // timestamp bounds), or directly edit the regex for power users.
    public sealed class TimestampDialog : Form
    {
        public string SelectedRegex { get; private set; }

        private readonly TimestampDetectionResult _det;
        private readonly LinesPreview _preview;
        private readonly TextBox _regexBox = new TextBox();
        private readonly Label _legend = new Label();
        private bool _adjustMode;

        public TimestampDialog(string fileName, TimestampDetectionResult det)
        {
            _det = det;
            SelectedRegex = det.Regex;

            Text = "Confirm timestamp position";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            MaximizeBox = false;
            ClientSize = new Size(640, 510);
            ApplyTheme();
            ThemeManager.ThemeChanged += (s, e) => { ApplyTheme(); Invalidate(true); };

            _preview = new LinesPreview(_det) { Top = 64, Left = 18, Width = 600, Height = 140 };
            _preview.BoundsAdjusted += OnBoundsAdjusted;

            BuildContents(fileName);
        }

        private void BuildContents(string fileName)
        {
            var t = ThemeManager.Current;

            var header = new Label
            {
                Text = "We auto-detected this timestamp. Confirm it matches every line, or click Adjust to drag-select.",
                Font = Fonts.Body, ForeColor = t.Text,
                Top = 16, Left = 18, Width = 604, Height = 38,
                AutoSize = false,
            };
            Controls.Add(header);

            var meta = new Label
            {
                Text = $"File  {fileName}  ·  showing first {_det.SampleLines.Count}",
                Font = Fonts.Tiny, ForeColor = t.TextMuted,
                Top = 44, Left = 18, Width = 604, Height = 16,
                AutoSize = false,
            };
            Controls.Add(meta);

            Controls.Add(_preview);

            _legend.Text = $"Detected timestamp · matched {_det.Matched}/{_det.TotalSampled} lines";
            _legend.Font = Fonts.Tiny;
            _legend.ForeColor = t.TextMuted;
            _legend.Top = 212; _legend.Left = 18; _legend.Width = 604; _legend.Height = 16;
            _legend.AutoSize = false;
            Controls.Add(_legend);

            // Regex library picker — fills the regex box from a known preset.
            var libLabel = new Label
            {
                Text = "Pattern library",
                Font = Fonts.Tiny, ForeColor = t.TextMuted,
                Top = 240, Left = 18, AutoSize = true,
            };
            Controls.Add(libLabel);

            var libCombo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Top = 258, Left = 18, Width = 280,
                Font = Fonts.Body,
                BackColor = t.InputBg, ForeColor = t.Text, FlatStyle = FlatStyle.Flat,
            };
            libCombo.Items.Add("(custom — use the regex below)");
            foreach (var p in Parsing.RegexLibrary.Presets)
                libCombo.Items.Add(p.Name);
            libCombo.SelectedIndex = 0;
            libCombo.SelectedIndexChanged += (s, e) =>
            {
                int idx = libCombo.SelectedIndex - 1;
                if (idx < 0) return;
                var preset = Parsing.RegexLibrary.Presets[idx];
                _regexBox.Text = preset.Pattern;
                SelectedRegex = preset.Pattern;
            };
            Controls.Add(libCombo);

            var regexLabel = new Label
            {
                Text = "Regex (power-user override)",
                Font = Fonts.Tiny, ForeColor = t.TextMuted,
                Top = 290, Left = 18, AutoSize = true,
            };
            Controls.Add(regexLabel);

            _regexBox.Text = _det.Regex;
            _regexBox.Font = Fonts.Mono;
            _regexBox.BackColor = t.InputBg;
            _regexBox.ForeColor = t.Text;
            _regexBox.BorderStyle = BorderStyle.FixedSingle;
            _regexBox.Top = 310; _regexBox.Left = 18; _regexBox.Width = 604;
            Controls.Add(_regexBox);

            // Buttons (footer)
            int btnY = ClientSize.Height - 60;
            var btnCancel = new Button { Text = "Cancel", Top = btnY, Width = 90, Height = 32, FlatStyle = FlatStyle.Flat };
            ApplyButtonStyle(btnCancel, kind: "ghost");
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            var btnAdjust = new Button { Text = "Adjust", Top = btnY, Width = 90, Height = 32, FlatStyle = FlatStyle.Flat };
            ApplyButtonStyle(btnAdjust, kind: "secondary");
            btnAdjust.Click += (s, e) => ToggleAdjust(btnAdjust);

            var btnUse = new Button { Text = "Use this pattern", Top = btnY, Width = 140, Height = 32, FlatStyle = FlatStyle.Flat };
            ApplyButtonStyle(btnUse, kind: "primary");
            btnUse.Click += (s, e) =>
            {
                SelectedRegex = _regexBox.Text;
                DialogResult = DialogResult.OK;
                Close();
            };

            // right-align footer buttons
            btnUse.Left    = ClientSize.Width - 140 - 18;
            btnAdjust.Left = btnUse.Left - 90 - 8;
            btnCancel.Left = btnAdjust.Left - 90 - 8;

            Controls.Add(btnCancel); Controls.Add(btnAdjust); Controls.Add(btnUse);
            AcceptButton = btnUse;
            CancelButton = btnCancel;
        }

        private void ToggleAdjust(Button btn)
        {
            _adjustMode = !_adjustMode;
            _preview.AdjustMode = _adjustMode;
            btn.Text = _adjustMode ? "Done" : "Adjust";
            _legend.Text = _adjustMode
                ? "Drag across any line to select the timestamp bounds. Pattern updates live."
                : $"Detected timestamp · matched {_det.Matched}/{_det.TotalSampled} lines";
            _legend.ForeColor = _adjustMode ? ThemeManager.Current.Accent : ThemeManager.Current.TextMuted;
        }

        private void OnBoundsAdjusted(int start, int end, string sourceLine)
        {
            if (start >= end || string.IsNullOrEmpty(sourceLine)) return;
            _det.StartIndex = start;
            _det.EndIndex = end;
            string regex = TimestampDetector.BuildRegexForBounds(sourceLine, start, end);
            _det.Regex = regex;
            _regexBox.Text = regex;
            SelectedRegex = regex;
        }

        private void ApplyTheme()
        {
            var t = ThemeManager.Current;
            BackColor = t.Bg;
            ForeColor = t.Text;
        }

        private void ApplyButtonStyle(Button b, string kind)
        {
            var t = ThemeManager.Current;
            b.FlatAppearance.BorderSize = kind == "secondary" ? 1 : 0;
            b.FlatAppearance.BorderColor = t.Border;
            switch (kind)
            {
                case "ghost":     b.BackColor = Color.Transparent; b.ForeColor = t.TextMuted; break;
                case "secondary": b.BackColor = t.PanelElev; b.ForeColor = t.Text; break;
                default:          b.BackColor = t.Accent; b.ForeColor = t.AccentFg; break;
            }
        }

        // Custom panel that paints the 5 sample lines with a highlighted timestamp.
        // In Adjust mode, the user can click-and-drag on any line to redefine the bounds.
        private sealed class LinesPreview : Panel
        {
            public bool AdjustMode { get; set; }
            public event Action<int, int, string> BoundsAdjusted;

            private readonly TimestampDetectionResult _det;
            private const int LeftGutter = 36;   // line number column
            private const int RowHeight = 22;
            private const int FirstRowY = 6;

            private bool _dragging;
            private int _dragStartChar;
            private int _dragRowIdx;

            public LinesPreview(TimestampDetectionResult det)
            {
                _det = det;
                DoubleBuffered = true;
                BorderStyle = BorderStyle.FixedSingle;
            }

            // -------- mouse: hit-test → row index + char column --------
            protected override void OnMouseDown(MouseEventArgs e)
            {
                if (!AdjustMode || e.Button != MouseButtons.Left) return;
                int row = (e.Y - FirstRowY) / RowHeight;
                if (row < 0 || row >= _det.SampleLines.Count) return;
                int ch = HitTestChar(_det.SampleLines[row], e.X);
                if (ch < 0) return;
                _dragging = true;
                _dragRowIdx = row;
                _dragStartChar = ch;
                _det.StartIndex = ch;
                _det.EndIndex = ch + 1;
                Cursor = Cursors.IBeam;
                Invalidate();
            }
            protected override void OnMouseMove(MouseEventArgs e)
            {
                if (!_dragging) return;
                int ch = HitTestChar(_det.SampleLines[_dragRowIdx], e.X);
                if (ch < 0) return;
                int s = System.Math.Min(_dragStartChar, ch);
                int en = System.Math.Max(_dragStartChar, ch) + 1;
                _det.StartIndex = s;
                _det.EndIndex = en;
                Invalidate();
            }
            protected override void OnMouseUp(MouseEventArgs e)
            {
                if (!_dragging) return;
                _dragging = false;
                Cursor = Cursors.Default;
                if (_det.StartIndex.HasValue && _det.EndIndex.HasValue)
                {
                    BoundsAdjusted?.Invoke(_det.StartIndex.Value, _det.EndIndex.Value, _det.SampleLines[_dragRowIdx]);
                }
            }

            // Hit-test: find character index whose painted glyph contains x.
            // Uses TextRenderer.MeasureText with the same font as paint.
            private int HitTestChar(string line, int x)
            {
                if (x < LeftGutter) return -1;
                int rel = x - LeftGutter;
                using (var f = Fonts.Mono)
                using (var g = CreateGraphics())
                {
                    int prevW = 0;
                    for (int i = 1; i <= line.Length; i++)
                    {
                        int w = TextRenderer.MeasureText(g, line.Substring(0, i), f, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding).Width;
                        if (rel <= (prevW + w) / 2) return i - 1;
                        prevW = w;
                    }
                    return line.Length - 1;
                }
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                var g = e.Graphics;
                var t = ThemeManager.Current;
                using (var b = new SolidBrush(t.InputBg)) g.FillRectangle(b, ClientRectangle);

                int y = FirstRowY;
                using (var f = Fonts.Mono)
                {
                    for (int i = 0; i < _det.SampleLines.Count; i++)
                    {
                        var line = _det.SampleLines[i];
                        TextRenderer.DrawText(g, (i + 1).ToString(), f, new Point(8, y), t.TextFaint, TextFormatFlags.NoPadding);

                        int x = LeftGutter;
                        if (_det.StartIndex.HasValue && _det.EndIndex.HasValue
                            && _det.StartIndex.Value <= line.Length && _det.EndIndex.Value <= line.Length)
                        {
                            int s = _det.StartIndex.Value;
                            int en = _det.EndIndex.Value;

                            string before = line.Substring(0, s);
                            int beforeW = TextRenderer.MeasureText(g, before, f, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding).Width;
                            TextRenderer.DrawText(g, before, f, new Point(x, y), t.Text, TextFormatFlags.NoPadding);
                            x += beforeW;

                            string sel = line.Substring(s, en - s);
                            int selW = TextRenderer.MeasureText(g, sel, f, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding).Width;
                            // Only highlight if this row matches detection's row OR if not in adjust mode (apply detection to all)
                            if (!AdjustMode || i == _dragRowIdx || _dragRowIdx == 0 && !_dragging)
                            {
                                using (var hl = new SolidBrush(t.AccentSoft))
                                    g.FillRectangle(hl, x - 1, y - 1, selW + 2, RowHeight - 4);
                                using (var pen = new Pen(t.Accent))
                                    g.DrawRectangle(pen, x - 1, y - 1, selW + 2, RowHeight - 4);
                            }
                            TextRenderer.DrawText(g, sel, f, new Point(x, y), t.Accent, TextFormatFlags.NoPadding);
                            x += selW;

                            string after = line.Substring(en);
                            TextRenderer.DrawText(g, after, f, new Point(x, y), t.Text, TextFormatFlags.NoPadding);
                        }
                        else
                        {
                            TextRenderer.DrawText(g, line, f, new Point(x, y), t.Text, TextFormatFlags.NoPadding);
                        }
                        y += RowHeight;
                    }
                }

                if (AdjustMode)
                {
                    using (var pen = new Pen(t.Accent, 1.5f))
                        g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
                }
            }
        }
    }
}
