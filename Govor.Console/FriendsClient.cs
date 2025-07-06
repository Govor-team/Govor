using System.Net.Http.Json;
using Govor.Contracts.DTOs;

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

        public async Task SendFriendRequestAsync(Guid targetUserId)
        {
            var response =  await _client.PostAsync($"api/friends/request?targetUserId={targetUserId}", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<FriendshipDto>> GetIncomingRequestsAsync()
        {
            var response = await _client.GetAsync("/api/friends/requests");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<FriendshipDto>>() ?? new List<FriendshipDto>();
        }

        public async Task AcceptFriendRequestAsync(Guid requesterId)
        {
            var response = await _client.PostAsync($"/api/friends/accept?friendshipId={requesterId}", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<UserDto>> GetFriendsAsync()
        {
            var response = await _client.GetAsync("/api/friends");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<UserDto>>() ?? new List<UserDto>();
        }
    }
    
}
