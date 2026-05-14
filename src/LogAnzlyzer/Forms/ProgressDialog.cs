using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LogAnzlyzer.Parsing;
using LogAnzlyzer.Theme;

namespace LogAnzlyzer.Forms
{
    // Modal progress dialog used while parsing large log files.
    // Hosts a CancellationTokenSource that the work delegate respects.
    public sealed class ProgressDialog : Form
    {
        private readonly Label _lblTitle = new Label();
        private readonly Label _lblDetail = new Label();
        private readonly ProgressBar _bar = new ProgressBar();
        private readonly Button _btnCancel = new Button();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public CancellationToken Token => _cts.Token;
        public Exception WorkError { get; private set; }
        public object WorkResult { get; private set; }

        public ProgressDialog(string title)
        {
            Text = "Working...";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false; MaximizeBox = false; ShowIcon = false;
            ClientSize = new Size(440, 150);
            ApplyTheme();

            _lblTitle.Text = title; _lblTitle.Font = Fonts.Body; _lblTitle.AutoSize = false;
            _lblTitle.Top = 16; _lblTitle.Left = 18; _lblTitle.Width = 404; _lblTitle.Height = 20;
            _lblDetail.Text = "Starting…"; _lblDetail.Font = Fonts.Tiny; _lblDetail.AutoSize = false;
            _lblDetail.Top = 38; _lblDetail.Left = 18; _lblDetail.Width = 404; _lblDetail.Height = 16;
            _bar.Top = 64; _bar.Left = 18; _bar.Width = 404; _bar.Height = 16; _bar.Style = ProgressBarStyle.Continuous; _bar.Maximum = 1000;
            _btnCancel.Text = "Cancel"; _btnCancel.Top = 100; _btnCancel.Width = 90; _btnCancel.Height = 30;
            _btnCancel.Left = ClientSize.Width - _btnCancel.Width - 18;
            _btnCancel.FlatStyle = FlatStyle.Flat;
            _btnCancel.BackColor = ThemeManager.Current.PanelElev;
            _btnCancel.ForeColor = ThemeManager.Current.Text;
            _btnCancel.FlatAppearance.BorderColor = ThemeManager.Current.Border;
            _btnCancel.Click += (s, e) => { _cts.Cancel(); _btnCancel.Enabled = false; _lblDetail.Text = "Cancelling…"; };

            Controls.Add(_lblTitle); Controls.Add(_lblDetail); Controls.Add(_bar); Controls.Add(_btnCancel);
        }

        public IProgress<LogParseProgress> CreateProgressReporter()
        {
            var ui = SynchronizationContext.Current ?? new SynchronizationContext();
            return new Progress<LogParseProgress>(p =>
            {
                if (IsDisposed) return;
                int pct = p.TotalBytes > 0 ? (int)(p.BytesRead * 1000 / p.TotalBytes) : 0;
                _bar.Value = System.Math.Max(0, System.Math.Min(1000, pct));
                _lblDetail.Text = $"{p.LinesParsed:N0} entries · {p.BytesRead / 1024 / 1024} / {p.TotalBytes / 1024 / 1024} MB"
                    + (p.IsStreaming ? "  ·  streaming mode" : "");
            });
        }

        // Run a worker on a background thread; close dialog when done.
        public DialogResult RunAsync<T>(Func<CancellationToken, IProgress<LogParseProgress>, T> work)
        {
            var progress = CreateProgressReporter();
            Shown += (s, e) =>
            {
                Task.Run(() =>
                {
                    try { WorkResult = work(_cts.Token, progress); }
                    catch (OperationCanceledException) { /* user cancel */ }
                    catch (Exception ex) { WorkError = ex; }
                    finally { BeginInvoke((Action)(() => DialogResult = _cts.IsCancellationRequested ? DialogResult.Cancel : DialogResult.OK)); }
                });
            };
            return ShowDialog();
        }

        private void ApplyTheme()
        {
            var t = ThemeManager.Current;
            BackColor = t.Bg; ForeColor = t.Text;
            _lblTitle.ForeColor = t.TextStrong;
            _lblDetail.ForeColor = t.TextMuted;
        }
    }
}
