using Core.Boundaries;


namespace Repository
{
    public class NotificationStoreCoordinator : INotificationDatabase
    {
        private readonly INotificationDatabase store;
        public NotificationStoreCoordinator(Harbor.Flag flag)
        {
            store = new EFCoreNotificationStore(flag);
        }

        public async Task<NotificationProfile> GetNotificationProfileAsync(long userId)
        {
            return await store.GetNotificationProfileAsync(userId);
        }

        public async Task UpdateNotificationProfileAsync(long userId, List<(string Property, object Value)> edits)
        {
            await store.UpdateNotificationProfileAsync(userId, edits);
        }

        public async Task<(HostNotificationSchedule, List<GuestNotificationSchedule>)> GetGatheringNotificationScheduleAsync(long gatheringId)
        {
            return await store.GetGatheringNotificationScheduleAsync(gatheringId);
        }

        public async Task UpdateGatheringHostNotificationScheduleAsync(long gatheringId, string gatheringWaitingId)
        {
            await store.UpdateGatheringHostNotificationScheduleAsync(gatheringId, gatheringWaitingId);
        }

        public async Task UpdateGatheringGuestNotificationSchedulesAsync(long gatheringId, params (long userId, string gatheringUpcomingId, string gatheringImminentId)[] guestSchedules)
        {
            await store.UpdateGatheringGuestNotificationSchedulesAsync(gatheringId, guestSchedules);
        }

        public async Task ClearGatheringNotificationScheduleAsync(long gatheringId)
        {
            await store.ClearGatheringNotificationScheduleAsync(gatheringId);
        }
    }
}
