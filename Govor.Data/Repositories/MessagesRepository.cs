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
            .ToListOrThrowIfEmpty(new NotFoundException("No messages found in the database"));
    }

    public async Task<Message> FindByIdAsync(Guid messageId)
    {
        return await _context.Messages
                   .AsNoTracking()
                   .Include(m => m.ReplyToMessage)
                   .FirstOrDefaultAsync(m => m.Id == messageId)
               ?? throw new NotFoundByKeyException<Guid>(messageId, "Message with given id does not exist");
    }

    public Task<List<Message>> FindBySenderIdAsync(Guid senderId)
    {
        throw new NotImplementedException();
    }

    public Task<List<Message>> FindByReceiverIdAsync(Guid receiverId)
    {
        throw new NotImplementedException();
    }

    public Task<List<Message>> FindBySenderAndReceiverIdAsync(Guid senderId, Guid receiverId, RecipientType recipientType = RecipientType.User)
    {
        throw new NotImplementedException();
    }

    public Task<List<Message>> FindBySentAtAsync(DateTime date)
    {
        throw new NotImplementedException();
    }

    public void Add(Message message)
    {
        throw new NotImplementedException();
    }

    public void Update(Message message)
    {
        throw new NotImplementedException();
    }

    public void Delete(Guid messageId)
    {
        throw new NotImplementedException();
    }

    public bool Exist(Message message)
    {
        throw new NotImplementedException();
    }
}