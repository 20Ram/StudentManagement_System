using Microsoft.EntityFrameworkCore;
using CrudOperation.Models;

namespace CrudOperation.Data
{
  public class AppDbContext : DbContext
  {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
      
    }
    public DbSet<Student> Students{get; set;}
    public DbSet<User> Users{get; set;}
    public DbSet<OtpVerification> OtpVerifications{get; set;}
  }
}