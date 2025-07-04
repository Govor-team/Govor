using Govor.Application.Interfaces;
using Govor.Application.Interfaces.Messages;
using Govor.Application.Interfaces.Messages.Parameters;
using Govor.Core.Models;
using Govor.Core.Repositories.Messages;
using Govor.Data;
using Microsoft.EntityFrameworkCore;

namespace Govor.Application.Services;

public class PrivateChatService : IChatService
{
    private readonly IVerifyFriendship _verifyFriendship;
    private readonly IMessagesRepository _messages;
    
    public PrivateChatService(IMessagesRepository messages, IVerifyFriendship verifyFriendship)
    {
        _messages = messages;
        _verifyFriendship = verifyFriendship;
    }
    
    public async Task<Result> SendMessageAsync(SendMessage newMessage)
    {
        try
        {
            await _verifyFriendship.VerifyAsync(newMessage.FromUserId, newMessage.RecipientId);

            var messageId = Guid.NewGuid();
            
            var message = new Message()
            {
                Id = messageId,
                EncryptedContent = newMessage.EncryptContent,
                IsEdited = false,
                SenderId = newMessage.FromUserId,
                RecipientId = newMessage.RecipientId,
                RecipientType = RecipientType.User,
                ReplyToMessageId = newMessage.ReplyToMessageId,
                
                MediaAttachments = newMessage.Media.Select(m => new MediaAttachments()
                {
                    Id = m.Id,
                    MessageId = messageId,
                    Type = m.Type,
                    MimeType = m.MimeType,
                    EncryptedKey = m.EncryptedKey,
                }).ToList()
            };
            
            await _messages.AddAsync(message);
            
            return new Result(true, null, messageId);
        }
        catch (Exception ex)
        {
            return new Result(false, ex, Guid.Empty);
        }
    }

    public Task<Result> EditMessageAsync(Guid editorId, Guid messageId, string newContent)
    {
        throw new NotImplementedException();
    }

    public Task<Result> DeleteMessageAsync(Guid editorId, Guid messageId)
    {
        throw new NotImplementedException();
    }
}