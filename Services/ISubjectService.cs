
using CrudOperation.DTOs.Subjects;

namespace CrudOperation.Service;

public interface ISubjectService
{
    Task<IReadOnlyList<SubjectResponseDto>> GetAllAsync(
        int? classId = null,
        bool includeInactive = false);

    Task<SubjectResponseDto?> GetByIdAsync(int id);

    Task<bool> CodeExistsAsync(
        int classId,
        string code,
        int? excludeId = null);

    Task<SubjectResponseDto> CreateAsync(CreateSubjectDto dto);

    Task<SubjectResponseDto?> UpdateAsync(
        int id,
        UpdateSubjectDto dto);

    Task<bool> DeactivateAsync(int id);
}
