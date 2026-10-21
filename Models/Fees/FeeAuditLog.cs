namespace CrudOperation.Models.Fees;

public class FeeAuditLog
{
    public long Id { get; set; }

    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;

    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? Reason { get; set; }

    public int? ActorUserId { get; set; }
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

    public string? CorrelationId { get; set; }
}