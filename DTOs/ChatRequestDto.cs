using System.ComponentModel.DataAnnotations;

namespace CrudOperation.DTOs
{
    public class ChatRequestDto
    {
        [Required]
        [StringLength(2000, MinimumLength = 1)]
        public string Message { get; set; } = string.Empty;

        public string? ConversationId { get; set; }
    }
}