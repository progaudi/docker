using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Progaudi.Tarantool.Bot.Models;

namespace Progaudi.Tarantool.Bot.Controllers
{
    [Route("hook")]
    public class HookController : Controller
    {
        public HookController(IServiceProvider provider)
        {
        }

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
                var trigger = new Trigger();
                trigger.Request.Config.Env.Branch = branch;
                var message = new HttpRequestMessage(HttpMethod.Post, "https://api.travis-ci.org/repo/progaudi%2Ftarantool-docker/requests ");
                message.Headers.Accept.ParseAdd("application/json");
                message.Content = new StringContent(JsonConvert.SerializeObject(trigger), Encoding.UTF8, "application/json");
                message.Headers.Add("Travis-API-Version", "3");
                //message.Headers.Authorization = AuthenticationHeaderValue.Parse();

                return client.SendAsync(message);
            }
        }
    }
}
