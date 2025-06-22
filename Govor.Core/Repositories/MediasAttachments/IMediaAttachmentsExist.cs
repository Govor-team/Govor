using Govor.Core.Models;

namespace Govor.Core.Repositories.MediasAttachments;

public interface IMediaAttachmentsExist
{
    bool Exists(Guid id);
    bool Exists(MediaAttachments attachments);
}