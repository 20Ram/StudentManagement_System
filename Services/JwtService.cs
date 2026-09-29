using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using CrudOperation.Models;

namespace CrudOperation.Service
{
  public class JwtService : IJwtService
  {
    private readonly IConfiguration _configuration;

    public JwtService (IConfiguration configuration)
    {
      _configuration = configuration;
    }

    public string GenerateToken(User user)
    {
      var JwtSettings = _configuration.GetSection("JwtSettings");
      var secretKey = JwtSettings["SecretKey"];
      var key =  new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
      var credentials = new SigningCredentials(key,SecurityAlgorithms.HmacSha256);

      var claims = new List<Claim>
      {
        new Claim(ClaimTypes.Name,user.Name),
        new Claim(ClaimTypes.Email,user.Email),
        new Claim(ClaimTypes.Role,user.Roll)
      };

      var token = new JwtSecurityToken(
        issuer: JwtSettings["Issuer"],
        audience: JwtSettings["Audience"],
        claims: claims,
        expires: DateTime.Now.AddHours(1),
        signingCredentials: credentials
      );
       return new JwtSecurityTokenHandler().WriteToken(token);
    }
  }
}