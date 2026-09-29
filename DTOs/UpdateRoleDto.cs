using System.ComponentModel.DataAnnotations;

namespace CrudOperation.DTOs
{
  public class UpdateRoledto()
  {
     [Required]
      public string Roll { get; set; } = string.Empty;
  }
}