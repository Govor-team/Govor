using AutoFixture;
using Govor.API.Common.SignalR.Helpers;
using Govor.API.Hubs;
using Govor.Application.Interfaces.UserOnlineStatus;
using Microsoft.Extensions.Logging;
using Moq;

namespace Govor.API.Tests.Hubs;

[TestFixture]
[TestOf(typeof(PresenceHub))]
public class PresenceHubTests
{

    private Mock<ILogger<PresenceHub>> _mockLogger;
    private Mock<IUserNotificationScopeService> _mockUserNotification;
    private Mock<IOnlineUserStore> _mockOnlineUserStore;
    private Mock<IHubUserAccessor> _mockHubUserAccessor;
    private Fixture _fixture;
    
    [SetUp]
    public void SetUp()
    {
        
    }
}