using System.Windows;

namespace CybersecurityChatbot
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize database
            Services.DatabaseService.InitializeDatabase();
        }
    }
}