using AutoMapper;
using Govor.Application.Profiles;
using Govor.Application.Users.UserOnlineStatus;
using Govor.Contracts.DTOs;

namespace Govor.API.Common.Mapping;

public class UserProfileToUserProfileDtoMappingAction  : IMappingAction<UserProfile, UserProfileDto>
{
    private readonly IOnlineUserStore _onlineUserStore;

    public UserProfileToUserProfileDtoMappingAction(IOnlineUserStore onlineUserStore)
    {
        _onlineUserStore = onlineUserStore;
    }
    
    public void Process(UserProfile source, UserProfileDto destination, ResolutionContext context)
    {
        destination.IsOnline = _onlineUserStore.IsOnline(source.Id);
    }
}