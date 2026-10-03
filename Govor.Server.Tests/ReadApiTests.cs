using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Govor.API.Hubs.Infrastructure;
using Govor.API.Hubs;
using Govor.Application.Storage;
using Govor.Contracts.Requests;
using Govor.Contracts.Responses;
using Govor.Domain.Models.Reactions;
using Microsoft.AspNetCore.SignalR;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Govor.Contracts.Responses.SignalR;
using Govor.Domain;
using Govor.Domain.Models.Users;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace Govor.Server.Tests;

[TestFixture]
[NonParallelizable]
public class ReadApiTests
{
    private const string Secret = "Govor-server-tests-only-signing-key-at-least-64-characters-long-123456";
    private TestDatabase _db = null!;
    private TestFactory _factory = null!;
    private HttpClient _client = null!;
    private Mock<IChatNotificationService> _notifier = null!;
    private Guid _session;

    [SetUp]
    public void SetUp()
    {
        _db = new TestDatabase();
        var session = new UserSession { UserId = _db.Bob, ExpiresAt = DateTime.UtcNow.AddDays(1) };
        _db.Context.UserSessions.Add(session);
        _db.Context.SaveChanges();
        _session = session.Id;
        _notifier = new Mock<IChatNotificationService>();
        _notifier.Setup(n => n.NotifyChatReadAsync(It.IsAny<ChatReadResponse>())).Returns(Task.CompletedTask);
        _notifier.Setup(n => n.NotifyMessageReactionsChangedAsync(It.IsAny<MessageReactionsChangedResponse>())).Returns(Task.CompletedTask);
        _notifier.Setup(n => n.NotifyChannelReactionPolicyChangedAsync(It.IsAny<ChannelReactionPolicyResponse>())).Returns(Task.CompletedTask);
        _factory = new TestFactory(_db, _notifier.Object);
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
        _db.Dispose();
    }

