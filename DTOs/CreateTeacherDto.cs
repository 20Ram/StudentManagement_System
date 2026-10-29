using System.ComponentModel.DataAnnotations;

namespace CrudOperation.DTOs
{
  public class CreateTeacherDto()
  {
    public string Name {get; set;} = string.Empty;
    public string Email {get; set;} = string.Empty;
     public int Age {get; set;}
     public string? Department {get; set;}
     public string Roll {get; set;} = string.Empty;
  }
}