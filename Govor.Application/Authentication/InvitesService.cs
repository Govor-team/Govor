using Govor.Domain;
using Govor.Domain.Common;
using Govor.Domain.Models;
using Govor.Domain.Models.Users;
using Microsoft.EntityFrameworkCore;
using SmartRes;

namespace Govor.Application.Authentication;

public class InvitesService : IInvitesService
{
    private readonly GovorDbContext _context;

    public InvitesService(GovorDbContext context)
    {
        _context = context;
    }
    
    public async Task<string> GetRoleNameAsync(User user)
    {
        return await GetRoleNameAsync(user.InviteId);
    }

    public async Task<string> GetRoleNameAsync(Guid sessionId)
    {
        var invitation = await _context.Invitations.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sessionId);
        
        if (invitation == null)
            return "User";
        
        return invitation.IsAdmin ? "Admin" : "User";
    }

    public async Task<Result<Invitation, Error>> ValidateAsync(string inviteCode)
    {
        var invite = await _context.Invitations
            .Include(s => s.Users)
            .FirstOrDefaultAsync(s => s.Code == inviteCode);

        if (invite == null)
            return Result.Failure<Invitation>(Error.NotFound("Auth.LinkNotFount","Invitation not found."));

        if (invite.EndDate < DateTime.Now || invite.MaxParticipants <= invite.Users.Count || invite.MaxParticipants <= invite.Participants)
        {
            invite.IsActive = false;
            await _context.SaveChangesAsync();

            return Result.Failure<Invitation>(
                Error.Failure(
                "Auth.InviteLinkInvalid", $"Invite link invalid: {inviteCode}"
                )
            );
        }

        return invite;
    }
}

