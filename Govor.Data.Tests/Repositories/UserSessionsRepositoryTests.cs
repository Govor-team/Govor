using AutoFixture;
using Govor.Core.Models;
using Govor.Data.Repositories;
using Govor.Data.Repositories.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Govor.Data.Tests.Repositories;

[TestFixture]
public class UserSessionsRepositoryTests
{
    private Fixture _fixture;
    private DbContextOptions<GovorDbContext> _options;
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
            .UseInMemoryDatabase(databaseName: $"DbGovor_{nameof(UserSessionsRepositoryTests)}_{_testIteration}")
            .Options;
    }
    
    [Test]
    public async Task GetAllSessions_ReturnsAllSessions()
    {
        // Arrange
        var random = new Random();
        var sessions = _fixture.CreateMany<UserSession>(random.Next(2, 10)).ToList();

        await using var context = new GovorDbContext(_options);
        var repository = new UserSessionsRepository(context);

        context.UserSessions.AddRange(sessions);
        await context.SaveChangesAsync();

        // Act 
        var result = await repository.GetAllAsync();

        // Assert 
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(sessions.Count));
        Assert.That(result, Is.EquivalentTo(sessions));
    }
    
    [Test]
    public void Given_EmptySetDb_When_GetAll_Should_Throw_NotFoundException()
    {
        // Arrange 
        using var context = new GovorDbContext(_options);
        var repository = new UserSessionsRepository(context);
        // Act & Assert 
        Assert.ThrowsAsync<NotFoundException>(async () => await repository.GetAllAsync());
    }

    [Test]
    public async Task Given_ValidId_When_GetByIdAsync_ShouldReturnSession()
    {
        // Arrange
        var random = new Random();
        var sessions = _fixture.CreateMany<UserSession>(random.Next(2, 10)).ToList();
        
        var id = sessions.First().Id;
        
        await using var context = new GovorDbContext(_options);
        var repository = new UserSessionsRepository(context);

        context.UserSessions.AddRange(sessions);
        await context.SaveChangesAsync();
        // Act 
        var result = await repository.GetByIdAsync(id);
        // Assert 
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(id));
        Assert.That(result, Is.EqualTo(sessions.First()));
    }

    [Test]
    public void Given_InvalidId_When_GetByIdAsync_Should_Throw_NotFoundByKeyException()
    {
        // Arrange 
        using var context = new GovorDbContext(_options);
        var repository = new UserSessionsRepository(context);
        // Act & Assert 
        Assert.ThrowsAsync<NotFoundByKeyException<Guid>>(async () => await repository.GetByIdAsync(_fixture.Create<Guid>()));
    }
    
    [Test]
    public async Task Given_ValidCreatedAt_When_GetByCreatedAtAsync_ShouldReturnSession()
    {
        // Arrange
        var random = new Random();
        var sessions = _fixture.CreateMany<UserSession>(random.Next(2, 10)).ToList();
        
        var created = sessions.First().CreatedAt;
        
        await using var context = new GovorDbContext(_options);
        var repository = new UserSessionsRepository(context);

        context.UserSessions.AddRange(sessions);
        await context.SaveChangesAsync();
        
        // Act 
        var list = await repository.GetByCreatedAtAsync(created);
        
        
        // Assert 
        var result = list.First();
        Assert.That(result, Is.Not.Null);
        Assert.That(result.CreatedAt, Is.EqualTo(created));
        Assert.That(result, Is.EqualTo(sessions.First()));
    }

    [Test]
    public void Given_ValidCreatedAt_When_GetByCreatedAtAsync_Should_Throw_NotFoundByKeyException()
    {
        // Arrange 
        using var context = new GovorDbContext(_options);
        var repository = new UserSessionsRepository(context);
        // Act & Assert 
        Assert.ThrowsAsync<NotFoundByKeyException<DateTime>>(async () => await repository.GetByCreatedAtAsync(_fixture.Create<DateTime>()));
    }
    
    [Test]
    public async Task Given_ValidExpiresAt_When_GetByExpiresAtAsync_ShouldReturnSession()
    {
        // Arrange
        var random = new Random();
        var sessions = _fixture.CreateMany<UserSession>(random.Next(2, 10)).ToList();
        
        var expiresAt = sessions.First().ExpiresAt;
        
        await using var context = new GovorDbContext(_options);
        var repository = new UserSessionsRepository(context);

        context.UserSessions.AddRange(sessions);
        await context.SaveChangesAsync();
        
        // Act 
        var list = await repository.GetByExpiresAtAsync(expiresAt);
        
        // Assert 
        var result = list.First();
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ExpiresAt, Is.EqualTo(expiresAt));
        Assert.That(result, Is.EqualTo(sessions.First()));
    }

    [Test]
    public void Given_ValidExpiresAt_When_GetByExpiresAtAsync_Should_Throw_NotFoundByKeyException()
    {
        // Arrange 
        using var context = new GovorDbContext(_options);
        var repository = new UserSessionsRepository(context);
        // Act & Assert 
        Assert.ThrowsAsync<NotFoundByKeyException<DateTime>>(async () => await repository.GetByExpiresAtAsync(_fixture.Create<DateTime>()));
    }
    
    [Test]
    public async Task Given_isRevoked_When_GetByRevokedAsync_ShouldReturnSession()
    {
        // Arrange
        var random = new Random();
        var sessions = _fixture.Build<UserSession>()
            .With(f => f.IsRevoked, true)
            .CreateMany(random.Next(2, 10)).ToList();
        
        var isRevoke = sessions.First().IsRevoked;
        
        await using var context = new GovorDbContext(_options);
        var repository = new UserSessionsRepository(context);

        context.UserSessions.AddRange(sessions);
        await context.SaveChangesAsync();
        
        // Act 
        var result = await repository.GetByRevokedAsync(isRevoke);
        
        // Assert 
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EquivalentTo(sessions));
    }

    [Test]
    public void Given_InvalidRevoked_When_GetByExpiresAtAsync_Should_Throw_NotFoundByKeyException()
    {
        // Arrange 
        using var context = new GovorDbContext(_options);
        var repository = new UserSessionsRepository(context);
        // Act & Assert 
        Assert.ThrowsAsync<NotFoundByKeyException<bool>>(async () => await repository.GetByRevokedAsync(false));
    }
    
    [Test]
    public async Task Given_ValidUserSessions_When_AddAsync_Then_UserSessionsAdded()
    {
        // Arrange
        var session = _fixture.Create<UserSession>();
        
        await using var context = new GovorDbContext(_options);
        var repository = new UserSessionsRepository(context);
    
        // Act 
        await repository.AddAsync(session);
        
        // Assert 
        Assert.That(context.UserSessions.Count, Is.EqualTo(1));
        Assert.That(context.UserSessions.First(), Is.EqualTo(session));
    }
    
    [Test]
    public async Task Given_InvalidUserSessions_When_AddAsync_Should_Throw_AdditionException()
    {
        // Arrange
        await using var context = new GovorDbContext(_options);
        var repository = new UserSessionsRepository(context);
        
        // Act & Assert 
        Assert.ThrowsAsync<AdditionException>(async () => await repository.AddAsync(default));
    }
    
    [Test]
    public async Task Given_ExistUserSessions_When_Exist_Then_ReturnTrue()
    {
        // Arrange
        var session = _fixture.Create<UserSession>();
        
        await using var context = new GovorDbContext(_options);
        var repository = new UserSessionsRepository(context);
        
        context.UserSessions.Add(session);
        await context.SaveChangesAsync();
        
        // Act 
        var result1 = repository.Exist(session);
        var result2 = repository.Exist(session.Id);
        var result3 = repository.Exist(session.RefreshToken);
        
        // Assert 
        Assert.That(result1, Is.True);
        Assert.That(result2, Is.True);
        Assert.That(result3, Is.True);
    }

    [Test]
    public async Task Given_NotExistUserSessions_When_Exist_Then_ReturnFalse()
    {
        // Arrange
        var session = _fixture.Create<UserSession>();
        
        await using var context = new GovorDbContext(_options);
        var repository = new UserSessionsRepository(context);
        
        // Act 
        var result1 = repository.Exist(session);
        var result2 = repository.Exist(session.Id);
        var result3 = repository.Exist(session.RefreshToken);
        // Assert 
        Assert.That(result1, Is.False);
        Assert.That(result2, Is.False);
        Assert.That(result3, Is.False);
    }
}