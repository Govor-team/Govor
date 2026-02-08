using Microsoft.AspNetCore.SignalR;

namespace Govor.API.Common.SignalR.Helpers;

public class HubUserAccessor : IHubUserAccessor
{
    private readonly ILogger<HubUserAccessor> _logger;

    public HubUserAccessor(ILogger<HubUserAccessor> logger)
    {
        _logger = logger;
    }

    public Guid GetUserId(HubCallerContext context, bool suppressException = false)
    {
        var userIdClaim = context.User?.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            if (!suppressException)
            {
                _logger.LogError("Could not retrieve sender userId. Claim was: {UserIDClaim}", userIdClaim);
                throw new UnauthorizedAccessException("userID claim is missing or invalid.");
            }

            return Guid.Empty;
        }

        return userId;
    }
}