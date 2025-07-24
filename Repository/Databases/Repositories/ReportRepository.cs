using Microsoft.EntityFrameworkCore;
using Repository.Databases.Contexts;
using Repository.Databases.Entities.Reports;
using UserReport = Repository.Databases.Entities.Reports.UserReport;


namespace Repository.Databases.Stores
{
    public class ReportRepository : Repository, IReportDatabase
    {
        internal ReportRepository(Func<CardinalContext> contextFactory) : base(contextFactory)
        {
        }

        public async Task ReportUserAsync(long userId, long targetUserId, DateTimeOffset timeOfReport, UserReportType reportType, string reportDetails)
        {
            UserReport toCreate = new()
            {
                FilingUserId = userId,
                OtherId = targetUserId,
                Type = reportType,
                FilingDate = timeOfReport,
                Notes = reportDetails
            };

            await using CardinalContext ctx = initContext();
            ctx.UserReports.Add(toCreate);
            await ctx.SaveChangesAsync();
        }
        public async Task<(List<Core.Boundaries.UserReport>, List<Core.Boundaries.PostReport>)> GetReportsForUserAsync(long userId)
        {
            await using CardinalContext ctx = initContext();

            List<Core.Boundaries.UserReport> userReports = await ctx.UserReports.
                                                           Where(r => r.OtherId == userId).
                                                           Select(r => new Core.Boundaries.UserReport
                                                           (
                                                               r.Id, 
                                                               r.FilingUserId ?? 0, 
                                                               r.OtherId, 
                                                               r.FilingDate, 
                                                               r.Type, 
                                                               r.Notes
                                                           )).
                                                           ToListAsync();

            List<Core.Boundaries.PostReport> postReports = await ctx.Posts.
                                                           Where(p => p.AuthorId == userId).
                                                           Join
                                                           (
                                                                ctx.PostReports,
                                                                p => p.Id,
                                                                pr => pr.PostId,
                                                                (p, pr) => new Core.Boundaries.PostReport
                                                           (
                                                               pr.Id,
                                                               pr.FilingUserId ?? 0,
                                                               pr.PostId,
                                                               pr.FilingDate,
                                                               pr.Type,
                                                               pr.Notes
                                                           )).
                                                           ToListAsync();

            return (userReports, postReports);
        }

        public async Task<(List<Core.Boundaries.UserReport>, List<Core.Boundaries.PostReport>)> GetReportsByUserAsync(long userId)
        {
            await using CardinalContext ctx = initContext();

            List<Report> reports = await ctx.Reports.Where(r => r.FilingUserId == userId).ToListAsync();

            return (reports.OfType<Core.Boundaries.UserReport>().ToList(), reports.OfType<Core.Boundaries.PostReport>().ToList());
        }

        public async Task<List<Core.Boundaries.PostReport>> GetReportsForPostAsync(long postId)
        {
            await using CardinalContext ctx = initContext();

            return await 
            ctx.PostReports.
            Where(r => r.PostId == postId).
            Select(r => new Core.Boundaries.PostReport
            (
                r.Id,
                r.FilingUserId ?? 0,
                r.PostId,
                r.FilingDate,
                r.Type,
                r.Notes
            )).
            ToListAsync();
        }

        public async Task ReportPostAsync(long userId, long snapshotId, DateTimeOffset timeOfReport, PostReportType reportType, string reportDetails)
        {
            Entities.Reports.PostReport toCreate = new()
            {
                FilingUserId = userId,
                PostId = snapshotId,
                Type = reportType,
                FilingDate = timeOfReport,
                Notes = reportDetails
            };

            await using CardinalContext ctx = initContext();
            ctx.PostReports.Add(toCreate);
            await ctx.SaveChangesAsync();
        }
    }
}
