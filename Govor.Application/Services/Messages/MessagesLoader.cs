using Govor.Application.Interfaces;
using Govor.Core.Infrastructure.Extensions;
using Govor.Core.Models.Messages;
using Govor.Core.Repositories.Groups;
using Govor.Core.Repositories.Messages;
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
    
    public async Task<List<Message>> LoadLastMessagesInUserChat(Guid userId, Guid currentUser, Guid? startMessageId, int pageSize = 20)
    {
        if(userId == Guid.Empty)
            throw new ArgumentException("User id cannot be empty");
        
        if(!_privateChatsRepository.Exist(userId, currentUser))
            throw new InvalidOperationException("Private chat not found");
        
        try
        {
            var chat  = await _privateChatsRepository.GetByMembersAsync(userId, currentUser);
            
            return await _dbContext.Messages
                .AsNoTracking()
                .Include(m => m.MediaAttachments)
                .ThenInclude(m => m.MediaFile)
                .AsSplitQuery()
                .Where(m => m.RecipientType == RecipientType.User &&
                           m.RecipientId == chat.Id)
                .Take(pageSize)
                .ToListOrThrowIfEmpty(new NotFoundException("Messages not found"));
        }
        catch (NotFoundException ex)
        {
            return new List<Message>();
        }
    }

    public async Task<List<Message>> LoadLastMessagesInChatGroup(Guid chatId, Guid currentUser, Guid? startMessageId, int pageSize = 20)
    {
        if(chatId == Guid.Empty)
            throw new ArgumentException("Chat id cannot be empty");
        
        if(!await _groupsRepository.IsUserMemberOfGroupAsync(currentUser, chatId))
            throw new UnauthorizedAccessException("You are not in a group.");
        
        try
        {
            return await _dbContext.Messages
                .AsNoTracking()
                .Include(m => m.MediaAttachments)
                .ThenInclude(m => m.MediaFile)
                .AsSplitQuery()
                .Where(m => m.RecipientType == RecipientType.Group &&
                            m.RecipientId == chatId)
                .Take(pageSize)
                .ToListOrThrowIfEmpty(new NotFoundException("Messages not found"));
        }
        catch (NotFoundException ex)
        {
            return new List<Message>();
        }
    }
}