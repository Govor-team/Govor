using Govor.Application.Interfaces.PushNotifications;
using Govor.Application.Interfaces.PushNotifications.Models;
using Govor.Application.PushNotifications;
using Govor.Application.Services.PushNotifications;
using Govor.Domain.Repositories.PushTokens;
using Microsoft.Extensions.Logging;
using Moq;

namespace Govor.Application.Tests.Services.PushNotifications;

[TestFixture]
public class PushNotificationServiceTests
{
    private Mock<IPushNotificationProvider> _providerMock;
    private Mock<IPushTokenRepository> _tokenRepoMock;
    private Mock<ILogger<PushNotificationService>> _loggerMock;

    private PushNotificationService _service;

    [SetUp]
    public void Setup()
    {
        _providerMock = new Mock<IPushNotificationProvider>(MockBehavior.Strict);
        _tokenRepoMock = new Mock<IPushTokenRepository>(MockBehavior.Strict);
        _loggerMock = new Mock<ILogger<PushNotificationService>>();

        _service = new PushNotificationService(
            _providerMock.Object,
            _tokenRepoMock.Object,
            _loggerMock.Object);
    }

    #region SendToUserAsync

    [Test]
    public async Task SendToUserAsync_NoTokens_DoesNotCallProvider()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _tokenRepoMock
            .Setup(r => r.GetActiveTokensAsync(userId))
            .ReturnsAsync(new List<string>());

        // Act
        await _service.SendToUserAsync(userId, "title", "body", "chat");

        // Assert
        _providerMock.Verify(
            p => p.SendMulticastAsync(It.IsAny<IReadOnlyList<string>>(), It.IsAny<PushMessage>()),
            Times.Never);
    }

    [Test]
    public async Task SendToUserAsync_SuccessfulSend_CallsProvider()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokens = new List<string> { "t1", "t2" };

        _tokenRepoMock
            .Setup(r => r.GetActiveTokensAsync(userId))
            .ReturnsAsync(tokens);

        _providerMock
            .Setup(p => p.SendMulticastAsync(tokens, It.IsAny<PushMessage>()))
            .ReturnsAsync(new SendPushResult(0, 0, []));

        // Act
        await _service.SendToUserAsync(userId, "title", "body", "chat");

        // Assert
        _providerMock.Verify(
            p => p.SendMulticastAsync(tokens, It.IsAny<PushMessage>()),
            Times.Once);

        _tokenRepoMock.Verify(
            r => r.RemoveTokensAsync(It.IsAny<IEnumerable<string>>()),
            Times.Never);
    }

    [Test]
    public async Task SendToUserAsync_WithFailures_RemovesInvalidTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokens = new List<string> { "t1", "t2" };
        var failed = new List<string> { "t2" };

        _tokenRepoMock
            .Setup(r => r.GetActiveTokensAsync(userId))
            .ReturnsAsync(tokens);

        _providerMock
            .Setup(p => p.SendMulticastAsync(tokens, It.IsAny<PushMessage>()))
            .ReturnsAsync(new SendPushResult(0, 1, failed));

        _tokenRepoMock
            .Setup(r => r.RemoveTokensAsync(failed))
            .Returns(Task.CompletedTask);

        // Act
        await _service.SendToUserAsync(userId, "title", "body", "chat");

        // Assert
        _tokenRepoMock.Verify(r => r.RemoveTokensAsync(failed), Times.Once);
    }

    #endregion

    #region SendToUsersAsync

    [Test]
    public async Task SendToUsersAsync_NoTokens_DoesNotCallProvider()
    {
        // Arrange
        var users = new[] { Guid.NewGuid() };

        _tokenRepoMock
            .Setup(r => r.GetActiveTokensUsersAsync(users))
            .ReturnsAsync(new List<string>());

        // Act
        await _service.SendToUsersAsync(users, "title", "body", "chat");

        // Assert
        _providerMock.Verify(
            p => p.SendMulticastAsync(It.IsAny<IReadOnlyList<string>>(), It.IsAny<PushMessage>()),
            Times.Never);
    }

    #endregion

    #region SendToSessionAsync

    [Test]
    public async Task SendToSessionAsync_NoToken_DoesNothing()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        _tokenRepoMock
            .Setup(r => r.GetActiveTokenBySessionAsync(sessionId))
            .ReturnsAsync("");

        // Act
        await _service.SendToSessionAsync(sessionId, "title", "body", "chat");

        // Assert
        _providerMock.Verify(
            p => p.SendToTokenAsync(It.IsAny<string>(), It.IsAny<PushMessage>()),
            Times.Never);
    }

    [Test]
    public async Task SendToSessionAsync_WithFailure_RemovesToken()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var token = "t1";

        _tokenRepoMock
            .Setup(r => r.GetActiveTokenBySessionAsync(sessionId))
            .ReturnsAsync(token);

        _providerMock
            .Setup(p => p.SendToTokenAsync(token, It.IsAny<PushMessage>()))
            .ReturnsAsync(new SendPushResult(0, 1, [token]));

        _tokenRepoMock
            .Setup(r => r.RemoveTokensAsync(It.IsAny<IEnumerable<string>>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.SendToSessionAsync(sessionId, "title", "body", "chat");

        // Assert
        _tokenRepoMock.Verify(
            r => r.RemoveTokensAsync(It.Is<IEnumerable<string>>(x => x.Contains(token))),
            Times.Once);
    }

    #endregion
}