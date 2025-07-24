using AutoFixture;
using Govor.API.Common.SignalR.Helpers;
using Govor.API.Hubs;
using Govor.Application.Interfaces.UserOnlineStatus;
using Govor.Core.Models.Users;
using Govor.Core.Repositories.Users;
using Microsoft.AspNetCore.SignalR;
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
    private Mock<IUsersRepository> _mockUserRepository;
    private Mock<IHubCallerClients> _clientsMock = null!;
    private Mock<IClientProxy> _clientProxyMock = null!;
    private Mock<HubCallerContext> _mockContext = null!;
    private Mock<IGroupManager> _groupManagerMock = null!;
    private Fixture _fixture;
    private PresenceHub _hub;
    private Guid _userId;
    
    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        
        _mockLogger = new Mock<ILogger<PresenceHub>>();
        _mockUserNotification = new Mock<IUserNotificationScopeService>();
        _mockOnlineUserStore = new Mock<IOnlineUserStore>();
        _mockHubUserAccessor = new Mock<IHubUserAccessor>();
        _mockUserRepository = new Mock<IUsersRepository>();
        
        _clientsMock = new Mock<IHubCallerClients>();
        _clientProxyMock = new Mock<IClientProxy>();
        
        _userId = Guid.NewGuid();
        _mockHubUserAccessor.Setup(h => h.GetUserId(
            It.IsAny<HubCallerContext>(),
            It.IsAny<bool>()))
            .Returns(_userId);
        
        
        _clientsMock.Setup(c => c.Group(It.IsAny<string>())).Returns(_clientProxyMock.Object);
        _mockContext = new Mock<HubCallerContext>();
        _mockContext.Setup(c => c.ConnectionId).Returns("test-connection");
        
        _groupManagerMock = new Mock<IGroupManager>();
        
        _groupManagerMock.Setup(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default))
            .Returns(Task.CompletedTask);
        
        _hub = new PresenceHub(
            _mockLogger.Object,
            _mockUserNotification.Object,
            _mockOnlineUserStore.Object,
            _mockHubUserAccessor.Object,
            _mockUserRepository.Object)
        {
            Clients = _clientsMock.Object,
            Context = _mockContext.Object,
            Groups = _groupManagerMock.Object,
        };
    }
    
    // Tests for OnConnectedAsync action
    [Test]
    public async Task OnConnectedAsync_SetsUserOnlineAndNotifiesFriends()
    {
        // Arrange
        _mockUserRepository.Setup(f => f.ExistsByIdAsync(_userId))
            .ReturnsAsync(true);

        var friendsIds = _fixture.CreateMany<Guid>().ToList();
        
        _mockUserNotification.Setup(n => n.GetNotifiedUsers(_userId))
            .ReturnsAsync(friendsIds);
        
        // Act
        await _hub.OnConnectedAsync();

        // Assert
        _mockOnlineUserStore.Verify(store => store.SetOnlineUser(_userId), Times.Once);
        
        _clientsMock.Verify(c => c.Group(It.IsAny<string>()), Times.Exactly(friendsIds.Count));
        
        foreach (var friendId in friendsIds)
        {
            _clientProxyMock.Verify(proxy =>
                    proxy.SendCoreAsync("UserOnline",
                        It.Is<object[]>(args => args.Length == 1 && (Guid)args[0] == _userId),
                        It.IsAny<CancellationToken>()),
                Times.Exactly(friendsIds.Count)); 
        }
    }
    
    [Test]
    public async Task OnConnectedAsync_UserIdInvalidOrNotExists_AbortsConnection()
    {
        // Arrange
        _mockHubUserAccessor.Setup(h => h.GetUserId(It.IsAny<HubCallerContext>(), false))
            .Returns(Guid.Empty);

        // Act
        await _hub.OnConnectedAsync();

        // Assert
        _mockOnlineUserStore.Verify(store => store.SetOnlineUser(It.IsAny<Guid>()), Times.Never);
        _mockContext.Verify(c => c.Abort(), Times.Once);
    }
    
    [Test]
    public async Task OnConnectedAsync_UserNotExists_AbortsConnection()
    {
        // Arrange
        _mockUserRepository.Setup(f => f.ExistsByIdAsync(_userId))
            .ReturnsAsync(false);
        
        // Act
        await _hub.OnConnectedAsync();

        // Assert
        _mockOnlineUserStore.Verify(store => store.SetOnlineUser(It.IsAny<Guid>()), Times.Never);
        _mockContext.Verify(c => c.Abort(), Times.Once);
    }
    
    // Tests for OnDisconnectedAsync action
    [Test]
    public async Task OnDisconnectedAsync_SetsUserOffline_UpdatesWasOnline_NotifiesFriends()
    {
        // Arrange 
        var user = new User { Id = _userId, WasOnline = DateTime.MinValue };

        var friendsIds = _fixture.CreateMany<Guid>().ToList();
        _mockUserRepository.Setup(r => r.FindByIdAsync(_userId))
            .ReturnsAsync(user);

        _mockUserNotification.Setup(n => n.GetNotifiedUsers(_userId))
            .ReturnsAsync(friendsIds);
        
        // Act 
        await _hub.OnDisconnectedAsync(null!);
       
        // Assert 
        _mockOnlineUserStore.Verify(store => store.SetOfflineUser(_userId), Times.Once);
        _mockUserRepository.Verify(r => r.UpdateAsync(It.Is<User>(u => u.Id == _userId && u.WasOnline > DateTime.MinValue)), Times.Once);
        _clientsMock.Verify(c => c.Group(It.IsAny<string>()), Times.AtLeastOnce);
        
        foreach (var friendId in friendsIds)
        {
            _clientProxyMock.Verify(proxy =>
                    proxy.SendCoreAsync("UserOffline",
                        It.Is<object[]>(args => args.Length == 1 && (Guid)args[0] == _userId),
                        It.IsAny<CancellationToken>()),
                Times.Exactly(friendsIds.Count)); 
        }
    }
    
    [Test]
    public async Task OnDisconnectedAsync_UserIdEmpty_LogsAndSkips()
    {
        // Arrange
        _mockHubUserAccessor.Setup(h => h.GetUserId(It.IsAny<HubCallerContext>(), true))
            .Returns(Guid.Empty);
        
        // Act
        await _hub.OnDisconnectedAsync(null!);

        // Assert
        _mockOnlineUserStore.Verify(s => s.SetOfflineUser(It.IsAny<Guid>()), Times.Never);
        _mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        
        _clientProxyMock.Verify(proxy =>
                proxy.SendCoreAsync("UserOffline",
                    It.Is<object[]>(args => args.Length == 1 && (Guid)args[0] == _userId),
                    It.IsAny<CancellationToken>()),
            Times.Never); 
    }
}