namespace CrudOperation.Models.Fees;

public class FeeNotification
{
    public int Id { get; set; }

    public string RecipientEmail { get; set; } = string.Empty;
    public int AssignmentId { get; set; }
    public int? InstallmentId { get; set; }

    public FeeNotificationType Type { get; set; }
    public FeeNotificationStatus Status { get; set; }
        = FeeNotificationStatus.Pending;

    public DateTime ScheduledAtUtc { get; set; }
    public DateTime? SentAtUtc { get; set; }

    public int RetryCount { get; set; }
    public string? LastError { get; set; }
    public string DeduplicationKey { get; set; } = string.Empty;

    public StudentFeeAssignment Assignment { get; set; } = null!;
    public FeeInstallment? Installment { get; set; }
}