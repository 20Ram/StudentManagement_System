
using CrudOperation.DTOs.Subjects;
using CrudOperation.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrudOperation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class SubjectsController : ControllerBase
{
    private readonly ISubjectService _subjectService;

    public SubjectsController(ISubjectService subjectService)
    {
        _subjectService = subjectService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? classId,
        [FromQuery] bool includeInactive = false)
    {
        var subjects = await _subjectService.GetAllAsync(
            classId, includeInactive);

        return Ok(subjects);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var subject = await _subjectService.GetByIdAsync(id);

        if (subject is null)
            return NotFound(new { message = "Subject not found." });

        return Ok(subject);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateSubjectDto dto)
    {
        try
        {
            var subject = await _subjectService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = subject.Id },
                subject);
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

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, UpdateSubjectDto dto)
    {
        try
        {
            var subject = await _subjectService.UpdateAsync(id, dto);

            if (subject is null)
                return NotFound(new { message = "Subject not found." });

            return Ok(subject);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var result = await _subjectService.DeactivateAsync(id);

        if (!result)
            return NotFound(new { message = "Subject not found." });

        return Ok(new { message = "Subject deactivated successfully." });
    }
}
