using System.Collections.Concurrent;
using Govor.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Govor.API.Hubs.Infrastructure;

public class ConnectionManager : IConnectionManager
{
    private readonly IUserGroupsGetterService _userGroupsGetterService;
    private readonly IUserPrivateChatsGetterService _userPrivateChatsGetterService;
    private readonly IConnectionStore _connectionStore;
    private readonly IHubContext<ChatsHub> _hubContext;

    public ConnectionManager(
        IUserGroupsGetterService userGroupsGetterService,
        IConnectionStore connectionStore,
        IUserPrivateChatsGetterService userPrivateChatsGetterService, 
        IHubContext<ChatsHub> hubContext)
    {
        _userGroupsGetterService = userGroupsGetterService;
        _connectionStore = connectionStore;
        _userPrivateChatsGetterService = userPrivateChatsGetterService;
        _hubContext = hubContext;
    }

    public async Task OnConnectedAsync(string connectionId, Guid userId)
    {
        // user
        await _hubContext.Groups.AddToGroupAsync(connectionId, ChatHubConstants.GetUserGroup(userId));
        _connectionStore.AddConnection(userId, connectionId);
        
        // groups
        var userGroups = await _userGroupsGetterService.GetUserGroupsAsync(userId);
        foreach (var group in userGroups)
        {
            await _hubContext.Groups.AddToGroupAsync(connectionId, ChatHubConstants.GetChatGroup(group.Id));
        }
        
        // private chats
        var chats = await _userPrivateChatsGetterService.GetUserChatsAsync(userId);
        foreach (var group in chats)
        {
            await _hubContext.Groups.AddToGroupAsync(connectionId, ChatHubConstants.GetPrivateChat(group.Id));
        }
    }

    public async Task OnDisconnectedAsync(string connectionId, Guid userId)
    {
        if (userId != Guid.Empty)
        {
            await _hubContext.Groups.RemoveFromGroupAsync(connectionId, ChatHubConstants.GetUserGroup(userId));
            _connectionStore.RemoveConnection(userId, connectionId);
        }
    }
}