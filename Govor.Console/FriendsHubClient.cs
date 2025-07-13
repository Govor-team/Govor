using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;
using Govor.Contracts.Responses.SignalR;

namespace Govor.ConsoleClient;


public class FriendsHubClient
{
    private readonly string _hubUrl;
    private readonly string _jwtToken;
    private HubConnection _connection;

    public FriendsHubClient(string hubUrl, string jwtToken)
    {
        _hubUrl = hubUrl;
        _jwtToken = jwtToken;
    }

    public async Task ConnectAsync()
    {
        _connection = new HubConnectionBuilder()
            .WithUrl($"{_hubUrl}/friends", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(_jwtToken);
            })
            .WithAutomaticReconnect()
            .Build();

        // Событие: получен входящий запрос в друзья
        _connection.On<string>("FriendRequestReceived", userId =>
        {
            Console.WriteLine($"📨 Friend request received from: {userId}");
        });

        // Событие: запрос принят
        _connection.On<string>("FriendRequestAccepted", friendshipId =>
        {
            Console.WriteLine($"✅ Friend request accepted! Friendship ID: {friendshipId}");
        });

        // Событие: запрос отклонён
        _connection.On<string>("FriendRequestRejected", friendshipId =>
        {
            Console.WriteLine($"❌ Friend request rejected! Friendship ID: {friendshipId}");
        });

        await _connection.StartAsync();
        Console.WriteLine("✅ Connected to FriendsHub");
    }

    public async Task SendFriendRequestAsync(Guid targetUserId)
    {
        var result = await _connection.InvokeAsync<HubResult<object>>("SendRequest", targetUserId);
        Console.WriteLine($"📤 Friend request sent. Status: {result.Status}");
    }

    public async Task AcceptFriendRequestAsync(Guid friendshipId)
    {
        var result = await _connection.InvokeAsync<HubResult<object>>("AcceptRequest", friendshipId);
        Console.WriteLine($"👍 Accept result: {result.Status}");
    }

    public async Task RejectFriendRequestAsync(Guid friendshipId)
    {
        var result = await _connection.InvokeAsync<HubResult<object>>("RejectRequest", friendshipId);
        Console.WriteLine($"👎 Reject result: {result.Status}");
    }
}
