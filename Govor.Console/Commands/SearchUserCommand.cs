using System;
using System.Linq;
using System.Threading.Tasks;

namespace Govor.ConsoleClient.Commands
{
    public class SearchUserCommand : BaseCommand
    {
        public override async Task ExecuteAsync(string? argument)
        {
            if (!EnsureLoggedIn()) return;

            if (string.IsNullOrWhiteSpace(argument))
            {
                Console.Write("Введите имя пользователя для поиска: ");
                argument = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(argument))
                {
                    Console.WriteLine("[Ошибка] Поисковый запрос не может быть пустым.");
                    return;
                }
            }

            try
            {
                var foundUsers = await FriendsClient.SearchAsync(argument);
                if (foundUsers.Any())
                {
                    Console.WriteLine("Найденные пользователи:");
                    foreach (var user in foundUsers)
                    {
                        Console.WriteLine($"- {user.Username} [{user.Id}]");
                    }
                }
                else
                {
                    Console.WriteLine("Пользователи не найдены.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Ошибка] {ex.Message}");
            }
        }

        public override string GetHelp()
        {
            return "/search [имя_пользователя] - Найти пользователей по имени. Если имя не указано, запросит ввод.";
        }
    }
}
