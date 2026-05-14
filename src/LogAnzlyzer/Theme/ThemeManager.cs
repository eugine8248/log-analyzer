using System;
using LogAnzlyzer.Storage;

namespace LogAnzlyzer.Theme
{
    // Central theme state. Controls subscribe to ThemeChanged and re-paint themselves.
    public static class ThemeManager
    {
        public static event EventHandler ThemeChanged;
        public static ThemeColors Current { get; private set; } = ThemeColors.Dark();

        public static void Set(ThemeMode mode)
        {
            if (Current.Mode == mode) return;
            Current = mode == ThemeMode.Dark ? ThemeColors.Dark() : ThemeColors.Light();
            CacheDatabase.SetSetting("theme", mode.ToString().ToLowerInvariant());
            ThemeChanged?.Invoke(null, EventArgs.Empty);
        }

        public static void Toggle() => Set(Current.Mode == ThemeMode.Dark ? ThemeMode.Light : ThemeMode.Dark);

        public static void LoadFromCache()
        {
            var pref = CacheDatabase.GetSetting("theme", "dark");
            Current = pref == "light" ? ThemeColors.Light() : ThemeColors.Dark();
        }
    }
}
