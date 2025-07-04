using Govor.Application.Interfaces;
// using Govor.Application.Interfaces.Messages; // No longer needed
// using Govor.Application.Interfaces.Messages.Parameters; // No longer needed
// using Govor.Core.Models; // No longer needed unless other methods use Core models
// using Govor.Core.Repositories.Messages; // No longer needed
using Microsoft.Extensions.Logging;

namespace Govor.Application.Services;

// This service might handle creation/deletion of private chat sessions if that's
// more than just sending a message to a user (e.g., if private chats are explicit entities).
// For now, it primarily relies on IVerifyFriendship.
// If friendship verification is the only role, its methods could be absorbed elsewhere,
// or this service could be enhanced with other private chat management features.
public class PrivateChatService // No longer implements IChatService
{
    private readonly IVerifyFriendship _verifyFriendship;
    private readonly ILogger<PrivateChatService> _logger;

    public PrivateChatService(IVerifyFriendship verifyFriendship, ILogger<PrivateChatService> logger)
    {
        _verifyFriendship = verifyFriendship;
        _logger = logger;
    }

    // Example of a method that might remain or be added:
    // public async Task<bool> CanUserChatWithAsync(Guid userId1, Guid userId2)
    // {
    //     try
    //     {
    //         await _verifyFriendship.VerifyAsync(userId1, userId2);
    //         return true;
    //     }
    //     catch (FriendshipException) // Or whatever exception VerifyAsync throws
    //     {
    //         return false;
    //     }
    // }

    // All message sending, editing, deleting methods are removed as they are now in MessageService.
    // If this service has no other responsibilities, it could be a candidate for removal,
    // and IVerifyFriendship could be injected directly where needed (e.g. MessageService).
    // However, keeping it allows for future expansion of private chat-specific logic.
    public void PlaceholderMethod()
    {
        _logger.LogInformation("PrivateChatService is currently a placeholder after message management refactoring.");
    }
}