using System;
using System.Windows;

namespace LineBuddy
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Create and show the main window
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}