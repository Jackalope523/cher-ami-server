namespace Repository
{
    class ConnectionStoreCoordinator : IConnectionDatabase
    {
        private readonly IConnectionDatabase store;

        public ConnectionStoreCoordinator(Harbor.Flag flag)
        {
            store = new EFCoreConnectionStore(flag);
        }

        public async Task AddConnectionAsync(long userId, string connectionId)
        {
            await store.AddConnectionAsync(userId, connectionId);
        }

        public async Task DeleteConnectionAsync(string connectionId)
        {
            await store.DeleteConnectionAsync(connectionId);
        }

        public async Task<List<string>> GetConnectionsAsync(long userId)
        {
            return await store.GetConnectionsAsync(userId);
        }

        public async Task<Dictionary<long, List<string>>> GetConnectionsAsync(params long[] userIds)
        {
            return await store.GetConnectionsAsync(userIds);
        }
    }
}
