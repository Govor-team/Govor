using Govor.Core.Models;

namespace Govor.Application.Interfaces.Messages.Parameters;

public record SendMessage(
    string EncryptContent, 
    Guid? ReplyToMessageId,
    RecipientType RecipientType,
    Guid RecipientId,
    Guid FromUserId,
    DateTime SendAt, 
    IEnumerable<SendMedia> Media);