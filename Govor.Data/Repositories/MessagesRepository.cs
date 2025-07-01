using Govor.Core.Infrastructure.Extensions;
using Govor.Core.Infrastructure.Validators;
using Govor.Core.Models;
using Govor.Core.Repositories.Messages;
using Govor.Data.Repositories.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Govor.Data.Repositories;

public class MessagesRepository : IMessagesRepository
{
    private GovorDbContext _context;
    private IObjectValidator<Message> _validator;
    public MessagesRepository(GovorDbContext context, IObjectValidator<Message> validator)
    {
        _context = context;
        _validator = validator;
    }
    
    public async Task<List<Message>> GetAllAsync()
    {
        return await _context.Messages
            .AsNoTracking()
            .Include(m => m.ReplyToMessage)
            .AsSplitQuery()
            .ToListOrThrowIfEmpty(new NotFoundException("No messages found in the database"));
    }

    public async Task<Message> FindByIdAsync(Guid messageId)
    {
        return await _context.Messages
                   .AsNoTracking()
                   .Include(m => m.ReplyToMessage)
                   .AsSplitQuery()
                   .FirstOrDefaultAsync(m => m.Id == messageId)
               ?? throw new NotFoundByKeyException<Guid>(messageId, "Message with given id does not exist");
    }

    public async Task<List<Message>> FindBySenderIdAsync(Guid senderId)
    {
        return await _context.Messages
            .AsNoTracking()
            .Include(m => m.ReplyToMessage)
            .AsSplitQuery()
            .Where(m => m.SenderId == senderId)
            .ToListOrThrowIfEmpty(new NotFoundByKeyException<Guid>(senderId, "Messages with given sender id do not exist"));
    }

    public async Task<List<Message>> FindByReceiverIdAsync(Guid receiverId)
    {
        return await _context.Messages
            .AsNoTracking()
            .Include(m => m.ReplyToMessage)
            .AsSplitQuery()
            .Where(m => m.RecipientId == receiverId)
            .ToListOrThrowIfEmpty(new NotFoundByKeyException<Guid>(receiverId, "Messages with given recipient id do not exist"));
    }

    public async Task<List<Message>> FindBySenderAndReceiverIdAsync(Guid senderId, Guid receiverId, RecipientType recipientType = RecipientType.User)
    {
        return await _context.Messages
            .AsNoTracking()
            .Include(m => m.ReplyToMessage)
            .AsSplitQuery()
            .Where(m => m.SenderId == senderId 
                        && m.RecipientId == receiverId 
                        && m.RecipientType == recipientType)
            .ToListOrThrowIfEmpty(new NotFoundException("No messages found in the database"));
    }
    
    public async Task<List<Message>> FindBySentAtAsync(DateTime date)
    {
        return await _context.Messages
            .AsNoTracking()
            .Include(m => m.ReplyToMessage)
            .AsSplitQuery()
            .Where(m => m.SentAt == date)
            .ToListOrThrowIfEmpty(new NotFoundByKeyException<DateTime>(date, "Messages sent at date do not exist"));
    }

    public async Task AddAsync(Message message)
    {
        try
        {
            _validator.Validate(message);

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
        }
        catch (InvalidObjectException<Message> ex)
        {
            throw new AdditionException("Message with given data invalid", ex);
        }
        catch (Exception ex)
        {
            throw new AdditionException("Cannot add message", ex);
        }
    }
    
    // TODO: Test this block
    public async Task UpdateAsync(Message message)
    {
        try
        {
            _validator.Validate(message);

            var rowsAffected = await _context.Messages
                .Where(m => m.Id == message.Id)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(m => m.SenderId, message.SenderId)
                    .SetProperty(m => m.RecipientId, message.RecipientId)
                    .SetProperty(m => m.SentAt, message.SentAt)
                    .SetProperty(m => m.RecipientType, message.RecipientType)
                    .SetProperty(m => m.EncryptedContent, message.EncryptedContent)
                    .SetProperty(m => m.EditedAt, message.EditedAt)
                    .SetProperty(m => m.IsEdited, message.IsEdited)
                    .SetProperty(m => m.Reactions, message.Reactions)
                    .SetProperty(m => m.ReplyToMessageId, message.ReplyToMessageId)
                    .SetProperty(m => m.MessageViews, message.MessageViews)
                    .SetProperty(m => m.MediaAttachments, message.MediaAttachments)
                   
                );

            if (rowsAffected == 0)
                throw new UpdateException($"Not found message by given id {message.Id}");
        }
        catch (Exception ex)
        {
            throw new UpdateException($"Error when updating the message {message.Id}", ex);
        }
    }

    public async Task RemoveAsync(Guid messageId)
    {
        try
        {
            var result = await FindByIdAsync(messageId);

            _context.Messages.Remove(result);
            await _context.SaveChangesAsync();
        }
        catch (NotFoundByKeyException<Guid> ex)
        {
            throw new RemoveException($"Not found message by given id {messageId}", ex);
        }
        catch (Exception ex)
        {
            throw new RemoveException("Error when removing the message", ex);
        }
    }

    public bool Exist(Message message)
    {
        _validator.Validate(message);
        
        return _context.Messages.Any(
            m => m.Id == message.Id &&
            m.SenderId == message.SenderId && 
            m.RecipientId == message.RecipientId &&
            m.EncryptedContent == message.EncryptedContent &&
            m.EditedAt == message.EditedAt &&
            m.IsEdited == message.IsEdited &&
            m.SentAt == message.SentAt && 
            m.ReplyToMessageId == message.ReplyToMessageId
        );
    }

    public bool Exist(Guid messageId)
    {
        return _context.Messages.Any(m => m.Id == messageId);
    }
}
