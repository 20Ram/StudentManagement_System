namespace CrudOperation.DTOs
{
  public class PaymentOrderResponseDto
  {
    public int PaymentTransactionId {get; set;}
    public string RazorpayOrderId {get; set;} = string.Empty;
    public long AmountInPaise {get; set;}
    public string Currency {get; set;} = "INR";
    public string KeyId {get; set;} = string.Empty;

  }
}