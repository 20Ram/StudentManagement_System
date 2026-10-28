// using Microsoft.EntityFrameworkCore;
// using CrudOperation.Models;


// namespace CrudOperation.Data
// {
//     public class AppDbContext : DbContext
//     {
//         public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
//         {

//         }
//         public DbSet<Student> Students { get; set; }
//         public DbSet<User> Users { get; set; }
//         public DbSet<OtpVerification> OtpVerifications { get; set; }
//         public DbSet<Teacher> Teachers { get; set; }
//         public DbSet<Document> Documents { get; set; }
//         public DbSet<ChatMessage> ChatMessages { get; set; } = null!;
//         public DbSet<AuditLog> AuditLogs { get; set; }
//         public DbSet<PaymentTransaction> PaymentTransactions { get; set; } = null!;
//         public DbSet<Course> Courses { get; set; }
//         public DbSet<Class> Classes { get; set; }
//         public DbSet<Subject> Subjects { get; set; }
//         public DbSet<StudentEnrollment> StudentEnrollments { get; set; }
//         public DbSet<TeacherAssignment> TeacherAssignments { get; set; }
//         public DbSet<StudentFee> StudentFees => Set<StudentFee>();


//         protected override void OnModelCreating(ModelBuilder modelBuilder)
//         {
//             modelBuilder.Entity<User>()
//                 .HasOne(u => u.Student)
//                 .WithOne(s => s.User)
//                 .HasForeignKey<Student>(s => s.UserId);

//             modelBuilder.Entity<User>()
//                 .HasOne(u => u.Teacher)
//                 .WithOne(t => t.User)
//                 .HasForeignKey<Teacher>(t => t.UserId);

//             modelBuilder.Entity<Student>()
//                 .HasMany(s => s.Documents)
//                 .WithOne(d => d.student)
//                 .HasForeignKey(d => d.StudentId);

//             modelBuilder.Entity<User>()
//                 .HasMany(u => u.UploadDocuments)
//                 .WithOne(d => d.userUploaded)
//                 .HasForeignKey(d => d.UserId)
//                 .OnDelete(DeleteBehavior.Restrict);

//             modelBuilder.Entity<Class>()
//                 .HasOne(c => c.Course)
//                 .WithMany(c => c.Classes)
//                 .HasForeignKey(c => c.CourseId);

//             modelBuilder.Entity<Subject>()
//                 .HasOne(s => s.Class)
//                 .WithMany(c => c.Subjects)
//                 .HasForeignKey(s => s.ClassId);

//             modelBuilder.Entity<StudentEnrollment>()
//                 .HasOne(e => e.Student)
//                 .WithMany()
//                 .HasForeignKey(e => e.StudentId);

//             modelBuilder.Entity<StudentEnrollment>()
//                 .HasOne(e => e.Class)
//                 .WithMany(c => c.StudentEnrollments)
//                 .HasForeignKey(e => e.ClassId);

//             modelBuilder.Entity<TeacherAssignment>()
//                 .HasOne(t => t.Teacher)
//                 .WithMany()
//                 .HasForeignKey(t => t.TeacherId);

//             modelBuilder.Entity<TeacherAssignment>()
//                 .HasOne(t => t.Subject)
//                 .WithMany(s => s.TeacherAssignments)
//                 .HasForeignKey(t => t.SubjectId);

//             modelBuilder.Entity<StudentEnrollment>()
//                 .HasIndex(e => new { e.StudentId, e.ClassId })
//                 .IsUnique();

//             modelBuilder.Entity<TeacherAssignment>()
//                 .HasIndex(a => new { a.TeacherId, a.SubjectId })
//                 .IsUnique();

//             modelBuilder.Entity<Class>(e =>
//             {
//                 e.HasKey(c => c.Id);
//                 e.Property(c => c.Section).HasMaxLength(10).IsRequired();
//                 e.Property(c => c.AcademicYear).HasMaxLength(20).IsRequired();
//                 e.Property(c => c.Semester).IsRequired();

//                 e.HasOne(c => c.Course)
//                  .WithMany(course => course.Classes)
//                  .HasForeignKey(c => c.CourseId)
//                  .OnDelete(DeleteBehavior.Restrict);

