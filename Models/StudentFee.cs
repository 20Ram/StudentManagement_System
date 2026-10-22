namespace CrudOperation.Models
{
    public class StudentFee
    {
        public int Id { get; set; }
        public int StudentId { get; set; }   
        public Student Student { get; set; } = null!; 
        public string Title { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public DateTime DueDate { get; set; }
    }
}