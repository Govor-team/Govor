using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Govor.ConsoleClient.Commands
{
    public class RegisterCommand : BaseCommand
    {
        public override async Task ExecuteAsync(string? argument)
        {
            Console.Write("username: ");
            var regUsername = Console.ReadLine();

            Console.Write("password: ");
            var regPassword = Console.ReadLine();

            Console.Write("invitation code: ");
            var inviteCode = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(regUsername) || string.IsNullOrWhiteSpace(regPassword) || string.IsNullOrWhiteSpace(inviteCode))
            {
                Console.WriteLine("[Ошибка] Имя пользователя, пароль и код приглашения не могут быть пустыми.");
                return;
            }

            try
            {
                var authToken = await HttpClientService.RegisterAsync(regUsername, regPassword, inviteCode);
                SetAuthToken(authToken);

                // Initialize FriendsClient after successful registration
                var sharedClient = new HttpClient { BaseAddress = new Uri(HttpClientService.GetBaseUrl()) };
                sharedClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
                var friendsClient = new FriendsClient(sharedClient);

                // Re-initialize services in BaseCommand with the new FriendsClient
                Program.UpdateFriendsClient(friendsClient); // <-- единственный нужный вызов

                await InitializeHubConnectionAsync();
                Console.WriteLine("[Успех] Регистрация завершена. Токен сохранен.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Ошибка] {ex.Message}");
            }
        }

        public override string GetHelp()
        {
            return "/reg - Зарегистрировать новый аккаунт с помощью кода приглашения.";
        }
    }
}
