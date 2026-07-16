using AutoFixture;
using Govor.Application.Groups;
using Govor.Application.Interfaces;
using Govor.Domain.Models;
using Govor.Domain.Repositories.Groups;
using Govor.Data.Repositories.Exceptions;
using Moq;

namespace Govor.Application.Tests.Services;

[TestFixture]
public class UserGroupsGetterServiceTests
{
    private Fixture _fixture;
    private Mock<IGroupsRepository> _repositoryMock;
    private IUserGroupsGetterService _getterService;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();

        _fixture.Behaviors
            .OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        
        _repositoryMock = new Mock<IGroupsRepository>();
        
        _getterService = new UserGroupsGetterService(_repositoryMock.Object);
    }

    [Test]
    public async Task GetUserGroups_ShouldReturnAllUserGroups()
    {
        // Arrange 
        var chats = _fixture.CreateMany<ChatGroup>();
        var userId = chats.First().Members.First().Id;

        _repositoryMock.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync([chats.First()]);
        
        // Act 
        var result = await _getterService.GetUserGroupsAsync(userId);
        
        // Assert 
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(result, Is.EquivalentTo([chats.First()]));
    }
    
    [Test]
    public async Task GetUserGroups_ButGroupDoesNotExist_ShouldReturnEmptyList()
    {
        // Arrange 
        var userId = _fixture.Create<Guid>();
        
        _repositoryMock.Setup(r => r.GetByUserIdAsync(userId))
            .ThrowsAsync(new NotFoundByKeyException<Guid>(userId));
        
        // Act 
        var result = await _getterService.GetUserGroupsAsync(userId);
        
        // Assert 
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(0));
    }
}