using CrudOperation.DTOs;
using Org.BouncyCastle.Utilities;

namespace CrudOperation.Service
{
  public interface IDocumentService
  {
    Task<DocumentResponseDto> UploadAsync (DocumentUploadDto dto,int userId,string role);
    Task<List<DocumentResponseDto>> GetByStudentIdAsync(int studentId,int userId,string role);

    Task<(byte[] FileBytes, string ContentType,string FileName)? > GetFileAsync(int documentId,  int userId, string role);
  }
}