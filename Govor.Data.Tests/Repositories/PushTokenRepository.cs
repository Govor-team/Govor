using Govor.Core.Models.Users;
using Govor.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Govor.Data.Tests.Repositories;

[TestFixture]
public class PushTokenRepositoryTests
{
    private GovorDbContext _context;
    private PushTokenRepository _repository;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<GovorDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new GovorDbContext(options);
        _repository = new PushTokenRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    #region GetActiveTokensAsync

    [Test]
    public async Task GetActiveTokensAsync_ReturnsUserTokens()
    {
        var userId = Guid.NewGuid();

        _context.UserPushTokens.Add(new UserPushToken
        {
            UserId = userId,
            Token = "token1",
            IsActive = true
        });

        await _context.SaveChangesAsync();

        var result = await _repository.GetActiveTokensAsync(userId);

        Assert.That(result, Contains.Item("token1"));
    }

    #endregion

    #region GetActiveTokensUsersAsync

    [Test]
    public async Task GetActiveTokensUsersAsync_ReturnsDistinctActiveTokens()
    {
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();

        _context.UserPushTokens.AddRange(
            new UserPushToken { UserId = user1, Token = "t1", IsActive = true },
            new UserPushToken { UserId = user2, Token = "t2", IsActive = true },
            new UserPushToken { UserId = user2, Token = "inactive", IsActive = false }
        );

        await _context.SaveChangesAsync();

        var result = await _repository.GetActiveTokensUsersAsync([user1, user2, user1]);

        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result, Does.Not.Contain("inactive"));
    }

    [Test]
    public async Task GetActiveTokensUsersAsync_EmptyInput_ReturnsEmpty()
    {
        var result = await _repository.GetActiveTokensUsersAsync([]);

        Assert.That(result, Is.Empty);
    }

    #endregion

    #region GetActiveTokenBySessionAsync

    [Test]
    public async Task GetActiveTokenBySessionAsync_ReturnsToken()
    {
        var sessionId = Guid.NewGuid();

        _context.UserPushTokens.Add(new UserPushToken
        {
            UserSessionId = sessionId,
            Token = "sessionToken",
            IsActive = true
        });

        await _context.SaveChangesAsync();

        var result = await _repository.GetActiveTokenBySessionAsync(sessionId);

        Assert.That(result, Is.EqualTo("sessionToken"));
    }

    [Test]
    public async Task GetActiveTokenBySessionAsync_EmptySession_ReturnsEmptyString()
    {
        var result = await _repository.GetActiveTokenBySessionAsync(Guid.Empty);

        Assert.That(result, Is.EqualTo(""));
    }

    #endregion

    #region AddOrUpdateTokenAsync

    [Test]
    public void AddOrUpdateTokenAsync_InvalidUserId_Throws()
    {
        Assert.ThrowsAsync<ArgumentException>(() =>
            _repository.AddOrUpdateTokenAsync(Guid.Empty, null, "token", "android"));
    }

    [Test]
    public void AddOrUpdateTokenAsync_EmptyToken_Throws()
    {
        Assert.ThrowsAsync<ArgumentException>(() =>
            _repository.AddOrUpdateTokenAsync(Guid.NewGuid(), null, "", "android"));
    }

    [Test]
    public async Task AddOrUpdateTokenAsync_AddsNewToken()
    {
        var userId = Guid.NewGuid();

        await _repository.AddOrUpdateTokenAsync(userId, null, "newToken", "android");

        var token = await _context.UserPushTokens.FirstOrDefaultAsync();

        Assert.That(token, Is.Not.Null);
        Assert.That(token!.UserId, Is.EqualTo(userId));
        Assert.That(token.IsActive, Is.True);
    }

    [Test]
    public async Task AddOrUpdateTokenAsync_UpdatesExistingToken()
    {
        var userId = Guid.NewGuid();

        var existing = new UserPushToken
        {
            UserId = Guid.NewGuid(),
            Token = "sameToken",
            Platform = "ios",
            IsActive = false
        };

        _context.UserPushTokens.Add(existing);
        await _context.SaveChangesAsync();

        await _repository.AddOrUpdateTokenAsync(userId, null, "sameToken", "android");

        var updated = await _context.UserPushTokens.FirstAsync();

        Assert.That(updated.UserId, Is.EqualTo(userId));
        Assert.That(updated.Platform, Is.EqualTo("android"));
        Assert.That(updated.IsActive, Is.True);
    }

    #endregion

    #region RemoveTokensAsync

    [Test]
    public async Task RemoveTokensAsync_RemovesTokens()
    {
        _context.UserPushTokens.Add(new UserPushToken
        {
            Token = "t1",
            UserId = Guid.NewGuid()
        });

        await _context.SaveChangesAsync();

        await _repository.RemoveTokensAsync(["t1"]);

        Assert.That(_context.UserPushTokens.Count(), Is.EqualTo(0));
    }

    [Test]
    public async Task RemoveTokensAsync_EmptyInput_DoesNothing()
    {
        await _repository.RemoveTokensAsync([]);

        Assert.Pass();
    }

    #endregion

    #region DeactivateTokenBySessionAsync

    [Test]
    public async Task DeactivateTokenBySessionAsync_DeactivatesTokens()
    {
        var sessionId = Guid.NewGuid();

        _context.UserPushTokens.Add(new UserPushToken
        {
            UserSessionId = sessionId,
            Token = "t1",
            IsActive = true
        });

        await _context.SaveChangesAsync();

        await _repository.DeactivateTokenBySessionAsync(sessionId);

        var token = await _context.UserPushTokens.FirstAsync();

        Assert.That(token.IsActive, Is.False);
    }

    [Test]
    public async Task DeactivateTokenBySessionAsync_EmptySession_DoesNothing()
    {
        await _repository.DeactivateTokenBySessionAsync(Guid.Empty);

        Assert.Pass();
    }

    #endregion
}