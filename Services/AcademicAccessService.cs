using CrudOperation.Data;
using Microsoft.EntityFrameworkCore;

namespace CrudOperation.Service;

public class AcademicAccessService : IAcademicAccessService
{
    private readonly AppDbContext _db;

    public AcademicAccessService(AppDbContext db)
    {
        _db = db;
    }

    public Task<bool> TeacherHasClassAsync(int teacherUserId, int classId, CancellationToken ct = default) =>
        _db.TeacherAssignments.AnyAsync(a =>
            a.Teacher.UserId == teacherUserId &&
            a.Teacher.IsActive &&
            a.Subject.IsActive &&
            a.Subject.ClassId == classId, ct);

    public Task<bool> TeacherHasSubjectAsync(int teacherUserId, int subjectId, CancellationToken ct = default) =>
        _db.TeacherAssignments.AnyAsync(a =>
            a.Teacher.UserId == teacherUserId &&
            a.Teacher.IsActive &&
            a.Subject.IsActive &&
            a.SubjectId == subjectId, ct);

    public Task<bool> TeacherHasStudentAsync(int teacherUserId, int studentId, CancellationToken ct = default) =>
        _db.StudentEnrollments.AnyAsync(e =>
            e.StudentId == studentId &&
            e.IsCurrent &&
            _db.TeacherAssignments.Any(a =>
                a.Teacher.UserId == teacherUserId &&
                a.Teacher.IsActive &&
                a.Subject.IsActive &&
                a.Subject.ClassId == e.ClassId), ct);
}