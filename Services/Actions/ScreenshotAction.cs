using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace LineBuddy.Services.Actions
{
    public class ScreenshotAction : IActionHandler
    {
        public string Name => "screenshot";
        public bool IsRisky => false;
        public async Task<string> ExecuteAsync(JObject args)
        {
            var svc = new ActionService();
            return await svc.ScreenshotAsync();
        }
    }
}
