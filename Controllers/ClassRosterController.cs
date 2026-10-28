using System.Security.Claims;
using CrudOperation.Data;
using CrudOperation.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrudOperation.Controllers;

[ApiController]
[Route("api/Classes/{classId:int}")]
[Authorize(Roles = "Admin,Teacher")]
public class ClassRosterController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IAcademicAccessService _access;

    public ClassRosterController(AppDbContext db, IAcademicAccessService access)
    {
        _db = db;
        _access = access;
    }

    private async Task<bool> CanAccessClassAsync(int classId, CancellationToken ct)
    {
        if (User.IsInRole("Admin"))
            return await _db.Classes.AnyAsync(c => c.Id == classId, ct);

        return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
               && await _access.TeacherHasClassAsync(userId, classId, ct);
    }

    // GET /api/Classes/2/students
    [HttpGet("students")]
    public async Task<IActionResult> Students(int classId, CancellationToken ct)
    {
        // 404 (not 403) so a teacher cannot probe which class ids exist.
        if (!await CanAccessClassAsync(classId, ct))
            return NotFound(new { message = "Class not found." });

        var students = await _db.StudentEnrollments
            .AsNoTracking()
            .Where(e => e.ClassId == classId && e.IsCurrent && e.Student.IsActive)
            .OrderBy(e => e.Student.Name)
            .Select(e => new
            {
                StudentId = e.StudentId,
                e.Student.Name,
                e.Student.Email,
                e.Student.Age,
                e.Student.Marks,
                e.EnrolledAt
            })
            .ToListAsync(ct);

        return Ok(students);
    }

    // GET /api/Classes/2/subjects
    [HttpGet("subjects")]
    public async Task<IActionResult> Subjects(int classId, CancellationToken ct)
    {
        if (!await CanAccessClassAsync(classId, ct))
            return NotFound(new { message = "Class not found." });

        var subjects = await _db.Subjects
            .AsNoTracking()
            .Where(s => s.ClassId == classId && s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => new
            {
                s.Id,
                s.Code,
                s.Name,
                s.Credits,
                Teachers = s.TeacherAssignments
                    .Where(a => a.Teacher.IsActive)
                    .Select(a => new { a.TeacherId, a.Teacher.Name })
                    .ToList()
            })
            .ToListAsync(ct);

        return Ok(subjects);
    }
}