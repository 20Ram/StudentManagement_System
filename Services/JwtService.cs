using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CrudOperation.Service
{
  public class JwtService : IJwtService
  {
    private readonly IConfiguration _configuration;

    public JetService (IConfiguration configuration)
    {
      _configuration = configuration;
    }
  }
}