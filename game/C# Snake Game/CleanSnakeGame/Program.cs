using System;
using System.Windows.Forms;

namespace CleanSnakeGame
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // Initialize local database (creates DB file and tables if missing)
            try
            {
                Database.Initialize();
            }
            catch (Exception ex)
            {
                // Fallback: do not block app if DB init fails
                Console.WriteLine($"Database initialization failed: {ex.Message}");
            }
            Application.Run(new MainMenuForm());
        }
    }
}
