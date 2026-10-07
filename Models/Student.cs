namespace CrudOperation.Models 
{
  public class Student
  {
    public int Id {get; set;}
    public int UserId {get; set;}
    public string Name {get; set;} 
    public string Email {get; set;} 
    public int Age {get; set;}
    public string Course {get; set;} 
    public bool IsActive {get; set;} = true;
    public double Marks {get; set;}
    public User? User {get; set;}
    public ICollection<Document> Documents {get; set;} = new List<Document>();
  }
}