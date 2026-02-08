using System.Collections.Concurrent;
using Govor.Application.Interfaces.UserOnlineStatus;

namespace Govor.Application.Services.UserOnlineStatus;

public class OnlineUserStore : IOnlineUserStore
{
    private readonly ConcurrentDictionary<Guid, HashSet<string>> _userConnections = new();

    public bool AddConnection(Guid userId, string connectionId)
    {
        bool isFirstConnection = false;

        _userConnections.AddOrUpdate(userId, 
            _ => {
                isFirstConnection = true;
                return new HashSet<string> { connectionId };
            },
            (_, connections) => {
                lock (connections)
                {
                    if (connections.Count == 0) isFirstConnection = true;
                    connections.Add(connectionId);
                }
                return connections;
            });

        return isFirstConnection;
    }

    public bool RemoveConnection(Guid userId, string connectionId)
    {
        bool isLastConnection = false;

        if (_userConnections.TryGetValue(userId, out var connections))
        {
            lock (connections)
            {
                connections.Remove(connectionId);
                if (connections.Count == 0)
                {
                    isLastConnection = true;
                    _userConnections.TryRemove(userId, out _);
                }
            }
        }

        return isLastConnection;
    }

    public IEnumerable<string> GetConnections(Guid userId)
    {
        if (_userConnections.TryGetValue(userId, out var connections))
        {
            lock (connections)
            {
                return connections.ToList();
            }
        }
        return Enumerable.Empty<string>();
    }

    public bool IsOnline(Guid userId)
    {
        return _userConnections.TryGetValue(userId, out var connections) && connections.Count > 0;
    }

    public IReadOnlyCollection<Guid> GetAllOnlineUsers()
    {
        return _userConnections.Keys.ToList();
    }
}