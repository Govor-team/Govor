using AutoFixture;
using Govor.Core.Infrastructure.Validators;
using Govor.Core.Models;
using Govor.Data;
using Govor.Data.Repositories;
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
}