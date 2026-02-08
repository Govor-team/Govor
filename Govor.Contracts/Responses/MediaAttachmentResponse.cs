namespace Govor.Contracts.Responses;

public class MediaAttachmentResponse
{
    public Guid Id { get; set; }
    public Guid MessageId { get; set; }
    public Guid MediaFileId { get; set; }
}