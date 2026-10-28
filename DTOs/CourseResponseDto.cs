namespace CrudOperation.DTOs;

public class CourseResponseDto
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int DurationYears { get; set; }

    public bool IsActive { get; set; }

    public int ClassCount { get; set; }
}