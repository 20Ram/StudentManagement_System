using System.ComponentModel.DataAnnotations;

namespace CrudOperation.Models;
public class Course
{
    public int Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int DurationYears { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Class> Classes { get; set; } = new List<Class>();
}