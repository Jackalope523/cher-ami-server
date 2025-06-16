using Core.Boundaries;

namespace Repository
{
    public class AdminStoreCoordinator : IAdminDatabase
    {
        private readonly IAdminDatabase store;

        public AdminStoreCoordinator(Harbor.Flag flag)
        {
            store = new EFCoreAdminStore(flag);
        }

        public async Task<List<CoreGathering>> GetAllActiveGatheringsAsync(DateTimeOffset currentTime)
        {
            return await store.GetAllActiveGatheringsAsync(currentTime);
        }

        public async Task VoidGatheringAsync(long gatheringId)
        {
            await store.VoidGatheringAsync(gatheringId);
        }

        public async Task VoidUserAsync(long userId)
        {
           await store.VoidUserAsync(userId);
        }
    }
}
