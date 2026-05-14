using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using LogAnzlyzer.Theme;

namespace LogAnzlyzer.Forms
{
    public sealed class AboutDialog : Form
    {
        public AboutDialog()
        {
            Text = "About LogAnzlyzer";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            Width = 480;
            Height = 380;
            MinimizeBox = false;
            MaximizeBox = false;
            ApplyTheme();
            BuildContents();
        }

        private void BuildContents()
        {
            var t = ThemeManager.Current;

            // Hero icon (rounded square + check chart icon)
            var iconPanel = new Panel { Top = 24, Left = (Width - 56) / 2, Width = 56, Height = 56, BackColor = Color.Transparent };
            iconPanel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var b = new SolidBrush(t.AccentSoft)) g.FillRectangle(b, 0, 0, 56, 56);
                using (var pen = new Pen(t.Accent, 2.5f)) g.DrawLines(pen, new[] { new Point(8, 36), new Point(20, 24), new Point(28, 28), new Point(48, 12) });
            };
            Controls.Add(iconPanel);

            var name = new Label { Text = "LogAnzlyzer", Font = Fonts.Heading, ForeColor = t.TextStrong, Top = 90, Left = 0, Width = Width, TextAlign = ContentAlignment.MiddleCenter };
            Controls.Add(name);
            var ver = new Label { Text = "v0.1.0 · build 2026.05.14", Font = Fonts.Mono, ForeColor = t.TextMuted, Top = 116, Left = 0, Width = Width, TextAlign = ContentAlignment.MiddleCenter };
            Controls.Add(ver);
            var desc = new Label { Text = "A Windows desktop tool for inspecting per-event delays\nand surfacing the P1 (low 1%) tail latency in any timestamped log.",
                Font = Fonts.Body, ForeColor = t.Text, Top = 142, Left = 40, Width = Width - 80, Height = 36, TextAlign = ContentAlignment.MiddleCenter };
            Controls.Add(desc);

            // Info card
            var info = new Panel { Top = 200, Left = 24, Width = Width - 48, Height = 100, BackColor = t.PanelElev, BorderStyle = BorderStyle.FixedSingle };
            int yy = 6;
            AddInfoRow(info, "Repository", "github.com/eugine8248/log-analyzer", t.Accent, ref yy, true);
            AddInfoRow(info, "Licence", "MIT", t.Text, ref yy);
            AddInfoRow(info, "Runtime", ".NET Framework 4.8 · WinForms", t.Text, ref yy);
            AddInfoRow(info, "Author", "open-source community", t.Text, ref yy);
            Controls.Add(info);

            var btn = new Button { Text = "Close", Top = Height - 60, Left = Width - 100, Width = 80, Height = 28, FlatStyle = FlatStyle.Flat };
            btn.BackColor = t.Accent; btn.ForeColor = t.AccentFg; btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) => Close();
            Controls.Add(btn);
            AcceptButton = btn;
        }

        private void AddInfoRow(Panel host, string label, string value, Color valueColor, ref int y, bool clickable = false)
        {
            var t = ThemeManager.Current;
            var l = new Label { Text = label, Font = Fonts.Small, ForeColor = t.TextMuted, Top = y, Left = 12, AutoSize = true };
            var v = new Label { Text = value, Font = clickable ? Fonts.Mono : Fonts.Body, ForeColor = valueColor, Top = y, Left = host.Width - 12, AutoSize = true };
            v.Left = host.Width - 14 - v.PreferredWidth;
            if (clickable)
            {
                v.Cursor = Cursors.Hand;
                v.Click += (s, e) => Process.Start("https://" + value);
            }
            host.Controls.Add(l);
            host.Controls.Add(v);
            y += 22;
        }

        private void ApplyTheme()
        {
            var t = ThemeManager.Current;
            BackColor = t.Bg;
            ForeColor = t.Text;
        }
    }
}
