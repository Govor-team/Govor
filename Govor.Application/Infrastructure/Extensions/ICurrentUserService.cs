namespace Govor.Application.Infrastructure.Extensions;

public interface ICurrentUserService
{
    Guid GetCurrentUserId();
}