using System.Collections.Concurrent;

public class OnlineUserTracker
{
    private readonly ConcurrentDictionary<string, HashSet<string>> _users
        = new();

    public void Add(string userId, string connectionId)
    {
        _users.AddOrUpdate(userId,
            _ => new HashSet<string> { connectionId },
            (_, connections) =>
            {
                lock (connections)
                {
                    connections.Add(connectionId);
                    return connections;
                }
            });
    }

    public void Remove(string userId, string connectionId)
    {
        if (!_users.TryGetValue(userId, out var connections))
            return;

        lock (connections)
        {
            connections.Remove(connectionId);

            if (connections.Count == 0)
                _users.TryRemove(userId, out _);
        }
    }

    public IReadOnlyCollection<string> GetOnlineUsers()
        => _users.Keys.ToList();
}
