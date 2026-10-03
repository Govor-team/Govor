using Govor.Application.Interfaces;
using Govor.Domain.Models.Messages;
using Govor.Domain;
using Govor.Domain.Common;
using Microsoft.EntityFrameworkCore;
using SmartRes;

namespace Govor.Application.Messages;

public class MessagesLoader : IMessagesLoader
{
    private readonly GovorDbContext _dbContext;
    
    public MessagesLoader(GovorDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<Result<List<Message>,Error>> LoadMessagesInUserChat(
        Guid privateChatId,
        Guid currentUser,
        Guid? startMessageId,
        int before = 20,
        int after = 2)
    {
        if (privateChatId == Guid.Empty)
            return Result.Failure<List<Message>>(Error.Failure(nameof(ArgumentException),"PrivateChatId id cannot be empty."));
        
        if (!await _dbContext.HasChatAccessAsync(currentUser, privateChatId, RecipientType.User))
            return Result.Failure<List<Message>>(Error.Forbidden("Chat.AccessDenied", "You are not a member of this chat."));
        
        var query = _dbContext.Messages
            .AsNoTracking()
            .Include(m => m.MessageViews)
            .Include(m => m.Reactions)
            .Include(m => m.MediaAttachments)
                .ThenInclude(m => m.MediaFile)
            .Where(m => m.RecipientType == RecipientType.User && m.RecipientId == privateChatId);

        return await FetchPaginatedMessagesAsync(query, startMessageId, before, after);
    }

    public async Task<Result<List<Message>,Error>> LoadMessagesInChatGroup(
        Guid chatId,
        Guid currentUser,
        Guid? startMessageId,
        int before = 20,
        int after = 2)
    {
        if (chatId == Guid.Empty)
            return Result.Failure<List<Message>>(Error.Failure(nameof(ArgumentException),"Chat id cannot be empty."));
        
        var isMember = await _dbContext.GroupMemberships
            .AnyAsync(gm => gm.UserId == currentUser && gm.GroupId == chatId && !gm.IsBanned);
            
        if (!isMember)
            return Result.Failure<List<Message>>(Error.Forbidden("Chat.AccessDenied", "You are not a member of this chat."));
        
        var query = _dbContext.Messages
            .AsNoTracking()
            .Include(m => m.MessageViews)
            .Include(m => m.Reactions)
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
                .ThenByDescending(m => m.Id)
                .Take(before)
                .OrderBy(m => m.SentAt)
                .ThenBy(m => m.Id)
                .ToListAsync();
        }
        
        var startMessage = await baseQuery.FirstOrDefaultAsync(m => m.Id == startMessageId.Value);
        if (startMessage == null) 
            return [];
        
        var beforeMessages = await baseQuery
            .Where(m => m.SentAt < startMessage.SentAt ||
                (m.SentAt == startMessage.SentAt && m.Id.CompareTo(startMessage.Id) < 0))
            .OrderByDescending(m => m.SentAt)
            .ThenByDescending(m => m.Id)
            .Take(before)
            .ToListAsync();
        
        var afterMessages = await baseQuery
            .Where(m => m.SentAt > startMessage.SentAt ||
                (m.SentAt == startMessage.SentAt && m.Id.CompareTo(startMessage.Id) > 0))
            .OrderBy(m => m.SentAt)
            .ThenBy(m => m.Id)
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
