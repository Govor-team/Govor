using Govor.Application.Exceptions.InvitesService;
using Govor.Application.Interfaces.Authentication;
using Govor.Core.Models;
using Govor.Core.Models.Users;
using Govor.Core.Repositories.Invaites;
using Govor.Data.Repositories.Exceptions;

namespace Govor.Application.Services;

public class InvitesService : IInvitesService
{
    private readonly IInvitesRepository _invitesRepository;

    public InvitesService(IInvitesRepository invitesRepository)
    {
        _invitesRepository = invitesRepository;
    }
    
    public async Task<string> GetRoleAsync(User user)
    {
        try
        {
            var invitation = await _invitesRepository.FindByIdAsync(user.InviteId);
            return invitation.IsAdmin ? "Admin" : "User";
        }
        catch (NotFoundByKeyException<Guid>)
        {
            return "User"; 
        }
    }

    public async Task<Invitation> ValidateAsync(string inviteCode)
    {
        try
        {
            var invite = await _invitesRepository.FindByCodeAsync(inviteCode);

            if (invite.EndDate < DateTime.Now || invite.MaxParticipants <= invite.Users.Count)
            {
                invite.IsActive = false;
                await _invitesRepository.UpdateAsync(invite);
                throw new InviteLinkInvalidException(inviteCode);
            }

            return invite;
        }
        catch (NotFoundByKeyException<string>)
        {
            throw new InviteLinkInvalidException(inviteCode);
        }
    }


    public string GenerateInvitationLink(Invitation invitation)
    {
        throw new NotImplementedException();
    }
}

