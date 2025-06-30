using AutoFixture;
using Govor.Application.Exceptions.FriendsService;
using Govor.Application.Interfaces;
using Govor.Application.Services;
using Govor.Core.Models;
using Govor.Core.Repositories.Friendships;
using Govor.Core.Repositories.Users;
using Govor.Data.Repositories.Exceptions;
using Moq;

namespace Govor.API.Tests.UnitTests.Services;

[TestFixture]
public class FriendsServiceTests
{
    private Fixture _fixture;
    private Mock<IUsersRepository> _usersRepositoryMock;
    private Mock<IFriendshipsRepository> _friendshipsRepositoryMock;
    private IFriendsService _service;

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

        _service = new FriendsService(_usersRepositoryMock.Object, _friendshipsRepositoryMock.Object);
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
    public void SearchUsersAsync_Throws_SearchUsersException_IfThrowsNotFoundByKeyException_String_Guid()
    {
        // Arrange 
        _usersRepositoryMock
            .Setup(u => u.SearchPotentialFriendsAsync(It.IsAny<Guid>(), It.IsAny<string>()))
            .ThrowsAsync(new NotFoundByKeyException<(string,Guid)>(("test",Guid.NewGuid()),"test"));
        
        // Axt & Assert 
        Assert.ThrowsAsync<SearchUsersException>(async () => await _service.SearchUsersAsync("test", Guid.NewGuid()));
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
    public async Task SearchUsersAsync_Throws_UnauthorizedAccessException_IfThrowsSomeExceptionInUsersRepository()
    {
        // Arrange 
        _usersRepositoryMock
            .Setup(u => u.SearchPotentialFriendsAsync(It.IsAny<Guid>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("test"));
        
        // Act & Assert 
        Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _service.SearchUsersAsync("test", Guid.NewGuid()));
    }
    
    [Test]
    public async Task SearchUsersAsync_Throws_UnauthorizedAccessException_IfThrowsSomeExceptionInFriendshipsRepository()
    {
        // Arrange 
        _friendshipsRepositoryMock
            .Setup(u => u.FindByUserIdAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new Exception("test"));
        
        // Act & Assert 
        Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _service.SearchUsersAsync("test", Guid.NewGuid()));
    }
    
    // SendFriendRequestAsync
    
    [Test]
    public void SendFriendRequestAsync_SendingToSelf_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act + Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.SendFriendRequestAsync(userId, userId));

        Assert.That(ex.Message, Is.EqualTo("Cannot send a request to self user"));
    }

    [Test]
    public void SendFriendRequestAsync_RequestAlreadyExists_ThrowsRequestAlreadySentException()
    {
        // Arrange
        var fromUserId = Guid.NewGuid();
        var toUserId = Guid.NewGuid();

        _friendshipsRepositoryMock
            .Setup(repo => repo.Exist(fromUserId, toUserId))
            .Returns(true);

        // Act + Assert
        Assert.ThrowsAsync<RequestAlreadySentException>(() =>
            _service.SendFriendRequestAsync(fromUserId, toUserId));
    }

    [Test]
    public async Task SendFriendRequestAsync_ValidRequest_CallsAddAsync()
    {
        // Arrange
        var fromUserId = Guid.NewGuid();
        var toUserId = Guid.NewGuid();

        _friendshipsRepositoryMock
            .Setup(repo => repo.Exist(fromUserId, toUserId))
            .Returns(false);

        // Act
        await _service.SendFriendRequestAsync(fromUserId, toUserId);

        // Assert
        _friendshipsRepositoryMock.Verify(repo =>
                repo.AddAsync(It.Is<Friendship>(f =>
                    f.RequesterId == fromUserId &&
                    f.AddresseeId == toUserId &&
                    f.Status == FriendshipStatus.Pending &&
                    f.Id != Guid.Empty
                )),
            Times.Once);
    }
}
