namespace Repository
{
    public class SnapshotStoreCoordinator : ISegmentDatabase
    {
        private readonly ISegmentDatabase store;

        public SnapshotStoreCoordinator(Harbor.Flag flag)
        {
            store = new EFCoreSnapshotStore(flag);
        }

        public async Task<PostShard> AddPostAsync(long gatheringId, long posterId, DateTimeOffset timePosted)
        { 
             return await store.AddPostAsync(gatheringId, posterId, timePosted);  
        }

        public async Task<List<PostShard>> GenerateColumnForUserAsync(long id, DateTimeOffset depthCharge, DateTimeOffset lastDepthCharge)
        {
           return await store.GenerateColumnForUserAsync(id, depthCharge, lastDepthCharge);   
        }

        public async Task<PostShard> GetPostAsync(long id)
        {
            return await store.GetPostAsync(id);
        }

        public async Task<List<PostShard>> GetPostsByUserAsync(long id)
        {
            return await store.GetPostsByUserAsync(id);
        }

        public async Task AcclaimSnapshotAsync(long postId, long voterId)
        {           
          await store.AcclaimSnapshotAsync(postId, voterId);
        }

        public async Task<List<PostShard>> GetPostsForSegmentAsync(long id)
        {
            return await store.GetPostsForSegmentAsync(id);
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
