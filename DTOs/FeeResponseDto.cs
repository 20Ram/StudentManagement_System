namespace CrudOperation.DTOs
{
  public class FeeResponseDto
  {
    public int Id {get; set;}
    public int StudentId {get; set;}
    public string StudentName {get; set;} = string.Empty;
    public string Title {get; set;} = string.Empty;
    public decimal TotalAmount {get; set;}
    public decimal PaidAmount {get; set;}
    public decimal DueAmount {get; set;}
    public DateTime DueDate {get; set;}
    public string Status {get; set;} = string.Empty;
  }
}
