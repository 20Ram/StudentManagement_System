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