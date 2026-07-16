using Govor.Domain.Models;
using Govor.Domain.Models.Messages;

namespace Govor.Application.Medias;

public interface IMediaService
{
    public Task<MediaUploadResult> UploadMediaAsync(Media file);
    public Task DeleteMediaAsync(Guid fileId);
    public Task<Media> GetMediaByUrlAsync(string url);
    public Task<Media> GetMediaByIdAsync(Guid mediaId);
    public Task<bool> HasMediaAsync(Guid mediaId);
    public Task<bool> HasMediaByUrlAsync(string url);
    Task AttachToMessageAsync(Guid mediaId, Guid messageId);
}

public record Media(Guid UploaderId,
    DateTime UploadedOn,
    string FileName,
    byte[] Data,
    MediaType Type,
    string MimeType,
    string EncryptedKey,
    MediaOwnerType OwnerType,
    Guid? OwnerId);

public record MediaUploadResult(Guid MediaId, string Url);