using Govor.ConsoleClient;

internal class Program
{
    private const string HubBaseUrl = "https://govor-team-govor-88b3.twc1.net/hubs";
    private static string jwtToken = "eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySWQiOiI4NDRjNzkyMi1hNjdlLTRlMzctYWI0MC0wNThlZDE5NzM5NjMiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsImV4cCI6MTc1MjQ4MzE0MH0.ECXLPwIljdeWgWUJtBiXXQXKMFC8Iw0x7_zdjbsUe1I"; // сюда подставьте валидный JWT

    static async Task Main()
    {
        Console.Write("🔑 JWT Token: ");
        jwtToken = Console.ReadLine();

        var client = new FriendsHubClient(HubBaseUrl, jwtToken);
        await client.ConnectAsync();

        while (true)
        {
            Console.WriteLine("\n1. Send friend request");
            Console.WriteLine("2. Accept request");
            Console.WriteLine("3. Reject request");
            Console.Write("➡ Choose option: ");
            var option = Console.ReadLine();

            switch (option)
            {
                case "1":
                    Console.Write("Target UserId: ");
                    if (Guid.TryParse(Console.ReadLine(), out var targetId))
                    {
                        await client.SendFriendRequestAsync(targetId);
                    }
                    break;
                case "2":
                    Console.Write("FriendshipId to accept: ");
                    if (Guid.TryParse(Console.ReadLine(), out var acceptId))
                    {
                        await client.AcceptFriendRequestAsync(acceptId);
                    }
                    break;
                case "3":
                    Console.Write("FriendshipId to reject: ");
                    if (Guid.TryParse(Console.ReadLine(), out var rejectId))
                    {
                        await client.RejectFriendRequestAsync(rejectId);
                    }
                    break;
                default:
                    Console.WriteLine("❗ Unknown option");
                    break;
            }
        }
    }
}