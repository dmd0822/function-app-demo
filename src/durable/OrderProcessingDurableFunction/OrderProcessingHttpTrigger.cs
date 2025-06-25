using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace OrderProcessingDurableFunction;


public class OrderProcessingHttpTriggers
{
    private readonly ILogger<OrderProcessingHttpTriggers> _logger;

    public OrderProcessingHttpTriggers(ILogger<OrderProcessingHttpTriggers> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Azure Function that starts a new order processing workflow by scheduling a Durable Function orchestration.
    /// </summary>
    /// <param name="req">
    /// The HTTP request data containing the order details in the request body.
    /// </param>
    /// <param name="client">
    /// The DurableTaskClient used to schedule the orchestration instance.
    /// </param>
    /// <returns>
    /// An <see cref="HttpResponseData"/> containing the instance ID of the started orches
    /// tration and the order ID, or an error message if the request fails.
    /// Returns HTTP 202 Accepted with instance details if successful, or HTTP 400 Bad Request
    /// if an error occurs during processing.
    /// </returns>
    [Function("StartOrderProcessing")]
    public async Task<HttpResponseData> StartOrderProcessing(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "orders/start")] HttpRequestData req,
        [DurableClient] DurableTaskClient client)
    {
        _logger.LogInformation("Starting new order processing workflow");

        try
        {
            var requestBody = await req.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var orderRequest = JsonSerializer.Deserialize<OrderStartRequest>(requestBody!, options);

            // Create order data
            var orderData = new OrderData
            {
                OrderId = Guid.NewGuid().ToString(),
                CustomerEmail = orderRequest!.CustomerEmail,
                Amount = orderRequest.Amount,
                ProductName = orderRequest.ProductName,
                Quantity = orderRequest.Quantity,
                OrderDate = DateTime.UtcNow
            };

            string instanceId = await client.ScheduleNewOrchestrationInstanceAsync("OrderProcessing_Orchestrator", orderData);

            _logger.LogInformation("Started orchestration with ID = '{InstanceId}' for order {OrderId}", instanceId, orderData.OrderId);

            var response = req.CreateResponse(HttpStatusCode.Accepted);
            await response.WriteAsJsonAsync(new
            {
                instanceId,
                orderData.OrderId,
                statusQueryGetUri = $"{req.Url.Scheme}://{req.Url.Authority}/api/orders/{instanceId}/status",
                approvalUri = $"{req.Url.Scheme}://{req.Url.Authority}/api/orders/approve"
            });

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting order processing: {Message}", ex.Message);

            var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await errorResponse.WriteAsJsonAsync(new { error = ex.Message });
            return errorResponse;
        }
    }

    /// <summary>
    /// Azure Function that retrieves the status of a specific Durable Function orchestration instance.
    /// </summary>
    /// <param name="req">
    /// The HTTP request data containing the instance ID in the route.
    /// </param>
    /// <param name="client">
    /// The DurableTaskClient used to query the status of the orchestration instance.
    /// </param>
    /// <param name="instanceId">
    /// The ID of the orchestration instance to retrieve status for, provided in the route.
    /// </param>
    /// <returns>
    /// An <see cref="HttpResponseData"/> containing the status of the orchestration instance
    /// or an error message if the instance is not found or an exception occurs.
    /// Returns HTTP 200 OK with instance details if found, or HTTP 404 Not Found if the instance does not exist.
    /// </returns>
    [Function("GetOrderStatus")]
    public async Task<HttpResponseData> GetOrderStatus(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "orders/{instanceId}/status")] HttpRequestData req,
        [DurableClient] DurableTaskClient client,
        string instanceId)
    {
        _logger.LogInformation("Getting status for instance: {InstanceId}", instanceId);

        try
        {
            var metadata = await client.GetInstanceAsync(instanceId);

            if (metadata == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteAsJsonAsync(new { error = $"Instance {instanceId} not found" });
                return notFoundResponse;
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new
            {
                instanceId = metadata.InstanceId,
                runtimeStatus = metadata.RuntimeStatus.ToString(),
                input = metadata.SerializedInput,
                output = metadata.SerializedOutput,
                createdAt = metadata.CreatedAt,
                lastUpdatedAt = metadata.LastUpdatedAt
            });

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting status for instance {InstanceId}: {Message}", instanceId, ex.Message);

            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new { error = ex.Message });
            return errorResponse;
        }
    }

    /// <summary>
    /// Azure Function that handles approval of orders by raising an approval event to a Durable Function instance.
    /// </summary>
    /// <param name="req">
    /// The HTTP request data containing the approval information in the request body.
    /// </param>
    /// <param name="client">
    /// The DurableTaskClient used to raise the approval event to the orchestrator instance.
    /// </param>
    /// <returns>
    /// An <see cref="HttpResponseData"/> indicating the result of the approval operation.
    /// Returns HTTP 200 OK if the event is sent successfully, or HTTP 400 Bad Request if an error occurs.
    /// </returns>
    [Function("ApproveOrder")]
    public async Task<HttpResponseData> ApproveOrder(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "orders/approve")] HttpRequestData req,
        [DurableClient] DurableTaskClient client)
    {
        try
        {
            var requestBody = await req.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var approvalRequest = JsonSerializer.Deserialize<ApprovalRequest>(requestBody!, options);

            _logger.LogInformation("Sending approval event to instance: {InstanceId}", approvalRequest!.InstanceId);

            await client.RaiseEventAsync(approvalRequest.InstanceId, "ManagerApproval", approvalRequest.Approved);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new
            {
                message = "Approval event sent successfully",
                approved = approvalRequest.Approved
            });

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing approval: {Message}", ex.Message);

            var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await errorResponse.WriteAsJsonAsync(new { error = ex.Message });
            return errorResponse;
        }
    }

    /// <summary>
    /// HTTP-triggered Azure Function to terminate a running orchestration instance.
    /// </summary>
    /// <param name="req">The HTTP request data.</param>
    /// <param name="client">The Durable Task client used to interact with orchestrations.</param>
    /// <param name="instanceId">The ID of the orchestration instance to terminate.</param>
    /// <returns>
    /// An <see cref="HttpResponseData"/> indicating the result of the termination request.
    /// Returns 200 OK if the orchestration was terminated successfully, or 500 Internal Server Error if an exception occurred.
    /// </returns>
    [Function("TerminateOrder")]
    public async Task<HttpResponseData> TerminateOrder(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "orders/{instanceId}/terminate")] HttpRequestData req,
        [DurableClient] DurableTaskClient client,
        string instanceId)
    {
        _logger.LogInformation("Terminating orchestration: {InstanceId}", instanceId);

        try
        {
            await client.TerminateInstanceAsync(instanceId, "Terminated by user request");

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new
            {
                message = $"Orchestration {instanceId} terminated successfully"
            });

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error terminating instance {InstanceId}: {Message}", instanceId, ex.Message);

            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new { error = ex.Message });
            return errorResponse;
        }
    }
}