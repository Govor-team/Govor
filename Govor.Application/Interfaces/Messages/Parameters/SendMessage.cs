using Govor.Core.Models; // Added for RecipientType

namespace Govor.Application.Interfaces.Messages.Parameters;

public record SendMessage(
    string EncryptContent, 
    Guid? ReplyToMessageId,
    Guid RecipientId,
    RecipientType RecipientType, // Added this field
    Guid FromUserId,
    DateTime SendAt, 
    IEnumerable<SendMedia> Media);