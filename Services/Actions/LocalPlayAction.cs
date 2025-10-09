using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace LineBuddy.Services.Actions
{
    public class LocalPlayAction : IActionHandler
    {
        public string Name => "localPlay";
        public bool IsRisky => false;
        public Task<string> ExecuteAsync(JObject args)
        {
            var svc = new ActionService();
            var query = args?.Value<string>("query") ?? string.Empty;
            var msg = svc.Play("local", query);
            return Task.FromResult(msg);
        }
    }
}
