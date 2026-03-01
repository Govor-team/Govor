using AutoFixture;
using Govor.Application.Interfaces;
using Govor.Application.Services;
using Govor.Core.Models;
using Govor.Core.Repositories.PrivateChats;
using Microsoft.Extensions.Logging;
using Moq;

namespace Govor.Application.Tests.Services;

[TestFixture]
public class UserPrivateChatsCreatorTests
{
    private Fixture _fixture;
    private Mock<ILogger<UserPrivateChatsCreator>> _mockLogger;
    private Mock<IPrivateChatsRepository> _mockPrivateChats;
    private Mock<IPrivateChatGroupManager> _mockPrivateChatGroupManager;
    private IUserPrivateChatsCreator _service;
    
    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _fixture.Behaviors
            .OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _mockLogger = new Mock<ILogger<UserPrivateChatsCreator>>();
        _mockPrivateChats = new Mock<IPrivateChatsRepository>();
        _mockPrivateChatGroupManager = new Mock<IPrivateChatGroupManager>();

        _service = new UserPrivateChatsCreator(
            _mockPrivateChats.Object, 
            _mockPrivateChatGroupManager.Object,
            _mockLogger.Object);
    }
    
    [Test]
    public async Task CreateAsync_ShouldCreateNewChat_WhenChatDoesNotExist()
    {
        // Arrange
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();

        _mockPrivateChats
            .Setup(x => x.Exist(userA, userB))
            .Returns(false);

        PrivateChat? addedChat = null;

        _mockPrivateChats
            .Setup(x => x.AddAsync(It.IsAny<PrivateChat>()))
            .Callback<PrivateChat>(chat => addedChat = chat)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateAsync(userA, userB);

        // Assert
        _mockPrivateChats.Verify(x => x.AddAsync(It.IsAny<PrivateChat>()), Times.Once);
        _mockPrivateChatGroupManager.Verify(x => x.AddUsersToPrivateChatGroupAsync(It.IsAny<PrivateChat>()), Times.Once);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.UserAId, Is.EqualTo(userA));
        Assert.That(result.UserBId, Is.EqualTo(userB));
        Assert.That(result.Id, Is.Not.EqualTo(Guid.Empty));

        Assert.That(addedChat, Is.Not.Null);
        Assert.That(addedChat!.Id, Is.EqualTo(result.Id));
    }

    [Test]
    public async Task CreateAsync_ShouldReturnExistingChat_WhenChatAlreadyExists()
    {
        // Arrange
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();

        var existingChat = _fixture.Build<PrivateChat>()
            .With(x => x.UserAId, userA)
            .With(x => x.UserBId, userB)
            .Create();

        _mockPrivateChats
            .Setup(x => x.Exist(userA, userB))
            .Returns(true);

        _mockPrivateChats
            .Setup(x => x.GetByMembersAsync(userA, userB))
            .ReturnsAsync(existingChat);

        // Act
        var result = await _service.CreateAsync(userA, userB);

        // Assert
        _mockPrivateChats.Verify(x => x.AddAsync(It.IsAny<PrivateChat>()), Times.Never);
        
        _mockPrivateChatGroupManager.Verify(
            x => x.AddUsersToPrivateChatGroupAsync(It.IsAny<PrivateChat>()),
            Times.Never);
        
        _mockPrivateChats.Verify(x => x.GetByMembersAsync(userA, userB), Times.Once);

        Assert.That(result, Is.EqualTo(existingChat));
    }
}