using AutoFixture;
using Govor.Domain.Infrastructure.Validators;
using Govor.Domain.Models;
using Govor.Domain;
using Govor.Domain.Repositories;
using Govor.Domain.Repositories.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Govor.Domain.Tests.Repositories;

[TestFixture]
public class GroupRepositoryTests
{
    private Fixture _fixture;
    private DbContextOptions<GovorDbContext> _options;
    private IObjectValidator<ChatGroup> _validator = new ChatGroupValidator();
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
            .UseInMemoryDatabase(databaseName: $"DbGovor_{nameof(GroupRepositoryTests)}_{_testIteration}")
            .Options;
    }

    [Test]
    public async Task Given_NotEmptySetDb_When_GetAll_Then_ReturnAll()
    {
        // Arrange
        var random = new Random();
        var chats = _fixture.CreateMany<ChatGroup>(random.Next(2, 10)).ToList();

        await using var context = new GovorDbContext(_options);
        var repository = new GroupRepository(context, _validator);

        context.ChatGroups.AddRange(chats);
        await context.SaveChangesAsync();

        // Act 

        var result = await repository.GetAllAsync();

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
        var repository = new GroupRepository(context, _validator);

        // Act & Assert 
        Assert.ThrowsAsync<NotFoundException>(async () => await repository.GetAllAsync());
    }

    [Test]
    public async Task Given_ValidChatId_When_GetById_Then_ReturnChat()
    {
        // Arrange 
        var chats = _fixture.CreateMany<ChatGroup>(10);
        var id = chats.First().Id;

        await using var context = new GovorDbContext(_options);
        var repository = new GroupRepository(context, _validator);

        context.ChatGroups.AddRange(chats);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(id);

        // Assert 
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo(chats.First()));
    }

    [Test]
    public void Given_InvalidMessageId_When_FindById_Should_Throw_NotFoundByKeyException()
    {
        // Arrange 
        using var context = new GovorDbContext(_options);
        var repository = new GroupRepository(context, _validator);

        // Act & Assert 
        Assert.ThrowsAsync<NotFoundByKeyException<Guid>>(async () =>
            await repository.GetByIdAsync(_fixture.Create<Guid>()));
    }

    [Test]
    public async Task Given_ValidQuery_When_SearchByNameAsync_Then_Returns_ChatGroups()
    {
        // Arrange
        var random = new Random();
        var chats = _fixture.CreateMany<ChatGroup>(random.Next(3, 10)).ToList();

        using var context = new GovorDbContext(_options);
        var repository = new GroupRepository(context, _validator);

        context.ChatGroups.AddRange(chats);
        await context.SaveChangesAsync();

        // Act 
        var result = await repository.SearchByNameAsync(chats[0].Name[..14]);

        // Assert 
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result.First(), Is.EqualTo(chats.First()));
    }

    [Test]
    public void Given_InvalidQuery_When_SearchPotentialFriendsAsync_Should_Throw_NotFoundByKeyException()
    {
        // Arrange 
        using var context = new GovorDbContext(_options);
        var repository = new GroupRepository(context, _validator);

        // Act & Assert 
        Assert.ThrowsAsync<NotFoundByKeyException<string>>(async () => await
            repository.SearchByNameAsync(_fixture.Create<string>()));
    }

    [Test]
    public async Task Given_ValidAdminId_When_GetByAdminIdAsync_Returns_ChatGroups()
    {
        // Arrange 
        var random = new Random();
        var chats = _fixture.CreateMany<ChatGroup>(random.Next(3, 10)).ToList();
        var adminId = chats.First().Admins.First().UserId;

        using var context = new GovorDbContext(_options);
        var repository = new GroupRepository(context, _validator);

        context.ChatGroups.AddRange(chats);
        await context.SaveChangesAsync();
        // Act 
        var result = await repository.GetByAdminIdAsync(adminId);
        // Assert 
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result.First(), Is.EqualTo(chats.First()));
    }

    [Test]
    public void Given_ValidInvalidAdminId_When_GetByAdminIdAsync_Throws_NotFoundByKeyException()
    {
        // Arrange 
        using var context = new GovorDbContext(_options);
        var repository = new GroupRepository(context, _validator);

        // Act & Assert 
        Assert.ThrowsAsync<NotFoundByKeyException<Guid>>(async () => await
            repository.GetByAdminIdAsync(_fixture.Create<Guid>()));
    }
    [Test]
    public async Task Given_ValidMemberId_When_GetByAdminIdAsync_Returns_ChatGroups()
    {
        // Arrange 
        var random = new Random();
        var chats = _fixture.CreateMany<ChatGroup>(random.Next(3, 10)).ToList();
        var userId = chats.First().Members.First().UserId;

        using var context = new GovorDbContext(_options);
        var repository = new GroupRepository(context, _validator);

        context.ChatGroups.AddRange(chats);
        await context.SaveChangesAsync();
        // Act 
        var result = await repository.GetByUserIdAsync(userId);
        // Assert 
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result.First(), Is.EqualTo(chats.First()));
    }

    [Test]
    public void Given_ValidInvalidMemberId_When_GetByAdminIdAsync_Throws_NotFoundByKeyException()
    {
        // Arrange 
        using var context = new GovorDbContext(_options);
        var repository = new GroupRepository(context, _validator);

        // Act & Assert 
        Assert.ThrowsAsync<NotFoundByKeyException<Guid>>(async () => await
            repository.GetByUserIdAsync(_fixture.Create<Guid>()));
    }
    
    [Test]
    public async Task Given_ValidChatGroup_When_AddAsync_Then_PrivateChatAdded()
    {
        // Arrange
        var chat = _fixture.Create<ChatGroup>();
        
        await using var context = new GovorDbContext(_options);
        var repository = new GroupRepository(context, _validator);
    
        // Act 
        await repository.AddAsync(chat);
        
        // Assert 
        Assert.That(context.ChatGroups.Count, Is.EqualTo(1));
        Assert.That(context.ChatGroups.First(), Is.EqualTo(chat));
    }
    
    [Test]
    public void Given_InvalidChatGroup_When_AddAsync_Should_Throw_AdditionException()
    {
        // Arrange
        using var context = new GovorDbContext(_options);
        var repository = new GroupRepository(context, _validator);
        
        // Act & Assert 
        Assert.ThrowsAsync<AdditionException>(async () => await repository.AddAsync(default));
    }
    
    [Test]
    public async Task Given_ExistChatGroup_When_Exist_Then_ReturnTrue()
    {
        // Arrange
        var chat = _fixture.Create<ChatGroup>();
        
        await using var context = new GovorDbContext(_options);
        var repository = new GroupRepository(context, _validator);
        
        context.ChatGroups.Add(chat);
        await context.SaveChangesAsync();
        
        // Act 
        var result1 = repository.Exist(chat.Id);
        var result2 = repository.Exist(chat);
        
        
        // Assert 
        Assert.That(result1, Is.True);
        Assert.That(result2, Is.True);
    }

    [Test]
    public async Task Given_NotExistChatGroup_When_Exist_Then_ReturnFalse()
    {
        // Arrange
        var chat = _fixture.Create<ChatGroup>();
        
        await using var context = new GovorDbContext(_options);
        var repository = new GroupRepository(context, _validator);
        
        // Act 
        var result1 = repository.Exist(chat.Id);
        var result2 = repository.Exist(chat);
        
        // Assert 
        Assert.That(result1, Is.False);
        Assert.That(result2, Is.False);
    }

    [Test]
    public async Task Given_ValidUserIdAndChatGroupId_When_IsUserMemberOfGroupAsync_Then_ReturnTrue()
    {
        // Arrange
        var chat = _fixture.Create<ChatGroup>();
        var userId = chat.Members.First().UserId;
        
        await using var context = new GovorDbContext(_options);
        var repository = new GroupRepository(context, _validator);
        
        await context.ChatGroups.AddAsync(chat);
        await context.SaveChangesAsync();
        
        // Act 
        var result = await repository.IsUserMemberOfGroupAsync(userId, chat.Id);
        
        // Assert 
        Assert.That(result, Is.True);
    }
    
    [Test]
    public async Task Given_InvalidValidUserIdAndChatGroupId_When_IsUserMemberOfGroupAsync_Then_ReturnFalse()
    {
        // Arrange
        var chat = _fixture.Create<ChatGroup>();
        var userId = chat.Members.First().UserId;
        
        await using var context = new GovorDbContext(_options);
        var repository = new GroupRepository(context, _validator);
        
        // Act 
        var result = await repository.IsUserMemberOfGroupAsync(userId, chat.Id);
        
        // Assert 
        Assert.That(result, Is.False);
    }
}