namespace Repository
{
    public class SnapshotStoreCoordinator : ISnapshotDatabase
    {
        private readonly ISnapshotDatabase store;

        public SnapshotStoreCoordinator(Harbor.Flag flag)
        {
            store = new EFCoreSnapshotStore(flag);
        }

        public async Task<SnapshotShard> AddSnapshotAsync(long gatheringId, long posterId, DateTimeOffset timePosted)
        { 
             return await store.AddSnapshotAsync(gatheringId, posterId, timePosted);  
        }

        public async Task<List<SnapshotShard>> GenerateColumnForUserAsync(long id, DateTimeOffset depthCharge, DateTimeOffset lastDepthCharge)
        {
           return await store.GenerateColumnForUserAsync(id, depthCharge, lastDepthCharge);   
        }

        public async Task<SnapshotShard> GetSnapshotAsync(long id)
        {
            return await store.GetSnapshotAsync(id);
        }

        public async Task<List<SnapshotShard>> GetSnapshotsByUserAsync(long id)
        {
            return await store.GetSnapshotsByUserAsync(id);
        }

        public async Task AcclaimSnapshotAsync(long postId, long voterId)
        {           
          await store.AcclaimSnapshotAsync(postId, voterId);
        }

        public async Task<List<SnapshotShard>> GetSnapshotsForGatheringAsync(long id)
        {
            return await store.GetSnapshotsForGatheringAsync(id);
        }

        public async Task DeleteSnapshotAcclaimAsync(long snapshotId, long voterId)
        {
            await store.DeleteSnapshotAcclaimAsync(snapshotId, voterId);
        }

        public async Task SoftDeleteAsync(long snapshotId)
        {
            await store.SoftDeleteAsync(snapshotId);
        }

        public async Task HardDeleteAsync(long snapshotId)
        {
            await store.HardDeleteAsync(snapshotId);
        }
    }
}
