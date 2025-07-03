using Govor.Application.Interfaces.Messages.Parameters;

namespace Govor.Application.Interfaces.Messages;

public interface IMessageSendingService
{
    Task<Result> SendMessageAsync(SendMessage newMessage);
}