using Govor.Application.Interfaces;
using Govor.Core.Models.Messages;
using Govor.Core.Repositories.Groups;
using Govor.Core.Repositories.PrivateChats;
using Govor.Data;
using Govor.Data.Repositories.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Govor.Application.Services.Messages;

public class MessagesLoader : IMessagesLoader
{
    private IGroupsRepository _groupsRepository;
    private IPrivateChatsRepository _privateChatsRepository;
    private GovorDbContext _dbContext;
    
    public MessagesLoader(
        IGroupsRepository groupsRepository,
        IPrivateChatsRepository privateChatsRepository,
        GovorDbContext dbContext)
    {
        _groupsRepository = groupsRepository;
        _privateChatsRepository = privateChatsRepository;
        _dbContext = dbContext;
    }
    
    public async Task<List<Message>> LoadMessagesInUserChat(
        Guid userId,
        Guid currentUser,
        Guid? startMessageId,
        int before = 20,
        int after = 2)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User id cannot be empty");

        if (!_privateChatsRepository.Exist(userId, currentUser))
            throw new InvalidOperationException("Private chat not found");

        var chat = await _privateChatsRepository.GetByMembersAsync(userId, currentUser);

        var query = _dbContext.Messages
            .AsNoTracking()
            .Include(m => m.MediaAttachments)
            .ThenInclude(m => m.MediaFile)
            .Where(m => m.RecipientType == RecipientType.User &&
                        m.RecipientId == chat.Id);

        if (startMessageId is null)
        {
            return await query
                .OrderByDescending(m => m.SentAt)
                .Take(before)
                .ToListAsync();
        }

        var startMessage = await _dbContext.Messages.FindAsync(startMessageId.Value);
        if (startMessage == null)
            throw new NotFoundException("Start message not found");

        var beforeMessages = await query
            .Where(m => m.SentAt < startMessage.SentAt)
            .OrderByDescending(m => m.SentAt)
            .Take(before)
            .ToListAsync();

        var afterMessages = await query
            .Where(m => m.SentAt > startMessage.SentAt)
            .OrderBy(m => m.SentAt)
            .Take(after)
            .ToListAsync();

        // older -> start -> newer
        var result = beforeMessages
            .OrderBy(m => m.SentAt)
            .Concat(new[] { startMessage })
            .Concat(afterMessages)
            .ToList();

        return result;
    }


    public async Task<List<Message>> LoadMessagesInChatGroup(
        Guid chatId,
        Guid currentUser,
        Guid? startMessageId,
        int before = 20,
        int after = 2)
    {
        if (chatId == Guid.Empty)
            throw new ArgumentException("Chat id cannot be empty");

        var isMember = await _groupsRepository.IsUserMemberOfGroupAsync(currentUser, chatId);
        if (!isMember)
            throw new UnauthorizedAccessException("You are not a member of this group.");

        var baseQuery = _dbContext.Messages
            .AsNoTracking()
            .Include(m => m.MediaAttachments)
            .ThenInclude(m => m.MediaFile)
            .AsSplitQuery()
            .Where(m => m.RecipientType == RecipientType.Group && m.RecipientId == chatId);

        if (startMessageId is null)
        {
            return await baseQuery
                .OrderByDescending(m => m.SentAt)
                .Take(before)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        var startMessage = await _dbContext.Messages
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == startMessageId.Value && 
                                      m.RecipientType == RecipientType.Group && 
                                      m.RecipientId == chatId);

        if (startMessage == null)
            throw new NotFoundException("Start message not found in this group.");

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

        var result = beforeMessages
            .OrderBy(m => m.SentAt)
            .Concat(new[] { startMessage })
            .Concat(afterMessages)
            .ToList();

        return result;
    }

}