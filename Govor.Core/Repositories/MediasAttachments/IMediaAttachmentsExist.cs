using Govor.Core.Models;

namespace Govor.Core.Repositories.MediasAttachments;

public interface IMediaAttachmentsExist
{
    bool Exist(Guid id);
    bool Exist(MediaAttachments attachments);
}