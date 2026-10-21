namespace CrudOperation.Models.Fees;

public class StudentFeeItem
{
    public int Id { get; set; }
    public int AssignmentId { get; set; }

    public int? FeeCategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;

    public decimal OriginalAmount { get; set; }
    public string? Description { get; set; }

    public StudentFeeAssignment Assignment { get; set; } = null!;
}