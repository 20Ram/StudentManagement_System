using System.ComponentModel.DataAnnotations;

namespace CrudOperation.DTOs;

public class AssignTeacherDto
{
    [Range(1, int.MaxValue)] 
    public int TeacherId { get; set; }
    [Range(1, int.MaxValue)] 
    public int SubjectId { get; set; }
}