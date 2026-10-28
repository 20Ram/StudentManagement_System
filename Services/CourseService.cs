using CrudOperation.Data;
using CrudOperation.DTO;
using CrudOperation.Models;
using Microsoft.EntityFrameworkCore;

namespace CrudOperation.Service;

public class CourseService : ICourseService
{
    private readonly AppDbContext _context;

    public CourseService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CourseResponseDto>> GetAllAsync(
        bool includeInactive = false)
    {
        var query = _context.Courses.AsNoTracking();

        if (!includeInactive)
        {
            query = query.Where(c => c.IsActive);
        }

        return await query
            .OrderBy(c => c.Name)
            .Select(c => new CourseResponseDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                DurationYears = c.DurationYears,
                IsActive = c.IsActive,
                ClassCount = c.Classes.Count()
            })
            .ToListAsync();
    }

    public async Task<CourseResponseDto?> GetByIdAsync(int id)
    {
        return await _context.Courses
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CourseResponseDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                DurationYears = c.DurationYears,
                IsActive = c.IsActive,
                ClassCount = c.Classes.Count()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<bool> CodeExistsAsync(
        string code,
        int? excludeId = null)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();

        var query = _context.Courses
            .Where(c => c.Code == normalizedCode);

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<CourseResponseDto?> CreateAsync(
        CreateCourseDto dto)
    {
        var normalizedCode = dto.Code.Trim().ToUpperInvariant();

        // Check again inside the service.
        if (await CodeExistsAsync(normalizedCode))
        {
            return null;
        }

        var course = new Course
        {
            Code = normalizedCode,
            Name = dto.Name.Trim(),
            DurationYears = dto.DurationYears,
            IsActive = true
        };

        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(course.Id);
    }

    public async Task<CourseResponseDto?> UpdateAsync(
        int id,
        UpdateCourseDto dto)
    {
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null)
        {
            return null;
        }

        var normalizedCode = dto.Code.Trim().ToUpperInvariant();

        if (await CodeExistsAsync(normalizedCode, id))
        {
            return null;
        }

        course.Code = normalizedCode;
        course.Name = dto.Name.Trim();
        course.DurationYears = dto.DurationYears;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(course.Id);
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null)
        {
            return false;
        }

        course.IsActive = false;

        await _context.SaveChangesAsync();

        return true;
    }
}