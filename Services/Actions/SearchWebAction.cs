using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace LineBuddy.Services.Actions
{
    public class SearchWebAction : IActionHandler
    {
        public string Name => "searchWeb";
        public bool IsRisky => false;
        public Task<string> ExecuteAsync(JObject args)
        {
            var svc = new ActionService();
            var engine = args?.Value<string>("engine") ?? "google";
            var query = args?.Value<string>("query") ?? string.Empty;
            var msg = svc.Search(engine, query);
            return Task.FromResult(msg);
        }
    }
}
