namespace CrudOperation.Models.Fees;

public class PaymentGatewayEvent
{
    public int Id { get; set; }

    public string Provider { get; set; } = "Razorpay";
    public string ProviderEventId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;

    public string PayloadHash { get; set; } = string.Empty;

    public DateTime ReceivedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAtUtc { get; set; }

    public GatewayEventStatus ProcessingStatus { get; set; }
        = GatewayEventStatus.Received;
}