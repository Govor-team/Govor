using AutoFixture;
using Govor.Domain.Infrastructure.Validators;
using Govor.Domain.Models;
using Govor.Domain;
using Govor.Domain.Repositories;
using Govor.Domain.Repositories.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Govor.Domain.Tests.Repositories;

[TestFixture]
public class InvitesRepositoryTests
{
    private Fixture _fixture;
    private DbContextOptions<GovorDbContext> _options;
    private IObjectValidator<Invitation> _validator = new InvitationValidator();
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
            .UseInMemoryDatabase(databaseName: $"DbGovor_{nameof(InvitesRepositoryTests)}_{_testIteration}")
            .Options;
        _testIteration += 1;
    }
    
    [Test]
    public async Task Given_NotEmptyDbSet_When_GetAllAsync_Then_Invites_Are_Returned()
    {
        // Arrange 
        var random = new Random();
        var invitations = _fixture.CreateMany<Invitation>(random.Next(3, 10)).ToList();

        await using var context = new GovorDbContext(_options);
        var repository = new InvitesRepository(context, _validator);
        
        context.Invitations.AddRange(invitations);
        await context.SaveChangesAsync();
        
        // Act 
        var result = await repository.GetAllAsync();
        
        // Assert 
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(invitations.Count()));
        Assert.That(result, Is.EquivalentTo(invitations));
    }
    
    [Test]
    public async Task Given_EmptyDbSet_When_GetAllAsync_Should_Throw_NotFoundException()
    {
        // Arrange 
        await using var context = new GovorDbContext(_options);
        var repository = new InvitesRepository(context, _validator);
        
        // Act & Assert 
        Assert.ThrowsAsync<NotFoundException>(async () => await repository.GetAllAsync());
    }

    [Test]
    public async Task Given_ValidInvitesId_When_FindByIdAsync_Then_Invites_Are_Returned()
    {
        // Arrange 
        var invitation = _fixture.Create<Invitation>();
        await using var context = new GovorDbContext(_options);
        var repository = new InvitesRepository(context, _validator);
        
        context.Invitations.Add(invitation);
        await context.SaveChangesAsync();
        
        // Act 
        var result = await repository.FindByIdAsync(invitation.Id);
        
        // Assert 
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(invitation.Id));
    }
    
    [Test]
    public async Task Given_InvalidInvitesId_When_FindByIdAsync_Should_Throw_NotFoundByKeyException()
    {
        // Arrange 
        await using var context = new GovorDbContext(_options);
        var repository = new InvitesRepository(context, _validator);
        
        // Act & Assert 
        Assert.ThrowsAsync<NotFoundByKeyException<Guid>>(async () => await repository.FindByIdAsync(Guid.Empty));
    }
    
    [Test]
    public async Task Given_ValidInvitesCode_When_FindByCodeAsync_Then_Invites_Are_Returned()
    {
        // Arrange 
        var invitation = _fixture.Create<Invitation>();
        await using var context = new GovorDbContext(_options);
        var repository = new InvitesRepository(context, _validator);
        
        context.Invitations.Add(invitation);
        await context.SaveChangesAsync();
        
        // Act 
        var result = await repository.FindByCodeAsync(invitation.Code);
        
        // Assert 
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(invitation.Id));
    }
    
    [Test]
    public async Task Given_InvalidCode_When_FindByCodeAsync_Should_Throw_NotFoundByKeyException()
    {
        // Arrange 
        await using var context = new GovorDbContext(_options);
        var repository = new InvitesRepository(context, _validator);
        
        // Act & Assert 
        Assert.ThrowsAsync<NotFoundByKeyException<string>>(async () => await repository.FindByCodeAsync(_fixture.Create<string>()));
    }
    
    [Test]
    public async Task Given_ValidAdminInvites_When_FindAdminsInvitesAsync_Then_Invites_Are_Returned()
    {
        // Arrange 
        var random = new Random();
        var invitations = _fixture.CreateMany<Invitation>(random.Next(3, 10)).ToList();
        
        foreach (var inv in invitations)
        {
            inv.IsAdmin = true;
        }
        
        await using var context = new GovorDbContext(_options);
        var repository = new InvitesRepository(context, _validator);
        
        context.Invitations.AddRange(invitations);
        await context.SaveChangesAsync();
        
        // Act 
        var result = await repository.FindAdminsInvitesAsync();
        
        // Assert 
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(invitations.Count()));
        Assert.That(result, Is.EquivalentTo(invitations));
    }
    
    [Test]
    public async Task Given_InvalidAdminsInvites_When_FindAdminsInvitesAsync_Should_Throw_NotFoundByKeyException()
    {
        // Arrange 
        await using var context = new GovorDbContext(_options);
        var repository = new InvitesRepository(context, _validator);
        
        // Act & Assert 
        Assert.ThrowsAsync<NotFoundByKeyException<bool>>(async () => await repository.FindAdminsInvitesAsync());
    }
    
    [Test]
    public async Task Given_ValidAdmin_When_AddAsync_Then_Added()
    {
        // Arrange
        var invitation = _fixture.Create<Invitation>();
        invitation.Code = string.Join("", _fixture.CreateMany<char>(12));
        invitation.EndDate = invitation.DateCreated.AddHours(1);
        
        await using var context = new GovorDbContext(_options);
        var repository = new InvitesRepository(context, _validator);
    
        // Act 
        await repository.AddAsync(invitation);
        
        // Assert 
        Assert.That(context.Invitations.Count, Is.EqualTo(1));
        Assert.That(context.Invitations.First(), Is.EqualTo(invitation));
    }
    
    [Test]
    public async Task Given_InvalidAdmin_When_AddAsync_Should_Throw_AdditionException()
    {
        // Arrange
        await using var context = new GovorDbContext(_options);
        var repository = new InvitesRepository(context, _validator);
        
        // Act & Assert 
        Assert.ThrowsAsync<AdditionException>(async () => await repository.AddAsync(default));
    }
    
    [Test]
    public async Task Given_NotExistInvitesCode_When_Exist_Then_Returns_False()
    {
        // Arrange
        await using var context = new GovorDbContext(_options);
        var repository = new InvitesRepository(context, _validator);
        
        // Act 
        var result = repository.Exist(Guid.NewGuid());
        var result2 = repository.Exist(_fixture.Create<string>());
        
        Assert.That(result, Is.False);
        Assert.That(result2, Is.False);
    }
    
    [Test]
    public async Task Given_ExistInvites_When_Exist_Then_Returns_False()
    {
        // Arrange
        var invitation = _fixture.Create<Invitation>();
        
        invitation.Code = string.Join("", _fixture.CreateMany<char>(12));
        invitation.EndDate = invitation.DateCreated.AddHours(1);
        
        await using var context = new GovorDbContext(_options);
        var repository = new InvitesRepository(context, _validator);
        
        context.Invitations.Add(invitation);
        await context.SaveChangesAsync();
        
        // Act 
        var result = repository.Exist(invitation.Id);
        var result2 = repository.Exist(invitation);
        var result3 = repository.Exist(invitation.Code); 
        
        Assert.That(result, Is.True);
        Assert.That(result2, Is.True);
        Assert.That(result3, Is.True);
    }
    
    
    [Test]
    public async Task Given_InvalidInvites_When_Exist_Then_Returns_False()
    {
        // Arrange
        await using var context = new GovorDbContext(_options);
        var repository = new InvitesRepository(context, _validator);
        
        // Act & Assert 
        Assert.Throws<InvalidObjectException<Invitation>>(() => repository.Exist(default(Invitation)));
    }
}