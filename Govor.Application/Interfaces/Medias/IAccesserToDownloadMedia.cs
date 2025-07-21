namespace Govor.Application.Interfaces.Medias;

public interface IAccesserToDownloadMedia
{
    Task<bool> HasAccessAsync(Guid mediaFileId, Guid userId);
}