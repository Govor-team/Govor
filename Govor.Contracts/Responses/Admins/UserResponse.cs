namespace Govor.Contracts.Responses.Admins;

public class UserResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Description { get; set; }
    public string PasswordHash { get; set; }
    public DateTime WasOnline { get; set; }
    public DateOnly CreatedOn { get; set; }
    public Guid IconId {get; set;}
    public Guid InviteId {get; set;}
    public bool IsAdmin {get; set;}
}