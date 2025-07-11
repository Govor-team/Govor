using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Govor.ConsoleClient.Commands;
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
        private static FriendsClient? _friendsClient; // Renamed to avoid conflict with BaseCommand
        static HubConnection? _hubConnection;
        static Dictionary<string, List<string>> ChatHistory = new();
        static string? CurrentChatUser = null;
        static Guid CurrentChatUserId = Guid.Empty;

        private static Dictionary<string, ICommand> _commands = new();

        static async Task Main()
        {
            InitializeCommands();
            BaseCommand.InitializeServices(
                _friendsClient, // Initially null, will be set by Login/Register commands
                HttpService,
                () => AuthToken,
                (token) => AuthToken = token,
                InitializeHubConnection,
                () => _hubConnection
            );

            LoginWindow();

            while (true)
            {
                Console.Write(CurrentChatUser == null ? "/> " : ">> ");
                var input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input)) continue;

                if (input.StartsWith("/"))
                    await HandleCommandAsync(input);
                else if (CurrentChatUser != null)
                    await SendMessageAsync(input);
                else
                    Console.WriteLine("[Система] Введите команду, например /help или /friends");
            }
        }

        static void InitializeCommands()
        {
            _commands = new Dictionary<string, ICommand>
            {
                { "/login", new LoginCommand() },
                { "/reg", new RegisterCommand() },
                { "/search", new SearchUserCommand() },
                { "/friends", new ListFriendsCommand() },
                { "/friend", new SendFriendRequestCommand() }, // Alias for SendFriendRequest
                { "/sendrequest", new SendFriendRequestCommand() },
                { "/accept", new AcceptFriendRequestCommand() },
                { "/reject", new RejectFriendRequestCommand() },
                { "/incoming", new ListIncomingRequestsCommand() },
                { "/outgoing", new ListOutgoingRequestsCommand() },
                { "/block", new BlockUserCommand() },
                { "/unblock", new UnblockUserCommand() },
                
                { "/adminlistallfs", new AdminListAllFriendshipsCommand() },
                { "/adminlistuserfs", new AdminListUserFriendshipsCommand() },
                { "/adminremovefs", new AdminRemoveFriendshipCommand() },
                // Help command is special as it needs the list of commands
                // It will be added after other commands are initialized
            };
            _commands.Add("/help", new HelpCommand(_commands));
            _commands.Add("-h", new HelpCommand(_commands)); // Alias for help

            // Add other general commands not refactored yet if any, or create classes for them
            // For example, /exit, /chat, /back will be handled separately for now or made into commands
        }

        // Public static method to allow commands (like Login/Register) to update the FriendsClient
        public static void UpdateFriendsClient(FriendsClient newFriendsClient)
        {
            _friendsClient = newFriendsClient;
            // Re-initialize services in BaseCommand with the new FriendsClient
            BaseCommand.InitializeServices(
                _friendsClient,
                HttpService,
                () => AuthToken,
                (token) => AuthToken = token,
                InitializeHubConnection,
                () => _hubConnection
            );
        }


        static async Task HandleCommandAsync(string input)
        {
            var parts = input.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            var commandKey = parts[0].ToLower();
            var argument = parts.Length > 1 ? parts[1] : null;

            if (_commands.TryGetValue(commandKey, out var commandInstance))
            {
                try
                {
                    await commandInstance.ExecuteAsync(argument);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Критическая ошибка выполнения команды {commandKey}]: {ex.Message}");
                    // Log detailed exception: Console.WriteLine(ex.ToString());
                }
            }
            else
            {
                // Handle non-refactored commands for now
                switch (commandKey)
                {
                    case "/exit":
                        Console.WriteLine("[Система] Выход...");
                        if (_hubConnection != null)
                        {
                            await _hubConnection.StopAsync();
                            await _hubConnection.DisposeAsync();
                        }
                        Environment.Exit(0);
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
                        Console.Clear(); // Optionally clear console when going back
                        LoginWindow(); // Or some other general view
                        break;
                    default:
                        Console.WriteLine($"[Ошибка] Неизвестная команда: {commandKey}. Введите /help для списка команд.");
                        break;
                }
            }
        }

        static async Task InitializeHubConnection()
        {
            if (string.IsNullOrEmpty(AuthToken))
            {
                // This check might be redundant if commands calling this ensure login first
                Console.WriteLine("[Ошибка] Токен аутентификации отсутствует. Пожалуйста, войдите в систему.");
                return;
            }

            if (_hubConnection != null && _hubConnection.State != HubConnectionState.Disconnected)
            {
                Console.WriteLine("[SignalR] Соединение уже установлено или устанавливается.");
                return;
            }

            _hubConnection = new HubConnectionBuilder()
                .WithUrl($"{baseUrl}/hubs/chats", options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(AuthToken);
                })
                .Build();

            _hubConnection.On<UserMessageResponse>("ReceiveMessage", (message) =>
            {
                var myId = GetMyUserId();
                var senderName = message.SenderId == myId ? "Ты" : CurrentChatUser ?? message.SenderId.ToString() ?? "Неизвестно";
                var displayMessage = $"[{message.SentAt:HH:mm}] {senderName} >> {message.EncryptedContent}";
                
                // Determine chat key based on sender/recipient, ensuring consistency
                string chatKey = message.SenderId == myId ? message.RecipientId.ToString() : message.SenderId.ToString();
                string activeChatDisplayUser = CurrentChatUser; // User whose chat window is open

                // If the message belongs to the currently active chat window
                if (CurrentChatUser != null && (message.SenderId == CurrentChatUserId || (message.RecipientId == CurrentChatUserId && message.SenderId == myId)))
                {
                     if (!ChatHistory.ContainsKey(activeChatDisplayUser))
                     {
                         ChatHistory[activeChatDisplayUser] = new List<string>();
                     }
                     ChatHistory[activeChatDisplayUser].Add(displayMessage);

                     // Smartly update console without disrupting user input too much
                     int originalCursorTop = Console.CursorTop;
                     int originalCursorLeft = Console.CursorLeft;
                     Console.MoveBufferArea(0, originalCursorTop, Console.BufferWidth, 1, 0, originalCursorTop +1); // scroll current line down
                     Console.SetCursorPosition(0, originalCursorTop);
                     Console.WriteLine($" * {displayMessage}");
                     Console.SetCursorPosition(originalCursorLeft, originalCursorTop +1); // +1 because writeline added a line
                     Console.Write(">> "); // Re-display prompt part
                }
                else // Notification for message not in the current chat
                {
                    Console.WriteLine($"\n[Новое сообщение от {message.SenderId.ToString()}]: {message.EncryptedContent}");
                    Console.Write(CurrentChatUser == null ? "/> " : ">> "); // Re-display prompt
                }
            });
            
            _hubConnection.On<UserMessageResponse>("MessageSent", (message) =>
            {
                // Optional: Handle confirmation that message was sent successfully by the server
                // For console, maybe a subtle notification or log if verbose mode is on.
                // Example: Console.WriteLine($"[Система] Сообщение для {message.RecipientUsername} доставлено на сервер.");
            });

            _hubConnection.On<string>("FriendRequestReceived", (message) => // Assuming message is a simple string notification
            {
                Console.WriteLine($"\n[Система] Новый запрос в друзья: {message}");
                Console.Write(CurrentChatUser == null ? "/> " : ">> ");
            });

            _hubConnection.On<string>("FriendRequestAccepted", (message) =>
            {
                Console.WriteLine($"\n[Система] Ваш запрос в друзья принят: {message}");
                Console.Write(CurrentChatUser == null ? "/> " : ">> ");
            });

            _hubConnection.On<string>("FriendRemoved", (message) =>
            {
                Console.WriteLine($"\n[Система] Пользователь удалил вас из друзей или вы удалили его: {message}");
                Console.Write(CurrentChatUser == null ? "/> " : ">> ");
            });

            _hubConnection.On<string>("UserBlocked", (message) =>
            {
                Console.WriteLine($"\n[Система] Пользователь заблокирован: {message}");
                Console.Write(CurrentChatUser == null ? "/> " : ">> ");
            });
            _hubConnection.On<string>("UserUnblocked", (message) =>
            {
                Console.WriteLine($"\n[Система] Пользователь разблокирован: {message}");
                 Console.Write(CurrentChatUser == null ? "/> " : ">> ");
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
            if (!handler.CanReadToken(AuthToken)) return Guid.Empty;
            var jwtToken = handler.ReadJwtToken(AuthToken);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "userId" || claim.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
            return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) ? userId : Guid.Empty;
        }


        static void ShowChat(string user)
        {
            Console.Clear();
            Console.WriteLine($"/*====================================");
            Console.WriteLine($" * Личные сообщения | {user} ({CurrentChatUserId})");
            Console.WriteLine($" * (/back для выхода из чата)");
            Console.WriteLine($" *====================================");

            if (!ChatHistory.ContainsKey(user))
            {
                ChatHistory[user] = new List<string>();
                // Potentially load chat history here via an API call if desired
                // ChatHistory[user].Add($"[Система] Начало чата с {user}. Введите сообщение или /back.");
            }
            else
            {
                foreach (var msg in ChatHistory[user])
                    Console.WriteLine(" * " + msg);
            }
            Console.WriteLine(" *------------------------------------");
        }

        static async Task SendMessageAsync(string messageContent)
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
                EncryptedContent = messageContent,
                RecipientType = RecipientType.User
            };

            try
            {
                await _hubConnection.InvokeAsync("Send", messageRequest);
                
                var displayMessage = $"[{DateTime.Now:HH:mm}] Ты >> {messageContent}";
                if (!ChatHistory.ContainsKey(CurrentChatUser!))
                {
                    ChatHistory[CurrentChatUser!] = new List<string>();
                }
                ChatHistory[CurrentChatUser!].Add(displayMessage);
                // No Console.WriteLine here, message will be echoed by "ReceiveMessage" if server sends it to self,
                // or we rely on user seeing their own typed message.
                // The ReceiveMessage handler should ideally handle self-messages for consistent display.
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
            Console.WriteLine("  * /help - список команд");
        }
    }
}

// LoginResponse class was here, can be removed if not used elsewhere or moved to a Contracts project.
// Assuming it's implicitly handled by HttpClientService or was specific to the old Program.cs structure.
// public class LoginResponse
// {
//    public string token { get; set; }
// }