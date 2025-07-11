using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace Govor.ConsoleClient.Commands
{
    public class UnblockUserCommand : BaseCommand
    {
        public override async Task ExecuteAsync(string? argument)
        {
            if (!EnsureLoggedIn() || !EnsureHubConnection()) return;

            Guid targetUserId;
            if (string.IsNullOrWhiteSpace(argument) || !Guid.TryParse(argument, out targetUserId))
            {
                Console.Write("Введите ID пользователя, которого хотите разблокировать: ");
                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input) || !Guid.TryParse(input, out targetUserId))
                {
                    Console.WriteLine("[Ошибка] Неверный или пустой ID пользователя.");
                    return;
                }
            }

            try
            {
                // API uses Hub for this: UnblockUser(Guid targetUserId)
                // Located in Govor.API/Hubs/FriendsHub.cs
                await HubConnection.InvokeAsync("UnblockUser", targetUserId);
                Console.WriteLine($"Пользователь {targetUserId} разблокирован.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Ошибка разблокировки пользователя] {ex.Message}");
            }
        }

        public override string GetHelp()
        {
            return "/unblock [ID_пользователя] - Разблокировать пользователя. Если ID не указан, запросит ввод.";
        }
    }
}
