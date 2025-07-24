using Microsoft.EntityFrameworkCore;
using Repository.Databases.Contexts;
using Repository.Databases.Entities;

namespace Repository.Databases.Stores
{
    class ConnectionRepository : Repository, IConnectionDatabase
    {
        internal ConnectionRepository(Func<CardinalContext> contextFactory) : base(contextFactory)
        {
        }

        public async Task AddConnectionAsync(long userId, string connectionId)
        {
            await using CardinalContext ctx = initContext();

            Connection toAdd = new() { UserId = userId, ConnectionId = connectionId};
            ctx.Connections.Add(toAdd);
            await ctx.SaveChangesAsync();
        }

        public async Task DeleteConnectionAsync(string connectionId)
        {
            await using CardinalContext ctx = initContext();

            await ctx.Connections.
            Where(c => c.ConnectionId == connectionId).
            ExecuteUpdateAsync(setter => setter.SetProperty(s => s.SoftDeleted, true));
        }

        public async Task<List<string>> GetConnectionsAsync(long userId)
        {
            await using CardinalContext ctx = initContext();

            return await ctx.Connections.
                         Where(c => c.UserId == userId).
                         Select(c => c.ConnectionId).
                         ToListAsync();
        }

        public async Task<Dictionary<long, List<string>>> GetConnectionsAsync(params long[] userIds)
        {
            await using CardinalContext ctx = initContext();

            List<Connection> connections = await ctx.Connections.
                                                 Where(c => userIds.Contains(c.UserId)).
                                                 ToListAsync();

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
