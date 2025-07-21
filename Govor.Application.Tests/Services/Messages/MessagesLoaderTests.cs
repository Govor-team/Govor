using Govor.Application.Interfaces;
using Govor.Application.Services.Messages;
using Govor.Core.Models;
using Govor.Core.Models.Messages;
using Govor.Core.Repositories.Groups;
using Govor.Core.Repositories.PrivateChats;
using Govor.Data;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Govor.Application.Tests.Services.Messages;

[TestFixture]
public class MessagesLoaderTests
{
    private IMessagesLoader _loader;
    private Mock<IPrivateChatsRepository> _privateChatsRepoMock;
    private Mock<IGroupsRepository> _groupsRepoMock;
    private GovorDbContext _dbContext;

    private Guid _currentUserId = Guid.NewGuid();
    private Guid _otherUserId = Guid.NewGuid();
    private Guid _groupChatId = Guid.NewGuid();

    [SetUp]
    public void Setup()
    {
        _privateChatsRepoMock = new Mock<IPrivateChatsRepository>();
        _groupsRepoMock = new Mock<IGroupsRepository>();

        var options = new DbContextOptionsBuilder<GovorDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new GovorDbContext(options);

        _loader = new MessagesLoader(_groupsRepoMock.Object, _privateChatsRepoMock.Object, _dbContext);
    }
    
    // Tests for LoadLastMessagesInUserChat action 
    [Test]
    public async Task LoadLastMessagesInUserChat_ReturnsMessages_WhenChatExists()
    {
        // Arrange
        var chatId = Guid.NewGuid();

        _privateChatsRepoMock.Setup(r => r.Exist(_currentUserId, _otherUserId)).Returns(true);
        _privateChatsRepoMock.Setup(r => r.GetByMembersAsync(_currentUserId, _otherUserId))
            .ReturnsAsync(new PrivateChat() { Id = chatId });

        var messages = new List<Message>
        {
            new() { Id = Guid.NewGuid(), RecipientId = chatId, RecipientType = RecipientType.User }
        };

        await _dbContext.Messages.AddRangeAsync(messages);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _loader.LoadLastMessagesInUserChat(_currentUserId, _otherUserId, null);

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task LoadLastMessagesInUserChat_ReturnsEmpty_WhenNoMessages()
    {
        // Arrange 
        _privateChatsRepoMock.Setup(r => r.Exist(_currentUserId, _otherUserId)).Returns(true);
        _privateChatsRepoMock.Setup(r => r.GetByMembersAsync(_currentUserId, _otherUserId))
            .ReturnsAsync(new PrivateChat { Id = Guid.NewGuid() });
        // Act
        var result = await _loader.LoadLastMessagesInUserChat(_currentUserId, _otherUserId, null);

        // Assert 
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void LoadLastMessagesInUserChat_Throws_WhenUserIdIsEmpty()
    {
        // Act & Assert 
        var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
            await _loader.LoadLastMessagesInUserChat(Guid.Empty, _currentUserId, null));

        Assert.That(ex.Message, Does.Contain("User id cannot be empty"));
    }

    [Test]
    public void LoadLastMessagesInUserChat_Throws_WhenChatNotExists()
    {
        // Arrange 
        _privateChatsRepoMock.Setup(r => r.Exist(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(false);

        // Act & Assert 
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _loader.LoadLastMessagesInUserChat(_currentUserId, _otherUserId, null));

        Assert.That(ex.Message, Is.EqualTo("Private chat not found"));
    }
    
    // Tests for LoadLastMessagesInChatGroup action 
    [Test]
    public async Task LoadLastMessagesInChatGroup_ReturnsMessages_WhenUserIsMember()
    {
        // Arrange 
        _groupsRepoMock.Setup(g => g.IsUserMemberOfGroupAsync(_currentUserId, _groupChatId))
            .ReturnsAsync(true);

        var messages = new List<Message>
        {
            new() { Id = Guid.NewGuid(), RecipientId = _groupChatId, RecipientType = RecipientType.Group }
        };

        await _dbContext.Messages.AddRangeAsync(messages);
        await _dbContext.SaveChangesAsync();
        
        // Act
        var result = await _loader.LoadLastMessagesInChatGroup(_groupChatId, _currentUserId, null);

        // Assert 
        Assert.That(result, Has.Count.EqualTo(1));
    }

    [Test]
    public void LoadLastMessagesInChatGroup_Throws_WhenChatIdEmpty()
    {
        // Act & Assert  
        var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
            await _loader.LoadLastMessagesInChatGroup(Guid.Empty, _currentUserId, null));
        
        Assert.That(ex.Message, Does.Contain("Chat id cannot be empty"));
    }

    [Test]
    public void LoadLastMessagesInChatGroup_Throws_WhenUserNotInGroup()
    {
        // Arrange 
        _groupsRepoMock.Setup(g => g.IsUserMemberOfGroupAsync(_currentUserId, _groupChatId))
            .ReturnsAsync(false);
        
        // Act & Assert 
        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _loader.LoadLastMessagesInChatGroup(_groupChatId, _currentUserId, null));

        Assert.That(ex.Message, Is.EqualTo("You are not in a group."));
    }

    [Test]
    public async Task LoadLastMessagesInChatGroup_ReturnsEmpty_WhenNoMessages()
    {
        // Arrange 
        _groupsRepoMock.Setup(g => g.IsUserMemberOfGroupAsync(_currentUserId, _groupChatId))
            .ReturnsAsync(true);
        
        // Act 
        var result = await _loader.LoadLastMessagesInChatGroup(_groupChatId, _currentUserId, null);

        // Assert 
        Assert.That(result, Is.Empty);
    }
    
    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }
}