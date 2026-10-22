using Microsoft.EntityFrameworkCore;
using CrudOperation.Models;

namespace CrudOperation.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<Student> Students { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<OtpVerification> OtpVerifications { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; } = null!;
        public DbSet<StudentFee> StudentFees => Set<StudentFee>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasOne(u => u.Student)
                .WithOne(s => s.User)
                .HasForeignKey<Student>(s => s.UserId);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Teacher)
                .WithOne(t => t.User)
                .HasForeignKey<Teacher>(t => t.UserId);

            modelBuilder.Entity<Student>()
                .HasMany(s => s.Documents)
                .WithOne(d => d.student)
                .HasForeignKey(d => d.StudentId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.UploadDocuments)
                .WithOne(d => d.userUploaded)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PaymentTransaction>(e => {
                e.HasKey(x => x.Id);
                e.Property(x => x.Amount).HasPrecision(10, 2);
                e.Property(x => x.Currency).HasMaxLength(3).IsRequired();
                e.Property(x => x.Method).HasMaxLength(20).IsRequired();
                e.Property(x => x.UserId).HasMaxLength(100).IsRequired();
                e.Property(x => x.RazorpayOrderId).HasMaxLength(100).IsRequired();
                e.Property(x => x.RazorpayPaymentId).HasMaxLength(100);
                e.Property(x => x.Status).HasMaxLength(20).IsRequired();
                e.Property(x => x.FailureReason).HasMaxLength(250);
                e.HasIndex(x => x.RazorpayOrderId).IsUnique();
 
                e.HasIndex(x => x.RazorpayPaymentId).IsUnique();
                e.HasIndex(x => new { x.UserId, x.CreatedAtUtc });
                e.HasIndex(x => new { x.FeeItemId, x.Status });
  
                e.HasOne<StudentFee>()
                    .WithMany()
                    .HasForeignKey(x => x.FeeItemId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<StudentFee>(e =>
            {
                e.Property(x => x.Title).HasMaxLength(100).IsRequired();
                e.Property(x => x.TotalAmount).HasPrecision(10, 2);
                e.Property(x => x.PaidAmount).HasPrecision(10, 2);
            });
        }
    }
}