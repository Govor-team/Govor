using AutoMapper;
using Govor.Application.Profiles;
using Govor.Contracts.DTOs;
using Govor.Contracts.Responses;
using Govor.Domain.Models;
using Govor.Domain.Models.Messages;
using Govor.Domain.Models.Users;

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

        CreateMap<UserProfile, UserProfileDto>()
            .AfterMap<UserProfileToUserProfileDtoMappingAction>();
        
        CreateMap<Friendship, FriendshipDto>();

        CreateMap<UserSession, SessionDto>();

        CreateMap<UserProfile, UserProfileDto>();
    }
}