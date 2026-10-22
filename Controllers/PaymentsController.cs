using System.Security.Claims;
using CrudOperation.DTOs;
using CrudOperation.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CrudOperation.Controllers
{
    [ApiController]
    [Route("api/payments")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IRazorpayPaymentService _payments;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(IRazorpayPaymentService payments, ILogger<PaymentsController> logger)
        {
            _payments = payments;
            _logger = logger;
        }

        [HttpPost("create-order")]
        [Authorize(Roles = "Student,Admin")]
        [EnableRateLimiting("create-order")]
        public Task<IActionResult> CreateOrder([FromBody] CreatePaymentOrderDto dto, CancellationToken ct) =>
            Run(async () =>
            {
                string? userId = GetUserId();
                if (string.IsNullOrWhiteSpace(userId))
                    return Unauthorized(new { message = "User ID claim is missing." });

                return Ok(await _payments.CreateOrderAsync(dto.FeeItemId, userId, IsAdmin(), ct));
            });

        [HttpPost("verify")]
        [Authorize(Roles = "Student,Admin")]
        [EnableRateLimiting("verify-payment")]
        public Task<IActionResult> Verify([FromBody] VerifyPaymentDto dto, CancellationToken ct) =>
            Run(async () =>
            {
                string? userId = GetUserId();
                if (string.IsNullOrWhiteSpace(userId))
                    return Unauthorized(new { message = "User ID claim is missing." });

                var result = await _payments.VerifyPaymentAsync(dto, userId, IsAdmin(), ct);
                return result.Success
                    ? Ok(new { message = result.Message })
                    : BadRequest(new { message = result.Message });
            });

        [HttpGet("history")]
        [Authorize(Roles = "Admin,Student")]
        public Task<IActionResult> History(CancellationToken ct) =>
            Run(async () =>
            {
                string? userId = GetUserId();
                if (string.IsNullOrWhiteSpace(userId))
                    return Unauthorized(new { message = "User ID claim is missing." });

                return Ok(await _payments.GetHistoryAsync(userId, IsAdmin(), ct));
            });

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Student")]
        public Task<IActionResult> Get(int id, CancellationToken ct) =>
            Run(async () =>
            {
                string? userId = GetUserId();
                if (string.IsNullOrWhiteSpace(userId))
                    return Unauthorized(new { message = "User ID claim is missing." });

                var p = await _payments.GetByIdAsync(id, userId, IsAdmin(), ct);
                return p == null
                    ? NotFound(new { message = "Payment transaction not found." })
                    : Ok(p);
            });


        // [HttpPost("webhook")]
        // [AllowAnonymous]
        // [RequestSizeLimit(1_000_000)]
        // public async Task<IActionResult> Webhook(CancellationToken ct)
        // {
        //     // The signature is computed over the exact bytes Razorpay sent - read them raw.
        //     using var ms = new MemoryStream();
        //     await Request.Body.CopyToAsync(ms, ct);
        //     byte[] rawBody = ms.ToArray();

        //     string? signature = Request.Headers["X-Razorpay-Signature"].FirstOrDefault();

        //     try
        //     {
        //         bool ok = await _payments.HandleWebhookAsync(rawBody, signature, ct);
        //         return ok ? Ok() : BadRequest();
        //     }
        //     catch (Exception ex)
        //     {
        //         // 500 tells Razorpay to retry later (it retries for up to 24 hours).
        //         _logger.LogError(ex, "Razorpay webhook processing failed.");
        //         return StatusCode(500);
        //     }
        // }

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

        private string? GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? User.FindFirstValue("UserId");

        private bool IsAdmin() => User.IsInRole("Admin");
    }
}
