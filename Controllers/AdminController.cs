using CrudOperation.Data;
using CrudOperation.DTOs;
using CrudOperation.Service;
using CrudOperation.Models;
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
    private readonly IEmailService _emailService;
    private readonly IOtpService _otpService;
    public AdminController(AppDbContext context,IEmailService emailService,IOtpService otpService )
    {
      _context = context;
      _emailService = emailService;
      _otpService = otpService;
    }

    [HttpGet("dashboard")]
    public IActionResult Dashboard()
    {  
      var totalStudent = _context.Users.Count(u => u.Roll == "Student");
      
      var reportAverage = _context.Users.Average(u => u.Student.Marks);

      var totalTeacher = _context.Users.Count(u => u.Roll == "Teacher");
       
      return Ok(new
        {
          message = "Welcome to Admin Dashboard" ,
          totalStudent,
          reportAverage,
          totalTeacher
        });
    }

    [HttpGet("user")]
    public async Task<IActionResult> GetUsers()
    {
      
      var users = await _context.Users
                .Select(u => new{
                u.Id,u.Email,u.Name,u.Roll,u.IsEmailVerified})
                .Where(u => u.IsEmailVerified == true)
                .ToListAsync();
      return Ok(users);
    }

     [HttpGet("teachers")]
        public async Task<IActionResult> GetTeachers()
        {
            var teachers = await _context.Users
                .Where(u => u.Roll == "Teacher" && u.IsEmailVerified == true)
                .ToListAsync();

            return Ok(teachers);
        } 

        [HttpGet("students")]
        public async Task<IActionResult> GetStudents()
        {
            var students = await _context.Users
                .Where(u => u.Roll == "Student" && u.IsEmailVerified == true)
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
      [HttpDelete("Delete-User{id}")]
      public async Task<IActionResult> DeleteUser(int id)
      {
      var removeUser = _context.Users.FirstOrDefault(s => s.Id == id);
      if (removeUser == null)
      {
        return NotFound(removeUser);
      }
      removeUser.IsEmailVerified = false;
      await _context.SaveChangesAsync();
      return Ok(removeUser);
    }
    
    [HttpPost("create-student")]
    public async Task<IActionResult> CreateUser(CreateStudentDto dto)
        {
            var allowedRoles = new[] { "Student", "Teacher" };

            if (!allowedRoles.Contains(dto.Roll))
            {
               return BadRequest(new
               {
                 message = "Invalid role."
             });
           }
            var email = dto.Email.Trim().ToLowerInvariant();

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (existingUser != null && existingUser.IsEmailVerified)
            {
                return BadRequest(new
                {
                    message = "Email is already registered."
                });
            }

            if (existingUser == null)
            {
                var user = new User
                {
                    Name = dto.Name.Trim(),
                    Email = email,
                    Roll = dto.Roll,
                    IsEmailVerified = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                if(dto.Roll == "Student")
                {
                    var student = new Student
                    {  
                        UserId = user.Id,
                        Name = dto.Name.Trim(),
                        Email = user.Email,
                        Age = dto.Age,
                        Course = dto.Course,
                        Marks = dto.Marks
                    };
                    _context.Students.Add(student);
                    await _context.SaveChangesAsync();
                }

            }
            else
            {
                existingUser.Name = dto.Name.Trim();
                await _context.SaveChangesAsync();
            }  

            return Ok(new
            {
                message = "OTP sent successfully to your email."
            });
        }

        [HttpPost("create-teacher")]
    public async Task<IActionResult> Createteacher(CreateTeacherDto dto)
        {
            var allowedRoles = new[] { "Student", "Teacher" };

            if (!allowedRoles.Contains(dto.Roll))
            {
               return BadRequest(new
               {
                 message = "Invalid role."
             });
           }
            var email = dto.Email.Trim().ToLowerInvariant();

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (existingUser != null && existingUser.IsEmailVerified)
            {
                return BadRequest(new
                {
                    message = "Email is already registered."
                });
            }

            if (existingUser == null)
            {
                var user = new User
                {
                    Name = dto.Name.Trim(),
                    Email = email,
                    Roll = dto.Roll,
                    IsEmailVerified = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                if(dto.Roll == "Teacher")
                {
                    var teacher = new Teacher
                    {  
                        UserId = user.Id,
                        Name = dto.Name.Trim(),
                        Email = email,
                        Age = dto.Age,
                        Department = dto.Department
                    };
                    _context.Teachers.Add(teacher);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                existingUser.Name = dto.Name.Trim();
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                message = "OTP sent successfully to your email."
            });
        }
  }
}