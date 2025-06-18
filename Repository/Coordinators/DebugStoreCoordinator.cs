namespace Repository
{
    public class DebugStoreCoordinator: IDebugDatabase
    {
        private readonly  IDebugDatabase store;

        public DebugStoreCoordinator(Harbor.Flag flag)
        {
            store = new EFCoreDebugStore(flag);
        }

        public async Task DrainDatabaseAsync()
        {
            await store.DrainDatabaseAsync();
        }

        public async Task VoidUserAsync(long userId)
        {
            await store.VoidUserAsync(userId);
        }
    }
}
