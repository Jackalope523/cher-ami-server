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
    }
}
