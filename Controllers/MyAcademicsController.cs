using System.Security.Claims;
using CrudOperation.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrudOperation.Controllers;

[ApiController]
[Route("api/my")]
[Authorize]
public class MyAcademicsController : ControllerBase
{
    private readonly AppDbContext _db;

    public MyAcademicsController(AppDbContext db)
    {
        _db = db;
    }

    private bool TryGetUserId(out int userId) =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
        
    [HttpGet("academics")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> MyAcademics(CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new { message = "User ID claim is missing." });

        var current = await _db.StudentEnrollments
            .AsNoTracking()
            .Where(e => e.Student.UserId == userId && e.IsCurrent && e.Class.IsActive)
            .Select(e => new
            {
                e.ClassId,
                CourseId = e.Class.CourseId,
                CourseCode = e.Class.Course.Code,
                CourseName = e.Class.Course.Name,
                e.Class.Semester,
                e.Class.Section,
                e.Class.AcademicYear,
                e.EnrolledAt,
                Classmates = e.Class.StudentEnrollments.Count(x => x.IsCurrent)
            })
            .FirstOrDefaultAsync(ct);

        if (current == null)
        {
            return Ok(new
            {
                enrolled = false,
                message = "You are not enrolled in a class yet. Please contact your admin.",
                @class = (object?)null,
                subjects = Array.Empty<object>()
            });
        }

        var subjects = await _db.Subjects
            .AsNoTracking()
            .Where(s => s.ClassId == current.ClassId && s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => new
            {
                s.Id,
                s.Code,
                s.Name,
                s.Credits,
                Teachers = s.TeacherAssignments
                    .Where(a => a.Teacher.IsActive)
                    .Select(a => a.Teacher.Name)
                    .ToList()
            })
            .ToListAsync(ct);

        return Ok(new { enrolled = true, @class = current, subjects });
    }
 
    [HttpGet("teaching")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> MyTeaching(CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new { message = "User ID claim is missing." });

        var list = await _db.TeacherAssignments
            .AsNoTracking()
            .Where(a => a.Teacher.UserId == userId &&
                        a.Subject.IsActive &&
                        a.Subject.Class.IsActive)
            .OrderBy(a => a.Subject.Class.Course.Name)
            .ThenBy(a => a.Subject.Class.Semester)
            .ThenBy(a => a.Subject.Name)
            .Select(a => new
            {
                AssignmentId = a.Id,
                a.SubjectId,
                SubjectCode = a.Subject.Code,
                SubjectName = a.Subject.Name,
                a.Subject.Credits,
                a.Subject.ClassId,
                CourseName = a.Subject.Class.Course.Name,
                a.Subject.Class.Semester,
                a.Subject.Class.Section,
                a.Subject.Class.AcademicYear,
                StudentCount = a.Subject.Class.StudentEnrollments.Count(e => e.IsCurrent)
            })
            .ToListAsync(ct);

        return Ok(list);
    }
}