using AutoFixture;
using Govor.Core.Infrastructure.Validators;
using Govor.Core.Models;
using Govor.Data;
using Govor.Data.Repositories;
using Govor.Data.Repositories.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Govor.API.Tests.IntegrationTests.EF.Repositories;

[TestFixture]
public class UsersRepositoryTests
{
    private Fixture _fixture;
    private DbContextOptions<GovorDbContext> _options;
    private readonly IObjectValidator<User> _userValidator = new UserValidator();
    
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
    public async Task GetAll_Then_Returns_All_Users()
    {
        // Arrange
        var random = new Random();
        var users = _fixture.CreateMany<User>(random.Next(2, 10)).ToList();
        
        await using var context = new GovorDbContext(_options);
        var userRepository = new UsersRepository(context, _userValidator);
        
        context.Users.AddRange(users);
        await context.SaveChangesAsync();
        
        // Act 
        
        var result = await userRepository.GetAll();
        
        // Assert 
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(users.Count));
        Assert.That(result.Select(u => u.Id), Is.EquivalentTo(users.Select(u => u.Id)));
        Assert.That(result.Select(u => u.Username), Is.EquivalentTo(users.Select(u => u.Username)));
    }

    [Test]
    public async Task Given_ValidUserId_When_FindById_Then_Returns_User()
    {
        // Arrange
        var user  = _fixture.Create<User>();
        await using var context = new GovorDbContext(_options);
        var userRepository = new UsersRepository(context, _userValidator);
        
        context.Users.Add(user);
        await context.SaveChangesAsync();
        
        // Act 
        var result = await userRepository.FindById(user.Id);
        
        // Assert 
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Username, Is.EqualTo(user.Username));
        Assert.That(result.Id, Is.EqualTo(user.Id));
    }
    
    [Test]
    public async Task Given_InvalidUserId_When_FindById_Should_Throw_NotFoundException()
    {
        // Arrange 
        var id = Guid.NewGuid();
        
        await using var context = new GovorDbContext(_options);
        var userRepository = new UsersRepository(context, _userValidator);
        
        // Act & Assert  
        Assert.ThrowsAsync<NotFoundByKeyException<Guid>>(async () => await userRepository.FindById(id));
    }
    
    [Test]
    public async Task Given_RangeValidUserId_When_FindByRangeId_Then_Returns_Users()
    {
        // Arrange 
        var random = new Random();
        var users = _fixture.CreateMany<User>(random.Next(2, 10)).ToList();
        
        await using var context = new GovorDbContext(_options);
        var userRepository = new UsersRepository(context, _userValidator);
        
        context.Users.AddRange(users);
        await context.SaveChangesAsync();
        
        // Act 
        var result = await userRepository.FindByRangeId(users.Select(u => u.Id));
        
        // Assert 
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(users.Count));
        Assert.That(result.Select(r => r.Id), Is.EquivalentTo(users.Select(u => u.Id)));
        Assert.That(result.Select(u => u.Username), Is.EquivalentTo(users.Select(u => u.Username)));
    }
    
    [Test]
    public async Task Given_InvalidRangeId_When_FindByRangeId_Should_Throw_NotFoundException()
    {
        // Arrange 
        var random = new Random();
        var ids = _fixture.CreateMany<Guid>(random.Next(2, 10)).ToList();
        
        await using var context = new GovorDbContext(_options);
        var userRepository = new UsersRepository(context, _userValidator);
        
        // Act & Assert 
        Assert.ThrowsAsync<NotFoundByKeyException<IEnumerable<Guid>>>(async () => await userRepository.FindByRangeId(ids));
    }

    [Test]
    public async Task Given_ValidUsername_When_FindByUsername_Then_Returns_User()
    {
        // Arrange 
        var user = _fixture.Create<User>();
        
        await using var context = new GovorDbContext(_options);
        var userRepository = new UsersRepository(context, _userValidator);
        
        context.Users.Add(user);
        await context.SaveChangesAsync();
        
        // Act 
        var result = await userRepository.FindByUsername(user.Username);
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Username, Is.EqualTo(user.Username));
        Assert.That(result.Id, Is.EqualTo(user.Id));
    }
    
    [Test]
    public async Task Given_InvalidUsername_When_FindByUsername_Should_Throw_NotFoundByKeyException()
    {
        // Arrange 
        string username = _fixture.Create<string>();
        
        await using var context = new GovorDbContext(_options);
        var userRepository = new UsersRepository(context, _userValidator);
        // Act & Assert 
        
        Assert.ThrowsAsync<NotFoundByKeyException<string>>(async () => await userRepository.FindByUsername(username));
    }
    
    [Test]
    public async Task Given_ValidUsernames_When_FindByRangeUsernames_Then_Returns_Users()
    {
        // Arrange 
        var random = new Random();
        var users = _fixture.CreateMany<User>(random.Next(3, 10)).ToList();
        
        await using var context = new GovorDbContext(_options);
        var userRepository = new UsersRepository(context, _userValidator);
        
        context.Users.AddRange(users);
        await context.SaveChangesAsync();
        
        // Act 
        var result = await userRepository.FindByRangeUsernames(users.Select(u => u.Username));
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Select(u => u.Username), Is.EquivalentTo(users.Select(u => u.Username)));
        Assert.That(result.Select(u => u.Id), Is.EquivalentTo(users.Select(u => u.Id)));
    }
    
    [Test]
    public async Task Given_InvalidUsernames_When_FindByRangeUsernames_Should_Throw_NotFoundByKeyException()
    {
        // Arrange 
        var random = new Random();
        var usernames = _fixture.CreateMany<string>(random.Next(3, 10)).ToList();
        
        await using var context = new GovorDbContext(_options);
        var userRepository = new UsersRepository(context, _userValidator);
        
        // Act & Assert 
        Assert.ThrowsAsync<NotFoundByKeyException<IEnumerable<string>>>(async () => await userRepository.FindByRangeUsernames(usernames));
    }

    [Test]
    public async Task Given_ValidDateOnly_When_FindByCreatedDate_Then_Returns_Users()
    {
        // Arrange 
        var random = new Random();
        var users = _fixture.CreateMany<User>(random.Next(3, 10)).ToList();
        
        var selectedDate = users[random.Next(users.Count)].CreatedOn;
        
        await using var context = new GovorDbContext(_options);
        var userRepository = new UsersRepository(context, _userValidator);

        context.Users.AddRange(users);
        await context.SaveChangesAsync();

        // Act 
        var result = await userRepository.FindUsersByCreatedDate(selectedDate);

        // Assert
        var expectedUsers = users
            .Where(u => u.CreatedOn == selectedDate)
            .ToList();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(expectedUsers.Count));
        Assert.That(result.Select(u => u.Id), Is.EquivalentTo(expectedUsers.Select(u => u.Id)));
    }

    [Test]
    public async Task Given_InvalidDateOnly_When_FindByCreatedDate_Should_Throw_NotFoundByKeyException()
    {
        // Arrange 
        var date = _fixture.Create<DateOnly>();
        
        await using var context = new GovorDbContext(_options);
        var userRepository = new UsersRepository(context, _userValidator);
        
        // Act & Assert 
        Assert.ThrowsAsync<NotFoundByKeyException<DateOnly>>(async () => await userRepository.FindUsersByCreatedDate(date));
    }

    [Test]
    public async Task Given_ValidUser_When_AddUser_Then_CreateUser()
    {
        var user = _fixture.Create<User>();
        
        await using var context = new GovorDbContext(_options);
        var userRepository = new UsersRepository(context, _userValidator);
        
        // Act 
        userRepository.Add(user);
        var res = context.Users.Find(user.Id);
        
        // Assert 
        
        Assert.That(res, Is.Not.Null);
        Assert.That(res.Username, Is.EqualTo(user.Username));
        Assert.That(res.Id, Is.EqualTo(user.Id));
    }

    [Test]
    public async Task Given_InvalidUser_When_AddUser_Should_Throw_AdditionUserException()
    {
        var user = _fixture.Create<User>();
        user.Username = string.Empty;
        
        await using var context = new GovorDbContext(_options);
        var userRepository = new UsersRepository(context, _userValidator);
        
        // Act & Assert
        Assert.ThrowsAsync<AdditionUserException>(async () => await userRepository.Add(user));
    }
    
}