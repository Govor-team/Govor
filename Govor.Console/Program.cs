using Microsoft.AspNetCore.SignalR.Client;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Govor.Contracts.Requests;
using Govor.Contracts.Requests.SignalR;
using Govor.Contracts.Responses.SignalR;
using Govor.Core.Models;


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
        //static string baseUrl = "https://govor-team-govor-88b3.twc1.net";
        static string baseUrl = "https://localhost:7155";
        static string? AuthToken = null;
        static HttpClientService HttpService = new(baseUrl);
        private static FriendsClient? friendsClient;
        static HubConnection? _hubConnection;
        static Dictionary<string, List<string>> ChatHistory = new();
        static string? CurrentChatUser = null;
        static Guid CurrentChatUserId = Guid.Empty;

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
                        sharedClient.BaseAddress = new Uri(baseUrl);
                        sharedClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthToken);

                        friendsClient = new FriendsClient(sharedClient);
                        await InitializeHubConnection();
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
                        sharedClient.BaseAddress = new Uri(baseUrl);
                        sharedClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthToken);

                        friendsClient = new FriendsClient(sharedClient);
                        await InitializeHubConnection();
                        Console.WriteLine("[Успех] Регистрация завершена. Токен сохранен.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Ошибка] {ex.Message}");
                    }
                    break;
                case "/search":
                    if (friendsClient == null)
                    {
                        Console.WriteLine("[Ошибка] Сначала войдите в систему.");
                        break;
                    }
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
                    if (_hubConnection != null)
                    {
                        await _hubConnection.StopAsync();
                        await _hubConnection.DisposeAsync();
                    }
                    Environment.Exit(0);
                    break;

                case "/friends":
                    if (friendsClient == null)
                    {
                        Console.WriteLine("[Ошибка] Сначала войдите в систему.");
                        break;
                    }
                    var friends = await friendsClient.GetFriendsAsync();
                    foreach (var f in friends)
                    {
                        Console.WriteLine($"{f.Username} | был онлайн: {f.WasOnline} [{f.Id}]");
                    }
                    break;
                case "/friend":
                    if (friendsClient == null)
                    {
                        Console.WriteLine("[Ошибка] Сначала войдите в систему.");
                        break;
                    }
                    Console.Write("Введите ID пользователя, которому хотите отправить запрос: ");
                    var targetId = Guid.Parse(Console.ReadLine());
                    await friendsClient.SendFriendRequestAsync(targetId);
                    Console.WriteLine("Запрос отправлен");
                    break;

                case "/accept":
                    if (friendsClient == null)
                    {
                        Console.WriteLine("[Ошибка] Сначала войдите в систему.");
                        break;
                    }
                    Console.Write("Введите ID заявки, которую хотите принять принимаете: ");
                    var acceptId = Guid.Parse(Console.ReadLine());
                    await friendsClient.AcceptFriendRequestAsync(acceptId);
                    Console.WriteLine("Принято");
                    break;
                case "/incoming":
                    if (friendsClient == null)
                    {
                        Console.WriteLine("[Ошибка] Сначала войдите в систему.");
                        break;
                    }
                    var requests = await friendsClient.GetIncomingRequestsAsync();
                    foreach (var r in requests)
                    {
                        Console.WriteLine($"Запрос от: {r.RequesterId} (добавьте через /accept {r.Id})");
                    }
                    break;
                case "/chat":
                    if (string.IsNullOrEmpty(argument))
                    {
                        Console.WriteLine("[Ошибка] Укажите имя друга и ID через пробел (например /chat Egor GUID)");
                        break;
                    }

                    var chatArgs = argument.Split(' ', 2);
                    if (chatArgs.Length < 2 || !Guid.TryParse(chatArgs[1], out var chatUserId))
                    {
                        Console.WriteLine("[Ошибка] Неверный формат. Укажите имя друга и ID (например /chat Egor GUID)");
                        break;
                    }

                    CurrentChatUser = chatArgs[0];
                    CurrentChatUserId = chatUserId;
                    ShowChat(CurrentChatUser);
                    break;

                case "/back":
                    CurrentChatUser = null;
                    CurrentChatUserId = Guid.Empty;
                    break;

                default:
                    Console.WriteLine("[Ошибка] Неизвестная команда");
                    break;
            }
        }

        static async Task InitializeHubConnection()
        {
            if (string.IsNullOrEmpty(AuthToken))
            {
                Console.WriteLine("[Ошибка] Токен аутентификации отсутствует. Пожалуйста, войдите в систему.");
                return;
            }

            _hubConnection = new HubConnectionBuilder()
                .WithUrl($"{baseUrl}/api/chats", options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(AuthToken);
                })
                .Build();

            _hubConnection.On<UserMessageResponse>("ReceiveMessage", (message) =>
            {
                var senderName = message.SenderId == GetMyUserId() ? "Ты" : CurrentChatUser ?? "Неизвестно"; // Simplified for now
                var displayMessage = $"[{message.SentAt:HH:mm}] {senderName} >> {message.EncryptedContent}";
                
                string chatKey = message.SenderId == GetMyUserId() ? message.RecipientId.ToString() : message.SenderId.ToString();

                if (CurrentChatUser != null && (message.SenderId == CurrentChatUserId || message.RecipientId == CurrentChatUserId))
                {
                     if (!ChatHistory.ContainsKey(CurrentChatUser))
                     {
                         ChatHistory[CurrentChatUser] = new List<string>();
                     }
                     ChatHistory[CurrentChatUser].Add(displayMessage);
                     Console.SetCursorPosition(0, Console.CursorTop); // Move cursor to beginning of line
                     Console.Write(new string(' ', Console.WindowWidth -1)); // Clear current line
                     Console.SetCursorPosition(0, Console.CursorTop);
                     Console.WriteLine($" * {displayMessage}"); // Display new message
                     Console.Write(">> "); // Re-display prompt
                }
                else
                {
                    // Handle notification for messages not in the current chat - TBD
                    Console.WriteLine($"\n[Новое сообщение от {message.SenderId}]: {message.EncryptedContent}");
                     Console.Write(CurrentChatUser == null ? "/> " : ">> ");
                }
            });
            
            _hubConnection.On<UserMessageResponse>("MessageSent", (message) =>
            {
                // Optional: Handle confirmation that message was sent successfully by the server
                // This could be useful for updating UI to show message status (e.g., "sent", "delivered")
                // For console, just logging or ignoring might be fine.
                // Console.WriteLine($"[Система] Сообщение {message.MessageId} доставлено на сервер.");
            });

            try
            {
                await _hubConnection.StartAsync();
                Console.WriteLine("[SignalR] Соединение установлено.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SignalR Ошибка] {ex.Message}");
            }
        }

        static Guid GetMyUserId()
        {
            if (AuthToken == null) return Guid.Empty;
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(AuthToken);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "userId");
            return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) ? userId : Guid.Empty;
        }


        static void ShowChat(string user)
        {
            Console.Clear();
            Console.WriteLine($"/*====================================");
            Console.WriteLine($" * Личные сообщения | {user} ({CurrentChatUserId})");
            Console.WriteLine($" *====================================");

            if (!ChatHistory.ContainsKey(user))
            {
                ChatHistory[user] = new List<string>();
                // Example initial messages - consider fetching history if implementing that
                // ChatHistory[user].Add($"[Система] Начало чата с {user}");
            }

            foreach (var msg in ChatHistory[user])
                Console.WriteLine(" * " + msg);

            Console.WriteLine(" *------------------------------------");
        }

        static async void SendMessage(string messageContent)
        {
            if (string.IsNullOrWhiteSpace(messageContent) || _hubConnection == null || CurrentChatUserId == Guid.Empty) return;

            var myUserId = GetMyUserId();
            if (myUserId == Guid.Empty)
            {
                Console.WriteLine("[Ошибка] Не удалось определить ID пользователя. Попробуйте войти снова.");
                return;
            }

            var messageRequest = new MessageRequest
            {
                RecipientId = CurrentChatUserId,
                EncryptedContent = messageContent, // Assuming content is not actually encrypted for console client simplicity for now
                RecipientType = RecipientType.User, 
                // MediaAttachments and ReplyToMessageId can be added if needed
            };

            try
            {
                await _hubConnection.InvokeAsync("Send", messageRequest);
                
                // Add to local history immediately for responsiveness. 
                // Server will send "ReceiveMessage" to self as well if needed, or "MessageSent" for confirmation.
                var displayMessage = $"[{DateTime.Now:HH:mm}] Ты >> {messageContent}";
                if (!ChatHistory.ContainsKey(CurrentChatUser!))
                {
                    ChatHistory[CurrentChatUser!] = new List<string>();
                }
                ChatHistory[CurrentChatUser!].Add(displayMessage);
                // Console.WriteLine($"Ты >> {messageContent}"); // Already handled by input echo + ReceiveMessage from self
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Ошибка отправки] {ex.Message}");
            }
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