//                 e.HasIndex(c => new
//                 {
//                     c.CourseId,
//                     c.Semester,
//                     c.Section,
//                     c.AcademicYear
//                 }).IsUnique();
//             });

//             modelBuilder.Entity<Subject>(e =>
//             {
//                 e.Property(s => s.Code)
//                     .HasMaxLength(20)
//                     .IsRequired();
//                 e.Property(s => s.Name)
//                     .HasMaxLength(100)
//                     .IsRequired();
//                 e.HasOne(s => s.Class)
//                     .WithMany(c => c.Subjects)
//                     .HasForeignKey(s => s.ClassId)
//                     .OnDelete(DeleteBehavior.Restrict);
//                 e.HasIndex(s => new { s.ClassId, s.Code })
//                     .IsUnique();
//             });

//             modelBuilder.Entity<PaymentTransaction>(e =>
//             {
//                 e.HasKey(x => x.Id);
//                 e.Property(x => x.Amount).HasPrecision(10, 2);
//                 e.Property(x => x.Currency).HasMaxLength(3).IsRequired();
//                 e.Property(x => x.Method).HasMaxLength(20).IsRequired();
//                 e.Property(x => x.UserId).HasMaxLength(100).IsRequired();
//                 e.Property(x => x.RazorpayOrderId).HasMaxLength(100).IsRequired();
//                 e.Property(x => x.RazorpayPaymentId).HasMaxLength(100);
//                 e.Property(x => x.Status).HasMaxLength(20).IsRequired();
//                 e.Property(x => x.FailureReason).HasMaxLength(250);
//                 e.HasIndex(x => x.RazorpayOrderId).IsUnique();

//                 e.HasIndex(x => x.RazorpayPaymentId).IsUnique();
//                 e.HasIndex(x => new { x.UserId, x.CreatedAtUtc });
//                 e.HasIndex(x => new { x.FeeItemId, x.Status });

//                 e.HasOne<StudentFee>()
//                     .WithMany()
//                     .HasForeignKey(x => x.FeeItemId)
//                     .OnDelete(DeleteBehavior.Restrict);
//             });

//             modelBuilder.Entity<StudentFee>(e =>
//             {
//                 e.Property(x => x.Title).HasMaxLength(100).IsRequired();
//                 e.Property(x => x.TotalAmount).HasPrecision(10, 2);
//                 e.Property(x => x.PaidAmount).HasPrecision(10, 2);
//             });

//             modelBuilder.Entity<Course>(e =>
//             {
//                 e.HasKey(c => c.Id);
//                 e.Property(c => c.Code).HasMaxLength(20).IsRequired();
//                 e.Property(c => c.Name).HasMaxLength(100).IsRequired();
//                 e.HasIndex(c => c.Code).IsUnique();
//                 e.Property(c => c.DurationYears).IsRequired();
//             });
//                 }
//     }
// }




using Microsoft.EntityFrameworkCore;
using CrudOperation.Models;

