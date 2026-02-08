using Microsoft.AspNetCore.SignalR;

namespace Govor.API.Common.SignalR.Helpers;

public interface IHubUserAccessor
{
    Guid GetUserId(HubCallerContext context, bool suppressException = false);
}
