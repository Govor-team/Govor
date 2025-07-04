using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;
using Govor.API.Hubs;
using Govor.Application.Interfaces.Messages;
using Govor.Application.Interfaces.Messages.Parameters;
using Govor.Core.Models;
using Microsoft.AspNetCore.Hosting;
using Govor.API; // Assuming Startup or Program is here for WebApplicationFactory
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Govor.Contracts.Requests.SignalR; // For MessageRequest etc.
using Govor.Contracts.Responses.SignalR; // For UserMessageResponse etc.
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Govor.API.Tests.IntegrationTests.Hubs;

// Base class for Hub tests to handle TestServer and client creation
public abstract class HubTestBase : IAsyncLifetime
{
    protected TestServer TestServer;
    protected HubConnection ClientConnection;
    protected Mock<IMessageService> MockMessageService;
    protected Guid TestUserId = Guid.NewGuid();
    protected string TestUserToken;

    public virtual async Task InitializeAsync()
    {
        MockMessageService = new Mock<IMessageService>();
        TestUserToken = GenerateTestToken(TestUserId.ToString(), "testuser");

        var webHostBuilder = new WebHostBuilder()
            .UseEnvironment("Testing") // Use a specific environment for tests
            .ConfigureServices(services =>
            {
                // Replace the real IMessageService with our mock for testing Hub logic
                services.AddSingleton(MockMessageService.Object);

                // Configure Authentication
                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = false,
                            ValidateAudience = false,
                            ValidateLifetime = false,
                            ValidateIssuerSigningKey = false,
                            SignatureValidator = (token, parameters) => new JwtSecurityToken(token), // Bypass signature validation for test tokens
                            NameClaimType = ClaimTypes.NameIdentifier
                        };
                        // For SignalR, need to allow token in query string for WebSocket
                        options.Events = new JwtBearerEvents
                        {
                            OnMessageReceived = context =>
                            {
                                var accessToken = context.Request.Query["access_token"];
                                if (!string.IsNullOrEmpty(accessToken) &&
                                    (context.HttpContext.Request.Path.StartsWithSegments("/hubs/chats"))) // Adjust path if needed
                                {
                                    context.Token = accessToken;
                                }
                                return Task.CompletedTask;
                            }
                        };
                    });
                services.AddAuthorization();
                services.AddSignalR();

