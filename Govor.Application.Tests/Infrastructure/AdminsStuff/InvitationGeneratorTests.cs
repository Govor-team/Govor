using AutoFixture;
using Govor.Application.Infrastructure.AdminsStuff;
using Govor.Application.Interfaces;
using Govor.Domain.Models;
using Govor.Domain.Repositories.Invaites;
using Moq;

namespace Govor.Application.Tests.Infrastructure.AdminsStuff;

[TestFixture]
public class InvitationGeneratorTests
{
    private Fixture _fixture;
    private Mock<IInvitesRepository> _invitesRepositoryMock;
    private IInvitationGenerator _invitationGenerator;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _invitesRepositoryMock = new Mock<IInvitesRepository>();
        _invitationGenerator = new InvitationGenerator(_invitesRepositoryMock.Object);
    }
    
    [Test]
    public async Task GenerateInvitationCode_ShouldReturnNonEmptyString_WhenCalled()
    {
        // Arrange
        var endDate = _fixture.Create<DateTime>();
        var maxUsers = _fixture.Create<int>();
        var isAdmin = _fixture.Create<bool>();
        var description = _fixture.Create<string>();

        // Act
        var code = await _invitationGenerator.GenerateInvitationCode(endDate, maxUsers, isAdmin, description);

        // Assert
        Assert.That(code, Is.Not.Null.And.Not.Empty);
    }
    

    [Test]
    public async Task GenerateInvitationCode_ShouldCallAddAsyncOnRepository_WithCorrectInvitation()
    {
        // Arrange
        var endDate = DateTime.UtcNow.AddDays(7); // Specific date for easier assertion if needed
        var maxUsers = 10;
        var isAdmin = false;
        var description = "Test Invitation";
        Invitation capturedInvitation = null;

        _invitesRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Invitation>()))
            .Callback<Invitation>(inv => capturedInvitation = inv)
            .Returns(Task.CompletedTask);

        // Act
        var code = await _invitationGenerator.GenerateInvitationCode(endDate, maxUsers, isAdmin, description);

        // Assert
        _invitesRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Invitation>()), Times.Once);
        Assert.That(capturedInvitation, Is.Not.Null);
        Assert.That(capturedInvitation.EndDate.ToUniversalTime(), Is.EqualTo(endDate.ToUniversalTime()));
        Assert.That(capturedInvitation.MaxParticipants, Is.EqualTo(maxUsers));
        Assert.That(capturedInvitation.IsAdmin, Is.EqualTo(isAdmin));
        Assert.That(capturedInvitation.Description, Is.EqualTo(description));
        Assert.That(capturedInvitation.Code, Is.EqualTo(code));
        Assert.That(capturedInvitation.IsActive, Is.True); // Assuming new invitations are active by default
    }
    
    [Test]
    public async Task GenerateInvitationCode_ShouldGenerateUniqueCode_WhenCalled()
    {
        // Arrange
        var endDate = _fixture.Create<DateTime>();
        var maxUsers = _fixture.Create<int>();
        var isAdmin = _fixture.Create<bool>();
        var description = _fixture.Create<string>();

        // Act
        var code1 = await _invitationGenerator.GenerateInvitationCode(endDate, maxUsers, isAdmin, description);
        var code2 = await _invitationGenerator.GenerateInvitationCode(endDate, maxUsers, isAdmin, description);

        // Assert
        Assert.That(code1, Is.Not.EqualTo(code2));
    }
}