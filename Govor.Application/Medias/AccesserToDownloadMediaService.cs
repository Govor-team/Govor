using Govor.Domain.Models;
using Govor.Domain;
using Microsoft.EntityFrameworkCore;

namespace Govor.Application.Medias;

public class AccesserToDownloadMediaService : IAccesserToDownloadMedia
{
    private readonly GovorDbContext _dbContext;

    public AccesserToDownloadMediaService(GovorDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HasAccessAsync(Guid mediaId, Guid userId)
    {
        var media = await _dbContext.MediaFiles
            .AsNoTracking()
            .Where(m => m.Id == mediaId)
            .Select(m => new { m.OwnerType, m.OwnerId, m.UploaderId })
            .FirstOrDefaultAsync();

        if (media is null)
            return false;

        return media.OwnerType switch
        {
            MediaOwnerType.Avatar => true, // media.OwnerId == userId
            MediaOwnerType.GroupAvatar => await _dbContext.GroupMemberships
                .AnyAsync(gm => gm.GroupId == media.OwnerId && gm.UserId == userId),

            MediaOwnerType.Message => await _dbContext.MediaAttachments
                .AnyAsync(ma =>
                    ma.MediaFileId == mediaId &&
                    (
                        ma.Message.SenderId == userId ||
                        ma.Message.RecipientId == userId ||
                        _dbContext.GroupMemberships.Any(gm =>
                            gm.GroupId == ma.Message.RecipientId && gm.UserId == userId)
                    )),

            MediaOwnerType.System => true,
            _ => false
        };
    }
}