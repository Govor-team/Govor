using System.Globalization;
using Govor.Application.Infrastructure.Common;
using Govor.Application.Storage;
using Govor.Domain;
using Govor.Domain.Common;
using Govor.Domain.Models;
using Govor.Domain.Models.Messages;
using Govor.Domain.Models.Reactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartRes;

namespace Govor.Application.Reactions;

public class ReactionPackService(GovorDbContext context, INowDateTimeProvider clock,
    IReactionMediaProcessor processor, IStorageService storage, ILogger<ReactionPackService> logger) : IReactionPackService
{
    public async Task<List<ReactionPack>> ListAsync(Guid userId, bool installedOnly = false,
        bool includeDisabled = false, int skip = 0, int take = 50)
    {
        var query = context.ReactionPacks.AsNoTracking();
        if (!includeDisabled) query = query.Where(p => p.IsEnabled);
        if (installedOnly) query = query.Where(p => p.IsDefault || context.UserReactionPacks
            .Any(s => s.UserId == userId && s.PackId == p.Id));
        return await query.OrderByDescending(p => p.IsDefault).ThenBy(p => p.CreatedAt).ThenBy(p => p.Id)
            .Skip(Math.Max(0, skip)).Take(Math.Clamp(take, 1, 100))
            .Include(p => p.Reactions.Where(r => includeDisabled || r.IsEnabled)).AsSplitQuery().ToListAsync();
    }

    public async Task<Result<ReactionPack, Error>> GetAsync(Guid packId, bool includeDisabled = false)
    {
        var pack = await context.ReactionPacks.AsNoTracking()
            .Include(p => p.Reactions.Where(r => includeDisabled || r.IsEnabled))
            .FirstOrDefaultAsync(p => p.Id == packId && (includeDisabled || p.IsEnabled));
        return pack is null ? Result.Failure<ReactionPack>(Missing()) : Result.Success(pack);
    }

    public async Task<Result<ReactionPack, Error>> GetSharedAsync(string shareCode)
    {
        var id = await context.ReactionPacks.Where(p => p.ShareCode == shareCode && p.IsEnabled)
            .Select(p => (Guid?)p.Id).FirstOrDefaultAsync();
        return id.HasValue ? await GetAsync(id.Value) : Result.Failure<ReactionPack>(Missing());
    }

    // Definitions remain readable after disabling so historical reactions still render.
    public async Task<Result<ReactionItem, Error>> GetReactionAsync(Guid reactionId)
    {
        var item = await context.ReactionItems.AsNoTracking().FirstOrDefaultAsync(r => r.Id == reactionId);
        return item is null ? Result.Failure<ReactionItem>(Missing()) : Result.Success(item);
    }

    public async Task<Result<Unit, Error>> SubscribeAsync(Guid userId, Guid packId, bool subscribe)
    {
        var pack = await context.ReactionPacks.AsNoTracking().FirstOrDefaultAsync(p => p.Id == packId && (!subscribe || p.IsEnabled));
        if (pack is null) return Result.Failure(Missing());
        if (pack.IsDefault)
            return subscribe ? Result.Success() : Result.Failure(Invalid("The default pack is always available."));
        if (subscribe)
            await context.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO \"UserReactionPacks\" (\"UserId\", \"PackId\") VALUES ({userId}, {packId}) ON CONFLICT (\"UserId\", \"PackId\") DO NOTHING");
        else
            await context.UserReactionPacks.Where(p => p.UserId == userId && p.PackId == packId).ExecuteDeleteAsync();
        return Result.Success();
    }

    public async Task<Result<ReactionPack, Error>> CreateAsync(Guid actorId, string name, string description)
    {
        if (!await IsAdminAsync(actorId)) return Result.Failure<ReactionPack>(Denied());
        if (!ValidText(name, description)) return Result.Failure<ReactionPack>(Invalid("Invalid name or description."));
        var pack = new ReactionPack
        {
            Id = Guid.NewGuid(), Name = name.Trim(), Description = description.Trim(),
            ShareCode = Guid.NewGuid().ToString("N"), CreatedByUserId = actorId, CreatedAt = clock.Now
        };
        context.ReactionPacks.Add(pack);
        await context.SaveChangesAsync();
        return pack;
    }

    public async Task<Result<ReactionPack, Error>> UpdateAsync(Guid actorId, Guid packId, string name, string description, bool enabled)
    {
        if (!await IsAdminAsync(actorId)) return Result.Failure<ReactionPack>(Denied());
        if (!ValidText(name, description)) return Result.Failure<ReactionPack>(Invalid("Invalid name or description."));
        var pack = await context.ReactionPacks.FirstOrDefaultAsync(p => p.Id == packId);
        if (pack is null) return Result.Failure<ReactionPack>(Missing());
        if (pack.IsDefault) return Result.Failure<ReactionPack>(Invalid("The default pack cannot be modified."));
        pack.Name = name.Trim(); pack.Description = description.Trim(); pack.IsEnabled = enabled;
        await context.SaveChangesAsync();
        return pack;
    }

    public async Task<Result<Unit, Error>> DisableAsync(Guid actorId, Guid packId)
    {
        if (!await IsAdminAsync(actorId)) return Result.Failure(Denied());
        var pack = await context.ReactionPacks.FirstOrDefaultAsync(p => p.Id == packId);
        if (pack is null) return Result.Failure(Missing());
        if (pack.IsDefault) return Result.Failure(Invalid("The default pack cannot be disabled."));
        pack.IsEnabled = false;
        await context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<ReactionItem, Error>> AddEmojiAsync(Guid actorId, Guid packId, string name, string emoji)
    {
        if (string.IsNullOrWhiteSpace(emoji) || emoji.Length > 64 || StringInfo.ParseCombiningCharacters(emoji).Length != 1 ||
            !emoji.EnumerateRunes().Any(r => System.Text.Rune.GetUnicodeCategory(r) == UnicodeCategory.OtherSymbol))
            return Result.Failure<ReactionItem>(Invalid("Provide a single emoji grapheme."));
        return await AddAsync(actorId, packId, name, emoji, null);
    }

    public async Task<Result<ReactionItem, Error>> AddMediaAsync(Guid actorId, Guid packId, string name, byte[] data,
        CancellationToken cancellationToken = default)
    {
        if (!await IsAdminAsync(actorId)) return Result.Failure<ReactionItem>(Denied());
        if (!await context.ReactionPacks.AnyAsync(p => p.Id == packId && !p.IsDefault && p.IsEnabled))
            return Result.Failure<ReactionItem>(Invalid("An active custom pack is required."));
        var processed = await processor.ProcessAsync(data, cancellationToken);
        if (processed.IsFailure) return Result.Failure<ReactionItem>(processed.Error);
        return await AddAsync(actorId, packId, name, null, processed.Value);
    }

    private async Task<Result<ReactionItem, Error>> AddAsync(Guid actorId, Guid packId, string name,
        string? emoji, ProcessedReactionMedia? media)
    {
        if (!await IsAdminAsync(actorId)) return Result.Failure<ReactionItem>(Denied());
        if (!ValidText(name, "")) return Result.Failure<ReactionItem>(Invalid("Invalid reaction name."));
        var operationId = Guid.NewGuid();
        return await context.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            var completed = await context.ReactionItems.AsNoTracking().FirstOrDefaultAsync(r => r.Id == operationId);
            if (completed is not null) return Result.Success(completed);
            await using var transaction = await context.Database.BeginTransactionAsync();
            // Lock the pack to serialize the item limit and concurrent disabling.
            var locked = await context.ReactionPacks.Where(p => p.Id == packId && !p.IsDefault && p.IsEnabled)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Name, p => p.Name));
            if (locked == 0) return Result.Failure<ReactionItem>(Invalid("An active custom pack is required."));
            if (await context.ReactionItems.CountAsync(r => r.PackId == packId) >= 100)
                return Result.Failure<ReactionItem>(Invalid("A pack can contain at most 100 reactions, including disabled entries."));

            var item = new ReactionItem
            {
                Id = operationId, PackId = packId, Name = name.Trim(), Emoji = emoji,
                Kind = media?.Kind ?? ReactionKind.Emoji, CreatedAt = clock.Now
            };
            item.Code = $":reaction_{item.Id:N}:";
            string? path = null;
            try
            {
                if (media is not null)
                {
                    path = await storage.SaveAsync(media.Data, item.Id.ToString("N") + media.Extension);
                    var file = new MediaFile
                    {
                        Id = Guid.NewGuid(), UploaderId = actorId, Url = path, MineType = media.MimeType,
                        DateCreated = clock.Now, OwnerType = MediaOwnerType.Reaction, OwnerId = item.Id,
                        MediaType = MediaType.Image
                    };
                    context.MediaFiles.Add(file);
                    item.MediaFileId = file.Id; item.Width = media.Width; item.Height = media.Height;
                    item.SizeBytes = media.Data.Length; item.DurationMilliseconds = media.DurationMilliseconds;
                }
                context.ReactionItems.Add(item);
                await context.SaveChangesAsync();
            }
            catch
            {
                if (path is not null)
                {
                    try { await storage.RemoveAsync(path); }
                    catch (Exception ex) { logger.LogError(ex, "Cannot remove failed reaction upload {ReactionId}", item.Id); }
                }
                // Clear failed tracked additions before an execution-strategy retry.
                context.ChangeTracker.Clear();
                throw;
            }
            // Do not delete the file on an ambiguous Commit outcome. The stable operation ID
            // above detects an already committed item when the execution strategy retries.
            try { await transaction.CommitAsync(); }
            catch { context.ChangeTracker.Clear(); throw; }
            return Result.Success(item);
        });
    }

    public async Task<Result<Unit, Error>> DisableReactionAsync(Guid actorId, Guid packId, Guid reactionId)
    {
        if (!await IsAdminAsync(actorId)) return Result.Failure(Denied());
        var item = await context.ReactionItems.Include(r => r.Pack)
            .FirstOrDefaultAsync(r => r.Id == reactionId && r.PackId == packId);
        if (item is null) return Result.Failure(Missing());
        if (item.Pack.IsDefault) return Result.Failure(Invalid("Default reactions cannot be disabled."));
        item.IsEnabled = false;
        await context.SaveChangesAsync();
        return Result.Success();
    }

    private Task<bool> IsAdminAsync(Guid actorId) => context.Users.AnyAsync(u => u.Id == actorId && u.Invite != null && u.Invite.IsAdmin);
    private static bool ValidText(string name, string description) => !string.IsNullOrWhiteSpace(name) &&
        name.Trim().Length <= 100 && description is not null && description.Length <= 500;
    private static Error Invalid(string message) => Error.Validation("ReactionPack.Invalid", message);
    private static Error Missing() => Error.NotFound("ReactionPack.NotFound", "Reaction pack or item not found.");
    private static Error Denied() => Error.Forbidden("ReactionPack.AdminRequired", "Administrator permission is required.");
}
