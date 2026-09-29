namespace CrudOperation.DTOs
{
  public class LoginResponseDto
  {
    public string Token {get; set;} = string.Empty;
    public string Name {get; set;} = string.Empty;
    public int UserId {get; set;}
    public string Email {get; set;} = string.Empty;
    public string Roll {get; set;} = string.Empty;
  }
}