using Microsoft.EntityFrameworkCore;
using Repository.Entities;

namespace Repository
{
    public class EFCoreDebugStore : QueryStore, IDebugDatabase
    {
        public EFCoreDebugStore(Harbor.Flag flag) : base(flag)
        {

        }

        public async Task DrainDatabaseAsync()
        {
            storeSentry.ExecuteWrite(ctx => ctx.SnapshotLinks.ExecuteDelete());
            storeSentry.ExecuteWrite(ctx => ctx.GatheringLinks.ExecuteDelete());
            storeSentry.ExecuteWrite(ctx => ctx.UserRelationships.ExecuteDelete());
            storeSentry.ExecuteWrite(ctx => ctx.UserReports.ExecuteDelete());
            storeSentry.ExecuteWrite(ctx => ctx.GatheringReports.ExecuteDelete());
            storeSentry.ExecuteWrite(ctx => ctx.Snapshots.ExecuteDelete());
            storeSentry.ExecuteWrite(ctx => ctx.Telegrams.ExecuteDelete());
            storeSentry.ExecuteWrite(ctx => ctx.Subscriptions.ExecuteDelete());
            storeSentry.ExecuteWrite(ctx => ctx.Penalties.ExecuteDelete());
            storeSentry.ExecuteWrite(ctx => ctx.Gatherings.ExecuteDelete());
            storeSentry.ExecuteWrite(ctx => ctx.Users.ExecuteDelete());
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
