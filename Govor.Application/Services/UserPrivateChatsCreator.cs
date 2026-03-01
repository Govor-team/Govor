using Govor.Application.Interfaces;
using Govor.Core.Models;
using Govor.Core.Repositories.PrivateChats;
using Microsoft.Extensions.Logging;

namespace Govor.Application.Services;

public class UserPrivateChatsCreator : IUserPrivateChatsCreator
{
    private readonly IPrivateChatsRepository _privateChats;
    private readonly IPrivateChatGroupManager _privateChatGroupManager;
    private readonly ILogger<UserPrivateChatsCreator> _logger;
    
    public UserPrivateChatsCreator(
        IPrivateChatsRepository privateChats,
        IPrivateChatGroupManager privateChatGroupManager,
        ILogger<UserPrivateChatsCreator> logger)
    {
        _privateChats = privateChats;
        _privateChatGroupManager = privateChatGroupManager;
        _logger = logger;
    }

    public async Task<PrivateChat> CreateAsync(Guid userIdA, Guid userIdB)
    {
        if (!_privateChats.Exist(userIdA, userIdB))
        {
            var recipientId = Guid.NewGuid();
            var privateChat = new PrivateChat()
            {
                Id = recipientId,
                UserAId = userIdA,
                UserBId = userIdB
            };
            
            await _privateChats.AddAsync(privateChat);
            await _privateChatGroupManager.AddUsersToPrivateChatGroupAsync(privateChat); // Notifying 
            
            _logger.LogInformation($"User for {userIdA} and {userIdB} was created private chat with Id: {recipientId}");
            return privateChat;
        }
        
        _logger.LogInformation($"Private chat already exists for {userIdA} and {userIdB}; Returning already created chat...");
        return await _privateChats.GetByMembersAsync(userIdA, userIdB);
    }
}