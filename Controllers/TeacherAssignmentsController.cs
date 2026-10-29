using System.Security.Claims;
using CrudOperation.Data;
using CrudOperation.DTOs;
using CrudOperation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrudOperation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class TeacherAssignmentsController : ControllerBase
{
    private readonly AppDbContext _db;

    public TeacherAssignmentsController(AppDbContext db)
    {
        _db = db;
    }

    private string AdminEmail => User.FindFirstValue(ClaimTypes.Email) ?? "System";

    // GET /api/TeacherAssignments?teacherId=3&subjectId=9
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? teacherId,
        [FromQuery] int? subjectId,
        CancellationToken ct = default)
    {
        var query = _db.TeacherAssignments.AsNoTracking().AsQueryable();

        if (teacherId.HasValue) query = query.Where(a => a.TeacherId == teacherId.Value);
        if (subjectId.HasValue) query = query.Where(a => a.SubjectId == subjectId.Value);

        var list = await query
            .OrderBy(a => a.Subject.Name)
            .Select(a => new TeacherAssignmentResponseDto
            {
                Id = a.Id,
                TeacherId = a.TeacherId,
                TeacherName = a.Teacher.Name,
                SubjectId = a.SubjectId,
                SubjectCode = a.Subject.Code,
                SubjectName = a.Subject.Name,
                ClassId = a.Subject.ClassId,
                CourseName = a.Subject.Class.Course.Name,
                Semester = a.Subject.Class.Semester,
                Section = a.Subject.Class.Section,
                AcademicYear = a.Subject.Class.AcademicYear
            })
            .ToListAsync(ct);

        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> Assign(AssignTeacherDto dto, CancellationToken ct)
    {
        var teacher = await _db.Teachers
            .FirstOrDefaultAsync(t => t.Id == dto.TeacherId && t.IsActive, ct);
        if (teacher == null)
            return NotFound(new { message = "Teacher not found or inactive." });

        var subject = await _db.Subjects
            .FirstOrDefaultAsync(s => s.Id == dto.SubjectId && s.IsActive && s.Class.IsActive, ct);
        if (subject == null)
            return BadRequest(new { message = "Subject not found or inactive." });

        var exists = await _db.TeacherAssignments
            .AnyAsync(a => a.TeacherId == teacher.Id && a.SubjectId == subject.Id, ct);
        if (exists)
            return Conflict(new { message = "This teacher is already assigned to this subject." });

        var assignment = new TeacherAssignment { TeacherId = teacher.Id, SubjectId = subject.Id };
        _db.TeacherAssignments.Add(assignment);

        _db.AuditLogs.Add(new AuditLog
        {
            UserEmail = AdminEmail,
            Action = "ASSIGN_TEACHER",
            Details = $"Assigned teacher {teacher.Email} to subject {subject.Code}"
        });

        await _db.SaveChangesAsync(ct);
        return Ok(new { message = "Teacher assigned successfully.", assignmentId = assignment.Id });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remove(int id, CancellationToken ct)
    {
        var assignment = await _db.TeacherAssignments
            .Include(a => a.Teacher)
            .Include(a => a.Subject)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (assignment == null)
            return NotFound(new { message = "Assignment not found." });

        _db.TeacherAssignments.Remove(assignment);

        _db.AuditLogs.Add(new AuditLog
        {
            UserEmail = AdminEmail,
            Action = "REMOVE_TEACHER_ASSIGNMENT",
            Details = $"Removed teacher {assignment.Teacher.Email} from subject {assignment.Subject.Code}"
        });

        await _db.SaveChangesAsync(ct);
        return Ok(new { message = "Assignment removed." });
    }
}