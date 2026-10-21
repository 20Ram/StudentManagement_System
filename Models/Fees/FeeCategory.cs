namespace CrudOperation.Models.Fees;

public class FeeCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsRecurring { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<FeeStructureItem> FeeStructureItems { get; set; }
        = new List<FeeStructureItem>();
}
