using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace OrderProcessingDurableFunction;

public static class OrderProcessingActivities
{
    /// <summary>
    /// Validates the inventory for the given order.
    /// </summary>
    /// <param name="order"></param>
    /// <param name="executionContext"></param>
    /// <returns></returns>
    [Function("ValidateInventory")]
    public static async Task<bool> ValidateInventory(
        [ActivityTrigger] OrderData order,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger("ValidateInventory");
        logger.LogInformation("Validating inventory for order {OrderId}", order.OrderId);
        
        // Simulate inventory check
        await Task.Delay(2000);
        
        // Simulate 90% success rate
        var isAvailable = new Random().NextDouble() > 0.1;
        
        logger.LogInformation("Inventory validation result: {IsAvailable}", isAvailable);
        return isAvailable;
    }

    /// <summary>
    /// Processes the payment for the given order.
    /// This activity simulates payment processing and returns a transaction ID.
    /// </summary>
    /// <param name="order"></param>
    /// <param name="executionContext"></param>
    /// <returns></returns>
    [Function("ProcessPayment")]
    public static async Task<string> ProcessPayment(
        [ActivityTrigger] OrderData order,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger("ProcessPayment");
        logger.LogInformation("Processing payment for order {OrderId}, Amount: ${Amount}", order.OrderId, order.Amount);
        
        // Simulate payment processing
        await Task.Delay(3000);
        
        var transactionId = Guid.NewGuid().ToString("N")[..8];
        
        logger.LogInformation("Payment processed successfully. Transaction ID: {TransactionId}", transactionId);
        return transactionId;
    }

    /// <summary>
    /// Creates a shipment for the given order.
    /// This activity simulates shipment creation and returns a tracking number.
    /// </summary>
    /// <param name="order"></param>
    /// <param name="executionContext"></param>
    /// <returns></returns>
    [Function("CreateShipment")]
    public static async Task<string> CreateShipment(
        [ActivityTrigger] OrderData order,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger("CreateShipment");
        logger.LogInformation("Creating shipment for order {OrderId}", order.OrderId);
        
        // Simulate shipment creation
        await Task.Delay(1500);
        
        var trackingNumber = $"TRK{new Random().Next(100000, 999999)}";
        
        logger.LogInformation("Shipment created. Tracking number: {TrackingNumber}", trackingNumber);
        return trackingNumber;
    }

    /// <summary>
    /// Sends a confirmation email for the given order.
    /// This activity simulates sending an email and logs the action.
    /// </summary>
    /// <param name="order"></param>
    /// <param name="executionContext"></param>
    /// <returns></returns>
    [Function("SendConfirmationEmail")]
    public static async Task SendConfirmationEmail(
        [ActivityTrigger] OrderData order,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger("SendConfirmationEmail");
        logger.LogInformation("Sending confirmation email for order {OrderId} to {CustomerEmail}", order.OrderId, order.CustomerEmail);
        
        // Simulate email sending
        await Task.Delay(1000);
        
        logger.LogInformation("Confirmation email sent successfully");
    }

    /// <summary>
    /// Finalizes the order by performing any necessary cleanup or final actions.
    /// This activity simulates finalizing the order and logs the action.
    /// </summary>
    /// <param name="order"></param>
    /// <param name="executionContext"></param>
    /// <returns></returns>
    [Function("FinalizeOrder")]
    public static async Task FinalizeOrder(
        [ActivityTrigger] OrderData order,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger("FinalizeOrder");
        logger.LogInformation("Finalizing order {OrderId}", order.OrderId);
        
        // Simulate order finalization
        await Task.Delay(500);
        
        logger.LogInformation("Order {OrderId} finalized successfully", order.OrderId);
    }

    /// <summary>
    /// Cancels the order by performing compensating actions if any step fails.
    /// This activity simulates order cancellation and logs the action.
    /// </summary>
    /// <param name="order"></param>
    /// <param name="executionContext"></param>
    /// <returns></returns>
    [Function("CancelOrder")]
    public static async Task CancelOrder(
        [ActivityTrigger] OrderData order,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger("CancelOrder");
        logger.LogInformation("Cancelling order {OrderId}", order.OrderId);
        
        // Simulate order cancellation
        await Task.Delay(1000);
        
        logger.LogInformation("Order {OrderId} cancelled successfully", order.OrderId);
    }
}