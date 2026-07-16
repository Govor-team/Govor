using Govor.Application.Interfaces;
using Govor.Domain.Models.Messages;
using Govor.Domain;
using Microsoft.EntityFrameworkCore;

namespace Govor.Application.Messages;

public class MessagesLoader : IMessagesLoader
{
    private readonly GovorDbContext _dbContext;
    
    public MessagesLoader(GovorDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<List<Message>> LoadMessagesInUserChat(
        Guid privateChatId,
        Guid currentUser,
        Guid? startMessageId,
        int before = 20,
        int after = 2)
    {
        if (privateChatId == Guid.Empty)
            throw new ArgumentException("PrivateChatId id cannot be empty", nameof(privateChatId));
        
        var chatExists = await _dbContext.PrivateChats.AnyAsync(c => c.Id == privateChatId);
        if (!chatExists) 
            return [];
        
        var query = _dbContext.Messages
            .AsNoTracking()
            .Include(m => m.MediaAttachments)
                .ThenInclude(m => m.MediaFile)
            .Where(m => m.RecipientType == RecipientType.User && m.RecipientId == privateChatId);

        return await FetchPaginatedMessagesAsync(query, startMessageId, before, after);
    }

    public async Task<List<Message>> LoadMessagesInChatGroup(
        Guid chatId,
        Guid currentUser,
        Guid? startMessageId,
        int before = 20,
        int after = 2)
    {
        if (chatId == Guid.Empty)
            throw new ArgumentException("Chat id cannot be empty", nameof(chatId));
        
        var isMember = await _dbContext.GroupMemberships
            .AnyAsync(gm => gm.UserId == currentUser && gm.GroupId == chatId);
            
        if (!isMember) 
            return [];
        
        var query = _dbContext.Messages
            .AsNoTracking()
            .Include(m => m.MediaAttachments)
                .ThenInclude(m => m.MediaFile)
            .AsSplitQuery()
            .Where(m => m.RecipientType == RecipientType.Group && m.RecipientId == chatId);

        return await FetchPaginatedMessagesAsync(query, startMessageId, before, after);
    }
    
    private static async Task<List<Message>> FetchPaginatedMessagesAsync(
        IQueryable<Message> baseQuery, 
        Guid? startMessageId, 
        int before, 
        int after)
    {
        if (startMessageId is null)
        {
            return await baseQuery
                .OrderByDescending(m => m.SentAt)
                .Take(before)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }
        
        var startMessage = await baseQuery.FirstOrDefaultAsync(m => m.Id == startMessageId.Value);
        if (startMessage == null) 
            return [];
        
        var beforeMessages = await baseQuery
            .Where(m => m.SentAt < startMessage.SentAt)
            .OrderByDescending(m => m.SentAt)
            .Take(before)
            .ToListAsync();
        
        var afterMessages = await baseQuery
            .Where(m => m.SentAt > startMessage.SentAt)
            .OrderBy(m => m.SentAt)
            .Take(after)
            .ToListAsync();


        beforeMessages.Reverse();

        var result = new List<Message>(beforeMessages.Count + 1 + afterMessages.Count);
        result.AddRange(beforeMessages);
        result.Add(startMessage);
        result.AddRange(afterMessages);

        return result;
    }
}
