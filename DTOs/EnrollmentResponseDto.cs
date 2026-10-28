namespace CrudOperation.DTOs;
 
public class EnrollmentResponseDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string? StudentName { get; set; }
    public string? StudentEmail { get; set; }
    public int ClassId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public int Semester { get; set; }
    public string Section { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
    public DateTime EnrolledAt { get; set; }
    public bool IsCurrent { get; set; }
}