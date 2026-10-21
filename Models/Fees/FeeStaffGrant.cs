namespace CrudOperation.Models.Fees;

public class FeeStaffGrant
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public int GrantedByUserId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime GrantedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAtUtc { get; set; }

    public string? Reason { get; set; }
}