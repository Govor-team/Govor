namespace Govor.Application.Profiles;

public class UserProfile
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? IconId { get; set; }
}
