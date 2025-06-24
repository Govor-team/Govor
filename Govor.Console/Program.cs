using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Govor.Core.DTOs;


/*====================================
 *Горелые Петушки Чат
 *====================================
 *
 * [16:30] Егор >> Привет, Питухам!
 * [16:31] Anon >> Че как?
 *                   Я норм << [16:32]
 *------------------------------------
 * >> "Привет, Егор!"
 */

class Program
{
    private static HttpClient _httpClient = new HttpClient();
    private static HubConnection _hubConnection;
    private static string _jwtToken;
    private static Guid _userId;
    private static Guid _toUserId;

    static async Task Main(string[] args)
    {
        Console.WriteLine("Welcome to the Chat Client!");
        Console.WriteLine("Enter your username:");
        string username = Console.ReadLine();
        Console.WriteLine("Enter your password:");
        string password = Console.ReadLine();

        // Аутентификация и получение JWT
        bool authenticated = await AuthenticateAsync(username, password);
        if (!authenticated)
        {
            Console.WriteLine("Authentication failed. Exiting...");
            return;
        }

        Console.Clear();

        // Извлечение UserId из JWT
        _userId = ExtractUserIdFromJwt(_jwtToken);
        Console.WriteLine($"Authenticated successfully! Your UserId: {_userId}");

        // Настройка SignalR
        await SetupSignalRAsync();

        // Обработка команд
        Console.WriteLine("Enter command /chat {username} to send a chat message");
        Console.WriteLine("Enter commands in format: /send {message}, in chat {username}");
        Console.WriteLine("Type 'exit' to quit.");

        while (true)
        {
            string input = Console.ReadLine();
            if (input.ToLower() == "exit")
                break;

            await ProcessCommandAsync(input);
        }

        // Отключение SignalR
        await _hubConnection.StopAsync();
    }

    private static async Task<bool> AuthenticateAsync(string username, string password)
    {
        try
        {
            RegistrationRequest loginData = new RegistrationRequest()
            {
                Name = username,
                Password = password
            };

            var response = await _httpClient.PostAsJsonAsync(
                "https://localhost:7155/api/Auth/login",
                loginData);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Login failed: {response.StatusCode}");
                return false;
            }

            var json = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<LoginResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            _jwtToken = tokenResponse.token;
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Authentication error: {ex.Message}");
            return false;
        }
    }

    private static Guid ExtractUserIdFromJwt(string jwtToken)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwtToken);
            var userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "userID")?.Value;
            return Guid.TryParse(userIdClaim, out Guid userId) ? userId : Guid.Empty;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting UserId: {ex.Message}");
            return Guid.Empty;
        }
    }

    private static async Task SetupSignalRAsync()
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7155/api/chats", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(_jwtToken);
            })
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<string, Guid>("Receive", (message, senderId) =>
        {
            Console.WriteLine($"[{DateTime.Now}] From {senderId}: {message}");
        });

        try
        {
            await _hubConnection.StartAsync();
            Console.WriteLine("Connected to SignalR hub.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR connection error: {ex.Message}");
        }
    }

    private static async Task ProcessCommandAsync(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return;

        var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3 || parts[0].ToLower() != "/send")
        {
            Console.WriteLine("Invalid command. Use: /send {toUserId} {message}");
            return;
        }

        if (!Guid.TryParse(parts[1], out Guid toUserId))
        {
            Console.WriteLine("Invalid toUserId format. Must be a valid Guid.");
            return;
        }

        string message = string.Join(" ", parts[2..]);
        if (string.IsNullOrWhiteSpace(message))
        {
            Console.WriteLine("Message cannot be empty.");
            return;
        }

        try
        {
            await _hubConnection.InvokeAsync("Send", message, toUserId);
            Console.WriteLine($"Message sent to {toUserId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message: {ex.Message}");
        }
    }
}

public class LoginResponse
{
    public string token { get; set; }
}