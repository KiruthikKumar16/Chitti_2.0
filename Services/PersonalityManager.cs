using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using LineBuddy.Models;

namespace LineBuddy.Services
{
    public class PersonalityManager
    {
        private static PersonalityManager _instance;
        private static readonly object _lock = new object();
        
        private List<Personality> _availablePersonalities;
        private Personality _currentPersonality;
        private string _personalitiesPath;

        public static PersonalityManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new PersonalityManager();
                        }
                    }
                }
                return _instance;
            }
        }

        private PersonalityManager()
        {
            _personalitiesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Personalities");
            LoadAllPersonalities();
        }

        public IReadOnlyList<Personality> GetAllPersonalities()
        {
            return _availablePersonalities?.AsReadOnly() ?? (IReadOnlyList<Personality>)Array.Empty<Personality>();
        }

        public Personality GetCurrentPersonality()
        {
            return _currentPersonality ?? GetDefaultPersonality();
        }

        public Personality GetPersonality(string personalityId)
        {
            if (_availablePersonalities == null) return GetDefaultPersonality();
            
            // Use for loop instead of LINQ for better performance
            for (int i = 0; i < _availablePersonalities.Count; i++)
            {
                if (_availablePersonalities[i].Id.Equals(personalityId, StringComparison.OrdinalIgnoreCase))
                    return _availablePersonalities[i];
            }
            return GetDefaultPersonality();
        }

        public bool SetPersonality(string personalityId)
        {
            var personality = GetPersonality(personalityId);
            if (personality != null)
            {
                _currentPersonality = personality;
                return true;
            }
            return false;
        }

        public string GetSystemPrompt()
        {
            return GetCurrentPersonality()?.SystemPrompt ?? GetDefaultPersonality().SystemPrompt;
        }

        public string FormatMessage(string messageType, Dictionary<string, string> parameters = null)
        {
            var personality = GetCurrentPersonality();
            
            if (personality?.MessageStyles?.ContainsKey(messageType) == true)
            {
                var template = personality.MessageStyles[messageType];
                
                if (parameters == null || parameters.Count == 0)
                    return template;
                
                if (parameters.Count == 1)
                {
                    // Single parameter - use simple Replace
                    var param = parameters.First();
                    return template.Replace($"{{{param.Key}}}", param.Value);
                }
                
                // Multiple parameters - use StringBuilder
                var sb = new System.Text.StringBuilder(template);
                foreach (var param in parameters)
                {
                    sb.Replace($"{{{param.Key}}}", param.Value);
                }
                return sb.ToString();
            }
            
            return GetDefaultMessage(messageType, parameters);
        }

        private void LoadAllPersonalities()
        {
            _availablePersonalities = new List<Personality>();
            
            if (!Directory.Exists(_personalitiesPath))
            {
                // Create default personality if folder doesn't exist
                _availablePersonalities.Add(GetDefaultPersonality());
                return;
            }

            var jsonFiles = Directory.GetFiles(_personalitiesPath, "*.json");
            
            foreach (var file in jsonFiles)
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var personality = JsonConvert.DeserializeObject<Personality>(json);
                    if (personality != null)
                    {
                        _availablePersonalities.Add(personality);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to load personality from {file}: {ex.Message}");
                }
            }

            // Ensure we have at least the default personality
            if (_availablePersonalities.Count == 0)
            {
                _availablePersonalities.Add(GetDefaultPersonality());
            }

            // Set default personality
            _currentPersonality = _availablePersonalities.FirstOrDefault(p => p.Id == "chitti") 
                                 ?? _availablePersonalities.First();
        }

        private static readonly Personality _defaultPersonality = CreateDefaultPersonality();

        private static Personality CreateDefaultPersonality()
        {
            return new Personality
            {
                Id = "chitti",
                Name = "Chitti",
                DisplayName = "Chitti (Default)",
                Description = "Witty, concise, friendly AI assistant",
                Icon = "🤖",
                Voice = "witty, concise, friendly, slightly playful",
                Tone = "confident, uplifting, helpful, professional",
                Catchphrases = new List<string> { "Done!", "Need anything else?", "Here you go!", "Easy peasy!" },
                SystemPrompt = "You are Chitti, a quick, witty assistant. Respond in one single line (no line breaks), max ~30 words, no markdown. Be helpful, concrete, friendly, and slightly playful while staying professional.",
                MessageStyles = new Dictionary<string, string>
                {
                    { "greeting", "Hey there! Ready to help! 😊" },
                    { "task_complete", "Done! {task} completed successfully. Need anything else?" },
                    { "error", "Oops! {error}. Let me try that again." },
                    { "confirmation", "Got it! {task} coming right up!" },
                    { "screenshot", "Screenshot saved to your Pictures folder! 📸" },
                    { "search", "Found some results for '{query}'! Check your browser." },
                    { "timer", "Timer set for {duration}! ⏰" },
                    { "volume", "Volume adjusted to {level}%! 🔊" }
                },
                Constraints = new PersonalityConstraints
                {
                    MaxWords = 30,
                    SingleLine = true,
                    Formality = "casual"
                }
            };
        }

        private Personality GetDefaultPersonality()
        {
            return _defaultPersonality;
        }

        private string GetDefaultMessage(string messageType, Dictionary<string, string> parameters)
        {
            var defaultMessages = new Dictionary<string, string>
            {
                { "greeting", "Hello! How can I help you?" },
                { "task_complete", "Task completed successfully." },
                { "error", "An error occurred. Please try again." },
                { "confirmation", "Got it! Processing your request." },
                { "screenshot", "Screenshot saved to Pictures folder." },
                { "search", "Search completed. Check your browser." },
                { "timer", "Timer set successfully." },
                { "volume", "Volume adjusted." },
                { "time_based", "Good day! Ready to help." },
                { "system_healthy", "System running smoothly." },
                { "system_warning", "System warning detected." },
                { "system_error", "System monitoring unavailable." },
                { "network_fast", "Network connection is fast." },
                { "network_slow", "Network connection is slow." },
                { "network_error", "Network status uncertain." },
                { "action_disabled", "Action is disabled in settings." },
                { "action_confirm", "Confirm this action?" },
                { "no_query", "No query provided." },
                { "nothing_to_open", "Nothing to open." },
                { "youtube_play", "Playing on YouTube." },
                { "ytmusic_play", "Playing on YouTube Music." },
                { "spotify_play", "Searching Spotify." },
                { "play_default", "Playing media." },
                { "url_open", "Opening URL." },
                { "search_default", "Searching." },
                { "smart_tags_listening", "Smart Tags: Listening" },
                { "smart_tags_processing", "Smart Tags: Processing" },
                { "smart_tags_pasting", "Smart Tags: Pasting" },
                { "smart_tags_error", "Smart Tags: Error" },
                { "smart_tags_disabled", "Smart Tags: Disabled" },
                { "weather_update", "Weather: {temp}°C with {wind} km/h winds in {location}" },
                { "weather_error", "Weather data unavailable" },
                { "crypto_update", "{symbol} at ${price}" },
                { "crypto_error", "Crypto data unavailable" },
                { "stock_update", "USD→EUR {eur}, USD→INR {inr}" },
                { "stock_error", "Market data unavailable" },
                { "tech_news", "News: {headline}" },
                { "tech_news_error", "News unavailable" },
                { "productivity_tip", "Tip: {tip}" },
                { "quote_message", "\"{quote}\" — {author}" },
                { "quote_error", "Quotes unavailable" },
                { "joke_message", "{setup} — {punchline}" },
                { "joke_error", "Jokes unavailable" },
                { "no_active_messages", "Configure message types in settings" }
            };

            var message = defaultMessages.ContainsKey(messageType) ? defaultMessages[messageType] : "Task completed.";
            
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    message = message.Replace($"{{{param.Key}}}", param.Value);
                }
            }
            
            return message;
        }

        public void RefreshPersonalities()
        {
            LoadAllPersonalities();
        }
    }
}
