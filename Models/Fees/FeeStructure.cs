namespace CrudOperation.Models.Fees;

public class FeeStructure
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
    public string Course { get; set; } = string.Empty;
    public int Version { get; set; } = 1;

    public FeeStructureStatus Status { get; set; }
        = FeeStructureStatus.Draft;

    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }

    public int CreatedByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<FeeStructureItem> Items { get; set; }
        = new List<FeeStructureItem>();

    public ICollection<StudentFeeAssignment> StudentAssignments { get; set; }
        = new List<StudentFeeAssignment>();
}