using System.ComponentModel.DataAnnotations;

namespace CrudOperation.DTOs
{
  public class RegisterRequestDto
  {
    [Required]
    public string Name {get; set;} = string.Empty;
    [Required]
    [EmailAddress]
    public string Email {get; set;} = string.Empty;
    [Required]
    public string Roll {get; set;}  = string.Empty;
  }
}