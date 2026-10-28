using System.ComponentModel.DataAnnotations;

namespace CrudOperation.DTOs;

public class EnrollStudentDto
{
    [Range(1, int.MaxValue)] 
    public int StudentId { get; set; }
    [Range(1, int.MaxValue)] 
    public int ClassId { get; set; }
}