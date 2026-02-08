using AutoMapper;
using Govor.Application.Interfaces.UserOnlineStatus;
using Govor.Contracts.DTOs;
using Govor.Core.Models.Users;

namespace Govor.API.Extensions.Mapping;

public class UserToUserDtoMappingAction : IMappingAction<User, UserDto>
{
    private readonly IOnlineUserStore _onlineUserStore;

    public UserToUserDtoMappingAction(IOnlineUserStore onlineUserStore)
    {
        _onlineUserStore = onlineUserStore;
    }

    public void Process(User source, UserDto destination, ResolutionContext context)
    {
        destination.IsOnline = _onlineUserStore.IsOnline(source.Id);
    }
}