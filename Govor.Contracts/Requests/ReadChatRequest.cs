using System.ComponentModel.DataAnnotations;
using Govor.Domain.Models.Messages;

namespace Govor.Contracts.Requests;

public class ReadChatRequest
{
    [EnumDataType(typeof(RecipientType))]
    public RecipientType RecipientType { get; set; }

    // Visible messages, or an inclusive boundary. Neither means the current chat snapshot.
    [MaxLength(100)]
    public List<Guid>? MessageIds { get; set; }
    public Guid? UpToMessageId { get; set; }
}
