using CrudOperation.Data;
using CrudOperation.DTOs;
using CrudOperation.Models;
using Microsoft.EntityFrameworkCore;
namespace CrudOperation.Service
{
  public class DocumentService : IDocumentService
  {
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public DocumentService(AppDbContext context, IWebHostEnvironment environment)
    {
      _context = context;
      _environment = environment;
    }
    public async Task<DocumentResponseDto> UploadAsync(DocumentUploadDto dto,int userId,string roll)
    {
      var student =await _context.Students.FindAsync(dto.StudentId);
      if(student == null)
      {
        throw new Exception ("Student Not Found.");
      }
      if(dto.File == null || dto.File.Length == 0)
      {
        throw new Exception ("File is reqired");
      }
      var allowedExtenstion = new[]
      {
        ".pdf",
            ".jpg",
            ".jpeg",
            ".png",
            ".doc",
            ".docxpdf",
      };
      var extension = Path.GetExtension(dto.File.FileName);
      if (!allowedExtenstion.Contains(extension))
      {
        throw new Exception ("File Type is Not Found.");
      }

      var maxFileSize = 5 * 1024 * 1024;
      if (dto.File.Length > maxFileSize)
      {
        throw new Exception ("File Size is More than 5 MB.");
      }
      
      if (student == null) 
      { 
        throw new Exception("Student not found."); 
      }
       
      if (roll.Equals("Student", StringComparison.OrdinalIgnoreCase)) 
      { if (student.UserId != userId) 
        { 
          throw new UnauthorizedAccessException( "Students can only upload their own documents."); 
        }
      }

      var uploadFolder = Path.Combine(_environment.WebRootPath,"Uploads","Documents");
      if (!Directory.Exists(uploadFolder))
      {
         Directory.CreateDirectory(uploadFolder);      
      }
      var storedFileName = $"{Guid.NewGuid()}{extension}";
      var fullPath = Path.Combine(uploadFolder,storedFileName);

      using (var stream =new FileStream(
           fullPath,
           FileMode.Create
      ))
      {
        await dto.File.CopyToAsync(stream);
      }

      var document = new Document
      {
        StudentId = dto.StudentId,
        FileName = dto.File.FileName,
        UserId = userId,
        StoredFileName = storedFileName,
        FilePath = $"/Upload/document/{storedFileName}",
        ContentType = dto.File.ContentType,
        FileSize = dto.File.Length,
        UploadedAt = DateTime.UtcNow
      };
      _context.Add(document);
      await _context.SaveChangesAsync();
      return new DocumentResponseDto
      {
            Id = document.Id,

            StudentId = document.StudentId,
            UploadUserId = userId,

            FileName = document.FileName,

            ContentType = document.ContentType,

            FileSize = document.FileSize,

            UploadedAt = document.UploadedAt,

            ViewUrl = $"/api/documents/{document.Id}/view",

            DownloadUrl =
                $"/api/documents/{document.Id}/download"
      };
    }
    public async Task<(byte[] FileBytes, string ContentType,string FileName)? > GetFileAsync(int documentId, int userId, string roll)
    {
      var document = await _context.Documents
                     .Include(d => d.student)
                     .FirstOrDefaultAsync( d => d.Id == documentId);
      if (document == null)
      {
        return null;
      }

      if (roll.Equals( "Student", StringComparison.OrdinalIgnoreCase)) 
      { 
        if (document.student == null || document.student.UserId != userId) 
        { 
          throw new UnauthorizedAccessException( "Students can only access their own documents.");
        }
      }
      var fullPath = Path.Combine(
        _environment.WebRootPath,
        "uploads",
        "documents",
        document.StoredFileName
      );
      if (!File.Exists(fullPath))
      {
        return null;
      }
      var butes = await File.ReadAllBytesAsync(fullPath);

      return (
        butes,
        document.ContentType,
        document.FileName
      );
    }
    public async Task<List<DocumentResponseDto>> GetByStudentIdAsync(int studentId,int userId,   string roll)
    {  
      var document = await _context.Documents
                   .FirstOrDefaultAsync( s => s.Id == studentId);
                   

      if (roll.Equals( "Student", StringComparison.OrdinalIgnoreCase)) 
      { 
        if (document.UserId != userId) 
        { 
          throw new UnauthorizedAccessException( "Students can only view their own documents."); 
        } 
      }

      var documents = await _context.Documents 
                    .Where(d => d.StudentId == studentId) 
                    .OrderByDescending( d => d.UploadedAt) 
                    .ToListAsync();
    
      return documents.Select(d => new DocumentResponseDto
      {
        Id = d.Id,
        StudentId = d.StudentId,
        UploadUserId = d.UserId,
        FileName = d.FileName,
        ContentType = d.ContentType,
        FileSize = d.FileSize,
        UploadedAt = d.UploadedAt,
        ViewUrl = $"/api/documents/{d.Id}/view",
        DownloadUrl = $"/api/documents/{d.Id}/download"
      }).ToList();  
    }
    public async Task DeleteAsync(int documentId,int userId,string roll)
    {
      var document = await _context.Documents
        .Include(d => d.student)
        .FirstOrDefaultAsync(d => d.Id == documentId);

      if (document == null)
      {  
        throw new Exception("Document not found.");
      }

      if (roll.Equals("Teacher", StringComparison.OrdinalIgnoreCase))
      {    
        throw new UnauthorizedAccessException(
        "Teachers cannot delete documents.");
      }

      if (roll.Equals("Student", StringComparison.OrdinalIgnoreCase))
      {
        if (document.student == null ||
            document.student.UserId != userId)
        {
            throw new UnauthorizedAccessException(
                "You can only delete your own documents.");
        }
      }

      var webRootPath = _environment.WebRootPath;

      if (string.IsNullOrEmpty(webRootPath))
      {
        webRootPath = Path.Combine(
            _environment.ContentRootPath,
            "wwwroot");
      }

      var filePath = Path.Combine(
        webRootPath,
        "uploads",
        "documents",
        document.StoredFileName);

      if (File.Exists(filePath))
        File.Delete(filePath);

      _context.Documents.Remove(document);

      await _context.SaveChangesAsync();
    }
  }
}