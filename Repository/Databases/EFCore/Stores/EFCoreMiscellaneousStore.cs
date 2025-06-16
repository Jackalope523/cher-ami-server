namespace Repository
{
    internal class EFCoreMiscellaneousStore : QueryStore, IMiscellaneousDatabase
    {
        public EFCoreMiscellaneousStore(Harbor.Flag flag) : base(flag)
        {
        }

        public async Task SaveFeedbackAsync(string comments, DateTimeOffset time)
        {
            await storeSentry.ExecuteWriteAsync(ctx => ctx.Feedback.Add(new() {Comments = comments, Time = time }));
        }

        public async Task SaveFeedbackAsync(string comments, DateTimeOffset time, long userId)
        {
            await storeSentry.ExecuteWriteAsync(ctx => ctx.Feedback.Add(new() { Comments = comments, Time = time, UserId = userId }));
        }
    }
}
