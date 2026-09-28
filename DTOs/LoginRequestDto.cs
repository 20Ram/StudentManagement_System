using System.ComponentModel.DataAnnotations;

namespace CrudOperation.DTOs
{
  public class LoginRequestDto
  {
    [Required]
    [EmailAddress]
    public string Email {get; set;} = string.Empty;

  }
}