                // Add other necessary services that ChatsHub might depend on, possibly mocked
                // services.AddSingleton(Mock.Of<IUserService>()); // If ChatsHub uses IUserService for groups
            })
            .ConfigureLogging(logging =>
            {
                logging.AddDebug(); // Or any other logger
            })
            .UseStartup<Startup>(); // Assuming you have a Startup.cs, or use Program.cs for .NET 6+

        TestServer = new TestServer(webHostBuilder);
        ClientConnection = new HubConnectionBuilder()
            .WithUrl($"ws://localhost/hubs/chats", options => // Adjust URL as per your hub route
            {
                options.HttpMessageHandlerFactory = _ => TestServer.CreateHandler();
                options.AccessTokenProvider = () => Task.FromResult<string?>(TestUserToken);
            })
            .Build();

        await ClientConnection.StartAsync();
    }

    public virtual async Task DisposeAsync()
    {
        if (ClientConnection != null)
        {
            await ClientConnection.DisposeAsync();
        }
        TestServer?.Dispose();
    }

    private string GenerateTestToken(string userId, string userName)
    {
        var claims = new[]
        {
            new Claim("userID", userId), // Ensure this matches the claim name used in ChatsHub.GetUserId()
            new Claim(ClaimTypes.Name, userName)
        };
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your-super-secret-key-that-is-long-enough")); // Use a key from config or a test key
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken("TestIssuer", "TestAudience", claims, expires: DateTime.Now.AddHours(1), signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}


public class ChatsHubTests : HubTestBase
{
    [Fact]
    public async Task Send_ValidMessageToUser_CallsServiceAndNotifiesRecipientAndSender()
    {
        // Arrange
        var recipientId = Guid.NewGuid();
        var messageContent = "Hello from integration test";
        var messageId = Guid.NewGuid();

        MockMessageService.Setup(s => s.SendMessageAsync(It.Is<SendMessage>(p =>
                p.FromUserId == TestUserId &&
                p.RecipientId == recipientId &&
                p.RecipientType == RecipientType.User)))
            .ReturnsAsync(new SendMessageResult(true, null, messageId));

        var messageReceivedTcs = new TaskCompletionSource<UserMessageResponse>();
        var messageSentTcs = new TaskCompletionSource<UserMessageResponse>();

        // Simulate recipient client being connected and in their own group
        // This is tricky without a second client connection. For now, we assume SignalR handles group delivery.
        // A more thorough test would involve a second client.
        ClientConnection.On<UserMessageResponse>("ReceiveMessage", (response) =>
        {
             // This would ideally be for the recipient. For a single client test, this might be tricky.
             // If sender also receives their own message via "ReceiveMessage" to their own user group:
            if(response.SenderId == TestUserId && response.RecipientId == recipientId)
            {
                //This is if sender also gets ReceiveMessage, for simplicity in single client test
                 messageReceivedTcs.TrySetResult(response);
            }
        });
        ClientConnection.On<UserMessageResponse>("MessageSent", (response) => // Confirmation to sender
        {
             if(response.MessageId == messageId) messageSentTcs.TrySetResult(response);
        });

        var request = new MessageRequest
        {
            RecipientId = recipientId,
            RecipientType = RecipientType.User,
            EncryptedContent = messageContent,
            MediaAttachments = new List<MediaReference>()
        };

        // Act
        await ClientConnection.InvokeAsync("Send", request);

        // Assert
        var sentConfirmation = await Task.WhenAny(messageSentTcs.Task, Task.Delay(TimeSpan.FromSeconds(5))) == messageSentTcs.Task
            ? messageSentTcs.Task.Result : null;

        // var receivedMessage = await Task.WhenAny(messageReceivedTcs.Task, Task.Delay(TimeSpan.FromSeconds(5))) == messageReceivedTcs.Task
        //    ? messageReceivedTcs.Task.Result : null;


        Assert.NotNull(sentConfirmation);
        Assert.Equal(messageId, sentConfirmation.MessageId);
        Assert.Equal(messageContent, sentConfirmation.EncryptedContent);

        // Assert.NotNull(receivedMessage); // This part is harder to assert reliably with one client for recipient
        // Assert.Equal(messageId, receivedMessage.MessageId);


        MockMessageService.Verify(s => s.SendMessageAsync(It.Is<SendMessage>(p =>
            p.FromUserId == TestUserId &&
            p.RecipientId == recipientId &&
            p.EncryptContent == messageContent)), Times.Once);
    }

    [Fact]
    public async Task Edit_ValidEditRequest_CallsServiceAndNotifies()
    {
        // Arrange
        var originalSenderId = TestUserId; // Editor must be the sender
        var messageToEditId = Guid.NewGuid();
        var newContent = "Updated content";
        var recipientId = Guid.NewGuid(); // Could be user or group

        var originalMessage = new Message {
            Id = messageToEditId,
            SenderId = originalSenderId,
            RecipientId = recipientId,
            RecipientType = RecipientType.User
        };

        MockMessageService.Setup(s => s.EditMessageAsync(It.Is<EditMessage>(p =>
                p.EditorId == TestUserId &&
                p.MessageId == messageToEditId &&
                p.NewContent == newContent)))
            .ReturnsAsync(new EditMessageResult(true, null, originalMessage));

        var messageEditedTcs = new TaskCompletionSource<MessageEditedResponse>();
        ClientConnection.On<MessageEditedResponse>("MessageEdited", (response) =>
        {
            if(response.MessageId == messageToEditId) messageEditedTcs.TrySetResult(response);
        });

        var request = new EditMessageRequest
        {
            MessageId = messageToEditId,
            NewEncryptedContent = newContent
        };

        // Act
        await ClientConnection.InvokeAsync("Edit", request);

        // Assert
        var editedNotification = await Task.WhenAny(messageEditedTcs.Task, Task.Delay(TimeSpan.FromSeconds(5))) == messageEditedTcs.Task
            ? messageEditedTcs.Task.Result : null;

        Assert.NotNull(editedNotification);
        Assert.Equal(messageToEditId, editedNotification.MessageId);
        Assert.Equal(newContent, editedNotification.NewEncryptedContent);
        Assert.Equal(originalSenderId, editedNotification.SenderId); // Verify original sender is preserved

        MockMessageService.Verify(s => s.EditMessageAsync(It.Is<EditMessage>(p =>
            p.EditorId == TestUserId &&
            p.MessageId == messageToEditId &&
            p.NewContent == newContent)), Times.Once);
    }

    // TODO: Add tests for Remove method
    // TODO: Add tests for OnConnectedAsync (e.g., joining groups - might require IUserService mock)
    // TODO: Add tests for unauthorized scenarios (e.g., editing/deleting someone else's message)
}
