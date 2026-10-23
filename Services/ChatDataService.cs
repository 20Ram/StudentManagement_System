using CrudOperation.Data;
using Microsoft.EntityFrameworkCore;

namespace CrudOperation.Service
{
    public class ChatDataService : IChatDataService
    {
        private readonly AppDbContext _context;

        public ChatDataService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<string> GetStudentContextAsync(
            string userId)
        {
            if (!int.TryParse(userId, out int id))
            {
                return "The authenticated user ID is invalid.";
            }

            var student = await _context.Students
                .AsNoTracking()
                .Where(s => s.UserId == id)
                .Select(s => new
                {
                    s.Name,
                    s.Email,
                    s.Age,
                    s.Course,
                    s.IsActive,
                    s.Marks
                })
                .FirstOrDefaultAsync();

            if (student == null)
            {
                return "No student profile was found for this account.";
            }

            return $"""
                Authorized student profile:
                Name: {student.Name}
                Email: {student.Email}
                Age: {student.Age}
                Course: {student.Course}
                Active: {student.IsActive}
                Marks: {student.Marks}
                """;
        }

        public async Task<string> GetCourseContextAsync(
            string userId,
            string role)
        {
            if (!int.TryParse(userId, out int id))
            {
                return "The authenticated user ID is invalid.";
            }

            if (role.Equals(
                "Student", StringComparison.OrdinalIgnoreCase))
            {
                var student = await _context.Students
                    .AsNoTracking()
                    .Where(s => s.UserId == id)
                    .Select(s => new
                    {
                        s.Name,
                        s.Course,
                        s.IsActive
                    })
                    .FirstOrDefaultAsync();

                if (student == null)
                {
                    return "No student course information was found.";
                }

                return $"""
                    Authorized course information:
                    Student: {student.Name}
                    Course: {student.Course}
                    Active: {student.IsActive}
                    """;
            }

            if (role.Equals(
                "Teacher", StringComparison.OrdinalIgnoreCase))
            {
                var teacher = await _context.Teachers
                    .AsNoTracking()
                    .Where(t => t.UserId == id)
                    .Select(t => new
                    {
                        t.Name,
                        t.Department,
                        t.IsActive
                    })
                    .FirstOrDefaultAsync();

                if (teacher == null)
                {
                    return "No teacher profile was found for this account.";
                }

                return $"""
                    Authorized teacher information:
                    Teacher: {teacher.Name}
                    Department: {teacher.Department}
                    Active: {teacher.IsActive}

                    The current data model does not define teacher-to-course
                    assignments, so assigned courses cannot be verified.
                    """;
            }

            if (role.Equals(
                "Admin", StringComparison.OrdinalIgnoreCase))
            {
                var courses = await _context.Students
                    .AsNoTracking()
                    .Where(s => s.Course != null && s.Course != "")
                    .Select(s => s.Course)
                    .Distinct()
                    .ToListAsync();

                return "Distinct course names found in student records: " +
                    (courses.Count == 0
                        ? "No course names found."
                        : string.Join(", ", courses));
            }

            return "Course information is unavailable for this role.";
        }

        public async Task<string> GetStudentStatisticsAsync(
            string role)
        {
            bool isAdmin = role.Equals(
                "Admin", StringComparison.OrdinalIgnoreCase);

            bool isTeacher = role.Equals(
                "Teacher", StringComparison.OrdinalIgnoreCase);

            if (!isAdmin && !isTeacher)
            {
                return "Student statistics are not available for this role.";
            }

            var totalStudents = await _context.Students.CountAsync();

            var activeStudents = await _context.Students
                .CountAsync(s => s.IsActive);

            var inactiveStudents = totalStudents - activeStudents;

            var averageMarks = totalStudents > 0
                ? await _context.Students.AverageAsync(s => s.Marks)
                : 0;

            return $"""
                Student statistics:
                Total students: {totalStudents}
                Active students: {activeStudents}
                Inactive students: {inactiveStudents}
                Average marks: {averageMarks:F2}

                These statistics were calculated from the Students table.
                """;
        }

        public async Task<string> GetDocumentContextAsync(
            string userId,
            string role)
        {
            if (!int.TryParse(userId, out int id))
            {
                return "The authenticated user ID is invalid.";
            }

            IQueryable<Models.Document> query =
                _context.Documents.AsNoTracking();

            if (role.Equals("Student",
                StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(d =>
                    d.student != null &&
                    d.student.UserId == id);
            }
            else if (role.Equals("Admin",
                StringComparison.OrdinalIgnoreCase))
            {
            }
            else if (role.Equals("Teacher",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Teacher document access is not configured " +
                       "until student assignment permissions are implemented.";
            }
            else
            {
                return "Document access is not available for this role.";
            }

            var documents = await query
                .OrderByDescending(d => d.UploadedAt)
                .Select(d => new
                {
                    d.FileName,
                    d.ContentType,
                    d.FileSize,
                    d.UploadedAt
                })
                .Take(50)
                .ToListAsync();

            if (documents.Count == 0)
            {
                return "No authorized documents were found.";
            }

            return "Authorized document list:\n" +
                string.Join("\n", documents.Select(d =>
                    $"File: {d.FileName}; " +
                    $"Type: {d.ContentType}; " +
                    $"Size: {d.FileSize} bytes; " +
                    $"Uploaded: {d.UploadedAt:yyyy-MM-dd HH:mm}"));
        }

        public async Task<string> GetFeeContextAsync(string studentId)
        {
            if (!int.TryParse(studentId, out int id))
            {
                return "The authenticated student ID is invalid.";
            }

            var fees = await _context.StudentFees
                .AsNoTracking()
                .Where(f => f.StudentId == id) 
                .Select(f => new
                {
                    f.PaidAmount,
                    f.TotalAmount,
                    f.DueDate,
                    f.Title,
                })
                .ToListAsync(); 

            if (fees.Count == 0)
            {
                return "No fee details were found for this account.";
            }

            var feeDetailsList = fees.Select(f => $"""
                - Title: {f.Title}
                Paid Amount: {f.PaidAmount}
                Total Amount: {f.TotalAmount}
                Due Date: {f.DueDate:yyyy-MM-dd}
                """);

            return "Authorized student Fee Details:\n" + string.Join("\n", feeDetailsList);
        }
    }
}
