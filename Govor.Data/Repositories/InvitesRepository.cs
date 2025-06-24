using Govor.Core.Infrastructure.Validators;
using Govor.Core.Models;
using Govor.Core.Repositories.Invaites;

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
        throw new NotImplementedException();
    }

    public Task<Invitation> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<Invitation> GetByCodeAsync(string code)
    {
        throw new NotImplementedException();
    }

    public Task<List<Invitation>> GetAdminsInvitesAsync()
    {
        throw new NotImplementedException();
    }

    public Task AddAsync(Invitation invitation)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(Invitation invitation)
    {
        throw new NotImplementedException();
    }

    public Task RemoveAsync(Invitation invitation)
    {
        throw new NotImplementedException();
    }

    public bool Exist(Invitation invitation)
    {
        throw new NotImplementedException();
    }

    public bool Exist(Guid guid)
    {
        throw new NotImplementedException();
    }
}