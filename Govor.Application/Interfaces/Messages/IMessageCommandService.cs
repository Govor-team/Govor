using Govor.Application.Interfaces.Messages.Parameters;
using Govor.Core.Models.Messages;

namespace Govor.Application.Interfaces.Messages;

// Combining IChatService and IGroupService functionalities relevant to messages
public interface IMessageCommandService
{
    Task<SendMessageResult> SendMessageAsync(SendMessage messageParameters);
    Task<EditMessageResult> EditMessageAsync(EditMessage messageParameters);
    Task<DeleteMessageResult> DeleteMessageAsync(DeleteMessage messageParameters);
    
    // Potentially other message-related methods like:
    // Task<GetMessagesResult> GetMessagesAsync(Guid userId, Guid chatId, RecipientType chatType, int pageNumber, int pageSize);
    // Task<Result> MarkMessageAsReadAsync(Guid userId, Guid messageId);
}



public record SendMessageResult(bool IsSuccess, Exception? Exception, Message Message) 
    : Result(IsSuccess, Exception, Message?.Id ?? Guid.Empty);

public record EditMessageResult(bool IsSuccess, Exception? Exception, Message? OriginalMessage) 
    : Result(IsSuccess, Exception, OriginalMessage?.Id ?? Guid.Empty)
{

}

public record DeleteMessageResult(bool IsSuccess, Exception? Exception, Message? OriginalMessage) 
    : Result(IsSuccess, Exception, OriginalMessage?.Id ?? Guid.Empty);
