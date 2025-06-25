namespace Govor.Contracts.Requests;

public class CreateInvitationRequest
{
    public DateTime EndDate { get; set; }
    public int MaxParticipants { get; set; }
    public bool IsAdmin { get; set; }
    public string Description { get; set; }
}