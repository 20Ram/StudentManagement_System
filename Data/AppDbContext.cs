using Microsoft.EntityFrameworkCore;
using CrudOperation.Models;
using CrudOperation.Models.Fees;

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
        public DbSet<FeeCategory> FeeCategories => Set<FeeCategory>();
        public DbSet<FeeStructure> FeeStructures => Set<FeeStructure>();
        public DbSet<FeeStructureItem> FeeStructureItems => Set<FeeStructureItem>();
        public DbSet<StudentFeeAssignment> StudentFeeAssignments => Set<StudentFeeAssignment>();
        public DbSet<StudentFeeItem> StudentFeeItems => Set<StudentFeeItem>();
        public DbSet<FeeInstallment> FeeInstallments => Set<FeeInstallment>();
        public DbSet<FeePayment> FeePayments => Set<FeePayment>();
        public DbSet<FeePaymentAllocation> FeePaymentAllocations => Set<FeePaymentAllocation>();
        public DbSet<FeeAdjustment> FeeAdjustments => Set<FeeAdjustment>();
        public DbSet<FeeAdjustmentAllocation> FeeAdjustmentAllocations 
            => Set<FeeAdjustmentAllocation>();
        public DbSet<FeePolicy> FeePolicies => Set<FeePolicy>();
        public DbSet<FeeLateFeeAssessment> FeeLateFeeAssessments => Set<FeeLateFeeAssessment>();
        public DbSet<FeeRefund> FeeRefunds => Set<FeeRefund>();
        public DbSet<FeeRefundReversal> FeeRefundReversals => Set<FeeRefundReversal>();
        public DbSet<FeeApproval> FeeApprovals => Set<FeeApproval>();
        public DbSet<FeeNotification> FeeNotifications => Set<FeeNotification>();
        public DbSet<FeeAuditLog> FeeAuditLogs => Set<FeeAuditLog>();
        public DbSet<PaymentGatewayEvent> PaymentGatewayEvents => Set<PaymentGatewayEvent>();
        public DbSet<FeeStaffGrant> FeeStaffGrants => Set<FeeStaffGrant>();
        public DbSet<FeeReceiptSequence> FeeReceiptSequences => Set<FeeReceiptSequence>();
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
        }
    }
}