using Govor.Application.Interfaces.Authentication;
using Govor.Application.Services.Authentication;
using Govor.Application.Services.UserSessions;
using Govor.Core.Models;
using Govor.Core.Models.Users;
using Govor.Core.Repositories.UserSessionsRepository;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

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
    private const string DeviceInfo = "Chrome on Windows";
    private const string GeneratedToken = "new-refresh-token";

    [SetUp]
    public void Setup()
    {
        _repositoryMock = new Mock<IUserSessionsRepository>();
        _jwtServiceMock = new Mock<IJwtService>();
        _loggerMock = new Mock<ILogger<UserSessionOpener>>();
        _options = Options.Create(new JwtRefreshOption { RefreshTokenLifetimeDays = 30 });

        _service = new UserSessionOpener(
            _repositoryMock.Object,
            _jwtServiceMock.Object,
            _options,
            _loggerMock.Object
        );

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

        _jwtServiceMock
            .Setup(j => j.GenerateRefreshTokenAsync(_user))
            .ReturnsAsync(GeneratedToken);
    }

    [Test]
    public async Task OpenSessionAsync_ShouldReturnExistingToken_IfSessionValid()
    {
        // Arrange 
        var session = new Core.Models.UserSession
        {
            UserId = _user.Id,
            DeviceInfo = DeviceInfo,
            RefreshToken = "valid-token",
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            ExpiresAt = DateTime.UtcNow.AddDays(10),
            IsRevoked = false
        };

        _repositoryMock
            .Setup(r => r.GetByUserIdAsync(_user.Id))
            .ReturnsAsync(new List<UserSession> { session });
    
        // Act 
        var result = await _service.OpenSessionAsync(_user, DeviceInfo);

        // Asser 
        Assert.That(result, Is.EqualTo("valid-token"));
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<UserSession>()), Times.Never);
    }

    [Test]
    public async Task OpenSessionAsync_ShouldUpdateSession_IfExpiredOrRevoked()
    {
        // Arrange 
        var session = new Core.Models.UserSession
        {
            UserId = _user.Id,
            DeviceInfo = DeviceInfo,
            RefreshToken = "old-token",
            CreatedAt = DateTime.UtcNow.AddDays(-40),
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            IsRevoked = false
        };

        _repositoryMock
            .Setup(r => r.GetByUserIdAsync(_user.Id))
            .ReturnsAsync(new List<UserSession> { session });
    
        // Act 
        var result = await _service.OpenSessionAsync(_user, DeviceInfo);

        // Assert 
        Assert.That(result, Is.EqualTo(GeneratedToken));
        _repositoryMock.Verify(r => r.UpdateAsync(It.Is<UserSession>(s => s.RefreshToken == GeneratedToken)), Times.Once);
    }

    [Test]
    public async Task OpenSessionAsync_ShouldCreateNewSession_IfNoneExists()
    {
        // Arrange 
        _repositoryMock
            .Setup(r => r.GetByUserIdAsync(_user.Id))
            .ReturnsAsync(new List<UserSession>());

        // Act 
        var result = await _service.OpenSessionAsync(_user, DeviceInfo);
        
        // Assert 
        Assert.That(result, Is.EqualTo(GeneratedToken));
        _repositoryMock.Verify(r => r.AddAsync(It.Is<UserSession>(s =>
            s.UserId == _user.Id &&
            s.DeviceInfo == DeviceInfo &&
            s.RefreshToken == GeneratedToken
        )), Times.Once);
    }
}
