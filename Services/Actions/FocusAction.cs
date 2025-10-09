using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace LineBuddy.Services.Actions
{
    public class FocusAction : IActionHandler
    {
        public string Name => "focus";
        public bool IsRisky => false;
        public Task<string> ExecuteAsync(JObject args)
        {
            var svc = new ActionService();
            var token = args?.Value<string>("duration") ?? "25m";
            var msg = svc.Focus(ParseDuration(token));
            return Task.FromResult(msg);
        }

        private static TimeSpan ParseDuration(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return TimeSpan.FromMinutes(25);
            token = token.ToLower().Trim();
            if (token.EndsWith("ms") && int.TryParse(token[..^2], out var ms)) return TimeSpan.FromMilliseconds(ms);
            if (token.EndsWith("s") && int.TryParse(token[..^1], out var s)) return TimeSpan.FromSeconds(s);
            if (token.EndsWith("m") && int.TryParse(token[..^1], out var m)) return TimeSpan.FromMinutes(m);
            if (token.EndsWith("h") && int.TryParse(token[..^1], out var h)) return TimeSpan.FromHours(h);
            if (int.TryParse(token, out var minutes)) return TimeSpan.FromMinutes(minutes);
            return TimeSpan.FromMinutes(25);
        }
    }
}
