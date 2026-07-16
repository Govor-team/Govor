namespace Govor.Application.Infrastructure.Extensions;

public interface ICurrentUserSessionService
{
    Guid GetUserSessionId();
}