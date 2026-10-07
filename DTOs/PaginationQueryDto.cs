namespace CrudOperation.DTOs
{
  public class PaginationQueryDto  
  {
    public string Search {get; set;}
    public bool IsActive {get; set;}
    public int PageNumber {get; set;} = 1;
    public int PageSize {get; set;} = 20;
    public string SortBy {get; set;} = "Id";
    public string SortOrder {get; set;} = "asc";
  }
}