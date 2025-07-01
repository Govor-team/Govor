using AutoFixture;
using Govor.API.Controllers;
using Govor.Application.Exceptions.FriendsService;
using Govor.Application.Interfaces;
using Govor.Application.Interfaces.Infrastructure.Extensions;
using Govor.Contracts.DTOs;
using Govor.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Govor.API.Tests.IntegrationTests.Controllers;

[TestFixture]
public class FriendsControllerTests
{
    private Fixture _fixture;
    private Mock<ILogger<FriendsController>> _loggerMock;
    private Mock<IFriendsService> _friendsServiceMock;
    private Mock<ICurrentUserService> _currentUserServiceMock;
    private FriendsController _controller;
    
    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        
        _loggerMock = new Mock<ILogger<FriendsController>>();
        _friendsServiceMock = new Mock<IFriendsService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        
        _controller = new FriendsController(
            _loggerMock.Object,
            _friendsServiceMock.Object,
            _currentUserServiceMock.Object
        );
    }
    
    // Tests for Search action 
    [Test]
    public async Task Search_ValidRequest_ReturnsOkResult()
    {
        var users = _fixture.CreateMany<User>().ToList();
        var userId = _fixture.Create<Guid>();
        var query = _fixture.Create<string>();
        
        _currentUserServiceMock.Setup(c => c.GetCurrentUserId()).Returns(userId);
        
        _friendsServiceMock.Setup(f => f.SearchUsersAsync(query, userId))
            .ReturnsAsync(users);
        
        // Act 
        var result = await _controller.Search(query);
        
        var okResult = result as OkObjectResult;
        dynamic value = okResult.Value;
        
        List<UserDto> userDtos = value as List<UserDto>;
        
        // Assert 
        Assert.That(value, Is.Not.Null);
        Assert.That(value.Count, Is.EqualTo(users.Count));
        Assert.That(userDtos.Select(u => u.Id), Is.EqualTo(users.Select(u => u.Id)));
    }

    [Test]
    public async Task Search_InvalidQuery_BadRequest()
    {
        // Act 
        var result = await _controller.Search(string.Empty);
        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult.Value, Is.EqualTo("Query cannot be empty"));
    }
    
    
    [Test]
    public async Task Search_NotFound_IfThrowsSearchUsersException()
    {
        // Arrange 
        _friendsServiceMock.Setup(f => f.SearchUsersAsync(It.IsAny<string>(), It.IsAny<Guid>()))
            .ThrowsAsync(new SearchUsersException(_fixture.Create<string>()));
        
        // Act 
        var result = await _controller.Search(_fixture.Create<string>());
        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }
    
    [Test]
    public async Task Search_StatusCode500_IfThrowsSomeException()
    {
        // Arrange 
        _friendsServiceMock.Setup(f => f.SearchUsersAsync(It.IsAny<string>(), It.IsAny<Guid>()))
            .ThrowsAsync(new Exception(_fixture.Create<string>()));
        
        // Act 
        var result = await _controller.Search(_fixture.Create<string>());
        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = result as ObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(500));
    }
    
    // Test for SendRequest action
    [Test]
    public async Task SendRequest_ValidRequest_ReturnsOk()
    {
        var targetUserId = Guid.NewGuid();

        var result = await _controller.SendRequest(targetUserId);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult?.Value.ToString(), Does.Contain("Friend request sent"));
    }

    [Test]
    public async Task SendRequest_Throws_InvalidOperationException_ReturnsUnprocessableEntity()
    {
        var targetUserId = Guid.NewGuid();
        
        _currentUserServiceMock.Setup(c => c.GetCurrentUserId()).Returns(targetUserId);
        
        _friendsServiceMock.Setup(f => f.SendFriendRequestAsync(targetUserId, targetUserId))
            .ThrowsAsync(new InvalidOperationException());
        
        var result = await _controller.SendRequest(targetUserId);

        Assert.That(result, Is.InstanceOf<UnprocessableEntityObjectResult>());
    }
    
    [Test]
    public async Task SendRequest_Throws_RequestAlreadySentException_ReturnsConflict()
    {
        var targetUserId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        
        _currentUserServiceMock.Setup(c => c.GetCurrentUserId()).Returns(currentUserId);
        
        _friendsServiceMock.Setup(f => f.SendFriendRequestAsync(targetUserId, currentUserId))
            .ThrowsAsync(new RequestAlreadySentException(currentUserId, targetUserId));
        
        var result = await _controller.SendRequest(targetUserId);

        Assert.That(result, Is.InstanceOf<ConflictObjectResult>());
    }
    
    [Test]
    public async Task SendRequest_StatusCode500_IfThrowsSomeException()
    {
        // Arrange 
        _friendsServiceMock.Setup(f => f.SendFriendRequestAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ThrowsAsync(new Exception(_fixture.Create<string>()));
        
        // Act 
        var result = await _controller.Search(_fixture.Create<string>());
        
        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = result as ObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(500));
    }
    
    // Tests for GetIncomingRequests action 
    [Test]
    public async Task GetIncomingRequests_ValidRequest_ReturnsOkResult()
    {
        // Arrange
        var currentId = _fixture.Create<Guid>();
        var friendships = _fixture.CreateMany<Friendship>().ToList();
        
        _currentUserServiceMock.Setup(c => c.GetCurrentUserId())
            .Returns(currentId);
        
        _friendsServiceMock.Setup(f => f.GetIncomingRequestsAsync(currentId))
            .ReturnsAsync(friendships);
        // Act 
        var result = await _controller.GetIncomingRequests();
        
        // Assert 
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        List<FriendshipDto> value = okResult.Value as List<FriendshipDto>;
        Assert.That(value, Is.Not.Null);
        Assert.That(value.Count, Is.EqualTo(friendships.Count));
    }

    [Test]
    public async Task GetIncomingRequests_Throws_InvalidOperationException_ReturnsBadRequest()
    {
        // Arrange
        var currentId = _fixture.Create<Guid>();
        
        _currentUserServiceMock.Setup(c => c.GetCurrentUserId())
            .Returns(currentId);
        
        _friendsServiceMock.Setup(f => f.GetIncomingRequestsAsync(currentId))
            .ThrowsAsync(new InvalidOperationException());
        
        // Act 
        var result = await _controller.GetIncomingRequests();
        
        // Assert 
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequestResult = result as BadRequestObjectResult;
        dynamic value = badRequestResult.Value;
        Assert.That(value, Is.EqualTo("Failed to get friend requests. User data missing."));
    }

    [Test]
    public async Task GetIncomingRequest_StatusCode500_IfThrowsSomeException()
    {
        // Arrange
        var currentId = _fixture.Create<Guid>();
        
        _currentUserServiceMock.Setup(c => c.GetCurrentUserId())
            .Returns(currentId);
        
        _friendsServiceMock.Setup(f => f.GetIncomingRequestsAsync(currentId))
            .ThrowsAsync(new Exception());
        
        // Act 
        var result = await _controller.GetIncomingRequests();
        
        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = result as ObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(500));
    }
    
    // Tests for AcceptFriend action 
    [Test]
    public async Task AcceptFriend_ValidRequest_ReturnsOkResult()
    {
        // Arrange 
        var currentId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        
        _currentUserServiceMock.Setup(c => c.GetCurrentUserId())
            .Returns(currentId);
        // Act 
        var result = await _controller.AcceptFriend(targetUserId);
        
        // Assert 
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }
    
    [Test]
    public async Task AcceptFriend_InvalidOperationException_ReturnsNotFound()
    {
        // Arrange 
        var currentId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        
        _currentUserServiceMock.Setup(c => c.GetCurrentUserId())
            .Returns(currentId);
        
        _friendsServiceMock.Setup(f => f.AcceptFriendRequestAsync(targetUserId, currentId))
            .ThrowsAsync(new InvalidOperationException());
        
        // Act 
        var result = await _controller.AcceptFriend(targetUserId);
        
        // Assert 
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }
    [Test]
    public async Task AcceptFriend_UnauthorizedAccessException_ReturnsForbid()
    {
        // Arrange 
        var currentId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        
        _currentUserServiceMock.Setup(c => c.GetCurrentUserId())
            .Returns(currentId);
        
        _friendsServiceMock.Setup(f => f.AcceptFriendRequestAsync(targetUserId, currentId))
            .ThrowsAsync(new UnauthorizedAccessException());
        
        // Act 
        var result = await _controller.AcceptFriend(targetUserId);
        
        // Assert 
        Assert.That(result, Is.InstanceOf<ForbidResult>());
    }
    
    [Test]
    public async Task AcceptFriend_Exception_ReturnsStatusCode500()
    {
        // Arrange 
        var currentId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        
        _currentUserServiceMock.Setup(c => c.GetCurrentUserId())
            .Returns(currentId);
        
        _friendsServiceMock.Setup(f => f.AcceptFriendRequestAsync(targetUserId, currentId))
            .ThrowsAsync(new Exception());
        
        // Act 
        var result = await _controller.AcceptFriend(targetUserId);
        
        // Assert 
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = result as ObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(500));
    }
    
    // Tests for GetFriends action 
    [Test]
    public async Task GetFriends_ValidRequest_ReturnsOkResult()
    {
        // Arrange 
        var currentId = _fixture.Create<Guid>();
        var users = _fixture.CreateMany<User>().ToList();
        
        _currentUserServiceMock.Setup(c => c.GetCurrentUserId())
            .Returns(currentId);

        _friendsServiceMock.Setup(f => f.GetFriendsAsync(currentId))
            .ReturnsAsync(users);
        // Act 
        var result = await _controller.GetFriends();
        
        // Assert 
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        List<UserDto> value = okResult.Value as List<UserDto>;
        Assert.That(value, Is.Not.Null);
        Assert.That(value.Count, Is.EqualTo(users.Count));
    }
    
    [Test]
    public async Task GetFriends_InvalidOperationException_ReturnsBadRequest()
    {
        // Arrange 
        var currentId = _fixture.Create<Guid>();
        var users = _fixture.CreateMany<User>().ToList();
        
        _currentUserServiceMock.Setup(c => c.GetCurrentUserId())
            .Returns(currentId);

        _friendsServiceMock.Setup(f => f.GetFriendsAsync(currentId))
            .ThrowsAsync(new InvalidOperationException());
        
        // Act 
        var result = await _controller.GetFriends();
        
        // Assert 
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }
    
    [Test]
    public async Task GetFriends_Exception_ReturnsStatusCode500()
    {
        // Arrange 
        var currentId = _fixture.Create<Guid>();
        var users = _fixture.CreateMany<User>().ToList();
        
        _currentUserServiceMock.Setup(c => c.GetCurrentUserId())
            .Returns(currentId);

        _friendsServiceMock.Setup(f => f.GetFriendsAsync(currentId))
            .ThrowsAsync(new Exception());
        
        // Act 
        var result = await _controller.GetFriends();
        
        // Assert 
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = result as ObjectResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(500));
    }
    
    [TearDown]
    public void TearDown()
    {
        _controller.Dispose();
    }
}