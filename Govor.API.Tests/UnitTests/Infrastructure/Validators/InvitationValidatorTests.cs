using AutoFixture;
using Govor.Core.Infrastructure.Validators;
using Govor.Core.Models;

namespace Govor.API.Tests.UnitTests.Infrastructure.Validators;

[TestFixture]
public class InvitationValidatorTests
{
    private IObjectValidator<Invitation> _messageValidator;
    private Fixture _fixture;

    public InvitationValidatorTests()
    {
        _messageValidator = new InvitationValidator();
        
        _fixture = new Fixture();

        _fixture.Behaviors
            .OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }
    
    [Test]
    public void Given_ValidInvitation_When_Exist_Then_Returns_True()
    {
        var invitation = _fixture.Create<Invitation>();
        
        // Act & Assert 
        Assert.DoesNotThrow( () => _messageValidator.Validate(invitation));
        Assert.That(_messageValidator.TryValidate(invitation), Is.True);
    }

    [Test]
    public void Given_NullInvitation_When_Exist_Then_Returns_False()
    {
        // Act & Assert 
        Assert.Throws<InvalidObjectException<Invitation>>( () => _messageValidator.Validate(default));
        Assert.That(_messageValidator.TryValidate(default), Is.False);
    }
    
    [Test]
    public void Given_EmptyInvitationId_When_Exist_Then_Returns_False()
    {
        // Arrange 
        var invitation = _fixture.Create<Invitation>();
        invitation.Id = Guid.Empty;
        
        // Act & Assert 
        Assert.Throws<InvalidObjectException<Invitation>>( () => _messageValidator.Validate(invitation));
        Assert.That(_messageValidator.TryValidate(invitation), Is.False);
    }
    
    [Test]
    public void Given_EmptyInvitationCreationDate_When_Exist_Then_Returns_False()
    {
        // Arrange 
        var invitation = _fixture.Create<Invitation>();
        invitation.DateCreated = DateTime.MinValue;
        
        // Act & Assert 
        Assert.Throws<InvalidObjectException<Invitation>>( () => _messageValidator.Validate(invitation));
        Assert.That(_messageValidator.TryValidate(invitation), Is.False);
    }
    
    [Test]
    public void Given_InvalidEndDate_When_Exist_Then_Returns_False()
    {
        // Arrange 
        var invitation = _fixture.Create<Invitation>();
        invitation.EndDate = invitation.DateCreated.AddDays(-1);
        
        // Act & Assert 
        Assert.Throws<InvalidObjectException<Invitation>>( () => _messageValidator.Validate(invitation));
        Assert.That(_messageValidator.TryValidate(invitation), Is.False);
    }
    
    [Test]
    public void Given_InvalidMaxParticipants_When_Exist_Then_Returns_False()
    {
        // Arrange 
        var invitation = _fixture.Create<Invitation>();
        invitation.MaxParticipants = new Random().Next(-5, 0);
        
        // Act & Assert 
        Assert.Throws<InvalidObjectException<Invitation>>( () => _messageValidator.Validate(invitation));
        Assert.That(_messageValidator.TryValidate(invitation), Is.False);
    }
    
    [Test]
    public void Given_InvalidCode_When_Exist_Then_Returns_False()
    {
        // Arrange 
        var invitation = _fixture.Create<Invitation>();
        invitation.Code = string.Empty;
        
        // Act & Assert 
        Assert.Throws<InvalidObjectException<Invitation>>( () => _messageValidator.Validate(invitation));
        Assert.That(_messageValidator.TryValidate(invitation), Is.False);
    }
}