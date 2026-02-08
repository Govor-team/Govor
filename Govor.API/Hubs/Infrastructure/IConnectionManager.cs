namespace Govor.API.Hubs.Infrastructure;

public interface IConnectionManager
{
    Task OnConnectedAsync(string connectionId, Guid userId);
    Task OnDisconnectedAsync(string connectionId, Guid userId);
}