using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

class FriendsHubClient
{
    private readonly HubConnection _connection;

    public FriendsHubClient(string hubUrl, string jwtToken)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(jwtToken);
            })
            .WithAutomaticReconnect()
            .Build();

        // Подписка на события от сервера
        _connection.On<Guid>("FriendRequestReceived", OnFriendRequestReceived);
        _connection.On<Guid>("FriendRequestAccepted", OnFriendRequestAccepted);
        _connection.On<Guid>("FriendRequestRejected", OnFriendRequestRejected);
    }

    private void OnFriendRequestReceived(Guid userId)
    {
        Console.WriteLine($"[Event] Получен запрос в друзья от пользователя: {userId}");
    }

    private void OnFriendRequestAccepted(Guid friendshipId)
    {
        Console.WriteLine($"[Event] Запрос в друзья принят. ID дружбы: {friendshipId}");
    }

    private void OnFriendRequestRejected(Guid friendshipId)
    {
        Console.WriteLine($"[Event] Запрос в друзья отклонён. ID дружбы: {friendshipId}");
    }

    public async Task StartAsync()
    {
        await _connection.StartAsync();
        Console.WriteLine("Подключено к FriendsHub");
    }

    public async Task StopAsync()
    {
        await _connection.StopAsync();
        Console.WriteLine("Отключено от FriendsHub");
    }

    public async Task SendRequest(Guid targetUserId)
    {
        try
        {
            await _connection.InvokeAsync("SendRequest", targetUserId);
            Console.WriteLine($"Запрос на добавление в друзья отправлен пользователю {targetUserId}");
        }
        catch (HubException ex)
        {
            Console.WriteLine($"Ошибка при отправке запроса: {ex.Message}");
        }
    }

    public async Task AcceptRequest(Guid friendshipId)
    {
        try
        {
            await _connection.InvokeAsync("AcceptRequest", friendshipId);
            Console.WriteLine($"Запрос в друзья с ID {friendshipId} принят");
        }
        catch (HubException ex)
        {
            Console.WriteLine($"Ошибка при принятии запроса: {ex.Message}");
        }
    }

    public async Task RejectRequest(Guid friendshipId)
    {
        try
        {
            await _connection.InvokeAsync("RejectRequest", friendshipId);
            Console.WriteLine($"Запрос в друзья с ID {friendshipId} отклонён");
        }
        catch (HubException ex)
        {
            Console.WriteLine($"Ошибка при отклонении запроса: {ex.Message}");
        }
    }
}

class Program
{
    static async Task TestMain(string[] args)
    {
        // Адрес хаба SignalR
        string hubUrl = "https://yourserver.com/api/friends";

        // JWT токен для аутентификации (замени на настоящий)
        string jwtToken = "your_jwt_token_here";

        var client = new FriendsHubClient(hubUrl, jwtToken);

        await client.StartAsync();

        Console.WriteLine("Команды:");
        Console.WriteLine("send <userId> - Отправить запрос в друзья");
        Console.WriteLine("accept <friendshipId> - Принять запрос");
        Console.WriteLine("reject <friendshipId> - Отклонить запрос");
        Console.WriteLine("exit - Выйти");

        while (true)
        {
            Console.Write("> ");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
                continue;

            var parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var command = parts[0].ToLowerInvariant();

            if (command == "exit")
                break;

            if (parts.Length < 2)
            {
                Console.WriteLine("Ошибка: не хватает аргументов");
                continue;
            }

            if (!Guid.TryParse(parts[1], out var id))
            {
                Console.WriteLine("Ошибка: некорректный GUID");
                continue;
            }

            switch (command)
            {
                case "send":
                    await client.SendRequest(id);
                    break;
                case "accept":
                    await client.AcceptRequest(id);
                    break;
                case "reject":
                    await client.RejectRequest(id);
                    break;
                default:
                    Console.WriteLine("Неизвестная команда");
                    break;
            }
        }

        await client.StopAsync();
    }
}
