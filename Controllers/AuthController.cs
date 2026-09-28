using Microsoft.AspNetCore.Mvc;
using CrudOperation.Data;
using CrudOperation.Models;
using CrudOperation.Service;
using CrudOperation.DTOs;
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

    public AuthController(AppDbContext context, IEmailService emailService, IOtpService otpService)
    {
      _context = context;
      _emailService = emailService;
      _otpService = otpService;
    }
    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser(RegisterRequestDto dto)
    {
       var existingUser = await _context.Users.FirstOrDefaultAsync(
               u => u.Email == dto.Email
       );
       if(existingUser != null)
      {
        return BadRequest(new{ Message = "user already exists"});    
      }

      var otp = await _otpService.GenerateOtpAsync(dto.Email);
      await _emailService.SendEmailAsync(dto.Email, "OTP Verification",$"Your OTP is: {otp}. It will expire in 5 minutes.");
      return Ok(new {Message = "OTP sent to your email. "});

    }
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp(VerifyOtpDto dto)
    {
      var isValid = await _otpService.VerifyOtpAsync(dto.Email, dto.Otp);
      if (!isValid)
      {
        return BadRequest(new { Message = "Invalid OTP"});
      }
      var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
      if (user == null)
      {
       return BadRequest(new { message = "User registration information not found." });
      }
      user.IsEmailVerified = true; 
      await _context.SaveChangesAsync(); 
      return Ok(new { message = "Registration successful. Email verified." });
    }

  }
}



// using CrudOperation.Data;
// using CrudOperation.DTOs;
// using CrudOperation.Models;
// using CrudOperation.Service;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;

// namespace CrudOperation.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class AuthController : ControllerBase
//     {
//         private readonly AppDbContext _context;
//         private readonly IEmailService _emailService;
//         private readonly IOtpService _otpService;

//         public AuthController(
//             AppDbContext context,
//             IEmailService emailService,
//             IOtpService otpService)
//         {
//             _context = context;
//             _emailService = emailService;
//             _otpService = otpService;
//         }

//         [HttpPost("register")]
//         public async Task<IActionResult> RegisterUser(RegisterRequestDto dto)
//         {
//             var email = dto.Email.Trim().ToLowerInvariant();

//             var existingUser = await _context.Users
//                 .FirstOrDefaultAsync(u => u.Email == email);

//             if (existingUser != null && existingUser.IsEmailVerified)
//             {
//                 return BadRequest(new
//                 {
//                     message = "Email is already registered."
//                 });
//             }

//             if (existingUser == null)
//             {
//                 var user = new User
//                 {
//                     Name = dto.Name.Trim(),
//                     Email = email,
//                     Roll = dto.Roll ?? "Student",
//                     IsEmailVerified = false
//                 };

//                 _context.Users.Add(user);
//                 await _context.SaveChangesAsync();
//             }
//             else
//             {
//                 existingUser.Name = dto.Name.Trim();
//                 await _context.SaveChangesAsync();
//             }

//             var otp = await _otpService.GenerateOtpAsync(email);

//             await _emailService.SendEmailAsync(
//                 email,
//                 "Student Management System - Email Verification",
//                 $"""
//                 Hello {dto.Name},

//                 Your OTP is: {otp}

//                 This OTP will expire in 5 minutes.

//                 Please do not share this OTP with anyone.

//                 Regards,
//                 Student Management System
//                 """
//             );

//             return Ok(new
//             {
//                 message = "OTP sent successfully to your email."
//             });
//         }

//         [HttpPost("verify-otp")]
//         public async Task<IActionResult> VerifyOtp(VerifyOtpDto dto)
//         {
//             var email = dto.Email.Trim().ToLowerInvariant();

//             var isValid = await _otpService.VerifyOtpAsync(
//                 email,
//                 dto.Otp.Trim()
//             );

//             if (!isValid)
//             {
//                 return BadRequest(new
//                 {
//                     message = "Invalid or expired OTP."
//                 });
//             }

//             var user = await _context.Users
//                 .FirstOrDefaultAsync(u => u.Email == email);

//             if (user == null)
//             {
//                 return BadRequest(new
//                 {
//                     message = "User not found."
//                 });
//             }

//             user.IsEmailVerified = true;

//             await _context.SaveChangesAsync();

//             return Ok(new
//             {
//                 message = "Registration successful. Email verified."
//             });
//         }
//     }
// }

