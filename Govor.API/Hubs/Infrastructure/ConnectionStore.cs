using System.Collections.Concurrent;
using Govor.Application.Infrastructure.Extensions;

namespace Govor.API.Hubs.Infrastructure;

public class ConnectionStore : IConnectionStore
{
    private readonly ConcurrentDictionary<Guid, HashSet<string>> _connections = new();

    public void AddConnection(Guid userId, string connectionId)
    {
        var conns = _connections.GetOrAdd(userId, _ => new HashSet<string>());
        lock (conns) conns.Add(connectionId);
    }

    public void RemoveConnection(Guid userId, string connectionId)
    {
        if (_connections.TryGetValue(userId, out var conns))
        {
            lock (conns)
            {
                conns.Remove(connectionId);
                if (!conns.Any()) _connections.TryRemove(userId, out _);
            }
        }
    }

    public IEnumerable<string> GetConnections(Guid userId)
    {
        return _connections.TryGetValue(userId, out var conns) ? conns.ToList() : Enumerable.Empty<string>();
    }
}