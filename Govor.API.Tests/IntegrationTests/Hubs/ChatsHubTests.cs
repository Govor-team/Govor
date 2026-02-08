using AutoFixture;
using Govor.API.Common.SignalR.Helpers;
using Govor.API.Hubs;
using Govor.Application.Interfaces;
using Govor.Application.Interfaces.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Govor.API.Tests.IntegrationTests.Hubs;

[TestFixture]
public class ChatsHubTests
{
    /*
    private Mock<ILogger<ChatsHub>> _loggerMock;
    private Mock<IMessageCommandService> _messageServiceMock;
    private Mock<IUserGroupsGetterService> _userGroupsServiceMock;
    private Mock<IHubUserAccessor> _hubUserAccessorMock;
    private Fixture _fixture;
    private ChatsHub _chatsHub;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        
        _messageServiceMock = new Mock<IMessageCommandService>();
        _userGroupsServiceMock = new Mock<IUserGroupsGetterService>();
        _loggerMock = new Mock<ILogger<ChatsHub>>();
        _hubUserAccessorMock = new Mock<IHubUserAccessor>();

        _chatsHub = new ChatsHub(
            _loggerMock.Object,
            _messageServiceMock.Object,
            _userGroupsServiceMock.Object,
            _hubUserAccessorMock.Object
        );
    }
    
    // Test for Send action 
    [Test]
    public void SendMessage_Success()
    {
        
    }


    [TearDown]
    public void TearDown()
    {
        _chatsHub?.Dispose();
    }
    */
}