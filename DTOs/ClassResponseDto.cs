
namespace CrudOperation.DTOs;

public class ClassResponseDto
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public string CourseCode { get; set; } = string.Empty;

    public string CourseName { get; set; } = string.Empty;

    public int Semester { get; set; }

    public string Section { get; set; } = string.Empty;

    public string AcademicYear { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public int StudentCount { get; set; }
}
