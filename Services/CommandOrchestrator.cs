using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using LineBuddy.Services.Actions;
using LineBuddy.Models;

namespace LineBuddy.Services
{
    public class CommandOrchestrator
    {
        private readonly LLMService _llm;
        private readonly ActionService _actions;
        private readonly AppSettings _settings;
        private readonly Dictionary<string, IActionHandler> _registry;
        private static string _lastPlannedInput = string.Empty;
        private static string _lastPlanJson = string.Empty;

        private static readonly HashSet<string> AllowedSafe = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "openUrl","searchWeb","searchImages","youtubeSearch","ytmusicSearch","spotifySearch","utubeAutoPlay","localPlay",
            "timer","notepad","screenshot","volume","focus","break"
        };

        private static readonly HashSet<string> AllowedRisky = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "lock","sleep"
        };

        public CommandOrchestrator(LLMService llm, ActionService actions, AppSettings settings)
        {
            _llm = llm;
            _actions = actions;
            _settings = settings;
            _registry = new Dictionary<string, IActionHandler>(System.StringComparer.OrdinalIgnoreCase)
            {
                { "openUrl", new OpenUrlAction() },
                { "searchWeb", new SearchWebAction() },
                { "utubeAutoPlay", new UtubeAutoPlayAction() },
                { "notepad", new NotepadAction() },
                { "timer", new TimerAction() },
                { "screenshot", new ScreenshotAction() },
                { "volume", new VolumeAction() },
                { "focus", new FocusAction() },
                { "break", new BreakAction() },
                { "searchImages", new SearchImagesAction() },
                { "localPlay", new LocalPlayAction() },
                { "openFolder", new OpenFolderAction() },
                { "revealFile", new RevealFileAction() },
                { "appLaunch", new AppLaunchAction() }
            };
        }

        public async Task<(bool handled, string message, bool needsConfirm, Func<Task<string>> confirmAction)> HandleAsync(string input)
        {
            // Quick heuristic: if URL present, treat as openUrl
            if (TryOpenUrlFast(input, out var quickMsg))
            {
                return (true, quickMsg, false, null);
            }

            // Local intent heuristics (no-key fallback)
            var text = (input ?? string.Empty).Trim();
            var lower = text.ToLowerInvariant();

            // play <query>
            var mPlay = Regex.Match(text, @"^(play|pls play|please play)\s+(.+)$", RegexOptions.IgnoreCase);
            if (mPlay.Success)
            {
                var q = mPlay.Groups[2].Value.Trim();
                // support "play on youtube music ..." or "play on spotify ..."
                if (Regex.IsMatch(q, @"\bon\s+youtube\s+music\b", RegexOptions.IgnoreCase))
                {
                    q = Regex.Replace(q, @"\bon\s+youtube\s+music\b", string.Empty, RegexOptions.IgnoreCase).Trim();
                    return (true, _actions.YtMusic(q), false, null);
                }
                if (Regex.IsMatch(q, @"\bon\s+spotify\b", RegexOptions.IgnoreCase))
                {
                    q = Regex.Replace(q, @"\bon\s+spotify\b", string.Empty, RegexOptions.IgnoreCase).Trim();
                    return (true, _actions.Spotify(q), false, null);
                }
                // default to utube autoplay
                var msg = await _actions.UtubeAsync(q);
                return (true, msg, false, null);
            }

            // search images: "search <query> images" or "search images for <query>"
            var mImg1 = Regex.Match(lower, @"^search\s+(.+?)\s+images$", RegexOptions.IgnoreCase);
            var mImg2 = Regex.Match(lower, @"^search\s+images\s+for\s+(.+)$", RegexOptions.IgnoreCase);
            if (mImg1.Success || mImg2.Success)
            {
                var q = (mImg1.Success ? mImg1.Groups[1].Value : mImg2.Groups[1].Value).Trim();
                var msg = _actions.Search("google", q + " images");
                return (true, msg, false, null);
            }

            // open notepad <text> | take a note ... | note: ...
            var mNote1 = Regex.Match(lower, @"^(open\s+)?notepad\s+(.+)$", RegexOptions.IgnoreCase);
            var mNote2 = Regex.Match(lower, @"^(take|create)\s+a\s+note\s+(?:of\s+)?(.+)$", RegexOptions.IgnoreCase);
            var mNote3 = Regex.Match(text, @"^note:\s*(.+)$", RegexOptions.IgnoreCase);
            if (mNote1.Success || mNote2.Success || mNote3.Success)
            {
                var noteText = mNote1.Success ? mNote1.Groups[2].Value : mNote2.Success ? mNote2.Groups[2].Value : mNote3.Groups[1].Value;
                var msg = _actions.Notepad(noteText.Trim());
                return (true, msg, false, null);
            }

            // timer: set an alarm|timer for 10m/10 minutes | remind me in 10m
            var mTimer1 = Regex.Match(lower, @"^(set|start)\s+(an\s+)?(alarm|timer)\s+(for\s+)?([0-9]+\s*(ms|s|m|h|minutes?|hours?))$", RegexOptions.IgnoreCase);
            var mTimer2 = Regex.Match(lower, @"^remind\s+me\s+in\s+([0-9]+\s*(ms|s|m|h|minutes?|hours?))$", RegexOptions.IgnoreCase);
            if (mTimer1.Success || mTimer2.Success)
            {
                var durToken = mTimer1.Success ? mTimer1.Groups[5].Value : mTimer2.Groups[1].Value;
                durToken = durToken.Replace("minutes", "m").Replace("minute", "m").Replace("hours", "h").Replace("hour", "h").Trim();
                var msg = _actions.Timer(ParseDuration(durToken));
                return (true, msg, false, null);
            }

            // screenshot: take a screenshot | capture screen
            if (Regex.IsMatch(lower, @"^(take|capture)\s+(a\s+)?(screenshot|screen\s*shot)\b", RegexOptions.IgnoreCase))
            {
                var msg = await _actions.ScreenshotAsync();
                return (true, msg, false, null);
            }

            // volume: mute/unmute/volume up/down
            if (Regex.IsMatch(lower, @"^(mute|unmute)\s+volume$", RegexOptions.IgnoreCase))
            {
                var mode = lower.StartsWith("mute") ? "mute" : "unmute";
                var msg = _actions.Volume(mode);
                return (true, msg, false, null);
            }
            if (Regex.IsMatch(lower, @"^volume\s+(up|down)$", RegexOptions.IgnoreCase) || Regex.IsMatch(lower, @"^(increase|decrease)\s+volume$", RegexOptions.IgnoreCase))
            {
                var mode = lower.Contains("up") || lower.Contains("increase") ? "up" : "down";
                var msg = _actions.Volume(mode);
                return (true, msg, false, null);
            }

            // lock / sleep computer
            if (Regex.IsMatch(lower, @"^lock\s+(pc|computer|workstation)$", RegexOptions.IgnoreCase) || lower.Equals("lock"))
            {
                if (_settings.AllowRiskyActionsWithConfirmation)
                {
                    return (true, "Confirm: lock?", true, async () => _actions.Lock());
                }
            }
            if (Regex.IsMatch(lower, @"^(put|go)\s+to\s+sleep$", RegexOptions.IgnoreCase) || lower.Equals("sleep") || lower.Contains("sleep the computer"))
            {
                if (_settings.AllowRiskyActionsWithConfirmation)
                {
                    return (true, "Confirm: sleep?", true, async () => _actions.Sleep());
                }
            }

            // search web: search for X | google X | bing X | duckduckgo X
            var mSearchFor = Regex.Match(lower, @"^search\s+(for\s+)?(.+)$", RegexOptions.IgnoreCase);
            if (mSearchFor.Success)
            {
                var q = (mSearchFor.Groups[2].Value).Trim();
                var msg = _actions.Search("google", q);
                return (true, msg, false, null);
            }
            var mGoogle = Regex.Match(lower, @"^google\s+(.+)$", RegexOptions.IgnoreCase);
            if (mGoogle.Success)
            {
                var q = mGoogle.Groups[1].Value.Trim();
                var msg = _actions.Search("google", q);
                return (true, msg, false, null);
            }
            var mBing = Regex.Match(lower, @"^bing\s+(.+)$", RegexOptions.IgnoreCase);
            if (mBing.Success)
            {
                var q = mBing.Groups[1].Value.Trim();
                var msg = _actions.Search("bing", q);
                return (true, msg, false, null);
            }
            var mDd = Regex.Match(lower, @"^(ddg|duckduckgo)\s+(.+)$", RegexOptions.IgnoreCase);
            if (mDd.Success)
            {
                var q = mDd.Groups[2].Value.Trim();
                var msg = _actions.Search("duckduckgo", q);
                return (true, msg, false, null);
            }

            string planJson;
            if (string.Equals(input, _lastPlannedInput, StringComparison.OrdinalIgnoreCase))
            {
                planJson = _lastPlanJson;
            }
            else
            {
                var planTask = _llm.QueryActionsAsync(input);
                var completed = await Task.WhenAny(planTask, Task.Delay(1200));
                planJson = completed == planTask ? planTask.Result : string.Empty;
                _lastPlannedInput = input;
                _lastPlanJson = planJson;
            }
            if (string.IsNullOrWhiteSpace(planJson)) return (false, string.Empty, false, null);

            if (!TryParsePlan(planJson, out var action, out var args, out var risk))
            {
                return (false, string.Empty, false, null);
            }

            bool isSafe = string.Equals(risk, "safe", StringComparison.OrdinalIgnoreCase);
            if (isSafe)
            {
                if (!AllowedSafe.Contains(action)) return (false, string.Empty, false, null);
                if (!IsActionEnabledBySettings(action)) 
                {
                    var disabledMsg = PersonalityManager.Instance.FormatMessage("action_disabled", new Dictionary<string, string> { { "action", action } }) 
                                    ?? "⚠️ Action disabled in settings";
                    return (true, disabledMsg, false, null);
                }
                var msg = await ExecuteAsync(action, args);
                return (true, msg, false, null);
            }
            else
            {
                if (!AllowedRisky.Contains(action)) return (false, string.Empty, false, null);
                if (!_settings.AllowRiskyActionsWithConfirmation) return (false, string.Empty, false, null);
                // Prepare confirm callback
                var confirmMsg = PersonalityManager.Instance.FormatMessage("action_confirm", new Dictionary<string, string> { { "action", action } }) 
                                ?? $"Confirm: {action}?";
                return (true, confirmMsg, true, async () => await ExecuteAsync(action, args));
            }
        }

        private bool IsActionEnabledBySettings(string action)
        {
            switch (action.ToLower())
            {
                case "utubeautoplay":
                case "localplay":
                    return _settings.EnableMediaActions;
                case "searchweb":
                case "searchimages":
                case "openurl":
                    return _settings.EnableSearchActions;
                case "notepad":
                    return _settings.EnableNotes;
                case "timer":
                    return _settings.EnableTimers;
                case "screenshot":
                    return _settings.EnableScreenshots;
                case "volume":
                    return _settings.EnableVolume;
                case "focus":
                case "break":
                    return _settings.EnableFocusBreak;
                case "openfolder":
                case "revealfile":
                    return _settings.EnableFoldersFiles;
                case "applaunch":
                    return _settings.EnableAppLaunch;
                default:
                    return true;
            }
        }

        private bool TryParsePlan(string json, out string action, out JObject args, out string risk)
        {
            action = string.Empty; risk = "safe"; args = new JObject();
            try
            {
                var obj = JObject.Parse(json);
                action = obj.Value<string>("action") ?? string.Empty;
                args = (obj["args"] as JObject) ?? new JObject();
                risk = obj.Value<string>("risk") ?? "safe";
                return !string.IsNullOrWhiteSpace(action);
            }
            catch { return false; }
        }

        private async Task<string> ExecuteAsync(string action, JObject args)
        {
            if (_registry.TryGetValue(action, out var handler))
            {
                return await handler.ExecuteAsync(args);
            }

            switch (action.ToLower())
            {
                case "openurl":
                    return _actions.Open(args.Value<string>("url"));
                case "searchweb":
                    return _actions.Search(args.Value<string>("engine") ?? "google", args.Value<string>("query") ?? "");
                case "searchimages":
                    var q = args.Value<string>("query") ?? string.Empty;
                    return _actions.Search("google", q + " images");
                case "youtubesearch":
                    return _actions.Youtube(args.Value<string>("query") ?? "");
                case "utubeautoplay":
                    return await _actions.UtubeAsync(args.Value<string>("query") ?? "");
                case "ytmusicsearch":
                    return _actions.YtMusic(args.Value<string>("query") ?? "");
                case "spotifysearch":
                    return _actions.Spotify(args.Value<string>("query") ?? "");
                case "localplay":
                    return _actions.Play("local", args.Value<string>("query") ?? "");
                case "timer":
                    var token = args.Value<string>("duration") ?? "5m";
                    var ts = ParseDuration(token);
                    return _actions.Timer(ts);
                case "notepad":
                    return _actions.Notepad(args.Value<string>("text") ?? string.Empty);
                case "screenshot":
                    return await _actions.ScreenshotAsync();
                case "volume":
                    return _actions.Volume(args.Value<string>("mode") ?? string.Empty);
                case "focus":
                    return _actions.Focus(ParseDuration(args.Value<string>("duration") ?? "25m"));
                case "break":
                    return _actions.Break(ParseDuration(args.Value<string>("duration") ?? "5m"));
                case "lock":
                    return _actions.Lock();
                case "sleep":
                    return _actions.Sleep();
                default:
                    return string.Empty;
            }
        }

        private static TimeSpan ParseDuration(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return TimeSpan.FromMinutes(5);
            token = token.ToLower().Trim();
            if (token.EndsWith("ms") && int.TryParse(token[..^2], out var ms)) return TimeSpan.FromMilliseconds(ms);
            if (token.EndsWith("s") && int.TryParse(token[..^1], out var s)) return TimeSpan.FromSeconds(s);
            if (token.EndsWith("m") && int.TryParse(token[..^1], out var m)) return TimeSpan.FromMinutes(m);
            if (token.EndsWith("h") && int.TryParse(token[..^1], out var h)) return TimeSpan.FromHours(h);
            if (int.TryParse(token, out var minutes)) return TimeSpan.FromMinutes(minutes);
            return TimeSpan.FromMinutes(5);
        }

        private bool TryOpenUrlFast(string input, out string msg)
        {
            msg = string.Empty;
            var m = Regex.Match(input, @"https?://\S+", RegexOptions.IgnoreCase);
            if (m.Success)
            {
                msg = _actions.Open(m.Value);
                return true;
            }
            return false;
        }
    }
}
