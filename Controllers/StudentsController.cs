// using CrudOperation.Models;
// using Microsoft.AspNetCore.Mvc;
// using CrudOperation.Data;
// using CrudOperation.DTOs;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.EntityFrameworkCore;
// namespace CrudOperation.Controllers
// {
//  [ApiController]
//  [Route("api/[controller]")]
//  [Authorize(Roles = "Student")]
//  public class StudentsController : ControllerBase
//   {
//     private readonly AppDbContext _context;
    
//     public StudentsController(AppDbContext context)
//     {
//       _context = context;
//     } 

//     [HttpPut("Update-student")]
//     public async Task<IActionResult> UpdateStudent(UpdateStudentDto dto)
//     { 
//       var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

//       if (string.IsNullOrEmpty(email))
//       {
//         return Unauthorized(new
//         {
//             message = "Email claim missing from token"
//         });
//       }
 
//       var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

//       if (user == null)
//       {
//         return NotFound(new
//         {
//             message = "User not found"
//         });
//       }
//       var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);

//       if (student == null)
//       {
//         return NotFound(new
//         {
//             message = "Student record not found"
//         });
//       }

//       if (!string.IsNullOrWhiteSpace(dto.Name))
//       {
//         user.Name = dto.Name;
//         student.Name = dto.Name;
//       }

//       if (!string.IsNullOrWhiteSpace(dto.Email))
//       {
//         user.Email = dto.Email;
//         student.Email = dto.Email;
//       }

//       if (dto.Age.HasValue)
//       {
//         student.Age = dto.Age.Value;
//       }

//       if (!string.IsNullOrWhiteSpace(dto.Course))
//       {
//         student.Course = dto.Course;
//       }

//       if (dto.Marks.HasValue)
//       {
//         student.Marks = dto.Marks.Value;
//       }
 
//       await _context.SaveChangesAsync();
//       return Ok(new
//       {
//         student = new
//         {
//             user.Id,
//             user.Name,
//             user.Email,
//             student.Age,
//             student.Course,
//             student.Marks
//         }
//       });
//     }
//     [HttpGet("student-profile")]
//     public async Task<IActionResult> Profile()
//     {
//       var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
//       var student = await _context.Users
//         .Where(u => u.Email == email && u.Roll == "Student")
//         .Select(u => new
//         {
//             u.Id,
//             u.Name,
//             u.Email,
//             u.Roll
            
//         }).FirstOrDefaultAsync();
//          var students = await _context.Students
//         .Where(s => s.Email == email)
//         .Select(s => new
//         {
//             s.Age,
//             s.Course,
//             s.Marks,
//         }).FirstOrDefaultAsync();

//       if (student == null)
//       {
//         return NotFound(new
//         {
//             message = "Student profile not found"
//         });
//       }
//       return Ok(new
//       {
//         student.Id,
//         student.Email,
//         student.Name,
//         student.Roll,
//         students.Age,
//         students.Course,
//         students.Marks
//       });
//     }
//   } 
// }



using CrudOperation.DTOs;
using CrudOperation.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CrudOperation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Student")]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _studentService;
        public StudentsController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        [HttpPut("Update-student")]
        public async Task<IActionResult> UpdateStudent(UpdateStudentDto dto)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized(new
                {
                    message = "Email claim missing from token"
                });
            }

            var result = await _studentService.UpdateAsync(dto ,email);
            if (result == null)
            {
                return NotFound(new
                {
                    message = "User or student record not found."
                });
            }
            return Ok(result);
        }

        [HttpGet("student-profile")]
        public async Task<IActionResult> Profile()
        {
            var email = User.FindFirst(
                ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized(new
                {
                    message = "Email claim missing from token"
                });
            }

            var result = await _studentService.GetProfileAsync(email);
            if (result == null)
            {
                return NotFound(new
                {
                    message = "Student profile not found"
                });
            }
            return Ok(result);
        }
    }
}