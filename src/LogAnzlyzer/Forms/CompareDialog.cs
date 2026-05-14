using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using LogAnzlyzer.Storage;
using LogAnzlyzer.Theme;

namespace LogAnzlyzer.Forms
{
    // Lets the user pick 2+ logs to compare. Pre-fills with recent files
    // (checkboxes), with a Browse button to add more from disk.
    public sealed class CompareDialog : Form
    {
        public List<string> SelectedPaths { get; } = new List<string>();

        private readonly CheckedListBox _list = new CheckedListBox();

        public CompareDialog()
        {
            Text = "Compare logs";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false; MaximizeBox = false;
            ClientSize = new Size(560, 420);
            ApplyTheme();
            BuildContents();
        }

        private void BuildContents()
        {
            var t = ThemeManager.Current;

            var hdr = new Label
            {
                Text = "Pick 2 or more logs to overlay on a comparison chart.",
                Font = Fonts.Body, ForeColor = t.Text,
                Top = 16, Left = 18, Width = 530, Height = 20,
                AutoSize = false,
            };
            Controls.Add(hdr);

            _list.Top = 44; _list.Left = 18; _list.Width = 524; _list.Height = 280;
            _list.BorderStyle = BorderStyle.FixedSingle;
            _list.BackColor = t.InputBg;
            _list.ForeColor = t.Text;
            _list.Font = Fonts.Mono;
            _list.IntegralHeight = false;
            foreach (var p in CacheDatabase.GetRecentFiles(20))
            {
                _list.Items.Add(p);
            }
            Controls.Add(_list);

            var btnBrowse = MakeBtn("Browse…", 332, kind: "secondary");
            btnBrowse.Click += (s, e) =>
            {
                using (var ofd = new OpenFileDialog())
                {
                    ofd.Multiselect = true;
                    ofd.Filter = "Log files (*.log;*.txt)|*.log;*.txt|All files (*.*)|*.*";
                    if (ofd.ShowDialog(this) == DialogResult.OK)
                    {
                        foreach (var p in ofd.FileNames)
                        {
                            int idx = _list.Items.IndexOf(p);
                            if (idx < 0) idx = _list.Items.Add(p);
                            _list.SetItemChecked(idx, true);
                        }
                    }
                }
            };

            var btnCancel = MakeBtn("Cancel", ClientSize.Height - 50, kind: "ghost");
            var btnCompare = MakeBtn("Compare selected", ClientSize.Height - 50, kind: "primary");
            btnCancel.Width = 90; btnCompare.Width = 150;
            btnCompare.Left = ClientSize.Width - btnCompare.Width - 18;
            btnCancel.Left = btnCompare.Left - btnCancel.Width - 8;
            btnBrowse.Left = 18;

            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            btnCompare.Click += (s, e) =>
            {
                SelectedPaths.Clear();
                foreach (var item in _list.CheckedItems)
                {
                    var p = item.ToString();
                    if (File.Exists(p)) SelectedPaths.Add(p);
                }
                if (SelectedPaths.Count < 2)
                {
                    MessageBox.Show(this, "Pick at least 2 logs to compare.", "Compare", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                DialogResult = DialogResult.OK;
                Close();
            };

            Controls.Add(btnBrowse); Controls.Add(btnCancel); Controls.Add(btnCompare);
            AcceptButton = btnCompare;
            CancelButton = btnCancel;
        }

        private Button MakeBtn(string text, int top, string kind)
        {
            var t = ThemeManager.Current;
            var b = new Button { Text = text, Top = top, Width = 120, Height = 30, FlatStyle = FlatStyle.Flat };
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
