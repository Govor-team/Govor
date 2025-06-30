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

        // Act & Assert
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

        // Act & Assert
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
    
    // AcceptFriendRequestAsync
    [Test]
    public async Task AcceptFriendRequestAsync_ValidRequest_CallsUpdateAsync()
    {
        var friendship = _fixture.Build<Friendship>()
            .With(f => f.Status, FriendshipStatus.Pending)
            .Create();

        _friendshipsRepositoryMock
            .Setup(r => r.GetByIdAsync(friendship.Id))
            .ReturnsAsync(friendship);

        // Act: вызываем именно Accept, не Send
        await _service.AcceptFriendRequestAsync(friendship.Id, friendship.AddresseeId);

        // Assert
        _friendshipsRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Friendship>(f =>
            f.Id == friendship.Id &&
            f.RequesterId == friendship.RequesterId &&
            f.AddresseeId == friendship.AddresseeId &&
            f.Status == FriendshipStatus.Accepted
        )), Times.Once);
    }

    [Test]
    public void AcceptFriendRequestAsync_Throws_InvalidOperationException_IfStatusNotEqualPending()
    {
        var friendship = _fixture.Build<Friendship>()
            .With(f => f.Status, FriendshipStatus.Accepted)
            .Create();
        
        _friendshipsRepositoryMock
            .Setup(r => r.GetByIdAsync(friendship.Id))
            .ReturnsAsync(friendship);

        // Act & Assert 
        Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.AcceptFriendRequestAsync(friendship.Id, friendship.AddresseeId));
    }
    [Test]
    public void AcceptFriendRequestAsync_Throws_UnauthorizedAccessException_IfCurrentUserIdIsNotAddressee()
    {
        // Arrange
        var friendship = _fixture.Build<Friendship>()
            .With(f => f.Status, FriendshipStatus.Pending)
            .Create();

        _friendshipsRepositoryMock
            .Setup(r => r.GetByIdAsync(friendship.Id))
            .ReturnsAsync(friendship);

        var wrongUserId = Guid.NewGuid(); // не совпадает с AddresseeId

        // Act & Assert
        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.AcceptFriendRequestAsync(friendship.Id, wrongUserId));
    }
    
    [Test]
    public void AcceptFriendRequestAsync_Throws_InvalidOperationException_IfFriendshipNotFound()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _friendshipsRepositoryMock
            .Setup(r => r.GetByIdAsync(requestId))
            .ThrowsAsync(new NotFoundByKeyException<Guid>(requestId));

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.AcceptFriendRequestAsync(requestId, userId));
    }

    [Test]
    public async Task AcceptFriendRequestAsync_ChangesStatusToAccepted()
    {
        var friendship = _fixture.Build<Friendship>()
            .With(f => f.Status, FriendshipStatus.Pending)
            .Create();

        _friendshipsRepositoryMock
            .Setup(r => r.GetByIdAsync(friendship.Id))
            .ReturnsAsync(friendship);

        Friendship updatedFriendship = null;

        _friendshipsRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Friendship>()))
            .Callback<Friendship>(f => updatedFriendship = f)
            .Returns(Task.CompletedTask);

        await _service.AcceptFriendRequestAsync(friendship.Id, friendship.AddresseeId);

        Assert.That(updatedFriendship.Status, Is.EqualTo(FriendshipStatus.Accepted));
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
    
    // GetIncomingRequestsAsync

    [Test]
    public async Task GetIncomingRequestsAsync_ReturnsFriendships_IfFriendshipsExists()
    {
        // Arrange 
        var userId = Guid.NewGuid();
        
        var friendships = _fixture.CreateMany<Friendship>().ToList();
        
        var user = _fixture.Build<User>()
            .With(u => u.Id, userId)
            .With(u => u.ReceivedFriendRequests, friendships)
            .Create();
        
        friendships.ForEach(f =>
        {
            f.AddresseeId = userId;
            f.Addressee = user;
            f.Status = FriendshipStatus.Pending;
        });
        
        _usersRepositoryMock.Setup(f => f.FindByIdAsync(userId))
            .ReturnsAsync(user);
        
        // Act 
        var result = await _service.GetIncomingRequestsAsync(userId);
        
        // Assert 
        Assert.That(result.Count, Is.EqualTo(friendships.Count));
        Assert.That(result.Select(u => u.Id), Is.EquivalentTo(friendships.Select(f => f.Id)));
    }
    
    [Test]
    public void GetIncomingRequestsAsync_ThrowsInvalidOperationException_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _usersRepositoryMock
            .Setup(r => r.FindByIdAsync(userId))
            .ThrowsAsync(new NotFoundByKeyException<Guid>(userId));

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.GetIncomingRequestsAsync(userId));
    }

}
