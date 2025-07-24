using Microsoft.EntityFrameworkCore;
using Repository.Databases.Contexts;
using Repository.Entities;

namespace Repository.Databases.Stores
{
    public class DebugRepository : Repository, IDebugDatabase
    {
        internal DebugRepository(Func<CardinalContext> contextFactory) : base(contextFactory)
        {
        }

        public async Task DrainDatabaseAsync()
        {
            await using CardinalContext ctx = initContext();
            await using var transaction = await ctx.Database.BeginTransactionAsync();

            try
            {
                ctx.SnapshotLinks.ExecuteDelete();
                ctx.CircleMemberships.ExecuteDelete();
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
