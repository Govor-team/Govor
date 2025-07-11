using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Govor.Contracts.DTOs; // Required for FriendshipDto

namespace Govor.ConsoleClient.Commands
{
    public class ListOutgoingRequestsCommand : BaseCommand
    {
        public override async Task ExecuteAsync(string? argument)
        {
            if (!EnsureLoggedIn()) return;

            try
            {
                // This functionality maps to GET api/friends/responses in FriendsRequestQueryController
                // FriendsClient doesn't have a dedicated method, so we use HttpClientService or direct HttpClient
                var response = await HttpClientService.GetAsync("api/friends/responses");
                response.EnsureSuccessStatusCode();

                var requests = await response.Content.ReadFromJsonAsync<System.Collections.Generic.List<FriendshipDto>>();

                if (requests != null && requests.Any())
                {
                    Console.WriteLine("Отправленные вами заявки в друзья (ожидают ответа):");
                    foreach (var r in requests)
                    {
                        // If current user is RequesterId, then AddresseeId is the one they sent to.
                        Console.WriteLine($"- Запрос ID: {r.Id}. Пользователю ID: {r.AddresseeId}. Статус: {r.Status}.");
                    }
                }
                else
                {
                    Console.WriteLine("У вас нет отправленных заявок, ожидающих ответа.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Ошибка] {ex.Message}");
            }
        }

        public override string GetHelp()
        {
            return "/outgoing - Показать список отправленных вами заявок в друзья, ожидающих ответа.";
        }
    }
}
