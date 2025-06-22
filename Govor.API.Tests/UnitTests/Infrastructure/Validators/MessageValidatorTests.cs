using AutoFixture;
using Govor.Core.Infrastructure.Validators;
using Govor.Core.Models;

namespace Govor.API.Tests.UnitTests.Infrastructure.Validators;

[TestFixture]
public class MessageValidatorTests
{
    private IObjectValidator<Message> _messageValidator;
    private Fixture _fixture;

    public MessageValidatorTests()
    {
        _messageValidator = new MessageValidator();
        
        _fixture = new Fixture();

        _fixture.Behaviors
            .OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }
    
    [Test]
    public void Given_NullMessage_When_Validate_Should_Throw_InvalidObjectException()
    {
        // Act & Assert 
        Assert.ThrowsAsync<InvalidObjectException<Message>>(async () => _messageValidator.Validate(default));
    }
    
    [Test]
    public void Given_NullMessage_When_TryValidate_Then_Returns_False()
    {
        // Act & Assert 
        Assert.That(_messageValidator.TryValidate(default), Is.False);
    }
    
    [Test]
    public void Given_EmptyIdMessage_When_Validate_Should_Throw_InvalidObjectException()
    {
        // Arrange
        Message message = _fixture.Create<Message>();
        message.Id = Guid.Empty;
        
        // Act & Assert 
        Assert.ThrowsAsync<InvalidObjectException<Message>>(async () => _messageValidator.Validate(message));
        Assert.That(_messageValidator.TryValidate(message), Is.False);
    }
    
    [Test]
    public void Given_EmptySenderIdMessage_When_Validate_Should_Throw_InvalidObjectException()
    {
        // Arrange
        Message message = _fixture.Create<Message>();
        message.SenderId = Guid.Empty;
        
        // Act & Assert 
        Assert.ThrowsAsync<InvalidObjectException<Message>>(async () => _messageValidator.Validate(message));
        Assert.That(_messageValidator.TryValidate(message), Is.False);
    }
    
    [Test]
    public void Given_EmptyRecipientIdMessage_When_Validate_Should_Throw_InvalidObjectException()
    {
        // Arrange
        Message message = _fixture.Create<Message>();
        message.RecipientId = Guid.Empty;
        
        // Act & Assert 
        Assert.ThrowsAsync<InvalidObjectException<Message>>(async () => _messageValidator.Validate(message));
        Assert.That(_messageValidator.TryValidate(message), Is.False);
    }
    
    [Test]
    public void Given_EmptyEncryptedMessageAndNotEmptyMedia_When_Validate_Should_Throw_InvalidObjectException()
    {
        // Arrange
        Message message = _fixture.Create<Message>();
        message.EncryptedContent = string.Empty;
        
        Assert.DoesNotThrowAsync(async () => _messageValidator.Validate(message));
        Assert.That(_messageValidator.TryValidate(message), Is.True);
    }
    
    [Test]
    public void Given_EmptyEncryptedMessageAndEmptyMedia_When_Validate_Should_Throw_InvalidObjectException()
    {
        // Arrange
        Message message = _fixture.Create<Message>();
        message.EncryptedContent = string.Empty;
        message.MediaAttachments = default;
        
        Assert.ThrowsAsync<InvalidObjectException<Message>>(async () => _messageValidator.Validate(message));
        Assert.That(_messageValidator.TryValidate(message), Is.False);
    }

    [Test]
    public void Given_IsEditAndEmptyEditeTimeMessage_When_Validate_Should_Throw_InvalidObjectException()
    {
        // Arrange
        Message message = _fixture.Create<Message>();
        message.IsEdited = true;
        message.EditedAt = DateTime.MinValue;
        
        Assert.ThrowsAsync<InvalidObjectException<Message>>(async () => _messageValidator.Validate(message));
        Assert.That(_messageValidator.TryValidate(message), Is.False);
    }

    [Test]
    public void Given_EmptySentAtMessage_When_Validate_Should_Throw_InvalidObjectException()
    {
        // Arrange
        Message message = _fixture.Create<Message>();
        message.SentAt = DateTime.MinValue;
        
        Assert.ThrowsAsync<InvalidObjectException<Message>>(async () => _messageValidator.Validate(message));
        Assert.That(_messageValidator.TryValidate(message), Is.False);
    }

    [Test]
    public void Given_ValidMessage_When_Validate_Then_Should_NotThrow()
    {
        // Arrange
        Message message = _fixture.Create<Message>();
        
        // Act & Assert 
        Assert.DoesNotThrowAsync(async () => _messageValidator.Validate(message));
        Assert.That(_messageValidator.TryValidate(message), Is.True);
    }
}