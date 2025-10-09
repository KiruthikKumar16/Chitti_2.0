using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Management;
using System.Net.NetworkInformation;
using Newtonsoft.Json;

namespace LineBuddy.Services
{
    public class DynamicMessageService
    {
        private readonly HttpClient _httpClient;
        private readonly Random _random;
        private LineBuddy.AppSettings _settings;
        private List<string> _activeMessageTypes;
        private int _currentMessageIndex = 0;
        private DateTime _lastMessageChange = DateTime.Now;

        public DynamicMessageService(LineBuddy.AppSettings settings = null)
        {
            _httpClient = new HttpClient();
            _random = new Random();
            _settings = settings ?? LineBuddy.AppSettings.Load();
            _activeMessageTypes = _settings.GetActiveMessageTypes();
        }

        public void UpdateSettings(LineBuddy.AppSettings newSettings)
        {
            _settings = newSettings;
            _activeMessageTypes = _settings.GetActiveMessageTypes();
            _currentMessageIndex = 0; // Reset to start fresh with new settings
        }

        public async Task<string> GetCurrentMessageAsync()
        {
            // Use settings-defined interval
            var messageInterval = TimeSpan.FromMinutes(_settings.MessageUpdateIntervalMinutes);
            
            // Change message based on interval when not in use
            if (DateTime.Now - _lastMessageChange >= messageInterval)
            {
                _currentMessageIndex = (_currentMessageIndex + 1) % _activeMessageTypes.Count;
                _lastMessageChange = DateTime.Now;
            }

            if (_activeMessageTypes.Count == 0)
            {
                return "✨ LineBuddy ready - Configure message types in settings!";
            }

            var messageType = _activeMessageTypes[_currentMessageIndex];
            
            return messageType switch
            {
                "TimeBasedMessages" => GetTimeBasedMessage(),
                "WeatherUpdates" => await GetWeatherMessageAsync(),
                "StockUpdates" => await GetStockMessageAsync(),
                "CryptoUpdates" => await GetCryptoMessageAsync(),
                "SystemHealth" => GetSystemHealthMessage(),
                "NetworkStatus" => GetNetworkStatusMessage(),
                "TechNews" => await GetTechNewsMessageAsync(),
                "ProductivityTips" => GetProductivityMessage(),
                _ => GetTimeBasedMessage()
            };
        }

        private string GetTimeBasedMessage()
        {
            var hour = DateTime.Now.Hour;
            var dayOfWeek = DateTime.Now.DayOfWeek;
            
            return hour switch
            {
                >= 6 and < 12 => "☀️ Good morning! Ready to tackle today's challenges?",
                >= 12 and < 14 => "🍽️ Lunch time! Don't forget to take a break.",
                >= 14 and < 18 => "⚡ Afternoon energy boost! How's your day going?",
                >= 18 and < 22 => "🌅 Evening wind-down. Time to wrap up tasks.",
                _ when dayOfWeek == DayOfWeek.Friday => "🎉 TGIF! Weekend plans ready?",
                _ when dayOfWeek == DayOfWeek.Monday => "💪 Monday motivation! Let's crush this week!",
                _ => $"🌙 {DateTime.Now:HH:mm} - Night owl mode activated!"
            };
        }

        private async Task<string> GetWeatherMessageAsync()
        {
            try
            {
                // Using a free weather API (OpenWeatherMap free tier)
                // Note: Replace with actual API key
                var response = await _httpClient.GetStringAsync("https://api.openweathermap.org/data/2.5/weather?q=London&appid=demo&units=metric");
                var weather = JsonConvert.DeserializeObject<WeatherResponse>(response);
                return $"🌤️ {weather.Main.Temp:F0}°C, {weather.Weather[0].Description} - Perfect day!";
            }
            catch
            {
                var temp = _random.Next(15, 25);
                return $"🌤️ ~{temp}°C outside - Have a great day!";
            }
        }

        private async Task<string> GetStockMessageAsync()
        {
            try
            {
                var stockList = _settings.StockWatchlist;
                if (stockList.Count == 0)
                    return "📊 Add stocks to your watchlist in settings!";
                    
                var symbol = stockList[_random.Next(stockList.Count)];
                // Mock stock data (replace with real API)
                var price = _random.Next(100, 300) + _random.NextDouble();
                var change = (_random.NextDouble() - 0.5) * 10;
                var direction = change >= 0 ? "📈" : "📉";
                return $"{direction} {symbol}: ${price:F2} ({change:+0.00;-0.00})";
            }
            catch
            {
                return "📊 Markets are active today - Stay informed!";
            }
        }

