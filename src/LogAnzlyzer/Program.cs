using System;
using System.Windows.Forms;
using LogAnzlyzer.Forms;
using LogAnzlyzer.Storage;
using LogAnzlyzer.Theme;

namespace LogAnzlyzer
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            CacheDatabase.Initialize();
            ThemeManager.LoadFromCache();

            Application.Run(new MainForm());
        }
    }
}
