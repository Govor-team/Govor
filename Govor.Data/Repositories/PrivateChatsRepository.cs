using Govor.Core.Infrastructure.Extensions;
using Govor.Core.Infrastructure.Validators;
using Govor.Core.Models;
using Govor.Core.Repositories.PrivateChats;
using Govor.Data.Repositories.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Govor.Data.Repositories;

public class PrivateChatsRepository : IPrivateChatsRepository
{
    private GovorDbContext _context;
    private IObjectValidator<PrivateChat> _validator;

    public PrivateChatsRepository(GovorDbContext context, IObjectValidator<PrivateChat> validator)
    {
        _context = context;
        _validator = validator;
    }
    
    public async Task<List<PrivateChat>> GetAllAsync()
    {
        return await _context.PrivateChats
            .AsNoTracking()
            .ToListOrThrowIfEmpty(new NotFoundException("Database is empty"));
    }

    public async Task<PrivateChat> GetByIdAsync(Guid id)
    {
        return await _context.PrivateChats
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundByKeyException<Guid>(id, "Private Chat with given Id does not exist");
    }

    public async Task<PrivateChat> GetByMembersAsync(Guid memberAId, Guid memberBId)
    {
        return await _context.PrivateChats
            .AsNoTracking()
            .FirstOrDefaultAsync(f => (f.UserAId == memberAId && f.UserBId == memberBId) || (f.UserAId == memberBId && f.UserBId == memberAId))
            ?? throw new NotFoundByKeyException<(Guid, Guid)>((memberAId, memberBId), "Private Chat with given members Id's does not exist");
    }

    public async Task AddAsync(PrivateChat chat)
    {
        try
        {
            _validator.Validate(chat);

            _context.PrivateChats.Add(chat);
            await _context.SaveChangesAsync();
        }
        catch (InvalidObjectException<PrivateChat> ex)
        {
            throw new AdditionException("Private chat with given data invalid", ex);
        }
        catch (Exception ex)
        {
            throw new AdditionException("Cannot add private chat", ex);
        }
    }

    public async Task UpdateAsync(PrivateChat chat)
    {
        try
        {
            _validator.Validate(chat);

            var rowsAffected = await _context.PrivateChats
                .Where(m => m.Id == chat.Id)
                .ExecuteUpdateAsync(c => c
                    .SetProperty(c => c.UserAId, chat.UserAId)
                    .SetProperty(c => c.UserBId, chat.UserBId)
                    .SetProperty(c => c.Messages, chat.Messages)
                );

            if (rowsAffected == 0)
                throw new UpdateException($"Not found private chat by given id {chat.Id}");
        }
        catch (Exception ex)
        {
            throw new UpdateException($"Error when updating the private chat {chat.Id}", ex);
        }
    }

    public async Task RemoveAsync(Guid chatId)
    {
        try
        {
            var result = await GetByIdAsync(chatId);

            _context.PrivateChats.Remove(result);
            await _context.SaveChangesAsync();
        }
        catch (NotFoundByKeyException<Guid> ex)
        {
            throw new RemoveException($"Not found private chat by given id {chatId}", ex);
        }
        catch (Exception ex)
        {
            throw new RemoveException("Error when removing the private chat", ex);
        }
    }

    public bool Exist(Guid chatId)
    {
        return _context.PrivateChats.Any(c => c.Id == chatId);
    }

    public bool Exist(Guid userAId, Guid userBId)
    {
        return _context.PrivateChats.Any(f => (f.UserAId == userAId && f.UserBId == userBId) 
                                              || (f.UserAId == userBId && f.UserBId == userAId));
    }
}