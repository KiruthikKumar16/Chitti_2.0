using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace LineBuddy.Services.Actions
{
    public class RevealFileAction : IActionHandler
    {
        public string Name => "revealFile";
        public bool IsRisky => false;
        public Task<string> ExecuteAsync(JObject args)
        {
            var name = (args?.Value<string>("name") ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name)) return Task.FromResult("⚠️ No file name provided");
            try
            {
                var root = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var files = Directory.EnumerateFiles(root, "*" + name + "*", SearchOption.AllDirectories)
                    .Take(50)
                    .ToList();
                var best = files.FirstOrDefault();
                if (best == null) return Task.FromResult("🔎 File not found");
                Process.Start(new ProcessStartInfo { FileName = "explorer.exe", Arguments = $"/select,\"{best}\"" });
                return Task.FromResult("📄 Revealed file");
            }
            catch (Exception ex)
            {
                return Task.FromResult($"⚠️ Reveal failed: {ex.Message}");
            }
        }
    }
}
