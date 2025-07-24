using Repository.Contexts;

namespace Repository.Repositories
{
    internal class MiscellaneousRepository : Repository, IMiscellaneousDatabase
    {
        internal MiscellaneousRepository(Func<CardinalContext> contextFactory) : base(contextFactory)
        {
        }

        public async Task SaveFeedbackAsync(string comments, DateTimeOffset time)
        {
            await using CardinalContext ctx = initContext();
            ctx.Feedback.Add(new() { Comments = comments, Time = time });
            await ctx.SaveChangesAsync();
        }

        public async Task SaveFeedbackAsync(string comments, DateTimeOffset time, long userId)
        {
            await using CardinalContext ctx = initContext();
            ctx.Feedback.Add(new() { Comments = comments, Time = time, UserId = userId });
            await ctx.SaveChangesAsync();
        }
    }
}
