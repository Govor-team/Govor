using Govor.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Govor.Application.PingHandler;

public class PingHandlerService : IPingHandlerService
{
    private readonly GovorDbContext _context;
    private readonly IMemoryCache _cache;

    public PingHandlerService(GovorDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task Ping(Guid userId)
    {
        var cacheKey = $"LastPing_{userId}";
        if (_cache.TryGetValue(cacheKey, out DateTime lastPing) && 
            DateTime.UtcNow.Subtract(lastPing).TotalSeconds < 30)
        {
            return; // Пропускаем слишком частые пинги
        }

        await _context.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(u => u.SetProperty(x => x.WasOnline, DateTime.UtcNow));

        _cache.Set(cacheKey, DateTime.UtcNow, TimeSpan.FromSeconds(30));
    }
}