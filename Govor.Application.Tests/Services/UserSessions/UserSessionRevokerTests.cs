using Govor.Application.Services.UserSessions;
using Govor.Core.Models;
using Govor.Core.Repositories.UserSessionsRepository;
using Govor.Data.Repositories.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Govor.Application.Tests.Services.UserSessions;

[TestFixture]
[TestOf(typeof(UserSessionRevoker))]
public class UserSessionRevokerTests
{
        private Mock<IUserSessionsRepository> _sessionsRepositoryMock;
        private Mock<ILogger<UserSessionRevoker>> _loggerMock;
        private UserSessionRevoker _revoker;

        [SetUp]
        public void SetUp()
        {
            _sessionsRepositoryMock = new Mock<IUserSessionsRepository>();
            _loggerMock = new Mock<ILogger<UserSessionRevoker>>();
            _revoker = new UserSessionRevoker(_sessionsRepositoryMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task CloseSessionByIdAsync_ValidSessionAndUserId_SetsIsRevokedAndUpdates()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var session = new UserSession { Id = sessionId, UserId = userId, IsRevoked = false };
            _sessionsRepositoryMock.Setup(r => r.GetByIdAsync(sessionId)).ReturnsAsync(session);

            // Act
            await _revoker.CloseSessionByIdAsync(sessionId, userId);

            // Assert
            Assert.That(session.IsRevoked, Is.True);
            _sessionsRepositoryMock.Verify(r => r.UpdateAsync(session), Times.Once());
            _loggerMock.VerifyNoOtherCalls();
        }

        [Test]
        public void CloseSessionByIdAsync_UnauthorizedUser_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var differentUserId = Guid.NewGuid();
            var session = new UserSession { Id = sessionId, UserId = userId, IsRevoked = false };
            _sessionsRepositoryMock.Setup(r => r.GetByIdAsync(sessionId)).ReturnsAsync(session);

            // Act & Assert
            var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
                _revoker.CloseSessionByIdAsync(sessionId, differentUserId));
            Assert.That(ex.Message, Contains.Substring($"User {differentUserId} does not belong to this session {sessionId}"));
            _loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once());
            _sessionsRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<UserSession>()), Times.Never());
        }

        [Test]
        public async Task CloseSessionByIdAsync_AlreadyRevokedSession_DoesNotUpdate()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var session = new UserSession { Id = sessionId, UserId = userId, IsRevoked = true };
            _sessionsRepositoryMock.Setup(r => r.GetByIdAsync(sessionId)).ReturnsAsync(session);

            // Act
            await _revoker.CloseSessionByIdAsync(sessionId, userId);

            // Assert
            _sessionsRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<UserSession>()), Times.Never());
            _loggerMock.VerifyNoOtherCalls();
        }

        [Test]
        public void CloseSessionByIdAsync_NonExistentSession_ThrowsNotFoundException()
        {
            // Arrange
            var sessionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            _sessionsRepositoryMock.Setup(r => r.GetByIdAsync(sessionId))
                .ThrowsAsync(new NotFoundByKeyException<Guid>(sessionId));

            // Act & Assert
            var ex = Assert.ThrowsAsync<NotFoundException>(() => 
                _revoker.CloseSessionByIdAsync(sessionId, userId));
            Assert.That(ex.Message, Is.EqualTo("Session not found"));
            _loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once());
            _sessionsRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<UserSession>()), Times.Never());
        }

        [Test]
        public async Task CloseAllSessionsAsync_MultipleSessions_RevokesNonRevokedSessions()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var sessions = new List<UserSession>
            {
                new UserSession { Id = Guid.NewGuid(), UserId = userId, IsRevoked = false },
                new UserSession { Id = Guid.NewGuid(), UserId = userId, IsRevoked = true },
                new UserSession { Id = Guid.NewGuid(), UserId = userId, IsRevoked = false }
            };
            _sessionsRepositoryMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(sessions);

            // Act
            await _revoker.CloseAllSessionsAsync(userId);

            // Assert
            Assert.That(sessions[0].IsRevoked, Is.True);
            Assert.That(sessions[1].IsRevoked, Is.True);
            Assert.That(sessions[2].IsRevoked, Is.True);
            _sessionsRepositoryMock.Verify(r => r.UpdateAsync(sessions[0]), Times.Once());
            _sessionsRepositoryMock.Verify(r => r.UpdateAsync(sessions[2]), Times.Once());
            _sessionsRepositoryMock.Verify(r => r.UpdateAsync(sessions[1]), Times.Never());
            _loggerMock.VerifyNoOtherCalls();
        }

        [Test]
        public void CloseAllSessionsAsync_NoSessions_ThrowsNotFoundException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _sessionsRepositoryMock.Setup(r => r.GetByUserIdAsync(userId))
                .ThrowsAsync(new NotFoundByKeyException<Guid>(userId));

            // Act & Assert
            var ex = Assert.ThrowsAsync<NotFoundException>(() => 
                _revoker.CloseAllSessionsAsync(userId));
            Assert.That(ex.Message, Is.EqualTo("Session not found"));
            _loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once());
            _sessionsRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<UserSession>()), Times.Never());
        }
}