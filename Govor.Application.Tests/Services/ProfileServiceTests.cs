using Govor.Application.Services;
using Govor.Core.Models.Users;
using Govor.Core.Repositories.Users;
using Microsoft.Extensions.Logging;
using Moq;

namespace Govor.Application.Tests.Services;

[TestFixture]
public class ProfileServiceTests
{
    private Mock<IUsersRepository> _mockUsersRepo = null!;
    private Mock<ILogger<ProfileService>> _mockLogger = null!;
    private ProfileService _service = null!;
    private User _testUser = null!;

    [SetUp]
    public void SetUp()
    {
        _mockUsersRepo = new Mock<IUsersRepository>();
        _mockLogger = new Mock<ILogger<ProfileService>>();

        _service = new ProfileService(_mockUsersRepo.Object, _mockLogger.Object);

        _testUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Description = "old description",
            IconId = Guid.NewGuid()
        };
    }

    // ---------- GetUserProfileAsync ----------

    [Test]
    public async Task GetUserProfileAsync_ReturnsCorrectData()
    {
        // Arrange
        _mockUsersRepo.Setup(r => r.FindByIdAsync(_testUser.Id))
            .ReturnsAsync(_testUser);

        // Act
        var result = await _service.GetUserProfileAsync(_testUser.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(_testUser.Id));
        Assert.That(result.Username, Is.EqualTo("testuser"));
        Assert.That(result.Description, Is.EqualTo("old description"));
        Assert.That(result.IconId, Is.EqualTo(_testUser.IconId));

        _mockUsersRepo.Verify(r => r.FindByIdAsync(_testUser.Id), Times.Once);
    }

    [Test]
    public void GetUserProfileAsync_Throws_WhenUserNotFound()
    {
        // Arrange
        _mockUsersRepo.Setup(r => r.FindByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((User)null!);

        // Act + Assert
        Assert.ThrowsAsync<NullReferenceException>(() =>
            _service.GetUserProfileAsync(Guid.NewGuid()));
    }

    // ---------- SetDescription ----------

    [Test]
    public async Task SetDescription_UpdatesUserDescription()
    {
        // Arrange
        var newDescription = "new cool description";
        _mockUsersRepo.Setup(r => r.FindByIdAsync(_testUser.Id))
            .ReturnsAsync(_testUser);
        _mockUsersRepo.Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.SetDescription(newDescription, _testUser.Id);

        // Assert
        Assert.That(_testUser.Description, Is.EqualTo(newDescription));
        _mockUsersRepo.Verify(r => r.UpdateAsync(It.Is<User>(u => u.Description == newDescription)), Times.Once);
    }

    [Test]
    public void SetDescription_Throws_WhenUserNotFound()
    {
        // Arrange
        _mockUsersRepo.Setup(r => r.FindByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((User)null!);

        // Act + Assert
        Assert.ThrowsAsync<NullReferenceException>(() =>
            _service.SetDescription("desc", Guid.NewGuid()));
    }

    // ---------- SetNewIcon ----------

    [Test]
    public async Task SetNewIcon_UpdatesUserIconId()
    {
        // Arrange
        var newIconId = Guid.NewGuid();
        _mockUsersRepo.Setup(r => r.FindByIdAsync(_testUser.Id))
            .ReturnsAsync(_testUser);
        _mockUsersRepo.Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.SetNewIcon(_testUser.Id, newIconId);

        // Assert
        Assert.That(_testUser.IconId, Is.EqualTo(newIconId));
        _mockUsersRepo.Verify(r => r.UpdateAsync(It.Is<User>(u => u.IconId == newIconId)), Times.Once);
    }

    [Test]
    public void SetNewIcon_Throws_WhenUserNotFound()
    {
        // Arrange
        _mockUsersRepo.Setup(r => r.FindByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((User)null!);

        // Act + Assert
        Assert.ThrowsAsync<NullReferenceException>(() =>
            _service.SetNewIcon(Guid.NewGuid(), Guid.NewGuid()));
    }
}