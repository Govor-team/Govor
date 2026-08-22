using Govor.Application.Messages.Parameters;
using Govor.Domain;
using Govor.Domain.Common;
using Govor.Domain.Models.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartRes;

namespace Govor.Application.Messages;

public class MessageRemovingService : IMessageRemovingService
{
    private readonly GovorDbContext _govorDbContext;
    private readonly ILogger<MessageRemovingService> _logger;

    public MessageRemovingService
    (GovorDbContext govorDbContext, 
        ILogger<MessageRemovingService> logger)
    {
        _govorDbContext = govorDbContext;
        _logger = logger;
    }

    public async Task<Result<Message,Error>> DeleteMessageAsync(DeleteMessage deleteParams)
    {
       var message = await _govorDbContext.Messages.FirstOrDefaultAsync(m => m.Id == deleteParams.MessageId);
      
       if (message == null)
           return Result<Message, Error>.Failure(
               Error.NotFound(
               "MessageRemoving",
               "Message with given id doesn't exist")
           );

       var result = message.RecipientType switch
       {
           RecipientType.Group => await ValidateGroupRecipientAsync(message, deleteParams),
           RecipientType.User => await ValidateUserRecipientAsync(message, deleteParams),
           
           _ => Result<Message, Error>.Failure(Error.Failure(
                   "MessageRemoving.ArgumentOutOfRangeException",
                   $"Argument out of range: {nameof(message.RecipientType)}"
               )
           )
       };
       
       return result;
    }

    private async Task<Result<Message,Error>> ValidateGroupRecipientAsync(Message message, DeleteMessage deleteParams)
    {
        if (deleteParams.DeleterId == message.RecipientId)
        {
            return await ForceRemoveAsync(message);
        }
        else
        {
            // TODO made admin rules 
            return Result<Message, Error>.Failure(Error.Failure("MessageRemoving.HaveNoPermission", 
                $"You do not have permission to delete message {message.Id}"
                ));
        }
    }

    private async Task<Result<Message,Error>> ValidateUserRecipientAsync(Message message, DeleteMessage deleteParams)
    {
        if (deleteParams.DeleterId == message.RecipientId)
        {
           return await ForceRemoveAsync(message);
        }
        else
        {
           // TODO made hide rules
           return Result<Message, Error>.Failure(Error.Failure("MessageRemoving.HaveNoPermission",
               $"You do not have permission to delete message {message.Id}"
               ));
        }
    }

    private async Task<Result<Message, Error>> ForceRemoveAsync(Message message)
    { 
        _govorDbContext.Messages.Remove(message);
        await _govorDbContext.SaveChangesAsync();
            
        return message;
    }
}