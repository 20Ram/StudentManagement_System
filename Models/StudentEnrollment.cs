namespace CrudOperation.Models;

public class StudentEnrollment
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public Student Student { get; set; } = null!;

    public int ClassId { get; set; }

    public Class Class { get; set; } = null!;

    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    public bool IsCurrent { get; set; } = true;
}