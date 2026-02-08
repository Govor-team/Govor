using AutoFixture;
using Govor.Application.Exceptions.VerifyFriendship;
using Govor.Application.Services;
using Govor.Core.Models;
using Govor.Core.Repositories.Friendships;
using Govor.Data.Repositories.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Govor.Application.Tests.Services;

[TestFixture]
public class VerifyFriendshipTests
{
    private Fixture _fixture;
    private Mock<ILogger<VerifyFriendship>> _mockLogger;
    private Mock<IFriendshipsRepository> _mockFriendships;
    private VerifyFriendship _service;
    
    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _fixture.Behaviors
            .OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _mockLogger = new Mock<ILogger<VerifyFriendship>>();
        _mockFriendships = new Mock<IFriendshipsRepository>();

        _service = new VerifyFriendship(_mockFriendships.Object, _mockLogger.Object);
    }
    
    // Test for VerifyAsync action 
    [Test]
    public async Task VerifyAsync_Success()
    {
        // Arrange 
        var friendship1 = _fixture.Create<Friendship>();
        friendship1.Status = FriendshipStatus.Accepted;
        _mockFriendships.Setup(f => f.FindByUserIdAsync(friendship1.RequesterId))
            .ReturnsAsync([friendship1]);
        // Act 
        await _service.VerifyAsync(friendship1.RequesterId, friendship1.AddresseeId);
        // Assert 
        _mockFriendships.Verify(f => f.FindByUserIdAsync(friendship1.RequesterId), Times.Once());
        Assert.That(await _service.TryVerifyAsync(friendship1.RequesterId, friendship1.AddresseeId), Is.EqualTo(true));
    }

    [Test]
    public async Task VerifyAsync_IDsEmpty()
    {
        // Arrange 
        var friendship1 = _fixture.Create<Friendship>();
        friendship1.RequesterId = Guid.Empty;
        friendship1.AddresseeId = Guid.Empty;
        
        // Act & Assert 
        Assert.ThrowsAsync<ArgumentException>(() => _service.VerifyAsync(friendship1.RequesterId, friendship1.AddresseeId));
        Assert.That(await _service.TryVerifyAsync(friendship1.RequesterId, friendship1.AddresseeId), Is.EqualTo(false));
    }
    
    [Test]
    public async Task VerifyAsync_ThrowsNotFoundByKeyException_ThrowsFriendshipException()
    {
        // Arrange 
        var friendship1 = _fixture.Create<Friendship>();
        friendship1.Status = FriendshipStatus.Accepted;
        
        _mockFriendships.Setup(f => f.FindByUserIdAsync(friendship1.RequesterId))
            .ThrowsAsync(new NotFoundByKeyException<Guid>(friendship1.RequesterId));
        
        // Act & Assert 
        Assert.ThrowsAsync<FriendshipException>(async () => await _service.VerifyAsync(friendship1.RequesterId, friendship1.AddresseeId));
        _mockFriendships.Verify(f => f.FindByUserIdAsync(friendship1.RequesterId), Times.Once());
        Assert.That(await _service.TryVerifyAsync(friendship1.RequesterId, friendship1.AddresseeId), Is.EqualTo(false));
    }
    
    [Test]
    public async Task VerifyAsync_ReturnsEmptyFriendships_ThrowsFriendshipException()
    {
        // Arrange 
        var friendship1 = _fixture.Create<Friendship>();
        friendship1.Status = FriendshipStatus.Accepted;
        
        _mockFriendships.Setup(f => f.FindByUserIdAsync(friendship1.RequesterId))
            .ReturnsAsync(new List<Friendship>());
        
        // Act & Assert 
        Assert.ThrowsAsync<FriendshipException>(async () => await _service.VerifyAsync(friendship1.RequesterId, friendship1.AddresseeId));
        _mockFriendships.Verify(f => f.FindByUserIdAsync(friendship1.RequesterId), Times.Once());
        Assert.That(await _service.TryVerifyAsync(friendship1.RequesterId, friendship1.AddresseeId), Is.EqualTo(false));
    }
}