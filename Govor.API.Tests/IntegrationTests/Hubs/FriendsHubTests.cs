using Govor.API.Hubs;
using Govor.Application.Exceptions.FriendsService;
using Govor.Application.Interfaces.Friends;
using Govor.Application.Interfaces.Infrastructure.Extensions;
using Govor.Contracts.Responses.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;

namespace Govor.API.Tests.IntegrationTests.Hubs;

[TestFixture]
public class FriendsHubTests
{
    private Mock<IFriendRequestCommandService> _friendRequestServiceMock = null!;
    private Mock<ICurrentUserService> _currentUserServiceMock = null!;
    private Mock<IHubCallerClients> _clientsMock = null!;
    private Mock<IClientProxy> _clientProxyMock = null!;
    private Mock<ILogger<FriendsHub>> _loggerMock = null!;
    private FriendsHub _hub = null!;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _targetUserId = Guid.NewGuid();

    [SetUp]
    public void Setup()
    {
        _friendRequestServiceMock = new Mock<IFriendRequestCommandService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _clientsMock = new Mock<IHubCallerClients>();
        _clientProxyMock = new Mock<IClientProxy>();
        _loggerMock = new Mock<ILogger<FriendsHub>>();

        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(_userId);
        _clientsMock.Setup(c => c.User(It.IsAny<string>())).Returns(_clientProxyMock.Object);

        _hub = new FriendsHub(
            _friendRequestServiceMock.Object,
            _currentUserServiceMock.Object,
            _loggerMock.Object)
        {
            Clients = _clientsMock.Object
        };
    }
    
    // Tests for SendRequest action 
    [Test]
    public async Task SendRequest_ShouldReturnCreated_WhenSuccess()
    {
        // Act
        var result = await _hub.SendRequest(_targetUserId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Status, Is.EqualTo(HubResultStatus.Created));
        
        _friendRequestServiceMock.Verify(s => s.SendAsync(_userId, _targetUserId), Times.Once);
        
        _clientProxyMock.Verify(c => c.SendCoreAsync(
            "FriendRequestReceived",
            It.Is<object[]>(o => o[0]!.Equals(_userId)),
            default), Times.Once);
    }
    
    [Test]
    public async Task SendRequest_ShouldReturnBadRequest_WhenInvalidOperation()
    {
        // Arrange 
        _friendRequestServiceMock
            .Setup(s => s.SendAsync(_userId, _targetUserId))
            .ThrowsAsync(new InvalidOperationException("Invalid"));

        // Act 
        var result = await _hub.SendRequest(_targetUserId);

        // Assert 
        Assert.That(result.Status, Is.EqualTo(HubResultStatus.BadRequest));
        Assert.That(result.ErrorMessage, Is.EqualTo("Invalid"));
    }
    
    [Test]
    public async Task SendRequest_ShouldReturnConflict_WhenAlreadySent()
    {
        // Arrange 
        _friendRequestServiceMock
            .Setup(s => s.SendAsync(_userId, _targetUserId))
            .ThrowsAsync(new RequestAlreadySentException(_userId, _targetUserId));
    
        // Act 
        var result = await _hub.SendRequest(_targetUserId);

        // Assert 
        Assert.That(result.Status, Is.EqualTo(HubResultStatus.Conflict));
        Assert.That(result.ErrorMessage, Is.EqualTo($"Request was already sent from {_userId} to {_targetUserId}"));
    }
    
    [Test]
    public async Task SendRequest_ShouldReturnUnauthorized_WhenCurrentUserIsNotAuthenticated()
    {
        // Arrange 
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId())
            .Throws(new UnauthorizedAccessException("userId claim is missing or invalid"));

        // Act 
        var result = await _hub.SendRequest(_targetUserId);

