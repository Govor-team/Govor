using Govor.Domain.Models;
using Govor.Domain;
using Microsoft.EntityFrameworkCore;
using Govor.Domain.Models.Messages;

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
                .AnyAsync(gm => gm.GroupId == media.OwnerId && gm.UserId == userId && !gm.IsBanned),

            MediaOwnerType.Message => await _dbContext.MediaAttachments
                .AnyAsync(ma =>
                    ma.MediaFileId == mediaId &&
                    (
                        (ma.Message.RecipientType == RecipientType.User &&
                            _dbContext.PrivateChats.Any(c => c.Id == ma.Message.RecipientId &&
                                (c.UserAId == userId || c.UserBId == userId))) ||
                        (ma.Message.RecipientType == RecipientType.Group &&
                            _dbContext.GroupMemberships.Any(gm =>
                                gm.GroupId == ma.Message.RecipientId && gm.UserId == userId && !gm.IsBanned))
                    )),

            MediaOwnerType.System => true,
            MediaOwnerType.Reaction => await _dbContext.ReactionItems.AnyAsync(r => r.MediaFileId == mediaId),
            _ => false
        };
    }
}
