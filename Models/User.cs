namespace CrudOperation.Models
{
  public class User
  {
    public int Id {get; set;}
    public string Name {get; set;} = string.Empty;
    public string Email {get; set;} = string.Empty;
    public string Roll {get; set;} = string.Empty;
    public bool IsEmailVerified { get; set; }
  }
}