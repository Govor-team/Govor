using AutoMapper;
using Govor.Contracts.DTOs;
using Govor.Contracts.Responses;
using Govor.Core.Models;
using Govor.Core.Models.Messages;
using Govor.Core.Models.Users;

namespace Govor.API.Extensions;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Message, MessageResponse>();
        CreateMap<MediaAttachments, MediaAttachmentResponse>();
        CreateMap<MessageReaction, MessageReactionResponse>();
        CreateMap<MessageView, MessageViewResponse>();

        CreateMap<User, UserDto>();
        CreateMap<Friendship, FriendshipDto>();
    }
}