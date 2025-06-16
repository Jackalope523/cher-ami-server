using Core.Boundaries;


namespace Repository
{
    public class GatheringStoreCoordinator : IGatheringDatabase
    {
        private readonly IGatheringDatabase store;

        public GatheringStoreCoordinator(Harbor.Flag flag)
        {
            store = new EFCoreGatheringStore(flag);
        }

        public async Task<CoreGathering> CreateGatheringAsync(long hostId, string name, string description, DateTimeOffset startTime, double latitude, double longitude, string friendlyLocation, int groupMinimum, int groupMaximum, CharacterShard character, double Radius, bool isDynamic, int degreeOfPrivacy, DateTimeOffset timeOfCreation)
        {
            return await store.CreateGatheringAsync(hostId, name, description, startTime, latitude, longitude, friendlyLocation, groupMinimum, groupMaximum, character, Radius, isDynamic, degreeOfPrivacy, timeOfCreation);
        }

        public async Task<List<CoreGathering>> FindOngoingGatheringsForUserAsync(long id, DateTimeOffset currentTime) 
        {
            return await store.FindOngoingGatheringsForUserAsync(id, currentTime);
        }

        public async Task<List<CoreGathering>> FindUpcomingGatheringsForUserAsync(long id, DateTimeOffset currentTime) 
        {
            return await store.FindUpcomingGatheringsForUserAsync(id, currentTime);
        }

        public async Task<List<CoreGathering>> FindPastGatheringsForUserAsync(long id)
        {
            return await store.FindPastGatheringsForUserAsync(id);
        }

        public async Task<CoreGathering> FindGatheringAsync(long id)
        {
            return await store.FindGatheringAsync(id);
        }

        public async Task<List<CoreGathering>> FindGatheringsAsync(double latitude, double longitude, double distance)
        {
            return await store.FindGatheringsAsync(latitude, longitude, distance);
        }       
        
        public async Task DeleteUserStateAsync(long userId, long gatheringId) 
        { 
            await store.DeleteUserStateAsync(userId, gatheringId);
        }

        public async Task UpdateGatheringAsync(long id, List<(string Property, object Value)> edits)
        {
            await store.UpdateGatheringAsync(id, edits); 
        }   

        public async Task<List<(long UserId, DateTimeOffset Joined, DateTimeOffset? Left)>> GetGuestHistoryAsync(long id)
        {
            return await store.GetGuestHistoryAsync(id);
        }   
        
        public async Task<List<CoreGathering>> FindGatheringsByUserAsync(long userId)
        {
            return await store.FindGatheringsByUserAsync(userId);
        }  
        
        public async Task<GatheringBond?> GetUserStateAsync(long userId, long gatheringId)
        {
            return await store.GetUserStateAsync(userId, gatheringId);
        }

        public async Task SetUserStateAsync(long userId, long gatheringId, GatheringBond userState, DateTimeOffset time)
        {
            await store.SetUserStateAsync(userId, gatheringId, userState, time);
        }

        public async Task<List<(long UserId, GatheringBond State)>> GetAllUsersAsync(long gatheringId)
        {
            return await store.GetAllUsersAsync(gatheringId);
        }

        public async Task TerminateGatheringAsync(long id, DateTimeOffset time)
        {
            await store.TerminateGatheringAsync(id, time);
        }

        public async Task<bool> UserIsAuthorizedGuest(long userId, long gatheringId)
        {
            return await store.UserIsAuthorizedGuest(userId, gatheringId);
        }

        public async Task<List<long>> GetAuthorizedGuests(long gatheringId)
        {
            return await store.GetAuthorizedGuests(gatheringId);
        }

        public async Task AddGuestAuthorization(long gatheringId, long userId)
        {
            await store.AddGuestAuthorization(gatheringId, userId);
        }

        public async Task CancelGatheringAsync(long gatheringId)
        {
            await store.CancelGatheringAsync(gatheringId);
        }

        public async Task SoftDeleteAsync(long gatheringId)
        {
            await store.SoftDeleteAsync(gatheringId);
        }

        public async Task HardDeleteAsync(long gatheringId)
        {
            await store.HardDeleteAsync(gatheringId);
        }
    }
}

