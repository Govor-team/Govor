using Govor.Core.Models;

namespace Govor.Core.Infrastructure.Validators;

public class ChatGroupValidator : IObjectValidator<ChatGroup>
{
    public void Validate(ChatGroup chat)
    {
        try
        {
            if(chat is null)
                throw new ArgumentNullException(nameof(chat));
            if(chat.Id == Guid.Empty)
                throw new ArgumentException("Id of chat group can't be empty",nameof(chat.Id));
            if(string.IsNullOrEmpty(chat.Name))
                throw new ArgumentException("Name of chat group can't be empty",nameof(chat.Name));
            if(chat.Description is null)
                throw new ArgumentException("Description of chat group can't be null",nameof(chat.Description));
            if(chat.ImageId == Guid.Empty)
                throw new ArgumentException("ImageId of chat group can't be empty",nameof(chat.ImageId));
            if(chat.IsPrivate && chat.InviteCodes.Count <= 0)
                throw new ArgumentException("Private group must have invitation links",nameof(chat.InviteCodes));
            if(chat.Admins.Count <= 0)
                throw new ArgumentException("Chat must have owner",nameof(chat.Admins));
        }
        catch (Exception ex)
        {
            throw new InvalidObjectException<ChatGroup>(ex);
        }
    }

    public bool TryValidate(ChatGroup chat)
    {
        try
        {
            Validate(chat);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}