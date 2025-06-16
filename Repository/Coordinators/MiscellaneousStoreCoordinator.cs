namespace Repository
{
    internal class MiscellaneousStoreCoordinator : IMiscellaneousDatabase
    {
        private readonly IMiscellaneousDatabase store;

        public MiscellaneousStoreCoordinator(Harbor.Flag flag)
        {
            store = new EFCoreMiscellaneousStore(flag);
        }

        public async Task SaveFeedbackAsync(string comments, DateTimeOffset time)
        {
            await store.SaveFeedbackAsync(comments, time);
        }

        public async Task SaveFeedbackAsync(string comments, DateTimeOffset time, long userId)
        {
            await store.SaveFeedbackAsync(comments, time, userId);
        }
    }
}
