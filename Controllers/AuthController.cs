using CrudOperation.Data;
using CrudOperation.DTOs;
using CrudOperation.Models;
using CrudOperation.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrudOperation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IOtpService _otpService;
        private readonly IJwtService _jwtService;

        public AuthController(
            AppDbContext context,
            IEmailService emailService,
            IOtpService otpService,    
            IJwtService jwtService)
        {
            _context = context;
            _emailService = emailService;
            _otpService = otpService;
            _jwtService = jwtService;
        } 

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser(RegisterRequestDto dto)

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
                    IsEmailVerified = false
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            else
            {
                existingUser.Name = dto.Name.Trim();
                await _context.SaveChangesAsync();
            }

            var otp = await _otpService.GenerateOtpAsync(email);

            await _emailService.SendEmailAsync(
                email,
                "Student Management System - Email Verification",
                $"""
                Hello {dto.Name},

                Your OTP is: {otp}

                This OTP will expire in 5 minutes.

                Please do not share this OTP with anyone.

                Regards,
                Student Management System
                """
            );

            return Ok(new
            {
                message = "OTP sent successfully to your email."
            });
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp(VerifyOtpDto dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();

            var isValid = await _otpService.VerifyOtpAsync(
                email,
                dto.Otp.Trim()
            );

            if (!isValid)
            {
                return BadRequest(new
                {
                    message = "Invalid or expired OTP."
                });
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return BadRequest(new
                {
                    message = "User not found."
                });
            }

            user.IsEmailVerified = true;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Registration successful. Email verified."
            });
        }
        [HttpPost("Login")]
        public async Task<IActionResult> LoginUser(LoginRequestDto dto)
        {
           var email = dto.Email.Trim().ToLowerInvariant();
           var user = await _context.Users.FirstOrDefaultAsync(u=> u.Email == email);
           if(user == null)
           {
              return BadRequest(new { message = "User Not Found."});
           }
           if(!user.IsEmailVerified)
           {
             return BadRequest(new { message = "Email Not Verified."});
           }
           var otp = await _otpService.GenerateOtpAsync(email);
           await _emailService.SendEmailAsync(
                email,
                "Student Management System - Email Verification",
                $"""
                Hello {user.Name},

                Your OTP is: {otp}

                This OTP will expire in 5 minutes.

                Please do not share this OTP with anyone.

                Regards,
                Student Management System
                """
            );
            return Ok(new
            {
                message = "Login OTP sent Successfully to your email."
            }); 
        }
        [HttpPost("verify-Login")]
        public async Task<IActionResult> VerifyLogin(VerifyOtpDto dto)
        {
        var email = dto.Email.Trim().ToLowerInvariant();
        var isValid = await _otpService.VerifyOtpAsync(
            email,
            dto.Otp.Trim()
        );
        if (!isValid)
        {
          return BadRequest(new
          {
            message = "Invalid or expired OTP."
          });
        }
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            return BadRequest(new
            {
                message = "User not found."
            });
        }
        var token = _jwtService.GenerateToken(user);
        return Ok(new LoginResponseDto
        {
          UserId = user.Id,
          Email = user.Email,
          Name = user.Name,
          Roll = user.Roll,
          Token = token
        });
        }
    }
}

