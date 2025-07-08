using Core.Boundaries;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks.Dataflow;

namespace Repository
{
    public class EFCoreAdminStore : QueryStore, IAdminDatabase
    {
        internal EFCoreAdminStore(Func<CanaryContext> contextFactory) : base(contextFactory)
        {

        }

        public async Task<List<CoreGathering>> GetAllActiveGatheringsAsync(DateTimeOffset currentTime)
        {
            await using CanaryContext ctx = initContext();

            return await 
                ctx.Gatherings
                .Where(g => g.State == GatheringState.Alive && g.StartTime <= currentTime)
                .Select(
                (g) => new CoreGathering
                (
                    g.Id,
                    g.HostId ?? 0,
                    g.Title,
                    g.Description,
                    g.StartTime,
                    g.Location.Y,
                    g.Location.X,
                    g.FriendlyLocation,
                    g.EndTime,
                    g.State,
                    g.GroupMinimum,
                    g.GroupMaximum,
                    new CharacterShard(
                    g.Age,
                    g.Extroversion,
                    g.Athleticisme,
                    g.Chaos,
                    g.Competitiveness,
                    g.Industriousness,
                    g.NightOwl,
                    g.Openness),
                    g.Radius,
                    g.IsDynamic,
                    g.SoftDeleted,
                    g.NumberOfGuests,
                    g.DegreeOfPrivacy,
                    g.Visibility,
                    g.TimeOfCreation,
                    g.Decay
                 )).ToListAsync();
        }

        public async Task VoidGatheringAsync(long gatheringId)
        {
            await using CanaryContext ctx = initContext();
            await using var transaction = await ctx.Database.BeginTransactionAsync();

            try
            {
                await ctx.GuestClearances.Where(c => c.GatheringId == gatheringId).ExecuteDeleteAsync();
                await ctx.GatheringLinks.Where(l => l.GatheringId == gatheringId).ExecuteDeleteAsync();
                await ctx.GatheringReports.Where(r => r.GatheringId == gatheringId).ExecuteDeleteAsync();
                await ctx.UserReports.Where(r => r.GatheringId == gatheringId).ExecuteDeleteAsync();

                List<long> snapshots = await ctx.Snapshots.
                                       Where(s => s.GatheringId == gatheringId).
                                       Select(s => s.Id).
                                       ToListAsync();

                await ctx.SnapshotLinks.Where(l => snapshots.Contains(l.SnapshotId)).ExecuteDeleteAsync();
                await ctx.Snapshots.Where(s => s.GatheringId == gatheringId).ExecuteDeleteAsync();

                ctx.Gatherings.Remove(new Gathering { Id = gatheringId });
                await ctx.SaveChangesAsync();

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
            await using CanaryContext ctx = initContext();
            await using var transaction = await ctx.Database.BeginTransactionAsync();

            try
            {
                await ctx.Penalties.Where(p => p.PenalizedId == userId).ExecuteDeleteAsync();
                await ctx.SnapshotLinks.Where(l => l.UserId == userId).ExecuteDeleteAsync();
                await ctx.Snapshots.Where(s => s.OwnerId == userId).ExecuteDeleteAsync();
                await ctx.Subscriptions.Where(s => s.UserId == userId).ExecuteDeleteAsync();
                await ctx.Telegrams.Where(t => t.RecipientId == userId || t.NotifierId == userId).ExecuteDeleteAsync();
                await ctx.UserReports.Where(r => r.SelfId == userId || r.OtherId == userId).ExecuteDeleteAsync();
                await ctx.GatheringReports.Where(r => r.UserId == userId).ExecuteDeleteAsync();
                await ctx.UserRelationships.Where(l => l.SelfId == userId || l.OtherId == userId).ExecuteDeleteAsync();
                await ctx.GuestClearances.Where(c => c.UserId == userId).ExecuteDeleteAsync();
                await ctx.GatheringLinks.Where(l => l.UserId == userId).ExecuteDeleteAsync();
                await ctx.Gatherings.Where(e => e.HostId == userId).ExecuteDeleteAsync();
                
                ctx.Users.Remove(new User { Id = userId });
                await ctx.SaveChangesAsync();

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
