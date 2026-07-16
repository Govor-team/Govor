using Govor.Application.Exceptions.FriendsService;
using Govor.Application.Interfaces;
using Govor.Application.PrivateUserChats;
using Govor.Domain;
using Govor.Domain.Common;
using Govor.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Govor.Application.Friends;

public class FriendRequestCommandService : IFriendRequestCommandService
{
    private readonly GovorDbContext _context;
    private readonly IUserPrivateChatsCreator _privateChatsCreator;

    public FriendRequestCommandService(
        GovorDbContext context,
        IUserPrivateChatsCreator privateChatsCreator)
    {
        _context = context;
        _privateChatsCreator = privateChatsCreator;
    }
    
    public async Task<Result<Friendship>> SendAsync(Guid fromUserId, Guid toUserId)
    {
        if (fromUserId == toUserId)
           return Result<Friendship>.Failure(new Error(
               "Friendship.Send", 
               "Cannot send a request to self user")
           );
        
        var friendship = await _context.Friendships
            .FirstOrDefaultAsync(f => (f.RequesterId == fromUserId && f.AddresseeId == toUserId) || 
                                      (f.RequesterId == toUserId && f.AddresseeId == fromUserId));

        if (friendship is null)
        {
            friendship = new Friendship
            {
                Id = Guid.NewGuid(),
                RequesterId = fromUserId,
                AddresseeId = toUserId,
                Status = FriendshipStatus.Pending
            };
            await _context.Friendships.AddAsync(friendship);
        }
        else
        {
            if (friendship.Status == FriendshipStatus.Pending ||
                friendship.Status == FriendshipStatus.Accepted ||
                friendship.Status == FriendshipStatus.Blocked)
            {
                return Result<Friendship>.Failure(new Error(
                    "Friendship.Send", 
                    $"The request is already {friendship.Status}")
                );
            }
            
            friendship.RequesterId = fromUserId;
            friendship.AddresseeId = toUserId;
            friendship.Status = FriendshipStatus.Pending;
        }

        await _context.SaveChangesAsync();
        return friendship;
    }

    public async Task<Result<Friendship>> AcceptAsync(Guid requestId, Guid currentUserId)
    {
        var friendship = await _context.Friendships.FindAsync(requestId);
        
        if (friendship is null)
            return Result<Friendship>.Failure(new Error(
                "Friendship.Accept",
                "Friendship not found! You cant accept request!")
            );
            
        if (friendship.AddresseeId != currentUserId)
            return Result<Friendship>.Failure(new Error(
                "Friendship.Accept",
                "You cannot accept this request!")
            );

        if (friendship.Status != FriendshipStatus.Pending)
            return Result<Friendship>.Failure(new Error(
                "Friendship.Accept",
                "Request is already accepted!")
            );

        friendship.Status = FriendshipStatus.Accepted;
        
        await _context.SaveChangesAsync();
        
        await _privateChatsCreator.CreateAsync(friendship.AddresseeId, friendship.RequesterId);
    
        return friendship;
    }

    public async Task<Result<Friendship>> RejectAsync(Guid requestId, Guid currentUserId)
    {
        var friendship = await _context.Friendships.FindAsync(requestId);

        if (friendship == null)
            return Result<Friendship>.Failure(new Error(
                "Friendship.Reject",
                "Friendship not found! You cant reject request!")
            );

        if (friendship.AddresseeId != currentUserId)
            return Result<Friendship>.Failure(new Error(
                "Friendship.Reject",
                "You cannot reject this request!")
            );

        if (friendship.Status != FriendshipStatus.Pending && friendship.Status != FriendshipStatus.Rejected)
            return Result<Friendship>.Failure(new Error(
                "Friendship.Reject",
                $"Request is already {friendship.Status}")
            );

        friendship.Status = FriendshipStatus.Rejected;
        
        await _context.SaveChangesAsync();
        return friendship;
    }
}