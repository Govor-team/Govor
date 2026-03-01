using Govor.Application.Exceptions.VerifyFriendship;
using Govor.Application.Interfaces;
using Govor.Application.Interfaces.Medias;
using Govor.Application.Interfaces.Messages.Parameters;
using Govor.Application.Services.Messages;
using Govor.Core.Models;
using Govor.Core.Models.Messages;
using Govor.Core.Repositories.Groups;
using Govor.Core.Repositories.Messages;
using Govor.Core.Repositories.PrivateChats;
using Govor.Core.Repositories.Users;
using Govor.Data.Repositories.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Govor.Application.Tests.Services.Messages;

[TestFixture]
public class MessageCommandServiceTests
{
    private Mock<IMessagesRepository> _mockMessagesRepo;
    private Mock<IUsersRepository> _mockUsersRepo;
    private Mock<IGroupsRepository> _mockGroupsRepo;
    private Mock<IPrivateChatsRepository> _mockPrivateChatsRepo;
    private Mock<IVerifyFriendship> _mockVerifyFriendship;
    private Mock<IMediaService> _mockMediaService;
    private Mock<IUserPrivateChatsCreator> _mockUserPrivateChatCreator;
    private Mock<ILogger<MessageCommandService>> _mockLogger;
    private MessageCommandService _messageService;
    
    [SetUp]
    public void SetUp()
    {
        _mockMessagesRepo = new Mock<IMessagesRepository>();
        _mockUsersRepo = new Mock<IUsersRepository>();
        _mockGroupsRepo = new Mock<IGroupsRepository>();
        _mockPrivateChatsRepo = new Mock<IPrivateChatsRepository>();
        _mockVerifyFriendship = new Mock<IVerifyFriendship>();
        _mockMediaService = new Mock<IMediaService>();
        _mockUserPrivateChatCreator = new Mock<IUserPrivateChatsCreator>();
        _mockLogger = new Mock<ILogger<MessageCommandService>>();

        _messageService = new MessageCommandService(
            _mockMessagesRepo.Object,
            _mockUsersRepo.Object,
            _mockGroupsRepo.Object,
            _mockPrivateChatsRepo.Object,
            _mockUserPrivateChatCreator.Object,
            _mockVerifyFriendship.Object,
            _mockMediaService.Object,
            _mockLogger.Object);
    }

    
    // Test for SendMessageAsync action
    [Test]
    public async Task SendMessageAsync_ToUser_Success()
    {
        // Arrange
        var senderId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();

        var privateChat = new PrivateChat
        {
            Id = recipientId,
            UserAId = senderId,
            UserBId = Guid.NewGuid()
        };

        var sendParams = new SendMessage(
            "Hello",
            null,
            recipientId,
            RecipientType.User,
            senderId,
            DateTime.UtcNow,
            new List<SendMedia>());

        _mockPrivateChatsRepo
            .Setup(r => r.GetByIdAsync(recipientId))
            .ReturnsAsync(privateChat);

        _mockMessagesRepo
            .Setup(r => r.AddAsync(It.IsAny<Message>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _messageService.SendMessageAsync(sendParams);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Exception, Is.Null);

        _mockMessagesRepo.Verify(r => r.AddAsync(It.Is<Message>(m =>
            m.SenderId == senderId &&
            m.RecipientId == recipientId &&
            m.RecipientType == RecipientType.User &&
            m.EncryptedContent == "Hello")), Times.Once);
    }
    
    [Test]
    public async Task SendMessageAsync_ToUser_When_AttachMediaThrowsException_ReturnsFailure()
    {
        // Arrange
        var senderId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        var sendMediaId = Guid.NewGuid();

        var privateChat = new PrivateChat
        {
            Id = recipientId,
            UserAId = senderId,
            UserBId = recipientId
        };

        var sendParams = new SendMessage(
            "Hello",
            null,
            recipientId,
            RecipientType.User,
            senderId,
            DateTime.UtcNow,
            new List<SendMedia>
            {
                new SendMedia(sendMediaId, string.Empty)
            });

        _mockPrivateChatsRepo
            .Setup(r => r.GetByIdAsync(recipientId))
            .ReturnsAsync(privateChat);

        _mockMediaService
            .Setup(m => m.AttachToMessageAsync(sendMediaId, It.IsAny<Guid>()))
            .ThrowsAsync(new Exception("Unexpected DB error"));

        // Act
        var result = await _messageService.SendMessageAsync(sendParams);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Exception!.Message, Is.EqualTo("Unexpected DB error"));

        _mockMessagesRepo.Verify(r => r.AddAsync(It.IsAny<Message>()), Times.Never);
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

        _mockGroupsRepo.Setup(r => r.Exist(groupId)).Returns(true);
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
    public async Task SendMessageAsync_ToUser_PrivateChatNotFound_ReturnsFailure()
    {
        // Arrange
        var senderId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();

        var sendParams = new SendMessage(
            "Hello",
            null,
            recipientId,
            RecipientType.User,
            senderId,
            DateTime.UtcNow,
            new List<SendMedia>());

        _mockPrivateChatsRepo
            .Setup(r => r.GetByIdAsync(recipientId))
            .ReturnsAsync((PrivateChat?)null);

        // Act
        var result = await _messageService.SendMessageAsync(sendParams);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Exception, Is.TypeOf<KeyNotFoundException>());
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
        Assert.That(result.Exception,Is.TypeOf<KeyNotFoundException>());
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