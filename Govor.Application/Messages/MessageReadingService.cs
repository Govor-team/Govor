using Govor.Application.Infrastructure.Common;
using Govor.Domain;
using Govor.Domain.Common;
using Govor.Domain.Models.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartRes;

namespace Govor.Application.Messages;

public class MessageReadingService : IMessageReadingService
{
    private readonly GovorDbContext _dbContext;
    private readonly ILogger<MessageReadingService> _logger;
    private readonly INowDateTimeProvider _dateTimeProvider;


    public MessageReadingService(
        GovorDbContext dbContext,
        ILogger<MessageReadingService> logger, 
        INowDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<Message, Error>> ReadMessageAsync(Guid readerId, Guid messageId)
    {
        var message = await _dbContext.Messages
                .Include(msg => msg.MessageViews)
                .FirstOrDefaultAsync(msg => msg.Id == messageId);
        
        if(message is null)
            return Result<Message, Error>.Failure(Error.NotFound(
                "Message not found",
                "Message with id: " + messageId + " was not found.")
            );

        if (CanReadMessage(readerId, message))
        {
            var view = new MessageView()
            {
                Id = Guid.NewGuid(),
                MessageId = messageId,
                UserId = readerId,
                ViewedAt = _dateTimeProvider.Now,
            };
            
            _dbContext.MessageViews.Add(view);
            
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("Message with id: {Id} was read by user {user} at {time}", view.Id,  view.UserId, view.ViewedAt);
        }
        
        return message;
    }

    private bool CanReadMessage(Guid readerId, Message message)
    {
        if (message.RecipientType == RecipientType.User)
        {
            return _dbContext.PrivateChats.Any(pr => pr.UserAId == readerId || pr.UserBId == readerId) && 
                   message.MessageViews.All(mv => mv.UserId != readerId);
        }
        else
        {
            return false;
        }
    }
}