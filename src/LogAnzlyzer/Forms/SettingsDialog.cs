using System;
using System.Drawing;
using System.Windows.Forms;
using LogAnzlyzer.Storage;
using LogAnzlyzer.Theme;

namespace LogAnzlyzer.Forms
{
    public sealed class SettingsDialog : Form
    {
        public SettingsDialog()
        {
            Text = "Settings";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            Width = 660;
            Height = 460;
            MinimizeBox = false;
            MaximizeBox = false;
            ApplyTheme();
            ThemeManager.ThemeChanged += (s, e) => ApplyTheme();
            BuildContents();
        }

        private void BuildContents()
        {
            var t = ThemeManager.Current;

            // Tab rail (left, 140px)
            var rail = new Panel { Dock = DockStyle.Left, Width = 140, BackColor = t.Panel };
            string[] tabs = { "General", "Appearance", "Cache" };
            int activeTab = 1; // open on Appearance per design
            for (int i = 0; i < tabs.Length; i++)
            {
                int ii = i;
                var item = new Label
                {
                    Text = tabs[i],
                    Font = Fonts.Body,
                    ForeColor = i == activeTab ? t.TextStrong : t.Text,
                    BackColor = i == activeTab ? t.RowSelected : Color.Transparent,
                    Padding = new Padding(13, 8, 0, 8),
                    Top = 10 + i * 36,
                    Left = 0,
                    Width = 140,
                    Height = 32,
                    TextAlign = ContentAlignment.MiddleLeft,
                    BorderStyle = BorderStyle.None,
                };
                rail.Controls.Add(item);
                if (i == activeTab)
                {
                    var leftBar = new Panel { Top = item.Top, Left = 0, Width = 3, Height = item.Height, BackColor = t.Accent };
                    rail.Controls.Add(leftBar);
                }
            }
            Controls.Add(rail);

            // Body (Appearance tab content)
            var body = new Panel { Dock = DockStyle.Fill, BackColor = t.Bg, Padding = new Padding(22, 18, 22, 18) };
            Controls.Add(body);

            int y = 0;

            // Theme section
            var themeLabel = new Label { Text = "THEME", Font = Fonts.Tiny, ForeColor = t.TextMuted, Top = y, Left = 0, AutoSize = true };
            body.Controls.Add(themeLabel);
            y += 22;

            // 3 theme cards
            var cardDark = MakeThemeCard("Dark", isDark: true, isActive: ThemeManager.Current.Mode == ThemeMode.Dark, x: 0, y: y);
            var cardLight = MakeThemeCard("Light", isDark: false, isActive: ThemeManager.Current.Mode == ThemeMode.Light, x: 140, y: y);
            cardDark.Click += (s, e) => { ThemeManager.Set(ThemeMode.Dark); Close(); };
            cardLight.Click += (s, e) => { ThemeManager.Set(ThemeMode.Light); Close(); };
            body.Controls.Add(cardDark);
            body.Controls.Add(cardLight);
            y += cardDark.Height + 20;

            // Divider
            body.Controls.Add(new Panel { Top = y, Left = 0, Width = body.Width - 40, Height = 1, BackColor = t.BorderSoft });
            y += 14;

            // Settings rows
            AddRow(body, "Show stats sidebar", "Right panel with P1, median, P95/P99 values", true, ref y);
            AddRow(body, "Highlight P1 events in table", "Tail-latency rows get coral marker + bold delay", true, ref y);

            // Footer buttons
            var btnApply = new Button { Text = "Close", Top = Height - 70, Left = Width - 100, Width = 80, Height = 28, FlatStyle = FlatStyle.Flat };
            btnApply.BackColor = t.Accent; btnApply.ForeColor = t.AccentFg;
            btnApply.FlatAppearance.BorderSize = 0;
            btnApply.Click += (s, e) => Close();
            Controls.Add(btnApply);
            AcceptButton = btnApply;
        }

        private Panel MakeThemeCard(string label, bool isDark, bool isActive, int x, int y)
        {
            var t = ThemeManager.Current;
            var card = new Panel
            {
                Top = y, Left = x, Width = 130, Height = 96,
                BackColor = t.PanelElev,
                Cursor = Cursors.Hand,
            };
            card.Paint += (s, e) =>
            {
                var g = e.Graphics;
                using (var pen = new Pen(isActive ? t.Accent : t.Border, 1.5f))
                    g.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                // mini preview panel
                var prev = isDark ? Color.FromArgb(0x11, 0x14, 0x1a) : Color.FromArgb(0xfb, 0xfb, 0xfd);
                var prevText = isDark ? Color.FromArgb(0xd6, 0xda, 0xe3) : Color.FromArgb(0x20, 0x24, 0x2b);
                using (var b = new SolidBrush(prev)) g.FillRectangle(b, 8, 8, card.Width - 16, 56);
                using (var pen = new Pen(t.BorderSoft)) g.DrawRectangle(pen, 8, 8, card.Width - 16, 56);
                using (var b = new SolidBrush(t.Accent)) g.FillRectangle(b, 14, 18, 12, 4);
                using (var b = new SolidBrush(prevText)) { g.FillRectangle(b, 14, 26, 30, 2); g.FillRectangle(b, 14, 32, 22, 2); }
                using (var f = Fonts.Small)
                    TextRenderer.DrawText(g, label, f, new Point(10, 70), t.Text);
                if (isActive)
                {
                    using (var f = Fonts.Small)
                        TextRenderer.DrawText(g, "✓", f, new Point(card.Width - 24, 70), t.Accent);
                }
            };
            return card;
        }

        private void AddRow(Panel body, string label, string hint, bool on, ref int y)
        {
            var t = ThemeManager.Current;
            var lbl = new Label { Text = label, Font = Fonts.Body, ForeColor = t.Text, Top = y, Left = 0, AutoSize = true };
            body.Controls.Add(lbl);
            var sub = new Label { Text = hint, Font = Fonts.Tiny, ForeColor = t.TextMuted, Top = y + 18, Left = 0, AutoSize = true };
            body.Controls.Add(sub);
            var toggle = new CheckBox { Checked = on, Top = y, Left = body.Width - 80, Appearance = Appearance.Button, Width = 50, Height = 24, FlatStyle = FlatStyle.Flat };
            toggle.Text = on ? "ON" : "OFF";
            toggle.BackColor = on ? t.Accent : t.InputBorder;
            toggle.ForeColor = t.AccentFg;
            toggle.CheckedChanged += (s, e) =>
            {
                toggle.Text = toggle.Checked ? "ON" : "OFF";
                toggle.BackColor = toggle.Checked ? ThemeManager.Current.Accent : ThemeManager.Current.InputBorder;
            };
            body.Controls.Add(toggle);
            y += 50;
        }

        private void ApplyTheme()
        {
            var t = ThemeManager.Current;
            BackColor = t.Bg;
            ForeColor = t.Text;
        }
    }
}
