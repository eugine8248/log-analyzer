using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using LogAnzlyzer.Theme;

namespace LogAnzlyzer.Controls
{
    // 32px-tall custom titlebar matching the Claude Design pack:
    // logo (accent square + checkmark) + title text + themed min/max/close buttons.
    // Window dragging happens via WM_NCHITTEST in MainForm (this control's hit zone
    // returns HTCAPTION). Click handlers here drive minimize / maximize / close.
    public sealed class CustomTitleBar : Panel
    {
        public string Title
        {
            get => _title;
            set { _title = value ?? ""; Invalidate(); }
        }
        private string _title = "LogAnzlyzer";

        public Form HostForm { get; set; }   // set by MainForm

        // Window control button rects (right edge, computed each Paint)
        private Rectangle _btnMin, _btnMax, _btnClose;
        private int _hoverBtn = -1;   // 0 = min, 1 = max, 2 = close

        public CustomTitleBar()
        {
            DoubleBuffered = true;
            Height = 32;
            Dock = DockStyle.Top;
            ThemeManager.ThemeChanged += (s, e) => Invalidate();
        }

        public bool ContainsCaptionPoint(Point screenPt)
        {
            // True if a screen point falls within the draggable strip
            // (i.e. the titlebar minus the window-control buttons).
            var local = PointToClient(screenPt);
            if (!ClientRectangle.Contains(local)) return false;
            return !(_btnMin.Contains(local) || _btnMax.Contains(local) || _btnClose.Contains(local));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            int prev = _hoverBtn;
            _hoverBtn = _btnMin.Contains(e.Location) ? 0
                      : _btnMax.Contains(e.Location) ? 1
                      : _btnClose.Contains(e.Location) ? 2
                      : -1;
            if (_hoverBtn != prev) Invalidate();
        }
        protected override void OnMouseLeave(EventArgs e) { _hoverBtn = -1; Invalidate(); }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (HostForm == null) return;
            if (_btnMin.Contains(e.Location)) HostForm.WindowState = FormWindowState.Minimized;
            else if (_btnMax.Contains(e.Location))
                HostForm.WindowState = HostForm.WindowState == FormWindowState.Maximized
                    ? FormWindowState.Normal
                    : FormWindowState.Maximized;
            else if (_btnClose.Contains(e.Location)) HostForm.Close();
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            if (HostForm == null) return;
            if (ContainsCaptionPoint(PointToScreen(e.Location)))
                HostForm.WindowState = HostForm.WindowState == FormWindowState.Maximized
                    ? FormWindowState.Normal
                    : FormWindowState.Maximized;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            var t = ThemeManager.Current;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var b = new SolidBrush(t.Titlebar)) g.FillRectangle(b, ClientRectangle);
            using (var p = new Pen(t.BorderSoft)) g.DrawLine(p, 0, Height - 1, Width, Height - 1);

            // Logo: 14×14 rounded-square outline + checkmark, accent color
            int logoX = 12, logoY = (Height - 14) / 2;
            using (var pen = new Pen(t.Accent, 1.4f) { LineJoin = LineJoin.Round })
            {
                g.DrawRectangle(pen, logoX, logoY, 13, 13);
                g.DrawLines(pen, new[] {
                    new PointF(logoX + 3,  logoY + 9),
                    new PointF(logoX + 6,  logoY + 6),
                    new PointF(logoX + 8,  logoY + 7.5f),
                    new PointF(logoX + 11, logoY + 4),
                });
            }

            // Title text
            using (var f = Fonts.Small)
            {
                TextRenderer.DrawText(g, _title, f,
                    new Rectangle(logoX + 22, 0, Width - 200, Height),
                    t.TextMuted,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }

            // Window control buttons (46px wide each, right-aligned)
            int btnW = 46;
            _btnClose = new Rectangle(Width - btnW, 0, btnW, Height);
            _btnMax   = new Rectangle(Width - btnW * 2, 0, btnW, Height);
            _btnMin   = new Rectangle(Width - btnW * 3, 0, btnW, Height);
            DrawWinBtn(g, t, _btnMin, 0, "min");
            DrawWinBtn(g, t, _btnMax, 1, HostForm != null && HostForm.WindowState == FormWindowState.Maximized ? "restore" : "max");
            DrawWinBtn(g, t, _btnClose, 2, "close");
        }

        private void DrawWinBtn(Graphics g, ThemeColors t, Rectangle r, int idx, string kind)
        {
            bool hover = _hoverBtn == idx;
            if (hover)
            {
                Color hoverBg = (idx == 2) ? Color.FromArgb(232, 17, 35) : Color.FromArgb(28, 255, 255, 255);
                using (var b = new SolidBrush(hoverBg)) g.FillRectangle(b, r);
            }
            int cx = r.Left + r.Width / 2;
            int cy = r.Top + r.Height / 2;
            Color iconColor = (hover && idx == 2) ? Color.White : t.TextMuted;
            using (var pen = new Pen(iconColor, 1f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                if (kind == "min")
                    g.DrawLine(pen, cx - 5, cy + 1, cx + 5, cy + 1);
                else if (kind == "max")
                    g.DrawRectangle(pen, cx - 5, cy - 4, 10, 8);
                else if (kind == "restore")
                {
                    g.DrawRectangle(pen, cx - 5, cy - 2, 8, 6);
                    g.DrawRectangle(pen, cx - 3, cy - 4, 8, 6);
                }
                else // close
                {
                    g.DrawLine(pen, cx - 5, cy - 4, cx + 5, cy + 4);
                    g.DrawLine(pen, cx + 5, cy - 4, cx - 5, cy + 4);
                }
            }
        }
    }
}
