using Moq;
using Microsoft.Extensions.Logging;
using Govor.Application.Services;
using Govor.Application.Interfaces;
using Govor.Application.Interfaces.Messages;
using Govor.Application.Interfaces.Messages.Parameters;
using Govor.Core.Models;
using Govor.Core.Repositories.Messages;
using Govor.Core.Repositories.Users;
using Govor.Core.Repositories.Groups;
using Xunit;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Govor.Application.Exceptions.VerifyFriendship; // Assuming this exception type

namespace Govor.API.Tests.UnitTests.Services;

public class MessageServiceTests
{
    private readonly Mock<IMessagesRepository> _mockMessagesRepo;
    private readonly Mock<IUsersRepository> _mockUsersRepo;
    private readonly Mock<IGroupsRepository> _mockGroupsRepo;
    private readonly Mock<IVerifyFriendship> _mockVerifyFriendship;
    private readonly Mock<ILogger<MessageService>> _mockLogger;
    private readonly MessageService _messageService;

    public MessageServiceTests()
    {
        _mockMessagesRepo = new Mock<IMessagesRepository>();
        _mockUsersRepo = new Mock<IUsersRepository>();
        _mockGroupsRepo = new Mock<IGroupsRepository>();
        _mockVerifyFriendship = new Mock<IVerifyFriendship>();
        _mockLogger = new Mock<ILogger<MessageService>>();

        _messageService = new MessageService(
            _mockMessagesRepo.Object,
            _mockUsersRepo.Object,
            _mockGroupsRepo.Object,
            _mockVerifyFriendship.Object,
            _mockLogger.Object);
    }

    // --- SendMessageAsync Tests ---