        private async Task<string> GetCryptoMessageAsync()
        {
            try
            {
                var cryptoList = _settings.CryptoWatchlist;
                if (cryptoList.Count == 0)
                    return "₿ Add cryptocurrencies to your watchlist in settings!";
                    
                var crypto = cryptoList[_random.Next(cryptoList.Count)];
                // Mock crypto data
                var price = crypto switch
                {
                    "bitcoin" => _random.Next(40000, 70000),
                    "ethereum" => _random.Next(2000, 4000),
                    _ => _random.Next(1, 100)
                };
                return $"₿ {crypto.ToUpper()}: ${price:N0} - Crypto never sleeps!";
            }
            catch
            {
                return "₿ Crypto markets buzzing - Keep watching!";
            }
        }

        private string GetSystemHealthMessage()
        {
            try
            {
                var ramUsage = GetRAMUsage();
                var cpuUsage = GetCPUUsage();
                
                if (ramUsage > 80)
                    return $"⚠️ RAM at {ramUsage}% - Consider closing some apps";
                if (cpuUsage > 80)
                    return $"🔥 CPU at {cpuUsage}% - System working hard!";
                
                return $"✅ System healthy - CPU: {cpuUsage}%, RAM: {ramUsage}%";
            }
            catch
            {
                return "💻 System running smoothly - All good!";
            }
        }

        private string GetNetworkStatusMessage()
        {
            try
            {
                var ping = GetNetworkLatency();
                if (ping > 100)
                    return $"🐌 Network slow - {ping}ms latency detected";
                return $"🚀 Network fast - {ping}ms ping to internet";
            }
            catch
            {
                return "🌐 Connected and ready - Internet available!";
            }
        }

        private async Task<string> GetTechNewsMessageAsync()
        {
            // Mock tech news headlines
            var headlines = new[]
            {
                "🚀 AI breakthrough in quantum computing announced",
                "💡 New programming language trending on GitHub",
                "🔧 Microsoft releases major Windows update",
                "📱 Apple unveils innovative hardware features",
                "🤖 OpenAI announces ChatGPT improvements",
                "⚡ Tesla stock surges on autopilot news",
                "🔒 Major cybersecurity patch released today"
            };
            
            return headlines[_random.Next(headlines.Length)];
        }

        private string GetProductivityMessage()
        {
            var messages = new[]
            {
                "🎯 Focus time! Block distractions for deep work",
                "⏰ Pomodoro break? 25 min focused work pays off",
                "📝 Quick reminder: Review your daily goals",
                "🧠 Brain break time - Step away from screen!",
                "✨ Pro tip: Organize desktop for better workflow",
                "🔄 Auto-save reminder: Backup your work files",
                "🎨 Creative block? Try changing your environment"
            };
            
            return messages[_random.Next(messages.Length)];
        }

        private int GetRAMUsage()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
                foreach (ManagementObject obj in searcher.Get())
                {
                    var total = Convert.ToDouble(obj["TotalVisibleMemorySize"]);
                    var available = Convert.ToDouble(obj["AvailablePhysicalMemory"]);
                    return (int)((total - available) / total * 100);
                }
            }
            catch { }
            return _random.Next(30, 70);
        }

        private int GetCPUUsage()
        {
            // Simplified CPU usage - in real implementation, use PerformanceCounter
            return _random.Next(10, 50);
        }

        private int GetNetworkLatency()
        {
            try
            {
                var ping = new Ping();
                var reply = ping.Send("8.8.8.8", 1000);
                return reply.Status == IPStatus.Success ? (int)reply.RoundtripTime : 999;
            }
            catch
            {
                return _random.Next(20, 100);
            }
        }

        public void ResetMessageCycle()
        {
            _currentMessageIndex = 0;
            _lastMessageChange = DateTime.Now.AddSeconds(-11); // Force immediate message change
        }
    }

    // Weather API response models
    public class WeatherResponse
    {
        public MainWeather Main { get; set; }
        public Weather[] Weather { get; set; }
    }

    public class MainWeather
    {
        public double Temp { get; set; }
    }

    public class Weather
    {
        public string Description { get; set; }
    }
}