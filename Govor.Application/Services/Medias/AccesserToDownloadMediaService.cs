using Govor.Application.Interfaces.Medias;
using Govor.Core.Models.Messages;
using Govor.Data;
using Microsoft.EntityFrameworkCore;

namespace Govor.Application.Services.Medias;

public class AccesserToDownloadMediaService : IAccesserToDownloadMedia
{
    private readonly GovorDbContext _dbContext;

    public AccesserToDownloadMediaService(GovorDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> HasAccessAsync(Guid mediaFileId, Guid userId)
    {
        return await _dbContext.MediaAttachments
            .Include(ma => ma.Message)
            .AnyAsync(ma =>
                ma.MediaFileId == mediaFileId &&
                (
                    (ma.Message.RecipientType == RecipientType.User &&
                     (ma.Message.SenderId == userId || ma.Message.RecipientId == userId))
                    ||
                    (ma.Message.RecipientType == RecipientType.Group &&
                     _dbContext.GroupMemberships.Any(gm =>
                         gm.GroupId == ma.Message.RecipientId &&
                         gm.UserId == userId))
                ));
    }
}