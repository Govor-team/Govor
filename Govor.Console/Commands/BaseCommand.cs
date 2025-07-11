using System;
using System.Threading.Tasks;

namespace Govor.ConsoleClient.Commands
{
    public abstract class BaseCommand : ICommand
    {
        protected static FriendsClient? FriendsClient { get; private set; }
        protected static HttpClientService HttpClientService { get; private set; }
        protected static Func<string?> GetAuthToken { get; private set; }
        protected static Action<string> SetAuthToken { get; private set; }
        protected static Func<Task> InitializeHubConnectionAsync { get; private set; }
        protected static Microsoft.AspNetCore.SignalR.Client.HubConnection? HubConnection => _hubConnection?.Invoke();
        private static Func<Microsoft.AspNetCore.SignalR.Client.HubConnection?> _hubConnection;


        public static void InitializeServices(
            FriendsClient? friendsClient,
            HttpClientService httpClientService,
            Func<string?> getAuthToken,
            Action<string> setAuthToken,
            Func<Task> initializeHubConnectionAsync,
            Func<Microsoft.AspNetCore.SignalR.Client.HubConnection?> hubConnection)
        {
            FriendsClient = friendsClient;
            HttpClientService = httpClientService;
            GetAuthToken = getAuthToken;
            SetAuthToken = setAuthToken;
            InitializeHubConnectionAsync = initializeHubConnectionAsync;
            _hubConnection = hubConnection;
        }

        public abstract Task ExecuteAsync(string? argument);
        public abstract string GetHelp();

        protected boolEnsureLoggedIn()
        {
            if (GetAuthToken() == null)
            {
                System.Console.WriteLine("[Ошибка] Сначала войдите в систему. Используйте /login или /reg.");
                return false;
            }
            if (FriendsClient == null && !(this is LoginCommand || this is RegisterCommand)) // FriendsClient might not be needed for auth commands
            {
                 // Attempt to re-initialize FriendsClient if token exists but client is null
                // This can happen if the app was closed and token was persisted, but client was not re-instantiated.
                // However, the current setup in Program.cs initializes FriendsClient after login/reg.
                // This check is more of a safeguard.
                 System.Console.WriteLine("[Ошибка] Клиент для работы с друзьями не инициализирован. Попробуйте войти снова.");
                 return false;
            }
            return true;
        }

        protected boolEnsureHubConnection()
        {
            if (HubConnection == null || HubConnection.State != Microsoft.AspNetCore.SignalR.Client.HubConnectionState.Connected)
            {
                System.Console.WriteLine("[Ошибка] SignalR соединение не установлено. Попробуйте войти снова или проверьте соединение.");
                return false;
            }
            return true;
        }
    }
}