    [Fact]
    public async Task SendMessageAsync_ToUser_Success()
    {
        // Arrange
        var senderId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        var sendMessageParams = new SendMessage("Hello", null, recipientId, RecipientType.User, senderId, DateTime.UtcNow, new List<SendMedia>());

        _mockUsersRepo.Setup(r => r.ExistsByIdAsync(recipientId)).ReturnsAsync(true);
        _mockVerifyFriendship.Setup(v => v.VerifyAsync(senderId, recipientId)).Returns(Task.CompletedTask);
        _mockMessagesRepo.Setup(r => r.AddAsync(It.IsAny<Message>())).Returns(Task.CompletedTask);

        // Act
        var result = await _messageService.SendMessageAsync(sendMessageParams);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Exception);
        Assert.NotEqual(Guid.Empty, result.MessageId);
        _mockMessagesRepo.Verify(r => r.AddAsync(It.Is<Message>(m =>
            m.SenderId == senderId &&
            m.RecipientId == recipientId &&
            m.RecipientType == RecipientType.User &&
            m.EncryptedContent == "Hello")), Times.Once);
    }

    [Fact]
    public async Task SendMessageAsync_ToGroup_Success()
    {
        // Arrange
        var senderId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var sendMessageParams = new SendMessage("Hello Group", null, groupId, RecipientType.Group, senderId, DateTime.UtcNow, new List<SendMedia>());

        _mockGroupsRepo.Setup(r => r.ExistsAsync(groupId)).ReturnsAsync(true);
        // _mockGroupsRepo.Setup(r => r.IsUserMemberOfGroupAsync(senderId, groupId)).ReturnsAsync(true); // Assuming membership check
        _mockMessagesRepo.Setup(r => r.AddAsync(It.IsAny<Message>())).Returns(Task.CompletedTask);

        // Act
        var result = await _messageService.SendMessageAsync(sendMessageParams);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.MessageId);
        _mockMessagesRepo.Verify(r => r.AddAsync(It.Is<Message>(m => m.RecipientId == groupId && m.RecipientType == RecipientType.Group)), Times.Once);
    }

    [Fact]
    public async Task SendMessageAsync_ToUser_RecipientNotFound_ReturnsFailure()
    {
        // Arrange
        var senderId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        var sendMessageParams = new SendMessage("Hello", null, recipientId, RecipientType.User, senderId, DateTime.UtcNow, new List<SendMedia>());

        _mockUsersRepo.Setup(r => r.ExistsByIdAsync(recipientId)).ReturnsAsync(false);

        // Act
        var result = await _messageService.SendMessageAsync(sendMessageParams);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Exception);
        Assert.IsType<KeyNotFoundException>(result.Exception);
        Assert.Equal(Guid.Empty, result.MessageId);
    }

    [Fact]
    public async Task SendMessageAsync_ToUser_FriendshipVerificationFails_ReturnsFailure()
    {
        // Arrange
        var senderId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();
        var sendMessageParams = new SendMessage("Hello", null, recipientId, RecipientType.User, senderId, DateTime.UtcNow, new List<SendMedia>());

        _mockUsersRepo.Setup(r => r.ExistsByIdAsync(recipientId)).ReturnsAsync(true);
        _mockVerifyFriendship.Setup(v => v.VerifyAsync(senderId, recipientId)).ThrowsAsync(new FriendshipException("Not friends"));

        // Act
        var result = await _messageService.SendMessageAsync(sendMessageParams);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Exception);
        Assert.IsType<FriendshipException>(result.Exception);
        Assert.Equal(Guid.Empty, result.MessageId);
    }


    // --- EditMessageAsync Tests ---

    [Fact]
    public async Task EditMessageAsync_Success()
    {
        // Arrange
        var editorId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var originalMessage = new Message { Id = messageId, SenderId = editorId, EncryptedContent = "Old", RecipientId = Guid.NewGuid(), RecipientType = RecipientType.User };
        var editParams = new EditMessage(editorId, messageId, "New Content", DateTime.UtcNow);

        _mockMessagesRepo.Setup(r => r.GetByIdAsync(messageId)).ReturnsAsync(originalMessage);
        _mockMessagesRepo.Setup(r => r.UpdateAsync(It.IsAny<Message>())).Returns(Task.CompletedTask);

        // Act
        var result = await _messageService.EditMessageAsync(editParams);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.OriginalMessage);
        Assert.Equal(messageId, result.OriginalMessage!.Id);
        _mockMessagesRepo.Verify(r => r.UpdateAsync(It.Is<Message>(m =>
            m.Id == messageId &&
            m.EncryptedContent == "New Content" &&
            m.IsEdited == true &&
            m.EditedAt == editParams.EditedAt)), Times.Once);
    }

    [Fact]
    public async Task EditMessageAsync_MessageNotFound_ReturnsFailure()
    {
        // Arrange
        var editorId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var editParams = new EditMessage(editorId, messageId, "New Content", DateTime.UtcNow);

        _mockMessagesRepo.Setup(r => r.GetByIdAsync(messageId)).ReturnsAsync((Message?)null);

        // Act
        var result = await _messageService.EditMessageAsync(editParams);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.IsType<KeyNotFoundException>(result.Exception);
        Assert.Null(result.OriginalMessage);
    }

    [Fact]
    public async Task EditMessageAsync_NotSender_ReturnsFailure()
    {
        // Arrange
        var editorId = Guid.NewGuid();
        var senderId = Guid.NewGuid(); // Different from editorId
        var messageId = Guid.NewGuid();
        var originalMessage = new Message { Id = messageId, SenderId = senderId, EncryptedContent = "Old" };
        var editParams = new EditMessage(editorId, messageId, "New Content", DateTime.UtcNow);

        _mockMessagesRepo.Setup(r => r.GetByIdAsync(messageId)).ReturnsAsync(originalMessage);

        // Act
        var result = await _messageService.EditMessageAsync(editParams);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.IsType<UnauthorizedAccessException>(result.Exception);
        Assert.Null(result.OriginalMessage);
    }

    // --- DeleteMessageAsync Tests ---

    [Fact]
    public async Task DeleteMessageAsync_Success()
    {
        // Arrange
        var deleterId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var originalMessage = new Message { Id = messageId, SenderId = deleterId, RecipientId = Guid.NewGuid(), RecipientType = RecipientType.User };
        var deleteParams = new DeleteMessage(deleterId, messageId);

        _mockMessagesRepo.Setup(r => r.GetByIdAsync(messageId)).ReturnsAsync(originalMessage);
        // For soft delete, the repo's DeleteAsync would internally mark IsDeleted=true and save.
        // If it were hard delete, it would be _mockMessagesRepo.Setup(r => r.DeleteAsync(messageId)).Returns(Task.CompletedTask);
        // Assuming DeleteAsync in repo handles the soft delete logic (sets IsDeleted, DeletedAt, then calls UpdateAsync or similar)
        // For this test, we verify that the service calls the repo's DeleteAsync method.
        // The actual soft delete implementation is tested at the repository level.
        // However, MessageService is responsible for *initiating* the delete.
        // If MessageService directly manipulated IsDeleted, we'd mock repo.UpdateAsync.
        // Since it calls repo.DeleteAsync, we mock that.
        _mockMessagesRepo.Setup(r => r.DeleteAsync(messageId)).Returns(Task.CompletedTask);


        // Act
        var result = await _messageService.DeleteMessageAsync(deleteParams);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.OriginalMessage);
        Assert.Equal(messageId, result.OriginalMessage!.Id);
        _mockMessagesRepo.Verify(r => r.DeleteAsync(messageId), Times.Once);
    }

    [Fact]
    public async Task DeleteMessageAsync_MessageNotFound_ReturnsFailure()
    {
        // Arrange
        var deleterId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var deleteParams = new DeleteMessage(deleterId, messageId);

        _mockMessagesRepo.Setup(r => r.GetByIdAsync(messageId)).ReturnsAsync((Message?)null);

        // Act
        var result = await _messageService.DeleteMessageAsync(deleteParams);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.IsType<KeyNotFoundException>(result.Exception);
         Assert.Null(result.OriginalMessage);
    }

    [Fact]
    public async Task DeleteMessageAsync_NotSender_ReturnsFailure()
    {
        // Arrange
        var deleterId = Guid.NewGuid();
        var senderId = Guid.NewGuid(); // Different
        var messageId = Guid.NewGuid();
        var originalMessage = new Message { Id = messageId, SenderId = senderId };
        var deleteParams = new DeleteMessage(deleterId, messageId);

        _mockMessagesRepo.Setup(r => r.GetByIdAsync(messageId)).ReturnsAsync(originalMessage);

        // Act
        var result = await _messageService.DeleteMessageAsync(deleteParams);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.IsType<UnauthorizedAccessException>(result.Exception);
        Assert.Null(result.OriginalMessage);
    }
}
