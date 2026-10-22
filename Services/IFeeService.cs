using CrudOperation.DTOs;

namespace CrudOperation.Service
{
    public interface IFeeService
    {
        Task<FeeResponseDto> CreateAsync(CreateFeeDto dto, string adminEmail, CancellationToken cancellationToken = default);
        Task<List<FeeResponseDto>> GetAllAsync(int? studentId, string? status, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
        Task<List<FeeResponseDto>> GetForUserAsync(int userId, CancellationToken cancellationToken = default);
        Task<FeeResponseDto?> GetByIdAsync(int id, int userId, bool isAdmin, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, string adminEmail, CancellationToken cancellationToken = default);
    }
}
