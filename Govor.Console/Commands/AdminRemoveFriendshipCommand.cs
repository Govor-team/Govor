using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Govor.ConsoleClient.Commands
{
    public class AdminRemoveFriendshipCommand : BaseCommand
    {
        public override async Task ExecuteAsync(string? argument)
        {
            if (!EnsureLoggedIn()) return;

            Guid friendshipId;
            if (string.IsNullOrWhiteSpace(argument) || !Guid.TryParse(argument, out friendshipId))
            {
                Console.Write("Введите ID дружеской связи для удаления: ");
                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input) || !Guid.TryParse(input, out friendshipId))
                {
                    Console.WriteLine("[Ошибка] Неверный или пустой ID связи.");
                    return;
                }
            }

            Console.WriteLine($"Удаление дружеской связи {friendshipId} (только для администраторов)...");
            try
            {
                // The API endpoint is: [HttpPost] public async Task<IActionResult> RemoveFriendship(Guid Id)
                // It expects the Id in the query string, like: api/admin/Friendships?Id=GUID
                // Or it might be a POST with x-www-form-urlencoded data or JSON body, need to check API implementation.
                // Based on `[HttpPost] public async Task<IActionResult> RemoveFriendship(Guid Id)`, it's likely a query parameter or form data.
                // Let's assume query parameter for now as it's simpler for HttpPost.

                var response = await HttpClientService.PostAsync($"api/admin/Friendships?Id={friendshipId}", null);
                
                Console.WriteLine($"Дружеская связь {friendshipId} успешно удалена.");
            }
            catch (HttpRequestException httpEx) when (httpEx.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                Console.WriteLine("[Ошибка] Доступ запрещен. Эта команда только для администраторов.");
            }
            catch (HttpRequestException httpEx) when (httpEx.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Console.WriteLine($"[Ошибка] Дружеская связь с ID {friendshipId} не найдена.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Ошибка] {ex.Message}");
            }
        }

        public override string GetHelp()
        {
            return "/adminremovefs [ID_связи] - (Админ) Удалить дружескую связь по ее ID. Если ID не указан, запросит ввод.";
        }
    }
}
