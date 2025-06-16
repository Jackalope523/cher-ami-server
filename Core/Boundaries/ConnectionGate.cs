using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Boundaries
{
    public interface IConnectionDatabase
    {
        Task<List<string>> GetConnectionsAsync(long userId);
        Task<Dictionary<long, List<string>>> GetConnectionsAsync(params long[] userIds);
        Task AddConnectionAsync(long userId, string connectionId);
        Task DeleteConnectionAsync(string connectionId);
    }

    public interface IConnectionOperations
    {
        Task UserConnectedAsync(long userId, string connectionId);
        Task UserDisconnectedAsync(long userId, string connectionId);
    }

    public interface ISocketService
    {
        Task BroadcastAsync(Func<IClientSocket, Task> operation, params string[] connectionIds);
    }

    public interface IClientSocket : IMessageSocket
    { }
}

