using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using LogAnzlyzer.Theme;

namespace LogAnzlyzer.Controls
{
    // Empty-state drop zone. Shows the upload icon, instruction text, and
    // sample-format box. Fires FileDropped when the user drops a file or
    // clicks to browse.
    public sealed class DropZonePanel : Panel
    {
        public event EventHandler<string> FileDropped;

        private bool _hover;
        private bool _dragActive;

        public DropZonePanel()
        {
            DoubleBuffered = true;
            AllowDrop = true;
            Cursor = Cursors.Hand;
            Height = 200;

            DragEnter += (s, e) => { _dragActive = true; Invalidate();
                e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None; };
            DragLeave += (s, e) => { _dragActive = false; Invalidate(); };
            DragDrop += (s, e) =>
            {
                _dragActive = false; Invalidate();
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    if (files.Length > 0) FileDropped?.Invoke(this, files[0]);
                }
            };
            MouseEnter += (s, e) => { _hover = true; Invalidate(); };
            MouseLeave += (s, e) => { _hover = false; Invalidate(); };
            Click += (s, e) =>
            {
                using (var ofd = new OpenFileDialog())
                {
                    ofd.Filter = "Log files (*.log;*.txt)|*.log;*.txt|All files (*.*)|*.*";
                    if (ofd.ShowDialog(FindForm()) == DialogResult.OK)
                        FileDropped?.Invoke(this, ofd.FileName);
                }
            };

            ThemeManager.ThemeChanged += (s, e) => Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var t = ThemeManager.Current;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);

            // Dashed rounded border
            using (var path = RoundedRect(rect, 8))
            {
                using (var bg = new SolidBrush(_dragActive || _hover ? t.AccentSoft : t.Bg))
                    g.FillPath(bg, path);
                using (var pen = new Pen(_dragActive ? t.Accent : t.Border, 1.5f))
                {
                    pen.DashStyle = _dragActive ? DashStyle.Solid : DashStyle.Dash;
                    pen.DashPattern = new float[] { 5, 4 };
                    g.DrawPath(pen, path);
                }
            }

            // Upload arrow icon (centered, 28px)
            int cx = Width / 2;
            int cy = Height / 2 - 22;
            using (var pen = new Pen(t.Accent, 2f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                // shaft
                g.DrawLine(pen, cx, cy + 12, cx, cy);
                // arrowhead
                g.DrawLine(pen, cx - 6, cy + 6, cx, cy);
                g.DrawLine(pen, cx + 6, cy + 6, cx, cy);
                // tray
                g.DrawLine(pen, cx - 10, cy + 14, cx - 10, cy + 18);
                g.DrawLine(pen, cx + 10, cy + 14, cx + 10, cy + 18);
                g.DrawLine(pen, cx - 10, cy + 18, cx + 10, cy + 18);
            }

            // Title text
            var title = "Drag a log file here, or click to browse";
            using (var f = Fonts.Heading)
            using (var br = new SolidBrush(t.TextStrong))
            {
                var sz = g.MeasureString(title, f);
                g.DrawString(title, f, br, cx - sz.Width / 2, cy + 30);
            }

            // Hint text
            var hint = ".log, .txt — any timestamped log";
            using (var f = Fonts.Small)
            using (var br = new SolidBrush(t.TextMuted))
            {
                var sz = g.MeasureString(hint, f);
                g.DrawString(hint, f, br, cx - sz.Width / 2, cy + 56);
            }
        }

        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            var p = new GraphicsPath();
            int d = radius * 2;
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
    }
}
