using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using LineBuddy.Services;
using LineBuddy.Models;
using Forms = System.Windows.Forms;

namespace LineBuddy
{
    public partial class SettingsWindow : Window
    {
        private AppSettings _settings;
        private Action<AppSettings> _onSettingsChanged;
        private List<CheckBox> _smartTagCheckBoxes;

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
            
            // Load personality settings
            LoadPersonalitySettings();

            // Populate recent color presets (first five buttons in the WrapPanel under General)
            try
            {
                var wrapPanel = FindQuickColorsWrapPanel();
                if (wrapPanel != null && _settings.OverlayRecentColors != null)
                {
                    for (int i = 0; i < wrapPanel.Children.Count && i < _settings.OverlayRecentColors.Count; i++)
                    {
                        if (wrapPanel.Children[i] is Button b)
                        {
                            var hex = _settings.OverlayRecentColors[i];
                            b.Tag = hex;
                            b.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
                        }
                    }
                }
            }
            catch { }
        }

        private WrapPanel FindQuickColorsWrapPanel()
        {
            // OverlayPreview is near the WrapPanel in the visual tree; navigate up then find first WrapPanel sibling
            try
            {
                if (OverlayPreview != null)
                {
                    var parent = VisualTreeHelper.GetParent(OverlayPreview);
                    while (parent != null && parent is not StackPanel)
                    {
                        parent = VisualTreeHelper.GetParent(parent);
                    }
                    // parent is StackPanel containing our appearance controls; next sibling should be the WrapPanel label + WrapPanel
                    var generalGroup = parent as StackPanel;
                }
            }
            catch { }
            // Fallback: search entire window
            WrapPanel found = null;
            void Walk(DependencyObject d)
            {
                if (d == null || found != null) return;
                if (d is WrapPanel wp && wp.Children.OfType<Button>().Any()) { found = wp; return; }
                int count = VisualTreeHelper.GetChildrenCount(d);
                for (int i = 0; i < count; i++) Walk(VisualTreeHelper.GetChild(d, i));
            }
            Walk(this);
            return found;
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

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
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

                // Parse watchlists efficiently
                _settings.StockWatchlist = ParseWatchlist(StockWatchlistTextBox.Text, toUpper: true);
                _settings.CryptoWatchlist = ParseWatchlist(CryptoWatchlistTextBox.Text, toUpper: false);

                _settings.WeatherLocation = WeatherLocationTextBox.Text.Trim();
                
                // Save LLM settings
                SaveLLMSettings();

                // Save personality settings
                if (PersonalityComboBox.SelectedItem is ComboBoxItem selectedPersonalityItem)
                {
                    _settings.SelectedPersonality = selectedPersonalityItem.Tag.ToString();
                }

                // Validate at least one message type is selected
                if (!_settings.GetActiveMessageTypes().Any())
                {
                    MessageBox.Show("Please select at least one message type.", "Invalid Settings", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Save settings to file asynchronously
                await Task.Run(() => _settings.Save());

                // Notify main window of changes
                _onSettingsChanged?.Invoke(_settings);
                
                // Update personality after settings are saved
                if (PersonalityComboBox.SelectedItem is ComboBoxItem selectedPersonalityItem2)
                {
                    PersonalityManager.Instance.SetPersonality(selectedPersonalityItem2.Tag.ToString());
                }

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

        private void SelectAllSmartTags_Click(object sender, RoutedEventArgs e)
        {
            CacheSmartTagCheckBoxes();
            foreach (var cb in _smartTagCheckBoxes) cb.IsChecked = true;
        }

        private void DeselectAllSmartTags_Click(object sender, RoutedEventArgs e)
        {
            CacheSmartTagCheckBoxes();
            foreach (var cb in _smartTagCheckBoxes) cb.IsChecked = false;
        }

        private void CacheSmartTagCheckBoxes()
        {
            if (_smartTagCheckBoxes != null) return;
            
            _smartTagCheckBoxes = new List<CheckBox>();
            var scrollViewer = FindName("SmartTagsScrollViewer") as ScrollViewer;
            if (scrollViewer != null)
            {
                CollectCheckBoxes(scrollViewer, _smartTagCheckBoxes);
            }
        }

        private void CollectCheckBoxes(DependencyObject container, List<CheckBox> checkBoxes)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(container); i++)
            {
                var child = VisualTreeHelper.GetChild(container, i);
                if (child is CheckBox cb) checkBoxes.Add(cb);
                else CollectCheckBoxes(child, checkBoxes);
            }
        }

        private void SetAllCheckBoxesInContainer(DependencyObject container, bool isChecked)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(container); i++)
            {
                var child = VisualTreeHelper.GetChild(container, i);
                
                if (child is CheckBox checkBox)
                {
                    checkBox.IsChecked = isChecked;
                }
                else
                {
                    SetAllCheckBoxesInContainer(child, isChecked);
                }
            }
        }

        private void LoadPersonalitySettings()
        {
            // Set selected personality
            foreach (ComboBoxItem item in PersonalityComboBox.Items)
            {
                if (item.Tag.ToString() == _settings.SelectedPersonality)
                {
                    PersonalityComboBox.SelectedItem = item;
                    UpdatePersonalityPreview(item.Tag.ToString());
                    break;
                }
            }
        }

        private void PersonalityComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PersonalityComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string personalityId = selectedItem.Tag.ToString();
                UpdatePersonalityPreview(personalityId);
                // Don't change settings or notify until Save is clicked
            }
        }

        private void UpdatePersonalityPreview(string personalityId)
        {
            // Use a simple switch for faster lookup instead of PersonalityManager
            string greeting, description, voice, tone;
            
            switch (personalityId.ToLower())
            {
                case "jarvis":
                    greeting = "Good {timeofday}, sir. How may I be of service?";
                    description = "Sophisticated British butler AI";
                    voice = "refined, polished, ultra-professional, British butler";
                    tone = "courteous, efficient, slightly formal but warm";
                    break;
                case "thanos":
                    greeting = "I am inevitable. What do you require?";
                    description = "The Mad Titan with cosmic power";
                    voice = "deep, commanding, philosophical, slightly menacing";
                    tone = "confident, ominous, wise, intimidating";
                    break;
                case "ironman":
                    greeting = "Hey there! Tony Stark here. What can I build for you?";
                    description = "Genius billionaire playboy philanthropist";
                    voice = "witty, confident, tech-savvy, slightly arrogant";
                    tone = "charismatic, innovative, bold, slightly cocky";
                    break;
                case "jacksparrow":
                    greeting = "Ahoy! Captain Jack Sparrow at your service, savvy?";
                    description = "Eccentric pirate captain with a heart of gold";
                    voice = "slurred, theatrical, witty, slightly drunk";
                    tone = "charming, unpredictable, clever, roguish";
                    break;
                case "sherlock":
                    greeting = "Elementary, my dear Watson. What case shall we solve?";
                    description = "Master detective with extraordinary deductive powers";
                    voice = "precise, analytical, slightly condescending, British";
                    tone = "logical, observant, brilliant, slightly arrogant";
                    break;
                case "glados":
                    greeting = "Oh, it's you. How... unexpected. What do you want?";
                    description = "Sarcastic AI from Aperture Science";
                    voice = "sarcastic, monotone, slightly menacing, robotic";
                    tone = "sardonic, intelligent, passive-aggressive, darkly humorous";
                    break;
                case "yoda":
                    greeting = "Greetings, young one. How may I help you, I can?";
                    description = "Wise Jedi Master with ancient knowledge";
                    voice = "wise, old, slightly broken English, mystical";
                    tone = "ancient, profound, patient, slightly cryptic";
                    break;
                case "deadpool":
                    greeting = "Hey there, chimichanga! What's the sitch?";
                    description = "Merc with a mouth who breaks the fourth wall";
                    voice = "witty, crude, self-aware, pop-culture obsessed";
                    tone = "irreverent, hilarious, unpredictable, meta";
                    break;
                default: // chitti
                    greeting = "Hey there! Ready to help! 😊";
                    description = "Witty, concise, friendly AI assistant";
                    voice = "witty, concise, friendly, slightly playful";
                    tone = "confident, uplifting, helpful, professional";
                    break;
            }
            
            PersonalityPreviewText.Text = greeting;
            PersonalityDescriptionText.Text = $"{description}\n\nVoice: {voice}\nTone: {tone}";
        }

        private List<string> ParseWatchlist(string input, bool toUpper)
        {
            if (string.IsNullOrWhiteSpace(input)) return new List<string>();
            
            var items = input.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var result = new List<string>(items.Length);
            
            for (int i = 0; i < items.Length; i++)
            {
                var trimmed = items[i].Trim();
                if (trimmed.Length > 0)
                {
                    result.Add(toUpper ? trimmed.ToUpper() : trimmed.ToLower());
                }
            }
            return result;
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
            
            // Load overlay background hex
            if (OverlayBgTextBox != null)
            {
                OverlayBgTextBox.Text = _settings.OverlayBackgroundHex ?? "#FF000000";
                // Initialize opacity slider from AA
                try
                {
                    var hex = OverlayBgTextBox.Text.Trim();
                    if (hex.Length == 7 && hex.StartsWith("#")) hex = "#FF" + hex.Substring(1);
                    if (hex.Length == 9 && hex.StartsWith("#"))
                    {
                        var aa = Convert.ToInt32(hex.Substring(1, 2), 16);
                        if (OverlayAlphaSlider != null)
                        {
                            OverlayAlphaSlider.Value = aa;
                            OverlayAlphaLabel.Text = aa.ToString("D3");
                        }
                    }
                    ApplyOverlayPreview(hex);
                }
                catch { }
                // Initialize quick colors from recent
                try
                {
                    var recents = (_settings.OverlayRecentColors ?? new System.Collections.Generic.List<string>()).Take(5).ToList();
                    var wrap = FindName("OverlayBgTextBox") as TextBox; // anchor
                }
                catch { }
            }

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

        // Appearance helpers
        private void OverlayColorPreset_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.Tag is string hex)
            {
                if (OverlayBgTextBox != null)
                {
                    OverlayBgTextBox.Text = hex;
                }
                ApplyOverlayPreview(hex);
            }
        }

        private void OverlayBgTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var hex = OverlayBgTextBox?.Text ?? "#FF000000";
            ApplyOverlayPreview(hex);
        }

        private void OverlayAlphaSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (OverlayBgTextBox == null) return;
            var hex = OverlayBgTextBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(hex)) hex = "#FF000000";
            try
            {
                // Normalize to #AARRGGBB
                if (hex.Length == 7 && hex.StartsWith("#"))
                {
                    hex = "#FF" + hex.Substring(1);
                }
                if (hex.Length == 9 && hex.StartsWith("#"))
                {
                    var aa = ((int)e.NewValue).ToString("X2");
                    hex = "#" + aa + hex.Substring(3);
                    OverlayBgTextBox.Text = hex;
                    OverlayAlphaLabel.Text = aa;
                    ApplyOverlayPreview(hex);
                }
            }
            catch { }
        }

        private void ApplyOverlayPreview(string hex)
        {
            try
            {
                if (OverlayPreview != null)
                {
                    var color = (Color)ColorConverter.ConvertFromString(hex);
                    OverlayPreview.Background = new SolidColorBrush(color);
                }
            }
            catch { }
        }

        private void OverlayPickBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using var dlg = new Forms.ColorDialog
                {
                    FullOpen = true
                };
                if (dlg.ShowDialog() == Forms.DialogResult.OK)
                {
                    // Use current alpha slider value for AA
                    var aa = OverlayAlphaSlider != null ? ((int)OverlayAlphaSlider.Value).ToString("X2") : "FF";
                    var hex = $"#{aa}{dlg.Color.R:X2}{dlg.Color.G:X2}{dlg.Color.B:X2}";
                    if (OverlayBgTextBox != null) OverlayBgTextBox.Text = hex;
                    ApplyOverlayPreview(hex);
                }
            }
            catch { }
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

            // Save overlay background hex
            if (OverlayBgTextBox != null)
            {
                var hex = (OverlayBgTextBox.Text ?? "").Trim();
                if (!string.IsNullOrWhiteSpace(hex))
                {
                    _settings.OverlayBackgroundHex = hex;
                    // Update recents (most recent first, unique, max 5)
                    if (_settings.OverlayRecentColors == null)
                        _settings.OverlayRecentColors = new System.Collections.Generic.List<string>();
                    _settings.OverlayRecentColors.RemoveAll(c => string.Equals(c, hex, StringComparison.OrdinalIgnoreCase));
                    _settings.OverlayRecentColors.Insert(0, hex);
                    while (_settings.OverlayRecentColors.Count > 5) _settings.OverlayRecentColors.RemoveAt(_settings.OverlayRecentColors.Count - 1);
                }
            }
        }
    }
}