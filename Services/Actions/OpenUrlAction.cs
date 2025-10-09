using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace LineBuddy.Services.Actions
{
    public class OpenUrlAction : IActionHandler
    {
        public string Name => "openUrl";
        public bool IsRisky => false;
        public Task<string> ExecuteAsync(JObject args)
        {
            var svc = new ActionService();
            var url = args?.Value<string>("url");
            var msg = svc.Open(url);
            return Task.FromResult(msg);
        }
    }
}
