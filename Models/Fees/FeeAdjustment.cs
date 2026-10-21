namespace CrudOperation.Models.Fees;

public class FeeAdjustment
{
    public int Id { get; set; }
    public int AssignmentId { get; set; }

    public FeeAdjustmentType Type { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;

    public FeeAdjustmentStatus Status { get; set; }
        = FeeAdjustmentStatus.PendingApproval;

    public int RequestedByUserId { get; set; }
    public DateTime RequestedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAtUtc { get; set; }

    public StudentFeeAssignment Assignment { get; set; } = null!;

    public ICollection<FeeAdjustmentAllocation> Allocations { get; set; }
        = new List<FeeAdjustmentAllocation>();

    public ICollection<FeeApproval> Approvals { get; set; }
        = new List<FeeApproval>();
}