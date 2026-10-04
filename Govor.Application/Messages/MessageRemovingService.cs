using DeleteMessage = Govor.Application.Messages.Parameters.DeleteMessage;
using Govor.Domain;
using Govor.Domain.Common;
using Govor.Application.Groups;
using Govor.Domain.Models.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartRes;

namespace Govor.Application.Messages;

public class MessageRemovingService : IMessageRemovingService
{
    private readonly GovorDbContext _govorDbContext;
    private readonly ILogger<MessageRemovingService> _logger;

    public MessageRemovingService(GovorDbContext govorDbContext, ILogger<MessageRemovingService> logger)
    {
        _govorDbContext = govorDbContext;
        _logger = logger;
    }

    public async Task<Result<Message, Error>> DeleteMessageAsync(DeleteMessage deleteParams)
    {
        var target = await _govorDbContext.Messages.AsNoTracking().FirstOrDefaultAsync(m => m.Id == deleteParams.MessageId);
        if (target?.RecipientType != RecipientType.Group) return await DeleteCoreAsync(deleteParams);
        return await _govorDbContext.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var transaction = await _govorDbContext.Database.BeginTransactionAsync();
            try
            {
                await _govorDbContext.ChatGroups.Where(g => g.Id == target.RecipientId)
                    .ExecuteUpdateAsync(s => s.SetProperty(g => g.Name, g => g.Name));
                var result = await DeleteCoreAsync(deleteParams);
                if (result.IsSuccess) await transaction.CommitAsync();
                return result;
            }
            catch { _govorDbContext.ChangeTracker.Clear(); throw; }
        });
    }

    private async Task<Result<Message, Error>> DeleteCoreAsync(DeleteMessage deleteParams)
    {
        var message = await _govorDbContext.Messages.FirstOrDefaultAsync(m => m.Id == deleteParams.MessageId);
        if (message is null)
            return Result.Failure<Message>(Error.NotFound("Message.NotFound", "Message not found."));

        if (!await _govorDbContext.HasChatAccessAsync(deleteParams.DeleterId,
            message.RecipientId, message.RecipientType))
            return Result.Failure<Message>(Error.Forbidden("Chat.AccessDenied", "You are not a member of this chat."));

        // Do not silently turn a local hide into deletion for everyone.
        if (!deleteParams.ForceRemove)
            return Result.Failure<Message>(Error.Validation("Message.HideNotSupported",
                "HideForMe is not supported. Use ForceRemove to delete a message for everyone."));

        var canDelete = message.SenderId == deleteParams.DeleterId ||
            (message.RecipientType == RecipientType.Group &&
             await _govorDbContext.IsGroupAdministratorAsync(message.RecipientId, deleteParams.DeleterId));
        if (!canDelete)
            return Result.Failure<Message>(Error.Forbidden("Message.Remove.AccessDenied",
                "Only the author or a group administrator can delete this message."));

        _govorDbContext.Messages.Remove(message);
        await _govorDbContext.SaveChangesAsync();
        _logger.LogInformation("Message {MessageId} removed by {UserId}", message.Id, deleteParams.DeleterId);
        return message;
    }
}
