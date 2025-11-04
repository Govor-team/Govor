using Govor.Application.Interfaces;
using Govor.Application.Profiles;
using Govor.Core.Repositories.Users;
using Microsoft.Extensions.Logging;

namespace Govor.Application.Services;

public class ProfileService : IProfileService
{
    private readonly ILogger<ProfileService> _logger;
    private readonly IUsersRepository _userRepository;

    public ProfileService(IUsersRepository userRepository, ILogger<ProfileService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<UserProfile> GetUserProfileAsync(Guid userId)
    {
        _logger.LogInformation("Gettings user {userId} profile", userId);
        var user = await _userRepository.FindByIdAsync(userId);

        return new UserProfile
        {
            Id = user.Id,
            Username = user.Username,
            Description = user.Description,
            IconId = user.IconId
        };
    }

    public async Task SetDescription(string description, Guid userId)
    {
        var user = await _userRepository.FindByIdAsync(userId);

        user.Description = description;
        await _userRepository.UpdateAsync(user);
    }

    public async Task SetNewIcon(Guid userId, Guid iconId)
    {
        var user = await _userRepository.FindByIdAsync(userId);

        user.IconId = iconId;
        await _userRepository.UpdateAsync(user);
    }
}
