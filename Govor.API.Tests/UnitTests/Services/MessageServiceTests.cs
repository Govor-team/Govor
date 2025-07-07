using Govor.Application.Exceptions.VerifyFriendship;
using Govor.Application.Interfaces;
using Govor.Application.Interfaces.Messages.Parameters;
using Govor.Application.Services;
using Govor.Core.Models;
using Govor.Core.Repositories.Groups;
using Govor.Core.Repositories.Messages;
using Govor.Core.Repositories.PrivateChats;
using Govor.Core.Repositories.Users;
using Govor.Data.Repositories.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Govor.API.Tests.UnitTests.Services;

[TestFixture]
public class MessageServiceTests
{
    private Mock<IMessagesRepository> _mockMessagesRepo;
    private Mock<IUsersRepository> _mockUsersRepo;
    private Mock<IGroupsRepository> _mockGroupsRepo;
    private Mock<IVerifyFriendship> _mockVerifyFriendship;
    private Mock<IPrivateChatsRepository> _mockPrivateChats;
    private Mock<ILogger<MessageService>> _mockLogger;
    private MessageService _messageService;
    
    [SetUp]
    public void SetUp()
    {
        _mockMessagesRepo = new Mock<IMessagesRepository>();
        _mockUsersRepo = new Mock<IUsersRepository>();
        _mockGroupsRepo = new Mock<IGroupsRepository>();
        _mockVerifyFriendship = new Mock<IVerifyFriendship>();
        _mockPrivateChats = new Mock<IPrivateChatsRepository>();
        _mockLogger = new Mock<ILogger<MessageService>>();

        _messageService = new MessageService(
            _mockMessagesRepo.Object,
            _mockUsersRepo.Object,
            _mockGroupsRepo.Object,
            _mockVerifyFriendship.Object,
            _mockPrivateChats.Object,
            _mockLogger.Object);
    }

    
    // Test for SendMessageAsync action
    [Test]
    public async Task SendMessageAsync_ToUser_Success()
    {
        // Arrange
        var senderId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        
        var sendMessageParams = new SendMessage("Hello", 
            null,
            recipientId,
            RecipientType.User,
            senderId,
            DateTime.UtcNow,
            new List<SendMedia>());

        _mockUsersRepo.Setup(r => r.ExistsByIdAsync(recipientId)).ReturnsAsync(true);
        _mockVerifyFriendship.Setup(v => v.VerifyAsync(senderId, recipientId)).Returns(Task.CompletedTask);
        _mockMessagesRepo.Setup(r => r.AddAsync(It.IsAny<Message>())).Returns(Task.CompletedTask);
        _mockPrivateChats.Setup(c => c.Exist(senderId, recipientId)).Returns(true);
        _mockPrivateChats.Setup(c => c.GetByMembersAsync(senderId, recipientId)).ReturnsAsync(new PrivateChat(){Id = recipientId});
        // Act
        var result = await _messageService.SendMessageAsync(sendMessageParams);
        // Assert 
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Exception, Is.Null);
        
