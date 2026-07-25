using Govor.Domain;
using Govor.Domain.Common;
using Govor.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartRes;

namespace Govor.Application.Infrastructure.AdminsStuff;

public class InvitationGetter : IInvitationGetter
{
    private readonly ILogger<InvitationGetter> _logger;
    private readonly GovorDbContext  _context;

    public InvitationGetter(ILogger<InvitationGetter> logger, GovorDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<List<Invitation>> GetAllAsync()
    {
        return await _context.Invitations
            .AsNoTracking()
            .Where(iv => iv.IsActive)
            .ToListAsync();
    }

    public async Task<Result<Invitation, Error>> FindByIdAsync(Guid id)
    {
        var res = await _context.Invitations.AsNoTracking()
            .FirstOrDefaultAsync(iv => iv.Id == id);
        
        if (res is null)
            return Result.Failure<Invitation>(Error.NotFound(
                nameof(InvalidOperationException),
                "Invitation not found.")
            );
        
        return res;
    }
}