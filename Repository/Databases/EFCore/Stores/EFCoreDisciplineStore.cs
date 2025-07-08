using Core.Boundaries;
using Microsoft.EntityFrameworkCore;


namespace Repository
{
    public class EFCoreDisciplineStore : QueryStore, IDisciplineDatabase
    {
        internal EFCoreDisciplineStore(Func<CanaryContext> contextFactory) : base(contextFactory)
        {
        }

        public async Task<(List<Core.Boundaries.UserReport>, List<Core.Boundaries.GatheringReport>, List<Core.Boundaries.SnapshotReport>)> GetReportsByUserAsync(long id)
        {
            await using CanaryContext ctx = initContext();

            List<Core.Boundaries.UserReport> userReportsToReturn = await ctx.
            UserReports.
            Where(r => r.SelfId == id).
            Select(r => new Core.Boundaries.UserReport
            (
                r.Id,
                r.SelfId ?? 0,
                r.OtherId,
                r.FilingDate,
                r.Type,
                r.Notes
            )).
            ToListAsync();

            List<Core.Boundaries.GatheringReport> gatheringReportsToReturn = await ctx.
            GatheringReports.
            Where(r => r.UserId == id).
            Select(r => new Core.Boundaries.GatheringReport
            (
                r.Id,
                r.UserId ?? 0,
                r.GatheringId,
                r.FilingDate,
                r.Type,
                r.Notes
            )).
            ToListAsync();

            List<Core.Boundaries.SnapshotReport> snapshotReportsToReturn = await ctx.
            SnapshotReports.
            Where(r => r.UserId == id).
            Select(r => new Core.Boundaries.SnapshotReport
            (
                r.Id,
                r.UserId ?? 0,
                r.SnapshotId,
                r.FilingDate,
                r.Type,
                r.Notes
            )).
            ToListAsync();

            return (userReportsToReturn, gatheringReportsToReturn, snapshotReportsToReturn);
        }

        public async Task<List<Core.Boundaries.GatheringReport>> GetReportsForGatheringAsync(long id)
        {
            await using CanaryContext ctx = initContext();

            return await ctx.
            GatheringReports.
            Where(r => r.GatheringId == id).
            Select(r => new Core.Boundaries.GatheringReport
            (
                r.Id,
                r.UserId ?? 0,
                r.GatheringId,
                r.FilingDate,
                r.Type,
                r.Notes
            )).
            ToListAsync();
        }

        public async Task<(List<Core.Boundaries.UserReport>, List<Core.Boundaries.GatheringReport>, List<Core.Boundaries.SnapshotReport>)> GetReportsForUserAsync(long id)
        {
            await using CanaryContext ctx = initContext();

            List<Core.Boundaries.UserReport> userReportsToReturn = await
             ctx.UserReports.
             Where(r => r.OtherId == id).
             Select(r => new Core.Boundaries.UserReport
             (
                 r.Id,
                 r.SelfId ?? 0,
                 r.OtherId,
                 r.FilingDate,
                 r.Type,
                 r.Notes
             )).
            ToListAsync();       

            List<long> gatheringsHosted = await
                ctx.Gatherings.
                Where(e => e.HostId == id).
                Select(e => e.Id).
                ToListAsync();

            List<Core.Boundaries.GatheringReport>  gatheringReportsToReturn = await ctx.
            GatheringReports.
            Where(r => gatheringsHosted.Contains(r.GatheringId)).
            Select(r => new Core.Boundaries.GatheringReport
            (
                r.Id,
                r.UserId ?? 0,
                r.GatheringId,
                r.FilingDate,
                r.Type,
                r.Notes
            )).
            ToListAsync();

            List<long> snapshotsPosted = await 
               ctx.Snapshots.
               Where(s => s.OwnerId == id).
               Select(s => s.Id).
               ToListAsync();

            List<Core.Boundaries.SnapshotReport> snapshotReportsToReturn = await ctx.
            SnapshotReports.
            Where(r => snapshotsPosted.Contains(r.SnapshotId)).
            Select(r => new Core.Boundaries.SnapshotReport
            (
               r.Id,
               r.UserId ?? 0,
               r.SnapshotId,
               r.FilingDate,
               r.Type,
               r.Notes
            )).
            ToListAsync();

            return (userReportsToReturn, gatheringReportsToReturn, snapshotReportsToReturn);
        }

        public async Task ReportGatheringAsync(long userId, long gatheringId, DateTimeOffset timeOfReport, GatheringReportType reportType, string reportDetails)
        {
            GatheringReport toCreate = new()
            {
                UserId = userId,
                GatheringId = gatheringId,
                Type = reportType,
                FilingDate = timeOfReport,
                Notes = reportDetails
            };

            await using CanaryContext ctx = initContext();
            ctx.GatheringReports.Add(toCreate);
            await ctx.SaveChangesAsync();
        }

        public async Task ReportUserAsync(long userId, long targetUserId, DateTimeOffset timeOfReport, UserReportType reportType, string reportDetails)
        {
            UserReport toCreate = new()
            {
                SelfId = userId,
                OtherId = targetUserId,
                Type = reportType,
                FilingDate = timeOfReport,
                Notes = reportDetails
            };

            await using CanaryContext ctx = initContext();
            ctx.UserReports.Add(toCreate);
            await ctx.SaveChangesAsync();
        }

        public async Task ReportUserAsync(long selfId, long targetId, long gatheringId, DateTimeOffset timeOfReport, UserReportType reportType, string reportDetails)
        {
            UserReport toCreate = new()
            {
                SelfId = selfId,
                OtherId = targetId,
                GatheringId = gatheringId,
                Type = reportType,
                FilingDate = timeOfReport,
                Notes = reportDetails
            };

            await using CanaryContext ctx = initContext();
            ctx.UserReports.Add(toCreate);
            await ctx.SaveChangesAsync();
        }

        public async Task PenaliseUserAsync(long userId, PenaltyType offense, DateTimeOffset timeOfPenalty)
        {
            Penalty toAdd = new() 
            {
                PenalizedId = userId,
                Type = offense, 
                Time = timeOfPenalty 
            };

            await using CanaryContext ctx = initContext();
            ctx.Penalties.Add(toAdd);
            await ctx.SaveChangesAsync();
        }

        public async Task<List<PenaltyShard>> GetPenaltiesForUserAsync(long userId)
        {
            await using CanaryContext ctx = initContext();

            return await
            ctx.Penalties.
            Where(p => p.PenalizedId == userId).
            Select(p => new PenaltyShard(p.Type, p.Time)).
            ToListAsync();
        }

        public async Task<List<Core.Boundaries.SnapshotReport>> GetReportsForSnapshotAsync(long snapshotId)
        {
            await using CanaryContext ctx = initContext();

            return await 
            ctx.SnapshotReports.
            Where(r => r.SnapshotId == snapshotId).
            Select(r => new Core.Boundaries.SnapshotReport
            (
                r.Id,
                r.UserId ?? 0,
                r.SnapshotId,
                r.FilingDate,
                r.Type,
                r.Notes
            )).
            ToListAsync();
        }

        public async Task ReportSnapshotAsync(long userId, long snapshotId, DateTimeOffset timeOfReport, SnapshotReportType reportType, string reportDetails)
        {
            SnapshotReport toCreate = new()
            {
                UserId = userId,
                SnapshotId = snapshotId,
                Type = reportType,
                FilingDate = timeOfReport,
                Notes = reportDetails
            };

            await using CanaryContext ctx = initContext();
            ctx.SnapshotReports.Add(toCreate);
            await ctx.SaveChangesAsync();
        }
    }
}
