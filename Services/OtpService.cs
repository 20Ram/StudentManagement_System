using CrudOperation.Data;
using CrudOperation.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace CrudOperation.Service
{
  public class OtpService : IOtpService
  {
    private readonly AppDbContext _context;
    public OtpService(AppDbContext context)
    {
      _context = context;
    }
    public async Task<string> GenerateOtpAsync(string email)
    {
      var otpNumber = RandomNumberGenerator.GetInt32(100000,1000000);
      var otp = otpNumber.ToString();
      var otpVerification = new OtpVerification
      {
        Email = email,
        OtpHash = otp,
        ExpiresAt = DateTime.UtcNow.AddMinutes(5),
        IsUsed = false,
        Attempts = 0,
        CreatedAt = DateTime.UtcNow
      };
      _context.OtpVerifications.Add(otpVerification);
      await _context.SaveChangesAsync();
      return otp;
    }

    public async Task<bool> VerifyOtpAsync(string email, string otp)
    {
      var otpRecord = await _context.OtpVerifications
        .Where(o => o.Email == email && !o.IsUsed)
        .OrderByDescending(o => o.CreatedAt)
        .FirstOrDefaultAsync();
        if (otpRecord == null)
        {
          return false;
        }
        if (DateTime.UtcNow >= otpRecord.ExpiresAt)
        {
          _context.OtpVerifications.Remove(otpRecord);
          await _context.SaveChangesAsync();
          return false;
        }
        if (otpRecord.Attempts > 6)
        {
          return false;
        }
        otpRecord.Attempts++;

        if (otpRecord.OtpHash != otp)
        {
         await _context.SaveChangesAsync();
         return false;
        }
        otpRecord.IsUsed = true;
        await _context.SaveChangesAsync();
        return true;
    }
  }
}