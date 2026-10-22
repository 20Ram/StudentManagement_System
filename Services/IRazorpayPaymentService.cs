using CrudOperation.DTOs;

namespace CrudOperation.Service
{
    public sealed record PaymentVerificationResult(bool Success, string Message);

    public interface IRazorpayPaymentService
    {
        Task<PaymentOrderResponseDto> CreateOrderAsync(int feeItemId, string userId, bool isAdmin, CancellationToken cancellationToken = default);
        Task<PaymentVerificationResult> VerifyPaymentAsync(VerifyPaymentDto dto, string userId, bool isAdmin, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<PaymentHistoryDto>> GetHistoryAsync(string userId, bool isAdmin, CancellationToken cancellationToken = default);
        Task<PaymentHistoryDto?> GetByIdAsync(int id, string userId, bool isAdmin, CancellationToken cancellationToken = default);
        // Task<bool> HandleWebhookAsync(byte[] rawBody, string? signatureHeader, CancellationToken cancellationToken = default);
    }
}
