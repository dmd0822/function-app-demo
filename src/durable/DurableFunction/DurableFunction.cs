using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Worker;
using System.Threading.Tasks;

namespace DurableFunction
{
    public static class OrchestratorFunction
    {
        [Function("OrchestratorFunction")]
        public static async Task<string> RunOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context)
        {
            // Example: Call an activity function
            string result = await context.CallActivityAsync<string>("HelloActivity", "Azure");
            return $"Orchestration completed: {result}";
        }
    }

    public static class HelloActivity
    {
        [Function("HelloActivity")]
        public static string SayHello([ActivityTrigger] string name)
        {
            return $"Hello, {name}!";
        }
    }

    public static class HttpStart
    {
        [Function("HttpStart")]
        public static async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req,
            [DurableClient] DurableTaskClient client)
        {
            string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                nameof(OrchestratorFunction.RunOrchestrator));

            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await response.WriteStringAsync($"Started orchestration with ID = {instanceId}");
            return response;
        }
    }
}
