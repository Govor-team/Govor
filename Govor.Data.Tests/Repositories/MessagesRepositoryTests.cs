using AutoFixture;
using Govor.Domain.Infrastructure.Validators;
using Govor.Domain.Models;
using Govor.Domain.Models.Messages;
using Govor.Domain;
using Govor.Domain.Repositories;
using Govor.Domain.Repositories.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Govor.Domain.Tests.Repositories;

[TestFixture]
public class MessagesRepositoryTests
{
    private Fixture _fixture;
    private DbContextOptions<GovorDbContext> _options;
    private readonly IObjectValidator<Message> _messageValidator = new MessageValidator();
    private int _testIteration = 0;
    
    [SetUp]
    public void SetUp()
    {
        _testIteration += 1;
        
        _fixture = new Fixture();

        _fixture.Behaviors
            .OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _options = new DbContextOptionsBuilder<GovorDbContext>()
            .UseInMemoryDatabase(databaseName: $"DbGovor_{nameof(MessagesRepositoryTests)}_{_testIteration}")
            .Options;
    }

    [Test]
    public async Task Given_NotEmptySetDb_When_GetAllMessages_Then_ReturnAllMessages()
    {
        // Arrange
        var random = new Random();
        var messages = _fixture.CreateMany<Message>(random.Next(2, 10)).ToList();

        await using var context = new GovorDbContext(_options);
        var messagesRepository = new MessagesRepository(context, _messageValidator);

        context.Messages.AddRange(messages);
        await context.SaveChangesAsync();

        // Act 

        var result = await messagesRepository.GetAllAsync();

        // Assert 
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(messages.Count));
        Assert.That(result, Is.EquivalentTo(messages));
    }
    
    [Test]
    public void Given_EmptySetDb_When_GetAllMessages_Should_Throw_NotFoundException()
    {
        // Arrange 
        using var context = new GovorDbContext(_options);
        var messagesRepository = new MessagesRepository(context, _messageValidator);
        // Act & Assert 
        Assert.ThrowsAsync<NotFoundException>(async () => await messagesRepository.GetAllAsync());
    }
    
    [Test]
    public async Task Given_ValidMessageId_When_FindMessageById_Then_ReturnMessage()
    {
        // Arrange 
        var messages = _fixture.CreateMany<Message>(10);
        var id = messages.First().Id;
        
        await using var context = new GovorDbContext(_options);
        var messagesRepository = new MessagesRepository(context, _messageValidator);
        
        context.Messages.AddRange(messages);
        await context.SaveChangesAsync();
        
        // Act
        var result = await messagesRepository.FindByIdAsync(id);
        
        // Assert 
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo(messages.First()));
    }
    
    [Test]
    public async Task Given_InvalidMessageId_When_FindById_Should_Throw_NotFoundByKeyException()
    {
        // Arrange 
        await using var context = new GovorDbContext(_options);
        var messagesRepository = new MessagesRepository(context, _messageValidator);
        
        // Act & Assert 
        Assert.ThrowsAsync<NotFoundByKeyException<Guid>>(async () => await messagesRepository.FindByIdAsync(_fixture.Create<Guid>()));
    }

    [Test]
    public async Task Given_ValidSenderId_When_FindBySenderIdAsync_Then_ReturnMessages()
    {
        // Arrange 
        var messages = _fixture.CreateMany<Message>(10).ToList();
        var senderId =_fixture.Create<Guid>();
        
        foreach (var message in messages)
        {
            message.SenderId = senderId;
        }
        
        await using var context = new GovorDbContext(_options);
        var messagesRepository = new MessagesRepository(context, _messageValidator);
        
        context.Messages.AddRange(messages);
        await context.SaveChangesAsync();
        
        // Act
        var results = await messagesRepository.FindBySenderIdAsync(senderId);
        
        // Assert 
        Assert.That(results, Is.Not.Null);
        Assert.That(results, Is.EquivalentTo(messages));
    }
    
    [Test]
    public void Given_InvalidSenderId_When_FindBySenderId_Should_Throw_NotFoundByKeyException()
    {  
        // Arrange 
        using var context = new GovorDbContext(_options);
        var messagesRepository = new MessagesRepository(context, _messageValidator);
        
        // Act & Assert 
        Assert.ThrowsAsync<NotFoundByKeyException<Guid>>(async () => await messagesRepository.FindBySenderIdAsync(_fixture.Create<Guid>()));
    }
    
    [Test]
    public async Task Given_ValidReceiverId_When_FindByReceiverIdAsync_Then_ReturnMessages()
    {
        // Arrange 
        var messages = _fixture.CreateMany<Message>(10).ToList();
        var receiverId =_fixture.Create<Guid>();
        
        foreach (var message in messages)
        {
            message.RecipientId = receiverId;
        }
        
        await using var context = new GovorDbContext(_options);
        var messagesRepository = new MessagesRepository(context, _messageValidator);
        
        context.Messages.AddRange(messages);
        await context.SaveChangesAsync();
        
        // Act
        var results = await messagesRepository.FindByReceiverIdAsync(receiverId);
        
        // Assert 
        Assert.That(results, Is.Not.Null);
        Assert.That(results, Is.EquivalentTo(messages));
    }
    
    [Test]
    public void Given_InvalidReceiverId_When_FindByReceiverIdAsync_Should_Throw_NotFoundByKeyException()
    {  
        // Arrange 
        using var context = new GovorDbContext(_options);
        var messagesRepository = new MessagesRepository(context, _messageValidator);
        
        // Act & Assert 
        Assert.ThrowsAsync<NotFoundByKeyException<Guid>>(async () => await messagesRepository.FindByReceiverIdAsync(_fixture.Create<Guid>()));
    }
    
    [Test]
    public async Task Given_ValidSenderIdReceiverIdAndReceiverType_When_FindBySenderAndReceiverIdAsync_Then_ReturnMessages()
    {
        // Arrange 
        var messages = _fixture.CreateMany<Message>(10).ToList();
        
        var receiverId =_fixture.Create<Guid>();
        var senderId =_fixture.Create<Guid>();
        RecipientType recipientType =_fixture.Create<RecipientType>();
        
        foreach (var message in messages)
        {
            message.RecipientId = receiverId;
            message.SenderId = senderId;
            message.RecipientType = recipientType;
        }
        
        await using var context = new GovorDbContext(_options);
        var messagesRepository = new MessagesRepository(context, _messageValidator);
        
        context.Messages.AddRange(messages);
        await context.SaveChangesAsync();
        
        // Act 
        var results = await messagesRepository.FindBySenderAndReceiverIdAsync(senderId, receiverId, recipientType);
        
        Assert.That(results, Is.Not.Null);
        Assert.That(results, Is.EquivalentTo(messages));
    }
    
    [Test]
    public async Task Given_InvalidSenderId_When_FindBySenderAndReceiverIdAsync_Should_Throw_NotFoundByKeyException()
    {
        // Arrange 
        var messages = _fixture.CreateMany<Message>(10).ToList();
        
        var receiverId =_fixture.Create<Guid>();
        var senderId =_fixture.Create<Guid>();
        RecipientType recipientType =_fixture.Create<RecipientType>();
        
        foreach (var message in messages)
        {
            message.RecipientId = receiverId;
            message.RecipientType = recipientType;
        }
        
        await using var context = new GovorDbContext(_options);
        var messagesRepository = new MessagesRepository(context, _messageValidator);
        
        context.Messages.AddRange(messages);
        await context.SaveChangesAsync();
        
        // Act & Assert 
        Assert.ThrowsAsync<NotFoundException>(async () => await messagesRepository.FindBySenderAndReceiverIdAsync(senderId, receiverId, recipientType));
    }
        
    [Test]
    public async Task Given_InvalidReceiverId_When_FindBySenderAndReceiverIdAsync_Should_Throw_NotFoundByKeyException()
    {
        // Arrange 
        var messages = _fixture.CreateMany<Message>(10).ToList();
        
        var receiverId =_fixture.Create<Guid>();
        var senderId =_fixture.Create<Guid>();
        RecipientType recipientType =_fixture.Create<RecipientType>();
        
        foreach (var message in messages)
        {
            message.SenderId = senderId;
            message.RecipientType = recipientType;
        }
        
        await using var context = new GovorDbContext(_options);
        var messagesRepository = new MessagesRepository(context, _messageValidator);
        
        context.Messages.AddRange(messages);
        await context.SaveChangesAsync();
        
        // Act & Assert 
        Assert.ThrowsAsync<NotFoundException>(async () => await messagesRepository.FindBySenderAndReceiverIdAsync(senderId, receiverId, recipientType));
    }
    
    [Test]
    public async Task Given_ValidDateTime_When_FindByValidDateTimeAsync_Then_ReturnMessages()
    {
        // Assert 
        var messages = _fixture.CreateMany<Message>(10).ToList();
        var dateTime = _fixture.Create<DateTime>();

        foreach (var message in messages)
        {
            message.SentAt = dateTime;
        }
        
        await using var context = new GovorDbContext(_options);
        var messagesRepository = new MessagesRepository(context, _messageValidator);
        
        context.Messages.AddRange(messages);
        await context.SaveChangesAsync();
        
        // Act 
        var results = await messagesRepository.FindBySentAtAsync(dateTime);
        
        // Assert 
        Assert.That(results, Is.Not.Null);
        Assert.That(results, Is.EquivalentTo(messages));
    }

    [Test]
    public async Task Given_ValidMessage_When_AddAsync_Then_MessageAdded()
    {
        // Arrange
        var message = _fixture.Create<Message>();
        
        await using var context = new GovorDbContext(_options);
        var messagesRepository = new MessagesRepository(context, _messageValidator);
    
        // Act 
        await messagesRepository.AddAsync(message);
        
        // Assert 
        Assert.That(context.Messages.Count, Is.EqualTo(1));
        Assert.That(context.Messages.First(), Is.EqualTo(message));
    }
    
    [Test]
    public async Task Given_InvalidMessage_When_AddAsync_Should_Throw_AdditionException()
    {
        // Arrange
        await using var context = new GovorDbContext(_options);
        var messagesRepository = new MessagesRepository(context, _messageValidator);
        
        // Act & Assert 
        Assert.ThrowsAsync<AdditionException>(async () => await messagesRepository.AddAsync(default));
    }
    
    [Test]
    public async Task Given_ExistMessage_When_Exist_Then_ReturnTrue()
    {
        // Arrange
        var message = _fixture.Create<Message>();
        
        await using var context = new GovorDbContext(_options);
        var messagesRepository = new MessagesRepository(context, _messageValidator);
        
        context.Messages.Add(message);
        await context.SaveChangesAsync();
        
        // Act 
        var result = messagesRepository.Exist(message);
        var result2 = messagesRepository.Exist(message.Id);
        
        // Assert 
        Assert.That(result, Is.True);
        Assert.That(result2, Is.True);
    }

    [Test]
    public async Task Given_NotExistMessage_When_Exist_Then_ReturnFalse()
    {
        // Arrange
        var message = _fixture.Create<Message>();
        
        await using var context = new GovorDbContext(_options);
        var messagesRepository = new MessagesRepository(context, _messageValidator);
        
        // Act 
        var result = messagesRepository.Exist(message);
        var result2 = messagesRepository.Exist(message.Id);
        
        // Assert 
        Assert.That(result, Is.False);
        Assert.That(result2, Is.False);
    }
    
    [Test]
    public async Task Given_NotEqualMessage_When_Exist_Then_ReturnFalse()
    {
        // Arrange
        var message = _fixture.Create<Message>();
        var message2 = _fixture.Create<Message>();
        message.Id = message2.Id;
        
        await using var context = new GovorDbContext(_options);
        var messagesRepository = new MessagesRepository(context, _messageValidator);
        
        context.Messages.Add(message);
        await context.SaveChangesAsync();
        
        // Act 
        var result = messagesRepository.Exist(message2);
        
        // Assert 
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task Given_InvalidMessage_When_Exist_Should_Throw_InvalidObjectException()
    {
        // Arrange 
        var message = _fixture.Create<Message>();
        message.SentAt = DateTime.MinValue;
        message.RecipientId = Guid.Empty;
        
        await using var context = new GovorDbContext(_options);
        var messagesRepository = new MessagesRepository(context, _messageValidator);
        
        // Act & Assert 
        Assert.Throws<InvalidObjectException<Message>>(() => messagesRepository.Exist(message));
    }

    [Test]
    public void Given_NullMessage_When_Exist_Should_Throw_InvalidObjectException()
    {
        using var context = new GovorDbContext(_options);
        var messagesRepository = new MessagesRepository(context, _messageValidator);
        
        // Act & Assert 
        Assert.Throws<InvalidObjectException<Message>>(() => messagesRepository.Exist(default(Message)));
    }
}