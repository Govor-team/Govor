namespace Govor.Application.Interfaces.Infrastructure.Extensions;

public interface ICurrentUserSessionService
{
    Guid GetUserSessionId();
}