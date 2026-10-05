using CrudOperation.DTOs;
using CrudOperation.Service;
using Microsoft.AspNetCore.Mvc;

namespace CrudOperation.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class DocumentController : ControllerBase
  {
    private readonly IDocumentService _documentService;
  
    public DocumentController (IDocumentService documentService)
    {
      _documentService = documentService;
    }
    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] DocumentUploadDto dto)
    {
      try
      {
        var result = await _documentService.UploadAsync(dto);
        return Ok(result);
      }
      catch (Exception ex)
      {
        return BadRequest(new
        {
          message = ex.Message
        });
      }
    }
    [HttpGet("{id}/view")]
    public async Task<IActionResult> ViewDocument(int id)
    {
      var result =
        await _documentService.GetFileAsync(id);

      if (result == null)
        return NotFound();

      return File(
        result.Value.FileBytes,
        result.Value.ContentType
      );
    }
    [HttpGet("{id}/download")]
      public async Task<IActionResult> Download(int id)
      {
      var result =
        await _documentService.GetFileAsync(id);

      if (result == null)
        return NotFound();

      return File(
        result.Value.FileBytes,
        result.Value.ContentType,
        result.Value.FileName
      );
    }
  }  
}