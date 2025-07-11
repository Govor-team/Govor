using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Govor.Contracts.DTOs; // Required for FriendshipDto

namespace Govor.ConsoleClient.Commands
{
    public class AdminListUserFriendshipsCommand : BaseCommand
    {
        public override async Task ExecuteAsync(string? argument)
        {
            if (!EnsureLoggedIn()) return;

            Guid userId;
            if (string.IsNullOrWhiteSpace(argument) || !Guid.TryParse(argument, out userId))
            {
                Console.Write("Введите ID пользователя для просмотра его связей: ");
                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input) || !Guid.TryParse(input, out userId))
                {
                    Console.WriteLine("[Ошибка] Неверный или пустой ID пользователя.");
                    return;
                }
            }

            Console.WriteLine($"Получение всех связей для пользователя {userId} (только для администраторов)...");
            try
            {
                var response = await HttpClientService.GetAsync($"api/admin/Friendships/{userId}");
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    Console.WriteLine("[Ошибка] Доступ запрещен. Эта команда только для администраторов.");
                    return;
                }
                response.EnsureSuccessStatusCode();

                var friendships = await response.Content.ReadFromJsonAsync<List<FriendshipDto>>();

                if (friendships != null && friendships.Any())
                {
                    Console.WriteLine($"Дружеские связи для пользователя {userId}:");
                    foreach (var f in friendships)
                    {
                        Console.WriteLine($"- ID: {f.Id}, Пользователь1: {f.RequesterId}, Пользователь2: {f.AddresseeId}, Статус: {f.Status}");
                    }
                }
                else
                {
                    Console.WriteLine($"Дружеские связи для пользователя {userId} не найдены.");
                }
            }
            catch (HttpRequestException httpEx) when (httpEx.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                Console.WriteLine("[Ошибка] Доступ запрещен. Эта команда только для администраторов.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Ошибка] {ex.Message}");
            }
        }

        public override string GetHelp()
        {
            return "/adminlistuserfs [ID_пользователя] - (Админ) Показать все дружеские связи для указанного пользователя. Если ID не указан, запросит ввод.";
        }
    }
}
