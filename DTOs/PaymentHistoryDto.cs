namespace CrudOperation.DTOs
{
  public class PaymentHistoryDto
  {
    public int Id {get; set;}
    public int FeeItemId {get; set;}
    public decimal Amount {get; set;}
    public string Currency {get; set;} = "INR";
    public string Status {get; set;} = string.Empty;
    public string RazorpayOrderId {get; set;} =string.Empty;
    public string? RazorpayPaymentId {get; set;}
    public string? FailureReason {get; set;}
    public DateTime CreatedAtUtc {get; set;}
    public DateTime? PaidAtUtc {get; set;}
  }
}
