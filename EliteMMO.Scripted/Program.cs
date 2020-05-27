using System;
using System.Windows.Forms;
using EliteMMO.Scripted.Views;

namespace EliteMMO.Scripted
{
    internal static class Program
    {
        //private static EliteAPI core;
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow(null));
        }
    }
}