using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace LineBuddy
{
    public class AppSettings
    {
        public int MessageUpdateIntervalMinutes { get; set; } = 5;
        public List<string> EnabledMessageTypes { get; set; } = new()
        {
            "TimeBasedMessages",
            "SystemHealth",
            "NetworkStatus",
            "ProductivityTips",
            "WeatherUpdates",
            "StockUpdates", 
            "CryptoUpdates",
            "TechNews",
            "Quotes",
            "Jokes"
        };
        
        public bool ShowTimeBasedMessages { get; set; } = true;
        public bool ShowSystemHealth { get; set; } = true;
        public bool ShowNetworkStatus { get; set; } = true;
        public bool ShowProductivityTips { get; set; } = true;
        public bool ShowWeatherUpdates { get; set; } = true;
        public bool ShowStockUpdates { get; set; } = true;
        public bool ShowCryptoUpdates { get; set; } = true;
        public bool ShowTechNews { get; set; } = true;
        
        // Animation settings
        public int TypingAnimationSpeed { get; set; } = 30; // milliseconds per character, 0 = disabled
        
        // Smart Tags settings
        public bool SmartTagsEnabled { get; set; } = true;
        public int SmartTagsPastingSpeed { get; set; } = 50; // milliseconds per character for pasting animation
        public List<string> EnabledBuiltInTags { get; set; } = new() 
        { 
            "/grammar", "/polite", "/casual", "/formal", "/summary", "/expand", "/solve", "/translate", "/screen",
            "/tldr", "/eli5", "/proofread", "/keywords", "/outline", "/action", "/meeting", "/seo",
            "/confident", "/empathy", "/persuasive", "/apology", "/congratulate", "/decline",
            "/debug", "/comment", "/optimize", "/test", "/explain",
            "/joke", "/poem", "/story", "/emoji", "/gen-z", "/pirate", "/shakespeare",
            "/compare", "/pros-cons", "/fact-check", "/stats",
            "/schedule", "/price", "/recipe", "/directions", "/simplify", "/technical", "/linkedin", "/tweet", "/hashtags", "/reply",
            "/continue", "/rephrase", "/questions", "/title", "/tags", "/sentiment", "/readability", "/legal"
        };
        public Dictionary<string, string> CustomTags { get; set; } = new();
        
        // Screenshot settings
        public bool UseScreenshotContext { get; set; } = false; // Default to false as requested
        public bool CaptureActiveWindowOnly { get; set; } = false; // Changed to false - default to full screen
        
        // LLM Configuration settings
        public string LLMProvider { get; set; } = "Google Gemini"; // Default provider
        public string LLMModel { get; set; } = "gemini-2.0-flash-exp"; // Default model
        public string LLMApiKey { get; set; } = ""; // User must configure their own API key
        public string LLMBaseUrl { get; set; } = ""; // Not needed for cloud API providers
        
        // History settings
        public bool ShowConversationHistory { get; set; } = true;
        public int HistoryRangeMessages { get; set; } = 10; // Number of recent messages to include in context
        
        public List<string> StockWatchlist { get; set; } = new() { "AAPL", "GOOGL", "MSFT", "TSLA", "NVDA" };
        public List<string> CryptoWatchlist { get; set; } = new() { "bitcoin", "ethereum", "dogecoin" };
        public string WeatherLocation { get; set; } = "London";
        
        // Automation & Safety
        public bool AllowRiskyActionsWithConfirmation { get; set; } = true;
        public bool EnableMediaActions { get; set; } = true;
        public bool EnableSearchActions { get; set; } = true;
        public bool EnableNotes { get; set; } = true;
        public bool EnableTimers { get; set; } = true;
        public bool EnableScreenshots { get; set; } = true;
        public bool EnableVolume { get; set; } = true;
        public bool EnableFocusBreak { get; set; } = true;
        public bool EnableFoldersFiles { get; set; } = true;
        public bool EnableAppLaunch { get; set; } = true;
        public int MaxOpenTabs { get; set; } = 5;
        
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Chitti",
            "settings.json"
        );

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    return JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch
            {
                // If loading fails, return default settings
            }
            
            return new AppSettings();
        }

        public void Save()
        {
            try
            {
                var directory = Path.GetDirectoryName(SettingsPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                var json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(SettingsPath, json);
            }
            catch
            {
                // Handle save errors gracefully
            }
        }

        public List<string> GetActiveMessageTypes()
        {
            var activeTypes = new List<string>();
            
            if (ShowTimeBasedMessages) activeTypes.Add("TimeBasedMessages");
            if (ShowSystemHealth) activeTypes.Add("SystemHealth");
            if (ShowNetworkStatus) activeTypes.Add("NetworkStatus");
            if (ShowProductivityTips) activeTypes.Add("ProductivityTips");
            if (ShowWeatherUpdates) activeTypes.Add("WeatherUpdates");
            if (ShowStockUpdates) activeTypes.Add("StockUpdates");
            if (ShowCryptoUpdates) activeTypes.Add("CryptoUpdates");
            if (ShowTechNews) activeTypes.Add("TechNews");
            if (EnabledMessageTypes.Contains("Quotes")) activeTypes.Add("Quotes");
            if (EnabledMessageTypes.Contains("Jokes")) activeTypes.Add("Jokes");
            
            return activeTypes.Count > 0 ? activeTypes : new List<string> { "TimeBasedMessages" };
        }
    }
}