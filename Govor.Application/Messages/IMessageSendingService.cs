using Govor.Application.Messages.Parameters;

namespace Govor.Application.Messages;

public interface IMessageSendingService
{
    Task<SendMessageResult> SendMessageAsync(SendMessage sendParams);
}