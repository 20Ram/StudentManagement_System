namespace CrudOperation.Models.Fees;

public class FeePolicy
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public decimal FixedAmount { get; set; }
    public int GracePeriodDays { get; set; }

    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }

    public bool IsActive { get; set; } = true;

    public int CreatedByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<FeeLateFeeAssessment> Assessments { get; set; }
        = new List<FeeLateFeeAssessment>();
}