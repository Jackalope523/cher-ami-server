using Microsoft.EntityFrameworkCore;

namespace Repository
{
    class EFCoreConnectionStore : QueryStore, IConnectionDatabase
    {
        public EFCoreConnectionStore(Harbor.Flag flag) : base(flag)
        {

        }

        public async Task AddConnectionAsync(long userId, string connectionId)
        {
            Connection toAdd = new() { UserId = userId, ConnectionId = connectionId};
            await storeSentry.ExecuteWriteAsync(ctx => ctx.Connections.Add(toAdd));
        }

        public async Task DeleteConnectionAsync(string connectionId)
        {
            await storeSentry.ExecuteWriteAsync(ctx =>
               ctx.Connections.
               Where(c => c.ConnectionId == connectionId).
               ExecuteUpdateAsync(setter => setter.SetProperty(s => s.SoftDeleted, true)));
        }

        public async Task<List<string>> GetConnectionsAsync(long userId)
        {
            return await storeSentry.ExecuteReadAsync(ctx => 
                    ctx.Connections.
                    Where(c => c.UserId == userId).
                    Select(c => c.ConnectionId).
                    ToListAsync());
        }

        public async Task<Dictionary<long, List<string>>> GetConnectionsAsync(params long[] userIds)
        {
            List<Connection> connections = await storeSentry.ExecuteReadAsync(ctx =>
                                                ctx.Connections.
                                                Where(c => userIds.Contains(c.UserId)).
                                                ToListAsync());

            Dictionary<long, List<string>> toReturn = new();

            foreach (Connection connection in connections)
            {
                if (toReturn.ContainsKey(connection.UserId)) toReturn[connection.UserId].Add(connection.ConnectionId);
                else toReturn[connection.UserId] = new() { connection.ConnectionId };
            }

            return toReturn;
        }
    }
}
