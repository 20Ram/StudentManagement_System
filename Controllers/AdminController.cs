using CrudOperation.Data;
using CrudOperation.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace  CrudOperation.Controllers
{
   
  [ApiController]
  [Route("api/[controller]")]
  [Authorize(Roles = "Admin")]
  public class AdminController : ControllerBase
  {
    private readonly AppDbContext _context;
    public AdminController(AppDbContext context)
    {
      _context = context;
    }

    [HttpGet("dashboard")]
    public IActionResult Dashboard()
    {
      return Ok(new
        {
          message = "Welcome to Admin Dashboard"
        });
    }

    [HttpGet("user")]
    public async Task<IActionResult> GetUsers()
    {
      var users = await _context.Users
                .Select(u => new{
                u.Id,u.Email,u.Name,u.Roll,u.IsEmailVerified}).ToListAsync();
      return Ok(users);
    }

     [HttpGet("teachers")]
        public async Task<IActionResult> GetTeachers()
        {
            var teachers = await _context.Users
                .Where(u => u.Roll == "Teacher")
                .ToListAsync();

            return Ok(teachers);
        } 

        [HttpGet("students")]
        public async Task<IActionResult> GetStudents()
        {
            var students = await _context.Users
                .Where(u => u.Roll == "Student")
                .ToListAsync();

            return Ok(students);
        }
 
        [HttpPut("change-role/{id}")]
        public async Task<IActionResult> ChangeRole(
            int id,
            UpdateRoledto dto)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User not found"
                });
            }

            var allowedRoles = new[] {"Admin","Teacher","Student"};

            if (!allowedRoles.Contains(dto.Roll))
            {
                return BadRequest(new
                {
                    message = "Invalid role"
                });
            }

            user.Roll = dto.Roll;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Role updated successfully",
                userId = user.Id,
                role = user.Roll
            });
      }
  }
}