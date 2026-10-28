using CrudOperation.DTO;

namespace CrudOperation.Service;

public interface IClassService
{
    Task<IReadOnlyList<ClassResponseDto>> GetAllAsync(
        int? courseId = null,
        bool includeInactive = false);

    Task<ClassResponseDto?> GetByIdAsync(int id);

    Task<bool> ClassExistsAsync(
        int courseId,
        int semester,
        string section,
        string academicYear,
        int? excludeId = null);

    Task<ClassResponseDto?> CreateAsync(CreateClassDto dto);

    Task<ClassResponseDto?> UpdateAsync(int id, UpdateClassDto dto);

    Task<bool> DeactivateAsync(int id);
}