using Govor.Core.Models;

namespace Govor.Core.Infrastructure.Validators;

public class PrivateChatValidator : IObjectValidator<PrivateChat>
{
    public void Validate(PrivateChat chat)
    {
        try
        {
            if(chat is null)
                throw new ArgumentNullException(nameof(chat));
            if(chat.Id == Guid.Empty)
                throw new ArgumentException("Id cannot be empty", nameof(chat));
            if(chat.UserAId == Guid.Empty)
                throw new ArgumentException("UserAId cannot be empty", nameof(chat.UserAId));
            if(chat.UserBId == Guid.Empty)
                throw new ArgumentException("UserBId cannot be empty", nameof(chat.UserBId));
        }
        catch (Exception ex)
        {
            throw new InvalidObjectException<PrivateChat>(ex);
        }
    }

    public bool TryValidate(PrivateChat chat)
    {
        try
        {
            Validate(chat);
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
}