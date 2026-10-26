using System.Security.Claims;
using CrudOperation.DTOs;
using CrudOperation.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CrudOperation.Controllers
{
    [ApiController]
    [Route("api/fees")]
    [Authorize]
    public class FeesController : ControllerBase
    {
        private readonly IFeeService _fees;

        public FeesController(IFeeService fees)
        {
            _fees = fees;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public Task<IActionResult> Create([FromBody] CreateFeeDto dto, CancellationToken ct) =>
            Run(async () =>
            {
                var created = await _fees.CreateAsync(dto, GetEmail(), ct);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            });
 
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public Task<IActionResult> GetAll(
            [FromQuery] int? studentId, [FromQuery] string? status,
            [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20,
            CancellationToken ct = default) =>
            Run(async () => Ok(await _fees.GetAllAsync(studentId, status, pageNumber, pageSize, ct)));
 
        [HttpGet("my")]
        [Authorize(Roles = "Student")]
        public Task<IActionResult> My(CancellationToken ct) =>
            Run(async () =>
            {
                if (!TryGetUserId(out int userId))
                    return Unauthorized(new { message = "User ID claim is missing." });

                return Ok(await _fees.GetForUserAsync(userId, ct));
            });

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Student")]
        public Task<IActionResult> GetById(int id, CancellationToken ct) =>
            Run(async () =>
            {
                if (!TryGetUserId(out int userId))
                    return Unauthorized(new { message = "User ID claim is missing." });

                var fee = await _fees.GetByIdAsync(id, userId, User.IsInRole("Admin"), ct);
                return fee == null ? NotFound(new { message = "Fee not found." }) : Ok(fee);
            });

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public Task<IActionResult> Delete(int id, CancellationToken ct) =>
            Run(async () =>
            {
                await _fees.DeleteAsync(id, GetEmail(), ct);
                return Ok(new { message = "Fee deleted." });
            });

        private async Task<IActionResult> Run(Func<Task<IActionResult>> action)
        {
            try
            {
                return await action();
            }
            catch (PaymentException ex)
            {
                return StatusCode(ex.StatusCode, new { message = ex.Message });
            }
        }    

        private bool TryGetUserId(out int userId) =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);

        private string GetEmail() => User.FindFirstValue(ClaimTypes.Email) ?? "System";
    }
}
