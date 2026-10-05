namespace CrudOperation.DTOs
{
  public class DocumentResponseDto
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public string FileName { get; set; } 

    public string ContentType { get; set; }

    public long FileSize { get; set; }

    public DateTime UploadedAt { get; set; }

    public string ViewUrl { get; set; }

    public string DownloadUrl { get; set; }
}
}