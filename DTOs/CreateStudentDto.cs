namespace CrudOperation.DTOs
{
  public class CreateStudentDto
  {
    public string Name {get; set;} 
    public string Email {get; set;} 
    public int Age {get; set;}
    public string Course {get; set;} 
    public double Marks {get; set;}
    public string Roll {get; set;}
  }
}