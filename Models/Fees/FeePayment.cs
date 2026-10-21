namespace CrudOperation.Models.Fees;

public class FeePayment
{
    public int Id { get; set; }
    public int AssignmentId { get; set; }

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";

    public FeePaymentMethod Method { get; set; }
    public FeePaymentStatus Status { get; set; } = FeePaymentStatus.Pending;

    public string? ReceiptNumber { get; set; }
    public string? ProviderOrderId { get; set; }
    public string? ProviderPaymentId { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? IdempotencyKey { get; set; }

    public DateTime? PaidAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public int? RecordedByUserId { get; set; }

    public StudentFeeAssignment Assignment { get; set; } = null!;

    public ICollection<FeePaymentAllocation> Allocations { get; set; }
        = new List<FeePaymentAllocation>();

    public ICollection<FeeRefund> Refunds { get; set; }
        = new List<FeeRefund>();
}