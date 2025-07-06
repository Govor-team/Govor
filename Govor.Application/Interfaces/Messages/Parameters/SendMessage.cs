using Govor.Core.Models;

namespace Govor.Application.Interfaces.Messages.Parameters;

public record SendMessage(
    string EncryptContent, 
    Guid? ReplyToMessageId,
    Guid RecipientId,
    RecipientType RecipientType,
    Guid FromUserId,
    DateTime SendAt, 
    IEnumerable<SendMedia> Media);