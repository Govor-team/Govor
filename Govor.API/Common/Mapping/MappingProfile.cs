using AutoMapper;
using Govor.API.Extensions.Mapping;
using Govor.Application.Profiles;
using Govor.Contracts.DTOs;
using Govor.Contracts.Responses;
using Govor.Core.Models;
using Govor.Core.Models.Messages;
using Govor.Core.Models.Users;

namespace Govor.API.Common.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Message, MessageResponse>();
        CreateMap<MediaAttachments, MediaAttachmentResponse>();
        CreateMap<MessageReaction, MessageReactionResponse>();
        CreateMap<MessageView, MessageViewResponse>();

        CreateMap<User, UserDto>()
            .AfterMap<UserToUserDtoMappingAction>();
        
        CreateMap<Friendship, FriendshipDto>();

        CreateMap<UserSession, SessionDto>();

        CreateMap<UserProfile, UserProfileDto>();
    }
}