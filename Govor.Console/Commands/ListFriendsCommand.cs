using System;
using System.Linq;
using System.Threading.Tasks;

namespace Govor.ConsoleClient.Commands
{
    public class ListFriendsCommand : BaseCommand
    {
        public override async Task ExecuteAsync(string? argument)
        {
            if (!EnsureLoggedIn()) return;

            try
            {
                var friends = await FriendsClient.GetFriendsAsync();
                if (friends.Any())
                {
                    Console.WriteLine("Ваши друзья:");
                    foreach (var f in friends)
                    {
                        Console.WriteLine($"- {f.Username} | был онлайн: {f.WasOnline} [{f.Id}]");
                    }
                }
                else
                {
                    Console.WriteLine("У вас пока нет друзей.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Ошибка] {ex.Message}");
            }
        }

        public override string GetHelp()
        {
            return "/friends - Показать список ваших друзей.";
        }
    }
}
