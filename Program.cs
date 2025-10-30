using System;
using System.Windows.Forms;

namespace PreviewUnblock
{
    /// <summary>
    /// The main entry point for the application. This class bootstraps the
    /// WinForms runtime, configures high DPI support and starts the main form.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Configure WinForms to use system DPI awareness and default font.
            ApplicationConfiguration.Initialize();

            // Create and run the primary form.
            Application.Run(new MainForm());
        }
    }
}