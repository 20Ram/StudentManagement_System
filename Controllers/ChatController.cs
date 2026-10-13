using CrudOperation.DTOs;
using CrudOperation.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CrudOperation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask(
            [FromBody] ChatRequestDto request)
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("sub") ??
                User.FindFirstValue("UserId");

            var role =
                User.FindFirstValue(ClaimTypes.Role) ??
                User.FindFirstValue("role");

            if (string.IsNullOrWhiteSpace(userId) ||
                string.IsNullOrWhiteSpace(role))
            {
                return Forbid();
            }

            try
            {
                var result = await _chatService.GetReplyAsync(
                    request.Message, userId, role);

                return Ok(result);
            }
            catch (HttpRequestException)
            {
                return StatusCode(502, new
                {
                    message = "The AI service is temporarily unavailable."
                });
            }
            catch (InvalidOperationException)
            {
                return StatusCode(500, new
                {
                    message = "The AI service is not configured correctly."
                });
            }
        }
    }
}