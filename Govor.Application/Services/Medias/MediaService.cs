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
                Url = url,
                OwnerType = file.OwnerType,
                OwnerId = file.OwnerId,
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

    public async Task DeleteMediaAsync(Guid mediaId)
    {
        var mediaFile = await _dbContext.MediaFiles
                            .FirstOrDefaultAsync(x => x.Id == mediaId)
                        ?? throw new KeyNotFoundException($"No media found by given id {mediaId}");

        await _storageService.RemoveAsync(mediaFile.Url);
        
        _dbContext.MediaFiles.Remove(mediaFile);

        await _dbContext.SaveChangesAsync();
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
                string.Empty,
                mediaFile.OwnerType,
                mediaFile.OwnerId
            );
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogWarning(ex, "Media file not found on storage.");
            throw;
        }
    }

    public async Task<bool> HasMediaAsync(Guid mediaId)
    {
        return await _dbContext.MediaFiles.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == mediaId) is not null;
    }

    public async Task<bool> HasMediaByUrlAsync(string url)
    {
        return await _dbContext.MediaFiles.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Url == url) is not null;
    }

    public async Task AttachToMessageAsync(Guid mediaId, Guid messageId)
    {
        var mediaFile = await _dbContext.MediaFiles
            .FirstOrDefaultAsync(x => x.Id == mediaId)
            ?? throw new KeyNotFoundException($"No media found by given id {mediaId}");

        if (mediaFile.OwnerType != MediaOwnerType.Message)
        {
            _logger.LogWarning("Attempt to attach already owned media {MediaId}", mediaId);
            throw new InvalidOperationException($"Media {mediaId} is already attached to {mediaFile.OwnerType}");
        }

        mediaFile.OwnerType = MediaOwnerType.Message;
        mediaFile.OwnerId = messageId;

        _dbContext.MediaFiles.Update(mediaFile);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Media {MediaId} successfully attached to message {MessageId}", mediaId, messageId);
    }
}