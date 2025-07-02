using Govor.API.Controllers;
using Govor.Application.Interfaces;
using Govor.Application.Interfaces.Infrastructure.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Govor.API.Tests.IntegrationTests.Controllers;

[TestFixture]
public class OnlinePingingControllerTests
{
    private Mock<ILogger<OnlinePingingController>> _loggerMock;
    private Mock<IPingHandlerService> _pingHandlerServiceMock;
    private Mock<ICurrentUserService> _currentUserServiceMock;
    private OnlinePingingController _controller;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<OnlinePingingController>>();
        _pingHandlerServiceMock = new Mock<IPingHandlerService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _controller = new OnlinePingingController(_loggerMock.Object, _pingHandlerServiceMock.Object,
            _currentUserServiceMock.Object);
    }

    [Test]
    public async Task Ping_ValidUserId_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
        _pingHandlerServiceMock.Setup(x => x.Ping(userId)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Ping();

        // Assert
        Assert.That(result, Is.InstanceOf<OkResult>());
        _pingHandlerServiceMock.Verify(x => x.Ping(userId), Times.Once());
        _loggerMock.VerifyLog(LogLevel.Information, $"Ping from user {userId} processed successfully", Times.Once());
    }

    [Test]
    public async Task Ping_InvalidOperationException_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exception = new InvalidOperationException("User not found");
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
        _pingHandlerServiceMock.Setup(x => x.Ping(userId)).ThrowsAsync(exception);

        // Act
        var result = await _controller.Ping();

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequestResult = (BadRequestObjectResult)result;
        Assert.That(badRequestResult.Value, Is.EqualTo("User can't be found in our database."));
        _loggerMock.VerifyLog(LogLevel.Error, exception.Message, Times.Once());
    }

    [Test]
    public async Task Ping_UnauthorizedAccessException_ReturnsForbid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exception = new UnauthorizedAccessException("Unauthorized");
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
        _pingHandlerServiceMock.Setup(x => x.Ping(userId)).ThrowsAsync(exception);

        // Act
        var result = await _controller.Ping();

        // Assert
        Assert.That(result, Is.InstanceOf<ForbidResult>());
        _loggerMock.VerifyLog(LogLevel.Error, exception.Message, Times.Once());
    }

    [Test]
    public async Task Ping_GeneralException_ReturnsStatusCode500()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exception = new Exception("Unexpected error");
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Returns(userId);
        _pingHandlerServiceMock.Setup(x => x.Ping(userId)).ThrowsAsync(exception);

        // Act
        var result = await _controller.Ping();

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(500));
        _loggerMock.VerifyLog(LogLevel.Error, exception.Message, Times.Once());
    }

    [Test]
    public async Task Ping_UnauthorizedUserId_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var exception = new UnauthorizedAccessException("userID claim is missing or invalid");
        _currentUserServiceMock.Setup(x => x.GetCurrentUserId()).Throws(exception);

        // Act
        var result = await _controller.Ping();

        // Assert
        Assert.That(result, Is.InstanceOf<ForbidResult>());
        _loggerMock.VerifyLog(LogLevel.Error, exception.Message, Times.Once());
        _pingHandlerServiceMock.Verify(x => x.Ping(It.IsAny<Guid>()), Times.Never());
    }
}

// Helper extension for verifying logger calls
public static class LoggerMockExtensions
{
    public static void VerifyLog<T>(this Mock<ILogger<T>> logger, LogLevel level, string message, Times times)
    {
        logger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == level),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            times);
    }
}
