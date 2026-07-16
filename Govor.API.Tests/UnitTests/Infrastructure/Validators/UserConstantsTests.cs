using AutoFixture;
using Govor.Domain.Common.Constants;
using Govor.Domain.Models;
using Govor.Domain.Models.Users;

namespace Govor.API.Tests.UnitTests.Infrastructure.Validators;

[TestFixture]
public class UserConstantsTests 
{
    private IObjectValidator<User> _userValidator;
    private Fixture _fixture;
    
    [SetUp]
    public void SetUp()
    {
        _userValidator = new UserConstants();
        
        _fixture = new Fixture();

        _fixture.Behaviors
            .OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }
    
    [Test]
    public void Given_NullUser_ShouldThrow_InvalidObjectException()
    {
        // Act & Assert 
        Assert.ThrowsAsync<InvalidObjectException<User>>(async () => _userValidator.Validate(default));
    }
    
    [Test]
    public void Given_NullUser_Then_Returns_False()
    {
        // Act & Assert 
        Assert.That(_userValidator.TryValidate(default), Is.False);
    }
    
    [Test]
    public void Given_EmptyId_ShouldThrow_InvalidObjectException()
    {
        // Arrange 
        User user = new User()
        {
            PasswordHash = _fixture.Create<string>(),
            Username = _fixture.Create<string>(),
            Id = Guid.Empty,
            CreatedOn = DateOnly.FromDateTime(DateTime.Now),
        };
        
        // Act & Assert
        Assert.ThrowsAsync<InvalidObjectException<User>>(async () => _userValidator.Validate(user));
    }
    
    [Test]
    public void Given_EmptyId_Then_Returns_False()
    {
        // Arrange 
        User user = new User()
        {
            PasswordHash = _fixture.Create<string>(),
            Username = _fixture.Create<string>(),
            Id = Guid.Empty,
            CreatedOn = DateOnly.FromDateTime(DateTime.Now),
        };
        
        // Act & Assert
        Assert.That(_userValidator.TryValidate(user), Is.False);
    }
    
    [Test]
    public void Given_EmptyPassword_ShouldThrow_InvalidObjectException()
    {
        // Arrange
        User user = new User()
        {
            PasswordHash = string.Empty,
            Username = _fixture.Create<string>(),
            Id = Guid.NewGuid(),
            CreatedOn = DateOnly.FromDateTime(DateTime.Now),
        };
        
        // Act & Assert 
        Assert.ThrowsAsync<InvalidObjectException<User>>(async () => _userValidator.Validate(user));
    }

    [Test]
    public void Given_EmptyPassword_Then_Returns_False()
    {
        // Arrange
        User user = new User()
        {
            PasswordHash = string.Empty,
            Username = _fixture.Create<string>(),
            Id = Guid.NewGuid(),
            CreatedOn = DateOnly.FromDateTime(DateTime.Now),
        };
        
        // Act & Assert 
        Assert.That(_userValidator.TryValidate(user), Is.False);
    }
    
    [Test]
    public void Given_EmptyName_ShouldThrow_InvalidObjectException()
    {
        // Arrange
        User user = new User()
        {
            PasswordHash = _fixture.Create<string>(),
            Username = String.Empty,
            Id = Guid.NewGuid(),
            CreatedOn = DateOnly.FromDateTime(DateTime.Now),
        };
        
        // Act & Assert 
        Assert.ThrowsAsync<InvalidObjectException<User>>(async () => _userValidator.Validate(user));
    }
    
    [Test]
    public void Given_EmptyName_Then_Returns_False()
    {
        // Arrange
        User user = new User()
        {
            PasswordHash = _fixture.Create<string>(),
            Username = String.Empty,
            Id = Guid.NewGuid(),
            CreatedOn = DateOnly.FromDateTime(DateTime.Now),
        };
        
        // Act & Assert 
        Assert.That(_userValidator.TryValidate(user), Is.False);
    }
    
    [Test]
    public void Given_ValidUser_ShouldNotThrow_InvalidObjectException()
    {
        // Arrange
        User user = new User()
        {
            PasswordHash = _fixture.Create<string>(),
            Username = _fixture.Create<string>(),
            Id = Guid.NewGuid(),
            CreatedOn = DateOnly.FromDateTime(DateTime.Now),
        };
        
        // Act & Assert 
        Assert.DoesNotThrowAsync(async () => _userValidator.Validate(user));
    }
    
    [Test]
    public void Given_ValidUser_Then_Returns_True()
    {
        // Arrange
        User user = new User()
        {
            PasswordHash = _fixture.Create<string>(),
            Username = _fixture.Create<string>(),
            Id = Guid.NewGuid(),
            CreatedOn = DateOnly.FromDateTime(DateTime.Now),
        };
        
        // Act & Assert 
        Assert.That(_userValidator.TryValidate(user), Is.True);
    }
}