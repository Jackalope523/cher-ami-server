using Microsoft.EntityFrameworkCore;

namespace Repository
{
    internal class EFCoreMiscellaneousStore : QueryStore, IMiscellaneousDatabase
    {
        internal EFCoreMiscellaneousStore(Func<CanaryContext> contextFactory) : base(contextFactory)
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
