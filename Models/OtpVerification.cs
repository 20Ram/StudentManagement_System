namespace CrudOperation.Models
{
  public class OtpVerification
  {
    public int Id {get; set;}
    public string Email {get; set;} = string.Empty;
    public string OtpHash {get; set;} = string.Empty;
    public DateTime ExpiresAt {get; set;}
    public bool IsUsed {get; set;}
    public int Attempts {get; set;}
    public DateTime CreatedAt {get; set;}
  }
}