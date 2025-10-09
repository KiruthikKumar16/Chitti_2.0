using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace LineBuddy
{
    /// <summary>
    /// Interaction logic for InfoWindow.xaml
    /// </summary>
    public partial class InfoWindow : Window
    {
        public InfoWindow(int selectedTab = 0)
        {
            InitializeComponent();
            
            // Set initial tab
            if (selectedTab >= 0 && selectedTab < 3)
            {
                var tabControl = (TabControl)this.FindName("TabControl");
                if (tabControl != null && selectedTab < tabControl.Items.Count)
                {
                    tabControl.SelectedIndex = selectedTab;
                }
            }
            
            // Setup window behavior
            SetupWindow();
        }

        private void SetupWindow()
        {
            // Make window draggable
            this.MouseDown += (sender, e) =>
            {
                if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                {
                    this.DragMove();
                }
            };
            
            // Handle escape key to close
            this.KeyDown += (sender, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Escape)
                {
                    this.Close();
                }
            };
            
            // Focus the window
            this.Focus();
        }

        private void SocialLink_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string url)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not open link: {ex.Message}", "Error", 
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Static methods to open specific tabs from MainWindow
        public static void ShowAbout()
        {
            var window = new InfoWindow(0);
            window.Show();
        }

        public static void ShowHowToUse()
        {
            var window = new InfoWindow(1);
            window.Show();
        }

        public static void ShowContribute()
        {
            var window = new InfoWindow(2);
            window.Show();
        }
    }
}