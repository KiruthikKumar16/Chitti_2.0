using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace LineBuddy.Services
{
    public class ConversationEntry
    {
        public DateTime Timestamp { get; set; }
        public string UserQuery { get; set; } = string.Empty;
        public string AiResponse { get; set; } = string.Empty;
    }

    public class ConversationHistoryService
    {
        private static readonly string HistoryPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "LineBuddy",
            "conversation_history.json"
        );

        private List<ConversationEntry> _history = new();
        private readonly AppSettings _settings;

        public ConversationHistoryService(AppSettings settings)
        {
            _settings = settings;
            LoadHistory();
        }

        public void AddEntry(string userQuery, string aiResponse)
        {
            var entry = new ConversationEntry
            {
                Timestamp = DateTime.Now,
                UserQuery = userQuery,
                AiResponse = aiResponse
            };

            _history.Add(entry);

            // Keep only the last 100 entries to prevent unlimited growth
            if (_history.Count > 100)
            {
                _history = _history.Skip(_history.Count - 100).ToList();
            }

            SaveHistory();
        }

        public string GetContextualHistory()
        {
            if (!_settings.ShowConversationHistory || _history.Count == 0)
                return string.Empty;

            var recentEntries = _history
                .TakeLast(_settings.HistoryRangeMessages)
                .ToList();

            if (recentEntries.Count == 0)
                return string.Empty;

            var contextLines = new List<string> { "Recent conversation context:" };
            
            foreach (var entry in recentEntries)
            {
                contextLines.Add($"User: {entry.UserQuery}");
                contextLines.Add($"AI: {entry.AiResponse}");
            }

            return string.Join("\n", contextLines) + "\n\nCurrent query: ";
        }

        public List<ConversationEntry> GetRecentEntries(int count = 10)
        {
            return _history.TakeLast(count).ToList();
        }

        public void ClearHistory()
        {
            _history.Clear();
            SaveHistory();
        }

        private void LoadHistory()
        {
            try
            {
                if (File.Exists(HistoryPath))
                {
                    var json = File.ReadAllText(HistoryPath);
                    _history = JsonConvert.DeserializeObject<List<ConversationEntry>>(json) ?? new();
                }
            }
            catch
            {
                _history = new List<ConversationEntry>();
            }
        }

        private void SaveHistory()
        {
            try
            {
                var directory = Path.GetDirectoryName(HistoryPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonConvert.SerializeObject(_history, Formatting.Indented);
                File.WriteAllText(HistoryPath, json);
            }
            catch
            {
                // Handle save errors gracefully
            }
        }
    }
}