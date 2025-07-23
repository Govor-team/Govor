using Govor.Application.Interfaces.UserOnlineStatus;
using Govor.Application.Services.UserOnlineStatus;

namespace Govor.Application.Tests.Services.UserOnlineStatus;

[TestFixture]
public class OnlineUserStoreTests
{
    private IOnlineUserStore _store;
    private Guid _userId1;
    private Guid _userId2;

    [SetUp]
    public void Setup()
    {
        _store = new OnlineUserStore();
        _userId1 = Guid.NewGuid();
        _userId2 = Guid.NewGuid();
    }

    [Test]
    public void SetOnlineUser_UserIsMarkedOnline()
    {
        // Act 
        _store.SetOnlineUser(_userId1);

        // Assert 
        Assert.That(_store.IsOnline(_userId1), Is.True);
    }

    [Test]
    public void SetOfflineUser_UserIsNoLongerOnline()
    {
        // Act 
        _store.SetOnlineUser(_userId1);
        _store.SetOfflineUser(_userId1);
        
        // Assert 
        Assert.That(_store.IsOnline(_userId1), Is.False);
    }

    [Test]
    public void IsOnline_ReturnsFalse_ForUnknownUser()
    {
        // Act & Assert 
        Assert.That(_store.IsOnline(Guid.NewGuid()), Is.False);
    }

    [Test]
    public void GetAllOnlineUsers_ReturnsAllCurrentlyOnlineUsers()
    {
        // Arrange  
        _store.SetOnlineUser(_userId1);
        _store.SetOnlineUser(_userId2);

        // Act 
        var onlineUsers = _store.GetAllOnlineUsers();
    
        // Assert 
        Assert.That(onlineUsers, Is.EquivalentTo(new[] { _userId1, _userId2 }));
    }

    [Test]
    public void SetOnlineUser_Twice_DoesNotThrow()
    {
        // Act & Assert  
        Assert.DoesNotThrow(() =>
        {
            _store.SetOnlineUser(_userId1);
            _store.SetOnlineUser(_userId1); 
        });

        Assert.That(_store.IsOnline(_userId1), Is.True);
    }

    [Test]
    public void SetOfflineUser_ForUnknownUser_DoesNotThrow()
    {
        // Act & Assert 
        Assert.DoesNotThrow(() => _store.SetOfflineUser(Guid.NewGuid()));
    }
}