using Govor.Application.Interfaces;
using Govor.Application.Interfaces.Messages;
using Govor.Application.Interfaces.Messages.Parameters;
using Govor.Core.Models;
using Govor.Core.Models.Messages;
using Govor.Core.Repositories.Groups;
using Govor.Core.Repositories.Messages;
using Govor.Core.Repositories.PrivateChats;
using Govor.Core.Repositories.Users;
using Govor.Data.Repositories.Exceptions;
using Microsoft.Extensions.Logging;

namespace Govor.Application.Services.Messages;

public class MessageCommandService : IMessageCommandService
{
    private readonly IMessagesRepository _messagesRepository;
    private readonly IUsersRepository _usersRepository; // For validating user recipients
    private readonly IGroupsRepository _groupsRepository; // For validating group recipients and fetching members
    private readonly IPrivateChatsRepository _privateChats;
    private readonly IVerifyFriendship _verifyFriendship; // For private messages
    private readonly ILogger<MessageCommandService> _logger;

    public MessageCommandService(
        IMessagesRepository messagesRepository,
        IUsersRepository usersRepository,
        IGroupsRepository groupsRepository,
        IVerifyFriendship verifyFriendship,
        IPrivateChatsRepository privateChats,
        ILogger<MessageCommandService> logger)
    {
        _messagesRepository = messagesRepository;
        _usersRepository = usersRepository;
        _groupsRepository = groupsRepository;
        _verifyFriendship = verifyFriendship;
        _privateChats = privateChats;
        _logger = logger;
    }

    public async Task<SendMessageResult> SendMessageAsync(SendMessage sendParams)
    {
        Guid recipientId;
        try
        {
            // Validate recipient
            if (sendParams.RecipientType == RecipientType.User)
            {
                if (!await _usersRepository.ExistsByIdAsync(sendParams.RecipientId))
                {
                    _logger.LogWarning("Attempt to send message to non-existent user {RecipientId}", sendParams.RecipientId);
                    return new SendMessageResult(false, new KeyNotFoundException($"Recipient user {sendParams.RecipientId} not found."), default);
                }
                // Verify friendship for private messages
                await _verifyFriendship.VerifyAsync(sendParams.FromUserId, sendParams.RecipientId);
                recipientId = await GetPrivateChatsIdAsync(sendParams.FromUserId, sendParams.RecipientId);
            }
            else if (sendParams.RecipientType == RecipientType.Group)
            {
                if (!_groupsRepository.Exist(sendParams.RecipientId))
                {
                    _logger.LogWarning("Attempt to send message to non-existent group {GroupId}", sendParams.RecipientId);
                    return new SendMessageResult(false, new KeyNotFoundException($"Recipient group {sendParams.RecipientId} not found."), default);
                }
                // TODO: Optionally, verify if sender is a member of the group
                bool isMember = await _groupsRepository.IsUserMemberOfGroupAsync(sendParams.FromUserId, sendParams.RecipientId);
                if (!isMember)
                {
                   _logger.LogWarning("User {UserId} attempted to send message to group {GroupId} but is not a member", sendParams.FromUserId, sendParams.RecipientId);
                    return new SendMessageResult(false, new UnauthorizedAccessException("Sender is not a member of the group."), default);
                }
                
                recipientId = sendParams.RecipientId;
            }
            else
            {
                _logger.LogError("Invalid recipient type specified: {RecipientType}", sendParams.RecipientType);
                return new SendMessageResult(false, new ArgumentException("Invalid recipient type."), default);
            }
            
                        
            var messageId = Guid.NewGuid();
            var message = new Message
            {
                Id = messageId,
                SenderId = sendParams.FromUserId,
                RecipientId = recipientId, 
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
                }).ToList() ?? new List<MediaAttachments>()
            };

