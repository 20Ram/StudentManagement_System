namespace CrudOperation.DTOs
{
  public class PaginatedResponseDto<T>
  {
    public List<T> Data {get; set;} = new();
    public int PageNumber {get; set;}
    public int TotalRecord {get; set;}
    public int PageSize {get; set;}
    public int TotalPage {get; set;}
  }
}