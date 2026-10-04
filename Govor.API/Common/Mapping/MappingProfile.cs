using AutoMapper;
using Govor.Application.Profiles;
using Govor.Contracts.DTOs;
using Govor.Contracts.Responses;
using Govor.Domain.Models;
using Govor.Domain.Models.Messages;
using Govor.Domain.Models.Users;
using Govor.Application.Messages.Parameters;
using Govor.Contracts.Responses.SignalR;
using Govor.Application.Reactions;
using Govor.Domain.Models.Reactions;
using Govor.Application.Groups;

namespace Govor.API.Common.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ChatReadResult, ChatReadResponse>();
        CreateMap<ReactionPack, ReactionPackResponse>();
        CreateMap<ReactionItem, ReactionItemResponse>();
        CreateMap<ReactionCount, ReactionCountResponse>();
        CreateMap<MessageReactionState, MessageReactionsChangedResponse>();
        CreateMap<ChannelReactionPolicy, ChannelReactionPolicyResponse>();
        CreateMap<GroupSummary, GroupResponse>()
            .IncludeMembers(s => s.Group);
        CreateMap<ChatGroup, GroupResponse>();
        CreateMap<GroupMember, GroupMemberResponse>();
        CreateMap<GroupInvitation, GroupInvitationResponse>();
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

    }
}
