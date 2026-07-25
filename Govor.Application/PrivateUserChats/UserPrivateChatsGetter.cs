using Govor.Domain;
using Govor.Domain.Common;
using Govor.Domain.Models;
using Microsoft.EntityFrameworkCore;
using SmartRes;

namespace Govor.Application.PrivateUserChats;

public class UserPrivateChatsGetter : IUserPrivateChatsGetterService
{
    private readonly GovorDbContext _context;

    public UserPrivateChatsGetter(GovorDbContext context)
    {
        _context = context;
    }

    public async Task<List<PrivateChat>> GetUserChatsAsync(Guid userId)
    {
        return await _context.PrivateChats
            .AsNoTracking()
            .Where(p => p.UserAId == userId || p.UserBId == userId)
            .ToListAsync();
    }

    public async Task<Result<PrivateChat, Error>> GetPrivateChatAsync(Guid chatId)
    {
        var res = await _context.PrivateChats.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == chatId);
        
        if (res == null)
            return Result.Failure<PrivateChat>( 
                Error.Failure(
                    nameof(InvalidOperationException),
                    "PrivateChat not found.")
            );
        
        return res;
    }

    public async Task<bool> ExistChatAsync(Guid userIdA, Guid userIdB)
    {
        return await _context.PrivateChats
            .AsNoTracking()
            .AnyAsync(p => (p.UserAId == userIdA && p.UserBId == userIdB) || 
                           (p.UserBId == userIdA && p.UserAId == userIdB)
                           );
    }
}