using System;
using System.Linq;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Controls;

namespace LineBuddy
{
    public partial class SettingsWindow : Window
    {
        private AppSettings _settings;
        private Action<AppSettings> _onSettingsChanged;

        public SettingsWindow(AppSettings currentSettings, Action<AppSettings> onSettingsChanged)
        {
            InitializeComponent();
            _settings = currentSettings;
            _onSettingsChanged = onSettingsChanged;
            LoadSettings();
        }

        private void LoadSettings()
        {
            // Load update interval
            IntervalSlider.Value = _settings.MessageUpdateIntervalMinutes;
            IntervalLabel.Content = $"{_settings.MessageUpdateIntervalMinutes} minutes";

            // Load message type preferences
            TimeBasedCheckBox.IsChecked = _settings.ShowTimeBasedMessages;
            SystemHealthCheckBox.IsChecked = _settings.ShowSystemHealth;
            NetworkStatusCheckBox.IsChecked = _settings.ShowNetworkStatus;
            ProductivityCheckBox.IsChecked = _settings.ShowProductivityTips;
            WeatherCheckBox.IsChecked = _settings.ShowWeatherUpdates;
            StockCheckBox.IsChecked = _settings.ShowStockUpdates;
            CryptoCheckBox.IsChecked = _settings.ShowCryptoUpdates;
            TechNewsCheckBox.IsChecked = _settings.ShowTechNews;

            // Load watchlists
            StockWatchlistTextBox.Text = string.Join(", ", _settings.StockWatchlist);
            CryptoWatchlistTextBox.Text = string.Join(", ", _settings.CryptoWatchlist);
            WeatherLocationTextBox.Text = _settings.WeatherLocation;
            
            // Load typing animation speed
            TypingSpeedSlider.Value = _settings.TypingAnimationSpeed;
            UpdateTypingSpeedLabel(_settings.TypingAnimationSpeed);
            
            // Load history settings
            HistoryEnabledCheckBox.IsChecked = _settings.ShowConversationHistory;
            HistoryRangeSlider.Value = _settings.HistoryRangeMessages;
            HistoryRangeLabel.Content = $"{_settings.HistoryRangeMessages} messages";
            
            // Load Smart Tags settings
            SmartTagsEnabledCheckBox.IsChecked = _settings.SmartTagsEnabled;
            PastingSpeedSlider.Value = _settings.SmartTagsPastingSpeed;
            UpdatePastingSpeedLabel(_settings.SmartTagsPastingSpeed);
            
            // Load screenshot settings
            ScreenshotContextCheckBox.IsChecked = _settings.UseScreenshotContext;
            ActiveWindowOnlyCheckBox.IsChecked = _settings.CaptureActiveWindowOnly;
            
            // Load LLM settings
            LoadLLMSettings();
        }

