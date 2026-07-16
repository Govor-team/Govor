using AutoFixture;
using Govor.Domain.Infrastructure.Validators;
using Govor.Domain.Models;
using Govor.Domain;
using Govor.Domain.Repositories;
using Govor.Domain.Repositories.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Govor.Domain.Tests.Repositories;

[TestFixture]
public class FriendshipsRepositoryTests
{
    private Fixture _fixture;
    private DbContextOptions<GovorDbContext> _options;
    private IObjectValidator<Friendship> _validator = new FriendshipValidator();
    
    private int _testIteration = 0;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        
        _fixture.Behaviors
            .OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        
        _options = new DbContextOptionsBuilder<GovorDbContext>()
            .UseInMemoryDatabase(databaseName: $"DbGovor_{nameof(FriendshipsRepositoryTests)}_{_testIteration}")
            .Options;
        _testIteration += 1;
    }
    
        
    [Test]
    public async Task Given_NotEmptyDbSet_When_GetAllAsync_Then_Friendships_Are_Returned()
    {
        // Arrange 
        var random = new Random();
        var friendships = _fixture.CreateMany<Friendship>(random.Next(3, 10)).ToList();

        await using var context = new GovorDbContext(_options);
        var repository = new FriendshipsRepository(context, _validator);
        
        context.AddRange(friendships);
        await context.SaveChangesAsync();
        
        // Act 
        var result = await repository.GetAllAsync();
        
        // Assert 
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(friendships.Count()));
        Assert.That(result.Select(r => r.Id), Is.EquivalentTo(friendships.Select(r => r.Id)));
    }
    
    [Test]
    public async Task Given_EmptyDbSet_When_GetAllAsync_Should_Throw_NotFoundException()
    {
        // Arrange 
        await using var context = new GovorDbContext(_options);
        var repository = new FriendshipsRepository(context, _validator);
        
        // Act & Assert 
        Assert.ThrowsAsync<NotFoundException>(async () => await repository.GetAllAsync());
    }

    [Test]
    public async Task Given_ValidFriendshipId_When_GetByIdAsync_Then_Friendships_Are_Returned()
    {
        // Arrange 
        var random = new Random();
        var friendships = _fixture.CreateMany<Friendship>(random.Next(3, 10)).ToList();

        await using var context = new GovorDbContext(_options);
        var repository = new FriendshipsRepository(context, _validator);
        
        context.AddRange(friendships);
        await context.SaveChangesAsync();
        
        // Act 
        var result = await repository.GetByIdAsync(friendships.First().Id);
        
        // Assert 
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(friendships.First().Id));
    }
    
    [Test]
    public async Task Given_InvalidFriendshipId_When_GetByIdAsync_Should_Throw_NotFoundByKeyException()
    {
        // Arrange 
        await using var context = new GovorDbContext(_options);
        var repository = new FriendshipsRepository(context, _validator);
        
        // Act & Assert 
        Assert.ThrowsAsync<NotFoundByKeyException<Guid>>(async () => await repository.GetByIdAsync(_fixture.Create<Guid>()));
    }
    
    [Test]
    public async Task Given_ValidUserId_When_GetByIdAsync_Then_Friendships_Are_Returned()
    {
        // Arrange 
        var random = new Random();
        var friendships = _fixture.CreateMany<Friendship>(random.Next(3, 10)).ToList();

        await using var context = new GovorDbContext(_options);
        var repository = new FriendshipsRepository(context, _validator);
        
        context.AddRange(friendships);
        await context.SaveChangesAsync();
        
        // Act 
        var result = await repository.FindByUserIdAsync(friendships.First().RequesterId);
        
        // Assert 
        Assert.That(result, Is.Not.Null);
        Assert.That(result.First().Id, Is.EqualTo(friendships.First().Id));
    }
    
    [Test]
    public async Task Given_InvalidUserId_When_GetByIdAsync_Should_Throw_NotFoundByKeyException()
    {
        // Arrange 
        await using var context = new GovorDbContext(_options);
        var repository = new FriendshipsRepository(context, _validator);
        
        // Act & Assert 
        Assert.ThrowsAsync<NotFoundByKeyException<Guid>>(async () => await repository.FindByUserIdAsync(_fixture.Create<Guid>()));
    }
    
    [Test]
    public async Task Given_ValidFriendship_When_AddAsync_Then_Added()
    {
        // Arrange
        var friendship = _fixture.Create<Friendship>();
        
        await using var context = new GovorDbContext(_options);
        var repository = new FriendshipsRepository(context, _validator);
    
        // Act 
        await repository.AddAsync(friendship);
        
        // Assert 
        Assert.That(context.Friendships.Count, Is.EqualTo(1));
        Assert.That(context.Friendships.First().Id, Is.EqualTo(friendship.Id));
    }
    
    [Test]
    public async Task Given_InvalidFriendship_When_AddAsync_Should_Throw_AdditionException()
    {
        // Arrange
        await using var context = new GovorDbContext(_options);
        var repository = new FriendshipsRepository(context, _validator);
        
        // Act & Assert 
        Assert.ThrowsAsync<AdditionException>(async () => await repository.AddAsync(default));
    }
    
    [Test]
    public async Task Given_ExistFriendship_When_Exist_Then_True()
    {
        // Arrange
        var friendship = _fixture.Create<Friendship>();
        
        await using var context = new GovorDbContext(_options);
        var repository = new FriendshipsRepository(context, _validator);
        
        context.Friendships.Add(friendship);
        await context.SaveChangesAsync();
        
        // Act 
        var result = repository.Exist(friendship.RequesterId, friendship.AddresseeId);
        // Assert 
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task Given_NotExistFriendship_When_Exist_Then_False()
    {
        // Arrange
        await using var context = new GovorDbContext(_options);
        var repository = new FriendshipsRepository(context, _validator);
        // Act & Assert 
        Assert.That(repository.Exist(_fixture.Create<Guid>(), _fixture.Create<Guid>()), Is.False);
    }
}