using Govor.Application.Messages.Parameters;
using Microsoft.EntityFrameworkCore;
using Govor.Domain;
using Govor.Domain.Models.Messages;
using Microsoft.Extensions.Logging;

namespace Govor.Application.Messages;

public class MessageSendingService : IMessageSendingService
{
    private readonly GovorDbContext _dbContext;
    private readonly ILogger<MessageSendingService> _logger;

    public MessageSendingService(GovorDbContext dbContext, ILogger<MessageSendingService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<SendMessageResult> SendMessageAsync(SendMessage sendParams)
    {
        var validationResult = sendParams.RecipientType switch
        {
            RecipientType.User => await ValidateUserRecipientAsync(sendParams.RecipientId),
            RecipientType.Group => await ValidateGroupRecipientAsync(sendParams.FromUserId, sendParams.RecipientId),
            _ => (Success: false, Error: "Invalid recipient type.")
        };

        if (!validationResult.Success)
        {
            _logger.LogWarning("Message send failed: {Error}", validationResult.Error);
            return new SendMessageResult(false, new InvalidOperationException(validationResult.Error), default);
        }
        
        var messageId = Guid.NewGuid();
        var message = new Message
        {
            Id = messageId,
            SenderId = sendParams.FromUserId,
            RecipientId = sendParams.RecipientId,
            RecipientType = sendParams.RecipientType,
            EncryptedContent = sendParams.EncryptContent,
            SentAt = sendParams.SendAt,
            IsEdited = false,
            ReplyToMessageId = sendParams.ReplyToMessageId,
            MediaAttachments = sendParams.Media?.Select(m => new MediaAttachments
            {
                Id = Guid.NewGuid(),
                MessageId = messageId,
                MediaFileId = m.MediaId
            }).ToList() ?? []
        };
        
        await _dbContext.Messages.AddAsync(message);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Message {MessageId} sent successfully.", messageId);
        return new SendMessageResult(true, null, message);
    }

    private async Task<(bool Success, string Error)> ValidateUserRecipientAsync(Guid chatId)
    {
        var chatExists = await _dbContext.PrivateChats.AnyAsync(c => c.Id == chatId);
        return chatExists ? (true, null) : (false, $"Private chat {chatId} not found.");
    }

    private async Task<(bool Success, string Error)> ValidateGroupRecipientAsync(Guid userId, Guid groupId)
    {
        var groupExists = await _dbContext.ChatGroups.AnyAsync(g => g.Id == groupId);
        if (!groupExists) return (false, $"Group {groupId} not found.");
        
        var isMember = await _dbContext.GroupMemberships.AnyAsync(gm => gm.UserId == userId && gm.GroupId == groupId);
        if (!isMember) return (false, "Sender is not a member of the group.");

        return (true, null);
    }
}
