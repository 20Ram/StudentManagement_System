
using CrudOperation.DTO;
using CrudOperation.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrudOperation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ClassesController : ControllerBase
{
    private readonly IClassService _classService;

    public ClassesController(IClassService classService)
    {
        _classService = classService;
    }

    // GET /api/Classes
    // GET /api/Classes?courseId=1
    // GET /api/Classes?includeInactive=true
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? courseId = null,
        [FromQuery] bool includeInactive = false)
    {
        if (courseId.HasValue && courseId.Value <= 0)
            return BadRequest(new { message = "Invalid course ID." });

        var classes = await _classService.GetAllAsync(
            courseId, includeInactive);

        return Ok(classes);
    }

    // GET /api/Classes/1
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        if (id <= 0)
            return BadRequest(new { message = "Invalid class ID." });

        var result = await _classService.GetByIdAsync(id);

        if (result == null)
            return NotFound(new { message = "Class not found." });

        return Ok(result);
    }

    // POST /api/Classes
    [HttpPost]
    public async Task<IActionResult> Create(CreateClassDto dto)
    {
        try
        {
            var result = await _classService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result!.Id },
                result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // PUT /api/Classes/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateClassDto dto)
    {
        if (id <= 0)
            return BadRequest(new { message = "Invalid class ID." });

        try
        {
            var result = await _classService.UpdateAsync(id, dto);

            if (result == null)
                return NotFound(new { message = "Class not found." });

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // PATCH /api/Classes/1/deactivate
    [HttpPatch("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        if (id <= 0)
            return BadRequest(new { message = "Invalid class ID." });

        var success = await _classService.DeactivateAsync(id);

        if (!success)
            return NotFound(new { message = "Class not found." });

        return Ok(new { message = "Class deactivated successfully." });
    }
}
