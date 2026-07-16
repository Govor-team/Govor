using Govor.Application.Exceptions.InvitesService;
using Govor.Domain;
using Govor.Domain.Common;
using Govor.Domain.Models;
using Govor.Domain.Models.Users;
using Microsoft.EntityFrameworkCore;

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
        var invitation = await _context.Invitations.FirstOrDefaultAsync(s => s.Id == sessionId);
        
        if (invitation == null)
            return "User";
        
        return invitation.IsAdmin ? "Admin" : "User";
    }

    public async Task<Result<Invitation>> ValidateAsync(string inviteCode)
    {
        var invite = await _context.Invitations
            .Include(s => s.Users)
            .FirstOrDefaultAsync(s => s.Code == inviteCode);

        if (invite == null)
            return Result<Invitation>.Failure(Error.Null);

        if (invite.EndDate < DateTime.Now || invite.MaxParticipants <= invite.Users.Count)
        {
            invite.IsActive = false;
            await _context.SaveChangesAsync();

            return Result<Invitation>.Failure(new Error(
                "Auth.InviteLinkInvalid", $"Invite link invalid: {inviteCode}")
            );
        }

        return invite;
    }
}

