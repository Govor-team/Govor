namespace Govor.Application.Interfaces.Infrastructure.Extensions;

public interface ICurrentUserService
{
    Guid GetCurrentUserId();
}