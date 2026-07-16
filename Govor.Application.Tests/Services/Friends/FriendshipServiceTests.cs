using AutoFixture;
using Govor.Application.Interfaces.Friends;
using Govor.Application.Services.Friends;
using Govor.Domain.Models;
using Govor.Domain.Models.Users;
using Govor.Domain.Repositories.Friendships;
using Govor.Domain.Repositories.Users;
using Govor.Data.Repositories.Exceptions;
using Moq;

namespace Govor.Application.Tests.Services.Friends;

[TestFixture]
public class FriendshipServiceTests
{
    private Fixture _fixture;
    private Mock<IUsersRepository> _usersRepositoryMock;
    private Mock<IFriendshipsRepository> _friendshipsRepositoryMock;
    IFriendshipService _service;
    
    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _fixture.Behaviors
            .OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _usersRepositoryMock = new Mock<IUsersRepository>();
        _friendshipsRepositoryMock = new Mock<IFriendshipsRepository>();

        _service = new FriendshipService(_usersRepositoryMock.Object, _friendshipsRepositoryMock.Object);
    }
    
    // SearchUsersAsync
    [Test]
    public async Task SearchUsersAsync_ReturnsUsers_IfNotAlreadyFriends()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var user = _fixture.Create<User>();
        user.Id = Guid.NewGuid();

        _usersRepositoryMock
            .Setup(u => u.SearchPotentialFriendsAsync(userId, It.IsAny<string>()))
            .ReturnsAsync(new List<User> { user });

        _friendshipsRepositoryMock
            .Setup(f => f.FindByUserIdAsync(userId))
            .ReturnsAsync(new List<Friendship>()); // No friends

        // Act
        var result = await _service.SearchUsersAsync("test", userId);

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Id, Is.EqualTo(user.Id));
    }
    
    [Test]
    public async Task SearchUsersAsync_Returns_EmptyList_IfThrowsNotFoundByKeyException_String_Guid()
    {
        // Arrange 
        _usersRepositoryMock
            .Setup(u => u.SearchPotentialFriendsAsync(It.IsAny<Guid>(), It.IsAny<string>()))
            .ThrowsAsync(new NotFoundByKeyException<(string,Guid)>(("test",Guid.NewGuid()),"test"));
        
        // Act
        var users = await _service.SearchUsersAsync("test", Guid.NewGuid());
        
        // & Assert 
        Assert.That(users, Is.Empty);
    }
    
    [Test]
    public async Task SearchUsersAsync_ReturnsUsersWithoutFriendships_IfThrowsNotFoundByKeyException_Guid()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var user = _fixture.Create<User>();
        user.Id = Guid.NewGuid();

        _usersRepositoryMock
            .Setup(u => u.SearchPotentialFriendsAsync(userId, It.IsAny<string>()))
            .ReturnsAsync(new List<User> { user });

        _friendshipsRepositoryMock
            .Setup(f => f.FindByUserIdAsync(userId))
            .ThrowsAsync(new NotFoundByKeyException<Guid>(userId, "test"));
        
        // Act 
        var result = await _service.SearchUsersAsync("test", userId);
        
        // Assert 
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Id, Is.EqualTo(user.Id));
    }

    [Test]
    public void SearchUsersAsync_Throws_UnauthorizedAccessException_IfThrowsSomeExceptionInUsersRepository()
    {
        // Arrange 
        _usersRepositoryMock
            .Setup(u => u.SearchPotentialFriendsAsync(It.IsAny<Guid>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("test"));
        
        // Act & Assert 
        Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _service.SearchUsersAsync("test", Guid.NewGuid()));
    }
    
    [Test]
    public void SearchUsersAsync_Throws_UnauthorizedAccessException_IfThrowsSomeExceptionInFriendshipsRepository()
    {
        // Arrange 
        _usersRepositoryMock
            .Setup(u => u.SearchPotentialFriendsAsync(It.IsAny<Guid>(), "test"))
            .ThrowsAsync(new Exception("test"));
        
        // Act & Assert 
        Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await _service.SearchUsersAsync("test", Guid.NewGuid())
            );
    }
    
    // GetFriendsAsync
    [Test]
    public async Task GetFriendsAsync_ReturnsUsers_IfUserExists()
    {
        // Arrange 
        var userId = Guid.NewGuid();
        var friendships = _fixture.CreateMany<Friendship>().ToList();
        
        friendships.ForEach(f =>
        {
            f.RequesterId = userId;
            f.Status = FriendshipStatus.Accepted;
        });
        
        _friendshipsRepositoryMock.Setup(f => f.FindByUserIdAsync(userId))
            .ReturnsAsync(friendships);
        
        // Act 
        var result = await _service.GetFriendsAsync(userId);
        
        // Assert 
        Assert.That(result.Count, Is.EqualTo(friendships.Count));
        Assert.That(result, Is.EquivalentTo(friendships.Select(f => f.Addressee)));
    }
    
    [Test]
    public void GetFriendsAsync_Throws_InvalidOperationException_IfUserNotExists()
    {
        // Arrange 
        _friendshipsRepositoryMock.Setup(f => f.FindByUserIdAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new NotFoundByKeyException<Guid>(Guid.NewGuid()));
        
        // Act & Assert 
        Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.GetFriendsAsync(Guid.NewGuid()));
    }
}