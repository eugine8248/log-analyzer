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
            ClientSize = new Size(720, 460);
            MinimizeBox = false;
            MaximizeBox = false;
            ApplyTheme();
            ThemeManager.ThemeChanged += (s, e) => ApplyTheme();
            BuildContents();
        }

        private void BuildContents()
        {
            var t = ThemeManager.Current;

            // Footer first (Dock=Bottom must be added BEFORE Fill children to claim space)
            var footer = new Panel { Dock = DockStyle.Bottom, Height = 56, BackColor = t.Bg };
            var btnApply = new Button
            {
                Text = "Close",
                Width = 88, Height = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = t.Accent, ForeColor = t.AccentFg,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
            };
            btnApply.FlatAppearance.BorderSize = 0;
            btnApply.Click += (s, e) => Close();
            btnApply.Top = 14;
            btnApply.Left = footer.Width - btnApply.Width - 18;
            footer.Resize += (s, e) => btnApply.Left = footer.Width - btnApply.Width - 18;
            footer.Controls.Add(btnApply);
            Controls.Add(footer);
            AcceptButton = btnApply;

            // Tab rail (Dock=Left)
            var rail = new Panel { Dock = DockStyle.Left, Width = 150, BackColor = t.Panel };
            string[] tabs = { "General", "Appearance", "Cache" };
            int activeTab = 1;
            for (int i = 0; i < tabs.Length; i++)
            {
                var item = new Label
                {
                    Text = tabs[i],
                    Font = Fonts.Body,
                    ForeColor = i == activeTab ? t.TextStrong : t.Text,
                    BackColor = i == activeTab ? t.RowSelected : Color.Transparent,
                    Padding = new Padding(16, 8, 0, 8),
                    Top = 12 + i * 36,
                    Left = 0,
                    Width = 150,
                    Height = 32,
                    TextAlign = ContentAlignment.MiddleLeft,
                };
                rail.Controls.Add(item);
                if (i == activeTab)
                {
                    var leftBar = new Panel { Top = item.Top, Left = 0, Width = 3, Height = item.Height, BackColor = t.Accent };
                    rail.Controls.Add(leftBar);
                    leftBar.BringToFront();
                }
            }
            Controls.Add(rail);

            // Body (Dock=Fill — must be added LAST so it picks up remaining space cleanly)
            var body = new Panel { Dock = DockStyle.Fill, BackColor = t.Bg, Padding = new Padding(24, 20, 24, 20) };
            Controls.Add(body);

            int y = 0;

            // ---- THEME section
            body.Controls.Add(MakeSectionLabel("THEME", y));
            y += 24;

            var cardDark  = MakeThemeCard("Dark",  isDark: true,  isActive: ThemeManager.Current.Mode == ThemeMode.Dark,  x: 0,   y: y);
            var cardLight = MakeThemeCard("Light", isDark: false, isActive: ThemeManager.Current.Mode == ThemeMode.Light, x: 152, y: y);
            cardDark.Click  += (s, e) => { ThemeManager.Set(ThemeMode.Dark);  Close(); };
            cardLight.Click += (s, e) => { ThemeManager.Set(ThemeMode.Light); Close(); };
            body.Controls.Add(cardDark);
            body.Controls.Add(cardLight);
            y += 110;

            // Divider — anchored to fill width
            var div = new Panel { Top = y, Left = 0, Height = 1, BackColor = t.BorderSoft, Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top };
            div.Width = body.ClientSize.Width - body.Padding.Horizontal;
            body.Controls.Add(div);
            body.Resize += (s, e) => div.Width = body.ClientSize.Width - body.Padding.Horizontal;
            y += 16;

            // ---- BEHAVIOR section
            body.Controls.Add(MakeSectionLabel("BEHAVIOR", y));
            y += 24;

            AddRow(body, ref y, "Show stats sidebar",         "Right panel with P1, median, P95/P99 values", on: true);
            AddRow(body, ref y, "Highlight P1 events in table","Tail-latency rows get coral marker + bold delay", on: true);
            AddRow(body, ref y, "Datetime format",              "Displayed in tooltips and the status bar",        on: false, asInfo: "yyyy-MM-dd HH:mm:ss.fff");
        }

        // -------- helpers --------

        private Label MakeSectionLabel(string text, int y)
        {
            return new Label
            {
                Text = text,
                Font = Fonts.Tiny,
                ForeColor = ThemeManager.Current.TextMuted,
                Top = y, Left = 0, AutoSize = true,
            };
        }

        private Panel MakeThemeCard(string label, bool isDark, bool isActive, int x, int y)
        {
            var t = ThemeManager.Current;
            var card = new Panel
            {
                Top = y, Left = x, Width = 140, Height = 96,
                BackColor = t.PanelElev,
                Cursor = Cursors.Hand,
            };
            card.Paint += (s, e) =>
            {
                var g = e.Graphics;
                using (var pen = new Pen(isActive ? t.Accent : t.Border, 1.5f))
                    g.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                var prev = isDark ? Color.FromArgb(0x11, 0x14, 0x1a) : Color.FromArgb(0xfb, 0xfb, 0xfd);
                var prevText = isDark ? Color.FromArgb(0xd6, 0xda, 0xe3) : Color.FromArgb(0x20, 0x24, 0x2b);
                using (var b = new SolidBrush(prev)) g.FillRectangle(b, 8, 8, card.Width - 16, 56);
                using (var pen = new Pen(t.BorderSoft)) g.DrawRectangle(pen, 8, 8, card.Width - 16, 56);
                using (var b = new SolidBrush(t.Accent)) g.FillRectangle(b, 14, 18, 14, 4);
                using (var b = new SolidBrush(prevText)) { g.FillRectangle(b, 14, 26, 36, 2); g.FillRectangle(b, 14, 32, 24, 2); }
                using (var f = Fonts.Small)
                    TextRenderer.DrawText(g, label, f, new Point(10, 70), t.Text);
                if (isActive)
                {
                    using (var f = Fonts.Small)
                    {
                        var sz = TextRenderer.MeasureText("✓", f);
                        TextRenderer.DrawText(g, "✓", f, new Point(card.Width - sz.Width - 10, 70), t.Accent);
                    }
                }
            };
            return card;
        }

        // Settings row — proper anchoring so labels never clip and toggles stay glued to the right.
        private void AddRow(Panel host, ref int y, string label, string hint, bool on, string asInfo = null)
        {
            var t = ThemeManager.Current;

            var row = new Panel
            {
                Top = y, Left = 0, Height = 48, BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            };
            row.Width = host.ClientSize.Width - host.Padding.Horizontal;
            host.Resize += (s, e) => row.Width = host.ClientSize.Width - host.Padding.Horizontal;

            var lbl = new Label
            {
                Text = label, Font = Fonts.Body, ForeColor = t.Text,
                Top = 0, Left = 0, AutoSize = false, Height = 20,
                TextAlign = ContentAlignment.MiddleLeft,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            };
            lbl.Width = row.Width - 110; // reserve space for toggle/info on the right
            row.Resize += (s, e) => lbl.Width = row.Width - 110;
            row.Controls.Add(lbl);

            var sub = new Label
            {
                Text = hint, Font = Fonts.Tiny, ForeColor = t.TextMuted,
                Top = 22, Left = 0, AutoSize = false, Height = 18,
                TextAlign = ContentAlignment.MiddleLeft,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            };
            sub.Width = row.Width - 110;
            row.Resize += (s, e) => sub.Width = row.Width - 110;
            row.Controls.Add(sub);

            if (asInfo != null)
            {
                var info = new Label
                {
                    Text = asInfo, Font = Fonts.Mono, ForeColor = t.Text,
                    Top = 12, Width = 100, Height = 22,
                    TextAlign = ContentAlignment.MiddleRight,
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                };
                info.Left = row.Width - info.Width;
                row.Resize += (s, e) => info.Left = row.Width - info.Width;
                row.Controls.Add(info);
            }
            else
            {
                var toggle = new CheckBox
                {
                    Checked = on, Top = 12, Width = 50, Height = 24,
                    Appearance = Appearance.Button, FlatStyle = FlatStyle.Flat,
                    Text = on ? "ON" : "OFF",
                    BackColor = on ? t.Accent : t.InputBorder,
                    ForeColor = t.AccentFg,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                };
                toggle.FlatAppearance.BorderSize = 0;
                toggle.Left = row.Width - toggle.Width;
                row.Resize += (s, e) => toggle.Left = row.Width - toggle.Width;
                toggle.CheckedChanged += (s, e) =>
                {
                    toggle.Text = toggle.Checked ? "ON" : "OFF";
                    toggle.BackColor = toggle.Checked ? ThemeManager.Current.Accent : ThemeManager.Current.InputBorder;
                };
                row.Controls.Add(toggle);
            }

            host.Controls.Add(row);
            y += row.Height + 8;
        }

        private void ApplyTheme()
        {
            var t = ThemeManager.Current;
            BackColor = t.Bg;
            ForeColor = t.Text;
        }
    }
}
