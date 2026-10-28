namespace CrudOperation.DTOs;
public class TeacherAssignmentResponseDto
{
    public int Id { get; set; }
    public int TeacherId { get; set; }
    public string? TeacherName { get; set; }
    public int SubjectId { get; set; }
    public string SubjectCode { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public int ClassId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public int Semester { get; set; }
    public string Section { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
}