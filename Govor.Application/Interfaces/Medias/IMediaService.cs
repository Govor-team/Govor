using Govor.Core.Models.Messages;

namespace Govor.Application.Interfaces.Medias;

public interface IMediaService
{
    public Task<MediaUploadResult> UploadMediaAsync(Media file);
    public Task DeleteMediaAsync(Guid fileId);
    public Task<Media> GetMediaByUrlAsync(string url);
    public Task<Media> GetMediaByIdAsync(Guid mediaId);
}

public record Media(Guid UploaderId,
    DateTime UploadedOn,
    string FileName,
    byte[] Data,
    MediaType Type,
    string MimeType,
    string EncryptedKey);

public record MediaUploadResult(Guid? MediaId, string Url);