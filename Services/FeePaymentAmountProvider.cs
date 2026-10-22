using CrudOperation.Data;
using Microsoft.EntityFrameworkCore;

namespace CrudOperation.Service
{
    public class FeePaymentAmountProvider : IFeePaymentAmountProvider
    {
        private readonly AppDbContext _context;
        public FeePaymentAmountProvider(AppDbContext context) { _context = context; }

        public async Task<PayableFee?> GetPayableFeeAsync(
            int feeItemId, string currentUserId, bool isAdmin,
            CancellationToken cancellationToken = default)
        {
            var fee = await _context.StudentFees
                .AsNoTracking()
                .Include(f => f.Student)
                .SingleOrDefaultAsync(f => f.Id == feeItemId, cancellationToken);

            if (fee == null) return null;

            if (!isAdmin)
            {
                if (!int.TryParse(currentUserId, out int userIdInt)) return null;
                if (fee.Student.UserId != userIdInt) return null;
            }

            decimal due = fee.TotalAmount - fee.PaidAmount;
            if (due <= 0) return null;
            return new PayableFee(due, fee.Student.UserId, fee.Title);
        }
    }
}
