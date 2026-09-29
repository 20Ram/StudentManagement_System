using CrudOperation.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrudOperation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Teacher")]
    public class TeacherController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TeacherController(AppDbContext context)
        {
            _context = context;
        }
 
        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            return Ok(new
            {
                message = "Welcome to Teacher Dashboard"
            });
        }
 
        [HttpGet("students")]
        public async Task<IActionResult> GetStudents()
        {
            var students = await _context.Users
                .Where(u => u.Roll == "Student")
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email
                })
                .ToListAsync();

            return Ok(students);
        }
 
        [HttpGet("student/{id}")]
        public async Task<IActionResult> GetStudent(int id)
        {
            var student = await _context.Users
                .Where(u => u.Id == id && u.Roll == "Student")
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.Roll
                })
                .FirstOrDefaultAsync();

            if (student == null)
            {
                return NotFound(new
                {
                    message = "Student not found"
                });
            }

            return Ok(student);
        }
        // [HttpPut("teacher/update/marks")]
        // public async Task<IActionResult> UpdateMarks(
        //   ,CreateTeacherDto dto
        // )
        // {
          
        // }
    }
}