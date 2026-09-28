using CrudOperation.Models;

namespace CrudOperation.Service
{
  public interface IJwtService
  {
    string GenerateToken(User user);
  }
}