            await _messagesRepository.AddAsync(message);
            _logger.LogInformation("Message {MessageId} from {SenderId} to {RecipientId} ({RecipientType}) saved successfully.", messageId, sendParams.FromUserId, sendParams.RecipientId, sendParams.RecipientType);
            return new SendMessageResult(true, null, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message from {SenderId} to {RecipientId} ({RecipientType})", sendParams.FromUserId, sendParams.RecipientId, sendParams.RecipientType);
            return new SendMessageResult(false, ex, default);
        }
    }

    private async Task<Guid> GetPrivateChatsIdAsync(Guid userA, Guid userB)
    {
        Guid recipientId;
        // Makes new PrivateChat if not exist
        if (!_privateChats.Exist(userA, userB))
        {
            recipientId = Guid.NewGuid();
            await _privateChats.AddAsync(new PrivateChat()
                { Id = recipientId, UserAId = userA, UserBId = userB });
        }
        else
        {
            recipientId = (await _privateChats.GetByMembersAsync(userA, userB)).Id;
        }
        
        return recipientId;
    }

    public async Task<EditMessageResult> EditMessageAsync(EditMessage editParams)
    {
        try
        {
            var message = await _messagesRepository.FindByIdAsync(editParams.MessageId);

            if (message.SenderId != editParams.EditorId)
            {
                _logger.LogWarning("User {EditorId} attempted to edit message {MessageId} not sent by them (sender was {SenderId})", editParams.EditorId, editParams.MessageId, message.SenderId);
                return new EditMessageResult(false, new UnauthorizedAccessException("User is not authorized to edit this message."), null);
            }

            /*if (message.SentAt < DateTime.UtcNow.AddMinutes(-15))
                throw new Exception("Edit time limit exceeded");*/
            
            var originalMessageForNotification = new Message
            {
                Id = message.Id,
                SenderId = message.SenderId,
                RecipientId = message.RecipientId,
                RecipientType = message.RecipientType,
                SentAt = message.SentAt,
                ReplyToMessageId = message.ReplyToMessageId,
                Reactions = message.Reactions,
                MediaAttachments = message.MediaAttachments,
                MessageViews = message.MessageViews,
            };

            message.EncryptedContent = editParams.NewContent;
            message.IsEdited = true;
            message.EditedAt = editParams.EditedAt;

            await _messagesRepository.UpdateAsync(message);
            _logger.LogInformation("Message {MessageId} edited successfully by user {EditorId}", editParams.MessageId, editParams.EditorId);
            return new EditMessageResult(true, default, originalMessageForNotification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error editing message {MessageId} by user {EditorId}", editParams.MessageId, editParams.EditorId);
            return new EditMessageResult(false, ex, default);
        }
    }

    public async Task<DeleteMessageResult> DeleteMessageAsync(DeleteMessage deleteParams)
    {
        try
        {
            var message = await _messagesRepository.FindByIdAsync(deleteParams.MessageId);

            if (message.SenderId != deleteParams.DeleterId)
            {
                // TODO: Allow group admins to delete messages in their groups?
                // if (message.RecipientType == RecipientType.Group) {
                //    bool isAdmin = await _groupsRepository.IsUserAdminOfGroupAsync(deleteParams.DeleterId, message.RecipientId);
                //    if (!isAdmin) {
                //        _logger.LogWarning("User {DeleterId} (not sender or admin) attempted to delete group message {MessageId}", deleteParams.DeleterId, deleteParams.MessageId);
                //        return new DeleteMessageResult(false, new UnauthorizedAccessException("User is not authorized to delete this message."), null);
                //    }
                // } else {
                _logger.LogWarning(
                    "User {DeleterId} attempted to delete message {MessageId} not sent by them (sender was {SenderId})",
                    deleteParams.DeleterId, deleteParams.MessageId, message.SenderId);
                return new DeleteMessageResult(false,
                    new UnauthorizedAccessException("User is not authorized to delete this message."), default);
                // }
            }

            var originalMessageForNotification = new Message
            {
                Id = message.Id,
                SenderId = message.SenderId,
                RecipientId = message.RecipientId,
                RecipientType = message.RecipientType,
            };

            await _messagesRepository.RemoveAsync(deleteParams.MessageId);

            _logger.LogInformation("Message {MessageId} deleted successfully by user {DeleterId}",
                deleteParams.MessageId, deleteParams.DeleterId);
            return new DeleteMessageResult(true, default, originalMessageForNotification);
        }
        catch (NotFoundByKeyException<Guid> ex)
        {
            return new DeleteMessageResult(false, new KeyNotFoundException("Message not found", ex), default);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting message {MessageId} by user {DeleterId}", deleteParams.MessageId, deleteParams.DeleterId);
            return new DeleteMessageResult(false, ex, default);
        }
    }
}