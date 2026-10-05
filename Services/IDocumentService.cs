using CrudOperation.DTOs;
using Org.BouncyCastle.Utilities;

namespace CrudOperation.Service
{
  public interface IDocumentService
  {
    Task<DocumentResponseDto> UploadAsync (DocumentUploadDto dto);
    Task<List<DocumentResponseDto>> GetByStudentIdAsync(int studentId);

    Task<(byte[] FileBytes, string ContentType,string FileName)? > GetFileAsync(int documentId);
  }
}