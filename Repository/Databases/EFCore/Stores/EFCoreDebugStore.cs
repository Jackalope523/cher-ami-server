using Microsoft.EntityFrameworkCore;
using Repository.Entities;

namespace Repository
{
    public class EFCoreDebugStore : QueryStore, IDebugDatabase
    {
        internal EFCoreDebugStore(Func<CanaryContext> contextFactory) : base(contextFactory)
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

        public async Task VoidUserAsync(long userId)
        {
            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.Penalties.
                Where(p => p.PenalizedId == userId).
                ExecuteDeleteAsync());

            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.SnapshotLinks.
                Where(l => l.UserId == userId).
                ExecuteDeleteAsync());

            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.Snapshots.
                Where(s => s.OwnerId == userId).
                ExecuteDeleteAsync());

            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.Subscriptions.
                Where(s => s.UserId == userId).
                ExecuteDeleteAsync());

            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.Telegrams.
                Where(t => t.RecipientId == userId || t.NotifierId == userId).
                ExecuteDeleteAsync());

            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.UserReports.
                Where(r => r.SelfId == userId || r.OtherId == userId).
                ExecuteDeleteAsync());

            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.GatheringReports.
                Where(r => r.UserId == userId).
                ExecuteDeleteAsync());

            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.UserRelationships.
                Where(l => l.SelfId == userId || l.OtherId == userId).
                ExecuteDeleteAsync());

            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.GuestClearances.
                Where(c => c.UserId == userId).
                ExecuteDeleteAsync());

            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.GatheringLinks.
                Where(l => l.UserId == userId).
                ExecuteDeleteAsync());

            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.Gatherings.
                Where(e => e.HostId == userId).
                ExecuteDeleteAsync());

            await storeSentry.ExecuteWriteAsync(ctx =>
                ctx.Users.
                Remove(new User { Id = userId }));
        }
    }
}
