namespace CrudOperation.Service
{ 
    public sealed record PayableFee(decimal Amount, int OwnerUserId, string Title);

    public interface IFeePaymentAmountProvider
    {
        Task<PayableFee?> GetPayableFeeAsync(int feeItemId, string currentUserId, bool isAdmin,
            CancellationToken cancellationToken = default);
    }
}
