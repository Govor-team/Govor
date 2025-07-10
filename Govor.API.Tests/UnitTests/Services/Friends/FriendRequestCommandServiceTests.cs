using AutoFixture;
using Govor.Application.Exceptions.FriendsService;
using Govor.Application.Interfaces.Friends;
using Govor.Application.Services.Friends;
using Govor.Core.Models;
using Govor.Core.Repositories.Friendships;
using Govor.Core.Repositories.Users;
using Govor.Data.Repositories.Exceptions;
using Moq;

namespace Govor.API.Tests.UnitTests.Services.Friends;

[TestFixture]
public class FriendRequestCommandServiceTests
{
    private Fixture _fixture;
    private Mock<IUsersRepository> _usersRepositoryMock;
    private Mock<IFriendshipsRepository> _friendshipsRepositoryMock;
    private IFriendRequestCommandService _service;

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

        _service = new FriendRequestCommandService(_friendshipsRepositoryMock.Object);
    }
    
    // SendFriendRequestAsync
    [Test]
    public void SendFriendRequestAsync_SendingToSelf_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.SendAsync(userId, userId));

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
            _service.SendAsync(fromUserId, toUserId));
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
        await _service.SendAsync(fromUserId, toUserId);

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
        await _service.AcceptAsync(friendship.Id, friendship.AddresseeId);

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
        Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.AcceptAsync(friendship.Id, friendship.AddresseeId));
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
            await _service.AcceptAsync(friendship.Id, wrongUserId));
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
            await _service.AcceptAsync(requestId, userId));
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

        await _service.AcceptAsync(friendship.Id, friendship.AddresseeId);

        Assert.That(updatedFriendship.Status, Is.EqualTo(FriendshipStatus.Accepted));
    }
    
    // RejectFriend
    [Test]
    public async Task RejectFriendRequestAsync_ValidRequest_CallsRemoveAsync()
    {
        var friendship = _fixture.Build<Friendship>()
            .With(f => f.Status, FriendshipStatus.Pending)
            .Create();

        _friendshipsRepositoryMock
            .Setup(r => r.GetByIdAsync(friendship.Id))
            .ReturnsAsync(friendship);
        
        await _service.RejectAsync(friendship.Id, friendship.AddresseeId);

        // Assert
        _friendshipsRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Friendship>(f =>
            f.Id == friendship.Id &&
            f.RequesterId == friendship.RequesterId &&
            f.AddresseeId == friendship.AddresseeId &&
            f.Status == FriendshipStatus.Rejected
        )), Times.Once);
    }

    [Test]
    public void RejectFriendRequestAsync_Throws_InvalidOperationException_IfStatusNotEqualPendingOrReject()
    {
        var friendship = _fixture.Build<Friendship>()
            .With(f => f.Status, FriendshipStatus.Accepted)
            .Create();
        
        _friendshipsRepositoryMock
            .Setup(r => r.GetByIdAsync(friendship.Id))
            .ReturnsAsync(friendship);

        // Act & Assert 
        Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.RejectAsync(friendship.Id, friendship.AddresseeId));
    }
    
    [Test]
    public void RejectFriendRequestAsync_Throws_UnauthorizedAccessException_IfCurrentUserIdIsNotAddressee()
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
            await _service.RejectAsync(friendship.Id, wrongUserId));
    }
    
    [Test]
    public void RejectFriendRequestAsync_Throws_InvalidOperationException_IfFriendshipNotFound()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _friendshipsRepositoryMock
            .Setup(r => r.GetByIdAsync(requestId))
            .ThrowsAsync(new NotFoundByKeyException<Guid>(requestId));

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.RejectAsync(requestId, userId));
    }

    [Test]
    public async Task RejectFriendRequestAsync_ChangesStatusToAccepted()
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

        await _service.RejectAsync(friendship.Id, friendship.AddresseeId);

        Assert.That(updatedFriendship.Status, Is.EqualTo(FriendshipStatus.Rejected));
    }
    
   
}