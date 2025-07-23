namespace Govor.Application.Interfaces;

public interface IUserPresenceService
{
    DateTime WhenUserWasOnline(Guid userId);
}