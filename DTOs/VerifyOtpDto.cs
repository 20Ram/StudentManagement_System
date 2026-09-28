using System.ComponentModel.DataAnnotations;

namespace CrudOperation.DTOs
{
  public class VerifyOtpDto
  {
    [Required]
    [EmailAddress]
    public string Email {get; set;} = string.Empty;
    [Required]
    public string Otp {get; set;} = string.Empty;
  }
}