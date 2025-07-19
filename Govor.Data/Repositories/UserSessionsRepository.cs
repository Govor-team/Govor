using Govor.Core.Infrastructure.Extensions;
using Govor.Core.Models;
using Govor.Core.Repositories.UserSessionsRepository;
using Govor.Data.Repositories.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Govor.Data.Repositories;

public class UserSessionsRepository : IUserSessionsRepository
{
    private GovorDbContext _context;

    public UserSessionsRepository(GovorDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<UserSession>> GetAllAsync()
    {
        return await _context.UserSessions
            .AsNoTracking()
            .ToListOrThrowIfEmpty(new NotFoundException("Database is empty"));
    }

    public async Task<UserSession> GetByIdAsync(Guid sessionId)
    {
        return await _context.UserSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(session => session.Id == sessionId)
                ?? throw new NotFoundByKeyException<Guid>(sessionId, "Session with given id does not exist");
    }

    public async Task<List<UserSession>> GetByUserIdAsync(Guid userId)
    {
        return await _context.UserSessions
            .AsNoTracking()
            .Where(session => session.UserId == userId)
            .ToListOrThrowIfEmpty(new NotFoundByKeyException<Guid>(userId, "Sessions with given user id does not exist"));
    }

    public async Task<List<UserSession>> GetByCreatedAtAsync(DateTime createdAt)
    {
        return await _context.UserSessions
            .AsNoTracking()
            .Where(session => session.CreatedAt == createdAt)
            .ToListOrThrowIfEmpty(new NotFoundByKeyException<DateTime>(createdAt, "Sessions with given created at does not exist"));
    }

    public async Task<List<UserSession>> GetByExpiresAtAsync(DateTime expiresAt)
    {
        return await _context.UserSessions
            .AsNoTracking()
            .Where(session => session.ExpiresAt == expiresAt)
            .ToListOrThrowIfEmpty(new NotFoundByKeyException<DateTime>(expiresAt, "Sessions with given expires at does not exist"));
    }

    public async Task<List<UserSession>> GetByRevokedAsync(bool isRevoked)
    {
        return await _context.UserSessions
            .AsNoTracking()
            .Where(session => session.IsRevoked == isRevoked)
            .ToListOrThrowIfEmpty(new NotFoundByKeyException<bool>(isRevoked, "Sessions is revoked does not exist"));
    }

    public async Task<UserSession> GetByRefreshTokenAsync(string refreshToken)
    {
        return await _context.UserSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(session => session.RefreshToken == refreshToken)
                ?? throw new NotFoundByKeyException<string>(refreshToken, "Session with given refresh token does not exist");
    }

    public async Task AddAsync(UserSession userSession)
    {
        try
        {
            _context.UserSessions.Add(userSession);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new AdditionException("Failed to add user session", ex);
        }
    }

    public async Task UpdateAsync(UserSession userSession)
    {
        try
        {
            //_validator.Validate(user);

            var rowsAffected = await _context.UserSessions
                .Where(u => u.Id == userSession.Id)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(a => a.UserId, userSession.UserId)
                    .SetProperty(u => u.RefreshToken, userSession.RefreshToken)
                    .SetProperty(u => u.DeviceInfo, userSession.DeviceInfo)
                    .SetProperty(u => u.CreatedAt, userSession.CreatedAt)
                    .SetProperty(u => u.ExpiresAt, userSession.ExpiresAt)
                    .SetProperty(u => u.IsRevoked, userSession.IsRevoked)
                );

            if (rowsAffected == 0)
                throw new UpdateException($"Not found user session by given id {userSession.Id}");
        }
        catch (Exception ex)
        {
            throw new UpdateException($"Error when updating the user session {userSession.Id}", ex);
        }
    }

    public async Task RemoveAsync(Guid sessionId)
    {
        try
        {
            var result = await GetByIdAsync(sessionId);

            _context.UserSessions.Remove(result);
            await _context.SaveChangesAsync();
        }
        catch (NotFoundByKeyException<Guid> ex)
        {
            throw new RemoveException($"Not found session by given id {sessionId}", ex);
        }
        catch (Exception ex)
        {
            throw new RemoveException("Error when removing the session", ex);
        }
    }

    public async Task RemoveByUserIdAsync(Guid userId)
    {
        try
        {
            var result = await GetByUserIdAsync(userId);

            _context.UserSessions.RemoveRange(result);
            await _context.SaveChangesAsync();
        }
        catch (NotFoundByKeyException<Guid> ex)
        {
            throw new RemoveException($"Not found user sessions by given user id {userId}", ex);
        }
        catch (Exception ex)
        {
            throw new RemoveException("Error when removing the user sessions", ex);
        }
    }

    public bool Exist(Guid sessionId)
    {
        return _context.UserSessions.Any(session => session.Id == sessionId);
    }

    public bool Exist(string refresh)
    {
        return _context.UserSessions.Any(session => session.RefreshToken == refresh);
    }

    public bool Exist(UserSession userSession)
    {
        return _context.UserSessions.Any(session => session.Id == userSession.Id &&
                                                    session.RefreshToken == userSession.RefreshToken &&
                                                    session.IsRevoked == userSession.IsRevoked &&
                                                    session.CreatedAt == userSession.CreatedAt &&
                                                    session.ExpiresAt == userSession.ExpiresAt &&
                                                    session.DeviceInfo == userSession.DeviceInfo &&
                                                    session.UserId == userSession.UserId);
    }
}