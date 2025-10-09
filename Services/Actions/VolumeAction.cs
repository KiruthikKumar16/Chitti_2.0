using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace LineBuddy.Services.Actions
{
    public class VolumeAction : IActionHandler
    {
        public string Name => "volume";
        public bool IsRisky => false;
        public Task<string> ExecuteAsync(JObject args)
        {
            var svc = new ActionService();
            var mode = args?.Value<string>("mode") ?? string.Empty;
            var msg = svc.Volume(mode);
            return Task.FromResult(msg);
        }
    }
}
