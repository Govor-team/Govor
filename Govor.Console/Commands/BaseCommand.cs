using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace Govor.ConsoleClient.Commands
{
    public abstract class BaseCommand : ICommand
    {
        protected static FriendsClient? FriendsClient { get; private set; }
        protected static HttpClientService HttpClientService { get; private set; } = null!;
        protected static Func<string?> GetAuthToken { get; private set; } = null!;
        protected static Action<string> SetAuthToken { get; private set; } = null!;
        protected static Func<Task> InitializeHubConnectionAsync { get; private set; } = null!;
        protected static HubConnection HubConnection => _getHubConnection?.Invoke();
        
        private static Func<HubConnection?> _getHubConnection = null!;

        public static void InitializeServices(
            FriendsClient? friendsClient,
            HttpClientService httpClientService,
            Func<string?> getAuthToken,
            Action<string> setAuthToken,
            Func<Task> initializeHubConnectionAsync,
            Func<HubConnection?> getHubConnection)
        {
            FriendsClient = friendsClient;
            HttpClientService = httpClientService;
            GetAuthToken = getAuthToken;
            SetAuthToken = setAuthToken;
            InitializeHubConnectionAsync = initializeHubConnectionAsync;
            _getHubConnection = getHubConnection;
        }

        public abstract Task ExecuteAsync(string? argument);
        public abstract string GetHelp();

        protected bool EnsureLoggedIn()
        {
            if (GetAuthToken() == null)
            {
                Console.WriteLine("[Ошибка] Сначала войдите в систему. Используйте /login или /reg.");
                return false;
            }

            if (FriendsClient == null && !(this is LoginCommand || this is RegisterCommand))
            {
                Console.WriteLine("[Ошибка] Клиент для работы с друзьями не инициализирован. Попробуйте войти снова.");
                return false;
            }

            return true;
        }

        protected bool EnsureHubConnection()
        {
            var hub = _getHubConnection?.Invoke();
            if (hub == null || hub.State != HubConnectionState.Connected)
            {
                Console.WriteLine("[Ошибка] SignalR соединение не установлено. Попробуйте войти снова или проверьте соединение.");
                return false;
            }
            return true;
        }
    }
}
