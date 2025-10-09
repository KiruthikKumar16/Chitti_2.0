using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace LineBuddy.Services.Actions
{
    public interface IActionHandler
    {
        string Name { get; }
        bool IsRisky { get; }
        Task<string> ExecuteAsync(JObject args);
    }
}
