using System;
using System.Threading.Tasks;

namespace Govor.ConsoleClient.Commands
{
    public class AcceptFriendRequestCommand : BaseCommand
    {
        public override async Task ExecuteAsync(string? argument)
        {
            if (!EnsureLoggedIn() || !EnsureHubConnection()) return;

            Guid friendshipId;
            if (string.IsNullOrWhiteSpace(argument) || !Guid.TryParse(argument, out friendshipId))
            {
                Console.Write("Введите ID заявки, которую хотите принять: ");
                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input) || !Guid.TryParse(input, out friendshipId))
                {
                    Console.WriteLine("[Ошибка] Неверный или пустой ID заявки.");
                    return;
                }
            }

            try
            {
                // API uses Hub for this: AcceptFriendRequest(Guid friendshipId)
                await HubConnection.InvokeAsync("AcceptFriendRequest", friendshipId);
                Console.WriteLine("Заявка в друзья принята.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Ошибка принятия заявки] {ex.Message}");
            }
        }

        public override string GetHelp()
        {
            return "/accept [ID_заявки] - Принять входящую заявку в друзья. Если ID не указан, запросит ввод.";
        }
    }
}
