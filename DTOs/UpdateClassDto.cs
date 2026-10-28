
using System.ComponentModel.DataAnnotations;

namespace CrudOperation.DTOs;

public class UpdateClassDto
{
    [Range(1, 20)]
    public int Semester { get; set; }

    [Required]
    [StringLength(10, MinimumLength = 1)]
    public string Section { get; set; } = "A";

    [Required]
    [RegularExpression(
        @"^\d{4}-\d{4}$",
        ErrorMessage = "Academic year must use the format YYYY-YYYY.")]
    public string AcademicYear { get; set; } = string.Empty;
}
