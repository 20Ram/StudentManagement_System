
namespace CrudOperation.DTOs;

public class SubjectResponseDto
{
    public int Id { get; set; }
    public int ClassId { get; set; }
    public string SubjectCode { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public int Credits { get; set; }

    public int Semester { get; set; }
    public string Section { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}
