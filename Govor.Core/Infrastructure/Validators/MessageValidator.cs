using Govor.Core.Models;

namespace Govor.Core.Infrastructure.Validators;

public class MessageValidator : IObjectValidator<Message>
{
    public void Validate(Message message)
    {
        try
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));
            if (message.Id == Guid.Empty)
                throw new ArgumentException("Message ID cannot be empty", nameof(message.Id));
            if (message.SenderId == Guid.Empty)
                throw new ArgumentException("Sender ID cannot be empty", nameof(message.SenderId));
            if (message.RecipientId == Guid.Empty)
                throw new ArgumentException("Recipient ID cannot be empty", nameof(message.RecipientId));
            if(string.IsNullOrWhiteSpace(message.EncryptedContent))
                throw new ArgumentException("Encrypted content cannot be empty", nameof(message.EncryptedContent));
            if(message.IsEdited && message.EditedAt == DateTime.MinValue)
                throw new ArgumentException("Edited at time cannot be empty", nameof(message.EditedAt));
            if (message.EditedAt == DateTime.MinValue)
                throw new ArgumentException("Edited at time cannot be empty", nameof(message.EditedAt));
        }
        catch (Exception ex)
        {
            throw new InvalidObjectException<Message>(ex);
        }
    }

    public bool TryValidate(Message message)
    {
        try
        {
            Validate(message);
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
}