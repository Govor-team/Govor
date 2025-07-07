using AutoFixture;
using Govor.Core.Infrastructure.Validators;
using Govor.Core.Models;

namespace Govor.API.Tests.UnitTests.Infrastructure.Validators;

[TestFixture]
public class ChatGroupValidatorTests
{
    private IObjectValidator<ChatGroup> _validator = new ChatGroupValidator();
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
    public void Validate_ChatGroup_Valid()
    {
        // Arrange
        var chat = _fixture.Create<ChatGroup>();
        
        // Act & Assert 
        Assert.DoesNotThrow(() => _validator.Validate(chat));
        Assert.That(_validator.TryValidate(chat), Is.True);
    }
    
        
    [Test]
    public void Validate_ChatGroupIsNull_Invalid()
    {
        // Act & Assert 
        Assert.Throws<InvalidObjectException<ChatGroup>>(() => _validator.Validate(default));
        Assert.That(_validator.TryValidate(default), Is.False);
    }

    [Test]
    public void Validate_ChatGroupIdIsEmpty_Invalid()
    {
        // Arrange 
        var chat = _fixture.Create<ChatGroup>();
        chat.Id = Guid.Empty;
        
        // Act & Assert 
        Assert.Throws<InvalidObjectException<ChatGroup>>(() => _validator.Validate(chat));
        Assert.That(_validator.TryValidate(chat), Is.False);
    }

    [Test]
    public void Validate_ChatGroupNameIsEmpty_Invalid()
    {
        // Arrange 
        var chat = _fixture.Create<ChatGroup>();
        chat.Name = "";
        
        // Act & Assert 
        Assert.Throws<InvalidObjectException<ChatGroup>>(() => _validator.Validate(chat));
        Assert.That(_validator.TryValidate(chat), Is.False);
    }
    
    [Test]
    public void Validate_ChatGroupDescriptionIsNull_Invalid()
    {
        // Arrange 
        var chat = _fixture.Create<ChatGroup>();
        chat.Description = default;
        
        // Act & Assert 
        Assert.Throws<InvalidObjectException<ChatGroup>>(() => _validator.Validate(chat));
        Assert.That(_validator.TryValidate(chat), Is.False);
    }
    
    [Test]
    public void Validate_ChatGroupImageIdIsEmpty_Invalid()
    {
        // Arrange 
        var chat = _fixture.Create<ChatGroup>();
        chat.ImageId = Guid.Empty;
        
        // Act & Assert 
        Assert.Throws<InvalidObjectException<ChatGroup>>(() => _validator.Validate(chat));
        Assert.That(_validator.TryValidate(chat), Is.False);
    }

    [Test]
    public void Validate_ChatGroupIsPrivateAndInvitesCodesIsEmpty_Invalid()
    {
        // Arrange 
        var chat = _fixture.Create<ChatGroup>();
        chat.IsPrivate = true;
        chat.InviteCodes = new();
        
        // Act & Assert 
        Assert.Throws<InvalidObjectException<ChatGroup>>(() => _validator.Validate(chat));
        Assert.That(_validator.TryValidate(chat), Is.False);
    }
    
    [Test]
    public void Validate_ChatGroupAdminsIsEmpty_Invalid()
    {
        // Arrange 
        var chat = _fixture.Create<ChatGroup>();
        chat.Admins = new();
        
        // Act & Assert 
        Assert.Throws<InvalidObjectException<ChatGroup>>(() => _validator.Validate(chat));
        Assert.That(_validator.TryValidate(chat), Is.False);
    }
}