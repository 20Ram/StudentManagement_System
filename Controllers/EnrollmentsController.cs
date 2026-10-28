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
public class EnrollmentsController : ControllerBase
{
    private readonly AppDbContext _db;

    public EnrollmentsController(AppDbContext db)
    {
        _db = db;
    }

    private string AdminEmail => User.FindFirstValue(ClaimTypes.Email) ?? "System";

    // GET /api/Enrollments?classId=1&studentId=5&includeHistory=true
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? classId,
        [FromQuery] int? studentId,
        [FromQuery] bool includeHistory = false,
        CancellationToken ct = default)
    {
        var query = _db.StudentEnrollments.AsNoTracking().AsQueryable();

        if (classId.HasValue) query = query.Where(e => e.ClassId == classId.Value);
        if (studentId.HasValue) query = query.Where(e => e.StudentId == studentId.Value);
        if (!includeHistory) query = query.Where(e => e.IsCurrent);

        var list = await query
            .OrderBy(e => e.Student.Name)
            .Select(e => new EnrollmentResponseDto
            {
                Id = e.Id,
                StudentId = e.StudentId,
                StudentName = e.Student.Name,
                StudentEmail = e.Student.Email,
                ClassId = e.ClassId,
                CourseName = e.Class.Course.Name,
                Semester = e.Class.Semester,
                Section = e.Class.Section,
                AcademicYear = e.Class.AcademicYear,
                EnrolledAt = e.EnrolledAt,
                IsCurrent = e.IsCurrent
            })
            .ToListAsync(ct);

        return Ok(list);
    }

    // POST /api/Enrollments   { "studentId": 5, "classId": 2 }
    // A student has ONE current class. Enrolling into a new class automatically
    // closes the previous one (kept as history with IsCurrent = false).
    [HttpPost]
    public async Task<IActionResult> Enroll(EnrollStudentDto dto, CancellationToken ct)
    {
        var student = await _db.Students
            .FirstOrDefaultAsync(s => s.Id == dto.StudentId && s.IsActive, ct);
        if (student == null)
            return NotFound(new { message = "Student not found or inactive." });

        var cls = await _db.Classes
            .Include(c => c.Course)
            .FirstOrDefaultAsync(c => c.Id == dto.ClassId && c.IsActive && c.Course.IsActive, ct);
        if (cls == null)
            return BadRequest(new { message = "Class not found or inactive." });

        var enrollments = await _db.StudentEnrollments
            .Where(e => e.StudentId == student.Id)
            .ToListAsync(ct);

        var same = enrollments.FirstOrDefault(e => e.ClassId == cls.Id);
        if (same is { IsCurrent: true })
            return Conflict(new { message = "Student is already enrolled in this class." });

        foreach (var old in enrollments.Where(e => e.IsCurrent))
            old.IsCurrent = false;

        StudentEnrollment enrollment;
        if (same != null)
        {
            same.IsCurrent = true;
            same.EnrolledAt = DateTime.UtcNow;
            enrollment = same;
        }
        else
        {
            enrollment = new StudentEnrollment
            {
                StudentId = student.Id,
                ClassId = cls.Id,
                EnrolledAt = DateTime.UtcNow,
                IsCurrent = true
            };
            _db.StudentEnrollments.Add(enrollment);
        }

        // Keeps the old free-text Student.Course column in sync so existing screens still work.
        student.Course = cls.Course.Name;

        _db.AuditLogs.Add(new AuditLog
        {
            UserEmail = AdminEmail,
            Action = "ENROLL_STUDENT",
            Details = $"Enrolled student {student.Email} in {cls.Course.Code} sem {cls.Semester} {cls.Section} {cls.AcademicYear}"
        });

        await _db.SaveChangesAsync(ct);

        return Ok(new { message = "Student enrolled successfully.", enrollmentId = enrollment.Id });
    }

    // DELETE /api/Enrollments/7   -> removes the student from the class (history is kept)
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Unenroll(int id, CancellationToken ct)
    {
        var enrollment = await _db.StudentEnrollments
            .Include(e => e.Student)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        if (enrollment == null)
            return NotFound(new { message = "Enrollment not found." });

        enrollment.IsCurrent = false;

        _db.AuditLogs.Add(new AuditLog
        {
            UserEmail = AdminEmail,
            Action = "UNENROLL_STUDENT",
            Details = $"Removed student {enrollment.Student.Email} from class {enrollment.ClassId}"
        });

        await _db.SaveChangesAsync(ct);
        return Ok(new { message = "Student removed from the class." });
    }
}