namespace CrudOperation.Models.Fees;

public class FeeAdjustmentAllocation
{
    public int Id { get; set; }

    public int FeeAdjustmentId { get; set; }
    public int FeeInstallmentId { get; set; }

    public decimal Amount { get; set; }

    public FeeAdjustment FeeAdjustment { get; set; } = null!;
    public FeeInstallment FeeInstallment { get; set; } = null!;
}