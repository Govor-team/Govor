using Govor.API.Services.AdminsStuff.Interfaces;
using Govor.Core.Models;
using Govor.Core.Repositories.Invaites;

namespace Govor.Application.Infrastructure.AdminsStuff;

public class InvitationGenerator(IInvitesRepository repository) : IInvitationGenerator
{
    public async Task<string> GenerateInvitationCode(DateTime time, int maxUsers, bool isAdmin, string description = "")
    {
        Invitation newInvitation = new Invitation()
        {
            Id = Guid.NewGuid(),
            Description = description,
            MaxParticipants = maxUsers,
            DateCreated = DateTime.UtcNow,
            EndDate = time.ToUniversalTime(),
            Code = Guid.NewGuid().ToString("N"),
            IsAdmin = isAdmin
        };

       await repository.AddAsync(newInvitation);
        
        return newInvitation.Code;
    }
}