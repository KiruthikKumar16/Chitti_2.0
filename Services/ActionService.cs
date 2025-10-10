using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using LineBuddy.Models;

namespace LineBuddy.Services
{
    public class ActionService
    {
        private static readonly HttpClient _http = new HttpClient();

        public string Play(string target, string query)
        {
            target = (target ?? string.Empty).Trim().ToLower();
            query = (query ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(query)) 
            {
                return PersonalityManager.Instance.FormatMessage("no_query", null) ?? "⚠️ No query provided";
            }

            var parameters = new Dictionary<string, string> { { "query", query } };

            switch (target)
            {
                case "youtube":
                    return OpenUrl($"https://www.youtube.com/results?search_query={Uri.EscapeDataString(query)}", 
                        PersonalityManager.Instance.FormatMessage("youtube_play", parameters) ?? $"🔊 Playing on YouTube: {query}");
                case "ytmusic":
                case "youtube-music":
                    return OpenUrl($"https://music.youtube.com/search?q={Uri.EscapeDataString(query)}", 
                        PersonalityManager.Instance.FormatMessage("ytmusic_play", parameters) ?? $"🔊 Playing on YouTube Music: {query}");
                case "spotify":
                    return OpenUrl($"spotify:search:{Uri.EscapeDataString(query)}", 
                        PersonalityManager.Instance.FormatMessage("spotify_play", parameters) ?? $"🔊 Searching Spotify: {query}");
                case "local":
                    return PlayLocal(query);
                default:
                    // default to YouTube search
                    return OpenUrl($"https://www.youtube.com/results?search_query={Uri.EscapeDataString(query)}", 
                        PersonalityManager.Instance.FormatMessage("play_default", parameters) ?? $"🔊 Playing: {query}");
            }
        }

        public string Open(string urlOrQuery)
        {
            var text = (urlOrQuery ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(text)) 
            {
                return PersonalityManager.Instance.FormatMessage("nothing_to_open", null) ?? "⚠️ Nothing to open";
            }

            if (Uri.TryCreate(text, UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == "spotify"))
            {
                var parameters = new Dictionary<string, string> { { "url", uri.Host } };
                return OpenUrl(uri.ToString(), 
                    PersonalityManager.Instance.FormatMessage("url_open", parameters) ?? $"🔗 Opening: {uri.Host}");
            }

            // treat as YouTube search if not a URL
            var searchParams = new Dictionary<string, string> { { "query", text } };
            return OpenUrl($"https://www.youtube.com/results?search_query={Uri.EscapeDataString(text)}", 
                PersonalityManager.Instance.FormatMessage("search_default", searchParams) ?? $"🔎 Searching: {text}");
        }

