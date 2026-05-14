using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using LogAnzlyzer.Storage;
using LogAnzlyzer.Theme;
using LogAnzlyzer.Updates;

namespace LogAnzlyzer.Forms
{
    // Non-blocking modal shown when UpdateChecker finds a newer release.
    // Three actions: open download page, skip this version, remind later.
    // "Skip this version" persists in app_settings so we don't re-prompt for the same release.
    public sealed class UpdateAvailableDialog : Form
    {
        private readonly ReleaseInfo _release;

        public UpdateAvailableDialog(ReleaseInfo release)
        {
            _release = release;
            Text = "Update available";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false; MaximizeBox = false; ShowIcon = false;
            ClientSize = new Size(480, 320);
            ApplyTheme();
            BuildContents();
        }

        private void BuildContents()
        {
            var t = ThemeManager.Current;

            var hero = new Label
            {
                Text = $"Update available  ·  {_release.TagName}",
                Font = Fonts.Heading, ForeColor = t.TextStrong,
                Top = 20, Left = 20, Width = 440, Height = 26, AutoSize = false,
            };
            Controls.Add(hero);

            var sub = new Label
            {
                Text = $"You're on v{UpdateChecker.CurrentVersion.ToString(3)}. Latest: {_release.TagName}",
                Font = Fonts.Tiny, ForeColor = t.TextMuted,
                Top = 50, Left = 20, Width = 440, Height = 16, AutoSize = false,
            };
            Controls.Add(sub);

            var notes = new TextBox
            {
                Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = t.PanelElev, ForeColor = t.Text, Font = Fonts.Body,
                Top = 78, Left = 20, Width = 440, Height = 170,
                Text = (_release.Body ?? "(no release notes)").Replace("\n", Environment.NewLine),
            };
            Controls.Add(notes);

            int btnY = ClientSize.Height - 50;
            var btnSkip = MakeBtn("Skip this version", btnY, "ghost");
            var btnLater = MakeBtn("Remind me later", btnY, "secondary");
            var btnOpen = MakeBtn("Open download page", btnY, "primary");

            btnOpen.Width = 170; btnLater.Width = 130; btnSkip.Width = 130;
            btnOpen.Left = ClientSize.Width - btnOpen.Width - 18;
            btnLater.Left = btnOpen.Left - btnLater.Width - 8;
            btnSkip.Left = 18;

            btnSkip.Click += (s, e) =>
            {
                CacheDatabase.SetSetting("update.skipped_version", _release.TagName);
                DialogResult = DialogResult.Cancel; Close();
            };
            btnLater.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            btnOpen.Click += (s, e) =>
            {
                try { Process.Start(_release.HtmlUrl); } catch { }
                DialogResult = DialogResult.OK; Close();
            };

            Controls.Add(btnSkip); Controls.Add(btnLater); Controls.Add(btnOpen);
            AcceptButton = btnOpen;
            CancelButton = btnLater;
        }

        private Button MakeBtn(string text, int top, string kind)
        {
            var t = ThemeManager.Current;
            var b = new Button { Text = text, Top = top, Width = 130, Height = 32, FlatStyle = FlatStyle.Flat };
            b.FlatAppearance.BorderSize = kind == "secondary" ? 1 : 0;
            b.FlatAppearance.BorderColor = t.Border;
            switch (kind)
            {
                case "ghost":     b.BackColor = Color.Transparent; b.ForeColor = t.TextMuted; break;
                case "secondary": b.BackColor = t.PanelElev; b.ForeColor = t.Text; break;
                default:          b.BackColor = t.Accent; b.ForeColor = t.AccentFg; break;
            }
            return b;
        }

        private void ApplyTheme()
        {
            var t = ThemeManager.Current;
            BackColor = t.Bg; ForeColor = t.Text;
        }
    }
}
