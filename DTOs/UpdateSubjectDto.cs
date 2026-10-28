
using System.ComponentModel.DataAnnotations;

namespace CrudOperation.DTOs.Subjects;

public class UpdateSubjectDto
{
    [Required]
    [StringLength(20, MinimumLength = 2)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Range(0, 10)]
    public int Credits { get; set; }
}
