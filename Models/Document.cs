namespace CrudOperation.Models
{
  public class Document
  {
    public int Id {get; set;}
    public string FileName {get; set;} = string.Empty;
    public string StoredFileName {get; set;} = string.Empty;
    public string FilePath {get; set;} = string.Empty;
    public string ContentType {get; set;} = string.Empty;
    public long FileSize {get; set;}
    public DateTime UploadedAt {get; set;}
    public int StudentId {get; set;}
    public int UserId {get; set;}
    public Student? student {get; set;}
    public User? userUploaded {get; set;}
  }
}