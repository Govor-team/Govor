using AutoFixture;
using AutoMapper;
using Govor.API.Controllers;
using Govor.Application.Interfaces;
using Govor.Application.Interfaces.Infrastructure.Extensions;
using Govor.Contracts.Requests;
using Govor.Contracts.Responses;
using Govor.Core.Models.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Govor.API.Tests.IntegrationTests.Controllers;

[TestFixture]
public class ChatLoadControllerTests
{
    private Mock<ICurrentUserService> _currentUserServiceMock;
    private Mock<ILogger<ChatLoadController>> _loggerMock;
    private Mock<IMessagesLoader> _messagesLoaderMock;
    private Mock<IMapper> _mapperMock;
    private Fixture _fixture;
    private ChatLoadController _controller;
    
    private Guid _userId = Guid.NewGuid();
    private Guid _chatId = Guid.NewGuid();
    private Guid _currentId = Guid.NewGuid();
    
    
    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _loggerMock = new Mock<ILogger<ChatLoadController>>();
        _messagesLoaderMock = new Mock<IMessagesLoader>();
        _mapperMock = new Mock<IMapper>();
        
        _currentUserServiceMock.Setup(u => u.GetCurrentUserId()).Returns(_currentId);
        
        _controller = new ChatLoadController(
            _loggerMock.Object,
            _messagesLoaderMock.Object,
            _currentUserServiceMock.Object,
            _mapperMock.Object);
    }
    
    // Tests for GetChatMessages action 
    [Test]
    public async Task GetChatMessages_ValidRequest_ReturnsOkResult()
    {
        // Arrange 
        var random = new Random();
        var messages = _fixture.CreateMany<Message>(random.Next(3, 10)).ToList();
        var before = random.Next(1, 10);
        var after = random.Next(1, 10);

        var query = new MessageQuery()
        {
            Before = before,
            After = after,
            StartMessageId = null
        };
        
        _messagesLoaderMock.Setup(m => 
            m.LoadMessagesInChatGroup(_chatId, _currentId, null, before, after))
            .ReturnsAsync(messages);

        var messagesResponse = messages.Select(m => new MessageResponse()
        {
            Id = m.Id,
            SenderId = m.SenderId,
            RecipientId = m.RecipientId,
            RecipientType = m.RecipientType,
        }).ToList();
        
        _mapperMock.Setup(f => f.Map<List<MessageResponse>>(messages)).
            Returns(messagesResponse);
        
        // Act 
        var result = await _controller.GetGroupMessages(_chatId, query);
        
        // Assert 
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        
        var okResult = (OkObjectResult)result;
        var values = okResult.Value as List<MessageResponse>;
        
        Assert.That(values, Is.Not.Null);
        
        Assert.That(values.Select(v => v.Id), 
            Is.EquivalentTo(messagesResponse.Select(m => m.Id)));
        
        Assert.That(values.Select(v => v.SenderId), 
            Is.EquivalentTo(messagesResponse.Select(m => m.SenderId)));
        
        Assert.That(values.Select(v => v.RecipientId), 
            Is.EquivalentTo(messagesResponse.Select(m => m.RecipientId)));
        
        Assert.That(values.Select(v => v.RecipientType), 
            Is.EquivalentTo(messagesResponse.Select(m => m.RecipientType)));
    }
}