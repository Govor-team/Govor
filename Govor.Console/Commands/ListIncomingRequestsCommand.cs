using System;
using System.Linq;
using System.Threading.Tasks;

namespace Govor.ConsoleClient.Commands
{
    public class ListIncomingRequestsCommand : BaseCommand
    {
        public override async Task ExecuteAsync(string? argument)
        {
            if (!EnsureLoggedIn()) return;

            try
            {
                // This still uses REST client as per existing FriendsClient
                var requests = await FriendsClient.GetIncomingRequestsAsync();
                if (requests.Any())
                {
                    Console.WriteLine("Входящие заявки в друзья:");
                    foreach (var r in requests)
                    {
                        // Assuming you want to show who sent the request.
                        // The FriendshipDto contains RequesterId and AddresseeId.
                        // If current user is AddresseeId, then RequesterId is the one who sent it.
                        Console.WriteLine($"- Запрос ID: {r.Id}. От пользователя ID: {r.RequesterId}. Статус: {r.Status}. (принять через /accept {r.Id})");
                    }
                }
                else
                {
                    Console.WriteLine("У вас нет входящих заявок в друзья.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Ошибка] {ex.Message}");
            }
        }

        public override string GetHelp()
        {
            return "/incoming - Показать список входящих заявок в друзья.";
        }
    }
}
