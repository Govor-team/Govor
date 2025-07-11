using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace Govor.ConsoleClient.Commands
{
    public class BlockUserCommand : BaseCommand
    {
        public override async Task ExecuteAsync(string? argument)
        {
            if (!EnsureLoggedIn() || !EnsureHubConnection()) return;

            Guid targetUserId;
            if (string.IsNullOrWhiteSpace(argument) || !Guid.TryParse(argument, out targetUserId))
            {
                Console.Write("Введите ID пользователя, которого хотите заблокировать: ");
                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input) || !Guid.TryParse(input, out targetUserId))
                {
                    Console.WriteLine("[Ошибка] Неверный или пустой ID пользователя.");
                    return;
                }
            }

            try
            {
                // API uses Hub for this: BlockUser(Guid targetUserId)
                // Located in Govor.API/Hubs/FriendsHub.cs
                await HubConnection.InvokeAsync("BlockUser", targetUserId);
                Console.WriteLine($"Пользователь {targetUserId} заблокирован.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Ошибка блокировки пользователя] {ex.Message}");
            }
        }

        public override string GetHelp()
        {
            return "/block [ID_пользователя] - Заблокировать пользователя. Если ID не указан, запросит ввод.";
        }
    }
}
