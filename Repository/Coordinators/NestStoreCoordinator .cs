namespace Repository
{
    public class NestStoreCoordinator : INestDatabase
    {
        private readonly INestDatabase store;

        public NestStoreCoordinator(Harbor.Flag flag)
        {
            store = new EFCoreNestStore(flag);
        }
        
        public async Task FollowUserAsync(long selfId, long targetId, DateTimeOffset time) 
        {
            await store.FollowUserAsync(selfId, targetId, time);
        }
        public async Task UnfollowUserAsync(long selfId, long targetId) 
        {
            await store.UnfollowUserAsync(selfId, targetId);
        }
        public async Task BlockUserAsync(long selfId, long targetId, DateTimeOffset time) 
        {
            await store.BlockUserAsync(selfId, targetId, time);
        }
        public async Task UnblockUserAsync(long selfId, long targetId) 
        {
            await store.UnblockUserAsync(selfId, targetId);
        }

        public async Task<List<CoreUser>> GetFollowedUsersAsync(long id) 
        {
            return await store.GetFollowedUsersAsync(id);
        }
        public async Task<List<BlockedUserShard>> GetBlockedUsersAsync(long id) 
        {
            return await store.GetBlockedUsersAsync(id);
        }
        public async Task<List<CoreUser>> GetCompanionsAsync(long id)
        {
            return await store.GetCompanionsAsync(id);
        }

        public async Task<List<CoreUser>> GetUserFollowersAsync(long userId)
        {
            return await store.GetUserFollowersAsync(userId);
        }

        public async Task<List<CoreUser>> GetUsersBlockingAsync(long userId)
        {
           return await store.GetUsersBlockingAsync(userId);
        }
        public async Task<bool> HaveMutualGathering(long userId, long targetId)
        {
            return await store.HaveMutualGathering(userId, targetId);
        }

        public async Task<CoreGathering> GetFirstMutualGathering(long userId, long targetId)
        {
            return await store.GetFirstMutualGathering(userId, targetId);
        }

        public async Task<CoreGathering> GetLatestMutualGathering(long userId, long targetId)
        {
            return await store.GetLatestMutualGathering(userId, targetId);
        }

        public async Task<DateTimeOffset> BlockedSince(long userId, long targetId)
        {
            return await store.BlockedSince(userId, targetId);
        }

        public async Task<List<long>> ReturnStrangerDangerAsync(long userId, params long[] users)
        {
            return await store.ReturnStrangerDangerAsync(userId, users);
        }

        public async Task<List<CompanionshipRequestShard>> GetIncomingRequestsAsync(long userId)
        {
            return await store.GetIncomingRequestsAsync(userId);
        }

        public async Task<List<CompanionshipRequestShard>> GetOutgoingRequestsAsync(long userId)
        {
            return await store.GetOutgoingRequestsAsync(userId);
        }

        public async Task<List<CoreUser>> GetRecentlyMetAsync(long userId)
        {
            return await store.GetRecentlyMetAsync(userId);
        }
    }
}
