namespace Govor.Application.Medias;

public interface IAccesserToDownloadMedia
{
    Task<bool> HasAccessAsync(Guid mediaFileId, Guid userId);
}