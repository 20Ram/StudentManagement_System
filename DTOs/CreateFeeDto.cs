using System.ComponentModel.DataAnnotations;

namespace CrudOperation.DTOs
{
  public class CreateFeeDto
  {
    [Range(1, int.MaxValue)]
    public int StudentId {get; set;}

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Title {get; set;} = string.Empty;
 
    [Range(1, 10000000)]
    public decimal TotalAmount {get; set;}

    [Required]
    public DateTime DueDate {get; set;}
  }
}
