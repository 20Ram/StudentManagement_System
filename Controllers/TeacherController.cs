using CrudOperation.Data;
using CrudOperation.DTOs;
using CrudOperation.Models;
using CrudOperation.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CrudOperation.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  [Authorize(Roles = "Teacher")]
  public class TeacherController : ControllerBase
  {
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IAcademicAccessService _access;
    public TeacherController(AppDbContext context, IEmailService emailService, IAcademicAccessService access)
    {
      _context = context;
      _emailService = emailService;
      _access = access;
    }

    private int CurrentUserId =>
      int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    [HttpGet("dashboard")]
    public IActionResult Dashboard()
    {
      var totalStudent = _context.Students.Count();
      var average_Marks = _context.Students.Average(s => s.Marks);
      return Ok(new
      {
        message = "Welcome to Teacher Dashboard",
        totalStudent,
        average_Marks
      });
    }

    // [HttpGet("students")]
    // public async Task<IActionResult> GetStudents()
    // {
    //   var students = await _context.Users
    //       .Where(u => u.Roll == "Student" && u.IsEmailVerified == true)
    //       .ToListAsync();

    //   return Ok(students);
    // }

    [HttpGet("students")]
    public async Task<IActionResult> GetStudents()
    {
      var userId = CurrentUserId;

      var students = await _context.StudentEnrollments
        .AsNoTracking()
        .Where(e => e.IsCurrent && e.Student.IsActive &&
          _context.TeacherAssignments.Any(a =>
            a.Teacher.UserId == userId &&
            a.Teacher.IsActive &&
            a.Subject.IsActive &&
            a.Subject.ClassId == e.ClassId))
        .Select(e => new
        {
          e.Student.User!.Id,
          e.Student.User.Email,
          e.Student.User.Name,
          e.Student.User.Roll,
          e.Student.User.IsEmailVerified,
          e.Student.User.IsActive
        })
        .Distinct()
        .OrderBy(u => u.Name)
        .ToListAsync();

      return Ok(students);
    }

    [HttpGet("student")]
    public async Task<IActionResult> GetStudent(string email)
    {
      var student = await _context.Users
          .Where(u => u.Email == email && u.Roll == "Student")
          .Select(u => new
          {
            u.Id,
            u.Name,
            u.Email,
            u.Roll
          })
          .FirstOrDefaultAsync();
      var students = await _context.Students
          .Where(s => s.Email == email)
          .Select(s => new
          {
            s.Age,
            s.Course,
            s.Marks,
          })
          .FirstOrDefaultAsync();
      if (student == null || students == null)
      {
        return NotFound(new
        {
          message = "Student not found"
        });
      }
      
      return Ok(new
      {
        student,
        students
      });
    }

    [HttpGet("teacher-profile")]
    public async Task<IActionResult> Profile()
    {
      var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
      var teacher = await _context.Users
          .Where(u => u.Email == email && u.Roll == "Teacher")
          .Select(u => new
          {
            u.Id,
            u.Name,
            u.Email,
            u.Roll
          }).FirstOrDefaultAsync();
      var teachers = await _context.Teachers
          .Where(t => t.Email == email)
          .Select(t => new
          {
            t.Age,
            t.Department,
          }).FirstOrDefaultAsync();

      if (teacher == null || teachers == null)
      {
        return NotFound(new
        {
          message = "Teacher profile not found"
        });
      }
      return Ok(new
      {
        teacher.Id,
        teacher.Email,
        teacher.Name,
        teacher.Roll,
        teachers.Age,
        teachers.Department
      });
    }

    [HttpPut("Update-student{email}")]
    public async Task<IActionResult> UpdateStudent(string email, UpdateStudentDto dto)
    {


      if (string.IsNullOrEmpty(email))
      {
        return NotFound(new
        {
          message = "Email is Not Enterd."
        });
      }

      var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

      if (user == null)
      {
        return NotFound(new
        {
          message = "Student not found"
        });
      }
      var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);

      if (student == null)
      {
        return NotFound(new
        {
          message = "Student record not found"
        });
      }

      if (!string.IsNullOrWhiteSpace(dto.Name))
      {
        user.Name = dto.Name;
        student.Name = dto.Name;
      }

      if (!string.IsNullOrWhiteSpace(dto.Email))
      {
        user.Email = dto.Email;
        student.Email = dto.Email;
      }

      if (dto.Age.HasValue)
      {
        student.Age = dto.Age.Value;
      }

      if (!string.IsNullOrWhiteSpace(dto.Course))
      {
        student.Course = dto.Course;
      }

      if (dto.Marks.HasValue)
      {
        student.Marks = dto.Marks.Value;
      }

      if (dto.Marks.HasValue)
      {
        await _emailService.SendEmailAsync(
            student.Email ?? "defult@gmail.com",
            "Student Management System - Marks Updated",
            $"Hello {student.Name},\n\nYour profile for Marks  '{student.Course}' have been updated by your teacher. Your current marks are: {student.Marks}.\n\nRegards,\nStudent Management System"
        );
      }

      _context.AuditLogs.Add(new AuditLog
      {
        UserEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "System",
        Action = "UPDATE_STUDENT_MARKS",
        Details = $"Updated marks for student {student.Email} to {student.Marks}"
      });
      await _context.SaveChangesAsync();
      return Ok(new
      {
        student = new
        {
          user.Id,
          user.Name,
          user.Email,
          student.Age,
          student.Course,
          student.Marks
        }
      });
    }

    [HttpPut("Update-teacher")]
    public async Task<IActionResult> UpdateTeacher(UpdateTeacherDto dto)
    {
      var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

      if (string.IsNullOrEmpty(email))
      {
        return Unauthorized(new
        {
          message = "Email claim missing from token"
        });
      }

      var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

      if (user == null)
      {
        return NotFound(new
        {
          message = "User not found"
        });
      }
      var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == user.Id);

      if (teacher == null)
      {
        return NotFound(new
        {
          message = "Teacher record not found"
        });
      }

      if (!string.IsNullOrWhiteSpace(dto.Name))
      {
        user.Name = dto.Name;
        teacher.Name = dto.Name;
      }

      if (!string.IsNullOrWhiteSpace(dto.Email))
      {
        user.Email = dto.Email;
        teacher.Email = dto.Email;
      }

      if (dto.Age.HasValue)
      {
        teacher.Age = dto.Age.Value;
      }

      if (!string.IsNullOrWhiteSpace(dto.Department))
      {
        teacher.Department = dto.Department;
      }

      _context.AuditLogs.Add(new AuditLog
      {
        UserEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "System",
        Action = "UPDATE_Teacher",
        Details = $"Updated Teacher Profile {teacher.Email}"
      });

      await _context.SaveChangesAsync();
      return Ok(new
      {
        student = new
        {
          user.Id,
          user.Name,
          user.Email,
          teacher.Age,
          teacher.Department,
        }
      });
    }
  }
}