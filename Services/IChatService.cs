using CrudOperation.DTOs;

namespace CrudOperation.Service
{
    public interface IChatService
    {
        Task<ChatResponseDto> GetReplyAsync(string message,string userId,string role, string? requestedConversationId = null);
        
    }
}