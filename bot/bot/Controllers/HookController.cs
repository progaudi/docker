using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Progaudi.Tarantool.Bot.Controllers
{
    [Route("hook")]
    public class HookController : Controller
    {
        private static readonly Regex HeadExtractor = new Regex("\"ref\":\"refs/heads/(?<version>1.\\d+)\"", RegexOptions.Compiled | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
        private readonly ILogger<HookController> _logger;

        public HookController(ILogger<HookController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var value = await GetStringFromBody(Request.Body);
            var version = HeadExtractor.Match(value).Groups["version"].Value;
            _logger.LogWarning("Version = {Version}, value = {value}", version, value);

            var statusCode = 200;
            switch (version)
            {
                case "1.6":
                case "1.7":
                case "1.8":
                    var response = await TriggerBuild(version);
                    statusCode = (int) response.StatusCode;
                    break;
            }

            return new ObjectResult(new { version, value })
            {
                StatusCode = statusCode
            };
        }

        private static async Task<string> GetStringFromBody(Stream requestBody)
        {
            using (var memory = new MemoryStream())
            {
                await requestBody.CopyToAsync(memory);
                return Encoding.UTF8.GetString(memory.ToArray());
            }
        }

        private static async Task<HttpResponseMessage> TriggerBuild(string branch)
        {
            using (var client = new HttpClient())
            {
                var message = new HttpRequestMessage(HttpMethod.Post, "https://api.travis-ci.org/repo/progaudi%2Ftarantool-docker/requests ");
                message.Headers.Accept.ParseAdd("application/json");
                message.Headers.Add("Travis-API-Version", "3");
                message.Headers.Authorization = new AuthenticationHeaderValue("token", Startup.TravisToken);
                message.Content = new StringContent(
                    "{\"request\": {\"branch\":\"develop\",\"config\": {\"env\": {\"TARANTOOL_BRANCH\": \"" + branch + "\"}}}}",
                    Encoding.UTF8,
                    "application/json");

                return await client.SendAsync(message);
            }
        }
    }
}
