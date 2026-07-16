using Govor.Domain;
using Govor.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Govor.Application.PrivateUserChats;

public class UserPrivateChatsCreator : IUserPrivateChatsCreator
{
    private readonly GovorDbContext _context;
    private readonly IPrivateChatGroupManager _privateChatGroupManager;
    private readonly ILogger<UserPrivateChatsCreator> _logger;
    
    public UserPrivateChatsCreator(
        GovorDbContext context,
        IPrivateChatGroupManager privateChatGroupManager,
        ILogger<UserPrivateChatsCreator> logger)
    {
        _context = context;
        _privateChatGroupManager = privateChatGroupManager;
        _logger = logger;
    }

    public async Task<PrivateChat> CreateAsync(Guid userIdA, Guid userIdB)
    {
        var existingChat = await _context.PrivateChats
            .FirstOrDefaultAsync(c => (c.UserAId == userIdA && c.UserBId == userIdB) || 
                                      (c.UserAId == userIdB && c.UserBId == userIdA));

        if (existingChat is null)
        {
            var recipientId = Guid.NewGuid();
            var privateChat = new PrivateChat
            {
                Id = recipientId,
                UserAId = userIdA,
                UserBId = userIdB
            };
            
            await _context.PrivateChats.AddAsync(privateChat);
            await _context.SaveChangesAsync(); 
            
            await _privateChatGroupManager.AddUsersToPrivateChatGroupAsync(privateChat); 
            
            _logger.LogInformation($"Private chat created with Id: {recipientId} for users {userIdA} and {userIdB}");
            return privateChat;
        }
        
        _logger.LogInformation($"Private chat already exists for {userIdA} and {userIdB}; Returning existing chat...");
        return existingChat;
    }
}