using Govor.Core.Models.Messages;

namespace Govor.Core.Repositories.MediasAttachments;

public interface IMediaAttachmentsWriter
{
    Task AddAsync(MediaAttachments mediaAttachments);
    Task UpdateAsync(MediaAttachments attachments);
    Task RemoveAsync(Guid Id);
}