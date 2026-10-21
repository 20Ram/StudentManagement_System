namespace CrudOperation.Models.Fees;

public class FeeStructureItem
{
    public int Id { get; set; }

    public int FeeStructureId { get; set; }
    public int FeeCategoryId { get; set; }

    public decimal Amount { get; set; }
    public bool IsMandatory { get; set; } = true;
    public string? Description { get; set; }

    public FeeStructure FeeStructure { get; set; } = null!;
    public FeeCategory FeeCategory { get; set; } = null!;
}