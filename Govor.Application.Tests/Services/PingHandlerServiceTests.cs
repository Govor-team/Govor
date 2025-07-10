using Govor.Application.Services;
using Govor.Core.Models;
using Govor.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Govor.Application.Tests.Services;

[TestFixture]
public class PingHandlerServiceTests
{
    private GovorDbContext _dbContext = null!;
    private IMemoryCache _memoryCache = null!;
    private PingHandlerService _service = null!;
    private Guid _userId;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<GovorDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new GovorDbContext(options);
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _service = new PingHandlerService(_dbContext, _memoryCache);

        _userId = Guid.NewGuid();
        _dbContext.Users.Add(new User
        {
            Id = _userId,
            Username = "TestUser",
            WasOnline = DateTime.UtcNow.AddHours(-1),
            Description = "Test description",           
            PasswordHash = "hashed_password_here"      
        });
        _dbContext.SaveChanges();
    }
    
    [Test]
    public async Task Ping_DoesNotUpdate_WhenPingTooRecent()
    {
        // Arrange
        var initial = DateTime.UtcNow.AddMinutes(-1);
        _memoryCache.Set($"LastPing_{_userId}", DateTime.UtcNow);

        var user = await _dbContext.Users.FirstAsync(u => u.Id == _userId);
        var originalTime = user.WasOnline;

        // Act
        await _service.Ping(_userId);

        var updatedUser = await _dbContext.Users.FirstAsync(u => u.Id == _userId);

        // Assert
        Assert.That(updatedUser.WasOnline, Is.EqualTo(originalTime));
    }
}
