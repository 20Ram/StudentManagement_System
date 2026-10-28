
using System.ComponentModel.DataAnnotations;

namespace CrudOperation.DTOs;

public class CreateSubjectDto
{
    [Range(1, int.MaxValue)]
    public int ClassId { get; set; }

    [Required]
    [StringLength(20, MinimumLength = 2)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Range(0, 10)]
    public int Credits { get; set; }
}
