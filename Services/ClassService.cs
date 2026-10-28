using CrudOperation.Data;
using CrudOperation.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CrudOperation.Service;

public class ClassService : IClassService
{
    private readonly AppDbContext _context;

    public ClassService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ClassResponseDto>> GetAllAsync(
        int? courseId = null,
        bool includeInactive = false)
    {
        var query = _context.Classes
            .AsNoTracking()
            .AsQueryable();

        if (courseId.HasValue)
            query = query.Where(c => c.CourseId == courseId.Value);

        if (!includeInactive)
            query = query.Where(c => c.IsActive);

        return await query
            .OrderBy(c => c.Course.Name)
            .ThenBy(c => c.Semester)
            .ThenBy(c => c.Section)
            .Select(c => new ClassResponseDto
            {
                Id = c.Id,
                CourseId = c.CourseId,
                CourseCode = c.Course.Code,
                CourseName = c.Course.Name,
                Semester = c.Semester,
                Section = c.Section,
                AcademicYear = c.AcademicYear,
                IsActive = c.IsActive,
                StudentCount = c.StudentEnrollments.Count(e => e.IsCurrent)
            })
            .ToListAsync();
    }

    public async Task<ClassResponseDto?> GetByIdAsync(int id)
    {
        return await _context.Classes
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new ClassResponseDto
            {
                Id = c.Id,
                CourseId = c.CourseId,
                CourseCode = c.Course.Code,
                CourseName = c.Course.Name,
                Semester = c.Semester,
                Section = c.Section,
                AcademicYear = c.AcademicYear,
                IsActive = c.IsActive,
                StudentCount = c.StudentEnrollments.Count(e => e.IsCurrent)
            })
            .FirstOrDefaultAsync();
    }

    public async Task<bool> ClassExistsAsync(
        int courseId,
        int semester,
        string section,
        string academicYear,
        int? excludeId = null)
    {
        var normalizedSection = section.Trim().ToUpperInvariant();
        var normalizedYear = academicYear.Trim();

        var query = _context.Classes.Where(c =>
            c.CourseId == courseId &&
            c.Semester == semester &&
            c.Section == normalizedSection &&
            c.AcademicYear == normalizedYear);

        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task<ClassResponseDto?> CreateAsync(CreateClassDto dto)
    {
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == dto.CourseId && c.IsActive);

        if (course == null)
            throw new ArgumentException("The course does not exist or is inactive.");

        if (dto.Semester > course.DurationYears * 2)
            throw new ArgumentException(
                $"Semester cannot exceed {course.DurationYears * 2} for this course.");

        var section = dto.Section.Trim().ToUpperInvariant();
        var academicYear = dto.AcademicYear.Trim();

        if (await ClassExistsAsync(
            dto.CourseId, dto.Semester, section, academicYear))
        {
            throw new InvalidOperationException(
                "This class already exists for the selected course and academic year.");
        }

        var entity = new CrudOperation.Models.Class
        {
            CourseId = dto.CourseId,
            Semester = dto.Semester,
            Section = section,
            AcademicYear = academicYear,
            IsActive = true
        };

        _context.Classes.Add(entity);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(entity.Id);
    }

    public async Task<ClassResponseDto?> UpdateAsync(
        int id,
        UpdateClassDto dto)
    {
        var entity = await _context.Classes
            .Include(c => c.Course)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (entity == null)
            return null;

        if (dto.Semester > entity.Course.DurationYears * 2)
            throw new ArgumentException(
                $"Semester cannot exceed {entity.Course.DurationYears * 2} for this course.");

        var section = dto.Section.Trim().ToUpperInvariant();
        var academicYear = dto.AcademicYear.Trim();

        if (await ClassExistsAsync(
            entity.CourseId,
            dto.Semester,
            section,
            academicYear,
            id))
        {
            throw new InvalidOperationException(
                "Another class already exists with these details.");
        }

        entity.Semester = dto.Semester;
        entity.Section = section;
        entity.AcademicYear = academicYear;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        var entity = await _context.Classes
            .FirstOrDefaultAsync(c => c.Id == id);

        if (entity == null)
            return false;

        entity.IsActive = false;
        await _context.SaveChangesAsync();

        return true;
    }
}
