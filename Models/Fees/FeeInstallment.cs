namespace CrudOperation.Models.Fees;

public class FeeInstallment
{
    public int Id { get; set; }
    public int AssignmentId { get; set; }

    public int InstallmentNumber { get; set; }
    public DateTime DueDate { get; set; }

    public decimal OriginalAmount { get; set; }

    public FeeInstallmentStatus Status { get; set; }
        = FeeInstallmentStatus.Pending;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public StudentFeeAssignment Assignment { get; set; } = null!;

    public ICollection<FeePaymentAllocation> PaymentAllocations { get; set; }
        = new List<FeePaymentAllocation>();

    public ICollection<FeeAdjustmentAllocation> AdjustmentAllocations { get; set; }
        = new List<FeeAdjustmentAllocation>();

    public FeeLateFeeAssessment? LateFeeAssessment { get; set; }

    public ICollection<FeeRefundReversal> RefundReversals { get; set; }
        = new List<FeeRefundReversal>();
}