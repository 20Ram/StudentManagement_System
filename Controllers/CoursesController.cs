using CrudOperation.DTOs;
using CrudOperation.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrudOperation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    // GET: /api/Courses
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool includeInactive = false)
    {
        var courses = await _courseService.GetAllAsync(includeInactive);
        return Ok(courses);
    }

    // GET: /api/Courses/1
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> GetById(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid course ID." });
        }

        var course = await _courseService.GetByIdAsync(id);

        if (course == null)
        {
            return NotFound(new { message = "Course not found." });
        }

        return Ok(course);
    }

    // POST: /api/Courses
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(
        [FromBody] CreateCourseDto dto)
    {
        if (await _courseService.CodeExistsAsync(dto.Code))
        {
            return Conflict(new
            {
                message = "A course with this code already exists."
            });
        }

        var course = await _courseService.CreateAsync(dto);

        if (course == null)
        {
            return Conflict(new
            {
                message = "A course with this code already exists."
            });
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = course.Id },
            course);
    }

    // PUT: /api/Courses/1
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateCourseDto dto)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid course ID." });
        }

        var existingCourse = await _courseService.GetByIdAsync(id);

        if (existingCourse == null)
        {
            return NotFound(new { message = "Course not found." });
        }

        if (await _courseService.CodeExistsAsync(dto.Code, id))
        {
            return Conflict(new
            {
                message = "Another course already uses this code."
            });
        }

        var course = await _courseService.UpdateAsync(id, dto);

        if (course == null)
        {
            return Conflict(new
            {
                message = "The course could not be updated. Check its code."
            });
        }

        return Ok(course);
    }

    // PATCH: /api/Courses/1/deactivate
    [HttpPatch("{id:int}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid course ID." });
        }

        var deactivated = await _courseService.DeactivateAsync(id);

        if (!deactivated)
        {
            return NotFound(new { message = "Course not found." });
        }

        return Ok(new { message = "Course deactivated successfully." });
    }
}