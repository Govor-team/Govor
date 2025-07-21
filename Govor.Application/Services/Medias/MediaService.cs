using Govor.Application.Interfaces;
using Govor.Application.Interfaces.Medias;
using Govor.Core.Models;
using Govor.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Govor.Application.Services.Medias;

public class MediaService : IMediaService
{
    private ILogger<MediaService> _logger;
    private IStorageService _storageService;
    private GovorDbContext _dbContext;
    
    public MediaService(IStorageService storageService, GovorDbContext dbContext, ILogger<MediaService> logger)
    {
        _storageService = storageService;
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<MediaUploadResult> UploadMediaAsync(Media file)
    {
        try
        {
            var url = await _storageService.SaveAsync(file.Data, file.FileName);

            var mediaId = Guid.NewGuid();
            
            _dbContext.MediaFiles.Add(new MediaFile()
            {
                Id = mediaId, 
                UploaderId = file.UploaderId,
                DateCreated = file.UploadedOn,
                MediaType = file.Type,
                MineType = file.MimeType,
                Url = url
            });
            
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation($"Media uploaded: {url} with id: {mediaId} by {file.UploaderId}");
            
            return new MediaUploadResult(mediaId, url);
        }
        catch (ArgumentException ex)
        {
            throw new InvalidOperationException($"An error occured while uploading the media file: {ex.Message}");
        }
    }

    public Task DeleteMediaAsync(Guid fileId)
    {
        throw new NotImplementedException();
    }

    public Task<Media> GetMediaByUrlAsync(string url)
    {
        throw new NotImplementedException();
    }

    public async Task<Media> GetMediaByIdAsync(Guid mediaId)
    {
        try
        {
            var mediaFile = await _dbContext.MediaFiles
                                .AsNoTracking()
                                .FirstOrDefaultAsync(x => x.Id == mediaId)
                            ?? throw new KeyNotFoundException($"No media found by given id {mediaId}");

            // Загрузить бинарные данные из хранилища
            Stream dataStream = await _storageService.LoadAsync(mediaFile.Url);

            // Считать поток в byte[]
            using var memoryStream = new MemoryStream();
            await dataStream.CopyToAsync(memoryStream);
            var contentBytes = memoryStream.ToArray();
            
            _logger.LogInformation($"Media found: {mediaFile.MediaType} with id: {mediaFile.Id} and url: {mediaFile.Url}");
            
            // Вернуть объект Media
            return new Media(
                mediaFile.UploaderId,
                mediaFile.DateCreated,
                mediaFile.MediaType.ToString(),
                contentBytes,
                mediaFile.MediaType,
                mediaFile.MineType,
                string.Empty
            );
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogWarning(ex, "Media file not found on storage.");
            throw;
        }
    }
}