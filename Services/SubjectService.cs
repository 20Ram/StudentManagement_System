using CrudOperation.Data;
using CrudOperation.DTOs;
using CrudOperation.Models;
using Microsoft.EntityFrameworkCore;
namespace CrudOperation.Service;

public class SubjectService : ISubjectService
{
    private readonly AppDbContext _context;

    public SubjectService(AppDbContext context)
    {
        _context = context;
    }

    private static SubjectResponseDto ToDto(Subject subject)
    {
        return new SubjectResponseDto
        {
            Id = subject.Id,
            ClassId = subject.ClassId,
            SubjectCode = subject.Code,
            SubjectName = subject.Name,
            Credits = subject.Credits,
            Semester = subject.Class.Semester,
            Section = subject.Class.Section,
            AcademicYear = subject.Class.AcademicYear,
            IsActive = subject.IsActive
        };
    }

    public async Task<IReadOnlyList<SubjectResponseDto>> GetAllAsync(
        int? classId = null,
        bool includeInactive = false)
    {
        var query = _context.Subjects
            .AsNoTracking()
            .Include(s => s.Class)
            .AsQueryable();

        if (classId.HasValue)
            query = query.Where(s => s.ClassId == classId.Value);

        if (!includeInactive)
            query = query.Where(s => s.IsActive && s.Class.IsActive);

        var subjects = await query
            .OrderBy(s => s.Class.Semester)
            .ThenBy(s => s.Name)
            .ToListAsync();

        return subjects.Select(ToDto).ToList();
    }

    public async Task<SubjectResponseDto?> GetByIdAsync(int id)
    {
        var subject = await _context.Subjects
            .AsNoTracking()
            .Include(s => s.Class)
            .FirstOrDefaultAsync(s => s.Id == id);

        return subject is null ? null : ToDto(subject);
    }

    public async Task<bool> CodeExistsAsync(
        int classId,
        string code,
        int? excludeId = null)
    {
        code = code.Trim();

        return await _context.Subjects.AnyAsync(s =>
            s.ClassId == classId &&
            s.Code.ToUpper() == code.ToUpper() &&
            (!excludeId.HasValue || s.Id != excludeId.Value));
    }

    public async Task<SubjectResponseDto> CreateAsync(CreateSubjectDto dto)
    {
        var classExists = await _context.Classes.AnyAsync(c =>
            c.Id == dto.ClassId && c.IsActive);

        if (!classExists)
            throw new ArgumentException("The selected class does not exist or is inactive.");

        var code = dto.Code.Trim().ToUpperInvariant();

        if (await CodeExistsAsync(dto.ClassId, code))
            throw new InvalidOperationException(
                "This subject code already exists in the selected class.");

        var subject = new Subject
        {
            ClassId = dto.ClassId,
            Code = code,
            Name = dto.Name.Trim(),
            Credits = dto.Credits,
            IsActive = true
        };

        _context.Subjects.Add(subject);
        await _context.SaveChangesAsync();

        await _context.Entry(subject).Reference(s => s.Class).LoadAsync();

        return ToDto(subject);
    }

    public async Task<SubjectResponseDto?> UpdateAsync(
        int id,
        UpdateSubjectDto dto)
    {
        var subject = await _context.Subjects
            .Include(s => s.Class)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (subject is null)
            return null;

        if (!subject.IsActive)
            throw new InvalidOperationException(
                "An inactive subject cannot be updated.");

        var code = dto.Code.Trim().ToUpperInvariant();

        if (await CodeExistsAsync(subject.ClassId, code, id))
            throw new InvalidOperationException(
                "This subject code already exists in the selected class.");

        subject.Code = code;
        subject.Name = dto.Name.Trim();
        subject.Credits = dto.Credits;

        await _context.SaveChangesAsync();

        return ToDto(subject);
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        var subject = await _context.Subjects
            .FirstOrDefaultAsync(s => s.Id == id);

        if (subject is null)
            return false;

        subject.IsActive = false;
        await _context.SaveChangesAsync();

        return true;
    }
}
