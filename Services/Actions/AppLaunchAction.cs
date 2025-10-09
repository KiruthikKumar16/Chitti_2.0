using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace LineBuddy.Services.Actions
{
    public class AppLaunchAction : IActionHandler
    {
        public string Name => "appLaunch";
        public bool IsRisky => false;
        public Task<string> ExecuteAsync(JObject args)
        {
            var app = (args?.Value<string>("app") ?? string.Empty).ToLower().Trim();
            try
            {
                switch (app)
                {
                    case "calc":
                    case "calculator":
                        Process.Start(new ProcessStartInfo { FileName = "calc.exe", UseShellExecute = true });
                        return Task.FromResult("🧮 Calculator opened");
                    case "paint":
                        Process.Start(new ProcessStartInfo { FileName = "mspaint.exe", UseShellExecute = true });
                        return Task.FromResult("🎨 Paint opened");
                    case "notepad":
                        Process.Start(new ProcessStartInfo { FileName = "notepad.exe", UseShellExecute = true });
                        return Task.FromResult("📝 Notepad opened");
                    default:
                        if (!string.IsNullOrWhiteSpace(app))
                        {
                            Process.Start(new ProcessStartInfo { FileName = app, UseShellExecute = true });
                            return Task.FromResult($"🚀 Launched {app}");
                        }
                        return Task.FromResult("⚠️ Unknown app");
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult($"⚠️ Launch failed: {ex.Message}");
            }
        }
    }
}
