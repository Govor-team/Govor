using Govor.API.Hubs;
using Govor.API.Hubs.Infrastructure;
using Govor.Application.Infrastructure.Extensions;
using Govor.Application.Interfaces;
using Govor.Application.PrivateUserChats;
using Govor.Domain.Models;
using Microsoft.AspNetCore.SignalR;

namespace Govor.API.Hubs.Infrastructure;

public class PrivateChatGroupManager : IPrivateChatGroupManager
{
    private readonly IConnectionStore _connectionStore;
    private readonly IHubContext<ChatsHub> _hubContext;

    public PrivateChatGroupManager(IConnectionStore connectionStore, IHubContext<ChatsHub> hubContext)
    {
        _connectionStore = connectionStore;
        _hubContext = hubContext;
    }

    public async Task AddUsersToPrivateChatGroupAsync(PrivateChat privateChat)
    {
        var connectionsA = _connectionStore.GetConnections(privateChat.UserAId);
        var connectionsB = _connectionStore.GetConnections(privateChat.UserBId);

        foreach (var conn in connectionsA.Concat(connectionsB))
        {
            await _hubContext.Groups.AddToGroupAsync(conn, ChatHubConstants.GetPrivateChat(privateChat.Id));
        }
    }

    public async Task RemoveUsersFromPrivateChatGroupAsync(PrivateChat privateChat)
    {
        var connectionsA = _connectionStore.GetConnections(privateChat.UserAId);
        var connectionsB = _connectionStore.GetConnections(privateChat.UserBId);

        foreach (var conn in connectionsA.Concat(connectionsB))
        {
            await _hubContext.Groups.RemoveFromGroupAsync(conn, ChatHubConstants.GetPrivateChat(privateChat.Id));
        }
    }
}