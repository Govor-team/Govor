using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Govor.ConsoleClient.Commands
{
    public class LoginCommand : BaseCommand
    {
        public override async Task ExecuteAsync(string? argument)
        {
            Console.Write("username: ");
            var loginUsername = Console.ReadLine();

            Console.Write("password: ");
            var loginPassword = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(loginUsername) || string.IsNullOrWhiteSpace(loginPassword))
            {
                Console.WriteLine("[Ошибка] Имя пользователя и пароль не могут быть пустыми.");
                return;
            }

            try
            {
                var authToken = await HttpClientService.LoginAsync(loginUsername, loginPassword);
                SetAuthToken(authToken);

                // Initialize FriendsClient after successful login
                var sharedClient = new HttpClient { BaseAddress = new Uri(HttpClientService.GetBaseUrl()) };
                sharedClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
                var friendsClient = new FriendsClient(sharedClient);

                
                
                Program.UpdateFriendsClient(friendsClient); // <-- единственный нужный вызов

                await InitializeHubConnectionAsync();
                Console.WriteLine("[Успех] Вход выполнен. Токен сохранен.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Ошибка] {ex.Message}");
            }
        }

        public override string GetHelp()
        {
            return "/login - Войти в существующий аккаунт.";
        }
    }
}
