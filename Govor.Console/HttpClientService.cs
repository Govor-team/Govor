using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Govor.Contracts.Requests;

namespace Govor.ConsoleClient
{
    public class HttpClientService
    {
        private readonly HttpClient _client;

        public HttpClientService(string baseUrl)
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };
        }

        public string GetBaseUrl() => _client.BaseAddress?.ToString() ?? string.Empty;

        public async Task<string> GetAsync(string uri)
        {
            var response = await _client.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> PostAsync(string uri, object? payload = null)
        {
            HttpResponseMessage response;

            if (payload is null)
                response = await _client.PostAsync(uri, null);
            else
                response = await _client.PostAsJsonAsync(uri, payload);

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> RegisterAsync(string username, string password, string inviteCode)
        {
            var request = new RegistrationRequest
            {
                Name = username,
                Password = password,
                InviteLink = inviteCode
            };

            var response = await _client.PostAsJsonAsync("/api/auth/register", request);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
                return result?.Token;
            }

            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Registration failed: {error}");
        }

        public async Task<string> LoginAsync(string username, string password)
        {
            var request = new LoginRequest
            {
                Name = username,
                Password = password
            };

            var response = await _client.PostAsJsonAsync("/api/auth/login", request);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
                return result?.Token;
            }

            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Login failed: {error}");
        }
    }

    public class TokenResponse
    {
        public string Token { get; set; }
    }
}
