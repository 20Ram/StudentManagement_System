using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrudOperation.Models
{
    public class StudentFeeItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StudentFeeAssignmentId { get; set; }

        [ForeignKey(nameof(StudentFeeAssignmentId))]
        // public StudentFeeAssignment StudentFeeAssignment { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PaidAmount { get; set; } = 0;

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";
    }
}