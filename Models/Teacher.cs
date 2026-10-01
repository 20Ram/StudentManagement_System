namespace CrudOperation.Models
{
  public class Teacher
  {
    public int Id {get; set;}
    public int UserId {get; set;}
    public string Name {get; set;}
    public string Email {get; set;}
    public int Age {get; set;}
    public string Department {get; set;}
    public User? User {get; set;}
  }  
}