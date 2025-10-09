using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using System.Threading.Tasks;
using LineBuddy.Services;
using LineBuddy.Models;

namespace LineBuddy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly LLMService _llmService;
        private readonly SystemMonitorService _systemMonitorService;
        private DynamicMessageService _dynamicMessageService;
        private ConversationHistoryService _conversationHistory;
        private SmartTagService _smartTagService;
        private ClipboardIntegrationService _clipboardIntegration;
        private ScreenshotService _screenshotService;
        private readonly DispatcherTimer _updateTimer;
        private readonly DispatcherTimer _messageTimer;
        private readonly AppBarManager _appBarManager;
        private AppSettings _settings;
        private bool _isUserInteracting = false;
        private bool _hasUserQuestion = false;
        private bool _isTypingAnimation = false;
        private System.Threading.CancellationTokenSource _typingCancellation;

        public MainWindow()
        {
            InitializeComponent();
            
            // Load settings
            _settings = AppSettings.Load();
            
            // Initialize services
            _llmService = new LLMService(_settings);
            _systemMonitorService = new SystemMonitorService();
            _dynamicMessageService = new DynamicMessageService(_settings);
            _conversationHistory = new ConversationHistoryService(_settings);
            _screenshotService = new ScreenshotService();
            _smartTagService = new SmartTagService(_llmService, _conversationHistory, _screenshotService);
            _clipboardIntegration = new ClipboardIntegrationService(_smartTagService, this, _settings);
            _appBarManager = new AppBarManager();
            
            // Initialize Smart Tags
            InitializeSmartTags();
            
            // Setup window
            Style = (Style)Application.Current.Resources["ModernWindowStyle"];
            SetupWindow();
            
            // Setup timer for system monitoring
            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _updateTimer.Tick += UpdateSystemInfo;
            _updateTimer.Start();
            
            // Setup timer for dynamic messages with settings-based interval
            _messageTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(_settings.MessageUpdateIntervalMinutes)
            };
            _messageTimer.Tick += UpdateDynamicMessage;
            _messageTimer.Start();
            
            // Initial updates
            UpdateSystemInfo(null, null);
            UpdateDynamicMessage(null, null);
        }

        private void SetupWindow()
        {
            // Set window size and position at the very top
            Width = SystemParameters.PrimaryScreenWidth;
            Height = 40; // Back to original height
            Left = 0;
            Top = 0;
            
            // Ensure it starts at the very top
            WindowStartupLocation = WindowStartupLocation.Manual;
            
            // Register AppBar when window is loaded
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Use the new AppBarManager for proper AppBar registration
            _appBarManager.RegisterAppBar(this);
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            _appBarManager.UnregisterAppBar();
            Hide();
        }

        private async void UpdateDynamicMessage(object sender, EventArgs e)
        {
            // Only update dynamic message if user is not interacting
            if (!_isUserInteracting && !_hasUserQuestion && !_isTypingAnimation)
            {
                try
                {
                    var message = await _dynamicMessageService.GetCurrentMessageAsync();
                    await TypeTextWithAnimation(message, System.Windows.Media.Brushes.LightGray);
                }
                catch
                {
                    await TypeTextWithAnimation("✨ LineBuddy ready - What can I help with?", System.Windows.Media.Brushes.LightGray);
                }
            }
        }

        private async Task TypeTextWithAnimation(string text, System.Windows.Media.Brush foreground)
        {
            if (_isTypingAnimation) return;
            
            _isTypingAnimation = true;
            _typingCancellation?.Cancel();
            _typingCancellation = new System.Threading.CancellationTokenSource();
            
            try
            {
                QueryTextBox.Foreground = foreground;
                
                // If typing animation is disabled (speed = 0), show text immediately
                if (_settings.TypingAnimationSpeed == 0)
                {
                    QueryTextBox.Text = text;
                    return;
                }
                
                QueryTextBox.Text = "";
                
                for (int i = 0; i <= text.Length; i++)
                {
                    if (_typingCancellation.Token.IsCancellationRequested) break;
                    
                    QueryTextBox.Text = text.Substring(0, i);
                    await Task.Delay(_settings.TypingAnimationSpeed, _typingCancellation.Token);
                }
            }
            catch (TaskCanceledException)
            {
                // Animation was cancelled
            }
            finally
            {
                _isTypingAnimation = false;
            }
        }

        private async Task ShowThinkingAnimation()
        {
            _typingCancellation?.Cancel();
            _typingCancellation = new System.Threading.CancellationTokenSource();
            
            try
            {
                QueryTextBox.Foreground = System.Windows.Media.Brushes.Gray; // Changed from Yellow to Gray
                string[] thinkingStates = { "🤔 Thinking", "🤔 Thinking.", "🤔 Thinking..", "🤔 Thinking..." };
                int index = 0;
                
                while (!_typingCancellation.Token.IsCancellationRequested)
                {
                    QueryTextBox.Text = thinkingStates[index % thinkingStates.Length];
                    index++;
                    await Task.Delay(500, _typingCancellation.Token);
                }
            }
            catch (TaskCanceledException)
            {
                // Animation was cancelled
            }
        }

        private void QueryTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            _isUserInteracting = true;
            if (!_hasUserQuestion)
            {
                QueryTextBox.Text = "";
                QueryTextBox.Foreground = System.Windows.Media.Brushes.White;
            }
        }

        private void QueryTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(QueryTextBox.Text))
            {
                _isUserInteracting = false;
                _hasUserQuestion = false;
                UpdateDynamicMessage(null, null);
            }
        }

        private void QueryTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            _hasUserQuestion = !string.IsNullOrWhiteSpace(QueryTextBox.Text) && 
                              QueryTextBox.Foreground == System.Windows.Media.Brushes.White;
        }

        private async void QueryTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                await ProcessQuery();
            }
            else if (e.Key == System.Windows.Input.Key.Escape)
            {
                ClearAndResetToNormal();
            }
        }

        private void ClearAndResetToNormal()
        {
            // Cancel any ongoing animations
            _typingCancellation?.Cancel();
            
            QueryTextBox.Clear();
            _isUserInteracting = false;
            _hasUserQuestion = false;
            _isTypingAnimation = false;
            
            // Start dynamic messages again after 5 seconds
            Task.Run(async () =>
            {
                await Task.Delay(5000);
                if (!_isUserInteracting && !_hasUserQuestion)
                {
                    Dispatcher.Invoke(() => UpdateDynamicMessage(null, null));
                }
            });
        }

        private async Task ProcessQuery()
        {
            if (string.IsNullOrWhiteSpace(QueryTextBox.Text))
                return;

            var query = QueryTextBox.Text;
            
            // Start thinking animation
            var thinkingTask = ShowThinkingAnimation();
            QueryTextBox.IsEnabled = false;

            try
            {
                // Get conversation context if enabled
                var context = _conversationHistory.GetContextualHistory();
                
                // Capture screenshot if enabled
                string screenshotData = "";
                if (_settings.UseScreenshotContext)
                {
                    try
                    {
                        screenshotData = _settings.CaptureActiveWindowOnly 
                            ? await _screenshotService.CaptureActiveWindowAsync()
                            : await _screenshotService.CaptureScreenshotAsync();
                    }
                    catch (Exception ex)
                    {
                        // Continue without screenshot if capture fails
                        await TypeTextWithAnimation($"Screenshot failed: {ex.Message}", System.Windows.Media.Brushes.Orange);
                        await Task.Delay(1000);
                    }
                }
                
                var response = await _llmService.QueryAsync(query, context, screenshotData);
                
                // Add this interaction to history
                _conversationHistory.AddEntry(query, response);
                
                // Stop thinking animation and show response with typing effect
                _typingCancellation?.Cancel();
                await TypeTextWithAnimation(response, System.Windows.Media.Brushes.LightGray);
            }
            catch (Exception ex)
            {
                _typingCancellation?.Cancel();
                await TypeTextWithAnimation($"Error: {ex.Message}", System.Windows.Media.Brushes.Red);
            }
            finally
            {
                QueryTextBox.IsEnabled = true;
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            // Cancel any ongoing animations
            _typingCancellation?.Cancel();
            
            QueryTextBox.Clear();
            QueryTextBox.Focus();
            
            // Reset state and restart dynamic messages after a short delay
            _isUserInteracting = false;
            _hasUserQuestion = false;
            _isTypingAnimation = false;
            
            // Start dynamic messages again after 5 seconds
            Task.Run(async () =>
            {
                await Task.Delay(5000);
                if (!_isUserInteracting && !_hasUserQuestion)
                {
                    Dispatcher.Invoke(() => UpdateDynamicMessage(null, null));
                }
            });
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // Show the context menu when settings button is clicked
            SettingsButton.ContextMenu.IsOpen = true;
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(_settings, OnSettingsChanged);
            settingsWindow.Owner = this;
            settingsWindow.ShowDialog();
        }

        private void OnSettingsChanged(AppSettings newSettings)
        {
            _settings = newSettings;
            
            // Update message service with new settings
            _dynamicMessageService.UpdateSettings(_settings);
            
            // Update conversation history service with new settings
            _conversationHistory = new ConversationHistoryService(_settings);
            
            // Update message timer interval
            _messageTimer.Stop();
            _messageTimer.Interval = TimeSpan.FromMinutes(_settings.MessageUpdateIntervalMinutes);
            _messageTimer.Start();
            
            // Force immediate message update
            UpdateDynamicMessage(null, null);
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Restart Line Buddy?", "Restart", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                System.Diagnostics.Process.Start(Environment.ProcessPath);
                Application.Current.Shutdown();
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Exit Line Buddy?", "Exit", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _appBarManager.UnregisterAppBar();
                Application.Current.Shutdown();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void UpdateSystemInfo(object sender, EventArgs e)
        {
            var systemInfo = _systemMonitorService.GetSystemInfo();
            
            TimeLabel.Content = systemInfo.CurrentTime;
            BatteryLabel.Content = systemInfo.BatteryStatus;
            // NetworkLabel removed to make space for logo
        }
        
        private void InitializeSmartTags()
        {
            // Subscribe to clipboard integration events
            _clipboardIntegration.StatusChanged += OnSmartTagStatusChanged;
            _clipboardIntegration.ProcessingUpdate += OnSmartTagProcessingUpdate;
            
            // Start monitoring clipboard
            _clipboardIntegration.StartMonitoring();
        }
        
        private void OnSmartTagStatusChanged(SmartTagStatus status)
        {
            Dispatcher.Invoke(() =>
            {
                // Get the template elements
                var template = SmartTagsBubble.Template;
                var statusTextBlock = SmartTagsBubble.Template?.FindName("StatusText", SmartTagsBubble) as TextBlock;
                var glowBorder = SmartTagsBubble.Template?.FindName("GlowBorder", SmartTagsBubble) as Border;
                var glowEffect = SmartTagsBubble.Template?.FindName("GlowEffect", SmartTagsBubble) as DropShadowEffect;
                
                if (statusTextBlock != null)
                {
                    switch (status)
                    {
                        case SmartTagStatus.Listening:
                            statusTextBlock.Text = "LISTENING";
                            SmartTagsBubble.ToolTip = "Click to toggle Smart Tags (Currently: ON)";
                            SmartTagsBubble.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0x22, 0x00, 0x7A, 0xCC));
                            SmartTagsBubble.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0x00, 0x7A, 0xCC));
                            SmartTagsBubble.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0x00, 0x7A, 0xCC));
                            StopGlowAnimation(glowBorder, glowEffect);
                            break;
                        case SmartTagStatus.Processing:
                            statusTextBlock.Text = "PROCESSING";
                            SmartTagsBubble.ToolTip = "Smart Tags: Processing with AI...";
                            SmartTagsBubble.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0x22, 0xFF, 0x8C, 0x00));
                            SmartTagsBubble.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xFF, 0x8C, 0x00));
                            SmartTagsBubble.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xFF, 0x8C, 0x00));
                            StartGlowAnimation(glowBorder, glowEffect, System.Windows.Media.Color.FromArgb(0xFF, 0xFF, 0x8C, 0x00));
                            break;
                        case SmartTagStatus.Pasting:
                            statusTextBlock.Text = "PASTING";
                            SmartTagsBubble.ToolTip = "Smart Tags: Typing result...";
                            SmartTagsBubble.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0x22, 0x00, 0x80, 0x00));
                            SmartTagsBubble.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0x00, 0xFF, 0x00));
                            SmartTagsBubble.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0x00, 0xFF, 0x00));
                            StartGlowAnimation(glowBorder, glowEffect, System.Windows.Media.Color.FromArgb(0xFF, 0x00, 0xFF, 0x00));
                            break;
                        case SmartTagStatus.Error:
                            statusTextBlock.Text = "ERROR";
                            SmartTagsBubble.ToolTip = "Smart Tags: Error occurred - Click to retry";
                            SmartTagsBubble.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0x22, 0xFF, 0x00, 0x00));
                            SmartTagsBubble.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xFF, 0x00, 0x00));
                            SmartTagsBubble.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xFF, 0x00, 0x00));
                            StopGlowAnimation(glowBorder, glowEffect);
                            break;
                        case SmartTagStatus.Disabled:
                            statusTextBlock.Text = "OFF";
                            SmartTagsBubble.ToolTip = "Click to toggle Smart Tags (Currently: OFF)";
                            SmartTagsBubble.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0x22, 0x66, 0x66, 0x66));
                            SmartTagsBubble.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0x99, 0x99, 0x99));
                            SmartTagsBubble.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0x66, 0x66, 0x66));
                            StopGlowAnimation(glowBorder, glowEffect);
                            break;
                    }
                }
            });
        }
        
        private void OnSmartTagProcessingUpdate(string message)
        {
            // Could update a status area or tooltip with the processing message
            Dispatcher.Invoke(() =>
            {
                SmartTagsBubble.ToolTip = $"Smart Tags: {message}";
            });
        }
        
        private void SmartTagsBubble_Click(object sender, RoutedEventArgs e)
        {
            // Toggle Smart Tags on/off
            _settings.SmartTagsEnabled = !_settings.SmartTagsEnabled;
            _settings.Save();
            
            if (_settings.SmartTagsEnabled)
            {
                _clipboardIntegration?.StartMonitoring();
                OnSmartTagStatusChanged(SmartTagStatus.Listening);
            }
            else
            {
                _clipboardIntegration?.StopMonitoring();
                OnSmartTagStatusChanged(SmartTagStatus.Disabled);
            }
        }
        
        private void ScreenshotToggle_Click(object sender, RoutedEventArgs e)
        {
            // Toggle screenshot context on/off
            _settings.UseScreenshotContext = !_settings.UseScreenshotContext;
            _settings.Save();
            
            // Update icon appearance through template
            var template = ScreenshotToggle.Template;
            var screenshotIcon = template?.FindName("ScreenshotIcon", ScreenshotToggle) as TextBlock;
            
            if (screenshotIcon != null)
            {
                if (_settings.UseScreenshotContext)
                {
                    screenshotIcon.Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x7A, 0xCC)); // Blue when active
                    ScreenshotToggle.ToolTip = "Screenshot context: ON";
                }
                else
                {
                    screenshotIcon.Foreground = new SolidColorBrush(Color.FromRgb(0x66, 0x66, 0x66)); // Gray when off
                    ScreenshotToggle.ToolTip = "Screenshot context: OFF";
                }
            }
        }
        
        private void StartGlowAnimation(Border glowBorder, DropShadowEffect glowEffect, Color color)
        {
            if (glowBorder != null && glowEffect != null)
            {
                glowEffect.Color = color;
                
                var opacityAnimation = new DoubleAnimation
                {
                    From = 0.0,
                    To = 0.8,
                    Duration = TimeSpan.FromMilliseconds(800),
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever
                };
                
                var blurAnimation = new DoubleAnimation
                {
                    From = 5,
                    To = 20,
                    Duration = TimeSpan.FromMilliseconds(800),
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever
                };
                
                glowEffect.BeginAnimation(DropShadowEffect.OpacityProperty, opacityAnimation);
                glowEffect.BeginAnimation(DropShadowEffect.BlurRadiusProperty, blurAnimation);
            }
        }
        
        private void StopGlowAnimation(Border glowBorder, DropShadowEffect glowEffect)
        {
            if (glowBorder != null && glowEffect != null)
            {
                glowEffect.BeginAnimation(DropShadowEffect.OpacityProperty, null);
                glowEffect.BeginAnimation(DropShadowEffect.BlurRadiusProperty, null);
                glowEffect.Opacity = 0;
                glowEffect.BlurRadius = 5;
            }
        }
    }
}