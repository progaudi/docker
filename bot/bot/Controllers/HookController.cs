using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MediaTypeHeaderValue = Microsoft.Net.Http.Headers.MediaTypeHeaderValue;

namespace Progaudi.Tarantool.Bot.Controllers
{
    [Route("hook")]
    public class HookController : Controller
    {
        private static readonly ConcurrentDictionary<string, int> Counters = new ConcurrentDictionary<string, int>();
        private readonly ILogger<HookController> _logger;

        public HookController(ILogger<HookController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var response = Counters.Aggregate(
                new StringBuilder("Stats:").AppendLine(),
                (sb, x) => sb.AppendLine($"progaudi.tarantoo.docker.bot.{x.Key} {x.Value}"),
                sb => sb.ToString());
            return Content(response, new MediaTypeHeaderValue("text/plain") { Encoding = Encoding.UTF8 });
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PushPayload payload)
        {
            var version = payload.Ref.Replace("refs/heads/", string.Empty);
            _logger.LogWarning("Version = {Version}, ref = {Ref}", version, payload.Ref);
            Counters.AddOrUpdate("requests", 1, (s, i) => i + 1);

            var response = await TriggerBuild(version);
            var statusCode = (int) response.StatusCode;
            Counters.AddOrUpdate($"builds{{version=\"{version}\", code={statusCode}}}", 1, (s, i) => i + 1);

            return new ObjectResult(new { version, payload })
            {
                StatusCode = statusCode
            };
        }

        private static async Task<HttpResponseMessage> TriggerBuild(string branch, string tagPrefix = null)
        {
            using (var client = new HttpClient())
            {
                var message = new HttpRequestMessage(HttpMethod.Post, "https://api.travis-ci.org/repo/progaudi%2Ftarantool-docker/requests ");
                message.Headers.Accept.ParseAdd("application/json");
                message.Headers.Add("Travis-API-Version", "3");
                message.Headers.Authorization = new AuthenticationHeaderValue("token", Startup.TravisToken);
                var config = string.IsNullOrEmpty(tagPrefix)
                    ? "{\"request\": {\"branch\":\"develop\",\"config\": {\"env\": {\"TARANTOOL_BRANCH\": \"" + branch + "\"}}}}"
                    : "{\"request\": {\"branch\":\"develop\",\"config\": {\"env\": {\"TARANTOOL_BRANCH\": \"" + branch + "\", \"TARANTOOL_TAG_PREFIX\": \"" + tagPrefix + "\"}}}}";
                message.Content = new StringContent(
                    config,
                    Encoding.UTF8,
                    "application/json");

                return await client.SendAsync(message);
            }
        }

        public class PushPayload
        {
            [JsonProperty("ref")]
            public string Ref { get; set; }
        }
    }
}