        public async Task<string> UtubeAsync(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query)) return "⚠️ No query provided";
                var url = $"https://www.youtube.com/results?search_query={Uri.EscapeDataString(query)}";
                var html = await _http.GetStringAsync(url);
                // Try to find first watch URL
                var m = Regex.Match(html, @"/watch\?v=[A-Za-z0-9_-]{8,}");
                if (m.Success)
                {
                    var video = "https://www.youtube.com" + m.Value;
                    return OpenUrl(video, $"▶️ Playing: {query}");
                }
                return OpenUrl(url, $"🔎 Searching: {query}");
            }
            catch (Exception ex)
            {
                return $"⚠️ YouTube failed: {ex.Message}";
            }
        }

        public string Youtube(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return "⚠️ No query provided";
            return OpenUrl($"https://www.youtube.com/results?search_query={Uri.EscapeDataString(query)}", $"🔎 YouTube: {query}");
        }

        public string YtMusic(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return "⚠️ No query provided";
            return OpenUrl($"https://music.youtube.com/search?q={Uri.EscapeDataString(query)}", $"🔎 YT Music: {query}");
        }

        public string Spotify(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return "⚠️ No query provided";
            return OpenUrl($"spotify:search:{Uri.EscapeDataString(query)}", $"🔎 Spotify: {query}");
        }

        public string Search(string engine, string query)
        {
            engine = (engine ?? "google").ToLower();
            if (string.IsNullOrWhiteSpace(query)) return "⚠️ No query provided";
            string url = engine switch
            {
                "bing" => $"https://www.bing.com/search?q={Uri.EscapeDataString(query)}",
                "duckduckgo" => $"https://duckduckgo.com/?q={Uri.EscapeDataString(query)}",
                _ => $"https://www.google.com/search?q={Uri.EscapeDataString(query)}"
            };
            return OpenUrl(url, $"🔎 {engine}: {query}");
        }

        public string Lock()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "rundll32.exe",
                    Arguments = "user32.dll,LockWorkStation",
                    UseShellExecute = true
                });
                return "🔒 Locked";
            }
            catch (Exception ex)
            {
                return $"⚠️ Lock failed: {ex.Message}";
            }
        }

        public string Sleep()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "rundll32.exe",
                    Arguments = "powrprof.dll,SetSuspendState 0,1,0",
                    UseShellExecute = true
                });
                return "💤 Sleeping";
            }
            catch (Exception ex)
            {
                return $"⚠️ Sleep failed: {ex.Message}";
            }
        }

        // Volume via key simulation
        public string Volume(string subcommand)
        {
            subcommand = (subcommand ?? string.Empty).Trim().ToLower();
            if (subcommand == "mute") { KeyPress(VK_VOLUME_MUTE); return "🔇 Muted"; }
            if (subcommand == "unmute") { KeyPress(VK_VOLUME_MUTE); return "🔊 Unmuted"; }
            if (subcommand == "up") { for (int i = 0; i < 5; i++) KeyPress(VK_VOLUME_UP); return "🔊 Volume up"; }
            if (subcommand == "down") { for (int i = 0; i < 5; i++) KeyPress(VK_VOLUME_DOWN); return "🔉 Volume down"; }
            return "⚠️ Volume usage: /volume up|down|mute|unmute";
        }

        public async Task<string> ScreenshotAsync()
        {
            try
            {
                var svc = new ScreenshotService();
                var base64 = await svc.CaptureScreenshotAsync();
                var bytes = Convert.FromBase64String(base64);
                var dir = Path.Combine(Path.GetTempPath(), "Chitti_Screenshots");
                Directory.CreateDirectory(dir);
                var file = Path.Combine(dir, $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.jpg");
                File.WriteAllBytes(file, bytes);
                Process.Start(new ProcessStartInfo { FileName = "explorer.exe", Arguments = $"/select,\"{file}\"" });
                return "📸 Screenshot saved";
            }
            catch (Exception ex)
            {
                return $"⚠️ Screenshot failed: {ex.Message}";
            }
        }

        public string Notepad(string text)
        {
            try
            {
                var dir = Path.Combine(Path.GetTempPath(), "Chitti_Notes");
                Directory.CreateDirectory(dir);
                var file = Path.Combine(dir, $"note_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
                File.WriteAllText(file, text ?? string.Empty);
                Process.Start(new ProcessStartInfo { FileName = "notepad.exe", Arguments = $"\"{file}\"", UseShellExecute = true });
                return "📝 Note opened";
            }
            catch (Exception ex)
            {
                return $"⚠️ Notepad failed: {ex.Message}";
            }
        }

        public string Timer(TimeSpan duration, string label = "Timer done")
        {
            Task.Run(async () =>
            {
                await Task.Delay(duration);
                try
                {
                    System.Windows.Application.Current?.Dispatcher?.Invoke(() =>
                    {
                        System.Windows.MessageBox.Show(label, "Chitti", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    });
                }
                catch { }
            });
            return $"⏱️ Timer set for {duration}";
        }

        public string Focus(TimeSpan duration)
        {
            // Minimal: lower volume and set a timer
            for (int i = 0; i < 20; i++) KeyPress(VK_VOLUME_DOWN);
            Timer(duration, "Focus session complete");
            return $"🎯 Focus for {duration}";
        }

        public string Break(TimeSpan duration)
        {
            Timer(duration, "Break over—welcome back!");
            OpenUrl("https://www.youtube.com/results?search_query=desk+stretch+routine", "");
            return $"🧘 Break for {duration}";
        }

        private string PlayLocal(string query)
        {
            try
            {
                var musicDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
                if (!Directory.Exists(musicDir)) return "🎵 No Music folder found";
                var files = Directory.EnumerateFiles(musicDir, "*.*", SearchOption.AllDirectories)
                    .Where(p => p.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase)
                             || p.EndsWith(".wav", StringComparison.OrdinalIgnoreCase)
                             || p.EndsWith(".m4a", StringComparison.OrdinalIgnoreCase)
                             || p.EndsWith(".flac", StringComparison.OrdinalIgnoreCase));
                var best = files.OrderBy(p => LevenshteinDistance(Path.GetFileNameWithoutExtension(p).ToLower(), query.ToLower()))
                                .FirstOrDefault();
                if (best == null) return "🎵 No local match found";
                return OpenUrl(best, $"🎵 Playing local: {Path.GetFileName(best)}");
            }
            catch (Exception ex)
            {
                return $"⚠️ Local play failed: {ex.Message}";
            }
        }

        private string OpenUrl(string url, string confirmation)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                };
                Process.Start(psi);
                return confirmation;
            }
            catch (Exception ex)
            {
                return $"⚠️ Open failed: {ex.Message}";
            }
        }

        // Simple Levenshtein distance for local fuzzy match
        private static int LevenshteinDistance(string a, string b)
        {
            if (string.IsNullOrEmpty(a)) return b?.Length ?? 0;
            if (string.IsNullOrEmpty(b)) return a.Length;
            var d = new int[a.Length + 1, b.Length + 1];
            for (int i = 0; i <= a.Length; i++) d[i, 0] = i;
            for (int j = 0; j <= b.Length; j++) d[0, j] = j;
            for (int i = 1; i <= a.Length; i++)
            {
                for (int j = 1; j <= b.Length; j++)
                {
                    int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            return d[a.Length, b.Length];
        }

        // Key simulation for volume
        private const byte VK_VOLUME_MUTE = 0xAD;
        private const byte VK_VOLUME_DOWN = 0xAE;
        private const byte VK_VOLUME_UP = 0xAF;
        private const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        private static void KeyPress(byte vk)
        {
            keybd_event(vk, 0, KEYEVENTF_EXTENDEDKEY, UIntPtr.Zero);
            keybd_event(vk, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, UIntPtr.Zero);
        }
    }
}
