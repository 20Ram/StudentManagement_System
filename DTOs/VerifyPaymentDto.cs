using System.ComponentModel.DataAnnotations;

namespace CrudOperation.DTOs
{
  public class VerifyPaymentDto
  {
    [Range(1,int.MaxValue)]
    public int PaymentTransactionId {get; set;}
    [Required]
    public string RazorpayPaymentId {get; set;} = string.Empty;
    [Required]
    public string RazorpayOrderId {get; set;} = string.Empty;
    [Required]
    public string RazorpaySignature {get; set;} = string.Empty;
  }
}