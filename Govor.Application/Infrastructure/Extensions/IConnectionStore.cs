namespace Govor.Application.Infrastructure.Extensions;

public interface IConnectionStore
{
    void AddConnection(Guid userId, string connectionId);
    void RemoveConnection(Guid userId, string connectionId);
    IEnumerable<string> GetConnections(Guid userId);
}