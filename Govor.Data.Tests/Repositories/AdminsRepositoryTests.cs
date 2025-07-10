using AutoFixture;
using Govor.Core.Infrastructure.Validators;
using Govor.Core.Models;
using Govor.Data;
using Govor.Data.Repositories;
using Govor.Data.Repositories.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Govor.Data.Tests.Repositories;

[TestFixture]
public class AdminsRepositoryTests
{ 
    private Fixture _fixture;
    private DbContextOptions<GovorDbContext> _options;
    private IObjectValidator<Admin> _validator = new AdminValidator();
    
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
            .UseInMemoryDatabase(databaseName: $"DbGovor_{nameof(AdminsRepositoryTests)}_{_testIteration}")
            .Options;
        _testIteration += 1;
    }
    
    [Test]
    public async Task Given_NotEmptyDbSet_When_GetAllAsync_Then_Admins_Are_Returned()
    {
        // Arrange 
        var random = new Random();
        var admins = _fixture.CreateMany<Admin>(random.Next(3, 10)).ToList();

        await using var context = new GovorDbContext(_options);
        var repository = new AdminsRepository(context, _validator);
        
        context.AddRange(admins);
        await context.SaveChangesAsync();
        
        // Act 
        var result = await repository.GetAllAsync();
        
        // Assert 
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(admins.Count()));
        Assert.That(result.Select(r => r.UserId), Is.EquivalentTo(admins.Select(r => r.UserId)));
    }
    
    [Test]
    public async Task Given_EmptyDbSet_When_GetAllAsync_Should_Throw_NotFoundException()
    {
        // Arrange 
        await using var context = new GovorDbContext(_options);
        var repository = new AdminsRepository(context, _validator);
        
        // Act & Assert 
        Assert.ThrowsAsync<NotFoundException>(async () => await repository.GetAllAsync());
    }

    [Test]
    public async Task Given_ValidUserId_When_GetByIdAsync_Then_Admin_Should_Return_Admin()
    {
        // Arrange 
        var admin = _fixture.Create<Admin>();   
        await using var context = new GovorDbContext(_options);
        var repository = new AdminsRepository(context, _validator);
        
        context.Add(admin);
        await context.SaveChangesAsync();
        
        // Act 
        var result = await repository.GetByIdAsync(admin.UserId);
        
        // Assert 
        Assert.That(result, Is.Not.Null);
        Assert.That(result.UserId, Is.EqualTo(admin.UserId));
    }

    [Test]
    public async Task Given_InvalidUserId_When_GetByIdAsync_Should_Throw_NotFoundByKeyException()
    {
        // Arrange 
        await using var context = new GovorDbContext(_options);
        var repository = new AdminsRepository(context, _validator);
        
        // Act & Assert 
        Assert.ThrowsAsync<NotFoundByKeyException<Guid>>(async () => await repository.GetByIdAsync(_fixture.Create<Guid>()));
    }
    
    [Test]
    public async Task Given_ValidAdmin_When_AddAsync_Then_Added()
    {
        // Arrange
        var admin = _fixture.Create<Admin>();
        
        await using var context = new GovorDbContext(_options);
        var repository = new AdminsRepository(context, _validator);
    
        // Act 
        await repository.AddAsync(admin);
        
        // Assert 
        Assert.That(context.Admins.Count, Is.EqualTo(1));
        Assert.That(context.Admins.First(), Is.EqualTo(admin));
    }
    
    [Test]
    public async Task Given_InvalidAdmin_When_AddAsync_Should_Throw_AdditionException()
    {
        // Arrange
        await using var context = new GovorDbContext(_options);
        var repository = new AdminsRepository(context, _validator);
        
        // Act & Assert 
        Assert.ThrowsAsync<AdditionException>(async () => await repository.AddAsync(default));
    }
    
    [Test]
    public void Given_ValidAdmin_When_Exist_Then_Returns_True()
    {
        // Arrange
        var admin = _fixture.Create<Admin>();
        using var context = new GovorDbContext(_options);
        var repository = new AdminsRepository(context, _validator);
        
        context.Add(admin);
        context.SaveChanges();
        // Act 
        var result = repository.Exist(admin.UserId);
        var result2 = repository.Exist(admin);
        
        // Assert 
        Assert.That(result, Is.True);
        Assert.That(result2, Is.True);
    }
    
    [Test]
    public void Given_ExistAdmin_When_Exist_Then_Returns_False()
    {
        // Arrange
        var admin = _fixture.Create<Admin>();
        using var context = new GovorDbContext(_options);
        var repository = new AdminsRepository(context, _validator);
        
        // Act 
        var result = repository.Exist(admin.UserId);
        var result2 = repository.Exist(admin);
        
        // Assert 
        Assert.That(result, Is.False);
        Assert.That(result2, Is.False);
    }
    
    [Test]
    public void Given_NullAdmin_When_Exist_Should_Throw_InvalidObjectException()
    {
        // Arrange
        using var context = new GovorDbContext(_options);
        var repository = new AdminsRepository(context, _validator);
        
        // Act & Assert 
        Assert.Throws<InvalidObjectException<Admin>>(() => repository.Exist(default(Admin)));
    }
}
