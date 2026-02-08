using AutoMapper;
using Govor.Application.Interfaces.UserOnlineStatus;
using Govor.Application.Profiles;
using Govor.Contracts.DTOs;
using Govor.Core.Models.Users;

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