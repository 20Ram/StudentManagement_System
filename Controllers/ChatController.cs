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
        private readonly ILogger<ChatController> _logger;

        public ChatController(
            IChatService chatService,
            ILogger<ChatController> logger)
        {
            _chatService = chatService;
            _logger = logger;
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
                    request.Message,
                    userId,
                    role,
                    request.ConversationId);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(
                    ex, "Chat AI request failed.");

                return StatusCode(502, new
                {
                    message = "The AI assistant is temporarily " +
                              "unavailable. Please try again."
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(
                    ex, "Chat AI configuration error.");

                return StatusCode(500, new
                {
                    message = "The AI service is not configured " +
                              "correctly."
                });
            }
        }
    }
}
