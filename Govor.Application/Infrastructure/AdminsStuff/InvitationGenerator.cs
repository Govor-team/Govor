using Govor.Domain;
using Govor.Domain.Models;

namespace Govor.Application.Infrastructure.AdminsStuff;

public class InvitationGenerator(GovorDbContext context) : IInvitationGenerator
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

        await context.Invitations.AddAsync(newInvitation);
        
        await context.SaveChangesAsync();
        
        return newInvitation.Code;
    }
}