        private void IntervalSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IntervalLabel != null)
            {
                IntervalLabel.Content = $"{(int)e.NewValue} minutes";
            }
        }

        private void TypingSpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateTypingSpeedLabel((int)e.NewValue);
        }

        private void UpdateTypingSpeedLabel(int speed)
        {
            if (TypingSpeedLabel != null)
            {
                if (speed == 0)
                {
                    TypingSpeedLabel.Content = "Instant (No Animation)";
                }
                else if (speed <= 20)
                {
                    TypingSpeedLabel.Content = $"{speed}ms (Fast)";
                }
                else if (speed <= 40)
                {
                    TypingSpeedLabel.Content = $"{speed}ms (Normal)";
                }
                else
                {
                    TypingSpeedLabel.Content = $"{speed}ms (Slow)";
                }
            }
        }

        private void HistoryEnabledCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            // Event handler for history checkbox change
        }

        private void HistoryRangeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (HistoryRangeLabel != null)
            {
                HistoryRangeLabel.Content = $"{(int)e.NewValue} messages";
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Save update interval
                _settings.MessageUpdateIntervalMinutes = (int)IntervalSlider.Value;
                
                // Save typing animation speed
                _settings.TypingAnimationSpeed = (int)TypingSpeedSlider.Value;
                
                // Save history settings
                _settings.ShowConversationHistory = HistoryEnabledCheckBox.IsChecked ?? true;
                _settings.HistoryRangeMessages = (int)HistoryRangeSlider.Value;

                // Save message type preferences
                _settings.ShowTimeBasedMessages = TimeBasedCheckBox.IsChecked ?? false;
                _settings.ShowSystemHealth = SystemHealthCheckBox.IsChecked ?? false;
                _settings.ShowNetworkStatus = NetworkStatusCheckBox.IsChecked ?? false;
                _settings.ShowProductivityTips = ProductivityCheckBox.IsChecked ?? false;
                _settings.ShowWeatherUpdates = WeatherCheckBox.IsChecked ?? false;
                _settings.ShowStockUpdates = StockCheckBox.IsChecked ?? false;
                _settings.ShowCryptoUpdates = CryptoCheckBox.IsChecked ?? false;
                _settings.ShowTechNews = TechNewsCheckBox.IsChecked ?? false;

                // Save Smart Tags settings
                _settings.SmartTagsEnabled = SmartTagsEnabledCheckBox.IsChecked ?? true;
                _settings.SmartTagsPastingSpeed = (int)PastingSpeedSlider.Value;
                
                // Save screenshot settings
                _settings.UseScreenshotContext = ScreenshotContextCheckBox.IsChecked ?? false;
                _settings.CaptureActiveWindowOnly = ActiveWindowOnlyCheckBox.IsChecked ?? true;

                // Save watchlists
                _settings.StockWatchlist = StockWatchlistTextBox.Text
                    .Split(',')
                    .Select(s => s.Trim().ToUpper())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();

                _settings.CryptoWatchlist = CryptoWatchlistTextBox.Text
                    .Split(',')
                    .Select(s => s.Trim().ToLower())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();

                _settings.WeatherLocation = WeatherLocationTextBox.Text.Trim();
                
                // Save LLM settings
                SaveLLMSettings();

                // Validate at least one message type is selected
                if (!_settings.GetActiveMessageTypes().Any())
                {
                    MessageBox.Show("Please select at least one message type.", "Invalid Settings", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Save settings to file
                _settings.Save();

                // Notify main window of changes
                _onSettingsChanged?.Invoke(_settings);

                MessageBox.Show("Settings saved successfully!", "Settings", 
                    MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SmartTagsEnabledCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            // Can be used to enable/disable Smart Tags-related controls if needed
        }

        private void PastingSpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdatePastingSpeedLabel((int)e.NewValue);
        }

        private void UpdatePastingSpeedLabel(int speed)
        {
            if (PastingSpeedLabel != null)
            {
                if (speed == 0)
                {
                    PastingSpeedLabel.Content = "Instant (No Animation)";
                }
                else if (speed <= 25)
                {
                    PastingSpeedLabel.Content = $"{speed}ms (Fast)";
                }
                else if (speed <= 75)
                {
                    PastingSpeedLabel.Content = $"{speed}ms (Normal)";
                }
                else
                {
                    PastingSpeedLabel.Content = $"{speed}ms (Slow)";
                }
            }
        }

        private void AddCustomTagButton_Click(object sender, RoutedEventArgs e)
        {
            var tagName = CustomTagNameTextBox.Text.Trim();
            var tagDescription = CustomTagDescriptionTextBox.Text.Trim();

            if (string.IsNullOrEmpty(tagName) || string.IsNullOrEmpty(tagDescription))
            {
                MessageBox.Show("Please enter both tag name and description.", "Invalid Input", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!tagName.StartsWith("/"))
            {
                tagName = "/" + tagName;
            }

            // Add to custom tags (simplified for now)
            MessageBox.Show($"Custom tag '{tagName}' added successfully!\n\nDescription: {tagDescription}", "Custom Tag Added", 
                MessageBoxButton.OK, MessageBoxImage.Information);

            // Clear inputs
            CustomTagNameTextBox.Text = "/custom";
            CustomTagDescriptionTextBox.Text = "Enter instruction for your custom tag";
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        
        // LLM Settings Methods
        private void LoadLLMSettings()
        {
            // Set provider
            foreach (ComboBoxItem item in LLMProviderComboBox.Items)
            {
                if (item.Tag.ToString() == _settings.LLMProvider)
                {
                    LLMProviderComboBox.SelectedItem = item;
                    break;
                }
            }
            
            // Load API key
            LLMApiKeyPasswordBox.Password = _settings.LLMApiKey;
            
            // Show appropriate help panel
            UpdateProviderHelp(_settings.LLMProvider);
        }
        
        private void LLMProviderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LLMProviderComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string provider = selectedItem.Tag.ToString();
                UpdateProviderHelp(provider);
            }
        }
        
        private void UpdateProviderHelp(string provider)
        {
            // Hide all help panels first
            GeminiHelpPanel.Visibility = Visibility.Collapsed;
            OpenAIHelpPanel.Visibility = Visibility.Collapsed;
            AnthropicHelpPanel.Visibility = Visibility.Collapsed;
            
            // Show the appropriate help panel
            switch (provider)
            {
                case "gemini":
                    GeminiHelpPanel.Visibility = Visibility.Visible;
                    break;
                case "openai":
                    OpenAIHelpPanel.Visibility = Visibility.Visible;
                    break;
                case "anthropic":
                    AnthropicHelpPanel.Visibility = Visibility.Visible;
                    break;
            }
        }
        
        private void SaveLLMSettings()
        {
            // Save LLM provider
            if (LLMProviderComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                _settings.LLMProvider = selectedItem.Tag.ToString();
                
                // Set the optimal model for each provider (using working model names)
                switch (_settings.LLMProvider)
                {
                    case "gemini":
                        _settings.LLMModel = "gemini-2.0-flash-exp"; // This model works with image support
                        break;
                    case "openai":
                        _settings.LLMModel = "gpt-4o-mini";
                        break;
                    case "anthropic":
                        _settings.LLMModel = "claude-3-haiku-20240307";
                        break;
                }
            }
            
            // Save API key
            _settings.LLMApiKey = LLMApiKeyPasswordBox.Password;
            _settings.LLMBaseUrl = ""; // Not needed for API providers
        }
    }
}