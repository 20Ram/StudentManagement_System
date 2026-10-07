using CrudOperation.Data;
using CrudOperation.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CrudOperation.Service
{
    public class StudentService : IStudentService
    {
        private readonly AppDbContext _context;

        public StudentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<object> UpdateAsync(UpdateStudentDto dto,string email)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return null;

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == user.Id);

            if (student == null)
                return null;

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

            await _context.SaveChangesAsync();

            return new
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
            };
        }

        public async Task<object?> GetProfileAsync(
            string email)
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

            if (student == null)
                return null;

            var studentDetails = await _context.Students
                .Where(s => s.Email == email)
                .Select(s => new
                {
                    s.Age,
                    s.Course,
                    s.Marks
                })
                .FirstOrDefaultAsync();

            if (studentDetails == null)
                return null;

            return new
            {
                student.Id,
                student.Email,
                student.Name,
                student.Roll,
                studentDetails.Age,
                studentDetails.Course,
                studentDetails.Marks
            };
        }
        public async Task<PaginatedResponseDto<object>> GetStudentsAsync(PaginationQueryDto query)
        {
     
          if (query.PageNumber < 1)
          {
            query.PageNumber = 1;
          }
    
          if (query.PageSize < 1)
          {
            query.PageSize = 10;
          }

          if (query.PageSize > 100)
          {
            query.PageSize = 100;
          }

          var studentsQuery = _context.Students.AsNoTracking();
 
          if (!string.IsNullOrWhiteSpace(query.Search))
          {
            var search = query.Search.Trim();

            studentsQuery = studentsQuery.Where(s => s.Name.Contains(search) || s.Email.Contains(search) || s.Course.Contains(search));
          }
 
          query.SortBy = query.SortBy.ToLower() ?? "id";
          query.SortOrder = query.SortOrder.ToLower() ?? "asc";

          if (query.SortBy == "name")
          {
            studentsQuery = query.SortOrder == "desc"? studentsQuery.OrderByDescending(s => s.Name): studentsQuery.OrderBy(s => s.Name);
          }
          else if (query.SortBy == "email")
          {
            studentsQuery = query.SortOrder == "desc"? studentsQuery.OrderByDescending(s => s.Email): studentsQuery.OrderBy(s => s.Email);
          }
          else if (query.SortBy == "age")
          {
            studentsQuery = query.SortOrder == "desc" ? studentsQuery.OrderByDescending(s => s.Age): studentsQuery.OrderBy(s => s.Age);
          }
          else if (query.SortBy == "marks")
          {
            studentsQuery = query.SortOrder == "desc" ? studentsQuery.OrderByDescending(s => s.Marks): studentsQuery.OrderBy(s => s.Marks);
          }
          else
          {
            studentsQuery = query.SortOrder == "desc" ? studentsQuery.OrderByDescending(s => s.Id): studentsQuery.OrderBy(s => s.Id);
          }
 
          var totalRecord = await studentsQuery.CountAsync();
 
          var students = await studentsQuery
                         .Skip((query.PageNumber - 1) * query.PageSize)
                         .Take(query.PageSize)
                         .Select(s => (object)new
                         {
                           s.Id, 
                           s.Name,
                           s.Email,
                           s.Age,
                           s.Course,
                           s.Marks
                         })
                         .ToListAsync();
 

          var totalPage = (int)Math.Ceiling((double)totalRecord / query.PageSize);

          return new PaginatedResponseDto<object>
          {
             Data = students,
             PageNumber = query.PageNumber,
             PageSize = query.PageSize,
             TotalRecord = totalRecord,
             TotalPage = totalPage
          };
        }
    }
}