    [Test]
    public async Task ReadEndpointPersistsReceiptsNotifiesAndReturnsUnreadCount()
    {
        var message = _db.AddMessage();
        Authorize();
        var response = await _client.PostAsJsonAsync($"/api/chats/{_db.ChatId}/read",
            new { recipientType = 0, messageIds = new[] { message.Id } });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var body = await response.Content.ReadFromJsonAsync<ChatReadResponse>();
        Assert.That(body!.ReadCount, Is.EqualTo(1));
        Assert.That(body.UnreadCount, Is.Zero);
        _notifier.Verify(n => n.NotifyChatReadAsync(It.Is<ChatReadResponse>(r => r.ReaderId == _db.Bob && r.ChatId == _db.ChatId)), Times.Once);
        var count = await _client.GetAsync($"/api/chats/{_db.ChatId}/unread-count?recipientType=0");
        Assert.That(count.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(await count.Content.ReadAsStringAsync(), Does.Contain("\"unreadCount\":0"));
    }

    [Test]
    public async Task AnonymousAndRefreshTokenCannotUseReadApi()
    {
        var path = $"/api/chats/{_db.ChatId}/unread-count?recipientType=0";
        Assert.That((await _client.GetAsync(path)).StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        Authorize(tokenType: "refresh");
        Assert.That((await _client.GetAsync(path)).StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task SessionRevocationInvalidatesAnAlreadyIssuedAccessToken()
    {
        Authorize();
        await _db.Context.UserSessions.Where(s => s.Id == _session)
            .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.IsRevoked, true));
        var response = await _client.GetAsync($"/api/chats/{_db.ChatId}/unread-count?recipientType=0");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ForeignChatIsForbiddenAndInvalidSelectionIsBadRequest()
    {
        var message = _db.AddMessage();
        Authorize();
        var foreign = await _client.PostAsJsonAsync($"/api/chats/{Guid.NewGuid()}/read", new { recipientType = 0 });
        Assert.That(foreign.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        var invalid = await _client.PostAsJsonAsync($"/api/chats/{_db.ChatId}/read",
            new { recipientType = 0, messageIds = new[] { message.Id }, upToMessageId = message.Id });
        Assert.That(invalid.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(await _db.Context.MessageViews.CountAsync(), Is.Zero);
    }

    [Test]
    public async Task OrdinaryUserCannotReadAdminUserList()
    {
        Authorize();
        var response = await _client.GetAsync("/api/admin/Users/all");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task ReactionHttpApiReturnsVersionedCountsAndNotifiesOnlyOnChange()
    {
        var message = _db.AddMessage();
        Authorize();
        var packs = await _client.GetFromJsonAsync<List<ReactionPackResponse>>("/api/reaction-packs/mine");
        var reaction = packs!.Single(p => p.IsDefault).Reactions.First().Id;
        var path = $"/api/messages/{message.Id}/reactions";
        var response = await _client.PutAsJsonAsync(path + "/me", new { reactionId = reaction });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var state = await response.Content.ReadFromJsonAsync<MessageReactionsChangedResponse>();
        Assert.That(state!.Version, Is.EqualTo(1));
        Assert.That(state.ActorReaction!.ReactionId, Is.EqualTo(reaction));
        Assert.That(state.Counts.Single().Count, Is.EqualTo(1));
        Assert.That((await _client.PutAsJsonAsync(path + "/me", new { reactionId = reaction })).StatusCode, Is.EqualTo(HttpStatusCode.OK));
        _notifier.Verify(n => n.NotifyMessageReactionsChangedAsync(It.IsAny<MessageReactionsChangedResponse>()), Times.Once);
        Assert.That((await _client.PutAsJsonAsync(path + "/me", new { reactionId = Guid.Empty })).StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var removed = await _client.DeleteAsync(path + "/me");
        Assert.That(removed.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var empty = await _client.GetFromJsonAsync<MessageReactionsChangedResponse>(path);
        Assert.That(empty!.Version, Is.EqualTo(2));
        Assert.That(empty.Counts, Is.Empty);
    }

    [Test]
    public async Task AdminPackUploadUsesRealContentAndCanBeSharedAndDownloaded()
    {
        Authorize();
        Assert.That((await _client.PostAsJsonAsync("/api/admin/reaction-packs", new { name = "Custom" })).StatusCode,
            Is.EqualTo(HttpStatusCode.Forbidden));
        Authorize(role: "Admin");
        Assert.That((await _client.PostAsJsonAsync("/api/admin/reaction-packs", new { name = "Custom" })).StatusCode,
            Is.EqualTo(HttpStatusCode.Forbidden));
        ReactionTests.MakeAdmin(_db.Context, _db.Bob);
        var created = await _client.PostAsJsonAsync("/api/admin/reaction-packs", new { name = "Custom", description = "Shared" });
        Assert.That(created.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var pack = (await created.Content.ReadFromJsonAsync<ReactionPackResponse>())!;
        using var image = new Image<Rgba32>(256, 256);
        using var bytes = new MemoryStream();
        await image.SaveAsPngAsync(bytes);
        using var multipart = new MultipartFormDataContent();
        multipart.Add(new StringContent("Picture"), "name");
        var upload = new ByteArrayContent(bytes.ToArray());
        upload.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        multipart.Add(upload, "file", "incorrect.txt");
        var response = await _client.PostAsync($"/api/admin/reaction-packs/{pack.Id}/reactions/media", multipart);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var item = (await response.Content.ReadFromJsonAsync<ReactionItemResponse>())!;
        Assert.That(item.Kind, Is.EqualTo(ReactionKind.Image));
        Assert.That(item.Width, Is.EqualTo(128));
        Authorize();
        var shared = await _client.GetFromJsonAsync<ReactionPackResponse>(pack.SharePath);
        Assert.That(shared!.Reactions.Single().Id, Is.EqualTo(item.Id));
        Assert.That((await _client.PutAsync($"/api/reaction-packs/{pack.Id}/subscription", null)).StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        var downloaded = await _client.GetAsync(item.MediaUrl);
        Assert.That(downloaded.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(downloaded.Content.Headers.ContentType!.MediaType, Is.EqualTo("image/png"));
        using var decoded = Image.Load(await downloaded.Content.ReadAsByteArrayAsync());
        Assert.That(decoded.Width, Is.EqualTo(128));
    }

    [Test]
    public async Task SignalRReactionMethodsUseTheSameStateAndAccessChecks()
    {
        var message = _db.AddMessage();
        using var scope = _factory.Services.CreateScope();
        var hub = ActivatorUtilities.CreateInstance<ChatsHub>(scope.ServiceProvider);
        var caller = new Mock<HubCallerContext>();
        caller.SetupGet(c => c.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[]
        { new Claim("userId", _db.Bob.ToString()), new Claim("sid", _session.ToString()) }, "test")));
        hub.Context = caller.Object;
        var reaction = DefaultReactionPack.Items().First().Id;
        var result = await hub.React(message.Id, new SetReactionRequest { ReactionId = reaction });
        Assert.That(result.Status, Is.EqualTo(HubResultStatus.Success));
        Assert.That(result.Result!.Counts.Single().Count, Is.EqualTo(1));
        Assert.That((await hub.RemoveReaction(message.Id)).Status, Is.EqualTo(HubResultStatus.Success));
        Assert.That((await hub.React(Guid.NewGuid(), new SetReactionRequest { ReactionId = reaction })).Status, Is.EqualTo(HubResultStatus.NotFound));
    }

    private void Authorize(string tokenType = "access", string role = "User")
    {
        var token = new JwtSecurityToken(claims: new[]
        {
            new Claim("userId", _db.Bob.ToString()), new Claim("sid", _session.ToString()),
            new Claim("tokenType", tokenType), new Claim(ClaimTypes.Role, role)
        }, expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret)), SecurityAlgorithms.HmacSha256));
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", new JwtSecurityTokenHandler().WriteToken(token));
    }

    private sealed class TestFactory(TestDatabase db, IChatNotificationService notifier) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.UseSetting("Firebase:Enabled", "false");
            builder.UseSetting("JwtAccessOption:SecretKey", Secret);
            builder.ConfigureServices(services =>
            {
                services.AddDataProtection().UseEphemeralDataProtectionProvider();
                var worker = services.Single(d => d.ServiceType == typeof(IHostedService) && d.ImplementationType == typeof(ChatPushWorker));
                services.Remove(worker);
                services.RemoveAll<DbContextOptions<GovorDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<GovorDbContext>>();
                services.AddDbContext<GovorDbContext>(db.ConfigureOptions);
                services.RemoveAll<IChatNotificationService>();
                services.AddSingleton(notifier);
                services.RemoveAll<IStorageService>();
                services.AddSingleton<IStorageService>(new TestReactionStorage());
            });
        }
    }
}
