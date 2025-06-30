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

    
    
    [TearDown]
    public void TearDown()
    {
        _controller.Dispose();
    }
}