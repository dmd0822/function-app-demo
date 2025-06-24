using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Monkey.Services;
using System.Threading.Tasks;

namespace Monkey
{
    public class MonkeyFunction
    {
        private readonly ILogger<MonkeyFunction> _logger;

        public MonkeyFunction(ILogger<MonkeyFunction> logger)
        {
            _logger = logger;
        }

        [Function("GetMonkey")]
        public async Task<HttpResponseData> Run([
            HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]
            HttpRequestData req,
            FunctionContext executionContext)
        {
            var monkeyService = new MonkeyService();
            var monkeys = await monkeyService.GetMonkeys();
            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await response.WriteStringAsync(JsonSerializer.Serialize(monkeys));
            return response;
        }
    }
}
