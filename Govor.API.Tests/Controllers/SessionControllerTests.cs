using AutoMapper;
using Govor.API.Controllers;
using Govor.Application.Interfaces.Infrastructure.Extensions;
using Govor.Application.Interfaces.UserSession;
using Govor.Contracts.DTOs;
using Govor.Core.Models.Users;
using Govor.Data.Repositories.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Govor.API.Tests.Controllers;

[TestFixture]
[TestOf(typeof(SessionController))]
public class SessionControllerTests
{
    private Mock<ILogger<SessionController>> _loggerMock;
    private Mock<ICurrentUserService> _currentUserServiceMock;
    private Mock<ICurrentUserSessionService> _currentUserSessionServiceMock;
    private Mock<IUserSessionReader> _userSessionReaderMock;
    private Mock<IUserSessionRevoker> _userSessionRevokerMock;
    private Mock<IMapper> _mapperMock;
    private SessionController _controller;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<SessionController>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _currentUserSessionServiceMock = new Mock<ICurrentUserSessionService>();
        _userSessionReaderMock = new Mock<IUserSessionReader>();
        _userSessionRevokerMock = new Mock<IUserSessionRevoker>();
        _mapperMock = new Mock<IMapper>();
        _controller = new SessionController(
            _loggerMock.Object,
            _currentUserServiceMock.Object,
            _currentUserSessionServiceMock.Object,
            _userSessionReaderMock.Object,
            _userSessionRevokerMock.Object,
            _mapperMock.Object);
    }

    #region GetAllSessions
    [Test]
    public async Task GetAllSessions_Successful_ReturnsOkWithMappedSessions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessions = new List<UserSession> { new UserSession { Id = Guid.NewGuid(), UserId = userId } };
        var sessionDtos = new List<SessionDto> { new SessionDto { Id = sessions[0].Id } };
        _currentUserServiceMock.Setup(s => s.GetCurrentUserId()).Returns(userId);
        _userSessionReaderMock.Setup(r => r.GetAllSessionsAsync(userId)).ReturnsAsync(sessions);
        _mapperMock.Setup(m => m.Map<List<SessionDto>>(sessions)).Returns(sessionDtos);

        // Act
        var result = await _controller.GetAllSessions();

        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult.Value, Is.EqualTo(sessionDtos));
        _loggerMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task GetAllSessions_UnauthorizedAccess_ReturnsForbid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exception = new UnauthorizedAccessException("Unauthorized");
        _currentUserServiceMock.Setup(s => s.GetCurrentUserId()).Returns(userId);
        _userSessionReaderMock.Setup(r => r.GetAllSessionsAsync(userId)).ThrowsAsync(exception);

        // Act
        var result = await _controller.GetAllSessions();

        // Assert
        Assert.That(result, Is.TypeOf<ForbidResult>());
        _loggerMock.Verify(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            exception,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once());
    }

    [Test]
    public async Task GetAllSessions_UnexpectedError_Returns500()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exception = new Exception("Unexpected error");
        _currentUserServiceMock.Setup(s => s.GetCurrentUserId()).Returns(userId);
        _userSessionReaderMock.Setup(r => r.GetAllSessionsAsync(userId)).ThrowsAsync(exception);

        // Act
        var result = await _controller.GetAllSessions();

        // Assert
        Assert.That(result, Is.TypeOf<ObjectResult>());
        var statusResult = result as ObjectResult;
        Assert.That(statusResult.StatusCode, Is.EqualTo(500));
        Assert.That(statusResult.Value, Is.EqualTo("Unexpected Error! Please try again later."));
        _loggerMock.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            exception,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once());
    }
    #endregion

    #region CloseSession
    [Test]
    public async Task CloseSession_ValidSessionId_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.GetCurrentUserId()).Returns(userId);

        // Act
        var result = await _controller.CloseSession(sessionId);

        // Assert
        Assert.That(result, Is.TypeOf<OkResult>());
        _userSessionRevokerMock.Verify(r => r.CloseSessionByIdAsync(sessionId, userId), Times.Once());
        _loggerMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task CloseSession_EmptySessionId_ReturnsBadRequest()
    {
        // Arrange
        var sessionId = Guid.Empty;

        // Act
        var result = await _controller.CloseSession(sessionId);

        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult.Value, Is.EqualTo("Invalid sessionId."));
        _userSessionRevokerMock.Verify(r => r.CloseSessionByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never());
        _loggerMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task CloseSession_UnauthorizedAccess_ReturnsForbid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var exception = new UnauthorizedAccessException("Unauthorized");
        _currentUserServiceMock.Setup(s => s.GetCurrentUserId()).Returns(userId);
        _userSessionRevokerMock.Setup(r => r.CloseSessionByIdAsync(sessionId, userId)).ThrowsAsync(exception);

        // Act
        var result = await _controller.CloseSession(sessionId);

        // Assert
        Assert.That(result, Is.TypeOf<ForbidResult>());
        _loggerMock.Verify(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            exception,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once());
    }

    [Test]
    public async Task CloseSession_NotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var exception = new NotFoundException("Session not found");
        _currentUserServiceMock.Setup(s => s.GetCurrentUserId()).Returns(userId);
        _userSessionRevokerMock.Setup(r => r.CloseSessionByIdAsync(sessionId, userId)).ThrowsAsync(exception);

        // Act
        var result = await _controller.CloseSession(sessionId);

        // Assert
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult.Value, Is.EqualTo("Session not found"));
        _loggerMock.Verify(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            exception,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once());
    }

    [Test]
    public async Task CloseSession_InvalidOperation_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var exception = new InvalidOperationException("Invalid operation");
        _currentUserServiceMock.Setup(s => s.GetCurrentUserId()).Returns(userId);
        _userSessionRevokerMock.Setup(r => r.CloseSessionByIdAsync(sessionId, userId)).ThrowsAsync(exception);

        // Act
        var result = await _controller.CloseSession(sessionId);

        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult.Value, Is.EqualTo("Invalid operation"));
        _loggerMock.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            exception,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once());
    }
    #endregion

    #region CloseCurrentSession
    [Test]
    public async Task CloseCurrentSession_ValidSessionIdAndUserId_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.GetCurrentUserId()).Returns(userId);
        _currentUserSessionServiceMock.Setup(s => s.GetUserSessionId()).Returns(sessionId);

        // Act
        var result = await _controller.CloseCurrent();

        // Assert
        Assert.That(result, Is.TypeOf<OkResult>());
        _userSessionRevokerMock.Verify(r => r.CloseSessionByIdAsync(sessionId, userId), Times.Once());
        _loggerMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task CloseCurrentSession_UnauthorizedAccess_ReturnsForbid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var exception = new UnauthorizedAccessException("Unauthorized");
        _currentUserServiceMock.Setup(s => s.GetCurrentUserId()).Returns(userId);
        _currentUserSessionServiceMock.Setup(s => s.GetUserSessionId()).Returns(sessionId);
        _userSessionRevokerMock.Setup(r => r.CloseSessionByIdAsync(sessionId, userId)).ThrowsAsync(exception);

        // Act
        var result = await _controller.CloseCurrent();

        // Assert
        Assert.That(result, Is.TypeOf<ForbidResult>());
        _loggerMock.Verify(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            exception,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once());
    }

    [Test]
    public async Task CloseCurrentSession_NotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var exception = new NotFoundException("Session not found");
        _currentUserServiceMock.Setup(s => s.GetCurrentUserId()).Returns(userId);
        _currentUserSessionServiceMock.Setup(s => s.GetUserSessionId()).Returns(sessionId);
        _userSessionRevokerMock.Setup(r => r.CloseSessionByIdAsync(sessionId, userId)).ThrowsAsync(exception);

        // Act
        var result = await _controller.CloseCurrent();

        // Assert
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult.Value, Is.EqualTo("Session not found"));
        _loggerMock.Verify(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            exception,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once());
    }

    [Test]
    public async Task CloseCurrentSession_InvalidOperation_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var exception = new InvalidOperationException("Invalid operation");
        _currentUserServiceMock.Setup(s => s.GetCurrentUserId()).Returns(userId);
        _currentUserSessionServiceMock.Setup(s => s.GetUserSessionId()).Returns(sessionId);
        _userSessionRevokerMock.Setup(r => r.CloseSessionByIdAsync(sessionId, userId)).ThrowsAsync(exception);

        // Act
        var result = await _controller.CloseCurrent();

        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult.Value, Is.EqualTo("Invalid operation"));
        _loggerMock.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            exception,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once());
    }
    #endregion

    #region CloseAllSessions
    [Test]
    public async Task CloseAllSessions_Successful_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.GetCurrentUserId()).Returns(userId);

        // Act
        var result = await _controller.CloseAllSessions();

        // Assert
        Assert.That(result, Is.TypeOf<OkResult>());
        _userSessionRevokerMock.Verify(r => r.CloseAllSessionsAsync(userId), Times.Once());
        _loggerMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task CloseAllSessions_UnauthorizedAccess_ReturnsForbid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exception = new UnauthorizedAccessException("Unauthorized");
        _currentUserServiceMock.Setup(s => s.GetCurrentUserId()).Returns(userId);
        _userSessionRevokerMock.Setup(r => r.CloseAllSessionsAsync(userId)).ThrowsAsync(exception);

        // Act
        var result = await _controller.CloseAllSessions();

        // Assert
        Assert.That(result, Is.TypeOf<ForbidResult>());
        _loggerMock.Verify(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            exception,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once());
    }

    [Test]
    public async Task CloseAllSessions_UnexpectedError_Returns500()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exception = new Exception("Unexpected error");
        _currentUserServiceMock.Setup(s => s.GetCurrentUserId()).Returns(userId);
        _userSessionRevokerMock.Setup(r => r.CloseAllSessionsAsync(userId)).ThrowsAsync(exception);

        // Act
        var result = await _controller.CloseAllSessions();

        // Assert
        Assert.That(result, Is.TypeOf<ObjectResult>());
        var statusResult = result as ObjectResult;
        Assert.That(statusResult.StatusCode, Is.EqualTo(500));
        Assert.That(statusResult.Value, Is.EqualTo("Unexpected Error! Please try again later."));
        _loggerMock.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            exception,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once());
    }
    #endregion

    [TearDown]
    public void TearDown()
    {
        _controller?.Dispose();
    }
}