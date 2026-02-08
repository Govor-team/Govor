using AutoFixture;
using Govor.Application.Interfaces.Friends;
using Govor.Application.Services.Friends;
using Govor.Core.Models;
using Govor.Core.Models.Users;
using Govor.Core.Repositories.Friendships;
using Govor.Core.Repositories.Users;
using Govor.Data.Repositories.Exceptions;
using Moq;

namespace Govor.Application.Tests.Services.Friends;

[TestFixture]
public class FriendRequestQueryServiceTests
{
    private Fixture _fixture;
    private Mock<IUsersRepository> _usersRepositoryMock;
    private Mock<IFriendshipsRepository> _friendshipsRepositoryMock;
    private IFriendRequestQueryService _service;

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

        _service = new FriendRequestQueryService(_friendshipsRepositoryMock.Object);
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
        
        _friendshipsRepositoryMock.Setup(f => f.FindByUserIdAsync(userId))
            .ReturnsAsync(friendships);
        
        // Act 
        var result = await _service.GetIncomingAsync(userId);
        
        // Assert 
        Assert.That(result.Count, Is.EqualTo(friendships.Count));
        Assert.That(result.Select(u => u.Id), Is.EquivalentTo(friendships.Select(f => f.Id)));
    }
    
    [Test]
    public void GetIncomingRequestsAsync_ThrowsInvalidOperationException_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _friendshipsRepositoryMock
            .Setup(r => r.FindByUserIdAsync(userId))
            .ThrowsAsync(new NotFoundByKeyException<Guid>(userId));

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.GetIncomingAsync(userId));
    }
    
    // GetResponsesAsync
    [Test]
    public async Task GetResponsesAsync_ReturnsFriendships_IfFriendshipsExists()
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
            f.RequesterId = userId;
            f.Requester = user;
            f.Status = FriendshipStatus.Rejected;
        });
        
        _friendshipsRepositoryMock.Setup(f => f.FindByUserIdAsync(userId))
            .ReturnsAsync(friendships);
        
        // Act 
        var result = await _service.GetResponsesAsync(userId);
        
        // Assert 
        Assert.That(result.Count, Is.EqualTo(friendships.Count));
        Assert.That(result.Select(u => u.Id), Is.EquivalentTo(friendships.Select(f => f.Id)));
    }
    
    [Test]
    public void GetResponsesAsync_ThrowsInvalidOperationException_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _friendshipsRepositoryMock
            .Setup(r => r.FindByUserIdAsync(userId))
            .ThrowsAsync(new NotFoundByKeyException<Guid>(userId));

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.GetResponsesAsync(userId));
    }

}