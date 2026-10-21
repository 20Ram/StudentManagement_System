namespace CrudOperation.Models.Fees;

public class FeeApproval
{
    public int Id { get; set; }

    public int? FeeAdjustmentId { get; set; }
    public int? FeeRefundId { get; set; }

    public int ApproverUserId { get; set; }
    public FeeApprovalDecision Decision { get; set; }
        = FeeApprovalDecision.Pending;

    public string? Comments { get; set; }
    public DateTime? DecidedAtUtc { get; set; }

    public FeeAdjustment? FeeAdjustment { get; set; }
    public FeeRefund? FeeRefund { get; set; }

    public User Approver { get; set; } = null!;
}