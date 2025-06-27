using Microsoft.AspNetCore.SignalR.Client;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Govor.Contracts.Requests;


/*====================================
 *Личные сообщения| Егор 
 *====================================
 * [15:59] Вы добавили (Егор) в друзья
 * 
 * [16:30] Егор >> Привет!
 * [16:31] Ты >> Че как?
 *
 *------------------------------------
 * >> "Пойдем завтра гулять?!"
 */

namespace Govor.ConsoleClient
{
    class Program
    {
        static string? AuthToken = null;
        static HttpClientService HttpService = new("https://localhost:7155"); // поменяй URL на свой
        private static FriendsClient friendsClient;
        static Dictionary<string, List<string>> ChatHistory = new();
        static string CurrentChatUser = null;

        static async Task Main()
        {
            LoginWindow();

            while (true)
            {
                Console.Write(CurrentChatUser == null ? "/> " : ">> ");
                var input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input)) continue;

                if (input.StartsWith("/"))
                    HandleCommand(input);
                else if (CurrentChatUser != null)
                    SendMessage(input);
                else
                    Console.WriteLine("[Система] Введите команду, например /friends");
            }
        }

        static async void HandleCommand(string input)
        {
            var args = input.Split(' ', 2);
            var command = args[0].ToLower();
            var argument = args.Length > 1 ? args[1] : null;

            switch (command)
            {
                case "/login":
                    Console.Write("username: ");
                    var loginUsername = Console.ReadLine();

                    Console.Write("password: ");
                    var loginPassword = Console.ReadLine();

                    try
                    {
                        AuthToken = await HttpService.LoginAsync(loginUsername, loginPassword);
                        HttpClient sharedClient = new();
                        sharedClient.BaseAddress = new Uri("https://localhost:7155");
                        sharedClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthToken);

                        friendsClient = new FriendsClient(sharedClient);
                        Console.WriteLine("[Успех] Вход выполнен. Токен сохранен.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Ошибка] {ex.Message}");
                    }
                    break;
                case "/reg":
                    Console.Write("username: ");
                    var regUsername = Console.ReadLine();

                    Console.Write("password: ");
                    var regPassword = Console.ReadLine();

                    Console.Write("invitation: ");
                    var inviteCode = Console.ReadLine();

                    try
                    {
                        AuthToken = await HttpService.RegisterAsync(regUsername, regPassword, inviteCode);
                        HttpClient sharedClient = new();
                        sharedClient.BaseAddress = new Uri("https://localhost:7155");
                        sharedClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthToken);

                        friendsClient = new FriendsClient(sharedClient);
                        Console.WriteLine("[Успех] Регистрация завершена. Токен сохранен.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Ошибка] {ex.Message}");
                    }
                    break;
                case "/search":
                    Console.Write("Введите имя друга: ");
                    var q = Console.ReadLine();
                    var foundUsers = await friendsClient.SearchAsync(q);
                    foreach (var user in foundUsers)
                    {
                        Console.WriteLine($"{user.Username} [{user.Id}]");
                    }
                    break;
                
                case "/exit":
                    Console.WriteLine("[Система] Выход...");
                    Environment.Exit(0);
                    break;

                case "/friends":
                    var friends = await friendsClient.GetFriendsAsync();
                    foreach (var f in friends)
                    {
                        Console.WriteLine($"{f.Username} | был онлайн: {f.WasOnline}");
                    }
                    break;
                case "/friend":
                    Console.Write("Введите ID пользователя, которому хотите отправить запрос: ");
                    var targetId = Guid.Parse(Console.ReadLine());
                    await friendsClient.SendFriendRequestAsync(targetId);
                    Console.WriteLine("Запрос отправлен");
                    break;

                case "/accept":
                    Console.Write("Введите ID пользователя, чью заявку принимаете: ");
                    var acceptId = Guid.Parse(Console.ReadLine());
                    await friendsClient.AcceptFriendRequestAsync(acceptId);
                    Console.WriteLine("Принято");
                    break;
                case "/incoming":
                    var requests = await friendsClient.GetIncomingRequestsAsync();
                    foreach (var r in requests)
                    {
                        Console.WriteLine($"Запрос от: {r.RequesterId} (добавьте через /accept {r.RequesterId})");
                    }
                    break;
                case "/chat":
                    if (string.IsNullOrEmpty(argument))
                    {
                        Console.WriteLine("[Ошибка] Укажите имя друга");
                        break;
                    }

                    CurrentChatUser = argument;
                    ShowChat(argument);
                    break;

                case "/back":
                    CurrentChatUser = null;
                    break;

                default:
                    Console.WriteLine("[Ошибка] Неизвестная команда");
                    break;
            }
        }

        static void ShowChat(string user)
        {
            Console.Clear();
            Console.WriteLine($"/*====================================");
            Console.WriteLine($" * Личные сообщения | {user}");
            Console.WriteLine($" *====================================");

            if (!ChatHistory.ContainsKey(user))
            {
                ChatHistory[user] = new List<string>
                {
                    $"[15:59] Вы добавили ({user}) в друзья",
                    $"[16:30] {user} >> Привет!",
                    $"[16:31] Ты >> Че как?"
                };
            }

            foreach (var msg in ChatHistory[user])
                Console.WriteLine(" * " + msg);

            Console.WriteLine(" *------------------------------------");
        }

        static void SendMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;

            ChatHistory[CurrentChatUser].Add($"Ты >> {message}");
            Console.WriteLine($">> {message}");
        }

        static void LoginWindow()
        {
            Console.WriteLine($"/*====================================");
            Console.WriteLine($" * Добро пожаловать в Govor!");
            Console.WriteLine($" *====================================");
            Console.WriteLine("  * /reg - создать аккаунт ");
            Console.WriteLine("  * /login - войти");
        }
    }
}


public class LoginResponse
{
    public string token { get; set; }
}