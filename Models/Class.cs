using System.ComponentModel.DataAnnotations;

namespace CrudOperation.Models;

public class Class
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public Course Course { get; set; } = null!;

    public int Semester { get; set; }

    [MaxLength(10)]
    public string Section { get; set; } = "A";

    [MaxLength(20)]
    public string AcademicYear { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<StudentEnrollment> StudentEnrollments { get; set; }
        = new List<StudentEnrollment>();

    public ICollection<Subject> Subjects { get; set; }
        = new List<Subject>();
}