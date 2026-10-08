using CrudOperation.DTOs;
namespace CrudOperation.Service
{
  public interface IDocumentService
  {
    Task<DocumentResponseDto> UploadAsync (DocumentUploadDto dto,int userId,string roll);
    Task<List<DocumentResponseDto>> GetAllAsync(string? email, string? fileName, int userId, string roll);
    Task<List<DocumentResponseDto>> GetByStudentIdAsync(int studentId,int userId,string roll);
    Task<(byte[] FileBytes, string ContentType,string FileName)? > GetFileAsync(int documentId,  int userId, string roll);
     
    Task DeleteAsync(int documentId,int userId,string roll);}
}