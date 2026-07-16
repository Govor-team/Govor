using Govor.Domain;
using Govor.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Govor.Application.Profiles;

public class ProfileService : IProfileService
{
    private readonly ILogger<ProfileService> _logger;
    private readonly GovorDbContext _context;

    public ProfileService(GovorDbContext context, ILogger<ProfileService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<UserProfile>> GetUserProfileAsync(Guid userId)
    {
        _logger.LogInformation("Getting user {UserId} profile", userId);
     
        var profile = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(user => new UserProfile
            {
                Id = user.Id,
                Username = user.Username,
                Description = user.Description,
                IconId = user.IconId
            })
            .FirstOrDefaultAsync();

        if (profile is null)
        {
            return Result<UserProfile>.Failure(CreateNotFoundError(userId));
        }
        
        return profile;
    }

    public async Task<Result> SetDescription(string description, Guid userId)
    {
        _logger.LogInformation("Updating description for user {UserId}", userId);
        
        var updatedRows = await _context.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(u => u.Description, description));

        if (updatedRows == 0)
        {
            return Result.Failure(CreateNotFoundError(userId));
        }

        return Result.Success();
    }

    public async Task<Result> SetNewIcon(Guid userId, Guid iconId)
    {
        _logger.LogInformation("Updating icon for user {UserId}", userId);

       
        var updatedRows = await _context.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(u => u.IconId, iconId));

        if (updatedRows == 0)
        {
            return Result.Failure(CreateNotFoundError(userId));
        }
        
        return Result.Success();
    }
    
    private static Error CreateNotFoundError(Guid userId) => 
        new("Profile.UserNotFound", $"User with ID {userId} was not found.");
}
