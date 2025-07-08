using Microsoft.EntityFrameworkCore;
using Repository.Entities;

namespace Repository
{
    public class DebugRepository : Repository, IDebugDatabase
    {
        internal DebugRepository(Func<CanaryContext> contextFactory) : base(contextFactory)
        {
        }

        public async Task DrainDatabaseAsync()
        {
            await using CanaryContext ctx = initContext();
            await using var transaction = await ctx.Database.BeginTransactionAsync();

            try
            {
                ctx.SnapshotLinks.ExecuteDelete();
                ctx.GatheringLinks.ExecuteDelete();
                ctx.UserRelationships.ExecuteDelete();
                ctx.UserReports.ExecuteDelete();
                ctx.GatheringReports.ExecuteDelete();
                ctx.Snapshots.ExecuteDelete();
                ctx.Telegrams.ExecuteDelete();
                ctx.Subscriptions.ExecuteDelete();
                ctx.Penalties.ExecuteDelete();
                ctx.Gatherings.ExecuteDelete();
                ctx.Users.ExecuteDelete();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
