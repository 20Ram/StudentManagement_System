using CrudOperation.Data;
using CrudOperation.DTOs;
using CrudOperation.Models;
using Microsoft.EntityFrameworkCore;

namespace CrudOperation.Service
{
    public class FeeService : IFeeService
    {
        private readonly AppDbContext _context;

        public FeeService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<FeeResponseDto> CreateAsync(
            CreateFeeDto dto, string adminEmail, CancellationToken cancellationToken = default)
        {
            decimal total = decimal.Round(dto.TotalAmount, 2, MidpointRounding.AwayFromZero);
            if (total <= 0)
                throw new PaymentException("Total amount must be greater than zero.");

            var student = await _context.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == dto.StudentId && s.IsActive, cancellationToken);

            if (student == null)
                throw new PaymentException("Student not found or inactive.", 404);

            var fee = new StudentFee
            {
                StudentId = student.Id,
                Title = dto.Title.Trim(),
                TotalAmount = total,
                PaidAmount = 0,
                DueDate = dto.DueDate
            };

            _context.StudentFees.Add(fee);
            _context.AuditLogs.Add(new AuditLog
            {
                UserEmail = adminEmail,
                Action = "CREATE_FEE",
                Details = $"Created fee '{fee.Title}' of {total:0.00} for student {student.Email}"
            });
            await _context.SaveChangesAsync(cancellationToken);

            return ToDto(fee, student.Name);
        }

        public async Task<List<FeeResponseDto>> GetAllAsync(
            int? studentId, string? status, int pageNumber, int pageSize,
            CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var query = _context.StudentFees
                .AsNoTracking()
                .Include(f => f.Student)
                .AsQueryable();

            if (studentId.HasValue)
                query = query.Where(f => f.StudentId == studentId.Value);

            if (!string.IsNullOrWhiteSpace(status))
            {
                var today = DateTime.UtcNow.Date;
                switch (status.Trim().ToLowerInvariant())
                {
                    case "paid":
                        query = query.Where(f => f.PaidAmount >= f.TotalAmount);
                        break;
                    case "unpaid":
                        query = query.Where(f => f.PaidAmount == 0);
                        break;
                    case "partial":
                        query = query.Where(f => f.PaidAmount > 0 && f.PaidAmount < f.TotalAmount);
                        break;
                    case "overdue":
                        query = query.Where(f => f.PaidAmount < f.TotalAmount && f.DueDate < today);
                        break;
                    default:
                        throw new PaymentException("Status must be one of: paid, unpaid, partial, overdue.");
                }
            }

            var fees = await query
                .OrderByDescending(f => f.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return fees.Select(f => ToDto(f, f.Student?.Name)).ToList();
        }

        public async Task<List<FeeResponseDto>> GetForUserAsync(
            int userId, CancellationToken cancellationToken = default)
        {
            var fees = await _context.StudentFees
                .AsNoTracking()
                .Include(f => f.Student)
                .Where(f => f.Student.UserId == userId)
                .OrderBy(f => f.DueDate)
                .ToListAsync(cancellationToken);

            return fees.Select(f => ToDto(f, f.Student?.Name)).ToList();
        }

        public async Task<FeeResponseDto?> GetByIdAsync(
            int id, int userId, bool isAdmin, CancellationToken cancellationToken = default)
        {
            var fee = await _context.StudentFees
                .AsNoTracking()
                .Include(f => f.Student)
                .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

            if (fee == null) return null;
            // Same answer as "not found" so ids can't be probed.
            if (!isAdmin && fee.Student.UserId != userId) return null;

            return ToDto(fee, fee.Student?.Name);
        }

        public async Task DeleteAsync(
            int id, string adminEmail, CancellationToken cancellationToken = default)
        {
            var fee = await _context.StudentFees
                .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

            if (fee == null)
                throw new PaymentException("Fee not found.", 404);

            bool hasPayments = fee.PaidAmount > 0 ||
                await _context.PaymentTransactions.AnyAsync(t => t.FeeItemId == id, cancellationToken);

            if (hasPayments)
                throw new PaymentException(
                    "This fee already has payment activity and cannot be deleted.");

            _context.StudentFees.Remove(fee);
            _context.AuditLogs.Add(new AuditLog
            {
                UserEmail = adminEmail,
                Action = "DELETE_FEE",
                Details = $"Deleted fee {id} '{fee.Title}'"
            });
            await _context.SaveChangesAsync(cancellationToken);
        }

        private static FeeResponseDto ToDto(StudentFee fee, string? studentName)
        {
            decimal due = Math.Max(0, fee.TotalAmount - fee.PaidAmount);

            string status;
            if (due == 0) status = "Paid";
            else if (fee.DueDate.Date < DateTime.UtcNow.Date) status = "Overdue";
            else if (fee.PaidAmount > 0) status = "Partial";
            else status = "Unpaid";

            return new FeeResponseDto
            {
                Id = fee.Id,
                StudentId = fee.StudentId,
                StudentName = studentName ?? string.Empty,
                Title = fee.Title,
                TotalAmount = fee.TotalAmount,
                PaidAmount = fee.PaidAmount,
                DueAmount = due,
                DueDate = fee.DueDate,
                Status = status
            };
        }
    }
}
