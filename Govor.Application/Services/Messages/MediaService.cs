using Govor.Application.Interfaces;
using Govor.Application.Interfaces.Medias;

namespace Govor.Application.Services.Messages;

public class MediaService : IMediaService
{
    private IStorageService _storageService;
    
    public MediaService(IStorageService storageService)
    {
        _storageService = storageService;
    }
    
    public Task<MediaUploadResult> UploadMediaAsync(Media file)
    {
        throw new NotImplementedException();
    }

    public Task DeleteMediaAsync(Guid fileId)
    {
        throw new NotImplementedException();
    }

    public Task<MediaUploadResult> GetMediaAsync(string url)
    {
        throw new NotImplementedException();
    }
}