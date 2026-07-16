using Govor.Application.Messages.Parameters;
using Microsoft.EntityFrameworkCore;
using Govor.Domain;
using Govor.Domain.Models.Messages;
using Microsoft.Extensions.Logging;

namespace Govor.Application.Messages;

public class MessageEditingService : IMessageEditingService
{
    private readonly GovorDbContext _dbContext;
    private readonly ILogger<MessageEditingService> _logger;

    public MessageEditingService(GovorDbContext dbContext, ILogger<MessageEditingService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<EditMessageResult> EditMessageAsync(EditMessage editParams)
    {
        var message = await _dbContext.Messages
            .Include(m => m.MediaAttachments)
            .FirstOrDefaultAsync(m => m.Id == editParams.MessageId);

        if (message == null)
        {
            return new EditMessageResult(false, new KeyNotFoundException("Message not found."), null);
        }
        
        if (message.SenderId != editParams.EditorId)
        {
            _logger.LogWarning("User {EditorId} unauthorized to edit message {MessageId}", editParams.EditorId, editParams.MessageId);
            return new EditMessageResult(false, new UnauthorizedAccessException("User is not authorized to edit this message."), null);
        }
        
        var originalMessageSnapshot = new Message
        {
            Id = message.Id,
            SenderId = message.SenderId,
            RecipientId = message.RecipientId,
            RecipientType = message.RecipientType,
            SentAt = message.SentAt,
            ReplyToMessageId = message.ReplyToMessageId,
            MediaAttachments = message.MediaAttachments?.ToList() ?? []
        };
        
        message.EncryptedContent = editParams.NewContent;
        message.IsEdited = true;
        message.EditedAt = editParams.EditedAt;

        await _dbContext.SaveChangesAsync();
        
        _logger.LogInformation("Message {MessageId} edited successfully.", editParams.MessageId);
        return new EditMessageResult(true, null, originalMessageSnapshot);
    }
}
