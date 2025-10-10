using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Management;
using System.Net.NetworkInformation;
using Newtonsoft.Json;
using LineBuddy.Models;

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
                return PersonalityManager.Instance.FormatMessage("no_active_messages", null);
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
                "Quotes" => await GetQuoteMessageAsync(),
                "Jokes" => await GetJokeMessageAsync(),
                _ => GetTimeBasedMessage()
            };
        }

        private string GetTimeBasedMessage()
        {
            var hour = DateTime.Now.Hour;
            var dayOfWeek = DateTime.Now.DayOfWeek;
            var timeOfDay = GetTimeOfDay(hour);
            
            var parameters = new Dictionary<string, string>
            {
                { "timeofday", timeOfDay },
                { "hour", hour.ToString() },
                { "dayofweek", dayOfWeek.ToString() }
            };
            
            return PersonalityManager.Instance.FormatMessage("time_based", parameters);
        }

        private string GetTimeOfDay(int hour)
        {
            return hour switch
            {
                >= 6 and < 12 => "morning",
                >= 12 and < 14 => "lunch",
                >= 14 and < 18 => "afternoon",
                >= 18 and < 22 => "evening",
                _ => "night"
            };
        }

        private async Task<string> GetWeatherMessageAsync()
        {
            try
            {
                // Geocode location using Open-Meteo Geocoding (no key)
                var location = string.IsNullOrWhiteSpace(_settings.WeatherLocation) ? "London" : _settings.WeatherLocation;
                var geoUrl = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(location)}&count=1";
                var geoJson = await _httpClient.GetStringAsync(geoUrl);
                dynamic geo = JsonConvert.DeserializeObject(geoJson);
                double lat = (double)(geo?.results?[0]?.latitude ?? 51.5072);
                double lon = (double)(geo?.results?[0]?.longitude ?? -0.1276);

                // Fetch current weather from Open-Meteo (no key)
                var weatherUrl = $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&current_weather=true";
                var wJson = await _httpClient.GetStringAsync(weatherUrl);
                dynamic w = JsonConvert.DeserializeObject(wJson);
                double temp = (double)(w?.current_weather?.temperature ?? 20);
                double wind = (double)(w?.current_weather?.windspeed ?? 0);
                
                var parameters = new Dictionary<string, string>
                {
                    { "temp", temp.ToString("F0") },
                    { "wind", wind.ToString("F0") },
                    { "location", location }
                };
                
                return PersonalityManager.Instance.FormatMessage("weather_update", parameters);
            }
            catch
            {
                return PersonalityManager.Instance.FormatMessage("weather_error", null);
            }
        }

        private async Task<string> GetStockMessageAsync()
        {
            try
            {
                // Replace stocks with exchange rates (free, no key)
                var fxJson = await _httpClient.GetStringAsync("https://api.exchangerate.host/latest?base=USD&symbols=EUR,INR,JPY");
                dynamic fx = JsonConvert.DeserializeObject(fxJson);
                double eur = (double)(fx?.rates?.EUR ?? 0);
                double inr = (double)(fx?.rates?.INR ?? 0);
                
                var parameters = new Dictionary<string, string>
                {
                    { "eur", eur.ToString("F2") },
                    { "inr", inr.ToString("F2") }
                };
                
                return PersonalityManager.Instance.FormatMessage("stock_update", parameters);
            }
            catch
            {
                return PersonalityManager.Instance.FormatMessage("stock_error", null);
            }
        }

        private async Task<string> GetCryptoMessageAsync()
        {
            try
            {
                // CoinGecko simple price (no key)
                var ids = _settings.CryptoWatchlist.Count > 0 ? string.Join(",", _settings.CryptoWatchlist) : "bitcoin,ethereum";
                var url = $"https://api.coingecko.com/api/v3/simple/price?ids={Uri.EscapeDataString(ids)}&vs_currencies=usd";
                var json = await _httpClient.GetStringAsync(url);
                dynamic prices = JsonConvert.DeserializeObject(json);
                
                if (prices == null)
                {
                    return PersonalityManager.Instance.FormatMessage("crypto_error", null);
                }
                
                if (prices.bitcoin != null)
                {
                    double btc = (double)(prices.bitcoin.usd ?? 0);
                    var parameters = new Dictionary<string, string>
                    {
                        { "symbol", "BTC" },
                        { "price", btc.ToString("N0") }
                    };
                    return PersonalityManager.Instance.FormatMessage("crypto_update", parameters);
                }
                
                foreach (var id in _settings.CryptoWatchlist)
                {
                    var node = prices[id];
                    if (node != null)
                    {
                        double px = (double)(node.usd ?? 0);
                        var parameters = new Dictionary<string, string>
                        {
                            { "symbol", id.ToUpper() },
                            { "price", px.ToString("N0") }
                        };
                        return PersonalityManager.Instance.FormatMessage("crypto_update", parameters);
                    }
                }
                
                return PersonalityManager.Instance.FormatMessage("crypto_error", null);
            }
            catch
            {
                return PersonalityManager.Instance.FormatMessage("crypto_error", null);
            }
        }

        private string GetSystemHealthMessage()
        {
            try
            {
                var ramUsage = GetRAMUsage();
                var cpuUsage = GetCPUUsage();
                
                var parameters = new Dictionary<string, string>
                {
                    { "ramusage", ramUsage.ToString() },
                    { "cpuusage", cpuUsage.ToString() }
                };
                
                string messageType = (ramUsage > 80 || cpuUsage > 80) ? "system_warning" : "system_healthy";
                
                return PersonalityManager.Instance.FormatMessage(messageType, parameters);
            }
            catch
            {
                return PersonalityManager.Instance.FormatMessage("system_error", null);
            }
        }

        private string GetNetworkStatusMessage()
        {
            try
            {
                var ping = GetNetworkLatency();
                var parameters = new Dictionary<string, string>
                {
                    { "ping", ping.ToString() }
                };
                
                string messageType = ping > 100 ? "network_slow" : "network_fast";
                
                return PersonalityManager.Instance.FormatMessage(messageType, parameters);
            }
            catch
            {
                return PersonalityManager.Instance.FormatMessage("network_error", null);
            }
        }

        private async Task<string> GetTechNewsMessageAsync()
        {
            try
            {
                // Hacker News Algolia front page (no key)
                var json = await _httpClient.GetStringAsync("https://hn.algolia.com/api/v1/search?tags=front_page");
                dynamic data = JsonConvert.DeserializeObject(json);
                string title = data?.hits?[0]?.title;
                
                if (!string.IsNullOrWhiteSpace(title))
                {
                    var parameters = new Dictionary<string, string>
                    {
                        { "headline", title }
                    };
                    return PersonalityManager.Instance.FormatMessage("tech_news", parameters);
                }
            }
            catch { }
            
            return PersonalityManager.Instance.FormatMessage("tech_news_error", null);
        }

        private string GetProductivityMessage()
        {
            var tips = new[]
            {
                "Focus time! Block distractions for deep work",
                "Pomodoro break? 25 min focused work pays off",
                "Quick reminder: Review your daily goals",
                "Brain break time - Step away from screen!",
                "Organize desktop for better workflow",
                "Auto-save reminder: Backup your work files",
                "Creative block? Try changing your environment"
            };
            
            var selectedTip = tips[_random.Next(tips.Length)];
            var parameters = new Dictionary<string, string>
            {
                { "tip", selectedTip }
            };
            
            return PersonalityManager.Instance.FormatMessage("productivity_tip", parameters);
        }

        private async Task<string> GetQuoteMessageAsync()
        {
            try
            {
                var json = await _httpClient.GetStringAsync("https://zenquotes.io/api/random");
                dynamic arr = JsonConvert.DeserializeObject(json);
                string q = arr?[0]?.q;
                string a = arr?[0]?.a;
                
                if (!string.IsNullOrWhiteSpace(q) && !string.IsNullOrWhiteSpace(a))
                {
                    var parameters = new Dictionary<string, string>
                    {
                        { "quote", q },
                        { "author", a }
                    };
                    return PersonalityManager.Instance.FormatMessage("quote_message", parameters);
                }
            }
            catch { }
            
            return PersonalityManager.Instance.FormatMessage("quote_error", null);
        }

        private async Task<string> GetJokeMessageAsync()
        {
            try
            {
                var json = await _httpClient.GetStringAsync("https://official-joke-api.appspot.com/jokes/random");
                dynamic joke = JsonConvert.DeserializeObject(json);
                string setup = joke?.setup;
                string punch = joke?.punchline;
                
                if (!string.IsNullOrWhiteSpace(setup) && !string.IsNullOrWhiteSpace(punch))
                {
                    var parameters = new Dictionary<string, string>
                    {
                        { "setup", setup },
                        { "punchline", punch }
                    };
                    return PersonalityManager.Instance.FormatMessage("joke_message", parameters);
                }
            }
            catch { }
            
            return PersonalityManager.Instance.FormatMessage("joke_error", null);
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
            return 0;
        }

        private int GetCPUUsage()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT LoadPercentage FROM Win32_Processor");
                foreach (ManagementObject obj in searcher.Get())
                {
                    return Convert.ToInt32(obj["LoadPercentage"]);
                }
            }
            catch { }
            return 0;
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
                return 999;
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