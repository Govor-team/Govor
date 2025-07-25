using Govor.Application.Interfaces.Authentication;
using Govor.Application.Services.Authentication;
using Govor.Application.Services.UserSessions;
using Govor.Core.Models;
using Govor.Core.Models.Users;
using Govor.Core.Repositories.Users;
using Govor.Core.Repositories.UserSessionsRepository;
using Govor.Data.Repositories.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Govor.Application.Tests.Services.UserSessions;

[TestFixture]
public class UserSessionRefresherTests
{
    private Mock<IUserSessionsRepository> _sessionsRepoMock;
    private Mock<IUsersRepository> _usersRepoMock;
    private Mock<IJwtService> _jwtServiceMock;
    private Mock<ILogger<UserSessionRefresher>> _loggerMock;
    private Mock<IOptions<JwtRefreshOption>> _optionsMock;
    private JwtRefreshOption _options;
    private UserSessionRefresher _refresher;
    private const string OldRefreshToken = "old-refresh-token";
    private const string NewRefreshToken = "new-refresh-token";
    private const string NewAccessToken = "new-access-token";
    private User _user;
    private UserSession _session;

    [SetUp]
    public void Setup()
    {
        _sessionsRepoMock = new Mock<IUserSessionsRepository>();
        _usersRepoMock = new Mock<IUsersRepository>();
        _jwtServiceMock = new Mock<IJwtService>();
        _loggerMock = new Mock<ILogger<UserSessionRefresher>>();
        _optionsMock = new Mock<IOptions<JwtRefreshOption>>();
        
        _options = new JwtRefreshOption { RefreshTokenLifetimeDays = 30 };
        
        _optionsMock.SetupGet(o => o.Value).Returns(_options);
        
        _refresher = new UserSessionRefresher(
            _sessionsRepoMock.Object,
            _loggerMock.Object,
            _usersRepoMock.Object,
            _optionsMock.Object,
            _jwtServiceMock.Object);

        _user = new User
        {
            Id = Guid.NewGuid(),
            Username = "TestUser",
            PasswordHash = "hash",
            InviteId = Guid.NewGuid()
        };

        _session = new UserSession
        {
            RefreshToken = OldRefreshToken,
            UserId = _user.Id,
            DeviceInfo = "Chrome",
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            ExpiresAt = DateTime.UtcNow.AddDays(5),
            IsRevoked = false
        };
    }

    [Test]
    public async Task RefreshTokenAsync_ValidToken_ReturnsNewTokensAndCreatesNewSession()
    {
        // Arrange
        _sessionsRepoMock.Setup(r => r.GetByRefreshTokenAsync(OldRefreshToken)).ReturnsAsync(_session);
        _usersRepoMock.Setup(r => r.FindByIdAsync(_user.Id)).ReturnsAsync(_user);
        _jwtServiceMock.Setup(j => j.GenerateAccessTokenAsync(_user, _session.Id)).ReturnsAsync(NewAccessToken);
        _jwtServiceMock.Setup(j => j.GenerateRefreshTokenAsync(_user)).ReturnsAsync(NewRefreshToken);

        // Act
        var result = await _refresher.RefreshTokenAsync(OldRefreshToken);

        // Assert
        Assert.That(result.accessToken, Is.EqualTo(NewAccessToken));
        Assert.That(result.refreshToken, Is.EqualTo(NewRefreshToken));
        Assert.That(_session.IsRevoked, Is.True);

        _sessionsRepoMock.Verify(r => r.UpdateAsync(_session), Times.Once);
        _sessionsRepoMock.Verify(r => r.AddAsync(It.Is<UserSession>(s =>
            s.UserId == _user.Id &&
            s.RefreshToken == NewRefreshToken &&
            s.DeviceInfo == _session.DeviceInfo)), Times.Once);
    }

    [Test]
    public void RefreshTokenAsync_RevokedToken_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        _session.IsRevoked = true;
        _sessionsRepoMock.Setup(r => r.GetByRefreshTokenAsync(OldRefreshToken)).ReturnsAsync(_session);
        
        // Act & Assert 
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _refresher.RefreshTokenAsync(OldRefreshToken));

        Assert.That(ex.Message, Contains.Substring("Refresh token is invalid or expired"));
    }

    [Test]
    public void RefreshTokenAsync_ExpiredToken_ThrowsUnauthorizedAccessException()
    {
        // Arrange 
        _session.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);
        _sessionsRepoMock.Setup(r => r.GetByRefreshTokenAsync(OldRefreshToken)).ReturnsAsync(_session);
        
        // Act & Assert 
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _refresher.RefreshTokenAsync(OldRefreshToken));

        Assert.That(ex.Message, Contains.Substring("Refresh token is invalid or expired"));
    }

    [Test]
    public void RefreshTokenAsync_TokenNotFound_ThrowsUnauthorizedAccessException()
    {
        // Arrange 
        _sessionsRepoMock.Setup(r => r.GetByRefreshTokenAsync(OldRefreshToken))
            .ThrowsAsync(new NotFoundByKeyException<string>("token", OldRefreshToken));
        // Act & Assert 
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _refresher.RefreshTokenAsync(OldRefreshToken));

        Assert.That(ex.Message, Contains.Substring("Invalid refresh token"));
    }
}

