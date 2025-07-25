using Govor.Application.Services.UserSessions;
using Govor.Core.Models.Users;
using Govor.Core.Repositories.UserSessionsRepository;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Govor.Application.Interfaces.Authentication;
using Govor.Application.Services.Authentication;

namespace Govor.Application.Tests.Services.UserSessions;

[TestFixture]
public class UserSessionOpenerTests
{
    private Mock<IUserSessionsRepository> _repositoryMock;
    private Mock<IJwtService> _jwtServiceMock;
    private Mock<ILogger<UserSessionOpener>> _loggerMock;
    private IOptions<JwtRefreshOption> _options;
    private UserSessionOpener _service;
    private User _user;
    private Guid _sessionId;
    private const string DeviceInfo = "Chrome on Windows";
    private const string GeneratedToken = "new-refresh-token";
    private const string NewAccessToken = "new-access-token";

    [SetUp]
    public void Setup()
    {
        _repositoryMock = new Mock<IUserSessionsRepository>();
        _jwtServiceMock = new Mock<IJwtService>();
        _loggerMock = new Mock<ILogger<UserSessionOpener>>();
        _options = Options.Create(new JwtRefreshOption { RefreshTokenLifetimeDays = 30 });
        
        _sessionId = Guid.NewGuid();
        
        _user = new User
        {
            Id = Guid.NewGuid(),
            Username = "test",
            PasswordHash = "hashed",
            IconId = Guid.NewGuid(),
            CreatedOn = DateOnly.FromDateTime(DateTime.UtcNow),
            WasOnline = DateTime.UtcNow,
            InviteId = Guid.NewGuid()
        };

        _jwtServiceMock.Setup(j => j.GenerateRefreshTokenAsync(_user)).ReturnsAsync(GeneratedToken);
        _jwtServiceMock.Setup(j => j.GenerateAccessTokenAsync(_user, _sessionId)).ReturnsAsync(NewAccessToken);

        _service = new UserSessionOpener(
            _repositoryMock.Object,
            _jwtServiceMock.Object,
            _options,
            _loggerMock.Object
        );
    }

    [Test]
    public async Task OpenSessionAsync_ShouldReturnExistingToken_IfSessionValid()
    {
        // Arrange 
        var session = new UserSession
        {
            Id = _sessionId,
            UserId = _user.Id,
            DeviceInfo = DeviceInfo,
            RefreshToken = "valid-token",
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            ExpiresAt = DateTime.UtcNow.AddDays(10),
            IsRevoked = false
        };

        _repositoryMock.Setup(r => r.GetByUserIdAsync(_user.Id)).ReturnsAsync(new List<UserSession> { session });

        // Act 
        var result = await _service.OpenSessionAsync(_user, DeviceInfo);

        // Assert 
        Assert.That(result.refreshToken, Is.EqualTo(GeneratedToken));
        Assert.That(result.accessToken, Is.EqualTo(NewAccessToken));
        _repositoryMock.Verify(r => r.UpdateAsync(It.Is<UserSession>(s =>
            s.RefreshToken == GeneratedToken &&
            s.IsRevoked == false)), Times.Once);
    }

    [Test]
    public async Task OpenSessionAsync_ShouldUpdateSession_IfExpiredOrRevoked()
    {
        // Arrange 
        var session = new UserSession
        {
            Id = _sessionId,
            UserId = _user.Id,
            DeviceInfo = DeviceInfo,
            RefreshToken = "old-token",
            CreatedAt = DateTime.UtcNow.AddDays(-40),
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            IsRevoked = false
        };

        _repositoryMock.Setup(r => r.GetByUserIdAsync(_user.Id)).ReturnsAsync(new List<UserSession> { session });

        // Act 
        var result = await _service.OpenSessionAsync(_user, DeviceInfo);

        // Assert 
        Assert.That(result.refreshToken, Is.EqualTo(GeneratedToken));
        Assert.That(result.accessToken, Is.EqualTo(NewAccessToken));
        _repositoryMock.Verify(r => r.UpdateAsync(It.Is<UserSession>(s =>
            s.RefreshToken == GeneratedToken &&
            s.IsRevoked == false)), Times.Once);
    }

    [Test]
    public async Task OpenSessionAsync_ShouldCreateNewSession_IfNoneExists()
    {
        // Arrange 
        _repositoryMock.Setup(r => r.GetByUserIdAsync(_user.Id)).ReturnsAsync(new List<UserSession>());
        _jwtServiceMock.Setup(j => j.GenerateAccessTokenAsync(_user, It.IsAny<Guid>())).ReturnsAsync(NewAccessToken);
        
        // Act 
        var result = await _service.OpenSessionAsync(_user, DeviceInfo);

        // Assert 
        Assert.That(result.refreshToken, Is.EqualTo(GeneratedToken));
        Assert.That(result.accessToken, Is.EqualTo(NewAccessToken));
        _repositoryMock.Verify(r => r.AddAsync(It.Is<UserSession>(s =>
            s.UserId == _user.Id &&
            s.DeviceInfo == DeviceInfo &&
            s.RefreshToken == GeneratedToken
        )), Times.Once);
    }

    [Test]
    public async Task OpenSessionAsync_ShouldCreateNewSession_WhenNotFoundByKeyExceptionThrown()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByUserIdAsync(_user.Id))
            .ThrowsAsync(new Govor.Data.Repositories.Exceptions.NotFoundByKeyException<Guid>(_user.Id, "userId"));
        _jwtServiceMock.Setup(j => j.GenerateAccessTokenAsync(_user, It.IsAny<Guid>())).ReturnsAsync(NewAccessToken);
        
        // Act
        var result = await _service.OpenSessionAsync(_user, DeviceInfo);

        // Assert 
        Assert.That(result.refreshToken, Is.EqualTo(GeneratedToken));
        Assert.That(result.accessToken, Is.EqualTo(NewAccessToken));
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<UserSession>()), Times.Once);
    }
}
