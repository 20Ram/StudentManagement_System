namespace CrudOperation.DTOs
{
    public class StudentResponseDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; } 

        public string Roll { get; set; }

        public bool IsActive { get; set; }

        public int UserId { get; set; }
    }
}