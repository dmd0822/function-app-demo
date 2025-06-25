using System.Text.Json.Serialization;

namespace OrderProcessingDurableFunction;

public class OrderData
{
    public string OrderId { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime OrderDate { get; set; }
}

public class OrderStartRequest
{
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class ApprovalRequest
{
    [JsonPropertyName("instanceId")]
    public string InstanceId { get; set; } = string.Empty;
    
    [JsonPropertyName("approved")]
    public bool Approved { get; set; }
}