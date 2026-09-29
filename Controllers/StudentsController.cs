using CrudOperation.Models;
using Microsoft.AspNetCore.Mvc;
using CrudOperation.Data;
using CrudOperation.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace CrudOperation.Controllers
{
 [ApiController]
 [Route("api/[controller]")]
 [Authorize]
 public class StudentsController : ControllerBase
  {
    private readonly AppDbContext _context;
    
    public StudentsController(AppDbContext context)
    {
      _context = context;
    } 

    [HttpGet]
    public IActionResult GetAllStudents()
    {
      var students = _context.Students.ToList();
      return Ok(students);
    }

    [HttpGet("{id}")]
    public IActionResult GetStudent(int id)
    {
      var student = _context.Students.FirstOrDefault(s => s.Id == id);

      if (student == null)
      {
        return NotFound(student);
      }
      return Ok(student);
    }

    [HttpPost]
    public IActionResult CreateStudent(CreateStudentDto dto)
    {
      var student = new Student
      {
        Name = dto.Name,
        Email = dto.Email,
        Age = dto.Age,
        Course = dto.Course,
        Marks = dto.Marks
      };
      _context.Students.Add(student);
      _context.SaveChanges();
      return Ok(student);
    }
    
    [HttpPut("{id}")]
    public IActionResult UpdateStudent(int id,UpdateStudentDto dto)
    {
      var existingStudent = _context.Students.FirstOrDefault(s => s.Id == id);
      if (existingStudent == null)
      {
        return NotFound(existingStudent);
      }
      existingStudent.Name = dto.Name;
      existingStudent.Age = dto.Age;
      existingStudent.Email = dto.Email;
      existingStudent.Course = dto.Course;
      existingStudent.Marks = dto.Marks;
      
      _context.SaveChanges();
      return Ok(existingStudent);
    }
    // [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]

    public IActionResult DeleteStudent(int id)
    {
      var removeStudent = _context.Students.FirstOrDefault(s => s.Id == id);
      if (removeStudent == null)
      {
        return NotFound(removeStudent);
      }
      _context.Students.Remove(removeStudent);
      _context.SaveChanges();
      return Ok(removeStudent);
    }
    [Authorize(Roles = "Admin")]
    [HttpGet("admin")]
    public IActionResult AdminOnly()
    {
      return Ok(new
      {
        message = "Welcome Admin!"
      });
    }
  } 
}