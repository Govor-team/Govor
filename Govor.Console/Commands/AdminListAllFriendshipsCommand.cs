using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Govor.Contracts.DTOs; // Required for FriendshipDto

namespace Govor.ConsoleClient.Commands
{
    public class AdminListAllFriendshipsCommand : BaseCommand
    {
        public override async Task ExecuteAsync(string? argument)
        {
            if (!EnsureLoggedIn()) return;
            // Consider adding an admin role check here if possible,
            // though the API itself is protected by [Authorize(Roles = "Admin")]

            Console.WriteLine("Получение всех дружеских связей (только для администраторов)...");
            try
            {
                var response = await HttpClientService.GetAsync("api/admin/Friendships");
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    Console.WriteLine("[Ошибка] Доступ запрещен. Эта команда только для администраторов.");
                    return;
                }
                response.EnsureSuccessStatusCode();

                var friendships = await response.Content.ReadFromJsonAsync<List<FriendshipDto>>();

                if (friendships != null && friendships.Any())
                {
                    Console.WriteLine("Все дружеские связи в системе:");
                    foreach (var f in friendships)
                    {
                        Console.WriteLine($"- ID: {f.Id}, Пользователь1: {f.RequesterId}, Пользователь2: {f.AddresseeId}, Статус: {f.Status}");
                    }
                }
                else
                {
                    Console.WriteLine("Дружеские связи не найдены в системе.");
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
            return "/adminlistallfs - (Админ) Показать все дружеские связи в системе.";
        }
    }
}
