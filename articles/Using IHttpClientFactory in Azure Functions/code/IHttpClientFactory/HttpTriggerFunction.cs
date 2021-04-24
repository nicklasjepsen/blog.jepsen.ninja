using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Diagnostics;

namespace Blog.Jepsen.Ninja
{
    public class HttpTriggerFunction
    {
        private readonly HttpClient httpClient;
        private readonly HttpClient blogClient;
        public HttpTriggerFunction(IHttpClientFactory httpClientFactory, HttpClient httpClient)
        {
            this.httpClient = httpClient;
            blogClient = httpClientFactory.CreateClient("blog");
        }

        [FunctionName("HttpTriggerFunction_InjectedClient")]
        public async Task<IActionResult> HttpTriggerFunction_InjectedClient(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var url = "https://blog.jepsen.ninja";
            var sw = Stopwatch.StartNew();
            var response = await httpClient.GetAsync(url);
            return new OkObjectResult($"{url} returned {response.StatusCode} in {sw.ElapsedMilliseconds}ms.");
        }

        [FunctionName("HttpTriggerFunction_NamedHttpClient")]
        public async Task<IActionResult> HttpTriggerFunction_NamedHttpClient(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
    ILogger log)
        {
            var sw = Stopwatch.StartNew();
            var response = await blogClient.GetAsync(string.Empty);
            return new OkObjectResult($"{blogClient.BaseAddress} returned {response.StatusCode} in {sw.ElapsedMilliseconds}ms.");
        }
    }
}
