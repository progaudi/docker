using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Progaudi.Tarantool.Bot.Models;

namespace Progaudi.Tarantool.Bot.Controllers
{
    [Route("hook")]
    public class HookController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]PushPayload value)
        {
            switch (value.Ref)
            {
                case "refs/heads/1.6":
                case "refs/heads/1.7":
                case "refs/heads/1.8":
                    var response = await TriggerBuild(value.Ref.Replace("refs/heads/", string.Empty));
                    return new StatusCodeResult((int) response.StatusCode);
            }

            return Ok();
        }

        public Task<HttpResponseMessage> TriggerBuild(string branch)
        {
            using (var client = new HttpClient())
            {
                var message = new HttpRequestMessage(HttpMethod.Post, "https://api.travis-ci.org/repo/progaudi%2Ftarantool-docker/requests ");
                message.Headers.Accept.ParseAdd("application/json");
                var json = "\'{\"request\": {\"branch\":\"develop\",\"config\": {\"env\": {\"TARANTOOL_BRANCH\": \"" + branch + "\"}}}}";
                message.Content = new StringContent(json, Encoding.UTF8, "application/json");
                message.Headers.Add("Travis-API-Version", "3");
                message.Headers.Authorization = AuthenticationHeaderValue.Parse(Startup.TravisToken);

                return client.SendAsync(message);
            }
        }
    }
}
