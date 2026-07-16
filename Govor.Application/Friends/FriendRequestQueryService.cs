using Microsoft.EntityFrameworkCore;
using Govor.Domain;
using Govor.Domain.Models;

namespace Govor.Application.Friends;

public class FriendRequestQueryService : IFriendRequestQueryService
{
    private readonly GovorDbContext _context;


    public FriendRequestQueryService(GovorDbContext context)
    {
        _context = context;
    }

    public async Task<List<Friendship>> GetIncomingAsync(Guid userId)
    {
        return await _context.Friendships
            .AsNoTracking()
            .Include(f => f.Requester)
            .Include(f => f.Addressee)
            .Where(f => f.AddresseeId == userId && f.Status == FriendshipStatus.Pending)
            .ToListAsync();
    }

    public async Task<List<Friendship>> GetResponsesAsync(Guid userId)
    {
        return await _context.Friendships
            .AsNoTracking()
            .Include(f => f.Requester)
            .Include(f => f.Addressee)
            .Where(f => f.RequesterId == userId && f.Status != FriendshipStatus.Accepted)
            .ToListAsync();
    }
}