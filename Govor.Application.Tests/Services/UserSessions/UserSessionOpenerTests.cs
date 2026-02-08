using Govor.Application.Services.UserSessions;
using Govor.Core.Models.Users;
using Govor.Core.Repositories.UserSessionsRepository;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Govor.Application.Interfaces.Authentication;
using Govor.Application.Services.Authentication;
using Govor.Data.Repositories.Exceptions;

namespace Govor.Application.Tests.Services.UserSessions;

[TestFixture]
public class UserSessionOpenerTests
{
    private Mock<IUserSessionsRepository> _repositoryMock;
    private Mock<IJwtService> _jwtServiceMock;
    private Mock<IJwtTokenHasher> _jwtTokenHasherMock;
    private UserSessionOpener _service;
    private User _user;
    
    private const string DeviceInfo = "Chrome on Windows";
    private const string RefreshToken = "new-refresh-token";
    private const string AccessToken = "new-access-token";
    private const string TokenHash = "hashed-refresh-token";

    [SetUp]
    public void Setup()
    {
        _repositoryMock = new Mock<IUserSessionsRepository>();
        _jwtServiceMock = new Mock<IJwtService>();
        var loggerMock = new Mock<ILogger<UserSessionOpener>>();
        _jwtTokenHasherMock = new Mock<IJwtTokenHasher>();
        var options = Options.Create(new JwtRefreshOption { RefreshTokenLifetimeDays = 30 });
        
        _user = new User
        {
            Id = Guid.NewGuid(),
            Username = "test",
            Description = "some description",
            PasswordHash = "hashed-password",
            IconId = Guid.NewGuid(),
            CreatedOn = DateOnly.FromDateTime(DateTime.Now),
            WasOnline = DateTime.Now,
            InviteId =  Guid.NewGuid(),
            // ... 
        };
        
        _jwtServiceMock.Setup(j => j.GenerateRefreshTokenAsync(_user)).ReturnsAsync(RefreshToken);
        _jwtTokenHasherMock.Setup(h => h.HashToken(RefreshToken)).Returns(TokenHash);
        
        _jwtServiceMock.Setup(j => j.GenerateAccessTokenAsync(_user, It.IsAny<Guid>())).ReturnsAsync(AccessToken);


        _service = new UserSessionOpener(
            _repositoryMock.Object,
            _jwtServiceMock.Object,
            _jwtTokenHasherMock.Object,
            options,
            loggerMock.Object
        );
    }
    
    // UpdateExistingSessionAsync
    [Test]
    public async Task OpenSessionAsync_ShouldUpdateExistingSession_WhenFoundByDeviceInfo()
    {
        // Arrange 
        var existingSessionId = Guid.NewGuid();
        var existingSession = new UserSession
        {
            Id = existingSessionId,
            UserId = _user.Id,
            DeviceInfo = DeviceInfo,
            RefreshTokenHash = "old-hash",
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            ExpiresAt = DateTime.UtcNow.AddDays(10),
            IsRevoked = true
        };

        _repositoryMock.Setup(r => r.GetByUserIdAsync(_user.Id)).ReturnsAsync(new List<UserSession> { existingSession });
        
        // Act 
        var result = await _service.OpenSessionAsync(_user, DeviceInfo);

        // Assert 
        Assert.Multiple(() =>
        {
            Assert.That(result.refreshToken, Is.EqualTo(RefreshToken), "Должен быть возвращен новый RefreshToken.");
            Assert.That(result.accessToken, Is.EqualTo(AccessToken), "Должен быть возвращен новый AccessToken.");
        });
        
        _repositoryMock.Verify(r => r.UpdateAsync(It.Is<UserSession>(s => 
            s.Id == existingSessionId &&
            s.RefreshTokenHash == TokenHash &&
            s.IsRevoked == false && 
            s.ExpiresAt > DateTime.UtcNow 
        )), Times.Once, "Должен быть вызван метод UpdateAsync с новыми данными.");
        
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<UserSession>()), Times.Never);
    }

    // CreateNewSessionAsync
    
    [Test]
    public async Task OpenSessionAsync_ShouldCreateNewSession_IfNoneFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByUserIdAsync(_user.Id)).ReturnsAsync(new List<UserSession>());
        
        // Act 
        var result = await _service.OpenSessionAsync(_user, DeviceInfo);

        // Assert 
        Assert.Multiple(() =>
        {
            Assert.That(result.refreshToken, Is.EqualTo(RefreshToken), "Должен быть возвращен новый RefreshToken.");
            Assert.That(result.accessToken, Is.EqualTo(AccessToken), "Должен быть возвращен новый AccessToken.");
        });
        
        _repositoryMock.Verify(r => r.AddAsync(It.Is<UserSession>(s =>
            s.UserId == _user.Id &&
            s.DeviceInfo == DeviceInfo &&
            s.RefreshTokenHash == TokenHash &&
            s.IsRevoked == false
        )), Times.Once, "Должен быть вызван метод AddAsync для создания новой сессии.");
        
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<UserSession>()), Times.Never);
    }

    [Test]
    public async Task OpenSessionAsync_ShouldCreateNewSession_WhenRepositoryThrowsNotFoundByKeyException()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByUserIdAsync(_user.Id))
            .ThrowsAsync(new NotFoundByKeyException<Guid>(_user.Id, "userId")); 
        
        // Act
        var result = await _service.OpenSessionAsync(_user, DeviceInfo);

        // Assert 
        Assert.Multiple(() =>
        {
            Assert.That(result.refreshToken, Is.EqualTo(RefreshToken), "Должен быть возвращен новый RefreshToken.");
            Assert.That(result.accessToken, Is.EqualTo(AccessToken), "Должен быть возвращен новый AccessToken.");
        });
        
        _repositoryMock.Verify(r => r.AddAsync(It.Is<UserSession>(s =>
            s.RefreshTokenHash == TokenHash 
        )), Times.Once, "Должен быть вызван AddAsync после перехвата исключения.");
        
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<UserSession>()), Times.Never);
    }
}
