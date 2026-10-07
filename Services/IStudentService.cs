using CrudOperation.DTOs;

namespace CrudOperation.Service
{
  public interface IStudentService
  {
    Task<object> UpdateAsync(UpdateStudentDto dto,string email);
    Task<object> GetProfileAsync(string email);
    Task<PaginatedResponseDto<object>>GetStudentsAsync(StudentQueryDto query);
  }
}