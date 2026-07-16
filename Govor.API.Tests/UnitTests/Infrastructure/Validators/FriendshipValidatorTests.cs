using AutoFixture;
using Govor.Domain.Common.Constants;
using Govor.Domain.Models;

namespace Govor.API.Tests.UnitTests.Infrastructure.Validators;

[TestFixture]
public class FriendshipValidatorTests
{
    private IObjectValidator<Friendship> _friendshipValidator;
    private Fixture _fixture;
    
    public FriendshipValidatorTests()
    {
        _friendshipValidator = new FriendshipValidator();
        
        _fixture = new Fixture();

        _fixture.Behaviors
            .OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }
    
        
    [Test]
    public void Given_ValidFriendship_When_Validate_Then_Returns_True()
    {
        // Arrange 
        var friendship = _fixture.Create<Friendship>();
        
        // Act & Assert 
        Assert.DoesNotThrow( () => _friendshipValidator.Validate(friendship));
        Assert.That(_friendshipValidator.TryValidate(friendship), Is.True);
    }
    
    [Test]
    public void Given_EmptyFriendshipId_When_Validate_Then_Returns_False()
    {
        // Arrange 
        var friendship = _fixture.Create<Friendship>();
        friendship.Id = Guid.Empty;
        
        // Act & Assert 
        Assert.Throws<InvalidObjectException<Friendship>>( () => _friendshipValidator.Validate(friendship));
        Assert.That(_friendshipValidator.TryValidate(friendship), Is.False);
    }
    
    [Test]
    public void Given_EmptyRequesterId_When_Validate_Then_Returns_False()
    {
        // Arrange 
        var friendship = _fixture.Create<Friendship>();
        friendship.RequesterId = Guid.Empty;
        
        // Act & Assert 
        Assert.Throws<InvalidObjectException<Friendship>>( () => _friendshipValidator.Validate(friendship));
        Assert.That(_friendshipValidator.TryValidate(friendship), Is.False);
    }

    [Test]
    public void GivenSameRequesterId_When_Validate_Then_Returns_False()
    {
        // Arrange 
        var friendship = _fixture.Create<Friendship>();
        friendship.RequesterId = friendship.AddresseeId;
        
        // Act & Assert 
        Assert.Throws<InvalidObjectException<Friendship>>( () => _friendshipValidator.Validate(friendship));
        Assert.That(_friendshipValidator.TryValidate(friendship), Is.False);
    }
    
    [Test]
    public void Given_AddresseeId_When_Validate_Then_Returns_False()
    {
        // Arrange 
        var friendship = _fixture.Create<Friendship>();
        friendship.AddresseeId = Guid.Empty;
        
        // Act & Assert 
        Assert.Throws<InvalidObjectException<Friendship>>( () => _friendshipValidator.Validate(friendship));
        Assert.That(_friendshipValidator.TryValidate(friendship), Is.False);
    }
}