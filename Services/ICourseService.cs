using CrudOperation.DTO;

namespace CrudOperation.Service;

public interface ICourseService
{
    Task<IReadOnlyList<CourseResponseDto>> GetAllAsync(
        bool includeInactive = false);

    Task<CourseResponseDto?> GetByIdAsync(int id);

    Task<bool> CodeExistsAsync(string code, int? excludeId = null);

    Task<CourseResponseDto?> CreateAsync(CreateCourseDto dto);

    Task<CourseResponseDto?> UpdateAsync(
        int id,
        UpdateCourseDto dto);

    Task<bool> DeactivateAsync(int id);
}