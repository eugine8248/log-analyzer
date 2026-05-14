using System;
using System.Drawing;
using System.Windows.Forms;
using LogAnzlyzer.Parsing;
using LogAnzlyzer.Theme;

namespace LogAnzlyzer.Forms
{
    // Modal that shows the first 5 lines of the log with the auto-detected
    // timestamp portion highlighted. User can confirm or click "Adjust" to
    // override the regex. Power-user: edit the regex directly.
    public sealed class TimestampDialog : Form
    {
        public string SelectedRegex { get; private set; }

        private readonly TimestampDetectionResult _det;
        private readonly TextBox _regexBox = new TextBox();

        public TimestampDialog(string fileName, TimestampDetectionResult det)
        {
            _det = det;
            SelectedRegex = det.Regex;

            Text = "Confirm timestamp position";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            MaximizeBox = false;
            Width = 640;
            Height = 460;
            ApplyTheme();
            ThemeManager.ThemeChanged += (s, e) => { ApplyTheme(); Invalidate(true); };

            BuildContents(fileName);
        }

        private void BuildContents(string fileName)
        {
            var t = ThemeManager.Current;

            var header = new Label
            {
                Text = "We auto-detected this timestamp. Confirm it matches every line.",
                Font = Fonts.Body,
                ForeColor = t.Text,
                AutoSize = false,
                Top = 16, Left = 18, Width = 600, Height = 20,
            };
            Controls.Add(header);

            var meta = new Label
            {
                Text = $"File  {fileName}  ·  showing first {_det.SampleLines.Count}",
                Font = Fonts.Tiny,
                ForeColor = t.TextMuted,
                AutoSize = false,
                Top = 38, Left = 18, Width = 600, Height = 16,
            };
            Controls.Add(meta);

            // Lines preview panel — custom-painted
            var preview = new LinesPreview(_det) { Top = 64, Left = 18, Width = 600, Height = 140 };
            Controls.Add(preview);

            var legend = new Label
            {
                Text = $"detected timestamp · matched {_det.Matched}/{_det.TotalSampled} lines",
                Font = Fonts.Tiny,
                ForeColor = t.TextMuted,
                AutoSize = false,
                Top = 212, Left = 18, Width = 600, Height = 16,
            };
            Controls.Add(legend);

            var regexLabel = new Label
            {
                Text = "Regex (power-user override)",
                Font = Fonts.Tiny,
                ForeColor = t.TextMuted,
                Top = 240, Left = 18, AutoSize = true,
            };
            Controls.Add(regexLabel);

            _regexBox.Text = _det.Regex;
            _regexBox.Font = Fonts.Mono;
            _regexBox.BackColor = t.InputBg;
            _regexBox.ForeColor = t.Text;
            _regexBox.BorderStyle = BorderStyle.FixedSingle;
            _regexBox.Top = 260; _regexBox.Left = 18; _regexBox.Width = 600;
            Controls.Add(_regexBox);

            // Buttons
            var btnCancel = new Button { Text = "Cancel", Top = 370, Left = 380, Width = 80, Height = 30, FlatStyle = FlatStyle.Flat };
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            ApplyButtonStyle(btnCancel, ghost: true);

            var btnAdjust = new Button { Text = "Adjust", Top = 370, Left = 466, Width = 60, Height = 30, FlatStyle = FlatStyle.Flat };
            btnAdjust.Click += (s, e) => MessageBox.Show(this, "Drag-select adjustment is not yet implemented in v0.1 — edit the regex directly below the preview for now.", "Adjust", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ApplyButtonStyle(btnAdjust, ghost: false, secondary: true);

            var btnUse = new Button { Text = "Use this pattern", Top = 370, Left = 532, Width = 90, Height = 30, FlatStyle = FlatStyle.Flat };
            btnUse.Click += (s, e) =>
            {
                SelectedRegex = _regexBox.Text;
                DialogResult = DialogResult.OK;
                Close();
            };
            ApplyButtonStyle(btnUse, ghost: false);

            Controls.Add(btnCancel); Controls.Add(btnAdjust); Controls.Add(btnUse);
            AcceptButton = btnUse;
            CancelButton = btnCancel;
        }

        private void ApplyTheme()
        {
            var t = ThemeManager.Current;
            BackColor = t.Bg;
            ForeColor = t.Text;
        }

        private void ApplyButtonStyle(Button b, bool ghost, bool secondary = false)
        {
            var t = ThemeManager.Current;
            b.FlatAppearance.BorderSize = secondary ? 1 : 0;
            b.FlatAppearance.BorderColor = t.Border;
            if (ghost)
            {
                b.BackColor = Color.Transparent;
                b.ForeColor = t.TextMuted;
            }
            else if (secondary)
            {
                b.BackColor = t.PanelElev;
                b.ForeColor = t.Text;
            }
            else
            {
                b.BackColor = t.Accent;
                b.ForeColor = t.AccentFg;
            }
        }

        // Custom panel that paints the 5 sample lines with highlighted timestamps.
        private sealed class LinesPreview : Panel
        {
            private readonly TimestampDetectionResult _det;
            public LinesPreview(TimestampDetectionResult det)
            {
                _det = det;
                DoubleBuffered = true;
                BorderStyle = BorderStyle.FixedSingle;
            }
            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                var g = e.Graphics;
                var t = ThemeManager.Current;
                using (var b = new SolidBrush(t.InputBg)) g.FillRectangle(b, ClientRectangle);

                int y = 6;
                using (var f = Fonts.Mono)
                {
                    for (int i = 0; i < _det.SampleLines.Count; i++)
                    {
                        var line = _det.SampleLines[i];
                        // Line number
                        TextRenderer.DrawText(g, (i + 1).ToString(), f, new Point(8, y), t.TextFaint);

                        int x = 36;
                        if (_det.StartIndex.HasValue && _det.EndIndex.HasValue
                            && _det.StartIndex.Value <= line.Length && _det.EndIndex.Value <= line.Length)
                        {
                            // before
                            int before = _det.StartIndex.Value;
                            string a = line.Substring(0, before);
                            int len = TextRenderer.MeasureText(g, a, f).Width;
                            TextRenderer.DrawText(g, a, f, new Point(x, y), t.Text);
                            x += len;

                            // highlighted timestamp
                            string b1 = line.Substring(before, _det.EndIndex.Value - before);
                            int blen = TextRenderer.MeasureText(g, b1, f).Width;
                            using (var hl = new SolidBrush(t.AccentSoft))
                                g.FillRectangle(hl, x - 1, y - 1, blen + 2, 18);
                            using (var pen = new Pen(t.Accent))
                                g.DrawRectangle(pen, x - 1, y - 1, blen + 2, 18);
                            TextRenderer.DrawText(g, b1, f, new Point(x, y), t.Accent);
                            x += blen;

                            // after
                            string c = line.Substring(_det.EndIndex.Value);
                            TextRenderer.DrawText(g, c, f, new Point(x, y), t.Text);
                        }
                        else
                        {
                            TextRenderer.DrawText(g, line, f, new Point(x, y), t.Text);
                        }
                        y += 22;
                    }
                }
            }
        }
    }
}
