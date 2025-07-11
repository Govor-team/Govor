using System;
using System.Threading.Tasks;

namespace Govor.ConsoleClient.Commands
{
    public class RejectFriendRequestCommand : BaseCommand
    {
        public override async Task ExecuteAsync(string? argument)
        {
            if (!EnsureLoggedIn() || !EnsureHubConnection()) return;

            Guid friendshipId;
            if (string.IsNullOrWhiteSpace(argument) || !Guid.TryParse(argument, out friendshipId))
            {
                Console.Write("Введите ID заявки, которую хотите отклонить: ");
                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input) || !Guid.TryParse(input, out friendshipId))
                {
                    Console.WriteLine("[Ошибка] Неверный или пустой ID заявки.");
                    return;
                }
            }

            try
            {
                // API uses Hub for this: RejectFriendRequest(Guid friendshipId)
                // Located in Govor.API/Hubs/FriendsHub.cs
                await HubConnection.InvokeAsync("RejectFriendRequest", friendshipId);
                Console.WriteLine("Заявка в друзья отклонена.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Ошибка отклонения заявки] {ex.Message}");
            }
        }

        public override string GetHelp()
        {
            return "/reject [ID_заявки] - Отклонить входящую заявку в друзья. Если ID не указан, запросит ввод.";
        }
    }
}
