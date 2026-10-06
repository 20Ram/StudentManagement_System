using CrudOperation.DTOs;
using CrudOperation.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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
    [Authorize(Roles = "Admin,Teacher,Student")]
    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] DocumentUploadDto dto)
    {
      try
      {
        var userInfo = GetCurrentUser();
        if (userInfo == null) 
        { 
          return Unauthorized(); 
        }
        var result = await _documentService.UploadAsync(
          dto,userInfo.Value.UserId, userInfo.Value.Role);
        return Ok(new 
        { 
          message = "Document uploaded successfully.", document = result 
        });
      }
      catch (Exception ex)
      {
        return BadRequest(new
        {
          message = ex.Message
        });
      }
    }
    [Authorize(Roles = "Admin,Teacher,Student")]
    [HttpGet("{id}/view")]
    public async Task<IActionResult> ViewDocument(int id)
    {
      var userInfo = GetCurrentUser(); 
      if (userInfo == null) 
      { 
        return Unauthorized(); 
      }

      var result =
        await _documentService.GetFileAsync(
          id,userInfo.Value.UserId, userInfo.Value.Role);

      if (result == null)
      { return NotFound();}

      return File(result.Value.FileBytes,result.Value.ContentType);
    }
    
    [Authorize(Roles = "Admin,Teacher,Student")] 
    [HttpGet("{id}/download")] 
    public async Task<IActionResult> Download(int id) 
    { 
      try 
      { 
        var userInfo = GetCurrentUser(); 
        if (userInfo == null) 
        { 
          return Unauthorized(); 
        } 
        var result = await _documentService.GetFileAsync( id, userInfo.Value.UserId, userInfo.Value.Role); 
        if (result == null) 
        { 
          return NotFound(new { message = "Document not found." }); 
        } 
        return File( result.Value.FileBytes, result.Value.ContentType, result.Value.FileName ); 
        } 
        catch (Exception ex) 
        { 
          return BadRequest(new { message = ex.Message }); 
        }    
    }
    private (int UserId, string Role)? GetCurrentUser() 
    { 
      var userIdClaim = User.FindFirst( ClaimTypes.NameIdentifier)?.Value;

      var roleClaim = User.FindFirst( ClaimTypes.Role)?.Value; 

      if (!int.TryParse( userIdClaim, out int userId)) 
      { 
        return null; 
      } 
      if (string.IsNullOrEmpty(roleClaim)) 
      { 
        return null; 
      } 
      return (userId, roleClaim); 
    } 
    [Authorize(Roles = "Admin,Student")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
     try
      {
        var userInfo = GetCurrentUser();

        if (userInfo == null)
        {  return Unauthorized();}

        await _documentService.DeleteAsync(id,userInfo.Value.UserId,userInfo.Value.Role);

        return Ok(new
        {
            message = "Document deleted successfully."
        });
      }
      catch (Exception ex)
      {
        return BadRequest(new
        {
            message = ex.Message
        });
      }
    }
  }  
}