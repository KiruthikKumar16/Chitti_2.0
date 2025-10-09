using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace LineBuddy.Services.Actions
{
    public class OpenFolderAction : IActionHandler
    {
        public string Name => "openFolder";
        public bool IsRisky => false;
        public Task<string> ExecuteAsync(JObject args)
        {
            var which = (args?.Value<string>("which") ?? "downloads").ToLower();
            string path = which switch
            {
                "downloads" => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
                "documents" => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "music" => Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
                _ => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
            };
            try
            {
                Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
                return Task.FromResult($"📂 Opened {which}");
            }
            catch (Exception ex)
            {
                return Task.FromResult($"⚠️ Open folder failed: {ex.Message}");
            }
        }
    }
}
