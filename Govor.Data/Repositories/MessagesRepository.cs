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
    
    private IQueryable<Message> GetBaseQuery()
    {
        return _context.Messages.AsNoTracking();
    }

    public async Task<List<Message>> GetAllAsync()
    {
        return await GetBaseQuery()
            .Include(m => m.ReplyToMessage)
            .AsSplitQuery()
            .ToListOrThrowIfEmpty(new NotFoundException("No messages found in the database"));
    }

    public async Task<Message> FindByIdAsync(Guid messageId)
    {
        return await GetBaseQuery()
                   .Include(m => m.ReplyToMessage)
                   .Include(m => m.MediaAttachments)
                   .AsSplitQuery()
                   .FirstOrDefaultAsync(m => m.Id == messageId)
               ?? throw new NotFoundByKeyException<Guid>(messageId, "Message with given id does not exist.");
    }

    public async Task<List<Message>> FindBySenderIdAsync(Guid senderId)
    {
        return await GetBaseQuery()
            .Include(m => m.ReplyToMessage)
            .AsSplitQuery()
            .Where(m => m.SenderId == senderId)
            .ToListOrThrowIfEmpty(new NotFoundByKeyException<Guid>(senderId, "Messages with given sender id do not exist"));
    }

    public async Task<List<Message>> FindByReceiverIdAsync(Guid receiverId)
    {
        return await GetBaseQuery()
            .Include(m => m.ReplyToMessage)
            .AsSplitQuery()
            .Where(m => m.RecipientId == receiverId)
            .ToListOrThrowIfEmpty(new NotFoundByKeyException<Guid>(receiverId, "Messages with given recipient id do not exist"));
    }

    public async Task<List<Message>> FindBySenderAndReceiverIdAsync(Guid senderId, Guid receiverId, RecipientType recipientType = RecipientType.User)
    {
        return await GetBaseQuery()
            .Include(m => m.ReplyToMessage)
            .AsSplitQuery()
            .Where(m => m.SenderId == senderId 
                        && m.RecipientId == receiverId 
                        && m.RecipientType == recipientType)
            .ToListOrThrowIfEmpty(new NotFoundException("No messages found in the database"));
    }
    
    public async Task<List<Message>> FindBySentAtAsync(DateTime date)
    {
        return await GetBaseQuery()
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
                    .SetProperty(m => m.EncryptedContent, message.EncryptedContent)
                    .SetProperty(m => m.EditedAt, message.EditedAt)
                    .SetProperty(m => m.IsEdited, message.IsEdited)
                );

            if (rowsAffected == 0)
                throw new UpdateException($"Message with ID {message.Id} not found or no changes detected.");
        }
        catch (InvalidObjectException<Message> ex)
        {
            throw new UpdateException($"Invalid message data for ID {message.Id}", ex);
        }
        catch (Exception ex)
        {
            throw new UpdateException($"Error when updating message {message.Id}", ex);
        }
    }

    public async Task RemoveAsync(Guid messageId) // Reverted to Hard Delete
    {
        try
        {
            var message = await _context.Messages.FindAsync(messageId); // Find first, to ensure it exists
            if (message == null)
                throw new NotFoundByKeyException<Guid>(messageId, "Message not found.");

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
        }
        catch (NotFoundByKeyException<Guid> ex)
        {
            throw new RemoveException($"Message with ID {messageId} not found.", ex);
        }
        catch (Exception ex) // Catches DbUpdateException if there are FK constraints, etc.
        {
            throw new RemoveException($"Error when deleting message {messageId}", ex);
        }
    }

    public bool Exist(Message message)
    {
        _validator.Validate(message);
        
        return GetBaseQuery().Any(
            m => m.Id == message.Id &&
            m.SenderId == message.SenderId && 
            m.RecipientId == message.RecipientId &&
            m.EncryptedContent == message.EncryptedContent &&
            m.SentAt == message.SentAt && 
            m.ReplyToMessageId == message.ReplyToMessageId
        );
    }

    public bool Exist(Guid messageId)
    {
        return GetBaseQuery().Any(m => m.Id == messageId);
    }
}
