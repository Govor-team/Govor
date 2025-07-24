using AutoFixture;
using Govor.Application.Interfaces.UserSession;
using Govor.Application.Services.UserSessions;
using Govor.Core.Models;
using Govor.Core.Repositories.UserSessionsRepository;
using Govor.Data.Repositories.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Govor.Application.Tests.Services.UserSessions;

[TestFixture]
[TestOf(typeof(UserSessionReader))]
public class UserSessionReaderTests
{
    private Mock<IUserSessionsRepository>  _mockUserSessionsRepository;
    private Mock<ILogger<UserSessionReader>> _mockLogger;
    private Fixture _fixture;
    private IUserSessionReader _userSessionReader;
    
    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        
        _mockUserSessionsRepository = new Mock<IUserSessionsRepository>();
        _mockLogger = new Mock<ILogger<UserSessionReader>>();
        
        _userSessionReader = new UserSessionReader(_mockUserSessionsRepository.Object,
            _mockLogger.Object);
    }

    [Test]
    public async Task GetAllUserSessionsAsync_ShouldReturnAllUserSessions()
    {
        // Arrange 
        var sessios = _fixture.Build<UserSession>()
            .With(f => f.IsRevoked, false)
            .CreateMany().ToList();
        
        var userId = Guid.NewGuid();
        
        _mockUserSessionsRepository.Setup(f => f.GetByUserIdAsync(userId))
            .ReturnsAsync(sessios);
        // Act 
        var result = await _userSessionReader.GetAllSessionsAsync(userId);
        
        // Assert 
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EquivalentTo(sessios));
    }

    [Test]
    public async Task GetAllUserSessionsAsync_ThrowsNotFoundByKeyException_ReturnsEmptyList()
    {
        // Arrange 
        var userId = Guid.NewGuid();
        
        _mockUserSessionsRepository.Setup(f => f.GetByUserIdAsync(userId))
            .ThrowsAsync(new NotFoundByKeyException<Guid>(userId));
        
        // Act 
        var result = await _userSessionReader.GetAllSessionsAsync(userId);
        
        // Assert 
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }
}