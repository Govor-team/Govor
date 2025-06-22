using AutoFixture;
using Govor.Core.Infrastructure.Validators;
using Govor.Core.Models;
using Govor.Data;
using Govor.Data.Repositories;
using Govor.Data.Repositories.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Govor.API.Tests.IntegrationTests.EF.Repositories;

[TestFixture]
public class MessagesRepositoryTests
{
    private Fixture _fixture;
    private DbContextOptions<GovorDbContext> _options;
    private readonly IObjectValidator<Message> _messageValidator = new MessageValidator();

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();

        _fixture.Behaviors
            .OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _options = new DbContextOptionsBuilder<GovorDbContext>()
            .UseInMemoryDatabase(databaseName: "DbGovor")
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
        
    }
}