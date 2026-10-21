namespace CrudOperation.Models.Fees;

public class FeePaymentAllocation
{
    public int Id { get; set; }

    public int FeePaymentId { get; set; }
    public int FeeInstallmentId { get; set; }

    public decimal AllocatedAmount { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public FeePayment FeePayment { get; set; } = null!;
    public FeeInstallment FeeInstallment { get; set; } = null!;
}