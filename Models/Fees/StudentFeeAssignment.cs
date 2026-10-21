namespace CrudOperation.Models.Fees;

public class StudentFeeAssignment
{
    public int Id { get; set; }

    public int StudentId { get; set; }
    public int FeeStructureId { get; set; }

    public string AcademicYear { get; set; } = string.Empty;
    public decimal OriginalAmount { get; set; }

    public DateTime AssignedAtUtc { get; set; } = DateTime.UtcNow;
    public int AssignedByUserId { get; set; }

    public FeeAssignmentStatus Status { get; set; }
        = FeeAssignmentStatus.Active;

    public Student Student { get; set; } = null!;
    public FeeStructure FeeStructure { get; set; } = null!;

    public ICollection<StudentFeeItem> Items { get; set; }
        = new List<StudentFeeItem>();

    public ICollection<FeeInstallment> Installments { get; set; }
        = new List<FeeInstallment>();

    public ICollection<FeePayment> Payments { get; set; }
        = new List<FeePayment>();

    public ICollection<FeeAdjustment> Adjustments { get; set; }
        = new List<FeeAdjustment>();

    public ICollection<FeeNotification> Notifications { get; set; }
        = new List<FeeNotification>();
}