        _mockMessagesRepo.Verify(r => r.AddAsync(It.Is<Message>(m => 
            m.SenderId == senderId && 
            m.RecipientId == recipientId && 
            m.RecipientType == RecipientType.User &&
            m.EncryptedContent == "Hello")), Times.Once);
    }
    
    [Test]
    public async Task SendMessageAsync_ToGroup_Success()
    {
        // Arrange
        var senderId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        
        var sendMessageParams = new SendMessage("Hello Group",
            null,
            groupId,
            RecipientType.Group,
            senderId,
            DateTime.UtcNow,
            new List<SendMedia>());

        _mockGroupsRepo.Setup(r => r.Exists(groupId)).Returns(true);
        _mockGroupsRepo.Setup(r => r.IsUserMemberOfGroupAsync(senderId, groupId)).ReturnsAsync(true); 
        _mockMessagesRepo.Setup(r => r.AddAsync(It.IsAny<Message>())).Returns(Task.CompletedTask);

        // Act
        var result = await _messageService.SendMessageAsync(sendMessageParams);
        
        // Assert 
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Exception, Is.Null);
        _mockMessagesRepo.Verify(r => r.AddAsync(It.Is<Message>(m =>
            m.RecipientId == groupId &&
            m.RecipientType == RecipientType.Group)), Times.Once);
    }
    
    [Test]
    public async Task SendMessageAsync_ToUser_RecipientNotFound_ReturnsFailure()
    {
        // Arrange
        var senderId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        
        var sendMessageParams = new SendMessage("Hello",
            null,
            recipientId,
            RecipientType.User,
            senderId,
            DateTime.UtcNow,
            new List<SendMedia>());

        _mockUsersRepo.Setup(r => r.ExistsByIdAsync(recipientId)).ReturnsAsync(false);

        // Act
        var result = await _messageService.SendMessageAsync(sendMessageParams);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Exception, Is.Not.Null);
        Assert.That(result.Exception,Is.TypeOf<KeyNotFoundException>());
        Assert.That(result.Message, Is.Null);
    }

    [Test]
    public async Task SendMessageAsync_ToUser_FriendshipVerificationFails_ReturnsFailure()
    {
        // Arrange
        var senderId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        
        var sendMessageParams = new SendMessage("Hello",
            null,
            recipientId,
            RecipientType.User,
            senderId,
            DateTime.UtcNow,
            new List<SendMedia>()
            );

        _mockUsersRepo.Setup(r => r.ExistsByIdAsync(recipientId)).ReturnsAsync(true);
        _mockVerifyFriendship.Setup(v => v.VerifyAsync(senderId, recipientId)).ThrowsAsync(new FriendshipException("Not friends"));

        // Act
        var result = await _messageService.SendMessageAsync(sendMessageParams);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Exception, Is.Not.Null);
        Assert.That(result.Exception,Is.TypeOf<FriendshipException>());
        Assert.That(result.Message, Is.Null);
    }
    
    
    
    // Test for EditMessageAsync action 
    [Test]
    public async Task EditMessageAsync_Success()
    {
        // Arrange
        var editorId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        
        var originalMessage = new Message 
        { 
            Id = messageId, 
            SenderId = editorId,
            EncryptedContent = "Old",
            RecipientId = Guid.NewGuid(),
            RecipientType = RecipientType.User
        };
        
        var editParams = new EditMessage(editorId, messageId, "New Content", DateTime.UtcNow);

        _mockMessagesRepo.Setup(r => r.FindByIdAsync(messageId)).ReturnsAsync(originalMessage);
        _mockMessagesRepo.Setup(r => r.UpdateAsync(It.IsAny<Message>())).Returns(Task.CompletedTask);

        // Act
        var result = await _messageService.EditMessageAsync(editParams);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.OriginalMessage, Is.Not.Null);
        
        Assert.That(messageId, Is.EqualTo(result.OriginalMessage!.Id));
        
        _mockMessagesRepo.Verify(r => r.UpdateAsync(It.Is<Message>(m => 
            m.Id == messageId && 
            m.EncryptedContent == "New Content" && 
            m.IsEdited == true && 
            m.EditedAt == editParams.EditedAt)), Times.Once);
    }
    
    [Test]
    public async Task EditMessageAsync_MessageNotFound_ReturnsFailure()
    {
        // Arrange
        var editorId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var editParams = new EditMessage(editorId, messageId, "New Content", DateTime.UtcNow);

        _mockMessagesRepo.Setup(r => r.FindByIdAsync(messageId)).
            ThrowsAsync(new NotFoundByKeyException<Guid>(messageId));

        // Act
        var result = await _messageService.EditMessageAsync(editParams);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Exception, Is.Not.Null);
        Assert.That(result.Exception,Is.TypeOf<NotFoundByKeyException<Guid>>());
        Assert.That(result.OriginalMessage, Is.Null);
    }

    [Test]
    public async Task EditMessageAsync_NotSender_ReturnsFailure()
    {
        // Arrange
        var editorId = Guid.NewGuid();
        var senderId = Guid.NewGuid(); // Different from editorId
        var messageId = Guid.NewGuid();
        var originalMessage = new Message { Id = messageId, SenderId = senderId, EncryptedContent = "Old" };
        var editParams = new EditMessage(editorId, messageId, "New Content", DateTime.UtcNow);

        _mockMessagesRepo.Setup(r => r.FindByIdAsync(messageId)).ReturnsAsync(originalMessage);

        // Act
        var result = await _messageService.EditMessageAsync(editParams);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Exception, Is.Not.Null);
        Assert.That(result.Exception,Is.TypeOf<UnauthorizedAccessException>());
        Assert.That(result.OriginalMessage, Is.Null);
    }
    
    // Test for DeleteMessageAsync action
    [Test]
    public async Task DeleteMessageAsync_Success()
    {
        // Arrange
        var deleterId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var originalMessage = new Message { Id = messageId, SenderId = deleterId, RecipientId = Guid.NewGuid(), RecipientType = RecipientType.User };
        var deleteParams = new DeleteMessage(deleterId, messageId);

        _mockMessagesRepo.Setup(r => r.FindByIdAsync(messageId)).ReturnsAsync(originalMessage);
        _mockMessagesRepo.Setup(r => r.RemoveAsync(messageId)).Returns(Task.CompletedTask);

        // Act
        var result = await _messageService.DeleteMessageAsync(deleteParams);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.OriginalMessage, Is.Not.Null);
        Assert.That(messageId, Is.EqualTo(result.OriginalMessage!.Id));
        _mockMessagesRepo.Verify(r => r.RemoveAsync(messageId), Times.Once);
    }
    
    [Test]
    public async Task DeleteMessageAsync_MessageNotFound_ReturnsFailure()
    {
        // Arrange
        var deleterId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var deleteParams = new DeleteMessage(deleterId, messageId);

        _mockMessagesRepo.Setup(r => r.FindByIdAsync(messageId)).
            ThrowsAsync(new NotFoundByKeyException<Guid>(messageId));

        // Act
        var result = await _messageService.DeleteMessageAsync(deleteParams);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Exception, Is.Not.Null);
        Assert.That(result.Exception,Is.TypeOf<NotFoundByKeyException<Guid>>());
        Assert.That(result.OriginalMessage, Is.Null);
    }

    [Test]
    public async Task DeleteMessageAsync_NotSender_ReturnsFailure()
    {
        // Arrange
        var deleterId = Guid.NewGuid();
        var senderId = Guid.NewGuid(); // Different
        var messageId = Guid.NewGuid();
        var originalMessage = new Message { Id = messageId, SenderId = senderId };
        var deleteParams = new DeleteMessage(deleterId, messageId);
        
        _mockMessagesRepo.Setup(r => r.FindByIdAsync(messageId)).ReturnsAsync(originalMessage);

        // Act
        var result = await _messageService.DeleteMessageAsync(deleteParams);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Exception, Is.Not.Null);
        Assert.That(result.Exception,Is.TypeOf<UnauthorizedAccessException>());
        Assert.That(result.OriginalMessage, Is.Null);
    }
}