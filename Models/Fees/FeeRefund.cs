namespace CrudOperation.Models.Fees;

public class FeeRefund
{
    public int Id { get; set; }
    public int FeePaymentId { get; set; }

    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;

    public FeeRefundStatus Status { get; set; }
        = FeeRefundStatus.PendingApproval;

    public bool RestoresLiability { get; set; }

    public string? ProviderRefundId { get; set; }
    public int RequestedByUserId { get; set; }

    public DateTime RequestedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAtUtc { get; set; }
    public DateTime? ProcessedAtUtc { get; set; }

    public FeePayment Payment { get; set; } = null!;

    public ICollection<FeeRefundReversal> Reversals { get; set; }
        = new List<FeeRefundReversal>();

    public ICollection<FeeApproval> Approvals { get; set; }
        = new List<FeeApproval>();
}