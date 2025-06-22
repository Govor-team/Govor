using Govor.Core.Models;

namespace Govor.Core.Repositories.Messages;

public interface IMessagesReader
{
    Task<List<Message>> GetAllAsync();
    Task<Message> FindByIdAsync(Guid messageId);
    Task<List<Message>> FindBySenderIdAsync(Guid senderId);
    Task<List<Message>> FindByReceiverIdAsync(Guid receiverId);
    Task<List<Message>> FindBySenderAndReceiverIdAsync(Guid senderId, Guid receiverId, RecipientType recipientType = RecipientType.User);
    Task<List<Message>> FindBySentAtAsync(DateTime date);
}