using Govor.Core.Infrastructure.Extensions;
using Govor.Core.Infrastructure.Validators;
using Govor.Core.Models;
using Govor.Core.Models.Users;
using Govor.Core.Repositories.Invaites;
using Govor.Data.Repositories.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Govor.Data.Repositories;

public class InvitesRepository : IInvitesRepository
{
    private GovorDbContext _context;
    private IObjectValidator<Invitation> _validator;

    public InvitesRepository(GovorDbContext context, IObjectValidator<Invitation> validator)
    {
        _context = context;
        _validator = validator;
    }
    
    public async Task<List<Invitation>> GetAllAsync()
    {
        return await _context.Invitations
            .AsNoTracking()
            .Include(i => i.Users)
            .AsSplitQuery()
            .ToListOrThrowIfEmpty(new NotFoundException("Database is empty"));
    }

    public async Task<Invitation> FindByIdAsync(Guid id)
    {
        return await _context.Invitations
            .AsNoTracking()
            .Include(i => i.Users)
            .AsSplitQuery()
            .FirstOrDefaultAsync(i => i.Id == id)
            ?? throw new NotFoundByKeyException<Guid>(id, "Invitation with given id does not exist");
    }

    public async Task<Invitation> FindByCodeAsync(string code)
    {
        return await _context.Invitations
            .AsNoTracking()
            .Include(i => i.Users)
            .AsSplitQuery()
            .FirstOrDefaultAsync(i => i.Code == code)
            ?? throw new NotFoundByKeyException<string>(code, "Invitation with given code does not exist");
    }

    public async Task<List<Invitation>> FindAdminsInvitesAsync()
    {
        return await _context.Invitations
            .AsNoTracking()
            .Include(i => i.Users)
            .AsSplitQuery()
            .Where(i => i.IsAdmin)
            .ToListOrThrowIfEmpty(new NotFoundByKeyException<bool>(true, "Admins invites do not exist"));
    }

    public async Task AddAsync(Invitation invitation)
    {
        try
        {
            _validator.Validate(invitation);

            if(Exist(invitation.Code))
                throw new AdditionException("Invitation with given code already exists");
                
            _context.Invitations.Add(invitation);
            await _context.SaveChangesAsync();
        }
        catch (InvalidObjectException<Admin> ex)
        {
            throw new AdditionException("Invitation with given data invalid", ex);
        }
        catch (Exception ex)
        {
            throw new AdditionException("Cannot add invitation", ex);
        };
    }

    public async Task UpdateAsync(Invitation invitation)
    {
        try
        {
            _validator.Validate(invitation);

            var rowsAffected = await _context.Invitations
                .Where(a => a.Id == invitation.Id)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(i => i.IsAdmin, invitation.IsAdmin)
                    .SetProperty(i => i.Code, invitation.Code)
                    .SetProperty(i => i.Description, invitation.Description)
                    .SetProperty(i => i.DateCreated, invitation.DateCreated)
                    .SetProperty(i => i.EndDate, invitation.EndDate)
                    .SetProperty(i => i.MaxParticipants, invitation.MaxParticipants)
                    .SetProperty(i => i.Users, invitation.Users)
                );

            if (rowsAffected == 0)
                throw new UpdateException($"Not found invitation by given id {invitation.Id}");

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new UpdateException($"Error when updating the invitation {invitation.Id}", ex);
        }
    }

    public async Task RemoveAsync(Invitation invitation)
    {
        try
        {
            var result = await FindByIdAsync(invitation.Id);

            _context.Invitations.Remove(result);
            await _context.SaveChangesAsync();
        }
        catch (NotFoundByKeyException<Guid> ex)
        {
            throw new RemoveException($"Not found invitation by given id {invitation.Id}", ex);
        }
        catch (Exception ex)
        {
            throw new RemoveException("Error when removing the invitation", ex);
        }
    }

    public bool Exist(Invitation invitation)
    {
        _validator.Validate(invitation);
        return _context.Invitations.Any(i => i.Id == invitation.Id && 
                                             i.Code == invitation.Code &&
                                             i.IsAdmin == invitation.IsAdmin &&
                                             i.DateCreated == invitation.DateCreated &&
                                             i.EndDate == invitation.EndDate &&
                                             i.Description == invitation.Description &&
                                             i.MaxParticipants == invitation.MaxParticipants
            
        );
    }

    public bool Exist(string code)
    {
        return _context.Invitations.Any(i => i.Code == code);
    }
    
    public bool Exist(Guid guid)
    {
        return _context.Invitations.Any(i => i.Id == guid);
    }
}