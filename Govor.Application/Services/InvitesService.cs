using Govor.API.Services.Authentication.Interfaces;
using Govor.Application.Exceptions.InvitesService;
using Govor.Core.Models;
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
    
    public async Task<string> GetRole(User user)
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

    public Invitation Validate(string inviteCode)
    {
        var invite = _invitesRepository.FindByCodeAsync(inviteCode).Result;

        if (invite.EndDate < DateTime.Now ||
            invite.MaxParticipants <= invite.Users.Count)
        {
            invite.IsActive = false;
            _invitesRepository.UpdateAsync(invite);
            throw new InviteLinkInvalidException(inviteCode);
        }

        return invite;
    }

    public string GenerateInvitationLink(Invitation invitation)
    {
        throw new NotImplementedException();
    }
}

