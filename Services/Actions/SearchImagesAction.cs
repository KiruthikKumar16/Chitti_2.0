using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace LineBuddy.Services.Actions
{
    public class SearchImagesAction : IActionHandler
    {
        public string Name => "searchImages";
        public bool IsRisky => false;
        public Task<string> ExecuteAsync(JObject args)
        {
            var svc = new ActionService();
            var query = args?.Value<string>("query") ?? string.Empty;
            var msg = svc.Search("google", (query + " images").Trim());
            return Task.FromResult(msg);
        }
    }
}
