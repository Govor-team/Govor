using Govor.Application.Infrastructure.Common;
using Govor.Application.Reactions;
using Govor.Application.Storage;
using Govor.Domain;
using Govor.Domain.Common;
using Govor.Domain.Models;
using Govor.Domain.Models.Messages;
using Govor.Domain.Models.Reactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartRes;

namespace Govor.Application.Groups;

public interface IGroupAvatarService
{
    Task<Result<Guid, Error>> UploadAsync(Guid actorId, Guid groupId, byte[] data, CancellationToken cancellationToken = default);
    Task<Result<Unit, Error>> RemoveAsync(Guid actorId, Guid groupId);
}

public class GroupAvatarService (GovorDbContext context,
    INowDateTimeProvider clock,
    IReactionMediaProcessor processor,
    IStorageService storage,
    ILogger<GroupAvatarService> logger) : IGroupAvatarService
{
    public async Task<Result<Guid, Error>> UploadAsync(Guid actorId, Guid groupId, byte[] data, CancellationToken cancellationToken = default)
    {
        if (!await IsOwnerAsync(actorId, groupId)) return Result.Failure<Guid>(Denied());
        var processed = await processor.ProcessAsync(data, cancellationToken);
        if (processed.IsFailure) return Result.Failure<Guid>(processed.Error);
        if (processed.Value.Kind == ReactionKind.Gif)
            return Result.Failure<Guid>(Error.Validation("Group.Avatar.Invalid", "A static PNG, JPEG or WebP image is required."));
        var operationId = Guid.NewGuid();
        return await context.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            var completed = await context.MediaFiles.AsNoTracking().FirstOrDefaultAsync(f => f.Id == operationId);
            if (completed is not null) return Result.Success(completed.Id);
            await using var transaction = await context.Database.BeginTransactionAsync();
            await context.ChatGroups.Where(g => g.Id == groupId).ExecuteUpdateAsync(s => s.SetProperty(g => g.Name, g => g.Name));
            if (!await IsOwnerAsync(actorId, groupId)) return Result.Failure<Guid>(Denied());
            string? path = null;
            try
            {
                path = await storage.SaveAsync(processed.Value.Data, operationId.ToString("N") + ".png");
                context.MediaFiles.Add(new MediaFile
                {
                    Id = operationId, UploaderId = actorId, OwnerType = MediaOwnerType.GroupAvatar, OwnerId = groupId,
                    DateCreated = clock.Now, MineType = "image/png", MediaType = MediaType.Image, Url = path
                });
                await context.SaveChangesAsync();
                await context.ChatGroups.Where(g => g.Id == groupId).ExecuteUpdateAsync(s => s.SetProperty(g => g.ImageId, operationId));
            }
            catch
            {
                context.ChangeTracker.Clear();
                if (path is not null)
                    try { await storage.RemoveAsync(path); }
                    catch (Exception ex) { logger.LogError(ex, "Cannot remove failed group avatar {MediaId}", operationId); }
                throw;
            }
            // Preserve the file if commit has an ambiguous outcome; retry checks operationId.
            try { await transaction.CommitAsync(); }
            catch { context.ChangeTracker.Clear(); throw; }
            return Result.Success(operationId);
        });
    }

    public Task<Result<Unit, Error>> RemoveAsync(Guid actorId, Guid groupId) => context.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
    {
        await using var transaction = await context.Database.BeginTransactionAsync();
        await context.ChatGroups.Where(g => g.Id == groupId).ExecuteUpdateAsync(s => s.SetProperty(g => g.Name, g => g.Name));
        if (!await IsOwnerAsync(actorId, groupId)) return Result.Failure(Denied());
        await context.ChatGroups.Where(g => g.Id == groupId).ExecuteUpdateAsync(s => s.SetProperty(g => g.ImageId, Guid.Empty));
        await transaction.CommitAsync();
        return Result.Success();
    });

    private Task<bool> IsOwnerAsync(Guid actorId, Guid groupId) => context.ChatGroups.AnyAsync(g => g.Id == groupId && g.OwnerUserId == actorId &&
        context.GroupMemberships.Any(m => m.GroupId == groupId && m.UserId == actorId && !m.IsBanned));
    private static Error Denied() => Error.Forbidden("Group.Avatar.OwnerRequired", "Only the active group owner can change its avatar.");
}
