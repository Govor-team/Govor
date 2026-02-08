using AutoFixture;
using Govor.Core.Infrastructure.Validators;
using Govor.Core.Models;

namespace Govor.API.Tests.UnitTests.Infrastructure.Validators;

[TestFixture]
public class PrivateChatValidatorTests
{
    private IObjectValidator<PrivateChat> _validator = new PrivateChatValidator();
    private Fixture _fixture;
    
    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();

        _fixture.Behaviors
            .OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Test]
    public void Validate_PrivateChat_Valid()
    {
        // Arrange
        var privateChat = _fixture.Create<PrivateChat>();
        
        // Act & Assert 
        Assert.DoesNotThrow(() => _validator.Validate(privateChat));
        Assert.That(_validator.TryValidate(privateChat), Is.True);
    }

    [Test]
    public void Validate_When_PrivateChatIsNull_Invalid()
    {
        // Act & Assert 
        Assert.Throws<InvalidObjectException<PrivateChat>>(() => _validator.Validate(default));
        Assert.That(_validator.TryValidate(default), Is.False);
    }

    [Test]
    public void Validate_When_PrivateChatIdIsEmpty_Invalid()
    {
        // Arrange 
        var privateChat = _fixture.Create<PrivateChat>();
        privateChat.Id = Guid.Empty;
        // Act & Assert 
        Assert.Throws<InvalidObjectException<PrivateChat>>(() => _validator.Validate(privateChat));
        Assert.That(_validator.TryValidate(privateChat), Is.False);
    }
    
    [Test]
    public void Validate_When_UserAIdIsEmpty_Invalid()
    {
        // Arrange 
        var privateChat = _fixture.Create<PrivateChat>();
        privateChat.UserAId = Guid.Empty;
        // Act & Assert 
        Assert.Throws<InvalidObjectException<PrivateChat>>(() => _validator.Validate(privateChat));
        Assert.That(_validator.TryValidate(privateChat), Is.False);
    }
    
    [Test]
    public void Validate_When_UserBIdIsEmpty_Invalid()
    {
        // Arrange 
        var privateChat = _fixture.Create<PrivateChat>();
        privateChat.UserBId = Guid.Empty;
        // Act & Assert 
        Assert.Throws<InvalidObjectException<PrivateChat>>(() => _validator.Validate(privateChat));
        Assert.That(_validator.TryValidate(privateChat), Is.False);
    }
}