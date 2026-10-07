using CrudOperation.DTOs;
using CrudOperation.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrudOperation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Teacher")]
    public class StudentPaginationController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentPaginationController(
            IStudentService studentService)
        {
            _studentService = studentService;
        }

        [HttpGet("students")]
        public async Task<IActionResult> GetStudents(
            [FromQuery] PaginationQueryDto query)
        {
            var result = await _studentService
                .GetStudentsAsync(query);

            return Ok(result);
        }
    }
}