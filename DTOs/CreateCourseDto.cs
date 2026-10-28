using System.ComponentModel.DataAnnotations;

namespace CrudOperation.DTO;

public class CreateCourseDto
{
    [Required]
    [StringLength(20, MinimumLength = 2)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Range(1, 10)]
    public int DurationYears { get; set; }
}