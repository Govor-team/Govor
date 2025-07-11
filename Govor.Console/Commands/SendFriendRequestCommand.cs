using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace Govor.ConsoleClient.Commands
{
    public class SendFriendRequestCommand : BaseCommand
    {
        public override async Task ExecuteAsync(string? argument)
        {
            if (!EnsureLoggedIn() || !EnsureHubConnection()) return;

            Guid targetUserId;
            if (string.IsNullOrWhiteSpace(argument) || !Guid.TryParse(argument, out targetUserId))
            {
                Console.Write("Введите ID пользователя, которому хотите отправить запрос: ");
                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input) || !Guid.TryParse(input, out targetUserId))
                {
                    Console.WriteLine("[Ошибка] Неверный или пустой ID пользователя.");
                    return;
                }
            }

            try
            {
                // API uses Hub for this: SendFriendRequest(Guid targetUserId)
                await HubConnection.InvokeAsync("SendRequest", targetUserId);
                Console.WriteLine("Запрос в друзья отправлен.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Ошибка отправки запроса] {ex.Message}");
            }
        }

        public override string GetHelp()
        {
            return "/friend [ID_пользователя] - Отправить запрос в друзья пользователю с указанным ID. Если ID не указан, запросит ввод.";
        }
    }
}
