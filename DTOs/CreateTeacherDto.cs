using System.ComponentModel.DataAnnotations;

namespace CrudOperation.DTOs
{
  public class CreateTeacherDto()
  {
    public string Name {get; set;}
    public string Email {get; set;}
     public int Age {get; set;}
     public string Department {get; set;}
     public string Roll {get; set;}
  }
}