namespace CrudOperation.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
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

        public DbSet<Course> Courses { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Subject> Subjects { get; set; }

        public DbSet<StudentEnrollment> StudentEnrollments { get; set; }
        public DbSet<TeacherAssignment> TeacherAssignments { get; set; }

        public DbSet<StudentFee> StudentFees => Set<StudentFee>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User - Student relationship
            modelBuilder.Entity<User>()
                .HasOne(u => u.Student)
                .WithOne(s => s.User)
                .HasForeignKey<Student>(s => s.UserId);

            // User - Teacher relationship
            modelBuilder.Entity<User>()
                .HasOne(u => u.Teacher)
                .WithOne(t => t.User)
                .HasForeignKey<Teacher>(t => t.UserId);

            // Student - Documents relationship
            modelBuilder.Entity<Student>()
                .HasMany(s => s.Documents)
                .WithOne(d => d.student)
                .HasForeignKey(d => d.StudentId);

            // User - Uploaded Documents relationship
            modelBuilder.Entity<User>()
                .HasMany(u => u.UploadDocuments)
                .WithOne(d => d.userUploaded)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Course configuration
            modelBuilder.Entity<Course>(e =>
            {
                e.HasKey(c => c.Id);

                e.Property(c => c.Code)
                    .HasMaxLength(20)
                    .IsRequired();

                e.Property(c => c.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                e.Property(c => c.DurationYears)
                    .IsRequired();

                e.HasIndex(c => c.Code)
                    .IsUnique();
            });

            // Course - Class relationship and Class configuration
            modelBuilder.Entity<Class>(e =>
            {
                e.HasKey(c => c.Id);

                e.Property(c => c.Section)
                    .HasMaxLength(10)
                    .IsRequired();

                e.Property(c => c.AcademicYear)
                    .HasMaxLength(20)
                    .IsRequired();

                e.Property(c => c.Semester)
                    .IsRequired();

                e.HasOne(c => c.Course)
                    .WithMany(course => course.Classes)
                    .HasForeignKey(c => c.CourseId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(c => new
                {
                    c.CourseId,
                    c.Semester,
                    c.Section,
                    c.AcademicYear
                }).IsUnique();
            });

            // Class - Subject relationship and Subject configuration
            modelBuilder.Entity<Subject>(e =>
            {
                e.HasKey(s => s.Id);

                e.Property(s => s.Code)
                    .HasMaxLength(20)
                    .IsRequired();

                e.Property(s => s.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                e.HasOne(s => s.Class)
                    .WithMany(c => c.Subjects)
                    .HasForeignKey(s => s.ClassId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(s => new { s.ClassId, s.Code })
                    .IsUnique();
            });

            // Student Enrollment relationships
            modelBuilder.Entity<StudentEnrollment>(e =>
            {
                e.HasOne(x => x.Student)
                    .WithMany()
                    .HasForeignKey(x => x.StudentId);

                e.HasOne(x => x.Class)
                    .WithMany(c => c.StudentEnrollments)
                    .HasForeignKey(x => x.ClassId);

                e.HasIndex(x => new { x.StudentId, x.ClassId })
                    .IsUnique();
            });

            // Teacher Assignment relationships
            modelBuilder.Entity<TeacherAssignment>(e =>
            {
                e.HasOne(x => x.Teacher)
                    .WithMany()
                    .HasForeignKey(x => x.TeacherId);

                e.HasOne(x => x.Subject)
                    .WithMany(s => s.TeacherAssignments)
                    .HasForeignKey(x => x.SubjectId);

                e.HasIndex(x => new { x.TeacherId, x.SubjectId })
                    .IsUnique();
            });

            // Payment Transaction configuration
            modelBuilder.Entity<PaymentTransaction>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.Amount)
                    .HasPrecision(10, 2);

                e.Property(x => x.Currency)
                    .HasMaxLength(3)
                    .IsRequired();

                e.Property(x => x.Method)
                    .HasMaxLength(20)
                    .IsRequired();

                e.Property(x => x.UserId)
                    .HasMaxLength(100)
                    .IsRequired();

                e.Property(x => x.RazorpayOrderId)
                    .HasMaxLength(100)
                    .IsRequired();

                e.Property(x => x.RazorpayPaymentId)
                    .HasMaxLength(100);

                e.Property(x => x.Status)
                    .HasMaxLength(20)
                    .IsRequired();

                e.Property(x => x.FailureReason)
                    .HasMaxLength(250);

                e.HasIndex(x => x.RazorpayOrderId)
                    .IsUnique();

                e.HasIndex(x => x.RazorpayPaymentId)
                    .IsUnique();

                e.HasIndex(x => new { x.UserId, x.CreatedAtUtc });

                e.HasIndex(x => new { x.FeeItemId, x.Status });

                e.HasOne<StudentFee>()
                    .WithMany()
                    .HasForeignKey(x => x.FeeItemId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Student Fee configuration
            modelBuilder.Entity<StudentFee>(e =>
            {
                e.Property(x => x.Title)
                    .HasMaxLength(100)
                    .IsRequired();

                e.Property(x => x.TotalAmount)
                    .HasPrecision(10, 2);

                e.Property(x => x.PaidAmount)
                    .HasPrecision(10, 2);
            });
        }
    }
}
