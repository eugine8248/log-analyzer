using System.Drawing;

namespace LogAnzlyzer.Theme
{
    // Mirror of design/pack/loganzlyzer/project/tokens.css.
    // Two themes (Dark default, Light toggle). Single source of truth for all chrome.
    public enum ThemeMode { Dark, Light }

    public sealed class ThemeColors
    {
        public Color Bg, BgElev, Panel, PanelElev, Tabstrip, TabActive, Titlebar, Menubar;
        public Color Statusbar, StatusbarFg;
        public Color Border, BorderSoft, Divider;
        public Color Text, TextStrong, TextMuted, TextFaint;
        public Color Accent, AccentHover, AccentPress, AccentFg, AccentSoft;
        public Color P1, P1Soft, Median, MedianSoft, Warning, Error;
        public Color RowHover, RowSelected;
        public Color InputBg, InputBorder, InputFocus;
        public Color ScrollThumb, Grid;
        public ThemeMode Mode;

        public static ThemeColors Dark() => new ThemeColors
        {
            Mode = ThemeMode.Dark,
            Bg          = ColorTranslator.FromHtml("#11141a"),
            BgElev      = ColorTranslator.FromHtml("#161a21"),
            Panel       = ColorTranslator.FromHtml("#181c24"),
            PanelElev   = ColorTranslator.FromHtml("#1d222b"),
            Tabstrip    = ColorTranslator.FromHtml("#1a1e26"),
            TabActive   = ColorTranslator.FromHtml("#11141a"),
            Titlebar    = ColorTranslator.FromHtml("#0e1116"),
            Menubar     = ColorTranslator.FromHtml("#14171e"),
            Statusbar   = ColorTranslator.FromHtml("#20467a"),
            StatusbarFg = ColorTranslator.FromHtml("#e5edf8"),
            Border      = ColorTranslator.FromHtml("#262c36"),
            BorderSoft  = ColorTranslator.FromHtml("#1f242d"),
            Divider     = ColorTranslator.FromHtml("#232831"),
            Text        = ColorTranslator.FromHtml("#d6dae3"),
            TextStrong  = ColorTranslator.FromHtml("#f1f3f7"),
            TextMuted   = ColorTranslator.FromHtml("#7a8497"),
            TextFaint   = ColorTranslator.FromHtml("#545d6e"),
            Accent      = ColorTranslator.FromHtml("#5b8def"),
            AccentHover = ColorTranslator.FromHtml("#6d9bf7"),
            AccentPress = ColorTranslator.FromHtml("#4a78d8"),
            AccentFg    = Color.White,
            AccentSoft  = Color.FromArgb(41, 91, 141, 239),
            P1          = ColorTranslator.FromHtml("#ff8b6b"),
            P1Soft      = Color.FromArgb(36, 255, 139, 107),
            Median      = ColorTranslator.FromHtml("#59c2a8"),
            MedianSoft  = Color.FromArgb(36, 89, 194, 168),
            Warning     = ColorTranslator.FromHtml("#e8a86a"),
            Error       = ColorTranslator.FromHtml("#ff7a72"),
            RowHover    = Color.FromArgb(9, 255, 255, 255),
            RowSelected = Color.FromArgb(41, 91, 141, 239),
            InputBg     = ColorTranslator.FromHtml("#0d1015"),
            InputBorder = ColorTranslator.FromHtml("#2d3340"),
            InputFocus  = ColorTranslator.FromHtml("#5b8def"),
            ScrollThumb = ColorTranslator.FromHtml("#2c333f"),
            Grid        = Color.FromArgb(10, 255, 255, 255),
        };

        public static ThemeColors Light() => new ThemeColors
        {
            Mode = ThemeMode.Light,
            Bg          = ColorTranslator.FromHtml("#fbfbfd"),
            BgElev      = ColorTranslator.FromHtml("#ffffff"),
            Panel       = ColorTranslator.FromHtml("#f1f3f6"),
            PanelElev   = ColorTranslator.FromHtml("#ffffff"),
            Tabstrip    = ColorTranslator.FromHtml("#eaecf0"),
            TabActive   = ColorTranslator.FromHtml("#fbfbfd"),
            Titlebar    = ColorTranslator.FromHtml("#e3e6eb"),
            Menubar     = ColorTranslator.FromHtml("#f1f3f6"),
            Statusbar   = ColorTranslator.FromHtml("#2f5fbe"),
            StatusbarFg = Color.White,
            Border      = ColorTranslator.FromHtml("#d8dce3"),
            BorderSoft  = ColorTranslator.FromHtml("#e4e7ec"),
            Divider     = ColorTranslator.FromHtml("#e4e7ec"),
            Text        = ColorTranslator.FromHtml("#20242b"),
            TextStrong  = ColorTranslator.FromHtml("#0c0e12"),
            TextMuted   = ColorTranslator.FromHtml("#6c7484"),
            TextFaint   = ColorTranslator.FromHtml("#9ba2b0"),
            Accent      = ColorTranslator.FromHtml("#3461d6"),
            AccentHover = ColorTranslator.FromHtml("#4571e3"),
            AccentPress = ColorTranslator.FromHtml("#2851bf"),
            AccentFg    = Color.White,
            AccentSoft  = Color.FromArgb(26, 52, 97, 214),
            P1          = ColorTranslator.FromHtml("#c43c10"),
            P1Soft      = Color.FromArgb(26, 196, 60, 16),
            Median      = ColorTranslator.FromHtml("#1a8a73"),
            MedianSoft  = Color.FromArgb(26, 26, 138, 115),
            Warning     = ColorTranslator.FromHtml("#b8721d"),
            Error       = ColorTranslator.FromHtml("#b81f15"),
            RowHover    = Color.FromArgb(7, 0, 0, 0),
            RowSelected = Color.FromArgb(26, 52, 97, 214),
            InputBg     = Color.White,
            InputBorder = ColorTranslator.FromHtml("#c8ccd4"),
            InputFocus  = ColorTranslator.FromHtml("#3461d6"),
            ScrollThumb = ColorTranslator.FromHtml("#c4c8d0"),
            Grid        = Color.FromArgb(13, 0, 0, 0),
        };
    }

    public static class Fonts
    {
        public const string UiFamily = "Segoe UI";
        public const string MonoFamily = "Cascadia Mono";
        public const string MonoFallback = "Consolas";

        public static Font Body  => new Font(UiFamily, 9.75f);   // ~13px
        public static Font Small => new Font(UiFamily, 9.0f);    // ~12px
        public static Font Tiny  => new Font(UiFamily, 8.25f);   // ~11px
        public static Font Heading => new Font(UiFamily, 13.5f, FontStyle.Bold); // ~18px
        public static Font Stat => new Font(UiFamily, 21f);      // ~28px
        public static Font StatHero => new Font(UiFamily, 33f, FontStyle.Bold); // ~44px
        public static Font Mono => new Font(IsMonoInstalled() ? MonoFamily : MonoFallback, 9.0f);

        private static bool? _monoInstalled;
        private static bool IsMonoInstalled()
        {
            if (_monoInstalled.HasValue) return _monoInstalled.Value;
            using (var fam = new System.Drawing.Text.InstalledFontCollection())
            {
                foreach (var f in fam.Families)
                    if (f.Name == MonoFamily) { _monoInstalled = true; return true; }
            }
            _monoInstalled = false;
            return false;
        }
    }
}
