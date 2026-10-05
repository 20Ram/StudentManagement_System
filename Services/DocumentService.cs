using CrudOperation.Data;
using CrudOperation.DTOs;
using CrudOperation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Utilities;

namespace CrudOperation.Service
{
  public class DoumentService : IDocumentService
  {
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public DoumentService(AppDbContext context, IWebHostEnvironment environment)
    {
      _context = context;
      _environment = environment;
    }
    public async Task<DocumentResponseDto> UploadAsync(DocumentUploadDto dto)
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

            FileName = document.FileName,

            ContentType = document.ContentType,

            FileSize = document.FileSize,

            UploadedAt = document.UploadedAt,

            ViewUrl = $"/api/documents/{document.Id}/view",

            DownloadUrl =
                $"/api/documents/{document.Id}/download"
      };
    }
    public async Task<(byte[] FileBytes, string ContentType,string FileName)? > GetFileAsync(int documentId)
    {
      var document = await _context.Documents.FindAsync(documentId);
      if (document == null)
      {
        return null;
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
  public async Task<List<DocumentResponseDto>> GetByStudentIdAsync(int studentId)
  {  
    var document = await _context.Documents
                   .Where(d => d.StudentId == studentId)
                   .ToListAsync();
    
    return document.Select(d => new DocumentResponseDto
    {
      Id = d.Id,
      StudentId = d.StudentId,
      FileName = d.FileName,
      ContentType = d.ContentType,
      FileSize = d.FileSize,
      UploadedAt = d.UploadedAt,
      ViewUrl = $"/api/documents/{d.Id}/view",
      DownloadUrl = $"/api/documents/{d.Id}/download"
    }).ToList();
      
  }
}
}