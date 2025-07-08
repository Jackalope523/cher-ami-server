using Core.Boundaries;
using Microsoft.EntityFrameworkCore;
using Repository.Databases.EFCore.Entities.Reports;


namespace Repository
{
    public class ReportRepository : Repository, IReportDatabase
    {
        internal ReportRepository(Func<CanaryContext> contextFactory) : base(contextFactory)
        {
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

        public async Task<List<PostReport>> GetReportsForPostAsync(long snapshotId)
        {
            await using CanaryContext ctx = initContext();

            return await 
            ctx.SnapshotReports.
            Where(r => r.SnapshotId == snapshotId).
            Select(r => new PostReport
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

        public async Task ReportSnapshotAsync(long userId, long snapshotId, DateTimeOffset timeOfReport, PostReportType reportType, string reportDetails)
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
