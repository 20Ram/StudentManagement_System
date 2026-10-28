
using System.ComponentModel.DataAnnotations;

namespace CrudOperation.Models;

public class Subject
{
    public int Id { get; set; }

    [Required]
    public int ClassId { get; set; }

    public Class Class { get; set; } = null!;

    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int Credits { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<TeacherAssignment> TeacherAssignments { get; set; }
        = new List<TeacherAssignment>();
}
