using AutoFixture;
using Govor.Core.Infrastructure.Validators;
using Govor.Core.Models;
using Govor.Data;
using Govor.Data.Repositories;
using Govor.Data.Repositories.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Govor.API.Tests.IntegrationTests.EF.Repositories;

[TestFixture]
public class PrivateChatsRepositoryTests
{
    private Fixture _fixture;
    private DbContextOptions<GovorDbContext> _options;
    private IObjectValidator<PrivateChat> _validator = new PrivateChatValidator();
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
            .UseInMemoryDatabase(databaseName: $"DbGovor_{nameof(PrivateChatsRepositoryTests)}_{_testIteration}")
            .Options;
    }
    
    [Test]
    public async Task Given_NotEmptySetDb_When_GetAll_Then_ReturnAll()
    {
        // Arrange
        var random = new Random();
        var chats = _fixture.CreateMany<PrivateChat>(random.Next(2, 10)).ToList();

        await using var context = new GovorDbContext(_options);
        var messagesRepository = new PrivateChatsRepository(context, _validator);

        context.PrivateChats.AddRange(chats);
        await context.SaveChangesAsync();

        // Act 

        var result = await messagesRepository.GetAllAsync();

        // Assert 
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(chats.Count));
        Assert.That(result, Is.EquivalentTo(chats));
    }
    
    [Test]
    public void Given_EmptySetDb_When_GetAll_Should_Throw_NotFoundException()
    {
        // Arrange 
        using var context = new GovorDbContext(_options);
        var repository = new PrivateChatsRepository(context, _validator);
        // Act & Assert 
        Assert.ThrowsAsync<NotFoundException>(async () => await repository.GetAllAsync());
    }
    
    [Test]
    public async Task Given_ValidChatId_When_GetById_Then_ReturnChat()
    {
        // Arrange 
        var chats = _fixture.CreateMany<PrivateChat>(10);
        var id = chats.First().Id;
        
        await using var context = new GovorDbContext(_options);
        var repository = new PrivateChatsRepository(context, _validator);
        
        context.PrivateChats.AddRange(chats);
        await context.SaveChangesAsync();
        
        // Act
        var result = await repository.GetByIdAsync(id);
        
        // Assert 
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo(chats.First()));
    }
    
    [Test]
    public async Task Given_InvalidMessageId_When_FindById_Should_Throw_NotFoundByKeyException()
    {
        // Arrange 
        await using var context = new GovorDbContext(_options);
        var repository = new PrivateChatsRepository(context, _validator);
        
        // Act & Assert 
        Assert.ThrowsAsync<NotFoundByKeyException<Guid>>(async () => await repository.GetByIdAsync(_fixture.Create<Guid>()));
    }
    
    [Test]
    public async Task Given_MembersId_When_GetByMembersAsync_Then_ReturnChat()
    {
        // Arrange 
        var chats = _fixture.CreateMany<PrivateChat>(10);
        var id1 = chats.First().UserAId;
        var id2 = chats.First().UserBId;
        
        await using var context = new GovorDbContext(_options);
        var repository = new PrivateChatsRepository(context, _validator);
        
        context.PrivateChats.AddRange(chats);
        await context.SaveChangesAsync();
        
        // Act
        var result = await repository.GetByMembersAsync(id1, id2);
        
        // Assert 
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo(chats.First()));
    }
    
    [Test]
    public async Task Given_InvalidMembersId_When_GetByMembersAsync_Should_Throw_NotFoundByKeyException()
    {
        // Arrange 
        await using var context = new GovorDbContext(_options);
        var repository = new PrivateChatsRepository(context, _validator);
        
        // Act & Assert 
        Assert.ThrowsAsync<NotFoundByKeyException<(Guid, Guid)>>(async () => await repository.GetByMembersAsync(_fixture.Create<Guid>(), _fixture.Create<Guid>()));
    }
    
    [Test]
    public async Task Given_ValidMessage_When_AddAsync_Then_MessageAdded()
    {
        // Arrange
        var chat = _fixture.Create<PrivateChat>();
        
        await using var context = new GovorDbContext(_options);
        var repository = new PrivateChatsRepository(context, _validator);
    
        // Act 
        await repository.AddAsync(chat);
        
        // Assert 
        Assert.That(context.PrivateChats.Count, Is.EqualTo(1));
        Assert.That(context.PrivateChats.First(), Is.EqualTo(chat));
    }
    
    [Test]
    public async Task Given_InvalidMessage_When_AddAsync_Should_Throw_AdditionException()
    {
        // Arrange
        await using var context = new GovorDbContext(_options);
        var repository = new PrivateChatsRepository(context, _validator);
        
        // Act & Assert 
        Assert.ThrowsAsync<AdditionException>(async () => await repository.AddAsync(default));
    }
    
    [Test]
    public async Task Given_ExistMessage_When_Exist_Then_ReturnTrue()
    {
        // Arrange
        var chat = _fixture.Create<PrivateChat>();
        
        await using var context = new GovorDbContext(_options);
        var repository = new PrivateChatsRepository(context, _validator);
        
        context.PrivateChats.Add(chat);
        await context.SaveChangesAsync();
        
        // Act 
        var result1 = repository.Exist(chat.UserAId, chat.UserBId);
        var result2 = repository.Exist(chat.Id);
        
        
        // Assert 
        Assert.That(result1, Is.True);
        Assert.That(result2, Is.True);
    }

    [Test]
    public async Task Given_NotExistMessage_When_Exist_Then_ReturnFalse()
    {
        // Arrange
        var chat = _fixture.Create<PrivateChat>();
        
        await using var context = new GovorDbContext(_options);
        var repository = new PrivateChatsRepository(context, _validator);
        
        // Act 
        var result1 = repository.Exist(chat.UserAId, chat.UserBId);
        var result2 = repository.Exist(chat.Id);
        
        // Assert 
        Assert.That(result1, Is.False);
        Assert.That(result2, Is.False);
    }
}