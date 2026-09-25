namespace CrudOperation.DTOs
{
  public class UpdateStudentDto
  {
    public string Name {get; set;} = string.Empty;
    public string Email {get; set;} = string.Empty;
    public int Age {get; set;}
    public string Course {get; set;} = string.Empty;
    public double Marks {get; set;}
  }
}