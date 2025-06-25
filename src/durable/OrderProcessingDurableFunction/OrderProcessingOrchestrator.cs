using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace OrderProcessingDurableFunction;


public static class OrderProcessingOrchestrator
{
    /// <summary>
    /// Orchestrates the order processing workflow.
    /// </summary>
    /// <param name="context">The orchestration context for managing durable function state and activities.</param>
    /// <param name="executionContext">The function execution context for logging and configuration.</param>
    /// <returns>
    /// A string describing the result of the order processing, including success or failure details.
    /// </returns>
    /// <remarks>
    /// This orchestrator handles the entire order processing flow, including:
    /// <list type="number">
    /// <item>Validating inventory</item>
    /// <item>Processing payment</item>
    /// <item>Creating shipment</item>
    /// <item>Sending confirmation email</item>
    /// <item>Waiting for manager approval if order amount exceeds $1000</item>
    /// <item>Finalizing the order</item>
    /// </list>
    /// If any step fails, compensating actions are performed to cancel the order.
    /// The orchestrator also logs detailed information about each step, including whether it is replaying,
    /// which is useful for debugging and understanding the flow of execution.
    /// The orchestrator is designed to be resilient and can handle failures gracefully by performing compensating actions.
    /// </remarks>
    [Function("OrderProcessing_Orchestrator")]
    public static async Task<string> RunOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger("OrderProcessing_Orchestrator");
        var order = context.GetInput<OrderData>()!;

        logger.LogInformation("Starting order processing for Order ID: {OrderId}", order.OrderId);

        var results = new List<string>();

        // This will help you see replay happening
        logger.LogInformation("🔄 Orchestrator starting - IsReplaying: {IsReplaying}", context.IsReplaying);

        try
        {
            // Step 1: Validate inventory
            logger.LogInformation("About to validate inventory - IsReplaying: {IsReplaying}", context.IsReplaying);
            var inventoryResult = await context.CallActivityAsync<bool>("ValidateInventory", order);
            logger.LogInformation("Inventory validated: {Result} - IsReplaying: {IsReplaying}", inventoryResult, context.IsReplaying);

            if (!inventoryResult)
            {
                return "Order failed: Insufficient inventory";
            }
            results.Add("Inventory validated");

            // Step 2: Process payment
            logger.LogInformation("About to process payment - IsReplaying: {IsReplaying}", context.IsReplaying);
            var paymentResult = await context.CallActivityAsync<string>("ProcessPayment", order);
            logger.LogInformation("Payment processed: {Result} - IsReplaying: {IsReplaying}", paymentResult, context.IsReplaying);
            results.Add($"Payment processed: {paymentResult}");

            // Step 3: Create shipment
            logger.LogInformation("About to create shipment - IsReplaying: {IsReplaying}", context.IsReplaying);
            var shipmentResult = await context.CallActivityAsync<string>("CreateShipment", order);
            logger.LogInformation("Shipment created: {Result} - IsReplaying: {IsReplaying}", shipmentResult, context.IsReplaying);
            results.Add($"Shipment created: {shipmentResult}");

            // Step 4: Send confirmation email
            logger.LogInformation("About to send confirmation email - IsReplaying: {IsReplaying}", context.IsReplaying);
            await context.CallActivityAsync("SendConfirmationEmail", order);
            logger.LogInformation("Confirmation email sent - IsReplaying: {IsReplaying}", context.IsReplaying);
            results.Add("Confirmation email sent");

            // Step 5: Wait for external approval (if order > $1000)
            if (order.Amount > 1000)
            {
                logger.LogInformation("Step 5: Waiting for manager approval");

                // Set timeout for 24 hours
                var expiry = context.CurrentUtcDateTime.AddHours(24);
                var timeoutTask = context.CreateTimer(expiry, CancellationToken.None);

                var approvalTask = context.WaitForExternalEvent<bool>("ManagerApproval");

                var winner = await Task.WhenAny(approvalTask, timeoutTask);

                if (winner == approvalTask)
                {
                    var approved = approvalTask.Result;

                    if (approved)
                    {
                        results.Add("Manager approval received");
                    }
                    else
                    {
                        // Cancel the order
                        logger.LogInformation("About to cancel order - IsReplaying: {IsReplaying}", context.IsReplaying);
                        await context.CallActivityAsync("CancelOrder", order);
                        logger.LogInformation("Order cancelled by manager - IsReplaying: {IsReplaying}", context.IsReplaying);
                        return "Order cancelled by manager";
                    }
                }
                else
                {
                    // Timeout occurred
                    logger.LogInformation("About to cancel order - IsReplaying: {IsReplaying}", context.IsReplaying);
                    await context.CallActivityAsync("CancelOrder", order);
                    logger.LogInformation("Order cancelled due to timeout - IsReplaying: {IsReplaying}", context.IsReplaying);
                    return "Order cancelled due to timeout";
                }
            }

            // Step 6: Finalize order            
            logger.LogInformation("About to finalize order - IsReplaying: {IsReplaying}", context.IsReplaying);
            await context.CallActivityAsync("FinalizeOrder", order);
            logger.LogInformation("Order finalized - IsReplaying: {IsReplaying}", context.IsReplaying);
            results.Add("Order finalized");

            return $"Order {order.OrderId} processed successfully. Steps: {string.Join(", ", results)}";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing order {OrderId}: {Message}", order.OrderId, ex.Message);

            // Compensating actions
            await context.CallActivityAsync("CancelOrder", order);
            return $"Order {order.OrderId} failed and was cancelled: {ex.Message}";
        }
    }
}