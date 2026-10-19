
namespace CrudOperation.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }

        public string ConversationId { get; set; } =
            string.Empty;

        public int UserId { get; set; }
        public string Role { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } =
            DateTime.UtcNow;
    }
}
