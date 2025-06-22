using Govor.Core.Models;
using Exception = System.Exception;

namespace Govor.Core.Infrastructure.Validators;

public class MediaAttachmentsValidator : IObjectValidator<MediaAttachments>
{
    public void Validate(MediaAttachments attachments)
    {
        try
        {
            if (attachments is null)
                throw new ArgumentNullException(nameof(attachments));
            if(attachments.Id == Guid.Empty)
                throw new ArgumentException("Id cannot be empty", nameof(attachments));
            if(attachments.MessageId == Guid.Empty)
                throw new ArgumentException("MessageId cannot be empty", nameof(attachments));
            if(string.IsNullOrWhiteSpace(attachments.FilePath))
                throw new ArgumentException("File path cannot be empty", nameof(attachments));
            if(string.IsNullOrWhiteSpace(attachments.EncryptedKey))
                throw new ArgumentException("Encrypted key cannot be empty", nameof(attachments));
        }
        catch (Exception ex)
        {
            throw new InvalidObjectException<MediaAttachments>(ex);
        }
        
    }

    public bool TryValidate(MediaAttachments attachments)
    {
        try
        {
            Validate(attachments);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}