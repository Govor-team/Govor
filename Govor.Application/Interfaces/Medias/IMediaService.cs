using Govor.Core.Models;

namespace Govor.Application.Interfaces.Medias;

public interface IMediaService
{
    public Task<MediaUploadResult> UploadMediaAsync(Media file);
    public Task DeleteMediaAsync(Guid fileId);
    public Task<MediaUploadResult> GetMediaAsync(string url);
}

public record Media(byte[] Data, string FileName, MediaType Type, string MineType, string EncryptedKey);

public record MediaUploadResult(Guid? MediaId, string Url);