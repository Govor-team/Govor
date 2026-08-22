using Govor.Domain.Common;
using Govor.Domain.Models;
using Govor.Domain.Models.Messages;
using SmartRes;

namespace Govor.Application.Medias;

public interface IMediaService
{
    public Task<Result<MediaUploadResult, Error>> UploadMediaAsync(Media file);
    public Task<Result<Unit, Error>> DeleteMediaAsync(Guid fileId);
    public Task<Result<Media, Error>> GetMediaByUrlAsync(string url);
    public Task<Result<Media, Error>> GetMediaByIdAsync(Guid mediaId);
    public Task<bool> HasMediaAsync(Guid mediaId);
    public Task<bool> HasMediaByUrlAsync(string url);
    public Task<Result<Unit, Error>> AttachToMessageAsync(Guid mediaId, Guid messageId);
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