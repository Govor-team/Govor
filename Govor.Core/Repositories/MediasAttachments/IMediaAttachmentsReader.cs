using Govor.Core.Models;

namespace Govor.Core.Repositories.MediasAttachments;

public interface IMediaAttachmentsReader
{
    Task<List<MediaAttachments>> GetAllAsync();
    Task<MediaAttachments> FindByIdAsync(Guid id);
    Task<List<MediaAttachments>> GetAllByMessageId(Guid messageId);
}