namespace CrudOperation.Models
{
    public class PaymentTransaction
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int FeeItemId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public string Method { get; set; } = "Razorpay";
        public string RazorpayOrderId { get; set; } = string.Empty;
        public string? RazorpayPaymentId { get; set; }
        public string Status { get; set; } = PaymentStatus.Pending;
        public string? FailureReason { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAtUtc { get; set; }
    }
}