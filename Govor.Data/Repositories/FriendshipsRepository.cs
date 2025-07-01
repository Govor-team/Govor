using Govor.Core.Infrastructure.Extensions;
using Govor.Core.Infrastructure.Validators;
using Govor.Core.Models;
using Govor.Core.Repositories.Friendships;
using Govor.Data.Repositories.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Govor.Data.Repositories;

public class FriendshipsRepository : IFriendshipsRepository
{
    private GovorDbContext _context;
    private IObjectValidator<Friendship> _validator;
    
    public FriendshipsRepository(GovorDbContext context, IObjectValidator<Friendship> validator)
    {
        _context = context;    
        _validator = validator;
    }
    
    public async Task<List<Friendship>> GetAllAsync()
    {
        return await _context.Friendships
            .AsNoTracking()
            .Include(x => x.Requester)
            .Include(x => x.Addressee)
            .AsSplitQuery()
            .ToListOrThrowIfEmpty(new NotFoundException("Database is empty"));
    }

    public async Task<Friendship> GetByIdAsync(Guid id)
    {
        return await _context.Friendships
            .AsNoTracking()
            .Include(x => x.Requester)
            .Include(x => x.Addressee)
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundByKeyException<Guid>(id, "Friendship with given id was not found");
    }

    public async Task<List<Friendship>> FindByUserIdAsync(Guid userId)
    {
        return await _context.Friendships
            .AsNoTracking()
            .Include(x => x.Requester)
            .Include(x => x.Addressee)
            .AsSplitQuery()
            .Where(x => x.RequesterId == userId)
            .ToListOrThrowIfEmpty(new NotFoundByKeyException<Guid>(userId, "Friendship with given user id was not found"));
    }

    public async Task AddAsync(Friendship friendship)
    {
        try
        {
            _validator.Validate(friendship);

            _context.Friendships.Add(friendship);
            await _context.SaveChangesAsync();
        }
        catch (InvalidObjectException<Admin> ex)
        {
            throw new AdditionException("Admin with given data invalid", ex);
        }
        catch (Exception ex)
        {
            throw new AdditionException("Cannot add admin", ex);
        }
    }

    public async Task UpdateAsync(Friendship friendship)
    {
        try
        {
            _validator.Validate(friendship);

            var rowsAffected = await _context.Friendships
                .Where(a => a.Id == friendship.Id)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(a => a.RequesterId, friendship.RequesterId)
                    .SetProperty(a => a.AddresseeId, friendship.AddresseeId)
                    .SetProperty(a => a.Status, friendship.Status)
                );

            if (rowsAffected == 0)
                throw new UpdateException($"Not found friendship by given id {friendship.Id}");
        }
        catch (Exception ex)
        {
            throw new UpdateException($"Error when updating the friendship {friendship.Id}", ex);
        }
    }

    public async Task RemoveAsync(Friendship friendship)
    {
        try
        {
            var result = await GetByIdAsync(friendship.Id);

            _context.Friendships.Remove(result);
            await _context.SaveChangesAsync();
        }
        catch (NotFoundByKeyException<Guid> ex)
        {
            throw new RemoveException($"Not found friendship by given id {friendship.Id}", ex);
        }
        catch (Exception ex)
        {
            throw new RemoveException("Error when removing the friendship", ex);
        }
    }

    public bool Exist(Guid requesterId, Guid addresseeId)
    {
        return _context.Friendships.AsNoTracking().Any(x => (x.RequesterId == requesterId && x.AddresseeId == addresseeId));
    }
}