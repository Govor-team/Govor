using AutoMapper;
using Govor.API.Controllers;
using Govor.Application.Interfaces;
using Govor.Application.Interfaces.Infrastructure.Extensions;
using Govor.Application.Interfaces.Medias;
using Govor.Application.Profiles;
using Govor.Contracts.DTOs;
using Govor.Data.Repositories.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Govor.API.Tests;

[TestFixture]
public class ProfileControllerTests
{
    private Mock<ILogger<ProfileController>> _mockLogger = null!;
    private Mock<IMediaService> _mockMediaService = null!;
    private Mock<IProfileService> _mockProfileService = null!;
    private Mock<ICurrentUserService> _mockCurrentUserService = null!;
    private Mock<IMapper> _mockMapper = null!;
    private ProfileController _controller = null!;

    private Guid _userId;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<ProfileController>>();
        _mockMediaService = new Mock<IMediaService>();
        _mockProfileService = new Mock<IProfileService>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockMapper = new Mock<IMapper>();

        _controller = new ProfileController(
            _mockLogger.Object,
            _mockMediaService.Object,
            _mockProfileService.Object,
            _mockCurrentUserService.Object,
            _mockMapper.Object);

        _userId = Guid.NewGuid();
        _mockCurrentUserService.Setup(x => x.GetCurrentUserId()).Returns(_userId);
    }

    [Test]
    public async Task DowloadProfile_ReturnsOk_WhenProfileExists()
    {
        // Arrange
        var userProfile = new UserProfile
        {
            Id = _userId,
            Username = "test_user",
            Description = "test description",
            IconId = Guid.NewGuid()
        };

        var userProfileDto = new UserProfileDto
        {
            Id = _userId,
            Username = "test_user",
            Description = "test description",
            IconId = userProfile.IconId
        };

        _mockProfileService.Setup(s => s.GetUserProfileAsync(_userId))
            .ReturnsAsync(userProfile);

        _mockMapper.Setup(m => m.Map<UserProfileDto>(userProfile))
            .Returns(userProfileDto);

        // Act
        var result = await _controller.DowloadProfile();

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.StatusCode, Is.EqualTo(200));
        Assert.That(okResult.Value, Is.EqualTo(userProfileDto));
    }

    [Test]
    public async Task DowloadProfile_ReturnsNotFound_WhenProfileIsNull()
    {
        // Arrange
        _mockProfileService.Setup(s => s.GetUserProfileAsync(_userId))
            .ReturnsAsync((UserProfile?)null);

        // Act
        var result = await _controller.DowloadProfile();

        // Assert
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
        Assert.That(notFoundResult!.Value, Is.EqualTo("Profile not found"));
    }

    [Test]
    public async Task DowloadProfile_ReturnsForbid_WhenUnauthorizedAccessExceptionThrown()
    {
        // Arrange
        _mockProfileService.Setup(s => s.GetUserProfileAsync(_userId))
            .ThrowsAsync(new UnauthorizedAccessException("Access denied"));

        // Act
        var result = await _controller.DowloadProfile();

        // Assert
        Assert.That(result, Is.InstanceOf<ForbidResult>());
    }

    [Test]
    public async Task DowloadProfile_ReturnsBadRequest_WhenNotFoundExceptionThrown()
    {
        // Arrange
        _mockProfileService.Setup(s => s.GetUserProfileAsync(_userId))
            .ThrowsAsync(new NotFoundException("User not found"));

        // Act
        var result = await _controller.DowloadProfile();

        // Assert
        var badRequest = result as BadRequestObjectResult;
        Assert.That(badRequest, Is.Not.Null);
        Assert.That(badRequest!.Value, Is.EqualTo("User not found"));
    }

    [Test]
    public async Task DowloadProfile_Returns500_WhenUnexpectedExceptionThrown()
    {
        // Arrange
        _mockProfileService.Setup(s => s.GetUserProfileAsync(_userId))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.DowloadProfile();

        // Assert
        var objectResult = result as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
        Assert.That(objectResult.Value, Is.EqualTo("Unexpected Error! Please try again later."));
    }
}