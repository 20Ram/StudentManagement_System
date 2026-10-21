namespace CrudOperation.Models.Fees;

public class FeeLateFeeAssessment
{
    public int Id { get; set; }

    public int FeeInstallmentId { get; set; }
    public int FeePolicyId { get; set; }

    public decimal Amount { get; set; }
    public FeeLateFeeStatus Status { get; set; } = FeeLateFeeStatus.Pending;

    public DateTime AssessedAtUtc { get; set; } = DateTime.UtcNow;

    public FeeInstallment FeeInstallment { get; set; } = null!;
    public FeePolicy FeePolicy { get; set; } = null!;
}