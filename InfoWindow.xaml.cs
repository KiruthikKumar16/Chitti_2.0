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
        private static InfoWindow _instance;
        private static readonly object _lock = new object();

        public InfoWindow(int selectedTab = 0)
        {
            InitializeComponent();
            
            // Set initial tab
            if (selectedTab >= 0 && selectedTab < 3)
            {
                TabControl.SelectedIndex = selectedTab;
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
            ShowWindow(0);
        }

        public static void ShowHowToUse()
        {
            ShowWindow(1);
        }

        public static void ShowContribute()
        {
            ShowWindow(2);
        }

        private static void ShowWindow(int selectedTab)
        {
            lock (_lock)
            {
                if (_instance == null || !_instance.IsVisible)
                {
                    _instance = new InfoWindow(selectedTab);
                    _instance.Show();
                }
                else
                {
                    // Window already exists, just switch to the requested tab
                    _instance.TabControl.SelectedIndex = selectedTab;
                    _instance.Activate();
                    _instance.Focus();
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            lock (_lock)
            {
                _instance = null;
            }
            base.OnClosed(e);
        }
    }
}