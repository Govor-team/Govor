using System.Net.Http.Json;
using Govor.Contracts.DTOs;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks; // Added for Task

namespace Govor.ConsoleClient
{
    public class FriendsClient
    {
        private readonly HttpClient _client;

        public FriendsClient(HttpClient client)
        {
            _client = client;
        }
        
        public async Task<List<UserDto>> SearchAsync(string query)
        {
            var response = await _client.GetAsync($"/api/friends/search?query={query}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<UserDto>>() ?? new List<UserDto>();
        }

        // SendFriendRequestAsync was removed as the SendFriendRequestCommand now uses SignalR directly.
        // public async Task SendFriendRequestAsync(Guid targetUserId)
        // {
        //     var response =  await _client.PostAsync($"api/friends/request?targetUserId={targetUserId}", null);
        //     response.EnsureSuccessStatusCode();
        // }

        public async Task<List<FriendshipDto>> GetIncomingRequestsAsync()
        {
            var response = await _client.GetAsync("/api/friends/requests");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<FriendshipDto>>() ?? new List<FriendshipDto>();
        }

        // AcceptFriendRequestAsync was removed as the AcceptFriendRequestCommand now uses SignalR directly.
        // public async Task AcceptFriendRequestAsync(Guid friendshipId) // Parameter was requesterId, should be friendshipId
        // {
        //     var response = await _client.PostAsync($"/api/friends/accept?friendshipId={friendshipId}", null);
        //     response.EnsureSuccessStatusCode();
        // }

        public async Task<List<UserDto>> GetFriendsAsync()
        {
            var response = await _client.GetAsync("/api/friends");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<UserDto>>() ?? new List<UserDto>();
        }
    }
    
}