        // Assert 
        Assert.That(result.Status, Is.EqualTo(HubResultStatus.Unauthorized));
        Assert.That(result.ErrorMessage, Is.EqualTo("userId claim is missing or invalid"));
    }
    
    [Test]
    public async Task SendRequest_ShouldReturnError_WhenSomeExceptionOccurs()
    {
        // Arrange 
        _friendRequestServiceMock
            .Setup(s => s.SendAsync(_userId, _targetUserId))
            .ThrowsAsync(new Exception("error"));
    
        // Act 
        var result = await _hub.SendRequest(_targetUserId);

        // Assert 
        Assert.That(result.Status, Is.EqualTo(HubResultStatus.ServerError));
        Assert.That(result.ErrorMessage, Is.EqualTo("Unexpected error! Please try later!"));
    }
    
    // Test for AcceptRequest action
    [Test]
    public async Task AcceptRequest_ShouldReturnOk_WhenSuccess()
    {
        // Arrange 
        var friendshipId = Guid.NewGuid();
        
        // Act 
        var result = await _hub.AcceptRequest(friendshipId);

        // Assert 
        Assert.That(result.Status, Is.EqualTo(HubResultStatus.Success));
        _friendRequestServiceMock.Verify(s => s.AcceptAsync(friendshipId, _userId), Times.Once);
        
        _clientProxyMock.Verify(c => c.SendCoreAsync(
            "FriendRequestAccepted",
            It.Is<object[]>(o => o[0]!.Equals(friendshipId)),
            default), Times.Once);
    }
    
    [Test]
    public async Task AcceptRequest_ShouldReturnBadRequest_InvalidOperation()
    {
        // Arrange 
        var friendshipId = Guid.NewGuid();
        _friendRequestServiceMock
            .Setup(s => s.AcceptAsync(friendshipId, _userId))
            .ThrowsAsync(new InvalidOperationException("Invalid"));

        // Act 
        var result = await _hub.AcceptRequest(friendshipId);

        // Assert 
        Assert.That(result.Status, Is.EqualTo(HubResultStatus.BadRequest));
        Assert.That(result.ErrorMessage, Is.EqualTo("Invalid"));
    }
    
        
    [Test]
    public async Task AcceptRequest_ShouldReturnUnauthorized_UnauthorizedAccess()
    {
        // Arrange 
        var friendshipId = Guid.NewGuid();
        _friendRequestServiceMock
            .Setup(s => s.AcceptAsync(friendshipId, _userId))
            .ThrowsAsync(new UnauthorizedAccessException("userId claim is missing or invalid"));

        // Act 
        var result = await _hub.AcceptRequest(friendshipId);

        // Assert 
        Assert.That(result.Status, Is.EqualTo(HubResultStatus.Unauthorized));
        Assert.That(result.ErrorMessage, Is.EqualTo("userId claim is missing or invalid"));
    }
    
    [Test]
    public async Task AcceptRequest_ShouldReturnError_WhenSomeExceptionOccurs()
    {
        // Arrange 
        var friendshipId = Guid.NewGuid();
        _friendRequestServiceMock
            .Setup(s => s.AcceptAsync(friendshipId, _userId))
            .ThrowsAsync(new Exception("error"));

        // Act 
        var result = await _hub.AcceptRequest(friendshipId);

        // Assert 
        Assert.That(result.Status, Is.EqualTo(HubResultStatus.ServerError));
        Assert.That(result.ErrorMessage, Is.EqualTo("Unexpected error! Please try later!"));
    }
    
    // Test for RejectRequest action 
    [Test]
    public async Task RejectRequest_ShouldReturnOk_WhenSuccess()
    {
        // Arrange 
        var friendshipId = Guid.NewGuid();

        // Act 
        var result = await _hub.RejectRequest(friendshipId);

        // Assert 
        Assert.That(result.Status, Is.EqualTo(HubResultStatus.Success));
        _friendRequestServiceMock.Verify(s => s.RejectAsync(friendshipId, _userId), Times.Once);
        
        _clientProxyMock.Verify(c => c.SendCoreAsync(
            "FriendRequestRejected",
            It.Is<object[]>(o => o[0]!.Equals(friendshipId)),
            default), Times.Once);
    }
    
    [Test]
    public async Task RejectRequest_ShouldReturnBadRequest_InvalidOperation()
    {
        // Arrange 
        var friendshipId = Guid.NewGuid();
        _friendRequestServiceMock
            .Setup(s => s.RejectAsync(friendshipId, _userId))
            .ThrowsAsync(new InvalidOperationException("Invalid"));

        // Act 
        var result = await _hub.RejectRequest(friendshipId);

        // Assert 
        Assert.That(result.Status, Is.EqualTo(HubResultStatus.BadRequest));
        Assert.That(result.ErrorMessage, Is.EqualTo("Invalid"));
    }
    
        
    [Test]
    public async Task RejectRequest_ShouldReturnUnauthorized_UnauthorizedAccess()
    {
        // Arrange 
        var friendshipId = Guid.NewGuid();
        _friendRequestServiceMock
            .Setup(s => s.RejectAsync(friendshipId, _userId))
            .ThrowsAsync(new UnauthorizedAccessException("userId claim is missing or invalid"));

        // Act 
        var result = await _hub.RejectRequest(friendshipId);

        // Assert 
        Assert.That(result.Status, Is.EqualTo(HubResultStatus.Unauthorized));
        Assert.That(result.ErrorMessage, Is.EqualTo("userId claim is missing or invalid"));
    }
    
    [Test]
    public async Task RejectRequest_ShouldReturnError_WhenSomeExceptionOccurs()
    {
        // Arrange 
        var friendshipId = Guid.NewGuid();
        _friendRequestServiceMock
            .Setup(s => s.RejectAsync(friendshipId, _userId))
            .ThrowsAsync(new Exception("error"));

        // Act 
        var result = await _hub.RejectRequest(friendshipId);

        // Assert 
        Assert.That(result.Status, Is.EqualTo(HubResultStatus.ServerError));
        Assert.That(result.ErrorMessage, Is.EqualTo("Unexpected error! Please try later!"));
    }
}
