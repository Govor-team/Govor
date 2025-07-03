namespace Govor.Application.Interfaces.Messages.Parameters;

public record SendMessage(
    string EncryptContent, 
    Guid? ReplyToMessageId,
    Guid RecipientId,
    Guid FromUserId,
    DateTime SendAt, 
    IEnumerable<SendMedia> Media);