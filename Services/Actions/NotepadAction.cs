using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace LineBuddy.Services.Actions
{
    public class NotepadAction : IActionHandler
    {
        public string Name => "notepad";
        public bool IsRisky => false;
        public Task<string> ExecuteAsync(JObject args)
        {
            var svc = new ActionService();
            var text = args?.Value<string>("text") ?? string.Empty;
            var msg = svc.Notepad(text);
            return Task.FromResult(msg);
        }
    }
}
