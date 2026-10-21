namespace CrudOperation.Models.Fees;

public class FeeRefundReversal
{
    public int Id { get; set; }

    public int FeeRefundId { get; set; }
    public int FeeInstallmentId { get; set; }

    public decimal Amount { get; set; }

    public FeeRefund FeeRefund { get; set; } = null!;
    public FeeInstallment FeeInstallment { get; set; } = null!;
}